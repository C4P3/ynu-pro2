# ゲーム設計のまとめ（2025年6月版）

これまでの開発で構築してきた、ミスタードリラー風タイピングゲームの全体設計について解説します。現在のアーキテクチャは、各クラスの役割が明確に分離されており、将来的な機能追加にも対応しやすい、堅牢な構造になっています。

---

### 全体像と設計思想

このプロジェクトは、**各クラスが自身の役割に集中する「単一責任の原則」**と、**クラス間の直接的な命令を減らす「イベント駆動」**の考え方を重視しています。

- **Singletonパターン:** `GameManager`、`LevelManager`、`ItemManager` など、シーンに必ず一つしか存在しない管理クラスはシングルトンとし、他のクラスから `Instance` を通じて簡単にアクセスできるようにしています。
- **状態管理:** `PlayerController` は、`PlayerState` という「状態」を持つことで、自身の行動（移動できる、タイピング中など）を自律的に決定します。
- **イベント駆動:** `TypingManager` は、タイピングが完了しても `PlayerController` に直接命令しません。「終わりました」というイベント（合図）を送るだけです。それを受け取った `PlayerController` が、次の行動を判断します。これにより、お互いの依存度が下がり、コードが安定します。
- **データとロジックの分離:** `ItemData` のように、アイテムの性能をデータ（ScriptableObject）として分離することで、プログラマーでなくてもアイテムのバランス調整が容易になっています。

---

### 主要クラスの役割と責務

#### 1. `GameManager`
- **役割:** ゲームのグローバルな状態管理者。
- **責務:**
  - 酸素ゲージの管理（時間経過による減少、アイテムによる回復）。
  - ゲームオーバーの判定。
  - （将来的には）スコアやゲーム全体の進行管理。

#### 2. `LevelManager`
- **役割:** ステージの「建築家」。
- **責務:**
  - パーリンノイズを利用した、ブロックの塊の自動生成。
  - `ItemManager` に問い合わせて、アイテムをタイルマップ上に配置。
  - プレイヤーの初期地点に安全な空洞を確保。
  - ブロックの破壊処理（連結破壊、範囲破壊）。

#### 3. `PlayerController`
- **役割:** プレイヤー自身の「頭脳」。
- **責務:**
  - `PlayerState`（状態）の管理。
  - `Shift + WASD` による移動入力の受付。
  - 自身の状態に基づき、移動、タイピング開始、アイテム取得などの行動を決定。
  - `TypingManager` からのイベントを受け取り、その後の行動（ブロック破壊の依頼、移動）を判断。

#### 4. `TypingManager`
- **役割:** タイピングミニゲームの「進行役」。
- **責務:**
  - タイピングUIの表示・非表示。
  - 問題文の選択と表示。
  - 文字入力の判定（Shiftキーによる大文字/小文字の問題に対応済み）。
  - タイピングの成功・キャンセルを**イベント（`OnTypingEnded`）**で通知。

#### 5. `ItemManager`
- **役割:** アイテムの「専門家」であり「データベース」。
- **責務:**
  - 全ての `ItemData` とその出現率を一元管理。
  - `LevelManager` からの問い合わせに応じて、生成すべきアイテムを返す。
  - `PlayerController` からアイテム取得の報告を受け、アイテムの効果（酸素回復、爆発など）を発動させる。

#### 6. `ItemData` (ScriptableObject)
- **役割:** アイテムの「設計図」。
- **責務:**
  - アイテムの名前、見た目、効果の種類、性能（回復量や爆発範囲など）といったデータを保持する。

---

### クラス図

各クラスの関係性を視覚的に表現した図です。

```mermaid
classDiagram
    class GameManager {
        +Instance
        +RecoverOxygen(float)
    }
    class LevelManager {
        +Instance
        +DestroyConnectedBlocks(Vector3Int)
        +ExplodeBlocks(Vector3Int, int)
    }
    class PlayerController {
        -PlayerState _currentState
        -HandleRoamingState()
        -HandleMovingState()
        -HandleTypingEnded(bool)
    }
    class TypingManager {
        +StartTyping(Vector3Int)
        +OnTypingEnded(bool) event
    }
    class ItemManager {
        +Instance
        +GetRandomItemToSpawn() : ItemData
        +AcquireItem(TileBase, Vector3Int)
    }
    class ItemData {
        <<ScriptableObject>>
        +itemName
        +effectType
        +itemTile
    }

    PlayerController --> TypingManager : "依頼(StartTyping)"
    TypingManager -->> PlayerController : "イベント通知(OnTypingEnded)"
    PlayerController --> LevelManager : "破壊依頼"
    
    ItemManager "1" -- "N" ItemData : "管理する"
    LevelManager --> ItemManager : "配置アイテムを問い合わせ"
    PlayerController --> ItemManager : "取得を報告"
    ItemManager --> GameManager : "酸素回復を依頼"
    ItemManager --> LevelManager : "爆破を依頼"
```

---

### 代表的な処理のフロー（タイピング成功時）

1. `PlayerController` が `Roaming` 状態のとき、`Shift + W` 入力を検知。
2. `PlayerController` は移動先にブロックがあることを確認。
3. `PlayerController` は自身の状態を `Typing` に変更し、対象ブロックの座標を記憶。
4. `PlayerController` が `TypingManager.StartTyping()` を呼び出し、タイピング開始を依頼。
5. `TypingManager` はUIを表示し、入力を処理。プレイヤーが正しく入力する。
6. `TypingManager` は `OnTypingEnded(true)` イベントを発行して「成功しました」とだけ通知。
7. イベントを待ち受けていた `PlayerController` が `HandleTypingEnded(true)` メソッドを実行。
8. `PlayerController` が `LevelManager.DestroyConnectedBlocks()` を呼び出し、記憶していた座標のブロック破壊を依頼。
9. `PlayerController` が `MoveTo()` を実行し、自身の状態を `Moving` に変更して移動を開始する。

この設計により、各クラスが自分の仕事に集中でき、非常に見通しの良い構造が実現できています。この安定した土台の上で、安心して新しいデザインや機能を追加していくことができます。
