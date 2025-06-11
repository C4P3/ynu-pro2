using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("References")]
    public Tilemap blockTilemap;
    public Transform playerTransform; // PlayerControllerではなくTransformを直接参照

    [Header("Level Generation")]
    public TileBase[] blockTiles;
    public int chunkSize = 16; // チャンクの1辺のタイル数
    public int generationRadiusInChunks = 2; // プレイヤー中心に何チャンク先まで生成するか

    // 生成済みのチャンク座標を記録するHashSet
    private HashSet<Vector2Int> generatedChunks = new HashSet<Vector2Int>();

    void Awake()
    {
        if (Instance == null) { Instance = this; } else { Destroy(gameObject); }
    }

    void Start()
    {
        // ゲーム開始時にプレイヤー周辺のチャンクを生成
        CheckAndGenerateChunksAroundPlayer();
    }

    /// <summary>
    /// プレイヤーの現在位置からチャンク座標を計算する
    /// </summary>
    private Vector2Int WorldToChunkPos(Vector3 worldPos)
    {
        Vector3Int gridPos = blockTilemap.WorldToCell(worldPos);
        int x = Mathf.FloorToInt((float)gridPos.x / chunkSize);
        int y = Mathf.FloorToInt((float)gridPos.y / chunkSize);
        return new Vector2Int(x, y);
    }

    /// <summary>
    /// プレイヤーの周囲のチャンクをチェックし、未生成なら生成する
    /// </summary>
    public void CheckAndGenerateChunksAroundPlayer()
    {
        if (playerTransform == null) return;

        Vector2Int playerChunkPos = WorldToChunkPos(playerTransform.position);

        // プレイヤー中心のチャンク範囲をループ
        for (int x = -generationRadiusInChunks; x <= generationRadiusInChunks; x++)
        {
            for (int y = -generationRadiusInChunks; y <= generationRadiusInChunks; y++)
            {
                Vector2Int targetChunkPos = new Vector2Int(playerChunkPos.x + x, playerChunkPos.y + y);

                // もし、そのチャンクがまだ生成されていなければ
                if (!generatedChunks.Contains(targetChunkPos))
                {
                    GenerateChunk(targetChunkPos);
                    // 生成済みとして記録
                    generatedChunks.Add(targetChunkPos);
                }
            }
        }
    }

    /// <summary>
    /// 指定された座標のチャンク内にブロックを敷き詰める
    /// </summary>
    private void GenerateChunk(Vector2Int chunkPos)
    {
        int startX = chunkPos.x * chunkSize;
        int startY = chunkPos.y * chunkSize;
        int endX = startX + chunkSize;
        int endY = startY + chunkSize;

        // チャンク内の全タイルをループ
        for (int x = startX; x < endX; x++)
        {
            for (int y = startY; y < endY; y++)
            {
                Vector3Int tilePos = new Vector3Int(x, y, 0);

                // ※重要※ 既存の破壊処理との干渉を避けるため、
                // 既に何らかのタイルがある場合は上書きしない
                if (blockTilemap.HasTile(tilePos)) continue;

                // プレイヤーの初期生成エリアなど、特定の場所を空けておくルールもここに追加できる
                // 例：ゲーム開始地点(0,0)周辺は生成しない
                if (Mathf.Abs(x) < 5 && y > -5)
                {
                    continue;
                }

                GenerateBlockAt(tilePos);
            }
        }
    }

    /// <summary>
    /// 指定した座標にランダムなブロックを1つ配置する
    /// </summary>
    private void GenerateBlockAt(Vector3Int position)
    {
        if (blockTiles.Length == 0) return;
        TileBase randomTile = blockTiles[Random.Range(0, blockTiles.Length)];
        blockTilemap.SetTile(position, randomTile);
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
