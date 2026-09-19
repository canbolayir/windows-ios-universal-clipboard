import socket, struct, time

local_ip = "192.168.1.14"

def qname(name):
    out = bytearray()
    for label in name.split("."):
        b = label.encode("ascii")
        out.append(len(b))
        out.extend(b)
    out.append(0)
    return bytes(out)

query = struct.pack("!HHHHHH", 0, 0, 1, 0, 0, 0) + qname("copybridge.local") + struct.pack("!HH", 1, 1)

s = socket.socket(socket.AF_INET, socket.SOCK_DGRAM, socket.IPPROTO_UDP)
s.bind((local_ip, 0))
s.setsockopt(socket.IPPROTO_IP, socket.IP_MULTICAST_IF, socket.inet_aton(local_ip))
s.settimeout(3)
s.sendto(query, ("224.0.0.251", 5353))

deadline = time.time() + 3
while time.time() < deadline:
    try:
        data, addr = s.recvfrom(4096)
    except socket.timeout:
        break
    if b"copybridge" in data.lower():
        answer = ".".join(str(x) for x in data[-4:])
        print(f"MDNS_LAN_OK responder={addr[0]} answer={answer}")
        if answer != local_ip:
            raise SystemExit(2)
        raise SystemExit(0)

print("MDNS_LAN_FAIL")
raise SystemExit(1)
