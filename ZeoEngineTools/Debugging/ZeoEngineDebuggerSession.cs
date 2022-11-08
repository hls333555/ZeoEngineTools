using System;
using System.IO;
using Mono.Debugging.Client;
using Mono.Debugging.Soft;
using Mono.Debugger.Soft;

namespace ZeoEngineTools.Debugging
{
    internal class ZeoEngineDebuggerSession : SoftDebuggerSession
    {
        private bool m_bIsAttached;

        protected override void OnRun(DebuggerStartInfo startInfo)
        {
            m_bIsAttached = true;
            var zeoEngineStartInfo = startInfo as ZeoEngineStartInfo;
            base.OnRun(zeoEngineStartInfo);
        }

        protected override void OnConnectionError(Exception ex)
        {
            // The session was manually terminated
            if (HasExited)
            {
                base.OnConnectionError(ex);
                return;
            }

            if (ex is VMDisconnectedException || ex is IOException)
            {
                HasExited = true;
                base.OnConnectionError(ex);
                return;
            }

            string message = "An error occured when trying to attach to ZeoEngine. Please make sure that ZeoEngine is running and that it's up-to-date.";
            message += Environment.NewLine;
            message += string.Format("Message: {0}", ex != null ? ex.Message : "No error message provided.");

            if (ex != null)
            {
                message += Environment.NewLine;
                message += string.Format("Source: {0}", ex.Source);
                message += Environment.NewLine;
                message += string.Format("Stack Trace: {0}", ex.StackTrace);

                if (ex.InnerException != null)
                {
                    message += Environment.NewLine;
                    message += string.Format("Inner Exception: {0}", ex.InnerException.ToString());
                }
            }
			
            _ = ZeoEngineToolsPackage.Instance.ShowErrorMessageBoxAsync("Connection Error", message);
            base.OnConnectionError(ex);
        }

        protected override void OnExit()
        {
            if (m_bIsAttached)
            {
                m_bIsAttached = false;
                base.OnDetach();
            }
            else
            {
                base.OnExit();
            }
        }
    }
}
