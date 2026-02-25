using MomEase.core.DTOS.AssessmentDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.Interfaces
{
    public interface IAssessmentService
    {
        Task<IEnumerable<AssessmentDto>> GetAllAsync();
        Task<AssessmentDto?> GetByIdAsync(int id);
        Task<AssessmentDto> CreateAsync(CreateAssessmentDto dto);
        Task<AssessmentDto?> UpdateAsync(int id, UpdateAssessmentDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
