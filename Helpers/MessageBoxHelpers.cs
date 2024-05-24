using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using MsBox.Avalonia;

namespace RelinkModOrganizer.Helpers;

public static class MessageBoxHelpers
{
    public static async Task InfoAsync(string message)
    {
        var msgBox = MessageBoxManager.GetMessageBoxStandard(
            string.Empty,
            message,
            MsBox.Avalonia.Enums.ButtonEnum.Ok,
            MsBox.Avalonia.Enums.Icon.Info,
            WindowStartupLocation.CenterScreen);
        await msgBox.ShowAsync();
    }

    public static async Task ErrorAsync(string message)
    {
        var msgBox = MessageBoxManager.GetMessageBoxStandard(
            string.Empty,
            message,
            MsBox.Avalonia.Enums.ButtonEnum.Ok,
            MsBox.Avalonia.Enums.Icon.Error,
            WindowStartupLocation.CenterScreen);
        await msgBox.ShowAsync();
    }
}