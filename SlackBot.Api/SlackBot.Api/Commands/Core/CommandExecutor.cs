using System.Threading.Tasks;

namespace SlackBot.Api.Commands.Core
{
    internal readonly struct CommandExecutorResult
    {
        public string CommandName { get; }
        public string Output { get; }
        public string RawWords { get; }
        public string FilteredKeyword { get; }
        public string UserName { get; }
        public string ChannelId { get; }
        public bool IsBroadcast { get; }

        public CommandExecutorResult(string commanedName, string output, string rawWords, string filteredKeyword, string user, string channel, bool isBroadcast)
        {
            CommandName = commanedName;
            Output = output;
            RawWords = rawWords;
            FilteredKeyword = filteredKeyword;
            UserName = user;
            ChannelId = channel;
            IsBroadcast = isBroadcast;
        }
    }

    internal static class CommandExecutor
    {
        public static ValueTask<CommandExecutorResult> ExecuteAsync(string user, string channel, string rawWords)
        {
            return ExecuteAsync(user, channel, rawWords, CommandSelector.Default);
        }

        public static async ValueTask<CommandExecutorResult> ExecuteAsync<T>(string user, string channel, string rawWords, T commandSelector) where T : ICommandSelector
        {
            var (command, keywords) = await commandSelector.GetCommandAndKeywordAsync(rawWords);
            var filteredKeyword = keywords.Filter.GetFilterdKeyword(rawWords);
            
            var output = await command.CreateOutputAsync(user, channel, filteredKeyword, rawWords);

            return new CommandExecutorResult(command.GetType().Name, output, rawWords, filteredKeyword, user, channel, command.IsBroadcast);
        }
    }
}
