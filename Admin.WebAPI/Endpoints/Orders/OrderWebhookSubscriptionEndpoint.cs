//using Admin.Application.Common.Interfaces;

//using FastEndpoints;

//namespace Admin.WebAPI.Endpoints.Orders;

//public class OrderWebhookSubscriptionEndpoint : Endpoint<OrderWebhookSubscriptionRequest, IResult>
//{
//    private readonly IWebhookSubscriptionService _webhookService;

//    public override async Task HandleAsync(OrderWebhookSubscriptionRequest req, CancellationToken ct)
//    {
//        await _webhookService.RegisterWebhookAsync("OrderStatus", req.CallbackUrl, req.Secret);
//        await SendNoContentAsync(ct);
//    }
//}

//public record OrderWebhookSubscriptionRequest
//{
//    public object CallbackUrl { get; set; }
//    public object Secret { get; set; }
//}
