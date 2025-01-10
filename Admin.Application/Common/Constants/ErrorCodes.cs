using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Admin.Application.Common.Constants;
public static class ErrorCodes
{
    // Authentication & Authorization
    public const string Unauthorized = "AUTH001";
    public const string InvalidCredentials = "AUTH002";
    public const string TokenExpired = "AUTH003";
    public const string InsufficientPermissions = "AUTH004";

    // Resource errors
    public const string ResourceNotFound = "RES001";
    public const string ResourceAlreadyExists = "RES002";
    public const string ResourceInUse = "RES003";

    // Validation errors
    public const string ValidationFailed = "VAL001";
    public const string InvalidInput = "VAL002";
    public const string RequiredFieldMissing = "VAL003";

    // Business rule violations
    public const string BusinessRuleViolation = "BUS001";
    public const string InsufficientStock = "BUS002";
    public const string OrderStatusInvalid = "BUS003";

    // System errors
    public const string InternalServerError = "SYS001";
    public const string DatabaseError = "SYS002";
    public const string ExternalServiceError = "SYS003";
    public const string CacheError = "SYS004";
}
