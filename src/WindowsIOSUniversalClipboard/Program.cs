using System.Buffers.Binary;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;

const int Port = 8765;
const int MdnsPort = 5353;
const string MdnsName = "copybridge.local";

var dataDir = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
    "WindowsIOSUniversalClipboard");
Directory.CreateDirectory(dataDir);

var configPath = Path.Combine(dataDir, "config.json");
var logPath = Path.Combine(dataDir, "app.log");
var configLock = new object();
var approvalGate = new SemaphoreSlim(1, 1);
var config = LoadConfig(configPath);
var mdnsState = "starting";

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
                $"A device at {ip} wants to send copied text to this PC.\n\n" +
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

    await approvalGate.WaitAsync();
    try
    {
        if (IsApproved(ip))
            return true;

        Log($"Pairing request from {ip}");
        var allowed = await PromptApprovalAsync(ip);

        if (allowed)
        {
            lock (configLock)
                config.ApprovedDevices.Add(ip);

            SaveConfig();
            Log($"Approved {ip}");
        }
        else
        {
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
            for (var i = 0; i < 5; i++)
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

IEnumerable<(IPAddress Address, IPAddress Mask)> GetLocalIpv4()
{
    foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
    {
        if (nic.OperationalStatus != OperationalStatus.Up)
            continue;

        if (nic.NetworkInterfaceType is NetworkInterfaceType.Loopback or NetworkInterfaceType.Tunnel)
            continue;

        IPInterfaceProperties properties;
        try { properties = nic.GetIPProperties(); }
        catch { continue; }

        foreach (var unicast in properties.UnicastAddresses)
        {
            if (unicast.Address.AddressFamily != AddressFamily.InterNetwork)
                continue;

            if (IPAddress.IsLoopback(unicast.Address))
                continue;

            var bytes = unicast.Address.GetAddressBytes();
            if (bytes[0] == 169 && bytes[1] == 254)
                continue;

            if (unicast.IPv4Mask is null)
                continue;

            yield return (unicast.Address, unicast.IPv4Mask);
        }
    }
}

bool SameSubnet(IPAddress left, IPAddress right, IPAddress mask)
{
    var a = left.GetAddressBytes();
    var b = right.GetAddressBytes();
    var m = mask.GetAddressBytes();

    if (a.Length != 4 || b.Length != 4 || m.Length != 4)
        return false;

    for (var i = 0; i < 4; i++)
        if ((a[i] & m[i]) != (b[i] & m[i]))
            return false;

    return true;
}

IPAddress? PickAddressFor(IPAddress remote)
{
    remote = NormalizeIp(remote);
    var candidates = GetLocalIpv4().ToList();

    foreach (var item in candidates)
        if (SameSubnet(item.Address, remote, item.Mask))
            return item.Address;

    return candidates.FirstOrDefault().Address;
}

string ReadDnsName(ReadOnlySpan<byte> packet, ref int offset)
{
    var labels = new List<string>();
    var originalOffset = -1;
    var guard = 0;

    while (offset < packet.Length && guard++ < 32)
    {
        var length = packet[offset++];

        if (length == 0)
            break;

        if ((length & 0xC0) == 0xC0)
        {
            if (offset >= packet.Length)
                break;

            var pointer = ((length & 0x3F) << 8) | packet[offset++];
            if (pointer >= packet.Length)
                break;

            if (originalOffset < 0)
                originalOffset = offset;

            offset = pointer;
            continue;
        }

        if (offset + length > packet.Length)
            break;

        labels.Add(Encoding.ASCII.GetString(packet.Slice(offset, length)));
        offset += length;
    }

    if (originalOffset >= 0)
        offset = originalOffset;

    return string.Join('.', labels);
}

byte[] EncodeDnsName(string name)
{
    using var ms = new MemoryStream();

    foreach (var label in name.Split('.', StringSplitOptions.RemoveEmptyEntries))
    {
        var bytes = Encoding.ASCII.GetBytes(label);
        ms.WriteByte((byte)bytes.Length);
        ms.Write(bytes);
    }

    ms.WriteByte(0);
    return ms.ToArray();
}

byte[] BuildMdnsResponse(ReadOnlySpan<byte> query, IPAddress address)
{
    var name = EncodeDnsName(MdnsName);
    var result = new byte[12 + name.Length + 10 + 4];

    if (query.Length >= 2)
    {
        result[0] = query[0];
        result[1] = query[1];
    }

    BinaryPrimitives.WriteUInt16BigEndian(result.AsSpan(2, 2), 0x8400); // response + authoritative
    BinaryPrimitives.WriteUInt16BigEndian(result.AsSpan(6, 2), 1);      // one answer

    var offset = 12;
    name.CopyTo(result.AsSpan(offset));
    offset += name.Length;

    BinaryPrimitives.WriteUInt16BigEndian(result.AsSpan(offset, 2), 1);       // A
    BinaryPrimitives.WriteUInt16BigEndian(result.AsSpan(offset + 2, 2), 0x8001); // IN + cache flush
    BinaryPrimitives.WriteUInt32BigEndian(result.AsSpan(offset + 4, 4), 120); // TTL
    BinaryPrimitives.WriteUInt16BigEndian(result.AsSpan(offset + 8, 2), 4);
    address.GetAddressBytes().CopyTo(result.AsSpan(offset + 10, 4));

    return result;
}

bool QueryRequestsCopyBridge(ReadOnlySpan<byte> packet)
{
    if (packet.Length < 12)
        return false;

    var questionCount = BinaryPrimitives.ReadUInt16BigEndian(packet.Slice(4, 2));
    var offset = 12;

    for (var i = 0; i < questionCount; i++)
    {
        var name = ReadDnsName(packet, ref offset);
        if (offset + 4 > packet.Length)
            return false;

        var type = BinaryPrimitives.ReadUInt16BigEndian(packet.Slice(offset, 2));
        offset += 4; // type + class

        if (name.Equals(MdnsName, StringComparison.OrdinalIgnoreCase) &&
            (type == 1 || type == 255))
            return true;
    }

    return false;
}

async Task RunMdnsResponderAsync(CancellationToken cancellationToken)
{
    try
    {
        using var udp = new UdpClient(AddressFamily.InterNetwork);
        udp.Client.ExclusiveAddressUse = false;
        udp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        udp.Client.Bind(new IPEndPoint(IPAddress.Any, MdnsPort));
        udp.JoinMulticastGroup(IPAddress.Parse("224.0.0.251"));

        mdnsState = "running";
        Log($"{MdnsName} mDNS responder running.");

        var multicast = new IPEndPoint(IPAddress.Parse("224.0.0.251"), MdnsPort);

        while (!cancellationToken.IsCancellationRequested)
        {
            UdpReceiveResult received;
            try
            {
                received = await udp.ReceiveAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            if (!QueryRequestsCopyBridge(received.Buffer))
                continue;

            var localAddress = PickAddressFor(received.RemoteEndPoint.Address);
            if (localAddress is null)
                continue;

            var response = BuildMdnsResponse(received.Buffer, localAddress);

            try
            {
                await udp.SendAsync(response, multicast, cancellationToken);
                await udp.SendAsync(response, received.RemoteEndPoint, cancellationToken);
            }
            catch (Exception ex)
            {
                Log($"mDNS send failed: {ex.Message}");
            }
        }
    }
    catch (Exception ex)
    {
        mdnsState = "failed";
        Log($"mDNS responder failed: {ex}");
    }
}

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls($"http://0.0.0.0:{Port}");
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 2 * 1024 * 1024;
});

var app = builder.Build();

_ = Task.Run(() => RunMdnsResponderAsync(app.Lifetime.ApplicationStopping));

app.MapGet("/health", () => Results.Ok(new
{
    ok = true,
    hostname = MdnsName,
    mdns = mdnsState,
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
h1{line-height:1.1}
.ok{font-weight:700}
</style>
</head>
<body>
<h1>Windows iOS Universal Clipboard</h1>
<p class="ok">Windows is ready.</p>
<div class="card">
<strong>iPhone Shortcut URL</strong><br>
<code>http://copybridge.local:8765/copy</code>
</div>
<p>Add the shared iPhone Shortcut, copy any text, then run the Shortcut (or assign it to Back Tap).</p>
<p>The first time an iPhone connects, Windows will ask whether you want to allow that device. Approve it once; after that, Copy → Back Tap → Ctrl+V.</p>
<p><small>Designed for trusted local networks. Clipboard text is sent over your LAN, not through a cloud service.</small></p>
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
