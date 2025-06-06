using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
namespace CGTUnity.Fungus.SaveSystem
{

    /// <summary>
    /// Loads save data for a whole game/playthrough.
    /// </summary>
    public class GameLoader : SaveLoader<GameSaveData>
    {
        [SerializeField] private UnityEvent onLoadStarted; // Событие при начале загрузки
        [SerializeField] private UnityEvent onLoadCompleted; // Событие при завершении
        [SerializeField] private SaveMenu saveMenu;
        [SerializeField] private Button saveButton;
        [SerializeField] private Button loadButton;
        [SerializeField] private Button restartButton;
        protected List<SaveLoader> subloaders = new List<SaveLoader>(); 
        // Handles loading the different types of data.

        #region Methods
        protected virtual void Awake()
        {
            saveButton.onClick.AddListener(saveMenu.SaveToSelectedSlot);
            loadButton.onClick.AddListener(saveMenu.LoadFromSelectedSlot);
            restartButton.onClick.AddListener(RestartGame);
            subloaders.AddRange(GetComponents<SaveLoader>());
            subloaders.RemoveAll(subloader => subloader == this); // This can't be its own subloader.
            subloaders.Sort(SortSubloadersByPriority);
        }
        private void RestartGame()
        {
            // Реализация перезапуска игры (зависит от вашей логики)
            Debug.Log("Game restarted");
        }
        /// <summary>
        /// Loads a whole game based on the state the passed save data holds. Starts by loading the scene
        /// the save data is registered with.
        /// </summary>
        public override bool Load(GameSaveData saveData)
        {
            if (saveData == null)
            {
                Debug.LogError("Save data is null!");
                return false;
            }

            onLoadStarted?.Invoke(); // Уведомляем UI

            UnityAction<Scene, LoadSceneMode> onSceneLoaded = null;
            onSceneLoaded = (scene, mode) =>
            {
                if (mode == LoadSceneMode.Single)
                {
                    var loader = FindObjectOfType<GameLoader>();
                    if (loader == null)
                    {
                        Debug.LogError("No GameLoader in scene: " + scene.name);
                        return;
                    }

                    SceneManager.sceneLoaded -= onSceneLoaded;
                    loader.LoadState(saveData);
                    onLoadCompleted?.Invoke(); // Уведомляем UI о завершении
                }
            };

            SceneManager.sceneLoaded += onSceneLoaded;
            SceneManager.LoadScene(saveData.SceneName); // Можно заменить на LoadSceneAsync
            return true;
        }


        public bool LoadLatestSave(SaveManager saveManager)
        {
            var latestSave = saveManager.GetLatestSave();
            if (latestSave != null)
                return Load(latestSave);
            else
                Debug.LogWarning("No saves found.");
            return false;
        }

        protected static int SortSubloadersByPriority(SaveLoader first, SaveLoader second)
        {
            return second.LoadPriority.CompareTo(first.LoadPriority);
        }

        public virtual void LoadState(GameSaveData saveData)
        {
            if (!string.IsNullOrEmpty(saveData.ProgressMarkerKey))
                ProgressMarker.latestExecuted = ProgressMarker.FindWithKey(saveData.ProgressMarkerKey);

            foreach (var loader in subloaders)
            {
                if (loader == null)
                {
                    Debug.LogWarning(name + " has a null subloader.");
                    continue;
                }

                foreach (var item in saveData.Items)
                    loader.Load(item);
            }
        }
    }

    #endregion
}

