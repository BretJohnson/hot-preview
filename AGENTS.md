# AI Agent Guidelines

This file provides guidance to AI agents and LLMs when working with code in this repository to ensure high-quality, consistent contributions.

## Project Overview

### Description

**HotPreview** is a cross-platform UI component preview system for .NET, similar to Storybook for JavaScript or SwiftUI Previews. It enables developers to work on pages and controls in isolation, making UI development and testing faster and more efficient.

### Repository Architecture

- `HotPreview` - Core library with `[Preview]`, `[UIComponent]` attributes and base types
- `HotPreview.SharedModel` - Protocol definitions, reflection utilities, and app services
- `HotPreview.App.Maui` - MAUI platform implementation with preview navigation and rendering
- `HotPreview.DevToolsApp` - WinUI3 desktop application for visual component management
- `HotPreview.DevTools` - Global tool launcher for DevTools
- `HotPreview.Tooling` - Tooling infrastructure, Roslyn-based component discovery, and MCP server for AI-assisted development workflows (includes McpServer module)
- `samples` - Sample applications demonstrating HotPreview usage
- `platforms` - Platform-specific implementations (e.g., MAUI, WPF)

## Code Style Guidelines

### C# Conventions

#### Formatting & Structure
- **Indentation**: 4 spaces for C# code, 2 spaces for XML/JSON/XAML
- **Braces**: Allman style (opening brace on new line), always use braces for all code blocks (if, for, while, etc.)
- **Line endings**: CRLF on Windows, LF on other platforms
- **Nullable reference types**: Enabled by default (`<Nullable>enable</Nullable>`)
- **Implicit usings**: Enabled by default (except where explicitly disabled)

#### Naming Conventions
- **Classes/Methods/Properties**: PascalCase
- **Fields**: camelCase with underscore prefix for private fields (`_fieldName`)
- **Parameters/Local variables**: camelCase
- **Constants**: PascalCase
- **Interfaces**: PascalCase with 'I' prefix (`IServiceName`)
- **Generic type parameters**: Single uppercase letter (`T`, `TKey`, `TValue`)
- **Abbreviations**: Treat "UI" as a word (e.g., `UIComponent`, not `UiComponent`)

#### Code Organization
- **Using directives**: Outside namespace, System directives first
- **File structure**: One primary type per file
- **Namespace**: Match folder structure
- **Access modifiers**: Always explicit, prefer most restrictive appropriate level
- **Control flow**: Always use braces, even for single-line statements
- **Variable declarations**: Avoid `var` unless the type is obvious from the right-hand side (e.g., `new SomeType()`, LINQ queries with obvious types)

```csharp
// Preferred - always use braces
if (condition)
{
    DoSomething();
}

// Avoid - single-line without braces
if (condition)
    DoSomething();
```

## Testing Guidelines

### Test Structure
- **Framework**: MSTest
- **Mocking**: Use Moq for dependency mocking
- **Naming**: Descriptive test method names following `MethodName_Scenario_ExpectedResult` pattern
- **Organization**: Group related tests in test classes, use `[TestInitialize]` for setup

### Mocking Best Practices
- Mock external dependencies and I/O operations
- Prefer testing success cases with proper mocks over only testing failure cases
- Use `Times.Once`, `Times.Never` etc. to verify method calls
- Setup mocks to return realistic data that matches production scenarios

### Testing
- Write tests for new functionality using established patterns
- Mock external dependencies appropriately
- Include both positive and negative test cases
- Use descriptive test names and clear arrange/act/assert structure

## Development Guidelines

### Refactoring
- There's no need for backward compatibility yet. Prefer simpler new code instead.
- Update related tests when modifying code
- Follow the existing architectural patterns
- Consider cross-platform compatibility

### Documentation
- Update relevant documentation when making changes
- Include code examples in documentation
- Use clear, concise language
- Follow the established documentation structure
