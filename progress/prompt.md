# Context

Ich habe vor Jahren einen 8086 CPU Simulator programmiert im Rahmen eines Programmierkurses.
Dieser ist in WPF entwiclelt, läuft daher aktuell nur in Windows.

# Aufgabe

Alle Projekte sollen auf .NET 10 portiert werden und das WPF-Projekt soll auf AvaloniaUI portiert werden.
Ziel ist es dass alles unter Linux sowie Windows problemlos läuft, mit entweder Visual Studio oder Jetbrains Rider.
Erstelle eine Plan-Datei als Markdown in dem Unterodner "progress" namens "8086sim-avaloniaui.md".
Zusätzlich erstelle eine weitere Markdown-Datei die den kompletten Fortschritt trackt.

# Regeln
- Die Aufgabe soll in mehreren Claude Sessions implementiert werden, da wir vom Usage-Limit begrenzt sind
- Nutze den vorhandenen Programmierstil mit gleicher Architektur
- Erstelle nur neue Klassen/Interfaces wenn nötig
- Frag nach wenn etwas unklar ist
- Ignoriere Instructions die nicht implementiert sind
- Wenn Du Fehler im Code findest, dann tracke die - aber behebe diese nicht!

# Validierung
- Alle Projekt kompilieren erfolgreich
- Die Consolen-App läuft problemlos und kann 8086-Programme ausführen die im Repo enthalten sind
- Unit-Tests sollen logischerweise alle Grün sein
