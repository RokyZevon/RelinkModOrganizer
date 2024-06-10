using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using DynamicData.Binding;
using MsBox.Avalonia;
using ReactiveUI;
using RelinkModOrganizer.Helpers;
using RelinkModOrganizer.Mappers;
using RelinkModOrganizer.Services;

namespace RelinkModOrganizer.ViewModels;

public class ModListViewModel : ViewModelBase
{
    private readonly ConfigurationService _configService;
    private readonly ModificationService _modificationService;

    //private readonly IFileProvider _fileProvider;

    public ModListViewModel(
        ConfigurationService configService,
        ModificationService modification)
    //IFileProvider fileProvider
    {
        _configService = configService;
        _modificationService = modification;
        //_fileProvider = fileProvider;

        LoadModItems();

        OpenModsFolderCommand = ReactiveCommand.Create(OpenModsFolderHandler);
        ReloadModsCommand = ReactiveCommand.Create(ReloadModsCommandHandler);
        ModItCommand = ReactiveCommand.CreateFromTask(ModItHandlerAsync);
    }

    public ObservableCollectionExtended<ModItemViewModel> ModItems { get; private set; } = [];

    public ICommand OpenModsFolderCommand { get; }
    public ICommand ReloadModsCommand { get; }
    public ICommand ModItCommand { get; }

    public async Task ReOrderAsync()
    {
        if (!ModItems.Any())
            return;

        var mods = _configService.Config.Mods;

        foreach (var item in ModItems)
        {
            item.Order = ModItems.IndexOf(item);
            var mod = mods[item.Id];
            if (mod != null)
                mod.Order = item.Order;
        }

        mods = mods.OrderBy(m => m.Value.Order)
            .ToDictionary(kv => kv.Key, kv => kv.Value);

        await _configService.SaveChangesAsync();
    }

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
            .Except(async (err) => await MessageBoxHelpers.ErrorAsync(err));

        LoadModItems();
    }

    private void LoadModItems()
    {
        ModItems.Clear();
        ModItems.AddRange(_configService.Config.Mods.Values.OrderBy(mod => mod.Order).Select(mod => mod.ToViewModel()).ToList());
        foreach (var item in ModItems)
            item.WhenAnyValue(mi => mi.Name, mi => mi.Enabled, mi => mi.Order)
                .Subscribe(_ =>
                {
                    item.MapTo(_configService.Config.Mods[item.Id]);
                    _configService.Config.Mods = _configService.Config.Mods
                        .AsEnumerable()
                        .OrderBy(mod => mod.Value.Order)
                        .ToDictionary(kv => kv.Key, kv => kv.Value);
                });

        _configService.SaveChanges();
    }

    private async Task ModItHandlerAsync()
    {
        if (string.IsNullOrWhiteSpace(_configService.Config.GameDirPath))
        {
            await MessageBoxHelpers.ErrorAsync(Ls["gameNotFound"]);
            return;
        }

        // check if game exe updated
        var gameExePath = Path.Combine(_configService.Config.GameDirPath, Consts.GameExeName);
        var newMd5 = Md5Helper.CalculateMd5(gameExePath);
        if (newMd5 != _configService.Config.GameExeMd5)
        {
            // clean old outdated data index
            var bakPath = Path.Combine(AppContext.BaseDirectory, Consts.GameIndexBakName);
            File.Delete(Path.Combine(bakPath));
            await _modificationService.BackUpGameIndexAsync(bakPath);

            // save the new md5
            _configService.Config.GameExeMd5 = newMd5;
        }

        // check if there are any conflict mods, then alert
        var conflicMods = _modificationService.GetConflicMods();
        if (conflicMods is { Count: > 0 })
        {
            var messages = conflicMods.Select(m =>
                $"""
                {Ls["conflictsContent1"]}
                {string.Join(", \n", m)},
                {Ls["conflictsContent2"]}
                """);
            var message = string.Join("\n\n", messages);
            var msBoxParams = new MsBox.Avalonia.Dto.MessageBoxCustomParams
            {
                ButtonDefinitions = [new() { Name = Ls["ok"] }, new() { Name = Ls["cancel"] }],
                ContentTitle = Ls["conflictsTitle"],
                ContentMessage = message,
                Icon = MsBox.Avalonia.Enums.Icon.Warning,
                Topmost = true,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
            };
            var msBox = MessageBoxManager.GetMessageBoxCustom(msBoxParams);
            var msBoxResult = await msBox.ShowAsync();
            if (msBoxResult == Ls["cancel"])
                return;
        }

        if (!_modificationService.TryCleanUpGameDir()
            .Except(async (err) => { await MessageBoxHelpers.ErrorAsync(err); })
            .Success)
            return;

        if (!(await _modificationService.TryCopyModsAsync())
            .Except(async (err) => { await MessageBoxHelpers.ErrorAsync(err); })
            .Success)
            return;

        if (!(await _modificationService.TryGenerateDataIndexAsync())
            .Except(async (err) => { await MessageBoxHelpers.ErrorAsync(err); })
            .Success)
            return;

        await MessageBoxHelpers.InfoAsync(Ls["modInstallSuccess"]);
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