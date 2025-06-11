using UnityEngine;

[CreateAssetMenu(fileName = "NewBombItem", menuName = "TypingDriller/Bomb Item Data")]
public class BombItemData : ItemData
{
    [Header("Bomb Settings")]
    public int radius; // 爆発する範囲（半径）

    private void OnValidate()
    {
    effectType = ItemEffectType.Bomb;
    }
}