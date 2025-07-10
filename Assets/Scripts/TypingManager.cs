using UnityEngine;
using TMPro;
using Models;


/// タイピングのUI表示と入力判定を管理するクラス

public class TypingManager : MonoBehaviour
{
    [HideInInspector]
    public int playerIndex; // NetworkPlayerInputから設定される

    // タイピング終了時のイベント
    public event System.Action<bool> OnTypingEnded; 
    // UIの参照
    [Header("UI References")]
    public GameObject typingPanel;
    private TextMeshProUGUI _typedText;
    // タイピング用データ管理
    private TypingTextStore _typingTextStore;
    private CurrentTypingTextModel _typingModel = new CurrentTypingTextModel();
    private Vector3Int _initialMoveDirection;

    //効果音用AudioClipの追加
    [Header("Sound Effects")]
    [SerializeField] private AudioClip typingSound;
    [SerializeField] private AudioClip successSound;
    [SerializeField] private AudioClip missSound;

    private AudioSource audioSource;

    void Awake()
    {
        _typingTextStore = new TypingTextStore();
        _typingTextStore.LoadFromCsv(); // LoadFromCsvをpublicにする
    }

    void Start()
    {
        // パネルを非表示にしておく
        if (typingPanel != null)
        {
            typingPanel.SetActive(false);
        }
        _typedText = typingPanel.GetComponentInChildren<TextMeshProUGUI>();

        //AudioSourceの初期化
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        // OSを自動判定して設定
        #if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
            // Mac環境の場合
            _typingModel.SetOperatingSystemName(OperatingSystemName.Mac);
            Debug.Log("OS: Macに設定しました。");
        #elif UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            // Windows環境の場合
            _typingModel.SetOperatingSystemName(OperatingSystemName.Windows);
            Debug.Log("OS: Windowsに設定しました。");
        #else
            // その他の環境（Linuxなど）の場合、デフォルトとしてWindowsを設定
            _typingModel.SetOperatingSystemName(OperatingSystemName.Windows);
            Debug.Log("OS: その他（デフォルトでWindows）に設定しました。");
        #endif
    }

    // タイピン開始時の初期化処理
    public void StartTyping(Vector3Int moveDirection, int clusterSize)
    {
        _initialMoveDirection = moveDirection;
        // clusterSizeに応じてテキストを取得
        TypingText currentTypingText = _typingTextStore.GetRandomTypingTextForCluster(clusterSize);

        // ひらがなをローマ字に変換
        var converter = new ConvertHiraganaToRomanModel();
        var initialRomanChars = converter.ConvertHiraganaToRoman(currentTypingText.hiragana.ToCharArray());
        // モデルに情報をセット
        _typingModel.SetTitle(currentTypingText.title);
        _typingModel.SetCharacters(initialRomanChars);
        _typingModel.ResetCharactersIndex();
        // UIの更新
        UpdateTypedText();
        // パネルの表示
        if (typingPanel != null)
        {
            typingPanel.SetActive(true);
        }
    }

    void Update()
    {
        // パネルが非表示なら何も行わない
        if (typingPanel == null || !typingPanel.activeSelf) return;
        // シフト+移動キーでキャンセル
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            Vector3Int cancelMoveVec = Vector3Int.zero;
            if (Input.GetKeyDown(KeyCode.W)) cancelMoveVec = Vector3Int.up;
            if (Input.GetKeyDown(KeyCode.S)) cancelMoveVec = Vector3Int.down;
            if (Input.GetKeyDown(KeyCode.A)) cancelMoveVec = Vector3Int.left;
            if (Input.GetKeyDown(KeyCode.D)) cancelMoveVec = Vector3Int.right;

            if (cancelMoveVec != Vector3Int.zero && cancelMoveVec != _initialMoveDirection)
            {
                CancelTyping();
                return;
            }
        }
        // 入力文字を1文字ずつ判定
        if (!string.IsNullOrEmpty(Input.inputString))
        {
            foreach (char c in Input.inputString)
            {
                // アルファベット小文字とハイフン(-)を処理対象とする
                if ((c >= 'a' && c <= 'z') || c == '-')
                {
                    var result = _typingModel.TypeCharacter(c);

                    if (result == TypeResult.Incorrect)
                    {
                        PlaySound(missSound); // missSoundの再生
                        if (GameManagerMulti.Instance != null)
                        {
                            GameManagerMulti.Instance.AddMissType(playerIndex);
                        }
                    }
                    else if (result == TypeResult.Correct)
                    {
                        PlaySound(typingSound); // typingSoundの再生
                    }

                    UpdateTypedText(); // UIの更新

                    // 入力が正しく完了したら終了処理
                    if (result == TypeResult.Finished)
                    {
                        OnTypingComplete();
                        return;
                    }
                }
            }
        }
    }

    // タイピングが成功裏に完了した際の処理
    void OnTypingComplete()
    {
        if (typingPanel != null)
        {
            typingPanel.SetActive(false);
        }
        PlaySound(successSound);  //successSoundの再生
        OnTypingEnded?.Invoke(true);
    }

    // タイピングを中断する処理
    private void CancelTyping()
    {
        if (typingPanel != null)
        {
            typingPanel.SetActive(false);
        }
        OnTypingEnded?.Invoke(false);
    }

    // UIのテキストを更新する処理
    void UpdateTypedText()
    {
        // タイトル（日本語）とローマ字進捗を表示
        string title = _typingModel.Title;
        string currentRomaji = _typingModel.GetRomajiString();
        int typedIndex = _typingModel.TypedIndex;
        // 入力済み部分を赤色で表示
        string highlightedText = $"<color=red>{currentRomaji.Substring(0, typedIndex)}</color>";
        string remainingText = currentRomaji.Substring(typedIndex);
        string romajiLine = highlightedText + remainingText;

        _typedText.text = $"{title}\n{romajiLine}";
    }

    // 効果音を再生する処理
    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip); 

       }
    }
}