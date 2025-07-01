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
    public void Activate(Vector3Int center, Tilemap blockTilemap, Tilemap itemTilemap)
    {
        float minDist = float.MaxValue;
        Vector3Int? nearestItemPos = null;

        // itemTilemapの全範囲を走査
        BoundsInt bounds = itemTilemap.cellBounds;
        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            var tile = itemTilemap.GetTile(pos);
            // 既にウンチタイルでないアイテムタイルのみ対象
            if (tile != null && tile != unchiTile)
            {
                float dist = (pos - center).sqrMagnitude;
                if (dist < minDist)
                {
                    minDist = dist;
                    nearestItemPos = pos;
                    Debug.Log($"最も近いアイテムタイル: {pos} 距離: {Mathf.Sqrt(minDist)}");
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
