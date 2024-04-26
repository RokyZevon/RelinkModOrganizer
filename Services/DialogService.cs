using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using RelinkModOrganizer.ViewModels;
using RelinkModOrganizer.Views;

namespace RelinkModOrganizer.Services;

public class DialogService(DialogWindowViewModel vm)
{
    public void ShowDialog(string message) =>
        ShowDialog(new List<string> { message });

    public void ShowDialog(IEnumerable<string> messages)
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop ||
            desktop.MainWindow is null)
            return;

        vm.Messages.Clear();
        vm.Messages.AddRange(messages);

        Dispatcher.UIThread.Invoke(() =>
        {
            var dialogWindow = new DialogWindow { DataContext = vm };
            dialogWindow.ShowDialog(desktop.MainWindow);
        });
    }
}