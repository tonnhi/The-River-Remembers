using System.Collections.Generic;
using UnityEngine;
using ConNuoc.Core;
using ConNuoc.Data;
using ConNuoc.UI;

namespace ConNuoc.NPC
{
    /// <summary>
    /// Put this on an NPC GameObject with a Collider2D (Is Trigger = on).
    /// When the player is inside the trigger and presses the interact key, it opens
    /// DialogueUI with this NPC's DecisionSO — if DecisionResolver says it's currently offerable.
    ///
    /// Visual feedback (both optional, tick/untick in the Inspector):
    ///  - Tint:    NPC sprite is grey normally and turns red when you can talk to them.
    ///  - Outline: a red outline/halo appears around the NPC when you can talk to them.
    /// "Can talk" = player is in range AND the decision is currently offerable, so an NPC goes
    /// back to idle once their decision is used up or while they're still locked behind a tier.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class DecisionHotspot : MonoBehaviour
    {
        [SerializeField] private DecisionSO decision;
        [SerializeField] private KeyCode interactKey = KeyCode.E;
        [SerializeField] private GameObject promptIcon; // optional "!" icon, shown/hidden automatically

        [Header("Visual feedback")]
        [Tooltip("Leave empty to auto-find a SpriteRenderer on this object or its children.")]
        [SerializeField] private SpriteRenderer spriteRenderer;

        [SerializeField] private bool useTint = true;
        [SerializeField] private Color idleColor = new Color(0.55f, 0.55f, 0.55f, 1f);
        [SerializeField] private Color activeColor = new Color(1f, 0.25f, 0.25f, 1f);

        [SerializeField] private bool useOutline = true;
        [SerializeField] private Color outlineColor = new Color(1f, 0.2f, 0.2f, 1f);
        [Tooltip("How much bigger the outline sprite is than the NPC. 1.15 = a thin border.")]
        [SerializeField] private float outlineScale = 1.15f;

        /// <summary>Every active hotspot in the scene (used by EndingManager).</summary>
        public static readonly List<DecisionHotspot> All = new List<DecisionHotspot>();

        /// <summary>True if this NPC currently has something to offer, in range or not.</summary>
        public bool HasOfferableDecision =>
            DecisionResolver.Instance != null && DecisionResolver.Instance.CanOffer(decision);

        private bool _playerInRange;
        private GameObject _outline;

        private void OnEnable()
        {
            if (!All.Contains(this)) All.Add(this);
        }

        private void OnDisable()
        {
            All.Remove(this);
        }

        private void Reset()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void Awake()
        {
            if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            if (useOutline) BuildOutline();
            SetHighlighted(false);
        }

        // Builds a slightly larger copy of the sprite behind the NPC, coloured red.
        // No extra assets or shaders needed; works with any sprite.
        private void BuildOutline()
        {
            if (spriteRenderer == null || spriteRenderer.sprite == null) return;

            _outline = new GameObject("Outline");
            _outline.transform.SetParent(spriteRenderer.transform, false);
            _outline.transform.localScale = Vector3.one * outlineScale;

            var sr = _outline.AddComponent<SpriteRenderer>();
            sr.sprite = spriteRenderer.sprite;
            sr.color = outlineColor;
            sr.sortingLayerID = spriteRenderer.sortingLayerID;
            sr.sortingOrder = spriteRenderer.sortingOrder - 1; // behind the NPC
            _outline.SetActive(false);
        }

        private void Update()
        {
            bool offerable = DecisionResolver.Instance != null && DecisionResolver.Instance.CanOffer(decision);
            bool canTalkNow = _playerInRange && offerable;

            SetHighlighted(canTalkNow);
            if (promptIcon != null) promptIcon.SetActive(canTalkNow);

            if (canTalkNow && Input.GetKeyDown(interactKey))
            {
                DialogueUI.Instance.Open(decision);
            }
        }

        private void SetHighlighted(bool on)
        {
            if (useTint && spriteRenderer != null)
            {
                spriteRenderer.color = on ? activeColor : idleColor;
            }
            if (_outline != null && _outline.activeSelf != on)
            {
                _outline.SetActive(on);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player")) _playerInRange = true;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                _playerInRange = false;
                if (promptIcon != null) promptIcon.SetActive(false);
            }
        }
    }
}
