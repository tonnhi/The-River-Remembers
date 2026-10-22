using System.Collections;
using TMPro;
using UnityEngine;

public class RubbishCleanupMinigame : Minigame
{
    [SerializeField] private float durationSeconds = 10f;
    [SerializeField] private int targetCount = 10;
    [SerializeField] private float resultDisplaySeconds = 1.25f;
    [SerializeField] private LayerMask rubbishLayers = ~0;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI collectedText;
    [SerializeField] private TextMeshProUGUI resultText;

    private float _timeRemaining;
    private int _collectedCount;
    private bool _finished;

    protected override void BeginMinigame()
    {
        float timeBonus = MinigameManager.Instance != null
            ? MinigameManager.Instance.CurrentTimeBonusSeconds
            : 0f;
        _timeRemaining = durationSeconds + timeBonus;
        UpdateLabels();
        if (resultText != null) resultText.text = string.Empty;
    }

    private void Update()
    {
        if (_finished) return;

        _timeRemaining = Mathf.Max(0f, _timeRemaining - Time.deltaTime);
        UpdateLabels();

        if (_timeRemaining <= 0f)
        {
            Finish(false);
            return;
        }

        if (Input.GetMouseButtonDown(0)) TryCollectAtPointer();
    }

    private void TryCollectAtPointer()
    {
        Camera gameCamera = Camera.main;
        if (gameCamera == null) return;

        Vector3 pointerPosition = gameCamera.ScreenToWorldPoint(Input.mousePosition);
        Collider2D hit = Physics2D.OverlapPoint(pointerPosition, rubbishLayers);
        if (hit == null) return;

        RubbishTarget target = hit.GetComponentInParent<RubbishTarget>();
        if (target == null || !target.Collect()) return;

        _collectedCount++;
        UpdateLabels();
        if (_collectedCount >= targetCount) Finish(true);
    }

    private void Finish(bool succeeded)
    {
        if (_finished) return;

        _finished = true;
        if (resultText != null)
        {
            resultText.text = succeeded ? "Cleanup complete!" : "Time's up!";
        }

        StartCoroutine(ReturnAfterResult());
    }

    private IEnumerator ReturnAfterResult()
    {
        yield return new WaitForSeconds(resultDisplaySeconds);
        EndMinigame(_collectedCount >= targetCount);
    }

    private void UpdateLabels()
    {
        if (timerText != null) timerText.text = $"Time: {_timeRemaining:0.0}";
        if (collectedText != null) collectedText.text = $"Rubbish: {_collectedCount}/{targetCount}";
    }
}