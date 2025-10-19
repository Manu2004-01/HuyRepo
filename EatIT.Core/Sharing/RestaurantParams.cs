using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EatIT.Core.Sharing
{
    public class RestaurantParams
    {
        //Search
        private string _search;

        public string Search
        {
            get => _search;
            set => _search = value?.ToLower();
        }
    }
}
