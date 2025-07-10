using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Models
{
    public struct TypingText
    {
        public string title;    // 表示用の日本語
        public string hiragana; // タイピング判定用のひらがな
        public int textLength;  // ひらがなの文字数

        public TypingText(string title, string hiragana)
        {
            this.title = title;
            this.hiragana = hiragana;
            this.textLength = hiragana != null ? hiragana.Length : 0;
        }
    }

    public class TypingTextStore
    {
        private List<TypingText> _allTexts = new List<TypingText>();

        // 難易度設定: 0=初級, 1=中級, 2=上級
        public int levelSetting = 1;

        public TypingTextStore()
        {
            // ここではなにもしない
        }

        public void LoadFromCsv()
        {
            TextAsset csvFile = Resources.Load<TextAsset>("TypingTextStore");
            if (csvFile == null)
            {
                Debug.LogError("RypingTextStore.csvがResourcesに見つかりません。");
                return;
            }
            var lines = csvFile.text.Split('\n');
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var cols = line.Trim().Split(',');
                if (cols.Length < 2) continue;
                string title = cols[0].Trim();
                string hiragana = cols[1].Trim();
                _allTexts.Add(new TypingText(title, hiragana));
            }
        }

        /// <summary>
        /// 指定した塊サイズと現在の難易度に応じて、条件に合うランダムなテキストを返す
        /// </summary>
        public TypingText GetRandomTypingTextForCluster(int clusterSize)
        {
            int minLen = 1, maxLen = 99;
            switch (levelSetting)
            {
                case 0: // 初級
                    if (clusterSize <= 4)      { minLen = 2; maxLen = 4; }
                    else if (clusterSize <= 7) { minLen = 5; maxLen = 7; }
                    else                       { minLen = 8; maxLen = 10; }
                    break;
                case 1: // 中級
                    if (clusterSize <= 2)      { minLen = 2; maxLen = 4; }
                    else if (clusterSize <= 4) { minLen = 5; maxLen = 7; }
                    else if (clusterSize <= 7) { minLen = 8; maxLen = 10; }
                    else                       { minLen = 11; maxLen = 13; }
                    break;
                case 2: // 上級
                    if (clusterSize <= 2)      { minLen = 5; maxLen = 7; }
                    else if (clusterSize <= 4) { minLen = 8; maxLen = 10; }
                    else if (clusterSize <= 7) { minLen = 11; maxLen = 13; }
                    else                       { minLen = 14; maxLen = 16; }
                    break;
            }

            var candidates = _allTexts.Where(t => t.textLength >= minLen && t.textLength <= maxLen).ToList();
            if (candidates.Count == 0)
            {
                Debug.LogWarning($"条件に合うテキストがありません（level={levelSetting}, cluster={clusterSize}, len={minLen}-{maxLen}）");
                return default;
            }
            return candidates[UnityEngine.Random.Range(0, candidates.Count)];
        }
    }
}