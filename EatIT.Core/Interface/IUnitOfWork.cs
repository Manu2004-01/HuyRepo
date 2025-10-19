using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EatIT.Core.Interface
{
     public interface IUnitOfWork
    {
        public IUserRoleRepository UserRoleRepository { get; }
        public IUserRepository UserRepository { get; }
        public IDishRepository DishRepository { get; }
        public ITagRepository TagRepository { get; }
        public IRestaurantRepository RestaurantRepository { get; }
        public IRatingRepository RatingRepository { get; }
        public IFavoriteRepository FavoriteRepository { get; }
    }
}
