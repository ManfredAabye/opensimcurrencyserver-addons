# OpenSim Web Portal - Seitenübersicht

## 📄 Alle verfügbaren Seiten

### Firestorm Viewer Integration (GridInfoService)

- **Welcome** - `/welcome` - Hauptseite für Firestorm Viewer (welcome parameter)
- **Splash** - `/splash` - Splash-Seite mit Sidebar-Navigation
- **Guide** - `/guide` - Destination Guide für V3 Viewer (DestinationGuide parameter)
- **Terms of Service (Form)** - `/tos` - Nutzungsbedingungen mit Accept/Decline Formular
- **Terms of Service (Read-only)** - `/termsofservice` - Nutzungsbedingungen nur lesen
- **RSS Info** - `/rss` - RSS Feed Informationsseite
- **Help** - `/help` - Hilfeseite mit FAQ
- **404** - `/404` - Custom Error-Seite

### Grid-Manager Integration (GridInfoService)

- **Economy** - `/economy` - Wirtschafts-/Währungsinformationen (economy parameter)
- **Password Recovery** - `/password` - Passwort-Wiederherstellung (password parameter)
- **Grid Status** - `/gridstatus` - Grid-Status Monitor mit Auto-Refresh (GridStatus parameter)
- **Grid Status RSS** - `/gridstatusrss` - Grid-Status als RSS 2.0 XML Feed (GridStatusRSS parameter)
- **Search** - `/search` - Grid-Suche für Places, People, Events, Groups (SearchURL parameter)
- **Avatar Picker** - `/avatars` - Avatar-Auswahl für V3 Viewer (AvatarPicker parameter)

### Öffentliche Seiten

- **Home** - `/` - Startseite mit Übersicht und Statistiken
- **About** - `/portal/about` - Über das Projekt, Features, Credits
- **Login** - `/portal/login` - Benutzer-Anmeldung
- **Register** - `/portal/register` - Neue Konten registrieren
- **Passwort vergessen** - `/portal/forgot-password` - Passwort-Reset

### Benutzer-Seiten (Login erforderlich)

- **Account** - `/portal/account` - Account-Informationen und Viewer-Verbindungsdaten
- **Inventar** - `/portal/inventory` - Inventar-Browser mit IAR Upload/Download

### Admin-Seiten (Admin-Rechte erforderlich)

- **Benutzerverwaltung** - `/portal/admin/users` - Alle Benutzer verwalten
- **Konsole** - `/portal/admin/console` - Web-basierte OpenSim Konsole

### API Endpoints

- **Message API** - `/api/message` - REST API mit Server-Status (JSON)

## 🎨 Template-Struktur

```bash
websites/
├── templates/
│   # Firestorm Viewer Integration
│   ├── welcome.html          # Welcome Page (GridInfoService welcome)
│   ├── splash.html           # Splash Page mit Navigation
│   ├── guide.html            # Destination Guide (DestinationGuide)
│   ├── tos.html              # Terms of Service mit Accept/Decline
│   ├── termsofservice.html   # Terms of Service (nur lesen)
│   ├── rss.html              # RSS Feed Info
│   ├── help.html             # Help Page mit FAQ
│   ├── 404.html              # Custom Error Page
│   # Grid-Manager Integration
│   ├── economy.html          # Economy Info (economy parameter)
│   ├── password.html         # Password Recovery (password parameter)
│   ├── gridstatus.html       # Grid Status Monitor (GridStatus)
│   ├── gridstatusrss.xml     # Grid Status RSS Feed (GridStatusRSS)
│   ├── search.html           # Grid Search (SearchURL)
│   ├── avatars.html          # Avatar Picker (AvatarPicker)
│   # Portal Seiten
│   ├── layout.html           # Master-Template (alte Seiten)
│   ├── home.html             # Startseite
│   ├── login.html            # Login-Formular
│   ├── register.html         # Registrierungs-Formular
│   ├── account.html          # Account-Übersicht
│   ├── inventory.html        # Inventar-Browser
│   ├── forgot-password.html  # Passwort-Reset
│   ├── about.html            # Über-Seite
│   ├── admin-users.html      # Admin: Benutzerverwaltung
│   └── admin-console.html    # Admin: Web-Konsole
└── css/
    └── style.css             # Einheitliches CSS mit Custom Properties
```

## 🚀 Features pro Seite

### Firestorm Viewer Seiten

#### Welcome (`welcome.html`)

✅ Hero-Sektion mit Grid-Information
✅ Grid-Statistiken Cards (Benutzer online, Regionen, aktive Benutzer)
✅ Feature-Highlights
✅ Moderne Bootstrap 5 Oberfläche
✅ Orange/Schwarz Theme
✅ Template-Variablen: `{{ GridName }}`, `{{ UsersInworld }}`, `{{ RegionsTotal }}`, `{{ UsersTotal }}`, `{{ UsersActive }}`

#### Splash (`splash.html`)

✅ Drei-Spalten-Layout mit Sidebar-Navigation
✅ Hauptmenü-Links (Home, About, Help, Register, Login)
✅ Content-Bereich für dynamischen Inhalt
✅ Responsive Design
✅ Template-Variablen: `{{ GridName }}`

#### Guide (`guide.html`)

✅ Destination Guide mit Region-Cards
✅ hop://-Protocol Links für Teleports
✅ Region-Bilder und Beschreibungen
✅ Beliebte Ziele vorgestellt
✅ V3 Viewer Integration

#### TOS (`tos.html`)

✅ Vollständige Nutzungsbedingungen
✅ Accept/Decline Formular
✅ Pflichtfeld-Checkboxen
✅ POST zu `/accept-tos`
✅ Bestätigungsmeldungen

#### Terms of Service (`termsofservice.html`)

✅ Read-only Nutzungsbedingungen
✅ Übersichtliche Gliederung
✅ Sections: Nutzungsregeln, Verbotene Inhalte, Haftung, Datenschutz
✅ Zurück-Navigation

#### RSS (`rss.html`)

✅ RSS Feed Informationen
✅ Feed-URL mit Copy-Funktion
✅ Anleitung zur RSS-Integration
✅ Beispiel-Feed-Items

#### Help (`help.html`)

✅ FAQ mit Bootstrap Accordion
✅ Kategorien: Getting Started, Troubleshooting, Features
✅ Suchfunktion (geplant)
✅ Support-Kontakt

#### 404 Error Page (`404.html`)

✅ Custom Error-Design
✅ Fehlercode-Anzeige
✅ Hilfreiche Links (Home, Help, About)
✅ Suchfunktion

### Grid-Manager Seiten

#### Economy (`economy.html`)

✅ Währungsinformationen (OS$)
✅ Features: Kaufen, Verkaufen, Überweisen, Spenden
✅ Benutzer-Balance Anzeige
✅ Starting Balance Information
✅ Template-Variablen: `{{ UserBalance }}`, `{{ StartingBalance }}`, `{{ CurrencySymbol }}`

#### Password Recovery (`password.html`)

✅ Passwort-Wiederherstellungs-Formular
✅ Felder: firstName, lastName, email
✅ POST zu `/password-reset`
✅ Sicherheitshinweise
✅ E-Mail-Validierung

#### Grid Status (`gridstatus.html`)

✅ Live Grid-Status Monitor
✅ Auto-Refresh alle 60 Sekunden
✅ Service-Status-Karten (Grid, Login, Database, Inventory)
✅ Uptime-Anzeige
✅ Template-Variablen: `{{ Uptime }}`, `{{ UsersInworld }}`, `{{ RegionsTotal }}`

#### Grid Status RSS (`gridstatusrss.xml`)

✅ RSS 2.0 XML Feed
✅ Grid-Statistiken als Feed-Items
✅ Content-Type: application/rss+xml
✅ Automatische Updates
✅ Template-Variablen: `{{ CurrentDateTime }}`, `{{ Timestamp }}`

#### Search (`search.html`)

✅ Grid-weite Suchfunktion
✅ Filter: All, Places, People, Events, Groups
✅ Radio-Button-Auswahl
✅ Suchergebnis-Kategorien
✅ Backend-Integration erforderlich

#### Avatar Picker (`avatars.html`)

✅ Avatar-Auswahl Grid
✅ Avatar-Cards mit Bildern
✅ JavaScript selectAvatar() Funktion
✅ confirmSelection() für Bestätigung
✅ V3 Viewer Integration

### Portal Seiten

#### Home (`home.html`)

✅ Grid-Statistiken (Benutzer, Regionen, Status)
✅ Feature-Cards mit direkten Links
✅ Getting Started Guide
✅ Responsive Design

### Login (`login.html`)

✅ E-Mail/Passwort Formular
✅ Passwort-Anzeige Toggle
✅ "Passwort vergessen" Link
✅ Registrierungs-Link

### Register (`register.html`)

✅ Vollständiges Registrierungsformular
✅ Avatar-Auswahl (Standard-Avatare)
✅ Passwort-Bestätigung
✅ E-Mail-Validierung
✅ Client-seitige Validierung

### Account (`account.html`)

✅ Profil-Card mit Avatar
✅ Account-Informationen (UUID, E-Mail, Level, Erstellungsdatum)
✅ Viewer-Verbindungsdaten (Grid URL, Copy-to-Clipboard)
✅ Account-Status und letzte Anmeldung
✅ Profil bearbeiten / Passwort ändern Buttons

### Inventory (`inventory.html`)

✅ Ordner-Baum (Animationen, Kleidung, Objekte, etc.)
✅ Inventar-Inhalt anzeigen
✅ IAR Upload Modal
✅ IAR Download Funktion
✅ Statistiken (Items, Ordner, Größe)
✅ Neue Ordner erstellen
✅ Items löschen

### Forgot Password (`forgot-password.html`)

✅ E-Mail-Eingabe für Reset
✅ Hinweis-Card mit Informationen
✅ Zurück zum Login Link

### About (`about.html`)

✅ Projekt-Informationen
✅ Feature-Liste (verfügbar ✅ / in Entwicklung 🔄)
✅ Technologie-Stack
✅ Credits und Danksagungen

### Admin: Users (`admin-users.html`)

✅ Benutzer-Tabelle mit allen Accounts
✅ Suche und Filter (nach Level)
✅ Benutzer-Aktionen (Anzeigen, Bearbeiten, Löschen)
✅ Massen-Löschfunktion
✅ Statistiken (Gesamt, Online, Neue, Admins)
✅ Neuen Benutzer erstellen

### Admin: Console (`admin-console.html`)

✅ Echtzeit-Console-Output
✅ Befehlseingabe mit History
✅ Schnellbefehle (help, show status, show users, etc.)
✅ Auto-Scroll Toggle
✅ Console leeren
✅ Pfeiltasten-Navigation in History
✅ Farbcodierung (Info, Warning, Error, User)

## 🔧 Template-Variablen

### Neue Seiten (mit {{ }} Syntax)

Die Firestorm Viewer und Grid-Manager Seiten verwenden `{{ Variable }}` Syntax:

**Grid-Variablen:**

- `{{ GridName }}` - Name des Grids
- `{{ GridOwner }}` - Besitzer des Grids
- `{{ GridURL }}` - Grid-URL

**Statistik-Variablen:**

- `{{ UsersInworld }}` - Aktuell online Benutzer
- `{{ RegionsTotal }}` - Gesamtzahl Regionen
- `{{ UsersTotal }}` - Gesamtzahl Benutzer
- `{{ UsersActive }}` - Aktive Benutzer (letzte 30 Tage)
- `{{ Uptime }}` - Server-Uptime

**Benutzer-Variablen:**

- `{{ UserFirstName }}` - Vorname
- `{{ UserLastName }}` - Nachname
- `{{ UserEmail }}` - E-Mail
- `{{ UserUUID }}` - UUID
- `{{ UserBalance }}` - Kontostand

**Währungs-Variablen:**

- `{{ CurrencySymbol }}` - Währungssymbol (OS$)
- `{{ StartingBalance }}` - Start-Guthaben

**Zeit-Variablen:**

- `{{ CurrentDateTime }}` - Aktuelles Datum/Zeit
- `{{ Timestamp }}` - Unix-Timestamp

### Alte Seiten (mit {{}} Syntax)

Die Portal-Seiten verwenden `{{VARIABLE}}` Syntax:

**Globale Variablen:**

- `{{GRID_NAME}}` - Name des Grids
- `{{CONTENT}}` - Seiten-Inhalt (nur in layout.html)
- `{{HEAD_EXTRA}}` - Zusätzliche Head-Tags

**Benutzer-Variablen:**

- `{{USER_FIRSTNAME}}` - Vorname
- `{{USER_LASTNAME}}` - Nachname
- `{{USER_EMAIL}}` - E-Mail
- `{{USER_UUID}}` - UUID
- `{{USER_LEVEL}}` - Account Level
- `{{USER_NAME}}` - Vollständiger Name

**System-Variablen:**

- `{{CREATED_DATE}}` - Account-Erstellungsdatum
- `{{LAST_LOGIN}}` - Letzte Anmeldung
- `{{LOGIN_URI}}` - Grid Login-URI
- `{{TOTAL_USERS}}` - Gesamt-Benutzer
- `{{ONLINE_USERS}}` - Online-Benutzer
- `{{TOTAL_REGIONS}}` - Gesamt-Regionen
- `{{UPTIME}}` - Server-Uptime

**Bedingte Blöcke:**

```html
{{#IF_LOGGED_IN}}
  ... angezeigt wenn eingeloggt ...
{{ELSE}}
  ... angezeigt wenn nicht eingeloggt ...
{{/IF_LOGGED_IN}}

{{#IF_ADMIN}}
  ... nur für Admins sichtbar ...
{{/IF_ADMIN}}
```

## 🎯 Nächste Schritte

### Phase 2: Backend-Integration

1. **Authentication Handler** - Login/Logout Funktionalität
2. **UserAccountService Integration** - Echte Benutzerdaten
3. **Session Management** - Cookie-basierte Sessions
4. **Registration Backend** - Account-Erstellung
5. **Password Recovery** - E-Mail-basierter Reset

### Phase 3: Erweiterte Features

1. **InventoryService Integration** - Echtes Inventar anzeigen
2. **IAR Upload/Download** - File-Handling
3. **GridService Integration** - Regionen-Management
4. **Console Commands** - Remote Command Execution
5. **Real-time Updates** - WebSocket/SignalR für Live-Daten

### Phase 4: Zusätzliche Seiten

1. **Regions** - `/portal/regions` - Regionen-Browser
2. **Groups** - `/portal/groups` - Gruppen-Verwaltung
3. **Friends** - `/portal/friends` - Freundesliste
4. **Messages** - `/portal/messages` - IM-System
5. **Settings** - `/portal/settings` - Benutzer-Einstellungen

## 📊 Status-Übersicht

### Firestorm Viewer Integration

| Seite | Template | Handler | Backend | Status |
|-------|----------|---------|---------|--------|
| Welcome | ✅ | ✅ | ⭕ | Funktional (statisch) |
| Splash | ✅ | ✅ | ⭕ | Funktional (statisch) |
| Guide | ✅ | ✅ | ⭕ | UI fertig |
| TOS (Form) | ✅ | ✅ | ⭕ | UI fertig |
| Terms of Service | ✅ | ✅ | ✅ | Fertig |
| RSS Info | ✅ | ✅ | ✅ | Fertig |
| Help | ✅ | ✅ | ✅ | Fertig |
| 404 Error | ✅ | ✅ | ✅ | Fertig |

### Grid-Manager Integration

| Seite | Template | Handler | Backend | Status |
|-------|----------|---------|---------|--------|
| Economy | ✅ | ✅ | ⭕ | UI fertig |
| Password Recovery | ✅ | ✅ | ⭕ | UI fertig |
| Grid Status | ✅ | ✅ | ⭕ | Funktional (statisch) |
| Grid Status RSS | ✅ | ✅ | ⭕ | XML Feed fertig |
| Search | ✅ | ✅ | ⭕ | UI fertig |
| Avatar Picker | ✅ | ✅ | ⭕ | UI fertig |

### Portal Seiten x

| Seite | Template | Handler | Backend | Status |
|-------|----------|---------|---------|--------|
| Home | ✅ | ✅ | ⭕ | Funktional (statisch) |
| Login | ✅ | ✅ | ⭕ | UI fertig |
| Register | ✅ | ✅ | ⭕ | UI fertig |
| Account | ✅ | ✅ | ⭕ | UI fertig |
| Inventory | ✅ | ✅ | ⭕ | UI fertig |
| Forgot Password | ✅ | ✅ | ⭕ | UI fertig |
| About | ✅ | ✅ | ✅ | Fertig |
| Admin: Users | ✅ | ✅ | ⭕ | UI fertig |
| Admin: Console | ✅ | ✅ | ⭕ | UI fertig |

**Legende:**

- ✅ = Komplett implementiert
- ⭕ = Noch nicht implementiert
- ⚠️ = In Arbeit

## 🌐 Test-URLs

Mit laufendem Server (Port 8100):

**Firestorm Viewer Seiten:**

- <http://localhost:8100/welcome> - Welcome
- <http://localhost:8100/splash> - Splash
- <http://localhost:8100/guide> - Destination Guide
- <http://localhost:8100/tos> - Terms of Service (Formular)
- <http://localhost:8100/termsofservice> - Terms of Service (Read-only)
- <http://localhost:8100/rss> - RSS Info
- <http://localhost:8100/help> - Hilfe
- <http://localhost:8100/404> - 404 Error

**Grid-Manager Seiten:**

- <http://localhost:8100/economy> - Economy
- <http://localhost:8100/password> - Password Recovery
- <http://localhost:8100/gridstatus> - Grid Status
- <http://localhost:8100/gridstatusrss> - Grid Status RSS (XML)
- <http://localhost:8100/search> - Search
- <http://localhost:8100/avatars> - Avatar Picker

**Portal Seiten:**

- <http://localhost:8100> - Home
- <http://localhost:8100/portal/login> - Login
- <http://localhost:8100/portal/register> - Registrierung
- <http://localhost:8100/portal/account> - Account
- <http://localhost:8100/portal/inventory> - Inventar
- <http://localhost:8100/portal/forgot-password> - Passwort vergessen
- <http://localhost:8100/portal/about> - Über
- <http://localhost:8100/portal/admin/users> - Admin: Benutzer
- <http://localhost:8100/portal/admin/console> - Admin: Konsole

**API:**

- <http://localhost:8100/api/message> - API (JSON)

## 💡 Tipps für Entwickler

1. **Templates bearbeiten**: Dateien unter `addon-modules/OpenSim-Web-Portal/websites/templates/`
2. **Nach Änderungen kopieren**: `Copy-Item "...\templates\*.html" "bin\portal\templates\" -Force`
3. **Server neustarten**: Um Template-Änderungen zu sehen (kein Rebuild nötig)
4. **CSS anpassen**: `websites/css/custom.css` dann nach `bin/portal/css/` kopieren
5. **Neue Handler**: In `WebPortalServer.cs` unter `SetupHttpServer()` registrieren

## 🎨 Design-System

### Neues CSS-System (style.css)

**Firestorm Viewer & Grid-Manager Seiten verwenden einheitliches CSS mit Custom Properties:**

- **Framework**: Bootstrap 5.3.2
- **Icons**: Bootstrap Icons 1.11.1
- **CSS-Datei**: `websites/css/style.css`
- **Theme-System**: CSS Custom Properties (einfach änderbar)
- **Farben** (Custom Properties in :root):
  - `--primary-color`: #ff6600 (Orange - Hauptfarbe)
  - `--primary-hover`: #ff8833 (Orange Hell - Hover)
  - `--bg-dark`: #1a1a1a (Schwarz - Hintergrund)
  - `--bg-darker`: #0d0d0d (Schwarz Dunkel)
  - `--text-primary`: #ffffff (Weiß - Text)
  - `--text-secondary`: #cccccc (Grau Hell - Sekundärtext)
  - `--border-color`: #333333 (Grau - Rahmen)
- **Custom Classes**:
  - `.btn-os-primary` - Orange Buttons
  - `.os-card` - Styled Cards
  - `.os-header` - Einheitlicher Header
  - `.os-footer` - Einheitlicher Footer
  - `.status-indicator` - Status Badges
- **Features**:
  - Dark Theme optimiert
  - Responsive Design
  - Smooth Animations
  - Custom Scrollbar
  - Box Shadows & Hover Effects
- **Farben ändern**: Einfach die Werte in `:root` in `style.css` anpassen

### Altes Design-System (Portal Seiten)

**Alte Portal-Seiten verwenden Bootstrap Standard-Klassen:**

- **Framework**: Bootstrap 5.3.2
- **Icons**: Bootstrap Icons 1.11.1
- **Farben**:
  - Primary: Blau (Navigation, Hauptaktionen)
  - Success: Grün (Erfolg, Online-Status)
  - Info: Cyan (Informationen)
  - Warning: Orange (Warnungen, Admin)
  - Danger: Rot (Fehler, Lösch-Aktionen)
- **Schriftart**: System-Standard (sans-serif)
- **Responsive**: Mobile-first Design

**⚠️ Hinweis**: Die alten Portal-Seiten sollten auf das neue CSS-System migriert werden für einheitliches Design.
