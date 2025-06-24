// LevelManager.cs
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;

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
    public static LevelManager Instance { get; private set; }

    [Header("Cluster Generation Settings")]

    [Header("References")]
    public Tilemap blockTilemap;
    public Tilemap itemTilemap;
    public Transform playerTransform;

    [Header("Cluster Generation Settings")]
    public BlockType[] blockTypes;
    [Tooltip("値が小さいほど大きな塊に、大きいほど小さな塊になります。0.1前後がおすすめ。")]
    public float noiseScale = 0.1f;
    [Tooltip("この値よりノイズが大きい場所だけにブロックを生成します。値を上げると空間が増えます。")]
    [Range(0, 1)] public float blockThreshold = 0.4f;

    [Header("Item Generation Settings")]
    [Tooltip("アイテムを配置する候補地ができる確率")]
    [Range(0, 1)] public float itemAreaChance = 0.02f;

    [Header("Internal Settings")]
    public int chunkSize = 16;
    public int generationRadiusInChunks = 2;

    private HashSet<Vector2Int> generatedChunks = new HashSet<Vector2Int>();
    private Vector2[] noiseOffsets;

    private Vector3Int _playerStartPosition;

    void Awake()
    {
        if (Instance == null) { Instance = this; } else { Destroy(gameObject); }

        // ブロックの種類ごとに異なるノイズを生成するため、ランダムなオフセットを最初に作っておく
        noiseOffsets = new Vector2[blockTypes.Length];
        for (int i = 0; i < blockTypes.Length; i++)
        {
            noiseOffsets[i] = new Vector2(Random.Range(-10000f, 10000f), Random.Range(-10000f, 10000f));
        }
    }

    void Start()
    {
        // ★追加: ゲーム開始時のプレイヤー座標をグリッド座標として保存
        if(playerTransform != null)
        {
            _playerStartPosition = blockTilemap.WorldToCell(playerTransform.position);
        }
        
        // ゲーム開始時にプレイヤー周辺のチャンクを生成
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

                // ★追加: この場所がプレイヤーの初期位置の周囲1マス以内なら、何もせず次のループへ
                if (Mathf.Abs(tilePos.x - _playerStartPosition.x) <= 1 &&
                    Mathf.Abs(tilePos.y - _playerStartPosition.y) <= 1)
                {
                    continue;
                }

                // 既存のタイルがあればスキップ
                if (blockTilemap.HasTile(tilePos) || itemTilemap.HasTile(tilePos)) continue;

                // --- アイテム配置ロジック ---
                if (Random.value < itemAreaChance)
                {
                    if (ItemManager.Instance != null)
                    {
                        ItemData selectedItem = ItemManager.Instance.GetRandomItemToSpawn();
                        if (selectedItem != null)
                        {
                            itemTilemap.SetTile(tilePos, selectedItem.itemTile);
                        }
                    }
                    continue; // アイテムを置いたらブロックは置かない
                }
                
                // --- ブロック生成ロジック ---
                BlockType chosenBlock = null;
                float maxNoiseValue = -1f;
                for (int i = 0; i < blockTypes.Length; i++)
                {
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
    public void DestroyConnectedBlocks(Vector3Int startPos)
    {
        TileBase targetTile = blockTilemap.GetTile(startPos);
        if (targetTile == null) return;
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
            blockTilemap.SetTile(pos, null);
        }
    }
    
    /// <summary>
    /// 指定された中心と半径のブロックとアイテムを破壊する（爆弾用）
    /// </summary>
    public void ExplodeBlocks(Vector3Int center, int radius)
    {
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                if(x*x + y*y > radius*radius) continue; // 円形の範囲
                Vector3Int pos = center + new Vector3Int(x, y, 0);
                blockTilemap.SetTile(pos, null);
                itemTilemap.SetTile(pos, null);
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
}