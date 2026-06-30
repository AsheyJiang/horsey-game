using UnityEngine;

namespace HorseRace
{
    public sealed class GameFlowController : MonoBehaviour
    {
        [SerializeField] private GameUIRoot uiRoot;
        [SerializeField] private HorseRaceGame horseRaceGame;
        [SerializeField] private bool startAtRace;

        private void Awake()
        {
            ResolveReferences();
        }

        private void Start()
        {
            if (startAtRace || uiRoot == null || uiRoot.MainMenuScreen == null)
            {
                ShowRace();
            }
            else
            {
                ShowMainMenu();
            }
        }

        public void Configure(GameUIRoot root, HorseRaceGame game, bool shouldStartAtRace)
        {
            uiRoot = root;
            horseRaceGame = game;
            startAtRace = shouldStartAtRace;
        }

        public void ShowMainMenu()
        {
            ResolveReferences();
            horseRaceGame?.ExitRace();
            uiRoot?.ShowMainMenu();
        }

        public void ShowRace()
        {
            ResolveReferences();
            uiRoot?.ShowRace();
            horseRaceGame?.EnterRace();
        }

        private void ResolveReferences()
        {
            if (uiRoot == null)
            {
                uiRoot = FindObjectOfType<GameUIRoot>();
            }

            if (horseRaceGame == null)
            {
                horseRaceGame = GetComponent<HorseRaceGame>();
            }

            if (horseRaceGame == null)
            {
                horseRaceGame = FindObjectOfType<HorseRaceGame>();
            }
        }
    }
}
