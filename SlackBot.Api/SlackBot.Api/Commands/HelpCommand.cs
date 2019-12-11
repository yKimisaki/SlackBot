using SlackBot.Api.Commands.Core;
using System.Linq;
using System.Threading.Tasks;

namespace SlackBot.Api.Commands
{
    internal class HelpCommand : CommandBase
    {
        public override bool IsBroadcast => false;
        public override (string, string) DefaultCommandAndHelpMessage => ("help", "");

        public override ValueTask<string> CreateOutputAsync(string user, string channel, string filteredKeyword, string rawWords)
        {
            return new ValueTask<string>("\n" + CommandSelector.Default.AllCommands
                .Where(x => !string.IsNullOrWhiteSpace(x.DefaultCommandAndHelpMessage.HelpMessage))
                .Select(x => $"{x.DefaultCommandAndHelpMessage.DefaultCommand}:  {x.DefaultCommandAndHelpMessage.HelpMessage}" + (x.IsBroadcast ? " (broadcast)" : " (only you)"))
                .Aggregate((x, y) => $"{x}\n{y}"));
        }
    }
}
