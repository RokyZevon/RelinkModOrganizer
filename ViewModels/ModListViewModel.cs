using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using DynamicData.Binding;
using RelinkModOrganizer.Models;
using RelinkModOrganizer.Services;
using Microsoft.Extensions.FileProviders;
using ReactiveUI;

namespace RelinkModOrganizer.ViewModels;

public class ModListViewModel : ViewModelBase
{
    private readonly ConfigurationService _configService;
    private readonly ModificationService _modificationService;
    private readonly IFileProvider _fileProvider;
    private readonly DialogService _dialogService;

    public ModListViewModel(
        ConfigurationService configService,
        ModificationService modification,
        IFileProvider fileProvider,
        DialogService dialogService)
    {
        _configService = configService;
        _modificationService = modification;
        _fileProvider = fileProvider;
        _dialogService = dialogService;

        ModList.AddRange(_configService.Config.Mods.Values);
        OpenModsFolderCommand = ReactiveCommand.Create(OpenModsFolderHandler);
        ReloadModsCommand = ReactiveCommand.Create(ReloadModsCommandHandler);
        ModItCommand = ReactiveCommand.CreateFromTask(ModItHandlerAsync);

        foreach (var mod in ModList)
            mod.WhenAnyValue(m => m.Enabled).Subscribe(async enabled =>
            {
                _configService.Config.Mods[mod.Id].Enabled = enabled;
                await _configService.SaveChangesAsync();
            });
    }

    public ObservableCollectionExtended<Mod> ModList { get; } = [];

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
        (await _modificationService.LoadModsFromDiskAsync())
            .Except(_dialogService.ShowDialog);

        ModList.Clear();
        ModList.AddRange(_configService.Config.Mods.Values);
    }

    private async Task ModItHandlerAsync()
    {
        if (string.IsNullOrWhiteSpace(_configService.Config.GameDirPath))
        {
            _dialogService.ShowDialog("Please locate game in Settings first");
            return;
        }

        // check if there are any conflict mods, then alert
        var conflicMods = _modificationService.GetConflicMods();
        if (conflicMods != null && conflicMods.Count != 0)
        {
            var messages = conflicMods.Select(m =>
$@"Found conflicts between:

{string.Join(", \n", m)},

please ensure only one of them is enabled.
");
            _dialogService.ShowDialog(messages);
            return;
        }

        if (!_modificationService.TryCleanUpGameDir()
            .Except(_dialogService.ShowDialog).Success)
            return;

        if (!(await _modificationService.TryCopyModsAsync())
            .Except(_dialogService.ShowDialog).Success)
            return;

        if (!(await _modificationService.TryGenerateDataIndexAsync())
            .Except(_dialogService.ShowDialog).Success)
            return;

        _dialogService.ShowDialog("Mods installed successfully");
    }

    //private void WatchModsDirectory()
    //{
    //    var token = _fileProvider.Watch("*");
    //    token.RegisterChangeCallback(async state => await OnChangedAsync(state), null);

    //    async Task OnChangedAsync(object? state)
    //    {
    //        ModList.Clear();
    //        ModList.AddRange(_configService.Config.Mods.Values);
    //        (await _modificationService.LoadModsFromDiskAsync())
    //            .Except(_dialogService.ShowDialog);
    //        WatchModsDirectory();
    //    }
    //}
}