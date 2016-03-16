using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FurnitureReplacement
{
    public partial class FurnitureReplacementForm : Form
    {
        Dictionary<string, string> FurnitureType;
        string CacheFolder = null;
        public FurnitureReplacementForm()
        {
            InitializeComponent();
            FurnitureType = new Dictionary<string, string>();
            FurnitureType.Add("壁纸", "wall");
            FurnitureType.Add("地板", "floor");
            FurnitureType.Add("椅子", "desk");
            FurnitureType.Add("窗户", "window");
            FurnitureType.Add("装饰", "object");
            FurnitureType.Add("家具", "chest");
        }

        protected virtual void Form1_Shown(object sender, EventArgs e)
        {
            if (!System.IO.Directory.Exists(ElectronicObserver.Utility.Configuration.Config.CacheSettings.CacheFolder))
            {
                label1.Visible = true;
                this.Enabled = false;
            }
            else
            {
                CacheFolder = ElectronicObserver.Utility.Configuration.Config.CacheSettings.CacheFolder;
                CacheFolder += "\\kcs\\resources\\image\\furniture\\";
            }


            comboBox1.Items.AddRange(FurnitureType.Keys.ToArray());
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            button2.Enabled = false;
            listBox1.Items.Clear();
            PurgeImage();

            if (comboBox1.SelectedIndex < 0)
            {
                return;
            }
            string Type = FurnitureType[comboBox1.Text];
            string path = CacheFolder + Type;

            if (System.IO.Directory.Exists(path))
            {
                var AllPicList = System.IO.Directory.GetFiles(path, "*.png").Select(name => System.IO.Path.GetFileName(name));
                var PicList = AllPicList.Where((filename) => { return filename.ToLower().IndexOf(".hack.") < 0; });
                listBox1.Items.AddRange(PicList.ToArray());
                button2.Enabled = true;
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex < 0)
            {
                PurgeImage();
                return;
            }
            string Type = FurnitureType[comboBox1.Text];
            string path = CacheFolder + Type;
            var filename = listBox1.GetItemText(listBox1.SelectedItem);
            var picfile = path + "\\" + filename;
            pictureBox1.Image = Image.FromFile(picfile);
            button1.Enabled = true;
        }

        private void PurgeImage()
        {
            pictureBox1.Image = null;
            button1.Enabled = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string Type = FurnitureType[comboBox1.Text];
            string path = CacheFolder + Type;
            var filename = listBox1.GetItemText(listBox1.SelectedItem);
            var picfile = path + "\\" + filename;
            System.IO.File.Copy(picfile, path + "\\001.hack.png", true);
            MessageBox.Show("替换成功,更换成初始家具并且刷新母港可以看到效果(可能需要清除缓存)");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string Type = FurnitureType[comboBox1.Text];
            string path = CacheFolder + Type;
            System.IO.File.Delete(path + "\\001.hack.png");
            MessageBox.Show(comboBox1.Text + "类别已经成功恢复");
        }
    }
}
