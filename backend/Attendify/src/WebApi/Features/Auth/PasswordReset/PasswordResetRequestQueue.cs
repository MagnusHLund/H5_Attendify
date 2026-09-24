using System.Threading.Channels;

namespace Attendify.Features.Auth.PasswordReset;

public sealed class PasswordResetRequestQueue : IPasswordResetRequestQueue
{
    private const int Capacity = 256;

    private readonly Channel<string> _channel = Channel.CreateBounded<string>(
        new BoundedChannelOptions(Capacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false,
        }
    );

    public ValueTask EnqueueAsync(string email, CancellationToken cancellationToken) =>
        _channel.Writer.WriteAsync(email, cancellationToken);

    public IAsyncEnumerable<string> ReadAllAsync(CancellationToken cancellationToken) =>
        _channel.Reader.ReadAllAsync(cancellationToken);
}
