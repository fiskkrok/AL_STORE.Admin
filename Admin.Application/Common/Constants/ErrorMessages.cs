using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Admin.Application.Common.Constants;
public static class ErrorMessages
{
    public static string GetMessage(string errorCode) => errorCode switch
    {
        ErrorCodes.Unauthorized => "Unauthorized access",
        ErrorCodes.InvalidCredentials => "Invalid credentials provided",
        ErrorCodes.TokenExpired => "Authentication token has expired",
        ErrorCodes.InsufficientPermissions => "Insufficient permissions to perform this action",

        ErrorCodes.ResourceNotFound => "Requested resource was not found",
        ErrorCodes.ResourceAlreadyExists => "Resource already exists",
        ErrorCodes.ResourceInUse => "Resource is currently in use",

        ErrorCodes.ValidationFailed => "Validation failed",
        ErrorCodes.InvalidInput => "Invalid input provided",
        ErrorCodes.RequiredFieldMissing => "Required field is missing",

        ErrorCodes.BusinessRuleViolation => "Business rule violation",
        ErrorCodes.InsufficientStock => "Insufficient stock available",
        ErrorCodes.OrderStatusInvalid => "Invalid order status transition",

        ErrorCodes.InternalServerError => "An internal server error occurred",
        ErrorCodes.DatabaseError => "Database operation failed",
        ErrorCodes.ExternalServiceError => "External service call failed",
        ErrorCodes.CacheError => "Cache operation failed",

        _ => "An unexpected error occurred"
    };
}
