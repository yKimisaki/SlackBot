using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

namespace SlackBot.Api.Commands.Core
{
    internal sealed class CommandSelector : ICommandSelector
    {
        public static CommandSelector Default { get; } = new CommandSelector();

        public IReadOnlyCollection<ICommand> AllCommands { get; }

        private CommandSelector()
        {
            AllCommands = Assembly.GetExecutingAssembly()
                   .GetTypes()
                   .Where(x => !x.IsInterface && !x.IsAbstract)
                   .Where(x => typeof(ICommand).IsAssignableFrom(x))
                   .Select(x => Activator.CreateInstance(x) as ICommand)
                   .Where(x => x != null)
                   .Select(x => x!)
                   .ToArray();
        }

        public ValueTask<(ICommand, CommandKeyword)> GetCommandAndKeywordAsync(string rowWords)
        {
            var fixedWordHit = AllCommands.FirstOrDefault(x => x.GetPriority(rowWords) != CommandPriority.None);
            var candidateHit = AllCommands
                .SelectMany(command =>
                {
                    // デフォルトコマンドと一致するならそちらを優先
                    var (DefaultCommand, _) = command.DefaultCommandAndHelpMessage;
                    if (DefaultCommand == rowWords)
                    {
                        return new[] { (command, new CommandKeyword(CommandPriority.Default, new AsLikeKeywordFilter(DefaultCommand))) };
                    }

                    // 無い場合はフィルタでマッチ
                    return command.Keywords
                        .Where(keyword => keyword.Filter.IsMatch(rowWords))
                        .Select(keyword => (command, keyword));
                })
                .OrderBy(x => x.keyword.Priority)
                .FirstOrDefault();

            if (fixedWordHit != null)
            {
                var fixedWordPriority = fixedWordHit.GetPriority(rowWords);
                if (fixedWordPriority >= candidateHit.keyword.Priority)
                {
                    return new ValueTask<(ICommand, CommandKeyword)>((fixedWordHit, new CommandKeyword(fixedWordPriority, new AsLikeKeywordFilter(fixedWordHit.DefaultCommandAndHelpMessage.Item1))));
                }
            }

            if (candidateHit.command != null)
            {
                return new ValueTask<(ICommand, CommandKeyword)>(candidateHit);
            }

            throw new CommandNotFoundException(rowWords);
        }
    }

    internal class CommandNotFoundException : Exception
    {
        public string RowWords { get; }
        public CommandNotFoundException(string rowWords) : base() => RowWords = rowWords;
    }
}

#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously