using Microsoft.AspNetCore.Http;
using PostCare.core.DTOS.ChildDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostCare.core.Interfaces
{
    public interface IChildService
    {
        Task<ChildDto> CreateChildAsync(int userId, CreateChildDto dto);
        Task<List<ChildDto>> GetUserChildrenAsync(int userId);
        Task<ChildDto> GetChildByIdAsync(int childId, int userId);
        Task<ChildDto> UpdateChildAsync(int childId, int userId, UpdateChildDto dto);
        Task<bool> DeleteChildAsync(int childId, int userId);
        Task<string> UploadChildPhotoAsync(int childId, int userId, IFormFile photo);
        Task<bool> DeleteChildPhotoAsync(int childId, int userId);
    }
}
