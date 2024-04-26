# RelinkModOrganizer

<p align="right">[ <a href="https://github.com/RokyZevon/RelinkModOrganizer/blob/master/README.md">English</a> | 简体中文 ]</p>

一个简易的 Granblue Relink Mod 管理器, 支持 Windows / Steam Deck

## 使用

1. 在 `Settings` 界面, 点击 `Locate Game` 来找到 `granblue_fantasy_relink.exe`
2. 转到 `Mod List` 界面, 点击 `Open Mods Folder` 后将解压好的 Mod 文件夹复制进去, 然后点击 `Reload` 来加载列表
3. 在列表中将想要生效的 Mod 激活 (`Enabled`) , 最后点击 `Mod it`
4. 用常规方式启动游戏即可

后续如果变更了列表中的 Mod, 请重新点击 `Mod it`

## 构建

SDK: .NET 8.0

- 如不需要 AOT 单文件, 需先将 `RelinkModOrganizer.csproj` 中的 `<PublishAotSingleFile>` 设置为 `false`.
  运行 `dotnet publish`, 这样会在 `publish` 下生成 3 个额外的 dll 文件

- 如需构建成单个 AOT 可执行文件, 需从 [SkiaSharp.Static](https://github.com/2ndlab/SkiaSharp.Static) 和 [ANGLE.Static](https://github.com/2ndlab/ANGLE.Static) 下载相应的静态库, 并将其放在 `Natives/Windows-x64` 文件夹下.
  运行 `dotnet publish`, 之后可以安全地删除上述 .dll 文件

## 致谢

Modding 的核心逻辑来自于 [gbfrelink.utility.manager](https://github.com/WistfulHopes/gbfrelink.utility.manager), 感谢他们的杰出成果!
