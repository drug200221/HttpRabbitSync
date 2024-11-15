using RabbitMQ.Client;

namespace CheckerHttpEvents;

public static class GlobalVariable
{
    public static readonly string FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app.config.json");

    public static readonly ConnectionFactory ConnectionFactory = new ()
    {
        HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOSTNAME"),
        Port = int.Parse(Environment.GetEnvironmentVariable("RABBITMQ_PORT")),
        UserName = Environment.GetEnvironmentVariable("RABBITMQ_DEFAULT_USER"),
        Password = Environment.GetEnvironmentVariable("RABBITMQ_DEFAULT_PASS"),
    };
}