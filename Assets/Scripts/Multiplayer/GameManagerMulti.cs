using UnityEngine;
using Mirror;
using System.Collections;
using System.Collections.Generic; // SyncListのために追加

// PlayerDataをstructに変更し、Mirrorで同期できるようにします。
[System.Serializable]
public struct PlayerData
{
    public string playerName; // ★ プレイヤー名を追加
    public float currentOxygen;
    public int blocksDestroyed;
    public int missTypes;
    public bool isGameOver;
    public bool isOxygenInvincible;

    public PlayerData(float maxOxygen, string name = "Player") // ★ コンストラクタを更新
    {
        playerName = name;
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

    [SyncVar]
    public float matchTime;

    [SyncVar]
    public int winnerIndex = -1; // -1:未定, 0:P1, 1:P2, -2:引き分け

    public readonly SyncList<PlayerData> playerData = new SyncList<PlayerData>();

    [Header("Scene References")]
    [Tooltip("Player1のLevelManager")]
    public LevelManager levelManagerP1;
    [Tooltip("Player2のLevelManager")]
    public LevelManager levelManagerP2;
    [Tooltip("Player1の仮想カメラ")]
    public Unity.Cinemachine.CinemachineCamera vcamP1;
    [Tooltip("Player2の仮想カメラ")]
    public Unity.Cinemachine.CinemachineCamera vcamP2;
    [Tooltip("Player1のタイピングパネル")]
    public GameObject typingPanelP1;
    [Tooltip("Player2のタイピングパネル")]
    public GameObject typingPanelP2;

    [Header("Prefabs")]
    [Tooltip("シーンにGameSceneBGMManagerが存在しない場合に生成するプレハブ")]
    public GameObject gameSceneBGMManagerPrefab;

    private Coroutine _endGameCoroutine;
    private bool _isGamePlaying = false; // GameDataSyncの状態をローカルで保持

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
        // BGMマネージャーがシーンになければ生成する
        if (GameSceneBGMManager.Instance == null && gameSceneBGMManagerPrefab != null)
        {
            GameObject bgmManager = Instantiate(gameSceneBGMManagerPrefab);
            NetworkServer.Spawn(bgmManager);
        }
        InitializeGame();
    }
    
    void Update()
    {
        // isServerかつ、GameDataSyncから受け取ったプレイ中の状態フラグをチェック
        if (!isServer || !_isGamePlaying) return;

        matchTime += Time.deltaTime;
        UpdatePlayersOxygen();
    }

    #endregion

    #region Server-side Logic

    [Server]
    private void InitializeGame()
    {
        playerData.Clear();
        // ★ 初期プレイヤー名を指定して追加
        playerData.Add(new PlayerData(maxOxygen, "Player 1"));
        playerData.Add(new PlayerData(maxOxygen, "Player 2"));
        matchTime = 0f;
        winnerIndex = -1;
        _isGamePlaying = false; // 初期状態ではプレイ中ではない

        if (_endGameCoroutine != null)
        {
            StopCoroutine(_endGameCoroutine);
            _endGameCoroutine = null;
        }
    }

    [Server]
    private void UpdatePlayersOxygen()
    {
        bool needsWinnerCheck = false;
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
                needsWinnerCheck = true;
            }
            
            playerData[i] = data; // 酸素量とゲームオーバー状態の変更をSyncListに反映
        }

        if (needsWinnerCheck)
        {
            CheckForWinner();
        }
    }

    [Server]
    private void CheckForWinner()
    {
        if (!_isGamePlaying) return;

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
            // GameDataSyncの状態をPostGameに変更するようリクエスト
            if(isServer)
            {
                GameDataSync.Instance.EndGame();
            }
            winnerIndex = (aliveCount == 1) ? lastAlivePlayerIndex : -2; // 1人ならその人が勝ち、0人なら引き分け
            Debug.Log($"Match Finished. Winner Index: {winnerIndex}");
        }
    }

    #endregion

    #region Client-side Hooks

    // GameDataSyncからのグローバルなゲームステート変更をハンドル
    private void HandleGameSystemStateChanged(GameState newState)
    {
        // サーバーとクライアントの両方でフラグを更新
        _isGamePlaying = (newState == GameState.Playing);

        // BGMの制御
        if (GameSceneBGMManager.Instance != null)
        {
            if (newState == GameState.Playing)
            {
                // gameBGMが設定されているか確認してから再生
                if (GameSceneBGMManager.Instance.gameBGM != null)
                {
                    GameSceneBGMManager.Instance.PlayBGM(GameSceneBGMManager.Instance.gameBGM);
                }
                else
                {
                    Debug.LogWarning("GameSceneBGMManagerにgameBGMが設定されていません。");
                }
            }
            else if (newState == GameState.PostGame)
            {
                GameSceneBGMManager.Instance.StopBGM();
            }
        }

        if (isServer)
        {
            Debug.Log($"Server detected GameState change to {newState}. _isGamePlaying is now {_isGamePlaying}");
            if (newState == GameState.PostGame)
            {
                // サーバー側でゲーム終了処理
            }
        }
    }

    #endregion

    #region Public Server Methods (called via Commands from player)

    // ★ プレイヤー名を設定するサーバーメソッド
    [Server]
    public void ServerSetPlayerName(int playerIndex, string name)
    {
        int index = playerIndex - 1;
        if (index < 0 || index >= playerData.Count) return;

        PlayerData data = playerData[index];
        data.playerName = name;
        playerData[index] = data;
        Debug.Log($"Player {playerIndex}'s name set to {name}");
    }

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
    public void ServerApplyPoisonToOpponent(int attackerPlayerIndex, float amount)
    {
        NetworkPlayerInput opponentNPI = GetOpponentNPI(attackerPlayerIndex);
        if (opponentNPI == null) return;

        int targetIndex = opponentNPI.playerIndex - 1;
        if (targetIndex < 0 || targetIndex >= playerData.Count || playerData[targetIndex].isGameOver) return;

        PlayerData data = playerData[targetIndex];
        data.currentOxygen = Mathf.Max(0, data.currentOxygen - Mathf.Abs(amount));
        playerData[targetIndex] = data;

        // 相手のクライアントで毒エフェクトを再生
        opponentNPI.RpcPlayDebuffEffect(ItemEffectType.Poison);
    }

    [Server]
    public void ServerStunOpponent(int attackerPlayerIndex, float duration)
    {
        NetworkPlayerInput opponentNPI = GetOpponentNPI(attackerPlayerIndex);
        if (opponentNPI == null) return;

        // 相手をスタンさせ、クライアントでエフェクトを再生
        opponentNPI.RpcStunPlayer(duration);
        opponentNPI.RpcPlayDebuffEffect(ItemEffectType.Thunder);
    }

    [Server]
    public void ServerPlaceUnchiOnOpponent(int attackerPlayerIndex)
    {
        NetworkPlayerInput opponentNPI = GetOpponentNPI(attackerPlayerIndex);
        if (opponentNPI == null) return;

        opponentNPI.RpcPlaceUnchiTile();
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

        // コルーチン終了時にプレイヤーデータがまだ存在し、ゲームオーバーでないことを確認
        if (index < playerData.Count && !playerData[index].isGameOver)
        {
            data = playerData[index];
            data.isOxygenInvincible = false;
            playerData[index] = data;
        }
    }

    #endregion

    #region Helper Methods

    [Server]
    private NetworkPlayerInput GetOpponentNPI(int attackerPlayerIndex)
    {
        int targetPlayerIndex = (attackerPlayerIndex == 1) ? 2 : 1;

        foreach (var conn in NetworkServer.connections.Values)
        {
            NetworkPlayerInput npi = conn.identity.GetComponent<NetworkPlayerInput>();
            if (npi != null && npi.playerIndex == targetPlayerIndex)
            {
                return npi;
            }
        }
        return null;
    }

    #endregion
}
