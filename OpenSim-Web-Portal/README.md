# OpenSim Web Portal

Modernes Web-Interface für OpenSimulator basierend auf Bootstrap 5.

## Features

✅ **Basis-Server** (implementiert):

- Webinterface auf Port 8100 (konfigurierbar)
- REST API Endpoint `/api/message` mit JSON Antwort
- Konsolenbefehle: `help`, `show status`, `shutdown`
- OpenSim-kompatibles Logging (log4net)
- BaseOpenSimServer Pattern
- Generischer Page Handler für alle HTML-Seiten

✅ **Alle Web-Seiten** (10 Templates erstellt):

**Öffentliche Seiten:**

- **Home** (`/`) - Startseite mit Grid-Statistiken und Feature-Cards
- **About** (`/portal/about`) - Projektinformationen und Credits
- **Login** (`/portal/login`) - Benutzer-Anmeldung mit Passwort-Toggle
- **Register** (`/portal/register`) - Registrierung mit Avatar-Auswahl
- **Passwort vergessen** (`/portal/forgot-password`) - Passwort-Reset

**Benutzer-Seiten:**

- **Account** (`/portal/account`) - Profil, Viewer-Verbindungsdaten, Status
- **Inventar** (`/portal/inventory`) - Browser mit Ordner-Baum, IAR Upload/Download

**Admin-Seiten:**

- **Benutzerverwaltung** (`/portal/admin/users`) - Tabelle, Suche, Filter, Actions
- **Web-Konsole** (`/portal/admin/console`) - Echtzeit-Console mit Command History

✅ **Design-System**:

- Bootstrap 5.3.2 Framework
- Bootstrap Icons 1.11.1
- Responsive Mobile-First Design
- Custom CSS mit Animationen und Hover-Effekten
- Konsistente Navigation mit Dropdowns

🔄 **Backend-Integration** (nächste Phase):

- User Authentication (Login/Logout)
- Session Management (Cookie-based)
- UserAccountService Integration
- InventoryService Integration
- GridService Integration
- Real-time Console Commands

## Installation

### 1. Prebuild ausführen

```powershell
cd D:\opensimcurrencyserver-dotnet-12_2025\opensim
.\runprebuild.bat
```

### 2. Kompilieren

```powershell
dotnet build addon-modules\OpenSim-Web-Portal\OpenSim.Web.Portal\OpenSim.Web.Portal.csproj -c Release
```

### 3. Konfiguration

Kopiere `WebPortal.ini.example` nach `bin/WebPortal.ini` und passe die Einstellungen an:

```ini
[WebPortal]
    Port = 8100
```

### 4. Templates kopieren

Kopiere die Template-Dateien nach `bin/portal/`:

```powershell
# Templates
mkdir bin\portal\templates
Copy-Item addon-modules\OpenSim-Web-Portal\websites\templates\* bin\portal\templates\

# CSS
mkdir bin\portal\css
Copy-Item addon-modules\OpenSim-Web-Portal\websites\css\* bin\portal\css\
```

### 5. Server starten

```powershell
cd bin
dotnet OpenSim.Web.Portal.dll
```

## Verwendung

### Web Interface

Öffne deinen Browser und besuche folgende Seiten:

```bash
# Hauptseite
http://localhost:8100

# Alle Seiten
http://localhost:8100/portal/login            # Login
http://localhost:8100/portal/register         # Registrierung
http://localhost:8100/portal/account          # Account-Verwaltung
http://localhost:8100/portal/inventory        # Inventar-Browser
http://localhost:8100/portal/forgot-password  # Passwort vergessen
http://localhost:8100/portal/about            # Über das Projekt
http://localhost:8100/portal/admin/users      # Admin: Benutzer
http://localhost:8100/portal/admin/console    # Admin: Web-Konsole
http://localhost:8100
```

### API Endpoint

Teste die REST API:

```bash
curl http://localhost:8100/api/message
```

Antwort:

```json
{
    "message": "Hello from OpenSim Web Portal!",
    "timestamp": "2025-12-13 22:00:00",
    "server": "OpenSim.Web.Portal",
    "version": "1.0.0",
    "status": "online",
    "uptime": "0d 0h 5m"
}
```

### Konsolen-Befehle

Im Server-Terminal kannst du folgende Befehle verwenden:

- **help** - Zeigt alle verfügbaren Befehle
- **show status** - Zeigt Server-Status und Uptime
- **shutdown** - Stoppt den Server

## Architektur

### Dateien

```bash
OpenSim-Web-Portal/
├── OpenSim.Web.Portal/
│   ├── WebPortalServer.cs          # Hauptserver-Klasse (532 Zeilen)
│   ├── WebPortal.ini.example       # Konfigurationsvorlage
│   └── WebPortal.log4net           # Logging-Konfiguration
├── websites/
│   ├── templates/                  # 10 HTML-Templates
│   │   ├── layout.html            # Master-Layout mit Navigation
│   │   ├── home.html              # Homepage mit Statistiken
│   │   ├── login.html             # Login-Formular
│   │   ├── register.html          # Registrierungs-Formular
│   │   ├── account.html           # Account-Verwaltung
│   │   ├── inventory.html         # Inventar-Browser
│   │   ├── forgot-password.html   # Passwort-Reset
│   │   ├── about.html             # Über-Seite
│   │   ├── admin-users.html       # Admin: Benutzerverwaltung
│   │   └── admin-console.html     # Admin: Web-Konsole
│   └── css/
│       └── custom.css             # Custom-Styles (300+ Zeilen)
├── WifiPages/                     # Referenz (alte Wifi-Templates)
├── README.md                      # Diese Datei
├── PAGES.md                       # Detaillierte Seiten-Dokumentation
└── prebuild-OpenSimWebPortal.xml  # Prebuild-Konfiguration
```

### Handler-Architektur

Der Server verwendet 4 HTTP-Handler:

1. **PortalHomeHandler** (`/`) - Homepage mit layout.html + home.html
2. **PortalApiHandler** (`/api/message`) - REST API Endpoint (JSON)
3. **PortalCssHandler** (`/portal/css/*`) - Statische CSS-Dateien
4. **PortalPageHandler** (generisch) - Alle anderen Seiten mit Template-System

Der **PortalPageHandler** ist generisch und serviert beliebige Seiten:

- Lädt `layout.html` als Master-Template
- Fügt spezifische Seite (z.B. `login.html`) als `{{CONTENT}}` ein
- Ersetzt alle Template-Variablen
- Registriert in `SetupHttpServer()` für jede Route

### Template-System

Einfaches aber mächtiges Platzhalter-System:

**Globale Variablen:**

- `{{GRID_NAME}}` - Grid-Name
- `{{CONTENT}}` - Seiteninhalt (nur in layout.html)

**Benutzer-Variablen:**

- `{{USER_FIRSTNAME}}`, `{{USER_LASTNAME}}` - Name
- `{{USER_EMAIL}}` - E-Mail-Adresse
- `{{USER_UUID}}` - Benutzer-UUID
- `{{USER_LEVEL}}` - Account-Level
- `{{CREATED_DATE}}`, `{{LAST_LOGIN}}` - Zeitstempel

**System-Variablen:**

- `{{LOGIN_URI}}` - Grid-Login-URI
- `{{TOTAL_USERS}}`, `{{ONLINE_USERS}}` - Statistiken
- `{{TOTAL_REGIONS}}`, `{{UPTIME}}` - Status

**Bedingte Blöcke** (noch nicht implementiert):

- `{{#IF_LOGGED_IN}}...{{/IF_LOGGED_IN}}` - Nur wenn eingeloggt
- `{{#IF_ADMIN}}...{{/IF_ADMIN}}` - Nur für Admins

Siehe [PAGES.md](PAGES.md) für vollständige Dokumentation!

## OpenSim Kompatibilität

✅ **Vollständig OpenSim-kompatibel**:

- BSD 3-Clause Lizenz
- BaseOpenSimServer Pattern
- OpenSim-Logging-Standards
- OpenSim-Konsolen-Integration
- OpenSim-Services-Integration
- XML-Dokumentationskommentare

Siehe [OpenSim-Addon-Example](../OpenSim-Addon-Example/) für weitere Implementierungsdetails.

## Entwicklung

### Weitere Handler hinzufügen

```csharp
// In WebPortalServer.cs -> SetupHttpServer()
m_httpServer.AddStreamHandler(new YourNewHandler());
```

### Neue Konsolen-Befehle

```csharp
// In RegisterConsoleCommands()
m_console.Commands.AddCommand("WebPortal", false, "your command",
    "your command",
    "Description",
    HandleYourCommand);
```

### Templates anpassen

Bearbeite die HTML-Dateien in `websites/templates/` und kopiere sie nach `bin/portal/templates/`.

## Roadmap

### Version 1.0 ✅ (Aktuell - 13.12.2025)

- ✅ Basis-Server mit BaseOpenSimServer Pattern
- ✅ Alle 10 HTML-Seiten komplett (Layout, Home, Login, Register, Account, Inventory, Forgot Password, About, Admin Users, Admin Console)
- ✅ Bootstrap 5.3.2 Design-System
- ✅ Generischer Page Handler
- ✅ REST API Endpoint
- ✅ Konsolen-Befehle
- ✅ Template-System mit Variablen
- ✅ Responsive Mobile-First Design
- ✅ Custom CSS mit Animationen

**Status:** ✅ Alle UI-Seiten fertig, bereit für Backend-Integration

### Version 1.1 (Nächste Phase)

**Backend-Integration:**

- [ ] Authentication Handler (Login/Logout mit UserAccountService)
- [ ] Session Management (Cookie-basierte Sessions)
- [ ] Registration Backend (Account-Erstellung)
- [ ] Password Recovery (E-Mail-basierter Reset)
- [ ] Account Edit (Profil-Bearbeitung)

**Geschätzte Dauer:** 2-3 Wochen

### Version 1.2 (Geplant)

**Inventory-Integration:**

- [ ] InventoryService Integration
- [ ] Inventory Browser mit echten Daten
- [ ] IAR Upload/Download Handler
- [ ] File-Handling für Assets
- [ ] Inventar-Suche

**Geschätzte Dauer:** 2-3 Wochen

### Version 1.3 (Geplant)

**Admin-Features:**

- [ ] GridService Integration (Regionen)
- [ ] Remote Console Commands
- [ ] Real-time Console Output (WebSocket/SignalR)
- [ ] User Administration Backend
- [ ] Statistiken und Monitoring

**Geschätzte Dauer:** 3-4 Wochen

### Version 2.0 (Zukunft)

**Erweiterte Features:**

- [ ] Groups Management
- [ ] Friends List
- [ ] IM-System über Web
- [ ] Asset Upload (Texturen, Sounds)
- [ ] Hypergrid-Integration
- [ ] Multi-Language Support

### Version 2.0 (Geplant)

- [ ] Admin Console im Web
- [ ] User Administration
- [ ] Group Management
- [ ] Access Control

## Lizenz

BSD 3-Clause License - Siehe OpenSimulator Projekt

## Credits

- Basiert auf OpenSim.Addon.Example
- Inspiriert von Diva Wifi
- UI Framework: Bootstrap 5.3.2
- Icons: Bootstrap Icons 1.11.1

## Support

Für Fragen und Issues siehe:

- OpenSimulator Wiki: <http://opensimulator.org/>
- OpenSim IRC: #opensim @ irc.libera.chat

---

**Status**: ✅ Basis-Server funktionsfähig | 🔄 Features in Entwicklung

**Version**: 1.0.0 - Initial Release
