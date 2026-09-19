using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;

var nic = NetworkInterface.GetAllNetworkInterfaces()
    .Where(n => n.OperationalStatus == OperationalStatus.Up)
    .Where(n => n.NetworkInterfaceType is NetworkInterfaceType.Ethernet or NetworkInterfaceType.Wireless80211)
    .Select(n => new { Nic = n, Props = n.GetIPProperties() })
    .Select(x => new
    {
        x.Nic,
        x.Props,
        Ip = x.Props.UnicastAddresses
            .FirstOrDefault(u => u.Address.AddressFamily == AddressFamily.InterNetwork &&
                                 !IPAddress.IsLoopback(u.Address))?.Address,
        Index = x.Props.GetIPv4Properties()?.Index ?? 0,
        HasGateway = x.Props.GatewayAddresses.Any(g => g.Address.AddressFamily == AddressFamily.InterNetwork &&
                                                       !g.Address.Equals(IPAddress.Any))
    })
    .FirstOrDefault(x => x.Ip is not null && x.HasGateway);

if (nic?.Ip is null)
    throw new Exception("No LAN interface found.");

Console.WriteLine($"LAN {nic.Ip} index={nic.Index} nic={nic.Nic.Name}");

using var reg = new BonjourARecord("copybridge.local.", nic.Ip, (uint)nic.Index);
reg.Start();

Console.WriteLine("READY");
Thread.Sleep(TimeSpan.FromSeconds(40));

sealed class BonjourARecord : IDisposable
{
    private const uint kDNSServiceFlagsUnique = 0x20;
    private const ushort kDNSServiceType_A = 1;
    private const ushort kDNSServiceClass_IN = 1;

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

    private readonly string _name;
    private readonly IPAddress _ip;
    private readonly uint _interfaceIndex;
    private IntPtr _connection;
    private IntPtr _record;
    private RegisterRecordReply? _callback;
    private Task? _processTask;
    private readonly ManualResetEventSlim _registered = new(false);
    private int _callbackError = int.MinValue;

    public BonjourARecord(string name, IPAddress ip, uint interfaceIndex)
    {
        _name = name;
        _ip = ip;
        _interfaceIndex = interfaceIndex;
    }

    public void Start()
    {
        var err = DNSServiceCreateConnection(out _connection);
        Console.WriteLine($"CREATE err={err} ref=0x{_connection.ToInt64():x}");
        if (err != 0 || _connection == IntPtr.Zero)
            throw new Exception($"DNSServiceCreateConnection failed: {err}");

        _callback = (sdRef, recordRef, flags, errorCode, context) =>
        {
            _callbackError = errorCode;
            Console.WriteLine($"CALLBACK error={errorCode}");
            _registered.Set();
        };

        var bytes = _ip.GetAddressBytes();
        err = DNSServiceRegisterRecord(
            _connection,
            out _record,
            kDNSServiceFlagsUnique,
            _interfaceIndex,
            _name,
            kDNSServiceType_A,
            kDNSServiceClass_IN,
            (ushort)bytes.Length,
            bytes,
            120,
            _callback,
            IntPtr.Zero);

        Console.WriteLine($"REGISTER err={err} record=0x{_record.ToInt64():x}");
        if (err != 0)
            throw new Exception($"DNSServiceRegisterRecord failed: {err}");

        _processTask = Task.Run(() =>
        {
            var result = DNSServiceProcessResult(_connection);
            Console.WriteLine($"PROCESS result={result}");
        });

        if (!_registered.Wait(TimeSpan.FromSeconds(5)))
            throw new TimeoutException("Record callback timeout.");

        if (_callbackError != 0)
            throw new Exception($"Record callback failed: {_callbackError}");
    }

    public void Dispose()
    {
        if (_connection != IntPtr.Zero)
        {
            DNSServiceRefDeallocate(_connection);
            _connection = IntPtr.Zero;
        }
        _registered.Dispose();
    }
}
