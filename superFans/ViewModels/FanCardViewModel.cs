using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using superFans.Models;
using superFans.Services;

namespace superFans.ViewModels;

public partial class FanCardViewModel : ObservableObject
{
    private readonly IFanControlService _fanControl;
    private readonly FanDevice _fan;

    public FanCardViewModel(FanDevice fan, IFanControlService fanControl)
    {
        _fan = fan;
        _fanControl = fanControl;
    }

    public FanDevice Fan => _fan;

    public string Name => _fan.Name;
    public FanType Type => _fan.Type;
    public bool IsControllable => _fan.IsControllable;

    public float Rpm => _fan.Rpm;
    public float DutyCycle => _fan.DutyCycle;
    public float AssociatedTempC => _fan.AssociatedTempC;
    public string RpmDisplay => $"{_fan.Rpm:F0} RPM";
    public string TempDisplay => $"{_fan.AssociatedTempC:F0} °C";
    public string DutyDisplay => $"{_fan.DutyCycle:F0}%";

    public FanControlMode Mode
    {
        get => _fan.Mode;
        set
        {
            if (_fan.Mode == FanControlMode.ReadOnly) return;
            _fan.Mode = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsAutoMode));
            OnPropertyChanged(nameof(IsManualMode));
            OnPropertyChanged(nameof(IsSliderEnabled));
            _fanControl.ApplyControl(_fan);
        }
    }

    public bool IsAutoMode => Mode == FanControlMode.Auto;
    public bool IsManualMode => Mode == FanControlMode.Manual;
    public bool IsSliderEnabled => _fan.IsSliderEnabled;

    public double TargetSpeed
    {
        get => _fan.TargetSpeed;
        set
        {
            _fan.TargetSpeed = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(TargetSpeedDisplay));
            if (Mode == FanControlMode.Manual)
                _fanControl.ApplyControl(_fan);
        }
    }

    public string TargetSpeedDisplay => $"{_fan.TargetSpeed:F0}%";

    public string TypeDisplay => Type switch
    {
        FanType.CpuFan => "CPU 风扇",
        FanType.GpuFan => "GPU 风扇",
        FanType.ChassisFan => "机箱风扇",
        FanType.Pump => "水泵",
        _ => "其他"
    };

    public string TypeIcon => Type switch
    {
        FanType.CpuFan => "",       // CPU chip icon
        FanType.GpuFan => "",       // GPU/Display icon
        FanType.ChassisFan => "",   // Fan icon
        FanType.Pump => "",         // Water drop
        _ => ""                     // Generic hardware
    };

    public void RefreshDisplay()
    {
        OnPropertyChanged(nameof(Rpm));
        OnPropertyChanged(nameof(RpmDisplay));
        OnPropertyChanged(nameof(DutyCycle));
        OnPropertyChanged(nameof(DutyDisplay));
        OnPropertyChanged(nameof(AssociatedTempC));
        OnPropertyChanged(nameof(TempDisplay));
        OnPropertyChanged(nameof(TargetSpeed));
        OnPropertyChanged(nameof(TargetSpeedDisplay));
        OnPropertyChanged(nameof(IsSliderEnabled));
    }

    [RelayCommand]
    private void ToggleMode()
    {
        if (!IsControllable) return;
        Mode = Mode == FanControlMode.Auto
            ? FanControlMode.Manual
            : FanControlMode.Auto;
    }

    [RelayCommand]
    private void SetAuto() => Mode = FanControlMode.Auto;

    [RelayCommand]
    private void SetManual() => Mode = FanControlMode.Manual;
}
