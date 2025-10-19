using EatIT.Core.DTOs;
using EatIT.Core.Entities;
using EatIT.Core.Sharing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EatIT.Core.Interface
{
    public interface IRatingRepository : IGenericRepository<Rating>
    {
        Task<IEnumerable<Rating>> GetAllAsync(RatingParams ratingParams);
        Task<bool> AddAsync(CreateRatingDTO dto);
        Task<bool> UpdateAsync(int id, UpdateRatingDTO dto);
        Task<bool> DeleteAsync(int id);
    }
}
