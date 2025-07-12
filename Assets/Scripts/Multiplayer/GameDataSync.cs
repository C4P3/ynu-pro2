// Assets/Scripts/Multiplayer/GameDataSync.cs

using Mirror;
using UnityEngine;
using System.Collections;
using System; // Actionのために追加

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

    // ★★★ クライアント側で状態変化を購読するためのイベントを追加 ★★★
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
        // 1. カウントダウン状態に移行
        currentState = GameState.Countdown;
        Debug.Log("Game state: Countdown");

        // 2. 3秒待機
        yield return new WaitForSeconds(3f);

        // 3. プレイ中状態に移行
        currentState = GameState.Playing;
        Debug.Log("Game state: Playing");
    }

    // SyncVarのフックメソッド（名前を_Hookに変更して意図を明確化）
    private void OnGameStateChanged_Hook(GameState oldState, GameState newState)
    {
        Debug.Log($"Game state changed from {oldState} to {newState}");

        // ★★★ 全クライアントでイベントを発行 ★★★
        OnGameStateChanged_Client?.Invoke(newState);

        // Countdownになったらマップ生成を開始するロジックはそのまま
        if (newState == GameState.Countdown)
        {
            StartCoroutine(GenerateMapsWhenReady());
        }
    }

    private IEnumerator GenerateMapsWhenReady()
    {
        Debug.Log("Waiting for LevelManagers to be ready...");

        // まず、LevelManagerのオブジェクト自体が見つかるのを待つ
        LevelManager levelManager1 = null;
        LevelManager levelManager2 = null;
        while (levelManager1 == null || levelManager2 == null)
        {
            levelManager1 = GameObject.Find("Grid_P1")?.GetComponent<LevelManager>();
            levelManager2 = GameObject.Find("Grid_P2")?.GetComponent<LevelManager>();
            yield return null; // 1フレーム待って再試行
        }

        Debug.Log("Found both LevelManager objects. Now waiting for playerTransforms...");

        // ★★★ 本題：両方のLevelManagerのplayerTransformが設定されるまでループして待機 ★★★
        while (levelManager1.playerTransform == null || levelManager2.playerTransform == null)
        {
            // NetworkPlayerInputのStart()が実行され、transformが設定されるのを待つ
            yield return null; // 1フレーム待って、次のフレームで再度whileの条件をチェック
        }

        // ループを抜けたら、両方の準備が整ったということ
        Debug.Log("Both LevelManagers are ready! Generating maps.");

        // 両方の準備が整ったので、マップを生成する
        levelManager1.GenerateMap();
        levelManager2.GenerateMap();
    }
}