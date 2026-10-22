using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class BinPlacementMinigame : Minigame
{
    [Serializable]
    private class Bin
    {
        public string shapeId;
        public Transform binTransform;
        public Collider2D dragCollider;
    }

    [Serializable]
    private class BinSpot
    {
        public string shapeId;
        public Transform snapPoint;
        public Collider2D spotCollider;
    }

    [SerializeField] private Bin[] bins = new Bin[3];
    [SerializeField] private BinSpot[] spots = new BinSpot[3];
    [SerializeField] private float durationSeconds = 10f;
    [SerializeField] private float resultDisplaySeconds = 1.25f;
    [SerializeField] private LayerMask interactionLayers = ~0;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI progressText;

    private Vector3[] _startingPositions;
    private bool[] _placed;
    private int _placedCount;
    private float _timeRemaining;
    private int _draggingBinIndex = -1;
    private Vector3 _dragOffset;
    private Camera _gameCamera;
    private bool _finished;

    protected override void BeginMinigame()
    {
        _gameCamera = Camera.main;

        if (!ValidateSetup())
        {
            if (statusText != null) statusText.text = "Bin placement is not configured.";
            return;
        }

        _startingPositions = new Vector3[bins.Length];
        _placed = new bool[bins.Length];
        float timeBonus = MinigameManager.Instance != null
            ? MinigameManager.Instance.CurrentTimeBonusSeconds
            : 0f;
        _timeRemaining = durationSeconds + timeBonus;
        for (int index = 0; index < bins.Length; index++)
        {
            _startingPositions[index] = bins[index].binTransform.position;
        }

        if (statusText != null) statusText.text = "Drag each bin to its matching spot.";
        UpdateLabels();
    }

    private void Update()
    {
        if (_finished || _startingPositions == null) return;

        _timeRemaining = Mathf.Max(0f, _timeRemaining - Time.deltaTime);
        UpdateLabels();
        if (_timeRemaining <= 0f)
        {
            Finish(false);
            return;
        }

        if (Input.GetMouseButtonDown(0)) BeginDrag();
        if (_draggingBinIndex >= 0 && Input.GetMouseButton(0)) UpdateDrag();
        if (_draggingBinIndex >= 0 && Input.GetMouseButtonUp(0)) EndDrag();
    }

    private bool ValidateSetup()
    {
        if (_gameCamera == null)
        {
            Debug.LogError("BinPlacementMinigame: the scene needs an enabled camera tagged MainCamera.", this);
            return false;
        }

        if (bins == null || spots == null || bins.Length != 3 || spots.Length != 3)
        {
            Debug.LogError("BinPlacementMinigame: assign exactly three bins and three spots.", this);
            return false;
        }

        foreach (Bin bin in bins)
        {
            if (bin == null || string.IsNullOrWhiteSpace(bin.shapeId) ||
                bin.binTransform == null || bin.dragCollider == null)
            {
                Debug.LogError("BinPlacementMinigame: each bin needs a shape ID, transform, and collider.", this);
                return false;
            }
        }

        foreach (BinSpot spot in spots)
        {
            if (spot == null || string.IsNullOrWhiteSpace(spot.shapeId) ||
                spot.snapPoint == null || spot.spotCollider == null)
            {
                Debug.LogError("BinPlacementMinigame: each spot needs a shape ID, snap point, and collider.", this);
                return false;
            }
        }

        for (int binIndex = 0; binIndex < bins.Length; binIndex++)
        {
            for (int otherIndex = binIndex + 1; otherIndex < bins.Length; otherIndex++)
            {
                if (bins[binIndex].shapeId == bins[otherIndex].shapeId ||
                    spots[binIndex].shapeId == spots[otherIndex].shapeId)
                {
                    Debug.LogError("BinPlacementMinigame: every bin and spot shape ID must be unique.", this);
                    return false;
                }
            }
        }

        foreach (Bin bin in bins)
        {
            bool hasMatchingSpot = false;
            foreach (BinSpot spot in spots)
            {
                if (bin.shapeId == spot.shapeId)
                {
                    hasMatchingSpot = true;
                    break;
                }
            }

            if (!hasMatchingSpot)
            {
                Debug.LogError("BinPlacementMinigame: every bin needs one spot with the same shape ID.", this);
                return false;
            }
        }

        return true;
    }

    private void BeginDrag()
    {
        Vector2 pointerPosition = GetPointerWorldPosition();
        Collider2D[] hits = Physics2D.OverlapPointAll(pointerPosition, interactionLayers);

        foreach (Collider2D hit in hits)
        {
            int binIndex = FindBinIndex(hit);
            if (binIndex < 0 || _placed[binIndex]) continue;

            _draggingBinIndex = binIndex;
            _dragOffset = bins[binIndex].binTransform.position - (Vector3)pointerPosition;
            return;
        }
    }

    private void UpdateDrag()
    {
        Vector2 pointerPosition = GetPointerWorldPosition();
        Transform binTransform = bins[_draggingBinIndex].binTransform;
        Vector3 position = (Vector3)pointerPosition + _dragOffset;
        position.z = _startingPositions[_draggingBinIndex].z;
        binTransform.position = position;
    }

    private void EndDrag()
    {
        int binIndex = _draggingBinIndex;
        _draggingBinIndex = -1;

        Vector2 pointerPosition = GetPointerWorldPosition();
        Collider2D[] hits = Physics2D.OverlapPointAll(pointerPosition, interactionLayers);
        BinSpot matchingSpot = FindSpotForBin(binIndex, hits);

        if (matchingSpot == null)
        {
            bins[binIndex].binTransform.position = _startingPositions[binIndex];
            if (statusText != null) statusText.text = "That spot does not match. Try another one.";
            return;
        }

        bins[binIndex].binTransform.SetPositionAndRotation(
            matchingSpot.snapPoint.position,
            matchingSpot.snapPoint.rotation);
        _placed[binIndex] = true;
        _placedCount++;
        UpdateLabels();

        if (_placedCount == bins.Length)
        {
            Finish(true);
        }
        else if (statusText != null)
        {
            statusText.text = "Good placement! Match the remaining bins.";
        }
    }

    private void Finish(bool succeeded)
    {
        if (_finished) return;

        _finished = true;
        if (statusText != null)
        {
            statusText.text = succeeded ? "All bins are in place!" : "Time's up! Try again.";
        }

        StartCoroutine(ReturnAfterResult(succeeded));
    }

    private IEnumerator ReturnAfterResult(bool succeeded)
    {
        yield return new WaitForSeconds(resultDisplaySeconds);
        EndMinigame(succeeded);
    }

    private void UpdateLabels()
    {
        if (timerText != null) timerText.text = $"Time: {_timeRemaining:0.0}";
        if (progressText != null) progressText.text = $"Bins: {_placedCount}/{bins.Length}";
    }

    private int FindBinIndex(Collider2D hit)
    {
        for (int index = 0; index < bins.Length; index++)
        {
            if (hit == bins[index].dragCollider ||
                hit.transform == bins[index].binTransform ||
                hit.transform.IsChildOf(bins[index].binTransform))
            {
                return index;
            }
        }

        return -1;
    }

    private BinSpot FindSpotForBin(int binIndex, Collider2D[] hits)
    {
        foreach (Collider2D hit in hits)
        {
            foreach (BinSpot spot in spots)
            {
                if ((hit == spot.spotCollider || hit.transform.IsChildOf(spot.snapPoint)) &&
                    bins[binIndex].shapeId == spot.shapeId)
                {
                    return spot;
                }
            }
        }

        return null;
    }

    private Vector2 GetPointerWorldPosition()
    {
        Vector3 pointerPosition = _gameCamera.ScreenToWorldPoint(Input.mousePosition);
        return new Vector2(pointerPosition.x, pointerPosition.y);
    }
}