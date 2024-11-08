using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using System.IO;
using System.Text;

public class AudioSnapshotEnumGenerator
{
    /// <summary>
    /// プロジェクト内のすべてのAudioMixerSnapshotを取得し、それぞれのAudioMixer名とスナップショット名を元にAudioSnapshotsという列挙型を生成します。
    /// 各スナップショットは "AudioMixer名_Snapshot名" の形式で登録されます。
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

        // AssetDatabaseからすべてのAudioMixerSnapshotを検索
        string[] snapshotGuids = AssetDatabase.FindAssets("t:AudioMixerSnapshot", new[] {"Assets/Audio"});
        if (snapshotGuids.Length == 0)
        {
            Debug.LogWarning("No AudioMixerSnapshots found in the project.");
            return;
        }

        // スクリプト生成用の文字列を構築
        StringBuilder enumScript = new StringBuilder();
        enumScript.AppendLine("public enum AudioSnapshots");
        enumScript.AppendLine("{");

        foreach (string guid in snapshotGuids)
        {
            // GUIDからパスを取得し、AudioMixerSnapshotオブジェクトをロード
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AudioMixerSnapshot snapshot = AssetDatabase.LoadAssetAtPath<AudioMixerSnapshot>(path);
            if (snapshot != null && snapshot.audioMixer != null)
            {
                // AudioMixerの名前とスナップショットの名前を組み合わせて、enum名を生成
                string mixerName = snapshot.audioMixer.name;
                string enumName = $"{mixerName}_{snapshot.name.Replace(" ", "_")}";

                // スクリプトにスナップショットを追加
                enumScript.AppendLine("    " + enumName + ",");
            }
        }

        enumScript.AppendLine("}");

        // ファイルに書き込み
        File.WriteAllText(enumPath, enumScript.ToString(), Encoding.UTF8);
        Debug.Log("AudioSnapshots enum generated at " + enumPath);

        // AssetDatabaseを更新して新しいenumを即座に使用可能に
        AssetDatabase.Refresh();
    }
}
