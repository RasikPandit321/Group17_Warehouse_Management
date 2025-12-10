using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WareHouse_Management.Environment
{
    public static class EnergyReporter
    {
        // Computes an EnergyReport from raw energy samples and a fixed sample interval
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

            // Compute average temperature across all samples
            double avgTemp = samples.Average(s => s.Temperature);

            // Count fan ON samples and calculate ON streaks (continuous ON intervals)
            int onSamples = 0;
            var streaks = new List<int>();
            int currentStreak = 0;

            foreach (var s in samples)
            {
                if (s.FanOn)
                {
                    onSamples++;
                    currentStreak++; // extend current ON sequence
                }
                else
                {
                    // When fan turns OFF, store streak and reset counter
                    if (currentStreak > 0)
                    {
                        streaks.Add(currentStreak);
                        currentStreak = 0;
                    }
                }
            }

            // Add last streak if the final samples ended with fan ON
            if (currentStreak > 0)
                streaks.Add(currentStreak);

            // Convert ON sample count to total seconds
            double totalOnSeconds = onSamples * sampleIntervalSeconds;

            // Compute average streak runtime in seconds
            double avgRuntimeSeconds =
                streaks.Count > 0 ? streaks.Average() * sampleIntervalSeconds : 0;

            // Percentage of time fan was ON
            double fanPercent = (double)onSamples / samples.Count * 100.0;

            // Compute simple energy score (lower is worse, 100 max)
            double score = CalculateEnergyScore(avgTemp, fanPercent);

            // Package result into a report structure
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

        // Saves the given report into a CSV file and returns the file path
        public static string SaveToCsv(EnergyReport report, string? directory = null)
        {
            if (report == null)
                throw new ArgumentNullException(nameof(report));

            // Default directory is the application's base directory
            if (directory == null)
                directory = AppContext.BaseDirectory;

            // Unique filename using timestamp
            string fileName = $"energy_report_{report.Timestamp:yyyyMMdd_HHmmss}.csv";
            string path = Path.Combine(directory, fileName);

            // Write CSV headers + single row of data
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

        // Basic scoring model used for demonstration purposes
        private static double CalculateEnergyScore(double avgTemp, double fanPercent)
        {
            double score = 100;

            // Higher temperatures reduce score
            score -= (avgTemp - 22.0);

            // More fan usage reduces score
            score -= (fanPercent / 2.0);

            // Clamp score into valid range
            if (score < 0) score = 0;
            if (score > 100) score = 100;

            return score;
        }
    }
}
