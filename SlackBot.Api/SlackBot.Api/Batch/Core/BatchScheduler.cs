using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace SlackBot.Api.Batch.Core
{
    internal readonly struct BatchExecutionResult : IEquatable<BatchExecutionResult>
    {
        public static BatchExecutionResult Invalid { get; } = new BatchExecutionResult();

        public bool IsInvalid { get { return this == Invalid; } }

        public string CommandName { get; }
        public string Output { get; }
        public string ChannelId { get; }

        public BatchExecutionResult(string commanedName, string output, string channel)
        {
            CommandName = commanedName;
            Output = output;
            ChannelId = channel;
        }

        public bool Equals(BatchExecutionResult other)
        {
            return CommandName == other.CommandName
                && Output == other.Output
                && ChannelId == other.ChannelId;
        }

        public override bool Equals(object? obj)
        {
            if (!(obj is BatchExecutionResult))
            {
                return false;
            }

            return Equals((BatchExecutionResult)obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(BatchExecutionResult left, BatchExecutionResult right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(BatchExecutionResult left, BatchExecutionResult right)
        {
            return !left.Equals(right);
        }
    }

    internal class ScheduledBatch
    {
        public bool IsValid { get { return Batch != null && NextExecutionTime.Ticks > 0; } }

        public int BatchId { get; }
        public IBatch Batch { get; }
        public DateTime RegisteredTime { get; }
        public DateTime NextExecutionTime { get; private set; }

        public ScheduledBatch(IBatch batch, DateTime now)
        {
            BatchId = Guid.NewGuid().GetHashCode();
            Batch = batch;
            RegisteredTime = now;
            NextExecutionTime = GetNextTime(batch, now);
        }

        public async ValueTask<BatchExecutionResult> ExecuteAsync(DateTime now)
        {
            if (now < NextExecutionTime)
            {
                return BatchExecutionResult.Invalid;
            }

            var nextExecutionTime = GetNextTime(Batch, now);
            var batchName = Batch.GetType().Name;
            var output = await Batch.ExecuteAsync(now, ref nextExecutionTime);
            NextExecutionTime = nextExecutionTime;

            if (string.IsNullOrWhiteSpace(output))
            {
                Console.WriteLine($"{batchName} is executed, but output is empty.");
                return BatchExecutionResult.Invalid;
            }

            Console.WriteLine($"{batchName} is executed, and will execute on {nextExecutionTime}.");
            return new BatchExecutionResult(batchName, output, Batch.TargetChannelId);
        }

        private static DateTime GetNextTime(IBatch batch, DateTime now)
        {
            var result = new DateTime();
            switch (batch.ScheduleType)
            {
                case ScheduleType.Daily:
                    result = batch.ScheduledTime > now.TimeOfDay
                        ? now.Date + batch.ScheduledTime
                        : now.Date + TimeSpan.FromDays(1) + batch.ScheduledTime;
                    break;
                case ScheduleType.Hourly:
                    result = batch.ScheduledTime.Minutes > now.TimeOfDay.Minutes
                        ? now.Date + batch.ScheduledTime + TimeSpan.FromHours(now.Hour)
                        : now.Date + TimeSpan.FromHours(1 + now.Hour) + batch.ScheduledTime;
                    break;
                case ScheduleType.Weekly:
                    result = batch.ScheduledTime > now.TimeOfDay + TimeSpan.FromDays((int)now.DayOfWeek)
                        ? now.Date - TimeSpan.FromDays((int)now.DayOfWeek) + batch.ScheduledTime
                        : now.Date + TimeSpan.FromDays(7 - (int)now.DayOfWeek) + batch.ScheduledTime;
                    break;
                case ScheduleType.Monthly:
                    result = batch.ScheduledTime > now.TimeOfDay + TimeSpan.FromDays(now.Day)
                        ? now.Date - TimeSpan.FromDays(now.Day) + batch.ScheduledTime
                        : now.Date + TimeSpan.FromDays(DateTime.DaysInMonth(now.Year, now.Month) - now.Day) + batch.ScheduledTime;
                    break;
                case ScheduleType.EndOfMonth when DateTime.DaysInMonth(now.Year, now.Month) == now.Day:
                    if (batch.ScheduledTime > now.TimeOfDay)
                    {
                        result = now.Date + batch.ScheduledTime;
                        break;
                    }
                    if (now.Month == 12)
                    {
                        result = new DateTime(now.Year + 1, 1, DateTime.DaysInMonth(now.Year + 1, 1)) + batch.ScheduledTime;
                        break;
                    }
                    result = new DateTime(now.Year, now.Month + 1, DateTime.DaysInMonth(now.Year, now.Month + 1)) + batch.ScheduledTime;
                    break;
                case ScheduleType.EndOfMonth:
                    result = new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month)) + batch.ScheduledTime;
                    break;
            }

            return result;
        }
    }

    internal class ScheduledBatchEqualityComparer : IEqualityComparer<ScheduledBatch>
    {
        public static ScheduledBatchEqualityComparer Default { get; } = new ScheduledBatchEqualityComparer();

        private ScheduledBatchEqualityComparer() { }

        public bool Equals(ScheduledBatch x, ScheduledBatch y)
        {
            return x.BatchId == y.BatchId;
        }

        public int GetHashCode(ScheduledBatch obj)
        {
            return obj.GetHashCode();
        }
    }

    internal class BatchScheduler : IDisposable
    {
        private HashSet<ScheduledBatch>? batches;
        private Task? task;
        private HttpClient httpClient = new HttpClient();

        private BatchScheduler() { }

        public static BatchScheduler Current { get; } = new BatchScheduler();

        public void Dispose()
        {
            batches?.Clear();
            task?.Dispose();
            httpClient.Dispose();
        }

        public void Start()
        {
            var now = DateTime.Now;

            batches = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(x => !x.IsInterface && !x.IsAbstract)
                .Where(x => typeof(IBatch).IsAssignableFrom(x))
                .Select(x => Activator.CreateInstance(x) as IBatch)
                .Where(x => x != null)
                .Select(x => new ScheduledBatch(x!, now))
                .ToHashSet(ScheduledBatchEqualityComparer.Default);

            task = Task.Run(RunShedulerAsync);
        }

        private async Task RunShedulerAsync()
        {
            if (batches == null || !batches.Any())
            {
                return;
            }

            var lastExecutionTime = default(DateTime);
            var nextTime = DateTime.Now;

            while (true)
            {
                batches.RemoveWhere(x => !x.IsValid);

                while (true)
                {
                    if (nextTime < DateTime.Now)
                    {
                        break;
                    }

                    await Task.Delay(TimeSpan.FromSeconds(1));
                }

                var now = DateTime.Now;
                lastExecutionTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, 0);

                foreach (var batch in batches)
                {
                    var result = await batch.ExecuteAsync(lastExecutionTime);
                    if (result.IsInvalid)
                    {
                        continue;
                    }

                    var response = new Dictionary<string, string>
                        {
                            { "token",  AppSettings.AccessToken },
                            { "channel", result.ChannelId },
                            { "text", $"{result.Output}" },
                            { "username", AppSettings.BotName },
                            { "reply_broadcast", "true" },
                        };

                    var content = new FormUrlEncodedContent(response);
                
                    Console.WriteLine($"Command: text: {result.CommandName}, channel: {result.ChannelId}, output: {result.Output}");
                    await httpClient.PostAsync("https://slack.com/api/chat.postMessage", content);
                }

                var executedTime = DateTime.Now - lastExecutionTime;
                nextTime = lastExecutionTime + TimeSpan.FromMinutes(executedTime.Minutes + 1);
            }
        }
    }
}
