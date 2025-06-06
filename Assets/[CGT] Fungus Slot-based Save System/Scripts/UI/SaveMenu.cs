using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CGTUnity.Fungus.SaveSystem
{
    /// <summary>
    /// Main manager of the save system's UI, meant to be the main interface used when
    /// accessing the save system through said UI.
    /// </summary>
    public class SaveMenu : MonoBehaviour
    {
        #region Fields
        public Dictionary<string, GameSaveData> WrittenSaves { get; private set; } = new Dictionary<string, GameSaveData>();
        [SerializeField] protected GameLoader gameLoader;
        [SerializeField] protected GameSaver gameSaver;
        [SerializeField] protected SaveSlotManager slotManager;
        [SerializeField] protected SaveManager saveManager;
        [SerializeField] protected Button loadButton;
        [SerializeField] protected Button saveButton;
        [SerializeField] protected Button continueButton;
        [SerializeField] protected Button restartButton;
        protected CanvasGroup canvasGroup;
        #endregion

        protected virtual void Awake()
        {
            // Make sure we have the components we need
            if (gameLoader == null) gameLoader = FindObjectOfType<GameLoader>();
            if (gameSaver == null) gameSaver = FindObjectOfType<GameSaver>();
            if (slotManager == null) slotManager = FindObjectOfType<SaveSlotManager>();
            if (saveManager == null) saveManager = FindObjectOfType<SaveManager>();
            canvasGroup = GetComponent<CanvasGroup>();
        }

        protected virtual void Start()
        {
            if (loadButton != null)
                loadButton.onClick.AddListener(LoadFromSelectedSlot);

            if (saveButton != null)
                saveButton.onClick.AddListener(SaveToSelectedSlot);

            if (continueButton != null)
                continueButton.onClick.AddListener(LoadLatestSave);

            if (restartButton != null)
                restartButton.onClick.AddListener(RestartGame);

            UpdateButtonsInteractivity();
        }

        public virtual void Open()
        {
            canvasGroup.alpha = 1;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;

            // Обновляем состояние кнопок при открытии меню
            UpdateButtonsInteractivity();

            // Обновляем список сохранений
            slotManager.UpdateSlots(); // Используем UpdateSlots, как в новом варианте
        }

        #region Saving
        /// <summary>
        /// Saves new save data to disk and the slot with the passed number.
        /// </summary>
        public virtual void SaveToSlot(int slotNumber)
        {
            saveManager.AddSave(slotNumber, true);
        }

        public virtual void SaveToSlot(SaveSlot slot)
        {
            saveManager.AddSave(slot, true);
        }

        private void RestartGame()
        {
            // Реализация перезапуска игры
            Debug.Log("Game restarted");
        }
        /// <summary>
        /// Saves new save data to disk and the slot the data was set to be assigned to.
        /// </summary>
        public virtual void SaveToSlot(GameSaveData saveData)
        {
            saveManager.AddSave(saveData, true);
        }

        public virtual void SaveToSelectedSlot()
        {
            var slot = slotManager.selectedSlot;

            if (slot == null)
            {
                Debug.LogWarning("SaveToSelectedSlot: No slot selected.");
                return;
            }

            Debug.Log("Saving to single slot...");

            var newSaveData = gameSaver.CreateSave(slot.Number);

            if (slot.SaveData != null)
            {
                saveManager.EraseSave(slot.SaveData);  // вместо ReplaceSave
            }

            saveManager.AddSave(newSaveData);

            slotManager.UpdateSlots(); // через slotManager, а не напрямую
        }

        public virtual void LoadLatestSave()
        {
            var latestSave = saveManager.GetLatestSave();
            if (latestSave != null)
            {
                saveManager.LoadSave(latestSave);
            }
            else
            {
                Debug.LogWarning("No saves found to load.");
            }
        }
        #endregion

        #region Loading
        /// <summary>
        /// Loads the save data assigned to the slot with the passed number, if 
        /// there is any.
        /// </summary>
        public virtual void LoadFromSlot(int slotNumber)
        {
            saveManager.LoadSave(slotNumber);
        }

        public virtual void LoadFromSlot(SaveSlot slot)
        {
            saveManager.LoadSave(slot);
        }

        public virtual void Load(GameSaveData saveData)
        {
            saveManager.LoadSave(saveData);
        }

        public virtual void LoadFromSelectedSlot()
        {
            var slot = slotManager.selectedSlot;
            if (slot == null || slot.SaveData == null)
                return;

            saveManager.LoadSave(slot.SaveData);
        }

        #endregion

        #region Save-erasing
        /// <summary>
        /// Clears the slot with the passed number, erasing the save data it had been
        /// representing.
        /// </summary>
        public virtual void ClearSlot(int slotNumber)
        {
            saveManager.EraseSave(slotNumber);
        }

        public virtual void ClearSlot(SaveSlot slot)
        {
            saveManager.EraseSave(slot);
        }

        public virtual void ClearSlot(GameSaveData saveData)
        {
            saveManager.EraseSave(saveData);
        }

        public virtual void ClearSelectedSlot()
        {
            var slot = slotManager.selectedSlot;
            if (slot == null || slot.SaveData == null)
                return;

            saveManager.EraseSave(slot.SaveData);
        }
        #endregion

        #region Save-retrieval

        /// <summary>
        /// Returns the save data assigned to the slot with the passed number.
        /// </summary>
        public virtual GameSaveData GetSaveFromSlot(int slotNumber)
        {
            var slot = slotManager.FindSlot(slotNumber);
            if (slot != null) return slot.SaveData;

            return null;
        }
        #endregion

        public bool HasExistingSaves()
        {
            // Обращаемся к статическому полю через тип
            return SaveManager.WrittenSaves != null && SaveManager.WrittenSaves.Count > 0;
        }

        public void UpdateButtonsInteractivity()
        {
            bool hasSaves = HasExistingSaves();

            if (loadButton != null)
                loadButton.interactable = hasSaves;

            if (continueButton != null)
                continueButton.interactable = hasSaves;

            if (saveButton != null)
                saveButton.interactable = true;

            if (restartButton != null)
                restartButton.interactable = true;
        }

        #region Menu Display
        public virtual void Close()
        {
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        #endregion
    }
}
