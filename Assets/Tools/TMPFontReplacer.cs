using UnityEngine;
using TMPro;
using UnityEditor;

public class TMPFontReplacer : MonoBehaviour
{
    [MenuItem("Tools/Replace TMP Fonts")]
    static void ReplaceTMPFonts()
    {
        TMP_FontAsset newFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/Stick-Regular SDF.asset");

        TMP_Text[] texts = Resources.FindObjectsOfTypeAll<TMP_Text>();
        int count = 0;
        foreach (TMP_Text t in texts)
        {
            if (t.font != newFont)
            {
                Undo.RecordObject(t, "Replace TMP Font");
                t.font = newFont;
                EditorUtility.SetDirty(t);
                count++;
            }
        }
        Debug.Log($"TMP フォントを置き換えました: {count} 件");
    }
}
