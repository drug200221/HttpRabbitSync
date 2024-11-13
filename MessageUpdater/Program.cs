using System.Text.Json.Nodes;
using CheckerHttpEvents;

namespace MessageUpdater;

static class Program
{
    private static readonly RabbitMqConsumer Consumer = new ();
    static async Task Main()
    {
        await Consumer.StartConsumingAsync(GetAllCameras(JsonNode.Parse(await File.ReadAllTextAsync(GlobalVariable.FilePath))));
    }
    
    static List<(string type, string id)> GetAllCameras(JsonNode? jsonNode)
    {
        var cameras = new List<(string type, string id)>();

        var branches = jsonNode!["Config"]?["Branches"];
        if (branches != null)
        {
            foreach (var branch in branches.AsArray())
            {
                var dormitories = branch!["Dormitories"];
                if (dormitories != null)
                {
                    foreach (var dormitory in dormitories.AsArray())
                    {
                        var camerasInDormitory = dormitory!["Cameras"];
                        if (camerasInDormitory != null)
                        {
                            foreach (var camera in camerasInDormitory.AsArray())
                            {
                                cameras.Add((camera!["type"]!.ToString(), camera["id"]!.ToString()));
                            }
                        }
                    }
                }
            }
        }

        return cameras;
    }
}
