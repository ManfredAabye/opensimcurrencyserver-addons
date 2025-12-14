# todo OpenSim-Web-Portal

## Information

Erstelle ein Webinterface Addon Modul für OpenSim mit folgenden Features:
Nutze das OpenSim-Addon-Example als Grundlage.
Nutze die WifiPages als Vorlage und erstelle anhand dieser ein modernes Webinterface auf Bootstrap 5 Basis.
Mache aus WifiPages OsWebPages.
Das Webinterface soll alle Funktionen bieten die auch die WifiPages bieten.

Jetzt fehlt noch der passende Server wie ind OpenSim-Addon-Example oder OpenSim-Money-Accounting.
Das Addon soll folgende Features bieten:

- Webinterface auf variablen Port Standard 9000
- REST API Endpoint `/api/message` der eine JSON Nachricht zurückgibt
- Konsolenbefehle `help`, `show status` und `shutdown`
- Logging im OpenSim Stil
- 100% OpenSim Kompatibilität wie im OpenSim-Addon-Example beschrieben

## TODO

Es fehlen wichtige einzelne Firetorm Viewer Seiten die über den Viewer aufgerufen werden wie:
welcome.html
splash.html
guide.html
tos.html
termsofservice.html
rss.html
help.html

Dazu noch:
404.html

Erstelle sie bitte Dateiweise.
Nutze dazu die WifiPages als Vorlage.

Firestorm Viewer erwartet folgende Grid-Manager Seiten aus dem OpenSim-Web-Portal.

Grid-Name
Grid-URI
Anmeldeseite
Hilfe-URI
Grid-Website
Grid-Support
Grid-Registrierung
Grid-Passwort-URI
Grid-Suche
Grid-Nachrichten-URI

Die Seiten in der OpenSim Konfiguration:

[LoginService]
WelcomeMessage = "Welcome, Avatar!"
AllowRemoteSetLoginLevel = "false"

; For V2 map
MapTileURL = "${Const|BaseURL}:${Const|PublicPort}/";

; Url to search service
; SearchURL = "${Const|BaseURL}:${Const|PublicPort}/";

; For V3 destination guide
; DestinationGuide = "${Const|BaseURL}/guide"

; For V3 avatar picker (( work in progress ))
; AvatarPicker = "${Const|BaseURL}/avatars"

[GridInfoService]
; login uri: for grid this is the login server URI
login = ${Const|BaseURL}:${Const|PublicPort}/

; long grid name: the long name of your grid
gridname = "the lost continent of opensim"

; short grid name: the short name of your grid
gridnick = "opensimgrid"

; login page: optional: if it exists it will be used to tell the client to use this as splash page
;welcome = ${Const|BaseURL}/welcome

; helper uri: optional: if it exists it will be used to tell the client to use this for all economy related things
;economy = ${Const|BaseURL}/economy

; web page of grid: optional: page providing further information about your grid
;about = ${Const|BaseURL}/about

; account creation: optional: page providing further information about obtaining a user account on your grid
;register = ${Const|BaseURL}/register

; help: optional: page providing further assistance for users of your grid
;help = ${Const|BaseURL}/help

; password help: optional: page providing password assistance for users of your grid
;password = ${Const|BaseURL}/password

; HG address of the gatekeeper, if you have one this is the entry point for all the regions of the world
; gatekeeper = ${Const|BaseURL}:${Const|PublicPort}/

; a http page for grid status
;GridStatus = ${Const|BaseURL}:${Const|PublicPort}/GridStatus
; a RSS page for grid status
;GridStatusRSS = ${Const|BaseURL}:${Const|PublicPort}/GridStatusRSS
