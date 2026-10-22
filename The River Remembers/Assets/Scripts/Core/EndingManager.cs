using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using ConNuoc.Data;
using ConNuoc.NPC;
using ConNuoc.UI;

namespace ConNuoc.Core
{
    public class EndingManager : MonoBehaviour
    {
        [SerializeField] private float checkDelaySeconds = 2.5f;

        [SerializeField] private bool restartSceneAfterEnding = false;

        [TextArea(3, 8)]
        [SerializeField]
        private string personalEndingText =
            "You did what one person could: you looked, you listened, and you acted on what was in your hands. The road to school still floods, and the people who could change that haven't heard from you yet.\n\nWho benefited from what you chose? Who gave something up? Could another action have solved part of the problem?";

        [TextArea(3, 8)]
        [SerializeField]
        private string communityEndingText =
            "You listened, and people began to trust you. But the clean-up never happened, so the drain and the bank are much as you found them. Xóm Sen has started to talk about the water together; the next step is still waiting.\n\nWho benefited from what you chose? Could another action have solved part of the problem?";

        [TextArea(3, 8)]
        [SerializeField]
        private string institutionalEndingText =
            "You took what the village had learned to the commune office, and Xóm Sen was heard. Whether it ends as a lasting fix or a quick one depends on what you asked for.\n\nWho benefited? Who gave something up? What do you think caused the flooding in the first place?";

        [TextArea(3, 8)]
        [SerializeField]
        private string institutionalEvidenceEnding =
            "You took what the village had learned to the commune office: the drain, the reeds, the market and the neighbours behind you. The drain upgrade is on the commune's plan. It will take time, but this time Xóm Sen was heard.\n\nWho benefited from what you chose? Who gave something up?";

        [TextArea(3, 8)]
        [SerializeField]
        private string institutionalQuickFixEnding =
            "The commune is digging the drain deeper this month, and the water will clear faster for now. But nobody asked why the road was flooding in the first place, and the problem may return with the next heavy rain.\n\nCould another action have solved part of the problem?";

        [TextArea(2, 4)]
        [SerializeField]
        private string reedsKeptLine = "The strip of reeds you asked Chú Tư to keep held the bank when the heavy rain came.";

        [TextArea(2, 4)]
        [SerializeField]
        private string reedsClearedLine = "When the heavy rain came, there were no reeds left to slow the water, and it ran straight onto the road to school.";

        private bool _ended;
        private bool _checkQueued;

        private void Start()
        {
            if (DecisionResolver.Instance != null)
            {
                DecisionResolver.Instance.OnDecisionResolved += HandleResolved;
            }
        }

        private void OnDestroy()
        {
            if (DecisionResolver.Instance != null)
            {
                DecisionResolver.Instance.OnDecisionResolved -= HandleResolved;
            }
        }

        private void HandleResolved(DecisionSO decision, DecisionOption option)
        {
            QueueCheck();
        }

        private void QueueCheck()
        {
            if (_ended || _checkQueued) return;
            StartCoroutine(CheckAfterDelay());
        }

        private IEnumerator CheckAfterDelay()
        {
            _checkQueued = true;
            yield return new WaitForSecondsRealtime(checkDelaySeconds);
            _checkQueued = false;

            if (_ended) yield break;

            if (AnyoneHasSomethingToOffer()) yield break;

            var engine = ConsequenceEngine.Instance;
            if (engine != null && engine.PendingCount > 0 && SeasonManager.Instance != null)
            {
                SeasonManager.Instance.AdvanceSeason();
                QueueCheck();
                yield break;
            }

            ShowEnding();
        }

        private bool AnyoneHasSomethingToOffer()
        {
            foreach (var hotspot in DecisionHotspot.All)
            {
                if (hotspot != null && hotspot.HasOfferableDecision) return true;
            }
            return false;
        }

        private void ShowEnding()
        {
            _ended = true;

            if (StoryPanelUI.Instance == null)
            {
                Debug.LogWarning("EndingManager: no StoryPanelUI in the scene, so the ending can't be shown.");
                return;
            }

            var cap = CapabilityManager.Instance;
            var resolver = DecisionResolver.Instance;
            string body;
            if (cap != null && cap.IsUnlocked(AgencyTier.Institutional))
            {
                if (resolver != null && resolver.HasFlag("petition_evidence") && !string.IsNullOrEmpty(institutionalEvidenceEnding))
                    body = institutionalEvidenceEnding;
                else if (resolver != null && resolver.HasFlag("petition_quickfix") && !string.IsNullOrEmpty(institutionalQuickFixEnding))
                    body = institutionalQuickFixEnding;
                else
                    body = institutionalEndingText;
            }
            else if (cap != null && cap.IsUnlocked(AgencyTier.Community)) body = communityEndingText;
            else body = personalEndingText;

            if (resolver != null)
            {
                if (resolver.HasFlag("reeds_kept") && !string.IsNullOrEmpty(reedsKeptLine))
                    body = reedsKeptLine + "\n\n" + body;
                else if (resolver.HasFlag("reeds_cleared") && !string.IsNullOrEmpty(reedsClearedLine))
                    body = reedsClearedLine + "\n\n" + body;
            }

            int eco = GetMeter(MeterType.Ecology);
            int trust = GetMeter(MeterType.Trust);
            int res = GetMeter(MeterType.Resources);

            var pages = new StoryPage[]
            {
                new StoryPage { speaker = "The River Remembers", text = body },
                new StoryPage
                {
                    speaker = "Where the village stands",
                    text = "Ecology " + eco + "     Community " + trust + "     Resources " + res +
                           "\n\n" + BuildReflection(eco, trust, res) +
                           "\n\nThere's more than one way through this village. Play again and choose differently."
                }
            };

            StoryPanelUI.Instance.Show(pages, OnEndingDismissed, restartSceneAfterEnding ? "Play again" : "Close");
        }

        private static int GetMeter(MeterType meter)
        {
            return MeterSystem.Instance != null ? MeterSystem.Instance.GetValue(meter) : 50;
        }

        private static string BuildReflection(int eco, int trust, int res)
        {
            if (eco >= 60 && trust >= 60) return "The river and your neighbours both feel looked after.";
            if (trust < 40) return "Trust ran thin along the way, and people may take longer to work with you again.";
            if (eco < 40) return "The river is struggling. It remembers what it was asked to carry.";
            if (res < 30) return "The village is stretched thin. Caring for the river took more than it could spare.";
            return "Some things improved and some paid the price. That's what real trade-offs look like.";
        }

        private void OnEndingDismissed()
        {
            if (restartSceneAfterEnding)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }
    }
}
