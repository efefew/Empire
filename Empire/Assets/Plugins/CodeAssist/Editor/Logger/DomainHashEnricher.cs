#nullable enable


using Serilog.Core;
using Serilog.Events;
using UnityEditor;

namespace Meryel.UnityCodeAssist.Editor.Logger
{
    public class DomainHashEnricher : ILogEventEnricher
    {
        private static readonly int domainHash;

        static DomainHashEnricher()
        {
            GUID guid = GUID.Generate();
            domainHash = guid.GetHashCode();
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
                "DomainHash", domainHash));
        }
    }
}