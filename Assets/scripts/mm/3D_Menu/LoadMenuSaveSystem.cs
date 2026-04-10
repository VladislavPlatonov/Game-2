using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadMenuSaveSystem : MonoBehaviour
{
    public static LoadMenuSaveSystem Instance;

    private string SavesFolder
    {
        get { return Path.Combine(Application.persistentDataPath, "MenuSaves"); }
    }

    private void Awake()
    {
        Instance = this;

        if (!Directory.Exists(SavesFolder))
            Directory.CreateDirectory(SavesFolder);
    }

    public void SaveGameToSlot(int slotIndex)
    {
        StartCoroutine(SaveGameRoutine(slotIndex));
    }

    private IEnumerator SaveGameRoutine(int slotIndex)
    {
        string saveFileName = "save_" + slotIndex + ".json";
        string screenshotFileName = "save_" + slotIndex + ".png";

        SaveMetadata meta = new SaveMetadata();
        meta.slotIndex = slotIndex;
        meta.saveTitle = "СЕЙВ " + slotIndex;
        meta.saveDate = System.DateTime.Now.ToString("dd.MM.yyyy HH:mm");
        meta.sceneName = SceneManager.GetActiveScene().name;
        meta.screenshotFileName = screenshotFileName;
        meta.saveFileName = saveFileName;

        string json = JsonUtility.ToJson(meta, true);
        File.WriteAllText(Path.Combine(SavesFolder, saveFileName), json);

        yield return new WaitForEndOfFrame();

        string screenshotPath = Path.Combine(SavesFolder, screenshotFileName);
        ScreenCapture.CaptureScreenshot(screenshotPath);
    }

    public SaveMetadata LoadMetadata(int slotIndex)
    {
        string path = Path.Combine(SavesFolder, "save_" + slotIndex + ".json");
        if (!File.Exists(path))
            return null;

        string json = File.ReadAllText(path);
        return JsonUtility.FromJson<SaveMetadata>(json);
    }

    public Texture2D LoadScreenshot(string screenshotFileName)
    {
        string path = Path.Combine(SavesFolder, screenshotFileName);
        if (!File.Exists(path))
            return null;

        byte[] bytes = File.ReadAllBytes(path);
        Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        texture.LoadImage(bytes);
        return texture;
    }

    public bool HasSave(int slotIndex)
    {
        return File.Exists(Path.Combine(SavesFolder, "save_" + slotIndex + ".json"));
    }

    public void DeleteSave(int slotIndex)
    {
        string savePath = Path.Combine(SavesFolder, "save_" + slotIndex + ".json");
        string screenshotPath = Path.Combine(SavesFolder, "save_" + slotIndex + ".png");

        if (File.Exists(savePath))
            File.Delete(savePath);

        if (File.Exists(screenshotPath))
            File.Delete(screenshotPath);
    }

    public void LoadSlot(int slotIndex)
    {
        SaveMetadata meta = LoadMetadata(slotIndex);
        if (meta == null)
        {
            Debug.Log("Слот пуст.");
            return;
        }

        SceneManager.LoadScene(meta.sceneName);
    }
}
