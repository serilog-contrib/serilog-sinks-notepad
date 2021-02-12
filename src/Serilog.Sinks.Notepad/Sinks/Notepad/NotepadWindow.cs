#region Copyright 2020-2021 C. Augusto Proiete & Contributors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

using System;
using System.Runtime.InteropServices;
using Serilog.Sinks.Notepad.Interop;

namespace Serilog.Sinks.Notepad
{
    /// <summary>
    /// Writes messages directly to the most recent `notepad.exe` process
    /// </summary>
    /// <example>
    /// Capture internal errors from Serilog sinks via <see cref="Serilog.Debugging.SelfLog"/>:
    /// <code>
    /// Serilog.Debugging.SelfLog.Enable(s => NotepadWindow.WriteLine($"Internal Error with Serilog: {s}"));
    /// </code>
    /// </example>
    public static class NotepadWindow
    {
        private static readonly Lazy<NotepadTextWriter> _notepadWindow =
            new Lazy<NotepadTextWriter>(() => new NotepadTextWriter());

        /// <summary>
        /// Writes a string to the most recent Notepad window open.
        /// If the given string is null, nothing is written to the text stream.
        /// </summary>
        /// <param name="value">The string to be written</param>
        public static void Write(string value)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return;
            }

            var notepadWindow = _notepadWindow.Value;
            notepadWindow.Write(value);
            notepadWindow.Flush();
        }

        /// <summary>
        /// Writes a line terminator to the most recent Notepad window open.
        /// </summary>
        public static void WriteLine()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return;
            }

            var notepadWindow = _notepadWindow.Value;
            notepadWindow.WriteLine();
            notepadWindow.Flush();
        }

        /// <summary>
        /// Writes a line terminator to the most recent Notepad window open.
        /// </summary>
        /// <param name="value">The string to be written</param>
        public static void WriteLine(string value)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return;
            }

            var notepadWindow = _notepadWindow.Value;
            notepadWindow.WriteLine(value);
            notepadWindow.Flush();
        }
    }
}
