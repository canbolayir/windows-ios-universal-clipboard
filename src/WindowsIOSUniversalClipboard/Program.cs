using System.Collections.Concurrent;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;

const int Port = 8765;
const string AliasHost = "copybridge.local";

var dataDir = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
    "WindowsIOSUniversalClipboard");
Directory.CreateDirectory(dataDir);

var configPath = Path.Combine(dataDir, "config.json");
var logPath = Path.Combine(dataDir, "app.log");

var configLock = new object();
var approvalGate = new SemaphoreSlim(1, 1);
var deniedUntil = new ConcurrentDictionary<string, DateTimeOffset>();
var config = LoadConfig(configPath);

void Log(string message)
{
    try
    {
        File.AppendAllText(
            logPath,
            $"{DateTimeOffset.Now:O} {message}{Environment.NewLine}",
            Encoding.UTF8);
    }
    catch { }
}

AppConfig LoadConfig(string path)
{
    try
    {
        if (File.Exists(path))
        {
            var loaded = JsonSerializer.Deserialize<AppConfig>(File.ReadAllText(path));
            if (loaded is not null)
                return loaded;
        }
    }
    catch (Exception ex)
    {
        Log($"Config load failed: {ex.Message}");
    }

    return new AppConfig();
}

void SaveConfig()
{
    lock (configLock)
    {
        var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(configPath, json, Encoding.UTF8);
    }
}

IPAddress NormalizeIp(IPAddress address) =>
    address.IsIPv4MappedToIPv6 ? address.MapToIPv4() : address;

bool IsApproved(string ip)
{
    lock (configLock)
        return config.ApprovedDevices.Contains(ip, StringComparer.OrdinalIgnoreCase);
}

async Task<bool> PromptApprovalAsync(string ip)
{
    var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

    var thread = new Thread(() =>
    {
        try
        {
            var result = MessageBox.Show(
                $"An iPhone or other device at {ip} wants to send copied text to this PC.\n\n" +
                "Only allow devices you recognize on a trusted local network.\n\n" +
                "Allow this device?",
                "Windows iOS Universal Clipboard",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);

            tcs.TrySetResult(result == DialogResult.Yes);
        }
        catch (Exception ex)
        {
            Log($"Approval dialog failed: {ex.Message}");
            tcs.TrySetResult(false);
        }
    });

    thread.IsBackground = true;
    thread.SetApartmentState(ApartmentState.STA);
    thread.Start();

    return await tcs.Task;
}

async Task<bool> EnsureApprovedAsync(IPAddress? remoteAddress)
{
    if (remoteAddress is null)
        return false;

    var remote = NormalizeIp(remoteAddress);
    if (IPAddress.IsLoopback(remote))
        return true;

    var ip = remote.ToString();

    if (IsApproved(ip))
        return true;

    if (deniedUntil.TryGetValue(ip, out var until) && until > DateTimeOffset.Now)
        return false;

    await approvalGate.WaitAsync();
    try
    {
        if (IsApproved(ip))
            return true;

        if (deniedUntil.TryGetValue(ip, out until) && until > DateTimeOffset.Now)
            return false;

        Log($"Pairing request from {ip}");
        var allowed = await PromptApprovalAsync(ip);

        if (allowed)
        {
            lock (configLock)
            {
                if (!config.ApprovedDevices.Contains(ip, StringComparer.OrdinalIgnoreCase))
                    config.ApprovedDevices.Add(ip);
            }

            deniedUntil.TryRemove(ip, out _);
            SaveConfig();
            Log($"Approved {ip}");
        }
        else
        {
            deniedUntil[ip] = DateTimeOffset.Now.AddMinutes(1);
            Log($"Denied {ip}");
        }

        return allowed;
    }
    finally
    {
        approvalGate.Release();
    }
}

IResult SetClipboardText(string text)
{
    Exception? error = null;
    using var done = new ManualResetEventSlim(false);

    var thread = new Thread(() =>
    {
        try
        {
            for (var attempt = 0; attempt < 5; attempt++)
            {
                try
                {
                    Clipboard.SetText(text);
                    error = null;
                    return;
                }
                catch (Exception ex)
                {
                    error = ex;
                    Thread.Sleep(80);
                }
            }
        }
        finally
        {
            done.Set();
        }
    });

    thread.IsBackground = true;
    thread.SetApartmentState(ApartmentState.STA);
    thread.Start();

    if (!done.Wait(TimeSpan.FromSeconds(5)))
        return Results.Problem("Clipboard timeout.");

    return error is null
        ? Results.Ok(new { ok = true })
        : Results.Problem(error.Message);
}

using var discovery = new BonjourPublisherManager(Log);

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls($"http://0.0.0.0:{Port}");
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 2 * 1024 * 1024;
});

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new
{
    ok = true,
    hostname = AliasHost,
    discovery = discovery.State,
    approvedDevices = config.ApprovedDevices.Count
}));

app.MapGet("/setup", () =>
{
    const string html = """
<!doctype html>
<html lang="en">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width,initial-scale=1">
<title>Windows iOS Universal Clipboard</title>
<style>
body{font-family:system-ui,-apple-system,Segoe UI,sans-serif;max-width:760px;margin:60px auto;padding:0 24px;line-height:1.55;color:#111}
code{background:#f2f2f2;padding:3px 7px;border-radius:6px}
.card{border:1px solid #ddd;border-radius:14px;padding:22px;margin:20px 0}
h1{line-height:1.1}.ok{font-weight:700}
</style>
</head>
<body>
<h1>Windows iOS Universal Clipboard</h1>
<p class="ok">Windows is ready.</p>
<div class="card">
<strong>Universal iPhone Shortcut address</strong><br>
<code>http://copybridge.local:8765/copy</code>
</div>
<p>Add the shared iPhone Shortcut. Copy text and run it.</p>
<p>The first time a device connects, Windows asks whether you want to allow it. Approve it once. After that: <strong>Copy → Back Tap → Ctrl+V</strong>.</p>
<p><small>Designed for trusted local networks. Clipboard text travels over your LAN and is not relayed through this project.</small></p>
</body>
</html>
""";
    return Results.Content(html, "text/html; charset=utf-8");
});

app.MapPost("/copy", async (HttpRequest request) =>
{
    CopyRequest? body;

    try
    {
        body = await request.ReadFromJsonAsync<CopyRequest>();
    }
    catch
    {
        return Results.BadRequest(new { ok = false, error = "Invalid JSON body." });
    }

    if (body?.Text is null || body.Text.Length > 1_000_000)
        return Results.BadRequest(new { ok = false, error = "Invalid text." });

    if (!await EnsureApprovedAsync(request.HttpContext.Connection.RemoteIpAddress))
        return Results.StatusCode(StatusCodes.Status403Forbidden);

    return SetClipboardText(body.Text);
});

app.Run();

sealed class AppConfig
{
    public List<string> ApprovedDevices { get; set; } = new();
}

sealed record CopyRequest(string Text);

sealed record LanEndpoint(uint InterfaceIndex, IPAddress Address, string Name);

sealed class BonjourPublisherManager : IDisposable
{
    private readonly Action<string> _log;
    private readonly object _gate = new();
    private readonly System.Threading.Timer _timer;
    private BonjourARecordPublisher? _publisher;
    private string _signature = "";
    private string _state = "starting";
    private bool _disposed;

    public BonjourPublisherManager(Action<string> log)
    {
        _log = log;
        _timer = new System.Threading.Timer(_ => Refresh(), null, 0, 5000);
    }

    public string State
    {
        get
        {
            lock (_gate)
                return _state;
        }
    }

    private void Refresh()
    {
        if (_disposed)
            return;

        try
        {
            var endpoints = FindLanEndpoints();
            var signature = string.Join(
                "|",
                endpoints
                    .OrderBy(x => x.InterfaceIndex)
                    .ThenBy(x => x.Address.ToString())
                    .Select(x => $"{x.InterfaceIndex}:{x.Address}"));

            lock (_gate)
            {
                if (signature == _signature && _publisher is not null)
                    return;
            }

            if (endpoints.Count == 0)
            {
                lock (_gate)
                {
                    _publisher?.Dispose();
                    _publisher = null;
                    _signature = "";
                    _state = "waiting-for-network";
                }
                return;
            }

            var next = new BonjourARecordPublisher(endpoints, _log);
            next.Start();

            lock (_gate)
            {
                _publisher?.Dispose();
                _publisher = next;
                _signature = signature;
                _state = "running";
            }

            _log($"Published copybridge.local on {string.Join(", ", endpoints.Select(x => $"{x.Name}={x.Address}"))}.");
        }
        catch (DllNotFoundException ex)
        {
            lock (_gate)
                _state = "bonjour-missing";
            _log($"Bonjour client library missing: {ex.Message}");
        }
        catch (EntryPointNotFoundException ex)
        {
            lock (_gate)
                _state = "bonjour-incompatible";
            _log($"Bonjour client API unavailable: {ex.Message}");
        }
        catch (Exception ex)
        {
            lock (_gate)
                _state = "failed";
            _log($"Discovery publish failed: {ex}");
        }
    }

    private static List<LanEndpoint> FindLanEndpoints()
    {
        var preferred = new List<LanEndpoint>();
        var fallback = new List<LanEndpoint>();

        foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (nic.OperationalStatus != OperationalStatus.Up)
                continue;

            if (nic.NetworkInterfaceType is NetworkInterfaceType.Loopback or NetworkInterfaceType.Tunnel)
                continue;

            IPInterfaceProperties props;
            IPv4InterfaceProperties? ipv4Props;

            try
            {
                props = nic.GetIPProperties();
                ipv4Props = props.GetIPv4Properties();
            }
            catch
            {
                continue;
            }

            if (ipv4Props is null || ipv4Props.Index <= 0)
                continue;

            var hasGateway = props.GatewayAddresses.Any(g =>
                g.Address.AddressFamily == AddressFamily.InterNetwork &&
                !g.Address.Equals(IPAddress.Any));

            foreach (var unicast in props.UnicastAddresses)
            {
                var address = unicast.Address;

                if (address.AddressFamily != AddressFamily.InterNetwork ||
                    IPAddress.IsLoopback(address))
                    continue;

                var bytes = address.GetAddressBytes();
                if (bytes[0] == 169 && bytes[1] == 254)
                    continue;

                var endpoint = new LanEndpoint((uint)ipv4Props.Index, address, nic.Name);
                fallback.Add(endpoint);

                if (hasGateway)
                    preferred.Add(endpoint);
            }
        }

        return (preferred.Count > 0 ? preferred : fallback)
            .GroupBy(x => (x.InterfaceIndex, x.Address))
            .Select(g => g.First())
            .ToList();
    }

    public void Dispose()
    {
        _disposed = true;
        _timer.Dispose();

        lock (_gate)
        {
            _publisher?.Dispose();
            _publisher = null;
        }
    }
}

sealed class BonjourARecordPublisher : IDisposable
{
    private const uint DNSServiceFlagsUnique = 0x20;
    private const ushort DNSServiceTypeA = 1;
    private const ushort DNSServiceClassIN = 1;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void RegisterRecordReply(
        IntPtr sdRef,
        IntPtr recordRef,
        uint flags,
        int errorCode,
        IntPtr context);

    [DllImport("dnssd.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern int DNSServiceCreateConnection(out IntPtr sdRef);

    [DllImport("dnssd.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern int DNSServiceRegisterRecord(
        IntPtr sdRef,
        out IntPtr recordRef,
        uint flags,
        uint interfaceIndex,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string fullname,
        ushort rrtype,
        ushort rrclass,
        ushort rdlen,
        byte[] rdata,
        uint ttl,
        RegisterRecordReply callback,
        IntPtr context);

    [DllImport("dnssd.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern int DNSServiceProcessResult(IntPtr sdRef);

    [DllImport("dnssd.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern void DNSServiceRefDeallocate(IntPtr sdRef);

    private readonly IReadOnlyList<LanEndpoint> _endpoints;
    private readonly Action<string> _log;
    private IntPtr _connection;
    private readonly List<IntPtr> _records = new();
    private RegisterRecordReply? _callback;
    private CountdownEvent? _registered;
    private Task? _processTask;
    private int _firstError;
    private int _disposed;

    public BonjourARecordPublisher(IReadOnlyList<LanEndpoint> endpoints, Action<string> log)
    {
        _endpoints = endpoints;
        _log = log;
    }

    public void Start()
    {
        var error = DNSServiceCreateConnection(out _connection);
        if (error != 0 || _connection == IntPtr.Zero)
            throw new InvalidOperationException($"DNSServiceCreateConnection failed: {error}");

        _registered = new CountdownEvent(_endpoints.Count);

        _callback = (_, _, _, callbackError, _) =>
        {
            if (callbackError != 0)
                Interlocked.CompareExchange(ref _firstError, callbackError, 0);

            try
            {
                _registered?.Signal();
            }
            catch { }
        };

        foreach (var endpoint in _endpoints)
        {
            var bytes = endpoint.Address.GetAddressBytes();

            error = DNSServiceRegisterRecord(
                _connection,
                out var record,
                DNSServiceFlagsUnique,
                endpoint.InterfaceIndex,
                "copybridge.local.",
                DNSServiceTypeA,
                DNSServiceClassIN,
                (ushort)bytes.Length,
                bytes,
                120,
                _callback,
                IntPtr.Zero);

            if (error != 0)
                throw new InvalidOperationException(
                    $"DNSServiceRegisterRecord failed on {endpoint.Name}/{endpoint.Address}: {error}");

            _records.Add(record);
        }

        _processTask = Task.Run(() =>
        {
            while (Volatile.Read(ref _disposed) == 0 && _connection != IntPtr.Zero)
            {
                try
                {
                    var result = DNSServiceProcessResult(_connection);
                    if (result != 0 && Volatile.Read(ref _disposed) == 0)
                    {
                        _log($"DNSServiceProcessResult returned {result}.");
                        break;
                    }
                }
                catch (Exception ex)
                {
                    if (Volatile.Read(ref _disposed) == 0)
                        _log($"Bonjour processing stopped: {ex.Message}");
                    break;
                }
            }
        });

        if (!_registered.Wait(TimeSpan.FromSeconds(5)))
            throw new TimeoutException("Bonjour A-record registration timed out.");

        if (_firstError != 0)
            throw new InvalidOperationException($"Bonjour A-record registration failed: {_firstError}");
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
            return;

        var connection = Interlocked.Exchange(ref _connection, IntPtr.Zero);
        if (connection != IntPtr.Zero)
        {
            try { DNSServiceRefDeallocate(connection); }
            catch { }
        }

        try { _processTask?.Wait(TimeSpan.FromSeconds(1)); }
        catch { }

        _registered?.Dispose();
    }
}
