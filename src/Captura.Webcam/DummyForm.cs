using System;
using System.Windows.Forms;

namespace Captura.WebCam
{
    internal class DummyForm : Form
    {
        public DummyForm()
        {
            Opacity = 0;
            ShowInTaskbar = false;
        }

        protected override void WndProc(ref Message message)
        {
            const int msgLeftButtonDown = 513;

            if (message.Msg == msgLeftButtonDown)
            {
                OnClick(EventArgs.Empty);
            }

            base.WndProc(ref message);
        }
    }
}