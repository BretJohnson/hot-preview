# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build Commands

- **Build solution**: `dotnet build` or `dotnet build HotPreview.sln`
- **Run tests**: `dotnet test`
- **Pack NuGet packages**: `dotnet pack` (outputs to `bin/Packages/`)
- **Install DevTools globally**: `dotnet tool install -g HotPreview.DevTools`
- **Launch DevTools**: `preview-devtools` (after installing globally)
- **Documentation**: `cd docfx && dotnet docfx --serve` (builds and serves docs locally)

## Development Setup

- Initialize dependencies: `./init.ps1` (PowerShell required)
- Requires .NET 9.0.300 SDK (see `global.json`)
- Strong-name signing enabled (`strongname.snk`)
- All projects use nullable reference types and implicit usings (except where disabled)

## Repository Architecture

### Core Components

**HotPreview Framework** - A cross-platform UI component preview system for .NET, similar to Storybook for JavaScript or SwiftUI Previews. The architecture consists of:

1. **Core Library** (`src/HotPreview/`) - Base attributes and types for defining previews
2. **Shared Model** (`src/HotPreview.SharedModel/`) - Cross-platform protocol and reflection utilities for preview discovery
3. **Platform Apps** (`src/platforms/`) - Platform-specific preview applications (MAUI, WPF)
4. **DevTools** (`src/tooling/`) - Visual development environment and tooling infrastructure

### Key Projects

- `HotPreview` - Core library with `[Preview]`, `[UIComponent]` attributes and base types
- `HotPreview.SharedModel` - Protocol definitions, reflection utilities, and app services
- `HotPreview.App.Maui` - MAUI platform implementation with preview navigation and rendering
- `HotPreview.DevToolsApp` - WinUI3 desktop application for visual component management
- `HotPreview.DevTools` - Global tool launcher for DevTools
- `HotPreview.Tooling` - Tooling infrastructure and Roslyn-based component discovery
- `HotPreview.MCPServer` - MCP server for AI-assisted development workflows

### Preview System

Previews are defined using `[Preview]` attributes on static methods that return UI component instances:

```csharp
#if PREVIEWS
[Preview]
public static MyComponent Preview() => new MyComponent();
#endif
```

The framework auto-discovers components and previews through:
- Reflection-based discovery at runtime
- Roslyn-based analysis in tooling
- JSON-RPC protocol for tool communication

### Build Integration

- MSBuild tasks in `HotPreview.AppBuildTasks` generate app settings and launch DevTools
- `PREVIEWS` conditional compilation symbol enabled in Debug builds
- NuGet packages include MSBuild targets for seamless integration

### Testing

- Unit tests in `test/` directory using xUnit
- Sample applications in `samples/maui/` for integration testing
- Visual regression testing utilities in `HotPreview.Tooling/VisualTestUtils/`

## Key Patterns

- Component discovery through reflection and Roslyn analysis
- JSON-RPC for tool-to-app communication (StreamJsonRpc)
- Platform-agnostic core with pluggable platform implementations
- MSBuild integration for automatic tooling launch
- Conditional compilation for preview code isolation