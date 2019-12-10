using System.Linq;

namespace SlackBot.Api.Commands.Core
{
    internal interface IKeywordFilter
    {
        string Word { get; }
        bool IsMatch(string rawWords);
        string GetFilterdKeyword(string rawWords);
    }

    internal sealed class AsLikeKeywordFilter : IKeywordFilter
    {
        public string Word { get; }
        public AsLikeKeywordFilter(string word) => Word = word;

        public bool IsMatch(string rawWords) => rawWords.Contains(Word);
        public string GetFilterdKeyword(string rawWords) => rawWords;
    }

    internal class FixAsLikeKeywordFilter : IKeywordFilter
    {
        public string Word { get; }
        private string[] prefixes { get; }
        private string[] surfixes { get; }

        public FixAsLikeKeywordFilter(string word, string[] prefixes, string[] surfixes)
        {
            Word = word;
            this.prefixes = prefixes;
            this.surfixes = surfixes;
        }

        public bool IsMatch(string rawWords) => rawWords.Contains(Word);
        public string GetFilterdKeyword(string rawWords)
        {
            // ほげほげのなになに、で～のなになにをwordに指定した場合、ほげほげを返す
            foreach (var withPrefix in prefixes.Select(x => x + Word))
            {
                if (rawWords.Contains(withPrefix))
                {
                    return rawWords.Substring(0, rawWords.Length - withPrefix.Length);
                }
            }

            // ほげほげのなになに、でほげほげの～をwordに指定した場合、なになにを返す
            foreach (var withSurfix in surfixes.Select(x => Word + x))
            {
                if (rawWords.Contains(withSurfix))
                {
                    return rawWords.Substring(withSurfix.Length);
                }
            }

            return rawWords.Substring(Word.Length);
        }
    }
}
