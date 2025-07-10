# GEMINI.md

## 概要
このプロジェクトは、Unityで開発されたタイピング探索ゲームです。プレイヤーはタイピングによってキャラクターを操作し、ステージを探索します。シングルプレイモードと、PlayFabを利用したマルチプレイモードが実装されています。

## ゲームプレイ
- **タイピング**: ローマ字入力によるタイピングで、様々なアクションを行います。
- **探索**: ステージを探索し、アイテムを発見したり、ギミックを解除したりします。
- **アイテム**: ゲームを有利に進めるための様々なアイテムが存在します。（爆弾、酸素回復、毒、ロケット、スター、雷など）

## 主な機能
- **シングルプレイ**: 一人で遊ぶモードです。
- **マルチプレイ**: PlayFabを利用したオンラインマルチプレイに対応しており、ルーム作成や自動マッチングが可能です。
- **ランキング**: スコアなどを競うランキング機能があります。
- **チュートリアル**: ゲームの遊び方を学べるチュートorialがあります。

## 技術スタック
- **ゲームエンジン**: Unity
- **バックエンド**: PlayFab (認証、マッチメイキング)
- **言語**: C#

## 主要なスクリプトと連携

### マルチプレイの処理フロー

このゲームのマルチプレイは、**PlayFab (Lobby, Matchmaking)** と **Unity Relay (Mirror)** を組み合わせて実現されています。

1.  **認証 (PlayFabAuthManager)**
    *   ゲーム起動時に `PlayFabAuthManager` が `PlayFabClientAPI.LoginWithCustomID` を使用して、端末固有IDによる匿名認証を行います。
    *   これにより、各プレイヤーはPlayFab上で一意のエンティティとして認識されます。

2.  **ルーム作成 (ホスト側)**
    *   プレイヤーが「ホストになる」を選択すると、`PlayFabMatchmakingManager.CreateRoom()` が呼び出されます。
    *   `MyRelayNetworkManager.StartRelayHost()` が実行され、Unity Relayサーバー上でホストを開始します。
    *   成功すると、リレーサーバーへの接続情報（Join Code）が生成されます。
    *   `ShowJoinCodeCoroutine` でJoin Codeの生成を待ち、UIに表示します。

3.  **ルーム参加 (クライアント側)**
    *   別のプレイヤーがJoin Codeを入力して「参加する」を選択すると、`PlayFabMatchmakingManager.JoinRoom()` が呼び出されます。
    *   入力されたJoin Codeを `MyRelayNetworkManager.relayJoinCode` に設定し、`JoinRelayServer()` を呼び出してリレーサーバーにクライアントとして接続します。

4.  **プレイヤーオブジェクトの生成と同期**
    *   クライアントがサーバーに接続すると、サーバー側で `MyRelayNetworkManager.OnServerAddPlayer()` がトリガーされます。
    *   `base.OnServerAddPlayer()` によって、接続したクライアントに対応するプレイヤーオブジェクトがシーンに生成されます。
    *   生成されたプレイヤーオブジェクトの `NetworkPlayerInput` コンポーネントに、接続順に応じた `playerIndex` (1 or 2) を設定します。この `playerIndex` は `[SyncVar]` により全クライアントに同期されます。
    *   各クライアントの `NetworkPlayerInput.Start()` で、自身の `playerIndex` に応じて対応する`LevelManager` (Grid_P1 or Grid_P2) やカメラ (VCam1 or VCam2) などの参照を解決し、初期設定を行います。

5.  **ゲーム開始同期**
    *   `OnServerAddPlayer` 内で、接続プレイヤー数が2人になったことを検知すると、サーバーは `GameDataSync.Instance.StartGameSequence()` を呼び出します。
    *   `GameDataSync` は `[SyncVar]` でゲームの状態 (`GameState`) を管理するシングルトンです。
    *   `StartGameSequence` コルーチンにより、`GameState` が `WaitingForPlayers` -> `Countdown` -> `Playing` へと時間差で遷移します。
    *   `GameState` の変更は `[SyncVar]` の `hook` 機能により、全クライアントの `OnGameStateChanged` メソッドをトリガーします。
    *   `GameState` が `Countdown` になったタイミングで、各クライアントは `GenerateMapsWhenReady` コルーチンを開始し、同期されたシード値 (`mapSeed1`, `mapSeed2`) を使ってマップを生成します。これにより、全プレイヤーが同じマップでプレイできます。

6.  **ゲーム中の入力とアクションの同期**
    *   ローカルプレイヤーの入力は `NetworkPlayerInput.Update()` で検知されます。
    *   移動やタイピングなどのアクションが発生すると、`[Command]` 属性のついたメソッド（例: `CmdSendMoveInput`）を呼び出して、サーバーに処理を依頼します。
    *   サーバーは受け取ったコマンドを処理し、`[ClientRpc]` 属性のついたメソッド（例: `RpcReceiveMoveInput`）を呼び出して、全クライアントにその結果をブロードキャストします。
    *   各クライアントはRPCを受け取ると、対応する `PlayerController` のメソッドなどを実行し、キャラクターの移動やブロックの破壊といったアクションを同期させます。



- **`GameManager.cs`**:
    - ゲーム全体の進行管理（酸素、生存時間、スコア、ゲームオーバー処理）を行うシングルトン。
    - `PlayerController` から `RegisterLocalPlayer` を通じてプレイヤー情報を登録される。
    - `ItemManager` からアイテム効果（酸素回復、無敵化）の指示を受ける。
    - `TypingManager` からミスタイプの通知を受け取り、スコアに反映させる。
    - `UIController` に対して、ゲームオーバーUIの表示を指示する。

- **`PlayerController.cs`**:
    - プレイヤーの移動、状態（通常、移動中、タイピング中）を管理する。
    - `TypingManager` の `OnTypingEnded` イベントを購読し、タイピング完了後のブロック破壊や移動を制御する。
    - `LevelManager` と連携し、ブロックの破壊や生成を行う。
    - `ItemManager` の `AcquireItem` を呼び出し、アイテム取得処理を依頼する。
    - `AnimationManager` を通じて、プレイヤーのアニメーション（歩行、タイピング）を制御する。

- **`TypingManager.cs`**:
    - タイピングのロジック（入力判定、UI表示）を管理する。
    - `PlayerController` から `StartTyping` を呼び出され、タイピングを開始する。
    - タイピングが完了またはキャンセルされると `OnTypingEnded` イベントを発行し、`PlayerController` に通知する。
    - `GameManager` にミスタイプを通知する。

- **`ItemManager.cs`**:
    - アイテムのデータベースと効果の発動を管理するシングルトン。
    - `PlayerController` から `AcquireItem` を呼び出されると、アイテムの効果（`GameManager` への酸素回復指示、`LevelManager` へのブロック破壊指示など）を発動する。
    - `EffectManager` にエフェクト再生を依頼する。

- **`LevelManager.cs`**:
    - ステージ（タイルマップ）の生成と管理を行う。
    - `PlayerController` からの指示でブロックを破壊したり、プレイヤー周辺のチャンクを動的に生成したりする。
    - `ItemManager` から `GetRandomItemToSpawn` を呼び出し、配置するアイテムを決定する。

- **`PlayFabAuthManager.cs`**:
    - PlayFabへの匿名認証（カスタムID使用）を管理するシングルトン。
    - `UIController` と連携し、ログイン状況に応じてUIの表示を切り替える。

- **`PlayFabMatchmakingManager.cs`**:
    - PlayFabを利用したマッチメイキング（ルーム作成、参加）を管理するシングルトン。
    - `MyRelayNetworkManager` (Mirror) と連携し、リレーサーバーへの接続を制御する。
    - `UIController` と連携し、マッチング状況に応じてUIの表示を切り替える。

- **`UIController.cs`**:
    - ゲーム全体のUI（CanvasGroup）の表示/非表示を管理する。
    - `PlayFabAuthManager` やボタンのクリックイベントなどから呼び出され、画面遷移を実現する。