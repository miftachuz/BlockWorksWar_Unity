using System;
using System.Linq;
using ElasticSea.Framework.Extensions;

namespace Utils
{
    public static class EnumExtensions
    {
        public static T Next<T>(this T source) where T : Enum
        {
            var values = Enum.GetValues(typeof(T)).Cast<T>().ToArray();
            var index = values.IndexOf(source);
            return values[(index + 1) % values.Length];
        }
    }
}