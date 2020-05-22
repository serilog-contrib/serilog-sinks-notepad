// Copyright 2020 C. Augusto Proiete & Contributors
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

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Display;
using Serilog.Sinks.Notepad;
using Serilog.Sinks.Notepad.Interop;

namespace Serilog
{
    /// <summary>
    /// Adds the WriteTo.Notepad() extension method to <see cref="LoggerConfiguration"/>.
    /// </summary>
    public static class NotepadLoggerConfigurationExtensions
    {
        private static readonly object _defaultSyncRoot = new object();
        private const string _defaultNotepadOutputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}";

        /// <summary>
        /// Writes log events to Notepad.
        /// </summary>
        /// <param name="sinkConfiguration">Logger sink configuration.</param>
        /// <param name="restrictedToMinimumLevel">The minimum level for
        /// events passed through the sink. Ignored when <paramref name="levelSwitch"/> is specified.</param>
        /// <param name="outputTemplate">A message template describing the format used to write to the sink.
        /// The default is <code>"{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"</code>.</param>
        /// <param name="formatProvider">Supplies culture-specific formatting information, or null.</param>
        /// <param name="levelSwitch">A switch allowing the pass-through minimum level
        /// to be changed at runtime.</param>
        /// <param name="notepadProcessFinderFunc">A strategy to find the target `notepad.exe` process where log messages will be sent to.</param>
        /// <param name="syncRoot">An object that will be used to `lock` (sync) access to the Notepad output. If you specify this, you
        /// will have the ability to lock on this object, and guarantee that the Notepad sink will not be able to output anything while
        /// the lock is held.</param>
        /// <returns>Configuration object allowing method chaining.</returns>
        public static LoggerConfiguration Notepad(
            this LoggerSinkConfiguration sinkConfiguration,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
            string outputTemplate = _defaultNotepadOutputTemplate,
            IFormatProvider formatProvider = null,
            LoggingLevelSwitch levelSwitch = null,
            Func<Process> notepadProcessFinderFunc = null,
            object syncRoot = null)
        {
            if (sinkConfiguration is null) throw new ArgumentNullException(nameof(sinkConfiguration));

            if (!Enum.IsDefined(typeof(LogEventLevel), restrictedToMinimumLevel))
                throw new InvalidEnumArgumentException(nameof(restrictedToMinimumLevel), (int)restrictedToMinimumLevel,
                    typeof(LogEventLevel));

            syncRoot ??= _defaultSyncRoot;

            var formatter = new MessageTemplateTextFormatter(outputTemplate, formatProvider);

            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? sinkConfiguration.Sink(new NotepadSink(new NotepadTextWriter(notepadProcessFinderFunc), formatter, syncRoot), restrictedToMinimumLevel, levelSwitch)
                : sinkConfiguration.Sink(new NullSink(), restrictedToMinimumLevel, levelSwitch);

        }

        /// <summary>
        /// Writes log events to Notepad.
        /// </summary>
        /// <param name="sinkConfiguration">Logger sink configuration.</param>
        /// <param name="formatter">Controls the rendering of log events into text, for example to log JSON. To
        /// control plain text formatting, use the overload that accepts an output template.</param>
        /// <param name="syncRoot">An object that will be used to `lock` (sync) access to the Notepad output. If you specify this, you
        /// will have the ability to lock on this object, and guarantee that the Notepad sink will not be able to output anything while
        /// the lock is held.</param>
        /// <param name="restrictedToMinimumLevel">The minimum level for
        /// events passed through the sink. Ignored when <paramref name="levelSwitch"/> is specified.</param>
        /// <param name="levelSwitch">A switch allowing the pass-through minimum level
        /// to be changed at runtime.</param>
        /// <param name="notepadProcessFinderFunc">A strategy to find the target `notepad.exe` process where log messages will be sent to.</param>
        /// <returns>Configuration object allowing method chaining.</returns>
        public static LoggerConfiguration Notepad(
            this LoggerSinkConfiguration sinkConfiguration,
            ITextFormatter formatter,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
            LoggingLevelSwitch levelSwitch = null,
            Func<Process> notepadProcessFinderFunc = null,
            object syncRoot = null)
        {
            if (sinkConfiguration is null) throw new ArgumentNullException(nameof(sinkConfiguration));

            if (formatter is null) throw new ArgumentNullException(nameof(formatter));

            if (!Enum.IsDefined(typeof(LogEventLevel), restrictedToMinimumLevel))
                throw new InvalidEnumArgumentException(nameof(restrictedToMinimumLevel), (int)restrictedToMinimumLevel,
                    typeof(LogEventLevel));

            syncRoot ??= _defaultSyncRoot;

            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? sinkConfiguration.Sink(new NotepadSink(new NotepadTextWriter(notepadProcessFinderFunc), formatter, syncRoot), restrictedToMinimumLevel, levelSwitch)
                : sinkConfiguration.Sink(new NullSink(), restrictedToMinimumLevel, levelSwitch);
        }
    }
}
