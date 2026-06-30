using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace HorseRace
{
    public sealed class MainMenuController : MonoBehaviour
    {
        private const int HomeTabIndex = 2;
        private const int CoinGrantAmount = 500;

        private static readonly Color InactiveTabColor = new Color(0.82f, 0.82f, 0.82f);
        private static readonly Color ActiveTabColor = Color.white;

        [SerializeField] private MainMenuUIView view;
        [SerializeField] private GameFlowController gameFlow;
        [SerializeField] private HorseRaceGame horseRaceGame;

        private readonly UnityAction[] tabActions = new UnityAction[5];
        private UnityAction battleAction;
        private UnityAction coinAction;
        private bool eventsBound;

        public void Configure(MainMenuUIView targetView, HorseRaceGame targetGame, GameFlowController targetFlow)
        {
            view = targetView;
            horseRaceGame = targetGame;
            gameFlow = targetFlow;
        }

        private void OnEnable()
        {
            ResolveReferences();
            BindEvents();
            ShowTab(HomeTabIndex);
            RefreshCoinText();
        }

        private void OnDisable()
        {
            UnbindEvents();
        }

        private void ResolveReferences()
        {
            if (view == null)
            {
                view = GetComponent<MainMenuUIView>();
            }

            if (gameFlow == null)
            {
                gameFlow = FindObjectOfType<GameFlowController>();
            }

            if (horseRaceGame == null)
            {
                horseRaceGame = FindObjectOfType<HorseRaceGame>();
            }
        }

        private void BindEvents()
        {
            if (eventsBound || view == null || !view.IsConfigured)
            {
                return;
            }

            battleAction = EnterRace;
            coinAction = AddCoins;
            view.BattleButton.onClick.AddListener(battleAction);
            view.CoinButton.onClick.AddListener(coinAction);

            Button[] buttons = view.TabButtons;
            for (int i = 0; i < buttons.Length; i++)
            {
                int tabIndex = i;
                tabActions[i] = () => ShowTab(tabIndex);
                buttons[i].onClick.AddListener(tabActions[i]);
            }

            eventsBound = true;
        }

        private void UnbindEvents()
        {
            if (!eventsBound || view == null)
            {
                return;
            }

            if (battleAction != null)
            {
                view.BattleButton.onClick.RemoveListener(battleAction);
            }

            if (coinAction != null)
            {
                view.CoinButton.onClick.RemoveListener(coinAction);
            }

            Button[] buttons = view.TabButtons;
            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i] != null && tabActions[i] != null)
                {
                    buttons[i].onClick.RemoveListener(tabActions[i]);
                }
            }

            eventsBound = false;
        }

        private void EnterRace()
        {
            ResolveReferences();
            gameFlow?.ShowRace();
        }

        private void AddCoins()
        {
            ResolveReferences();
            horseRaceGame?.AddCoins(CoinGrantAmount);
            RefreshCoinText();
        }

        private void ShowTab(int index)
        {
            if (view == null || !view.IsConfigured)
            {
                return;
            }

            bool showHome = index == HomeTabIndex;
            view.HomePage.gameObject.SetActive(showHome);
            view.PlaceholderPage.gameObject.SetActive(!showHome);

            Graphic[] backgrounds = view.TabBackgrounds;
            for (int i = 0; i < backgrounds.Length; i++)
            {
                if (backgrounds[i] != null)
                {
                    backgrounds[i].color = i == index ? ActiveTabColor : InactiveTabColor;
                }
            }
        }

        private void RefreshCoinText()
        {
            if (view == null || view.CoinText == null || horseRaceGame == null)
            {
                return;
            }

            view.CoinText.text = horseRaceGame.Balance.ToString("N0");
        }
    }
}
