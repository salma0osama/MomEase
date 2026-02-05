using MomEase.core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.Interfaces
{
    public interface IAuthRepository
    {
        // User Operations
        Task<Users?> GetUserByEmailAsync(string email);
        Task<Users?> GetUserByIdAsync(int userId);
        Task<bool> EmailExistsAsync(string email);
        Task AddUserAsync(Users user);
        Task UpdateUserAsync(Users user);

        // RefreshToken Operations
        Task<RefreshToken?> GetRefreshTokenAsync(string token);
        Task AddRefreshTokenAsync(RefreshToken refreshToken);
        Task UpdateRefreshTokenAsync(RefreshToken refreshToken);

        // PasswordReset Operations
        Task<PasswordResetToken?> GetPasswordResetTokenAsync(string token);
        Task<PasswordResetToken> GetPasswordResetTokenByUserIdAsync(int userId, string otpCode);

        Task AddPasswordResetTokenAsync(PasswordResetToken token);
        Task UpdatePasswordResetTokenAsync(PasswordResetToken token);

        // Save Changes
        Task<int> SaveChangesAsync();
    }
}
