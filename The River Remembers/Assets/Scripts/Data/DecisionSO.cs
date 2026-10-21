using System;
using UnityEngine;

namespace ConNuoc.Data
{
    /// <summary>An alternative prompt shown once a story flag has been set.</summary>
    [Serializable]
    public struct PromptVariant
    {
        public string requiredFlag;
        [TextArea(3, 6)] public string promptText;
    }

    /// <summary>
    /// One decision scenario — triggered by talking to an NPC at a DecisionHotspot.
    /// Create via Assets > Create > Con Nuoc > Decision.
    /// </summary>
    [CreateAssetMenu(fileName = "Decision_", menuName = "Con Nuoc/Decision", order = 1)]
    public class DecisionSO : ScriptableObject
    {
        [Header("Identity")]
        public string decisionId;
        public string npcName;

        [Header("Gate")]
        [Tooltip("Minimum tier the player must have unlocked for this scenario to trigger at all.")]
        public AgencyTier requiredTier = AgencyTier.Personal;

        [Tooltip("Optional — this decision is only offered once this story flag has been set. Leave empty for no flag gate.")]
        public string requiredFlag;

        [Tooltip("If true, this decision can only be offered once per playthrough.")]
        public bool oneTimeOnly = true;

        [Header("Prompt")]
        [TextArea(3, 6)]
        public string promptText;

        [Tooltip("Optional — alternative prompts shown when an earlier choice set a flag. The first variant whose flag is set replaces the default prompt above. This is how NPCs react to what you did elsewhere.")]
        public PromptVariant[] promptVariants;

        [Header("Options")]
        public DecisionOption[] options;
    }
}
