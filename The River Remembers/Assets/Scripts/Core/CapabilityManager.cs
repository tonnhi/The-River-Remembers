using System;
using System.Collections.Generic;
using UnityEngine;
using ConNuoc.Data;

namespace ConNuoc.Core
{
    public class CapabilityManager : MonoBehaviour
    {
        public static CapabilityManager Instance { get; private set; }

        private readonly HashSet<AgencyTier> _unlocked = new HashSet<AgencyTier> { AgencyTier.Personal };

        public event Action<AgencyTier, string> OnTierUnlocked;

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
