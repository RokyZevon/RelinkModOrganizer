using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using DynamicData.Binding;
using ReactiveUI;

namespace RelinkModOrganizer.ViewModels.Designs;

public class DesignModListViewModel : ViewModelBase
{
    public DesignModListViewModel()
    {
        ModItems = new(Enumerable
            .Range(1, 20)
            .Select(i => new ModItemViewModel($"mod-{i}", $"Mod {i}")
            {
                Enabled = i % 2 != 0,
                Order = i
            }));

        OpenModsFolderCommand = ReactiveCommand.Create(OpenModsFolderHandler);
        ReloadModsCommand = ReactiveCommand.Create(ReloadModsCommandHandler);
        ModItCommand = ReactiveCommand.CreateFromTask(ModItHandlerAsync);
    }

    public ObservableCollectionExtended<ModItemViewModel> ModItems { get; private set; }

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