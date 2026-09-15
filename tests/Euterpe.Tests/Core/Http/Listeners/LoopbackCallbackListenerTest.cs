using System.Net;
using System.Net.Sockets;
using System.Text;
using Euterpe.Contracts.Account;
using Euterpe.Core.Http.Listeners;

namespace Euterpe.Tests.Core.Http.Listeners;

[Category("LoopbackCallbackListenerTests")]
[TestSubject(typeof(LoopbackCallbackListener))]
public sealed class LoopbackCallbackListenerTest
{
    private const string DonePageUrl = "https://euterpe-org.com/auth/app/done";

    [Test]
    [Timeout(5_000)]
    public async Task WaitForCallback_WithCodeAndState_ParsesResultAndRedirectsToDonePage(CancellationToken cancellationToken)
    {
        var (result, response) = await RoundTripAsync("GET /callback?code=abc&state=xyz HTTP/1.1", cancellationToken);

        using var _ = Assert.Multiple();
        await Assert.That(result.Code).IsEqualTo("abc");
        await Assert.That(result.State).IsEqualTo("xyz");
        await Assert.That(result.Error).IsNull();
        await Assert.That(response).StartsWith("HTTP/1.1 302 Found");
        await Assert.That(response).Contains($"Location: {DonePageUrl}");
    }

    [Test]
    [Timeout(5_000)]
    public async Task WaitForCallback_WithError_ParsesErrorAndReturnsBadRequest(CancellationToken cancellationToken)
    {
        var (result, response) = await RoundTripAsync("GET /callback?error=access_denied&state=xyz HTTP/1.1", cancellationToken);

        using var _ = Assert.Multiple();
        await Assert.That(result.Error).IsEqualTo("access_denied");
        await Assert.That(result.Code).IsNull();
        await Assert.That(response).StartsWith("HTTP/1.1 400 Bad Request");
    }

    [Test]
    [Timeout(5_000)]
    public async Task WaitForCallback_WithoutCodeOrError_IgnoresRequestThenCompletesOnRealCallback(CancellationToken cancellationToken)
    {
        using var listener = new LoopbackCallbackListener();
        var callbackTask = listener.WaitForCallbackAsync(cancellationToken);

        // A speculative / favicon request carries neither a code nor an error and must be ignored.
        var ignoredResponse = await SendAsync(listener.Port, "GET /favicon.ico HTTP/1.1", cancellationToken);

        using (Assert.Multiple())
        {
            await Assert.That(ignoredResponse).StartsWith("HTTP/1.1 404 Not Found");
            await Assert.That(callbackTask.IsCompleted).IsFalse();
        }

        await SendAsync(listener.Port, "GET /callback?code=abc&state=xyz HTTP/1.1", cancellationToken);
        var result = await callbackTask;

        await Assert.That(result.Code).IsEqualTo("abc");
    }

    [Test]
    [Timeout(5_000)]
    public async Task WaitForCallback_WhenRequestLineSplitAcrossReads_ParsesFullLine(CancellationToken cancellationToken)
    {
        using var listener = new LoopbackCallbackListener();
        var callbackTask = listener.WaitForCallbackAsync(cancellationToken);

        using var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, listener.Port, cancellationToken);
        await using var stream = client.GetStream();

        // Send the request line in two chunks so the listener must read past the first packet.
        await stream.WriteAsync(Encoding.ASCII.GetBytes("GET /callback?code=ab"), cancellationToken);
        await stream.FlushAsync(cancellationToken);
        await Task.Delay(50, cancellationToken);
        await stream.WriteAsync(Encoding.ASCII.GetBytes("c&state=xyz HTTP/1.1\r\n\r\n"), cancellationToken);

        var result = await callbackTask;

        await Assert.That(result.Code).IsEqualTo("abc");
    }

    [Test]
    [Timeout(5_000)]
    public async Task WaitForCallback_WhenCancelled_Throws(CancellationToken cancellationToken)
    {
        using var listener = new LoopbackCallbackListener();
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        await cts.CancelAsync();

        var act = async () => await listener.WaitForCallbackAsync(cts.Token);

        await Assert.That(act).Throws<OperationCanceledException>();
    }

    [Test]
    public async Task Port_NewListener_IsAssignedByTheOperatingSystem()
    {
        using var listener = new LoopbackCallbackListener();

        await Assert.That(listener.Port).IsGreaterThan(0);
    }

    private static async Task<(LoopbackCallbackResult Result, string Response)> RoundTripAsync(string requestLine, CancellationToken cancellationToken)
    {
        using var listener = new LoopbackCallbackListener();
        var callbackTask = listener.WaitForCallbackAsync(cancellationToken);

        var response = await SendAsync(listener.Port, requestLine, cancellationToken);
        var result = await callbackTask;

        return (result, response);
    }

    private static async Task<string> SendAsync(int port, string requestLine, CancellationToken cancellationToken)
    {
        using var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, port, cancellationToken);
        await using var stream = client.GetStream();

        await stream.WriteAsync(Encoding.ASCII.GetBytes($"{requestLine}\r\n\r\n"), cancellationToken);

        var buffer = new byte[1024];
        var read = await stream.ReadAsync(buffer, cancellationToken);
        return Encoding.ASCII.GetString(buffer, 0, read);
    }
}
