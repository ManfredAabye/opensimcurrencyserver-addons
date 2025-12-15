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

✅ **Authentifizierung & Sicherheit** (vollständig implementiert):

- **User Authentication** gegen OpenSim-Datenbank
- **Session Management** mit Cookie-basierten Sessions (30 Min. Timeout)
- **Login/Logout** mit automatischer Redirect-Funktionalität
- **User Registration** - erstellt echte OpenSim-Accounts
- **Passwort-Hashing** über OpenSim AuthenticationService (PBKDF2/MD5)
- **Geschützte Seiten** - automatische Auth-Prüfung mit Redirect zu Login
- **"Angemeldet bleiben"** - 30-Tage-Sessions
- **HttpOnly Cookies** - Schutz gegen XSS
- **Input-Validierung** - Server-seitige Prüfung aller Eingaben

✅ **Alle Web-Seiten** (23 Templates erstellt):

**Öffentliche Seiten:**

- **Home** (`/`) - Startseite mit Grid-Statistiken und Feature-Cards
- **About** (`/portal/about`) - Projektinformationen und Credits
- **Login** (`/portal/login`) - Benutzer-Anmeldung mit Passwort-Toggle
  - **POST /portal/login** - Login-Handler (Backend)
- **Register** (`/portal/register`) - Registrierung mit Avatar-Auswahl
  - **POST /portal/register** - Registrierungs-Handler (Backend)
- **Passwort vergessen** (`/portal/forgot-password`) - Passwort-Reset

**Geschützte Benutzer-Seiten** (Login erforderlich):

- **Account** (`/portal/account`) - Profil, Viewer-Verbindungsdaten, Status
- **Inventar** (`/portal/inventory`) - Browser mit Ordner-Baum, IAR Upload/Download
- **Passwort ändern** (`/password`) - Passwort-Änderung (Grid-Manager Integration)
- **Logout** (`/portal/logout`) - Session beenden

**Admin-Seiten:**

- **Benutzerverwaltung** (`/portal/admin/users`) - Tabelle, Suche, Filter, Actions
- **Web-Konsole** (`/portal/admin/console`) - Echtzeit-Console mit Command History

**Firestorm Viewer / Grid-Manager Seiten** (14 Seiten):

- `/welcome`, `/splash`, `/guide`, `/tos`, `/termsofservice`
- `/help`, `/economy`, `/gridstatus`, `/gridstatusrss` (RSS/XML)
- `/search`, `/avatars`, `/rss`, `/404`

✅ **Design-System**:

- Bootstrap 5.3.2 Framework
- Bootstrap Icons 1.11.1
- Responsive Mobile-First Design
- Custom CSS mit Animationen und Hover-Effekten
- Konsistente Navigation mit Dropdowns
- Einheitliches Orange/Black Theme (CSS Custom Properties)

✅ **Backend-Integration** (vollständig implementiert):

- **UserAccountService** - OpenSim User-Verwaltung
- **AuthenticationService** - OpenSim Passwort-Verifikation
- **SessionManager** - Thread-sichere Session-Verwaltung
- **ProtectedPageHandler** - Automatische Auth-Prüfung
- **MySQL/SQLite** - Datenbank-Anbindung konfigurierbar

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
    SessionTimeout = 30  ; Minuten

[DatabaseService]
    StorageProvider = "OpenSim.Data.MySQL.dll"
    ConnectionString = "Data Source=localhost;Database=opensim;User ID=opensim;Password=opensim_password;Old Guids=true;"

[UserAccountService]
    LocalServiceModule = "OpenSim.Services.UserAccountService.dll:UserAccountService"
    StorageProvider = "OpenSim.Data.MySQL.dll"
    ConnectionString = "Data Source=localhost;Database=opensim;User ID=opensim;Password=opensim_password;Old Guids=true;"

[AuthenticationService]
    LocalServiceModule = "OpenSim.Services.AuthenticationService.dll:PasswordAuthenticationService"
    StorageProvider = "OpenSim.Data.MySQL.dll"
    ConnectionString = "Data Source=localhost;Database=opensim;User ID=opensim;Password=opensim_password;Old Guids=true;"

[Const]
    GridName = "OpenSim Grid"
```

**Wichtig:**

- Passe den `ConnectionString` an deine Datenbank an!
- Verwendet die **gleiche Datenbank** wie OpenSim/Robust
- Login funktioniert mit **existierenden OpenSim-Accounts**
- Für SQLite statt MySQL: `StorageProvider = "OpenSim.Data.SQLite.dll"`

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

# Öffentliche Seiten
http://localhost:8100/portal/login            # Login (mit OpenSim-Account)
http://localhost:8100/portal/register         # Neuen Account erstellen
http://localhost:8100/portal/about            # Über das Projekt

# Geschützte Seiten (Login erforderlich)
http://localhost:8100/portal/account          # Account-Verwaltung
http://localhost:8100/portal/inventory        # Inventar-Browser
http://localhost:8100/password                # Passwort ändern
http://localhost:8100/portal/logout           # Logout

# Admin-Seiten
http://localhost:8100/portal/admin/users      # Admin: Benutzer
http://localhost:8100/portal/admin/console    # Admin: Web-Konsole

# Firestorm Viewer Seiten
http://localhost:8100/welcome                 # Welcome Page
http://localhost:8100/splash                  # Splash Screen
http://localhost:8100/gridstatus              # Grid Status
```

### Authentifizierung

**Neuen Account registrieren:**

1. Besuche `http://localhost:8100/portal/register`
2. Fülle das Formular aus:
   - Vorname (z.B. "Max")
   - Nachname (z.B. "Mustermann")
   - E-Mail
   - Passwort (mind. 6 Zeichen)
3. Account wird in OpenSim DB erstellt
4. Auto-Login nach Registrierung

**Mit existierendem OpenSim-Account einloggen:**

1. Besuche `http://localhost:8100/portal/login`
2. Eingabe:
   - Vorname
   - Nachname
   - Passwort (das gleiche wie im Viewer)
   - Optional: ☑ Angemeldet bleiben (30 Tage Session)
3. Redirect zu Account-Seite

**Geschützte Seiten:**

- Bei Zugriff ohne Login → automatischer Redirect zu `/portal/login?redirect=<original-url>`
- Nach Login → Redirect zurück zur ursprünglichen Seite
- Session läuft nach 30 Minuten Inaktivität ab (oder 30 Tage bei "Angemeldet bleiben")

**Logout:**

- Besuche `http://localhost:8100/portal/logout`
- Session wird zerstört
- Cookie gelöscht
- Redirect zur Startseite

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
│   ├── WebPortalServer.cs          # Hauptserver-Klasse (742 Zeilen)
│   ├── SessionManager.cs           # Session-Verwaltung (208 Zeilen)
│   ├── AuthenticationService.cs    # Auth gegen OpenSim DB (331 Zeilen)
│   ├── AuthHandlers.cs             # Login/Logout/Register Handler (285 Zeilen)
│   ├── ProtectedHandlers.cs        # Geschützte Seiten (329 Zeilen)
│   ├── WebPortal.ini.example       # Konfigurationsvorlage
│   └── WebPortal.log4net           # Logging-Konfiguration
├── websites/
│   ├── templates/                  # 23 HTML-Templates
│   │   ├── layout.html            # Master-Layout mit Navigation
│   │   ├── home.html              # Homepage mit Statistiken
│   │   ├── login.html             # Login-Formular
│   │   ├── register.html          # Registrierungs-Formular
│   │   ├── account.html           # Account-Verwaltung
│   │   ├── inventory.html         # Inventar-Browser
│   │   ├── forgot-password.html   # Passwort-Reset
│   │   ├── about.html             # Über-Seite
│   │   ├── admin-users.html       # Admin: Benutzerverwaltung
│   │   ├── admin-console.html     # Admin: Web-Konsole
│   │   ├── welcome.html           # Firestorm Welcome
│   │   ├── splash.html            # Firestorm Splash
│   │   ├── guide.html             # Destination Guide
│   │   ├── tos.html               # TOS Form
│   │   ├── termsofservice.html    # TOS Read-only
│   │   ├── help.html              # Help & Support
│   │   ├── economy.html           # Economy Info
│   │   ├── password.html          # Password Change
│   │   ├── gridstatus.html        # Grid Status
│   │   ├── gridstatusrss.xml      # RSS Feed (XML)
│   │   ├── search.html            # Grid Search
│   │   ├── avatars.html           # Avatar Picker
│   │   ├── rss.html               # RSS Info
│   │   └── 404.html               # Error Page
│   └── css/
│       └── style.css              # Unified CSS (323 Zeilen, CSS Custom Properties)
├── WifiPages/                     # Referenz (alte Wifi-Templates)
├── README.md                      # Diese Datei
├── PAGES.md                       # Detaillierte Seiten-Dokumentation
├── AUTHENTICATION.md              # Vollständige Auth-Dokumentation
└── prebuild-OpenSimWebPortal.xml  # Prebuild-Konfiguration
```

### Handler-Architektur

Der Server verwendet mehrere spezialisierte HTTP-Handler:

**Basis-Handler:**

1. **PortalHomeHandler** (`/`) - Homepage mit layout.html + home.html
2. **PortalApiHandler** (`/api/message`) - REST API Endpoint (JSON)
3. **PortalCssHandler** (`/portal/css/*`) - Statische CSS-Dateien
4. **PortalPageHandler** (generisch) - Öffentliche Seiten mit Template-System

**Authentifizierungs-Handler:**
5. **LoginHandler** (`POST /portal/login`) - Verarbeitet Login-Formulare
6. **LogoutHandler** (`GET /portal/logout`) - Beendet Sessions
7. **RegisterHandler** (`POST /portal/register`) - Erstellt neue Accounts

**Geschützte Handler (Auth erforderlich):**
8. **AccountPageHandler** (`GET /portal/account`) - Account-Seite
9. **InventoryPageHandler** (`GET /portal/inventory`) - Inventar-Seite
10. **PasswordPageHandler** (`GET /password`) - Passwort-Änderung

**ProtectedPageHandler Basis-Klasse:**

- Prüft automatisch Session-Cookie bei jedem Request
- Redirect zu `/portal/login?redirect=<url>` wenn nicht authentifiziert
- Extrahiert User-Daten aus Session für Template-Rendering
- Alle geschützten Handler erben von dieser Klasse

### Template-System

Einfaches aber mächtiges Platzhalter-System:

**Globale Variablen:**

- `{{GRID_NAME}}` - Grid-Name
- `{{CONTENT}}` - Seiteninhalt (nur in layout.html)
- `{{HEAD_EXTRA}}` - Zusätzliche Head-Tags
- `{{SCRIPT_EXTRA}}` - Zusätzliche Scripts

**Benutzer-Variablen (Session-basiert):**

- `{{USER_FIRSTNAME}}`, `{{USER_LASTNAME}}` - Name aus Session
- `{{USER_FULLNAME}}` - Vollständiger Name
- `{{USER_EMAIL}}` - E-Mail-Adresse
- `{{USER_UUID}}` - Benutzer-UUID (OpenSim)
- `{{USER_LEVEL}}` - Account-Level (0=User, 100=God, etc.)
- `{{LAST_LOGIN}}` - Zeitpunkt des Logins
- `{{USER_BALANCE}}` - Currency-Balance (wenn verfügbar)

**System-Variablen:**

- `{{LOGIN_URI}}` - Grid-Login-URI
- `{{TOTAL_USERS}}`, `{{ONLINE_USERS}}` - Statistiken
- `{{TOTAL_REGIONS}}`, `{{UPTIME}}` - Status

**Bedingte Blöcke (implementiert):**

- `{{#IF_LOGGED_IN}}...{{/IF_LOGGED_IN}}` - Nur wenn eingeloggt
- `{{#IF_NOT_LOGGED_IN}}...{{/IF_NOT_LOGGED_IN}}` - Nur für Gäste
- `{{ELSE}}` - Alternative für bedingte Blöcke

**Alert-System:**

- `{{#ALERTS}}...{{/ALERTS}}` - Zeigt Alert-Meldungen
- Fehler-Meldungen bei Login/Register über Query-Parameter

Siehe [PAGES.md](PAGES.md) und [AUTHENTICATION.md](AUTHENTICATION.md) für vollständige Dokumentation!

## OpenSim Kompatibilität

✅ **Vollständig OpenSim-kompatibel**:

- BSD 3-Clause Lizenz
- BaseOpenSimServer Pattern
- OpenSim-Logging-Standards
- OpenSim-Konsolen-Integration
- **IUserAccountService** - Standard OpenSim User-Verwaltung
- **IAuthenticationService** - Standard OpenSim Authentifizierung
- **Gleiche Datenbank** - Verwendet OpenSim `UserAccounts` und `auth` Tabellen
- **Passwort-Kompatibilität** - Gleiche Hashing-Algorithmen wie OpenSim/Robust
- **Service-Konfiguration** - Identisch zu Robust.ini Format
- XML-Dokumentationskommentare

**Login funktioniert mit existierenden OpenSim-Accounts - keine separate User-DB nötig!**

Siehe [OpenSim-Addon-Example](../OpenSim-Addon-Example/) und [AUTHENTICATION.md](AUTHENTICATION.md) für weitere Details.

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

### Version 1.0 ✅ (Abgeschlossen - 13.12.2025)

- ✅ Basis-Server mit BaseOpenSimServer Pattern
- ✅ Alle 23 HTML-Seiten komplett (Portal + Firestorm Viewer Integration)
- ✅ Bootstrap 5.3.2 Design-System mit einheitlichem Theme
- ✅ Generischer Page Handler
- ✅ REST API Endpoint
- ✅ Konsolen-Befehle
- ✅ Template-System mit Variablen
- ✅ Responsive Mobile-First Design
- ✅ Custom CSS mit Animationen (CSS Custom Properties)

**Status:** ✅ Vollständiges UI-System

### Version 1.1 ✅ (Abgeschlossen - 15.12.2025)

**Backend-Integration & Authentifizierung:**

- ✅ Authentication Handler (Login/Logout mit UserAccountService)
- ✅ Session Management (Cookie-basierte Sessions mit 30 Min. Timeout)
- ✅ Registration Backend (Account-Erstellung in OpenSim DB)
- ✅ Protected Page Handler (automatische Auth-Prüfung)
- ✅ OpenSim IUserAccountService Integration
- ✅ OpenSim IAuthenticationService Integration
- ✅ Passwort-Hashing (OpenSim-kompatibel)
- ✅ "Angemeldet bleiben" (30-Tage-Sessions)
- ✅ Vollständige Dokumentation (AUTHENTICATION.md)

**Status:** ✅ Produktionsbereit für User-Management

### Version 1.2 (Nächste Phase)

**Erweiterte Auth-Features:**

- [ ] Password Recovery (E-Mail-basierter Reset mit Token-System)
- [ ] Account Edit (Profil-Bearbeitung: Email, Bio, Avatar)
- [ ] Change Password POST Handler (Backend für Passwort-Änderung)
- [ ] Rate Limiting (Login-Versuche limitieren)
- [ ] CAPTCHA Integration (Schutz gegen Bots)

**Geschätzte Dauer:** 1-2 Wochen

### Version 1.3 (Geplant)

**Inventory-Integration:**

- [ ] InventoryService Integration
- [ ] Inventory Browser mit echten Daten
- [ ] IAR Upload/Download Handler
- [ ] File-Handling für Assets
- [ ] Inventar-Suche und Filter

**Geschätzte Dauer:** 2-3 Wochen

### Version 1.4 (Geplant)

**Admin-Features:**

- [ ] GridService Integration (Regionen anzeigen/verwalten)
- [ ] Remote Console Commands (Befehle an OpenSim senden)
- [ ] Real-time Console Output (WebSocket/SignalR)
- [ ] User Administration Backend (Ban, UserLevel ändern)
- [ ] Statistiken und Monitoring Dashboard
- [ ] Admin-Level Auth-Check

**Geschätzte Dauer:** 3-4 Wochen

### Version 2.0 (Zukunft)

**Erweiterte Features:**

- [ ] Groups Management (Gruppen erstellen/verwalten)
- [ ] Friends List (Freunde anzeigen/verwalten)
- [ ] IM-System über Web (Instant Messaging)
- [ ] Asset Upload (Texturen, Sounds, Animationen)
- [ ] Hypergrid-Integration (HG-User-Support)
- [ ] Multi-Language Support (i18n)
- [ ] Currency-System Integration (Balance, Transactions)
- [ ] MoneyServer Web-Interface (Payment History)

### Version 3.0 (Vision)

**Enterprise Features:**

- [ ] OAuth2/OpenID Connect Integration
- [ ] Two-Factor Authentication (2FA)
- [ ] API Token Management für externe Apps
- [ ] Webhook-System für Events
- [ ] Advanced Analytics Dashboard
- [ ] Mobile App (Progressive Web App)

## Lizenz

BSD 3-Clause License - Siehe OpenSimulator Projekt

## Credits

- Basiert auf OpenSim.Addon.Example
- Inspiriert von Diva Wifi
- UI Framework: Bootstrap 5.3.2
- Icons: Bootstrap Icons 1.11.1

## Sicherheitshinweise

### Produktiv-Umgebung

Für den Produktiv-Einsatz beachte:

⚠️ **HTTPS verwenden** - Setze einen Reverse-Proxy (nginx/Apache) mit SSL/TLS vor das Web Portal
⚠️ **Firewall konfigurieren** - Nur benötigte Ports freigeben
⚠️ **Starke Passwörter** - Passwort-Mindestlänge erhöhen (aktuell 6 Zeichen)
⚠️ **Rate Limiting** - Login-Versuche limitieren (noch nicht implementiert)
⚠️ **Session-Secret** - Verwende sichere Session-IDs (bereits implementiert mit 32 Bytes Random)
⚠️ **Datenbank-Sicherheit** - Separater DB-User mit minimalen Rechten

Siehe [AUTHENTICATION.md](AUTHENTICATION.md) für detaillierte Sicherheits-Dokumentation.

## Troubleshooting

### "AuthenticationService is null"

- Prüfe `WebPortal.ini` Konfiguration
- Prüfe Datenbank-Verbindung (ConnectionString)
- Stelle sicher, dass OpenSim Services-DLLs vorhanden sind

### "User not found"

- User muss in OpenSim DB existieren
- Registriere neuen User über `/portal/register`
- Oder erstelle User über OpenSim Console

### Session läuft sofort ab

- Prüfe `SessionTimeout` in `WebPortal.ini`
- Prüfe Browser-Cookies (DevTools → Application → Cookies)
- Suche nach `OPENSIM_SESSION` Cookie

### Port bereits belegt

- Standard: Port 8100
- Ändere in `WebPortal.ini`: `Port = <anderer-port>`
- Vermeide: 9000 (OpenSim), 8002 (Robust), 8008 (MoneyServer)

Weitere Hilfe: [AUTHENTICATION.md - Troubleshooting](AUTHENTICATION.md#troubleshooting)

## Support

Für Fragen und Issues siehe:

- OpenSimulator Wiki: <http://opensimulator.org/>
- OpenSim IRC: #opensim @ irc.libera.chat

**Dokumentation:**

- [README.md](README.md) - Hauptdokumentation (diese Datei)
- [AUTHENTICATION.md](AUTHENTICATION.md) - Vollständige Auth-Dokumentation
- [PAGES.md](PAGES.md) - Seiten-Übersicht und Template-Variablen
- [QUICKSTART.md](QUICKSTART.md) - Schnellstart-Guide

---

**Status**: ✅ Produktionsbereit | ✅ Vollständige Authentifizierung

**Version**: 1.1.0 - Auth & Session Management (15.12.2025)
