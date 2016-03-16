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
        public static int MaxLines = 10;
        List<string> VoiceText = new List<string>();
        public VoiceSubtitle()
        {
            InitializeComponent();


        }

        public void AddText(string Text)
        {
            VoiceText.Add(Text);
            if (VoiceText.Count > MaxLines)
                VoiceText.RemoveAt(0);
            textBox1.Text = string.Join("\r\n", VoiceText.ToArray());
            textBox1.SelectionLength = 0;
            textBox1.SelectionStart = 0;
        }

        private void textBox1_DoubleClick(object sender, EventArgs e)
        {
            VoiceText.Clear();
            textBox1.Clear();
        }

        private void CopyText_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(textBox1.SelectedText);
        }

        private void UseThird_Click(object sender, EventArgs e)
        {
            UseThird.Checked = VoiceData.UseThirdBuffer = !VoiceData.UseThirdBuffer;
            VoicePlugin.SaveConfig();
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            foreach(var item in SetMax.DropDownItems)
            {
                ((ToolStripMenuItem)item).Checked = false;
            }
            ((ToolStripMenuItem)sender).Checked = true;
            MaxLines = int.Parse(((ToolStripMenuItem)sender).Text);
            VoicePlugin.SaveConfig();
        }
    }
}
