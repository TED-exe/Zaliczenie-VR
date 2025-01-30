using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Slider = UnityEngine.UI.Slider;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private Slider progressBar;
    [SerializeField] private float fadeDuration = 0.5f; 
    
    [SerializeField] private GameObject CreditsPanel;

    [SerializeField] private GameObject InfoPanel;

    [SerializeField] private GameObject MainPanel;
    
    public void StartButton(string sceneName)
    {
        StartCoroutine(LoadSceneAsync(sceneName));
    }

    public void CreditsButton()
    {
        CreditsPanel.SetActive(true);
        MainPanel.SetActive(false);
        InfoPanel.SetActive(false);
    }

    public void InfoButton()
    {
        CreditsPanel.SetActive(false);
        MainPanel.SetActive(false);
        InfoPanel.SetActive(true);
    }
    
    public void ExitButton()
    {
        Debug.Log("Zamykanie gry...");

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    public void BackToMainMenu()
    {
        CreditsPanel.SetActive(false);
        MainPanel.SetActive(true);
        InfoPanel.SetActive(false);
    }
    private IEnumerator LoadSceneAsync(string sceneName)
    {
        // Aktywujemy ekran ładowania
        loadingScreen.SetActive(true);
        progressBar.value = 0f;

        // Opcjonalnie: Fade-in ekranu ładowania
        CanvasGroup canvasGroup = loadingScreen.GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            yield return StartCoroutine(FadeCanvas(canvasGroup, 0f, 1f, fadeDuration));
        }

        // Rozpocznij asynchroniczne ładowanie sceny
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false; // Blokujemy automatyczną aktywację

        // Aktualizujemy pasek postępu
        while (operation.progress < 0.9f)
        {
            progressBar.value = operation.progress;
            yield return null;
        }

        // Pasek na 100% przed aktywacją sceny
        progressBar.value = 1f;
        yield return new WaitForSeconds(0.5f); // Krótkie zatrzymanie, by gracz zauważył 100%

        // Aktywujemy scenę
        operation.allowSceneActivation = true;

        // Czekamy na zakończenie ładowania
        while (!operation.isDone)
        {
            yield return null;
        }

        // Opcjonalnie: Fade-out ekranu ładowania
        if (canvasGroup != null)
        {
            yield return StartCoroutine(FadeCanvas(canvasGroup, 1f, 0f, fadeDuration));
        }

        // Dezaktywujemy ekran ładowania
        loadingScreen.SetActive(false);
    }

    private IEnumerator FadeCanvas(CanvasGroup canvasGroup, float startAlpha, float endAlpha, float duration)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
            yield return null;
        }
        canvasGroup.alpha = endAlpha;
    }
}
