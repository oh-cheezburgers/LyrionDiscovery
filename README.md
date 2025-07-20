# LyrionDiscovery

A .NET 8 library for discovering Lyrion Music Servers (formerly Logitech Media Server/Squeezebox Server) on the local network using UDP broadcast.

## Features

- Discover Lyrion Music Servers on your local network
- Retrieve server information including name, version, UUID, and ports

## Usage

```csharp
using LyrionDiscovery;
using LyrionDiscovery.UdpClient;

var discovery = new Discovery();
var udpClient = new UdpClientWrapper();
var timeout = TimeSpan.FromSeconds(5);
var cancellationToken = CancellationToken.None;

var servers = discovery.Discover(udpClient, timeout, cancellationToken);

foreach (var server in servers)
{
    Console.WriteLine($"Found server: {server.Name} at {server.IPAddress}");
    Console.WriteLine($"Version: {server.Version}");
    Console.WriteLine($"JSON Port: {server.Json}");
    Console.WriteLine($"CLI Port: {server.Clip}");
}
```

## Requirements

- .NET 8.0 or later

## Building

```bash
dotnet build
dotnet test
```
