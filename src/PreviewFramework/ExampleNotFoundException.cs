using System;

namespace ExampleFramework.App;

public class ExampleNotFoundException : Exception
{
    public ExampleNotFoundException(string message) : base(message)
    {
    }
}
