using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Admin.Application.Common.Interfaces;
public interface IWebhookService
{
    Task RegisterWebhookAsync(string eventType, string callbackUrl, string secret);
    Task TriggerWebhooksAsync(string eventType, object payload);
}