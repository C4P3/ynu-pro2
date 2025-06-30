using UnityEngine;
using UnityEngine.UI; // Imageを扱うために必要

/// <summary>
/// 体力低下時の赤い画面エフェクトを管理するクラス
/// </summary>
public class RedPanelManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("体力が低下したときに赤く点滅させるUI Image")]
    [SerializeField] private Image _lowHealthEffectPanel;

    [Header("Effect Settings")]
    [Tooltip("この酸素量の割合以下になったらエフェクトを開始します")]
    [SerializeField, Range(0, 1)] private float _healthThreshold = 0.3f;

    [Tooltip("エフェクトの最大アルファ値（色の濃さ）")]
    [SerializeField, Range(0, 1)] private float _maxAlpha = 0.5f;

    void OnEnable()
    {
        // GameManagerのイベントに、自身のメソッドを登録
        GameManager.OnOxygenChanged += UpdateLowHealthEffect;
    }

    void OnDisable()
    {
        // オブジェクトが無効になるときに、登録を解除（メモリリーク防止）
        GameManager.OnOxygenChanged -= UpdateLowHealthEffect;
    }

    /// <summary>
    /// GameManagerから酸素量の変更通知を受け取って実行されるメソッド
    /// </summary>
    /// <param name="currentOxygen">現在の酸素量</param>
    /// <param name="maxOxygen">最大の酸素量</param>
    private void UpdateLowHealthEffect(float currentOxygen, float maxOxygen)
    {
        if (_lowHealthEffectPanel == null) return;

        // 酸素量の割合を計算
        float oxygenRatio = currentOxygen / maxOxygen;

        // 指定した閾値を下回っているかチェック
        if (oxygenRatio < _healthThreshold)
        {
            // 閾値を下回っている場合、体力に応じてアルファ値を計算
            // 体力が0に近づくほど、色が濃くなるようにマッピング
            float effectStrength = 1f - (oxygenRatio / _healthThreshold);
            float targetAlpha = Mathf.Lerp(0, _maxAlpha, effectStrength);

            // パネルの色を設定
            var color = _lowHealthEffectPanel.color;
            color.a = targetAlpha;
            _lowHealthEffectPanel.color = color;
        }
        else
        {
            // 閾値より多い場合は、パネルを透明にする
            var color = _lowHealthEffectPanel.color;
            color.a = 0;
            _lowHealthEffectPanel.color = color;
        }
    }
}