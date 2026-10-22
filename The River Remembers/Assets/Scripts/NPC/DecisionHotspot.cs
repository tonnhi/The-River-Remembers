using System.Collections.Generic;
using UnityEngine;
using ConNuoc.Core;
using ConNuoc.Data;
using ConNuoc.UI;

namespace ConNuoc.NPC
{
    [RequireComponent(typeof(Collider2D))]
    public class DecisionHotspot : MonoBehaviour
    {
        [SerializeField] private DecisionSO decision;
        [SerializeField] private KeyCode interactKey = KeyCode.E;
        [SerializeField] private GameObject promptIcon; // optional "!" icon, shown/hidden automatically

        [SerializeField] private SpriteRenderer spriteRenderer;

        [SerializeField] private bool useTint = true;
        [SerializeField] private Color idleColor = new Color(0.55f, 0.55f, 0.55f, 1f);
        [SerializeField] private Color activeColor = new Color(1f, 0.25f, 0.25f, 1f);
        [SerializeField] private Color completedColor = new Color(0.2f, 0.8f, 0.3f, 1f);

        [SerializeField] private bool useOutline = true;
        [SerializeField] private Color outlineColor = new Color(1f, 0.2f, 0.2f, 1f);
        [SerializeField] private float outlineScale = 1.15f;

        public static readonly List<DecisionHotspot> All = new List<DecisionHotspot>();

        public bool HasOfferableDecision =>
            DecisionResolver.Instance != null && DecisionResolver.Instance.CanOffer(decision);

        public DecisionSO Decision => decision;

        private bool _playerInRange;
        private GameObject _outline;
        private SpriteRenderer _outlineRenderer;

        private void OnEnable()
        {
            if (!All.Contains(this)) All.Add(this);
            if (DecisionResolver.Instance != null) DecisionResolver.Instance.RegisterTask(decision);
        }

        private void OnDisable()
        {
            All.Remove(this);
            if (DecisionResolver.Instance != null && !All.Exists(hotspot => hotspot.Decision == decision))
            {
                DecisionResolver.Instance.UnregisterTask(decision);
            }
        }

        private void Reset()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void Awake()
        {
            if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            if (useOutline) BuildOutline();
            SetHighlightState(false, false);
        }

        private void BuildOutline()
        {
            if (spriteRenderer == null || spriteRenderer.sprite == null) return;

            _outline = new GameObject("Outline");
            _outline.transform.SetParent(spriteRenderer.transform, false);
            _outline.transform.localScale = Vector3.one * outlineScale;

            _outlineRenderer = _outline.AddComponent<SpriteRenderer>();
            _outlineRenderer.sprite = spriteRenderer.sprite;
            _outlineRenderer.color = outlineColor;
            _outlineRenderer.sortingLayerID = spriteRenderer.sortingLayerID;
            _outlineRenderer.sortingOrder = spriteRenderer.sortingOrder - 1; // behind the NPC
            _outline.SetActive(false);
        }

        private void Update()
        {
            DecisionResolver resolver = DecisionResolver.Instance;
            bool offerable = resolver != null && resolver.CanOffer(decision);
            bool completed = resolver != null && resolver.IsCompleted(decision);
            bool canTalkNow = _playerInRange && offerable;

            SetHighlightState(offerable, completed);
            if (promptIcon != null) promptIcon.SetActive(canTalkNow);

            if (canTalkNow && Input.GetKeyDown(interactKey) && DialogueUI.Instance != null)
            {
                DialogueUI.Instance.Open(decision);
            }
        }

        private void SetHighlightState(bool offerable, bool completed)
        {
            Color highlightColor = completed ? completedColor : offerable ? activeColor : idleColor;

            if (useTint && spriteRenderer != null)
            {
                spriteRenderer.color = highlightColor;
            }

            bool showOutline = offerable || completed;
            if (_outline != null && _outline.activeSelf != showOutline)
            {
                _outline.SetActive(showOutline);
            }
            if (_outlineRenderer != null) _outlineRenderer.color = completed ? completedColor : outlineColor;
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
