using System.Reflection;

namespace RuTracker.Client {
    static class VersionExtractor {
        public static readonly string Version = typeof(VersionExtractor)
            .Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            .InformationalVersion;
    }
}