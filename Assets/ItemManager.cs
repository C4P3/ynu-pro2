using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;

public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance { get; private set; }

    // アイテムに関する設定は全てここに集約する
    [SerializeField] private List<ItemSpawnSetting> _itemSpawnSettings;

    private Dictionary<TileBase, ItemData> _itemDatabase;

    void Awake()
    {
        if (Instance == null) { Instance = this; } else { Destroy(gameObject); }
        BuildDatabase();
    }

    private void BuildDatabase()
    {
        _itemDatabase = new Dictionary<TileBase, ItemData>();
    foreach (var setting in _itemSpawnSettings)
    {
    if (!_itemDatabase.ContainsKey(setting.itemData.itemTile))
    {
    _itemDatabase.Add(setting.itemData.itemTile, setting.itemData);
    }
    }
    }

    // ★新しく追加：LevelManagerが呼び出すための公開メソッド
    // 重み付きランダムで配置すべきアイテムを1つ選んで返す
    public ItemData GetRandomItemToSpawn()
    {
    if (_itemSpawnSettings.Count == 0) return null;

    float totalWeight = _itemSpawnSettings.Sum(item => item.spawnWeight);
    if (totalWeight <= 0) return null;

    float randomValue = Random.Range(0, totalWeight);
    foreach (var setting in _itemSpawnSettings)
    {
        if (randomValue < setting.spawnWeight)
        {
            return setting.itemData;
        }
        randomValue -= setting.spawnWeight;
    }
    return null;
    }

    // プレイヤーがアイテムを取得した時に呼ばれる
    public void AcquireItem(TileBase itemTile, Vector3Int itemPosition)
    {
        if (!_itemDatabase.TryGetValue(itemTile, out ItemData data)) return;

        Debug.Log($"Acquired: {data.itemName}");

        // アイテムの種類に応じて効果を発動
        switch (data.effectType)
        {
            case ItemEffectType.OxygenRecovery:
                var oxygenData = data as OxygenRecoveryItemData;
                if (oxygenData != null && GameManager.Instance != null)
                {
                    GameManager.Instance.RecoverOxygen(oxygenData.recoveryAmount);
                }
                break;

            case ItemEffectType.Bomb:
                var bombData = data as BombItemData;
                if (bombData != null && LevelManager.Instance != null)
                {
                    LevelManager.Instance.ExplodeBlocks(itemPosition, bombData.radius);
                }
                break;
        }
    }
}