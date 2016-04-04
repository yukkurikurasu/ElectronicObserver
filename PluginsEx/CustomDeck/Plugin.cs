using ElectronicObserver.Window.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading.Tasks;

using Fiddler;

namespace CustomDeck
{
    public class CustomDeckPlugin : DialogPlugin
    {
        public override string MenuTitle
        {
            get { return "历史编成"; }
        }

        public override string Version
        {
            get { return "1.1.0.0"; }
        }

        public override Form GetToolWindow()
        {
            return new DeckMainForm();
        }

        public override PluginUpdateInformation UpdateInformation
        {
            get
            {

                PluginUpdateInformation inf = new PluginUpdateInformation(PluginUpdateInformation.UpdateType.Auto);
                inf.UpdateInformationURI = "http://herix001.github.io/74Plugins/CustomDeckUpdate.json";
                inf.PluginDownloadURI = "";
                return inf;

            }
        }
    }
}
