using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelGenerator : MonoBehaviour
{
    [Header("ステージ設定")]
    public int width = 8;  // ステージの幅 (COL_COUNT)
    public int height = 20; // ステージの高さ (ROW_COUNT)

    [Header("タイルマップとタイル")]
    public Tilemap blockTilemap;     // ブロックを配置するタイルマップ
    public Tile normalBlockTile;  // 通常ブロックのタイルアセット
    public Tile airCapsuleTile;   // エアカプセルのタイルアセット

    void Start()
    {
        GenerateLevel();
    }

    public void GenerateLevel()
    {
        // --- 1. 全体を通常ブロックで埋める ---
        // JavaScriptの for(let row = 1; ... に相当する処理
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // JavaScriptコードでは最初の行(row=0)を空けていたので、一番上(y=height-1)は置かない
                if (y == height - 1) continue;

                // UnityのTilemapの座標系に合わせて設定
                // Y座標はマイナスを付けると、配列の感覚（上から0, 1, 2...）と見た目が一致しやすい
                Vector3Int position = new Vector3Int(x, -y, 0);
                blockTilemap.SetTile(position, normalBlockTile);
            }
        }

        // --- 2. エアカプセルを配置する ---
        // JavaScriptの for(let row = 4; ... に相当する処理
        // こちらもUnityの座標系に合わせてyはマイナスで考える
        for (int y = 4; y < height; y++)
        {
            if (y > 5 && y % 4 == 0)
            {
                int x = Random.Range(0, width);

                Vector3Int airCapsulePos = new Vector3Int(x, -y, 0);

                // エアカプセルを配置
                blockTilemap.SetTile(airCapsulePos, airCapsuleTile);

                // JavaScriptの SetType0() に相当する処理
                // エアカプセルの周囲のブロックを消す（nullを設定するとタイルが消える）
                blockTilemap.SetTile(airCapsulePos + Vector3Int.up, null);    // 上
                blockTilemap.SetTile(airCapsulePos + Vector3Int.down, null);  // 下
                blockTilemap.SetTile(airCapsulePos + Vector3Int.left, null);  // 左
                blockTilemap.SetTile(airCapsulePos + Vector3Int.right, null); // 右
            }
        }
    }
}