using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class MenuManager : MonoBehaviour
{
    public void OnLoadButtonClicked()
    {
        string path = Path.Combine(Application.persistentDataPath, "Saves", "last_save.json");

        if (!File.Exists(path))
        {
            Debug.LogWarning("Сохранение не найдено.");
            return;
        }

        string json = File.ReadAllText(path);
        GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);

        // Сохраняем имя блока для следующей сцены
        PlayerPrefs.SetString("LoadBlockName", data.blockName);
        PlayerPrefs.Save();

        // Загружаем игровую сцену
        SceneManager.LoadScene("Game");
    }

    [System.Serializable]
    public class GameSaveData
    {
        public string blockName;
    }
}
