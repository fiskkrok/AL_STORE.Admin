using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Admin.Domain.ValueObjects;
public class ValueObject
{
    protected virtual IEnumerable<object> GetEqualityComponents()
    {
        throw new NotImplementedException();
    }
}
