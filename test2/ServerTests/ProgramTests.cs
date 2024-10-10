using System;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ServerTests
{
    public class ProgramTests
    {
        private const string ServerExecutable = @"..\..\..\..\Server\bin\Debug\net8.0\Server.exe"; // Path to the Server executable

        [Fact]
        public async Task TestBroadcastMessage_SendsOnlySubscribedMessages()
        {
            Process serverProcess = null;
            try
            {
                // Start the server in a separate process
                serverProcess = StartServerProcess();
                var client = new ClientWebSocket();
                var uri = new Uri("ws://localhost:5000/");

                // Act
                await client.ConnectAsync(uri, CancellationToken.None);
                // Subscribe to CPU topic
                var subscribeMessage = Encoding.UTF8.GetBytes("SUBSCRIBE: CPU");
                await client.SendAsync(new ArraySegment<byte>(subscribeMessage), WebSocketMessageType.Text, true, CancellationToken.None);

                await Task.Delay(1000); // Wait for the subscription to process

                var buffer = new byte[1024];

                // Read from client
                var result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                // Assert that we only receive CPU messages
                Assert.Contains("CPU:", message);
                Assert.DoesNotContain("TIME:", message);

                // Cleanup
                await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Test complete", CancellationToken.None);
            }
            finally
            {
                // Ensure the server process is terminated even if there is an error
                if (serverProcess != null && !serverProcess.HasExited)
                {
                    serverProcess.Kill();
                    serverProcess.Dispose();
                }
            }
        }

        private Process StartServerProcess()
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = ServerExecutable,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            return Process.Start(startInfo);
        }
    }
}
