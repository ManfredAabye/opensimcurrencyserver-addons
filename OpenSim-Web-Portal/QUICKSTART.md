# OpenSim Web Portal - Quick Start Guide

## 🚀 Schnellstart in 5 Schritten

### 1️⃣ Prebuild ausführen

```powershell
cd D:\opensimcurrencyserver-dotnet-12_2025\opensim
.\runprebuild.bat
```

✅ Erstellt `OpenSim.Web.Portal.csproj` im bin-Verzeichnis

### 2️⃣ Kompilieren

```powershell
.\compile.bat
```

✅ Kompiliert alle OpenSim-Module inkl. Web Portal
✅ Ausgabe: `bin\OpenSim.Web.Portal.dll`

### 3️⃣ Templates kopieren

```powershell
# Einmal ausführen, danach nur bei Template-Änderungen
Copy-Item "addon-modules\OpenSim-Web-Portal\websites\templates\*.html" "bin\portal\templates\" -Force
Copy-Item "addon-modules\OpenSim-Web-Portal\websites\css\custom.css" "bin\portal\css\" -Force
```

✅ Kopiert alle 10 HTML-Templates
✅ Kopiert Custom CSS

### 4️⃣ Server starten

**Windows:**

```powershell
cd bin
dotnet OpenSim.Web.Portal.dll
```

**Linux:**

```bash
cd bin
dotnet OpenSim.Web.Portal.dll
```

✅ Server läuft auf Port 8100
✅ Console-Befehle verfügbar: `help`, `show status`, `shutdown`
✅ Console bleibt interaktiv (keine sofortige Beendigung mehr)

**Hinweis für Linux/Docker:** Der Server läuft jetzt stabil und beendet sich nicht mehr sofort. Falls die Console nicht interaktiv ist (z.B. in systemd/Docker), läuft der Server trotzdem weiter und kann über HTTP erreicht werden.

### 5️⃣ Browser öffnen

Öffne einen dieser Links:

```bash
http://localhost:8100                      → Hauptseite
http://localhost:8100/portal/login         → Login
http://localhost:8100/portal/admin/console → Admin-Konsole
http://localhost:8100/api/message          → REST API
```

**Wichtig:** Port 8100 ist der Standard für Web Portal (8100 wird von OpenSim verwendet)

✅ Web-Interface läuft!

---

## 🔧 Troubleshooting

### Problem: Server beendet sich sofort unter Linux

**Gelöst!** Der Server verwendet jetzt ein verbessertes Console-System:

- Bleibt auch ohne interaktive Console laufen
- Funktioniert in Docker/systemd/tmux
- Falls Console nicht verfügbar ist, läuft der HTTP-Server trotzdem weiter

**Zum Beenden:**

- Interaktiv: `shutdown` Befehl eingeben
- Nicht-interaktiv: `kill <PID>` oder `Ctrl+C`

### Problem: "Template file not found"

**Lösung:** Templates wurden nicht kopiert

```powershell
Copy-Item "addon-modules\OpenSim-Web-Portal\websites\templates\*.html" "bin\portal\templates\" -Force
```

### Problem: "Port already in use" / "Address already in use"

**Fehler:** `System.Net.Sockets.SocketException (98): Address already in use`

**Ursache:** Der konfigurierte Port wird bereits verwendet.

**💡 Hinweis:** Der Standard-Port ist **8100** (nicht 8100!)  
**Port-Übersicht:**

- Port 8002 = Robust (Grid Services)
- Port 8008 = MoneyServer
- Port 8080 = Console API
- Port 8100 = Web Portal (Standard)
- Port 8100 = OpenSim (Region Server)

Lösung 1: Port ändern

Erstelle oder bearbeite `bin/WebPortal.ini`:

```ini
[WebPortal]
Port = 8100  ; oder einen anderen freien Port
```

Dann Server neu starten:

```bash
dotnet OpenSim.Web.Portal.dll
```

Lösung 2: Anderen Prozess beenden (Windows)

Finde heraus, welcher Prozess Port 8100 belegt:

```powershell
netstat -ano | Select-String ":8100"
# Zeigt PID des Prozesses
```

Prozess beenden:

```powershell
Stop-Process -Id <PID> -Force
```

Lösung 3: Anderen Prozess beenden (Linux)

Finde heraus, welcher Prozess Port 8100 belegt:

```bash
sudo lsof -i :8100
# oder
sudo netstat -tulpn | grep :8100
```

Prozess beenden:

```bash
kill <PID>
```

### Problem: CSS wird nicht geladen

**Lösung:** CSS-Datei kopieren

```powershell
Copy-Item "addon-modules\OpenSim-Web-Portal\websites\css\custom.css" "bin\portal\css\" -Force
```

### Problem: "OpenSim.Web.Portal.dll not found"

**Lösung:** Kompiliere das Projekt:

```powershell
cd D:\opensimcurrencyserver-dotnet-12_2025\opensim
.\compile.bat
```

---

## 📝 Häufige Aufgaben

### Templates bearbeiten

1. Datei öffnen: `addon-modules\OpenSim-Web-Portal\websites\templates\*.html`
2. Änderungen speichern
3. Nach `bin\portal\templates\` kopieren:

   ```powershell
   Copy-Item "addon-modules\OpenSim-Web-Portal\websites\templates\*.html" "bin\portal\templates\" -Force
   ```

4. Seite im Browser neu laden (F5)

**Hinweis:** Kein Rebuild nötig, nur Kopieren!

### CSS anpassen

1. Datei öffnen: `addon-modules\OpenSim-Web-Portal\websites\css\custom.css`
2. Styles ändern
3. Nach `bin\portal\css\` kopieren:

   ```powershell
   Copy-Item "addon-modules\OpenSim-Web-Portal\websites\css\custom.css" "bin\portal\css\" -Force
   ```

4. Seite im Browser neu laden (Ctrl+F5 für Hard Refresh)

### Neue Seite hinzufügen

1. Template erstellen: `addon-modules\OpenSim-Web-Portal\websites\templates\neuepage.html`
2. Handler registrieren in `WebPortalServer.cs`:

   ```csharp
   m_httpServer.AddStreamHandler(new Handlers.PortalPageHandler(
       m_templatePath, "neuepage", "/portal/neuepage"));
   ```

3. Kompilieren: `.\compile.bat`
4. Template kopieren (siehe oben)
5. Server starten

### Port ändern

1. Konfiguration erstellen (falls nicht vorhanden):

   ```powershell
   Copy-Item "addon-modules\OpenSim-Web-Portal\OpenSim.Web.Portal\WebPortal.ini.example" "bin\WebPortal.ini"
   ```

2. `bin\WebPortal.ini` öffnen und Port ändern:

   ```ini
   [WebPortal]
   Port = 9001
   ```

3. Server neu starten

### Console-Befehle verwenden

Im laufenden Server-Terminal:

```bash
help                → Zeigt alle Befehle
show status         → Server-Status und Uptime
shutdown            → Server beenden
```

---

## 🌐 Alle verfügbaren Seiten

| URL | Beschreibung | Status |
|-----|--------------|--------|
| `/` | Hauptseite mit Statistiken | ✅ UI fertig |
| `/portal/login` | Login-Formular | ✅ UI fertig |
| `/portal/register` | Registrierung | ✅ UI fertig |
| `/portal/account` | Account-Verwaltung | ✅ UI fertig |
| `/portal/inventory` | Inventar-Browser | ✅ UI fertig |
| `/portal/forgot-password` | Passwort-Reset | ✅ UI fertig |
| `/portal/about` | Über das Projekt | ✅ Fertig |
| `/portal/admin/users` | Benutzerverwaltung | ✅ UI fertig |
| `/portal/admin/console` | Web-Konsole | ✅ UI fertig |
| `/api/message` | REST API (JSON) | ✅ Fertig |

**Legende:**

- ✅ UI fertig = Template komplett, Backend folgt in Phase 2
- ✅ Fertig = Komplett funktional

---

## 📚 Weiterführende Dokumentation

- **[README.md](README.md)** - Vollständige Projekt-Dokumentation
- **[PAGES.md](PAGES.md)** - Detaillierte Seiten-Dokumentation mit Features und Template-Variablen
- **[WifiPages/](WifiPages/)** - Alte Wifi-Templates als Referenz

---

## 🎯 Nächste Schritte (Phase 2)

Das UI ist komplett fertig! Nächste Phase ist die Backend-Integration:

1. **Authentication System** - Login/Logout mit UserAccountService
2. **Session Management** - Cookie-basierte Sessions
3. **User Registration** - Account-Erstellung Backend
4. **Password Recovery** - E-Mail-basierter Reset
5. **Account Management** - Profil-Bearbeitung

Siehe [README.md - Roadmap](README.md#roadmap) für Details.

---

## 💡 Tipps & Tricks

### Entwicklungs-Workflow

1. **Änderungen an Templates/CSS:** Nur kopieren, kein Rebuild
2. **Änderungen an C#-Code:** Kompilieren mit `.\compile.bat`
3. **Server neu starten:** Nach C#-Änderungen erforderlich
4. **Browser Cache leeren:** Ctrl+F5 für Hard Refresh

### Performance

- Templates werden bei jedem Request neu geladen (Development-Mode)
- Für Production: Template-Caching implementieren
- CSS/JS sollten gecacht werden (Browser-Cache)

### Debugging

- **Log-Datei:** `bin\WebPortal.log` (falls konfiguriert)
- **Console-Output:** Direkt im Server-Terminal
- **Browser DevTools:** F12 für Network/Console

### Sicherheit (für Production)

- [ ] HTTPS aktivieren
- [ ] Session-Security implementieren
- [ ] Input-Validierung hinzufügen
- [ ] CSRF-Protection einbauen
- [ ] Rate-Limiting für API

---

## ❓ Support

Bei Fragen oder Problemen:

1. **Logs prüfen:** Console-Output und `bin\WebPortal.log`
2. **README.md lesen:** Vollständige Dokumentation
3. **PAGES.md lesen:** Seiten-spezifische Informationen
4. **OpenSim-Community:** <http://opensimulator.org/>

---

**Happy Coding!** 🚀

Das OpenSim Web Portal Team
