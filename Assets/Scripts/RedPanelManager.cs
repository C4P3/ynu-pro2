using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// 体力低下時の赤い画面エフェクトを管理するクラス
/// 体力の割合に応じて2段階で点滅速度が変化する
/// </summary>
public class RedPanelManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("体力が低下したときに赤く点滅させるUI Image")]
    [SerializeField] private Image _lowHealthEffectPanel;

    [Header("Effect Settings")]
    [Tooltip("点滅時のパネルのアルファ値（色の濃さ）")]
    [SerializeField, Range(0, 1)] private float _blinkAlpha = 0.5f;

    [Header("Danger Zone (少し危険)")]
    [Tooltip("この酸素量の割合以下で点滅を開始します")]
    [SerializeField, Range(0, 1)] private float _dangerThreshold = 0.3f;
    [Tooltip("少し危険な状態のときの点滅間隔")]
    [SerializeField] private float _dangerBlinkInterval = 0.5f;

    [Header("Critical Zone (非常に危険)")]
    [Tooltip("この酸素量の割合以下で点滅が速くなります")]
    [SerializeField, Range(0, 1)] private float _criticalThreshold = 0.1f;
    [Tooltip("非常に危険な状態のときの点滅間隔")]
    [SerializeField] private float _criticalBlinkInterval = 0.3f;

    private Coroutine _blinkingCoroutine;
    // 現在の点滅間隔を記録する変数。0は点滅していない状態を示す
    private float _currentBlinkInterval = 0f;

    void OnEnable()
    {
        GameManager.OnOxygenChanged += UpdateLowHealthEffect;
    }

    void OnDisable()
    {
        GameManager.OnOxygenChanged -= UpdateLowHealthEffect;
        if (_blinkingCoroutine != null)
        {
            StopCoroutine(_blinkingCoroutine);
        }
    }

    private void UpdateLowHealthEffect(float currentOxygen, float maxOxygen)
    {
        if (_lowHealthEffectPanel == null) return;

        float oxygenRatio = currentOxygen / maxOxygen;
        float targetInterval = 0f;

        // 1. 現在の体力割合から、目標とすべき点滅間隔を決定する
        if (oxygenRatio <= _criticalThreshold)
        {
            targetInterval = _criticalBlinkInterval; // 非常に危険
        }
        else if (oxygenRatio <= _dangerThreshold)
        {
            targetInterval = _dangerBlinkInterval; // 少し危険
        }
        else
        {
            targetInterval = 0f; // 安全
        }

        // 2. 目標の間隔と現在の間隔が違う場合のみ、処理を更新する
        if (targetInterval != _currentBlinkInterval)
        {
            // 既存のコルーチンがあれば停止
            if (_blinkingCoroutine != null)
            {
                StopCoroutine(_blinkingCoroutine);
                _blinkingCoroutine = null;
            }

            // 新しい間隔が0より大きい（点滅すべき）なら、新しいコルーチンを開始
            if (targetInterval > 0)
            {
                _blinkingCoroutine = StartCoroutine(BlinkEffectCoroutine(targetInterval));
            }
            else
            {
                // 点滅を停止し、パネルを非表示にする
                var color = _lowHealthEffectPanel.color;
                color.a = 0;
                _lowHealthEffectPanel.color = color;
            }

            // 3. 現在の状態を更新
            _currentBlinkInterval = targetInterval;
        }
    }

    private IEnumerator BlinkEffectCoroutine(float interval)
    {
        while (true)
        {
            var color = _lowHealthEffectPanel.color;
            color.a = _blinkAlpha;
            _lowHealthEffectPanel.color = color;

            yield return new WaitForSeconds(interval);

            color.a = 0;
            _lowHealthEffectPanel.color = color;

            yield return new WaitForSeconds(interval);
        }
    }
}