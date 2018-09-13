using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EQP_Sim.UI_Update
{
    public class FormMainUpdate
    {
        delegate void UpdateLog(string msg);

        public static void LogUpdate(string msg)
        {
            try
            {
                Form form = Application.OpenForms["FormMain"];
                RichTextBox W;
                if (form == null)
                    return;

                W = form.Controls.Find("Log_rt", true).FirstOrDefault() as RichTextBox;
                if (W == null)
                    return;

                if (W.InvokeRequired)
                {
                    UpdateLog ph = new UpdateLog(LogUpdate);
                    W.BeginInvoke(ph, msg);
                }
                else
                {
                    W.AppendText(msg + "\n");
                    if (W.Lines.Length > 1000)
                    {
                        W.Select(0, W.GetFirstCharIndexFromLine(W.Lines.Length - 1000));
                        W.SelectedText = "";
                    }
                    W.ScrollToCaret();
                }
            }
            catch
            {

            }
        }
    }
}
