using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class Minigame : MonoBehaviour
{
    [SerializeField] private string sceneName;
    [SerializeField] private string returnSceneName = "MainGame";

    public string SceneName => string.IsNullOrWhiteSpace(sceneName) ? gameObject.scene.name : sceneName;
    public string ReturnSceneName => returnSceneName;

    protected virtual void Start()
    {
        BeginMinigame();
    }

    protected abstract void BeginMinigame();

    protected void EndMinigame(bool succeeded)
    {
        if (MinigameManager.Instance != null)
        {
            MinigameManager.Instance.EndMinigame(this, succeeded);
            return;
        }

        SceneManager.UnloadSceneAsync(gameObject.scene);
    }
}