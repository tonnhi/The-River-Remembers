using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using ConNuoc.Data;
using ConNuoc.UI;
using ConNuoc.Core;

public class MinigameManager : MonoBehaviour
{
    public static MinigameManager Instance { get; private set; }

    public event UnityAction OnMinigameStarted;
    public event UnityAction OnMinigameEnded;

    private Scene _returnScene;
    private Scene _minigameScene;
    private DecisionSO _pendingDecision;
    private DecisionOption _pendingOption;
    private float _currentTimeBonusSeconds;
    private bool _minigameActive;
    private readonly System.Collections.Generic.List<Behaviour> _suppressedRenderComponents =
        new System.Collections.Generic.List<Behaviour>();
    private readonly System.Collections.Generic.List<bool> _renderComponentStates =
        new System.Collections.Generic.List<bool>();

    public float CurrentTimeBonusSeconds => _currentTimeBonusSeconds;

    private void Awake()
    {
        if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
    }

    public void StartMinigame(Minigame minigameName)
    {
        StartMinigame(minigameName.SceneName);
    }

    public void StartMinigame(string sceneName)
    {
        BeginMinigame(sceneName, null, null, 0f);
    }

    public void StartMinigame(DecisionSO decision, DecisionOption option)
    {
        BeginMinigame(option.minigameSceneName, decision, option, option.minigameTimeBonusSeconds);
    }

    private void BeginMinigame(string sceneName, DecisionSO decision, DecisionOption option, float timeBonusSeconds)
    {
        if (_minigameActive)
        {
            Debug.LogWarning("MinigameManager: a minigame is already active.");
            return;
        }

        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError("MinigameManager: cannot load a minigame with an empty scene name.");
            ReopenDecision(decision);
            return;
        }

        _minigameActive = true;
        _returnScene = SceneManager.GetActiveScene();
        _pendingDecision = decision;
        _pendingOption = option;
        _currentTimeBonusSeconds = Mathf.Max(0f, timeBonusSeconds);
        SuppressReturnSceneRendering();
        OnMinigameStarted?.Invoke();
        StartCoroutine(LoadMinigame(sceneName));
    }

    private IEnumerator LoadMinigame(string sceneName)
    {
        AsyncOperation loadOperation;
        try
        {
            loadOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        }
        catch (Exception exception)
        {
            Debug.LogError("MinigameManager: failed to load scene '" + sceneName + "'. " + exception.Message);
            FinishLoadFailure();
            yield break;
        }

        if (loadOperation == null)
        {
            Debug.LogError("MinigameManager: could not start loading scene '" + sceneName + "'.");
            FinishLoadFailure();
            yield break;
        }

        yield return loadOperation;
        _minigameScene = SceneManager.GetSceneByName(sceneName);
        if (!_minigameScene.IsValid() || !_minigameScene.isLoaded)
        {
            Debug.LogError("MinigameManager: loaded scene '" + sceneName + "' could not be found.");
            FinishLoadFailure();
            yield break;
        }

        SceneManager.SetActiveScene(_minigameScene);
    }

    public void EndMinigame(Minigame minigame, bool succeeded)
    {
        if (!_minigameActive) return;
        StartCoroutine(UnloadMinigame(succeeded));
    }

    private IEnumerator UnloadMinigame(bool succeeded)
    {
        if (_minigameScene.IsValid() && _minigameScene.isLoaded)
        {
            AsyncOperation unloadOperation = SceneManager.UnloadSceneAsync(_minigameScene);
            if (unloadOperation != null) yield return unloadOperation;
        }

        if (_returnScene.IsValid() && _returnScene.isLoaded)
        {
            SceneManager.SetActiveScene(_returnScene);
        }
        RestoreReturnSceneRendering();

        DecisionSO decision = _pendingDecision;
        DecisionOption option = _pendingOption;
        ClearPendingMinigame();
        OnMinigameEnded?.Invoke();

        if (decision == null) yield break;

        if (succeeded)
        {
            if (DecisionResolver.Instance != null)
            {
                DecisionResolver.Instance.ResolveCompletedMinigame(decision, option);
            }
        }
        else
        {
            ReopenDecision(decision);
        }
    }

    private void FinishLoadFailure()
    {
        DecisionSO decision = _pendingDecision;
        RestoreReturnSceneRendering();
        ClearPendingMinigame();
        OnMinigameEnded?.Invoke();
        ReopenDecision(decision);
    }

    private void SuppressReturnSceneRendering()
    {
        foreach (GameObject root in _returnScene.GetRootGameObjects())
        {
            foreach (Camera sceneCamera in root.GetComponentsInChildren<Camera>(true))
            {
                Suppress(sceneCamera);
            }

            foreach (Canvas sceneCanvas in root.GetComponentsInChildren<Canvas>(true))
            {
                Suppress(sceneCanvas);
            }
        }
    }

    private void Suppress(Behaviour component)
    {
        _suppressedRenderComponents.Add(component);
        _renderComponentStates.Add(component.enabled);
        component.enabled = false;
    }

    private void RestoreReturnSceneRendering()
    {
        for (int index = 0; index < _suppressedRenderComponents.Count; index++)
        {
            if (_suppressedRenderComponents[index] != null)
            {
                _suppressedRenderComponents[index].enabled = _renderComponentStates[index];
            }
        }

        _suppressedRenderComponents.Clear();
        _renderComponentStates.Clear();
    }

    private void ReopenDecision(DecisionSO decision)
    {
        if (decision != null && DialogueUI.Instance != null)
        {
            DialogueUI.Instance.Open(decision);
        }
    }

    private void ClearPendingMinigame()
    {
        _minigameActive = false;
        _minigameScene = default;
        _returnScene = default;
        _pendingDecision = null;
        _pendingOption = null;
    }
}
