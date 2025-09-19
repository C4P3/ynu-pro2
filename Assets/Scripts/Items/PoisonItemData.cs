using UnityEngine;

[CreateAssetMenu(menuName = "Item/PoisonItemData")]
public class PoisonItemData : ItemData
{
    [Header("毒アイテムの設定")]
    public float poisonAmount = 10f; // 減らす酸素量
}
