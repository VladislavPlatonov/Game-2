using UnityEngine;
using UnityEngine.SceneManagement;
using Poker;

public class SaveLoadUI : MonoBehaviour
{
    [Header("Slots count (for your UI logic if needed)")]
    [SerializeField] private int slotsCount = 3;

    public void SaveToSlot(int slot)
    {
        var sm = SoulManager.Instance;
        if (sm == null)
        {
            Debug.LogError("[SaveLoadUI] SoulManager.Instance not found!");
            return;
        }

        var data = new SaveData
        {
            sceneName = SceneManager.GetActiveScene().name,
            playerSouls = sm.GetPlayerSouls(),
            aiSouls = sm.GetAISouls(),
        };

        SaveSystem.Save(slot, data);
    }

    public void LoadFromSlot(int slot)
    {
        var data = SaveSystem.Load(slot);
        if (data == null) return;

        // Если сохранение из другой сцены — грузим её (упрощённо)
        if (data.sceneName != SceneManager.GetActiveScene().name)
        {
            Debug.Log($"[SaveLoadUI] Loading scene '{data.sceneName}' from slot {slot}...");
            Time.timeScale = 1f;
            SceneManager.LoadScene(data.sceneName);
            return;
        }

        var sm = SoulManager.Instance;
        if (sm == null)
        {
            Debug.LogError("[SaveLoadUI] SoulManager.Instance not found!");
            return;
        }

        // ✅ ВАЖНО: без DebugAdd — просто устанавливаем значения
        sm.SetSouls(data.playerSouls, data.aiSouls);

        Debug.Log($"[SaveLoadUI] Loaded slot {slot}: P={data.playerSouls}, AI={data.aiSouls}");
    }

    public bool HasSave(int slot) => SaveSystem.Has(slot);
}