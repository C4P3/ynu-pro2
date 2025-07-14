using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using System.Linq; // ★ LINQを使うために追加

public class PlayFabLeaderboardManager : MonoBehaviour
{
    public static PlayFabLeaderboardManager Instance { get; private set; }

    [Header("Leaderboard Settings")]
    private const string LeaderboardName = "SinglePlayerScore";

    [Header("UI Prefab & Parent")]
    public GameObject rankingEntryPrefab; // ★ ランキング1行分のプレハブ
    public Transform rankingsParent;      // ★ 統一されたランキングの親(Content)

    [Header("UI Feedback")]
    public TextMeshProUGUI statusText;

    // ★ APIの結果を一時的に保持するリスト
    private List<PlayerLeaderboardEntry> _topPlayersResult;
    private List<PlayerLeaderboardEntry> _aroundPlayerResult;
    private int _pendingApiCallbacks;

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

    // このメソッドをランキングボタンのOnClickイベントに設定します
    public void OnRankingButtonPressed()
    {
        if (statusText != null) statusText.text = "ランキングを取得中...";

        // ★ 処理開始前にリセット
        _topPlayersResult = null;
        _aroundPlayerResult = null;
        _pendingApiCallbacks = 2; // 2つのAPI呼び出しを待つ

        // 既存のランキング表示をクリア
        foreach (Transform child in rankingsParent)
        {
            Destroy(child.gameObject);
        }

        GetTopLeaderboard();
        GetLeaderboardAroundPlayer();
    }

    private void GetTopLeaderboard()
    {
        var request = new GetLeaderboardRequest { StatisticName = LeaderboardName, StartPosition = 0, MaxResultsCount = 5 };
        PlayFabClientAPI.GetLeaderboard(request, OnGetTopLeaderboardSuccess, OnLeaderboardError);
    }

    private void OnGetTopLeaderboardSuccess(GetLeaderboardResult result)
    {
        _topPlayersResult = result.Leaderboard;
        _pendingApiCallbacks--;
        if (_pendingApiCallbacks == 0) ProcessCombinedLeaderboard();
    }

    private void GetLeaderboardAroundPlayer()
    {
        var request = new GetLeaderboardAroundPlayerRequest { StatisticName = LeaderboardName, MaxResultsCount = 5 };
        PlayFabClientAPI.GetLeaderboardAroundPlayer(request, OnGetLeaderboardAroundPlayerSuccess, OnLeaderboardError);
    }

    private void OnGetLeaderboardAroundPlayerSuccess(GetLeaderboardAroundPlayerResult result)
    {
        _aroundPlayerResult = result.Leaderboard;
        _pendingApiCallbacks--;
        if (_pendingApiCallbacks == 0) ProcessCombinedLeaderboard();
    }

    // ★ 2つのAPI呼び出しが完了した後に実行される
    private void ProcessCombinedLeaderboard()
    {
        if (statusText != null) statusText.text = ""; // ロード表示を消す

        if (_topPlayersResult == null || _aroundPlayerResult == null)
        {
            Debug.LogError("API results are not ready.");
            return;
        }

        // 1. 2つのリストを合体させ、PlayFabIdをキーにして重複を排除する
        var combined = new Dictionary<string, PlayerLeaderboardEntry>();
        foreach (var entry in _topPlayersResult)
        {
            combined[entry.PlayFabId] = entry;
        }
        foreach (var entry in _aroundPlayerResult)
        {
            combined[entry.PlayFabId] = entry;
        }

        // 2. 順位で並び替える
        List<PlayerLeaderboardEntry> sortedList = combined.Values.OrderBy(e => e.Position).ToList();

        // 3. 上位10件（もしくはそれ以下）をUIに表示する
        UpdateLeaderboardUI(sortedList);
    }

    private void UpdateLeaderboardUI(List<PlayerLeaderboardEntry> leaderboard)
    {
        if (rankingsParent == null) return;

        // 上位10件に絞る
        int displayCount = Mathf.Min(leaderboard.Count, 10);

        for(int i = 0; i < displayCount; i++)
        {
            var entry = leaderboard[i];
            GameObject newEntryObj = Instantiate(rankingEntryPrefab, rankingsParent);
            RankingEntry rankingEntry = newEntryObj.GetComponent<RankingEntry>();
            
            if(rankingEntry != null)
            {
                string displayName = string.IsNullOrEmpty(entry.DisplayName) ? entry.PlayFabId : entry.DisplayName;
                rankingEntry.SetData(entry.Position + 1, displayName, entry.StatValue);
            }
        }
    }
    
    private void OnLeaderboardError(PlayFabError error)
    {
        Debug.LogError("Leaderboard acquisition failed: " + error.GenerateErrorReport());
        if (statusText != null) statusText.text = "ランキングの取得に失敗しました。";
        _pendingApiCallbacks--; // エラー時もカウントを減らす
    }
}
