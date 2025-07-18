using System;
using System.Reflection;
using HotPreview;

// Test classes to verify AutoGeneratePreview functionality

// This class should allow auto-generation (default behavior)
[UIComponent("Test Component 1")]
public class TestComponent1
{
    public TestComponent1() { }
}

// This class should NOT allow auto-generation (explicitly disabled)
[UIComponent("Test Component 2", autoGeneratePreview: false)]
public class TestComponent2
{
    public TestComponent2() { }
}

// This class should allow auto-generation (explicitly enabled)
[UIComponent("Test Component 3", autoGeneratePreview: true)]
public class TestComponent3
{
    public TestComponent3() { }
}

// This class should allow auto-generation (null value, default behavior)
[UIComponent("Test Component 4", autoGeneratePreview: null)]
public class TestComponent4
{
    public TestComponent4() { }
}

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Testing UIComponentAttribute AutoGeneratePreview functionality:");
        Console.WriteLine();

        TestAttribute<TestComponent1>("TestComponent1 (default)");
        TestAttribute<TestComponent2>("TestComponent2 (false)");
        TestAttribute<TestComponent3>("TestComponent3 (true)");
        TestAttribute<TestComponent4>("TestComponent4 (null)");
    }

    static void TestAttribute<T>(string description)
    {
        Type type = typeof(T);
        UIComponentAttribute? attribute = type.GetCustomAttribute<UIComponentAttribute>();
        
        Console.WriteLine($"{description}:");
        Console.WriteLine($"  DisplayName: {attribute?.DisplayName ?? "null"}");
        Console.WriteLine($"  AutoGeneratePreview: {attribute?.AutoGeneratePreview?.ToString() ?? "null"}");
        Console.WriteLine();
    }
}
