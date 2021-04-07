using System;
using System.Collections.Generic;

namespace server {
    public static class Utils {
        public static void SafeForEach<T>(this IList<T> list, Action<T> action) {
            for (var i = list.Count - 1; i >= 0; i--) {
                if (i >= list.Count) continue;
                action(list[i]);
            }
        }
    }
}