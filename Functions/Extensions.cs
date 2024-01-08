using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectLauncher.Functions
{
    public static class Extensions
    {
        public static bool TryFind<T>(this List<T> values, Predicate<T> predicate, out T result)
        {
            var e = values.Find(predicate);
            if (e != null)
            {
                result = e;
                return true;
            }

            result = default;
            return false;
        }
    }
}
