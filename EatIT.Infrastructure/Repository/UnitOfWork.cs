using AutoMapper;
using EatIT.Core.Interface;
using EatIT.Infrastructure.Data;
using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EatIT.Infrastructure.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDBContext _dbContext;

        private readonly IFileProvider _fileProvider;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public IUserRoleRepository UserRoleRepository {  get; }

        public IUserRepository UserRepository {  get; }

        public IDishRepository DishRepository { get; }

        public ITagRepository TagRepository { get; }

        public IRestaurantRepository RestaurantRepository { get; }


        public IRatingRepository RatingRepository { get; }
        
        public IFavoriteRepository FavoriteRepository { get; }

        public UnitOfWork(ApplicationDBContext context, IFileProvider fileProvider, IMapper mapper)
        {
            _dbContext = context;
            _fileProvider = fileProvider;
            _mapper = mapper;

            UserRoleRepository = new UserRoleRepository(_dbContext);
            UserRepository = new UserRepository(_dbContext, _fileProvider, _mapper);
            TagRepository = new TagRepository(_dbContext, _fileProvider, _mapper);
            DishRepository = new DishRepository(_dbContext, _fileProvider, _mapper);
            RestaurantRepository = new RestaurantRepository(_dbContext, _fileProvider, _mapper);
            RatingRepository = new RatingRepository(context, _fileProvider, _mapper);
            FavoriteRepository = new FavoriteRepository(context, _mapper);
        }
    }
}
