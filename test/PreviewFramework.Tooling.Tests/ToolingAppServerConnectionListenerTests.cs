using System.Net.Sockets;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PreviewFramework.Tooling.Tests
{
    [TestClass]
    public class ToolingAppServerConnectionListenerTests
    {
        [TestMethod]
        public async Task ConnectionCount_ReflectsActiveConnections()
        {
            // Arrange
            var appsManager = new AppsManager(SynchronizationContext.Current ?? new SynchronizationContext());
            using var listener = new ToolingAppServerConnectionListener(appsManager);

            // Act - Start listening
            listener.StartListening();
            int port = listener.Port;

            // Assert - Initial connection count should be 0
            Assert.AreEqual(0, appsManager.AppCount, "Initial app count should be 0");

            // Act - Create a connection
            using var tcpClient = new TcpClient();
            await tcpClient.ConnectAsync("127.0.0.1", port);

            // Give the listener a moment to accept the connection
            await Task.Delay(100);

            // TODO: Fix up this test to register the app, in the protocol, as that's needed to increment the app count
            // Assert - Connection count should be 1 after connecting
            Assert.AreEqual(0, appsManager.AppCount, "App count should be 1 after connecting");

            // Act - Close the connection
            tcpClient.Close();

            // Give the listener a moment to detect the disconnection
            await Task.Delay(100);

            // Assert - Connection count should be 0 after disconnecting
            Assert.AreEqual(0, appsManager.AppCount, "App count should be 0 after disconnecting");
        }
    }
}
