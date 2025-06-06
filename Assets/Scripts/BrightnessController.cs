using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BrightnessController : MonoBehaviour
{
    public static BrightnessController Instance { get; private set; }
    public RawImage brightnessOverlay;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeOverlay();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeOverlay()
    {
        if (brightnessOverlay == null)
        {
            CreateBrightnessOverlay();
        }
    }

    private void CreateBrightnessOverlay()
    {
        GameObject canvasObj = new GameObject("BrightnessCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;
        DontDestroyOnLoad(canvasObj);

        GameObject overlayObj = new GameObject("BrightnessOverlay");
        overlayObj.transform.SetParent(canvasObj.transform);
        brightnessOverlay = overlayObj.AddComponent<RawImage>();

        brightnessOverlay.color = Color.black;
        brightnessOverlay.rectTransform.anchorMin = Vector2.zero;
        brightnessOverlay.rectTransform.anchorMax = Vector2.one;
        brightnessOverlay.rectTransform.offsetMin = Vector2.zero;
        brightnessOverlay.rectTransform.offsetMax = Vector2.zero;
    }

  
    public void SetDarkness(float darkness)
    {
        if (brightnessOverlay != null)
        {
            Color c = brightnessOverlay.color;
            c.a = Mathf.Clamp01(darkness); // 0 (прозрачно) - 1 (полное затемнение)
            brightnessOverlay.color = c;
        }
    }
    public void SetBrightnessSmooth(float targetTransparency, float duration = 0.5f)
    {
        StartCoroutine(FadeTransparency(targetTransparency, duration));
    }

    private IEnumerator FadeTransparency(float target, float duration)
    {
        float start = 1 - brightnessOverlay.color.a; 
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float newTransparency = Mathf.Lerp(start, target, elapsed / duration);
            SetDarkness(newTransparency);
            elapsed += Time.deltaTime;
            yield return null;
        }

        SetDarkness(target);
    }

   
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
       
        if (brightnessOverlay == null)
        {
            CreateBrightnessOverlay();
            float darkness = PlayerPrefs.GetFloat("Brightness", 0f);
            SetDarkness(darkness);
        }
    }
}

