#nullable enable


using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Display;

namespace Meryel.UnityCodeAssist.Editor.Logger
{
    //**--
    // remove this in unity???
    // need to serialize/deserialize data to survive domain reload, which will effect performance
    // right now data is lost during domain reloads, which makes its function kinda useless
    // or maybe move it to a external process like com.unity.process-server
    public class MemorySink : ILogEventSink
    {
        private const int logsLimit = 30;
        private const int warningLimit = 5;
        private const int errorLimit = 3;
        private readonly ConcurrentQueue<LogEvent[]> errorLogs;
        private readonly ConcurrentQueue<LogEvent> logs;

        private readonly string outputTemplate;
        private readonly ConcurrentQueue<LogEvent[]> warningLogs;

        public MemorySink(string outputTemplate)
        {
            this.outputTemplate = outputTemplate;

            logs = new ConcurrentQueue<LogEvent>();
            warningLogs = new ConcurrentQueue<LogEvent[]>();
            errorLogs = new ConcurrentQueue<LogEvent[]>();
        }

        public bool HasError => !errorLogs.IsEmpty;
        public bool HasWarning => !warningLogs.IsEmpty;
        public int ErrorCount => errorLogs.Count;
        public int WarningCount => warningLogs.Count;

        public void Emit(LogEvent logEvent)
        {
            if (logEvent == null)
                return;

            logs.Enqueue(logEvent);
            if (logs.Count > logsLimit)
                logs.TryDequeue(out _);

            if (logEvent.Level == LogEventLevel.Warning)
            {
                var warningAndLeadingLogs = logs.ToArray();
                warningLogs.Enqueue(warningAndLeadingLogs);
                if (warningLogs.Count > warningLimit)
                    warningLogs.TryDequeue(out _);
            }

            if (logEvent.Level == LogEventLevel.Error)
            {
                var errorAndLeadingLogs = logs.ToArray();
                errorLogs.Enqueue(errorAndLeadingLogs);
                if (errorLogs.Count > errorLimit)
                    errorLogs.TryDequeue(out _);
            }
        }

        public string Export()
        {
            IFormatProvider? formatProvider = null;
            MessageTemplateTextFormatter formatter = new(
                outputTemplate, formatProvider);

            string result = string.Empty;

            using (MemoryStream outputStream = new())
            {
                UTF8Encoding encoding = new(false);
                using StreamWriter output = new(outputStream, encoding);
                if (!errorLogs.IsEmpty)
                {
                    var errorArray = errorLogs.ToArray();
                    foreach (var error in errorArray)
                    foreach (LogEvent logEvent in error)
                        formatter.Format(logEvent, output);
                }

                if (!warningLogs.IsEmpty)
                {
                    var warningArray = warningLogs.ToArray();
                    foreach (var warning in warningArray)
                    foreach (LogEvent logEvent in warning)
                        formatter.Format(logEvent, output);
                }

                if (!logs.IsEmpty)
                {
                    var logArray = logs.ToArray();
                    foreach (LogEvent? logEvent in logArray) formatter.Format(logEvent, output);
                }

                output.Flush();

                outputStream.Seek(0, SeekOrigin.Begin);
                using StreamReader streamReader = new(outputStream, encoding);
                result = streamReader.ReadToEnd();
            }

            return result;
        }
    }
}