using UnityEngine;

namespace HorseRace
{
    public sealed class GameUIRoot : MonoBehaviour
    {
        [SerializeField] private Canvas canvas;
        [SerializeField] private RectTransform screenRoot;
        [SerializeField] private RectTransform mainMenuScreen;
        [SerializeField] private RectTransform raceScreen;
        [SerializeField] private RectTransform overlayRoot;

        public Canvas Canvas => canvas;
        public RectTransform ScreenRoot => screenRoot;
        public RectTransform MainMenuScreen => mainMenuScreen;
        public RectTransform RaceScreen => raceScreen;
        public RectTransform OverlayRoot => overlayRoot;

        public void Configure(
            Canvas targetCanvas,
            RectTransform targetScreenRoot,
            RectTransform targetMainMenuScreen,
            RectTransform targetRaceScreen,
            RectTransform targetOverlayRoot)
        {
            canvas = targetCanvas;
            screenRoot = targetScreenRoot;
            mainMenuScreen = targetMainMenuScreen;
            raceScreen = targetRaceScreen;
            overlayRoot = targetOverlayRoot;
        }

        public void ShowMainMenu()
        {
            SetScreenState(mainMenuScreen, true);
            SetScreenState(raceScreen, false);
        }

        public void ShowRace()
        {
            SetScreenState(mainMenuScreen, false);
            SetScreenState(raceScreen, true);
        }

        private static void SetScreenState(RectTransform screen, bool active)
        {
            if (screen != null)
            {
                screen.gameObject.SetActive(active);
            }
        }
    }
}
