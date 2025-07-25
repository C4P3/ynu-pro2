using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Item/RocketItemData")]
public class RocketItemData : ItemData
{
    [Header("ロケットアイテムの設定")]
    // 変更点: ビームエフェクト用のプレハブを別途定義します。
    // acquisitionEffectPrefabは取得時にその場で再生されるエフェクト、
    // beamEffectPrefabは進行方向に発射されるエフェクトとして使い分けます。
    public GameObject beamEffectPrefab; 

    /// <summary>
    /// ロケットアイテムの効果を発動する
    /// </summary>
    public void Activate(Transform player, Vector3Int direction, Tilemap blockTilemap)
    {
        // プレイヤーの現在位置をグリッド座標に変換
        Vector3Int startPos = blockTilemap.WorldToCell(player.position);

        // 10マス分、指定方向にブロックを破壊
        for (int i = 1; i <= 10; i++)
        {
            Vector3Int targetPos = startPos + direction * i;
            if (blockTilemap.HasTile(targetPos))
            {
                blockTilemap.SetTile(targetPos, null);
            }
        }
        // // エフェクト再生（任意）
        // if (effectPrefab != null)
        // {
        //     GameObject.Instantiate(effectPrefab, player.position, Quaternion.identity);
        // }
    }
}