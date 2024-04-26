using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using DynamicData;
using RelinkModOrganizer.Models;
using RelinkModOrganizer.Services;
using Microsoft.Extensions.FileProviders;
using ReactiveUI;

namespace RelinkModOrganizer.ViewModels.Designs;

public class DesignModListViewModel : ViewModelBase
{
    public DesignModListViewModel()
    {
        var mods = new List<Mod>()
        {
            new("mod 1", "Mod 1") { Enabled = true },
            new("mod 2", "Mod 2") { Enabled = false },
            new("mod 3", "Mod 3") { Enabled = true },
            new("mod 4", "Mod 4") { Enabled = false },
            new("mod 5", "Mod 5") { Enabled = true },
            new("mod 6", "Mod 6") { Enabled = false },
            new("mod 7", "Mod 7") { Enabled = true },
            new("mod 8", "Mod 8") { Enabled = false },
            new("mod 9", "Mod 9") { Enabled = true },
            new("mod 10", "Mod 10") { Enabled = false }
        };

        ModList = new(mods);

        OpenModsFolderCommand = ReactiveCommand.Create(OpenModsFolderHandler);
        ReloadModsCommand = ReactiveCommand.Create(ReloadModsCommandHandler);
        ModItCommand = ReactiveCommand.CreateFromTask(ModItHandlerAsync);
    }

    public ObservableCollection<Mod> ModList { get; }

    public ICommand OpenModsFolderCommand { get; }
    public ICommand ReloadModsCommand { get; }
    public ICommand ModItCommand { get; }

    private static void OpenModsFolderHandler()
    {
        var modsDirPath = Path.Combine(AppContext.BaseDirectory, Consts.ModsDirName);
        if (!Directory.Exists(modsDirPath))
            Directory.CreateDirectory(modsDirPath);

        var fileBrowser = OperatingSystem.IsWindows() ? "explorer.exe" :
                          OperatingSystem.IsLinux() ? "xdg-open" :
                          OperatingSystem.IsMacOS() ? "open" :
                          throw new NotSupportedException("Unsupported operating system.");

        Process.Start(fileBrowser, modsDirPath);
    }

    private async void ReloadModsCommandHandler()
    {
        await Task.Delay(1);
    }

    private async Task ModItHandlerAsync()
    {
        await Task.Delay(1);
    }
}