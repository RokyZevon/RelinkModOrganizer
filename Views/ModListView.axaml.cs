using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace RelinkModOrganizer.Views;

public partial class ModListView : UserControl
{
    public ModListView()
    {
        InitializeComponent();
    }

    private void Image_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        if (sender is Control ctl)
            FlyoutBase.ShowAttachedFlyout(ctl);
    }
}