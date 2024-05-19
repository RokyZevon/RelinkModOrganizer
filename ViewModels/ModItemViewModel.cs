using System.ComponentModel.DataAnnotations;
using Avalonia.Media.Imaging;
using ReactiveUI;

namespace RelinkModOrganizer.ViewModels;

public class ModItemViewModel(
    string id,
    string name,
    Bitmap? previewImage = null
    ) : ViewModelBase
{
    private string name = name;
    private bool enabled;
    private int order = -1;

    public string Id => id;

    [Required(AllowEmptyStrings = false, ErrorMessage = "Name is required.")]
    public string Name
    {
        get => name;
        set => this.RaiseAndSetIfChanged(ref name, value);
    }

    public bool Enabled
    {
        get => enabled;
        set => this.RaiseAndSetIfChanged(ref enabled, value);
    }

    public int Order
    {
        get => order;
        set => order = this.RaiseAndSetIfChanged(ref order, value);
    }

    public Bitmap? PreviewImage => previewImage;

    public override string ToString() => Name;
}