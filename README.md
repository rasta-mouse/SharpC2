# SharpC2

Command and Control Framework written in C#.

## Getting Started

Download the latest Release packages.  These are built as self-contained assemblies which include the .NET runtime.

### Start the Team Server

```text
$ sudo ./teamserver <teamserver-ip> <shared-password>
Certificate thumbprint: 8E2D3F2B4F476A39188A4BBD9AD69DA92F9A2A66
```

### Start the Client

```text
$ ./SharpC2
  ____    _                               ____   ____
 / ___|  | |__     __ _   _ __   _ __    / ___| |___ \
 \___ \  | '_ \   / _` | | '__| | '_ \  | |       __) |
  ___) | | | | | | (_| | | |    | |_) | | |___   / __/
 |____/  |_| |_|  \__,_| |_|    | .__/   \____| |_____|
                                |_|
Host: <teamserver-ip>
Nick: <nick>
Pass: <shared-password>

Server Certificate
==================
Issuer: CN=192.168.1.229
Serial: 00A3F6F865BC648F10
Thumbprint: 8E2D3F2B4F476A39188A4BBD9AD69DA92F9A2A66
Not Before: Tue, 25 Oct 2022 13:50:50 GMT
Not After: Thu, 26 Oct 2023 13:50:50 GMT

Accept? [y/n] (y):
```

## Building

Use `dotnet publish` to build your own Release assemblies.  For example:

```text
dotnet publish -c Release --self-contained true -r linux-x64
```

This requires a .NET SDK.
