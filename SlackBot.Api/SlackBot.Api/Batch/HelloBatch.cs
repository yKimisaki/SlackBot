using SlackBot.Api.Batch.Core;
using SlackBot.Api.Extensions;
using System;
using System.Threading.Tasks;

namespace SlackBot.Api.Batch
{
    internal class HelloBatch : IBatch
    {
        public ScheduleType ScheduleType => ScheduleType.Daily;
        public TimeSpan ScheduledTime => new TimeSpan(10, 0, 0);
        public string TargetChannelId => "";

        public ValueTask<string> ExecuteAsync(DateTime currentExecutionTime, ref DateTime nextExecutionTime)
        {
            if (currentExecutionTime.DayOfWeek == DayOfWeek.Sunday || 
                currentExecutionTime.DayOfWeek == DayOfWeek.Saturday ||
                currentExecutionTime.IsHoliday())
            {
                return default;
            }

            if (currentExecutionTime.Month == 1 && currentExecutionTime.Day == 1)
            {
                return new ValueTask<string>($"A happy new year!");
            }

            return new ValueTask<string>($"Hello!");
        }
    }
}
