# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

An 8086 CPU simulator in C#/.NET 9, built for the [Computer Enhance](https://www.computerenhance.com) course. It decodes 8086 machine code back into assembly and (for a subset of instructions) executes it, visualizing internal CPU state. Only a handful of instructions are actually *executed*; decoding/disassembly covers far more. The GUI has no real program output beyond a live-updating memory window.

## Build, test, run

```bash
# Restore submodules first (test/sample listings live here)
git submodule update --init --recursive

dotnet build CPU8086.sln
dotnet test CPU8086.Tests/CPU8086.Tests.csproj          # MSTest
dotnet test --filter "FullyQualifiedName~InstructionDecodesTests"   # single test class
dotnet test --filter "Name=IsParity"                    # single test method

dotnet run --project CPU8086.Console -- <args>          # CLI disassembler/runner
```

The GUI (`CPU8086.GUI`) targets `net9.0-windows` with `UseWPF=true` (WPFHexaEditor + DevExpressMvvm), so it only builds/runs on Windows. The current branch `upgrade/avalonia-ui` intends to migrate this to cross-platform Avalonia; the core/console/tests projects are plain `net9.0` and build anywhere.

## Project layout

- **CPU8086** — the core library (`Final.CPU8086`). No UI dependencies. This is where almost all logic lives.
- **CPU8086.Console** — CLI front end for decoding/running listings (`Final.CPU8086`, despite the namespace overlap).
- **CPU8086.GUI** — WPF visualizer. MVVM (`MainViewModel`), custom `BinaryGridView`, an "auto service" behavior pattern (`IAutoService` / `AttachServiceBehavior`) wiring view-layer services to view models.
- **CPU8086.Resources** — embeds the course's `performance_aware` listings and `x80186` test binaries as assembly resources; `InstructionStreamResources` enumerates and loads them by name. Tests and the GUI read inputs from here, not the filesystem.
- **CPU8086.Tests** — MSTest. Decode tests reassemble each embedded listing and compare operand-by-operand against the reference `.asm`.
- **InstructionTableParser** (`Final.ITP`) — a *code generator*, not part of the runtime. It parses `x86asmref.htm` and emits the giant `InstructionTable.Load()` method. Behavior is toggled by `#define`s at the top of `Program.cs` (`GENERATE_CS`, `EXPORT_TO_CSV`, `GENERATE_INSTRUCTION_CLASSES`). Do not hand-edit the generated table by preference — regenerate it here.

## Core architecture (read these to understand the big picture)

The flow is **bytes → decode → `Instruction` → execute → state changes**.

- **`CPU.cs`** (~1800 lines) is the hub. It owns `MemoryState`, `RegisterState`, the `InstructionTable`, and an `InstructionExecuter`. Key entry points:
  - `DecodeNext` / `TryDecodeNext` — decode one instruction from a `ReadOnlySpan<byte>` at a position.
  - `GetAssembly` / `GetAssemblyLines` — disassemble a whole stream to text.
  - `LoadProgram`, `BeginStepping` / `Step` / `Run` / `StopStepping` — execution lifecycle.
  - `LoadRegister`/`StoreRegister`, `LoadMemory`/`StoreMemory`, `GetAbsoluteMemoryAddress`, `GetSegmentAddress` — the register/memory model. Memory is the full 1 MB with a fixed segmented layout (Data/Code/Stack/Extra at hardcoded base addresses — see the constants at the top of `CPU.cs`).
  - `ComputeCycles` — approximate (not cycle-exact) cycle counting including EA and transfer penalties, driven by `CyclesTable`.

- **Decoding tables** live in `Instructions/`. `InstructionTable` is a 256-entry array indexed by the first opcode byte, populated by the generated `Load()`. Each entry is an `InstructionDefinition` carrying mnemonic, data width, flags, the affected-flags string (e.g. `"o---szap-"`), platform, byte length, and arrays of `FieldDefinition` (how to pull mod/reg/rm/displacement/immediate bytes) and `OperandDefinition` (operand shapes like `"(word)rmw"`, `"(byte)ib"`). The decoder walks these definitions to build a concrete `Instruction` with resolved `InstructionOperand`s.

- **Execution** lives in `Execution/`. `InstructionExecuter` builds a dispatch table indexed by `InstructionType` in its constructor — only the wired-up types (`MOV`, `ADD`, `SUB`, `CMP`, the conditional/direct jumps, `LOOP*`, `PUSH/POP`, `PUSHF/POPF`) are executable; everything else decodes but won't run. To add execution for an instruction, register a handler in that constructor table. Execution produces `ExecutedInstruction` / `ExecutedChange` records (against `IRunState`/`RunState`) describing register/memory/flag mutations, which the GUI consumes for visualization.

- **Error handling**: most fallible methods return `OneOf<T, Error>` (the OneOf library) rather than throwing. Match on the result; don't assume success.

- **Types/** holds the value model: `Register`/`RegisterType`/`REGMappingTable`, `Mnemonic`, `Immediate`, `DataType`/`DataWidth`, `EffectiveAddressCalculation`, `MemoryAddress`, `SegmentType`, `ModType`, `Platform`, `AssemblyLine`, `OutputValueMode` (controls hex/decimal operand formatting).

## Conventions

- Every project uses `RootNamespace` `Final.CPU8086` (except ITP → `Final.ITP`), even the console app — namespaces don't track folder/project names.
- `Documentation/` contains the Intel manuals and timing PDFs the tables are derived from; consult them for opcode/flag/cycle questions.
- `clear.bat` is a Windows-only scratch cleaner (deletes `.vs`, build dirs, etc.).
