using LibreHardwareMonitor.Hardware;
using superFans.Models;

namespace superFans.Services;

public class FanControlService : IFanControlService
{
    private readonly SafetyGuardService _safety;

    public FanControlService(SafetyGuardService safety)
    {
        _safety = safety;
    }

    public void ApplyControl(FanDevice fan)
    {
        if (fan.Control is null || fan.Mode == FanControlMode.ReadOnly)
            return;

        double speed = fan.Mode switch
        {
            FanControlMode.Manual => fan.TargetSpeed,
            FanControlMode.Auto => fan.Curve.Evaluate(fan.AssociatedTempC),
            _ => 20
        };

        speed = _safety.ClampSpeed(speed, fan.AssociatedTempC, fan.Mode);

        try
        {
            fan.Control.SetSoftware((float)speed);
        }
        catch
        {
            // Best-effort: hardware may reject software control
        }
    }

    public void ReleaseControl(FanDevice fan)
    {
        if (fan.Control is null) return;

        try
        {
            fan.Control.SetDefault();
        }
        catch
        {
            // Best-effort release
        }
    }

    public void ReleaseAll(IEnumerable<FanDevice> fans)
    {
        foreach (var fan in fans)
            ReleaseControl(fan);
    }
}
