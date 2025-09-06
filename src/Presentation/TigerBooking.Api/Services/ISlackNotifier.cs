namespace TigerBooking.Api.Services;

public interface ISlackNotifier
{
    Task SendAsync(string title, string message, CancellationToken cancellationToken = default);
}
