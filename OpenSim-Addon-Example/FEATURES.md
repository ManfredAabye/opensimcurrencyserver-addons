# OpenSim Example Addon - Neue Features

## Übersicht

Das OpenSim Example Addon wurde erweitert, um Informationen aus der `Robust.ini` Datei zu lesen und Daten aus der MySQL-Datenbank von OpenSim abzurufen.

## Neue Funktionen

### 1. Robust.ini Konfigurationsleser

Der `RobustConfigReader` liest Konfigurationsdaten aus der `Robust.ini` Datei:

- **BaseHostname**: Der Hostname oder die IP-Adresse des Robust-Servers
- **PublicPort**: Der öffentliche Port
- **PrivatePort**: Der private Port
- **StorageProvider**: Der verwendete Datenbankanbieter (z.B. OpenSim.Data.MySQL.dll)
- **ConnectionString**: Die Datenbankverbindungszeichenfolge (mit maskiertem Passwort)

### 2. MySQL Datenbankzugriff

Der `DatabaseHelper` ermöglicht den Zugriff auf die OpenSim MySQL-Datenbank:

- **Benutzeranzahl**: Anzahl der registrierten Benutzer (aus `UserAccounts` Tabelle)
- **Regionenanzahl**: Anzahl der verfügbaren Regionen (aus `regions` Tabelle)

### 3. Neue API-Endpunkte

#### `/api/robust-config`

Gibt Konfigurationsdaten aus der Robust.ini zurück:

```json
{
    "baseHostname": "127.0.0.1",
    "publicPort": "8002",
    "privatePort": "8003",
    "databaseProvider": "OpenSim.Data.MySQL.dll",
    "timestamp": "2025-12-15 10:30:45",
    "status": "success"
}
```

#### `/api/database-stats`

Gibt Datenbankstatistiken zurück:

```json
{
    "userCount": 5,
    "regionCount": 3,
    "timestamp": "2025-12-15 10:30:45",
    "status": "success"
}
```

#### `/api/message`

Der ursprüngliche API-Endpunkt (unverändert):

```json
{
    "message": "Hello OpenSim User from API!",
    "timestamp": "2025-12-15 10:30:45",
    "server": "OpenSim.Addon.Example",
    "status": "success"
}
```

### 4. Neue Konsolenbefehle

#### `show robust-config`

Zeigt die Robust.ini-Konfiguration in der Konsole an:

```bash
===========================================
       Robust.ini Configuration
===========================================
BaseHostname         = 127.0.0.1
PublicPort          = 8002
PrivatePort         = 8003
StorageProvider     = OpenSim.Data.MySQL.dll
ConnectionString    = Data Source=localhost;Database=opensim;User ID=opensim;Password=*****;Old Guids=true;SslMode=None;
===========================================
```

#### `show database-stats`

Zeigt Datenbankstatistiken in der Konsole an:

```bash
===========================================
       Database Statistics
===========================================
User Count:    5
Region Count:  3
===========================================
```

#### `show status`

Zeigt den Server-Status an (erweitert, ursprünglicher Befehl):

```bash
===========================================
       Example Server Status
===========================================
Port:          9000
Web Interface: http://localhost:9000
API Endpoint:  http://localhost:9000/api/message
Status:        Running
===========================================
```

#### `hello`

Zeigt eine "Hallo World" Nachricht an (ursprünglicher Befehl)

## Web-Interface

Das Web-Interface wurde erweitert und bietet nun drei Buttons zum Testen der verschiedenen API-Endpunkte:

- **API Message**: Testet den ursprünglichen Message-Endpunkt
- **Robust.ini Config**: Zeigt die Robust.ini-Konfiguration an
- **Database Stats**: Zeigt Datenbankstatistiken an

Die Web-Oberfläche ist unter `http://localhost:9000` verfügbar.

## Technische Details

### Abhängigkeiten

Das Projekt verwendet folgende zusätzliche Abhängigkeiten:

- `MySql.Data`: Für den MySQL-Datenbankzugriff
- `Nini.Config`: Für das Lesen von INI-Dateien
- Standard OpenSim Framework-Bibliotheken

### Neue Klassen

1. **RobustConfigReader**: Liest und parst die Robust.ini Datei
2. **DatabaseHelper**: Stellt Methoden für Datenbankabfragen bereit
3. **RobustConfigHandler**: HTTP-Handler für Robust.ini-Daten
4. **DatabaseStatsHandler**: HTTP-Handler für Datenbankstatistiken

## Verwendung

### Server starten

```powershell
cd d:\opensimcurrencyserver-dotnet-12_2025\opensim\bin
.\OpenSim.Addon.Example.exe
```

### API-Aufrufe testen

```powershell
# Robust.ini Konfiguration abrufen
curl http://localhost:9000/api/robust-config

# Datenbankstatistiken abrufen
curl http://localhost:9000/api/database-stats

# Message API abrufen
curl http://localhost:9000/api/message
```

### Konsolenbefehle verwenden

Nach dem Start des Servers können Sie die folgenden Befehle in der Konsole eingeben:

```bash
show robust-config
show database-stats
show status
hello
```

## Fehlerbehandlung

- Wenn die `Robust.ini` nicht gefunden wird, versucht der Reader, die `Robust.ini.example` zu lesen
- Bei Datenbankfehlern werden entsprechende Fehlermeldungen zurückgegeben
- Connection-String-Passwörter werden in der Ausgabe automatisch maskiert

## Zukünftige Erweiterungen

Mögliche zukünftige Verbesserungen:

- Mehr Datenbankabfragen (z.B. aktive Benutzer, Inventarstatistiken)
- Konfiguration weiterer OpenSim-INI-Dateien
- Echtzeit-Monitoring von OpenSim-Servern
- Erweiterte Statistiken und Grafiken im Web-Interface
- RESTful API für CRUD-Operationen

## Lizenz

Wie das Haupt-OpenSimulator-Projekt, steht dieses Addon unter der BSD-Lizenz.
