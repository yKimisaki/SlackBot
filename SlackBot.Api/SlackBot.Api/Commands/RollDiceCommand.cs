using SlackBot.Api.Commands.Core;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Minamo.SlackBot.Models.Edge
{
    internal class RollDiceCommand : CommandBase
    {
        public override bool IsBroadcast => true;
        public override (string, string) DefaultCommandAndHelpMessage => ("dice", "ダイスコードを使用してサイコロを振る");

        public override CommandPriority GetPriority(string rawWords)
        {
            var match = new Regex(@"^(?!\d+$)(([1-9]\d*)?[Dd]?[1-9]\d*( ?[+-] ?)?)+(?<![+-] ?)$").Match(rawWords);

            if (match.Success)
            {
                return CommandPriority.High;
            }

            return CommandPriority.None;
        }

        public override ValueTask<string> CreateOutputAsync(string user, string channel, string filteredKeyword, string rawWords)
        {
            var dicecode = "2d6";
            if (rawWords != DefaultCommand)
            {
                switch (rawWords[0])
                {
                    case 'd':
                        rawWords = "1" + rawWords;
                        break;
                    case '+':
                        rawWords = "0" + rawWords;
                        break;
                    case '-':
                        rawWords = "0" + rawWords;
                        break;
                }
                dicecode = rawWords.Replace(" ", "").ToLower().Replace("+d", "+1d").Replace("-d", "-1d");
            }
            return new ValueTask<string>($"サイコロ {rawWords} の結果は {GetValue(dicecode)} 。");
        }

        public int GetValue(string command)
        {
            var value = 0;
            var index = 0;
            var random = new Random();
            foreach (var dCommandOrNum in command.Split('+', '-'))
            {
                var isAdd = true;
                if (index != 0 && index < command.Length)
                {
                    isAdd = command[index] != '-';
                }
                var currentValue = 0;
                if (dCommandOrNum.Contains('d'))
                {
                    var numAndSurface = dCommandOrNum.Split('d');
                    for(var i = 0; i < int.Parse(numAndSurface[0]); ++i)
                    {
                        currentValue += 1 + random.Next(0, int.Parse(numAndSurface[1]));
                    }
                    index += dCommandOrNum.Length;
                }
                else
                {
                    currentValue = int.Parse(dCommandOrNum);
                    index += dCommandOrNum.Length;
                }

                value += (isAdd ? +currentValue : -currentValue);
            }

            return value;
        }
    }
}
