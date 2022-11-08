using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using EnvDTE;
using Microsoft.VisualStudio;
using Mono.Debugging.Soft;
using Mono.Debugging.VisualStudio;
using ZeoEngineTools.Debugging;
using Task = System.Threading.Tasks.Task;

namespace ZeoEngineTools
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class AttachToZeoEditorCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("a812abc2-8042-45f1-abfb-cbb89f3f0e3b");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        private IVsSolutionBuildManager m_SolutionBuildManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="AttachToZeoEditorCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private AttachToZeoEditorCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static AttachToZeoEditorCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in AttachToZeoEditorCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new AttachToZeoEditorCommand(package, commandService);
            Instance.m_SolutionBuildManager = await package.GetServiceAsync(typeof(IVsSolutionBuildManager)) as IVsSolutionBuildManager;
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            
            m_SolutionBuildManager.get_StartupProject(out var vsHierarchy);
            vsHierarchy.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ExtObject, out object projectObj);
            Project project = projectObj as Project;
            var startArgs = new SoftDebuggerConnectArgs(project.Name, IPAddress.Parse("127.0.0.1"), ZeoEngineToolsPackage.Instance.GeneralOptions.ConnectionPort)
            {
                MaxConnectionAttempts = ZeoEngineToolsPackage.Instance.GeneralOptions.MaxConnectionAttempts
            };

            var startInfo = new ZeoEngineStartInfo(startArgs, null, project)
            {
                WorkingDirectory = ZeoEngineToolsPackage.Instance.SolutionEventsListener?.SolutionDirectory
            };

            var session = new ZeoEngineDebuggerSession();
            session.Breakpoints.Clear();
            var launcher = new MonoDebuggerLauncher(new Progress<string>());
            launcher.StartSession(startInfo, session);
        }
    }
}
