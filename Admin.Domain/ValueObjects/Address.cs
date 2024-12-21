using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Admin.Domain.ValueObjects;
public class Address : ValueObject
{
    public string Street { get; private set; }
    public string City { get; private set; }
    public string State { get; private set; }
    public string PostalCode { get; private set; }
    public string Country { get; private set; }
    public Address(
        string street,
        string city,
        string state,
        string postalCode,
        string country)
    {
        Street = street;
        City = city;
        State = state;
        PostalCode = postalCode;
        Country = country;
    }
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return State;
        yield return PostalCode;
        yield return Country;
    }
}
