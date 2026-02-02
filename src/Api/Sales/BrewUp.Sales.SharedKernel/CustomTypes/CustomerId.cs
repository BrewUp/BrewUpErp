namespace BrewUp.Sales.SharedKernel.CustomTypes;

public record CustomerId
{
    public string Value { get; init; }

    public CustomerId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("CustomerId cannot be null or whitespace.", nameof(value));
        
        Value = value;
    }
    
    public static CustomerId New() => new(Guid.NewGuid().ToString());
}