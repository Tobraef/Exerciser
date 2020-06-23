using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Essentials;

namespace App1.Tools
{
    public interface IDistanceProvider
    {
        Task BeginMeasures();

        /// <summary>
        /// Measures from either last Distance call or BeginMeasure call
        /// </summary>
        /// <returns>Meters</returns>
        Task<double> Distance();
    }

    public class StepDistanceProvider : IDistanceProvider
    {
        private int currentStepCounter_;
        private readonly double stepLength_ = 0.3;

        ~StepDistanceProvider()
        {
            Accelerometer.Stop();
        }

        public Task BeginMeasures()
        {
            return Task.Run(() =>
            {
                Accelerometer.Start(SensorSpeed.UI);
                Accelerometer.ShakeDetected += (o, e) => currentStepCounter_++;
                currentStepCounter_ = 0;
            });
        }

        public Task<double> Distance()
        {
            return Task.Run(() =>
            {
                int steps = currentStepCounter_;
                currentStepCounter_ = 0;
                return stepLength_ * steps;
            });
        }
    }

    public class DistanceProvider : IDistanceProvider
    {
        private Location lastLocation_;
        private const GeolocationAccuracy accuracy = GeolocationAccuracy.High;

        public async Task BeginMeasures()
        {
            lastLocation_ = await Geolocation.GetLocationAsync();
        }

        public async Task<double> Distance()
        {
            var req = new GeolocationRequest(accuracy);
            var current = await Geolocation.GetLocationAsync(req);
            var before = lastLocation_;
            lastLocation_ = current;
            return before.CalculateDistance(current, DistanceUnits.Kilometers) * 1000;
        }
    }
}
