#nullable enable


using System;
using Serilog.Core;
using Serilog.Events;
using UnityEngine;

namespace Meryel.UnityCodeAssist.Editor.Logger
{
    public class UnityOutputWindowSink : ILogEventSink
    {
        private readonly IFormatProvider? _formatProvider;

        public UnityOutputWindowSink(IFormatProvider? formatProvider)
        {
            _formatProvider = formatProvider;
        }

        public void Emit(LogEvent logEvent)
        {
            string? message = logEvent.RenderMessage(_formatProvider);

            switch (logEvent.Level)
            {
                case LogEventLevel.Verbose:
                case LogEventLevel.Debug:
                case LogEventLevel.Information:
                    Debug.Log(message);
                    break;
                case LogEventLevel.Warning:
                    Debug.LogWarning(message);
                    break;
                case LogEventLevel.Error:
                case LogEventLevel.Fatal:
                    Debug.LogError(message);
                    break;
            }
        }
    }
}