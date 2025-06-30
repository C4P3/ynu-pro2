// EffectManager.cs
using UnityEngine;
using UnityEngine.Tilemaps; // Tilemapクラスを利用するために追加
using System.Collections; // コルーチンを使うために追加
/// <summary>
/// ゲーム内のエフェクト（パーティクルなど）の再生と管理を行うクラス
/// </summary>
public class EffectManager : MonoBehaviour
{
    // シングルトンパターンの実装
    public static EffectManager Instance { get; private set; }

    [Header("References")]
    [Tooltip("エフェクトの座標変換の基準となるタイルマップ。LevelManagerのBlockTilemapなどを設定してください。")]
    public Tilemap referenceTilemap; // ワールド座標への変換に利用する

    [Tooltip("エフェクトを追従させる対象のTransform。通常はプレイヤーを設定します。")]
    public Transform followTarget; // エフェクトが追従する対象

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// アイテムデータとグリッド座標から、アイテム取得エフェクトを再生する
    /// </summary>
    /// <param name="itemData">取得されたアイテムのデータ</param>
    /// <param name="gridPosition">アイテムが存在したグリッド座標</param>
    public void PlayItemAcquisitionEffect(ItemData itemData, Vector3Int gridPosition)
    {
        // ItemDataまたは、その中にエフェクトプレハブが設定されていなければ何もしない
        if (itemData == null || itemData.acquisitionEffectPrefab == null)
        {
            return;
        }

        // 座標変換の基準となるタイルマップが未設定の場合は警告を出す
        if (referenceTilemap == null)
        {
            Debug.LogWarning("EffectManagerにreferenceTilemapが設定されていません。エフェクトは原点に表示されます。");
            // 基準タイルマップがなくても、とりあえず原点でエフェクトを再生する
            PlayEffect(itemData.acquisitionEffectPrefab, Vector3.zero);
            return;
        }

        // グリッド座標を、そのセルの中央のワールド座標に変換する
        Vector3 worldPosition = referenceTilemap.GetCellCenterWorld(gridPosition);

        // 既存のPlayEffectメソッドを呼び出して、指定の座標でエフェクトを再生
        PlayEffect(itemData.acquisitionEffectPrefab, worldPosition);
    }

    /// <summary>
    /// 指定されたプレハブからエフェクトを生成し、指定の位置で再生する
    /// </summary>
    /// <param name="effectPrefab">再生するエフェクトのGameObjectプレハブ</param>
    /// <param name="position">エフェクトを再生するワールド座標</param>
    public void PlayEffect(GameObject effectPrefab, Vector3 position)
    {
        if (effectPrefab == null)
        {
            Debug.LogWarning("PlayEffect was called with a null prefab.");
            return;
        }

        // 生成したエフェクトのインスタンスを保持する
        GameObject effectInstance = Instantiate(effectPrefab, position, Quaternion.identity);

        // 生成したエフェクトにParticleSystemがついているかチェック
        ParticleSystem ps = effectInstance.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            // ParticleSystemの再生が終了した後にオブジェクトを破棄する
            // ps.main.durationだけだと、パーティクルの生存時間(startLifetime)が考慮されないため、
            // durationとstartLifetimeの最大値を取ることで、おおよその終了時間を担保します。
            // これでも消えない場合は、パーティクルプレハブのStopActionを"Destroy"に設定するのが最も確実です。
            float lifeTime = Mathf.Max(ps.main.duration, ps.main.startLifetime.constantMax);
            Destroy(effectInstance, lifeTime);
        }
        else
        {
            // パーティクルシステムがない場合、5秒後に消去する（保険）
            Destroy(effectInstance, 5f);
            Debug.LogWarning($"The effect '{effectInstance.name}' does not have a ParticleSystem component. It will be destroyed in 5 seconds.");
        }
    }
    /// <summary>
    /// 指定された対象に追従するエフェクトを一定時間再生します。
    /// </summary>
    /// <param name="effectPrefab">再生するエフェクトのプレハブ</param>
    /// <param name="duration">再生時間（秒）</param>
    public void PlayFollowEffect(GameObject effectPrefab, float duration)
    {
        if (effectPrefab == null)
        {
            Debug.LogWarning("PlayFollowEffect was called with a null prefab.");
            return;
        }
        // 追従対象が設定されていない場合は警告を出し、処理を中断
        if (followTarget == null)
        {
            Debug.LogWarning("EffectManagerにFollow Targetが設定されていません。追従エフェクトを再生できません。");
            return;
        }

        // 追従と時間経過後の破棄を行うコルーチンを開始
        StartCoroutine(FollowAndDestroyCoroutine(effectPrefab, duration));
    }

    /// <summary>
    /// エフェクトを追従させ、指定時間後に破棄するコルーチン
    /// </summary>
    private IEnumerator FollowAndDestroyCoroutine(GameObject effectPrefab, float duration)
    {
        // 追従対象の子オブジェクトとしてエフェクトを生成。これにより、対象の移動に自動で追従する。
        GameObject effectInstance = Instantiate(effectPrefab, followTarget.position, Quaternion.identity, followTarget);

        // 指定された時間だけ待機
        yield return new WaitForSeconds(duration);

        // 待機後、エフェクトオブジェクトがまだ存在していれば（何らかの理由で先に破棄されていないか確認）破棄する
        if (effectInstance != null)
        {
            Destroy(effectInstance);
        }
    }
}