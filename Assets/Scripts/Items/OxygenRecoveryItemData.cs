using UnityEngine;

[CreateAssetMenu(fileName = "NewOxygenItem", menuName = "TypingDriller/Oxygen Recovery Item Data")]
public class OxygenRecoveryItemData : ItemData
{
    [Header("Oxygen Recovery Settings")]
    public float recoveryAmount; // 酸素の回復量
    
    private void OnValidate()
    {
        effectType = ItemEffectType.OxygenRecovery;
    }
}