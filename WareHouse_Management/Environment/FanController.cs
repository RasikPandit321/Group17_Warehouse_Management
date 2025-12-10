using System;

namespace WareHouse_Management.Environment
{
    /// <summary>
    /// Controls a cooling fan based on temperature using hysteresis.
    /// - Fan turns ON when temperature exceeds OnThreshold.
    /// - Fan turns OFF when temperature drops below OffThreshold.
    /// - When temperature is between thresholds, the previous fan state is kept.
    /// </summary>
    public class FanController
    {
        public double OnThreshold { get; }       // Temperature at which fan turns ON
        public double OffThreshold { get; }      // Temperature at which fan turns OFF

        public bool IsOn { get; private set; }   // Current fan state
        public double CurrentTemperature { get; private set; } // Last measured temperature

        public FanController(double onThreshold, double offThreshold)
        {
            // Ensure OFF threshold is lower to guarantee hysteresis behavior
            if (offThreshold >= onThreshold)
            {
                throw new ArgumentException(
                    "offThreshold must be strictly less than onThreshold to provide hysteresis.");
            }

            OnThreshold = onThreshold;
            OffThreshold = offThreshold;
            IsOn = false; // Fan starts OFF
        }

        /// <summary>
        /// Accepts a new temperature reading and updates fan state.
        /// Uses hysteresis to avoid rapidly toggling the fan.
        /// </summary>
        public void UpdateTemperature(double temperature)
        {
            CurrentTemperature = temperature;

            // Turn fan ON when going above upper threshold
            if (!IsOn && temperature > OnThreshold)
            {
                IsOn = true;
            }
            // Turn fan OFF when going below lower threshold
            else if (IsOn && temperature < OffThreshold)
            {
                IsOn = false;
            }
            // If in between thresholds, keep previous IsOn value
        }
    }
}
