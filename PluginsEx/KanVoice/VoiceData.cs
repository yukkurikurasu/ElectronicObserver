using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
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
        System.DateTime LastUpdateTime;
        bool Updating;
        public static string ConfigFile;
        public string LocalFile;
        public static bool UseThirdBuffer = false;
        public static bool IgnoreBlankSubtitles = false;
        public static SubtitleDisplayArea subtitleDisplayArea = SubtitleDisplayArea.DockForm;
        public static int MaxLines = 10;
        

        public void Init()
        {
            LocalFile = System.Windows.Forms.Application.StartupPath + "\\Settings\\Subtitles.json";
            ConfigFile = System.Windows.Forms.Application.StartupPath + "\\Settings\\VoiceSubtitle.xml";
            JavaScriptSerializer Serializer = new JavaScriptSerializer();

            if (File.Exists(LocalFile))
            {
                var localjson = File.ReadAllText(LocalFile);
                try
                {
                    var localdata = Serializer.DeserializeObject(localjson) as Dictionary<string, object>;
                    if (localdata != null)
                    {
                        data = localdata;
                    }
                }
                catch { }
            }
            if (data == null)
            {
                string s = Encoding.UTF8.GetString(Properties.Resources.subtitles);
                data = (Dictionary<string, object>)Serializer.DeserializeObject(s);
            }
            Updating = true;
            Task.Factory.StartNew(UpdateVoice);

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

        void CheckUpdate()
        {
            if (!Updating)
            {
                Updating = true;
                if (System.DateTime.Now > LastUpdateTime.AddHours(12))
                {
                    Task.Factory.StartNew(UpdateVoice);
                }
            }
        }

        void UpdateVoice()
        {
            string LocalVer = data["version"].ToString();
            ElectronicObserver.Utility.Logger.Add(2, string.Format("开始检查字幕更新数据,当前版本是({0})", LocalVer));
            string Url = "http://api.kcwiki.moe/subtitles/diff/" + LocalVer;
            WebRequest wReq = System.Net.WebRequest.Create(Url);
            // Get the response instance.
            WebResponse wResp = wReq.GetResponse();
            Stream respStream = wResp.GetResponseStream();
            var JavaScriptSerializer = new JavaScriptSerializer();
            int count = 0;
            using (System.IO.StreamReader reader = new System.IO.StreamReader(respStream))
            {
                var diffstring = reader.ReadToEnd();
                try
                {
                    var diff = JavaScriptSerializer.DeserializeObject(diffstring) as Dictionary<string, object>;
                    if (diff != null && diff.Count > 0)
                    {
                        lock (data)
                        {
                            foreach (var shipdata in diff)
                            {
                                if (shipdata.Value is Dictionary<string, object>)
                                {
                                    var shipvoice = shipdata.Value as Dictionary<string, object>;
                                    count += shipvoice.Count;
                                    if (!data.ContainsKey(shipdata.Key))
                                    {
                                        data[shipdata.Key] = shipvoice;
                                        File.AppendAllText(@"d:\1.txt", shipdata.Key + ":" + shipvoice);
                                    }
                                    else
                                    {
                                        var LocalVoices = data[shipdata.Key] as Dictionary<string, object>;
                                        foreach (var voice in shipvoice)
                                        {
                                            LocalVoices[voice.Key] = voice.Value;
                                            //File.AppendAllText(@"d:\1.txt", voice.Key + ":" + voice.Value);
                                        }
                                    }
                                }
                                else
                                {
                                    data[shipdata.Key] = shipdata.Value;
                                    //File.AppendAllText(@"d:\1.txt", shipdata.Key + ":" + shipdata.Value);
                                }
                            }
                        }
                    }
                }
                catch { count = -1; }
            }
            LastUpdateTime = System.DateTime.Now;
            if (count == -1)
            {
                ElectronicObserver.Utility.Logger.Add(2, "字幕数据更新出现错误,稍后会再次检查更新");
            }
            else if (count == 0)
            {
                ElectronicObserver.Utility.Logger.Add(2, "字幕数据检查更新完成,无需更新");
            }
            else
            {
                ElectronicObserver.Utility.Logger.Add(2, string.Format("字幕数据更新完成({0}),更新了({1})条语音", data["version"], count));
                File.WriteAllText(LocalFile, JavaScriptSerializer.Serialize(data));
            }
            Updating = false;
        }

        public string GetVoice(int shipid, int voiceID)
        {
            CheckUpdate();
            lock (data)
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
            }
            return null;
        }

        public string GetVoice(string ShipCode, int FileName)
        {
            CheckUpdate();
            var ship = KCDatabase.Instance.MasterShips.Values.FirstOrDefault(e => { return e.ResourceName == ShipCode; });
            if (ship == null)
                return null;
            int shipid = ship.ShipID;
            string ShipName = ship.Name;
            lock (data)
            {
                if (voiceMap.ContainsKey(shipid) && data.ContainsKey(shipid.ToString()))
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
            }
            return IgnoreBlankSubtitles ? null : "[" + ShipName + "]: 这句语音还没有台词呢,到舰娘百科网站帮忙维护吧";
        }
    }
}
