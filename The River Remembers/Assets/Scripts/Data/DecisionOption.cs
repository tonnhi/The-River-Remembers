using System;
using UnityEngine;

namespace ConNuoc.Data
{
    [Serializable]
    public class DecisionOption
    {
        [TextArea(2, 4)]
        public string optionText;

        public AgencyTier requiredTier = AgencyTier.Personal;

        public MeterDelta[] immediateDeltas;

        public bool unlocksTier;
        public AgencyTier tierUnlocked;

        public string setsFlag;

        public string minigameSceneName;

        public float minigameTimeBonusSeconds;

        public bool hasDelayedConsequence;
        public DelayedConsequence delayedConsequence;
    }
}
