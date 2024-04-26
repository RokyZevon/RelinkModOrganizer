using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Controls;
using RelinkModOrganizer.Models;
using ReactiveUI;

namespace RelinkModOrganizer.Services;

public class ConfigurationService
{
    public ConfigurationService()
    {
#if DEBUG
        if (Design.IsDesignMode)
        {
            Config = new Config();
            return;
        }
#endif

        var path = Path.Combine(AppContext.BaseDirectory, Consts.ConfigName);
        Config = LoadOrCreateFrom(path);
    }

    public Config Config { get; private set; }

    public async Task SaveChangesAsync() =>
        await File.WriteAllTextAsync(
            Path.Combine(AppContext.BaseDirectory, Consts.ConfigName),
            Config.ToJson());

    public bool IsGameDirPathValid() =>
        !string.IsNullOrWhiteSpace(Config.GameDirPath) &&
        Path.GetFileName(Config.GameDirPath) == Consts.GameExeName &&
        File.Exists(Config.GameDirPath);

    #region Private

    private static Config LoadOrCreateFrom(string path)
    {
        Config? config = null;

        if (File.Exists(path))
        {
            var json = File.ReadAllText(path);
            config = Config.TryParseFromJson(json);
        }

        if (config == null)
        {
            config = new Config();
            var json = config.ToJson();
            File.WriteAllText(path, json);
        }

        return config;
    }

    #endregion Private
}

public class Config : ReactiveObject
{
    public string? GameDirPath { get; set; }

    /// <summary>
    /// key: Id
    /// value: Mod
    /// </summary>
    public Dictionary<string, Mod> Mods { get; set; } = [];

    public static Config? TryParseFromJson(string json)
    {
        try
        {
            return JsonSerializer.Deserialize(json, AppJsonSerializerContext.Default.Config);
        }
        catch
        {
            return null;
        }
    }

    public string ToJson() =>
        JsonSerializer.Serialize(this, AppJsonSerializerContext.Default.Config);
}