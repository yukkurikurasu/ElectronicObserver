using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using WeifenLuo.WinFormsUI.Docking;

namespace KanVoice
{
    public partial class VoiceSubtitle : DockContent
    {

        public VoiceSubtitle()
        {
            InitializeComponent();


        }

        public void AddText(string Text)
        {
            if (textBox1.Lines.Length >= 10)
                textBox1.Clear();
            textBox1.Text += Text + Environment.NewLine;
            textBox1.SelectionLength = 0;
        }

        private void textBox1_DoubleClick(object sender, EventArgs e)
        {
            textBox1.Clear();
        }
    }
}
