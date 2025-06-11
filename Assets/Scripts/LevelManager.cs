// LevelManager.cs
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class BlockType
{
    public string name;
    public TileBase tile;
    public float probabilityWeight = 1.0f;
}

// アイテムの出現設定を管理するクラス（ファイル先頭に追加）
[System.Serializable]
public class ItemSpawnSetting
{
    public ItemData itemData;
    [Tooltip("他のアイテムと比較したときの出現しやすさ")]
    public float spawnWeight;
}

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("References")]
    public Tilemap blockTilemap;
    public Tilemap itemTilemap; // ItemTilemapへの参照を追加
    public Transform playerTransform;

    [Header("Cluster Generation Settings")]
    public BlockType[] blockTypes;
    public float noiseScale = 0.1f;
    [Range(0, 1)]
    public float blockThreshold = 0.4f;

    [Header("Item Generation Settings")] // アイテム設定項目を追加
    [Tooltip("アイテムを配置する候補地ができる確率")]
    [Range(0, 1)]
    public float itemAreaChance = 0.02f;
    [Tooltip("生成するアイテムとその出現率のリスト")]
    public List<ItemSpawnSetting> itemSpawnSettings = new List<ItemSpawnSetting>();

    [Header("Internal Settings")]
    public int chunkSize = 16;
    public int generationRadiusInChunks = 2;

    private HashSet<Vector2Int> generatedChunks = new HashSet<Vector2Int>();
    private Vector2[] noiseOffsets;

    void Awake()
    {
        if (Instance == null) { Instance = this; } else { Destroy(gameObject); }
        noiseOffsets = new Vector2[blockTypes.Length];
        for (int i = 0; i < blockTypes.Length; i++)
        {
            noiseOffsets[i] = new Vector2(Random.Range(-10000f, 10000f), Random.Range(-10000f, 10000f));
        }
    }

    void Start()
    {
        CheckAndGenerateChunksAroundPlayer();
    }

    private void GenerateChunk(Vector2Int chunkPos)
    {
        int startX = chunkPos.x * chunkSize;
        int startY = chunkPos.y * chunkSize;

        for (int x = startX; x < startX + chunkSize; x++)
        {
            for (int y = startY; y < startY + chunkSize; y++)
            {
                Vector3Int tilePos = new Vector3Int(x, y, 0);
                if (blockTilemap.HasTile(tilePos) || itemTilemap.HasTile(tilePos)) continue;

                // --- アイテム用の空洞を先に決める ---
                if (Random.value < itemAreaChance)
                {
                    ItemData selectedItem = GetRandomItem();
                    if (selectedItem != null)
                    {
                        itemTilemap.SetTile(tilePos, selectedItem.itemTile);
                    }
                    continue; // アイテムを置いたらブロックは置かない
                }
                
                // (既存のブロック生成ロジックはここから)
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

    private ItemData GetRandomItem()
    {
        if (itemSpawnSettings.Count == 0) return null;
        float totalWeight = itemSpawnSettings.Sum(item => item.spawnWeight);
        if (totalWeight <= 0) return null;
        float randomValue = Random.Range(0, totalWeight);
        foreach (var setting in itemSpawnSettings)
        {
            if (randomValue < setting.spawnWeight) return setting.itemData;
            randomValue -= setting.spawnWeight;
        }
        return null;
    }
    
    // 爆発処理をLevelManagerに追加
    public void ExplodeBlocks(Vector3Int center, int radius)
    {
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                if(x*x + y*y > radius*radius) continue;
                Vector3Int pos = center + new Vector3Int(x, y, 0);
                blockTilemap.SetTile(pos, null);
                itemTilemap.SetTile(pos, null);
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
