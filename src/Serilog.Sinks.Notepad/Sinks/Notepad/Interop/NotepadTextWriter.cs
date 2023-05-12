#region Copyright 2020-2023 C. Augusto Proiete & Contributors
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

using Serilog.Debugging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Serilog.Sinks.Notepad.Interop
{
    internal class NotepadTextWriter : StringWriter
    {
        private readonly Func<Process> _notepadProcessFinderFunc;
        private Process _currentNotepadProcess;
        private IntPtr _currentNotepadEditorHandle;
        private bool _disposed;

        public NotepadTextWriter(Func<Process> notepadProcessFinderFunc = null)
            : base(new StringBuilder())
        {
            _notepadProcessFinderFunc = notepadProcessFinderFunc ?? TryFindMostRecentNotepadProcess;
        }

        public override void Flush()
        {
            EnsureNotDisposed();

            base.Flush();

            var currentNotepadProcess = _currentNotepadProcess;
            var targetNotepadProcess = _notepadProcessFinderFunc();

            if (currentNotepadProcess is null || targetNotepadProcess is null || currentNotepadProcess.Id != targetNotepadProcess.Id)
            {
                _currentNotepadProcess = currentNotepadProcess = targetNotepadProcess;
                _currentNotepadEditorHandle = IntPtr.Zero;

                if (currentNotepadProcess is null || currentNotepadProcess.HasExited)
                {
                    // No instances of Notepad found... Nothing to do
                    return;
                }

                var notepadWindowHandle = currentNotepadProcess.MainWindowHandle;

                var notepadEditorHandle = FindNotepadEditorHandle(notepadWindowHandle);
                if (notepadEditorHandle == IntPtr.Zero)
                {
                    SelfLog.WriteLine($"Unable to access a Notepad Editor on process {currentNotepadProcess.ProcessName} ({currentNotepadProcess.Id})");
                    return;
                }

                _currentNotepadEditorHandle = notepadEditorHandle;
            }

            // Get how many characters are in the Notepad editor already
            var textLength = User32.SendMessage(_currentNotepadEditorHandle, User32.WM_GETTEXTLENGTH, IntPtr.Zero, IntPtr.Zero);

            // Set the caret position to the end of the text
            User32.SendMessage(_currentNotepadEditorHandle, User32.EM_SETSEL, (IntPtr)textLength, (IntPtr)textLength);

            var buffer = base.GetStringBuilder();
            var message = buffer.ToString();

            // Write the log message to Notepad
            User32.SendMessage(_currentNotepadEditorHandle, User32.EM_REPLACESEL, (IntPtr)1, message);

            buffer.Clear();
        }

        protected override void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _currentNotepadProcess = null;
                _currentNotepadEditorHandle = IntPtr.Zero;
                _disposed = true;
            }

            base.Dispose(disposing);
        }

        private static Process TryFindMostRecentNotepadProcess()
        {
            var mostRecentNotepadProcess = Process.GetProcessesByName("notepad")
                .Where(p => !p.HasExited)
                .OrderByDescending(p => p.StartTime)
                .FirstOrDefault();

            return mostRecentNotepadProcess;
        }

        private static IntPtr FindNotepadEditorHandle(IntPtr notepadWindowHandle)
        {
            // Windows 11 uses the new RichEditD2DPT class:
            // https://devblogs.microsoft.com/math-in-office/windows-11-notepad/#some-implementation-details
            if (User32.FindWindowEx(notepadWindowHandle, IntPtr.Zero, "RichEditD2DPT", null) is var richEditHandle
                && richEditHandle != IntPtr.Zero)
            {
                return richEditHandle;
            }

            // Issue #59 - Alternate way of finding the RichEditD2DPT class:
            if (FindEditorHandleThroughChildWindows(notepadWindowHandle) is var richEditHandleFromChildren
                && richEditHandleFromChildren != IntPtr.Zero)
            {
                return richEditHandleFromChildren;
            }

            return User32.FindWindowEx(notepadWindowHandle, IntPtr.Zero, "Edit", null);
        }

        private void EnsureNotDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

        private static string GetClassNameFromWindow(IntPtr handle)
        {
            StringBuilder sb = new StringBuilder(256);
            var ret = User32.GetClassName(handle, sb, sb.Capacity);
            return ret != 0 ? sb.ToString() : string.Empty;
        }

        private static bool EnumWindow(IntPtr handle, IntPtr pointer)
        {
            GCHandle gch = GCHandle.FromIntPtr(pointer);
            List<IntPtr> list = gch.Target as List<IntPtr>;
            if (list == null)
            {
                throw new InvalidCastException("GCHandle Target could not be cast as List<IntPtr>");
            }

            if (string.Equals(GetClassNameFromWindow(handle), "RichEditD2DPT", StringComparison.OrdinalIgnoreCase))
            {
                list.Add(handle);

                // Stop enumerating - we found the one.
                return false;
            }

            return true;
        }

        private static IntPtr FindEditorHandleThroughChildWindows(IntPtr notepadWindowHandle)
        {
            List<IntPtr> result = new List<IntPtr>(1);
            GCHandle listHandle = GCHandle.Alloc(result);
            try
            {
                User32.Win32Callback childProc = new User32.Win32Callback(EnumWindow);
                User32.EnumChildWindows(notepadWindowHandle, childProc, GCHandle.ToIntPtr(listHandle));
            }
            finally
            {
                if (listHandle.IsAllocated)
                    listHandle.Free();
            }
            return result.FirstOrDefault();
        }
    }
}
