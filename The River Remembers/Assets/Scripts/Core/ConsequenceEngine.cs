using System;
using System.Collections.Generic;
using UnityEngine;
using ConNuoc.Data;

namespace ConNuoc.Core
{
    /// <summary>
    /// The Consequence/Delay Engine. A decision option queues a DelayedConsequence; this
    /// fires it however many seasons later it specifies, applying its meter deltas and
    /// surfacing the mentor explanation text via OnConsequenceFired.
    /// Keep this to the single sequence the prototype scope calls for (one dyke/shrimp-pond
    /// style trade-off) — more than one in flight is fine for the system to handle, but the
    /// scope note on the Gameplay slide says "1 delayed-consequence sequence" for the demo.
    /// </summary>
    public class ConsequenceEngine : MonoBehaviour
    {
        public static ConsequenceEngine Instance { get; private set; }

        private class PendingConsequence
        {
            public int fireOnSeason;
            public DelayedConsequence consequence;
        }

        private readonly List<PendingConsequence> _pending = new List<PendingConsequence>();

        /// <summary>How many consequences are queued but haven't landed yet.</summary>
        public int PendingCount => _pending.Count;

        /// <summary>Fired when a queued consequence lands: (explanationText).</summary>
        public event Action<string> OnConsequenceFired;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        /// <summary>Call when a decision option with hasDelayedConsequence is chosen.</summary>
        public void Queue(DelayedConsequence consequence, int currentSeason)
        {
            _pending.Add(new PendingConsequence
            {
                fireOnSeason = currentSeason + Mathf.Max(1, consequence.seasonsFromNow),
                consequence = consequence
            });
        }

        /// <summary>Called by SeasonManager after it advances to newSeason.</summary>
        public void Tick(int newSeason)
        {
            for (int i = _pending.Count - 1; i >= 0; i--)
            {
                if (_pending[i].fireOnSeason != newSeason) continue;

                var c = _pending[i].consequence;
                if (MeterSystem.Instance != null)
                {
                    MeterSystem.Instance.ApplyAll(c.deltas);
                }
                if (!string.IsNullOrEmpty(c.setsFlag) && DecisionResolver.Instance != null)
                {
                    DecisionResolver.Instance.SetFlag(c.setsFlag);
                }
                OnConsequenceFired?.Invoke(c.explanationText);
                _pending.RemoveAt(i);
            }
        }
    }
}
