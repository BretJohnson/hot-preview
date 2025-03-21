using System.Collections.Generic;

namespace Microsoft.UIPreview.Maui;

public class ShellPreview
{
    public string Route { get; }
    public IDictionary<string, object> Parameters{ get; }

    public ShellPreview(string route, IDictionary<string, object> parameters)
    {
        this.Route = route;
        this.Parameters = parameters;
    }

    public ShellPreview(string route)
    {
        this.Route = route;
        this.Parameters = new Dictionary<string, object>
        {
        };
    }

    public ShellPreview(string route, string parameter1, object value1)
    {
        this.Route = route;
        this.Parameters = new Dictionary<string, object>
        {
            { parameter1, value1 }
        };
    }

    public ShellPreview(string route, string parameter1, object value1, string parameter2, object value2)
    {
        this.Route = route;
        this.Parameters = new Dictionary<string, object>
        {
            { parameter1, value1 },
            { parameter2, value2 }
        };
    }
}
