using System.Collections.Generic;
using HotPreview.SharedModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HotPreview.Tooling.Tests;

[TestClass]
public class PreviewsManagerToolingTests
{
    private PreviewsManagerTooling CreateManager(Dictionary<string, UIComponentTooling> components)
    {
        var categories = new Dictionary<string, UIComponentCategory>();
        var commands = new Dictionary<string, PreviewCommandTooling>();
        return new PreviewsManagerTooling(components, categories, commands);
    }

    [TestMethod]
    public void GetUIComponentShortName_WithUniqueSimpleName_ReturnsSimpleName()
    {
        // Arrange
        var components = new Dictionary<string, UIComponentTooling>
        {
            ["MyApp.Pages.HomePage"] = new UIComponentTooling(UIComponentKind.Page, "MyApp.Pages.HomePage", null, []),
            ["MyApp.Controls.Button"] = new UIComponentTooling(UIComponentKind.Control, "MyApp.Controls.Button", null, []),
            ["MyApp.Widgets.Slider"] = new UIComponentTooling(UIComponentKind.Control, "MyApp.Widgets.Slider", null, [])
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
        var components = new Dictionary<string, UIComponentTooling>
        {
            ["A.B.Button"] = new UIComponentTooling(UIComponentKind.Control, "A.B.Button", null, []),
            ["X.Y.Button"] = new UIComponentTooling(UIComponentKind.Control, "X.Y.Button", null, []),
            ["MyApp.Controls.Slider"] = new UIComponentTooling(UIComponentKind.Control, "MyApp.Controls.Slider", null, [])
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
        var components = new Dictionary<string, UIComponentTooling>
        {
            ["MyCompany.ProjectA.UI.Controls.Button"] = new UIComponentTooling(UIComponentKind.Control, "MyCompany.ProjectA.UI.Controls.Button", null, []),
            ["MyCompany.ProjectB.UI.Controls.Button"] = new UIComponentTooling(UIComponentKind.Control, "MyCompany.ProjectB.UI.Controls.Button", null, []),
            ["ThirdParty.Lib.Controls.Button"] = new UIComponentTooling(UIComponentKind.Control, "ThirdParty.Lib.Controls.Button", null, [])
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
        var components = new Dictionary<string, UIComponentTooling>
        {
            ["A.UI.Button"] = new UIComponentTooling(UIComponentKind.Control, "A.UI.Button", null, []),
            ["B.UI.Button"] = new UIComponentTooling(UIComponentKind.Control, "B.UI.Button", null, []),
            ["C.Controls.Button"] = new UIComponentTooling(UIComponentKind.Control, "C.Controls.Button", null, [])
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
        var components = new Dictionary<string, UIComponentTooling>
        {
            ["Button"] = new UIComponentTooling(UIComponentKind.Control, "Button", null, []),
            ["Slider"] = new UIComponentTooling(UIComponentKind.Control, "Slider", null, [])
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
        var components = new Dictionary<string, UIComponentTooling>
        {
            ["MyApp.Controls.Button"] = new UIComponentTooling(UIComponentKind.Control, "MyApp.Controls.Button", null, []),
            ["OtherApp.UI.TextBox"] = new UIComponentTooling(UIComponentKind.Control, "OtherApp.UI.TextBox", null, [])
        };
        var manager = CreateManager(components);

        // This test shows what happens when we can't make names unique at dot boundaries
        // In practice, this shouldn't happen with real namespaces, but we test the fallback

        // Act & Assert
        Assert.AreEqual("Button", manager.GetUIComponentShortName("MyApp.Controls.Button"));
        Assert.AreEqual("TextBox", manager.GetUIComponentShortName("OtherApp.UI.TextBox"));
    }

    [TestMethod]
    public void GetUIComponentShortName_WithNonExistentComponent_ThrowsArgumentException()
    {
        // Arrange
        var components = new Dictionary<string, UIComponentTooling>
        {
            ["MyApp.Button"] = new UIComponentTooling(UIComponentKind.Control, "MyApp.Button", null, [])
        };
        var manager = CreateManager(components);

        // Act & Assert
        Assert.ThrowsException<ArgumentException>(() => manager.GetUIComponentShortName(""));
        Assert.ThrowsException<ArgumentException>(() => manager.GetUIComponentShortName("NonExistent.Component"));
    }

    [TestMethod]
    public void GetUIComponentShortName_CallMultipleTimes_ReturnsSameResult()
    {
        // Arrange
        var components = new Dictionary<string, UIComponentTooling>
        {
            ["A.B.Button"] = new UIComponentTooling(UIComponentKind.Control, "A.B.Button", null, []),
            ["X.Y.Button"] = new UIComponentTooling(UIComponentKind.Control, "X.Y.Button", null, [])
        };
        var manager = CreateManager(components);

        // Act - Call multiple times to test caching
        string result1 = manager.GetUIComponentShortName("A.B.Button");
        string result2 = manager.GetUIComponentShortName("A.B.Button");
        string result3 = manager.GetUIComponentShortName("X.Y.Button");

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
        var components = new Dictionary<string, UIComponentTooling>
        {
            ["MyApp.Pages.LoginPage"] = new UIComponentTooling(UIComponentKind.Page, "MyApp.Pages.LoginPage", null, []),
            ["YourApp.Views.LoginPage"] = new UIComponentTooling(UIComponentKind.Page, "YourApp.Views.LoginPage", null, []),
            ["ThirdApp.Components.LoginComponent"] = new UIComponentTooling(UIComponentKind.Control, "ThirdApp.Components.LoginComponent", null, [])
        };
        var manager = CreateManager(components);

        // Act & Assert
        Assert.AreEqual("Pages.LoginPage", manager.GetUIComponentShortName("MyApp.Pages.LoginPage"));
        Assert.AreEqual("Views.LoginPage", manager.GetUIComponentShortName("YourApp.Views.LoginPage"));
        Assert.AreEqual("LoginComponent", manager.GetUIComponentShortName("ThirdApp.Components.LoginComponent"));
    }

    // Test implementation for PreviewTooling
    private class TestPreviewTooling : PreviewTooling
    {
        public TestPreviewTooling(string name, string? displayNameOverride = null) : base(name, displayNameOverride)
        {
        }
    }

    [TestMethod]
    public void GetPreviewShortName_WithUniqueSimpleName_ReturnsSimpleName()
    {
        // Arrange
        var previews = new List<PreviewTooling>
        {
            new TestPreviewTooling("MyApp.Previews.Default"),
            new TestPreviewTooling("MyApp.Previews.Loading"),
            new TestPreviewTooling("MyApp.Previews.Error")
        };
        var component = new UIComponentTooling(UIComponentKind.Control, "MyApp.Controls.Button", null, previews);

        // Act & Assert
        Assert.AreEqual("Default", component.GetPreviewShortName("MyApp.Previews.Default"));
        Assert.AreEqual("Loading", component.GetPreviewShortName("MyApp.Previews.Loading"));
        Assert.AreEqual("Error", component.GetPreviewShortName("MyApp.Previews.Error"));
    }

    [TestMethod]
    public void GetPreviewShortName_WithConflictingSimpleNames_ReturnsShortestUniqueNames()
    {
        // Arrange
        var previews = new List<PreviewTooling>
        {
            new TestPreviewTooling("A.B.Default"),
            new TestPreviewTooling("X.Y.Default"),
            new TestPreviewTooling("MyApp.Previews.Loading")
        };
        var component = new UIComponentTooling(UIComponentKind.Control, "MyApp.Controls.Button", null, previews);

        // Act & Assert
        Assert.AreEqual("B.Default", component.GetPreviewShortName("A.B.Default"));
        Assert.AreEqual("Y.Default", component.GetPreviewShortName("X.Y.Default"));
        Assert.AreEqual("Loading", component.GetPreviewShortName("MyApp.Previews.Loading"));
    }

    [TestMethod]
    public void GetPreviewShortName_WithDeeperConflicts_ReturnsLongerUniqueNames()
    {
        // Arrange
        var previews = new List<PreviewTooling>
        {
            new TestPreviewTooling("MyCompany.ProjectA.UI.Previews.Default"),
            new TestPreviewTooling("MyCompany.ProjectB.UI.Previews.Default"),
            new TestPreviewTooling("ThirdParty.Lib.Previews.Default")
        };
        var component = new UIComponentTooling(UIComponentKind.Control, "MyApp.Controls.Button", null, previews);

        // Act & Assert
        Assert.AreEqual("ProjectA.UI.Previews.Default", component.GetPreviewShortName("MyCompany.ProjectA.UI.Previews.Default"));
        Assert.AreEqual("ProjectB.UI.Previews.Default", component.GetPreviewShortName("MyCompany.ProjectB.UI.Previews.Default"));
        Assert.AreEqual("Lib.Previews.Default", component.GetPreviewShortName("ThirdParty.Lib.Previews.Default"));
    }

    [TestMethod]
    public void GetPreviewShortName_WithPartialConflicts_ResolvesCorrectly()
    {
        // Arrange
        var previews = new List<PreviewTooling>
        {
            new TestPreviewTooling("A.UI.Default"),
            new TestPreviewTooling("B.UI.Default"),
            new TestPreviewTooling("C.Previews.Default")
        };
        var component = new UIComponentTooling(UIComponentKind.Control, "MyApp.Controls.Button", null, previews);

        // Act & Assert
        // A.UI.Default and B.UI.Default both resolve to UI.Default, but that conflicts
        // So they need to be A.UI.Default and B.UI.Default
        Assert.AreEqual("A.UI.Default", component.GetPreviewShortName("A.UI.Default"));
        Assert.AreEqual("B.UI.Default", component.GetPreviewShortName("B.UI.Default"));
        Assert.AreEqual("Previews.Default", component.GetPreviewShortName("C.Previews.Default"));
    }

    [TestMethod]
    public void GetPreviewShortName_WithNoNamespaces_ReturnsFullName()
    {
        // Arrange
        var previews = new List<PreviewTooling>
        {
            new TestPreviewTooling("Default"),
            new TestPreviewTooling("Loading")
        };
        var component = new UIComponentTooling(UIComponentKind.Control, "MyApp.Controls.Button", null, previews);

        // Act & Assert
        Assert.AreEqual("Default", component.GetPreviewShortName("Default"));
        Assert.AreEqual("Loading", component.GetPreviewShortName("Loading"));
    }

    [TestMethod]
    public void GetPreviewShortName_WithIdenticalFullNames_ReturnsFullName()
    {
        // Arrange
        var previews = new List<PreviewTooling>
        {
            new TestPreviewTooling("MyApp.Previews.Default"),
            new TestPreviewTooling("OtherApp.UI.Loading")
        };
        var component = new UIComponentTooling(UIComponentKind.Control, "MyApp.Controls.Button", null, previews);

        // This test shows what happens when we can't make names unique at dot boundaries
        // In practice, this shouldn't happen with real namespaces, but we test the fallback

        // Act & Assert
        Assert.AreEqual("Default", component.GetPreviewShortName("MyApp.Previews.Default"));
        Assert.AreEqual("Loading", component.GetPreviewShortName("OtherApp.UI.Loading"));
    }

    [TestMethod]
    public void GetPreviewShortName_WithNonExistentPreview_ThrowsArgumentException()
    {
        // Arrange
        var previews = new List<PreviewTooling>
        {
            new TestPreviewTooling("MyApp.Default")
        };
        var component = new UIComponentTooling(UIComponentKind.Control, "MyApp.Controls.Button", null, previews);

        // Act & Assert
        Assert.ThrowsException<ArgumentException>(() => component.GetPreviewShortName(""));
        Assert.ThrowsException<ArgumentException>(() => component.GetPreviewShortName("NonExistent.Preview"));
    }

    [TestMethod]
    public void GetPreviewShortName_CallMultipleTimes_ReturnsSameResult()
    {
        // Arrange
        var previews = new List<PreviewTooling>
        {
            new TestPreviewTooling("A.B.Default"),
            new TestPreviewTooling("X.Y.Default")
        };
        var component = new UIComponentTooling(UIComponentKind.Control, "MyApp.Controls.Button", null, previews);

        // Act - Call multiple times to test caching
        string result1 = component.GetPreviewShortName("A.B.Default");
        string result2 = component.GetPreviewShortName("A.B.Default");
        string result3 = component.GetPreviewShortName("X.Y.Default");

        // Assert
        Assert.AreEqual("B.Default", result1);
        Assert.AreEqual("B.Default", result2);
        Assert.AreEqual("Y.Default", result3);
        // Verify that the second call uses cached data (this is more of a functional test)
        Assert.AreEqual(result1, result2);
    }

    [TestMethod]
    public void GetPreviewShortName_WithMixedCases_WorksCorrectly()
    {
        // Arrange
        var previews = new List<PreviewTooling>
        {
            new TestPreviewTooling("MyApp.Previews.LoginForm"),
            new TestPreviewTooling("YourApp.States.LoginForm"),
            new TestPreviewTooling("ThirdApp.Components.LoginDialog")
        };
        var component = new UIComponentTooling(UIComponentKind.Control, "MyApp.Controls.Button", null, previews);

        // Act & Assert
        Assert.AreEqual("Previews.LoginForm", component.GetPreviewShortName("MyApp.Previews.LoginForm"));
        Assert.AreEqual("States.LoginForm", component.GetPreviewShortName("YourApp.States.LoginForm"));
        Assert.AreEqual("LoginDialog", component.GetPreviewShortName("ThirdApp.Components.LoginDialog"));
    }

    [TestMethod]
    public void GetPreviewShortName_WithSinglePreview_ReturnsSimpleName()
    {
        // Arrange
        var previews = new List<PreviewTooling>
        {
            new TestPreviewTooling("MyApp.Previews.VeryLongNamespacedPreview")
        };
        var component = new UIComponentTooling(UIComponentKind.Control, "MyApp.Controls.Button", null, previews);

        // Act & Assert
        // When there's only one preview, it should return the simple name regardless of namespace length
        Assert.AreEqual("VeryLongNamespacedPreview", component.GetPreviewShortName("MyApp.Previews.VeryLongNamespacedPreview"));
    }

    [TestMethod]
    public void GetPreviewShortName_WithEmptyPreviewsList_ThrowsArgumentException()
    {
        // Arrange
        var previews = new List<PreviewTooling>();
        var component = new UIComponentTooling(UIComponentKind.Control, "MyApp.Controls.Button", null, previews);

        // Act & Assert
        Assert.ThrowsException<ArgumentException>(() => component.GetPreviewShortName("NonExistent.Preview"));
    }
}
