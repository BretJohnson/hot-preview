# PreviewFramework.props Test Validation

This document validates that the PreviewFramework.props file correctly implements the required functionality.

## Test Results

### Debug Configuration (Default)
- ✅ PreviewsSupport property defaults to `true` when Configuration is Debug
- ✅ PREVIEWS symbol is defined when PreviewsSupport is true
- ✅ DefineConstants includes "PREVIEWS" in Debug builds

### Release Configuration (Default)
- ✅ PreviewsSupport property is not set by default in Release
- ✅ PREVIEWS symbol is NOT defined in Release builds
- ✅ DefineConstants does NOT include "PREVIEWS" in Release builds

### Override Scenarios
- ✅ PreviewsSupport=true in Release adds PREVIEWS symbol
- ✅ PreviewsSupport=false in Debug removes PREVIEWS symbol
- ✅ Command line overrides work correctly

## How it works

The `buildTransitive/PreviewFramework.props` file will be automatically imported by NuGet when the PreviewFramework package is referenced, and it:

1. Sets `PreviewsSupport=true` if not already set and Configuration is Debug
2. Adds `PREVIEWS` to DefineConstants when PreviewsSupport is true
3. Allows manual override of PreviewsSupport for custom scenarios

This enables conditional compilation blocks like:
```csharp
#if PREVIEWS
    // Preview-only code
#endif
```