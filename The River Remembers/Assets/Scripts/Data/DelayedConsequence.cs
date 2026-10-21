using System;
using UnityEngine;

namespace ConNuoc.Data
{
    /// <summary>
    /// The Consequence/Delay Engine payload: effects queued now, applied N seasons later,
    /// with a mentor line explaining the mechanism when it lands. This is the one system
    /// the GDD calls out as unique to this design — keep it to the single sequence the
    /// prototype scope calls for (the dyke/shrimp-pond trade-off) unless you deliberately add more.
    /// </summary>
    [Serializable]
    public struct DelayedConsequence
    {
        [Tooltip("How many seasons from now this fires. 1 = next season.")]
        public int seasonsFromNow;

        [Tooltip("Mentor line shown when the consequence lands, e.g. Bà Sáu explaining why.")]
        [TextArea(2, 4)]
        public string explanationText;

        public MeterDelta[] deltas;

        [Tooltip("Optional — a story flag set when this consequence lands. Use it to open a follow-up conversation (e.g. a repair decision).")]
        public string setsFlag;
    }
}
