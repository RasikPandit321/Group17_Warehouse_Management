using System;
using System.Collections.Generic;

namespace WareHouse_Management.Controllers
{
    public class SortingController
    {
        private readonly Dictionary<string, string> gatePositions = new()
        {
            {"ZoneA", "Gate1"},
            {"ZoneB", "Gate2"},
            {"ZoneC", "Gate3"}
        };

        private readonly Dictionary<string, int> retryCount = new();
        private const int MaxRetries = 3;

        public string LastRoutedZone { get; private set; } = string.Empty;
        public string LastRoutedGate { get; private set; } = string.Empty;

        public bool RoutePackage(string zone)
        {
            if (string.IsNullOrWhiteSpace(zone) || !gatePositions.ContainsKey(zone))
                return false;

            string gate = gatePositions[zone];
            bool success = SendGateCommand(gate);

            if (success)
            {
                LastRoutedZone = zone;
                LastRoutedGate = gate;
                retryCount[zone] = 0;
                return true;
            }

            if (!retryCount.ContainsKey(zone))
                retryCount[zone] = 0;

            retryCount[zone]++;
            if (retryCount[zone] < MaxRetries)
                return RoutePackage(zone);

            retryCount[zone] = 0;
            return false;
        }

        private bool SendGateCommand(string gate)
        {
            // Simulate 90 % success rate
            Random random = new();
            return random.Next(0, 10) > 1;
        }
    }
}
