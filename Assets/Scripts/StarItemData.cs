using UnityEngine;

[CreateAssetMenu(menuName = "Item/StarItemData")]
public class StarItemData : ItemData
{
    [Tooltip("無敵時間（秒）")]
    public float invincibleDuration = 5f;

    private void OnValidate()
    {
        effectType = ItemEffectType.Star;
    }
}