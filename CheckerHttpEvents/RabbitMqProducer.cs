using System.Text;
using RabbitMQ.Client;

namespace CheckerHttpEvents;

public class RabbitMqProducer
{
    private const string QueueName = "facesCompleteEventsQueue";
    private readonly ConnectionFactory _factory = GlobalVariable.ConnectionFactory;

    public async Task SendMessageAsync(string message)
    {
        await using var connection = await _factory.CreateConnectionAsync();
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