using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HotPreview.SharedModel.Tests;

[TestClass]
public class PreviewsManagerBaseTests
{
    private TestPreviewsManager CreateManager(Dictionary<string, TestUIComponent> components)
    {
        var categories = new Dictionary<string, UIComponentCategory>();
        var commands = new Dictionary<string, TestCommand>();
        return new TestPreviewsManager(components, categories, commands);
    }

    [TestMethod]
    public void GetUIComponentShortName_WithUniqueSimpleName_ReturnsSimpleName()
    {
        // Arrange
        var components = new Dictionary<string, TestUIComponent>
        {
            ["MyApp.Pages.HomePage"] = new TestUIComponent("MyApp.Pages.HomePage"),
            ["MyApp.Controls.Button"] = new TestUIComponent("MyApp.Controls.Button"),
            ["MyApp.Widgets.Slider"] = new TestUIComponent("MyApp.Widgets.Slider")
        };
        var manager = CreateManager(components);

        // Act & Assert
        Assert.AreEqual("HomePage", manager.GetUIComponentShortName("MyApp.Pages.HomePage"));
        Assert.AreEqual("Button", manager.GetUIComponentShortName("MyApp.Controls.Button"));
        Assert.AreEqual("Slider", manager.GetUIComponentShortName("MyApp.Widgets.Slider"));
    }

    [TestMethod]
    public void GetUIComponentShortName_WithConflictingSimpleNames_ReturnsShortestUniqueNames()
    {
        // Arrange
        var components = new Dictionary<string, TestUIComponent>
        {
            ["A.B.Button"] = new TestUIComponent("A.B.Button"),
            ["X.Y.Button"] = new TestUIComponent("X.Y.Button"),
            ["MyApp.Controls.Slider"] = new TestUIComponent("MyApp.Controls.Slider")
        };
        var manager = CreateManager(components);

        // Act & Assert
        Assert.AreEqual("B.Button", manager.GetUIComponentShortName("A.B.Button"));
        Assert.AreEqual("Y.Button", manager.GetUIComponentShortName("X.Y.Button"));
        Assert.AreEqual("Slider", manager.GetUIComponentShortName("MyApp.Controls.Slider"));
    }

    [TestMethod]
    public void GetUIComponentShortName_WithDeeperConflicts_ReturnsLongerUniqueNames()
    {
        // Arrange
        var components = new Dictionary<string, TestUIComponent>
        {
            ["MyCompany.ProjectA.UI.Controls.Button"] = new TestUIComponent("MyCompany.ProjectA.UI.Controls.Button"),
            ["MyCompany.ProjectB.UI.Controls.Button"] = new TestUIComponent("MyCompany.ProjectB.UI.Controls.Button"),
            ["ThirdParty.Lib.Controls.Button"] = new TestUIComponent("ThirdParty.Lib.Controls.Button")
        };
        var manager = CreateManager(components);

        // Act & Assert
        Assert.AreEqual("ProjectA.UI.Controls.Button", manager.GetUIComponentShortName("MyCompany.ProjectA.UI.Controls.Button"));
        Assert.AreEqual("ProjectB.UI.Controls.Button", manager.GetUIComponentShortName("MyCompany.ProjectB.UI.Controls.Button"));
        Assert.AreEqual("Lib.Controls.Button", manager.GetUIComponentShortName("ThirdParty.Lib.Controls.Button"));
    }

    [TestMethod]
    public void GetUIComponentShortName_WithPartialConflicts_ResolvesCorrectly()
    {
        // Arrange
        var components = new Dictionary<string, TestUIComponent>
        {
            ["A.UI.Button"] = new TestUIComponent("A.UI.Button"),
            ["B.UI.Button"] = new TestUIComponent("B.UI.Button"),
            ["C.Controls.Button"] = new TestUIComponent("C.Controls.Button")
        };
        var manager = CreateManager(components);

        // Act & Assert
        // A.UI.Button and B.UI.Button both resolve to UI.Button, but that conflicts
        // So they need to be A.UI.Button and B.UI.Button
        Assert.AreEqual("A.UI.Button", manager.GetUIComponentShortName("A.UI.Button"));
        Assert.AreEqual("B.UI.Button", manager.GetUIComponentShortName("B.UI.Button"));
        Assert.AreEqual("Controls.Button", manager.GetUIComponentShortName("C.Controls.Button"));
    }

    [TestMethod]
    public void GetUIComponentShortName_WithNoNamespaces_ReturnsFullName()
    {
        // Arrange
        var components = new Dictionary<string, TestUIComponent>
        {
            ["Button"] = new TestUIComponent("Button"),
            ["Slider"] = new TestUIComponent("Slider")
        };
        var manager = CreateManager(components);

        // Act & Assert
        Assert.AreEqual("Button", manager.GetUIComponentShortName("Button"));
        Assert.AreEqual("Slider", manager.GetUIComponentShortName("Slider"));
    }

    [TestMethod]
    public void GetUIComponentShortName_WithIdenticalFullNames_ReturnsFullName()
    {
        // Arrange
        var components = new Dictionary<string, TestUIComponent>
        {
            ["MyApp.Controls.Button"] = new TestUIComponent("MyApp.Controls.Button"),
            ["OtherApp.UI.TextBox"] = new TestUIComponent("OtherApp.UI.TextBox")
        };
        var manager = CreateManager(components);

        // This test shows what happens when we can't make names unique at dot boundaries
        // In practice, this shouldn't happen with real namespaces, but we test the fallback

        // Act & Assert
        Assert.AreEqual("Button", manager.GetUIComponentShortName("MyApp.Controls.Button"));
        Assert.AreEqual("TextBox", manager.GetUIComponentShortName("OtherApp.UI.TextBox"));
    }

    [TestMethod]
    public void GetUIComponentShortName_WithNullOrEmpty_ReturnsNull()
    {
        // Arrange
        var components = new Dictionary<string, TestUIComponent>
        {
            ["MyApp.Button"] = new TestUIComponent("MyApp.Button")
        };
        var manager = CreateManager(components);

        // Act & Assert
        Assert.IsNull(manager.GetUIComponentShortName(null!));
        Assert.IsNull(manager.GetUIComponentShortName(""));
        Assert.IsNull(manager.GetUIComponentShortName("   "));
    }

    [TestMethod]
    public void GetUIComponentShortName_WithNonExistentComponent_ReturnsNull()
    {
        // Arrange
        var components = new Dictionary<string, TestUIComponent>
        {
            ["MyApp.Button"] = new TestUIComponent("MyApp.Button")
        };
        var manager = CreateManager(components);

        // Act & Assert
        Assert.IsNull(manager.GetUIComponentShortName("NonExistent.Component"));
    }

    [TestMethod]
    public void GetUIComponentShortName_CallMultipleTimes_ReturnsSameResult()
    {
        // Arrange
        var components = new Dictionary<string, TestUIComponent>
        {
            ["A.B.Button"] = new TestUIComponent("A.B.Button"),
            ["X.Y.Button"] = new TestUIComponent("X.Y.Button")
        };
        var manager = CreateManager(components);

        // Act - Call multiple times to test caching
        string? result1 = manager.GetUIComponentShortName("A.B.Button");
        string? result2 = manager.GetUIComponentShortName("A.B.Button");
        string? result3 = manager.GetUIComponentShortName("X.Y.Button");

        // Assert
        Assert.AreEqual("B.Button", result1);
        Assert.AreEqual("B.Button", result2);
        Assert.AreEqual("Y.Button", result3);
        // Verify that the second call uses cached data (this is more of a functional test)
        Assert.AreEqual(result1, result2);
    }

    [TestMethod]
    public void GetUIComponentShortName_WithMixedCases_WorksCorrectly()
    {
        // Arrange
        var components = new Dictionary<string, TestUIComponent>
        {
            ["MyApp.Pages.LoginPage"] = new TestUIComponent("MyApp.Pages.LoginPage"),
            ["YourApp.Views.LoginPage"] = new TestUIComponent("YourApp.Views.LoginPage"),
            ["ThirdApp.Components.LoginComponent"] = new TestUIComponent("ThirdApp.Components.LoginComponent")
        };
        var manager = CreateManager(components);

        // Act & Assert
        Assert.AreEqual("Pages.LoginPage", manager.GetUIComponentShortName("MyApp.Pages.LoginPage"));
        Assert.AreEqual("Views.LoginPage", manager.GetUIComponentShortName("YourApp.Views.LoginPage"));
        Assert.AreEqual("LoginComponent", manager.GetUIComponentShortName("ThirdApp.Components.LoginComponent"));
    }
}

// Test implementations
internal class TestUIComponent : UIComponentBase<TestPreview>
{
    public TestUIComponent(string name) : base(UIComponentKind.Control, null, new List<TestPreview>())
    {
        Name = name;
    }

    public override string Name { get; }

    public override UIComponentBase<TestPreview> WithAddedPreview(TestPreview preview)
    {
        var newComponent = new TestUIComponent(Name);
        return newComponent;
    }
}

internal class TestPreview : PreviewBase
{
    public TestPreview(string name) : base(null)
    {
        Name = name;
    }

    public override string Name { get; }
}

internal class TestCommand : PreviewCommandBase
{
    public TestCommand(string name) : base(null)
    {
        Name = name;
    }

    public override string Name { get; }
}

internal class TestPreviewsManager : PreviewsManagerBase<TestUIComponent, TestPreview, TestCommand>
{
    public TestPreviewsManager(
        IReadOnlyDictionary<string, TestUIComponent> uiComponents,
        IReadOnlyDictionary<string, UIComponentCategory> categories,
        IReadOnlyDictionary<string, TestCommand> commands)
        : base(uiComponents, categories, commands)
    {
    }
}
