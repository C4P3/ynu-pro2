using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Item/RocketItemData")]
public class RocketItemData : ItemData
{
    [Header("ロケットアイテムの設定")]
    public GameObject effectPrefab; // 取得時エフェクト（任意）

    /// <summary>
    /// ロケットアイテムの効果を発動する
    /// </summary>
    /// <param name="player">プレイヤーのTransform</param>
    /// <param name="direction">プレイヤーの向き（Vector3Int）</param>
    /// <param name="blockTilemap">ブロックのTilemap</param>
    public void Activate(Transform player, Vector3Int direction, Tilemap blockTilemap)
    {
        // プレイヤーの現在位置をグリッド座標に変換
        Vector3Int startPos = blockTilemap.WorldToCell(player.position);

        // 7マス分、指定方向にブロックを破壊
        for (int i = 1; i <= 7; i++)
        {
            Vector3Int targetPos = startPos + direction * i;
            if (blockTilemap.HasTile(targetPos))
            {
                blockTilemap.SetTile(targetPos, null);
            }
        }
        Debug.Log("ロケットアイテム");

        // エフェクト再生（任意）
        if (effectPrefab != null)
        {
            GameObject.Instantiate(effectPrefab, player.position, Quaternion.identity);
        }
    }
}

// 例: Item取得処理
if (itemData is RocketItemData rocketItem)
{
    // プレイヤーのTransform, 向き, Tilemapを渡す
    rocketItem.Activate(player.transform, direction, blockTilemap);
}
