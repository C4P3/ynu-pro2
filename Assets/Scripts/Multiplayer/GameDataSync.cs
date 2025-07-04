// Multiplayer/GameDataSync.cs
using Mirror;
using UnityEngine;

public class GameDataSync : NetworkBehaviour
{
    // シングルトンとしてアクセスできるようにする
    public static GameDataSync Instance { get; private set; }

    [SyncVar]
    public long mapSeed1;
    
    [SyncVar]
    public long mapSeed2;

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
}