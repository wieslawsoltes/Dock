using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Dock.Avalonia.AvalonDock.ImageTests;

internal sealed class ImageHttpServer : IAsyncDisposable
{
    private readonly byte[] _image;
    private readonly TcpListener _listener = new(IPAddress.Loopback, 0);
    private readonly CancellationTokenSource _stop = new();
    private readonly ECDsa? _key;
    private readonly X509Certificate2? _certificate;
    private readonly Task _serve;
    private readonly bool _pauseBody;
    internal Uri BaseUri { get; }
    internal ConcurrentQueue<string> Requests { get; } = new();
    internal TaskCompletionSource BodyStarted { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

    internal ImageHttpServer(byte[] image, bool tls, bool pauseBody = false)
    {
        _image = image;
        _pauseBody = pauseBody;
        if (tls)
        {
            _key = ECDsa.Create(ECCurve.NamedCurves.nistP256);
            CertificateRequest request = new("CN=localhost", _key, HashAlgorithmName.SHA256);
            SubjectAlternativeNameBuilder names = new();
            names.AddIpAddress(IPAddress.Loopback);
            request.CertificateExtensions.Add(names.Build());
            _certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(1));
        }
        _listener.Start();
        BaseUri = new($"{(tls ? "https" : "http")}://127.0.0.1:{((IPEndPoint)_listener.LocalEndpoint).Port}/");
        _serve = ServeAsync();
    }

    internal HttpClient CreateClient()
    {
        HttpClientHandler handler = new();
        if (_certificate is not null)
            handler.ServerCertificateCustomValidationCallback = (_, certificate, _, errors) =>
                errors is SslPolicyErrors.None or SslPolicyErrors.RemoteCertificateChainErrors &&
                certificate?.GetCertHashString() == _certificate.GetCertHashString();
        return new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(5) };
    }

    private async Task ServeAsync()
    {
        while (!_stop.IsCancellationRequested)
        {
            try
            {
                using TcpClient client = await _listener.AcceptTcpClientAsync(_stop.Token).ConfigureAwait(false);
                using Stream stream = _certificate is null ? client.GetStream() : new SslStream(client.GetStream());
                if (stream is SslStream tls)
                    await tls.AuthenticateAsServerAsync(new SslServerAuthenticationOptions { ServerCertificate = _certificate }, _stop.Token).ConfigureAwait(false);
                using StreamReader reader = new(stream, Encoding.ASCII, false, 1024, true);
                string? first = await reader.ReadLineAsync(_stop.Token).ConfigureAwait(false);
                if (first is null) continue;
                string? line;
                do { line = await reader.ReadLineAsync(_stop.Token).ConfigureAwait(false); } while (!string.IsNullOrEmpty(line));
                string path = first.Split(' ')[1];
                Requests.Enqueue(path);
                bool redirect = path == "/redirect";
                string headers = redirect
                    ? "HTTP/1.1 302 Found\r\nLocation: /icon.png\r\nContent-Length: 0\r\nConnection: close\r\n\r\n"
                    : $"HTTP/1.1 200 OK\r\nContent-Type: image/png\r\nContent-Length: {_image.Length}\r\nConnection: close\r\n\r\n";
                await stream.WriteAsync(Encoding.ASCII.GetBytes(headers), _stop.Token).ConfigureAwait(false);
                if (_pauseBody)
                {
                    await stream.FlushAsync(_stop.Token).ConfigureAwait(false);
                    BodyStarted.TrySetResult();
                    await Task.Delay(Timeout.Infinite, _stop.Token).ConfigureAwait(false);
                }
                if (!redirect) await stream.WriteAsync(_image, _stop.Token).ConfigureAwait(false);
                await stream.FlushAsync(_stop.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (_stop.IsCancellationRequested) { }
            catch (SocketException) when (_stop.IsCancellationRequested) { }
            catch (AuthenticationException) { } // A normal client rejects the test's untrusted certificate.
            catch (IOException) { } // A rejected TLS handshake can close the transport before an HTTP request.
        }
    }

    public async ValueTask DisposeAsync()
    {
        _stop.Cancel();
        _listener.Stop();
        await _serve.WaitAsync(TimeSpan.FromSeconds(5));
        _certificate?.Dispose();
        _key?.Dispose();
        _stop.Dispose();
    }
}
