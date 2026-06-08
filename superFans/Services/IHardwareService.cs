using superFans.Models;

namespace superFans.Services;

public interface IHardwareService
{
    List<FanDevice> ScanFans();
    void UpdateSensors(IEnumerable<FanDevice> fans);
    void Shutdown();
}
