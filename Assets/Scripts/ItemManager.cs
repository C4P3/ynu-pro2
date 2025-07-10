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

        // 0からtotalWeightまでのランダムな値を生成
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
    // 引数に GameManagerMulti を追加
    public void AcquireItem(TileBase itemTile, Vector3Int itemPosition, LevelManager levelManager, Transform playerTransform)
    {
        if (!_itemDatabase.TryGetValue(itemTile, out ItemData data)) return;

        Debug.Log($"Acquired: {data.itemName}");

        levelManager.itemTilemap.SetTile(itemPosition, null);

        if (data.useSound != null && _audioSource != null)
        {
            _audioSource.PlayOneShot(data.useSound);
        }

        if (EffectManager.Instance != null)
        {
            EffectManager.Instance.PlayItemAcquisitionEffect(data, itemPosition, levelManager.itemTilemap);
            if (data.followEffectPrefab != null)
            {
                EffectManager.Instance.PlayFollowEffect(data.followEffectPrefab, data.followEffectDuration, playerTransform);
            }
        }

        // アイテムの種類に応じて効果を発動 (GameManager.InstanceをplayerGameManagerに置き換え)
        switch (data.effectType)
        {
            case ItemEffectType.OxygenRecovery:
                var oxygenData = data as OxygenRecoveryItemData;
                if (oxygenData != null)
                {
                    PlayerController playerController = playerTransform.GetComponent<PlayerController>();
                    if (playerController != null && GameManagerMulti.Instance != null)
                    {
                        GameManagerMulti.Instance.RecoverOxygen(playerController.playerIndex, oxygenData.recoveryAmount);
                    }
                }
                break;

            case ItemEffectType.Bomb:
                var bombData = data as BombItemData;
                if (bombData != null && levelManager != null)
                {
                    levelManager.ExplodeBlocks(itemPosition, bombData.radius);
                }
                break;

            case ItemEffectType.Star:
                var starData = data as StarItemData;
                if (starData != null)
                {
                    PlayerController playerController = playerTransform.GetComponent<PlayerController>();
                    if (playerController != null && GameManagerMulti.Instance != null)
                    {
                        GameManagerMulti.Instance.StartCoroutine(
                            GameManagerMulti.Instance.TemporaryOxygenInvincibility(playerController.playerIndex, starData.invincibleDuration)
                        );
                    }
                }
                break;

            case ItemEffectType.Rocket:
                var rocketData = data as RocketItemData;
                if (rocketData != null && levelManager != null)
                {
                    PlayerController playerController = playerTransform.GetComponent<PlayerController>();
                    Vector3Int direction = (playerController != null) ? playerController.GetLastMoveDirection() : Vector3Int.right;
                    if (EffectManager.Instance != null && rocketData.beamEffectPrefab != null)
                    {
                        EffectManager.Instance.PlayDirectionalEffect(rocketData.beamEffectPrefab, playerTransform.position, direction);
                    }
                    rocketData.Activate(playerTransform, direction, levelManager.blockTilemap);
                }
                break;

            case ItemEffectType.Unchi:
                var unchiData = data as UnchiItemData;
                if (unchiData != null && levelManager != null)
                {
                    Vector3Int playerGridCenter = levelManager.itemTilemap.WorldToCell(playerTransform.position);
                    unchiData.Activate(playerGridCenter, levelManager.blockTilemap, levelManager.itemTilemap);
                }
                break;

            case ItemEffectType.Poison:
                var poisonData = data as PoisonItemData;
                if (poisonData != null)
                {
                    PlayerController playerController = playerTransform.GetComponent<PlayerController>();
                    if (playerController != null && GameManagerMulti.Instance != null)
                    {
                        GameManagerMulti.Instance.RecoverOxygen(playerController.playerIndex, -Mathf.Abs(poisonData.poisonAmount));
                    }
                }
                break;

            case ItemEffectType.Thunder:
                var thunderData = data as ThunderItemData;
                if (thunderData != null && playerTransform != null)
                {
                    PlayerController playerController = playerTransform.GetComponent<PlayerController>();
                    if (playerController != null) playerController.Stun(thunderData.stunDuration);
                }
                break;
        }
    }
}