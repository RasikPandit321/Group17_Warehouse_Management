using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;

namespace WareHouse_Management
{
    public class RuleEngine
    {
        public string DetermineRoute(string barcode)
        {
            // Simple logic (you can replace this-later)
            return barcode.EndsWith("A") ? "Zone A" : "Zone B";
        }
    }
}
