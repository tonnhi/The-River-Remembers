using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ConNuoc.Core;
using ConNuoc.Data;

namespace ConNuoc.UI
{
    /// <summary>
    /// The dialogue/choice panel. Opens with a DecisionSO, lists only the options whose
    /// requiredTier is currently unlocked, and resolves whichever one the player clicks
    /// through DecisionResolver.
    ///
    /// Scene setup: put this on a root Canvas panel (start inactive). Assign:
    ///  - npcNameText, promptText  (TextMeshProUGUI)
    ///  - optionButtonPrefab       (a Button with a child TextMeshProUGUI, as its own prefab)
    ///  - optionsContainer         (a vertical layout group the buttons spawn into)
    /// </summary>
    public class DialogueUI : MonoBehaviour
    {
        public static DialogueUI Instance { get; private set; }

        [Header("Refs")]
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
                // Nothing selectable at the player's current capability: don't open (or pause
                // for) a dialogue the player could never close.
                Debug.LogWarning("DialogueUI: no options available for " + decision.decisionId);
                _current = null;
                return;
            }

            if (panelRoot != null) panelRoot.SetActive(true);
            Time.timeScale = 0f; // pause movement while dialogue is open; remove if undesired
        }

        // Picks the first prompt variant whose flag has been set, so NPCs can react to
        // earlier choices. Falls back to the decision's default prompt.
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
                // Always close and unpause, even if something in Resolve threw an error,
                // so a bug can never leave the game frozen on a paused screen.
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
