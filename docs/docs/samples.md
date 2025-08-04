# Sample Applications

HotPreview includes several sample applications that demonstrate different features and usage patterns. These samples serve as both learning resources and testing environments for the framework.

## Available Samples

### E-commerce MAUI Application

**Location:** `samples/maui/EcommerceMAUI/`  
**Description:** A complete e-commerce mobile application showcasing complex UI components and previews.

#### Features Demonstrated
- **Product browsing and search**
- **Shopping cart functionality**
- **User authentication flows**
- **Payment and checkout processes**
- **Order tracking**
- **Profile management**

#### Preview Examples

The sample includes extensive preview definitions for various component states:

**Card Component Previews:**
```csharp
#if PREVIEWS
    [Preview("No Cards")]
    public static CardView NoCards() => new(PreviewData.GetPreviewCards(0));

    [Preview("Single Card")]
    public static CardView SingleCard() => new(PreviewData.GetPreviewCards(1));

    [Preview("Two Cards")]
    public static CardView TwoCards() => new(PreviewData.GetPreviewCards(2));

    [Preview("Six Cards")]
    public static CardView SixCards() => new(PreviewData.GetPreviewCards(6));
#endif
```

**Cart Component States:**
```csharp
#if PREVIEWS
    [Preview("Empty Cart")]
    public static CartView EmptyCart() => new(PreviewData.GetEmptyCart());

    [Preview("Single Item Cart")]
    public static CartView SingleItemCart() => new(PreviewData.GetCart(1));

    [Preview("Medium Cart")]
    public static CartView MediumCart() => new(PreviewData.GetCart(3));

    [Preview("Large Cart")]
    public static CartView LargeCart() => new(PreviewData.GetCart(10));
#endif
```

#### Visual Regression Testing

The sample includes automated visual testing with snapshot comparisons:

**Generated Snapshots:**
- `EcommerceMAUI.Views.AddNewCardView.png`
- `EcommerceMAUI.Views.CartView-SingleItemCart.png`
- `EcommerceMAUI.Views.ProductDetailsView.png`
- `EcommerceMAUI.Views.HomePageView.png`

#### Running the Sample

```bash
cd samples/maui/EcommerceMAUI
dotnet build
dotnet run --project EcommerceMAUI.csproj
```

### Default Template with Content

**Location:** `samples/maui/DefaultTemplateWithContent/`  
**Description:** A project management application demonstrating data-driven UI components.

#### Features Demonstrated
- **Project and task management**
- **Category-based organization**
- **Data visualization with charts**
- **CRUD operations**
- **Tag-based filtering**

#### Components Included
- `ProjectCardView` - Project summary cards
- `TaskView` - Individual task components  
- `CategoryChart` - Data visualization
- `TagView` - Tag display components
- `AddButton` - Action components

#### Preview Data Management

The sample shows how to create comprehensive preview data:

```csharp
public static class PreviewData
{
    public static List<Project> GetPreviewProjects(int count)
    {
        return MockData.Projects.Take(count).ToList();
    }

    public static List<ProjectTask> GetPreviewTasks(int count)
    {
        return MockData.Tasks.Take(count).ToList();
    }

    public static CategoryChartData GetPreviewChartData()
    {
        return new CategoryChartData
        {
            Categories = MockData.Categories.Take(5).ToList(),
            Data = GenerateRandomData(5)
        };
    }
}
```

## Common Patterns

### Preview Data Organization

Both samples demonstrate effective preview data management:

**Centralized Data Service:**
```csharp
public static class PreviewData
{
    // Static data for consistent previews
    public static readonly Product FeaturedProduct = new()
    {
        Name = "Premium Headphones",
        Price = 299.99m,
        Description = "High-quality wireless headphones",
        IsAvailable = true
    };

    // Dynamic data generation
    public static List<Product> GetProducts(int count, bool includeFeatured = false)
    {
        var products = MockData.Products.Take(count).ToList();
        if (includeFeatured)
            products.Insert(0, FeaturedProduct);
        return products;
    }
}
```

### State Variation Patterns

**Loading and Error States:**
```csharp
#if PREVIEWS
    [Preview("Normal State")]
    public static ProductView Normal() => new(PreviewData.FeaturedProduct);

    [Preview("Loading State")]
    public static ProductView Loading() => new(isLoading: true);

    [Preview("Error State")]  
    public static ProductView Error() => new(hasError: true, errorMessage: "Failed to load product");

    [Preview("Empty State")]
    public static ProductView Empty() => new(isEmpty: true);
#endif
```

### Hierarchical Organization

**Grouped Previews:**
```csharp
#if PREVIEWS
    // Authentication flows
    [Preview("Auth/Login Form")]
    public static LoginView LoginForm() => new();

    [Preview("Auth/Register Form")]
    public static RegisterView RegisterForm() => new();

    [Preview("Auth/Forgot Password")]
    public static ForgotPasswordView ForgotPassword() => new();

    // Shopping features
    [Preview("Shop/Product List")]
    public static ProductListView ProductList() => new(PreviewData.GetProducts(10));

    [Preview("Shop/Product Details")]
    public static ProductDetailsView ProductDetails() => new(PreviewData.FeaturedProduct);
#endif
```

## Testing Integration

### Visual Regression Testing

Both samples include comprehensive visual testing:

**Test Structure:**
```
samples/maui/EcommerceMAUI/
├── snapshots/           # Reference images
├── test-results/        # Test output
└── VisualTests.cs       # Test definitions
```

**Example Test:**
```csharp
[Fact]
public async Task CartView_SingleItem_MatchesSnapshot()
{
    var component = CartView.SingleItemCart();
    var screenshot = await CaptureScreenshot(component);
    
    await VisualRegressionTester.AssertMatchesSnapshot(
        screenshot, 
        "EcommerceMAUI.Views.CartView-SingleItem.png"
    );
}
```

### Unit Testing

Integration with xUnit for component testing:

```csharp
[Fact]
public void ProductCard_WithValidProduct_DisplaysCorrectly()
{
    // Arrange
    var product = PreviewData.FeaturedProduct;
    
    // Act
    var card = new ProductCard(product);
    
    // Assert
    Assert.Equal(product.Name, card.ProductName.Text);
    Assert.Equal(product.Price.ToString("C"), card.Price.Text);
}
```

## Development Workflow

### Iterative Development

The samples demonstrate an efficient development workflow:

1. **Create Component:** Build basic UI component
2. **Add Preview:** Create initial `[Preview]` method
3. **Test States:** Add previews for different states
4. **Refine UI:** Use DevTools to iterate quickly
5. **Add Tests:** Create visual regression tests
6. **Document:** Update preview names and organization

### Best Practices from Samples

**Naming Conventions:**
- Use descriptive, hierarchical names
- Group related previews with "/" delimiters
- Include state information in names

**Data Management:**
- Centralize preview data in dedicated classes
- Use realistic data that represents actual usage
- Include edge cases and boundary conditions

**Component Design:**
- Design components to be preview-friendly
- Support parameterless constructors where possible
- Use dependency injection for complex dependencies

## Running All Samples

### Build All Samples
```bash
dotnet build samples/HotPreview-Samples.slnf
```

### Run Individual Samples
```bash
# E-commerce sample
cd samples/maui/EcommerceMAUI
dotnet run

# Project management sample  
cd samples/maui/DefaultTemplateWithContent
dotnet run
```

### Visual Testing
```bash
# Run visual regression tests
dotnet test samples/maui/EcommerceMAUI --logger "console;verbosity=detailed"
```

## Learning Resources

### Component Examples

Study the sample components to learn:
- **Effective preview design patterns**
- **State management for previews**
- **Data binding in preview contexts**
- **Platform-specific adaptations**

### Preview Strategies

The samples showcase various preview strategies:
- **Minimal previews** for simple components
- **Comprehensive state coverage** for complex components
- **Data-driven previews** for components with varying content
- **Interactive previews** with command support

### Testing Approaches

Learn from the sample testing strategies:
- **Visual regression testing** for UI consistency
- **Unit testing** for component logic
- **Integration testing** for component interactions
- **Cross-platform testing** for platform consistency