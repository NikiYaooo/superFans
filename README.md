# SuperFans — 智能风扇控制系统

Windows 桌面调温软件，支持每颗风扇独立手动/自动调速，基于 LibreHardwareMonitorLib 硬件直控。

## 功能

- **全域硬件自动识别** — 启动自动扫描 CPU 风扇、机箱风扇、显卡风扇、水泵，自动分类命名，区分可控/只读设备
- **单风扇独立手动调控** — 0%–100% 无级调节，每扇互不影响，带快捷档位（静音 20% / 标准 50% / 性能 75% / 全速 100%）
- **单风扇独立智能自动调速** — 每扇拥有独立的 5 段温控曲线，线性插值平滑调速：
  - < 30°C → 20%（极致静音）
  - 30–45°C → 20%→40% 缓升
  - 45–60°C → 40%→65%
  - 60–75°C → 65%→85%
  - > 85°C → 100% 满速
- **实时监控** — 1 秒级刷新，显示实时转速(RPM)、温度、占空比
- **安全保护** — 退出/崩溃自动交还主板控制，最低 20% 停转保护，85°C 强制满速

## 技术栈

- .NET 8.0 + WPF (MVVM)
- [LibreHardwareMonitorLib](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor) 硬件传感器直控
- CommunityToolkit.Mvvm

## 构建 & 运行

```bash
# 编译
dotnet build superFans/superFans.csproj

# 发布独立 exe（无需安装 .NET 运行时）
dotnet publish superFans/superFans.csproj -c Release -r win-x64 \
  --self-contained true -p:PublishSingleFile=true \
  -p:EnableCompressionInSingleFile=true \
  -p:IncludeNativeLibrariesForSelfExtract=true \
  -o publish/

# 运行（需要管理员权限访问硬件传感器）
./publish/superFans.exe
```

## 项目结构

```
superFans/
├── Models/         # 数据模型 (FanDevice, FanCurve, CurvePoint)
├── Services/       # 核心服务层
│   ├── HardwareService.cs     # 硬件扫描与传感器读写
│   ├── FanControlService.cs   # 风扇控制（手动/自动/曲线插值）
│   ├── SafetyGuardService.cs  # 安全保护（最低转速/高温强制满速）
│   └── MonitoringService.cs   # 1秒定时刷新
├── ViewModels/     # MVVM ViewModel 层
│   ├── MainViewModel.cs       # 主窗口 VM
│   └── FanCardViewModel.cs    # 单风扇卡片 VM
├── Views/          # WPF 视图层
│   ├── MainWindow.xaml        # 主窗口（侧边栏 + 风扇卡片网格）
│   └── FanCardView.xaml       # 风扇卡片控件
└── Converters/     # 值转换器
```

## 重要提示

- **必须以管理员身份运行**，否则无法访问硬件传感器
- 软件退出后所有风扇会立即交还主板控制，恢复出厂默认策略
- 程序异常崩溃时硬件控制链路自动释放，不会锁死风扇转速
