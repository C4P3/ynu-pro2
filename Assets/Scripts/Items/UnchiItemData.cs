using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Item/UnchiItemData")]
public class UnchiItemData : ItemData
{
    [Header("ウンチアイテムの設定")]
    public TileBase unchiTile; // 設置するウンチタイル

    /// <summary>
    /// 最も近くにあるアイテムタイルをウンチタイルに変更する
    /// </summary>
    public void Activate(Vector3Int playerGridCenter, Tilemap blockTilemap, Tilemap itemTilemap)
    {
        float minDist = float.MaxValue;
        Vector3Int? nearestItemPos = null;

        // itemTilemapの全範囲を走査
        BoundsInt bounds = itemTilemap.cellBounds;
        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            // プレイヤーの現在位置のタイルは対象外にする
            if (pos == playerGridCenter) continue; // プレイヤーの足元のタイルはスキップ
            var tile = itemTilemap.GetTile(pos);

            // 既にウンチタイルでないアイテムタイルのみ対象
            if (tile != null && tile != unchiTile)
            {
                float dist = (pos - playerGridCenter).sqrMagnitude;
                if (dist < minDist)
                {
                    minDist = dist;
                    nearestItemPos = pos;
                }
            }
        }
        // 最も近いアイテムタイルをウンチタイルに変更
        if (nearestItemPos.HasValue)
        {
            itemTilemap.SetTile(nearestItemPos.Value, null); // 既存のアイテムをitemTilemapから消す
            blockTilemap.SetTile(nearestItemPos.Value, unchiTile); // うんちお邪魔タイルをblockTilemapに設置
        }
    }
}
