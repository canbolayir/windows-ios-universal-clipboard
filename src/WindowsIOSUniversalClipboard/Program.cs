using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;

const int Port = 8765;
const string AppName = "Windows iOS Universal Clipboard";

var createdNew = false;
using var mutex = new Mutex(true, @"Local\WindowsIOSUniversalClipboard", out createdNew);
if (!createdNew)
{
    TryOpenSetup();
    return;
}

var appData = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
    "WindowsIOSUniversalClipboard");
Directory.CreateDirectory(appData);

var configPath = Path.Combine(appData, "config.json");
var logPath = Path.Combine(appData, "bridge.log");

var config = LoadOrCreateConfig(configPath);

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls($"http://0.0.0.0:{Port}");
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 1 * 1024 * 1024;
});
var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new
{
    ok = true,
    app = AppName,
    version = typeof(Program).Assembly.GetName().Version?.ToString() ?? "unknown"
}));

app.MapGet("/setup", (HttpContext context) =>
{
    if (!IsLoopback(context.Connection.RemoteIpAddress))
        return Results.NotFound();

    var hostnameEndpoint = $"http://{Environment.MachineName}.local:{Port}/copy";
    var ip = GetLanIpv4();
    var ipEndpoint = ip is null ? null : $"http://{ip}:{Port}/copy";

    return Results.Content(
        BuildSetupHtml(hostnameEndpoint, ipEndpoint, config.Token),
        "text/html; charset=utf-8");
});

app.MapGet("/setup.json", (HttpContext context) =>
{
    if (!IsLoopback(context.Connection.RemoteIpAddress))
        return Results.NotFound();

    var ip = GetLanIpv4();
    return Results.Ok(new
    {
        endpoint = $"http://{Environment.MachineName}.local:{Port}/copy",
        fallbackEndpoint = ip is null ? null : $"http://{ip}:{Port}/copy",
        token = config.Token
    });
});

app.MapPost("/copy", async (HttpRequest request) =>
{
    if (!Authorized(request, config.Token))
        return Results.Unauthorized();

    CopyRequest? payload;
    try
    {
        payload = await request.ReadFromJsonAsync<CopyRequest>();
    }
    catch
    {
        return Results.BadRequest(new { error = "Invalid JSON." });
    }

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

    SafeLog($"copied chars={payload.Text.Length}");
    return Results.Ok(new { ok = true });
});

SafeLog($"started host={Environment.MachineName} port={Port}");
app.Run();

bool Authorized(HttpRequest request, string token)
{
    if (!request.Headers.TryGetValue("X-Clipboard-Token", out var supplied))
        return false;

    var suppliedBytes = Encoding.UTF8.GetBytes(supplied.ToString());
    var tokenBytes = Encoding.UTF8.GetBytes(token);

    return suppliedBytes.Length == tokenBytes.Length &&
           CryptographicOperations.FixedTimeEquals(suppliedBytes, tokenBytes);
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
        finally
        {
            finished.Set();
        }
    });

    thread.SetApartmentState(ApartmentState.STA);
    thread.Start();

    if (!finished.Wait(TimeSpan.FromSeconds(5)))
        return new TimeoutException("Clipboard operation timed out.");

    return lastError;
}

BridgeConfig LoadOrCreateConfig(string path)
{
    try
    {
        if (File.Exists(path))
        {
            var existing = JsonSerializer.Deserialize<BridgeConfig>(File.ReadAllText(path));
            if (existing is not null && existing.Token.Length >= 32)
                return existing;
        }
    }
    catch
    {
        // Regenerate a safe local config if the file is unreadable.
    }

    var newConfig = new BridgeConfig(Convert.ToHexString(RandomNumberGenerator.GetBytes(24)).ToLowerInvariant());
    File.WriteAllText(path, JsonSerializer.Serialize(newConfig, new JsonSerializerOptions { WriteIndented = true }));
    return newConfig;
}

bool IsLoopback(IPAddress? address)
{
    if (address is null)
        return false;

    if (IPAddress.IsLoopback(address))
        return true;

    return address.IsIPv4MappedToIPv6 && IPAddress.IsLoopback(address.MapToIPv4());
}

string? GetLanIpv4()
{
    try
    {
        return NetworkInterface.GetAllNetworkInterfaces()
            .Where(nic => nic.OperationalStatus == OperationalStatus.Up &&
                          nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
            .SelectMany(nic => nic.GetIPProperties().UnicastAddresses)
            .Select(x => x.Address)
            .FirstOrDefault(ip =>
                ip.AddressFamily == AddressFamily.InterNetwork &&
                !IPAddress.IsLoopback(ip) &&
                !ip.ToString().StartsWith("169.254.", StringComparison.Ordinal))
            ?.ToString();
    }
    catch
    {
        return null;
    }
}

void SafeLog(string message)
{
    try
    {
        File.AppendAllText(
            logPath,
            $"{DateTimeOffset.Now:O} {message}{Environment.NewLine}");
    }
    catch
    {
        // Logging must never stop clipboard sync.
    }
}

void TryOpenSetup()
{
    try
    {
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = $"http://127.0.0.1:{Port}/setup",
            UseShellExecute = true
        });
    }
    catch
    {
    }
}

string BuildSetupHtml(string endpoint, string? fallbackEndpoint, string token)
{
    var endpointHtml = WebUtility.HtmlEncode(endpoint);
    var fallbackHtml = fallbackEndpoint is null ? "" : WebUtility.HtmlEncode(fallbackEndpoint);
    var tokenHtml = WebUtility.HtmlEncode(token);

    return $$"""
<!doctype html>
<html lang="en">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width,initial-scale=1">
<title>Windows iOS Universal Clipboard</title>
<style>
:root{font-family:Inter,ui-sans-serif,system-ui,-apple-system,BlinkMacSystemFont,"Segoe UI",sans-serif;color:#111;background:#f7f7f8}
body{max-width:760px;margin:0 auto;padding:48px 20px 80px}
.card{background:#fff;border:1px solid #e4e4e7;border-radius:18px;padding:24px;margin-top:18px;box-shadow:0 8px 30px rgba(0,0,0,.04)}
h1{font-size:32px;letter-spacing:-.03em;margin:0 0 8px}
h2{font-size:19px;margin:0 0 12px}
p{line-height:1.55;color:#52525b}
code{display:block;overflow-wrap:anywhere;background:#f4f4f5;border-radius:10px;padding:12px;font-size:13px}
button{border:0;border-radius:10px;padding:10px 14px;font-weight:650;cursor:pointer;margin-top:10px}
.primary{background:#111;color:#fff}
.secondary{background:#e4e4e7;color:#111}
ol{padding-left:22px;line-height:1.7}
.ok{display:inline-block;background:#dcfce7;color:#166534;border-radius:999px;padding:5px 9px;font-size:12px;font-weight:700}
small{color:#71717a}
</style>
</head>
<body>
<span class="ok">Bridge is running</span>
<h1>Windows iOS Universal Clipboard</h1>
<p>Copy text on your iPhone, trigger the Shortcut, then paste on Windows with Ctrl+V. Everything stays on your local network.</p>

<div class="card">
<h2>1. Shortcut endpoint</h2>
<code id="endpoint">{{endpointHtml}}</code>
<button class="primary" onclick="copyText('endpoint',this)">Copy endpoint</button>
{{(fallbackEndpoint is null ? "" : $"""<p><small>If the .local address does not work, use this LAN address instead:</small></p><code id="fallback">{fallbackHtml}</code><button class="secondary" onclick="copyText('fallback',this)">Copy fallback endpoint</button>""")}}
</div>

<div class="card">
<h2>2. Access token</h2>
<code id="token">{{tokenHtml}}</code>
<button class="primary" onclick="copyText('token',this)">Copy token</button>
<p><small>Keep this token private. It only authorizes devices on your local network to write text to your Windows clipboard.</small></p>
</div>

<div class="card">
<h2>3. iPhone Shortcut</h2>
<ol>
<li>Add the shared <strong>Windows â†’ iOS Universal Clipboard</strong> Shortcut.</li>
<li>When iOS asks the import questions, paste the endpoint and token shown above.</li>
<li>Assign the Shortcut to <strong>Settings â†’ Accessibility â†’ Touch â†’ Back Tap</strong>.</li>
<li>Copy text on iPhone â†’ Back Tap â†’ Ctrl+V on Windows.</li>
</ol>
</div>

<script>
async function copyText(id,button){
  await navigator.clipboard.writeText(document.getElementById(id).innerText);
  const old=button.innerText; button.innerText='Copied'; setTimeout(()=>button.innerText=old,1200);
}
</script>
</body>
</html>
""";
}

record BridgeConfig(string Token);
record CopyRequest(string Text);

