# RelinkModOrganizer

<p align="right">[ English | <a href="https://github.com/RokyZevon/RelinkModOrganizer/blob/master/README_zh.md">简体中文</a> ]</p>

A simple Granblue Relink Mod Organizer, supporting Windows / Steam Deck

Compatible with Reloaded-II Mods and other formats

## Usage

1. In `Settings` page, click `Locate Game` to find your `granblue_fantasy_relink.exe`
2. Go to `Mod List` page, click `Open Mods Folder` and copy the unzipped Mod folders into it, then click `Reload`
3. `Enable` the Mods that you want to use, then click `Mod it`
4. Launch the game as usual and enjoy!

If any Mods are changed later, please re-click `Mod it`

## Build

SDK: .NET 8.0

- If you don't need AOT single file, you need to change `<PublishAotSingleFile>` to `false` in `RelinkModOrganizer.csproj`.
  Then run `dotnet publish`, which will generate 3 additional dll files under `publish`

- If you want to build into a single AOT executable, you need to download the static libraries from [SkiaSharp.Static](https://github.com/2ndlab/SkiaSharp.Static) and [ANGLE.Static](https://github.com/2ndlab/ANGLE.Static), and put them into `Natives/Windows-x64` folder.
  After `dotnet publish` is done, you can safely delete the .dll files

## Credits

The core logic of Modding comes from [gbfrelink.utility.manager](https://github.com/WistfulHopes/gbfrelink.utility.manager), thanks for their great work!
