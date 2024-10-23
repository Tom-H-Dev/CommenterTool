using Microsoft.VisualStudio.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace CodeCommenter.Commands
{
    [Command(PackageIds.CodeCommenterSettings)]
    public class ToolWindow1 : BaseToolWindow<ToolWindow1>
    {
        public override string GetTitle(int toolWindowId) => "ToolWindow1";

        public override Type PaneType => typeof(Pane);

        public override Task<FrameworkElement> CreateAsync(int toolWindowId, CancellationToken cancellationToken)
        {
            return Task.FromResult<FrameworkElement>(new ToolWindow1Control());
        }

        [Guid("718dfb6a-f6b4-4bb2-8d2f-6c122d84ab72")]
        internal class Pane : ToolkitToolWindowPane
        {
            public Pane()
            {
                BitmapImageMoniker = KnownMonikers.ToolWindow;
            }

            
        }
    }
}
