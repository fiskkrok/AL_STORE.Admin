namespace Admin.Application.Common.Interfaces;

public interface IWebhookSubscriptionService
{
    Task RegisterWebhookAsync(string orderstatus, object callbackUrl, object secret);
    Task TriggerWebhooksAsync(string orderstatus, object o);
}