using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HorseRace
{
    public static class HorseRaceUIFactory
    {
        public sealed class BuildResult
        {
            public GameUIRoot Root { get; }
            public MainMenuUIView MainMenuView { get; }
            public HorseRaceUIView RaceView { get; }

            public BuildResult(GameUIRoot root, MainMenuUIView mainMenuView, HorseRaceUIView raceView)
            {
                Root = root;
                MainMenuView = mainMenuView;
                RaceView = raceView;
            }
        }

        private const float RaceContentWidth = 1900f;

        private static readonly Color TrackGreen = new Color(0.67f, 0.98f, 0.39f);
        private static readonly Color TrackLineBrown = new Color(0.46f, 0.29f, 0.11f);
        private static readonly Color TrackLineDarkBrown = new Color(0.26f, 0.14f, 0.04f);

        public static BuildResult Create(Transform owner, Font font)
        {
            EnsureEventSystem(owner);

            GameObject canvasObject = new GameObject(
                "AppCanvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster),
                typeof(GameUIRoot));
            canvasObject.transform.SetParent(owner, false);

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
            Image fieldBackground = CreateImage(
                canvasRect,
                "FieldBackground",
                null,
                new Color(0.73f, 0.99f, 0.46f),
                Vector2.zero,
                Vector2.one);
            fieldBackground.raycastTarget = false;

            RectTransform screenRoot = CreateRect(canvasRect, "ScreenRoot", Vector2.zero, Vector2.one);
            RectTransform mainMenuScreen = CreateRect(screenRoot, "MainMenuScreen", Vector2.zero, Vector2.one);
            RectTransform raceScreen = CreateRect(screenRoot, "RaceScreen", Vector2.zero, Vector2.one);
            RectTransform overlayRoot = CreateRect(canvasRect, "OverlayRoot", Vector2.zero, Vector2.one);
            mainMenuScreen.gameObject.SetActive(false);

            GameUIRoot uiRoot = canvasObject.GetComponent<GameUIRoot>();
            uiRoot.Configure(canvas, screenRoot, mainMenuScreen, raceScreen, overlayRoot);

            MainMenuUIView mainMenuView = MainMenuUIFactory.Create(
                mainMenuScreen,
                font,
                owner.GetComponentInParent<HorseRaceGame>(),
                owner.GetComponentInParent<GameFlowController>());

            HorseRaceUIView raceView = raceScreen.gameObject.AddComponent<HorseRaceUIView>();
            RectTransform portraitRoot = CreateRect(raceScreen, "PortraitFrame", Vector2.zero, Vector2.one);
            AspectRatioFitter fitter = portraitRoot.gameObject.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            fitter.aspectRatio = 0.5625f;
            Image portraitBackground = portraitRoot.gameObject.AddComponent<Image>();
            portraitBackground.color = new Color(0.73f, 0.99f, 0.46f);
            portraitBackground.raycastTarget = false;

            Button exitButton;
            Image exitButtonImage;
            Button coinButton;
            Image coinBackgroundImage;
            Image coinIconImage;
            Image coinPlusIconImage;
            Text balanceText;
            Text roundText;
            BuildHeader(
                portraitRoot,
                font,
                out exitButton,
                out exitButtonImage,
                out coinButton,
                out coinBackgroundImage,
                out coinIconImage,
                out coinPlusIconImage,
                out balanceText,
                out roundText);

            RectTransform trackPanel;
            RectTransform raceViewport;
            RectTransform raceContent;
            TrackDragSurface dragSurface;
            Text statusText;
            HorseRaceUIView.LaneView[] lanes;
            BuildTrack(
                portraitRoot,
                font,
                out trackPanel,
                out raceViewport,
                out raceContent,
                out dragSurface,
                out statusText,
                out lanes);

            RectTransform controlsPanel;
            Text rewardText;
            Image stakePanelImage;
            Text stakeText;
            Button decreaseStakeButton;
            Image decreaseStakeButtonImage;
            Button addStakeButton;
            Image addStakeButtonImage;
            Button startButton;
            Image startButtonBackgroundImage;
            Text startButtonText;
            BuildControls(
                portraitRoot,
                font,
                out controlsPanel,
                out rewardText,
                out stakePanelImage,
                out stakeText,
                out decreaseStakeButton,
                out decreaseStakeButtonImage,
                out addStakeButton,
                out addStakeButtonImage,
                out startButton,
                out startButtonBackgroundImage,
                out startButtonText);

            RectTransform resultOverlay;
            Image resultTicket;
            Text resultTitle;
            Text resultBody;
            Button nextRaceButton;
            BuildResultOverlay(
                portraitRoot,
                font,
                out resultOverlay,
                out resultTicket,
                out resultTitle,
                out resultBody,
                out nextRaceButton);

            HorseRaceUIView.ConfirmDialogView confirmDialog;
            BuildConfirmDialog(portraitRoot, font, out confirmDialog);

            raceView.ConfigureRoot(
                canvas,
                portraitRoot,
                trackPanel,
                raceViewport,
                raceContent,
                controlsPanel,
                resultOverlay,
                dragSurface);
            raceView.ConfigureHeader(
                exitButton,
                exitButtonImage,
                coinButton,
                coinBackgroundImage,
                coinIconImage,
                coinPlusIconImage,
                balanceText,
                roundText);
            raceView.ConfigureTrack(statusText, lanes);
            raceView.ConfigureControls(
                rewardText,
                stakePanelImage,
                stakeText,
                decreaseStakeButton,
                decreaseStakeButtonImage,
                addStakeButton,
                addStakeButtonImage,
                startButton,
                startButtonBackgroundImage,
                startButtonText);
            raceView.ConfigureResult(resultTicket, resultTitle, resultBody, nextRaceButton);
            raceView.ConfigureConfirmDialog(confirmDialog);

            return new BuildResult(uiRoot, mainMenuView, raceView);
        }

        private static void BuildHeader(
            RectTransform root,
            Font font,
            out Button exitButton,
            out Image exitButtonImage,
            out Button coinButton,
            out Image coinBackgroundImage,
            out Image coinIconImage,
            out Image coinPlusIconImage,
            out Text balanceText,
            out Text roundText)
        {
            RectTransform header = CreateRect(
                root,
                "Header",
                new Vector2(0.03f, 0.84f),
                new Vector2(0.97f, 0.98f));

            Text exitFallbackText;
            exitButton = CreateButton(
                header,
                "ExitButton",
                "<",
                72,
                new Vector2(0f, 0.08f),
                new Vector2(0.16f, 0.92f),
                font,
                out exitFallbackText);
            exitButtonImage = exitButton.GetComponent<Image>();
            exitButtonImage.color = new Color(0.68f, 0.84f, 0.52f);
            exitFallbackText.color = Color.white;
            AddOutline(exitFallbackText, new Color(0f, 0f, 0f, 0.4f), new Vector2(2f, -2f));

            Text coinFallbackText;
            coinButton = CreateButton(
                header,
                "CoinButton",
                string.Empty,
                40,
                new Vector2(0.30f, 0.28f),
                new Vector2(0.72f, 0.88f),
                font,
                out coinFallbackText);
            coinFallbackText.gameObject.SetActive(false);
            coinBackgroundImage = coinButton.GetComponent<Image>();
            coinBackgroundImage.color = new Color(0.9f, 0.86f, 0.68f);

            coinIconImage = CreateImage(
                coinButton.GetComponent<RectTransform>(),
                "CoinIcon",
                null,
                new Color(1f, 0.82f, 0.12f),
                new Vector2(0.02f, 0.06f),
                new Vector2(0.24f, 0.94f));
            coinIconImage.preserveAspect = true;
            coinIconImage.raycastTarget = false;

            coinPlusIconImage = CreateImage(
                coinButton.GetComponent<RectTransform>(),
                "CoinPlusIcon",
                null,
                new Color(0.77f, 1f, 0.14f),
                new Vector2(0.78f, 0.18f),
                new Vector2(0.97f, 0.82f));
            coinPlusIconImage.preserveAspect = true;
            coinPlusIconImage.raycastTarget = false;

            balanceText = CreateText(
                coinButton.GetComponent<RectTransform>(),
                "Balance",
                "0",
                34,
                Color.black,
                TextAnchor.MiddleCenter,
                FontStyle.Bold,
                new Vector2(0.23f, 0.12f),
                new Vector2(0.81f, 0.88f),
                font);
            balanceText.resizeTextMinSize = 20;
            balanceText.raycastTarget = false;

            roundText = CreateText(
                header,
                "Round",
                "第 1 场",
                24,
                Color.black,
                TextAnchor.MiddleCenter,
                FontStyle.Bold,
                new Vector2(0.34f, 0f),
                new Vector2(0.68f, 0.26f),
                font);
            roundText.raycastTarget = false;
        }
        private static void BuildTrack(
            RectTransform root,
            Font font,
            out RectTransform trackPanel,
            out RectTransform raceViewport,
            out RectTransform raceContent,
            out TrackDragSurface dragSurface,
            out Text statusText,
            out HorseRaceUIView.LaneView[] lanes)
        {
            trackPanel = CreatePanel(
                root,
                "TrackPanel",
                new Color(1f, 1f, 1f, 0f),
                new Vector2(0.03f, 0.305f),
                new Vector2(0.97f, 0.815f));

            statusText = CreateText(
                trackPanel,
                "Status",
                "请选择马匹",
                26,
                new Color(0.14f, 0.12f, 0.1f),
                TextAnchor.MiddleCenter,
                FontStyle.Bold,
                new Vector2(0.03f, 0.92f),
                new Vector2(0.97f, 1f),
                font);
            statusText.raycastTarget = false;

            raceViewport = CreateRect(
                trackPanel,
                "RaceViewport",
                new Vector2(0.01f, 0f),
                new Vector2(0.99f, 0.91f));
            Image viewportImage = raceViewport.gameObject.AddComponent<Image>();
            viewportImage.color = TrackGreen;
            viewportImage.raycastTarget = true;
            raceViewport.gameObject.AddComponent<RectMask2D>();
            dragSurface = raceViewport.gameObject.AddComponent<TrackDragSurface>();

            raceContent = CreateRect(raceViewport, "RaceContent", Vector2.zero, new Vector2(0f, 1f));
            raceContent.pivot = new Vector2(0f, 0.5f);
            raceContent.sizeDelta = new Vector2(RaceContentWidth, 0f);
            raceContent.anchoredPosition = Vector2.zero;

            RectTransform selectionOverlay = CreateRect(
                trackPanel,
                "LaneSelectionOverlay",
                new Vector2(0.01f, 0f),
                new Vector2(0.99f, 0.91f));

            lanes = new HorseRaceUIView.LaneView[4];
            const float laneHeight = 0.243f;
            const float laneGap = 0.01f;
            const float top = 0.995f;
            for (int i = 0; i < lanes.Length; i++)
            {
                float maxY = top - i * (laneHeight + laneGap);
                float minY = maxY - laneHeight;
                lanes[i] = BuildLane(raceContent, selectionOverlay, font, i, new Vector2(0f, minY), new Vector2(1f, maxY));
            }
        }

        private static HorseRaceUIView.LaneView BuildLane(
            RectTransform raceContent,
            RectTransform selectionOverlay,
            Font font,
            int index,
            Vector2 anchorMin,
            Vector2 anchorMax)
        {
            RectTransform laneRoot = CreateRect(raceContent, "Lane_" + index, anchorMin, anchorMax);
            Image laneImage = laneRoot.gameObject.AddComponent<Image>();
            laneImage.color = TrackGreen;

            RectTransform lineShadow = CreateRect(
                laneRoot,
                "GroundLineShadow_" + index,
                new Vector2(0f, 0.5f),
                new Vector2(1f, 0.5f),
                new Vector2(0f, -74f),
                new Vector2(0f, -70f));
            Image shadowImage = lineShadow.gameObject.AddComponent<Image>();
            shadowImage.color = TrackLineDarkBrown;
            shadowImage.raycastTarget = false;

            RectTransform groundLine = CreateRect(
                laneRoot,
                "GroundLine_" + index,
                new Vector2(0f, 0.5f),
                new Vector2(1f, 0.5f),
                new Vector2(0f, -71f),
                new Vector2(0f, -62f));
            Image groundLineImage = groundLine.gameObject.AddComponent<Image>();
            groundLineImage.color = TrackLineBrown;
            groundLineImage.raycastTarget = false;

            Image finishPost = CreateImage(
                laneRoot,
                "FinishPost",
                null,
                Color.white,
                new Vector2(0.972f, 0.06f),
                new Vector2(0.996f, 0.94f));
            finishPost.preserveAspect = true;
            finishPost.raycastTarget = false;

            Text nameText = CreateText(
                laneRoot,
                "Name",
                "Horse",
                34,
                Color.black,
                TextAnchor.MiddleLeft,
                FontStyle.Bold,
                new Vector2(0.02f, 0.50f),
                new Vector2(0.22f, 0.88f),
                font);
            Text statsText = CreateText(
                laneRoot,
                "Stats",
                "赔率 x3",
                18,
                new Color(0.1f, 0.1f, 0.1f, 0.8f),
                TextAnchor.UpperLeft,
                FontStyle.Bold,
                new Vector2(0.02f, 0.03f),
                new Vector2(0.24f, 0.42f),
                font);
            Text rankText = CreateText(
                laneRoot,
                "Rank",
                string.Empty,
                34,
                Color.white,
                TextAnchor.MiddleCenter,
                FontStyle.Bold,
                new Vector2(0.79f, 0.18f),
                new Vector2(0.84f, 0.82f),
                font);
            AddOutline(rankText, new Color(0f, 0f, 0f, 0.5f), new Vector2(2f, -2f));

            HorseRaceUIView.HorsePartView tail = CreatePartView(laneRoot, "TailView_" + index, false);
            HorseRaceUIView.HorsePartView backLowerLeg = CreatePartView(laneRoot, "BackLowerLegView_" + index, true);
            HorseRaceUIView.HorsePartView frontLowerLeg = CreatePartView(laneRoot, "FrontLowerLegView_" + index, true);
            HorseRaceUIView.HorsePartView backLeg = CreatePartView(laneRoot, "BackLegView_" + index, true);
            HorseRaceUIView.HorsePartView frontLeg = CreatePartView(laneRoot, "FrontLegView_" + index, true);
            HorseRaceUIView.HorsePartView neck = CreatePartView(laneRoot, "NeckView_" + index, false);
            HorseRaceUIView.HorsePartView body = CreatePartView(laneRoot, "BodyView_" + index, false);
            HorseRaceUIView.HorsePartView head = CreatePartView(laneRoot, "HeadView_" + index, false);
            SetPartPreview(tail, new Vector2(520f, 40f), new Vector2(62f, 14f), 18f);
            SetPartPreview(backLeg, new Vector2(580f, -24f), new Vector2(18f, 88f), 0f);
            SetPartPreview(backLowerLeg, new Vector2(580f, -58f), new Vector2(16f, 52f), 0f);
            SetPartPreview(frontLeg, new Vector2(646f, -24f), new Vector2(18f, 88f), 0f);
            SetPartPreview(frontLowerLeg, new Vector2(646f, -58f), new Vector2(16f, 52f), 0f);
            SetPartPreview(neck, new Vector2(687f, 50f), new Vector2(66f, 18f), 0f);
            SetPartPreview(body, new Vector2(612f, 40f), new Vector2(142f, 48f), 0f);
            SetPartPreview(head, new Vector2(714f, 50f), new Vector2(44f, 36f), 0f);

            float laneSpan = anchorMax.y - anchorMin.y;
            Vector2 buttonAnchorMin = new Vector2(0.84f, anchorMin.y + laneSpan * 0.28f);
            Vector2 buttonAnchorMax = new Vector2(0.975f, anchorMin.y + laneSpan * 0.74f);

            Text betLabel;
            Button betButton = CreateButton(
                selectionOverlay,
                "LaneBetButton_" + index,
                "选择",
                30,
                buttonAnchorMin,
                buttonAnchorMax,
                font,
                out betLabel);
            Image betButtonImage = betButton.GetComponent<Image>();
            betButtonImage.color = new Color(0.22f, 0.67f, 1f);
            betLabel.color = Color.white;
            AddOutline(betLabel, new Color(0f, 0f, 0f, 0.35f), new Vector2(1f, -1f));
            Image betIcon = CreateImage(
                betButton.GetComponent<RectTransform>(),
                "BetIcon",
                null,
                Color.white,
                new Vector2(0.16f, 0.14f),
                new Vector2(0.84f, 0.86f));
            betIcon.preserveAspect = true;
            betIcon.raycastTarget = false;
            betIcon.gameObject.SetActive(false);

            HorseRaceUIView.LaneView lane = new HorseRaceUIView.LaneView();
            lane.ConfigureBase(
                laneRoot,
                laneImage,
                groundLineImage,
                finishPost,
                nameText,
                statsText,
                rankText,
                betButton,
                betButtonImage,
                betIcon,
                betLabel);
            lane.ConfigureHorseParts(body, neck, head, tail, frontLeg, backLeg, frontLowerLeg, backLowerLeg);
            return lane;
        }
        private static void BuildControls(
            RectTransform root,
            Font font,
            out RectTransform controlsPanel,
            out Text rewardText,
            out Image stakePanelImage,
            out Text stakeText,
            out Button decreaseStakeButton,
            out Image decreaseStakeButtonImage,
            out Button addStakeButton,
            out Image addStakeButtonImage,
            out Button startButton,
            out Image startButtonBackgroundImage,
            out Text startButtonText)
        {
            controlsPanel = CreatePanel(
                root,
                "ControlsPanel",
                new Color(1f, 1f, 1f, 0f),
                new Vector2(0.03f, 0.025f),
                new Vector2(0.97f, 0.29f));

            rewardText = CreateText(
                controlsPanel,
                "Reward",
                "奖励：300",
                58,
                Color.white,
                TextAnchor.MiddleCenter,
                FontStyle.Bold,
                new Vector2(0.16f, 0.74f),
                new Vector2(0.84f, 0.98f),
                font);
            AddOutline(rewardText, Color.black, new Vector2(3f, -3f));

            stakePanelImage = CreateImage(
                controlsPanel,
                "StakePanel",
                null,
                new Color(1f, 0.94f, 0.65f),
                new Vector2(0.23f, 0.40f),
                new Vector2(0.77f, 0.73f));

            stakeText = CreateText(
                controlsPanel,
                "Stake",
                "100",
                62,
                Color.black,
                TextAnchor.MiddleCenter,
                FontStyle.Bold,
                new Vector2(0.30f, 0.43f),
                new Vector2(0.70f, 0.70f),
                font);

            Text decreaseLabel;
            decreaseStakeButton = CreateButton(
                controlsPanel,
                "DecreaseStake",
                "<",
                72,
                new Vector2(0.08f, 0.34f),
                new Vector2(0.26f, 0.76f),
                font,
                out decreaseLabel);
            decreaseStakeButtonImage = decreaseStakeButton.GetComponent<Image>();
            decreaseStakeButtonImage.color = Color.white;
            decreaseLabel.color = Color.black;
            AddOutline(decreaseStakeButtonImage, new Color(0f, 0f, 0f, 0.22f), new Vector2(2f, -2f));

            Text increaseLabel;
            addStakeButton = CreateButton(
                controlsPanel,
                "IncreaseStake",
                ">",
                72,
                new Vector2(0.74f, 0.34f),
                new Vector2(0.92f, 0.76f),
                font,
                out increaseLabel);
            addStakeButtonImage = addStakeButton.GetComponent<Image>();
            addStakeButtonImage.color = Color.white;
            increaseLabel.color = Color.black;
            AddOutline(addStakeButtonImage, new Color(0f, 0f, 0f, 0.22f), new Vector2(2f, -2f));

            startButton = CreateButton(
                controlsPanel,
                "StartRace",
                "开始",
                54,
                new Vector2(0.18f, 0.04f),
                new Vector2(0.82f, 0.31f),
                font,
                out startButtonText);
            startButtonBackgroundImage = startButton.GetComponent<Image>();
            startButtonBackgroundImage.color = new Color(1f, 0.74f, 0.16f);
            startButtonText.color = Color.black;
            AddOutline(startButtonBackgroundImage, new Color(0f, 0.32f, 0f, 0.22f), new Vector2(0f, -8f));
        }

        private static void BuildResultOverlay(
            RectTransform root,
            Font font,
            out RectTransform resultOverlay,
            out Image ticket,
            out Text title,
            out Text body,
            out Button nextRaceButton)
        {
            resultOverlay = CreateRect(root, "ResultOverlay", Vector2.zero, Vector2.one);
            Image overlayImage = resultOverlay.gameObject.AddComponent<Image>();
            overlayImage.color = new Color(0f, 0f, 0f, 0.56f);

            RectTransform resultCard = CreatePanel(
                resultOverlay,
                "ResultCard",
                new Color(0.98f, 0.92f, 0.76f, 0.99f),
                new Vector2(0.08f, 0.23f),
                new Vector2(0.92f, 0.77f));
            ticket = CreateImage(
                resultCard,
                "Ticket",
                null,
                Color.white,
                new Vector2(0.1f, 0.78f),
                new Vector2(0.9f, 0.92f));
            ticket.preserveAspect = true;
            ticket.raycastTarget = false;
            title = CreateText(
                resultCard,
                "ResultTitle",
                "比赛结果",
                44,
                new Color(0.15f, 0.12f, 0.1f),
                TextAnchor.MiddleCenter,
                FontStyle.Bold,
                new Vector2(0.08f, 0.61f),
                new Vector2(0.92f, 0.78f),
                font);
            body = CreateText(
                resultCard,
                "ResultBody",
                "比赛结果将在这里显示",
                29,
                new Color(0.2f, 0.16f, 0.12f),
                TextAnchor.UpperCenter,
                FontStyle.Bold,
                new Vector2(0.08f, 0.25f),
                new Vector2(0.92f, 0.6f),
                font);
            Text nextLabel;
            nextRaceButton = CreateButton(
                resultCard,
                "NextRace",
                "下一场",
                34,
                new Vector2(0.18f, 0.07f),
                new Vector2(0.82f, 0.21f),
                font,
                out nextLabel);
            nextRaceButton.GetComponent<Image>().color = new Color(0.84f, 0.28f, 0.22f);
            resultOverlay.gameObject.SetActive(false);
        }

        private static void BuildConfirmDialog(
            RectTransform root,
            Font font,
            out HorseRaceUIView.ConfirmDialogView confirmDialog)
        {
            RectTransform overlay = CreateRect(root, "ConfirmOverlay", Vector2.zero, Vector2.one);
            Image overlayImage = overlay.gameObject.AddComponent<Image>();
            overlayImage.color = new Color(0f, 0f, 0f, 0.58f);

            RectTransform dialogCard = CreatePanel(
                overlay,
                "ConfirmCard",
                new Color(0.96f, 0.91f, 0.74f, 1f),
                new Vector2(0.08f, 0.34f),
                new Vector2(0.92f, 0.66f));

            Text titleText = CreateText(
                dialogCard,
                "Title",
                "确认操作",
                42,
                new Color(0.17f, 0.13f, 0.1f),
                TextAnchor.MiddleCenter,
                FontStyle.Bold,
                new Vector2(0.08f, 0.72f),
                new Vector2(0.92f, 0.9f),
                font);

            Text messageText = CreateText(
                dialogCard,
                "Message",
                "确认信息",
                30,
                new Color(0.18f, 0.14f, 0.1f),
                TextAnchor.MiddleCenter,
                FontStyle.Bold,
                new Vector2(0.08f, 0.34f),
                new Vector2(0.92f, 0.72f),
                font);

            Text cancelLabel;
            Button cancelButton = CreateButton(
                dialogCard,
                "CancelButton",
                "取消",
                32,
                new Vector2(0.08f, 0.08f),
                new Vector2(0.45f, 0.26f),
                font,
                out cancelLabel);
            cancelButton.GetComponent<Image>().color = new Color(0.68f, 0.72f, 0.75f);
            cancelLabel.color = Color.black;

            Text confirmLabel;
            Button confirmButton = CreateButton(
                dialogCard,
                "ConfirmButton",
                "确定",
                32,
                new Vector2(0.55f, 0.08f),
                new Vector2(0.92f, 0.26f),
                font,
                out confirmLabel);
            confirmButton.GetComponent<Image>().color = new Color(0.98f, 0.71f, 0.16f);
            confirmLabel.color = Color.black;

            confirmDialog = new HorseRaceUIView.ConfirmDialogView();
            confirmDialog.Configure(overlay, titleText, messageText, cancelButton, cancelLabel, confirmButton, confirmLabel);
            overlay.gameObject.SetActive(false);
        }
        private static HorseRaceUIView.HorsePartView CreatePartView(
            RectTransform parent,
            string name,
            bool createHoof)
        {
            RectTransform rect = CreateRect(
                parent,
                name,
                new Vector2(0f, 0.5f),
                new Vector2(0f, 0.5f));
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(24f, 24f);
            Image image = rect.gameObject.AddComponent<Image>();
            image.color = Color.white;
            image.raycastTarget = false;

            if (createHoof)
            {
                Image hoof = CreateImage(
                    rect,
                    "LegHoof",
                    null,
                    new Color(0.08f, 0.06f, 0.04f),
                    new Vector2(-0.34f, -0.14f),
                    new Vector2(1.08f, 0.16f));
                hoof.raycastTarget = false;
            }

            return new HorseRaceUIView.HorsePartView(rect, image);
        }

        private static void SetPartPreview(
            HorseRaceUIView.HorsePartView part,
            Vector2 position,
            Vector2 size,
            float rotation)
        {
            part.Rect.anchoredPosition = position;
            part.Rect.sizeDelta = size;
            part.Rect.localRotation = Quaternion.Euler(0f, 0f, rotation);
        }

        private static void EnsureEventSystem(Transform owner)
        {
            if (UnityEngine.Object.FindObjectOfType<EventSystem>() != null)
            {
                return;
            }

            GameObject eventSystemObject = new GameObject(
                "EventSystem",
                typeof(EventSystem),
                typeof(StandaloneInputModule));
            eventSystemObject.transform.SetParent(owner, false);
        }

        private static RectTransform CreatePanel(
            Transform parent,
            string name,
            Color color,
            Vector2 anchorMin,
            Vector2 anchorMax)
        {
            RectTransform rect = CreateRect(parent, name, anchorMin, anchorMax);
            Image image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            return rect;
        }

        private static Image CreateImage(
            Transform parent,
            string name,
            Sprite sprite,
            Color color,
            Vector2 anchorMin,
            Vector2 anchorMax)
        {
            RectTransform rect = CreateRect(parent, name, anchorMin, anchorMax);
            Image image = rect.gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.type = Image.Type.Simple;
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
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = Mathf.Max(14, Mathf.RoundToInt(fontSize * 0.62f));
            text.resizeTextMaxSize = fontSize;
            return text;
        }

        private static Button CreateButton(
            Transform parent,
            string name,
            string label,
            int fontSize,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Font font,
            out Text labelText)
        {
            RectTransform rect = CreateRect(parent, name, anchorMin, anchorMax);
            Image image = rect.gameObject.AddComponent<Image>();
            image.color = new Color(0.21f, 0.29f, 0.28f);
            Button button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.transition = Selectable.Transition.ColorTint;
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.08f, 1.08f, 1.08f);
            colors.pressedColor = new Color(0.78f, 0.78f, 0.78f);
            colors.disabledColor = new Color(0.45f, 0.45f, 0.45f, 0.7f);
            button.colors = colors;

            labelText = CreateText(
                rect,
                "Label",
                label,
                fontSize,
                Color.white,
                TextAnchor.MiddleCenter,
                FontStyle.Bold,
                Vector2.zero,
                Vector2.one,
                font);
            labelText.raycastTarget = false;
            labelText.rectTransform.offsetMin = new Vector2(10f, 6f);
            labelText.rectTransform.offsetMax = new Vector2(-10f, -6f);
            return button;
        }

        private static RectTransform CreateRect(
            Transform parent,
            string name,
            Vector2 anchorMin,
            Vector2 anchorMax)
        {
            return CreateRect(parent, name, anchorMin, anchorMax, Vector2.zero, Vector2.zero);
        }

        private static RectTransform CreateRect(
            Transform parent,
            string name,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 offsetMin,
            Vector2 offsetMax)
        {
            GameObject gameObject = new GameObject(name, typeof(RectTransform));
            gameObject.transform.SetParent(parent, false);
            RectTransform rect = gameObject.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            return rect;
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

