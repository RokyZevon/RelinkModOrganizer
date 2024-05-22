using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using RelinkModOrganizer.Services;

namespace RelinkModOrganizer.ViewModels;

public class ViewModelBase : ReactiveObject
{
    private Dictionary<string, string> _ls = [];

    public ViewModelBase()
    {
        if (App.Current?.ServiceProvider.GetService<LocalizationService>() is { } localizationService)
        {
            Ls = localizationService.LocalizedStrings;

            MessageBus.Current.Listen<string>("LanguageChanged")
                .Subscribe(_ =>
                {
                    Ls = localizationService.LocalizedStrings;
                });
        }
    }

    /// <summary>
    /// Localized Strings
    /// </summary>
    public Dictionary<string, string> Ls
    {
        get => _ls;
        set => _ls = this.RaiseAndSetIfChanged(ref _ls, value);
    }
}