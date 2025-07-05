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
    private System.Random _prng;

    public UnchiItemData unchiItemData; // Inspectorで「うんち」アセットをセット

    void Awake()
    {
    }

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

        if(hasNullElement)
        {
            Debug.LogError("Aborting map generation due to NULL element in BlockTypes.", gameObject);
            return; // 空の要素があれば処理を中断
        }
        // ★★★ デバッグここまで ★★★
    
        // 以前Awakeにあったコードをここに移動
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
        
        // ★★★ prngではなく_prngを使うように変更 ★★★
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

    void Start()
    {
        // ★★★ このメソッド内の処理を、下のInitialGenerate()に移動します ★★★
        // このメソッドは空にするか、Awakeから移動してきたノイズ設定だけを残します。
    }

    // ★★★ 新しい公開メソッドを追加 ★★★
    /// <summary>
    /// プレイヤーの参照が設定された後に、初期チャンクを生成する
    /// </summary>
    public void InitialGenerate()
    {
        // ★★★ 以下の2行を追加 ★★★
        // Debug.Log($"InitialGenerate called for {gameObject.name}", gameObject);
        // Debug.Log($"Player Transform is: {(playerTransform != null ? playerTransform.name : "NULL")}", gameObject);

        // Start()から移動してきたコード
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

                // ★追加: この場所がプレイヤーの初期位置の周囲1マス以内なら、何もせず次のループへ
                if (Mathf.Abs(tilePos.x - _playerStartPosition.x) <= 1 &&
                    Mathf.Abs(tilePos.y - _playerStartPosition.y) <= 1)
                {
                    continue;
                }

                // 既存のタイルがあればスキップ
                if (blockTilemap.HasTile(tilePos) || itemTilemap.HasTile(tilePos)) continue;

                // --- アイテム配置ロジック ---
                // ★★★ UnityのRandom.valueではなく、_prng.NextDouble()を使う ★★★
                if (_prng.NextDouble() < itemAreaChance)
                {
                    if (ItemManager.Instance != null)
                    {
                        // ★★★ ItemManagerに、保持している_prngを渡す ★★★
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
            // 例：DestroyConnectedBlocksやExplodeBlocksなどのSetTile(null)前に
            if (blockTilemap.GetTile(pos) == unchiItemData.unchiTile) continue;
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
                // 例：DestroyConnectedBlocksやExplodeBlocksなどのSetTile(null)前に
                if (blockTilemap.GetTile(pos) == unchiItemData.unchiTile) continue;
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