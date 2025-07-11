
using UnityEngine;
using Mirror;
using System.Collections;
using System.Collections.Generic; // SyncListのために追加

public enum MatchState
{
    WaitingForPlayers,
    Playing,
    Finished
}

// PlayerDataをstructに変更し、Mirrorで同期できるようにします。
[System.Serializable]
public struct PlayerData
{
    public float currentOxygen;
    public int blocksDestroyed;
    public int missTypes;
    public bool isGameOver;
    public bool isOxygenInvincible;

    public PlayerData(float maxOxygen)
    {
        currentOxygen = maxOxygen;
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

    // --- 同期されるゲーム状態 ---
    [SyncVar(hook = nameof(OnMatchStateChanged))]
    public MatchState currentMatchState = MatchState.WaitingForPlayers;

    [SyncVar]
    public float matchTime;

    [SyncVar]
    public int winnerIndex = -1; // -1:未定, 0:P1, 1:P2, -2:引き分け

    public readonly SyncList<PlayerData> playerData = new SyncList<PlayerData>();

    private Coroutine _endGameCoroutine;

    #region Unity Lifecycle & Mirror Callbacks

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void OnEnable()
    {
        // GameDataSyncからのグローバルなゲーム状態変更を監視
        GameDataSync.OnGameStateChanged_Client += HandleGameSystemStateChanged;
    }

    void OnDisable()
    {
        GameDataSync.OnGameStateChanged_Client -= HandleGameSystemStateChanged;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        InitializeGame();
    }
    
    void Update()
    {
        if (!isServer || currentMatchState != MatchState.Playing) return;

        matchTime += Time.deltaTime;
        UpdatePlayersOxygen();
    }

    #endregion

    #region Server-side Logic

    [Server]
    private void InitializeGame()
    {
        playerData.Clear();
        playerData.Add(new PlayerData(maxOxygen));
        playerData.Add(new PlayerData(maxOxygen));
        matchTime = 0f;
        winnerIndex = -1;
        currentMatchState = MatchState.WaitingForPlayers;

        if (_endGameCoroutine != null)
        {
            StopCoroutine(_endGameCoroutine);
            _endGameCoroutine = null;
        }
    }

    [Server]
    private void UpdatePlayersOxygen()
    {
        for (int i = 0; i < playerData.Count; i++)
        {
            if (playerData[i].isGameOver) continue;

            PlayerData data = playerData[i];

            if (!data.isOxygenInvincible)
            {
                data.currentOxygen -= oxygenDecreaseRate * Time.deltaTime;
                data.currentOxygen = Mathf.Max(0, data.currentOxygen);
            }

            if (data.currentOxygen <= 0)
            {
                data.isGameOver = true;
                playerData[i] = data; // isGameOverの変更をSyncListに反映
                CheckForWinner();
                return; // 勝者判定ロジックに任せる
            }
            
            playerData[i] = data; // 酸素量の変更をSyncListに反映
        }
    }

    [Server]
    private void CheckForWinner()
    {
        if (currentMatchState != MatchState.Playing) return;

        int aliveCount = 0;
        int lastAlivePlayerIndex = -1;

        for (int i = 0; i < playerData.Count; i++)
        {
            if (!playerData[i].isGameOver)
            {
                aliveCount++;
                lastAlivePlayerIndex = i;
            }
        }

        if (aliveCount <= 1)
        {
            currentMatchState = MatchState.Finished;
            winnerIndex = (aliveCount == 1) ? lastAlivePlayerIndex : -2; // 1人ならその人が勝ち、0人なら引き分け
            Debug.Log($"Match Finished. Winner Index: {winnerIndex}");
        }
    }

    #endregion

    #region Client-side Hooks

    // GameDataSyncからのグローバルなゲームステート変更をハンドル
    private void HandleGameSystemStateChanged(GameState newState)
    {
        if (isServer && newState == GameState.Playing && currentMatchState == MatchState.WaitingForPlayers)
        {
            currentMatchState = MatchState.Playing;
            Debug.Log("Server has set MatchState to Playing.");
        }
    }

    // マッチステートが変更されたときに全クライアントで呼ばれるHook
    public void OnMatchStateChanged(MatchState oldState, MatchState newState)
    {
        Debug.Log($"Client detected MatchState change from {oldState} to {newState}");
        // この変更をPlayerHUDManagerが検知してUIを更新する
    }

    #endregion

    #region Public Server Methods (called via Commands from player)

    [Server]
    public void ServerRecoverOxygen(int playerIndex, float amount)
    {
        int index = playerIndex - 1;
        if (index < 0 || index >= playerData.Count || playerData[index].isGameOver) return;
        
        PlayerData data = playerData[index];
        data.currentOxygen = Mathf.Min(data.currentOxygen + amount, maxOxygen);
        playerData[index] = data;
    }

    [Server]
    public void ServerAddDestroyedBlock(int playerIndex)
    {
        int index = playerIndex - 1;
        if (index < 0 || index >= playerData.Count || playerData[index].isGameOver) return;

        PlayerData data = playerData[index];
        data.blocksDestroyed++;
        playerData[index] = data;
    }

    [Server]
    public void ServerAddMissType(int playerIndex)
    {
        int index = playerIndex - 1;
        if (index < 0 || index >= playerData.Count || playerData[index].isGameOver) return;

        PlayerData data = playerData[index];
        data.missTypes++;
        playerData[index] = data;
    }
    
    [Server]
    public void StartTemporaryInvincibility(int playerIndex, float duration)
    {
        StartCoroutine(TemporaryOxygenInvincibilityCoroutine(playerIndex, duration));
    }

    [Server]
    private IEnumerator TemporaryOxygenInvincibilityCoroutine(int playerIndex, float duration)
    {
        int index = playerIndex - 1;
        if (index < 0 || index >= playerData.Count) yield break;

        PlayerData data = playerData[index];
        data.isOxygenInvincible = true;
        playerData[index] = data;

        yield return new WaitForSeconds(duration);

        if (index < playerData.Count && !playerData[index].isGameOver)
        {
            data = playerData[index];
            data.isOxygenInvincible = false;
            playerData[index] = data;
        }
    }

    #endregion
}
