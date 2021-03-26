using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace shared.serialization {
    internal static class Utils {
        [MethodImpl(MethodImplOptions.NoInlining), DebuggerHidden]
        public static void KeepUnusedVariable<T>(ref T t) => t = ref Nop(ref t);

        [MethodImpl(MethodImplOptions.NoInlining), DebuggerHidden]
        private static ref T Nop<T>(ref T t) => ref t;
    }
}