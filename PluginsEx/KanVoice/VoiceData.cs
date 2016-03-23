using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ElectronicObserver;
using ElectronicObserver.Data;
using System.Web.Script.Serialization;

namespace KanVoice
{
    public enum SubtitleDisplayArea
    {
        DockForm = 0, StatusBar = 1, Both = 2
    }
    public class VoiceData
    {
        int[] voiceKey = new int[] { 604825, 607300, 613847, 615318, 624009, 631856, 635451, 637218, 640529, 643036, 652687, 658008, 662481, 669598, 675545, 685034, 687703, 696444, 702593, 703894, 711191, 714166, 720579, 728970, 738675, 740918, 743009, 747240, 750347, 759846, 764051, 770064, 773457, 779858, 786843, 790526, 799973, 803260, 808441, 816028, 825381, 827516, 832463, 837868, 843091, 852548, 858315, 867580, 875771, 879698, 882759, 885564, 888837, 896168 };
        Dictionary<int, Dictionary<int, int>> voiceMap = new Dictionary<int, Dictionary<int, int>>();
        Dictionary<string, object> data;
        public static string ConfigFile;
        public static bool UseThirdBuffer = false;
        public static bool IgnoreBlankSubtitles = false;
        public static SubtitleDisplayArea subtitleDisplayArea = SubtitleDisplayArea.DockForm;

        public void Init()
        {
            ConfigFile = System.Windows.Forms.Application.StartupPath + "\\Settings\\VoiceSubtitle.xml";
            JavaScriptSerializer Serializer = new JavaScriptSerializer();
            string s = Encoding.UTF8.GetString(Properties.Resources.subtitles);
            data = (Dictionary<string, object>)Serializer.DeserializeObject(s);

            for (int ShipID = 1; ShipID <= 500; ShipID++)
            {

                voiceMap[ShipID] = new Dictionary<int, int>();
                for (int i = 1; i < voiceKey.Length; i++)
                {
                    voiceMap[ShipID][ConvertFilename(ShipID, i)] = i;
                }
            }
        }

        int ConvertFilename(int ShipId, int VoiceId)
        {
            return (ShipId + 7) * 17 * (voiceKey[VoiceId] - voiceKey[VoiceId - 1]) % 99173 + 100000;
        }

        public string GetVoice(int shipid, int voiceID)
        {
            if (voiceMap.ContainsKey(shipid) && data.ContainsKey(shipid.ToString()))
            {
                string voiceid = voiceID.ToString();

                var voices = (Dictionary<string, object>)data[shipid.ToString()];
                if (voices.ContainsKey(voiceid))
                {
                    string text = voices[voiceid].ToString();
                    return text;
                }
                
            }
            return null;
        }

        public string GetVoice(string ShipCode, int FileName)
        {
            var ship = KCDatabase.Instance.MasterShips.Values.FirstOrDefault(e => { return e.ResourceName == ShipCode; });
            if (ship == null)
                return null;
            int shipid = ship.ShipID;
            string ShipName = ship.Name;
            if (voiceMap.ContainsKey(shipid) &&data.ContainsKey(shipid.ToString()))
            {
                var kan = voiceMap[shipid];
                if (kan.ContainsKey(FileName))
                {
                    string voiceid = kan[FileName].ToString();

                    var voices = (Dictionary<string, object>)data[shipid.ToString()];
                    if (voices.ContainsKey(voiceid))
                    {
                        string text = voices[voiceid].ToString();
                        return "[" + ShipName + "]: " + text;
                    }
                }
            }
            return IgnoreBlankSubtitles ? null : "[" + ShipName + "]: 这句语音还没有台词呢,到舰娘百科网站帮忙维护吧";
        }
    }
}
