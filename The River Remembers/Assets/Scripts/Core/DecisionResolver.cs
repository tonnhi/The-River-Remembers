using System;
using System.Collections.Generic;
using UnityEngine;
using ConNuoc.Data;

namespace ConNuoc.Core
{
    /// <summary>
    /// Applies the effects of a chosen DecisionOption: immediate meter deltas, tier unlocks,
    /// story flags, and queuing any delayed consequence. Also tracks which one-time decisions
    /// have fired, and which story flags are set (flags are how one choice opens or closes
    /// later conversations, so there is more than one path through the game).
    /// </summary>
    public class DecisionResolver : MonoBehaviour
    {
        public static DecisionResolver Instance { get; private set; }

        [Header("Pacing")]
        [Tooltip("If ticked, one season passes after every decision you resolve, so delayed " +
                 "consequences land as you play without needing the F5 debug key.")]
        [SerializeField] private bool advanceSeasonAfterDecision = true;

        private readonly HashSet<string> _completedDecisionIds = new HashSet<string>();
        private readonly HashSet<string> _flags = new HashSet<string>();

        /// <summary>Fired after a choice has been fully applied: (decision, chosen option).</summary>
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

        public bool IsCompleted(DecisionSO decision) => _completedDecisionIds.Contains(decision.decisionId);

        public bool HasFlag(string flag) => !string.IsNullOrEmpty(flag) && _flags.Contains(flag);

        public void SetFlag(string flag)
        {
            if (!string.IsNullOrEmpty(flag)) _flags.Add(flag);
        }

        public bool CanOffer(DecisionSO decision)
        {
            if (decision == null) return false;
            if (decision.oneTimeOnly && IsCompleted(decision)) return false;
            if (CapabilityManager.Instance != null && !CapabilityManager.Instance.IsUnlocked(decision.requiredTier))
                return false;
            if (!string.IsNullOrEmpty(decision.requiredFlag) && !_flags.Contains(decision.requiredFlag))
                return false;
            return true;
        }

        public void Resolve(DecisionSO decision, DecisionOption option)
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
