using System.IO;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public string sceneName;
    public int playerSouls;
    public int aiSouls;
    public long unixTime;
}

public static class SaveSystem
{
    private static string Dir => Path.Combine(Application.persistentDataPath, "saves");

    private static string SlotPath(int slot) => Path.Combine(Dir, $"slot_{slot}.json");

    public static void Save(int slot, SaveData data)
    {
        if (!Directory.Exists(Dir))
            Directory.CreateDirectory(Dir);

        data.unixTime = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SlotPath(slot), json);
        Debug.Log($"[SaveSystem] Saved slot {slot} -> {SlotPath(slot)}");
    }

    public static bool Has(int slot) => File.Exists(SlotPath(slot));

    public static SaveData Load(int slot)
    {
        string path = SlotPath(slot);
        if (!File.Exists(path))
        {
            Debug.LogWarning($"[SaveSystem] Slot {slot} not found.");
            return null;
        }

        string json = File.ReadAllText(path);
        return JsonUtility.FromJson<SaveData>(json);
    }

    public static void Delete(int slot)
    {
        string path = SlotPath(slot);
        if (File.Exists(path))
            File.Delete(path);
    }
}