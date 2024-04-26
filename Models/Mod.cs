using System.Collections.Generic;
using ReactiveUI;

namespace RelinkModOrganizer.Models;

public class Mod(string id, string name) : ReactiveObject
{
    private bool _enabled;

    /// <summary>
    /// Directory name in ./Mods
    /// </summary>
    public string Id { get; set; } = id;

    public string Name { get; set; } = name;

    public bool Enabled
    {
        get => _enabled;
        set => this.RaiseAndSetIfChanged(ref _enabled, value);
    }

    public HashSet<string> RelativeFilePaths { get; set; } = [];
}

public record ModItem(string Id, string Name);