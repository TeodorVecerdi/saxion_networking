using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace shared
{
    /**
     * Simple log class which wraps Console.WriteLine and adds some utility color for output coloring and 'clumping' the output.
     * Debug info is automatically removed if you build this library in Release mode (or remove the compiler flag DEBUG).
     * 
     * To have this information show up in Unity two things are required:
     * - Console info needs to be redirected to the Debug.Log (using UnitySystemConsoleRedirector.Redirect());
     * - You have to be running in either the editor, or have 'Scripting Define Symbols' include 'DEBUG' (without quotes)
     */
    public static class Log
    {
        //we keep track of the current color pushed on a stack, so we can have nested blocks of color
        private static Stack<ConsoleColor> _colors = new Stack<ConsoleColor>(new ConsoleColor[]{ConsoleColor.White});
        //we keep track of the last color pushed as a cheesy way to detect whether to add newlines to the output
        private static ConsoleColor _lastColor = ConsoleColor.Gray;
        //allows to temporarily disable output from code
        public static bool enabled = true;

        /**
         * Log an object (using it's ToString method), and caller class name to the Console, optionally using a specific color).
         * [CallerMemberName] is automatically filled in by the compiler.
         */
        [Conditional("DEBUG")]
        public static void LogInfo(object pInfo, object pCaller, ConsoleColor? pColor = null, [CallerMemberName]string pMemberName = "")
        {
            LogInfo(pInfo, pCaller.GetType(), pColor, pMemberName);
        }

        /**
         * Log an object (using it's ToString method), and caller class name to the Console, optionally using a specific color).
         * [CallerMemberName] is automatically filled in by the compiler.
         */
        [Conditional("DEBUG")]
        public static void LogInfo(object pInfo, Type pType, ConsoleColor? pColor = null, [CallerMemberName]string pMemberName = "")
        {
            if (!enabled) return;

            if (pColor != null) _colors.Push((ConsoleColor)pColor);
            if (_colors.Count > 0) setConsoleColor(_colors.Peek());
            Console.WriteLine($"{pType.Name}.{pMemberName}():{pInfo}");
            if (pColor != null) _colors.Pop();
        }

        [Conditional("DEBUG")]
        /**
         * Ensures all the next line are using the given color, until that color is popped again.
         */
        public static void PushForegroundColor (ConsoleColor pForegroundColor)
        {
            if (enabled) Console.WriteLine("");
            _colors.Push(pForegroundColor);
        }

        [Conditional("DEBUG")]
        /**
         * Pop a color from the color stack so we revert to the previous color on the stack
         */
        public static void PopForegroundColor()
        {
            _colors.Pop();
        }

        [Conditional("DEBUG")]
        private static void setConsoleColor(ConsoleColor pConsoleColor)
        {
            if (pConsoleColor != _lastColor)
            {
                Console.WriteLine();
                _lastColor = pConsoleColor;
            }

            Console.ForegroundColor = pConsoleColor;
        }
    }
}
