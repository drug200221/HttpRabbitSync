

namespace MessageUpdater;

static class Program
{
    static async Task Main()
    {
        RabbitMqConsumer consumer = new RabbitMqConsumer();

        while (true)
        {
            await consumer.StartConsumingAsync();
        }
    }
}
