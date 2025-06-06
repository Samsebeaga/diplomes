using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField] private Animation loadingAnimation; // ������ �� ��������� Animation
    [SerializeField] private float fixedLoadDuration = 10f; // ������������� 10 ������
    [SerializeField] private string animationName = "Loading"; // ��� ��������

    void Start()
    {
        // ��������� �������� ��������
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
        operation.allowSceneActivation = false; // ��������� �������������� �������

        while (timer < fixedLoadDuration)
        {
            timer += Time.deltaTime;

            // ������������� �������� � ��������
            if (loadingAnimation != null && loadingAnimation.IsPlaying(animationName))
            {
                // ����������� ����� ��� ����������� ��������
                float normalizedTime = timer / fixedLoadDuration;
                loadingAnimation[animationName].normalizedTime = normalizedTime % 1;
            }

            yield return null;
        }

        // �� ��������� 10 ������ ���������� �����
        operation.allowSceneActivation = true;
    }
}
