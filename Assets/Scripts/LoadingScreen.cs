using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField] private Animation loadingAnimation; // Ссылка на компонент Animation
    [SerializeField] private float fixedLoadDuration = 10f; // Фиксированные 10 секунд
    [SerializeField] private string animationName = "Loading"; // Имя анимации

    void Start()
    {
        // Запускаем анимацию загрузки
        if (loadingAnimation != null)
        {
            loadingAnimation.Play(animationName);
        }

        StartCoroutine(LoadAsync("Game"));
        
    }

    IEnumerator LoadAsync(string sceneName)
    {
        float timer = 0f;
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false; // Отключаем автоматический переход

        while (timer < fixedLoadDuration)
        {
            timer += Time.deltaTime;

            // Синхронизация анимации с таймером
            if (loadingAnimation != null && loadingAnimation.IsPlaying(animationName))
            {
                // Нормализуем время для циклической анимации
                float normalizedTime = timer / fixedLoadDuration;
                loadingAnimation[animationName].normalizedTime = normalizedTime % 1;
            }

            yield return null;
        }

        // По истечении 10 секунд активируем сцену
        operation.allowSceneActivation = true;
    }
}
