using System.Collections.Generic;
using System.Threading.Tasks;

namespace SlackBot.Api.Commands.Core
{
    internal interface ICommand
    {
        bool IsBroadcast { get; }
        (string DefaultCommand, string HelpMessage) DefaultCommandAndHelpMessage { get; }
        CommandPriority GetPriority(string rawWord);
        IReadOnlyCollection<CommandKeyword> Keywords { get; }
        ValueTask<string> CreateOutputAsync(string user, string channel, string filteredKeyword, string rawWords);
    }
}
