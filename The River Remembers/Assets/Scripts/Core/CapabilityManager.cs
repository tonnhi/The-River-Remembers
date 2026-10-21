using System;
using System.Collections.Generic;
using UnityEngine;
using ConNuoc.Data;

namespace ConNuoc.Core
{
    /// <summary>
    /// Tracks which Agency Ladder tiers the player has unlocked. This is deliberately NOT a
    /// score — a tier is either unlocked or it isn't, and the only UI feedback is a one-line
    /// "new form of action unlocked" toast (see UI/CapabilityToast.cs). See GDD Formal Elements
    /// and the Gameplay slide's capability-vs-XP framing.
    /// </summary>
    public class CapabilityManager : MonoBehaviour
    {
        public static CapabilityManager Instance { get; private set; }

        private readonly HashSet<AgencyTier> _unlocked = new HashSet<AgencyTier> { AgencyTier.Personal };

        /// <summary>Fired when a new tier unlocks: (tier, flavourText).</summary>
        public event Action<AgencyTier, string> OnTierUnlocked;

        [Header("Toast text shown when a tier unlocks (edit to taste)")]
        [SerializeField]
        private string communityUnlockText = "New form of action unlocked: Community organising";
        [SerializeField]
        private string institutionalUnlockText = "New form of action unlocked: Petitioning the district";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public bool IsUnlocked(AgencyTier tier) => _unlocked.Contains(tier);

        public void Unlock(AgencyTier tier)
        {
            if (_unlocked.Contains(tier)) return;
            _unlocked.Add(tier);

            string text = tier switch
            {
                AgencyTier.Community => communityUnlockText,
                AgencyTier.Institutional => institutionalUnlockText,
                _ => $"New form of action unlocked: {tier}"
            };

            OnTierUnlocked?.Invoke(tier, text);
        }
    }
}
