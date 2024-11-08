using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using System.IO;
using System.Text;

namespace dsgarage.Audio
{
    /// <summary>
    /// AudioSnapshotEnumGenerator は、プロジェクト内のすべての AudioMixerSnapshot アセットから列挙型 (enum) を自動生成するエディタ拡張です。
    /// 生成された列挙型 (AudioSnapshots) は、コード内で AudioMixerSnapshot を簡単に参照するために使用します。
    /// </summary>
    public class AudioSnapshotEnumGenerator
    {
        /// <summary>
        /// プロジェクト内のすべての AudioMixerSnapshot を含む AudioSnapshots という列挙型 (enum) を生成します。
        /// 生成された列挙型は、指定されたディレクトリに AudioSnapshotsEnum.cs として保存されます。
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

            // AssetDatabase からプロジェクト内のすべての AudioMixerSnapshot アセットを検索
            string[] snapshotGuids = AssetDatabase.FindAssets("t:AudioMixerSnapshot");
            if (snapshotGuids.Length == 0)
            {
                Debug.LogWarning("プロジェクトに AudioMixerSnapshots が見つかりませんでした。");
                return;
            }

            // Enum スクリプトの内容を構築
            StringBuilder enumScript = new StringBuilder();
            enumScript.AppendLine("public enum AudioSnapshots");
            enumScript.AppendLine("{");

            foreach (string guid in snapshotGuids)
            {
                // GUID からパスを取得し、AudioMixerSnapshot オブジェクトをロード
                string path = AssetDatabase.GUIDToAssetPath(guid);
                AudioMixerSnapshot snapshot = AssetDatabase.LoadAssetAtPath<AudioMixerSnapshot>(path);
                if (snapshot != null)
                {
                    // スナップショット名にスペースが含まれる場合はアンダースコアに置換
                    enumScript.AppendLine("    " + snapshot.name.Replace(" ", "_") + ",");
                }
            }

            enumScript.AppendLine("}");

            // ファイルに書き込み
            File.WriteAllText(enumPath, enumScript.ToString(), Encoding.UTF8);
            Debug.Log("AudioSnapshots enum が " + enumPath + " に生成されました。");

            // AssetDatabase を更新して、新しい enum がすぐに使用できるようにする
            AssetDatabase.Refresh();
        }
    }
}
