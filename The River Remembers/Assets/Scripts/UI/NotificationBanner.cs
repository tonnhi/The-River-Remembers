using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using ConNuoc.Core;

namespace ConNuoc.UI
{
    /// <summary>
    /// A single queued banner line at the top of the screen, used for two things:
    ///  - CapabilityManager.OnTierUnlocked  -> "New form of action unlocked: ..."
    ///  - ConsequenceEngine.OnConsequenceFired -> the mentor's delayed-consequence explanation
    /// Scene setup: a CanvasGroup with a TextMeshProUGUI child, start alpha 0.
    /// </summary>
    public class NotificationBanner : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TextMeshProUGUI bannerText;
        [SerializeField] private float secondsVisible = 3.5f;
        [SerializeField] private float fadeSeconds = 0.4f;

        private readonly Queue<string> _queue = new Queue<string>();
        private bool _showing;

        private void Start()
        {
            // Subscribing in Start (not OnEnable/Awake) sidesteps script-execution-order
            // issues: every object's Awake has already run by the time any Start runs,
            // so the singletons below are guaranteed to exist.
            if (CapabilityManager.Instance != null)
                CapabilityManager.Instance.OnTierUnlocked += HandleTierUnlocked;
            if (ConsequenceEngine.Instance != null)
                ConsequenceEngine.Instance.OnConsequenceFired += HandleConsequenceFired;

            if (canvasGroup != null) canvasGroup.alpha = 0f;
        }

        private void OnDisable()
        {
            if (CapabilityManager.Instance != null)
                CapabilityManager.Instance.OnTierUnlocked -= HandleTierUnlocked;
            if (ConsequenceEngine.Instance != null)
                ConsequenceEngine.Instance.OnConsequenceFired -= HandleConsequenceFired;
        }

        private void HandleTierUnlocked(ConNuoc.Data.AgencyTier tier, string text) => Enqueue(text);
        private void HandleConsequenceFired(string text) => Enqueue(text);

        private void Enqueue(string text)
        {
            _queue.Enqueue(text);
            if (!_showing) StartCoroutine(ShowNext());
        }

        private IEnumerator ShowNext()
        {
            _showing = true;
            while (_queue.Count > 0)
            {
                bannerText.text = _queue.Dequeue();
                yield return Fade(0f, 1f);
                yield return new WaitForSecondsRealtime(secondsVisible);
                yield return Fade(1f, 0f);
            }
            _showing = false;
        }

        private IEnumerator Fade(float from, float to)
        {
            float t = 0f;
            while (t < fadeSeconds)
            {
                t += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(from, to, t / fadeSeconds);
                yield return null;
            }
            canvasGroup.alpha = to;
        }
    }
}
