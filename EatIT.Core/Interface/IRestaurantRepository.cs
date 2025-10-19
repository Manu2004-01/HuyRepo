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
    public interface IRestaurantRepository : IGenericRepository<Restaurants>
    {
        Task<IEnumerable<Restaurants>> GetAllAsync(RestaurantParams restaurantParams);
        Task<bool> AddAsync(CreateRestaurantDTO dto);
        Task<bool> UpdateAsync(int id, UpdateRestaurantDTO dto);
        Task<bool> DeleteAsync(int id);
    }
}
