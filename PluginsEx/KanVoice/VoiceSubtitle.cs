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
        List<string> VoiceText = new List<string>();

        public static VoiceSubtitle VoiceSubtitleForm = null;

        public VoiceSubtitle(ElectronicObserver.Window.FormMain parent)
        {
            InitializeComponent();

        }

        public void AddText(string Text)
        {
            if (Text == null)
                return;
            VoiceText.Add(Text);
            if (VoiceText.Count > VoiceData.MaxLines)
                VoiceText.RemoveAt(0);
            textBox1.Text = string.Join("\r\n", VoiceText.ToArray());
            textBox1.SelectionLength = 0;
            textBox1.SelectionStart = textBox1.Text.Length;
            textBox1.ScrollToCaret();
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
            foreach (var item in SetMax.DropDownItems)
            {
                ((ToolStripMenuItem)item).Checked = false;
            }
            ((ToolStripMenuItem)sender).Checked = true;
            VoiceData.MaxLines = int.Parse(((ToolStripMenuItem)sender).Text);
            VoicePlugin.SaveConfig();
        }

        private void 我想帮忙完善台词ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http:\\zh.kcwiki.moe");
        }

        private void IgnoreItem_Click(object sender, EventArgs e)
        {
            IgnoreItem.Checked = VoiceData.IgnoreBlankSubtitles = !VoiceData.IgnoreBlankSubtitles;
            VoicePlugin.SaveConfig();
        }

        private void VoiceSubtitle_Load(object sender, EventArgs e)
        {
            VoiceSubtitleForm = this;
            Instance_ConfigurationChanged();

            this.HideOnClose = true;

            ElectronicObserver.Utility.Configuration.Instance.ConfigurationChanged += Instance_ConfigurationChanged;
            VoicePlugin.SubtitleConfigChanged += VoiceData_SubtitleConfigChanged;

            VoicePlugin.LoadConfig();
        }

        private void VoiceData_SubtitleConfigChanged(object sender, EventArgs e)
        {
            VoiceSubtitleForm = this;
            ((ToolStripMenuItem)(MenuArea.DropDownItems[(int)VoiceData.subtitleDisplayArea])).Checked = true;
            ((ToolStripMenuItem)(MenuLanguage.DropDownItems[(int)VoiceData.subtitleLanguage])).Checked = true;

            UseThird.Checked = VoiceData.UseThirdBuffer;
            IgnoreItem.Checked = VoiceData.IgnoreBlankSubtitles;
            if (VoiceData.MaxLines <= SetMax.DropDownItems.Count)
                ((ToolStripMenuItem)SetMax.DropDownItems[VoiceData.MaxLines - 1]).Checked = true;
        }

        void Instance_ConfigurationChanged()
        {
            textBox1.Font = ElectronicObserver.Utility.Configuration.Config.UI.MainFont;
            textBox1.ForeColor = ElectronicObserver.Utility.Configuration.Config.UI.ForeColor;
            textBox1.BackColor = ElectronicObserver.Utility.Configuration.Config.UI.BackColor;
        }

        private void AreaClick(object sender, EventArgs e)
        {
            foreach (var item in MenuArea.DropDownItems)
            {
                ((ToolStripMenuItem)item).Checked = false;
            }
            ((ToolStripMenuItem)sender).Checked = true;
            VoiceData.subtitleDisplayArea = (SubtitleDisplayArea)((ToolStripMenuItem)sender).Tag;
            VoicePlugin.SaveConfig();
        }

        private void LanguageClick(object sender, EventArgs e)
        {
            foreach (var item in MenuLanguage.DropDownItems)
            {
                ((ToolStripMenuItem)item).Checked = false;
            }
            ((ToolStripMenuItem)sender).Checked = true;
            VoiceData.subtitleLanguage = (SubtitleLanguage)((ToolStripMenuItem)sender).Tag;
            VoicePlugin.SaveConfig();
        }
    }
}
