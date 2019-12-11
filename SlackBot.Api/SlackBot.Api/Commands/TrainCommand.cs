using SlackBot.Api.Commands.Core;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SlackBot.Api.Commands
{
    internal class TrainCommand : CommandBase
    {
        public override bool IsBroadcast => true;
        public override (string, string) DefaultCommandAndHelpMessage => ("train", "関東圏の鉄道の遅延情報");

        private IReadOnlyDictionary<string, string> MetroTargets = new Dictionary<string, string>()
        {
            { "銀座線", "https://www.tokyometro.jp/unkou/history/ginza.html" },
            { "丸ノ内線", "https://www.tokyometro.jp/unkou/history/marunouchi.html" },
            { "日比谷線", "https://www.tokyometro.jp/unkou/history/hibiya.html" },
            { "東西線", "https://www.tokyometro.jp/unkou/history/touzai.html" },
            { "千代田線", "https://www.tokyometro.jp/unkou/history/chiyoda.html" },
            { "有楽町線", "https://www.tokyometro.jp/unkou/history/yurakucho.html" },
            { "半蔵門線", "https://www.tokyometro.jp/unkou/history/hanzoumon.html" },
            { "南北線", "https://www.tokyometro.jp/unkou/history/nanboku.html" },
            { "副都心線", "https://www.tokyometro.jp/unkou/history/fukutoshin.html" },
        };

        private IReadOnlyDictionary<string, string> OthersTargets = new Dictionary<string, string>() 
        { 
            { "JR東日本", "https://traininfo.jreast.co.jp/train_info/kanto.aspx" },
            { "東京急行電鉄", "https://www.tokyu.co.jp/unten2/unten.html" },
            { "西武鉄道", "https://www.seiburailway.jp/railwayinfo/" },
            { "東武鉄道", "http://tra-rep.tobu.jp/index.html" },
            { "京急", "https://unkou.keikyu.co.jp/" },
            { "都営地下鉄", "https://www.kotsu.metro.tokyo.jp/subway/schedule/" },
            { "京成電鉄", "http://www.jikokuhyo.co.jp/search/detail/line_is/kanto_keisei" },
            { "京王電鉄", "https://www.keio.co.jp/unkou/unkou_pc.html" },
            { "小田急電鉄", "https://www.odakyu.jp/cgi-bin/user/emg/emergency_bbs.pl?20161014045003" },
            { "東京モノレール", "http://www.jikokuhyo.co.jp/search/detail/line_is/kanto_tokyomonorail" },
            { "りんかい線", "https://service.twr.co.jp/service_info/information.html" },
            { "日暮里･舎人ライナー", "https://www.kotsu.metro.tokyo.jp/nippori_toneri/schedule/" },
            { "相模鉄道", "https://www.sotetsu.co.jp/train/status/" },
            { "ゆりかもめ", "https://www.yurikamome.co.jp/ride-guidance/operation.html" },
            { "つくばエクスプレス", "http://www.mir.co.jp/info/" },
            { "多摩モノレール", "https://www.tama-monorail.co.jp/monorail/operation/" },
            { "千葉モノレール", "https://chiba-monorail.co.jp/index.php/info-detail/" },
            { "横浜市交通局", "https://www.city.yokohama.lg.jp/kotsu/" },
            { "湘南モノレール", "https://www.city.yokohama.lg.jp/kotsu/" },
            { "横浜シーサイドライン", "https://www.seasideline.co.jp/guidance/train_info/" },
        };

        public TrainCommand()
        {
            RegisterKeyword(CommandPriority.Middle, "電車");
            RegisterKeyword(CommandPriority.Middle, "鉄道");
            RegisterKeyword(CommandPriority.Middle, "遅延");
            RegisterKeyword(CommandPriority.Middle, "運行");
        }

        public override async ValueTask<string> CreateOutputAsync(string user, string channel, string filteredKeyword, string rawWords)
        {
            using (var httpClient = new HttpClient())
            {
                var result = await httpClient.GetAsync("https://tetsudo.rti-giken.jp/free/delay.json");
                var parsed = await Utf8Json.JsonSerializer.DeserializeAsync<TrainInfoResult[]>(await result.Content.ReadAsStreamAsync());

                if (!parsed.Any())
                {
                    return $"今は関東各線の遅延情報はありません。";
                }

                var metro = parsed.Where(x => x.company == "東京メトロ").ToArray();
                var others = parsed.Where(x => x.company != "東京メトロ").ToArray();

                var sb = new StringBuilder();
                sb.AppendLine($"関東各線は以下で遅延しています。");
                foreach (var m in metro)
                {
                    if (m.name == null)
                    {
                        continue;
                    }

                    if (!MetroTargets.ContainsKey(m.name))
                    {
                        continue;
                    }
                    sb.AppendLine($"東京メトロ {m.name} {MetroTargets[m.name]}");
                }
                foreach (var o in others)
                {
                    if (o.name == null || o.company == null)
                    {
                        continue;
                    }

                    if (!OthersTargets.ContainsKey(o.company))
                    {
                        continue;
                    }
                    sb.AppendLine($"{o.company} {o.name} {OthersTargets[o.company]}");
                }
                return sb.ToString();
            }
        }
    }

#pragma warning disable CS0649
    [DataContract]
    public struct TrainInfoResult
    {
        [DataMember]
        public string? name;
        [DataMember]
        public string? company;
        [DataMember]
        public int lastupdate_gmt;
        [DataMember]
        public string? source;
    }
#pragma warning restore CS0649
}
