using UnityEngine;
using UnityEngine.UI;

namespace HorseRace
{
    public sealed class MainMenuUIView : MonoBehaviour
    {
        [Header("Pages")]
        [SerializeField] private RectTransform portraitRoot;
        [SerializeField] private RectTransform homePage;
        [SerializeField] private RectTransform placeholderPage;

        [Header("Header")]
        [SerializeField] private Button coinButton;
        [SerializeField] private Text coinText;

        [Header("Actions")]
        [SerializeField] private Button battleButton;
        [SerializeField] private Button[] tabButtons = new Button[5];
        [SerializeField] private Graphic[] tabBackgrounds = new Graphic[5];

        public RectTransform PortraitRoot => portraitRoot;
        public RectTransform HomePage => homePage;
        public RectTransform PlaceholderPage => placeholderPage;
        public Button CoinButton => coinButton;
        public Text CoinText => coinText;
        public Button BattleButton => battleButton;
        public Button[] TabButtons => tabButtons;
        public Graphic[] TabBackgrounds => tabBackgrounds;

        public bool IsConfigured =>
            portraitRoot != null && homePage != null && placeholderPage != null &&
            coinButton != null && coinText != null && battleButton != null &&
            tabButtons != null && tabButtons.Length == 5 &&
            tabBackgrounds != null && tabBackgrounds.Length == 5;

        public void Configure(
            RectTransform targetPortraitRoot,
            RectTransform targetHomePage,
            RectTransform targetPlaceholderPage,
            Button targetCoinButton,
            Text targetCoinText,
            Button targetBattleButton,
            Button[] targetTabButtons,
            Graphic[] targetTabBackgrounds)
        {
            portraitRoot = targetPortraitRoot;
            homePage = targetHomePage;
            placeholderPage = targetPlaceholderPage;
            coinButton = targetCoinButton;
            coinText = targetCoinText;
            battleButton = targetBattleButton;
            tabButtons = targetTabButtons;
            tabBackgrounds = targetTabBackgrounds;
        }
    }
}
