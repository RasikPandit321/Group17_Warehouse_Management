using System;

namespace WareHouse_Management.Environment
{
    /// <summary>
    /// Controls a cooling fan based on temperature.
    /// - Turns ON when temperature rises above OnThreshold.
    /// - Turns OFF when temperature falls below OffThreshold.
    /// - Between thresholds, it keeps the previous state (hysteresis).
    /// </summary>
    public class FanController
    {
        public double OnThreshold { get; }
        public double OffThreshold { get; }

        public bool IsOn { get; private set; }
        public double CurrentTemperature { get; private set; }

        public FanController(double onThreshold, double offThreshold)
        {
            if (offThreshold >= onThreshold)
            {
                throw new ArgumentException(
                    "offThreshold must be strictly less than onThreshold to provide hysteresis.");
            }

            OnThreshold = onThreshold;
            OffThreshold = offThreshold;
            IsOn = false;
        }

        /// <summary>
        /// Update the controller with a new temperature value.
        /// This may turn the fan ON or OFF depending on thresholds.
        /// </summary>
        public void UpdateTemperature(double temperature)
        {
            CurrentTemperature = temperature;

            // Turn fan ON when temperature goes above the ON threshold
            if (!IsOn && temperature > OnThreshold)
            {
                IsOn = true;
            }
            // Turn fan OFF when temperature goes below the OFF threshold
            else if (IsOn && temperature < OffThreshold)
            {
                IsOn = false;
            }
            // Otherwise, between thresholds: keep previous IsOn state
        }
    }
}