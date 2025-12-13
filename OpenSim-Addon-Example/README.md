# OpenSim Example Addon

Ein Beispiel-Addon-Modul für OpenSimulator, das die Grundlagen der Addon-Entwicklung demonstriert.

## Features

### Konsolen-Funktionen

- ✅ Gibt "Hallo World" beim Start in der Konsole aus
- ✅ Benutzerdefinierte Konsolen-Befehle:
  - `hello` - Gibt "Hallo World" aus
  - `show status` - Zeigt den Server-Status

### Web-Interface

- ✅ Einfache HTML-Seite auf Port 9000
- ✅ Zeigt "Hello OpenSim User!" an
- ✅ Modernes, responsives Design
- ✅ REST API Endpoint `/api/message`
- ✅ Interaktive API-Tests im Browser

## Installation

### 1. Build

```bash
# Prebuild ausführen
runprebuild.bat

# Kompilieren
dotnet build OpenSim.sln
```

### 2. Konfiguration

```bash
# Beispiel-Konfiguration kopieren
cd bin
copy ExampleServer.ini.example ExampleServer.ini
```

### 3. Starten

```bash
cd bin
dotnet OpenSim.Addon.Example.dll
```

## Konfiguration

**ExampleServer.ini:**

```ini
[ExampleServer]
Port = 9000
```

**Log-Konfiguration:**

- Datei: `ExampleServer.log4net`
- Logs: `ExampleServer.log` (tägliche Rotation)

## Web Interface

Nach dem Start ist das Web-Interface verfügbar unter:

- **URL:** <http://localhost:9000>
- **API:** <http://localhost:9000/api/message>

## Konsolen-Befehle

| Befehl | Beschreibung |
|--------|--------------|
| `hello` | Gibt "Hallo World" in der Konsole aus |
| `show status` | Zeigt Server-Status und Port |
| `shutdown` | Beendet den Server |

## Entwicklung

Dieses Addon basiert auf:

- **OpenSim.Console.Api** - HTTP Server und Konsolen-Integration
- **OpenSim.Money.Accounting** - Web-Interface Struktur

### Architektur

```bash
OpenSim.Addon.Example/
├── ExampleServer.cs          # Haupt-Server-Klasse
├── prebuild-*.xml            # Build-Konfiguration
└── README.md                 # Dokumentation

bin/
├── ExampleServer.ini.example # Konfigurations-Vorlage
└── ExampleServer.log4net     # Log4Net Konfiguration
```

### Erweiterung

**Neue Konsolen-Befehle:**

```csharp
m_console.Commands.AddCommand("Example", false, "mein befehl",
    "mein befehl", "Beschreibung",
    HandleMeinBefehl);
```

**Neue HTTP Handler:**

```csharp
m_httpServer.AddHTTPHandler("/mein/pfad", HandleMeinPfad);
```

**Neue API Endpoints:**

```csharp
private void HandleMeinEndpoint(string path, Stream request, Stream response, 
    IOSHttpRequest httpRequest, IOSHttpResponse httpResponse)
{
    httpResponse.ContentType = "application/json";
    // JSON zurückgeben
}
```

## Technische Details

- **Framework:** .NET 8.0
- **HTTP Server:** BaseHttpServer (OpenSim.Framework.Servers.HttpServer)
- **Logging:** log4net
- **Konsole:** OpenSim.Framework.Console
- **Base Class:** BaseOpenSimServer

## Lizenz

Siehe OpenSimulator Haupt-Lizenz (BSD)
