// ItemManager.cs
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// アイテムの出現設定（データと出現確率）を管理するためのクラス
/// </summary>
[System.Serializable]
public class ItemSpawnSetting
{
    public ItemData itemData;
    [Tooltip("他のアイテムと比較したときの出現しやすさ")]
    public float spawnWeight;
}

/// <summary>
/// アイテムに関する全てのデータを管理し、効果を発動するクラス
/// </summary>
public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance { get; private set; }

    [Header("Item Database")]
    [Tooltip("ここですべてのアイテムの種類と出現率を設定します")]
    [SerializeField] private List<ItemSpawnSetting> _itemSpawnSettings;

    // タイルからItemDataを高速に逆引きするための辞書
    private Dictionary<TileBase, ItemData> _itemDatabase;

    // 効果音を再生するためのAudioSource
    private AudioSource _audioSource;

    void Awake()
    {
        // シングルトンパターンの実装
        if (Instance == null) { Instance = this; } else { Destroy(gameObject); }

        // AudioSourceの初期化
        _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.playOnAwake = false; // 自動再生を無効化
        // ゲーム開始時にデータベースを構築する
        BuildDatabase();
    }

    /// <summary>
    /// 設定リストから、タイルをキーとするデータベース（辞書）を作成する
    /// </summary>
    private void BuildDatabase()
    {
        _itemDatabase = new Dictionary<TileBase, ItemData>();
        foreach (var setting in _itemSpawnSettings)
        {
            if (setting.itemData != null && !_itemDatabase.ContainsKey(setting.itemData.itemTile))
            {
                _itemDatabase.Add(setting.itemData.itemTile, setting.itemData);
            }
        }
    }

    /// <summary>
    /// LevelManagerが呼び出すための公開メソッド。
    /// 重み付きランダムで配置すべきアイテムを1つ選んで返す。
    /// </summary>
    /// <param name="prng">使用する疑似乱数生成器</param> // ★★★ 引数を追加 ★★★
    public ItemData GetRandomItemToSpawn(System.Random prng)
    {
        if (_itemSpawnSettings.Count == 0) return null;
        float totalWeight = _itemSpawnSettings.Sum(item => item.spawnWeight);
        if (totalWeight <= 0) return null;

        // ★★★ UnityのRandomではなく、渡されたprngを使う ★★★
        float randomValue = (float)prng.NextDouble() * totalWeight;

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

    /// <summary>
    /// プレイヤーがアイテムを取得した時に呼ばれるメソッド
    /// </summary>
    /// <param name="itemTile">取得したアイテムのタイル</param>
    /// <param name="itemPosition">取得したアイテムのタイルマップ座標</param>
    /// <param name="levelManager">アイテムを取得したプレイヤーが所属するLevelManager</param> // 引数を追加
    public void AcquireItem(TileBase itemTile, Vector3Int itemPosition, LevelManager levelManager, Transform playerTransform)
    {
        // データベースに登録されていないタイルであれば何もしない
        if (!_itemDatabase.TryGetValue(itemTile, out ItemData data)) return;

        Debug.Log($"Acquired: {data.itemName}");

        // アイテムの効果音を再生
        if (data.useSound != null && _audioSource != null)
        {
            _audioSource.PlayOneShot(data.useSound);
        }

        // EffectManagerに、どのアイテムをどの場所で取得したかを伝え、エフェクト再生を依頼する
        if (EffectManager.Instance != null)
        {
            // 引数に levelManager.itemTilemap を渡す
            EffectManager.Instance.PlayItemAcquisitionEffect(data, itemPosition, levelManager.itemTilemap);
            
            if (data.followEffectPrefab != null)
            {
                // 引数に playerTransform を渡す
                EffectManager.Instance.PlayFollowEffect(data.followEffectPrefab, data.followEffectDuration, playerTransform);
            }
        }
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
                // Instanceではなく、引数で渡されたlevelManagerを使う
                if (bombData != null && levelManager != null)
                {
                    levelManager.ExplodeBlocks(itemPosition, bombData.radius);
                }
                break;

            case ItemEffectType.Star:
                var starData = data as StarItemData;
                // StarItemDataに無敵時間が設定されていれば、GameManagerに無敵化を依頼
                if (starData != null && GameManager.Instance != null)
                {
                    GameManager.Instance.StartCoroutine(
                        GameManager.Instance.TemporaryOxygenInvincibility(starData.invincibleDuration)
                    );
                }
                break;

            case ItemEffectType.Rocket:
                var rocketData = data as RocketItemData;
                if (rocketData != null && levelManager != null)
                {
                    // プレイヤーの向きをPlayerControllerから取得
                    PlayerController playerController = playerTransform.GetComponent<PlayerController>();
                    Vector3Int direction = Vector3Int.right; // デフォルト
                    if (playerController != null)
                    {
                        direction = playerController.GetLastMoveDirection();
                    }
                    rocketData.Activate(playerTransform, direction, levelManager.blockTilemap);
                }
                break;
        }
    }
}