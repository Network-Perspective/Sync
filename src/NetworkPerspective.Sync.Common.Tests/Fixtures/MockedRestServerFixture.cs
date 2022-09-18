using System;
using System.Net.Http;

using WireMock.Server;

namespace NetworkPerspective.Sync.Common.Tests.Fixtures
{
    public class MockedRestServerFixture : IDisposable
    {
        public HttpClient HttpClient { get; }
        public WireMockServer WireMockServer { get; }

        public MockedRestServerFixture()
        {
            WireMockServer = WireMockServer.Start();

            HttpClient = new HttpClient
            {
                BaseAddress = new Uri(WireMockServer.Urls[0])
            };
        }

        public void Dispose()
        {
            HttpClient?.Dispose();
            WireMockServer?.Dispose();
        }
    }
}