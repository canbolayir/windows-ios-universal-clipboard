using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.Json;
using System.Windows.Forms;

const int Port = 8765;
const string AppName = "Windows iOS Universal Clipboard";
const string MdnsHost = "copybridge.local";

var createdNew = false;
using var mutex = new Mutex(true, @"Local\WindowsIOSUniversalClipboard", out createdNew);
if (!createdNew)
    return;

var appData = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
    "WindowsIOSUniversalClipboard");
Directory.CreateDirectory(appData);

var configPath = Path.Combine(appData, "config.json");
var logPath = Path.Combine(appData, "bridge.log");
var configLock = new object();
var config = LoadConfig(configPath);
Process? mdnsProcess = null;

void SafeLog(string message)
{
    try { File.AppendAllText(logPath, $"{DateTimeOffset.Now:O} {message}{Environment.NewLine}"); }
    catch { }
}

BridgeConfig LoadConfig(string path)
{
    try
    {
        if (File.Exists(path))
        {
            var existing = JsonSerializer.Deserialize<BridgeConfig>(File.ReadAllText(path));
            if (existing is not null)
                return existing;
        }
    }
    catch { }

    var fresh = new BridgeConfig(new List<string>());
    SaveConfig(fresh);
    return fresh;
}

void SaveConfig(BridgeConfig value)
{
    lock (configLock)
    {
        File.WriteAllText(configPath, JsonSerializer.Serialize(value, new JsonSerializerOptions { WriteIndented = true }));
    }
}

string NormalizeIp(IPAddress? ip)
{
    if (ip is null) return "unknown";
    if (ip.IsIPv4MappedToIPv6) ip = ip.MapToIPv4();
    return ip.ToString();
}

bool IsLoopback(IPAddress? ip)
{
    if (ip is null) return false;
    if (ip.IsIPv4MappedToIPv6) ip = ip.MapToIPv4();
    return IPAddress.IsLoopback(ip);
}

bool EnsureApproved(IPAddress? remoteIp)
{
    if (IsLoopback(remoteIp)) return true;

    var ip = NormalizeIp(remoteIp);
    lock (configLock)
    {
        if (config.AllowedDevices.Contains(ip, StringComparer.OrdinalIgnoreCase))
            return true;
    }

    DialogResult result = DialogResult.No;
    using var done = new ManualResetEventSlim(false);
    var thread = new Thread(() =>
    {
        try
        {
            result = MessageBox.Show(
                $"A device at {ip} wants to send text to your Windows clipboard.\n\nAllow this device?\n\nOnly approve devices on a network you trust.",
                "CopyBridge pairing request",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2,
                MessageBoxOptions.DefaultDesktopOnly);
        }
        finally { done.Set(); }
    });
    thread.SetApartmentState(ApartmentState.STA);
    thread.Start();

    if (!done.Wait(TimeSpan.FromSeconds(45)) || result != DialogResult.Yes)
    {
        SafeLog($"pair_denied ip={ip}");
        return false;
    }

    lock (configLock)
    {
        if (!config.AllowedDevices.Contains(ip, StringComparer.OrdinalIgnoreCase))
        {
            config.AllowedDevices.Add(ip);
            SaveConfig(config);
        }
    }

    SafeLog($"pair_allowed ip={ip}");
    return true;
}

Exception? SetClipboardText(string text)
{
    Exception? lastError = null;
    using var finished = new ManualResetEventSlim(false);
    var thread = new Thread(() =>
    {
        try
        {
            for (var attempt = 0; attempt < 8; attempt++)
            {
                try
                {
                    Clipboard.SetText(text);
                    lastError = null;
                    return;
                }
                catch (Exception ex)
                {
                    lastError = ex;
                    Thread.Sleep(75);
                }
            }
        }
        finally { finished.Set(); }
    });
    thread.SetApartmentState(ApartmentState.STA);
    thread.Start();

    if (!finished.Wait(TimeSpan.FromSeconds(5)))
        return new TimeoutException("Clipboard operation timed out.");
    return lastError;
}

string? GetLanIpv4()
{
    try
    {
        return NetworkInterface.GetAllNetworkInterfaces()
            .Where(nic => nic.OperationalStatus == OperationalStatus.Up && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
            .SelectMany(nic => nic.GetIPProperties().UnicastAddresses)
            .Select(x => x.Address)
            .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip) && !ip.ToString().StartsWith("169.254.", StringComparison.Ordinal))
            ?.ToString();
    }
    catch { return null; }
}

void StartMdnsPublisher()
{
    try
    {
        if (mdnsProcess is { HasExited: false })
            mdnsProcess.Kill(true);
    }
    catch { }

    var ip = GetLanIpv4();
    if (ip is null)
    {
        SafeLog("mdns_no_lan_ip");
        return;
    }

    var dnsSd = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "dns-sd.exe");
    if (!File.Exists(dnsSd))
    {
        SafeLog("mdns_dns_sd_missing");
        return;
    }

    try
    {
        mdnsProcess = Process.Start(new ProcessStartInfo
        {
            FileName = dnsSd,
            ArgumentList = { "-P", "CopyBridge", "_http._tcp", "local", Port.ToString(), MdnsHost, ip, "path=/copy" },
            UseShellExecute = false,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden
        });
        SafeLog($"mdns_started host={MdnsHost} ip={ip}");
    }
    catch (Exception ex)
    {
        SafeLog($"mdns_error {ex.GetType().Name}: {ex.Message}");
    }
}

StartMdnsPublisher();
NetworkChange.NetworkAddressChanged += (_, _) =>
{
    Thread.Sleep(750);
    StartMdnsPublisher();
};

AppDomain.CurrentDomain.ProcessExit += (_, _) =>
{
    try
    {
        if (mdnsProcess is { HasExited: false }) mdnsProcess.Kill(true);
    }
    catch { }
};

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls($"http://0.0.0.0:{Port}");
builder.WebHost.ConfigureKestrel(options => options.Limits.MaxRequestBodySize = 1 * 1024 * 1024);
var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new
{
    ok = true,
    app = AppName,
    endpoint = $"http://{MdnsHost}:{Port}/copy"
}));

app.MapPost("/copy", async (HttpContext context) =>
{
    if (!EnsureApproved(context.Connection.RemoteIpAddress))
        return Results.StatusCode(StatusCodes.Status403Forbidden);

    CopyRequest? payload;
    try { payload = await context.Request.ReadFromJsonAsync<CopyRequest>(); }
    catch { return Results.BadRequest(new { error = "Invalid JSON." }); }

    if (payload?.Text is null)
        return Results.BadRequest(new { error = "Missing text." });
    if (payload.Text.Length > 1_000_000)
        return Results.BadRequest(new { error = "Text is too large." });

    var error = SetClipboardText(payload.Text);
    if (error is not null)
    {
        SafeLog($"clipboard_error {error.GetType().Name}: {error.Message}");
        return Results.Problem("Could not write to the Windows clipboard.");
    }

    SafeLog($"copied ip={NormalizeIp(context.Connection.RemoteIpAddress)} chars={payload.Text.Length}");
    return Results.Ok(new { ok = true });
});

SafeLog($"started port={Port}");
app.Run();

record BridgeConfig(List<string> AllowedDevices);
record CopyRequest(string Text);
