using System.IO;
using Avalonia.Media.Imaging;
using RelinkModOrganizer.Models;
using RelinkModOrganizer.ViewModels;

namespace RelinkModOrganizer.Mappers;

public static class ModMapper
{
    public static ModItemViewModel ToViewModel(this Mod mod)
    {
        Bitmap? image = null;
        if (!string.IsNullOrWhiteSpace(mod.PreviewImagePath) &&
            File.Exists(mod.PreviewImagePath))
        {
            using var fileStream = File.OpenRead(mod.PreviewImagePath);
            var memoryStream = new MemoryStream();
            fileStream.CopyTo(memoryStream);
            memoryStream.Position = 0; // Reset the position to the start of the stream after copying
            image = new Bitmap(memoryStream);
        }

        return new(mod.Id, mod.Name, image)
        {
            Enabled = mod.Enabled,
            Order = mod.Order,
        };
    }

    public static void MapTo(this ModItemViewModel modItemViewModel, Mod mod)
    {
        mod.Name = modItemViewModel.Name;
        mod.Enabled = modItemViewModel.Enabled;
        mod.Order = modItemViewModel.Order;
    }
}