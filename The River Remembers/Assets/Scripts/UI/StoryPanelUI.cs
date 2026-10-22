using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace ConNuoc.UI
{
    [Serializable]
    public struct StoryPage
    {
        public string speaker;
        [TextArea(3, 8)] public string text;
    }

    public class StoryPanelUI : MonoBehaviour
    {
        public static StoryPanelUI Instance { get; private set; }

        [SerializeField] private GameObject panelRoot;
        [SerializeField] private TextMeshProUGUI speakerText;
        [SerializeField] private TextMeshProUGUI bodyText;
        [SerializeField] private Button nextButton;
        [SerializeField] private TextMeshProUGUI nextButtonLabel;

        [SerializeField] private string nextLabel = "Next";
        [SerializeField] private string lastPageLabel = "Begin";

        [SerializeField] private bool playIntroOnStart = true;
        [SerializeField]
        private StoryPage[] openingPages = new StoryPage[]
        {
            new StoryPage
            {
                speaker = "Narrator",
                text = "The River Remembers.\n\nIt's flood season in Xóm Sen, a stilt-house village on a Mekong Delta tributary. You are Mai, 13, and the water has always been part of your life."
            },
            new StoryPage
            {
                speaker = "Narrator",
                text = "This morning the road to school is under water again. Beside it, the drain is jammed with rubbish. It looks like a simple blockage, but you suspect it isn't."
            },
            new StoryPage
            {
                speaker = "Bà Sáu",
                text = "Clearing one drain won't change much, con. Talk to people: the farmers, the market, the commune. Each of them sees a different piece of it."
            },
            new StoryPage
            {
                speaker = "Bà Sáu",
                text = "What you can do alone is small. Together, and with the right people listening, it grows. Some of what you choose today will only show itself when the heavy rain comes."
            },
            new StoryPage
            {
                speaker = "How to play",
                text = "Move with WASD or the arrow keys.\nPress E near a villager to talk.\nWatch the three meters at the top: Ecology, Community and Resources."
            }
        };

        private StoryPage[] _pages;
        private int _index;
        private Action _onFinished;
        private string _lastLabelOverride;

        public bool IsOpen { get; private set; }

        private void Awake()
        {
            Instance = this;
            if (panelRoot != null) panelRoot.SetActive(false);
        }

        private void Start()
        {
            if (nextButton != null) nextButton.onClick.AddListener(Next);

            if (playIntroOnStart && openingPages != null && openingPages.Length > 0)
            {
                Show(openingPages);
            }
        }

        public void Show(StoryPage[] pages, Action onFinished = null, string lastLabelOverride = null)
        {
            _lastLabelOverride = lastLabelOverride;
            if (pages == null || pages.Length == 0)
            {
                onFinished?.Invoke();
                return;
            }

            _pages = pages;
            _index = 0;
            _onFinished = onFinished;
            IsOpen = true;

            if (panelRoot != null) panelRoot.SetActive(true);
            Time.timeScale = 0f; // freeze the world while the story plays
            Render();
        }

        private void Update()
        {
            if (!IsOpen) return;

            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                Next();
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                Close(); // skip the rest
            }
        }

        private void Next()
        {
            if (!IsOpen) return;

            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);

            _index++;
            if (_index >= _pages.Length)
            {
                Close();
                return;
            }
            Render();
        }

        private void Render()
        {
            var page = _pages[_index];
            if (speakerText != null) speakerText.text = page.speaker;
            if (bodyText != null) bodyText.text = page.text;
            if (nextButtonLabel != null)
            {
                string last = string.IsNullOrEmpty(_lastLabelOverride) ? lastPageLabel : _lastLabelOverride;
                nextButtonLabel.text = (_index >= _pages.Length - 1) ? last : nextLabel;
            }
        }

        private void Close()
        {
            IsOpen = false;
            if (panelRoot != null) panelRoot.SetActive(false);
            Time.timeScale = 1f;

            var callback = _onFinished;
            _onFinished = null;
            callback?.Invoke();
        }
    }
}
