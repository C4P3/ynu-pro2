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
            new TypingText { title = "情報工学概論", hiragana = "じょうほうこうがくがいろん" },
            new TypingText { title = "システムエンジニアリング", hiragana = "しすてむえんじにありんぐ" },
            new TypingText { title = "コンピュータシステムとコミュニケーション", hiragana = "こんぴゅーたしすてむとこみゅにけーしょん" },
            new TypingText { title = "情報リテラシ", hiragana = "じょうほうりてらし" },
            new TypingText { title = "プログラミング入門", hiragana = "ぷろぐらみんぐにゅうもん" },
            new TypingText { title = "線形代数学", hiragana = "せんけいだいすうがく" },
            new TypingText { title = "解析学", hiragana = "かいせきがく" },
            new TypingText { title = "離散数学", hiragana = "りさんすうがく" },
            new TypingText { title = "基礎化学", hiragana = "きそかがく" },
            new TypingText { title = "基礎力学", hiragana = "きそりきがく" },
            new TypingText { title = "微分方程式", hiragana = "びぶんほうていしき" },
            new TypingText { title = "確率統計", hiragana = "かくりつとうけい" },
            new TypingText { title = "関数論", hiragana = "かんすうろん" },
            new TypingText { title = "基礎解析力学", hiragana = "きそかいせきりきがく" },
            new TypingText { title = "量子力学", hiragana = "りょうしりきがく" },
            new TypingText { title = "基礎熱力学", hiragana = "きそねつりきがく" },
            new TypingText { title = "データサイエンス実践基礎", hiragana = "でーたさいえんすじっせんきそ" },
            new TypingText { title = "材料有機化学", hiragana = "ざいりょうゆうきかがく" },
            new TypingText { title = "材料無機化学", hiragana = "ざいりょうむきかがく" },
            new TypingText { title = "数値解析", hiragana = "すうちかいせき" },
            new TypingText { title = "応用数学演習", hiragana = "おうようすうがくえんしゅう" },
            new TypingText { title = "計測", hiragana = "けいそく" },
            new TypingText { title = "連続体力学", hiragana = "れんぞくたいりきがく" },
            new TypingText { title = "移動および速度論", hiragana = "いどうおよびそくどろん" },
            new TypingText { title = "計算機アーキテクチャ", hiragana = "けいさんきあーきてくちゃ" },
            new TypingText { title = "アルゴリズムとデータ構造", hiragana = "あるごりずむとでーたこうぞう" },
            new TypingText { title = "プログラミング演習", hiragana = "ぷろぐらみんぐえんしゅう" },
            new TypingText { title = "プロジェクトラーニング", hiragana = "ぷろじぇくとらーにんぐ" },
            new TypingText { title = "電子情報工学実験", hiragana = "でんしじょうほうこうがくじっけん" },
            new TypingText { title = "情報工学特別演習", hiragana = "じょうほうこうがくとくべつえんしゅう" },
            new TypingText { title = "卒業研究", hiragana = "そつぎょうけんきゅう" },
            new TypingText { title = "プログラミング", hiragana = "ぷろぐらみんぐ" },
            new TypingText { title = "論理回路", hiragana = "ろんりかいろ" },
            new TypingText { title = "コンピュータグラフィックス", hiragana = "こんぴゅーたぐらふぃっくす" },
            new TypingText { title = "マルチメディア情報処理", hiragana = "まるちめでぃあじょうほうしょり" },
            new TypingText { title = "応用数学", hiragana = "おうようすうがく" },
            new TypingText { title = "コンピュータネットワーク", hiragana = "こんぴゅーたねっとわーく" },
            new TypingText { title = "情報理論", hiragana = "じょうほうりろん" },
            new TypingText { title = "ことばと論理", hiragana = "ことばとろんり" },
            new TypingText { title = "プログラミング言語", hiragana = "ぷろぐらみんぐげんご" },
            new TypingText { title = "システムプログラム", hiragana = "しすてむぷろぐらむ" },
            new TypingText { title = "計算理論", hiragana = "けいさんりろん" },
            new TypingText { title = "人工知能", hiragana = "じんこうちのう" },
            new TypingText { title = "ディジタル信号処理", hiragana = "でぃじたるしんごうしょり" },
            new TypingText { title = "基礎制御理論", hiragana = "きそせいぎょりろん" },
            new TypingText { title = "コンパイラ", hiragana = "こんぱいら" },
            new TypingText { title = "情報物理セキュリティ", hiragana = "じょうほうぶつりせきゅりてぃ" },
            new TypingText { title = "計算機シミュレーション", hiragana = "けいさんきしみゅれーしょん" },
            new TypingText { title = "理論言語学", hiragana = "りろんげんごがく" },
            new TypingText { title = "データベース", hiragana = "でーたべーす" },
            new TypingText { title = "ソフトコンピューティング", hiragana = "そふとこんぴゅーてぃんぐ" },
            new TypingText { title = "感覚知覚システム論", hiragana = "かんかくちかくしすてむろん" },
            new TypingText { title = "画像音声情報処理", hiragana = "がぞうおんせいじょうほうしょり" },
            new TypingText { title = "暗号理論", hiragana = "あんごうりろん" },
            new TypingText { title = "自然言語処理", hiragana = "しぜんげんごしょり" },
            new TypingText { title = "情報社会倫理", hiragana = "じょうほうしゃかいりんり" },
            new TypingText { title = "システム最適化理論", hiragana = "しすてむさいてきかりろん" },
            new TypingText { title = "機械学習", hiragana = "きかいがくしゅう" },
            new TypingText { title = "サイバーフィジカルネットワークアーキテクチャ", hiragana = "さいばーふぃじかるねっとわーくあーきてくちゃ" },
            new TypingText { title = "総合応用工学概論", hiragana = "そうごうおうようこうがくがいろん" },
            new TypingText { title = "統計数理工学", hiragana = "とうけいすうりこうがく" },
            new TypingText { title = "先端電子情報工学", hiragana = "せんたんでんしじょうほうこうがく" },
            new TypingText { title = "医工学連携基礎", hiragana = "いこうがくれんけいきそ" },
        };

        public TypingText RandomTypingText =>
            _typingTexts[UnityEngine.Random.Range(0, _typingTexts.Count)];
    }
}