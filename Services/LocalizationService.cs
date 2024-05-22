using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Platform;
using ReactiveUI;

namespace RelinkModOrganizer.Services;

public class LocalizationService
{
    private readonly ConfigurationService _configService;

    public LocalizationService(
        ConfigurationService configService)
    {
        _configService = configService;

        var lang = _configService.Config.Language;
        LocalizedStrings = LoadLocalizedStringsAsync(lang).Result;
    }

    public Dictionary<string, string> LocalizedStrings { get; private set; }

    public async Task ChangeLanguageAsync(string language)
    {
        _configService.Config.Language = language;
        await _configService.SaveChangesAsync();

        LocalizedStrings = await LoadLocalizedStringsAsync(language);
        MessageBus.Current.SendMessage(language, "LanguageChanged");
    }

    private static async Task<Dictionary<string, string>> LoadLocalizedStringsAsync(string language = "en-US")
    {
        var stream = AssetLoader.Open(new Uri($"avares://RelinkModOrganizer/Assets/Langs/{language}.json"));
        return await JsonSerializer.DeserializeAsync(stream, AppJsonSerializerContext.Default.DictionaryStringString) ?? [];
    }
}