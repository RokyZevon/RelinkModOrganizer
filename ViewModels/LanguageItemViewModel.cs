namespace RelinkModOrganizer.ViewModels;

public class LanguageItemViewModel(string name, string code) : ViewModelBase
{
    public string Name { get; } = name;
    public string Code { get; } = code;
}