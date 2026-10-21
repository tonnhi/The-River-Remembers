using System;
using UnityEngine;

namespace ConNuoc.Data
{
    [Serializable]
    public class DecisionOption
    {
        [TextArea(2, 4)]
        public string optionText;

        [Tooltip("Tier this option requires to even be offered. Leave at Personal if no gate.")]
        public AgencyTier requiredTier = AgencyTier.Personal;

        [Tooltip("Immediate meter changes when this option is chosen.")]
        public MeterDelta[] immediateDeltas;

        [Tooltip("Optional — if this option unlocks a new tier, set it here. Capability, not XP.")]
        public bool unlocksTier;
        public AgencyTier tierUnlocked;

        [Tooltip("Optional — a story flag set when this option is chosen. Other decisions can require it (see DecisionSO.requiredFlag).")]
        public string setsFlag;

        [Tooltip("Optional — one delayed consequence this option queues (Consequence/Delay Engine).")]
        public bool hasDelayedConsequence;
        public DelayedConsequence delayedConsequence;
    }
}
