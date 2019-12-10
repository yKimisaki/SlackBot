using System.Threading.Tasks;

namespace SlackBot.Api.Commands.Core
{
    internal interface ICommandSelector
    {
        ValueTask<(ICommand, CommandKeyword)> GetCommandAndKeywordAsync(string keyword);
    }
}
