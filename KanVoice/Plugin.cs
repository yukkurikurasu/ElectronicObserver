using ElectronicObserver.Window.Plugins;
using ElectronicObserver;
using ElectronicObserver.Data;
using ElectronicObserver.Notifier;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.CSharp;
using Fiddler;

using System.Text.RegularExpressions;

using WeifenLuo.WinFormsUI.Docking;

namespace KanVoice
{

    public class VoicePlugin : ServerPlugin
    {
        public static VoiceSubtitle form = new VoiceSubtitle();
        public static ElectronicObserver.Window.FormMain Main = null;
        VoiceObserverPlugin VoiceObserverPlugin = new VoiceObserverPlugin();

        const string Title = "舰娘语音字幕";

        public override string MenuTitle
        {
            get { return Title; }
        }

        public override string Version
        {
            get { return "1.0.0.1"; }
        }

        public override bool RunService(ElectronicObserver.Window.FormMain main)
        {
            Main = main;
            Main.SubForms.Add(form);
            for (int i = 0; i < Main.MainMenuStrip.Items.Count; i++)
            {
                if (Main.MainMenuStrip.Items[i].Name == "StripMenu_View")
                {
                    var aa = (ToolStripMenuItem)Main.MainMenuStrip.Items[i];
                    aa.DropDownItems.Add(Title).Click += Plugin_Click;
                }
            }
            VoiceObserverPlugin.Data.Init();
            //ElectronicObserver.Utility.Configuration.Instance.AddObserverPlugin(VoiceObserverPlugin);
            return true;
        }
        void Plugin_Click(object sender, EventArgs e)
        {
            try
            {
                form.Show(Main.MainPanel);
            }
            catch
            {
                form = new VoiceSubtitle();
                form.Show(Main.MainPanel);
            }
        }

    }


    public class VoiceObserverPlugin : ObserverPlugin
    {
        const string Title = "舰娘语音字幕";
        public static VoiceData Data = new VoiceData();
        public override string MenuTitle
        {
            get { return Title; }
        }

        public override string Version
        {
            get { return "1.0.0.1"; }
        }

        public override bool OnBeforeRequest(Session oSession)
        {
            //System.IO.File.AppendAllText("d:\\1.txt", oSession.fullUrl + Environment.NewLine);
            
            return false;
        }

        public override bool OnAfterSessionComplete(Session oSession)
        {
            return false;
        }

        public override bool OnBeforeResponse(Session oSession)
        {
            try
            {
                Regex reg = new Regex("sound/kc(.*?)/(.*?).mp3");
                Match match = reg.Match(oSession.fullUrl);
                if (match.Success && match.Groups.Count == 3)
                {
                    //oSession.oRequest.headers["Pragma"] = "no-cache";
                    //oSession.oResponse.headers["Pragma"] = "no-cache";
                    string voice = Data.GetVoice(match.Groups[1].Value, int.Parse(match.Groups[2].Value));
                    if (voice != null)
                    {
                        VoicePlugin.form.AddText(voice);
                    }
                    return false;
                }
            }
            catch
            {
                ElectronicObserver.Utility.Logger.Add(3, string.Format("{0}", "语音字幕插件 出错"));
            }
            return false;
        }
    }
}
