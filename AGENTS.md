# AGENTS.md — dstream-console-output-provider

> .NET console output provider. Part of the [DStream ecosystem](https://github.com/katasec/dstream-mission-control).

## Role

`dstream-console-output-provider` is a **DStream output provider** built with the DStream .NET SDK. It receives JSON envelopes from the DStream orchestrator via stdin and displays them in human-readable formats (`simple`, `structured`, `json`). Distributed as an OCI artifact.

## Design Docs

- Architecture & protocol: [dstream/docs/design/design.md](https://github.com/katasec/dstream/blob/main/docs/design/design.md)
- .NET SDK reference: [dstream-dotnet-sdk](https://github.com/katasec/dstream-dotnet-sdk)
- Ecosystem inventory: [dstream-mission-control/docs/repository-inventory.md](https://github.com/katasec/dstream-mission-control/blob/main/docs/repository-inventory.md)

## Code Style (C#)

**Governing principle: Progressive Disclosure.** Code reveals intent in layers — what at the top, how one level deeper.

- **Outline-first**: The top 15–20 lines of any file or class must disclose intent and flow. If a reader must scroll to understand what a class does, refactor.
- **Small, composable methods**: Each method is one named step (~20–40 lines max). Callers read step names; drilling in reveals implementation.
- **Top-down method ordering**: Public entry points first, private helpers below. File reads like an outline.
- **Error handling**: Handle exceptions explicitly. No empty `catch { }`. Log at the boundary or propagate with context.
- **No deeply nested branching**: Max 2 levels. Use early returns (`if (x) return;`).
- **Side effects isolated**: File, network, console output — in clearly named methods, not mixed with logic.
- **Zero warnings**: Build must pass with zero warnings (`dotnet build` with no warnings). Treat warnings as errors.
- **Prefer explicit block syntax** over expression-bodied (`=>`) for multi-line method bodies.
- **No speculative abstractions**: Build for what the task requires.

## Behaviour Rules

- **Propose before editing**: Describe what you're about to change and why before modifying files.
- **Test every change**: Run `dotnet test` before considering work done.
- **Build before push**: `dotnet build` must succeed with zero errors and zero warnings.
- **Focus**: Only change what the task requires. Do not refactor surrounding code.
- **OCI packaging**: Changes to output formatting or behaviour must be reflected in the OCI artifact version bump.

## Provider Contract

- Reads JSON envelopes from stdin — one per line
- Renders to console (stdout) in the configured format
- Logs diagnostics to stderr
- Configured via the DStream .NET SDK command envelope on startup
- Must handle graceful shutdown via CancellationToken

## Task Context

Tasks arrive via GitHub Actions `workflow_dispatch` from `katasec/dstream-mission-control`. The issue body is your primary context. Read `## Task`, `## Context`, and `## Acceptance criteria` carefully before writing any code.
