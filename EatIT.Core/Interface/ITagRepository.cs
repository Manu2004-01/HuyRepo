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
    public interface ITagRepository : IGenericRepository<Tags>
    {
        Task<IEnumerable<Tags>> GetAllAsync();
        Task<bool> AddAsync(CreateTagDTO dto);
        Task<bool> UpdateAsync(int id, UpdateTagDTO dto);
        Task<bool> DeleteAsync(int id);
    }
}
