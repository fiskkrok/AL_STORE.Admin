using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ardalis.GuardClauses;

namespace Admin.Domain.ValueObjects;

public class ShippingInfo : ValueObject
{
    public string Carrier { get; private set; }
    public string TrackingNumber { get; private set; }
    public DateTime EstimatedDeliveryDate { get; private set; }
    public DateTime? ActualDeliveryDate { get; private set; }

    public ShippingInfo(
        string carrier,
        string trackingNumber,
        DateTime estimatedDeliveryDate)
    {
        Guard.Against.NullOrWhiteSpace(carrier, nameof(carrier));
        Guard.Against.NullOrWhiteSpace(trackingNumber, nameof(trackingNumber));

        Carrier = carrier;
        TrackingNumber = trackingNumber;
        EstimatedDeliveryDate = estimatedDeliveryDate;
    }

    public void SetDelivered(DateTime deliveryDate)
    {
        ActualDeliveryDate = deliveryDate;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Carrier;
        yield return TrackingNumber;
        yield return EstimatedDeliveryDate;
        if (ActualDeliveryDate.HasValue)
            yield return ActualDeliveryDate.Value;
    }
}
