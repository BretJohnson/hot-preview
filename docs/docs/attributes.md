# Hot Preview Attributes Reference

Hot  Preview provides several attributes to control how your UI components are discovered, displayed, and organized in the DevTools interface. This reference covers all available attributes and their usage.

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
[Preview]                             // Typical preview - default display name, automatic UI component type inference
[Preview("Display Name")]             // Overridden display name, with automatic UI component type inference
[Preview<MyComponent>]                // Explicitly specified UI component type
[Preview<MyComponent>("Display Name")]  // Overridden display name, with explicit UI component type
```

#### Usage Examples

```csharp
#if PREVIEWS
    // Basic preview with automatic type inference
    [Preview]
    public static CardView Preview() => new(PreviewData.GetCards(3));

    // Named previews with automatic type inference
    [Preview("0 cards")]
    public static CardView NoCards() => new(PreviewData.GetCards(0));

    [Preview("1 Card")]
    public static CardView SingleCard() => new(PreviewData.GetCards(1));

    [Preview("6 Cards")]
    public static CardView MultipleCards() => new(PreviewData.GetCards(6));

    // void method - UI component type inferred from containing class
    public static void AppNeedsUpdateState()
    {
    }

    // Explicit UI component type specification using generic attribute
    [Preview<ProductView>]
    public static CustomProductLayoutView CustomProductLayout() => new(PreviewData.GetProduct());
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
// Explicit display name for UI component
[UIComponent("Shopping Cart")]
public partial class CartView : ContentView
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
    [PreviewCommand]
    public static void ClearCache()
    {
        // Clear application cache
    }

    [PreviewCommand("Reset Application State"]
    public static void ResetAppState()
    {
        // Reset global state for testing
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
It's appropriate to use if the auto-generated preview doesn't work properly, so you don't want to see it in the UI, and you don't want to define a explicit preview for the component.

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
    // This component won't get an auto-generated preview, even if it has a parameterless constructor
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
