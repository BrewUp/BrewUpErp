namespace BrewUp.Infrastructure.RabbitMq;

public class RabbitMqSettings
{
    public required string Host { get; init; } = string.Empty;
    public required string ExchangeCommandName { get; init; } = string.Empty;
    public required string ExchangeEventName { get; init; } = string.Empty;
    public required string Username { get; init; } = string.Empty;
    public required string Password { get; init; } = string.Empty;
    public required string ClientId { get; init; } = string.Empty;
    public required bool UseRMQ { get; init; } = false;
}