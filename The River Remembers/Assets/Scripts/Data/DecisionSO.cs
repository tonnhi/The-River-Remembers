using System;
using UnityEngine;

namespace ConNuoc.Data
{
    [Serializable]
    public struct PromptVariant
    {
        public string requiredFlag;
        [TextArea(3, 6)] public string promptText;
    }

    [CreateAssetMenu(fileName = "Decision_", menuName = "Con Nuoc/Decision", order = 1)]
    public class DecisionSO : ScriptableObject
    {
        public string decisionId;
        public string npcName;

        public AgencyTier requiredTier = AgencyTier.Personal;

        public string requiredFlag;

        public bool oneTimeOnly = true;

        [TextArea(3, 6)]
        public string promptText;

        public PromptVariant[] promptVariants;

        public DecisionOption[] options;
    }
}
