using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

[CreateAssetMenu(menuName = "Item/UnchiItemData")]
public class UnchiItemData : ItemData
{
    [Header("ウンチアイテムの設定")]
    public TileBase unchiTile; // 設置するウンチタイル

    /// <summary>
    /// 最も近くにあるアイテムタイルをウンチタイルに変更する
    /// </summary>
    public void Activate(Vector3Int center, Tilemap blockTilemap, Tilemap itemTilemap)
    {
        float minDist = float.MaxValue;
        Vector3Int? nearestItemPos = null;

        // itemTilemapの全範囲を走査
        BoundsInt bounds = itemTilemap.cellBounds;
        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            if (itemTilemap.GetTile(pos) != null)
            {
                float dist = (pos - center).sqrMagnitude;
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
            itemTilemap.SetTile(nearestItemPos.Value, unchiTile);
        }
    }
}
