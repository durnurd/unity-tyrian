using System.IO;
using UnityEngine;

public static class FileIO
{
#if TYRIAN2000
    private const string folder = "tyrian2000";
#else
    private const string folder = "tyrian21";
#endif

    public static byte[] readAllBytes(string filename)
    {
        string path = Path.Combine(Application.streamingAssetsPath, folder, filename);
        if (File.Exists(path))
            return File.ReadAllBytes(path);
        return null;
    }

    public static void writeAllDataBytes(string filename, byte[] data)
    {
        File.WriteAllBytes(Path.Combine(Application.persistentDataPath, filename), data);
    }

    public static BinaryReader openData(string filename)
    {
        string path = Path.Combine(Application.persistentDataPath, filename);
        if (File.Exists(path))
            return new BinaryReader(File.OpenRead(path));
        return null;
    }

    public static BinaryReader open(string filename)
    {
        string path = Path.Combine(Application.streamingAssetsPath, folder, filename);
        if (File.Exists(path))
            return new BinaryReader(File.OpenRead(path));
        return null;
    }

    public static bool fileExists(string filename)
    {
        string path = Path.Combine(Application.streamingAssetsPath, folder, filename);
        return File.Exists(path);
    }

    public static sbyte[] ReadSBytes(this BinaryReader f, int count)
    {
        sbyte[] ret = new sbyte[count];
        for (int i = 0; i < count; ++i)
        {
            ret[i] = f.ReadSByte();
        }
        return ret;
    }

    public static ushort[] ReadUInt16s(this BinaryReader f, int count)
    {
        ushort[] ret = new ushort[count];
        for (int i = 0; i < count; ++i)
        {
            ret[i] = f.ReadUInt16();
        }
        return ret;
    }
}