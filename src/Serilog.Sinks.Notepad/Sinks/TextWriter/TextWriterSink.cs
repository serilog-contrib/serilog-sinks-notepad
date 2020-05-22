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
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;

namespace Serilog.Sinks.TextWriter
{
    internal class TextWriterSink : ILogEventSink, IDisposable
    {
        private readonly System.IO.TextWriter _textWriter;
        private readonly ITextFormatter _formatter;
        private readonly object _syncRoot;
        private bool _disposed;

        public TextWriterSink(System.IO.TextWriter textWriter, ITextFormatter formatter, object syncRoot)
        {
            _textWriter = textWriter ?? throw new ArgumentNullException(nameof(textWriter));
            _formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
            _syncRoot = syncRoot ?? throw new ArgumentNullException(nameof(syncRoot));
        }

        public void Emit(LogEvent logEvent)
        {
            EnsureNotDisposed();

            lock (_syncRoot)
            {
                _formatter.Format(logEvent, _textWriter);
                _textWriter.Flush();
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            _textWriter.Dispose();
            _disposed = true;
        }

        private void EnsureNotDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }
    }
}
