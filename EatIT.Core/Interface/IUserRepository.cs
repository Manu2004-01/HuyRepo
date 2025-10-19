using EatIT.Core.Entities;
using EatIT.Core.Sharing;
using EatIT.Infrastructure.Data;
using EatIT.Infrastructure.Data.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EatIT.Core.Interface
{
    public interface IUserRepository : IGenericRepository<Users>
    {
        Task<IEnumerable<Users>> GetAllAsync(UserParams userParams);
        Task<bool> AddAsync(CreateUserDTO dto);
        Task<bool> UpdateAsync(int id, UpdateUserDTO dto);
        Task<bool> UpdateProfileAsync(int id, UpdateUserProfileDTO dto);
        Task<bool> DeleteAsync(int id);
        Task<bool> UpdateUserLocationAsync(int userId, UpdateUserLocationDTO locationDto);
        Task<UserLocationResponseDTO?> GetUserLocationAsync(int userId);
    }
}
