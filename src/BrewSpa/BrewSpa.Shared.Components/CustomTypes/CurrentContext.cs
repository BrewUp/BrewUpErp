namespace BrewSpa.Shared.Components.CustomTypes;

public record CurrentContext(string Value)
{
    public static CurrentContext None { get; set; } = new ("None");
}