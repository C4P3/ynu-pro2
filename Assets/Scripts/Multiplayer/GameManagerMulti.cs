
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Mirror;

// PlayerDataクラスの定義
public class PlayerData
{
    public float currentOxygen;
    public float survivalTime;
    public int blocksDestroyed;
    public int missTypes;
    public bool isGameOver;
    public bool isOxygenInvincible;

    public PlayerData(float maxOxygen)
    {
        currentOxygen = maxOxygen;
        survivalTime = 0f;
        blocksDestroyed = 0;
        missTypes = 0;
        isGameOver = false;
        isOxygenInvincible = false;
    }
}

public class GameManagerMulti : NetworkBehaviour
{
    public static GameManagerMulti Instance { get; private set; }

    [Header("Game Settings")]
    public float maxOxygen = 100f;
    public float oxygenDecreaseRate = 0.5f;

    [Header("Player 1 UI")]
    public Slider oxygenSlider_P1;
    public TextMeshProUGUI oxygenText_P1;
    public TextMeshProUGUI survivalTimeDisplay_P1;
    public GameObject gameOverPanel_P1;
    public TextMeshProUGUI finalScoreText_P1;
    public TextMeshProUGUI finalSurvivalTimeText_P1;
    public TextMeshProUGUI finalBlocksDestroyedText_P1;
    public TextMeshProUGUI finalMissTypesText_P1;

    [Header("Player 2 UI")]
    public Slider oxygenSlider_P2;
    public TextMeshProUGUI oxygenText_P2;
    public TextMeshProUGUI survivalTimeDisplay_P2;
    public GameObject gameOverPanel_P2;
    public TextMeshProUGUI finalScoreText_P2;
    public TextMeshProUGUI finalSurvivalTimeText_P2;
    public TextMeshProUGUI finalBlocksDestroyedText_P2;
    public TextMeshProUGUI finalMissTypesText_P2;
    
    [Header("Oxygen Bar Colors")]
    public Color fullOxygenColor = Color.green;
    public Color lowOxygenColor = Color.yellow;
    public Color criticalOxygenColor = Color.red;

    private PlayerData[] _playerData;
    private bool _isGamePlaying = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        GameDataSync.OnGameStateChanged_Client += HandleGameStateChanged;
    }

    void OnDestroy()
    {
        GameDataSync.OnGameStateChanged_Client -= HandleGameStateChanged;
    }

    void Start()
    {
        InitializeGame();
    }

    void Update()
    {
        if (!_isGamePlaying) return;

        for (int i = 0; i < _playerData.Length; i++)
        {
            if (!_playerData[i].isGameOver)
            {
                UpdatePlayerData(i + 1);
            }
        }
    }

    private void HandleGameStateChanged(GameState newState)
    {
        _isGamePlaying = (newState == GameState.Playing);
        if (_isGamePlaying)
        {
            Debug.Log("Game is now playing. Starting player updates.");
        }
    }

    private void InitializeGame()
    {
        _playerData = new PlayerData[2];
        _playerData[0] = new PlayerData(maxOxygen);
        _playerData[1] = new PlayerData(maxOxygen);

        SetupUI(gameOverPanel_P1, false);
        SetupUI(gameOverPanel_P2, false);

        UpdateAllUI();
    }

    private void UpdatePlayerData(int playerIndex)
    {
        int index = playerIndex - 1;
        if (index < 0 || index >= _playerData.Length) return;

        // Oxygen
        if (!_playerData[index].isOxygenInvincible)
        {
            _playerData[index].currentOxygen -= oxygenDecreaseRate * Time.deltaTime;
            _playerData[index].currentOxygen = Mathf.Max(0, _playerData[index].currentOxygen);
            if (_playerData[index].currentOxygen <= 0)
            {
                GameOver(playerIndex);
            }
        }

        // Survival Time
        _playerData[index].survivalTime += Time.deltaTime;

        UpdateUIForPlayer(playerIndex);
    }

    public void GameOver(int playerIndex)
    {
        int index = playerIndex - 1;
        if (_playerData[index].isGameOver) return;

        _playerData[index].isGameOver = true;
        Debug.Log($"Player {playerIndex} Game Over!");

        DisplayGameOverResults(playerIndex);

        // Check if there is a winner
        int alivePlayers = 0;
        int winnerIndex = -1;
        for(int i = 0; i < _playerData.Length; i++)
        {
            if (!_playerData[i].isGameOver)
            {
                alivePlayers++;
                winnerIndex = i;
            }
        }

        if (alivePlayers == 1)
        {
            Debug.Log($"Player {winnerIndex + 1} is the winner!");
            // Optionally, you can call a method on the winner's panel as well
            // For now, we just stop the game updates for the loser.
            _isGamePlaying = false; // Or handle post-game state
        }
        else if (alivePlayers == 0)
        {
             Debug.Log("It's a draw!");
            _isGamePlaying = false;
        }
    }

    // --- Public methods to be called from other scripts ---

    public void RecoverOxygen(int playerIndex, float amount)
    {
        int index = playerIndex - 1;
        if (_playerData[index].isGameOver) return;
        _playerData[index].currentOxygen = Mathf.Min(_playerData[index].currentOxygen + amount, maxOxygen);
    }

    public void AddDestroyedBlock(int playerIndex)
    {
        int index = playerIndex - 1;
        if (!_playerData[index].isGameOver) _playerData[index].blocksDestroyed++;
    }

    public void AddMissType(int playerIndex)
    {
        int index = playerIndex - 1;
        if (!_playerData[index].isGameOver) _playerData[index].missTypes++;
    }
    
    public System.Collections.IEnumerator TemporaryOxygenInvincibility(int playerIndex, float duration)
    {
        int index = playerIndex - 1;
        _playerData[index].isOxygenInvincible = true;
        yield return new WaitForSeconds(duration);
        _playerData[index].isOxygenInvincible = false;
    }


    // --- UI Update Methods ---

    private void UpdateAllUI()
    {
        UpdateUIForPlayer(1);
        UpdateUIForPlayer(2);
    }

    private void UpdateUIForPlayer(int playerIndex)
    {
        int index = playerIndex - 1;
        Slider slider = (playerIndex == 1) ? oxygenSlider_P1 : oxygenSlider_P2;
        TextMeshProUGUI oxygenText = (playerIndex == 1) ? oxygenText_P1 : oxygenText_P2;
        TextMeshProUGUI timeDisplay = (playerIndex == 1) ? survivalTimeDisplay_P1 : survivalTimeDisplay_P2;

        // Update Oxygen UI
        if (slider != null)
        {
            slider.value = _playerData[index].currentOxygen / maxOxygen;
            Image fillImage = slider.fillRect.GetComponent<Image>();
            if (fillImage != null)
            {
                float oxygenPercentage = _playerData[index].currentOxygen / maxOxygen;
                if (oxygenPercentage <= 0.10f) fillImage.color = criticalOxygenColor;
                else if (oxygenPercentage <= 0.30f) fillImage.color = lowOxygenColor;
                else fillImage.color = fullOxygenColor;
            }
        }
        if (oxygenText != null)
        {
            oxygenText.text = $"酸素: {Mathf.CeilToInt(_playerData[index].currentOxygen)}";
        }

        // Update Survival Time UI
        if (timeDisplay != null)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(_playerData[index].survivalTime);
            timeDisplay.text = $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}.{timeSpan.Milliseconds / 10:D2}";
        }
    }

    private void DisplayGameOverResults(int playerIndex)
    {
        int index = playerIndex - 1;
        GameObject panel = (playerIndex == 1) ? gameOverPanel_P1 : gameOverPanel_P2;
        TextMeshProUGUI scoreText = (playerIndex == 1) ? finalScoreText_P1 : finalScoreText_P2;
        TextMeshProUGUI timeText = (playerIndex == 1) ? finalSurvivalTimeText_P1 : finalSurvivalTimeText_P2;
        TextMeshProUGUI blocksText = (playerIndex == 1) ? finalBlocksDestroyedText_P1 : finalBlocksDestroyedText_P2;
        TextMeshProUGUI missText = (playerIndex == 1) ? finalMissTypesText_P1 : finalMissTypesText_P2;

        SetupUI(panel, true);

        int score = Mathf.FloorToInt(_playerData[index].survivalTime) + _playerData[index].blocksDestroyed - _playerData[index].missTypes;
        score = Mathf.Max(0, score);

        if (scoreText != null) scoreText.text = $"スコア: {score}";
        if (timeText != null)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(_playerData[index].survivalTime);
            timeText.text = $"生存時間: {timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}.{timeSpan.Milliseconds / 10:D2}";
        }
        if (blocksText != null) blocksText.text = $"破壊したブロック数: {_playerData[index].blocksDestroyed}";
        if (missText != null) missText.text = $"ミスタイプ数: {_playerData[index].missTypes}";
    }

    private void SetupUI(GameObject panel, bool isActive)
    {
        if (panel == null) return;
        CanvasGroup cg = panel.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = isActive ? 1 : 0;
            cg.interactable = isActive;
            cg.blocksRaycasts = isActive;
        }
        else
        {
            panel.SetActive(isActive);
        }
    }
}
