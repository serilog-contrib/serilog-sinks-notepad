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

using System.Globalization;
using System.IO;
using FluentAssertions;
using Serilog.Formatting.Display;
using Serilog.Support;
using Xunit;

namespace Serilog.Sinks.TextWriter
{
    public class TextWriterSinkTests
    {
        private const string _outputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] {Message}{NewLine}{Exception}";
        private readonly object _syncRoot = new object();

        [Fact]
        public void EventsAreWrittenToTheTextWriter()
        {
            var sw = new StringWriter();
            var formatter = new MessageTemplateTextFormatter(_outputTemplate, null);

            var log = new LoggerConfiguration()
                .WriteTo.Sink(new TextWriterSink(sw, formatter, _syncRoot))
                .CreateLogger();

            var mt = Some.String();
            log.Information(mt);

            var s = sw.ToString();
            s.Should().Contain(mt);
        }

        [Fact]
        public void EventsAreWrittenToTheTextWriterUsingFormatProvider()
        {
            var sw = new StringWriter();
            var brazilianPortuguese = new CultureInfo("pt-BR");
            var formatter = new MessageTemplateTextFormatter(_outputTemplate, brazilianPortuguese);

            var log = new LoggerConfiguration()
                .WriteTo.Sink(new TextWriterSink(sw, formatter, _syncRoot))
                .CreateLogger();

            var mt = string.Format(brazilianPortuguese, "{0}", 1_234.567);
            log.Information("{0}", 1_234.567);

            var s = sw.ToString();
            s.Should().Contain(mt);
        }
    }
}
