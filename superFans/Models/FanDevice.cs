using CommunityToolkit.Mvvm.ComponentModel;
using LibreHardwareMonitor.Hardware;

namespace superFans.Models;

public enum FanType
{
    CpuFan,
    GpuFan,
    ChassisFan,
    Pump,
    Other
}

public enum FanControlMode
{
    ReadOnly,
    Auto,
    Manual
}

public partial class FanDevice : ObservableObject
{
    public string Name { get; init; } = string.Empty;
    public FanType Type { get; init; } = FanType.Other;
    public bool IsControllable { get; init; }

    // Raw hardware references
    public ISensor? RpmSensor { get; init; }
    public ISensor? TempSensor { get; init; }
    public IControl? Control { get; init; }

    [ObservableProperty]
    private float _rpm;

    [ObservableProperty]
    private float _dutyCycle;

    [ObservableProperty]
    private float _associatedTempC;

    [ObservableProperty]
    private FanControlMode _mode = FanControlMode.Auto;

    [ObservableProperty]
    private double _targetSpeed = 20;

    [ObservableProperty]
    private FanCurve _curve = new();

    partial void OnModeChanged(FanControlMode value)
    {
        OnPropertyChanged(nameof(IsSliderEnabled));
    }

    public bool IsSliderEnabled => Mode == FanControlMode.Manual;
}
