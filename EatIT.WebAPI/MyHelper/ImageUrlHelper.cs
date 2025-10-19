using AutoMapper;
using EatIT.Core.Entities;
using EatIT.Infrastructure.Data.DTOs;

namespace EatIT.WebAPI.MyHelper
{
    public static class ImageUrlHelper
    {
        public static string? ResolveImageUrl<T>(T source, IConfiguration configuration, string imagePropertyName)
        {
            if (source == null) return null;

            var property = source.GetType().GetProperty(imagePropertyName);
            if (property == null) return null;

            var imageValue = property.GetValue(source) as string;
            if (!string.IsNullOrEmpty(imageValue))
            {
                return configuration["API_url"] + imageValue;
            }
            return null;
        }
    }
}
