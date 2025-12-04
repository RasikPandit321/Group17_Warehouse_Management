using System;
using System.Collections.Generic;

namespace WareHouse_Management.Models
{
    public class RoutingModel
    {
        private readonly Dictionary<string, string> routingTable = new()
        {
            {"PKG-ABC123", "ZoneA"},
            {"PKG-XYZ789", "ZoneB"},
            {"PKG-LMN456", "ZoneC"}
        };

        private readonly Dictionary<string, string> routingLog = new(); // barcode -> timestamp

        public string GetTargetZone(string barcode)
        {
            if (string.IsNullOrWhiteSpace(barcode))
                return "ZoneError";

            barcode = barcode.ToUpperInvariant();

            string zone = routingTable.ContainsKey(barcode)
                ? routingTable[barcode]
                : "ZoneError";

            routingLog[barcode] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            return zone;
        }

        public bool HasLogEntry(string barcode)
        {
            return routingLog.ContainsKey(barcode);
        }

        public string GetTimestamp(string barcode)
        {
            return routingLog.ContainsKey(barcode) ? routingLog[barcode] : string.Empty;
        }
    }
}
