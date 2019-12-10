using SlackBot.Api.Commands.Core;
using System;
using System.Threading.Tasks;

namespace Minamo.SlackBot.Models.Edge
{
    internal class TimeCommand : CommandBase
    {
        public override bool IsBroadcast => false;
        public override (string, string) DefaultCommandAndHelpMessage => ("time", "現在の時刻を表示");

        public TimeCommand()
        {
            RegisterKeyword(CommandPriority.Low, "何時");
            RegisterKeyword(CommandPriority.Low, "時刻");
        }

        public override ValueTask<string> CreateOutputAsync(string user, string channel, string filteredKeyword, string rawWords)
        {
            return new ValueTask<string>($"今は{DateTime.Now.ToString("tthh時mm分ss秒")}。");
        }
    }
}
