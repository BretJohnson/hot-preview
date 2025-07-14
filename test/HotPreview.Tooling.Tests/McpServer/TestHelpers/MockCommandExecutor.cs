using Moq;

namespace HotPreview.Tooling.Tests.McpServer.TestHelpers;

public class MockCommandExecutor
{
    public Mock<Func<string, string[], Task<(int ExitCode, string Output, string Error)>>> Mock { get; }

    public MockCommandExecutor()
    {
        Mock = new Mock<Func<string, string[], Task<(int ExitCode, string Output, string Error)>>>();
    }

    public void SetupCommand(string command, string[] args, int exitCode = 0, string output = "", string error = "")
    {
        Mock.Setup(x => x(command, args))
            .ReturnsAsync((exitCode, output, error));
    }

    public void SetupCommandPrefix(string commandPrefix, int exitCode = 0, string output = "", string error = "")
    {
        Mock.Setup(x => x(It.Is<string>(cmd => cmd.StartsWith(commandPrefix)), It.IsAny<string[]>()))
            .ReturnsAsync((exitCode, output, error));
    }

    public void SetupAdbDeviceList(string[] deviceIds)
    {
        var output = "List of devices attached\n" +
                    string.Join("\n", deviceIds.Select(id => $"{id}\tdevice"));
        SetupCommand("adb", new[] { "devices" }, 0, output);
    }

    public void SetupIosSimulatorList(params (string id, string name, string state)[] simulators)
    {
        var devices = simulators.Select(sim =>
            $"    {sim.name} ({sim.id}) ({sim.state})").ToArray();
        var output = "== Devices ==\n-- iOS --\n" + string.Join("\n", devices);
        SetupCommand("xcrun", new[] { "simctl", "list", "devices" }, 0, output);
    }

    public void VerifyCommand(string command, string[] args, Times times)
    {
        Mock.Verify(x => x(command, args), times);
    }
}
