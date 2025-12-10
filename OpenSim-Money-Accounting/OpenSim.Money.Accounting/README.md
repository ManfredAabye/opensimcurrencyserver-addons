# OpenSim Money Accounting - Kassenbuchhaltung

Eine vereinfachte und reparierte Kassenbuchhaltungslösung für OpenSim mit REST API und Web-Dashboard.

## 📋 Wichtige Funktionen

Dieses System bietet eine vollständige Buchhaltung/Kassenbuchhaltung für OpenSim:

- ✅ **Mehrere Kontotypen**: Benutzer, Gruppen und Grid/Region Konten
- ✅ **Transaktionsverfolgung**: Vollständiger Verlauf aller Transaktionen
- ✅ **Saldoverwaltung**: Automatische Saldoaktualisierung
- ✅ **Berichterstattung**: Finanzberichte und Auswertungen
- ✅ **OpenSim Integration**: Synchronisation mit dem CurrencyServer
- ✅ **Fehlerbehandlung**: Transaktionsrollback bei Fehlern
- ✅ **REST API**: Einfacher Zugriff auf alle Buchhaltungsfunktionen
- ✅ **Web-Dashboard**: Übersichtliche Darstellung aller Daten

## 🔧 Behobene Probleme (Dezember 2025)

### Kompilierungsfehler behoben

1. **Assembly-Referenzen entfernt** - Die veralteten System.*-Assembly-Referenzen (System, System.Core, System.Data, System.Net, System.Net.Http, System.Web, System.Xml) wurden aus der prebuild.xml entfernt, da .NET 8.0 diese automatisch bereitstellt

2. **Console.Notice Fehler** - `m_console.Notice()` wurde durch `m_console.Output()` ersetzt, da die Notice-Methode nicht in ICommandConsole existiert

3. **Newtonsoft.Json entfernt** - Die Abhängigkeit von Newtonsoft.Json wurde durch System.Text.Json ersetzt (integriert in .NET 8.0)
   - `JsonConvert.SerializeObject` → `JsonSerializer.Serialize`
   - `JsonConvert.DeserializeObject` → `JsonSerializer.Deserialize`

### Code-Vereinfachungen

1. **ServiceResult\<T> Klasse** - Strukturierte Rückgabewerte für bessere Typsicherheit
2. **Vereinfachte Service-Methoden** - Alle Methoden geben nun typsichere Objekte statt JSON-Strings zurück
3. **CreateTransaction verbessert** - Akzeptiert jetzt direkt Parameter statt JSON-String
4. **GetAllTransactions hinzugefügt** - Fehlende Methode wurde implementiert
5. **DateTime Parameter** - GetFinancialReport verwendet nun DateTime statt String-Parameter
6. **Handler konsolidiert** - AccountingApiHandler verarbeitet sowohl GET als auch POST

## 🚀 Installation & Kompilierung

### Voraussetzungen

- **OpenSim** oder **Robust** Installation
- **MySQL Server** mit OpenSimCurrency Datenbank
- **.NET 8.0 SDK** oder höher

### Kompilieren

**WICHTIG:** Nach jeder Änderung an der `prebuild-OpenSimMoneyAccounting.xml` muss `runprebuild.bat` ausgeführt werden:

```powershell
# Zurück zum Hauptverzeichnis
cd ../../..

# Prebuild ausführen (generiert .csproj-Dateien neu)
.\runprebuild.bat

# In das Projektverzeichnis wechseln
cd addon-modules\OpenSim-Money-Accounting\OpenSim.Money.Accounting

# Projekt kompilieren (Debug)
dotnet build -c Debug

# Oder Release-Version
dotnet build -c Release
```

Das kompilierte Projekt wird automatisch nach `../../../bin/` kopiert.

**Hinweis:** Die .csproj-Datei wird von prebuild.xml generiert. Manuelle Änderungen an der .csproj werden beim nächsten `runprebuild.bat` überschrieben.

### Konfiguration

**WICHTIG:** Der AccountingServer verwendet die **gleiche `MoneyServer.ini`** wie der MoneyServer!

Die Datenbank-Verbindung wird aus der `[MySql]` Sektion gelesen:

```ini
[MySql]
hostname = "localhost"
database = "OpenSimCurrency"
username = "opensim"
password = "IhrPasswort"
port = "3306"

[MoneyServer]
AccountingPort = "5000"
```

**Der AccountingServer liest:**

- Datenbank-Einstellungen aus `[MySql]` (wie der MoneyServer)
- Port-Konfiguration aus `[MoneyServer]` → `AccountingPort`

**Häufiger Fehler:**

```text
Access denied for user 'opensim'@'localhost' (using password: NO)
```

**Lösung:** Prüfe dass:

1. Die Datei `bin/MoneyServer.ini` existiert
2. Das `password` Feld in der `[MySql]` Sektion ausgefüllt ist
3. Die MySQL-Zugangsdaten korrekt sind

**Test mit:**

```bash
AccountingServer# test database
```

### Server starten

**WICHTIG:** Vor dem ersten Start müssen die Webseiten-Dateien ins bin-Verzeichnis kopiert werden:

```powershell
# Webseiten kopieren (einmalig oder nach Änderungen)
cd d:\opensimcurrencyserver-dotnet-12_2025\opensim\bin
Copy-Item -Path "..\addon-modules\OpenSim-Money-Accounting\webseiten" -Destination "." -Recurse -Force

# Server starten
dotnet OpenSim.Money.Accounting.dll
```

Der Server startet auf **Port 5000**

### 📝 Log-Dateien

Der AccountingServer schreibt alle Aktivitäten in eine eigene Log-Datei:

**Log-Datei Speicherort:**

```bash
bin/AccountingServer.log
```

**Log-Konfiguration:**

Die Datei `bin/AccountingServer.log4net` steuert das Logging:

- **Rotation**: Täglich neue Datei mit Datum im Namen
- **Maximale Größe**: 10 MB pro Datei
- **Aufbewahrung**: Letzte 10 Dateien
- **Log-Level**: INFO (Standard), DEBUG für AccountingServer

**Log-Level ändern:**

Bearbeite `AccountingServer.log4net` und ändere:

```xml
<!-- Für mehr Details -->
<root>
  <level value="DEBUG" />
</root>

<!-- Für weniger Ausgaben -->
<root>
  <level value="WARN" />
</root>
```

**Beispiel Log-Einträge:**

```text
2025-12-10 12:00:01,234 INFO  - OpenSim.Money.Accounting [ACCOUNTING SERVER]: Starting Money Accounting Server
2025-12-10 12:00:01,345 INFO  - OpenSim.Money.Accounting [ACCOUNTING SERVER]: Database: localhost:3306/OpenSimCurrency
2025-12-10 12:00:02,456 INFO  - OpenSim.Money.Accounting [ACCOUNTING SERVER]: Server started on port 5000
```

### 🌐 Webseite aufrufen

Nach dem Start des Servers öffnen Sie im Browser:

```bash
http://localhost:5000
```

**Oder von einem anderen Computer im Netzwerk:**

```bash
http://[Server-IP]:5000
```

### 📁 Speicherort der Webseiten

**Die Webseiten laufen NICHT über /var/www/html oder einen klassischen Webserver!**

OpenSim.Money.Accounting hat einen **eigenen integrierten HTTP-Server** (BaseHttpServer von OpenSim).

**Dateipfade:**

1. **Source-Dateien** (zum Bearbeiten):

   ```bash
   d:\opensimcurrencyserver-dotnet-12_2025\opensim\addon-modules\OpenSim-Money-Accounting\webseiten\
   ├── index.html
   ├── app.js
   └── style.css
   ```

2. **Laufzeit-Dateien** (werden vom Server gelesen):

   ```bash
   d:\opensimcurrencyserver-dotnet-12_2025\opensim\bin\webseiten\
   ├── index.html
   ├── app.js
   └── style.css
   ```

**Wichtig:**

- Der Server läuft aus dem `bin/` Verzeichnis
- Die Webseiten müssen im `bin/webseiten/` Verzeichnis liegen
- Es wird **KEIN** Apache, nginx oder IIS benötigt!

### 🔄 Webseiten aktualisieren

Nach Änderungen an den Source-Dateien:

```powershell
cd d:\opensimcurrencyserver-dotnet-12_2025\opensim\bin
Copy-Item -Path "..\addon-modules\OpenSim-Money-Accounting\webseiten\*" -Destination "webseiten\" -Recurse -Force
```

## 📡 API Endpunkte

### Balance (Kontostand)

- `GET /api/balance/{userId}` - Kontostand eines Benutzers
- `GET /api/balance/all` - Alle Kontostände

### Transactions (Transaktionen)

- `GET /api/transactions` - Alle Transaktionen
- `GET /api/transactions/user/{userId}` - Transaktionen eines Benutzers
- `POST /api/transactions` - Neue Transaktion erstellen

**POST Beispiel:**

```json
{
  "senderId": "uuid-des-senders",
  "receiverId": "uuid-des-empfängers",
  "amount": 100,
  "transactionType": 2,
  "description": "Beschreibung"
}
```

### Users (Benutzer)

- `GET /api/users` - Alle Benutzer mit Kontoständen
- `GET /api/users/{userId}` - Einzelner Benutzer

### Dashboard & Reports

- `GET /api/dashboard` - Dashboard-Statistiken
- `GET /api/reports/financial?startDate=2025-01-01&endDate=2025-12-31` - Finanzbericht
- `GET /api/groups` - Gruppen-Accounts

## 🌐 Web-Interface

Die Webseiten befinden sich im `webseiten/` Verzeichnis:

- `index.html` - Dashboard
- `style.css` - Styling
- `app.js` - JavaScript-Logik

## 💻 Konsolen-Befehle

Der AccountingServer verfügt über folgende interaktive Konsolenbefehle:

### Server-Steuerung

- `shutdown` - Beendet den AccountingServer

### Diagnose

- `test database` - Testet die Datenbankverbindung und zeigt die Konfiguration
  
  ```bash
  Zeigt: Connection String, MySQL Version, Verbindungsstatus
  ```

### Informations-Befehle

- `show users [<limit>]` - Zeigt Benutzerliste mit Kontoständen (Standard: 10)

  ```bash
  Beispiel: show users 20
  ```

- `show groups [<limit>]` - Zeigt Gruppenstatistiken (Standard: 10)

  ```bash
  Beispiel: show groups 15
  ```

- `show transactions [<limit>]` - Zeigt letzte Transaktionen (Standard: 20)

  ```bash
  Beispiel: show transactions 50
  ```

- `show reports <days>` - Zeigt Finanzbericht für die letzten N Tage

  ```bash
  Beispiel: show reports 30
  ```

- `show stats` - Zeigt Dashboard-Statistiken

  ```bash
  Zeigt: Gesamtbenutzer, Gesamtguthaben, Transaktionen, aktive Benutzer
  ```

### Ausgabeformat

Alle Befehle liefern übersichtlich formatierte Tabellen mit:

- Benutzer: UUID, Name, Kontostand
- Gruppen: Name, Mitglieder, Gesamtguthaben
- Transaktionen: Zeit, Von → Zu, Betrag, Typ, Beschreibung
- Berichte: Transaktionen, Volumen, Durchschnitt, aktive Benutzer
- Statistiken: Benutzer, Guthaben, Transaktionen, Volumen

## 🛠️ Technische Details

### Architektur

- **AccountingService.cs** - Business Logic und Datenbankzugriff
- **AccountingHandlers.cs** - REST API Handler
- **AccountingServerBase.cs** - Server-Konfiguration und Initialisierung
- **AccountingProgram.cs** - Einstiegspunkt

### Datenbank-Tabellen

- `balances` - Kontostände (user, balance)
- `transactions` - Transaktionen (UUID, sender, receiver, amount, type, description, time)
- `userinfo` - Benutzerinformationen (uuid, username, lastname)

### Transaktionstypen

- 0 = Einzahlung
- 1 = Auszahlung  
- 2 = Überweisung (Standard)
- 4 = Kauf
- 5 = Verkauf

## 📝 Lizenz

Dieses Projekt ist Teil des OpenSimulator-Projekts und unterliegt der BSD-Lizenz.
