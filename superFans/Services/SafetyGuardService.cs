using superFans.Models;

namespace superFans.Services;

public class SafetyGuardService
{
    private const double MinSpeedPercent = 20;
    private const double CriticalTempC = 85;
    private const double CriticalSpeedPercent = 100;

    public double ClampSpeed(double requestedSpeed, float currentTemp, FanControlMode mode)
    {
        if (currentTemp >= CriticalTempC)
            return CriticalSpeedPercent;

        if (requestedSpeed < MinSpeedPercent)
            return MinSpeedPercent;

        if (requestedSpeed > CriticalSpeedPercent)
            return CriticalSpeedPercent;

        return requestedSpeed;
    }
}
