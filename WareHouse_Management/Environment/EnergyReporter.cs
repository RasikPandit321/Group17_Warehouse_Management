using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WareHouse_Management.Environment
{
    public static class EnergyReporter
    {
        public static EnergyReport ComputeFromSamples(
            IReadOnlyList<EnergySample> samples,
            double sampleIntervalSeconds)
        {
            if (samples == null)
                throw new ArgumentNullException(nameof(samples));

            if (samples.Count == 0)
                throw new ArgumentException("No samples provided.", nameof(samples));

            if (sampleIntervalSeconds <= 0)
                throw new ArgumentOutOfRangeException(nameof(sampleIntervalSeconds), "Interval must be positive.");

            // Average temperature
            double avgTemp = samples.Average(s => s.Temperature);

            // Fan ON samples
            int onSamples = 0;
            var streaks = new List<int>();
            int currentStreak = 0;

            foreach (var s in samples)
            {
                if (s.FanOn)
                {
                    onSamples++;
                    currentStreak++;
                }
                else
                {
                    if (currentStreak > 0)
                    {
                        streaks.Add(currentStreak);
                        currentStreak = 0;
                    }
                }
            }

            if (currentStreak > 0)
                streaks.Add(currentStreak);

            double totalOnSeconds = onSamples * sampleIntervalSeconds;

            double avgRuntimeSeconds =
                streaks.Count > 0 ? streaks.Average() * sampleIntervalSeconds : 0;

            double fanPercent = (double)onSamples / samples.Count * 100.0;

            double score = CalculateEnergyScore(avgTemp, fanPercent);

            return new EnergyReport
            {
                AverageTemperature = avgTemp,
                AverageFanRuntimeSeconds = avgRuntimeSeconds,
                TotalFanOnSeconds = totalOnSeconds,
                FanOnPercent = fanPercent,
                EnergyScore = score,
                Timestamp = DateTime.Now
            };
        }

        public static string SaveToCsv(EnergyReport report, string? directory = null)
        {
            if (report == null)
                throw new ArgumentNullException(nameof(report));

            if (directory == null)
                directory = AppContext.BaseDirectory;

            string fileName = $"energy_report_{report.Timestamp:yyyyMMdd_HHmmss}.csv";
            string path = Path.Combine(directory, fileName);

            using var writer = new StreamWriter(path);
            writer.WriteLine("Date,AvgTempC,AvgFanRuntimeSeconds,TotalFanOnSeconds,FanOnPercent,EnergyScore");
            writer.WriteLine(
                $"{report.Timestamp:yyyy-MM-dd}," +
                $"{report.AverageTemperature:F2}," +
                $"{report.AverageFanRuntimeSeconds:F2}," +
                $"{report.TotalFanOnSeconds:F2}," +
                $"{report.FanOnPercent:F1}," +
                $"{report.EnergyScore:F1}");

            return path;
        }

        private static double CalculateEnergyScore(double avgTemp, double fanPercent)
        {
            double score = 100;

            // Simple, explainable scoring model for slides:
            score -= (avgTemp - 22.0);   // hotter warehouse → lower score
            score -= (fanPercent / 2.0); // more fan usage → lower score

            if (score < 0) score = 0;
            if (score > 100) score = 100;

            return score;
        }
    }
}