using System;
using UnityEngine;
using UnityEngine.UI;

namespace HorseRace
{
    public sealed class HorseRaceUIView : MonoBehaviour
    {
        [Serializable]
        public sealed class HorsePartView
        {
            [SerializeField] private RectTransform rect;
            [SerializeField] private Image image;

            public RectTransform Rect => rect;
            public Image Image => image;

            public HorsePartView(RectTransform targetRect, Image targetImage)
            {
                rect = targetRect;
                image = targetImage;
            }
        }

        [Serializable]
        public sealed class LaneView
        {
            [SerializeField] private RectTransform root;
            [SerializeField] private Image background;
            [SerializeField] private Image groundLine;
            [SerializeField] private Image finishPost;
            [SerializeField] private Text nameText;
            [SerializeField] private Text statsText;
            [SerializeField] private Text rankText;
            [SerializeField] private Button betButton;
            [SerializeField] private Image betButtonImage;
            [SerializeField] private Image betIcon;
            [SerializeField] private Text betLabel;
            [SerializeField] private HorsePartView body;
            [SerializeField] private HorsePartView neck;
            [SerializeField] private HorsePartView head;
            [SerializeField] private HorsePartView tail;
            [SerializeField] private HorsePartView frontLeg;
            [SerializeField] private HorsePartView backLeg;
            [SerializeField] private HorsePartView frontLowerLeg;
            [SerializeField] private HorsePartView backLowerLeg;

            public RectTransform Root => root;
            public Image Background => background;
            public Image GroundLine => groundLine;
            public Image FinishPost => finishPost;
            public Text NameText => nameText;
            public Text StatsText => statsText;
            public Text RankText => rankText;
            public Button BetButton => betButton;
            public Image BetButtonImage => betButtonImage;
            public Image BetIcon => betIcon;
            public Text BetLabel => betLabel;
            public HorsePartView Body => body;
            public HorsePartView Neck => neck;
            public HorsePartView Head => head;
            public HorsePartView Tail => tail;
            public HorsePartView FrontLeg => frontLeg;
            public HorsePartView BackLeg => backLeg;
            public HorsePartView FrontLowerLeg => frontLowerLeg;
            public HorsePartView BackLowerLeg => backLowerLeg;

            public bool IsConfigured =>
                root != null && background != null && groundLine != null && finishPost != null &&
                nameText != null && statsText != null && rankText != null &&
                betButton != null && betButtonImage != null && betIcon != null && betLabel != null &&
                IsPartConfigured(body) && IsPartConfigured(neck) && IsPartConfigured(head) &&
                IsPartConfigured(tail) && IsPartConfigured(frontLeg) && IsPartConfigured(backLeg) &&
                IsPartConfigured(frontLowerLeg) && IsPartConfigured(backLowerLeg);

            public void ConfigureBase(
                RectTransform targetRoot,
                Image targetBackground,
                Image targetGroundLine,
                Image targetFinishPost,
                Text targetNameText,
                Text targetStatsText,
                Text targetRankText,
                Button targetBetButton,
                Image targetBetButtonImage,
                Image targetBetIcon,
                Text targetBetLabel)
            {
                root = targetRoot;
                background = targetBackground;
                groundLine = targetGroundLine;
                finishPost = targetFinishPost;
                nameText = targetNameText;
                statsText = targetStatsText;
                rankText = targetRankText;
                betButton = targetBetButton;
                betButtonImage = targetBetButtonImage;
                betIcon = targetBetIcon;
                betLabel = targetBetLabel;
            }

            public void ConfigureHorseParts(
                HorsePartView targetBody,
                HorsePartView targetNeck,
                HorsePartView targetHead,
                HorsePartView targetTail,
                HorsePartView targetFrontLeg,
                HorsePartView targetBackLeg,
                HorsePartView targetFrontLowerLeg,
                HorsePartView targetBackLowerLeg)
            {
                body = targetBody;
                neck = targetNeck;
                head = targetHead;
                tail = targetTail;
                frontLeg = targetFrontLeg;
                backLeg = targetBackLeg;
                frontLowerLeg = targetFrontLowerLeg;
                backLowerLeg = targetBackLowerLeg;
            }

            private static bool IsPartConfigured(HorsePartView part)
            {
                return part != null && part.Rect != null && part.Image != null;
            }
        }

        [Serializable]
        public sealed class ConfirmDialogView
        {
            [SerializeField] private RectTransform root;
            [SerializeField] private Text titleText;
            [SerializeField] private Text messageText;
            [SerializeField] private Button cancelButton;
            [SerializeField] private Text cancelButtonText;
            [SerializeField] private Button confirmButton;
            [SerializeField] private Text confirmButtonText;

            public RectTransform Root => root;
            public Text TitleText => titleText;
            public Text MessageText => messageText;
            public Button CancelButton => cancelButton;
            public Text CancelButtonText => cancelButtonText;
            public Button ConfirmButton => confirmButton;
            public Text ConfirmButtonText => confirmButtonText;

            public bool IsConfigured =>
                root != null && titleText != null && messageText != null &&
                cancelButton != null && cancelButtonText != null &&
                confirmButton != null && confirmButtonText != null;

            public void Configure(
                RectTransform targetRoot,
                Text targetTitleText,
                Text targetMessageText,
                Button targetCancelButton,
                Text targetCancelButtonText,
                Button targetConfirmButton,
                Text targetConfirmButtonText)
            {
                root = targetRoot;
                titleText = targetTitleText;
                messageText = targetMessageText;
                cancelButton = targetCancelButton;
                cancelButtonText = targetCancelButtonText;
                confirmButton = targetConfirmButton;
                confirmButtonText = targetConfirmButtonText;
            }
        }

        [Header("Root")]
        [SerializeField] private Canvas canvas;
        [SerializeField] private RectTransform portraitRoot;
        [SerializeField] private RectTransform trackPanel;
        [SerializeField] private RectTransform raceViewport;
        [SerializeField] private RectTransform raceContent;
        [SerializeField] private RectTransform controlsPanel;
        [SerializeField] private RectTransform resultOverlay;
        [SerializeField] private TrackDragSurface trackDragSurface;

        [Header("Header")]
        [SerializeField] private Button exitButton;
        [SerializeField] private Image exitButtonImage;
        [SerializeField] private Button coinButton;
        [SerializeField] private Image coinCounterBackgroundImage;
        [SerializeField] private Image coinIconImage;
        [SerializeField] private Image coinPlusIconImage;
        [SerializeField] private Text balanceText;
        [SerializeField] private Text roundText;

        [Header("Track")]
        [SerializeField] private Text statusText;
        [SerializeField] private LaneView[] lanes = new LaneView[4];

        [Header("Controls")]
        [SerializeField] private Text rewardText;
        [SerializeField] private Image stakePanelImage;
        [SerializeField] private Text stakeText;
        [SerializeField] private Button decreaseStakeButton;
        [SerializeField] private Image decreaseStakeButtonImage;
        [SerializeField] private Button addStakeButton;
        [SerializeField] private Image addStakeButtonImage;
        [SerializeField] private Button startButton;
        [SerializeField] private Image startButtonBackgroundImage;
        [SerializeField] private Text startButtonText;

        [Header("Result")]
        [SerializeField] private Image resultTicketImage;
        [SerializeField] private Text resultTitleText;
        [SerializeField] private Text resultBodyText;
        [SerializeField] private Button nextRaceButton;

        [Header("Confirm")]
        [SerializeField] private ConfirmDialogView confirmDialog;

        [Header("Dynamic Colors")]
        [SerializeField] private Color trackColor = new Color(0.67f, 0.98f, 0.39f);
        [SerializeField] private Color groundLineColor = new Color(0.45f, 0.28f, 0.1f);
        [SerializeField] private Color selectedBetColor = new Color(1f, 1f, 1f);
        [SerializeField] private Color unselectedBetColor = new Color(0.22f, 0.67f, 1f);

        public Canvas Canvas => canvas;
        public RectTransform PortraitRoot => portraitRoot;
        public RectTransform TrackPanel => trackPanel;
        public RectTransform RaceViewport => raceViewport;
        public RectTransform RaceContent => raceContent;
        public RectTransform ControlsPanel => controlsPanel;
        public RectTransform ResultOverlay => resultOverlay;
        public TrackDragSurface TrackDragSurface => trackDragSurface;
        public Button ExitButton => exitButton;
        public Image ExitButtonImage => exitButtonImage;
        public Button CoinButton => coinButton;
        public Image CoinCounterBackgroundImage => coinCounterBackgroundImage;
        public Image CoinIconImage => coinIconImage;
        public Image CoinPlusIconImage => coinPlusIconImage;
        public Text BalanceText => balanceText;
        public Text RoundText => roundText;
        public Text StatusText => statusText;
        public LaneView[] Lanes => lanes;
        public Text RewardText => rewardText;
        public Image StakePanelImage => stakePanelImage;
        public Text StakeText => stakeText;
        public Button DecreaseStakeButton => decreaseStakeButton;
        public Image DecreaseStakeButtonImage => decreaseStakeButtonImage;
        public Button AddStakeButton => addStakeButton;
        public Image AddStakeButtonImage => addStakeButtonImage;
        public Button StartButton => startButton;
        public Image StartButtonBackgroundImage => startButtonBackgroundImage;
        public Text StartButtonText => startButtonText;
        public Image ResultTicketImage => resultTicketImage;
        public Text ResultTitleText => resultTitleText;
        public Text ResultBodyText => resultBodyText;
        public Button NextRaceButton => nextRaceButton;
        public ConfirmDialogView ConfirmDialog => confirmDialog;
        public Color TrackColor => trackColor;
        public Color GroundLineColor => groundLineColor;
        public Color SelectedBetColor => selectedBetColor;
        public Color UnselectedBetColor => unselectedBetColor;

        public bool IsConfigured
        {
            get
            {
                if (canvas == null || portraitRoot == null || trackPanel == null || raceViewport == null ||
                    raceContent == null || controlsPanel == null || resultOverlay == null ||
                    exitButton == null || exitButtonImage == null || coinButton == null ||
                    coinCounterBackgroundImage == null || coinIconImage == null || coinPlusIconImage == null ||
                    balanceText == null || roundText == null || statusText == null || rewardText == null ||
                    stakePanelImage == null || stakeText == null || decreaseStakeButton == null ||
                    decreaseStakeButtonImage == null || addStakeButton == null || addStakeButtonImage == null ||
                    startButton == null || startButtonBackgroundImage == null || startButtonText == null ||
                    resultTitleText == null || resultBodyText == null || nextRaceButton == null ||
                    confirmDialog == null || !confirmDialog.IsConfigured ||
                    lanes == null || lanes.Length != 4)
                {
                    return false;
                }

                for (int i = 0; i < lanes.Length; i++)
                {
                    if (lanes[i] == null || !lanes[i].IsConfigured)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        public void ConfigureRoot(
            Canvas targetCanvas,
            RectTransform targetPortraitRoot,
            RectTransform targetTrackPanel,
            RectTransform targetRaceViewport,
            RectTransform targetRaceContent,
            RectTransform targetControlsPanel,
            RectTransform targetResultOverlay,
            TrackDragSurface targetTrackDragSurface)
        {
            canvas = targetCanvas;
            portraitRoot = targetPortraitRoot;
            trackPanel = targetTrackPanel;
            raceViewport = targetRaceViewport;
            raceContent = targetRaceContent;
            controlsPanel = targetControlsPanel;
            resultOverlay = targetResultOverlay;
            trackDragSurface = targetTrackDragSurface;
        }

        public void ConfigureHeader(
            Button targetExitButton,
            Image targetExitButtonImage,
            Button targetCoinButton,
            Image targetCoinCounterBackgroundImage,
            Image targetCoinIconImage,
            Image targetCoinPlusIconImage,
            Text targetBalance,
            Text targetRound)
        {
            exitButton = targetExitButton;
            exitButtonImage = targetExitButtonImage;
            coinButton = targetCoinButton;
            coinCounterBackgroundImage = targetCoinCounterBackgroundImage;
            coinIconImage = targetCoinIconImage;
            coinPlusIconImage = targetCoinPlusIconImage;
            balanceText = targetBalance;
            roundText = targetRound;
        }

        public void ConfigureTrack(Text targetStatus, LaneView[] targetLanes)
        {
            statusText = targetStatus;
            lanes = targetLanes;
        }

        public void ConfigureControls(
            Text targetReward,
            Image targetStakePanelImage,
            Text targetStake,
            Button targetDecreaseStake,
            Image targetDecreaseStakeImage,
            Button targetAddStake,
            Image targetAddStakeImage,
            Button targetStart,
            Image targetStartButtonBackgroundImage,
            Text targetStartText)
        {
            rewardText = targetReward;
            stakePanelImage = targetStakePanelImage;
            stakeText = targetStake;
            decreaseStakeButton = targetDecreaseStake;
            decreaseStakeButtonImage = targetDecreaseStakeImage;
            addStakeButton = targetAddStake;
            addStakeButtonImage = targetAddStakeImage;
            startButton = targetStart;
            startButtonBackgroundImage = targetStartButtonBackgroundImage;
            startButtonText = targetStartText;
        }

        public void ConfigureResult(
            Image targetTicket,
            Text targetTitle,
            Text targetBody,
            Button targetNextRace)
        {
            resultTicketImage = targetTicket;
            resultTitleText = targetTitle;
            resultBodyText = targetBody;
            nextRaceButton = targetNextRace;
        }

        public void ConfigureConfirmDialog(ConfirmDialogView targetConfirmDialog)
        {
            confirmDialog = targetConfirmDialog;
        }
    }
}
