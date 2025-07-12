using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;
using System;
using System.Collections;

/// <summary>
/// マルチプレイヤー時のHUD、カウントダウン、リザルトUIを管理するクラス。
/// GameDataSyncから同期されたGameStateに基づいてUIを制御する。
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
    [SerializeField] private GameObject countdownPanel; // 3, 2, 1, START を含むパネル
    [SerializeField] private TextMeshProUGUI countdownText;

    [Header("In-Game HUD")]
    [SerializeField] private TextMeshProUGUI matchTimeText;
    [SerializeField] private Slider myOxygenSlider;
    [SerializeField] private TextMeshProUGUI myOxygenText;
    [SerializeField] private Slider opponentOxygenSlider;
    [SerializeField] private TextMeshProUGUI opponentOxygenText;
    
    [Header("Typing Panel")]
    [SerializeField] private GameObject TypingPanel_P1;
    [SerializeField] private GameObject TypingPanel_P2;

    [Header("Result Panel")]
    [SerializeField] private TextMeshProUGUI resultText; // WIN, LOSE, DRAW
    [SerializeField] private TextMeshProUGUI finalMatchTimeText;
    [SerializeField] private TextMeshProUGUI myFinalScoreText;
    [SerializeField] private TextMeshProUGUI myFinalBlocksDestroyedText;
    [SerializeField] private TextMeshProUGUI myFinalMissTypesText;
    [SerializeField] private TextMeshProUGUI opponentFinalScoreText;
    [SerializeField] private TextMeshProUGUI opponentFinalBlocksDestroyedText;
    [SerializeField] private TextMeshProUGUI opponentFinalMissTypesText;

    [Header("Oxygen Bar Colors")]
    [SerializeField] private Color fullOxygenColor = Color.green;
    [SerializeField] private Color lowOxygenColor = Color.yellow;
    [SerializeField] private Color criticalOxygenColor = Color.red;

    private NetworkPlayerInput _localPlayerInput;
    private int _myPlayerIndex = -1;
    private int _opponentPlayerIndex = -1;

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
        // GameDataSyncの現在の状態に基づいてUIを初期化する
        // これにより、シーン参加時にイベントを逃した場合でも正しいUI状態から開始できる
        if (GameDataSync.Instance != null)
        {
            HandleGameStateChanged(GameDataSync.Instance.currentState);
        }
        else
        {
            // フォールバック：万が一GameDataSyncが見つからない場合は、待機状態にしておく
            waitingForPlayerPanel.SetActive(true);
            inGameHUDPanel.SetActive(false);
            resultPanel.SetActive(false);
            countdownPanel.SetActive(false);
        }
    }

    void Update()
    {
        // ローカルプレイヤーの情報を取得・設定
        if (_localPlayerInput == null && NetworkClient.localPlayer != null)
        {
            _localPlayerInput = NetworkClient.localPlayer.GetComponent<NetworkPlayerInput>();
            if (_localPlayerInput != null && _localPlayerInput.playerIndex != 0)
            {
                _myPlayerIndex = _localPlayerInput.playerIndex - 1;
                _opponentPlayerIndex = (_myPlayerIndex == 0) ? 1 : 0;
            }
        }

        // ゲームプレイ中のHUD更新
        if (GameDataSync.Instance != null && GameDataSync.Instance.currentState == GameState.Playing)
        {
            UpdateInGameHUD();
        }

        // waitingForPlayerPanelが表示されている間、Room IDを更新し続ける
        // ビルド版でroomIdの非同期取得に対応するため
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
        // 全てのパネルを一旦非表示にする（切り替えを確実にするため）
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
                inGameHUDPanel.SetActive(true); // HUDはカウントダウンから表示
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
        Debug.Log($"Room ID: {PlayFabMatchmakingManager.Instance.roomId}");
        if (roomIdInputField != null && PlayFabMatchmakingManager.Instance != null)
        {
            roomIdInputField.text = $"Room ID: {PlayFabMatchmakingManager.Instance.roomId}";
        }
    }

    private void UpdateInGameHUD()
    {
        var gm = GameManagerMulti.Instance;
        if (gm == null) return;

        // Match Time
        if (matchTimeText != null)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(gm.matchTime);
            matchTimeText.text = $"TIME {timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
        }

        // Player Data
        if (gm.playerData.Count < 2 || _myPlayerIndex == -1) return;
        UpdateSinglePlayerUI(myOxygenSlider, myOxygenText, gm.playerData[_myPlayerIndex]);
        UpdateSinglePlayerUI(opponentOxygenSlider, opponentOxygenText, gm.playerData[_opponentPlayerIndex]);
    }

    private void UpdateSinglePlayerUI(Slider slider, TextMeshProUGUI oxygenText, PlayerData data)
    {
        if (slider != null)
        {
            float maxOxygen = GameManagerMulti.Instance.maxOxygen;
            slider.value = data.currentOxygen / maxOxygen;
            Image fillImage = slider.fillRect.GetComponent<Image>();
            if (fillImage != null)
            {
                float oxygenPercentage = data.currentOxygen / maxOxygen;
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

        // Result Text
        if (resultText != null)
        {
            if (gm.winnerIndex == _myPlayerIndex) resultText.text = "WIN!";
            else if (gm.winnerIndex == -2) resultText.text = "DRAW";
            else resultText.text = "LOSE...";
        }

        // Final Time
        if (finalMatchTimeText != null)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(gm.matchTime);
            finalMatchTimeText.text = $"Match Time: {timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}.{timeSpan.Milliseconds / 10:D2}";
        }

        if (gm.playerData.Count < 2 || _myPlayerIndex == -1) return;
        
        // Final Stats
        DisplayFinalStats(myFinalScoreText, myFinalBlocksDestroyedText, myFinalMissTypesText, gm.playerData[_myPlayerIndex], gm.matchTime);
        DisplayFinalStats(opponentFinalScoreText, opponentFinalBlocksDestroyedText, opponentFinalMissTypesText, gm.playerData[_opponentPlayerIndex], gm.matchTime);
    }

    private void DisplayFinalStats(TextMeshProUGUI scoreText, TextMeshProUGUI blocksText, TextMeshProUGUI missText, PlayerData data, float time)
    {
        int score = Mathf.FloorToInt(time) + data.blocksDestroyed - data.missTypes;
        score = Mathf.Max(0, score);

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