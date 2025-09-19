using UnityEngine;

[CreateAssetMenu(menuName = "Item/StarItemData")]
public class StarItemData : ItemData
{
    [Tooltip("無敵時間（秒）")]
    public float invincibleDuration = 10f;

    private void OnValidate()
    {
        effectType = ItemEffectType.Star;
    }
}