using System;
using System.Collections.Generic;
using UnityEngine;
using ConNuoc.Data;

namespace ConNuoc.Core
{
    public class MeterSystem : MonoBehaviour
    {
        public static MeterSystem Instance { get; private set; }

        [SerializeField] private int startingValue = 50;
        [SerializeField] private int minValue = 0;
        [SerializeField] private int maxValue = 100;

        private readonly Dictionary<MeterType, int> _values = new Dictionary<MeterType, int>();

        public event Action<MeterType, int, int> OnMeterChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            foreach (MeterType m in Enum.GetValues(typeof(MeterType)))
            {
                _values[m] = startingValue;
            }
        }

        public int GetValue(MeterType meter) => _values[meter];

        public void Apply(MeterDelta delta)
        {
            Apply(delta.meter, delta.amount);
        }

        public void Apply(MeterType meter, int amount)
        {
            int oldValue = _values[meter];
            int newValue = Mathf.Clamp(oldValue + amount, minValue, maxValue);
            _values[meter] = newValue;
            OnMeterChanged?.Invoke(meter, newValue, newValue - oldValue);
        }

        public void ApplyAll(MeterDelta[] deltas)
        {
            if (deltas == null) return;
            foreach (var d in deltas)
            {
                Apply(d);
            }
        }
    }
}
