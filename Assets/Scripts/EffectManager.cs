// EffectManager.cs
using UnityEngine;

/// <summary>
/// ゲーム内のエフェクト（パーティクルなど）の再生と管理を行うクラス
/// </summary>
public class EffectManager : MonoBehaviour
{
    // シングルトンパターンの実装
    public static EffectManager Instance { get; private set; }

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

        // ★変更点: 生成したエフェクトのインスタンスを保持する
        GameObject effectInstance = Instantiate(effectPrefab, position, Quaternion.identity);

        // --- ★ここから追加 ---
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
        // --- ★ここまで追加 ---
    }
}