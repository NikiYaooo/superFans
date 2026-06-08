using System.Windows;
using System.Windows.Controls;
using superFans.ViewModels;

namespace superFans.Views;

public partial class FanCardView : UserControl
{
    public FanCardView()
    {
        InitializeComponent();
    }

    private void QuickSpeed_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn &&
            int.TryParse(btn.Tag?.ToString(), out int speed) &&
            DataContext is FanCardViewModel vm)
        {
            vm.TargetSpeed = speed;
        }
    }
}
