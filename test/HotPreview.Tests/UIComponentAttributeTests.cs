using HotPreview;
using Xunit;

public class UIComponentAttributeTests
{
    [Fact]
    public void UIComponentAttribute_DefaultConstructor_SetsPropertiesToNull()
    {
        // Act
        var attribute = new UIComponentAttribute();

        // Assert
        Assert.Null(attribute.DisplayName);
        Assert.Null(attribute.AutoGeneratePreview);
    }

    [Fact]
    public void UIComponentAttribute_WithDisplayName_SetsDisplayNameProperty()
    {
        // Arrange
        const string displayName = "Test Component";

        // Act
        var attribute = new UIComponentAttribute(displayName);

        // Assert
        Assert.Equal(displayName, attribute.DisplayName);
        Assert.Null(attribute.AutoGeneratePreview);
    }

    [Fact]
    public void UIComponentAttribute_WithAutoGeneratePreviewTrue_SetsAutoGeneratePreviewProperty()
    {
        // Act
        var attribute = new UIComponentAttribute(autoGeneratePreview: true);

        // Assert
        Assert.Null(attribute.DisplayName);
        Assert.True(attribute.AutoGeneratePreview);
    }

    [Fact]
    public void UIComponentAttribute_WithAutoGeneratePreviewFalse_SetsAutoGeneratePreviewProperty()
    {
        // Act
        var attribute = new UIComponentAttribute(autoGeneratePreview: false);

        // Assert
        Assert.Null(attribute.DisplayName);
        Assert.False(attribute.AutoGeneratePreview);
    }

    [Fact]
    public void UIComponentAttribute_WithBothParameters_SetsBothProperties()
    {
        // Arrange
        const string displayName = "Test Component";
        const bool autoGeneratePreview = false;

        // Act
        var attribute = new UIComponentAttribute(displayName, autoGeneratePreview);

        // Assert
        Assert.Equal(displayName, attribute.DisplayName);
        Assert.Equal(autoGeneratePreview, attribute.AutoGeneratePreview);
    }

    [Fact]
    public void UIComponentAttribute_WithNullAutoGeneratePreview_SetsAutoGeneratePreviewToNull()
    {
        // Act
        var attribute = new UIComponentAttribute("Test", null);

        // Assert
        Assert.Equal("Test", attribute.DisplayName);
        Assert.Null(attribute.AutoGeneratePreview);
    }
}
