using MomEase.core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.Interfaces
{
    /// <summary>
    /// Repository interface for Mother Profile operations
    /// </summary>
    public interface IMotherProfileRepository
    {
        /// <summary>
        /// Get mother profile by ID
        /// </summary>
        Task<MotherProfile?> GetByIdAsync(int motherId);

        /// <summary>
        /// Get mother profile by User ID
        /// </summary>
        Task<MotherProfile?> GetByUserIdAsync(int userId);

        /// <summary>
        /// Get mother profile with user data
        /// </summary>
        Task<MotherProfile?> GetByUserIdWithUserAsync(int userId);

        /// <summary>
        /// Check if user has mother profile
        /// </summary>
        Task<bool> ExistsByUserIdAsync(int userId);

        /// <summary>
        /// Add new mother profile
        /// </summary>
        Task<MotherProfile> AddAsync(MotherProfile motherProfile);

        /// <summary>
        /// Update mother profile
        /// </summary>
        Task UpdateAsync(MotherProfile motherProfile);

        /// <summary>
        /// Delete mother profile
        /// </summary>
        Task DeleteAsync(MotherProfile motherProfile);

        /// <summary>
        /// Save changes to database
        /// </summary>
        Task<int> SaveChangesAsync();
    }
}
