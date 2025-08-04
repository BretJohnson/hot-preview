# HotPreview Attributes Reference

HotPreview provides several attributes to control how your UI components are discovered, displayed, and organized in the DevTools interface. This reference covers all available attributes and their usage.

## Core Attributes

### PreviewAttribute

The `[Preview]` attribute is the primary way to define custom previews for your UI components.

**Target:** Methods and Classes  
**Namespace:** `HotPreview`

#### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `displayName` | `string?` | Optional display name for the preview. Use "/" delimiters for hierarchy (e.g., "Cards/Empty State") |
| `uiComponent` | `Type?` | Optional UI component type this preview represents |

#### Constructors

```csharp
[Preview()]                                    // Basic preview
[Preview("Display Name")]                      // Named preview
[Preview(typeof(MyComponent))]                 // Explicit component type
[Preview("Display Name", typeof(MyComponent))] // Named with explicit type
```

#### Usage Examples

```csharp
#if PREVIEWS
    // Basic preview
    [Preview]
    public static CardView Preview() => new(PreviewData.GetCards(3));

    // Multiple named previews
    [Preview("Empty State")]
    public static CardView NoCards() => new(PreviewData.GetCards(0));

    [Preview("Cards/Single Card")]
    public static CardView SingleCard() => new(PreviewData.GetCards(1));

    [Preview("Cards/Multiple Cards")]
    public static CardView MultipleCards() => new(PreviewData.GetCards(6));

    // Preview in different class with explicit type
    [Preview("Custom Layout", typeof(ProductView))]
    public static ProductView CustomProductLayout() => new(PreviewData.GetProduct());
#endif
```

### UIComponentAttribute

The `[UIComponent]` attribute allows you to explicitly mark classes as UI components and provide custom display names.

**Target:** Classes  
**Namespace:** `HotPreview`

#### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `displayName` | `string?` | Optional custom display name for the UI component |

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
| `displayName` | `string?` | Optional display name for the command. Use "/" delimiters for hierarchy |

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
#endif
```

## Configuration Attributes

### AutoGeneratePreviewAttribute

Controls whether auto-generated previews should be created for a UI component.

**Target:** Classes  
**Namespace:** `HotPreview`

#### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `autoGenerate` | `bool` | `true` to enable auto-generation, `false` to disable |

#### Usage Examples

```csharp
// Disable auto-generated previews for this component
[AutoGeneratePreview(false)]
public partial class ComplexView : ContentView
{
    // This component won't get auto-generated previews
    // You must define custom [Preview] methods
}

// Explicitly enable (though this is the default)
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

#### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `name` | `string` | Category name |
| `uiComponents` | `Type[]` | Array of UI component types in this category |

#### Usage Examples

```csharp
// In AssemblyInfo.cs or any source file
using HotPreview;

[assembly: UIComponentCategory("Navigation", typeof(HeaderView), typeof(FooterView), typeof(TabBar))]
[assembly: UIComponentCategory("Cards", typeof(ProductCard), typeof(CategoryCard), typeof(InfoCard))]
[assembly: UIComponentCategory("Forms", typeof(LoginForm), typeof(RegisterForm), typeof(ContactForm))]
```

### ControlUIComponentBaseTypeAttribute

Defines platform-specific base types for control components.

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

Defines platform-specific base types for page components.

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

## Best Practices

### Naming Conventions

- Use descriptive names that clearly indicate the preview state or variant
- Use "/" delimiters to create hierarchical organization in DevTools
- Keep names concise but meaningful

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

- **Previews not appearing**: Ensure you're building in Debug mode and the `PREVIEWS` symbol is defined
- **Categories not working**: Check that assembly-level attributes are properly declared
- **Auto-generation issues**: Use `[AutoGeneratePreview(false)]` to disable automatic preview creation for complex components