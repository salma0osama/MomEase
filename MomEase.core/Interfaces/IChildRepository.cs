using MomEase.core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.Interfaces
{
    public interface IChildRepository
    {
        // POST /api/children - إضافة طفل جديد
        Task<Child> AddChildAsync(Child child);

        // GET /api/children - الحصول على جميع أطفال المستخدم
        Task<List<Child>> GetUserChildrenAsync(int userId);

        // GET /api/children/{id} - الحصول على طفل معين
        Task<Child> GetChildByIdAsync(int childId);

        // PUT /api/children/{id} - تحديث بيانات طفل
        Task<Child> UpdateChildAsync(Child child);

        // DELETE /api/children/{id} - حذف طفل
        Task<bool> DeleteChildAsync(Child child);

        // POST /api/children/{id}/photo - تحديث صورة الطفل
        Task<Child> UpdateChildPhotoAsync(int childId, string photoUrl);

        // DELETE /api/children/{id}/photo - حذف صورة الطفل
        Task<Child> DeleteChildPhotoAsync(int childId);

        // Helper methods
        Task<bool> IsChildOwnedByUserAsync(int childId, int userId);
        Task<int> GetUserChildrenCountAsync(int userId);

    }
}
