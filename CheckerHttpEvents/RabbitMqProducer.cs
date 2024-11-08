using System.Text;
using RabbitMQ.Client;

namespace CheckerHttpEvents;

public class RabbitMqProducer
{
    private const string QueueName = "facesCompleteEventsQueue";

    public async Task SendMessageAsync(string message)
    {
        var factory = new ConnectionFactory()
        {
            HostName = "localhost",
            Port = 5673
        };

        await using var connection = await factory.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();
            
        await channel.QueueDeclareAsync(
            queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        var body = Encoding.UTF8.GetBytes(message);

        await channel.BasicPublishAsync(
            exchange: "",
            routingKey: QueueName,
            body: body);
    }
}