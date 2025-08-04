# Getting Started

This guide will help you set up HotPreview in your .NET application and create your first previews.

## Installation

### Prerequisites

- .NET 9.0.300 SDK or later (see `global.json` in the repository root)
- A supported .NET UI platform (currently .NET MAUI, with WPF support coming soon)

### Step 1: Install HotPreview DevTools

Install the global DevTools application:

```bash
dotnet tool install -g HotPreview.DevTools
```

### Step 2: Add Package Reference

Add the HotPreview package to your application project. We recommend only including it in Debug builds:

```xml
<PackageReference Condition="$(Configuration) == 'Debug'" Include="HotPreview.App.Maui" Version="..." />
```

### Step 3: Build and Run

Build your application in Debug mode and run it:

```bash
dotnet build
# Run your application
```

When you build and run your app:
- The DevTools application launches automatically (if not already running)
- Your app connects to DevTools when it starts
- DevTools displays a tree of your UI components and previews

## Auto-Generated Previews

HotPreview automatically creates previews for UI components that meet these criteria:

### Pages
- Derives (directly or indirectly) from `Microsoft.Maui.Controls.Page`
- Has a parameterless constructor OR constructor parameters that can be resolved via dependency injection

### Controls
- Derives from `Microsoft.Maui.Controls.View` (but is not a page)
- Has a parameterless constructor OR constructor parameters that can be resolved via dependency injection

## Creating Custom Previews

Custom previews give you full control over how your components are displayed. They allow you to:

- Support components with complex constructor requirements
- Provide realistic sample data
- Create multiple previews for different states
- Configure global app state for specific scenarios

### Basic Preview

Add a static method with the `[Preview]` attribute to your UI component class:

```csharp
#if PREVIEWS
    [Preview]
    public static ConfirmAddressView Preview() => new(PreviewData.GetPreviewProducts(1), 
        new DeliveryTypeModel(),
        new AddressModel()
        {
            StreetOne = "21, Alex Davidson Avenue",
            StreetTwo = "Opposite Omegatron, Vicent Quarters",
            City = "Victoria Island",
            State = "Lagos State"
        });
#endif
```

### Multiple Previews

Create multiple previews to show different states:

```csharp
#if PREVIEWS
    [Preview("Empty State")]
    public static CardView NoCards() => new(PreviewData.GetPreviewCards(0));

    [Preview("Single Card")]
    public static CardView SingleCard() => new(PreviewData.GetPreviewCards(1));

    [Preview("Multiple Cards")]
    public static CardView SixCards() => new(PreviewData.GetPreviewCards(6));
#endif
```

### Preview Guidelines

1. **Use conditional compilation**: Wrap preview code in `#if PREVIEWS` to exclude it from release builds
2. **Provide meaningful names**: Use descriptive names for multiple previews
3. **Use sample data**: Create realistic test data to showcase your components
4. **Location flexibility**: Preview methods can be in any class, but by convention are placed in the component class

## Navigation and Testing

Once your app is running with DevTools:

1. **Browse Components**: Use the DevTools tree view to explore your UI components
2. **Navigate Instantly**: Click any component or preview to navigate directly to it in your app
3. **Test States**: Use multiple previews to quickly test different data states
4. **Cross-Platform**: Run your app on different platforms and compare side-by-side

## Best Practices

- **Conditional Builds**: Always use `#if PREVIEWS` for preview code
- **Sample Data**: Create dedicated preview data classes for consistent testing
- **Descriptive Names**: Use clear, descriptive names for multiple previews
- **Edge Cases**: Create previews for empty states, error conditions, and loading states
- **Component Isolation**: Ensure previews work independently of app navigation state

## Next Steps

- [Learn about all available attributes](attributes.md)
- [Explore advanced features](features.md)
- [Check out the API reference](../api/)
