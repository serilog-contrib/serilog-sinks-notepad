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
