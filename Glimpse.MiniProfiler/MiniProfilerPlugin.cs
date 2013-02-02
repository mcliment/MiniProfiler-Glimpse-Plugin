﻿using System.Collections.Generic;
using System.Web;
using Glimpse.Core.Extensibility;
using StackExchange.Profiling;

namespace Glimpse.MiniProfiler
{
    [GlimpsePlugin()]
    public class MiniProfilerPlugin : IGlimpsePlugin
    {
        public object GetData(HttpContextBase application)
        {
            if (StackExchange.Profiling.MiniProfiler.Current == null)
            {
                return null;
            }

            var profiler = StackExchange.Profiling.MiniProfiler.Current;

            return FormatTimings(new[] {profiler.Root}, decimal.ToDouble(profiler.DurationMilliseconds));
        }

        public void SetupInit()
        {
            // Nothing
        }

        public string Name
        {
            get { return "Profiler"; }
        }

        private List<object[]> FormatTimings(IEnumerable<Timing> timings, double? parentDuration)
        {
            if (timings == null)
            {
                return null;
            }

            var result = new List<object[]>
                             {
                                 new[]
                                     {
                                         "Name", "Children", "Duration", "FromStart", "Queries", "QueryTime"
                                     }
                             };

            foreach (var timing in timings)
            {
                result.Add(new object[]
                               {
                                   timing.Name,
                                   FormatTimings(timing.Children, timing.DurationMilliseconds.HasValue ? decimal.ToDouble(timing.DurationMilliseconds.Value) : (double?) null),
                                   string.Format("{0} ms", parentDuration),
                                   string.Format("T+{0} ms", timing.StartMilliseconds),
                                   FormatSqlTiming(timing.SqlTimings),
                                   string.Format("{0} ms", timing.SqlTimingsDurationMilliseconds)
                               });
            }

            return result;
        }

        private List<object[]> FormatSqlTiming(IEnumerable<SqlTiming> timings)
        {
            if (timings == null)
            {
                return null;
            }

            var result = new List<object[]>
                             {
                                 new object[]
                                     {
                                         "Type", "Command", "Start", "Duration"
                                     }
                             };

            foreach (var timing in timings)
            {
                result.Add(new object[]
                               {
                                   timing.ExecuteType.ToString(),
                                   timing.CommandString,
                                   string.Format("T+{0} ms", timing.StartMilliseconds),
                                   string.Format("{0} ms ({1} ms)", timing.DurationMilliseconds, timing.FirstFetchDurationMilliseconds),
                               });
            }

            return result;
        }
    }
}