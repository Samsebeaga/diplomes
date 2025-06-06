using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Fungus;
using System.Runtime.Serialization.Formatters.Binary;
using static InGameMenuManager.CustomMenuDialog;
using System.Collections;

public class InGameMenuManager : MonoBehaviour
{
    private List<MenuOption> currentOptions = new List<MenuOption>();
    private GameObject menuButton;
    private GameObject inGameMenuPanel;
    private GameObject confirmExitPanel;
    private Button continueButton, saveButton, loadButton, settingsButton, quitButton;
    private Button confirmYesButton, confirmNoButton;

    [SerializeField] private Flowchart flowchart;
    [SerializeField] private EnhancedSettingsMenu settingsMenu;
    [SerializeField] private CustomMenuDialog customMenuDialog;

    private const string SAVE_DIRECTORY = "Saves";
    private const string SAVE_EXTENSION = ".save";

    [System.Serializable]
    public class MenuOptionData
    {
        public string text;
        public string targetBlockName;
    }

    [System.Serializable]
    public class SaveData
    {
        public string currentScene;
        public string flowchartName;
        public string blockName;
        public int commandIndex;
        public string sayText;
        public List<MenuOptionData> menuOptions;
        public bool isSayActive;
        public bool isMenuActive;
        public Dictionary<string, VariableData> variables;
    }

    [System.Serializable]
    public class VariableData
    {
        public string type;
        public string value;
    }

    private void Start()
    {
        if (!flowchart) flowchart = FindObjectOfType<Flowchart>();
        EnsureSaveDirectoryExists();
        CreateUI();

        Block block = flowchart.FindBlock("Start");
        if (block != null)
        {
            flowchart.ExecuteBlock(block);
        }
    }

    private void EnsureSaveDirectoryExists()
    {
        string savePath = Path.Combine(Application.persistentDataPath, SAVE_DIRECTORY);
        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }
    }

    private string GetSavePath(string slotName)
    {
        return Path.Combine(Application.persistentDataPath, SAVE_DIRECTORY, $"{slotName}{SAVE_EXTENSION}");
    }

    public void SaveGame()
    {
        SaveData saveData = new SaveData
        {
            currentScene = SceneManager.GetActiveScene().name,
            flowchartName = flowchart.name,
            blockName = flowchart.GetExecutingBlocks().Count > 0 ?
                flowchart.GetExecutingBlocks()[0].BlockName : "",
           commandIndex = flowchart.GetExecutingBlocks().Count > 0 ?
    flowchart.GetExecutingBlocks()[0].ActiveCommand.CommandIndex : 0,
            variables = SaveVariables(),
            isSayActive = SayDialog.ActiveSayDialog != null && SayDialog.ActiveSayDialog.isActiveAndEnabled,
            isMenuActive = customMenuDialog != null && customMenuDialog.gameObject.activeSelf
        };

        if (saveData.isSayActive)
        {
            saveData.sayText = GetCurrentSayText();
        }

        if (saveData.isMenuActive)
        {
            saveData.menuOptions = customMenuDialog.GetOptionsAsData();
        }

        string savePath = GetSavePath("slot1");
        BinaryFormatter formatter = new BinaryFormatter();

        using (FileStream stream = new FileStream(savePath, FileMode.Create))
        {
            formatter.Serialize(stream, saveData);
        }

        Debug.Log($"Игра сохранена: {savePath}");
    }

    public void LoadGame()
    {
        string savePath = GetSavePath("slot1");
        if (!File.Exists(savePath))
        {
            Debug.LogWarning("Файл сохранения не найден");
            return;
        }

        BinaryFormatter formatter = new BinaryFormatter();
        SaveData saveData;

        using (FileStream stream = new FileStream(savePath, FileMode.Open))
        {
            saveData = (SaveData)formatter.Deserialize(stream);
        }

        if (SceneManager.GetActiveScene().name != saveData.currentScene)
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.LoadScene(saveData.currentScene);

            void OnSceneLoaded(Scene scene, LoadSceneMode mode)
            {
                if (scene.name == saveData.currentScene)
                {
                    SceneManager.sceneLoaded -= OnSceneLoaded;
                    ContinueLoading(saveData);
                }
            }
            return;
        }

        ContinueLoading(saveData);
    }

    // Обработчик для события SceneManager.sceneLoaded
    private void ContinueLoadingHandler(Scene scene, LoadSceneMode mode)
    {
        // Этот метод необходим для отписки, но сам вызов происходит в анонимном методе выше
    }

    // Метод для продолжения загрузки сохранения после загрузки сцены
    private void ContinueLoading(SaveData saveData)
    {
        // 1. Полностью останавливаем текущее выполнение
        flowchart.StopAllBlocks();

        // 2. Закрываем все активные диалоги
        if (SayDialog.ActiveSayDialog != null)
        {
            SayDialog.ActiveSayDialog.gameObject.SetActive(false);
        }

        if (MenuDialog.ActiveMenuDialog != null)
        {
            MenuDialog.ActiveMenuDialog.gameObject.SetActive(false);
        }

        // 3. Восстанавливаем переменные
        LoadVariables(flowchart, saveData.variables);

        // 4. Находим нужный блок
        Block block = flowchart.FindBlock(saveData.blockName);
        if (block != null)
        {
            // 5. Запускаем блок с ТОЧНОЙ позиции сохранения
            flowchart.ExecuteBlock(block, saveData.commandIndex);

            // 6. Восстанавливаем SayDialog если нужно
            if (saveData.isSayActive && !string.IsNullOrEmpty(saveData.sayText))
            {
                StartCoroutine(RestoreSayNextFrame(saveData.sayText));
            }
            else if (saveData.isMenuActive)
            {
                customMenuDialog.RestoreMenuDialog(saveData.menuOptions, flowchart);
            }
        }

        CloseInGameMenu();
        Debug.Log($"Игра загружена из: {GetSavePath("slot1")}");
    }

    private IEnumerator RestoreSayNextFrame(string text)
    {
        yield return null;

        SayDialog sayDialog = SayDialog.GetSayDialog();
        if (sayDialog != null)
        {
            sayDialog.gameObject.SetActive(true);

            TextMeshProUGUI storyText = sayDialog.GetComponentInChildren<TextMeshProUGUI>();
            if (storyText != null)
            {
                storyText.text = text;
            }
            else
            {
                Text legacyText = sayDialog.GetComponentInChildren<Text>();
                if (legacyText != null)
                {
                    legacyText.text = text;
                }
            }
        }
    }

    private Dictionary<string, VariableData> SaveVariables()
    {
        var savedVariables = new Dictionary<string, VariableData>();
        foreach (var variable in flowchart.Variables)
        {
            string typeName = variable.GetType().Name;
            string valueStr = "";

            if (variable is BooleanVariable boolVar)
            {
                valueStr = boolVar.Value.ToString();
            }
            else if (variable is IntegerVariable intVar)
            {
                valueStr = intVar.Value.ToString();
            }
            else if (variable is FloatVariable floatVar)
            {
                valueStr = floatVar.Value.ToString();
            }
            else if (variable is StringVariable stringVar)
            {
                valueStr = stringVar.Value;
            }

            savedVariables[variable.Key] = new VariableData
            {
                type = typeName,
                value = valueStr
            };
        }
        return savedVariables;
    }
    public List<MenuOption> GetOptions()
    {
        return currentOptions;
    }
    
    private void LoadVariables(Flowchart targetFlowchart, Dictionary<string, VariableData> variables)
    {
        foreach (var varData in variables)
        {
            Variable variable = targetFlowchart.GetVariable(varData.Key);
            if (variable != null)
            {
                switch (varData.Value.type)
                {
                    case "BooleanVariable":
                        if (bool.TryParse(varData.Value.value, out bool boolValue))
                            ((BooleanVariable)variable).Value = boolValue;
                        break;
                    case "IntegerVariable":
                        if (int.TryParse(varData.Value.value, out int intValue))
                            ((IntegerVariable)variable).Value = intValue;
                        break;
                    case "FloatVariable":
                        if (float.TryParse(varData.Value.value, out float floatValue))
                            ((FloatVariable)variable).Value = floatValue;
                        break;
                    case "StringVariable":
                        ((StringVariable)variable).Value = varData.Value.value;
                        break;
                }
            }
        }
    }

    private string GetCurrentSayText()
    {
        SayDialog sayDialog = SayDialog.GetSayDialog();
        if (sayDialog != null && sayDialog.isActiveAndEnabled)
        {
            TextMeshProUGUI storyText = sayDialog.GetComponentInChildren<TextMeshProUGUI>();
            if (storyText != null) return storyText.text;

            Text legacyText = sayDialog.GetComponentInChildren<Text>();
            if (legacyText != null) return legacyText.text;
        }
        return null;
    }

    private void RestoreSayDialog(string text)
    {
        SayDialog sayDialog = SayDialog.GetSayDialog();
        if (sayDialog != null && !string.IsNullOrEmpty(text))
        {
            if (!sayDialog.gameObject.activeSelf)
            {
                sayDialog.gameObject.SetActive(true);
            }

            TextMeshProUGUI storyTextTMP = sayDialog.GetComponentInChildren<TextMeshProUGUI>();
            if (storyTextTMP != null)
            {
                storyTextTMP.text = text;
            }
            else
            {
                Text legacyText = sayDialog.GetComponentInChildren<Text>();
                if (legacyText != null)
                {
                    legacyText.text = text;
                }
            }
        }
    }

    [System.Serializable]
    public class CustomMenuDialog : MonoBehaviour
    {
        [SerializeField] private Transform optionsParent;
        [SerializeField] private Button optionButtonPrefab;
        [SerializeField] private Flowchart flowchart;
        private List<MenuOption> currentOptions = new List<MenuOption>();
        private MenuDialog dialog;
        [System.Serializable]
        public class MenuOption
        {
            public string Text;
            public Block TargetBlock;
            public Button UIButton;
        }
        public CustomMenuDialog(MenuDialog dialog)
        {
            this.dialog = dialog;
        }
        public List<(string text, string targetBlockName)> GetOptions()
        {
            var options = new List<(string, string)>();

            // Ищем все кнопки Button среди дочерних объектов MenuDialog
            Button[] buttons = dialog.GetComponentsInChildren<Button>(true);

            foreach (var button in buttons)
            {
                if (button.gameObject.activeInHierarchy)
                {
                    // Компонент MenuOption хранит данные об опции
                    var menuOption = button.GetComponent<MenuOption>();
                    if (menuOption != null)
                    {
                        string text = menuOption.Text;
                        string targetBlock = menuOption.TargetBlock != null ? menuOption.TargetBlock.BlockName : "";
                        options.Add((text, targetBlock));
                    }
                }
            }

            return options;
        }
    

    public void AddOption(string text, Block targetBlock)
        {
            Button button = Instantiate(optionButtonPrefab, optionsParent);
            var textComponent = button.GetComponentInChildren<Text>();
            if (textComponent != null)
            {
                textComponent.text = text;
            }

            button.onClick.AddListener(() =>
            {
                flowchart.ExecuteBlock(targetBlock);
            });

            currentOptions.Add(new MenuOption
            {
                Text = text,
                TargetBlock = targetBlock,
                UIButton = button
            });
        }

        public void AddOption(string text, Block targetBlock, Flowchart flowchart)
        {
            Button button = Instantiate(optionButtonPrefab, optionsParent);
            var textComponent = button.GetComponentInChildren<Text>();
            if (textComponent != null)
            {
                textComponent.text = text;
            }

            button.onClick.AddListener(() =>
            {
                flowchart.ExecuteBlock(targetBlock);
            });

            currentOptions.Add(new MenuOption
            {
                Text = text,
                TargetBlock = targetBlock,
                UIButton = button
            });
        }

        public void ClearOptions()
        {
            foreach (var opt in currentOptions)
            {
                if (opt.UIButton != null)
                {
                    Destroy(opt.UIButton.gameObject);
                }
            }
            currentOptions.Clear();
        }

        public List<MenuOptionData> GetOptionsAsData()
        {
            List<MenuOptionData> data = new List<MenuOptionData>();
            foreach (var opt in currentOptions)
            {
                data.Add(new MenuOptionData { text = opt.Text, targetBlockName = opt.TargetBlock.BlockName });
            }
            return data;
        }

        public void RestoreMenuDialog(List<MenuOptionData> options, Flowchart flowchart)
        {
            ClearOptions();
            foreach (var opt in options)
            {
                Block block = flowchart.FindBlock(opt.targetBlockName);
                if (block != null)
                {
                    AddOption(opt.text, block);
                }
            }
        }
    }

   
  
    
    public class MenuOption
    {
        public string Text;
        public Block TargetBlock;
        public Button UIButton;
    }


    private List<MenuOptionData> GetCurrentMenuOptions()
    {
        if (customMenuDialog == null) return null;

        List<MenuOptionData> options = new List<MenuOptionData>();

        foreach (var option in customMenuDialog.GetOptions())
        {
            options.Add(new MenuOptionData
            {
                text = option.text,                     // маленькая буква
                targetBlockName = option.targetBlockName // маленькая буква
            });
        }

        return options;
    }




    public void OpenInGameMenu()
    {
        inGameMenuPanel.SetActive(true);
        menuButton.SetActive(false);
        Time.timeScale = 0f;
    }

    public void CloseInGameMenu()
    {
        inGameMenuPanel.SetActive(false);
        menuButton.SetActive(true);
        Time.timeScale = 1f;
    }

    public void OpenSettings()
    {
        CloseInGameMenu();
        settingsMenu?.OpenSettings();
    }

    public void OpenConfirmExit()
    {
        confirmExitPanel.SetActive(true);
    }

    public void CloseConfirmExit()
    {
        confirmExitPanel.SetActive(false);
        inGameMenuPanel.SetActive(true);
    }

    public void ExitToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }

    // Вложенный класс CustomMenuDialog
   
              public void ClearOptions() { }
    
    public void CreateUI()
    {
        // Canvas
        GameObject canvasObj = new GameObject("InGameCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;
        canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObj.AddComponent<GraphicRaycaster>();

        // Кнопка "домик"
        menuButton = CreateIconButton("MenuButton", canvasObj.transform,
            new Vector2(60, 60), new Vector2(1, 1), new Vector2(-20, -20),
            "Icons/home");
        menuButton.GetComponent<Button>().onClick.AddListener(OpenInGameMenu);

        // Меню (увеличено для центрирования кнопок)
        inGameMenuPanel = CreateUIPanel("MenuPanel", canvasObj.transform,
            new Vector2(450, 500), new Vector2(0.5f, 0.5f), Vector2.zero);
        inGameMenuPanel.SetActive(false);

        // Параметры для центрирования кнопок
        float buttonWidth = 300f;
        float buttonHeight = 50f;
        float totalHeight = 350f; // Общая высота всех кнопок с промежутками
        float startY = totalHeight / 2 - buttonHeight / 2; // Начальная позиция для первой кнопки
        float spacing = 70f; // Расстояние между кнопками

        // Создание кнопок (центрированных по вертикали и горизонтали)
        continueButton = CreateUIButton("Продолжить", inGameMenuPanel.transform,
            new Vector2(buttonWidth, buttonHeight), new Vector2(0.5f, 0.5f),
            new Vector2(0, startY)).GetComponent<Button>();

        saveButton = CreateUIButton("Сохранить", inGameMenuPanel.transform,
            new Vector2(buttonWidth, buttonHeight), new Vector2(0.5f, 0.5f),
            new Vector2(0, startY - spacing)).GetComponent<Button>();

        loadButton = CreateUIButton("Загрузить", inGameMenuPanel.transform,
            new Vector2(buttonWidth, buttonHeight), new Vector2(0.5f, 0.5f),
            new Vector2(0, startY - 2 * spacing)).GetComponent<Button>();

        settingsButton = CreateUIButton("Настройки", inGameMenuPanel.transform,
            new Vector2(buttonWidth, buttonHeight), new Vector2(0.5f, 0.5f),
            new Vector2(0, startY - 3 * spacing)).GetComponent<Button>();

        quitButton = CreateUIButton("В главное меню", inGameMenuPanel.transform,
            new Vector2(buttonWidth, buttonHeight), new Vector2(0.5f, 0.5f),
            new Vector2(0, startY - 4 * spacing)).GetComponent<Button>();

        // Назначение действий кнопкам
        continueButton.onClick.AddListener(CloseInGameMenu);
        saveButton.onClick.AddListener(SaveGame);
        loadButton.onClick.AddListener(LoadGame);
        settingsButton.onClick.AddListener(OpenSettings);
        quitButton.onClick.AddListener(OpenConfirmExit);

        // Подтверждение выхода (также центрированное)
        confirmExitPanel = CreateUIPanel("ConfirmExitPanel", canvasObj.transform,
            new Vector2(400, 200), new Vector2(0.5f, 0.5f), Vector2.zero);
        confirmExitPanel.SetActive(false);

        CreateUIText("Вы уверены, что хотите выйти?\nВесь несохранённый прогресс будет потерян.",
            confirmExitPanel.transform, new Vector2(380, 80),
            new Vector2(0.5f, 0.7f), Vector2.zero);

        confirmYesButton = CreateUIButton("Да", confirmExitPanel.transform,
            new Vector2(150, 40), new Vector2(0.3f, 0.2f), Vector2.zero).GetComponent<Button>();
        confirmNoButton = CreateUIButton("Нет", confirmExitPanel.transform,
            new Vector2(150, 40), new Vector2(0.7f, 0.2f), Vector2.zero).GetComponent<Button>();

        confirmYesButton.onClick.AddListener(ExitToMainMenu);
        confirmNoButton.onClick.AddListener(CloseConfirmExit);
    }
    public GameObject CreateUIPanel(string name, Transform parent, Vector2 size, Vector2 anchor, Vector2 anchoredPosition)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        var rt = panel.AddComponent<RectTransform>();
        rt.sizeDelta = size;
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = anchor;
        rt.anchoredPosition = anchoredPosition;
        var img = panel.AddComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 0.85f);
        return panel;
    }

    public GameObject CreateUIButton(string text, Transform parent, Vector2 size, Vector2 anchor, Vector2 anchoredPosition)
    {
        GameObject btnObj = new GameObject(text);
        btnObj.transform.SetParent(parent, false);
        var rt = btnObj.AddComponent<RectTransform>();
        rt.sizeDelta = size;
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = anchor;
        rt.anchoredPosition = anchoredPosition;

        var img = btnObj.AddComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 0.6f);

        var btn = btnObj.AddComponent<Button>();

        GameObject txtObj = new GameObject("Text");
        txtObj.transform.SetParent(btnObj.transform, false);

        var txtRT = txtObj.AddComponent<RectTransform>();
        txtRT.anchorMin = Vector2.zero;
        txtRT.anchorMax = Vector2.one;
        txtRT.pivot = new Vector2(0.5f, 0.5f);

        var txt = txtObj.AddComponent<TextMeshProUGUI>();
        txt.text = text;
        txt.alignment = TextAlignmentOptions.Center;
        txt.color = Color.white;
        txt.fontSize = 24;
        txt.raycastTarget = false;

        return btnObj;
    }

    public GameObject CreateUIText(string text, Transform parent, Vector2 size, Vector2 anchor, Vector2 anchoredPosition)
    {
        GameObject txtObj = new GameObject("Label");
        txtObj.transform.SetParent(parent, false);

        var rt = txtObj.AddComponent<RectTransform>();
        rt.sizeDelta = size;
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = anchor;
        rt.anchoredPosition = anchoredPosition;

        var txt = txtObj.AddComponent<TextMeshProUGUI>();
        txt.text = text;
        txt.alignment = TextAlignmentOptions.Center;
        txt.color = Color.white;
        txt.fontSize = 22;
        txt.textWrappingMode = TextWrappingModes.Normal;
        txt.overflowMode = TextOverflowModes.Truncate;

        return txtObj;
    }

    public GameObject CreateIconButton(string name, Transform parent, Vector2 size, Vector2 anchor, Vector2 anchoredPosition, string iconPath)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);

        var rt = btnObj.AddComponent<RectTransform>();
        rt.sizeDelta = size;
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = anchor;
        rt.anchoredPosition = anchoredPosition;

        var img = btnObj.AddComponent<Image>();
        Sprite icon = Resources.Load<Sprite>(iconPath);
        img.sprite = icon;
        img.preserveAspect = true;

        btnObj.AddComponent<Button>();

        return btnObj;
    }
}
