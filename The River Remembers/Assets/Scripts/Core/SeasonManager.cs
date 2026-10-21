using System;
using UnityEngine;

namespace ConNuoc.Core
{
    /// <summary>
    /// Owns the season counter. For the prototype, call AdvanceSeason() from a debug key or
    /// a "sleep" interaction once you have a day/night loop — the scope note just needs "1
    /// flood season" to visibly progress, it doesn't need a full calendar.
    /// </summary>
    public class SeasonManager : MonoBehaviour
    {
        public static SeasonManager Instance { get; private set; }

        [SerializeField] private int currentSeason = 0;
        public int CurrentSeason => currentSeason;

        /// <summary>Fired after the season number increments.</summary>
        public event Action<int> OnSeasonAdvanced;

        [Header("Prototype debug")]
        [SerializeField] private bool allowDebugKey = true;
        [SerializeField] private KeyCode debugAdvanceKey = KeyCode.F5;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Update()
        {
            if (allowDebugKey && Input.GetKeyDown(debugAdvanceKey))
            {
                AdvanceSeason();
            }
        }

        public void AdvanceSeason()
        {
            currentSeason++;
            OnSeasonAdvanced?.Invoke(currentSeason);

            if (ConsequenceEngine.Instance != null)
            {
                ConsequenceEngine.Instance.Tick(currentSeason);
            }
        }
    }
}
