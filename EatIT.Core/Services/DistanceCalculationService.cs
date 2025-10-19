using EatIT.Core.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EatIT.Core.Services
{
    public class DistanceCalculationService : IDistanceCalculationService
    {
        private const double EarthRadiusKm = 6371.0;
        
        public double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            return CalculateDistanceInKm(lat1, lon1, lat2, lon2);
        }

        public double CalculateDistanceInKm(double lat1, double lon1, double lat2, double lon2)
        {
            // Sử dụng Haversine formula với độ chính xác cao hơn
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            var distance = EarthRadiusKm * c;
            
            // Trả về với độ chính xác cao hơn (3 chữ số thập phân)
            return Math.Round(distance, 3);
        }

        // Vincenty formula cho độ chính xác cao hơn (đã sửa lỗi scope)
        public double CalculateDistanceVincenty(double lat1, double lon1, double lat2, double lon2)
        {
            // Vincenty formula cho độ chính xác cao hơn
            var a = 6378137.0; // Bán kính trục lớn (m)
            var b = 6356752.314245; // Bán kính trục nhỏ (m)
            var f = (a - b) / a; // Độ dẹt

            var L = ToRadians(lon2 - lon1);
            var U1 = Math.Atan((1 - f) * Math.Tan(ToRadians(lat1)));
            var U2 = Math.Atan((1 - f) * Math.Tan(ToRadians(lat2)));

            var sinU1 = Math.Sin(U1);
            var cosU1 = Math.Cos(U1);
            var sinU2 = Math.Sin(U2);
            var cosU2 = Math.Cos(U2);

            double lambda = L;
            double lambdaP;
            int iterLimit = 100;

            // Khai báo tất cả biến cần thiết
            double sinLambda, cosLambda, sinSigma, cosSigma, sigma, sinAlpha, cos2Alpha, cos2SigmaM;
            double uSquared, A, B, deltaSigma, s;

            do
            {
                sinLambda = Math.Sin(lambda);
                cosLambda = Math.Cos(lambda);
                sinSigma = Math.Sqrt((cosU2 * sinLambda) * (cosU2 * sinLambda) +
                                   (cosU1 * sinU2 - sinU1 * cosU2 * cosLambda) * (cosU1 * sinU2 - sinU1 * cosU2 * cosLambda));

                if (sinSigma == 0) return 0; // Co-incident points

                cosSigma = sinU1 * sinU2 + cosU1 * cosU2 * cosLambda;
                sigma = Math.Atan2(sinSigma, cosSigma);
                sinAlpha = cosU1 * cosU2 * sinLambda / sinSigma;
                cos2Alpha = 1 - sinAlpha * sinAlpha;
                cos2SigmaM = cosSigma - 2 * sinU1 * sinU2 / cos2Alpha;

                if (double.IsNaN(cos2SigmaM)) cos2SigmaM = 0; // Equatorial line

                var C = f / 16 * cos2Alpha * (4 + f * (4 - 3 * cos2Alpha));
                lambdaP = lambda;
                lambda = L + (1 - C) * f * sinAlpha * (sigma + C * sinSigma * (cos2SigmaM + C * cosSigma * (-1 + 2 * cos2SigmaM * cos2SigmaM)));
            } while (Math.Abs(lambda - lambdaP) > 1e-12 && --iterLimit > 0);

            if (iterLimit == 0) return double.NaN; // Formula failed to converge

            uSquared = cos2Alpha * (a * a - b * b) / (b * b);
            A = 1 + uSquared / 16384 * (4096 + uSquared * (-768 + uSquared * (320 - 175 * uSquared)));
            B = uSquared / 1024 * (256 + uSquared * (-128 + uSquared * (74 - 47 * uSquared)));
            deltaSigma = B * sinSigma * (cos2SigmaM + B / 4 * (cosSigma * (-1 + 2 * cos2SigmaM * cos2SigmaM) -
                B / 6 * cos2SigmaM * (-3 + 4 * sinSigma * sinSigma) * (-3 + 4 * cos2SigmaM * cos2SigmaM)));

            s = b * A * (sigma - deltaSigma);
            return s / 1000; // Convert to km
        }

        private double ToRadians(double degrees)
        {
            return degrees * (Math.PI / 180);
        }
    }
}
