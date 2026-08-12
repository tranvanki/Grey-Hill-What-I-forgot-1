using UnityEngine;
using System.IO;
public static class SaveManager
{   // ── Save ─────────────────────────────────────────────────────
    private static string SavePath => Path.Combine(Application.persistentDataPath, "save.json");
    public static bool HasSave() => File.Exists(SavePath);
    //this function saves the game data to a JSON file
    public static void SaveToJSON(SaveData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
    }
    public static SaveData Load()
    {
        if (!HasSave()) return new SaveData();
        return JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath));
    }
    public static void Delete()
    {
        if (File.Exists(SavePath)) File.Delete(SavePath);
    }

}