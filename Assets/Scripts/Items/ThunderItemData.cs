using UnityEngine;

[CreateAssetMenu(menuName = "Item/ThunderItemData")]
public class ThunderItemData : ItemData
{
    [Header("雷アイテムの設定")]
    public float stunDuration = 5f; // 移動不能時間（秒）
}
