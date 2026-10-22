using System;
using UnityEngine;

namespace ConNuoc.Data
{
    [Serializable]
    public struct DelayedConsequence
    {
        public int seasonsFromNow;

        [TextArea(2, 4)]
        public string explanationText;

        public MeterDelta[] deltas;
        public string setsFlag;
    }
}
