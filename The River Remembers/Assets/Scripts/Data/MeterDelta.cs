using System;
using UnityEngine;

namespace ConNuoc.Data
{
    /// <summary>
    /// A single meter change. A decision option normally carries at least two of these,
    /// per the design rule: "every choice moves at least two of the three meters."
    /// </summary>
    [Serializable]
    public struct MeterDelta
    {
        public MeterType meter;
        [Range(-30, 30)] public int amount;
    }
}
