using SlackBot.Api.Commands.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Minamo.SlackBot.Models.Edge
{
    internal class LotCommand : CommandBase
    {
        public override bool IsBroadcast => true;
        public override (string, string) DefaultCommandAndHelpMessage => ("lot", "今日の運勢");

        private static readonly IReadOnlyList<string> results = new[]
        {
            "大吉",
            "吉",
            "中吉",
            "末吉",
            "凶",
        };

        public LotCommand()
        {
            RegisterKeyword(CommandPriority.Middle, "運勢");
            RegisterKeyword(CommandPriority.Middle, "くじ");
            RegisterKeyword(CommandPriority.Middle, "うらな", new[] { "って" });
            RegisterKeyword(CommandPriority.Middle, "占", new[] { "って" });
        }

        public override ValueTask<string> CreateOutputAsync(string user, string channel, string filteredKeyword, string rawWords)
        {
            var index = Math.Abs((user + DateTime.Now.Date.ToString()).GetHashCode()) % results.Count;
            return new ValueTask<string>($"今日のあなたの運勢は{results[index]}よ！");
        }
    }
}

