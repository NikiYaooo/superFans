using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using superFans.Models;
using superFans.Services;

namespace superFans.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IHardwareService _hardware;
    private readonly IFanControlService _fanControl;
    private readonly MonitoringService _monitoring;

    public ObservableCollection<FanCardViewModel> Fans { get; } = new();

    [ObservableProperty]
    private string _statusText = "正在扫描硬件…";

    [ObservableProperty]
    private int _totalFanCount;

    [ObservableProperty]
    private int _controllableFanCount;

    [ObservableProperty]
    private int _readOnlyFanCount;

    public MainViewModel(
        IHardwareService hardware,
        IFanControlService fanControl,
        MonitoringService monitoring)
    {
        _hardware = hardware;
        _fanControl = fanControl;
        _monitoring = monitoring;
    }

    public void Initialize()
    {
        try
        {
            var devices = _hardware.ScanFans();

            foreach (var device in devices)
            {
                var vm = new FanCardViewModel(device, _fanControl);
                Fans.Add(vm);
            }

            TotalFanCount = devices.Count;
            ControllableFanCount = devices.Count(d => d.IsControllable);
            ReadOnlyFanCount = devices.Count(d => !d.IsControllable);

            StatusText = $"已检测到 {TotalFanCount} 个风扇（{ControllableFanCount} 个可控，{ReadOnlyFanCount} 个只读）";

            _monitoring.Tick += () =>
            {
                System.Windows.Application.Current?.Dispatcher.InvokeAsync(OnMonitoringTick);
            };
            _monitoring.Start();
        }
        catch (Exception ex)
        {
            StatusText = $"硬件初始化失败：{ex.Message}";
        }
    }

    private void OnMonitoringTick()
    {
        try
        {
            var devices = Fans.Select(vm => vm.Fan);
            _hardware.UpdateSensors(devices);

            foreach (var fan in Fans)
            {
                fan.RefreshDisplay();

                if (fan.Mode == FanControlMode.Auto && fan.IsControllable)
                    _fanControl.ApplyControl(fan.Fan);
            }
        }
        catch
        {
            // Silently continue on next tick
        }
    }

    [RelayCommand]
    private void SetAllAuto()
    {
        foreach (var fan in Fans)
        {
            if (fan.IsControllable) fan.Mode = FanControlMode.Auto;
        }
        StatusText = "所有风扇已切换为自动模式";
    }

    [RelayCommand]
    private void SetAllManual()
    {
        foreach (var fan in Fans)
        {
            if (fan.IsControllable) fan.Mode = FanControlMode.Manual;
        }
        StatusText = "所有风扇已切换为手动模式";
    }

    public async Task ShutdownAsync()
    {
        await _monitoring.StopAsync();

        foreach (var fan in Fans)
        {
            if (fan.IsControllable)
                _fanControl.ReleaseControl(fan.Fan);
        }

        _hardware.Shutdown();
        StatusText = "风扇控制已交还主板，安全退出";
    }
}
