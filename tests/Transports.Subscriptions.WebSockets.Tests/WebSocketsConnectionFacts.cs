using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Xunit;

namespace GraphQL.Server.Transports.WebSockets.Tests
{
    public class WebSocketsConnectionFacts : IDisposable
    {
        public WebSocketsConnectionFacts()
        {
            _host = Host
                .CreateDefaultBuilder()
                .ConfigureWebHost(webHostBuilder =>
                {
                    webHostBuilder
                        .UseTestServer()
                        .UseStartup<TestStartup>();
                })
                .Start();

            _server = _host.GetTestServer();
        }

        private readonly IHost _host;
        private readonly TestServer _server;

        private Task<WebSocket> ConnectAsync(string protocol)
        {
            var client = _server.CreateWebSocketClient();
            client.ConfigureRequest = request => request.Headers.Add("Sec-WebSocket-Protocol", protocol);
            return client.ConnectAsync(new Uri("http://localhost/graphql"), CancellationToken.None);
        }

        [Fact]
        public async Task Should_accept_websocket_connection()
        {
            /* Given */
            /* When */
            var socket = await ConnectAsync("graphql-ws");

            /* Then */
            Assert.Equal(WebSocketState.Open, socket.State);
        }

        [Fact]
        public async Task Should_accept_websocket_connection_2()
        {
            /* Given */
            /* When */
            var socket = await ConnectAsync("graphql-transport-ws");

            /* Then */
            Assert.Equal(WebSocketState.Open, socket.State);
        }

        [Fact]
        public async Task Should_not_accept_websocket_with_wrong_protocol()
        {
            var err = await Should.ThrowAsync<InvalidOperationException>(() => ConnectAsync("do-not-accept"));
            err.Message.ShouldContain("400");
        }

        public void Dispose()
        {
            _server.Dispose();
            _host.Dispose();
        }
    }
}
