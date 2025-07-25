// Assets/Scripts/Multiplayer/GameDataSync.cs

using Mirror;
using UnityEngine;
using System.Collections;
using System;

public enum GameState
{
    WaitingForPlayers,
    Countdown,
    Playing,
    PostGame
}

public class GameDataSync : NetworkBehaviour
{
    public static GameDataSync Instance { get; private set; }

    [SyncVar] public long mapSeed1;
    [SyncVar] public long mapSeed2;

    [SyncVar(hook = nameof(OnGameStateChanged_Hook))]
    public GameState currentState = GameState.WaitingForPlayers;

    public static event Action<GameState> OnGameStateChanged_Client;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public override void OnStartServer()
    {
        // サーバー起動時にシード値を設定
        mapSeed1 = System.DateTime.Now.Ticks;
        mapSeed2 = System.DateTime.Now.Ticks + 1;

        Debug.Log($"Seeds generated: Seed1={mapSeed1}, Seed2={mapSeed2}");
    }

    [Server]
    public void StartGameSequence()
    {
        // 既にゲームが始まっている場合は何もしない
        if (currentState != GameState.WaitingForPlayers) return;

        StartCoroutine(GameSequenceCoroutine());
    }
    
    [Server]
    public void EndGame()
    {
        if (currentState == GameState.Playing)
        {
            currentState = GameState.PostGame;
            Debug.Log("Game state: PostGame");
        }
    }


    private IEnumerator GameSequenceCoroutine()
    {
        // カウントダウン状態に移行
        currentState = GameState.Countdown;
        Debug.Log("Game state: Countdown");

        // 3秒待機
        yield return new WaitForSeconds(3f);

        // プレイ中状態に移行
        currentState = GameState.Playing;
        Debug.Log("Game state: Playing");
    }

    // SyncVarのフックメソッド（名前を_Hookに変更して意図を明確化）
    private void OnGameStateChanged_Hook(GameState oldState, GameState newState)
    {
        Debug.Log($"Game state changed from {oldState} to {newState}");

        // 全クライアントでイベントを発行
        OnGameStateChanged_Client?.Invoke(newState);

        // Countdownになったらマップ生成を開始するロジックはそのまま
        if (newState == GameState.Countdown)
        {
            StartCoroutine(GenerateMapsWhenReady());
        }
    }

    private int _readyPlayerCount = 0;

    [Server]
    public void PlayerReady()
    {
        _readyPlayerCount++;
        Debug.Log($"Ready player count: {_readyPlayerCount}");
        // 2人揃ったらマップ生成を開始
        if (_readyPlayerCount == 2)
        {
            StartCoroutine(GenerateMapsWhenReady());
        }
    }

    private IEnumerator GenerateMapsWhenReady()
    {
        Debug.Log("Both players are ready! Generating maps.");

        // LevelManagerの取得は念のため残しておく
        LevelManager levelManager1 = GameObject.Find("Grid_P1")?.GetComponent<LevelManager>();
        LevelManager levelManager2 = GameObject.Find("Grid_P2")?.GetComponent<LevelManager>();

        if (levelManager1 != null && levelManager2 != null)
        {
            levelManager1.GenerateMap();
            levelManager2.GenerateMap();
        }
        else
        {
            Debug.LogError("Could not find one or both LevelManagers during map generation.");
        }
        yield return null;
    }
}