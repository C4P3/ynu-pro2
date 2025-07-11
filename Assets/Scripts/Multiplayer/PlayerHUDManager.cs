
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;
using System;

/// <summary>
/// マルチプレイヤー時のHUD（ヘッズアップディスプレイ）とリザルトUIを管理するクラス。
/// GameManagerMultiから同期されたデータをUIに反映させる責務を持つ。
/// </summary>
public class PlayerHUDManager : NetworkBehaviour
{
    [Header("Common UI")]
    [SerializeField] private TextMeshProUGUI matchTimeText;
    [SerializeField] private GameObject inGameHUDPanel; // 対戦中のUIパネル

    [Header("My Player UI")]
    [SerializeField] private Slider myOxygenSlider;
    [SerializeField] private TextMeshProUGUI myOxygenText;

    [Header("Opponent Player UI")]
    [SerializeField] private Slider opponentOxygenSlider;
    [SerializeField] private TextMeshProUGUI opponentOxygenText;

    [Header("Result Panel")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TextMeshProUGUI resultText; // WIN, LOSE, DRAW
    [SerializeField] private TextMeshProUGUI finalMatchTimeText;
    // My final stats
    [SerializeField] private TextMeshProUGUI myFinalScoreText;
    [SerializeField] private TextMeshProUGUI myFinalBlocksDestroyedText;
    [SerializeField] private TextMeshProUGUI myFinalMissTypesText;
    // Opponent's final stats
    [SerializeField] private TextMeshProUGUI opponentFinalScoreText;
    [SerializeField] private TextMeshProUGUI opponentFinalBlocksDestroyedText;
    [SerializeField] private TextMeshProUGUI opponentFinalMissTypesText;
    
    [Header("Oxygen Bar Colors")]
    [SerializeField] private Color fullOxygenColor = Color.green;
    [SerializeField] private Color lowOxygenColor = Color.yellow;
    [SerializeField] private Color criticalOxygenColor = Color.red;

    private NetworkPlayerInput _localPlayerInput;
    private int _myPlayerIndex = -1;
    private int _opponentPlayerIndex = -1;

    void Start()
    {
        // 初期状態ではすべてのUIを非表示にする
        if (inGameHUDPanel != null) inGameHUDPanel.SetActive(false);
        if (resultPanel != null) resultPanel.SetActive(false);
    }

    void Update()
    {
        // GameManagerとローカルプレイヤーが準備できるまで待つ
        if (GameManagerMulti.Instance == null) return;
        if (_localPlayerInput == null)
        {
            if (NetworkClient.localPlayer != null)
            {
                _localPlayerInput = NetworkClient.localPlayer.GetComponent<NetworkPlayerInput>();
                if (_localPlayerInput != null && _localPlayerInput.playerIndex != 0)
                {
                    // playerIndexが割り当てられるまで待つ
                    _myPlayerIndex = _localPlayerInput.playerIndex - 1; // 0-based index
                    _opponentPlayerIndex = (_myPlayerIndex == 0) ? 1 : 0;
                }
            }
            return; // 次のフレームで再試行
        }

        var gm = GameManagerMulti.Instance;
        switch (gm.currentMatchState)
        {
            case MatchState.WaitingForPlayers:
                // 待機中はUIを非表示
                if (inGameHUDPanel.activeSelf) inGameHUDPanel.SetActive(false);
                if (resultPanel.activeSelf) resultPanel.SetActive(false);
                break;

            case MatchState.Playing:
                // 対戦が始まったらHUDを表示
                if (!inGameHUDPanel.activeSelf) inGameHUDPanel.SetActive(true);
                if (resultPanel.activeSelf) resultPanel.SetActive(false);
                UpdateInGameHUD(gm);
                break;

            case MatchState.Finished:
                // 対戦が終了したらリザルトを表示
                if (inGameHUDPanel.activeSelf) inGameHUDPanel.SetActive(false);
                if (!resultPanel.activeSelf)
                {
                    ShowResultPanel(gm);
                }
                break;
        }
    }

    /// <summary>
    /// 対戦中のHUD情報を更新する
    /// </summary>
    private void UpdateInGameHUD(GameManagerMulti gm)
    {
        // 対戦時間を更新
        if (matchTimeText != null)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(gm.matchTime);
            matchTimeText.text = $"TIME {timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
        }

        // プレイヤーデータが準備できていなければ何もしない
        if (gm.playerData.Count < 2 || _myPlayerIndex == -1) return;

        // 自分と相手のUIを更新
        UpdateSinglePlayerUI(myOxygenSlider, myOxygenText, gm.playerData[_myPlayerIndex]);
        UpdateSinglePlayerUI(opponentOxygenSlider, opponentOxygenText, gm.playerData[_opponentPlayerIndex]);
    }

    /// <summary>
    /// 一人のプレイヤーの酸素UIを更新するヘルパーメソッド
    /// </summary>
    private void UpdateSinglePlayerUI(Slider slider, TextMeshProUGUI oxygenText, PlayerData data)
    {
        if (slider != null)
        {
            float maxOxygen = GameManagerMulti.Instance.maxOxygen;
            slider.value = data.currentOxygen / maxOxygen;
            Image fillImage = slider.fillRect.GetComponent<Image>();
            if (fillImage != null)
            {
                float oxygenPercentage = data.currentOxygen / maxOxygen;
                if (oxygenPercentage <= 0.10f) fillImage.color = criticalOxygenColor;
                else if (oxygenPercentage <= 0.30f) fillImage.color = lowOxygenColor;
                else fillImage.color = fullOxygenColor;
            }
        }
        if (oxygenText != null)
        {
            oxygenText.text = $"{Mathf.CeilToInt(data.currentOxygen)}";
        }
    }

    /// <summary>
    /// リザルトパネルを表示し、最終結果を反映する
    /// </summary>
    private void ShowResultPanel(GameManagerMulti gm)
    {
        if (resultPanel == null) return;
        resultPanel.SetActive(true);

        // 勝敗テキストを設定
        if (resultText != null)
        {
            if (gm.winnerIndex == _myPlayerIndex) resultText.text = "WIN!";
            else if (gm.winnerIndex == -2) resultText.text = "DRAW";
            else resultText.text = "LOSE...";
        }

        // 最終対戦時間を設定
        if (finalMatchTimeText != null)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(gm.matchTime);
            finalMatchTimeText.text = $"Match Time: {timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}.{timeSpan.Milliseconds / 10:D2}";
        }

        // プレイヤーデータが準備できていなければ何もしない
        if (gm.playerData.Count < 2 || _myPlayerIndex == -1) return;
        
        // 自分と相手の最終ステータスを表示
        DisplayFinalStats(myFinalScoreText, myFinalBlocksDestroyedText, myFinalMissTypesText, gm.playerData[_myPlayerIndex], gm.matchTime);
        DisplayFinalStats(opponentFinalScoreText, opponentFinalBlocksDestroyedText, opponentFinalMissTypesText, gm.playerData[_opponentPlayerIndex], gm.matchTime);
    }

    /// <summary>
    /// 一人のプレイヤーの最終スタッツをUIに表示するヘルパーメソッド
    /// </summary>
    private void DisplayFinalStats(TextMeshProUGUI scoreText, TextMeshProUGUI blocksText, TextMeshProUGUI missText, PlayerData data, float time)
    {
        int score = Mathf.FloorToInt(time) + data.blocksDestroyed - data.missTypes;
        score = Mathf.Max(0, score);

        if (scoreText != null) scoreText.text = $"Score: {score}";
        if (blocksText != null) blocksText.text = $"Blocks Destroyed: {data.blocksDestroyed}";
        if (missText != null) missText.text = $"Miss Types: {data.missTypes}";
    }
}
