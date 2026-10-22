using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ConNuoc.Core;
using ConNuoc.Data;

namespace ConNuoc.UI
{
    public class DialogueUI : MonoBehaviour
    {
        public static DialogueUI Instance { get; private set; }

        [SerializeField] private GameObject panelRoot;
        [SerializeField] private TextMeshProUGUI npcNameText;
        [SerializeField] private TextMeshProUGUI promptText;
        [SerializeField] private Button optionButtonPrefab;
        [SerializeField] private Transform optionsContainer;

        private readonly List<GameObject> _spawnedButtons = new List<GameObject>();
        private DecisionSO _current;

        private void Awake()
        {
            Instance = this;
            if (panelRoot != null) panelRoot.SetActive(false);
        }

        public void Open(DecisionSO decision)
        {
            _current = decision;
            npcNameText.text = decision.npcName;
            promptText.text = ResolvePrompt(decision);

            ClearButtons();

            foreach (var option in decision.options)
            {
                if (CapabilityManager.Instance != null && !CapabilityManager.Instance.IsUnlocked(option.requiredTier))
                    continue; // don't even show options above the player's current capability

                var btn = Instantiate(optionButtonPrefab, optionsContainer);
                var label = btn.GetComponentInChildren<TextMeshProUGUI>();
                if (label != null) label.text = option.optionText;

                var capturedOption = option; // local copy for the closure
                btn.onClick.AddListener(() => Choose(capturedOption));

                _spawnedButtons.Add(btn.gameObject);
            }

            if (_spawnedButtons.Count == 0)
            {
                Debug.LogWarning("DialogueUI: no options available for " + decision.decisionId);
                _current = null;
                return;
            }

            if (panelRoot != null) panelRoot.SetActive(true);
            Time.timeScale = 0f; // pause movement while dialogue is open; remove if undesired
        }

        private static string ResolvePrompt(DecisionSO decision)
        {
            if (decision.promptVariants != null && DecisionResolver.Instance != null)
            {
                foreach (var variant in decision.promptVariants)
                {
                    if (DecisionResolver.Instance.HasFlag(variant.requiredFlag))
                    {
                        return variant.promptText;
                    }
                }
            }
            return decision.promptText;
        }

        private void Choose(DecisionOption option)
        {
            var decision = _current;
            try
            {
                DecisionResolver.Instance.Resolve(decision, option);
            }
            finally
            {
                Close();
            }
        }

        private void Close()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
            Time.timeScale = 1f;
            ClearButtons();
            _current = null;
        }

        private void ClearButtons()
        {
            foreach (var go in _spawnedButtons) Destroy(go);
            _spawnedButtons.Clear();
        }
    }
}
