using MomEase.core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.Interfaces
{
    public interface IJwtService
    {
        string GenerateAccessToken(Users user);
        string GenerateRefreshToken();
        Task<RefreshToken> CreateRefreshTokenAsync(int userId, string token, string ipAddress);
        Task<RefreshToken> GetRefreshTokenAsync(string token);
        Task RevokeRefreshTokenAsync(string token, string ipAddress);
        Task<bool> ValidateRefreshTokenAsync(string token);
    }
}
