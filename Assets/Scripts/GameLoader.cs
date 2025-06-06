using UnityEngine;
using UnityEngine.SceneManagement;
using Fungus;
using System.IO;

public class GameLoader : MonoBehaviour
{
    public static GameLoader Instance;

    public string blockToLoad;
    public bool isLoadFromSave = false;
    public static class GameData
    {
        public static string blockToLoad;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    // ������, ����� ����� Loading ��������� ������
   

    public void LoadGameFromMenu()
    {
        GameData.blockToLoad = "Gost";
        SceneManager.LoadScene("Game");

        string path = Path.Combine(Application.persistentDataPath, "Saves", "last_save.json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            var data = JsonUtility.FromJson<GameSaveData>(json);
            blockToLoad = data.blockName;
            isLoadFromSave = true;

            SceneManager.LoadScene("Loading");
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Game")
        {
            if (isLoadFromSave && !string.IsNullOrEmpty(blockToLoad))
            {
                Flowchart flowchart = FindFirstObjectByType<Flowchart>();
                if (flowchart != null)
                {
                    Block block = flowchart.FindBlock(blockToLoad);
                    if (block != null)
                    {
                        flowchart.ExecuteBlock(block);
                        Debug.Log("�������� ����: " + blockToLoad);
                    }
                    else
                    {
                        Debug.LogError("���� �� ������: " + blockToLoad);
                    }
                }
                else
                {
                    Debug.LogError("Flowchart �� ������ � ����� Game.");
                }

                blockToLoad = null;
                isLoadFromSave = false; // ����������� ����������
            }
        }
    }

[System.Serializable]
public class GameSaveData
{
    public string blockName;
}
}