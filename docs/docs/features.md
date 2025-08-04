# Features

HotPreview provides a comprehensive set of features for efficient UI development and testing in .NET applications.

## Core Features

### 🚀 Streamlined Navigation

Jump directly to specific UI pages and components without navigating through multiple app screens.

**Benefits:**
- Dramatically reduces development time
- Eliminates repetitive manual testing workflows
- Enables instant access to any component state

**How it works:**
- DevTools displays a hierarchical tree of all UI components
- Click any component to navigate directly to it in your running app
- No need to manually navigate through complex app flows

### 🔄 Multi-state Testing

Quickly visualize UI components with different data inputs and states, ensuring responsive and robust interfaces across all scenarios.

**Supported scenarios:**
- Empty states
- Loading states  
- Error conditions
- Different data volumes (single item, multiple items, large datasets)
- Various user permissions or app states

**Example:**
```csharp
#if PREVIEWS
    [Preview("Empty Cart")]
    public static CartView EmptyCart() => new(PreviewData.GetCart(0));

    [Preview("Single Item")]
    public static CartView SingleItem() => new(PreviewData.GetCart(1));

    [Preview("Full Cart")]
    public static CartView FullCart() => new(PreviewData.GetCart(10));

    [Preview("Cart with Error")]
    public static CartView ErrorCart() => new(PreviewData.GetCartWithError());
#endif
```

### 📱 Cross-Platform Visualization

View your UI on multiple platforms simultaneously, enabling instant cross-platform comparison and consistency validation.

**Capabilities:**
- Run the same app on Windows, Android, iOS side-by-side
- Navigate to the same component across platforms instantly
- Compare visual consistency and behavior
- Test platform-specific adaptations

### 🛠️ DevTools Integration

A powerful desktop application that serves as your command center for UI development.

**DevTools features:**
- **Component Tree**: Hierarchical view of all UI components
- **Live Navigation**: Click-to-navigate functionality
- **Command Execution**: Run preview commands for state management
- **Multi-App Support**: Connect multiple app instances simultaneously
- **Auto-Discovery**: Automatic detection of components and previews

## Auto-Generation Features

### Intelligent Component Discovery

HotPreview automatically discovers and creates previews for UI components without any configuration.

**Auto-generated for:**
- **Pages**: Any class inheriting from platform page base types
- **Controls**: Any class inheriting from platform view base types
- **Dependency Injection**: Components with constructor parameters resolved via DI

**Customizable discovery:**
- Configure platform-specific base types
- Control auto-generation per component
- Organize components into categories

### Zero-Configuration Setup

Get started immediately without complex configuration:

1. Install DevTools globally
2. Add package reference to your app
3. Build and run in Debug mode

## Advanced Features

### Custom Preview Commands

Define commands that can be executed from DevTools to manipulate app state or perform testing actions.

```csharp
#if PREVIEWS
    [PreviewCommand("Reset User Session")]
    public static void ResetSession()
    {
        UserService.ClearSession();
        App.Current.MainPage = new LoginPage();
    }

    [PreviewCommand("Load Test Data")]
    public static async Task LoadTestData()
    {
        await DataService.LoadSampleDataAsync();
    }
#endif
```

### Hierarchical Organization

Use path-like naming to organize components and previews into logical hierarchies.

```csharp
#if PREVIEWS
    [Preview("Authentication/Login Form")]
    public static LoginView LoginForm() => new();

    [Preview("Authentication/Registration Form")]
    public static RegisterView RegisterForm() => new();

    [Preview("Shop/Product Card/Featured")]
    public static ProductCard FeaturedProduct() => new(PreviewData.GetFeaturedProduct());
#endif
```

### Conditional Compilation

Ensure preview code is completely excluded from release builds.

```csharp
#if PREVIEWS
    // All preview code goes here
    // Excluded from Release builds automatically
#endif
```

## Upcoming Features

### 🤖 AI-Driven Development *(Coming Soon)*

Built-in MCP (Model Context Protocol) server for AI-assisted UI development workflows.

**Planned capabilities:**
- AI agents can generate and execute previews
- Automatic screenshot comparison and feedback
- Visual regression testing with AI analysis
- Intelligent component generation based on visual requirements

### 📊 Visual Testing Utils

Advanced utilities for visual regression testing and comparison.

**Current capabilities:**
- Image snapshot generation
- Visual difference detection
- Automated screenshot comparison
- Integration with testing frameworks

### 🔧 Enhanced DevTools

Continuous improvements to the DevTools experience:
- Performance profiling
- Component dependency visualization  
- Advanced filtering and search
- Custom themes and layouts

## Integration Features

### MSBuild Integration

Seamless integration with your build process:
- Automatic DevTools launch during Debug builds
- App settings generation
- Conditional compilation symbol management

### Platform Support

**Current support:**
- .NET MAUI (Windows, Android, iOS, macOS)

**Planned support:**
- WPF
- WinUI 3
- Uno Platform
- Avalonia UI

### Development Workflow

**IDE Integration:**
- Works with Visual Studio
- Compatible with VS Code
- Command-line friendly

**Testing Integration:**
- xUnit compatibility
- Visual regression testing
- Snapshot testing utilities

## Performance Features

### Lazy Loading

Components and previews are loaded on-demand to maintain performance with large applications.

### Efficient Discovery

Roslyn-based analysis for fast component discovery without runtime overhead.

### Memory Management

Smart memory management to handle multiple app instances and component states efficiently.
