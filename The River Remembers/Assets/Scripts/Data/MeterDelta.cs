using System;
using UnityEngine;

namespace ConNuoc.Data
{
    [Serializable]
    public struct MeterDelta
    {
        public MeterType meter;
        [Range(-30, 30)] public int amount;
    }
}
