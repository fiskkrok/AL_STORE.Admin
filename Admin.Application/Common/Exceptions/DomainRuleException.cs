using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Admin.Application.Common.Exceptions;

public class DomainRuleException : AppException
{
    public DomainRuleException(string rule, string message)
        : base($"DomainRule.{rule}", message)
    {
    }
}
