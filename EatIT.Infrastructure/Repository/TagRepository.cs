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
    public class TagRepository : GenericRepository<Tags>, ITagRepository
    {
        private readonly ApplicationDBContext _context;
        private readonly IFileProvider _fileProvider;
        private readonly IMapper _mapper;

        public TagRepository(ApplicationDBContext context, IFileProvider fileProvider, IMapper mapper) : base(context)
        {
            _context = context;
            _fileProvider = fileProvider;
            _mapper = mapper;
        }

        public async Task<bool> AddAsync(CreateTagDTO dto)
        {
            if (dto.timage == null) return false;

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(dto.timage.FileName)}";
            var relativePath = Path.Combine("images", "tag", fileName);

            var fileInfo = _fileProvider.GetFileInfo(relativePath);
            var physicalPath = fileInfo.PhysicalPath;
            var dir = Path.GetDirectoryName(physicalPath);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            using (var fs = new FileStream(physicalPath, FileMode.Create))
            {
                await dto.timage.CopyToAsync(fs);
            }

            var tag = _mapper.Map<Tags>(dto);
            tag.TagImg = "/" + relativePath.Replace("\\", "/");
            await _context.Tags.AddAsync(tag);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateAsync(int id, UpdateTagDTO dto)
        {
            var currenttag = await _context.Tags.FindAsync(id);
            if (currenttag == null) return false;

            // Map scalar fields but keep RoleId and UserImg
            _mapper.Map(dto, currenttag);

            if (dto.timage != null)
            {
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(dto.timage.FileName)}";
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
                    await dto.timage.CopyToAsync(fs);
                }

                // Remove old image if exists
                if (!string.IsNullOrEmpty(currenttag.TagImg))
                {
                    var oldRelative = currenttag.TagImg.TrimStart('/');
                    var oldInfo = _fileProvider.GetFileInfo(oldRelative);
                    var oldPhysical = oldInfo.PhysicalPath;
                    if (File.Exists(oldPhysical)) File.Delete(oldPhysical);
                }

                currenttag.TagImg = "/" + relativePath.Replace("\\", "/");
            }

            // RoleId remains unchanged (ignored by mapping)
            //currenttag. = DateTime.UtcNow;

            _context.Tags.Update(currenttag);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var currenttag = await _context.Tags.FindAsync(id);
            if (!string.IsNullOrEmpty(currenttag.TagImg))
            {
                var pic_info = _fileProvider.GetFileInfo(currenttag.TagImg);
                var root_path = pic_info.PhysicalPath;
                System.IO.File.Delete($"{root_path}");

                //Delete Db
                _context.Tags.Remove(currenttag);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<IEnumerable<Tags>> GetAllAsync()
        {
            var getall = await _context.Tags.ToListAsync();
            return getall;
        }
    }
}
