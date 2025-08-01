// LevelManager.cs
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEditor;

/// <summary>
/// ブロックの種類とその設定を管理するためのクラス
/// </summary>
[System.Serializable]
public class BlockType
{
    public string name;
    public TileBase tile;
    [Tooltip("このブロックの出現しやすさ。値が大きいほど優先して選ばれやすくなる。")]
    public float probabilityWeight = 1.0f;
}

/// <summary>
/// ステージ（チャンク、ブロック、アイテム）の生成と管理を行うクラス
/// </summary>
public class LevelManager : MonoBehaviour
{
    [Header("Generation Seed")]
    public long mapSeed;

    [Header("Cluster Generation Settings")]

    [Header("References")]
    public Tilemap blockTilemap;
    public Tilemap itemTilemap;
    public Transform playerTransform;

    [Header("Cluster Generation Settings")]
    public BlockType[] blockTypes;
    [Tooltip("値が小さいほど大きな塊に、大きいほど小さな塊になります。0.1前後がおすすめ。")]
    public float noiseScale = 0.4f;
    [Tooltip("この値よりノイズが大きい場所だけにブロックを生成します。値を上げると空間が増えます。")]
    [Range(0, 1)] public float blockThreshold = 0.6f;

    [Header("Item Generation Settings")]
    [Tooltip("アイテムを配置する候補地ができる確率")]
    [Range(0, 1)] public float itemAreaChance = 0.02f;

    [Header("Internal Settings")]
    public int chunkSize = 16;
    public int generationRadiusInChunks = 2;

    private HashSet<Vector2Int> generatedChunks = new HashSet<Vector2Int>();
    private Vector2[] noiseOffsets;

    private Vector3Int _playerStartPosition;
    private System.Random _prng;

    public UnchiItemData unchiItemData;

    public void GenerateMap()
    {
        Debug.Log($"--- Checking BlockTypes for {gameObject.name} ---", gameObject);
        if (blockTypes == null || blockTypes.Length == 0)
        {
            Debug.LogError("BlockTypes array is NULL or EMPTY!", gameObject);
            return; // 配列がなければ処理を中断
        }

        bool hasNullElement = false;
        for (int i = 0; i < blockTypes.Length; i++)
        {
            if (blockTypes[i] == null)
            {
                Debug.LogError($"BlockTypes Element {i} is NULL!", gameObject);
                hasNullElement = true;
            }
            else
            {
                Debug.Log($"BlockTypes Element {i}: {blockTypes[i].name}", gameObject);
            }
        }

        if (hasNullElement)
        {
            Debug.LogError("Aborting map generation due to NULL element in BlockTypes.", gameObject);
            return; // 空の要素があれば処理を中断
        }

        if (GameDataSync.Instance != null)
        {
            if (gameObject.name == "Grid_P1")
            {
                this.mapSeed = GameDataSync.Instance.mapSeed1;
            }
            else if (gameObject.name == "Grid_P2")
            {
                this.mapSeed = GameDataSync.Instance.mapSeed2;
            }
        }
        else
        {
            this.mapSeed = System.DateTime.Now.Ticks;
        }

        _prng = new System.Random((int)mapSeed);

        // 乱数生成器の初期化
        noiseOffsets = new Vector2[blockTypes.Length];
        for (int i = 0; i < blockTypes.Length; i++)
        {
            noiseOffsets[i] = new Vector2(_prng.Next(-10000, 10000), _prng.Next(-10000, 10000));
        }

        // 以前InitialGenerateにあったワールド生成処理もここに統合する
        if (playerTransform != null)
        {
            _playerStartPosition = blockTilemap.WorldToCell(playerTransform.position);
        }
        CheckAndGenerateChunksAroundPlayer();
    }

    /// <summary>
    /// プレイヤーの参照が設定された後に、初期チャンクを生成する
    /// </summary>
    public void InitialGenerate()
    {
        // プレイヤーのTransformが設定されていない場合は何もしない
        if (playerTransform != null)
        {
            _playerStartPosition = blockTilemap.WorldToCell(playerTransform.position);
        }

        CheckAndGenerateChunksAroundPlayer();
    }

    /// <summary>
    /// プレイヤーの周囲のチャンクをチェックし、未生成なら生成する
    /// </summary>
    public void CheckAndGenerateChunksAroundPlayer()
    {
        if (playerTransform == null) return;
        Vector2Int playerChunkPos = WorldToChunkPos(playerTransform.position);
        for (int x = -generationRadiusInChunks; x <= generationRadiusInChunks; x++)
        {
            for (int y = -generationRadiusInChunks; y <= generationRadiusInChunks; y++)
            {
                Vector2Int targetChunkPos = new Vector2Int(playerChunkPos.x + x, playerChunkPos.y + y);
                if (!generatedChunks.Contains(targetChunkPos))
                {
                    GenerateChunk(targetChunkPos);
                    generatedChunks.Add(targetChunkPos);
                }
            }
        }
    }

    /// <summary>
    /// 指定された座標のチャンク内に、パーリンノイズを使ってブロックの塊やアイテムを生成する
    /// </summary>
    private void GenerateChunk(Vector2Int chunkPos)
    {
        int startX = chunkPos.x * chunkSize;
        int startY = chunkPos.y * chunkSize;

        for (int x = startX; x < startX + chunkSize; x++)
        {
            for (int y = startY; y < startY + chunkSize; y++)
            {
                Vector3Int tilePos = new Vector3Int(x, y, 0);

                // この場所がプレイヤーの初期位置の周囲1マス以内なら、何もせず次のループへ
                if (Mathf.Abs(tilePos.x - _playerStartPosition.x) <= 1 &&
                    Mathf.Abs(tilePos.y - _playerStartPosition.y) <= 1)
                {
                    continue;
                }

                // 既存のタイルがあればスキップ
                if (blockTilemap.HasTile(tilePos) || itemTilemap.HasTile(tilePos)) continue;

                // --- アイテム生成ロジック ---
                if (_prng.NextDouble() < itemAreaChance)
                {
                    if (ItemManager.Instance != null)
                    {
                        // ランダムにアイテムを選ぶ
                        ItemData selectedItem = ItemManager.Instance.GetRandomItemToSpawn(_prng);
                        if (selectedItem != null)
                        {
                            itemTilemap.SetTile(tilePos, selectedItem.itemTile);
                        }
                    }
                    continue;
                }

                // --- ブロック生成ロジック ---
                BlockType chosenBlock = null;
                float maxNoiseValue = -1f;
                for (int i = 0; i < blockTypes.Length; i++)
                {
                    if (unchiItemData != null && blockTypes[i].tile == unchiItemData.unchiTile)
                    {
                        continue; // ウンチお邪魔タイルは初期生成しない
                    }

                    float noiseX = (x + noiseOffsets[i].x) * noiseScale;
                    float noiseY = (y + noiseOffsets[i].y) * noiseScale;
                    float currentNoise = Mathf.PerlinNoise(noiseX, noiseY) + blockTypes[i].probabilityWeight;
                    if (currentNoise > maxNoiseValue)
                    {
                        maxNoiseValue = currentNoise;
                        chosenBlock = blockTypes[i];
                    }
                }
                if (chosenBlock != null && maxNoiseValue > (blockThreshold + chosenBlock.probabilityWeight))
                {
                    blockTilemap.SetTile(tilePos, chosenBlock.tile);
                }
            }
        }
    }

    /// <summary>
    /// 連結している同種のブロックをすべて破壊する
    /// </summary>
    public void DestroyConnectedBlocks(Vector3Int startPos, NetworkPlayerInput networkPlayerInput = null)
    {
        TileBase targetTile = blockTilemap.GetTile(startPos);
        if (targetTile == null) return;

        // 破壊開始地点がウンチタイルなら破壊しない
        if (unchiItemData != null && targetTile == unchiItemData.unchiTile) return;

        HashSet<Vector3Int> blocksToDestroy = new HashSet<Vector3Int>();
        Queue<Vector3Int> queue = new Queue<Vector3Int>();
        queue.Enqueue(startPos);
        blocksToDestroy.Add(startPos);

        while (queue.Count > 0)
        {
            Vector3Int currentPos = queue.Dequeue();
            CheckNeighbor(currentPos, Vector3Int.up, targetTile, queue, blocksToDestroy);
            CheckNeighbor(currentPos, Vector3Int.down, targetTile, queue, blocksToDestroy);
            CheckNeighbor(currentPos, Vector3Int.left, targetTile, queue, blocksToDestroy);
            CheckNeighbor(currentPos, Vector3Int.right, targetTile, queue, blocksToDestroy);
        }

        foreach (Vector3Int pos in blocksToDestroy)
        {
            // 破壊対象のブロックがウンチならスキップ
            if (unchiItemData != null && blockTilemap.GetTile(pos) == unchiItemData.unchiTile) continue;
            
            if (networkPlayerInput != null)
            {
                // マルチプレイ時：クライアントにタイル削除を通知
                networkPlayerInput.RpcRemoveTile(pos, true);
            }
            // シングルプレイ、またはサーバー自身のタイルマップを更新
            blockTilemap.SetTile(pos, null);

            // ブロック破壊数を加算
            if (networkPlayerInput != null)
            {
                networkPlayerInput.CmdAddDestroyedBlock();
            }
            else if (GameManager.Instance != null)
            {
                GameManager.Instance.AddDestroyedBlock();
            }
        }
    }

    /// <summary>
    /// 指定された中心と半径のブロックとアイテムを破壊する（爆弾用）
    /// </summary>
    public void ExplodeBlocks(Vector3Int center, int radius, NetworkPlayerInput networkPlayerInput = null)
    {
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                if (x * x + y * y > radius * radius) continue; // 円形の範囲
                Vector3Int pos = center + new Vector3Int(x, y, 0);

                // ブロック破壊判定
                if (blockTilemap.HasTile(pos))
                {
                    if (unchiItemData != null && blockTilemap.GetTile(pos) == unchiItemData.unchiTile)
                    {
                        // ウンチタイルは破壊しない
                    }
                    else if (itemTilemap.HasTile(pos))
                    {
                        // アイテムがある場所のブロックは破壊しない
                    }
                    else
                    {
                        if (networkPlayerInput != null)
                        {
                            // マルチプレイ時：クライアントにタイル削除を通知
                            networkPlayerInput.RpcRemoveTile(pos, true);
                        }
                        // シングルプレイ、またはサーバー自身のタイルマップを更新
                        blockTilemap.SetTile(pos, null);

                        if (networkPlayerInput != null)
                        {
                            networkPlayerInput.CmdAddDestroyedBlock();
                        }
                        else if (GameManager.Instance != null)
                        {
                            GameManager.Instance.AddDestroyedBlock();
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// DestroyConnectedBlocksのヘルパー関数。隣接タイルをチェックする
    /// </summary>
    private void CheckNeighbor(Vector3Int currentPos, Vector3Int direction, TileBase targetTile, Queue<Vector3Int> queue, HashSet<Vector3Int> blocksToDestroy)
    {
        Vector3Int neighborPos = currentPos + direction;
        if (blocksToDestroy.Contains(neighborPos)) return;
        if (blockTilemap.GetTile(neighborPos) != targetTile) return;
        queue.Enqueue(neighborPos);
        blocksToDestroy.Add(neighborPos);
    }

    /// <summary>
    /// ワールド座標をチャンク座標に変換する
    /// </summary>
    private Vector2Int WorldToChunkPos(Vector3 worldPos)
    {
        Vector3Int gridPos = blockTilemap.WorldToCell(worldPos);
        int x = Mathf.FloorToInt((float)gridPos.x / chunkSize);
        int y = Mathf.FloorToInt((float)gridPos.y / chunkSize);
        return new Vector2Int(x, y);
    }

    public int GetConnectedBlockCount(Vector3Int startPos)
    {
        // 幅優先探索で連結ブロック数をカウント
        var visited = new HashSet<Vector3Int>();
        var queue = new Queue<Vector3Int>();
        TileBase targetTile = blockTilemap.GetTile(startPos);
        if (targetTile == null) return 0;
        queue.Enqueue(startPos);
        visited.Add(startPos);

        Vector3Int[] directions = { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right };
        while (queue.Count > 0)
        {
            var pos = queue.Dequeue();
            foreach (var dir in directions)
            {
                var next = pos + dir;
                if (!visited.Contains(next) && blockTilemap.GetTile(next) == targetTile)
                {
                    visited.Add(next);
                    queue.Enqueue(next);
                }
            }
        }
        return visited.Count;
    }
}