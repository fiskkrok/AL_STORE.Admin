using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Admin.Application.Common.Interfaces;
public interface ITokenService
{
    Task<TokenResult> GenerateTokenAsync(string userId, string username, IEnumerable<string> roles, IEnumerable<string> permissions);
    Task<TokenResult> RefreshTokenAsync(string refreshToken);
    Task RevokeTokenAsync(string userId);
    Task<bool> ValidateTokenAsync(string token);
}
