using System;
using System.Collections.Generic;
using UnityEngine;
using ConNuoc.Data;
using ConNuoc.NPC;

namespace ConNuoc.Core
{
    public class DecisionResolver : MonoBehaviour
    {
        public static DecisionResolver Instance { get; private set; }

        [SerializeField] private bool advanceSeasonAfterDecision = true;

        private readonly HashSet<string> _completedDecisionIds = new HashSet<string>();
        private readonly HashSet<string> _flags = new HashSet<string>();
        private readonly List<DecisionSO> _taskQueue = new List<DecisionSO>();

        public IReadOnlyList<DecisionSO> TaskQueue => _taskQueue;

        public event Action<DecisionSO, DecisionOption> OnDecisionResolved;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            foreach (DecisionHotspot hotspot in DecisionHotspot.All)
            {
                if (hotspot != null) RegisterTask(hotspot.Decision);
            }
        }

        public void RegisterTask(DecisionSO decision)
        {
            if (decision != null && !_taskQueue.Contains(decision))
            {
                _taskQueue.Add(decision);
            }
        }

        public void UnregisterTask(DecisionSO decision)
        {
            _taskQueue.Remove(decision);
        }

        public bool IsCompleted(DecisionSO decision) =>
            decision != null && _completedDecisionIds.Contains(decision.decisionId);

        public bool HasFlag(string flag) => !string.IsNullOrEmpty(flag) && _flags.Contains(flag);

        public void SetFlag(string flag)
        {
            if (!string.IsNullOrEmpty(flag)) _flags.Add(flag);
        }

        public bool CanOffer(DecisionSO decision)
        {
            if (decision == null) return false;
            if (_taskQueue.Count > 0 && !_taskQueue.Contains(decision)) return false;
            if (decision.oneTimeOnly && IsCompleted(decision)) return false;
            if (CapabilityManager.Instance != null && !CapabilityManager.Instance.IsUnlocked(decision.requiredTier))
                return false;
            if (!string.IsNullOrEmpty(decision.requiredFlag) && !_flags.Contains(decision.requiredFlag))
                return false;
            return true;
        }

        public void Resolve(DecisionSO decision, DecisionOption option)
        {
            if (!string.IsNullOrWhiteSpace(option.minigameSceneName))
            {
                if (MinigameManager.Instance == null)
                {
                    Debug.LogError("DecisionResolver: cannot start minigame scene '" +
                                   option.minigameSceneName + "' because MinigameManager is missing.");
                    return;
                }

                MinigameManager.Instance.StartMinigame(decision, option);
                return;
            }

            ApplyOption(decision, option);
        }

        public void ResolveCompletedMinigame(DecisionSO decision, DecisionOption option)
        {
            ApplyOption(decision, option);
        }

        private void ApplyOption(DecisionSO decision, DecisionOption option)
        {
            if (MeterSystem.Instance != null)
            {
                MeterSystem.Instance.ApplyAll(option.immediateDeltas);
            }

            if (option.unlocksTier && CapabilityManager.Instance != null)
            {
                CapabilityManager.Instance.Unlock(option.tierUnlocked);
            }

            SetFlag(option.setsFlag);

            if (option.hasDelayedConsequence && ConsequenceEngine.Instance != null && SeasonManager.Instance != null)
            {
                ConsequenceEngine.Instance.Queue(option.delayedConsequence, SeasonManager.Instance.CurrentSeason);
            }

            if (decision.oneTimeOnly)
            {
                _completedDecisionIds.Add(decision.decisionId);
            }

            if (advanceSeasonAfterDecision && SeasonManager.Instance != null)
            {
                SeasonManager.Instance.AdvanceSeason();
            }

            OnDecisionResolved?.Invoke(decision, option);
        }
    }
}
