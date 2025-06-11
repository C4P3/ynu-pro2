using UnityEngine;
using UnityEngine.Tilemaps;

// アイテムの種類を判別しやすくするためのenum（列挙型）
public enum ItemEffectType
{
    OxygenRecovery,
    Bomb,
    // 将来のマルチプレイ用
    // OpponentDebuff _LockBlocks,
    // OpponentDebuff _NarrowVision
}

// [CreateAssetMenu] を使うと、Unityエディタの右クリックメニューからこのデータアセットを作成できるようになる
[CreateAssetMenu(fileName = "NewItemData", menuName = "TypingDriller/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Common Info")]
    public string itemName;
    public ItemEffectType effectType; // このアイテムがどの種類の効果を持つか
    public TileBase itemTile; // タイルマップに表示されるときのタイル
}