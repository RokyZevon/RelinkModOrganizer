using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using ReactiveUI;
using RelinkModOrganizer.Services;

namespace RelinkModOrganizer.ViewModels;

public class SettingsViewModel : ViewModelBase
{
    private string? _gameDirPath;
    //private ComboBoxItem? _selectedLanguage;
    private LanguageItemViewModel? _selectedLanguage;
    private readonly ConfigurationService _configurationService;
    private readonly ModificationService _modificationService;
    private readonly DialogService _dialogService;
    private readonly LocalizationService _localizationService;

    public SettingsViewModel(
        ConfigurationService configurationService,
        ModificationService modificationService,
        DialogService dialogService,
        LocalizationService localizationService)
    {
        _configurationService = configurationService;
        _modificationService = modificationService;
        _dialogService = dialogService;
        _localizationService = localizationService;

        LocateGameCommand = ReactiveCommand.CreateFromTask(LocateGameHandlerAsync);
        GameDirPath = _configurationService.Config.GameDirPath;

        SelectedLanguage = AvailableLanguages.FirstOrDefault(
            item => item.Code == _configurationService.Config.Language);

        this.WhenAnyValue(vm => vm.SelectedLanguage)
            .Subscribe(async item =>
            {
                if (item is null) return;
                await _localizationService.ChangeLanguageAsync(item.Code);
            });
    }

    public string? GameDirPath
    {
        get => _gameDirPath;
        set => _gameDirPath = this.RaiseAndSetIfChanged(ref _gameDirPath, value);
    }

    public LanguageItemViewModel? SelectedLanguage
    {
        get => _selectedLanguage;
        set => _selectedLanguage = this.RaiseAndSetIfChanged(ref _selectedLanguage, value);
    }

    public List<LanguageItemViewModel> AvailableLanguages { get; } =
    [
        new LanguageItemViewModel("English", "en-US"),
        new LanguageItemViewModel("简体中文", "zh-CN"),
        new LanguageItemViewModel("日本語", "ja-JP"),
    ];

    public ICommand LocateGameCommand { get; }

    private async Task LocateGameHandlerAsync()
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop ||
            desktop.MainWindow?.StorageProvider is not { } storageProvider)
            return;

        var files = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = Ls["locateGame"],
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
            _dialogService.ShowDialog(Ls["gameVersionUnsupported"]);
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