using System.Runtime.InteropServices;

const uint DNS_REQUEST_PENDING = 9506;
const uint DNS_QUERY_REQUEST_VERSION1 = 1;

var registration = new Registration();
registration.Start();

Console.WriteLine("READY");
Thread.Sleep(TimeSpan.FromSeconds(40));

sealed class Registration : IDisposable
{
    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate void RegisterComplete(uint status, IntPtr context, IntPtr instance);

    [StructLayout(LayoutKind.Sequential)]
    private struct DnsServiceInstance
    {
        public IntPtr pszInstanceName;
        public IntPtr pszHostName;
        public IntPtr ip4Address;
        public IntPtr ip6Address;
        public ushort wPort;
        public ushort wPriority;
        public ushort wWeight;
        public uint dwPropertyCount;
        public IntPtr keys;
        public IntPtr values;
        public uint dwInterfaceIndex;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct DnsServiceRegisterRequest
    {
        public uint Version;
        public uint InterfaceIndex;
        public IntPtr pServiceInstance;
        public IntPtr pRegisterCompletionCallback;
        public IntPtr pQueryContext;
        public IntPtr hCredentials;
        public int unicastEnabled;
    }

    [DllImport("dnsapi.dll", CallingConvention = CallingConvention.Winapi)]
    private static extern uint DnsServiceRegister(
        ref DnsServiceRegisterRequest request,
        IntPtr cancel);

    private IntPtr _serviceName;
    private IntPtr _hostName;
    private IntPtr _keys;
    private IntPtr _values;
    private IntPtr _instance;
    private RegisterComplete? _callback;
    private ManualResetEventSlim _done = new(false);
    private uint _callbackStatus = uint.MaxValue;

    public void Start()
    {
        _serviceName = Marshal.StringToHGlobalUni("CopyBridge._http._tcp.local");
        _hostName = Marshal.StringToHGlobalUni("copybridge.local");

        _keys = Marshal.AllocHGlobal(IntPtr.Size);
        _values = Marshal.AllocHGlobal(IntPtr.Size);
        Marshal.WriteIntPtr(_keys, IntPtr.Zero);
        Marshal.WriteIntPtr(_values, IntPtr.Zero);

        var instance = new DnsServiceInstance
        {
            pszInstanceName = _serviceName,
            pszHostName = _hostName,
            ip4Address = IntPtr.Zero,
            ip6Address = IntPtr.Zero,
            wPort = 8765,
            wPriority = 0,
            wWeight = 0,
            dwPropertyCount = 1,
            keys = _keys,
            values = _values,
            dwInterfaceIndex = 0
        };

        _instance = Marshal.AllocHGlobal(Marshal.SizeOf<DnsServiceInstance>());
        Marshal.StructureToPtr(instance, _instance, false);

        _callback = (status, context, registered) =>
        {
            _callbackStatus = status;
            Console.WriteLine($"CALLBACK status={status} instance=0x{registered.ToInt64():x}");
            _done.Set();
        };

        var request = new DnsServiceRegisterRequest
        {
            Version = 1,
            InterfaceIndex = 0,
            pServiceInstance = _instance,
            pRegisterCompletionCallback = Marshal.GetFunctionPointerForDelegate(_callback),
            pQueryContext = IntPtr.Zero,
            hCredentials = IntPtr.Zero,
            unicastEnabled = 0
        };

        var result = DnsServiceRegister(ref request, IntPtr.Zero);
        Console.WriteLine($"REGISTER result={result}");

        if (result != 9506)
            throw new InvalidOperationException($"DnsServiceRegister returned {result}");

        if (!_done.Wait(TimeSpan.FromSeconds(5)))
            throw new TimeoutException("Registration callback timeout");

        if (_callbackStatus != 0)
            throw new InvalidOperationException($"Registration callback failed: {_callbackStatus}");
    }

    public void Dispose()
    {
        if (_instance != IntPtr.Zero) Marshal.FreeHGlobal(_instance);
        if (_keys != IntPtr.Zero) Marshal.FreeHGlobal(_keys);
        if (_values != IntPtr.Zero) Marshal.FreeHGlobal(_values);
        if (_serviceName != IntPtr.Zero) Marshal.FreeHGlobal(_serviceName);
        if (_hostName != IntPtr.Zero) Marshal.FreeHGlobal(_hostName);
        _done.Dispose();
    }
}
