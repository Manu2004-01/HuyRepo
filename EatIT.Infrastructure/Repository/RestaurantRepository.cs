using AutoMapper;
using EatIT.Core.DTOs;
using EatIT.Core.Entities;
using EatIT.Core.Interface;
using EatIT.Core.Sharing;
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
    public class RestaurantRepository : GenericRepository<Restaurants>, IRestaurantRepository
    {
        private readonly ApplicationDBContext _context;
        private readonly IFileProvider _fileProvider;
        private readonly IMapper _mapper;

        public RestaurantRepository(ApplicationDBContext context, IFileProvider fileProvider, IMapper mapper) : base(context)
        {
            _context = context;
            _fileProvider = fileProvider;
            _mapper = mapper;
        }

        public async Task<bool> AddAsync(CreateRestaurantDTO dto)
        {
            if (dto.rimage == null) return false;

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(dto.rimage.FileName)}";
            var relativePath = Path.Combine("images", "restaurants", fileName);

            var fileInfo = _fileProvider.GetFileInfo(relativePath);
            var physicalPath = fileInfo.PhysicalPath;
            var dir = Path.GetDirectoryName(physicalPath);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            using (var fs = new FileStream(physicalPath, FileMode.Create))
            {
                await dto.rimage.CopyToAsync(fs);
            }

            var restaurant = _mapper.Map<Restaurants>(dto);
            restaurant.RestaurantImg = "/" + relativePath.Replace("\\", "/");
            
            await _context.Restaurants.AddAsync(restaurant);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Restaurants>> GetAllAsync(RestaurantParams restaurantParams)
        {
            var queryable = _context.Restaurants
                .Include(x => x.Tag)
                .Include(x => x.User)
                .AsNoTracking()
                .AsQueryable();
            //Search
            if (!string.IsNullOrEmpty(restaurantParams.Search))
            {
                queryable = queryable.Where(x => x.ResName.ToLower().Contains(restaurantParams.Search));
            }

            var list = await queryable.ToListAsync();
            return list;
        }

        public async Task<bool> UpdateAsync(int id, UpdateRestaurantDTO dto)
        {
            var currentRestaurant = await _context.Restaurants.FindAsync(id);
            if (currentRestaurant == null) return false;

            // Map scalar fields but keep RoleId and UserImg
            _mapper.Map(dto, currentRestaurant);

            if (dto.rimage != null)
            {
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(dto.rimage.FileName)}";
                var relativePath = Path.Combine("images", "user", fileName);

                var fileInfo = _fileProvider.GetFileInfo(relativePath);
                var physicalPath = fileInfo.PhysicalPath;
                var dir = Path.GetDirectoryName(physicalPath);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                using (var fs = new FileStream(physicalPath, FileMode.Create))
                {
                    await dto.rimage.CopyToAsync(fs);
                }

                // Remove old image if exists
                if (!string.IsNullOrEmpty(currentRestaurant.RestaurantImg))
                {
                    var oldRelative = currentRestaurant.RestaurantImg.TrimStart('/');
                    var oldInfo = _fileProvider.GetFileInfo(oldRelative);
                    var oldPhysical = oldInfo.PhysicalPath;
                    if (File.Exists(oldPhysical)) File.Delete(oldPhysical);
                }

                currentRestaurant.RestaurantImg = "/" + relativePath.Replace("\\", "/");
            }

            _context.Restaurants.Update(currentRestaurant);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var currentRestaurant = await _context.Restaurants.FindAsync(id);
            if (!string.IsNullOrEmpty(currentRestaurant.RestaurantImg))
            {
                var pic_info = _fileProvider.GetFileInfo(currentRestaurant.RestaurantImg);
                var root_path = pic_info.PhysicalPath;
                System.IO.File.Delete($"{root_path}");

                //Delete Db
                _context.Restaurants.Remove(currentRestaurant);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }
    }
}
