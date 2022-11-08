using EnvDTE;
using Mono.Debugging.Soft;
using Mono.Debugging.VisualStudio;

namespace ZeoEngineTools.Debugging
{
    internal class ZeoEngineStartInfo : StartInfo
    {
        public ZeoEngineStartInfo(SoftDebuggerStartArgs args, DebuggingOptions options, Project startupProject)
            : base(args, options, startupProject)
        {

        }
    }
}
