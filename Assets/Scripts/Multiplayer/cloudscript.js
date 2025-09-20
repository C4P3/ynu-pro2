// このハンドラは、クライアント(ホスト)から呼び出され、RelayのJoin Codeを共有します。
// サーバーサイドの権限で実行されるため、Title Entityへのデータ書き込みが可能です。
handlers.shareJoinCode = function (args, context) {
    // 1. クライアントから渡された引数を検証します。
    if (!args || !args.matchId || !args.joinCode) {
        throw "Invalid arguments: matchId and joinCode are required.";
    }

    var matchId = args.matchId;
    var joinCode = args.joinCode;
    
    // 2. entity.SetObjects APIを使用して、Title Entityにデータを書き込むリクエストを作成します。
    // PlayFab.settings.titleId を使うことで、このゲームのTitle Entityを正しく参照できます。
    var setObjectsRequest = {
        Entity: { Type: "title", Id: PlayFab.settings.titleId },
        Objects: [
            {
                ObjectName: matchId, // 各マッチを区別するために、オブジェクト名としてMatchIdを使用します。
                DataObject: {
                    "JoinCode": joinCode
                }
            }
        ]
    };

    // 3. APIを呼び出し、結果をログに出力します。
    try {
        var result = entity.SetObjects(setObjectsRequest);
        log.info("Successfully set JoinCode for match: " + matchId);
        return { success: true }; // 成功したことをクライアントに返します。
    } catch (e) {
        log.error("Failed to set objects for match: " + matchId, e);
        throw "Failed to set objects on behalf of title entity."; // エラーをクライアントに返します。
    }
};
