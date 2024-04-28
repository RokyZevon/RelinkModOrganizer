using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Text.Json.Serialization;
using Avalonia.Media.Imaging;
using ReactiveUI;

namespace RelinkModOrganizer.Models;

public class Mod(string id, string name) : ReactiveObject
{
    private bool _enabled;

    /// <summary>
    /// Directory name in ./Mods
    /// </summary>
    public string Id { get; set; } = id;

    [Required(AllowEmptyStrings = false, ErrorMessage = "Name is required.")]
    public string Name { get; set; } = name;

    public string? PreviewImagePath { get; set; }

    [JsonIgnore]
    public Bitmap? PreviewImage { get => LoadImage(); }

    public bool Enabled
    {
        get => _enabled;
        set => this.RaiseAndSetIfChanged(ref _enabled, value);
    }

    public HashSet<string> RelativeFilePaths { get; set; } = [];

    private Bitmap? LoadImage()
    {
        if (string.IsNullOrWhiteSpace(PreviewImagePath) ||
            !File.Exists(PreviewImagePath))
            return null;

        var stream = File.OpenRead(PreviewImagePath);
        return new Bitmap(stream);
    }
}

public record ModItem(string Id, string Name);