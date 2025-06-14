namespace PreviewFramework;

public class RoutePreview(string route)
{
    public string Route { get; } = route;
}

public class RoutePreview<T>(string route) : RoutePreview(route) where T : class
{
}
