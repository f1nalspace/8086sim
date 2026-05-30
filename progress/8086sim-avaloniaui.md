# Plan: 8086sim — Portierung auf .NET 10 + AvaloniaUI

> Ziel: Alle Projekte auf **.NET 10**. Das WPF-Projekt `CPU8086.GUI` auf **AvaloniaUI** portieren,
> sodass die komplette Lösung unter **Linux und Windows** läuft, mit **Visual Studio** oder **JetBrains Rider**.
>
> Diese Datei ist der **Plan** (Architektur, Entscheidungen, Phasen). Der laufende Status steht in
> [`8086sim-fortschritt.md`](./8086sim-fortschritt.md).

---

## 1. Rahmenbedingungen & Regeln

- Umsetzung über **mehrere Claude-Sessions** (Usage-Limit). Jede Phase ist in sich abschließbar.
- **Bestehenden Programmierstil & Architektur beibehalten.** Keine unnötigen neuen Klassen/Interfaces.
- Neue Klassen/Interfaces nur wo technisch nötig (z. B. MVVM-Ersatz, Avalonia-Behaviors).
- **Nicht implementierte Instructions ignorieren** (nur decodieren, nicht ausführen).
- **Gefundene Bugs werden getrackt, NICHT gefixt** (siehe Findings im Fortschritts-Tracker).
- Sprache der Doku: Deutsch. Code-Kommentare/Namen: wie bestehend (Englisch).

### Validierungsziele (Definition of Done)

| Ziel | Status-Anmerkung |
|---|---|
| Alle Projekte kompilieren erfolgreich (Linux + Windows) | Kernziel |
| Console-App läuft und verarbeitet die Repo-Listings | Disassembly funktioniert bereits; siehe Bug F-002 (exec) |
| Unit-Tests grün | **Eingeschränkt** — siehe Entscheidung unten |

### Wichtige Entscheidungen (mit dem Nutzer abgestimmt)

1. **Tests**: Bereits vor der Migration sind 14/19 Tests rot (Resource-Namen-Bug + nicht implementierte
   Instructions + abweichende Cycles). Entscheidung: **nur tracken, NICHT fixen**. Das Ziel "alle Tests grün"
   gilt damit als *durch Vorbedingungen blockiert* und wird im Tracker dokumentiert, nicht erzwungen.
   Maßgeblich ist: die Migration darf **keine zusätzlichen** Tests rot machen.
2. **MVVM-Framework**: Statt die ViewModels umzuschreiben, wird eine **Kompatibilitäts-Shim-Schicht**
   gebaut, die die **identische `DevExpress.Mvvm`-API** (gleiche Typen, Signaturen, Verhalten) anbietet,
   **intern aber CommunityToolkit.Mvvm** verwendet. Die bestehenden ViewModels bleiben dadurch
   **unverändert** (gleicher Stil/Architektur). Details: Abschnitt 3.1. Funktioniert in VS und Rider.
3. **Ribbon**: WPF-Ribbon wird in Avalonia **optisch nachgebaut** (Tab-basiertes Layout mit Gruppen).

---

## 2. Ist-Zustand (Bestandsaufnahme)

### Projekte

| Projekt | TFM heute | UI | Portier-Aufwand |
|---|---|---|---|
| `CPU8086` (Core, `Final.CPU8086`) | `net9.0` | – | Trivial (nur TFM) |
| `CPU8086.Console` | `net9.0` | – | Trivial (TFM); Console.ReadKey-Glitch siehe F-003 |
| `CPU8086.Resources` | `net9.0` | – | Trivial (TFM) |
| `CPU8086.Tests` (MSTest → **xUnit**) | `net9.0` | – | TFM + auf xUnit umgestellt (Session 2) |
| `InstructionTableParser` (`Final.ITP`) | `net9.0` | – | Trivial (TFM) — reiner Codegenerator |
| `CPU8086.GUI` | `net9.0-windows`, `UseWPF` | WPF | **Komplette Avalonia-Portierung** |

### Abhängigkeiten der GUI, die ersetzt werden müssen

- **`DevExpressMvvm` 22.1.3** → CommunityToolkit.Mvvm (+ eigene leichte Service-/Dispatcher-Hilfen).
  Genutzt: `ViewModelBase` (GetValue/SetValue POCO-Helfer, `RaisePropertyChanged`, `GetService<T>`),
  `ViewModelSource` (POCO-Weaving im XAML), `DelegateCommand`/`DelegateCommand<T>`,
  `ISupportServices`/`IServiceContainer`/`ServiceContainer`, `IDispatcherService`,
  `dxmvvm:Interaction.Behaviors`, `EventToCommand`, `DispatcherService`, `NumericToVisibilityConverter`,
  `Behavior<T>` (aus `DevExpress.Mvvm.UI.Interactivity`).
- **`WPFHexaEditor` 2.1.7** → **ersatzlos entfernen** (im Code/XAML nicht verwendet; das Hex-Grid ist
  der eigene `BinaryGridView`).
- **WPF Ribbon** (`System.Windows.Controls.Ribbon`) → Avalonia-Nachbau.

### GUI-Dateien (Inventar)

```
App.xaml(.cs)                         Window/App-Bootstrap
MainWindow.xaml(.cs)                  Hauptfenster: Ribbon, Register-Panel, Stream/Memory-Tabs,
                                      Assembly-Output, Instructions-DataGrid, Errors/Log, Flags
MainViewModel.cs                      Haupt-VM (ViewModelBase, IMemoryAddressResolverService)
LogItemViewModel.cs                   ViewModelBase
DecodeState.cs / StreamByte.cs        einfache Typen (UI-frei, übernehmbar)
IAutoService.cs                       Marker-Interface für Service-Registrierung
AutoServiceBehavior.cs                Behavior<T> (DevExpress) — registriert IAutoService im Container
Behaviors/AttachServiceBehavior.cs    leere abstrakte Behavior-Basis (vermutlich toter Code → prüfen)
Controls/BinaryGridView.xaml(.cs)     eigenes Hex/Bin-Grid (UserControl, DependencyProperties)
Controls/BinaryGridViewModel.cs       VM des Grids (ViewModelBase, Service-Lookup)
Controls/BinaryGridServiceBehavior.cs Behavior — registriert IMemoryAddressResolverService
Controls/BinaryGridEvents.cs          Event-Delegates/-Args
Controls/IBinaryGridService.cs        Service-Interface fürs Grid
Services/IMemoryAddressResolverService.cs
Converters/*.cs                       BoolToInt, BytePositionToString, BytesToStreamBytes,
                                      HexCellValue, IsInsideInstructionRange, IsInsideRange,
                                      LengthToPos  (teils MarkupExtension+IValue/IMultiValueConverter)
Resources/*.png                       24 Icons (Ribbon/Toolbar/Pager)
```

---

## 3. Ziel-Architektur (Avalonia)

Gleiche Schichtung wie heute — nur die UI-Technologie tauscht. **Core/Console/Resources/Tests/ITP bleiben
strukturell unverändert** (nur TFM `net10.0`).

### 3.1 MVVM-Kompatibilitäts-Shim (DevExpress.Mvvm-API auf CommunityToolkit.Mvvm)

**Leitidee:** Wir bauen eine eigene, schlanke Schicht, die *exakt* die heute verwendete
`DevExpress.Mvvm`-Oberfläche bereitstellt (gleiche Typnamen, gleiche Signaturen, gleiches Verhalten).
**Intern** delegiert sie an CommunityToolkit.Mvvm (`ObservableObject`, `RelayCommand`).
→ Die ViewModels (`MainViewModel`, `BinaryGridViewModel`, `LogItemViewModel`) bleiben **unverändert**.

**Namespace-Entscheidung:** Der Shim verwendet bewusst den Namespace **`DevExpress.Mvvm`** (und
`DevExpress.Mvvm.UI.Interactivity` für die Behavior-Basis), damit auch die `using`-Direktiven der
ViewModels unverändert bleiben → echtes Drop-in. (Namespaces sind in C# nicht „geschützt"; es bleibt
keinerlei DevExpress-Binärabhängigkeit.) *Alternative falls unerwünscht:* Namespace
`Final.CPU8086.Mvvm` + Anpassen der `using`-Zeilen.

**Ort:** Ordner `Mvvm/` im GUI-Projekt (minimal-invasiv). *Alternative:* eigenes Projekt `CPU8086.Mvvm`,
falls Wiederverwendung gewünscht.

#### Nachzubauende Typen & interne Umsetzung

| DevExpress-API (bleibt im Code) | Erforderliche Member | Interne Umsetzung (CT.Mvvm / eigen) |
|---|---|---|
| `ViewModelBase` (Basisklasse aller VMs, implementiert `INotifyPropertyChanged` + `ISupportServices`) | siehe unten | erbt von `ObservableObject`; zusätzlicher POCO-Backing-Store (Dictionary) |
| `T GetValue<T>([CallerMemberName] name)` | keyloses Property-Lesen aus Backing-Store | Dictionary `name→object`, Default `default(T)` |
| `bool SetValue<T>(T value, [name])` | keyloses Setzen + Notify | Backing-Store + `OnPropertyChanged(name)` |
| `bool SetValue<T>(T value, Action changed, [name])` | + Callback bei Änderung | wie oben, danach `changed()` |
| `bool SetValue<T>(ref T field, T value, [name])` | feldbasiert | `ObservableObject.SetProperty(ref field, value, name)` |
| `bool SetValue<T>(ref T field, T value, Action changed, [name])` | feldbasiert + Callback | `SetProperty(...)`; bei `true` → `changed()` |
| `void RaisePropertyChanged([name])` / `RaisePropertyChanged(string)` | Notify | `OnPropertyChanged(name)` |
| `void RaisePropertiesChanged(params string[])` | Mehrfach-Notify | Schleife über `OnPropertyChanged` |
| `T GetService<T>()` / `T GetService<T>(string key)` | Service-Lookup (typed + named) | über `ISupportServices.ServiceContainer` |
| `DelegateCommand` / `DelegateCommand<T>` | `Execute`, `CanExecute`, `RaiseCanExecuteChanged()`, ctor `(execute, canExecute=null)` | Wrapper um `RelayCommand`/`RelayCommand<T>`; `RaiseCanExecuteChanged()`→`NotifyCanExecuteChanged()` |
| `ISupportServices` | `IServiceContainer ServiceContainer { get; }` | von `ViewModelBase` implementiert (lazy Container) |
| `IServiceContainer` | `RegisterService(svc)`, `RegisterService(key, svc)`, `UnregisterService(svc)`, `GetService<T>()`, `GetService<T>(key)` | eigene Klasse `ServiceContainer` (typed + named Dictionary) |
| `IDispatcherService` | `void Invoke(Action)` (genutzte Signatur) | Wrapper um `Dispatcher.UIThread.Invoke/Post` |
| `Behavior<T>` (`DevExpress.Mvvm.UI.Interactivity`) | `AssociatedObject`, `OnAttached`, `OnDetaching` | erbt von `Avalonia.Xaml.Interactivity.Behavior<T>` (API quasi identisch) |

> Hinweis: `using DevExpress.Mvvm.Native;` in 3 Dateien ist **ungenutzt** (keine Native-Aufrufe) →
> Finding F-008. Der Shim braucht **kein** `Native`-Subnamespace; die Usings werden im Zuge der
> Portierung entfernt (zählt nicht als verbotener Bugfix, sondern als notwendige Migrations-Bereinigung).

#### XAML-seitige Konstrukte (kein Shim — Avalonia-Ersatz nötig)

Diese sind WPF/DevExpress-XAML-spezifisch und werden in `.axaml` ohnehin neu geschrieben:

| DevExpress-XAML heute | Avalonia-Ersatz |
|---|---|
| `DataContext="{dxmvvm:ViewModelSource ...}"` (POCO-Weaving) | entfällt — VM direkt als `DataContext` instanziieren |
| `dxmvvm:Interaction.Behaviors` + `EventToCommand(Loaded)` | `Avalonia.Xaml.Interactivity` (`Interaction.Behaviors`) oder `Loaded`-Event → Command |
| `dxmvvm:DispatcherService` (XAML-Behavior) | `IDispatcherService`-Shim per Code an VM gereicht |
| `NumericToVisibilityConverter` | Binding auf `IsVisible` + eigener `int→bool`-Converter (Avalonia kennt keine `Visibility`-Enum) |
| `Behavior<T>`-Instanzen im XAML (`AutoServiceBehavior`, `BinaryGridServiceBehavior`) | gleiche Klassen, Basis = Avalonia-Behavior; via `Interaction.Behaviors` eingebunden |

### Avalonia-spezifische Umbauten

- `DependencyProperty.Register(...)` → `StyledProperty<T>` / `AvaloniaProperty.Register<TOwner,T>`
  (im `BinaryGridView`). `PropertyChanged`-Callbacks über `AvaloniaProperty.Changed.Subscribe` bzw.
  `OnPropertyChanged`.
- `IValueConverter` / `IMultiValueConverter`: Avalonia-Namespace `Avalonia.Data.Converters`.
  `MarkupExtension`-Converter werden zu Instanzen in `*.axaml`-Resources (Avalonia unterstützt
  `MultiBinding` und `IMultiValueConverter`).
- XAML → **`.axaml`**; Namespaces auf `https://github.com/avaloniaui`.
  `Window`/`UserControl`/`Grid`/`TabControl`/`ItemsControl`/`Border`/`TextBlock`/`StackPanel`/`DockPanel`/
  `GridSplitter`/`ScrollViewer`/`WrapPanel` existieren in Avalonia (teils leicht andere Properties).
- **DataGrid**: Paket `Avalonia.Controls.DataGrid` + Theme-Include.
- **Ribbon**: Nachbau via `TabControl` (Tabs „Home"/„View") mit Gruppen-`Border`n und Icon-Buttons.
- **ListView/GridView** (Errors/Log) → Avalonia hat kein `GridView`; Ersatz `DataGrid` (read-only) oder
  `ItemsControl`/`ListBox` mit Spalten-Template. Empfehlung: read-only `DataGrid` (konsistent mit Instructions).
- `ToolTip="..."` → `ToolTip.Tip="..."`.
- `Visibility` → `IsVisible` (bool).
- `StringFormat`/`MultiBinding` Syntax minimal anders; prüfen.
- Bilder: PNGs als `AvaloniaResource`; URIs `avares://CPU8086.GUI/Resources/xxx.png` oder relativ.
- App-Bootstrap: `Program.Main` mit `BuildAvaloniaApp().StartWithClassicDesktopLifetime(args)`,
  `App.axaml` mit `FluentTheme` + `DataGrid`-Styles + `Application.DataTemplates`.

### GUI csproj (Ziel-Skizze)

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net10.0</TargetFramework>      <!-- plattformneutral, kein -windows mehr -->
    <RootNamespace>Final.CPU8086</RootNamespace>
    <BuiltInComInteropSupport>true</BuiltInComInteropSupport>
    <ApplicationManifest>app.manifest</ApplicationManifest> <!-- optional -->
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Avalonia" Version="11.*" />
    <PackageReference Include="Avalonia.Desktop" Version="11.*" />
    <PackageReference Include="Avalonia.Themes.Fluent" Version="11.*" />
    <PackageReference Include="Avalonia.Controls.DataGrid" Version="11.*" />
    <PackageReference Include="Avalonia.Xaml.Interactions" Version="11.*" />
    <PackageReference Include="CommunityToolkit.Mvvm" Version="8.*" />
    <!-- nur im Debug: Avalonia.Diagnostics -->
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\CPU8086.Resources\CPU8086.Resources.csproj" />
    <ProjectReference Include="..\CPU8086\CPU8086.csproj" />
  </ItemGroup>
  <ItemGroup>
    <AvaloniaResource Include="Resources\**\*.png" />
  </ItemGroup>
</Project>
```

> Genaue Avalonia-Version (11.x) zu Beginn von Phase 4 fixieren (kompatibel zu .NET 10 prüfen).

---

## 4. Phasenplan (sessiontauglich)

Jede Phase endet in einem **kompilierbaren** Zustand (außer wo anders vermerkt). Reihenfolge so gewählt,
dass die Validierung (Core/Console/Tests) früh und unabhängig von der UI grün gezogen werden kann.

### Phase 0 — Vorbereitung & Baseline *(teils erledigt in dieser Session)*
- [x] Repo, Build-System, Abhängigkeiten inventarisieren.
- [x] Baseline-Build Core+Console (net9) — kompiliert.
- [x] Baseline-Tests dokumentieren (14/19 rot, Ursachen erfasst → Findings).
- [x] Plan- und Fortschrittsdatei anlegen.
- [ ] (Session-Start) `git submodule update --init --recursive` sicherstellen.

### Phase 1 — Non-UI-Projekte auf .NET 10
- TFM `net9.0` → `net10.0` in: `CPU8086`, `CPU8086.Console`, `CPU8086.Resources`, `InstructionTableParser`.
- `CPU8086.Tests`: TFM `net10.0`; **Wechsel MSTest → xUnit** (Nutzerentscheidung Session 2):
  `xunit` 2.9.2, `xunit.runner.visualstudio` 2.8.2, `Microsoft.NET.Test.Sdk` 17.12.0, `coverlet.collector` 6.0.2.
  Code-Konvertierung: `[TestClass]`→entfällt, `[TestMethod]`→`[Fact]`, leeres `[TestInitialize]`→entfällt (Ctor),
  `Assert.AreEqual`→`Assert.Equal`, Message-Asserts→`Assert.True(cond, msg)`. xUnit funktioniert in VS & Rider.
- **Validierung Phase 1**: alle 5 Non-UI-Projekte kompilieren; Console disassembliert ein Listing;
  Testlauf zeigt **dieselben** Fehlschläge wie Baseline (keine *neuen* roten Tests).
- *Bugs aus dieser Phase nur tracken.*

### Phase 2 — Avalonia-Projektgerüst
- `CPU8086.GUI.csproj` umstellen (siehe Skizze): WPF raus, Avalonia + CT.Mvvm rein, WPFHexaEditor raus,
  DevExpress raus, TFM `net10.0`.
- Neu: `Program.cs` (Avalonia-Entry), `App.axaml`(`.cs`) mit FluentTheme + DataGrid-Styles.
- Leeres `MainWindow.axaml` als Platzhalter, App startet (leeres Fenster) unter Linux.
- **Validierung**: `dotnet build` der GUI grün; App startet headless/lokal.

### Phase 3 — MVVM-Kompatibilitäts-Shim (statt VM-Umschreibung)
- **Shim-Schicht** (`Mvvm/`, Namespace `DevExpress.Mvvm`) bauen — siehe Abschnitt 3.1:
  `ViewModelBase` (GetValue/SetValue/Raise*, ISupportServices), `DelegateCommand`/`DelegateCommand<T>`,
  `IServiceContainer`/`ServiceContainer`, `ISupportServices`, `IDispatcherService` — intern CT.Mvvm.
- ViewModels (`MainViewModel`, `BinaryGridViewModel`, `LogItemViewModel`) bleiben **unverändert**;
  nur ungenutzte `using DevExpress.Mvvm.Native;` entfernen (F-008).
- `IAutoService`, `IBinaryGridService`, `IMemoryAddressResolverService`, `DecodeState`, `StreamByte`,
  `Controls/BinaryGridEvents` übernehmen (UI-frei → meist unverändert).
- `Behaviors/AttachServiceBehavior` auf toten Code prüfen (F-006).
- **Umgesetzte Abweichung (Session 2):** `DelegateCommand` als eigene `ICommand`-Impl. statt `RelayCommand`-Wrapper
  (Werttyp-`CanExecute`-Semantik). Die **Behaviors** (`AutoServiceBehavior`, `BinaryGridServiceBehavior`) sind nach
  **Phase 4** verschoben — sie hängen an `BinaryGridView` + `DependencyProperty` und werden mit dem Control portiert.
- **Validierung**: GUI kompiliert; ViewModels lassen sich gegen den Shim instanziieren (noch ohne fertige Views).

### Phase 4 — Converters, BinaryGridView (Custom Control) & Behaviors
- **Behaviors** (`AutoServiceBehavior`, `BinaryGridServiceBehavior`) auf `Avalonia.Xaml.Interactivity.Behavior<T>`
  basieren lassen (API quasi identisch); dazu `Behavior<T>`-Shim + Paket `Avalonia.Xaml.Interactions`.
- Converter auf Avalonia portieren (`Avalonia.Data.Converters`); `NumericToVisibilityConverter`-Nutzung
  durch `IsVisible` + `int→bool`-Converter ersetzen.
- `BinaryGridView`: `DependencyProperty`→`StyledProperty`, Callbacks anpassen, `.axaml` neu aufbauen
  (ItemsControl/WrapPanel/Pager-Buttons, MultiBinding auf Range-Converter, Icons via avares://).
- **Validierung**: Control rendert isoliert (z. B. in Platzhalter-View).

### Phase 5 — MainWindow neu aufbauen
- Layout 1:1 nachbilden: Register-Panel (AX–DI, Segmente, IP, Flags), Stream/Memory-Tabs mit
  `BinaryGridView`, Assembly-Output (`ItemsControl`), Instructions-`DataGrid`, Errors/Log, Flags-Panel.
- **Ribbon nachbauen** (TabControl „Home"/„View", Gruppen, Icon-Buttons, Programm-ComboBox,
  Hex-Checkboxen). `EventToCommand(Loaded)` → Avalonia-Äquivalent.
- Binding-Details: `ElementName`, `RelativeSource FindAncestor`, `MultiBinding`, `StringFormat`.
- **Validierung**: App startet unter Linux, Programm-Auswahl lädt Listing, Disassembly + Instructions +
  Memory/Stream-Grid sichtbar; Run/Step/Stop/Reset funktionieren (für implementierte Instructions).

### Phase 6 — Plattform-/IDE-Verifikation & Feinschliff
- Manueller Smoke-Test Linux (und – falls verfügbar – Windows).
- Rider/VS: Solution lädt, alle Projekte bauen.
- Icons, Schriftgrößen, Tab-/Splitter-Verhalten prüfen.
- Aufgeräumte `clear.bat`/README-Hinweise optional (nur falls vom Nutzer gewünscht).
- **Endvalidierung** gegen Definition of Done; offene Findings final im Tracker zusammenfassen.

---

## 5. Risiken & offene Punkte

- **Avalonia DataGrid** Verhalten (Spalten-Auto-Sizing, ReadOnly-RowStyle/Trigger) weicht von WPF ab —
  Trigger-Logik (`isOnInstructionConverter`) ggf. über `Classes`/`DataTriggers`-Äquivalent lösen.
- **MultiBinding + MarkupExtension-Converter**: in Avalonia als Ressource-Instanzen; Syntax testen.
- **`ServiceContainer`/keyed Services**: DevExpress-Semantik genau nachbilden (named `"MemoryGridService"`).
- **Ribbon-Nachbau**: kein Standardcontrol → Eigenbau, optischer Kompromiss möglich.
- **Threading**: `MainViewModel` nutzt `Task.Run` + Dispatcher stark; UI-Updates müssen über
  `Dispatcher.UIThread` laufen (Avalonia ist strikt).
- **Tests bleiben rot** (per Entscheidung). Sicherstellen, dass die Migration die Zahl roter Tests
  nicht erhöht.

---

## 6. Bekannte Findings (Kurzliste — Details im Tracker)

- **F-001** Resource-Namen-Bug: `InstructionStreamResources.Get` ignoriert die `performance_aware.`-Gruppe
  → Tests & Console-Default-Arg finden Streams nicht. *(tracken, nicht fixen)*
- **F-002** Console `--exec`-Flag deklariert, aber nie ausgewertet — Console disassembliert nur, führt nicht aus.
- **F-003** Console `Console.ReadKey()` wirft bei umgeleiteter Eingabe (z. B. Pipe/CI).
- **F-004** WPFHexaEditor referenziert, aber ungenutzt.
- **F-005** Diverse Tests rot wegen nicht implementierter Instructions / abweichender Cycles. *(ignorieren)*
- **F-006** `Behaviors/AttachServiceBehavior` evtl. toter Code (während Phase 3 prüfen).
