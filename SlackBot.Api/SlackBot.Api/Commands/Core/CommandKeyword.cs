using System;
using System.Collections.Generic;

namespace SlackBot.Api.Commands.Core
{
    internal readonly struct CommandKeyword
    {
        public CommandPriority Priority { get; }
        public IKeywordFilter Filter { get; }

        public CommandKeyword(CommandPriority priority, IKeywordFilter filter)
        {
            Priority = priority;
            Filter = filter;
        }
    }

    internal sealed class CommandKeywordEqualityComparer : IEqualityComparer<CommandKeyword>
    {
        public static CommandKeywordEqualityComparer Default { get; } = new CommandKeywordEqualityComparer();

        private CommandKeywordEqualityComparer() { }

        public bool Equals(CommandKeyword x, CommandKeyword y)
        {
            return string.Equals(x.Filter.Word, y.Filter.Word, StringComparison.Ordinal);
        }

        public int GetHashCode(CommandKeyword obj)
        {
            return obj.Filter.Word.GetHashCode();
        }
    }
}
