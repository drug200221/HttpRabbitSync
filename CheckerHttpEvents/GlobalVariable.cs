using RabbitMQ.Client;

namespace CheckerHttpEvents;

public static class GlobalVariable
{
    public static readonly string FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app.config.json");
    
    public static readonly ConnectionFactory ConnectionFactory = new ()
    {
        HostName = "rabbitmq",
        Port = 5672,
        UserName = "rabbitmq",
        Password = "secretRabbitMQ"
    };
}