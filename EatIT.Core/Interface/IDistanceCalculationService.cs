using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EatIT.Core.Interface
{
    public interface IDistanceCalculationService
    {
        double CalculateDistance(double lat1, double lon1, double lat2, double lon2);
        double CalculateDistanceInKm(double lat1, double lon1, double lat2, double lon2);
    }
}
