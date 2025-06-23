using System.Net;
using Microsoft.Extensions.Logging;
using VisualTestUtils.AppConnector.Controller;

namespace SandboxServer;

public class SandboxController : AppConnectorController
{
    public SandboxController(IPAddress ipAddress, int port = 4243, ILogger? logger = null)
        : base(ipAddress, port, logger)
    {
    }
}
