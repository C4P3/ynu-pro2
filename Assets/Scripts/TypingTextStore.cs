namespace Models
{
    // TypingTextをstructで定義
    public struct TypingText
    {
        public string title;    // 表示用の日本語
        public string hiragana; // タイピング判定用のひらがな
    }

    public class TypingTextStore
    {
        private readonly System.Collections.Generic.List<TypingText> _typingTexts = new()
        {
            new TypingText { title = "犬も歩けば棒に当たる", hiragana = "いぬもあるけばぼうにあたる" },
            new TypingText { title = "論より証拠", hiragana = "ろんよりしょうこ" },
            new TypingText { title = "花より団子", hiragana = "はなよりだんご" },
            new TypingText { title = "需要と供給", hiragana = "じゅようときょうきゅう" },
            new TypingText { title = "七福神", hiragana = "しちふくじん" },
            new TypingText { title = "モッツァレラチーズ", hiragana = "もっつぁれらちーず" },
        };

        public TypingText RandomTypingText =>
            _typingTexts[UnityEngine.Random.Range(0, _typingTexts.Count)];
    }
}