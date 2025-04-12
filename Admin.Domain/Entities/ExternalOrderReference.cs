using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Admin.Domain.Entities;
public class ExternalOrderReference
{
    /// <summary>
    /// e.g., "Shopify", "WooCommerce"
    /// </summary>
    public string Source { get; private set; } 
    public string ExternalId { get; private set; }
    public DateTime ReceivedAt { get; private set; }
}