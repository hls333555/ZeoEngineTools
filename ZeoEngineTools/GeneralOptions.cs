using System.ComponentModel;
using Microsoft.VisualStudio.Shell;

namespace ZeoEngineTools
{
    public class GeneralOptions : DialogPage
    {

        [Category("Debugging")]
        [DisplayName("Connection Port")]
        [Description("The port that ZeoEngine is expected to use for the debugger.")]
        public int ConnectionPort { get; set; } = 2550;

        [Category("Debugging")]
        [DisplayName("Maximum Connection Attempts")]
        [Description("Determines how many connection attempts ZeoEngine Tools can make if it fails to attach to ZeoEngine (0 means inifite attempts. Default: 1)")]
        public int MaxConnectionAttempts { get; set; } = 1;

    }
}