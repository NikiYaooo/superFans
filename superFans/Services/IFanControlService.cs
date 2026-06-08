using superFans.Models;

namespace superFans.Services;

public interface IFanControlService
{
    void ApplyControl(FanDevice fan);
    void ReleaseControl(FanDevice fan);
    void ReleaseAll(IEnumerable<FanDevice> fans);
}
