using AutoMapper;
using EatIT.Core.DTOs;
using EatIT.Core.Entities;
using EatIT.Core.Interface;
using EatIT.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EatIT.Infrastructure.Repository
{
    public class FavoriteRepository : GenericRepository<Favorites>, IFavoriteRepository
    {
        private readonly ApplicationDBContext _context;
        private readonly IMapper _mapper;

        public FavoriteRepository(ApplicationDBContext context, IMapper mapper) : base(context)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<bool> AddAsync(CreateFavoriteDTO dto)
        {
            try
            {
                // Xử lý trường hợp dishid = 0 (coi như null)
                if (dto.dishid.HasValue && dto.dishid.Value == 0)
                {
                    dto.dishid = null;
                }
                
                // Kiểm tra nếu dishid null thì trả về false
                if (!dto.dishid.HasValue)
                {
                    return false;
                }
                
                // Kiểm tra sự tồn tại của dish nếu dishid có giá trị
                if (dto.dishid.HasValue)
                {
                    var dish = await _context.Dishes.FindAsync(dto.dishid.Value);
                    if (dish == null)
                    {
                        return false; // Dish không tồn tại
                    }
                }
                
                // Kiểm tra sự tồn tại của restaurant
                var restaurant = await _context.Restaurants.FindAsync(dto.restaurantid);
                if (restaurant == null)
                {
                    return false; // Restaurant không tồn tại
                }
                
                // Kiểm tra sự tồn tại của user
                var user = await _context.Users.FindAsync(dto.userid);
                if (user == null)
                {
                    return false; // User không tồn tại
                }
                
                var favorite = _mapper.Map<Favorites>(dto);
                await _context.Favorites.AddAsync(favorite);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                // Log lỗi chi tiết
                Console.WriteLine($"Error in AddAsync: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                throw; // Ném lại ngoại lệ để xử lý ở tầng controller
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var favorite = await _context.Favorites.FindAsync(id);
            if (favorite == null) return false;

            _context.Favorites.Remove(favorite);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Favorites>> GetAllAsync()
        {
            return await _context.Favorites
                .Include(x => x.User)
                .Include(x => x.Dish)
                .Include(x => x.Restaurant)
                .ToListAsync();
        }

        public async Task<IEnumerable<Favorites>> GetFavoritesByUserIdAsync(int userId)
        {
            return await _context.Favorites
                .Where(x => x.UserId == userId)
                .Include(x => x.User)
                .Include(x => x.Dish)
                .Include(x => x.Restaurant)
                .ToListAsync();
        }

        public async Task<bool> UpdateAsync(int id, UpdateFavoriteDTO dto)
        {
            var favorite = await _context.Favorites.FindAsync(id);
            if (favorite == null) return false;

            _mapper.Map(dto, favorite);

            _context.Favorites.Update(favorite);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}