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
    public enum SubtitleLanguage
    {
        chs = 0, jp = 1, both = 2
    }
    public class VoiceData
    {
        int[] voiceKey = new int[] { 604825, 607300, 613847, 615318, 624009, 631856, 635451, 637218, 640529, 643036, 652687, 658008, 662481, 669598, 675545, 685034, 687703, 696444, 702593, 703894, 711191, 714166, 720579, 728970, 738675, 740918, 743009, 747240, 750347, 759846, 764051, 770064, 773457, 779858, 786843, 790526, 799973, 803260, 808441, 816028, 825381, 827516, 832463, 837868, 843091, 852548, 858315, 867580, 875771, 879698, 882759, 885564, 888837, 896168 };
        Dictionary<int, Dictionary<int, int>> voiceMap = new Dictionary<int, Dictionary<int, int>>();
        List<Voice> Voices = new List<Voice>();
      
        public static string ConfigFile;
        public static bool UseThirdBuffer = false;
        public static bool IgnoreBlankSubtitles = false;
        public static SubtitleDisplayArea subtitleDisplayArea = SubtitleDisplayArea.DockForm;
        public static SubtitleLanguage subtitleLanguage = SubtitleLanguage.chs;
        public static int MaxLines = 10;

        public void Init()
        {

            ConfigFile = System.Windows.Forms.Application.StartupPath + "\\Settings\\VoiceSubtitle.xml";
            JavaScriptSerializer Serializer = new JavaScriptSerializer();
            Serializer.MaxJsonLength = Serializer.MaxJsonLength * 2;
            Voices.Add(new Voice("简体中文", "http://api.kcwiki.moe/subtitles/diff/"));
            Voices.Add(new Voice("日文", "http://api.kcwiki.moe/subtitles/jp/"));
            Voices[0].LocalFile = System.Windows.Forms.Application.StartupPath + "\\Settings\\Subtitles.json";
            Voices[1].LocalFile = System.Windows.Forms.Application.StartupPath + "\\Settings\\Subtitles.jp.json";
            Voices[0].InternalData = Properties.Resources.subtitles;
            Voices[1].InternalData = Properties.Resources.Subtitles_jp;
            Voices[1].CouldUpdate = false;
            for (int index = 0; index < Voices.Count; index++)
            {
                if (File.Exists(Voices[index].LocalFile))
                {
                    var localjson = File.ReadAllText(Voices[index].LocalFile);
                    try
                    {
                        var localdata = Serializer.DeserializeObject(localjson) as Dictionary<string, object>;
                        if (localdata != null)
                        {
                            Voices[index].VoiceData = localdata;
                        }
                    }
                    catch { }
                }
              
                if (Voices[index].VoiceData == null)
                {
                    string s = Encoding.UTF8.GetString(Voices[index].InternalData);
                    Voices[index].VoiceData = (Dictionary<string, object>)Serializer.DeserializeObject(s);
                }

                if (Voices[index].CouldUpdate)
                {
                    Voices[index].Updating = true;
                    Task.Factory.StartNew(UpdateVoice, Voices[index]);
                }
            }

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
            foreach (var voice in Voices)
            {
                if (!voice.Updating && voice.CouldUpdate)
                {
                    voice.Updating = true;
                    if (System.DateTime.Now > voice.LastUpdateTime.AddHours(12))
                    {
                        Task.Factory.StartNew(UpdateVoice, voice);
                    }
                }
            }
        }

        void UpdateVoice(object oi)
        {
            Voice voice = (Voice)oi;
            string LocalVer = voice.VoiceData["version"].ToString();
            ElectronicObserver.Utility.Logger.Add(2, string.Format("开始检查{1}字幕更新数据,当前版本是({0})", LocalVer, voice.Language));
            string Url = voice.UpdateUrl + LocalVer;
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
                        lock (voice.VoiceData)
                        {
                            foreach (var shipdata in diff)
                            {
                                if (shipdata.Value is Dictionary<string, object>)
                                {
                                    var shipvoice = shipdata.Value as Dictionary<string, object>;
                                    count += shipvoice.Count;
                                    if (!voice.VoiceData.ContainsKey(shipdata.Key))
                                    {
                                        voice.VoiceData[shipdata.Key] = shipvoice;
                                    }
                                    else
                                    {
                                        var LocalVoices = voice.VoiceData[shipdata.Key] as Dictionary<string, object>;
                                        foreach (var Singlevoice in shipvoice)
                                        {
                                            LocalVoices[Singlevoice.Key] = Singlevoice.Value;
                                            //File.AppendAllText(@"d:\1.txt", voice.Key + ":" + voice.Value);
                                        }
                                    }
                                }
                                else
                                {
                                    voice.VoiceData[shipdata.Key] = shipdata.Value;
                                    //File.AppendAllText(@"d:\1.txt", shipdata.Key + ":" + shipdata.Value);
                                }
                            }
                        }
                    }
                }
                catch { count = -1; }
            }
            voice.LastUpdateTime = System.DateTime.Now;
            if (count == -1)
            {
                ElectronicObserver.Utility.Logger.Add(2, string.Format("{0}字幕数据更新出现错误,稍后会再次检查更新", voice.Language));
            }
            else if (count == 0)
            {
                ElectronicObserver.Utility.Logger.Add(2, string.Format("{0}字幕数据检查更新完成,无需更新", voice.Language));
            }
            else
            {
                ElectronicObserver.Utility.Logger.Add(2, string.Format("{2}字幕数据更新完成({0}),更新了({1})条语音", voice.VoiceData["version"], count, voice.Language));
                File.WriteAllText(voice.LocalFile, JavaScriptSerializer.Serialize(voice.VoiceData));
            }
            voice.Updating = false;
        }
        public string GetVoice(int shipid, int voiceID )
        {
            CheckUpdate();
            StringBuilder builder = new StringBuilder();
            var ShipName = KCDatabase.Instance.MasterShips[shipid].Name;
            builder.Append("[" + ShipName + "]: ");
            string chsSubtitle, jpSubtitls;
            switch (subtitleLanguage)
            {
                case SubtitleLanguage.chs:
                    chsSubtitle = GetVoice(shipid, voiceID, Voices[0]);
                    if (string.IsNullOrWhiteSpace(chsSubtitle))
                        return null;
                    builder.Append(chsSubtitle); ;
                    break;
                case SubtitleLanguage.jp:
                    jpSubtitls = GetVoice(shipid, voiceID, Voices[1]);
                    if (string.IsNullOrWhiteSpace(jpSubtitls))
                        return null;
                    builder.Append(jpSubtitls);
                    break;
                case SubtitleLanguage.both:
                    chsSubtitle = GetVoice(shipid, voiceID, Voices[0]);
                    jpSubtitls = GetVoice(shipid, voiceID, Voices[1]);
                    bool chsExist = !string.IsNullOrWhiteSpace(chsSubtitle);
                    bool jpExist = !string.IsNullOrWhiteSpace(jpSubtitls);
                    if (chsExist)
                        builder.Append(chsSubtitle);
                    else if (!jpExist)
                    {
                        return null;
                    }
                    if (chsExist && jpExist)
                        builder.Append("\r\n(");
                    if (jpExist)
                        builder.Append(jpSubtitls);
                    if (chsExist && jpExist)
                        builder.Append(")");
                    break;
            }
            return builder.ToString();
        }
        public string GetVoice(int shipid, int voiceID, Voice voice)
        {
            lock (voice)
            {
                if (voiceMap.ContainsKey(shipid) && voice.VoiceData.ContainsKey(shipid.ToString()))
                {
                    string voiceid = voiceID.ToString();

                    var voices = (Dictionary<string, object>)voice.VoiceData[shipid.ToString()];
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
            var ship = KCDatabase.Instance.MasterShips.Values.FirstOrDefault(e => { return e.ResourceName == ShipCode; });
            if (ship == null)
                return null;
            int shipid = ship.ShipID;
            if (voiceMap.ContainsKey(shipid) && voiceMap[shipid].ContainsKey(FileName))
                return GetVoice(shipid, voiceMap[shipid][FileName]);
            return null;
        }
        
        
    }

    public class Voice
    {
        public string Language { get; set; }
        public string UpdateUrl { get; set; }
        public Dictionary<string, object> VoiceData { get; set; }
        public bool Updating = false;
        public bool CouldUpdate = true;
        public System.DateTime LastUpdateTime { get; set; }
        public string LocalFile { get; set; }
        public byte[] InternalData;

        public Voice(string language, string updateUrl)
        {
            Language = language;
            UpdateUrl = updateUrl;
        }
    }
}
