import socket, struct, time

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
s.settimeout(3)
s.sendto(query, ("224.0.0.251", 5353))

deadline = time.time() + 3
found = False
while time.time() < deadline:
    try:
        data, addr = s.recvfrom(4096)
    except socket.timeout:
        break
    if b"copybridge" in data.lower() and len(data) >= 4:
        # IPv4 A record is the last 4 bytes in our minimal response
        ip = ".".join(str(x) for x in data[-4:])
        print(f"MDNS_OK responder={addr[0]} answer={ip}")
        found = True
        break

if not found:
    print("MDNS_FAIL")
    raise SystemExit(1)
