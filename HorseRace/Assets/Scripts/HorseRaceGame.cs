using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;

namespace HorseRace
{
public sealed class HorseRaceGame : MonoBehaviour
{
	private enum RaceState
	{
		Betting,
		Racing,
		Result
	}

	private enum ConfirmAction
	{
		None,
		ExitToMenu,
		ForfeitRace
	}

	private sealed class RaceHorse
	{
		public string Name;

		public int Lane;

		public float Speed;

		public float Stamina;

		public float Focus;

		public float Chaos;

		public float Rating;

		public float Odds;

		public float Progress;

		public float FinishTime;

		public float CurrentBurst;

		public Color Color;

		public string FlairSpriteName;
	}

	private const int HorseCount = 4;

	private const int StartingBalance = 1000;

	private const int MinStake = 100;

	private const int StakeStep = 50;

	private const int CoinGrantAmount = 500;

	private const float DefaultRaceOdds = 3f;

	private const int MaxStake = 500;

	private const float RaceContentWidth = 1900f;

	private const float CameraFollowSharpness = 4.6f;

	private const float PhysicsStartUiX = 600f;

	private const float PhysicsGroundUiY = -66f;

	private const float PhysicsPixelsPerMeter = 78f;

	private const float TrackGroundLineHalfHeight = 5f;

	private const string BalanceKey = "HorseRaceMvp.Balance";

	private static readonly Color TrackGreen = new Color(0.62f, 0.92f, 0.34f);

	private static readonly Color TrackLineBrown = new Color(0.56f, 0.36f, 0.13f);

	private static readonly Color TrackLineDarkBrown = new Color(0.34f, 0.2f, 0.07f);

	private static HorseRaceGame instance;

	private readonly List<RaceHorse> horses = new List<RaceHorse>(4);

	private readonly Dictionary<string, AudioClip> audioClips = new Dictionary<string, AudioClip>();

	private HorseyAtlas spritesAtlas;

	private HorseyAtlas furnitureAtlas;

	private HorseyAtlas terrainAtlas;

	private HorseyAtlas locsAtlas;

	private Font uiFont;

	[SerializeField]
	private HorseRaceUIView uiView;

	private GameFlowController gameFlowController;

	private AudioSource audioSource;

	private Canvas canvas;

	private RectTransform portraitRoot;

	private RectTransform trackPanel;

	private RectTransform raceViewport;

	private RectTransform raceContent;

	private RectTransform controlsPanel;

	private RectTransform resultOverlay;

	private Text balanceText;

	private Text roundText;

	private Text statusText;

	private Text rewardText;

	private Text stakeText;

	private Text startButtonText;

	private Text resultTitleText;

	private Text resultBodyText;

	private Button startButton;

	private readonly RectTransform[] laneRects = (RectTransform[])(object)new RectTransform[4];

	private readonly Image[] laneImages = new Image[4];

	private readonly Image[] laneGroundLineImages = new Image[4];

	private readonly Image[] horseImages = new Image[4];

	private readonly Image[] horseFlairImages = new Image[4];

	private readonly RectTransform[] horseBodyViews = (RectTransform[])(object)new RectTransform[4];

	private readonly RectTransform[] horseNeckViews = (RectTransform[])(object)new RectTransform[4];

	private readonly RectTransform[] horseHeadViews = (RectTransform[])(object)new RectTransform[4];

	private readonly RectTransform[] horseTailViews = (RectTransform[])(object)new RectTransform[4];

	private readonly RectTransform[] horseFrontLegViews = (RectTransform[])(object)new RectTransform[4];

	private readonly RectTransform[] horseBackLegViews = (RectTransform[])(object)new RectTransform[4];

	private readonly RectTransform[] horseFrontLowerLegViews = (RectTransform[])(object)new RectTransform[4];

	private readonly RectTransform[] horseBackLowerLegViews = (RectTransform[])(object)new RectTransform[4];

	private readonly Image[] horseBodyImages = new Image[4];

	private readonly Image[] horseNeckImages = new Image[4];

	private readonly Image[] horseHeadImages = new Image[4];

	private readonly Image[] horseTailImages = new Image[4];

	private readonly Image[] horseFrontLegImages = new Image[4];

	private readonly Image[] horseBackLegImages = new Image[4];

	private readonly Image[] horseFrontLowerLegImages = new Image[4];

	private readonly Image[] horseBackLowerLegImages = new Image[4];

	private readonly PhysicalHorseController[] physicalHorses = new PhysicalHorseController[4];

	private readonly Text[] laneNameTexts = new Text[4];

	private readonly Text[] laneStatTexts = new Text[4];

	private readonly Text[] laneRankTexts = new Text[4];

	private readonly Button[] horseButtons = new Button[4];

	private readonly Image[] horseButtonImages = new Image[4];

	private readonly Image[] horseButtonIcons = new Image[4];

	private readonly Text[] horseButtonTexts = new Text[4];

	private readonly UnityAction[] laneBetActions = new UnityAction[4];

	private readonly Dictionary<string, Sprite> runtimeResourceSprites = new Dictionary<string, Sprite>();

	private UnityAction decreaseStakeAction;

	private UnityAction addStakeAction;

	private UnityAction startRaceAction;

	private UnityAction nextRaceAction;

	private UnityAction exitButtonAction;

	private UnityAction coinButtonAction;

	private UnityAction confirmCancelAction;

	private UnityAction confirmAcceptAction;

	private bool uiEventsBound;

	private bool initialized;

	private bool raceSessionActive;

	private RaceState state;

	private int selectedHorseIndex = -1;

	private int selectedStake = MinStake;

	private int balance;

	public int Balance => balance;

	private int roundNumber = 1;

	private float raceElapsed;

	private float hoofTimer;

	private float cameraX;

	private float manualCameraTimer;

	private ConfirmAction pendingConfirmAction;

	private float statusMessageTimer;

	private string statusMessageOverride;

	private int winnerIndex = -1;

	[RuntimeInitializeOnLoadMethod(/*Could not decode attribute arguments.*/)]
	private static void Launch()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Expected O, but got Unknown
		if (!((Object)(object)Object.FindObjectOfType<HorseRaceGame>() != (Object)null))
		{
			GameObject val = new GameObject("HorseRaceMVP");
			val.AddComponent<HorseRaceGame>();
		}
	}

	private void Awake()
	{
		if ((Object)(object)instance != (Object)null && (Object)(object)instance != (Object)(object)this)
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
			return;
		}
		instance = this;
		Object.DontDestroyOnLoad((Object)(object)((Component)this).gameObject);
		ConfigureRuntime();
		LoadAssets();
		if (!ResolveInterface())
		{
			enabled = false;
			return;
		}
		balance = PlayerPrefs.GetInt("HorseRaceMvp.Balance", 1000);
		NormalizeSelectedStake();
		initialized = true;
		if ((Object)(object)Object.FindObjectOfType<GameFlowController>() == (Object)null)
		{
			EnterRace();
		}
	}

	private void OnDestroy()
	{
		UnbindViewEvents();
		DestroyPhysicalHorses();
		if ((Object)(object)instance == (Object)(object)this)
		{
			instance = null;
		}
	}

	private void Update()
	{
		if (!raceSessionActive)
		{
			return;
		}

		UpdateStatusMessage(Time.deltaTime);
		if (state == RaceState.Racing)
		{
			UpdateRace(Time.deltaTime);
			UpdateHorseVisuals();
		}
		UpdateRaceCamera(Time.deltaTime);
	}

	private void UpdateStatusMessage(float deltaTime)
	{
		if (statusMessageTimer <= 0f)
		{
			return;
		}

		statusMessageTimer -= deltaTime;
		if (statusMessageTimer > 0f)
		{
			return;
		}

		statusMessageTimer = 0f;
		statusMessageOverride = null;
		UpdateAllUi();
	}

	private void ShowTemporaryStatus(string message)
	{
		statusMessageOverride = message;
		statusMessageTimer = 1.75f;
	}

	private void ClearTemporaryStatus()
	{
		statusMessageOverride = null;
		statusMessageTimer = 0f;
	}

	private bool HasSelectedHorse()
	{
		return selectedHorseIndex >= 0 && selectedHorseIndex < horses.Count;
	}

	private int GetMaximumAffordableStake()
	{
		if (balance < MinStake)
		{
			return 0;
		}

		int cappedBalance = Mathf.Min(balance, MaxStake);
		return MinStake + ((cappedBalance - MinStake) / StakeStep) * StakeStep;
	}

	private void NormalizeSelectedStake()
	{
		if (balance < MinStake)
		{
			selectedStake = MinStake;
			return;
		}

		int maxAffordable = GetMaximumAffordableStake();
		selectedStake = Mathf.Clamp(selectedStake, MinStake, maxAffordable);
		selectedStake -= (selectedStake - MinStake) % StakeStep;
	}

	private float GetDisplayedOdds()
	{
		return HasSelectedHorse() ? horses[selectedHorseIndex].Odds : DefaultRaceOdds;
	}

	private int GetPotentialReward()
	{
		return Mathf.RoundToInt(selectedStake * GetDisplayedOdds());
	}

	private string GetStatusLabel()
	{
		if (!string.IsNullOrEmpty(statusMessageOverride))
		{
			return statusMessageOverride;
		}

		if (state == RaceState.Betting)
		{
			return HasSelectedHorse() ? "已选：" + horses[selectedHorseIndex].Name : "请选择马匹";
		}

		if (state == RaceState.Racing)
		{
			return "比赛进行中";
		}

		return winnerIndex >= 0 ? "冠军：" + horses[winnerIndex].Name : "等待下一场";
	}

	private void ApplyStartButtonVisual(bool enabled)
	{
		if ((Object)(object)uiView == (Object)null)
		{
			return;
		}

		uiView.StartButtonBackgroundImage.color = enabled ? Color.white : new Color(0.62f, 0.62f, 0.62f, 1f);
		startButtonText.color = new Color(0.06f, 0.06f, 0.06f);
	}

	private void ConfigureRuntime()
	{
		Application.targetFrameRate = 60;
		Screen.orientation = (ScreenOrientation)1;
		Screen.autorotateToLandscapeLeft = false;
		Screen.autorotateToLandscapeRight = false;
		Screen.autorotateToPortrait = true;
		Screen.autorotateToPortraitUpsideDown = false;
	}

	private void LoadAssets()
	{
		uiFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
		spritesAtlas = HorseyAtlas.Load("sprites");
		furnitureAtlas = HorseyAtlas.Load("furniture");
		terrainAtlas = HorseyAtlas.Load("terrain");
		locsAtlas = HorseyAtlas.Load("locs");
		audioSource = ((Component)this).gameObject.AddComponent<AudioSource>();
		audioSource.playOnAwake = false;
		audioSource.spatialBlend = 0f;
		LoadClip("Location_BetMore");
		LoadClip("Location_BetMax");
		LoadClip("Location_StartRace");
		LoadClip("Location_WonRace");
		LoadClip("General_GainMoney");
		LoadClip("General_LoseMoney");
		LoadClip("General_NotEnoughMoney");
		LoadClip("Animal_Hoof1");
		LoadClip("Animal_Hoof2");
		LoadClip("Animal_Hoof3");
	}

	private void LoadClip(string clipName)
	{
		AudioClip val = Resources.Load<AudioClip>("HorseyGame/Audio/" + clipName);
		if ((Object)(object)val != (Object)null)
		{
			audioClips[clipName] = val;
		}
	}

	public void AssignUIView(HorseRaceUIView view)
	{
		uiView = view;
	}

	public void AddCoins(int amount)
	{
		if (amount <= 0)
		{
			return;
		}

		balance += amount;
		NormalizeSelectedStake();
		SaveBalance();
		if (raceSessionActive)
		{
			UpdateAllUi();
		}
	}

	public void EnterRace()
	{
		if (!initialized || raceSessionActive)
		{
			return;
		}

		raceSessionActive = true;
		StartNewRaceCard();
	}

	public void ExitRace()
	{
		if (!raceSessionActive)
		{
			return;
		}

		raceSessionActive = false;
		SetPhysicalHorsesRunning(false);
		DestroyPhysicalHorses();
	}

	private bool ResolveInterface()
	{
		if ((Object)(object)uiView == (Object)null)
		{
			uiView = Object.FindObjectOfType<HorseRaceUIView>();
		}

		if ((Object)(object)uiView != (Object)null && !uiView.IsConfigured)
		{
			GameUIRoot staleRoot = ((Component)uiView).GetComponentInParent<GameUIRoot>();
			if ((Object)(object)staleRoot != (Object)null)
			{
				((Component)staleRoot).gameObject.SetActive(false);
			}
			uiView = null;
		}

		GameUIRoot uiRoot = null;
		if ((Object)(object)uiView == (Object)null)
		{
			HorseRaceUIFactory.BuildResult build = HorseRaceUIFactory.Create(((Component)this).transform, uiFont);
			uiRoot = build.Root;
			uiView = build.RaceView;
		}
		else
		{
			uiRoot = ((Component)uiView).GetComponentInParent<GameUIRoot>();
		}

		if ((Object)(object)uiView == (Object)null || !uiView.IsConfigured)
		{
			Debug.LogError("HorseRaceUIView is missing required UGUI references.", this);
			return false;
		}

		gameFlowController = ((Component)this).GetComponent<GameFlowController>();
		if ((Object)(object)gameFlowController == (Object)null && (Object)(object)uiRoot != (Object)null)
		{
			gameFlowController = ((Component)this).gameObject.AddComponent<GameFlowController>();
			gameFlowController.Configure(uiRoot, this, shouldStartAtRace: false);
		}
		else if ((Object)(object)gameFlowController == (Object)null)
		{
			gameFlowController = Object.FindObjectOfType<GameFlowController>();
		}

		BindInterface(uiView);
		ApplyStaticViewAssets();
		return true;
	}

	private void BindInterface(HorseRaceUIView view)
	{
		canvas = view.Canvas;
		portraitRoot = view.PortraitRoot;
		trackPanel = view.TrackPanel;
		raceViewport = view.RaceViewport;
		raceContent = view.RaceContent;
		controlsPanel = view.ControlsPanel;
		resultOverlay = view.ResultOverlay;
		balanceText = view.BalanceText;
		roundText = view.RoundText;
		statusText = view.StatusText;
		rewardText = view.RewardText;
		stakeText = view.StakeText;
		startButton = view.StartButton;
		startButtonText = view.StartButtonText;
		resultTitleText = view.ResultTitleText;
		resultBodyText = view.ResultBodyText;

		HorseRaceUIView.LaneView[] lanes = view.Lanes;
		for (int i = 0; i < lanes.Length && i < 4; i++)
		{
			HorseRaceUIView.LaneView lane = lanes[i];
			laneRects[i] = lane.Root;
			laneImages[i] = lane.Background;
			laneGroundLineImages[i] = lane.GroundLine;
			laneNameTexts[i] = lane.NameText;
			laneStatTexts[i] = lane.StatsText;
			laneRankTexts[i] = lane.RankText;
			horseButtons[i] = lane.BetButton;
			horseButtonImages[i] = lane.BetButtonImage;
			horseButtonIcons[i] = lane.BetIcon;
			horseButtonTexts[i] = lane.BetLabel;
			horseBodyViews[i] = lane.Body.Rect;
			horseBodyImages[i] = lane.Body.Image;
			horseNeckViews[i] = lane.Neck.Rect;
			horseNeckImages[i] = lane.Neck.Image;
			horseHeadViews[i] = lane.Head.Rect;
			horseHeadImages[i] = lane.Head.Image;
			horseTailViews[i] = lane.Tail.Rect;
			horseTailImages[i] = lane.Tail.Image;
			horseFrontLegViews[i] = lane.FrontLeg.Rect;
			horseFrontLegImages[i] = lane.FrontLeg.Image;
			horseBackLegViews[i] = lane.BackLeg.Rect;
			horseBackLegImages[i] = lane.BackLeg.Image;
			horseFrontLowerLegViews[i] = lane.FrontLowerLeg.Rect;
			horseFrontLowerLegImages[i] = lane.FrontLowerLeg.Image;
			horseBackLowerLegViews[i] = lane.BackLowerLeg.Rect;
			horseBackLowerLegImages[i] = lane.BackLowerLeg.Image;
			horseImages[i] = lane.Body.Image;
			horseFlairImages[i] = lane.Tail.Image;
		}

		view.TrackDragSurface?.Initialize(this);
		BindViewEvents();
	}

	private void BindViewEvents()
	{
		if (uiEventsBound || (Object)(object)uiView == (Object)null)
		{
			return;
		}

		for (int i = 0; i < horseButtons.Length; i++)
		{
			int horseIndex = i;
			laneBetActions[i] = delegate
			{
				SelectHorse(horseIndex);
			};
			((UnityEvent)horseButtons[i].onClick).AddListener(laneBetActions[i]);
		}

		decreaseStakeAction = delegate
		{
			ChangeStake(-StakeStep);
		};
		addStakeAction = delegate
		{
			ChangeStake(StakeStep);
		};
		startRaceAction = StartRace;
		nextRaceAction = StartNewRaceCard;
		exitButtonAction = HandleExitPressed;
		coinButtonAction = delegate
		{
			AddCoins(CoinGrantAmount);
		};
		confirmCancelAction = CloseConfirmDialog;
		confirmAcceptAction = AcceptConfirmDialog;
		((UnityEvent)uiView.DecreaseStakeButton.onClick).AddListener(decreaseStakeAction);
		((UnityEvent)uiView.AddStakeButton.onClick).AddListener(addStakeAction);
		((UnityEvent)uiView.StartButton.onClick).AddListener(startRaceAction);
		((UnityEvent)uiView.NextRaceButton.onClick).AddListener(nextRaceAction);
		((UnityEvent)uiView.ExitButton.onClick).AddListener(exitButtonAction);
		((UnityEvent)uiView.CoinButton.onClick).AddListener(coinButtonAction);
		((UnityEvent)uiView.ConfirmDialog.CancelButton.onClick).AddListener(confirmCancelAction);
		((UnityEvent)uiView.ConfirmDialog.ConfirmButton.onClick).AddListener(confirmAcceptAction);
		uiEventsBound = true;
	}

	private void UnbindViewEvents()
	{
		if (!uiEventsBound || (Object)(object)uiView == (Object)null)
		{
			return;
		}

		for (int i = 0; i < horseButtons.Length; i++)
		{
			if ((Object)(object)horseButtons[i] != (Object)null && laneBetActions[i] != null)
			{
				((UnityEvent)horseButtons[i].onClick).RemoveListener(laneBetActions[i]);
			}
		}

		if (decreaseStakeAction != null)
		{
			((UnityEvent)uiView.DecreaseStakeButton.onClick).RemoveListener(decreaseStakeAction);
		}
		if (addStakeAction != null)
		{
			((UnityEvent)uiView.AddStakeButton.onClick).RemoveListener(addStakeAction);
		}
		if (startRaceAction != null)
		{
			((UnityEvent)uiView.StartButton.onClick).RemoveListener(startRaceAction);
		}
		if (nextRaceAction != null)
		{
			((UnityEvent)uiView.NextRaceButton.onClick).RemoveListener(nextRaceAction);
		}
		if (exitButtonAction != null)
		{
			((UnityEvent)uiView.ExitButton.onClick).RemoveListener(exitButtonAction);
		}
		if (coinButtonAction != null)
		{
			((UnityEvent)uiView.CoinButton.onClick).RemoveListener(coinButtonAction);
		}
		if (confirmCancelAction != null)
		{
			((UnityEvent)uiView.ConfirmDialog.CancelButton.onClick).RemoveListener(confirmCancelAction);
		}
		if (confirmAcceptAction != null)
		{
			((UnityEvent)uiView.ConfirmDialog.ConfirmButton.onClick).RemoveListener(confirmAcceptAction);
		}
		uiEventsBound = false;
	}

	private void ApplyStaticViewAssets()
	{
		AssignSprite(uiView.ExitButtonImage, ResourceSprite("Figma/DiN/back"), preserveAspect: true);
		AssignSprite(uiView.CoinCounterBackgroundImage, ResourceSprite("Figma/DiN/coin_counter_bg"), preserveAspect: false);
		AssignSprite(uiView.CoinIconImage, ResourceSprite("Figma/DiN/coin_icon"), preserveAspect: true);
		AssignSprite(uiView.CoinPlusIconImage, ResourceSprite("Figma/DiN/plus_icon"), preserveAspect: true);
		AssignSprite(uiView.StakePanelImage, ResourceSprite("Figma/DiN/stake_panel"), preserveAspect: false);
		AssignSprite(uiView.DecreaseStakeButtonImage, ResourceSprite("Figma/DiN/stake_arrow_left"), preserveAspect: true);
		AssignSprite(uiView.AddStakeButtonImage, ResourceSprite("Figma/DiN/stake_arrow_right"), preserveAspect: true);
		AssignSprite(uiView.StartButtonBackgroundImage, ResourceSprite("Figma/DiN/start_button_bg"), preserveAspect: false);
		AssignSpriteIfMissing(uiView.ResultTicketImage, spritesAtlas?.Get("WinningTicket"), preserveAspect: true);

		HorseRaceUIView.LaneView[] lanes = uiView.Lanes;
		for (int i = 0; i < lanes.Length; i++)
		{
			HorseRaceUIView.LaneView lane = lanes[i];
			AssignSpriteIfMissing(lane.FinishPost, spritesAtlas?.Get("FinishPost"), preserveAspect: true);
			AssignSprite(lane.BetButtonImage, ResourceSprite("Figma/DiN/select_button_bg"), preserveAspect: false);
			AssignSprite(lane.BetIcon, ResourceSprite("Figma/DiN/select_check"), preserveAspect: true);
		}
	}

	private static void AssignSprite(Image image, Sprite sprite, bool preserveAspect)
	{
		if ((Object)(object)image == (Object)null || (Object)(object)sprite == (Object)null)
		{
			return;
		}

		image.sprite = sprite;
		image.type = Image.Type.Simple;
		image.color = Color.white;
		image.preserveAspect = preserveAspect;
	}

	private static void AssignSpriteIfMissing(Image image, Sprite sprite, bool preserveAspect)
	{
		if ((Object)(object)image == (Object)null || (Object)(object)image.sprite != (Object)null ||
			(Object)(object)sprite == (Object)null)
		{
			return;
		}

		image.sprite = sprite;
		image.type = Image.Type.Simple;
		image.color = Color.white;
		image.preserveAspect = preserveAspect;
	}

	private Sprite ResourceSprite(string resourcePath)
	{
		if (string.IsNullOrEmpty(resourcePath))
		{
			return null;
		}

		if (runtimeResourceSprites.TryGetValue(resourcePath, out Sprite value))
		{
			return value;
		}

		Sprite sprite = Resources.Load<Sprite>(resourcePath);
		if ((Object)(object)sprite == (Object)null)
		{
			Texture2D texture = Resources.Load<Texture2D>(resourcePath);
			if ((Object)(object)texture == (Object)null)
			{
				return null;
			}
			sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
		}

		runtimeResourceSprites[resourcePath] = sprite;
		return sprite;
	}

	private void StartNewRaceCard()
	{
		state = RaceState.Betting;
		raceElapsed = 0f;
		hoofTimer = 0f;
		winnerIndex = -1;
		pendingConfirmAction = ConfirmAction.None;
		selectedHorseIndex = -1;
		ClearTemporaryStatus();
		CloseConfirmDialog();
		((Component)resultOverlay).gameObject.SetActive(false);
		GenerateRaceHorses();
		NormalizeSelectedStake();
		for (int i = 0; i < horses.Count; i++)
		{
			horses[i].Progress = 0f;
			horses[i].FinishTime = -1f;
			horses[i].CurrentBurst = Random.Range(0.95f, 1.08f);
		}
		CreatePhysicalHorses();
		Canvas.ForceUpdateCanvases();
		UpdateAllUi();
		UpdateHorseVisuals();
		manualCameraTimer = 0f;
		SetCameraX(0f);
	}

	private void GenerateRaceHorses()
	{
		horses.Clear();
		string[] array = LoadHorseNames();
		List<string> list = new List<string>(4);
		for (int i = 0; i < 4; i++)
		{
			string item = ((array.Length != 0) ? array[Random.Range(0, array.Length)] : ("Horse " + (i + 1)));
			while (list.Contains(item) && array.Length > 4)
			{
				item = array[Random.Range(0, array.Length)];
			}
			list.Add(item);
		}
		Color[] array2 = (Color[])(object)new Color[4]
		{
			new Color(0.94f, 0.47f, 0.34f),
			new Color(0.25f, 0.72f, 0.63f),
			new Color(0.96f, 0.78f, 0.3f),
			new Color(0.45f, 0.6f, 0.93f)
		};
		string[] array3 = new string[4] { "ItemSneakers", "ItemTopHat", "ItemCrown", "ItemJetEngine" };
		for (int j = 0; j < 4; j++)
		{
			RaceHorse raceHorse = new RaceHorse
			{
				Name = list[j],
				Lane = j,
				Speed = Random.Range(0.42f, 0.94f),
				Stamina = Random.Range(0.5f, 0.95f),
				Focus = Random.Range(0.45f, 0.92f),
				Chaos = Random.Range(0.25f, 0.9f),
				Color = array2[j],
				FlairSpriteName = array3[j]
			};
			raceHorse.Rating = Mathf.Max(0.2f, raceHorse.Speed * 0.52f + raceHorse.Stamina * 0.28f + raceHorse.Focus * 0.2f + Random.Range(-0.08f, 0.08f));
			raceHorse.Odds = DefaultRaceOdds;
			horses.Add(raceHorse);
		}
	}

	private string[] LoadHorseNames()
	{
		TextAsset val = Resources.Load<TextAsset>("HorseyGame/names");
		if ((Object)(object)val == (Object)null)
		{
			return Array.Empty<string>();
		}
		return (from name in val.text.Split(new char[2] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
			select name.Trim() into name
			where name.Length > 0
			select name).ToArray();
	}

	private void SelectHorse(int horseIndex)
	{
		if (state != RaceState.Betting || horseIndex < 0 || horseIndex >= horses.Count)
		{
			return;
		}

		selectedHorseIndex = selectedHorseIndex == horseIndex ? -1 : horseIndex;
		ClearTemporaryStatus();
		Play("Location_BetMore");
		UpdateAllUi();
	}

	private void ChangeStake(int delta)
	{
		if (state != RaceState.Betting)
		{
			return;
		}

		if (balance < MinStake)
		{
			Play("General_NotEnoughMoney");
			ShowTemporaryStatus("金币不足");
			UpdateAllUi();
			return;
		}

		int maxAffordable = GetMaximumAffordableStake();
		int nextStake = Mathf.Clamp(selectedStake + delta, MinStake, maxAffordable);
		if (nextStake == selectedStake)
		{
			UpdateAllUi();
			return;
		}

		selectedStake = nextStake;
		ClearTemporaryStatus();
		Play("Location_BetMore");
		UpdateAllUi();
	}

	private void HandleExitPressed()
	{
		if (state == RaceState.Result)
		{
			PerformExitToMenu();
			return;
		}

		int penalty = GetExitPenalty();
		ShowConfirmDialog(ConfirmAction.ExitToMenu, "确认退出", $"当前比赛尚未结束，退出将扣除 {penalty} 金币，确定退出吗？");
	}

	private int GetExitPenalty()
	{
		return Mathf.Min(balance, Mathf.Max(MinStake, selectedStake));
	}

	private void ShowConfirmDialog(ConfirmAction action, string title, string message)
	{
		pendingConfirmAction = action;
		uiView.ConfirmDialog.TitleText.text = title;
		uiView.ConfirmDialog.MessageText.text = message;
		uiView.ConfirmDialog.Root.gameObject.SetActive(true);
	}

	private void CloseConfirmDialog()
	{
		pendingConfirmAction = ConfirmAction.None;
		if ((Object)(object)uiView != (Object)null && uiView.ConfirmDialog != null && uiView.ConfirmDialog.Root != null)
		{
			uiView.ConfirmDialog.Root.gameObject.SetActive(false);
		}
	}

	private void AcceptConfirmDialog()
	{
		ConfirmAction action = pendingConfirmAction;
		CloseConfirmDialog();
		switch (action)
		{
			case ConfirmAction.ExitToMenu:
				PerformExitToMenu();
				break;
			case ConfirmAction.ForfeitRace:
				ForfeitRace();
				break;
		}
	}

	private void PerformExitToMenu()
	{
		if (state != RaceState.Result)
		{
			balance -= GetExitPenalty();
			SaveBalance();
		}

		if ((Object)(object)gameFlowController == (Object)null)
		{
			gameFlowController = Object.FindObjectOfType<GameFlowController>();
		}

		if ((Object)(object)gameFlowController != (Object)null)
		{
			gameFlowController.ShowMainMenu();
		}
		else
		{
			ExitRace();
		}
	}

	private void ForfeitRace()
	{
		SetPhysicalHorsesRunning(running: false);
		state = RaceState.Result;
		winnerIndex = -1;
		resultTitleText.text = "已放弃比赛";
		resultBodyText.text = $"你放弃了本场比赛\n下注 {selectedStake}\n没有获得奖金\n当前金币 {balance}";
		roundNumber++;
		((Component)resultOverlay).gameObject.SetActive(true);
		Play("General_LoseMoney");
		UpdateAllUi();
	}

	private void StartRace()
	{
		if (state == RaceState.Racing)
		{
			ShowConfirmDialog(ConfirmAction.ForfeitRace, "确认放弃", "确定放弃比赛吗？放弃比赛没有奖金。");
			return;
		}

		if (state != RaceState.Betting)
		{
			return;
		}

		if (!HasSelectedHorse())
		{
			Play("General_NotEnoughMoney");
			ShowTemporaryStatus("请先选择马匹");
			UpdateAllUi();
			return;
		}

		if (balance < MinStake || selectedStake > balance)
		{
			Play("General_NotEnoughMoney");
			ShowTemporaryStatus("金币不足");
			UpdateAllUi();
			return;
		}

		NormalizeSelectedStake();
		balance -= selectedStake;
		SaveBalance();
		ClearTemporaryStatus();
		state = RaceState.Racing;
		raceElapsed = 0f;
		hoofTimer = 0.15f;
		winnerIndex = -1;
		manualCameraTimer = 0f;
		SetCameraX(0f);
		SetPhysicalHorsesRunning(running: true);
		Play("Location_StartRace");
		UpdateAllUi();
	}

	private void UpdateRace(float deltaTime)
	{
		raceElapsed += deltaTime;
		hoofTimer -= deltaTime;
		if (hoofTimer <= 0f)
		{
			Play("Animal_Hoof" + Random.Range(1, 4), 0.45f);
			hoofTimer = Random.Range(0.18f, 0.31f);
		}
		for (int i = 0; i < horses.Count; i++)
		{
			RaceHorse raceHorse = horses[i];
			PhysicalHorseController physicalHorseController = physicalHorses[i];
			if ((Object)(object)physicalHorseController != (Object)null)
			{
				raceHorse.Progress = physicalHorseController.Progress;
			}
			if (raceHorse.FinishTime <= 0f && raceHorse.Progress >= 1f)
			{
				raceHorse.Progress = 1f;
				raceHorse.FinishTime = raceElapsed;
				if (winnerIndex < 0)
				{
					winnerIndex = i;
					Play("Location_WonRace", 0.75f);
				}
			}
		}
		if (winnerIndex < 0 && raceElapsed > 26f)
		{
			winnerIndex = FindLeadingHorseIndex();
			horses[winnerIndex].Progress = 1f;
			horses[winnerIndex].FinishTime = raceElapsed;
			Play("Location_WonRace", 0.75f);
		}
		bool flag = winnerIndex >= 0 && raceElapsed - horses[winnerIndex].FinishTime > 1.25f;
		bool flag2 = horses.All((RaceHorse horse) => horse.FinishTime > 0f);
		if (flag || flag2)
		{
			SetPhysicalHorsesRunning(running: false);
			SettleRace();
		}
	}

	private void SettleRace()
	{
		SetPhysicalHorsesRunning(running: false);
		state = RaceState.Result;
		RaceHorse raceHorse = horses[winnerIndex];
		bool isWinner = winnerIndex == selectedHorseIndex;
		int payout = isWinner ? Mathf.RoundToInt(selectedStake * raceHorse.Odds) : 0;
		if (isWinner)
		{
			balance += payout;
			Play("General_GainMoney");
		}
		else
		{
			Play("General_LoseMoney");
		}
		SaveBalance();
		List<int> list = (from item in horses.Select((RaceHorse horse, int index) => new
			{
				Horse = horse,
				Index = index
			})
			orderby (item.Horse.FinishTime > 0f) ? item.Horse.FinishTime : (999f - item.Horse.Progress)
			select item.Index).ToList();
		for (int i = 0; i < list.Count; i++)
		{
			laneRankTexts[list[i]].text = (i + 1).ToString();
		}

		string selectedName = HasSelectedHorse() ? horses[selectedHorseIndex].Name : "未选择";
		resultTitleText.text = isWinner ? "恭喜获胜" : "再来一场";
		resultBodyText.text = isWinner
			? $"{raceHorse.Name} 冲线第一\n下注 {selectedStake}\n奖励 {payout}\n当前金币 {balance}"
			: $"{raceHorse.Name} 赢得本场\n你的 {selectedName} 没有获得第一\n当前金币 {balance}";
		roundNumber++;
		((Component)resultOverlay).gameObject.SetActive(true);
		UpdateAllUi();
	}

	private void UpdateAllUi()
	{
		balanceText.text = balance.ToString("n0");
		roundText.text = $"第 {roundNumber} 场";
		rewardText.text = $"奖励：{GetPotentialReward():n0}";
		stakeText.text = selectedStake.ToString("n0");
		statusText.text = GetStatusLabel();
		startButtonText.text = state == RaceState.Racing ? "放弃" : "开始";

		bool canAdjustStake = state == RaceState.Betting && balance >= MinStake;
		int maxAffordable = GetMaximumAffordableStake();
		uiView.DecreaseStakeButton.interactable = canAdjustStake && selectedStake > MinStake;
		uiView.AddStakeButton.interactable = canAdjustStake && maxAffordable >= MinStake && selectedStake < maxAffordable;
		startButton.interactable = state == RaceState.Betting || state == RaceState.Racing;
		ApplyStartButtonVisual(state == RaceState.Racing || (HasSelectedHorse() && balance >= MinStake && selectedStake <= balance));

		for (int i = 0; i < horses.Count; i++)
		{
			RaceHorse raceHorse = horses[i];
			laneNameTexts[i].text = $"{i + 1}. {raceHorse.Name}";
			laneStatTexts[i].text = $"{FormLabel(raceHorse)}  赔率 x{raceHorse.Odds:0.0}";
			if (state != RaceState.Result)
			{
				laneRankTexts[i].text = string.Empty;
			}
			SetHorsePartColors(i, raceHorse.Color);
			bool isSelected = i == selectedHorseIndex;
			bool showButton = state == RaceState.Betting ? (selectedHorseIndex < 0 || isSelected) : isSelected;
			laneImages[i].color = uiView != null ? uiView.TrackColor : TrackGreen;
			if ((Object)(object)laneGroundLineImages[i] != (Object)null)
			{
				laneGroundLineImages[i].color = uiView != null ? uiView.GroundLineColor : TrackLineBrown;
			}
			horseButtons[i].gameObject.SetActive(showButton);
			horseButtons[i].interactable = state == RaceState.Betting;
			if (!showButton)
			{
				continue;
			}

			horseButtonImages[i].color = isSelected ? new Color(1f, 1f, 1f, 0f) : Color.white;
			horseButtonIcons[i].gameObject.SetActive(isSelected);
			horseButtonIcons[i].color = uiView != null ? uiView.SelectedBetColor : Color.white;
			horseButtonTexts[i].text = isSelected ? string.Empty : "选择";
			horseButtonTexts[i].color = Color.white;
		}
	}

	private void UpdateLaneDebugText(int index, RaceHorse horse, PhysicalHorseController controller)
	{
		if (index >= 0 && index < laneStatTexts.Length && !((Object)(object)laneStatTexts[index] == (Object)null))
		{
			if ((Object)(object)controller == (Object)null)
			{
                laneStatTexts[index].text = $"{FormLabel(horse)}  赔率 x{horse.Odds:0.0}";
				return;
			}
            laneStatTexts[index].text = $"{FormLabel(horse)}  赔率 x{horse.Odds:0.0}\n前 v:{controller.FrontLegSpeed:0}/{controller.LegMaxAngularSpeed:0} t:{controller.FrontLegTorque:0} a:{controller.FrontLegAngle:0}{LegDebugFlag(controller.FrontLegAtLimit, controller.FrontLegStalled)}\n后 v:{controller.BackLegSpeed:0}/{controller.LegMaxAngularSpeed:0} t:{controller.BackLegTorque:0} a:{controller.BackLegAngle:0}{LegDebugFlag(controller.BackLegAtLimit, controller.BackLegStalled)}";
		}
	}

	private static string LegDebugFlag(bool atLimit, bool stalled)
	{
		if (atLimit && stalled)
		{
            return " [限][卡]";
		}
		if (atLimit)
		{
            return " [限]";
		}
        return stalled ? " [卡]" : string.Empty;
	}

	private string FormLabel(RaceHorse horse)
	{
		float num = horse.Speed * 0.48f + horse.Stamina * 0.26f + horse.Focus * 0.26f;
		if (num > 0.78f)
		{
            return "热门";
		}
		if (num > 0.62f)
		{
            return "稳定";
		}
        return "冷门";
	}

	private void UpdateHorseVisuals()
	{
		//IL_026c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0271: Unknown result type (might be due to invalid IL or missing references)
		//IL_027b: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0314: Unknown result type (might be due to invalid IL or missing references)
		//IL_0319: Unknown result type (might be due to invalid IL or missing references)
		//IL_0323: Unknown result type (might be due to invalid IL or missing references)
		//IL_034c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0351: Unknown result type (might be due to invalid IL or missing references)
		//IL_035b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0384: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < horses.Count; i++)
		{
			RaceHorse raceHorse = horses[i];
			PhysicalHorseController physicalHorseController = physicalHorses[i];
			if ((Object)(object)physicalHorseController == (Object)null)
			{
				float num = Mathf.Lerp(600f, 1755f, Mathf.Clamp01(raceHorse.Progress));
				SetUiPart(horseBackLegViews[i], horseBackLegImages[i], new Vector2(num - 32f, -24f), new Vector2(18f, 88f), 0f, Color.Lerp(raceHorse.Color, Color.black, 0.35f));
				SetUiPartHidden(horseBackLowerLegViews[i]);
				SetUiPart(horseFrontLegViews[i], horseFrontLegImages[i], new Vector2(num + 34f, -24f), new Vector2(18f, 88f), 0f, Color.Lerp(raceHorse.Color, Color.black, 0.35f));
				SetUiPartHidden(horseFrontLowerLegViews[i]);
				SetUiPart(horseTailViews[i], horseTailImages[i], new Vector2(num - 92f, 36f), new Vector2(62f, 14f), 18f, Color.Lerp(raceHorse.Color, Color.black, 0.25f));
				SetUiPart(horseBodyViews[i], horseBodyImages[i], new Vector2(num, 40f), new Vector2(142f, 48f), 0f, raceHorse.Color);
				SetUiPart(horseNeckViews[i], horseNeckImages[i], new Vector2(num + 75f, 50f), new Vector2(66f, 18f), 0f, Color.Lerp(raceHorse.Color, Color.black, 0.18f));
				SetUiPart(horseHeadViews[i], horseHeadImages[i], new Vector2(num + 102f, 50f), new Vector2(44f, 36f), 0f, Color.Lerp(raceHorse.Color, Color.white, 0.24f));
				UpdateLaneDebugText(i, raceHorse, null);
			}
			else
			{
				SyncPhysicalPart(horseBackLegViews[i], horseBackLegImages[i], physicalHorseController.GetBackLegPose(), physicalHorseController.GroundY, Color.Lerp(raceHorse.Color, Color.black, 0.35f));
				SyncPhysicalPart(horseBackLowerLegViews[i], horseBackLowerLegImages[i], physicalHorseController.GetBackLowerLegPose(), physicalHorseController.GroundY, Color.Lerp(raceHorse.Color, Color.black, 0.35f));
				SyncPhysicalPart(horseFrontLegViews[i], horseFrontLegImages[i], physicalHorseController.GetFrontLegPose(), physicalHorseController.GroundY, Color.Lerp(raceHorse.Color, Color.black, 0.35f));
				SyncPhysicalPart(horseFrontLowerLegViews[i], horseFrontLowerLegImages[i], physicalHorseController.GetFrontLowerLegPose(), physicalHorseController.GroundY, Color.Lerp(raceHorse.Color, Color.black, 0.35f));
				SyncPhysicalPart(horseTailViews[i], horseTailImages[i], physicalHorseController.GetTailPose(), physicalHorseController.GroundY, Color.Lerp(raceHorse.Color, Color.black, 0.25f));
				SyncPhysicalPart(horseBodyViews[i], horseBodyImages[i], physicalHorseController.GetBodyPose(), physicalHorseController.GroundY, raceHorse.Color);
				SyncPhysicalPart(horseNeckViews[i], horseNeckImages[i], physicalHorseController.GetNeckPose(), physicalHorseController.GroundY, Color.Lerp(raceHorse.Color, Color.black, 0.18f));
				SyncPhysicalPart(horseHeadViews[i], horseHeadImages[i], physicalHorseController.GetHeadPose(), physicalHorseController.GroundY, Color.Lerp(raceHorse.Color, Color.white, 0.24f));
				UpdateLaneDebugText(i, raceHorse, physicalHorseController);
			}
		}
	}

	private void UpdateRaceCamera(float deltaTime)
	{
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)raceContent == (Object)null) && !((Object)(object)raceViewport == (Object)null))
		{
			if (manualCameraTimer > 0f)
			{
				manualCameraTimer -= deltaTime;
			}
			else if (state == RaceState.Racing && horses.Count > 0)
			{
				int num = FindLeadingHorseIndex();
				float num2 = UiXFromPhysicsX(((Object)(object)physicalHorses[num] != (Object)null) ? physicalHorses[num].LeadingX : (horses[num].Progress * 15.5f));
				Rect rect = raceViewport.rect;
                float value = num2 - rect.width * 0.52f;
				cameraX = Mathf.Lerp(cameraX, ClampCameraX(value), 1f - Mathf.Exp(-4.6f * deltaTime));
			}
			ApplyCameraX();
		}
	}

	internal void BeginTrackDrag()
	{
		manualCameraTimer = 3.5f;
	}

	internal void DragTrack(float deltaX)
	{
		manualCameraTimer = 3.5f;
		SetCameraX(cameraX - deltaX);
	}

	internal void EndTrackDrag()
	{
		manualCameraTimer = 2f;
	}

	private void SetCameraX(float value)
	{
		cameraX = ClampCameraX(value);
		ApplyCameraX();
	}

	private float ClampCameraX(float value)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		float num;
		if (!((Object)(object)raceViewport != (Object)null))
		{
			num = 1080f;
		}
		else
		{
			Rect rect = raceViewport.rect;
            num = rect.width;
		}
		float num2 = num;
		return Mathf.Clamp(value, 0f, Mathf.Max(0f, 1900f - num2));
	}

	private void ApplyCameraX()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)raceContent != (Object)null)
		{
			raceContent.anchoredPosition = new Vector2(0f - cameraX, 0f);
		}
	}

	private void CreatePhysicalHorses()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		DestroyPhysicalHorses();
		for (int i = 0; i < horses.Count && i < physicalHorses.Length; i++)
		{
			RaceHorse raceHorse = horses[i];
			physicalHorses[i] = PhysicalHorseController.Create(i, raceHorse.Color, raceHorse.Speed, raceHorse.Stamina, raceHorse.Focus, raceHorse.Chaos);
			physicalHorses[i].SetRunning(value: false);
			ApplyHorseVisualStyle(i, physicalHorses[i].Genes);
		}
	}

	private void DestroyPhysicalHorses()
	{
		for (int i = 0; i < physicalHorses.Length; i++)
		{
			PhysicalHorseController physicalHorseController = physicalHorses[i];
			if (!((Object)(object)physicalHorseController == (Object)null))
			{
				if (Application.isPlaying)
				{
					Object.Destroy((Object)(object)((Component)physicalHorseController).gameObject);
				}
				else
				{
					Object.DestroyImmediate((Object)(object)((Component)physicalHorseController).gameObject);
				}
				physicalHorses[i] = null;
			}
		}
	}

	private void SetPhysicalHorsesRunning(bool running)
	{
		for (int i = 0; i < physicalHorses.Length; i++)
		{
			physicalHorses[i]?.SetRunning(running);
		}
	}

	private int FindLeadingHorseIndex()
	{
		int result = 0;
		float num = float.NegativeInfinity;
		for (int i = 0; i < horses.Count; i++)
		{
			float num2 = (((Object)(object)physicalHorses[i] != (Object)null) ? physicalHorses[i].LeadingX : (horses[i].Progress * 15.5f));
			if (num2 > num)
			{
				num = num2;
				result = i;
			}
		}
		return result;
	}

	private void ApplyPartDecorationColors(RectTransform view, Color color)
	{
		SetDecorationColor(view, "LegHoof", new Color(0.08f, 0.06f, 0.04f));
	}
	private void SetDecorationColor(RectTransform view, string childName, Color color)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)view == (Object)null))
		{
			Transform val = ((Transform)view).Find(childName);
			Image image = default(Image);
			if (!((Object)(object)val == (Object)null) && ((Component)val).TryGetComponent<Image>(out image))
			{
				image.color = color;
			}
		}
	}

	private void ApplyHorseVisualStyle(int index, PhysicalHorseController.HorseGenes genes)
	{
		SetGeneratedSprite(horseBodyImages[index], HorseProceduralSprites.Body(genes));
		SetGeneratedSprite(horseHeadImages[index], HorseProceduralSprites.HeadBase(genes));
		SetGeneratedSprite(horseTailImages[index], HorseProceduralSprites.Tail(genes));
		SetGeneratedSprite(horseNeckImages[index], HorseProceduralSprites.Neck(genes));
		SetGeneratedSprite(horseFrontLegImages[index], HorseProceduralSprites.Leg(genes, lower: false));
		SetGeneratedSprite(horseBackLegImages[index], HorseProceduralSprites.Leg(genes, lower: false));
		SetGeneratedSprite(horseFrontLowerLegImages[index], HorseProceduralSprites.Leg(genes, lower: true));
		SetGeneratedSprite(horseBackLowerLegImages[index], HorseProceduralSprites.Leg(genes, lower: true));

		ConfigureLegDecorations(horseFrontLegViews[index], !genes.FrontLegHasKnee, genes);
		ConfigureLegDecorations(horseBackLegViews[index], !genes.BackLegHasKnee, genes);
		ConfigureLegDecorations(horseFrontLowerLegViews[index], showFoot: true, genes);
		ConfigureLegDecorations(horseBackLowerLegViews[index], showFoot: true, genes);
	}
	private void ConfigureLegDecorations(RectTransform view, bool showFoot, PhysicalHorseController.HorseGenes genes)
	{
		float footLength = Mathf.Lerp(0.54f, 0.98f, Mathf.InverseLerp(1.55f, 2.65f, genes.FootLengthScale));
		SetDecorationRect(view, "LegHoof", new Vector2(0.18f - footLength * 0.50f, -0.13f), new Vector2(0.48f + footLength, 0.15f), 0f);
		SetDecorationSprite(view, "LegHoof", HorseProceduralSprites.Foot(genes));
		SetDecorationActive(view, "LegHoof", showFoot);
	}
	private void SetGeneratedSprite(Image image, Sprite sprite)
	{
		if (!((Object)(object)image == (Object)null))
		{
			image.sprite = sprite;
			image.type = Image.Type.Simple;
			image.preserveAspect = false;
		}
	}

	private void SetDecorationRect(RectTransform view, string childName, Vector2 anchorMin, Vector2 anchorMax, float rotation)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)view == (Object)null)
		{
			return;
		}
		Transform val = ((Transform)view).Find(childName);
		if (!((Object)(object)val == (Object)null))
		{
			RectTransform val2 = (RectTransform)(object)((val is RectTransform) ? val : null);
			if (val2 != null)
			{
				val2.anchorMin = anchorMin;
				val2.anchorMax = anchorMax;
				val2.offsetMin = Vector2.zero;
				val2.offsetMax = Vector2.zero;
				((Transform)val2).localRotation = Quaternion.Euler(0f, 0f, rotation);
			}
		}
	}

	private void SetDecorationSprite(RectTransform view, string childName, Sprite sprite)
	{
		if (!((Object)(object)view == (Object)null))
		{
			Transform val = ((Transform)view).Find(childName);
			Image image = default(Image);
			if (!((Object)(object)val == (Object)null) && ((Component)val).TryGetComponent<Image>(out image))
			{
				image.sprite = sprite;
				image.type = Image.Type.Simple;
				image.preserveAspect = false;
			}
		}
	}

	private void SetDecorationActive(RectTransform view, string childName, bool active)
	{
		Transform val = (((Object)(object)view != (Object)null) ? ((Transform)view).Find(childName) : null);
		if ((Object)(object)val != (Object)null)
		{
			((Component)val).gameObject.SetActive(active);
		}
	}

	private Sprite CreateRectSprite()
	{
		return CreateShapeSprite("Rect", (float x, float y) => true);
	}

	private Sprite CreateBodySprite(float bellyDepth)
	{
		float depth = Mathf.Clamp01(bellyDepth);
		return CreateShapeSprite("Body" + Mathf.RoundToInt(depth * 100f), delegate(float x, float y)
		{
			float num = 0.02f;
			float num2 = 0.98f;
			float num3 = 0.94f;
			float num4 = 0.08f;
			if (depth > 0.01f)
			{
				float num5 = Mathf.Clamp01((x - 0.06f) / 0.88f);
				float num6 = Mathf.Sin(Mathf.PI * num5);
				float num7 = Mathf.InverseLerp(0.18f, 0.42f, depth);
				float num8 = Mathf.Lerp(0.25f, 0.35f, num7);
				float num9 = Mathf.Lerp(0.17f, 0.31f, num7);
				num4 = num8 - num9 * num6;
			}
			return x >= num && x <= num2 && y >= num4 && y <= num3;
		});
	}

	private Sprite CreateBodyBellySprite(float bellyDepth)
	{
		float depth = Mathf.Clamp01(bellyDepth);
		return CreateShapeSprite("BodyBelly" + Mathf.RoundToInt(depth * 100f), delegate(float x, float y)
		{
			float num = Mathf.Sin(Mathf.PI * Mathf.Clamp01(x));
			float num2 = Mathf.InverseLerp(0.18f, 0.42f, depth);
			float num3 = Mathf.Lerp(0.18f, 0.96f, num) * Mathf.Lerp(0.78f, 1f, num2);
			return y <= num3;
		});
	}

	private Sprite CreateHeadSprite(PhysicalHorseController.HorseGenes genes)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Clamp01(0.7f - genes.HeadMouthLength * 0.35f);
		Vector2[] points = new Vector2[10]
		{
			new Vector2(0.02f, 0.34f),
			new Vector2(0.3f, 0.24f),
			new Vector2(num, 0.18f),
			new Vector2(0.98f, 0.3f),
			new Vector2(0.88f, 0.46f),
			new Vector2(1f, 0.62f),
			new Vector2(0.62f, 0.76f),
			new Vector2(0.48f, 1f),
			new Vector2(0.38f, 0.64f),
			new Vector2(0.02f, 0.58f)
		};
		return CreatePolygonSprite("Head", points);
	}

	private Sprite CreateTailSprite(float baseWidth)
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Clamp(baseWidth * 0.5f, 0.12f, 0.48f);
		return CreatePolygonSprite("Tail" + Mathf.RoundToInt(baseWidth * 100f), new Vector2[3]
		{
			new Vector2(0.98f, 0.5f),
			new Vector2(0.02f, 0.5f - num),
			new Vector2(0.02f, 0.5f + num)
		});
	}

	private Sprite CreateTriangleSprite(bool pointUp)
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		Vector2[] points = !pointUp ? new Vector2[3]
		{
			new Vector2(0.5f, 0.98f),
			new Vector2(0.06f, 0.06f),
			new Vector2(0.94f, 0.16f)
		} : new Vector2[3]
		{
			new Vector2(0.5f, 0.98f),
			new Vector2(0.04f, 0.02f),
			new Vector2(0.96f, 0.02f)
		};
		return CreatePolygonSprite("Triangle" + pointUp, points);
	}

	private Sprite CreateFootSprite(float toeSlant)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Clamp01(toeSlant);
		return CreatePolygonSprite("Foot" + Mathf.RoundToInt(num * 100f), new Vector2[5]
		{
			new Vector2(0.04f, 0.2f),
			new Vector2(0.78f, 0.2f),
			new Vector2(0.98f, 0.2f + num),
			new Vector2(0.98f, 0.72f),
			new Vector2(0.12f, 0.62f)
		});
	}

	private Sprite CreatePolygonSprite(string name, Vector2[] points)
	{
		return CreateShapeSprite(name, (float x, float y) => IsPointInPolygon(new Vector2(x, y), points));
	}

	private Sprite CreateShapeSprite(string name, Func<float, float, bool> isInside)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Expected O, but got Unknown
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		Texture2D val = new Texture2D(48, 48, (TextureFormat)4, false);
		((Object)val).name = "Horse" + name;
		((Texture)val).filterMode = (FilterMode)0;
        Color32 val2 = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, 0);
        Color32 val3 = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
		for (int i = 0; i < 48; i++)
		{
			for (int j = 0; j < 48; j++)
			{
				float arg = ((float)j + 0.5f) / 48f;
				float arg2 = ((float)i + 0.5f) / 48f;
				val.SetPixel(j, i, isInside(arg, arg2) ? (Color)val3 : (Color)val2);
			}
		}
		val.Apply(false, false);
		return Sprite.Create(val, new Rect(0f, 0f, 48f, 48f), new Vector2(0.5f, 0.5f), 48f);
	}

	private static bool IsPointInPolygon(Vector2 point, Vector2[] polygon)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		int num = 0;
		int num2 = polygon.Length - 1;
		while (num < polygon.Length)
		{
			Vector2 val = polygon[num];
			Vector2 val2 = polygon[num2];
			if (val.y > point.y != val2.y > point.y)
			{
				float num3 = (val2.x - val.x) * (point.y - val.y) / (val2.y - val.y) + val.x;
				if (point.x < num3)
				{
					flag = !flag;
				}
			}
			num2 = num++;
		}
		return flag;
	}

	private void SyncPhysicalPart(RectTransform view, Image image, PhysicalHorseController.PartPose pose, float groundY, Color color)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		if (!pose.Visible)
		{
			SetUiPartHidden(view);
		}
		else if (!((Object)(object)view == (Object)null))
		{
            Vector2 position = new Vector2(UiXFromPhysicsX(pose.Position.x), -66f + (pose.Position.y - groundY) * 78f);
			SetUiPart(view, image, position, pose.Size * 78f, pose.Rotation, color);
		}
	}

	private void SetUiPartHidden(RectTransform view)
	{
		if ((Object)(object)view != (Object)null)
		{
			((Component)view).gameObject.SetActive(false);
		}
	}

	private void SetUiPart(RectTransform view, Image image, Vector2 position, Vector2 size, float rotation, Color color)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)view == (Object)null))
		{
			((Component)view).gameObject.SetActive(true);
			view.anchoredPosition = position;
			view.sizeDelta = size;
			((Transform)view).localRotation = Quaternion.Euler(0f, 0f, rotation);
			if ((Object)(object)image != (Object)null)
			{
				image.color = color;
			}
			ApplyPartDecorationColors(view, color);
		}
	}

	private void SetHorsePartColors(int index, Color color)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)horseBodyImages[index] != (Object)null)
		{
			horseBodyImages[index].color = color;
		}
		if ((Object)(object)horseNeckImages[index] != (Object)null)
		{
			horseNeckImages[index].color = Color.Lerp(color, Color.black, 0.18f);
		}
		if ((Object)(object)horseHeadImages[index] != (Object)null)
		{
			horseHeadImages[index].color = Color.Lerp(color, Color.white, 0.24f);
		}
		if ((Object)(object)horseTailImages[index] != (Object)null)
		{
			horseTailImages[index].color = Color.Lerp(color, Color.black, 0.25f);
		}
		if ((Object)(object)horseFrontLegImages[index] != (Object)null)
		{
			horseFrontLegImages[index].color = Color.Lerp(color, Color.black, 0.35f);
		}
		if ((Object)(object)horseBackLegImages[index] != (Object)null)
		{
			horseBackLegImages[index].color = Color.Lerp(color, Color.black, 0.35f);
		}
		if ((Object)(object)horseFrontLowerLegImages[index] != (Object)null)
		{
			horseFrontLowerLegImages[index].color = Color.Lerp(color, Color.black, 0.35f);
		}
		if ((Object)(object)horseBackLowerLegImages[index] != (Object)null)
		{
			horseBackLowerLegImages[index].color = Color.Lerp(color, Color.black, 0.35f);
		}
	}

	private float UiXFromPhysicsX(float physicsX)
	{
		return 600f + physicsX * 78f;
	}

	private Color LaneNameColor(int index)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		return (Color)(index switch
		{
			0 => new Color(1f, 0.47f, 0.46f), 
			1 => new Color(0.16f, 0.78f, 0.25f), 
			2 => new Color(0.82f, 0.25f, 0.9f), 
			_ => new Color(0.2f, 0.37f, 1f), 
		});
	}

	private void SaveBalance()
	{
		PlayerPrefs.SetInt("HorseRaceMvp.Balance", balance);
		PlayerPrefs.Save();
	}

	private void Play(string clipName, float volume = 1f)
	{
		if (!((Object)(object)audioSource == (Object)null) && audioClips.TryGetValue(clipName, out var value))
		{
			audioSource.PlayOneShot(value, volume);
		}
	}

	private Sprite FullTextureSprite(string resourceName)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		Texture2D val = Resources.Load<Texture2D>("HorseyGame/" + resourceName);
		if ((Object)(object)val == (Object)null)
		{
			return null;
		}
		((Texture)val).filterMode = (FilterMode)0;
		return Sprite.Create(val, new Rect(0f, 0f, (float)((Texture)val).width, (float)((Texture)val).height), new Vector2(0.5f, 0.5f), 100f);
	}


}
}





















