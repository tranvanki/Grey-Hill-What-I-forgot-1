using UnityEngine;
using System.IO;

/// <summary>
/// Helper script để hiển thị đường dẫn save file và mở folder chứa save
/// Gắn vào một GameObject bất kỳ trong scene, hoặc gọi từ Console
/// </summary>
public class SaveFilePathHelper : MonoBehaviour
{
    [ContextMenu("Log Save File Path")]
    public void LogSaveFilePath()
    {
        string savePath = Path.Combine(Application.persistentDataPath, "save.json");
        Debug.Log($"<color=cyan>===== SAVE FILE PATH =====</color>");
        Debug.Log($"<color=yellow>Full path: {savePath}</color>");
        Debug.Log($"<color=yellow>Folder: {Application.persistentDataPath}</color>");
        Debug.Log($"<color=yellow>File exists: {File.Exists(savePath)}</color>");
        
        if (File.Exists(savePath))
        {
            Debug.Log($"<color=green>Save file found! Click here to open folder:</color>");
            Debug.Log($"<color=green>{Application.persistentDataPath}</color>");
        }
    }

    [ContextMenu("Open Save Folder")]
    public void OpenSaveFolder()
    {
        string folder = Application.persistentDataPath;
        if (Directory.Exists(folder))
        {
            Application.OpenURL("file:///" + folder);
            Debug.Log($"<color=green>Opened folder: {folder}</color>");
        }
        else
        {
            Debug.LogWarning($"Folder doesn't exist yet: {folder}");
        }
    }

    [ContextMenu("Delete Save File")]
    public void DeleteSaveFile()
    {
        SaveManager.Delete();
        Debug.Log("<color=red>Save file deleted!</color>");
    }

    void Start()
    {
        // Auto log path khi scene bắt đầu
        LogSaveFilePath();
    }
}
