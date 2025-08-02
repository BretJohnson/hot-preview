using HotPreview.SharedModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HotPreview.SharedModel.Tests;

[TestClass]
public class StringUtilitiesTests
{
    [TestMethod]
    public void StartCase_WithNull_ReturnsEmptyString()
    {
        // Act
        var result = StringUtilities.StartCase(null!);

        // Assert
        Assert.AreEqual(string.Empty, result);
    }

    [TestMethod]
    public void StartCase_WithEmptyString_ReturnsEmptyString()
    {
        // Act
        var result = StringUtilities.StartCase(string.Empty);

        // Assert
        Assert.AreEqual(string.Empty, result);
    }

    [TestMethod]
    public void StartCase_WithWhitespaceOnly_ReturnsEmptyString()
    {
        // Act
        var result = StringUtilities.StartCase("   ");

        // Assert
        Assert.AreEqual(string.Empty, result);
    }

    [TestMethod]
    public void StartCase_WithSingleWord_CapitalizesFirstLetter()
    {
        // Act
        var result = StringUtilities.StartCase("hello");

        // Assert
        Assert.AreEqual("Hello", result);
    }

    [TestMethod]
    public void StartCase_WithCamelCase_ConvertsToStartCase()
    {
        // Act
        var result = StringUtilities.StartCase("fooBar");

        // Assert
        Assert.AreEqual("Foo Bar", result);
    }

    [TestMethod]
    public void StartCase_WithPascalCase_ConvertsToStartCase()
    {
        // Act
        var result = StringUtilities.StartCase("FooBar");

        // Assert
        Assert.AreEqual("Foo Bar", result);
    }

    [TestMethod]
    public void StartCase_WithKebabCase_ConvertsToStartCase()
    {
        // Act
        var result = StringUtilities.StartCase("foo-bar");

        // Assert
        Assert.AreEqual("Foo Bar", result);
    }

    [TestMethod]
    public void StartCase_WithKebabCaseAndExtraHyphens_ConvertsToStartCase()
    {
        // Act
        var result = StringUtilities.StartCase("--foo-bar--");

        // Assert
        Assert.AreEqual("Foo Bar", result);
    }

    [TestMethod]
    public void StartCase_WithSnakeCase_ConvertsToStartCase()
    {
        // Act
        var result = StringUtilities.StartCase("foo_bar");

        // Assert
        Assert.AreEqual("Foo Bar", result);
    }

    [TestMethod]
    public void StartCase_WithSnakeCaseAndExtraUnderscores_ConvertsToStartCase()
    {
        // Act
        var result = StringUtilities.StartCase("__foo_bar__");

        // Assert
        Assert.AreEqual("Foo Bar", result);
    }

    [TestMethod]
    public void StartCase_WithSpaces_ConvertsToStartCase()
    {
        // Act
        var result = StringUtilities.StartCase("foo bar");

        // Assert
        Assert.AreEqual("Foo Bar", result);
    }

    [TestMethod]
    public void StartCase_WithMultipleSpaces_ConvertsToStartCase()
    {
        // Act
        var result = StringUtilities.StartCase("  foo   bar  ");

        // Assert
        Assert.AreEqual("Foo Bar", result);
    }

    [TestMethod]
    public void StartCase_WithMixedSeparators_ConvertsToStartCase()
    {
        // Act
        var result = StringUtilities.StartCase("foo-bar_baz qux");

        // Assert
        Assert.AreEqual("Foo Bar Baz Qux", result);
    }

    [TestMethod]
    public void StartCase_WithNumbers_HandlesNumbersCorrectly()
    {
        // Act
        var result = StringUtilities.StartCase("foo2Bar3");

        // Assert
        Assert.AreEqual("Foo 2 Bar 3", result);
    }

    [TestMethod]
    public void StartCase_WithNumbersAndSeparators_HandlesCorrectly()
    {
        // Act
        var result = StringUtilities.StartCase("foo2-bar3_baz");

        // Assert
        Assert.AreEqual("Foo 2 Bar 3 Baz", result);
    }

    [TestMethod]
    public void StartCase_WithConsecutiveCapitals_HandlesAcronyms()
    {
        // Act
        var result = StringUtilities.StartCase("XMLHttpRequest");

        // Assert
        Assert.AreEqual("Xml Http Request", result);
    }

    [TestMethod]
    public void StartCase_WithMixedCase_NormalizesCorrectly()
    {
        // Act
        var result = StringUtilities.StartCase("fOoBar");

        // Assert
        Assert.AreEqual("F Oo Bar", result);
    }

    [TestMethod]
    public void StartCase_WithAlreadyFormattedString_ReturnsCorrectCase()
    {
        // Act
        var result = StringUtilities.StartCase("Foo Bar");

        // Assert
        Assert.AreEqual("Foo Bar", result);
    }

    [TestMethod]
    public void StartCase_WithSpecialCharacters_IgnoresSpecialChars()
    {
        // Act
        var result = StringUtilities.StartCase("foo@bar#baz");

        // Assert
        Assert.AreEqual("Foo@bar#baz", result);
    }

    [TestMethod]
    public void StartCase_WithSimpleWord_ProducesExpectedResults()
    {
        // Act & Assert
        Assert.AreEqual("Hello", StringUtilities.StartCase("hello"));
    }

    [TestMethod]
    public void StartCase_WithCamelCaseWord_ProducesExpectedResults()
    {
        // Act & Assert
        Assert.AreEqual("Hello World", StringUtilities.StartCase("helloWorld"));
    }

    [TestMethod]
    public void StartCase_WithKebabCaseWord_ProducesExpectedResults()
    {
        // Act & Assert
        Assert.AreEqual("Hello World", StringUtilities.StartCase("hello-world"));
    }

    [TestMethod]
    public void StartCase_WithSnakeCaseWord_ProducesExpectedResults()
    {
        // Act & Assert
        Assert.AreEqual("Hello World", StringUtilities.StartCase("hello_world"));
    }

    [TestMethod]
    public void StartCase_WithSpacedWords_ProducesExpectedResults()
    {
        // Act & Assert
        Assert.AreEqual("Hello World", StringUtilities.StartCase("hello world"));
    }

    [TestMethod]
    public void StartCase_WithAllCapsSnakeCase_ProducesExpectedResults()
    {
        // Act & Assert
        Assert.AreEqual("Hello World", StringUtilities.StartCase("HELLO_WORLD"));
    }

    [TestMethod]
    public void StartCase_WithHTMLElement_ProducesExpectedResults()
    {
        // Act & Assert
        Assert.AreEqual("Html Element", StringUtilities.StartCase("HTMLElement"));
    }

    [TestMethod]
    public void StartCase_WithGetElementById_ProducesExpectedResults()
    {
        // Act & Assert
        Assert.AreEqual("Get Element By Id", StringUtilities.StartCase("getElementById"));
    }
}