using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RelinkModOrganizer.Models;

public class Mod(string id, string name)
{
    /// <summary>
    /// Directory name in ./Mods
    /// </summary>
    public string Id { get; set; } = id;

    [Required(AllowEmptyStrings = false, ErrorMessage = "Name is required.")]
    public string Name { get; set; } = name;

    public string? PreviewImagePath { get; set; }

    public bool Enabled { get; set; }

    public int Order { get; set; }

    public HashSet<string> RelativeFilePaths { get; set; } = [];
}