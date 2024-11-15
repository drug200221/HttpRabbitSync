using DotNetEnv;
using RabbitMQ.Client;

namespace CheckerHttpEvents;

public static class GlobalVariable
{
    public static readonly string FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app.config.json");
    
    public static readonly ConnectionFactory ConnectionFactory = new ()
    {
        HostName = Env.GetString("RABBITMQ_HOSTNAME"),
        Port = (Env.GetInt("RABBITMQ_PORT")),
        UserName = Env.GetString("RABBITMQ_DEFAULT_USER"),
        Password = Env.GetString("RABBITMQ_DEFAULT_PASS")
    };
}