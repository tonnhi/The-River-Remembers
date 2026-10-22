using System;
using System.Collections.Generic;
using UnityEngine;
using ConNuoc.Data;

namespace ConNuoc.Core
{
    public class ConsequenceEngine : MonoBehaviour
    {
        public static ConsequenceEngine Instance { get; private set; }

        private class PendingConsequence
        {
            public int fireOnSeason;
            public DelayedConsequence consequence;
        }

        private readonly List<PendingConsequence> _pending = new List<PendingConsequence>();

        public int PendingCount => _pending.Count;

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

        public void Queue(DelayedConsequence consequence, int currentSeason)
        {
            _pending.Add(new PendingConsequence
            {
                fireOnSeason = currentSeason + Mathf.Max(1, consequence.seasonsFromNow),
                consequence = consequence
            });
        }

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
