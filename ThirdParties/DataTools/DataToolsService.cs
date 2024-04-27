// All core logic here comes from
// https://github.com/WistfulHopes/gbfrelink.utility.manager/blob/v1.0.7/gbfrelink.utility.manager/Mod.cs
// All credits to the original authors: WistfulHopes, Nenkai and others.
// Thanks for their great work!

using System;
using System.Buffers.Binary;
using System.IO;
using System.IO.Hashing;
using System.Text;
using System.Threading.Tasks;
using FlatSharp;
using GBFRDataTools.Entities;
using MessagePack;


namespace RelinkModOrganizer.ThirdParties.DataTools;

public class DataToolsService
{
    private IndexFile? _indexFile;

    public async Task LoadOriginalIndexFileAsync(string path) =>
        _indexFile = IndexFile.Serializer.Parse(await File.ReadAllBytesAsync(path));

    public void AddExternalFile(string relativeFilePath, string dstFilePath)
    {
        if (Path.GetExtension(relativeFilePath) == ".json")
            relativeFilePath = Path.ChangeExtension(relativeFilePath, ".msg");
        if (Path.GetExtension(dstFilePath) == ".json")
            dstFilePath = Path.ChangeExtension(dstFilePath, ".msg");

        var universalFilePath = relativeFilePath.Replace(
            Path.DirectorySeparatorChar,
            Path.AltDirectorySeparatorChar);

        var hashBytes = XxHash64.Hash(Encoding.ASCII.GetBytes(universalFilePath), 0);
        ulong hash = BinaryPrimitives.ReadUInt64BigEndian(hashBytes);

        long fileSize = new FileInfo(dstFilePath).Length;
        if (AddOrUpdateExternalFile(hash, (ulong)fileSize))
            Console.WriteLine($"- Index: Added {universalFilePath} as new external file");
        else
            Console.WriteLine($"- Index: Updated {universalFilePath} external file");
        RemoveArchiveFile(hash);
    }

    public async Task SaveIndexFileAsync(string dstPath)
    {
        ArgumentNullException.ThrowIfNull(_indexFile);

        var buffer = new byte[IndexFile.Serializer.GetMaxSize(_indexFile)];
        _indexFile.Codename = "mod-organizer";
        IndexFile.Serializer.Write(buffer, _indexFile);
        await File.WriteAllBytesAsync(dstPath, buffer);
    }

    private bool AddOrUpdateExternalFile(ulong hash, ulong fileSize)
    {
        ArgumentNullException.ThrowIfNull(_indexFile?.ExternalFileHashes);
        ArgumentNullException.ThrowIfNull(_indexFile?.ExternalFileSizes);

        bool added = false;
        int idx = _indexFile.ExternalFileHashes.BinarySearch(hash);
        if (idx < 0)
        {
            idx = _indexFile.ExternalFileHashes.AddSorted(hash);
            added = true;
            _indexFile.ExternalFileSizes.Insert(idx, fileSize);
        }
        else
        {
            _indexFile.ExternalFileSizes[idx] = fileSize;
        }

        return added;
    }

    private void RemoveArchiveFile(ulong hash)
    {
        ArgumentNullException.ThrowIfNull(_indexFile?.ArchiveFileHashes);
        ArgumentNullException.ThrowIfNull(_indexFile?.FileToChunkIndexers);

        int idx = _indexFile.ArchiveFileHashes.BinarySearch(hash);
        if (idx > -1)
        {
            _indexFile.ArchiveFileHashes.RemoveAt(idx);
            _indexFile.FileToChunkIndexers.RemoveAt(idx);
        }
    }

    public static async Task CopyModFileAsync(string src, string dst)
    {
        var copied = false;
        var ext = Path.GetExtension(src);
        try
        {
            copied = ext switch
            {
                ".minfo" => await TryProcessMinfoFileAsync(src, dst),
                ".json" => await TryProcessJsonFileAsync(src, dst),
                _ => default
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to process {ext} file, file will be copied instead - {ex.Message}");
        }

        if (!copied)
        {
            using var srcFs = File.OpenRead(src);
            using var dstFs = File.Create(dst);
            await srcFs.CopyToAsync(dstFs);
        }
    }

    private static async Task<bool> TryProcessMinfoFileAsync(string src, string dst)
    {
        // From the original auther:
        // GBFR v1.1.1 upgraded the minfo file - magic/build date changed, two new fields added.
        // Rendered models invisible due to magic check fail.
        // In order to avoid having ALL mod makers rebuild their mods, upgrade the magic as a post process operation

        var bytes = await File.ReadAllBytesAsync(src);
        var modelInfo = ModelInfo.Serializer.Parse(bytes);
        if (modelInfo.Magic >= 20240213)
            return false;

        modelInfo.Magic = 10000_01_01;
        var size = ModelInfo.Serializer.GetMaxSize(modelInfo);
        var buffer = new byte[size];
        ModelInfo.Serializer.Write(buffer, modelInfo);
        await File.WriteAllBytesAsync(dst, buffer);
        return true;
    }

    private static async Task<bool> TryProcessJsonFileAsync(string src, string dst)
    {
        var json = await File.ReadAllTextAsync(src);
        var msg = MessagePackSerializer.ConvertFromJson(json);
        await File.WriteAllBytesAsync(Path.ChangeExtension(dst, ".msg"), msg);
        return true;
    }
}