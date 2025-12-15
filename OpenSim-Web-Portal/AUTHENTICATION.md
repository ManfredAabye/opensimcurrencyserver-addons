# OpenSim Web Portal - Authentifizierungs-System

## Übersicht

Das Web Portal verfügt nun über ein vollständiges Authentifizierungssystem mit:

- ✅ **Backend-Authentifizierung** gegen OpenSim-Datenbank
- ✅ **Session-Management** mit Cookie-basierten Sessions
- ✅ **Passwort-Hashing** über OpenSim AuthenticationService
- ✅ **Geschützte Seiten** (nur für angemeldete User)
- ✅ **User-Registrierung** (neue Accounts erstellen)
- ✅ **Login/Logout** Funktionalität

## Neue Komponenten

### 1. SessionManager.cs

- Verwaltet User-Sessions mit eindeutigen Session-IDs
- Cookie-basierte Session-Speicherung
- Automatisches Session-Timeout (Standard: 30 Minuten)
- "Angemeldet bleiben" Funktion (30 Tage Sessions)
- Thread-sichere Session-Verwaltung

### 2. AuthenticationService.cs

- Anbindung an OpenSim IUserAccountService
- Anbindung an OpenSim IAuthenticationService
- Passwort-Verifikation mit OpenSim-Hashing
- User-Registrierung (CreateAccount)
- Passwort-Änderung (ChangePassword)
- Email- und Username-Validierung

### 3. AuthHandlers.cs

- **LoginHandler** (POST /portal/login)
  - Verarbeitet Login-Formulare
  - Erstellt Sessions
  - Setzt Session-Cookies
  - Redirect zu Account-Seite

- **LogoutHandler** (GET /portal/logout)
  - Zerstört aktive Session
  - Löscht Session-Cookie
  - Redirect zur Startseite

- **RegisterHandler** (POST /portal/register)
  - Verarbeitet Registrierungs-Formulare
  - Validiert Input (Passwort-Länge, Email, etc.)
  - Erstellt neuen OpenSim Account
  - Auto-Login nach Registrierung

### 4. ProtectedHandlers.cs

- **ProtectedPageHandler** (Basisklasse)
  - Prüft Session-Cookie automatisch
  - Redirect zu Login bei fehlender Authentifizierung
  - Extrahiert User-Daten aus Session

- **AccountPageHandler** (GET /portal/account)
  - Zeigt Account-Informationen
  - Nur für angemeldete User

- **InventoryPageHandler** (GET /portal/inventory)
  - Zeigt Inventory (Coming Soon)
  - Nur für angemeldete User

- **PasswordPageHandler** (GET /password)
  - Passwort-Änderung
  - Nur für angemeldete User

## Konfiguration

### WebPortal.ini

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
```

**Wichtig:** Passe die ConnectionStrings an deine Datenbank an!

## Login-Flow

1. User besucht `/portal/login`
2. Gibt Vorname, Nachname, Passwort ein
3. POST zu `/portal/login` Handler
4. AuthenticationService validiert gegen OpenSim DB
5. Bei Erfolg: SessionManager erstellt Session
6. Session-Cookie wird gesetzt (`OPENSIM_SESSION`)
7. Redirect zu `/portal/account`

## Session-Verwaltung

### Session-Cookie

```bash
Name: OPENSIM_SESSION
Wert: Base64-encodierte Session-ID (32 Bytes Random)
Path: /
HttpOnly: true (JavaScript kann nicht zugreifen)
SameSite: Lax
Max-Age: 0 (Session-Cookie) oder 2592000 (30 Tage bei "Angemeldet bleiben")
```

### Session-Daten

Jede Session speichert:

- SessionId (eindeutige ID)
- UserId (OpenSim UUID)
- FirstName, LastName
- Email
- UserLevel (0 = normal, 100 = God, etc.)
- CreatedAt (Session-Erstellung)
- LastAccess (letzte Aktivität)
- RememberMe (bool)

### Session-Timeout

- Standard: 30 Minuten Inaktivität
- "Angemeldet bleiben": 30 Tage
- Automatisches Cleanup abgelaufener Sessions

## Geschützte Seiten

Folgende Seiten erfordern Authentifizierung:

- `/portal/account` - Account-Management
- `/portal/inventory` - Inventory-Browser
- `/password` - Passwort-Änderung

Bei Zugriff ohne Login → Redirect zu `/portal/login?redirect=<ursprüngliche-url>`

## Registrierung

### Registrierungs-Flow

1. User besucht `/portal/register`
2. Füllt Formular aus (Vorname, Nachname, Email, Passwort)
3. POST zu `/portal/register` Handler
4. Validierung:
   - Alle Felder ausgefüllt?
   - Passwort mindestens 6 Zeichen?
   - Passwörter stimmen überein?
   - Username bereits vergeben?
   - Email bereits registriert?
5. Account-Erstellung in OpenSim DB
6. Passwort-Hash speichern
7. Auto-Login (Session erstellen)
8. Redirect zu `/portal/account?welcome=true`

### Validierungsregeln

- Vorname: Pflichtfeld, keine Duplikate zusammen mit Nachname
- Nachname: Pflichtfeld
- Email: Pflichtfeld, muss eindeutig sein, wird in `email` Feld gespeichert
- Passwort: Mindestens 6 Zeichen
- Passwort-Bestätigung: Muss mit Passwort übereinstimmen

## Sicherheit

### Implementierte Sicherheitsmaßnahmen

✅ **Passwort-Hashing**

- OpenSim AuthenticationService verwendet PBKDF2 oder MD5-basiertes Hashing
- Klartext-Passwörter werden nie gespeichert

✅ **Session-Sicherheit**

- Kryptographisch sichere Session-IDs (32 Bytes Random)
- HttpOnly Cookies (JavaScript-Schutz)
- SameSite=Lax (CSRF-Schutz)

✅ **Input-Validierung**

- Server-seitige Validierung aller Eingaben
- Prüfung auf Duplikate (Username, Email)
- Passwort-Stärke-Anforderungen

✅ **SQL-Injection-Schutz**

- OpenSim Data Layer verwendet Prepared Statements
- Keine direkten SQL-Queries mit User-Input

### Noch zu implementieren (optional)

⚠️ **Rate Limiting**

- Login-Versuche limitieren (gegen Brute-Force)

⚠️ **CAPTCHA**

- Bei Registrierung/Login gegen Bots

⚠️ **HTTPS**

- SSL/TLS für verschlüsselte Übertragung
- Wichtig für Produktiv-Umgebung!

⚠️ **Password Reset**

- Email-basierte Passwort-Zurücksetzung
- Token-System

⚠️ **2-Faktor-Authentifizierung**

- TOTP (Google Authenticator)
- Optional für erhöhte Sicherheit

## API-Endpunkte

### Öffentliche Endpunkte (kein Login)

- `GET /` - Home Page
- `GET /portal/login` - Login-Formular
- `POST /portal/login` - Login verarbeiten
- `GET /portal/register` - Registrierungs-Formular
- `POST /portal/register` - Registrierung verarbeiten
- `GET /portal/about` - About Page
- `GET /welcome` - Firestorm Welcome
- `GET /splash` - Firestorm Splash
- etc.

### Geschützte Endpunkte (Login erforderlich)

- `GET /portal/account` - Account-Info & Einstellungen
- `GET /portal/inventory` - Inventory-Browser
- `GET /password` - Passwort-Änderung
- `POST /password` - Passwort ändern (noch zu implementieren)

### Auth-Endpunkte

- `GET /portal/logout` - Session beenden

## Template-Platzhalter

Templates können folgende Session-bezogene Platzhalter verwenden:

```html
{{USER_FIRSTNAME}}      - Vorname des angemeldeten Users
{{USER_LASTNAME}}       - Nachname des angemeldeten Users
{{USER_FULLNAME}}       - Vollständiger Name (Vorname Nachname)
{{USER_EMAIL}}          - Email-Adresse
{{USER_UUID}}           - OpenSim UUID
{{USER_LEVEL}}          - User-Level (0, 100, etc.)
{{LAST_LOGIN}}          - Zeitpunkt des Logins

<!-- Bedingte Anzeige -->
{{#IF_LOGGED_IN}}
    Content nur für angemeldete User
{{/IF_LOGGED_IN}}

{{#IF_NOT_LOGGED_IN}}
    Content nur für Gäste
{{/IF_NOT_LOGGED_IN}}
```

## Fehlerbehandlung

### Login-Fehler

- Ungültige Credentials → Redirect zu `/portal/login?error=<message>`
- Fehlende Felder → Redirect zu `/portal/login?error=Bitte alle Felder ausfüllen`
- Session-Erstellung fehlgeschlagen → Redirect mit Fehler

### Registrierungs-Fehler

- Username vergeben → `/portal/register?error=Benutzername bereits vergeben`
- Email vergeben → `/portal/register?error=E-Mail bereits registriert`
- Passwörter stimmen nicht überein → Fehler-Meldung
- Passwort zu kurz → Fehler-Meldung

### Session-Fehler

- Abgelaufene Session → Automatisches Logout, Redirect zu Login
- Ungültige Session-ID → Redirect zu Login
- Fehlender Cookie → Redirect zu Login

## Logs

Das System loggt alle wichtigen Ereignisse:

```bash
[LOGIN HANDLER]: Login attempt for Max Mustermann
[LOGIN HANDLER]: Login successful for Max Mustermann
[LOGIN HANDLER]: Login failed for John Doe

[SESSION MANAGER]: Created session for Max Mustermann (ID: AbC12345, RememberMe: false)
[SESSION MANAGER]: Session AbC12345 expired for Max Mustermann
[SESSION MANAGER]: Destroyed session AbC12345 for Max Mustermann
[SESSION MANAGER]: Cleaned up 3 expired sessions

[LOGOUT HANDLER]: User Max Mustermann logged out

[REGISTER HANDLER]: Registration attempt for Jane Doe
[REGISTER HANDLER]: Registration successful for Jane Doe
[REGISTER HANDLER]: Registration failed for John Smith

[AUTH SERVICE]: User authenticated successfully: Max Mustermann (UUID: ...)
[AUTH SERVICE]: Authentication failed - user not found: Unknown User
[AUTH SERVICE]: Authentication failed - invalid password for: Max Mustermann
[AUTH SERVICE]: Created new account: Jane Doe (UUID: ...)
```

## Testen

### Manueller Test

1. **Registrierung testen:**

   ```bash
   http://localhost:8100/portal/register
   Vorname: Test
   Nachname: User
   Email: test@example.com
   Passwort: test1234
   ```

2. **Login testen:**

   ```bash
   http://localhost:8100/portal/login
   Vorname: Test
   Nachname: User
   Passwort: test1234
   ☑ Angemeldet bleiben
   ```

3. **Geschützte Seite testen:**

   ```bash
   http://localhost:8100/portal/account
   → Sollte Account-Info zeigen wenn eingeloggt
   → Sollte zu Login redirecten wenn nicht eingeloggt
   ```

4. **Logout testen:**

   ```bash
   http://localhost:8100/portal/logout
   → Session wird gelöscht
   → Redirect zur Startseite
   ```

5. **Session-Persistenz testen:**

   ```bash
   - Login mit "Angemeldet bleiben"
   - Browser schließen und neu öffnen
   - http://localhost:8100/portal/account besuchen
   - Sollte noch eingeloggt sein (30 Tage Cookie)
   ```

## Troubleshooting

### Problem: "AuthenticationService is null"

**Ursache:** Dienste konnten nicht geladen werden
**Lösung:**

- Prüfe WebPortal.ini Konfiguration
- Prüfe Datenbank-Verbindung
- Prüfe ob OpenSim.Services.*.dll vorhanden sind

### Problem: "User not found"

**Ursache:** User existiert nicht in OpenSim DB
**Lösung:**

- Prüfe Datenbank-Tabelle `UserAccounts`
- Erstelle User über Registrierung oder OpenSim Console

### Problem: "Invalid password"

**Ursache:** Falsches Passwort
**Lösung:**

- Passwort zurücksetzen über OpenSim Console: `reset user password <first> <last>`

### Problem: Session läuft sofort ab

**Ursache:** SessionTimeout zu niedrig oder Cookie wird nicht gesetzt
**Lösung:**

- Erhöhe SessionTimeout in WebPortal.ini
- Prüfe Browser-Cookies (DevTools → Application → Cookies)
- Prüfe ob `OPENSIM_SESSION` Cookie gesetzt wird

### Problem: "Connection refused" zur Datenbank

**Ursache:** Datenbank nicht erreichbar
**Lösung:**

- Prüfe MySQL-Server läuft
- Prüfe ConnectionString in WebPortal.ini
- Prüfe Firewall-Regeln

## Performance

### Session-Speicher

- In-Memory Dictionary (schnell, aber nicht persistent)
- Bei Server-Neustart gehen alle Sessions verloren
- Für Produktion: Erwäge Redis/Memcached für persistente Sessions

### Datenbank-Queries

- User-Lookup: 1 Query pro Login
- Session-Validierung: 0 Queries (nur Memory-Lookup)
- Registrierung: 2-3 Queries (User create, Password set)

### Caching

Aktuell kein Caching implementiert. Mögliche Optimierungen:

- User-Daten in Session cachen (bereits implementiert)
- Häufige DB-Queries cachen
- Static File Caching

## Next Steps

### Geplante Features

1. **Passwort-Reset per Email**
   - Token-System
   - SMTP-Konfiguration
   - Reset-Email-Templates

2. **User-Profile**
   - Avatar-Upload
   - Bio/Beschreibung
   - Social Links

3. **Admin-Panel**
   - User-Verwaltung
   - Ban/Unban User
   - User-Level ändern
   - Session-Übersicht

4. **API-Erweiterung**
   - REST API für User-Management
   - JSON Web Tokens (JWT)
   - OAuth2 Integration

5. **Sicherheits-Features**
   - Rate Limiting
   - CAPTCHA
   - 2FA
   - IP-Whitelisting

## Lizenz

Copyright (c) Contributors, <http://opensimulator.org/>
Siehe CONTRIBUTORS.TXT für vollständige Liste.

BSD 3-Clause License - siehe LICENSE.txt
