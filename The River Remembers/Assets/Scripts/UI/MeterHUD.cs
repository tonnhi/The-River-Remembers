using UnityEngine;
using UnityEngine.UI;
using ConNuoc.Core;
using ConNuoc.Data;

namespace ConNuoc.UI
{
    /// <summary>
    /// Always-visible systems-map HUD: three Sliders (0-100), one per meter, so the player
    /// can see trade-offs before confirming a choice, not just after (per the GDD).
    /// Scene setup: three UI Sliders in a Canvas, assign one per field here.
    /// </summary>
    public class MeterHUD : MonoBehaviour
    {
        [SerializeField] private Slider ecologySlider;
        [SerializeField] private Slider trustSlider;
        [SerializeField] private Slider resourcesSlider;

        private void Start()
        {
            // Subscribing in Start (not OnEnable/Awake) sidesteps script-execution-order
            // issues: every object's Awake has already run by the time any Start runs.
            if (MeterSystem.Instance != null)
            {
                MeterSystem.Instance.OnMeterChanged += HandleMeterChanged;
                RefreshAll();
            }
        }

        private void OnDisable()
        {
            if (MeterSystem.Instance != null)
            {
                MeterSystem.Instance.OnMeterChanged -= HandleMeterChanged;
            }
        }

        private void RefreshAll()
        {
            if (MeterSystem.Instance == null) return;
            SetSlider(ecologySlider, MeterSystem.Instance.GetValue(MeterType.Ecology));
            SetSlider(trustSlider, MeterSystem.Instance.GetValue(MeterType.Trust));
            SetSlider(resourcesSlider, MeterSystem.Instance.GetValue(MeterType.Resources));
        }

        private void HandleMeterChanged(MeterType meter, int newValue, int delta)
        {
            switch (meter)
            {
                case MeterType.Ecology: SetSlider(ecologySlider, newValue); break;
                case MeterType.Trust: SetSlider(trustSlider, newValue); break;
                case MeterType.Resources: SetSlider(resourcesSlider, newValue); break;
            }
        }

        private void SetSlider(Slider slider, int value)
        {
            if (slider == null) return;
            slider.minValue = 0;
            slider.maxValue = 100;
            slider.value = value;
        }
    }
}
