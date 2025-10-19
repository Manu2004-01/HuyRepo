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
    public class DishRepository : GenericRepository<Dishes>, IDishRepository
    {
        private readonly ApplicationDBContext _context;
        private readonly IFileProvider _fileProvider;
        private readonly IMapper _mapper;

        public DishRepository(ApplicationDBContext context, IFileProvider fileProvider, IMapper mapper) : base(context)
        {
            _context = context;
            _fileProvider = fileProvider;
            _mapper = mapper;
        }

        public async Task<bool> AddAsync(CreateDishDTO dto)
        {
            if (dto.dimage == null) return false;

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(dto.dimage.FileName)}";
            var relativeDir = Path.Combine("images", "dishes");

            var physicalRoot = (_fileProvider as PhysicalFileProvider)?.Root
                ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

            var dir = Path.Combine(physicalRoot, relativeDir);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var physicalPath = Path.Combine(dir, fileName);

            using (var fs = new FileStream(physicalPath, FileMode.Create))
            {
                await dto.dimage.CopyToAsync(fs);
            }

            var dish = _mapper.Map<Dishes>(dto);
            dish.DishImage = "/" + Path.Combine(relativeDir, fileName).Replace("\\", "/");
            await _context.Dishes.AddAsync(dish);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Dishes>> GetAllAsync(DishParams dishParams)
        {
            //Sorting
            var queryable = _context.Dishes
                .Include(x => x.Restaurant)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrEmpty(dishParams.Sorting))
            {
                queryable = dishParams.Sorting switch
                {
                    "price_desc" => queryable.OrderByDescending(x => x.DishPrice),
                    "price_asc" => queryable.OrderBy(x => x.DishPrice),
                    _ => queryable.OrderByDescending(x => x.Id)
                };
            }
            else
            {
                queryable = queryable.OrderByDescending(x => x.Id);
            }

            //Filter by Dish price
            if (dishParams.DishPrice.HasValue)
            {
                queryable = queryable.Where(x => x.DishPrice == dishParams.DishPrice.Value);
            }

            //Search
            if (!string.IsNullOrEmpty(dishParams.Search))
            {
                queryable = queryable.Where(x => x.DishName.ToLower().Contains(dishParams.Search));
            }

            var list = await queryable.ToListAsync();
            return list;
        }

        public async Task<bool> UpdateAsync(int id, UpdateDishDTO dto)
        {
            var currentDish = await _context.Dishes.FindAsync(id);
            if (currentDish == null) return false;

            _mapper.Map(dto, currentDish);
            if (dto.dimage != null)
            {
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(dto.dimage.FileName)}";
                var relativeDir = Path.Combine("images", "dishes");
                var physicalRoot = (_fileProvider as PhysicalFileProvider)?.Root
                    ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var dir = Path.Combine(physicalRoot, relativeDir);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                var physicalPath = Path.Combine(dir, fileName);

                using (var fs = new FileStream(physicalPath, FileMode.Create))
                {
                    await dto.dimage.CopyToAsync(fs);
                }

                // xóa ảnh cũ nếu có
                if (!string.IsNullOrEmpty(currentDish.DishImage))
                {
                    var oldRelative = currentDish.DishImage.TrimStart('/'); // images/dishes/xxx.jpg
                    var oldPhysical = Path.Combine(physicalRoot, oldRelative);
                    if (File.Exists(oldPhysical)) File.Delete(oldPhysical);
                }

                currentDish.DishImage = "/" + Path.Combine(relativeDir, fileName).Replace("\\", "/");
            }

            // RoleId remains unchanged (ignored by mapping)
            currentDish.UpdateAt = DateTime.UtcNow;

            _context.Dishes.Update(currentDish);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var currentDish = await _context.Dishes.FindAsync(id);
            if (!string.IsNullOrEmpty(currentDish.DishImage))
            {
                var pic_info = _fileProvider.GetFileInfo(currentDish.DishImage);
                var root_path = pic_info.PhysicalPath;
                System.IO.File.Delete($"{root_path}");

                //Delete Db
                _context.Dishes.Remove(currentDish);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }
    }
}
