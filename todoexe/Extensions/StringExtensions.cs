using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace todoexe.Extensions
{
    internal static class StringExtensions
    {
        public static string JoinRange(this string[] arr, int fromIndex)
        {
            return String.Join(" ", arr.Skip(fromIndex));
        }

        public static bool HasValue(this string value)
        {
            return !String.IsNullOrEmpty(value);
        }

        public static string NullIfEmpty(this string value)
        {
            return String.IsNullOrEmpty(value) ? null : value;
        }

        public static int? ToNullableInt(this string value)
        {
            int? ret = null;
            if (value.HasValue()) ret = Convert.ToInt32(value);

            return ret;
        }

    }
}
