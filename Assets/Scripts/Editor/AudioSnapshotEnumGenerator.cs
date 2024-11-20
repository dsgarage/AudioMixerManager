using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

public class AudioSnapshotEnumGenerator
{
    /// <summary>
    /// プロジェクト内のすべてのAudioMixerとそのスナップショットを取得し、それぞれのAudioMixer名とスナップショット名を元にAudioSnapshotsという列挙型を生成します。
    /// 各スナップショットは "AudioMixer名_Snapshot名" の形式で登録されます。
    /// スナップショット名に数字や記号が含まれている場合、そのエントリはコメントアウトされます。
    /// </summary>
    [MenuItem("Tools/Generate AudioSnapshots Enum")]
    public static void GenerateEnum()
    {
        // Enumスクリプトを保存するディレクトリとファイル名
        string directoryPath = "Assets/Scripts";
        string enumPath = Path.Combine(directoryPath, "AudioSnapshotsEnum.cs");

        // ディレクトリが存在しない場合は作成
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        // AssetDatabaseからすべてのAudioMixerを検索
        string[] mixerGuids = AssetDatabase.FindAssets("t:AudioMixer", new[] { "Assets/Audio" });
        if (mixerGuids.Length == 0)
        {
            Debug.LogWarning("プロジェクト内にAudioMixerが見つかりませんでした。");
            return;
        }

        // スクリプト生成用の文字列を構築
        StringBuilder enumScript = new StringBuilder();
        enumScript.AppendLine("public enum AudioSnapshots");
        enumScript.AppendLine("{");

        foreach (string guid in mixerGuids)
        {
            // GUIDからパスを取得し、AudioMixerオブジェクトをロード
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AudioMixer mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(path);
            if (mixer != null)
            {
                // SerializedObjectを使用して内部データにアクセス
                SerializedObject serializedMixer = new SerializedObject(mixer);
                SerializedProperty snapshotsProperty = serializedMixer.FindProperty("m_Snapshots");

                if (snapshotsProperty != null && snapshotsProperty.isArray)
                {
                    for (int i = 0; i < snapshotsProperty.arraySize; i++)
                    {
                        SerializedProperty snapshotProp = snapshotsProperty.GetArrayElementAtIndex(i);
                        AudioMixerSnapshot snapshot = snapshotProp.objectReferenceValue as AudioMixerSnapshot;

                        if (snapshot != null)
                        {
                            // AudioMixerの名前とスナップショットの名前を組み合わせて、enum名を生成
                            string mixerName = mixer.name.Replace(" ", "_");
                            string snapshotName = snapshot.name.Replace(" ", "_");
                            string enumName = $"{mixerName}_{snapshotName}";

                            // 有効なC#の識別子かをチェック
                            if (IsValidIdentifier(enumName))
                            {
                                // スクリプトにスナップショットを追加
                                enumScript.AppendLine("    " + enumName + ",");
                            }
                            else
                            {
                                // 無効な場合はコメントアウト
                                enumScript.AppendLine("    // " + enumName + "  // 名前に無効な文字が含まれています");
                            }
                        }
                    }
                }
                else
                {
                    Debug.LogWarning($"Mixer '{mixer.name}' 内のスナップショットを取得できませんでした。");
                }
            }
        }

        enumScript.AppendLine("}");

        // ファイルに書き込み
        File.WriteAllText(enumPath, enumScript.ToString(), Encoding.UTF8);
        Debug.Log("AudioSnapshots列挙型を生成しました: " + enumPath);

        // AssetDatabaseを更新して新しいenumを即座に使用可能に
        AssetDatabase.Refresh();
    }

    // 有効なC#識別子かどうかをチェックする関数
    private static bool IsValidIdentifier(string identifier)
    {
        if (string.IsNullOrEmpty(identifier))
            return false;

        // 識別子がキーワードでないかチェック
        if (IsCSharpKeyword(identifier))
            return false;

        // 最初の文字がアルファベットまたはアンダースコアである必要がある
        if (!Regex.IsMatch(identifier[0].ToString(), @"^[a-zA-Z_]+$"))
            return false;

        // 残りの文字がアルファベット、数字、またはアンダースコアである必要がある
        if (!Regex.IsMatch(identifier, @"^[a-zA-Z_][a-zA-Z0-9_]*$"))
            return false;

        return true;
    }

    // C#のキーワードかどうかをチェックする関数
    private static bool IsCSharpKeyword(string word)
    {
        string[] keywords = new string[]
        {
            "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char",
            "checked", "class", "const", "continue", "decimal", "default", "delegate",
            "do", "double", "else", "enum", "event", "explicit", "extern", "false",
            "finally", "fixed", "float", "for", "foreach", "goto", "if", "implicit",
            "in", "int", "interface", "internal", "is", "lock", "long", "namespace",
            "new", "null", "object", "operator", "out", "override", "params", "private",
            "protected", "public", "readonly", "ref", "return", "sbyte", "sealed", "short",
            "sizeof", "stackalloc", "static", "string", "struct", "switch", "this",
            "throw", "true", "try", "typeof", "uint", "ulong", "unchecked", "unsafe",
            "ushort", "using", "virtual", "void", "volatile", "while"
        };
        foreach (string keyword in keywords)
        {
            if (word == keyword)
                return true;
        }
        return false;
    }
}
