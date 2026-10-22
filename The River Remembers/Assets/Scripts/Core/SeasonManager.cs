using System;
using UnityEngine;

namespace ConNuoc.Core
{
    public class SeasonManager : MonoBehaviour
    {
        public static SeasonManager Instance { get; private set; }

        [SerializeField] private int currentSeason = 0;
        public int CurrentSeason => currentSeason;

        public event Action<int> OnSeasonAdvanced;

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
