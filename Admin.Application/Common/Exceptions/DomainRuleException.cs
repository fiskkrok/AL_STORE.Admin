namespace Admin.Application.Common.Exceptions;

public class DomainRuleException : AppException
{
    public DomainRuleException(string rule, string message)
        : base($"DomainRule.{rule}", message)
    {
    }
}
