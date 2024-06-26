﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using ReactiveUI;
using RelinkModOrganizer.Models;
using RelinkModOrganizer.ThirdParties.DataTools;
using Results = RelinkModOrganizer.TryResults;

namespace RelinkModOrganizer.Services;

public class ModificationService(
    ConfigurationService configService,
    DataToolsService dataToolsService,
    LocalizationService localizationService)
{
    public async Task<TryResult> LoadModsFromDiskAsync()
    {
        var modsDirPath = Path.Combine(AppContext.BaseDirectory, Consts.ModsDirName);

        var modIds = new List<string>();
        foreach (var modPath in Directory.EnumerateFileSystemEntries(
            modsDirPath,
            "*",
            SearchOption.TopDirectoryOnly))
        {
            var dataDir = Directory
                .EnumerateDirectories(modPath, Consts.GameDataDirName, SearchOption.AllDirectories)
                .FirstOrDefault();

            if (string.IsNullOrWhiteSpace(dataDir))
            {
                var msg = string.Format(
                    localizationService.LocalizedStrings["noDataDirInMod"],
                    Consts.GameDataDirName,
                    modPath);
                return Results.Error(msg);
            }

            var modId = Path.GetFileName(modPath);
            var relativeFilePaths = Directory
                .EnumerateFiles(dataDir, "*", SearchOption.AllDirectories)
                .Select(srcFilePath => srcFilePath[(dataDir.Length + 1)..])
                .ToHashSet();

            var mod = configService.Config.Mods.GetValueOrDefault(modId) ??
                new Mod(modId, string.Empty);
            await SetModNameAndIconAsync(mod, modPath);

            mod.RelativeFilePaths = relativeFilePaths;

            configService.Config.Mods[modId] = mod;
            modIds.Add(modId);
        }

        foreach (var id in configService.Config.Mods.Keys.Except(modIds))
            configService.Config.Mods.Remove(id);

        await configService.SaveChangesAsync();

        return Results.Ok();
    }

    public HashSet<HashSet<string>>? GetConflicMods()
    {
        var enabledMods = configService.Config.Mods.Values.Where(mod => mod.Enabled);
        if (!enabledMods.Any())
            return null;

        return enabledMods
            .SelectMany(m => m.RelativeFilePaths, (mod, file) => new
            {
                mod.Id,
                mod.Name,
                File = file
            })
            .GroupBy(m => m.File)
            .Where(g => g.Count() > 1)
            .Select(g => g.Select(item => $"{item.Name} ({item.Id})").ToHashSet())
            .ToHashSet(new HashSetComparer());
    }

    public async Task BackUpGameIndexAsync(string dst)
    {
        var gameDirPath = configService.Config.GameDirPath;
        if (string.IsNullOrWhiteSpace(gameDirPath))
            return;

        var src = Path.Combine(gameDirPath, Consts.GameIndexName);
        if (!File.Exists(src))
            return;

        using var srcFs = File.OpenRead(src);
        using var dstFs = File.Create(dst);
        await srcFs.CopyToAsync(dstFs);
    }

    public TryResult TryCleanUpGameDir()
    {
        var gameDirPath = configService.Config.GameDirPath;
        if (string.IsNullOrWhiteSpace(gameDirPath))
            return Results.Error(localizationService.LocalizedStrings["gameNotFound"]);

        var dataDirPath = Path.Combine(gameDirPath, Consts.GameDataDirName);
        try
        {
            var excepts = new[] { "ui", "sound" };
            var removeDirs = Directory
                .EnumerateDirectories(dataDirPath, "*", SearchOption.TopDirectoryOnly)
                .ExceptBy(excepts, Path.GetFileName);
            foreach (var dir in removeDirs)
                Directory.Delete(dir, true);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return Results.Error(ex.Message);
        }

        return Results.Ok();
    }

    public async Task<TryResult> TryCopyModsAsync()
    {
        var gameDirPath = configService.Config.GameDirPath;
        if (string.IsNullOrWhiteSpace(gameDirPath))
            return Results.Error(localizationService.LocalizedStrings["gameNotFound"]);

        var gameDataPath = Path.Combine(gameDirPath, Consts.GameDataDirName);
        if (!Directory.Exists(gameDataPath))
            return Results.Error(localizationService.LocalizedStrings["noDataDirInGame"]);

        var enabledMods = configService.Config.Mods.Values.Where(mod => mod.Enabled);
        if (!enabledMods.Any())
            return Results.Error(localizationService.LocalizedStrings["noEnabledMods"]);

        // Make sure bigger order mods are copied first, then smaller order mods can overwrite them
        enabledMods = enabledMods.OrderByDescending(mod => mod.Order);

        var modsDirPath = Path.Combine(AppContext.BaseDirectory, Consts.ModsDirName);

        try
        {
            var indexFilePath = Path.Combine(AppContext.BaseDirectory, Consts.GameIndexBakName);
            await dataToolsService.LoadOriginalIndexFileAsync(indexFilePath);

            foreach (var mod in enabledMods)
            {
                var srcModPath = Path.Combine(modsDirPath, mod.Id);
                var dataDirPath = Directory
                    .EnumerateDirectories(srcModPath, Consts.GameDataDirName, SearchOption.AllDirectories)
                    .FirstOrDefault();
                if (dataDirPath == null)
                {
                    var msg = string.Format(
                        localizationService.LocalizedStrings["noDataDirInMod"],
                        Consts.GameDataDirName,
                        srcModPath);
                    return Results.Error(msg);
                }

                foreach (var filePath in mod.RelativeFilePaths)
                {
                    var srcFilePath = Path.Combine(dataDirPath, filePath);
                    var dstFilePath = Path.Combine(gameDataPath, filePath);
                    var dstDirPath = Path.GetDirectoryName(dstFilePath)!;
                    if (!Directory.Exists(dstDirPath))
                        Directory.CreateDirectory(dstDirPath);

                    await DataToolsService.CopyModFileAsync(srcFilePath, dstFilePath);
                    dataToolsService.AddExternalFile(filePath, dstFilePath);
                }
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            return Results.Error(localizationService.LocalizedStrings["permDeny"] + ex.Message);
        }
        catch (Exception ex)
        {
            return Results.Error(localizationService.LocalizedStrings["copyError"] + ex.Message);
        }

        return Results.Ok();
    }

    public async Task<TryResult> TryGenerateDataIndexAsync()
    {
        var gameDirPath = configService.Config.GameDirPath;
        if (string.IsNullOrWhiteSpace(gameDirPath))
            return Results.Error(localizationService.LocalizedStrings["gameNotFound"]);

        try
        {
            var indexFilePath = Path.Combine(gameDirPath, Consts.GameIndexName);
            await dataToolsService.SaveIndexFileAsync(indexFilePath);
        }
        catch (ArgumentNullException ex)
        {
            return Results.Error(localizationService.LocalizedStrings["invalidDataI"] + ex.Message);
        }
        catch (Exception ex)
        {
            return Results.Error(localizationService.LocalizedStrings["errGenDataI"] + ex.Message);
        }

        return Results.Ok();
    }

    private static async Task SetModNameAndIconAsync(Mod mod, string modPath)
    {
        var modConfigJson = Directory
            .GetFiles(modPath, Consts.ReloadedModConfigName, SearchOption.AllDirectories)
            .FirstOrDefault();

        if (string.IsNullOrWhiteSpace(modConfigJson))
        {
            mod.Name = mod.Id;
            return;
        }

        using var fs = File.OpenRead(modConfigJson);
        var doc = await JsonDocument.ParseAsync(fs);

        if (string.IsNullOrWhiteSpace(mod.Name))
        {
            doc.RootElement.TryGetProperty("ModName", out var modNameElem);
            var name = modNameElem.GetString();
            if (!string.IsNullOrWhiteSpace(name))
                mod.Name = name;
        }

        doc.RootElement.TryGetProperty("ModIcon", out var modIconElem);
        var icon = modIconElem.GetString();
        if (!string.IsNullOrWhiteSpace(icon))
        {
            var modDir = Path.GetDirectoryName(modConfigJson)!; // should not be null
            var previewImagePath = Path.Combine(modDir, icon);
            mod.PreviewImagePath = previewImagePath;
        }
    }

    private class HashSetComparer : IEqualityComparer<HashSet<string>>
    {
        public bool Equals(HashSet<string>? x, HashSet<string>? y) =>
            x != null && y != null && x.SetEquals(y);

        public int GetHashCode(HashSet<string> obj) =>
            obj.Aggregate(0, (hash, item) => hash ^ item.GetHashCode());
    }
}