# RawCleaner

RawCleaner ist eine moderne, schnelle und benutzerfreundliche Windows-Desktopanwendung (WPF), die Fotografen hilft, Speicherplatz zu sparen und Ordnung in ihren Foto-Workflows zu halten.

Wenn du Fotos aussortierst und die JPEGs löschst, bleiben die großen RAW-Dateien oft unbemerkt auf der Festplatte liegen. RawCleaner analysiert deine Ordner, findet diese "verwaisten" RAW-Dateien ohne passendes JPEG und löscht sie mit einem Klick.

## ✨ Hauptfunktionen

* **Intelligente Synchronisation:** Vergleicht JPEG- und RAW-Ordner und findet RAW-Dateien, zu denen das passende JPEG fehlt.
* **Sichere Bereinigung:** Löscht verwaiste RAW-Dateien erst nach deiner ausdrücklichen Bestätigung und einer vorherigen Analyse.
* **Ordner-Sortierung (Mixed Content):** Wenn JPEGs und RAWs im selben Ordner landen, kann RawCleaner die RAW-Dateien automatisch in einen Unterordner verschieben.
* **Detaillierte Berichte:** Exportiere die Analyse-Ergebnisse als CSV-Datei, um genau nachzuvollziehen, welche Dateien behalten oder gelöscht wurden.
* **Windows Explorer Integration:** Registriere RawCleaner direkt im Rechtsklick-Kontextmenü von Windows, um Ordner noch schneller zu analysieren.
* **Vollständig anpassbar:** Konfiguriere deine eigenen RAW-Dateiendungen und Standard-Ordnernamen in den Einstellungen.

## 🛠️ Technologien & Architektur

Dieses Projekt wurde mit Fokus auf sauberen, wartbaren Code entwickelt:

* **C# / .NET** für die performante Logik.
* **WPF (Windows Presentation Foundation)** für die Benutzeroberfläche.
* **MVVM-Pattern** für eine strikte Trennung von Logik und UI.
* **CommunityToolkit.Mvvm** für effizientes Data-Binding und Commands.
* **WPF UI (lepo.co)** für das moderne Windows 11 Fluent Design.

## 🚀 Erste Schritte

### Voraussetzungen
* .NET SDK 
* Visual Studio 2022 (oder eine vergleichbare IDE)

### Installation
1. Klone das Repository über `git clone https://github.com/PhilippSchmid98/raw-cleaner.git`.
2. Öffne die Solution in Visual Studio.
3. Stelle die NuGet-Pakete wieder her.
4. Kompiliere das Projekt und starte die Anwendung.

## 📖 Verwendungf

1. **Ordner auswählen:** Wähle deinen Ordner mit den JPEGs sowie den Ordner mit den dazugehörigen RAW-Dateien. 
2. **Analysieren:** Klicke auf "Analyse starten". RawCleaner zeigt dir an, wie viele verwaiste RAW-Dateien gefunden wurden.
3. **Bereinigen:** Klicke auf "Bereinigen", um die überflüssigen RAW-Dateien unwiderruflich zu löschen.
4. **Bericht exportieren:** Speichere die Datei-Aktionen zur Sicherheit als CSV-Datei ab.

## ⚙️ Einstellungen

Über das Zahnrad-Symbol erreichst du die Einstellungen. Dort kannst du festlegen:

* Welche Dateiendungen als RAW-Dateien behandelt werden sollen.
* Welche Ordnernamen das Tool automatisch als RAW-Unterordner erkennen soll.
* Den Standardnamen für das Verschieben von RAWs aus gemischten Ordnern.

Die Einstellungen werden lokal unter `%AppData%\RawCleaner\settings.json` gespeichert.

## 🤝 Mitwirken

Beiträge sind jederzeit willkommen! 

1. Öffne ein Issue.
2. Forke das Projekt.
3. Erstelle einen Feature-Branch.
4. Committe deine Änderungen.
5. Pushe den Branch.
6. Erstelle einen Pull Request.

## 📄 Lizenz

Dieses Projekt ist unter der MIT-Lizenz lizenziert.