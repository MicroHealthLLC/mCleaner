using System.Diagnostics;
using System.Windows.Documents;

namespace mCleaner.Helpers.Controls
{
    public class HyperlinkEx : Hyperlink
    {
        protected override void OnClick()
        {
            base.OnClick();

            Process p = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = this.NavigateUri.AbsoluteUri
                }
            };
            p.Start();
        }
    }
}
