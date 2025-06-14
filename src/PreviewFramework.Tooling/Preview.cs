namespace PreviewFramework.Tooling;

public abstract class Preview(string name, string? displayName) : PreviewBase(displayName)
{
    private readonly string _name = name;

    public override string Name => _name;
}
