using System;
using ReactiveUI;

namespace RelinkModOrganizer.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly ModListViewModel _modListViewModel;
    private readonly SettingsViewModel _settingsViewModel;

    private bool _isGamePathSet;
    private int _selectedIndex;
    private ViewModelBase _contentViewModel;

    public MainWindowViewModel(
        ModListViewModel modListViewModel,
        SettingsViewModel settingsViewModel)
    {
        _modListViewModel = modListViewModel;
        _settingsViewModel = settingsViewModel;

        AddGamePathCheck();
        _selectedIndex = IsGamePathSet ? 0 : 1;
        _contentViewModel = IsGamePathSet ? _modListViewModel : _settingsViewModel;

        AddNavigation();
    }

    public bool IsGamePathSet
    {
        get => _isGamePathSet;
        set => _isGamePathSet = this.RaiseAndSetIfChanged(ref _isGamePathSet, value);
    }

    public int SelectedIndex
    {
        get => _selectedIndex;
        set => _selectedIndex = this.RaiseAndSetIfChanged(ref _selectedIndex, value);
    }

    public ViewModelBase ContentViewModel
    {
        get => _contentViewModel;
        set => _contentViewModel = this.RaiseAndSetIfChanged(ref _contentViewModel, value);
    }

    private void AddNavigation() =>
        this.WhenAnyValue(vm => vm.SelectedIndex)
            .Subscribe(i => ContentViewModel = i switch
            {
                0 => _modListViewModel,
                1 => _settingsViewModel,
                _ => ContentViewModel,
            });

    private void AddGamePathCheck() =>
        _settingsViewModel.WhenAnyValue(vm => vm.GameDirPath)
            .Subscribe(path => IsGamePathSet = !string.IsNullOrWhiteSpace(path));
}