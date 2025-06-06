using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using System.Collections.Generic;
using System.Collections;

public class EnhancedSettingsMenu : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Dropdown brightnessDropdown;
    [SerializeField] private Dropdown volumeDropdown;
    [SerializeField] private Dropdown fullscreenDropdown;
    [SerializeField] private BrightnessController brightnessController;
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button backButton;

    [Header("Animation")]
    [SerializeField] private float fadeDuration = 0.3f;
    private CanvasGroup settingsCanvasGroup;

    [Header("Canvas Management")]
    [SerializeField] private Canvas mainCanvas;
    [SerializeField] private Canvas settingsCanvas;

    private int previousBrightness;
    private int previousVolume;
    private int previousFullscreen;
    private void Awake()
    {
        InitializeCanvasGroup();
        InitializeBrightnessController();
        DontDestroyOnLoad(this.gameObject);
    }

    private void InitializeCanvasGroup()
    {
        if (settingsPanel != null)
        {
            settingsCanvasGroup = settingsPanel.GetComponent<CanvasGroup>() ??
                                settingsPanel.AddComponent<CanvasGroup>();
        }
    }

    private void InitializeBrightnessController()
    {
        if (brightnessController == null)
        {
            brightnessController = BrightnessController.Instance;
        }
    }

    private void Start()
    {
        SetupEventListeners();
        InitializeDropdowns();
        HideSettingsPanel();
        
    }
    private void SetupEventListeners()
    {
        brightnessDropdown?.onValueChanged.AddListener(ApplyBrightness);
        volumeDropdown?.onValueChanged.AddListener(ApplyVolume);
        fullscreenDropdown?.onValueChanged.AddListener(ApplyFullscreen);
        saveButton?.onClick.AddListener(OnSaveButtonClicked);
        backButton?.onClick.AddListener(OnBackButtonClicked);
    }

    private void HideSettingsPanel()
    {
        settingsPanel?.SetActive(false);
        settingsCanvas?.gameObject.SetActive(false);
    }

    public void InitializeDropdowns()
    {
        InitializeBrightnessDropdown();
        InitializeVolumeDropdown();
        InitializeFullscreenDropdown();
    }

    private void InitializeBrightnessDropdown()
    {
        if (brightnessDropdown != null)
        {
            brightnessDropdown.ClearOptions();
            brightnessDropdown.AddOptions(new List<string> {
            "0% (нет затемнения)",
            "20% (нет затемнения)",
            "40%",
            "60%",
            "80%",
            "100% (макс. затемнение)"
        });
        }
    }

    private void InitializeVolumeDropdown()
    {
        if (volumeDropdown != null)
        {
            volumeDropdown.ClearOptions();
            var options = new List<string>();
            for (int i = 0; i <= 100; i += 20)
            {
                options.Add($"{i}%");
            }
            volumeDropdown.AddOptions(options);
        }
    }

    private void InitializeFullscreenDropdown()
    {
        if (fullscreenDropdown != null)
        {
            fullscreenDropdown.ClearOptions();
            fullscreenDropdown.AddOptions(new List<string> { "Выкл", "Вкл" });
        }
    }
    public void OpenSettings()
    {
        SaveCurrentSettings();
        ShowSettingsPanel();
    }
    private void SaveCurrentSettings()
    {
        previousBrightness = brightnessDropdown.value;
        previousVolume = volumeDropdown.value;
        previousFullscreen = fullscreenDropdown.value;
    }

   
    public void OnSaveButtonClicked()
    {
        SaveSettings();
        CloseSettings();
    }
    public void Open()
    {
        gameObject.SetActive(true);
    }

    public void OnBackButtonClicked()
    {
        // Откатываем значения к предыдущим
        brightnessDropdown.value = previousBrightness;
        volumeDropdown.value = previousVolume;
        fullscreenDropdown.value = previousFullscreen;

        // Применяем откат
        ApplyBrightness(previousBrightness);
        ApplyVolume(previousVolume);
        ApplyFullscreen(previousFullscreen);

        CloseSettings();
    }

   
    public IEnumerator FadeIn()
    {
        float elapsed = 0;
        while (elapsed < fadeDuration)
        {
            settingsCanvasGroup.alpha = Mathf.Lerp(0, 1, elapsed / fadeDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        settingsCanvasGroup.alpha = 1;
    }

    public IEnumerator FadeOut(System.Action onComplete)
    {
        float elapsed = 0;
        while (elapsed < fadeDuration)
        {
            settingsCanvasGroup.alpha = Mathf.Lerp(1, 0, elapsed / fadeDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        settingsCanvasGroup.alpha = 0;
        onComplete?.Invoke();
    }

    public void ApplyBrightness(int valueIndex)
    {
        
        float darkness = Mathf.Max(0, (valueIndex - 1) * 0.25f);
        if (brightnessController != null)
        {
            brightnessController.SetDarkness(darkness);
        }
    }

    public void ApplyVolume(int valueIndex)
    {
        if (audioMixer != null)
        {
            float value = valueIndex * 0.2f;
            float volume = Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20;
            audioMixer.SetFloat("MasterVolume", volume);
        }
    }


    public void ApplyFullscreen(int valueIndex)
    {
        Screen.fullScreen = valueIndex == 1;
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetInt("Volume", volumeDropdown.value);
        PlayerPrefs.SetInt("Fullscreen", fullscreenDropdown.value);
        float darkness = Mathf.Max(0, (brightnessDropdown.value - 1) * 0.25f);
        PlayerPrefs.SetFloat("Brightness", darkness);
        PlayerPrefs.Save();
    }
    private void ShowSettingsPanel()
    {
        mainCanvas?.gameObject.SetActive(false);
        settingsCanvas?.gameObject.SetActive(true);
        settingsPanel?.SetActive(true);

        // Пауза игры при открытии настроек
        Time.timeScale = 0f;

        if (settingsCanvasGroup != null)
        {
            StartCoroutine(FadeIn());
        }
    }

    public void CloseSettings()
    {
        if (settingsCanvasGroup != null)
            StartCoroutine(FadeOut(() =>
            {
                if (settingsPanel != null)
                    settingsPanel.SetActive(false);
                if (settingsCanvas != null)
                    settingsCanvas.gameObject.SetActive(false);
                if (mainCanvas != null)
                    mainCanvas.gameObject.SetActive(true);

                // Возобновление игры после закрытия настроек
                Time.timeScale = 1f;
            }));
        else
        {
            if (settingsPanel != null)
                settingsPanel.SetActive(false);
            if (settingsCanvas != null)
                settingsCanvas.gameObject.SetActive(false);
            if (mainCanvas != null)
                mainCanvas.gameObject.SetActive(true);

            // Возобновление игры после закрытия настроек
            Time.timeScale = 1f;
        }
    }
}