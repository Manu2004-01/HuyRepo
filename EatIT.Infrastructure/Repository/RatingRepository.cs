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
    public class RatingRepository : GenericRepository<Rating>, IRatingRepository
    {
        private readonly ApplicationDBContext _context;
        private readonly IFileProvider _fileProvider;
        private readonly IMapper _mapper;

        public RatingRepository(ApplicationDBContext context, IFileProvider fileProvider, IMapper mapper) : base(context) 
        {
            _context = context;
            _fileProvider = fileProvider;
            _mapper = mapper;
        }

        public async Task<bool> AddAsync(CreateRatingDTO dto)
        {
            var rating = _mapper.Map<Rating>(dto);
            rating.CreateAt = DateTime.UtcNow;
            await _context.Ratings.AddAsync(rating);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Rating>> GetAllAsync(RatingParams ratingParams)
        {
            var queryable = _context.Ratings
                .Include(x => x.User)
                .Include(x => x.Restaurant)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrEmpty(ratingParams.Sorting))
            {
                queryable = ratingParams.Sorting switch
                {
                    "createdAt_desc" => queryable.OrderByDescending(x => x.CreateAt),
                    "createdAt_asc" => queryable.OrderBy(x => x.CreateAt),
                    _ => queryable.OrderByDescending(x => x.CreateAt)
                };
            }
            else
            {
                queryable = queryable.OrderByDescending(x => x.CreateAt);
            }

            var list = await queryable.ToListAsync();
            return list;
        }

        public async Task<IEnumerable<Rating>> GetByRestaurantAsync(int restaurantId, RatingParams ratingParams)
        {
            var queryable = _context.Ratings
                .Include(x => x.User)
                .Include(x => x.Restaurant)
                .AsNoTracking()
                .Where(x => x.RestaurantId == restaurantId)
                .AsQueryable();

            if (!string.IsNullOrEmpty(ratingParams.Sorting))
            {
                queryable = ratingParams.Sorting switch
                {
                    "createdAt_desc" => queryable.OrderByDescending(x => x.CreateAt),
                    "createdAt_asc" => queryable.OrderBy(x => x.CreateAt),
                    _ => queryable.OrderByDescending(x => x.CreateAt)
                };
            }
            else
            {
                queryable = queryable.OrderByDescending(x => x.CreateAt);
            }

            return await queryable.ToListAsync();
        }

        public async Task<bool> UpdateAsync(int id, UpdateRatingDTO dto)
        {
            var currentRating = await _context.Ratings.FindAsync(id);
            if (currentRating == null) return false;

            _mapper.Map(dto, currentRating);

            _context.Ratings.Update(currentRating);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var currentRating = await _context.Ratings.FindAsync(id);
            if (currentRating == null) return false;
            _context.Ratings.Remove(currentRating);
            await _context.SaveChangesAsync(); 
            return true;
        }
    }
}
