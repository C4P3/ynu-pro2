// Multiplayer/GameDataSync.cs
using Mirror;
using UnityEngine;
using System.Collections;

public enum GameState
{
    WaitingForPlayers, // プレイヤー待機中
    Countdown,         // 開始前カウントダウン
    Playing,           // プレイ中
    PostGame           // ゲーム終了後（リザルト表示など）
}


public class GameDataSync : NetworkBehaviour
{
    // シングルトンとしてアクセスできるようにする
    public static GameDataSync Instance { get; private set; }

    [SyncVar]
    public long mapSeed1;

    [SyncVar]
    public long mapSeed2;

    // ★★★ 現在のゲーム状態を同期するSyncVarを追加 ★★★
    // hookを設定すると、値が変化した時に指定したメソッドが自動で呼ばれる
    [SyncVar(hook = nameof(OnGameStateChanged))]
    public GameState currentState = GameState.WaitingForPlayers;

    public override void OnStartServer()
    {
        // サーバー側でシングルトンを設定
        Instance = this;

        // サーバー起動時にシード値を設定
        mapSeed1 = System.DateTime.Now.Ticks;
        mapSeed2 = System.DateTime.Now.Ticks + 1;

        Debug.Log($"Seeds generated: Seed1={mapSeed1}, Seed2={mapSeed2}");
    }

    public override void OnStartClient()
    {
        // クライアント側でシングルトンを設定
        Instance = this;
    }

    [Server]
    public void StartGameSequence()
    {
        // 既にゲームが始まっている場合は何もしない
        if (currentState != GameState.WaitingForPlayers) return;

        StartCoroutine(GameSequenceCoroutine());
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

    // ★★★ currentStateの値が変化した時に全クライアントで呼ばれるフックメソッド ★★★
    private void OnGameStateChanged(GameState oldState, GameState newState)
    {
        Debug.Log($"Game state changed from {oldState} to {newState}");

        // ★★★ カウントダウン開始時に、責任をもってマップ生成を行う ★★★
        if (newState == GameState.Countdown)
        {
            StartCoroutine(GenerateMapsWhenReady());
            // ここでカウントダウンUIを表示するなどの処理も行える
        }
        else if (newState == GameState.Playing)
        {
            // カウントダウンUIを非表示にするなど
        }
        else if (newState == GameState.PostGame)
        {
            // 「You Win/Lose」UIを表示するなど
        }
    }
    // ★★★ コルーチンの名前と中身を以下のように変更 ★★★
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