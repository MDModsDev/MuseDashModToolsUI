using Euterpe.Shared.Http;

namespace Euterpe.Tests.Shared.Http;

[Category("XRequestIdHandlerTests")]
[TestSubject(typeof(XRequestIdHandler))]
public sealed class XRequestIdHandlerTest
{
    [Test]
    public async Task SendAsync_NewRequest_AddsXRequestIdHeader()
    {
        var inner = Mock.HttpHandler();
        inner.OnAnyRequest().Respond();
        using var handler = new XRequestIdHandler { InnerHandler = inner };
        using var client = new HttpClient(handler);

        await client.GetAsync("https://example.test/");

        var values = inner.Requests.Single().Headers["X-Request-Id"].ToArray();
        using var assertions = Assert.Multiple();
        await Assert.That(values).HasSingleItem();
        await Assert.That(Guid.TryParse(values[0], out _)).IsTrue();
    }

    [Test]
    public async Task SendAsync_MultipleRequests_GeneratesDifferentIdPerRequest()
    {
        var inner = Mock.HttpHandler();
        inner.OnAnyRequest().Respond();
        using var handler = new XRequestIdHandler { InnerHandler = inner };
        using var client = new HttpClient(handler);

        await client.GetAsync("https://example.test/a");
        await client.GetAsync("https://example.test/b");

        var firstId = inner.Requests[0].Headers["X-Request-Id"].Single();
        var secondId = inner.Requests[1].Headers["X-Request-Id"].Single();
        await Assert.That(firstId).IsNotEqualTo(secondId);
    }
}
