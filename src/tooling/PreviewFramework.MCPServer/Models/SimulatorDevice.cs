#nullable disable
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace MobileDevMcpServer.Models
{
    public class SimulatorDevices
    {
        [JsonPropertyName("devices")]
        public Dictionary<string, List<SimulatorDevice>> Devices { get; set; }
    }

    public class SimulatorDevice
    {
        [JsonPropertyName("udid")]
        public string Udid { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("deviceTypeIdentifier")]
        public string Runtime { get; set; }

        [JsonPropertyName("state")]
        public string State { get; set; }
    }
}
