using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public void StartGame()
    {
        if (GameLoader.Instance != null)
        {
            GameLoader.Instance.blockToLoad = null;
            GameLoader.Instance.isLoadFromSave = false;
        }

        SceneManager.LoadScene("Loading");
    }

}