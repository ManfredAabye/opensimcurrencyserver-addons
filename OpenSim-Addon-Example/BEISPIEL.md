# OpenSim Example Addon - Dokumentation

## 🚀 Übersicht

Dieses Addon ist ein vollständiges Beispiel für die Entwicklung von OpenSimulator Addon-Modulen. Es demonstriert:

- ✅ **Konsolen-Integration** - "Hallo World" Ausgabe beim Start
- ✅ **Web-Interface** - Moderne HTML5-Seite mit "Hello OpenSim User!"
- ✅ **REST API** - JSON-basierter API Endpoint
- ✅ **Custom Commands** - Eigene Konsolen-Befehle
- ✅ **HTTP Server** - BaseHttpServer Integration
- ✅ **Konfiguration** - INI-Datei Support
- ✅ **Logging** - log4net Integration

## 📁 Projekt-Struktur

```bash
OpenSim-Addon-Example/
├── README.md                                 # Dieses Dokument
├── prebuild-OpenSimAddonExample.xml          # Prebuild XML für Projekterstellung
│
└── OpenSim.Addon.Example/
    └── ExampleServer.cs                      # Haupt-Server-Klasse mit:
        ├── ExampleServer (BaseOpenSimServer) # Hauptklasse
        ├── ExamplePageHandler                # HTML-Seite Handler
        └── ExampleApiHandler                 # API Endpoint Handler

Konfiguration in bin/:
├── ExampleServer.ini.example                 # Konfigurations-Vorlage
└── ExampleServer.log4net                     # log4net Konfiguration
```

## 🔧 Installation & Build

### 1. Prebuild ausführen

```bash
cd D:\opensimcurrencyserver-dotnet-12_2025\opensim
.\runprebuild.bat
```

### 2. Kompilieren

```bash
dotnet build OpenSim.sln -c Release
```

**Oder nur das Example Addon:**

```bash
dotnet build addon-modules\OpenSim-Addon-Example\OpenSim.Addon.Example\OpenSim.Addon.Example.csproj -c Release
```

### 3. Konfiguration (optional)

```bash
cd bin
copy ExampleServer.ini.example ExampleServer.ini
# ExampleServer.ini anpassen falls Port ändern
```

### 4. Starten

```bash
cd bin
dotnet OpenSim.Addon.Example.dll
```

## 💻 Verwendung

### Konsolen-Ausgabe

Beim Start erscheint in der Konsole:

```bash
===========================================
  HALLO WORLD - OpenSim Example Addon!
===========================================
12:00:06 - [EXAMPLE SERVER]: Hallo World
12:00:06 - [EXAMPLE SERVER]: Web Interface: http://localhost:9000
12:00:06 - [EXAMPLE SERVER]: Server startup complete
```

### Konsolen-Befehle

| Befehl | Beschreibung |
|--------|--------------|
| `hello` | Gibt "Hallo World" aus |
| `show status` | Zeigt Server-Status (Port, URL) |
| `shutdown` | Beendet den Server (vererbt) |

**Beispiel:**

```bash
ExampleServer# hello
[EXAMPLE SERVER]: Hallo World - Befehl ausgeführt!
*** HALLO WORLD ***

ExampleServer# show status
[EXAMPLE SERVER]: Server läuft auf Port 9000
[EXAMPLE SERVER]: Web Interface: http://localhost:9000
```

### Web-Interface

**URL:** <http://localhost:9000>

**Features:**

- 🚀 Modernes, responsives Design
- 📋 Server-Informationen (Name, Port, Status)
- 🔌 Interaktiver API-Test Button
- 🎨 Gradient-Design mit Animations

**Screenshot (Text-Version):**

```bash
╔══════════════════════════════════════╗
║   🚀                                 ║
║   OpenSim Example Addon              ║
║   Hello OpenSim User!                ║
║                                      ║
║   📋 Addon Informationen             ║
║   Name: OpenSim.Addon.Example        ║
║   Port: 9000                         ║
║   Status: ✅ Aktiv                   ║
║   Konsolen-Ausgabe: Hallo World      ║
║                                      ║
║   🔌 API Test                        ║
║   [API abrufen]                      ║
╚══════════════════════════════════════╝
```

### REST API

**Endpoint:** `GET /api/message`

**Response:**

```json
{
    "message": "Hello OpenSim User from API!",
    "timestamp": "2025-12-13 12:00:06",
    "server": "OpenSim.Addon.Example",
    "status": "success"
}
```

**cURL Beispiel:**

```bash
curl http://localhost:9000/api/message
```

**Browser-Test:**
Auf der Hauptseite auf "API abrufen" klicken.

## 🛠️ Entwicklung

### Architektur

Das Addon basiert auf **BaseOpenSimServer** und verwendet:

1. **ExampleServer** (Hauptklasse)
   - Erbt von `BaseOpenSimServer`
   - Implementiert `Startup()` Methode
   - Verwaltet HTTP Server und Konsole

2. **Handler-Klassen** (Namespace: OpenSim.Addon.Example.Handlers)
   - `ExamplePageHandler` - Serviert HTML-Seite
   - `ExampleApiHandler` - Serviert JSON API
   - Beide erben von `BaseStreamHandler`

### Code-Struktur

```csharp
// Handler für HTTP Requests
public class ExamplePageHandler : BaseStreamHandler
{
    public ExamplePageHandler() : base("GET", "/") { }
    
    protected override byte[] ProcessRequest(...)
    {
        // HTML zurückgeben
        httpResponse.ContentType = "text/html";
        return Encoding.UTF8.GetBytes(html);
    }
}

// Haupt-Server
public class ExampleServer : BaseOpenSimServer
{
    public override void Startup()
    {
        // 1. Konfiguration laden
        // 2. HTTP Server starten
        // 3. Handler registrieren
        // 4. Konsolen-Loop starten
    }
}
```

### Neue Features hinzufügen

**1. Neuer Konsolen-Befehl:**

```csharp
m_console.Commands.AddCommand("Example", false, "mein befehl",
    "mein befehl", "Beschreibung",
    (module, args) => {
        m_log.Info("Mein Befehl ausgeführt!");
    });
```

**2. Neuer HTTP Endpoint:**

```csharp
public class MeinHandler : BaseStreamHandler
{
    public MeinHandler() : base("GET", "/mein/pfad") { }
    
    protected override byte[] ProcessRequest(...)
    {
        // Response zurückgeben
    }
}

// In RegisterWebsiteHandlers():
m_httpServer.AddStreamHandler(new MeinHandler());
```

**3. Konfiguration hinzufügen:**

```ini
# In ExampleServer.ini
[ExampleServer]
Port = 9000
MeineOption = "Wert"
```

```csharp
// In ReadIniConfig():
string meineOption = serverConfig.GetString("MeineOption", "Default");
```

## 📚 Verwendete Patterns

### 1. BaseOpenSimServer Pattern

- Vererbt grundlegende Server-Funktionalität
- Implementiert `Startup()` für Initialisierung
- `Work()` für Konsolen-Loop

### 2. BaseStreamHandler Pattern

- Handler für HTTP Requests
- `ProcessRequest()` gibt byte[] zurück
- Saubere Trennung von Routes

### 3. LocalConsole Pattern

- Integration in OpenSim Konsolen-System
- `m_console.Commands.AddCommand()` für Befehle
- `m_console.Prompt()` für interaktive Eingabe

## 🔍 Debugging

**Log-Datei:** `bin/ExampleServer.log`

**Log-Level ändern:**

```xml
<!-- In ExampleServer.log4net -->
<logger name="OpenSim.Addon.Example">
    <level value="DEBUG" />  <!-- INFO, DEBUG, WARN, ERROR -->
</logger>
```

**Console-Ausgabe:**
Alle Logs erscheinen auch in der Konsole mit Zeitstempel.

## 🎓 Lernziele

Dieses Addon zeigt:

| Konzept | Implementierung |
|---------|----------------|
| **Server-Initialisierung** | `Startup()` Methode |
| **HTTP Server** | `BaseHttpServer` + `AddStreamHandler()` |
| **Konsolen-Integration** | `LocalConsole` + `AddCommand()` |
| **Konfiguration** | `IniConfigSource` + `IConfig` |
| **Logging** | `log4net` + `ILog` |
| **HTTP Handler** | `BaseStreamHandler` + `ProcessRequest()` |
| **HTML Serving** | String → byte[] → Response |
| **JSON API** | String.Format + JSON string |

## 📖 Weiterführende Ressourcen

### Vergleich mit anderen Addons

**OpenSim.Console.Api:**

- Komplexeres Beispiel mit Authentifizierung
- WebSocket Handler
- JSON-Konfiguration
- Token-basierte Sicherheit

**OpenSim.Money.Accounting:**

- Datenbank-Integration (MySQL)
- Mehrere API Endpoints
- CSS/JS Dateien als separate Dateien
- Dashboard mit Charts

### OpenSimulator Dokumentation

- Wiki: <http://opensimulator.org/wiki/>
- GitHub: <https://github.com/opensim/opensim>
- Forums: <http://opensimulator.org/viewforum.php>

## 🚦 Status & Features

- ✅ Kompiliert erfolgreich (.NET 8.0)
- ✅ Server startet auf Port 9000
- ✅ Konsolen-Ausgabe "Hallo World"
- ✅ Web-Interface zeigt "Hello OpenSim User!"
- ✅ API Endpoint funktioniert
- ✅ Custom Commands funktionieren
- ✅ Log4net Logging aktiv
- ✅ INI Konfiguration unterstützt

## 📝 Lizenz

Siehe OpenSimulator Haupt-Lizenz (BSD 3-Clause)

## 👨‍💻 Entwicklung

**Erstellt:** 13. Dezember 2025  
**Framework:** .NET 8.0  
**OpenSimulator Version:** 0.9.3+  
**Basierend auf:** OpenSim.Console.Api & OpenSim.Money.Accounting

---

Happy Coding! 🚀
