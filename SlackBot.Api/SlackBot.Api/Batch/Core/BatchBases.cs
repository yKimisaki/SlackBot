using System;
using System.Threading.Tasks;

namespace SlackBot.Api.Batch.Core
{
    internal enum ScheduleType
    {
        None = 0,

        Daily = 10,
        Hourly = 20,
        Weekly = 30,
        Monthly = 40,
        EndOfMonth = 41,
    }

    internal interface IBatch
    {
        ValueTask<string> ExecuteAsync(DateTime currentExecutionTime, ref DateTime nextExecutionTime);
        string TargetChannelId { get; }
        ScheduleType ScheduleType { get; }
        TimeSpan ScheduledTime { get; }
    }

    internal abstract class HourlyBatch : IBatch
    {
        public abstract string TargetChannelId { get; }
        public ScheduleType ScheduleType => ScheduleType.Hourly;
        public TimeSpan ScheduledTime => new TimeSpan(0, ScheduledMinutes, 0);
        public abstract int ScheduledMinutes { get; }
        public abstract ValueTask<string> ExecuteAsync(DateTime currentExecutionTime, ref DateTime nextExecutionTime);
    }

    internal abstract class DailyBatch : IBatch
    {
        public abstract string TargetChannelId { get; }
        public ScheduleType ScheduleType => ScheduleType.Daily;
        TimeSpan IBatch.ScheduledTime => new TimeSpan(ScheduledTime.hours, ScheduledTime.minutes, 0);
        public abstract (int hours, int minutes) ScheduledTime { get; }
        public abstract ValueTask<string> ExecuteAsync(DateTime currentExecutionTime, ref DateTime nextExecutionTime);
    }

    internal abstract class WeeklyBatch : IBatch
    {
        public abstract string TargetChannelId { get; }
        public ScheduleType ScheduleType => ScheduleType.Weekly;
        TimeSpan IBatch.ScheduledTime => new TimeSpan((int)ScheduledTime.dayOfWeek, ScheduledTime.hours, ScheduledTime.minutes, 0);
        public abstract (DayOfWeek dayOfWeek, int hours, int minutes) ScheduledTime { get; }
        public abstract ValueTask<string> ExecuteAsync(DateTime currentExecutionTime, ref DateTime nextExecutionTime);
    }

    internal abstract class MonthlyBatch : IBatch
    {
        public virtual bool IsEndOfMonth { get; }
        public abstract string TargetChannelId { get; }
        public ScheduleType ScheduleType => ScheduleType.Monthly;
        TimeSpan IBatch.ScheduledTime => new TimeSpan(IsEndOfMonth ? -ScheduledTime.date + 1 : ScheduledTime.date, ScheduledTime.hours, ScheduledTime.minutes, 0);
        public abstract (int date, int hours, int minutes) ScheduledTime { get; }
        public abstract ValueTask<string> ExecuteAsync(DateTime currentExecutionTime, ref DateTime nextExecutionTime);
    }
}
