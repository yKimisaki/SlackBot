using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SlackBot.Api.Commands.Core
{
    internal abstract class CommandBase : ICommand
    {
        public abstract bool IsBroadcast { get; }
        public abstract (string, string) DefaultCommandAndHelpMessage { get; }
        protected string DefaultCommand => DefaultCommandAndHelpMessage.Item1;

        private HashSet<CommandKeyword> keywords = new HashSet<CommandKeyword>(CommandKeywordEqualityComparer.Default);
        public IReadOnlyCollection<CommandKeyword> Keywords => keywords;

        public virtual CommandPriority GetPriority(string rawWords)
        {
            return CommandPriority.None;
        }

        protected void RegisterKeyword(CommandPriority priority, string word)
        {
            RegisterKeyword(priority, new AsLikeKeywordFilter(word));
        }

        protected void RegisterKeyword(CommandPriority priority, string word, string[] surfixes)
        {
            RegisterKeyword(priority, new FixAsLikeKeywordFilter(word, Array.Empty<string>(), surfixes));
        }

        protected void RegisterKeyword(CommandPriority priority, string[] prefixes, string word)
        {
            RegisterKeyword(priority, new FixAsLikeKeywordFilter(word, prefixes, Array.Empty<string>()));
        }

        protected void RegisterKeyword(CommandPriority priority, string[] prefixes, string word, string[] surfixes)
        {
            RegisterKeyword(priority, new FixAsLikeKeywordFilter(word, prefixes, surfixes));
        }

        protected void RegisterKeyword(CommandPriority priority, IKeywordFilter keywordFilter)
        {
            keywords.Add(new CommandKeyword(priority, keywordFilter));
        }

        public abstract ValueTask<string> CreateOutputAsync(string user, string channel, string filteredKeyword, string rawWords);
    }
}
