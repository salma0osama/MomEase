using MomEase.core.Entities;
using MomEase.core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.Interfaces
{
    public interface IDiseaseRepository
    {
        Task<List<Diseases>> GetAllAsync();
        Task<Diseases> GetByIdAsync(int id);
        Task<Diseases> GetByNameAsync(SkinAnalysisDiseaseName name);
    }
}
