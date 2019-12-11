using SlackBot.Api.Commands.Core;
using System;
using System.Threading.Tasks;

namespace SlackBot.Api.Commands
{
    internal class DateCommand : CommandBase
    {
        public override bool IsBroadcast => false;
        public override (string, string) DefaultCommandAndHelpMessage => ("date", "日にちと曜日を表示");

        public DateCommand()
        {
            RegisterKeyword(CommandPriority.Low, "何日");
            RegisterKeyword(CommandPriority.Low, "日付");
            RegisterKeyword(CommandPriority.Low, "曜日");
        }

        public override ValueTask<string> CreateOutputAsync(string user, string channel, string filteredKeyword, string rawWords)
        {
            if (rawWords.Contains("一昨日") || rawWords.Contains("おととい") || rawWords.Contains("おとつい"))
            {
                return new ValueTask<string>($"明日は{(DateTime.Now - TimeSpan.FromDays(1)).ToString("gyyyy年MM月dd日(dddd)")}。");
            }
            else if (rawWords.Contains("昨日") || rawWords.Contains("前日") || rawWords.Contains("きのう"))
            {
                return new ValueTask<string>($"明日は{(DateTime.Now - TimeSpan.FromDays(1)).ToString("gyyyy年MM月dd日(dddd)")}。");
            }
            else if (rawWords.Contains("明日") || rawWords.Contains("翌日") || rawWords.Contains("あした") || rawWords.Contains("あす"))
            {
                return new ValueTask<string>($"明日は{(DateTime.Now + TimeSpan.FromDays(1)).ToString("gyyyy年MM月dd日(dddd)")}。");
            }
            else if (rawWords.Contains("明後日") || rawWords.Contains("あさって"))
            {
                return new ValueTask<string>($"明後日は{(DateTime.Now + TimeSpan.FromDays(2)).ToString("gyyyy年MM月dd日(dddd)")}。");
            }
            else if (rawWords.Contains("明々後日") || rawWords.Contains("明々後日") || rawWords.Contains("しあさって"))
            {
                return new ValueTask<string>($"明々後日は{(DateTime.Now + TimeSpan.FromDays(3)).ToString("gyyyy年MM月dd日(dddd)")}。");
            }
            else
            {
                return new ValueTask<string>($"今日は{DateTime.Now.ToString("gyyyy年MM月dd日(dddd)")}。");
            }
        }
    }
}

