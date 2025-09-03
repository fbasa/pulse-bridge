namespace PulseBridge.Worker;

public interface IEmailSender { Task SendAsync(string to, string subject, string body, CancellationToken ct); }

public sealed class SendEmailJobHandler(IEmailSender email) : IJobHandler
{
    public string JobType => "send-email";

    public async Task HandleAsync(long jobId, string payload, CancellationToken ct)
    {
        // parse payload JSON to fields (pseudo)
        // var dto = JsonSerializer.Deserialize<EmailDto>(payload)!;
        await email.SendAsync("user@example.com", "Hi", payload, ct);
    }
}