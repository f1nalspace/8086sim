# Fortschritt: 8086sim — .NET 10 + AvaloniaUI

> Lebende Status-Datei zur Begleitung des Plans [`8086sim-avaloniaui.md`](./8086sim-avaloniaui.md).
> Über mehrere Claude-Sessions gepflegt. **Legende:** ⬜ offen · 🟡 in Arbeit · ✅ erledigt · ⛔ blockiert.

**Stand:** 2026-05-29 · **Branch:** `upgrade/avalonia-ui`

---

## Gesamtstatus

| Phase | Inhalt | Status |
|---|---|---|
| 0 | Vorbereitung & Baseline | ✅ erledigt |
| 1 | Non-UI-Projekte → .NET 10 (+ xUnit) | ✅ erledigt |
| 2 | Avalonia-Projektgerüst | ✅ erledigt |
| 3 | MVVM-Infrastruktur | ⬜ |
| 4 | Converters & BinaryGridView | ⬜ |
| 5 | MainWindow-Nachbau (inkl. Ribbon) | ⬜ |
| 6 | Plattform-/IDE-Verifikation | ⬜ |

---

## Aufgaben (Checkliste)

### Phase 0 — Vorbereitung & Baseline
- [x] Projekte/Abhängigkeiten inventarisiert
- [x] Baseline-Build Core + Console (net9) → kompiliert
- [x] Console-Disassembly auf Linux verifiziert (`--res performance_aware.listing_0037_single_register_mov`)
- [x] Baseline-Tests dokumentiert: **5 grün / 14 rot** (Ursachen siehe Findings)
- [x] Plan- & Fortschrittsdatei angelegt
- [x] Bei Session-Start: `git submodule update --init --recursive` geprüft (Session 2: aktuell)

### Phase 1 — Non-UI-Projekte → .NET 10 (+ xUnit-Umstellung)
- [x] `CPU8086` TFM → `net10.0`
- [x] `CPU8086.Console` TFM → `net10.0`
- [x] `CPU8086.Resources` TFM → `net10.0`
- [x] `InstructionTableParser` TFM → `net10.0`
- [x] `CPU8086.Tests` TFM → `net10.0`
- [x] **Testframework MSTest → xUnit** (Nutzeranforderung Session 2): `xunit` 2.9.2 + `xunit.runner.visualstudio` 2.8.2 + `Microsoft.NET.Test.Sdk` 17.12.0 + `coverlet.collector` 6.0.2. Testcode konvertiert: `[TestClass]` entfernt, `[TestMethod]`→`[Fact]`, leere `[TestInitialize]` entfernt (Setup = Ctor), `Assert.AreEqual`→`Assert.Equal`, 2 Asserts mit Message → `Assert.True(cond, msg)`, `Assert.Fail` bleibt (xUnit ≥2.6).
- [x] Validierung: 5 Non-UI-Projekte bauen (0 Fehler/Warnungen); Console disassembliert (`MOV CX, BX`); Tests **5 grün / 14 rot** = identisch zur Baseline, **keine neuen** roten Tests.

### Phase 2 — Avalonia-Projektgerüst
- [x] `CPU8086.GUI.csproj` umgestellt: `net10.0`, `WinExe`, RootNamespace `Final.CPU8086`. **Raus:** `UseWPF`, `DevExpressMvvm`, `WPFHexaEditor` (F-004 erledigt), WPF-`Resource`-PNGs. **Rein:** `Avalonia` 11.3.17, `Avalonia.Desktop`, `Avalonia.Themes.Fluent`, `Avalonia.Diagnostics` (Debug). `AvaloniaResource Include="Resources\**\*.png"`. CT.Mvvm folgt in Phase 3.
- [x] `Program.cs` (Avalonia-Entry, `BuildAvaloniaApp().StartWithClassicDesktopLifetime`) + `App.axaml(.cs)` mit `FluentTheme`. (DataGrid-Styles erst in Phase 4/5, wenn DataGrid eingeführt wird.)
- [x] Platzhalter-`MainWindow.axaml(.cs)`; App startet unter Linux.
- [x] **Alte WPF-Bootstrap-Dateien gelöscht** (`git rm`): `App.xaml(.cs)`, `MainWindow.xaml(.cs)`, `AssemblyInfo.cs` (WPF-`ThemeInfo`), `Controls/BinaryGridView.xaml`. Alle aus git-Historie wiederherstellbar (Referenz für Phase 4/5).
- [x] **Temporär aus dem Build genommen** (via `<Compile Remove>`, sichtbar als `<None>`): `MainViewModel.cs`, `LogItemViewModel.cs`, `AutoServiceBehavior.cs`, `DecodeState.cs`, `StreamByte.cs`, `IAutoService.cs`, `Behaviors/**`, `Controls/**`, `Converters/**`, `Services/**`. **→ Phase 3/4 müssen diese Removes wieder entfernen, sobald portiert.**
- [x] Validierung: `dotnet build CPU8086.sln` grün (0 Fehler); GUI-Fenster lief im Smoke-Test 8 s stabil unter X11 (`:0`), keine Exception; Tests unverändert 5/14; xUnit2004-Warnungen in `FlagsTests` bereinigt (`Assert.False/True`).

### Phase 3 — MVVM-Kompatibilitäts-Shim (DevExpress.Mvvm-API auf CT.Mvvm)
- [ ] Shim `ViewModelBase` (GetValue/SetValue-Overloads, Raise*, ISupportServices) — intern `ObservableObject`
- [ ] Shim `DelegateCommand` / `DelegateCommand<T>` (`RaiseCanExecuteChanged`) — intern `RelayCommand`
- [ ] Shim `IServiceContainer` + `ServiceContainer` (typed + named), `ISupportServices`
- [ ] Shim `IDispatcherService` — Wrapper um `Dispatcher.UIThread`
- [ ] ViewModels **unverändert** lauffähig gegen Shim (`MainViewModel`, `BinaryGridViewModel`, `LogItemViewModel`)
- [ ] Ungenutzte `using DevExpress.Mvvm.Native;` entfernt (F-008)
- [ ] Behaviors auf `Avalonia.Xaml.Interactivity.Behavior<T>` basiert (`AutoServiceBehavior`, `BinaryGridServiceBehavior`)
- [ ] `Behaviors/AttachServiceBehavior` auf toten Code geprüft (F-006)
- [ ] UI-freie Typen übernommen (`IAutoService`, Service-Interfaces, `DecodeState`, `StreamByte`, `BinaryGridEvents`)
- [ ] Validierung: GUI kompiliert

### Phase 4 — Converters & BinaryGridView
- [ ] Converter portiert (`Avalonia.Data.Converters`)
- [ ] `NumericToVisibilityConverter`-Nutzung ersetzt (IsVisible + int→bool)
- [ ] `BinaryGridView`: StyledProperties + `.axaml` neu aufgebaut
- [ ] Validierung: Control rendert isoliert

### Phase 5 — MainWindow-Nachbau
- [ ] Register-Panel (AX–DI, Segmente, IP)
- [ ] Flags-Panel
- [ ] Stream-/Memory-Tabs mit `BinaryGridView`
- [ ] Assembly-Output (`ItemsControl`)
- [ ] Instructions-`DataGrid`
- [ ] Errors-/Log-Ansicht (read-only DataGrid)
- [ ] **Ribbon nachgebaut** (Home/View, Gruppen, Icon-Buttons, Programm-ComboBox, Hex-Checkboxen)
- [ ] `Loaded`-Command verdrahtet
- [ ] Validierung: Listing laden, Disassembly/Instructions/Memory sichtbar; Run/Step/Stop/Reset funktionieren

### Phase 6 — Plattform-/IDE-Verifikation
- [ ] Smoke-Test Linux
- [ ] Smoke-Test Windows (falls verfügbar)
- [ ] Rider lädt & baut Solution
- [ ] Visual Studio lädt & baut Solution
- [ ] Endvalidierung gegen Definition of Done

---

## Findings (Bugs/Smells — getrackt, NICHT gefixt)

| ID | Schwere | Ort | Beschreibung | Entscheidung |
|---|---|---|---|---|
| F-001 | hoch | `CPU8086.Resources/InstructionStreamResources.cs` `Get()` | Prependet nur `Final.CPU8086.Resources.`, ignoriert die Untergruppe `performance_aware.`. Aufruf mit bloßem `listing_xxxx` liefert `null`. Bricht 14 Decode-Tests (Stream null → `ArgumentNullException`) und den Console-Default-Launcharg. | tracken, nicht fixen |
| F-002 | mittel | `CPU8086.Console/Program.cs` | `--exec`-Flag (`ExecuteArgConstant`) deklariert, aber im `Main` nie ausgewertet. Console disassembliert nur (`GetAssembly`), führt keine Programme aus. | tracken, nicht fixen |
| F-003 | niedrig | `CPU8086.Console/Program.cs:201` | `Console.ReadKey()` wirft `InvalidOperationException` bei umgeleiteter/fehlender Konsoleneingabe (Pipe, CI, headless). | tracken, nicht fixen |
| F-004 | niedrig | `CPU8086.GUI.csproj` | `WPFHexaEditor` referenziert, aber nirgends verwendet. Bei Migration ersatzlos entfernbar. | ✅ **erledigt (Phase 2)** — ersatzlos entfernt |
| F-005 | info | `CPU8086.Tests` | Mehrere Tests rot, weil Instructions nicht implementiert sind bzw. Cycle-Zählung abweicht. | **ignorieren** (per Nutzer) |
| F-006 | niedrig | `CPU8086.GUI/Behaviors/AttachServiceBehavior.cs` | Leere abstrakte Behavior-Basis ohne ersichtliche Verwendung — möglicher toter Code. | in Phase 3 prüfen |
| F-007 | info | `CPU8086.Console/Properties/launchSettings.json` | Default-Arg `--res listing_0050_challenge_jumps` greift wegen F-001 nicht (Resource nicht gefunden). | tracken |
| F-008 | niedrig | `MainViewModel.cs`, `Controls/BinaryGridViewModel.cs`, `Converters/HexCellValueConverter.cs` | `using DevExpress.Mvvm.Native;` vorhanden, aber **ungenutzt** (keine Native-Aufrufe). | im Zuge der Portierung entfernen |

> Hinweis zu **F-005**: laut Nutzer sind rote Tests teils erwartbar (nicht implementierte Instructions /
> Cycles). Diese werden nicht als Migrationsdefekt gewertet.

---

## Baseline-Messwerte (vor Migration, net9)

- **Build**: `CPU8086`, `CPU8086.Console` kompilieren ohne Fehler/Warnungen.
- **Console**: `dotnet run --project CPU8086.Console -- --res performance_aware.listing_0037_single_register_mov`
  → korrektes Disassembly (`MOV CX, BX`).
- **Tests**: `dotnet test CPU8086.Tests` → **gesamt 19, grün 5, rot 14** (Hauptursache F-001, dazu F-005).
- **SDK**: .NET 10.0.108 vorhanden (auch 9.0.117). Runtime 10.0.8 vorhanden.

---

## Session-Log

### Session 1 — 2026-05-29
- Bestandsaufnahme der gesamten Lösung (6 Projekte, GUI-Dateien, Abhängigkeiten).
- Baseline-Builds/-Tests gemessen; Console-Disassembly auf Linux bestätigt.
- Entscheidungen mit Nutzer abgestimmt: Tests rot lassen (nur tracken); MVVM = CommunityToolkit.Mvvm;
  Ribbon nachbauen; rote Tests durch fehlende Instructions/Cycles ignorieren.
- Plan (`8086sim-avaloniaui.md`) und dieser Tracker erstellt.
- Findings F-001…F-008 erfasst.
- **Zusatzentscheidung:** MVVM wird als Kompatibilitäts-Shim umgesetzt — identische `DevExpress.Mvvm`-API,
  intern CommunityToolkit.Mvvm. ViewModels bleiben unverändert. (Plan Abschnitt 3.1, Phase 3 angepasst.)

### Session 2 — 2026-05-29
- Session-Start-Check: `git submodule update --init --recursive` ok (`computer_enhance` vorhanden).
- Baseline auf net9 re-bestätigt: **5 grün / 14 rot**.
- **Phase 1 umgesetzt:** TFM `net9.0`→`net10.0` in `CPU8086`, `CPU8086.Console`, `CPU8086.Resources`,
  `InstructionTableParser`, `CPU8086.Tests`. Alle 5 Non-UI-Projekte bauen ohne Fehler/Warnungen.
- **Neue Nutzeranforderung mitten in Session: Testframework MSTest → xUnit.** Umgesetzt (Pakete + Code-Konvertierung,
  Details in Phase-1-Checkliste). Ergebnis: **5 grün / 14 rot** — bitidentisch zur Baseline, keine neuen roten Tests.
- Console-Disassembly auf net10 verifiziert (`MOV CX, BX`); F-003 (ReadKey bei Pipe) tritt erwartungsgemäß
  weiter auf, kein Regress.
- **Phase 2 umgesetzt:** GUI-Projekt auf Avalonia 11.3.17 / `net10.0` umgestellt; WPF/DevExpress/WPFHexaEditor
  entfernt (F-004 ✅); alte WPF-Bootstrap-Dateien gelöscht; Avalonia-`Program.cs`/`App.axaml`/Platzhalter-`MainWindow`
  angelegt. Noch nicht portierte WPF-Quellen temporär via `<Compile Remove>` ausgeschlossen (Phase 3/4 reaktivieren).
  Validierung: Solution baut grün; Avalonia-Fenster startet stabil unter X11.
- **Avalonia-Version fixiert:** 11.3.17 (aktuelle Stable-Linie, .NET-10-kompatibel).
- **Nächster Schritt:** Phase 3 — MVVM-Kompatibilitäts-Shim (`DevExpress.Mvvm`-API auf CommunityToolkit.Mvvm),
  dann ViewModels/Behaviors/Services schrittweise wieder einkompilieren (die `<Compile Remove>`-Einträge entfernen).
