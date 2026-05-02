using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public static class FileIO
{
#if TYRIAN2000
    private const string folder = "tyrian2000";
#else
    private const string folder = "tyrian21";
#endif

    private static string getPath(string filename)
    {
#if UNITY_WEBGL
        return Path.Combine(Application.temporaryCachePath, "downloaded", folder, filename.Replace("#","POUND"));
#else
        return Path.Combine(Application.streamingAssetsPath, folder, filename);
#endif
    }

    public static byte[] readAllBytes(string filename)
    {
        string path = getPath(filename);
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

#if UNITY_WEBGL
    public static IEnumerator e_download(string filename)
    {
        if (fileExists(filename))
        {
            yield break;
        }
        Debug.Log("Downloading " + filename);
        string path = Path.Combine(Application.streamingAssetsPath, folder, filename);
        Debug.Log("path = " + path);
        string destination = getPath(filename);
        string directory = Path.GetDirectoryName(destination);
        Directory.CreateDirectory(directory);
        Debug.Log("destination = " + destination);
        using (UnityWebRequest r = UnityWebRequest.Get(path)) {
            Debug.Log("starting request");
            yield return r.SendWebRequest();
            Debug.Log("finished request with result = " + r.result);
            if (r.result == UnityWebRequest.Result.Success) {
                byte[] bytes = r.downloadHandler.data;
                Debug.Log("writing bytesCount = " + bytes.Length);
                File.WriteAllBytes(destination, bytes);
                Debug.Log("finished write");
            }
        }
    }
#endif

    public static BinaryReader open(string filename)
    {
        Debug.Log("open " + filename);
        string path = getPath(filename);
        Debug.Log("path = " + path);
        Debug.Log("exists = " + File.Exists(path));
        if (File.Exists(path))
            return new BinaryReader(File.OpenRead(path));
        return null;
    }

    public static bool fileExists(string filename)
    {
        return File.Exists(getPath(filename));
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