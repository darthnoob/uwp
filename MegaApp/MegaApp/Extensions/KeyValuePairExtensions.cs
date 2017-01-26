using System.Collections.Generic;

namespace MegaApp.Extensions
{
    internal static class KeyValuePairExtensions
    {
        public static bool IsNull<T, TU>(this KeyValuePair<T, TU> pair)
        {
            return pair.Equals(default(KeyValuePair<T, TU>));
        }
    }
}
