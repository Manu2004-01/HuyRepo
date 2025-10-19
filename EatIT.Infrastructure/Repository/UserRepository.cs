using AutoMapper;
using EatIT.Core.Entities;
using EatIT.Core.Interface;
using EatIT.Core.Sharing;
using EatIT.Infrastructure.Data;
using EatIT.Infrastructure.Data.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EatIT.Infrastructure.Repository
{
    public class UserRepository : GenericRepository<Users>, IUserRepository
    {
        private readonly ApplicationDBContext _context;
        private readonly IFileProvider _fileProvider;
        private readonly IMapper _mapper;

        public UserRepository(ApplicationDBContext context, IFileProvider fileProvider, IMapper mapper) : base(context)
        {
            _context = context;
            _fileProvider = fileProvider;
            _mapper = mapper;
        }

        //Create new user
        public async Task<bool> AddAsync(CreateUserDTO dto)
        {
            var user = _mapper.Map<Users>(dto);
            
            // Only process image if it's provided
            if (dto.image != null)
            {
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(dto.image.FileName)}";
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
                    await dto.image.CopyToAsync(fs);
                }

                user.UserImg = "/" + relativePath.Replace("\\", "/");
            }
            else
            {
                // Set a default image path or null if no image is provided
                user.UserImg = null; // or set a default image path
            }

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return true;
        }

        //Update user (Admin only)
        public async Task<bool> UpdateAsync(int id, UpdateUserDTO dto)
        {
            var currentUser = await _context.Users.FindAsync(id);
            if (currentUser == null) return false;

            currentUser.UserName = dto.UserName;
            currentUser.UserAddress = dto.UserAddress;
            currentUser.Password = dto.Password;
            currentUser.Email = dto.Email;
            currentUser.PhoneNumber = dto.PhoneNumber;

            if (dto.image != null)
            {
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(dto.image.FileName)}";
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
                    await dto.image.CopyToAsync(fs);
                }

                // Remove old image if exists
                if (!string.IsNullOrEmpty(currentUser.UserImg))
                {
                    var oldRelative = currentUser.UserImg.TrimStart('/');
                    var oldInfo = _fileProvider.GetFileInfo(oldRelative);
                    var oldPhysical = oldInfo.PhysicalPath;
                    if (File.Exists(oldPhysical)) File.Delete(oldPhysical);
                }

                currentUser.UserImg = "/" + relativePath.Replace("\\", "/");
            }

            // RoleId remains unchanged (ignored by mapping)
            currentUser.UpdateAt = DateTime.UtcNow;

            _context.Users.Update(currentUser);
            await _context.SaveChangesAsync();
            return true;
        }

        //Update user profile (User can update their own profile)
        public async Task<bool> UpdateProfileAsync(int id, UpdateUserProfileDTO dto)
        {
            var currentUser = await _context.Users.FindAsync(id);
            if (currentUser == null) return false;

            // Only update allowed fields: UserName, PhoneNumber, UserAddress, and UserImg
            currentUser.UserName = dto.UserName;
            currentUser.PhoneNumber = dto.PhoneNumber;
            currentUser.UserAddress = dto.UserAddress;

            // Handle image update if provided
            if (dto.image != null)
            {
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(dto.image.FileName)}";
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
                    await dto.image.CopyToAsync(fs);
                }

                // Remove old image if exists
                if (!string.IsNullOrEmpty(currentUser.UserImg))
                {
                    var oldRelative = currentUser.UserImg.TrimStart('/');
                    var oldInfo = _fileProvider.GetFileInfo(oldRelative);
                    var oldPhysical = oldInfo.PhysicalPath;
                    if (File.Exists(oldPhysical)) File.Delete(oldPhysical);
                }

                currentUser.UserImg = "/" + relativePath.Replace("\\", "/");
            }

            // Update timestamp
            currentUser.UpdateAt = DateTime.UtcNow;

            _context.Users.Update(currentUser);
            await _context.SaveChangesAsync();
            return true;
        }

        //Delete User
        public async Task<bool> DeleteAsync(int id)
        {
            var currentuser = await _context.Users.FindAsync(id);
            if (!string.IsNullOrEmpty(currentuser.UserImg))
            {
                var pic_info = _fileProvider.GetFileInfo(currentuser.UserImg);
                var root_path = pic_info.PhysicalPath;
                System.IO.File.Delete($"{root_path}");

                //Delete Db
                _context.Users.Remove(currentuser);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        //Get users list
        public async Task<IEnumerable<Users>> GetAllAsync(UserParams userParams)
        {
            //Sorting
            var queryable = _context.Users
                .Include(x => x.Role)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrEmpty(userParams.Sorting))
            {
                queryable = userParams.Sorting switch
                {
                    "createdAt_desc" => queryable.OrderByDescending(x => x.CreateAt),
                    "createdAt_asc" => queryable.OrderBy(x => x.CreateAt),

                    "userName_asc" => queryable.OrderBy(x => x.UserName),
                    "userName_desc" => queryable.OrderByDescending(x => x.UserName),

                    "role_asc" => queryable.OrderBy(x => x.Role.RoleName).ThenBy(x => x.UserName),
                    "role_desc" => queryable.OrderByDescending(x => x.Role.RoleName).ThenBy(x => x.UserName),

                    "isActive_desc" => queryable.OrderByDescending(x => x.IsActive).ThenBy(x => x.UserName),
                    "isActive_asc" => queryable.OrderBy(x => x.IsActive).ThenBy(x => x.UserName),

                    "updatedAt_desc" => queryable.OrderByDescending(x => x.UpdateAt),
                    "updatedAt_asc" => queryable.OrderBy(x => x.UpdateAt),

                    "id_asc" => queryable.OrderBy(x => x.Id),
                    "id_desc" => queryable.OrderByDescending(x => x.Id),

                    _ => queryable.OrderByDescending(x => x.CreateAt)
                };
            }
            else
            {
                queryable = queryable.OrderByDescending(x => x.CreateAt);
            }

            //Filter by Role Id
            if (userParams.Roleid.HasValue)
            {
                queryable = queryable.Where(x => x.RoleId == userParams.Roleid.Value);
            }

            //Page Size
            queryable = queryable.Skip((userParams.Pagesize) * (userParams.Pagenumber - 1)).Take(userParams.Pagesize);

            //Search
            if (!string.IsNullOrEmpty(userParams.Search))
            {
                queryable = queryable.Where(x => x.UserName.ToLower().Contains(userParams.Search));
            }

            var list = await queryable.ToListAsync();
            return list;
        }

        public async Task<bool> UpdateUserLocationAsync(int userId, UpdateUserLocationDTO locationDto)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return false; // User không tồn tại
                }

                // Cập nhật tọa độ và thời gian
                user.UserLatitude = locationDto.UserLatitude;
                user.UserLongitude = locationDto.UserLongitude;
                user.LastLocationUpdate = DateTime.UtcNow;
                user.UpdateAt = DateTime.UtcNow; // Cập nhật thời gian sửa đổi

                // Lưu thay đổi
                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception)
            {
                return false; // Có lỗi xảy ra
            }
        }

        public async Task<UserLocationResponseDTO> GetUserLocationAsync(int userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null || user.UserLatitude == null || user.UserLongitude == null)
                {
                    return null;
                }

                return new UserLocationResponseDTO
                {
                    UserLatitude = user.UserLatitude.Value,
                    UserLongitude = user.UserLongitude.Value,
                    LastLocationUpdate = user.LastLocationUpdate ?? DateTime.MinValue
                };
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
