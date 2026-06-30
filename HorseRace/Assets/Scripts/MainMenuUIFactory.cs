using UnityEngine;
using UnityEngine.UI;

namespace HorseRace
{
    public static class MainMenuUIFactory
    {
        private static readonly Color NavigationColor = new Color(0.133f, 0.157f, 0.208f);
        private static readonly Color TabColor = new Color(0.216f, 0.251f, 0.337f);

        public static MainMenuUIView Create(
            RectTransform screen,
            Font font,
            HorseRaceGame game,
            GameFlowController flow)
        {
            MainMenuUIView view = screen.gameObject.AddComponent<MainMenuUIView>();
            MainMenuController controller = screen.gameObject.AddComponent<MainMenuController>();

            RectTransform portraitRoot = CreateRect(screen, "MainPortraitFrame", Vector2.zero, Vector2.one);
            VerticalGradientGraphic background = portraitRoot.gameObject.AddComponent<VerticalGradientGraphic>();
            background.raycastTarget = false;
            CreateTopShade(portraitRoot);

            RectTransform homePage = CreateRect(portraitRoot, "HomePage", Vector2.zero, Vector2.one);
            RectTransform placeholderPage = CreateRect(portraitRoot, "PlaceholderPage", Vector2.zero, Vector2.one);
            placeholderPage.gameObject.SetActive(false);

            RectTransform mainVisual = CreatePanel(
                homePage,
                "MainVisualPlaceholder",
                new Color(0.95f, 0.95f, 0.98f, 0.96f),
                new Vector2(0.5f, 0.6f),
                new Vector2(0.5f, 0.6f));
            mainVisual.pivot = new Vector2(0.5f, 0.5f);
            mainVisual.sizeDelta = new Vector2(700f, 700f);
            mainVisual.GetComponent<Image>().raycastTarget = false;

            Button coinButton;
            Text coinText;
            BuildCoinCounter(portraitRoot, font, out coinButton, out coinText);

            Button battleButton = BuildBattleButton(homePage, font);

            Button[] tabButtons;
            Graphic[] tabBackgrounds;
            BuildTabBar(portraitRoot, font, out tabButtons, out tabBackgrounds);

            view.Configure(
                portraitRoot,
                homePage,
                placeholderPage,
                coinButton,
                coinText,
                battleButton,
                tabButtons,
                tabBackgrounds);
            controller.Configure(view, game, flow);
            return view;
        }

        private static void CreateTopShade(RectTransform root)
        {
            RectTransform shadeRect = CreateRect(
                root,
                "TopShade",
                new Vector2(0f, 0.84f),
                Vector2.one);
            VerticalGradientGraphic shade = shadeRect.gameObject.AddComponent<VerticalGradientGraphic>();
            shade.Configure(
                new[]
                {
                    new Color(0f, 0f, 0f, 0.72f),
                    new Color(0f, 0f, 0f, 0.26f),
                    new Color(0f, 0f, 0f, 0f)
                },
                new[] { 0f, 0.5f, 1f });
            shade.raycastTarget = false;
        }
        private static void BuildCoinCounter(
            RectTransform root,
            Font font,
            out Button coinButton,
            out Text coinText)
        {
            RectTransform counterRect = CreateRect(
                root,
                "CoinCounterButton",
                Vector2.one,
                Vector2.one);
            counterRect.pivot = Vector2.one;
            counterRect.sizeDelta = new Vector2(181f, 181f * 34f / 72f);
            counterRect.anchoredPosition = new Vector2(-30f, -110f);

            Image hitArea = counterRect.gameObject.AddComponent<Image>();
            hitArea.color = new Color(1f, 1f, 1f, 0f);
            coinButton = counterRect.gameObject.AddComponent<Button>();
            ConfigureButton(coinButton, hitArea);

            RawImage counterBackground = CreateRawImage(
                counterRect,
                "FigmaCounterBackground",
                "Figma/RaidRush/coin_counter_bg",
                new Vector2(12f / 72f, 7f / 34f),
                new Vector2(1f, 31f / 34f));
            if (counterBackground.texture == null)
            {
                counterBackground.color = new Color(0f, 0.2f, 0.263f, 0.96f);
                AddOutline(counterBackground, Color.black, new Vector2(2f, -2f));
            }

            RawImage coinIcon = CreateRawImage(
                counterRect,
                "FigmaCoinIcon",
                "Figma/RaidRush/coin_icon",
                new Vector2(2f / 72f, 4f / 34f),
                new Vector2(32f / 72f, 1f));
            if (coinIcon.texture == null)
            {
                coinIcon.gameObject.SetActive(false);
                Text fallbackCoin = CreateText(
                    counterRect,
                    "CoinFallback",
                    "$",
                    34,
                    new Color(1f, 0.82f, 0.08f),
                    TextAnchor.MiddleCenter,
                    FontStyle.Bold,
                    new Vector2(0f, 0f),
                    new Vector2(0.38f, 1f),
                    font);
                AddOutline(fallbackCoin, Color.black, new Vector2(1f, -1f));
            }

            RawImage plusIcon = CreateRawImage(
                counterRect,
                "FigmaPlusIcon",
                "Figma/RaidRush/plus_icon",
                new Vector2(19f / 72f, 3f / 34f),
                new Vector2(33f / 72f, 17f / 34f));
            if (plusIcon.texture == null)
            {
                plusIcon.gameObject.SetActive(false);
                CreateText(
                    counterRect,
                    "PlusFallback",
                    "+",
                    28,
                    new Color(0.65f, 1f, 0.08f),
                    TextAnchor.MiddleCenter,
                    FontStyle.Bold,
                    new Vector2(0.24f, 0f),
                    new Vector2(0.48f, 0.62f),
                    font);
            }

            coinText = CreateText(
                counterRect,
                "CoinValue",
                "0",
                28,
                Color.white,
                TextAnchor.MiddleCenter,
                FontStyle.Bold,
                new Vector2(32f / 72f, 10f / 34f),
                new Vector2(68f / 72f, 28f / 34f),
                font);
            AddOutline(coinText, Color.black, new Vector2(1f, -1f));
        }
        private static Button BuildBattleButton(RectTransform homePage, Font font)
        {
            RectTransform rect = CreateRect(
                homePage,
                "BattleButton",
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f));
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(395f, 170f);
            rect.anchoredPosition = new Vector2(0f, 648f);
            RawImage graphic = rect.gameObject.AddComponent<RawImage>();
            graphic.texture = LoadTexture("Figma/RaidRush/battle_cta_bg");
            graphic.color = graphic.texture != null ? Color.white : new Color(1f, 0.79f, 0f);
            AddOutline(graphic, Color.black, new Vector2(2f, -2f));

            Button button = rect.gameObject.AddComponent<Button>();
            ConfigureButton(button, graphic);

            Text label = CreateText(
                rect,
                "Label",
                "BATTLE",
                66,
                Color.white,
                TextAnchor.MiddleCenter,
                FontStyle.Bold,
                new Vector2(0.04f, 0.08f),
                new Vector2(0.96f, 0.92f),
                font);
            AddOutline(label, Color.black, new Vector2(4f, -4f));
            return button;
        }

        private static void BuildTabBar(
            RectTransform root,
            Font font,
            out Button[] buttons,
            out Graphic[] backgrounds)
        {
            RectTransform navigation = CreatePanel(
                root,
                "BottomNavigation",
                NavigationColor,
                new Vector2(0f, 0f),
                new Vector2(1f, 0f));
            navigation.pivot = new Vector2(0.5f, 0f);
            navigation.sizeDelta = new Vector2(0f, 301f);

            Image shine = CreateImage(
                navigation,
                "Shine",
                new Color(0.36f, 0.43f, 0.6f),
                new Vector2(0f, 104f / 120f),
                new Vector2(1f, 109f / 120f));
            shine.raycastTarget = false;

            RectTransform tabSet = CreateRect(
                navigation,
                "TabSet",
                new Vector2(0f, 28f / 120f),
                new Vector2(1f, 104f / 120f));

            float[] minX = { 0f, 79f / 430f, 158f / 430f, 275f / 430f, 354f / 430f };
            float[] maxX = { 76f / 430f, 155f / 430f, 272f / 430f, 351f / 430f, 1f };
            string[] names = { "ShopTab", "CollectionTab", "HomeTab", "LockedTab", "UpgradeTab" };
            string[] texturePaths =
            {
                "Figma/RaidRush/tab_shop",
                "Figma/RaidRush/tab_collection",
                "Figma/RaidRush/tab_home",
                "Figma/RaidRush/tab_locked",
                "Figma/RaidRush/tab_upgrade"
            };

            buttons = new Button[5];
            backgrounds = new Graphic[5];
            for (int i = 0; i < buttons.Length; i++)
            {
                RectTransform tab = CreateRect(
                    tabSet,
                    names[i],
                    new Vector2(minX[i], 0f),
                    new Vector2(maxX[i], 1f));

                Image hitArea = tab.gameObject.AddComponent<Image>();
                hitArea.color = new Color(1f, 1f, 1f, 0f);
                buttons[i] = tab.gameObject.AddComponent<Button>();
                ConfigureButton(buttons[i], hitArea);

                Vector2 graphicMax = i == 2
                    ? new Vector2(1f, 97f / 76f)
                    : Vector2.one;
                RawImage graphic = CreateRawImage(
                    tab,
                    "FigmaGraphic",
                    texturePaths[i],
                    Vector2.zero,
                    graphicMax);
                if (graphic.texture == null)
                {
                    graphic.color = TabColor;
                    BuildTabIcon(tab, i, font);
                }

                backgrounds[i] = graphic;
            }

            Image safeArea = CreateImage(
                navigation,
                "BottomSafeArea",
                new Color(0.196f, 0.227f, 0.306f),
                Vector2.zero,
                new Vector2(1f, 25f / 120f));
            safeArea.raycastTarget = false;

            Image homeIndicator = CreateImage(
                safeArea.rectTransform,
                "HomeIndicator",
                new Color(1f, 1f, 1f, 0.2f),
                new Vector2(0.315f, 0.38f),
                new Vector2(0.685f, 0.58f));
            homeIndicator.raycastTarget = false;
        }
        private static void BuildTabIcon(RectTransform tab, int index, Font font)
        {
            switch (index)
            {
                case 0:
                    BuildShopIcon(tab);
                    break;
                case 1:
                    BuildCollectionIcon(tab);
                    break;
                case 2:
                    BuildHomeIcon(tab, font);
                    break;
                case 3:
                    BuildLockIcon(tab);
                    break;
                default:
                    BuildUpgradeIcon(tab);
                    break;
            }
        }

        private static void BuildShopIcon(RectTransform tab)
        {
            CreateImage(tab, "BagHandle", new Color(0.82f, 0.82f, 0.82f), new Vector2(0.28f, 0.56f), new Vector2(0.72f, 0.78f));
            CreateImage(tab, "BagBody", new Color(0.12f, 0.61f, 0.94f), new Vector2(0.24f, 0.22f), new Vector2(0.76f, 0.62f));
        }

        private static void BuildCollectionIcon(RectTransform tab)
        {
            Color[] colors =
            {
                new Color(0.57f, 0.13f, 0.81f),
                new Color(0.98f, 0.65f, 0.02f),
                new Color(0f, 0.76f, 0.95f)
            };
            float[] rotations = { 22f, 8f, -10f };
            float[] positions = { 0.27f, 0.39f, 0.51f };
            for (int i = 0; i < colors.Length; i++)
            {
                Image card = CreateImage(
                    tab,
                    "Card" + i,
                    colors[i],
                    new Vector2(positions[i], 0.22f),
                    new Vector2(positions[i] + 0.34f, 0.78f));
                card.rectTransform.localRotation = Quaternion.Euler(0f, 0f, rotations[i]);
                AddOutline(card, Color.black, new Vector2(2f, -2f));
            }
        }

        private static void BuildHomeIcon(RectTransform tab, Font font)
        {
            CreateImage(tab, "LeftTower", new Color(0.82f, 0.89f, 0.96f), new Vector2(0.27f, 0.55f), new Vector2(0.43f, 0.9f));
            CreateImage(tab, "RightTower", new Color(0.82f, 0.89f, 0.96f), new Vector2(0.57f, 0.55f), new Vector2(0.73f, 0.9f));
            CreateImage(tab, "BattleBase", new Color(0.32f, 0.62f, 0.95f), new Vector2(0.25f, 0.34f), new Vector2(0.75f, 0.58f));
            Text label = CreateText(tab, "Label", "BATTLE", 24, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold, new Vector2(0.08f, 0.02f), new Vector2(0.92f, 0.34f), font);
            AddOutline(label, Color.black, new Vector2(2f, -2f));
        }

        private static void BuildLockIcon(RectTransform tab)
        {
            CreateImage(tab, "ShackleLeft", new Color(0.72f, 0.75f, 0.78f), new Vector2(0.31f, 0.53f), new Vector2(0.39f, 0.8f));
            CreateImage(tab, "ShackleRight", new Color(0.72f, 0.75f, 0.78f), new Vector2(0.61f, 0.53f), new Vector2(0.69f, 0.8f));
            CreateImage(tab, "ShackleTop", new Color(0.72f, 0.75f, 0.78f), new Vector2(0.37f, 0.72f), new Vector2(0.63f, 0.82f));
            Image body = CreateImage(tab, "LockBody", new Color(0.45f, 0.49f, 0.53f), new Vector2(0.25f, 0.2f), new Vector2(0.75f, 0.58f));
            AddOutline(body, Color.black, new Vector2(2f, -2f));
        }

        private static void BuildUpgradeIcon(RectTransform tab)
        {
            CreateImage(tab, "UpgradeBase", new Color(0.13f, 0.75f, 0.91f), new Vector2(0.27f, 0.18f), new Vector2(0.73f, 0.4f));
            CreateImage(tab, "UpgradeCore", new Color(0.78f, 0.86f, 0.92f), new Vector2(0.38f, 0.38f), new Vector2(0.62f, 0.7f));
            Image cap = CreateImage(tab, "UpgradeCap", new Color(0.93f, 0.9f, 0.22f), new Vector2(0.3f, 0.65f), new Vector2(0.7f, 0.82f));
            cap.rectTransform.localRotation = Quaternion.Euler(0f, 0f, -8f);
        }

        private static RectTransform CreatePanel(
            Transform parent,
            string name,
            Color color,
            Vector2 anchorMin,
            Vector2 anchorMax)
        {
            Image image = CreateImage(parent, name, color, anchorMin, anchorMax);
            return image.rectTransform;
        }

        private static Image CreateImage(
            Transform parent,
            string name,
            Color color,
            Vector2 anchorMin,
            Vector2 anchorMax)
        {
            RectTransform rect = CreateRect(parent, name, anchorMin, anchorMax);
            Image image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static Texture2D LoadTexture(string resourcePath)
        {
            Texture2D texture = Resources.Load<Texture2D>(resourcePath);
            if (texture != null)
            {
                return texture;
            }

            Sprite sprite = Resources.Load<Sprite>(resourcePath);
            return sprite != null ? sprite.texture : null;
        }

        private static RawImage CreateRawImage(
            Transform parent,
            string name,
            string resourcePath,
            Vector2 anchorMin,
            Vector2 anchorMax)
        {
            RectTransform rect = CreateRect(parent, name, anchorMin, anchorMax);
            RawImage image = rect.gameObject.AddComponent<RawImage>();
            image.texture = LoadTexture(resourcePath);
            image.color = Color.white;
            image.raycastTarget = false;
            return image;
        }

        private static Text CreateText(
            Transform parent,
            string name,
            string value,
            int fontSize,
            Color color,
            TextAnchor alignment,
            FontStyle style,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Font font)
        {
            RectTransform rect = CreateRect(parent, name, anchorMin, anchorMax);
            Text text = rect.gameObject.AddComponent<Text>();
            text.font = font;
            text.text = value;
            text.fontSize = fontSize;
            text.color = color;
            text.alignment = alignment;
            text.fontStyle = style;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = Mathf.Max(12, Mathf.RoundToInt(fontSize * 0.55f));
            text.resizeTextMaxSize = fontSize;
            text.raycastTarget = false;
            return text;
        }

        private static RectTransform CreateRect(
            Transform parent,
            string name,
            Vector2 anchorMin,
            Vector2 anchorMax)
        {
            GameObject gameObject = new GameObject(name, typeof(RectTransform));
            gameObject.transform.SetParent(parent, false);
            RectTransform rect = gameObject.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return rect;
        }

        private static void ConfigureButton(Button button, Graphic target)
        {
            button.targetGraphic = target;
            button.transition = Selectable.Transition.ColorTint;
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.08f, 1.08f, 1.08f);
            colors.pressedColor = new Color(0.76f, 0.76f, 0.76f);
            colors.disabledColor = new Color(0.45f, 0.45f, 0.45f, 0.7f);
            button.colors = colors;
        }

        private static void AddOutline(Graphic graphic, Color color, Vector2 distance)
        {
            Outline outline = graphic.gameObject.AddComponent<Outline>();
            outline.effectColor = color;
            outline.effectDistance = distance;
            outline.useGraphicAlpha = true;
        }
    }
}

