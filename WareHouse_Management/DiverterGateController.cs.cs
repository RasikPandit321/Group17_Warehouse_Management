using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;

namespace WareHouse_Management
{
    public class DiverterGateController
    {
        public void ActivateGate(string zone)
        {
            Console.WriteLine($"🚦 Diverter moving package to {zone}\n");
        }
    }
}
