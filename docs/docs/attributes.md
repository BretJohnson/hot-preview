# HotPreview Attributes Reference

HotPreview provides several attributes to control how your UI components are discovered, displayed, and organized in the DevTools interface. This reference covers all available attributes and their usage.

## Core Attributes

### PreviewAttribute

The `[Preview]` attribute is the primary way to define custom previews for your UI components.

**Target:** Methods and Classes
**Namespace:** `HotPreview`

#### Overview

The `PreviewAttribute` comes in two forms:
- **Non-generic**: `PreviewAttribute` - Automatically infers UI component type
- **Generic**: `PreviewAttribute<TUIComponent>` - Explicitly specifies UI component type

#### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `displayName` | `string?` | Optional display name override for the preview, determining how it appears in navigation UI. If not specified, uses method/class name converted to start case (e.g., "MyPreview" becomes "My Preview") |

#### Type Inference

- **Method return type**: UI component type is inferred from the method's return type
- **Void methods**: UI component type is inferred from the containing class
- **Explicit specification**: Use `PreviewAttribute<TUIComponent>` when you need to specify a different UI component type

#### Constructors

```csharp
[Preview()]                           // Basic preview with automatic type inference
[Preview("Display Name")]             // Named preview with automatic type inference
[Preview<MyComponent>()]              // Explicit component type
[Preview<MyComponent>("Display Name")] // Named with explicit type
```

#### Usage Examples

```csharp
#if PREVIEWS
    // Basic preview with automatic type inference
    [Preview]
    public static CardView Preview() => new(PreviewData.GetCards(3));

    // Named previews with automatic type inference
    [Preview("Empty State")]
    public static CardView NoCards() => new(PreviewData.GetCards(0));

    [Preview("Cards/Single Card")]
    public static CardView SingleCard() => new(PreviewData.GetCards(1));

    [Preview("Cards/Multiple Cards")]
    public static CardView MultipleCards() => new(PreviewData.GetCards(6));

    // Void method - type inferred from containing class
    [Preview("Navigate to Product")]
    public static void NavigateToProduct()
    {
        // Navigation logic - type inferred from containing class
    }

    // Explicit type specification using generic attribute
    [Preview<ProductView>("Custom Layout")]
    public static ProductView CustomProductLayout() => new(PreviewData.GetProduct());

    // Generic attribute when method is in different class
    [Preview<CartView>("Shopping Cart Preview")]
    public static CartView CreateCartPreview() => new(PreviewData.GetCart());
#endif
```

### UIComponentAttribute

The `[UIComponent]` attribute allows you to explicitly mark classes as UI components and provide custom display names.

**Target:** Classes
**Namespace:** `HotPreview`

#### Overview

Normally UI components don't need to be defined explicitly (defining a preview is sufficient), but this attribute can be used to define a display name for the component.

#### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `displayName` | `string?` | Optional display name override for the UI component. If not specified, the class name is used (without namespace) |

#### Usage Examples

```csharp
[UIComponent("Shopping Cart")]
public partial class CartView : ContentView
{
    // Component implementation
}

[UIComponent] // Uses class name as display name
public partial class ProductCard : ContentView
{
    // Component implementation
}
```

### PreviewCommandAttribute

The `[PreviewCommand]` attribute defines commands that can be executed from the DevTools interface.

**Target:** Methods
**Namespace:** `HotPreview`

#### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `displayName` | `string?` | Optional display name override for the command, determining how it appears in navigation UI. If not specified, the method name is used |

#### Usage Examples

```csharp
#if PREVIEWS
    [PreviewCommand("Reset App State")]
    public static void ResetAppState()
    {
        // Reset global state for testing
        App.Current.MainPage = new AppShell();
    }

    [PreviewCommand("Data/Load Sample Data")]
    public static void LoadSampleData()
    {
        // Load test data
        DataService.LoadSampleData();
    }

    [PreviewCommand] // Uses method name as display name
    public static void ClearCache()
    {
        // Clear application cache
    }
#endif
```

## Configuration Attributes

### AutoGeneratePreviewAttribute

Controls whether auto-generated previews should be created for a UI component.

**Target:** Classes
**Namespace:** `HotPreview`

#### Overview

When present on a class and the `autoGenerate` property is `false`, auto-generation is disabled. This attribute provides explicit control over the auto-generation behavior for UI components.

#### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `autoGenerate` | `bool` | Controls whether auto-generated previews should be created for this component. When set to `false`, auto-generation is disabled |

#### Usage Examples

```csharp
// Disable auto-generated previews for this component
[AutoGeneratePreview(false)]
public partial class ComplexView : ContentView
{
    // This component won't get auto-generated previews
    // You must define custom [Preview] methods
}

// Explicitly enable auto-generation
[AutoGeneratePreview(true)]
public partial class SimpleView : ContentView
{
    // Auto-generated previews will be created
}
```

## Assembly-Level Attributes

### UIComponentCategoryAttribute

Defines categories for organizing UI components in the DevTools interface.

**Target:** Assembly
**Namespace:** `HotPreview`

#### Overview

Categories are used for display purposes only. If no category is specified for a component, the category name defaults to "Pages" or "Controls", depending on whether the UI component is a page or not. This attribute can be specified multiple times for a single category, in which case the UI components are combined together.

#### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `name` | `string` | The name of the category |
| `uiComponents` | `Type[]` | The UI components that belong to this category |

#### Usage Examples

```csharp
// In AssemblyInfo.cs or any source file
using HotPreview;

[assembly: UIComponentCategory("Navigation", typeof(HeaderView), typeof(FooterView), typeof(TabBar))]
[assembly: UIComponentCategory("Cards", typeof(ProductCard), typeof(CategoryCard), typeof(InfoCard))]
[assembly: UIComponentCategory("Forms", typeof(LoginForm), typeof(RegisterForm), typeof(ContactForm))]

// Multiple attributes for the same category combine the components
[assembly: UIComponentCategory("Data Display", typeof(ProductCard), typeof(UserProfile))]
[assembly: UIComponentCategory("Data Display", typeof(StatisticsView), typeof(ChartView))]
```

### ControlUIComponentBaseTypeAttribute

Specifies the base type for control UI components on a specific platform.

**Target:** Assembly
**Namespace:** `HotPreview`

#### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `platform` | `string` | Platform identifier (e.g., "MAUI", "WPF") |
| `baseType` | `string` | Fully qualified base type name |

#### Usage Examples

```csharp
// In AssemblyInfo.cs
[assembly: ControlUIComponentBaseType("MAUI", "Microsoft.Maui.Controls.View")]
[assembly: ControlUIComponentBaseType("WPF", "System.Windows.Controls.Control")]
```

### PageUIComponentBaseTypeAttribute

Specifies the base type for page UI components on a specific platform.

**Target:** Assembly
**Namespace:** `HotPreview`

#### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `platform` | `string` | Platform identifier (e.g., "MAUI", "WPF") |
| `baseType` | `string` | Fully qualified base type name |

#### Usage Examples

```csharp
// In AssemblyInfo.cs
[assembly: PageUIComponentBaseType("MAUI", "Microsoft.Maui.Controls.Page")]
[assembly: PageUIComponentBaseType("WPF", "System.Windows.Window")]
```

## Recent Improvements

### Enhanced Type Inference

The `PreviewAttribute` now provides improved type inference capabilities:

- **Automatic inference**: UI component types are automatically inferred from method return types
- **Void method support**: For void methods, the type is inferred from the containing class
- **Generic attribute**: Use `PreviewAttribute<TUIComponent>` for explicit type specification
- **Flexible usage**: Works with both static methods and class-based previews

### Simplified Auto-Generation Control

Auto-generation control has been streamlined:

- **Dedicated attribute**: `AutoGeneratePreviewAttribute` provides explicit control over auto-generation
- **Clear semantics**: `false` disables auto-generation, `true` enables it
- **Separation of concerns**: Auto-generation control is separate from UI component definition

### Improved Documentation Standards

All attributes now follow consistent documentation standards:

- **Comprehensive parameter documentation**: All parameters include detailed descriptions
- **Clear usage examples**: Examples demonstrate real-world usage patterns
- **Consistent formatting**: XML documentation follows project standards for clarity

## Best Practices

### Naming Conventions

- Use descriptive names that clearly indicate the preview state or variant
- Use "/" delimiters to create hierarchical organization in DevTools
- Keep names concise but meaningful
- Follow start case convention (e.g., "My Preview" instead of "myPreview")

### Preview Organization

```csharp
#if PREVIEWS
    // Group related previews using hierarchy
    [Preview("States/Loading")]
    public static ProductView LoadingState() => new(isLoading: true);

    [Preview("States/Error")]
    public static ProductView ErrorState() => new(hasError: true);

    [Preview("States/Empty")]
    public static ProductView EmptyState() => new(isEmpty: true);

    // Organize by data variations
    [Preview("Data/Single Item")]
    public static CartView SingleItem() => new(PreviewData.GetCart(1));

    [Preview("Data/Multiple Items")]
    public static CartView MultipleItems() => new(PreviewData.GetCart(5));

    // Use generic attribute when type needs explicit specification
    [Preview<ProductView>("Cross-Component/Product in Cart Context")]
    public static CartView ProductInCartContext() => new(PreviewData.GetCartWithProduct());

    // Void methods with type inference from containing class
    [Preview("Navigation/Navigate to Details")]
    public static void NavigateToDetails()
    {
        // Navigation logic - type inferred from containing class
    }
#endif
```

### Conditional Compilation

Always wrap preview code in conditional compilation directives:

```csharp
#if PREVIEWS
    [Preview]
    public static MyView Preview() => new(PreviewData.GetSampleData());

    [PreviewCommand("Reset State")]
    public static void ResetState() => AppState.Reset();
#endif
```

### Assembly Configuration

Set up assembly-level attributes to customize component discovery:

```csharp
// Configure platform-specific base types
[assembly: ControlUIComponentBaseType("MAUI", "Microsoft.Maui.Controls.View")]
[assembly: PageUIComponentBaseType("MAUI", "Microsoft.Maui.Controls.Page")]

// Organize components into logical categories
[assembly: UIComponentCategory("Layout", typeof(HeaderView), typeof(SidebarView))]
[assembly: UIComponentCategory("Data Display", typeof(ProductCard), typeof(UserProfile))]
```

## Troubleshooting

### Common Issues

- **Previews not appearing**: Ensure you're building in Debug mode and the `PREVIEWS` symbol is defined
- **Categories not working**: Check that assembly-level attributes are properly declared in `AssemblyInfo.cs` or source files
- **Auto-generation issues**: Use `[AutoGeneratePreview(false)]` to disable automatic preview creation for complex components
- **Type inference problems**: Use the generic `PreviewAttribute<TUIComponent>` when automatic type inference doesn't work as expected
- **Void method previews**: Ensure void preview methods are in the correct class context for proper type inference

### Migration Notes

If you're updating from older versions:

- **Generic syntax**: Use `[Preview<MyComponent>()]` instead of `[Preview(typeof(MyComponent))]` for explicit type specification
- **Auto-generation control**: Use `[AutoGeneratePreview(false)]` instead of the removed `UIComponentAttribute.AutoGeneratePreview` property
- **Documentation**: All attributes now have comprehensive XML documentation for better IntelliSense support
