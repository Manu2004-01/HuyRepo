using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EatIT.Core.DTOs
{
    public class FoodSuggestionDTO
    {
        /// <summary>
        /// Bán kính tìm kiếm nhà hàng (km). Mặc định là 5 km nếu không được cung cấp.
        /// </summary>
        public double? RadiusKm { get; set; }
    }
}

