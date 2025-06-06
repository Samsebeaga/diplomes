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
            Debug.LogWarning("���������� �� �������.");
            return;
        }

        string json = File.ReadAllText(path);
        GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);

        // ��������� ��� ����� ��� ��������� �����
        PlayerPrefs.SetString("LoadBlockName", data.blockName);
        PlayerPrefs.Save();

        // ��������� ������� �����
        SceneManager.LoadScene("Game");
    }

    [System.Serializable]
    public class GameSaveData
    {
        public string blockName;
    }
}
