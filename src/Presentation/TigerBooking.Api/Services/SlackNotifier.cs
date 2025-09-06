using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;

namespace TigerBooking.Api.Services;

public class SlackNotifier : ISlackNotifier
{
    private readonly HttpClient _httpClient;
    private readonly string? _webhookUrl;

    public SlackNotifier(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _webhookUrl = configuration.GetSection("Slack")?.GetValue<string>("WebhookUrl");
    }

    public async Task SendAsync(string title, string message, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(_webhookUrl)) return;

        var payload = new
        {
            text = $"*{title}*\n{message}"
        };

        try
        {
            await _httpClient.PostAsJsonAsync(_webhookUrl, payload, cancellationToken);
        }
        catch
        {
            // swallow - notifier should not crash the app
        }
    }
}
