using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EatIT.Core.Sharing
{
    public class DishParams
    {
        //Sorting
        public string Sorting { get; set; }

        //Filtering by Price
        public decimal? DishPrice { get; set; }

        //Search
        private string _search;

        public string Search
        {
            get => _search;
            set => _search = value?.ToLower();
        }
    }
}
