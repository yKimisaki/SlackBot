

namespace SlackBot.Api.Commands.Core
{
    internal enum CommandPriority
    {
        None =  0,

        Low = 30,
        Middle = 50,
        High = 70,

        Default = 90,
    }
}
