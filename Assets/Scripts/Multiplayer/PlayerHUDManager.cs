using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;
using System;
using System.Collections;

/// <summary>
/// マルチプレイヤー時のHUD、カウントダウン、リザルトUIを管理するクラス。
/// P1/P2のUIにそれぞれのプレイヤーの情報を表示し、自分がどちらかを(あなた)で示す。
/// </summary>
public class PlayerHUDManager : NetworkBehaviour
{
    public static PlayerHUDManager Instance { get; private set; }

    [Header("State Panels")]
    [SerializeField] private GameObject waitingForPlayerPanel;
    [SerializeField] private GameObject inGameHUDPanel;
    [SerializeField] private GameObject resultPanel;

    [Header("Waiting UI")]
    [SerializeField] private TMP_InputField roomIdInputField;

    [Header("Countdown UI")]
    [SerializeField] private GameObject countdownPanel;
    [SerializeField] private TextMeshProUGUI countdownText;

    [Header("In-Game HUD - P1")]
    [SerializeField] private TextMeshProUGUI playerLabelP1;
    [SerializeField] private Slider oxygenSliderP1;
    [SerializeField] private TextMeshProUGUI oxygenTextP1;
    
    [Header("In-Game HUD - P2")]
    [SerializeField] private TextMeshProUGUI playerLabelP2;
    [SerializeField] private Slider oxygenSliderP2;
    [SerializeField] private TextMeshProUGUI oxygenTextP2;

    [Header("In-Game HUD - Common")]
    [SerializeField] private TextMeshProUGUI matchTimeText;
    
    [Header("Typing Panel")]
    [SerializeField] private GameObject TypingPanel_P1;
    [SerializeField] private GameObject TypingPanel_P2;

    [Header("Result Panel")]
    [SerializeField] private TextMeshProUGUI resultText; // WIN, LOSE, DRAW
    [SerializeField] private TextMeshProUGUI finalMatchTimeText;
    [Header("Result Panel - P1")]
    [SerializeField] private TextMeshProUGUI resultPlayerLabelP1;
    [SerializeField] private TextMeshProUGUI finalScoreTextP1;
    [SerializeField] private TextMeshProUGUI finalBlocksDestroyedTextP1;
    [SerializeField] private TextMeshProUGUI finalMissTypesTextP1;
    [Header("Result Panel - P2")]
    [SerializeField] private TextMeshProUGUI resultPlayerLabelP2;
    [SerializeField] private TextMeshProUGUI finalScoreTextP2;
    [SerializeField] private TextMeshProUGUI finalBlocksDestroyedTextP2;
    [SerializeField] private TextMeshProUGUI finalMissTypesTextP2;

    [Header("Oxygen Bar Colors")]
    [SerializeField] private Color fullOxygenColor = Color.green;
    [SerializeField] private Color lowOxygenColor = Color.yellow;
    [SerializeField] private Color criticalOxygenColor = Color.red;

    [Header("Self Identifier")]
    [Tooltip("自分の情報であることを示すためにプレイヤー名に付与する接尾辞")]
    [SerializeField] private string selfSuffix = " (あなた)";

    private NetworkPlayerInput _localPlayerInput;
    private int _myPlayerIndex = -1;

    #region Unity Lifecycle & Event Subscriptions

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void OnEnable()
    {
        GameDataSync.OnGameStateChanged_Client += HandleGameStateChanged;
    }

    void OnDisable()
    {
        GameDataSync.OnGameStateChanged_Client -= HandleGameStateChanged;
    }

    void Start()
    {
        if (GameDataSync.Instance != null)
        {
            HandleGameStateChanged(GameDataSync.Instance.currentState);
        }
        else
        {
            waitingForPlayerPanel.SetActive(true);
            inGameHUDPanel.SetActive(false);
            resultPanel.SetActive(false);
            countdownPanel.SetActive(false);
        }
    }

    void Update()
    {
        if (_localPlayerInput == null && NetworkClient.localPlayer != null)
        {
            _localPlayerInput = NetworkClient.localPlayer.GetComponent<NetworkPlayerInput>();
            if (_localPlayerInput != null && _localPlayerInput.playerIndex != 0)
            {
                _myPlayerIndex = _localPlayerInput.playerIndex - 1;
            }
        }

        if (GameDataSync.Instance != null && GameDataSync.Instance.currentState == GameState.Playing)
        {
            UpdateInGameHUD();
        }

        if (waitingForPlayerPanel.activeSelf && PlayFabMatchmakingManager.Instance != null)
        {
            if (!string.IsNullOrEmpty(PlayFabMatchmakingManager.Instance.roomId) && 
                roomIdInputField.text != $"Room ID: {PlayFabMatchmakingManager.Instance.roomId}")
            {
                UpdateWaitingPanel();
            }
        }
    }

    #endregion

    #region UI State Management

    private void HandleGameStateChanged(GameState newState)
    {
        waitingForPlayerPanel.SetActive(false);
        inGameHUDPanel.SetActive(false);
        resultPanel.SetActive(false);
        countdownPanel.SetActive(false);

        switch (newState)
        {
            case GameState.WaitingForPlayers:
                waitingForPlayerPanel.SetActive(true);
                UpdateWaitingPanel();
                break;

            case GameState.Countdown:
                inGameHUDPanel.SetActive(true);
                countdownPanel.SetActive(true);
                StartCoroutine(CountdownCoroutine());
                break;

            case GameState.Playing:
                inGameHUDPanel.SetActive(true);
                break;

            case GameState.PostGame:
                resultPanel.SetActive(true);
                ShowResultPanel();
                break;
        }
    }

    private IEnumerator CountdownCoroutine()
    {
        countdownText.text = "3";
        yield return new WaitForSeconds(1f);
        countdownText.text = "2";
        yield return new WaitForSeconds(1f);
        countdownText.text = "1";
        yield return new WaitForSeconds(1f);
        countdownText.text = "START!";
        yield return new WaitForSeconds(0.5f);
        countdownPanel.SetActive(false);
    }

    #endregion

    #region UI Content Updates

    private void UpdateWaitingPanel()
    {
        if (roomIdInputField != null && PlayFabMatchmakingManager.Instance != null)
        {
            roomIdInputField.text = $"Room ID: {PlayFabMatchmakingManager.Instance.roomId}";
        }
    }

    private void UpdateInGameHUD()
    {
        var gm = GameManagerMulti.Instance;
        if (gm == null) return;

        if (matchTimeText != null)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(gm.matchTime);
            matchTimeText.text = $"TIME {timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
        }

        if (gm.playerData.Count < 2 || _myPlayerIndex == -1) return;

        // P1のUIにP1(インデックス0)のデータを設定
        UpdateSinglePlayerUI(playerLabelP1, oxygenSliderP1, oxygenTextP1, gm.playerData[0], 0);
        // P2のUIにP2(インデックス1)のデータを設定
        UpdateSinglePlayerUI(playerLabelP2, oxygenSliderP2, oxygenTextP2, gm.playerData[1], 1);
    }

    private void UpdateSinglePlayerUI(TextMeshProUGUI nameLabel, Slider slider, TextMeshProUGUI oxygenText, PlayerData data, int playerIndex)
    {
        if (nameLabel != null)
        {
            string displayName = data.playerName ?? "";
            // 自分のプレイヤーインデックスと一致する場合、接尾辞を追加
            if (playerIndex == _myPlayerIndex)
            {
                displayName += selfSuffix;
            }
            nameLabel.text = displayName;
        }

        if (slider != null)
        {
            float maxOxygen = GameManagerMulti.Instance.maxOxygen;
            slider.value = data.currentOxygen / maxOxygen;
            Transform fillTransform = slider.transform.Find("Fill Area/Fill");
            if (fillTransform == null) return;

            RectTransform fillRect = fillTransform as RectTransform;
            if (fillRect == null) return;

            float oxygenPercentage = data.currentOxygen / maxOxygen;
            float newPosX = Mathf.Lerp(-460f, 0f, oxygenPercentage);
            fillRect.anchoredPosition = new Vector2(newPosX, fillRect.anchoredPosition.y);

            Image fillImage = fillTransform.GetComponent<Image>();
            if (fillImage != null)
            {
                if (oxygenPercentage <= 0.10f) fillImage.color = criticalOxygenColor;
                else if (oxygenPercentage <= 0.30f) fillImage.color = lowOxygenColor;
                else fillImage.color = fullOxygenColor;
            }
        }
        if (oxygenText != null)
        {
            oxygenText.text = $"{Mathf.CeilToInt(data.currentOxygen)}";
        }
    }

    private void ShowResultPanel()
    {
        var gm = GameManagerMulti.Instance;
        if (gm == null) return;

        if (resultText != null)
        {
            if (gm.winnerIndex == _myPlayerIndex) resultText.text = "WIN!";
            else if (gm.winnerIndex == -2) resultText.text = "DRAW";
            else resultText.text = "LOSE...";
        }

        if (finalMatchTimeText != null)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(gm.matchTime);
            finalMatchTimeText.text = $"Match Time: {timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}.{timeSpan.Milliseconds / 10:D2}";
        }

        if (gm.playerData.Count < 2 || _myPlayerIndex == -1) return;
        
        // P1のUIにP1(インデックス0)のデータを表示
        DisplayFinalStats(resultPlayerLabelP1, finalScoreTextP1, finalBlocksDestroyedTextP1, finalMissTypesTextP1, gm.playerData[0], 0, gm.matchTime);
        // P2のUIにP2(インデックス1)のデータを表示
        DisplayFinalStats(resultPlayerLabelP2, finalScoreTextP2, finalBlocksDestroyedTextP2, finalMissTypesTextP2, gm.playerData[1], 1, gm.matchTime);
    }

    private void DisplayFinalStats(TextMeshProUGUI nameLabel, TextMeshProUGUI scoreText, TextMeshProUGUI blocksText, TextMeshProUGUI missText, PlayerData data, int playerIndex, float time)
    {
        int score = Mathf.FloorToInt(time) + data.blocksDestroyed - data.missTypes;
        score = Mathf.Max(0, score);

        if (nameLabel != null)
        {
            string displayName = data.playerName ?? "";
            // 自分のプレイヤーインデックスと一致する場合、接尾辞を追加
            if (playerIndex == _myPlayerIndex)
            {
                displayName += selfSuffix;
            }
            nameLabel.text = displayName;
        }

        if (scoreText != null) scoreText.text = $"Score: {score}";
        if (blocksText != null) blocksText.text = $"Blocks Destroyed: {data.blocksDestroyed}";
        if (missText != null) missText.text = $"Miss Types: {data.missTypes}";
    }
    
    public GameObject GetTypingPanel(string key)
    {
        switch (key) {
            case "TypingPanel_P1":
                return TypingPanel_P1;
            case "TypingPanel_P2":
                return TypingPanel_P2;
            default:
                return null;
        }
    }
    #endregion
}