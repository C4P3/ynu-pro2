using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;

// BlockTypeクラスは変更なしでOK
[System.Serializable]
public class BlockType
{
    public string name;
    public TileBase tile;
    [Tooltip("このブロックの出現しやすさ。値が大きいほど優先して選ばれやすくなる。")]
    public float probabilityWeight = 1.0f;
    // Min/Max Y は今回の要件では使わないが、将来のために残しておいても良い
}

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("References")]
    public Tilemap blockTilemap;
    public Transform playerTransform;

    [Header("Cluster Generation Settings")]
    [Tooltip("生成するブロックの種類と、その出現しやすさを設定します。")]
    public BlockType[] blockTypes;
    [Tooltip("値が小さいほど大きな塊に、大きいほど小さな塊になります。0.1前後がおすすめ。")]
    public float noiseScale = 0.1f;
    [Tooltip("この値よりノイズが大きい場所だけにブロックを生成します。値を上げると空間が増えます。")]
    [Range(0, 1)]
    public float blockThreshold = 0.4f;

    [Header("Item Area Settings")]
    [Tooltip("アイテムを置くための単独の空洞ができる確率。塊生成とは別に処理されます。")]
    [Range(0, 1)]
    public float itemAreaChance = 0.02f;

    [Header("Internal Settings")]
    [Tooltip("チャンクの1辺のタイル数")]
    public int chunkSize = 16;
    [Tooltip("プレイヤー中心に何チャンク先まで生成するか")]
    public int generationRadiusInChunks = 2;

    private HashSet<Vector2Int> generatedChunks = new HashSet<Vector2Int>();
    private Vector2[] noiseOffsets; // ブロックの種類ごとにノイズをずらすためのオフセット

    void Awake()
    {
        if (Instance == null) { Instance = this; } else { Destroy(gameObject); }

        // ブロックの種類ごとに異なるノイズを生成するため、ランダムなオフセットを最初に作っておく
        noiseOffsets = new Vector2[blockTypes.Length];
        for (int i = 0; i < blockTypes.Length; i++)
        {
            // 非常に大きな値でオフセットを作り、事実上別のノイズ関数のように見せる
            noiseOffsets[i] = new Vector2(Random.Range(-10000f, 10000f), Random.Range(-10000f, 10000f));
        }
    }

    void Start()
    {
        CheckAndGenerateChunksAroundPlayer();
    }
    
    /// <summary>
    /// 指定された座標のチャンク内に、パーリンノイズを使ってブロックの塊を生成する
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
                if (blockTilemap.HasTile(tilePos)) continue;

                // --- 1. アイテム用の空洞を先に決める ---
                if (Random.value < itemAreaChance)
                {
                    continue; // このマスは何もせず、空洞のままにする
                }

                // --- 2. パーリンノイズでどのブロックを置くか決める ---
                BlockType chosenBlock = null;
                float maxNoiseValue = -1f;

                // 全てのブロックタイプに対してノイズ値を計算
                for (int i = 0; i < blockTypes.Length; i++)
                {
                    // ノイズ計算用の座標を準備
                    float noiseX = (x + noiseOffsets[i].x) * noiseScale;
                    float noiseY = (y + noiseOffsets[i].y) * noiseScale;
                    
                    // パーリンノイズを計算し、出現しやすさで補正
                    float currentNoise = Mathf.PerlinNoise(noiseX, noiseY) + blockTypes[i].probabilityWeight;

                    // 最も値が大きいノイズを持つブロックを候補とする
                    if (currentNoise > maxNoiseValue)
                    {
                        maxNoiseValue = currentNoise;
                        chosenBlock = blockTypes[i];
                    }
                }

                // --- 3. ブロックを配置する ---
                // 最もノイズ値が大きかったブロックを、閾値を超えていれば配置
                // 閾値以下の場所は自然な形の空洞になる
                if (chosenBlock != null && maxNoiseValue > (blockThreshold + chosenBlock.probabilityWeight)) // probabilityWeightの分を閾値からも引く
                {
                    blockTilemap.SetTile(tilePos, chosenBlock.tile);
                }
            }
        }
    }
    
    // CheckAndGenerateChunksAroundPlayer, WorldToChunkPos, DestroyConnectedBlocks など、
    // 他のメソッドは前のバージョンから変更ありません。
    // ... (以下、変更のないメソッドは省略) ...
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
    private Vector2Int WorldToChunkPos(Vector3 worldPos)
    {
        Vector3Int gridPos = blockTilemap.WorldToCell(worldPos);
        int x = Mathf.FloorToInt((float)gridPos.x / chunkSize);
        int y = Mathf.FloorToInt((float)gridPos.y / chunkSize);
        return new Vector2Int(x, y);
    }


    /// <summary>
    /// 連結している同種のブロックをすべて破壊する（この部分は変更なし）
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

    private void CheckNeighbor(Vector3Int currentPos, Vector3Int direction, TileBase targetTile, Queue<Vector3Int> queue, HashSet<Vector3Int> blocksToDestroy)
    {
        Vector3Int neighborPos = currentPos + direction;
        if (blocksToDestroy.Contains(neighborPos)) return;
        if (blockTilemap.GetTile(neighborPos) != targetTile) return;
        queue.Enqueue(neighborPos);
        blocksToDestroy.Add(neighborPos);
    }
}
