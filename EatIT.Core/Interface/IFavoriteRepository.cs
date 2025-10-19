using EatIT.Core.DTOs;
using EatIT.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EatIT.Core.Interface
{
    public interface IFavoriteRepository : IGenericRepository<Favorites>
    {
        Task<IEnumerable<Favorites>> GetAllAsync();
        Task<IEnumerable<Favorites>> GetFavoritesByUserIdAsync(int userId);
        Task<bool> AddAsync(CreateFavoriteDTO dto);
        Task<bool> UpdateAsync(int id, UpdateFavoriteDTO dto);
        Task<bool> DeleteAsync(int id);
    }
}