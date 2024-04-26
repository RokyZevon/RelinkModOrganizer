using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using RelinkModOrganizer.Services;
using ReactiveUI;

namespace RelinkModOrganizer.ViewModels;

public class SettingsViewModel : ViewModelBase
{
    private string? _gameDirPath;

    private readonly ConfigurationService _configurationService;
    private readonly ModificationService _modificationService;
    private readonly DialogService _dialogService;

    public SettingsViewModel(
        ConfigurationService configurationService,
        ModificationService modificationService,
        DialogService dialogService)
    {
        _configurationService = configurationService;
        _modificationService = modificationService;
        _dialogService = dialogService;

        LocateGameCommand = ReactiveCommand.CreateFromTask(LocateGameHandlerAsync);
        GameDirPath = _configurationService.Config.GameDirPath;
    }

    public string? GameDirPath
    {
        get => _gameDirPath;
        set => _gameDirPath = this.RaiseAndSetIfChanged(ref _gameDirPath, value);
    }

    public ICommand LocateGameCommand { get; }

    private async Task LocateGameHandlerAsync()
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop ||
            desktop.MainWindow?.StorageProvider is not { } storageProvider)
            return;

        var files = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Locate Game Exe",
            AllowMultiple = false,
            FileTypeFilter = [new("exe") { Patterns = [Consts.GameExeName] }]
        });

        if (files is not { Count: > 0 })
            return;

        var gameExePath = files[0].Path.LocalPath;
        if (OperatingSystem.IsWindows() &&
            FileVersionInfo.GetVersionInfo(gameExePath) is not
            {
                ProductMajorPart: >= Consts.GameMajorVer,
                ProductMinorPart: >= Consts.GameMinorVer,
            })
        {
            _dialogService.ShowDialog("Selected game version is not supported, please ensure your game version is up-to-date");
            return;
        }

        var gameDirPath = Path.GetDirectoryName(gameExePath);

        _configurationService.Config.GameDirPath = gameDirPath;
        await _configurationService.SaveChangesAsync();

        var dst = Path.Combine(AppContext.BaseDirectory, Consts.GameIndexBakName);
        if (!File.Exists(dst))
            await _modificationService.BackUpGameIndexAsync(dst);

        GameDirPath = gameDirPath;
    }
}