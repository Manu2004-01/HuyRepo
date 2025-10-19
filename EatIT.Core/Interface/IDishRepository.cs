using EatIT.Core.DTOs;
using EatIT.Core.Entities;
using EatIT.Core.Sharing;
using EatIT.Infrastructure.Data.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EatIT.Core.Interface
{
    public interface IDishRepository : IGenericRepository<Dishes>
    {
        Task<IEnumerable<Dishes>> GetAllAsync(DishParams dishParams);
        Task<bool> AddAsync(CreateDishDTO dto);
        Task<bool> UpdateAsync(int id, UpdateDishDTO dto);
        Task<bool> DeleteAsync(int id);
    }
}
