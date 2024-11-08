using UnityEngine;

namespace dsgarage.Audio
{
    /// <summary>
    /// AudioMixerTester は、AudioMixerManager のスナップショットの切り替えをテストするためのデバッグ用クラスです。
    /// 現在のスナップショット状態を画面上に表示し、選択されたスナップショットに切り替え可能です。
    /// </summary>
    public class AudioMixerTester : MonoBehaviour
    {
        // AudioMixerManagerを参照（起動時に自動検索）
        private AudioMixerManager audioMixerManager;

        // 切り替え用のAudioSnapshots選択
        public AudioSnapshots selectedSnapshot;

        // 遷移時間（フェード時間）
        public float transitionTime = 1.0f;

        private AudioSnapshots lastSnapshot;

        private void Start()
        {
            // シーン内のAudioMixerManagerを検索して設定
            audioMixerManager = FindObjectOfType<AudioMixerManager>();
            if (audioMixerManager == null)
            {
                Debug.LogError("AudioMixerManager was not found in the scene.");
                return;
            }

            // 初回のスナップショット表示更新
            UpdateSnapshotDisplay();
        }

        private void Update()
        {
            // 選択されたスナップショットが変更されたら切り替え
            if (selectedSnapshot != lastSnapshot)
            {
                SwitchToSelectedSnapshot();
                lastSnapshot = selectedSnapshot;
            }
        }

        /// <summary>
        /// 選択されたスナップショットに切り替えます。
        /// </summary>
        public void SwitchToSelectedSnapshot()
        {
            if (audioMixerManager != null)
            {
                AudioMixerManager.SwitchSnapshot(selectedSnapshot, transitionTime);
                UpdateSnapshotDisplay();
            }
        }

        /// <summary>
        /// 現在のスナップショット名を画面上に表示します。
        /// </summary>
        private void OnGUI()
        {
            // デバッグ情報の表示位置を設定
            GUIStyle labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };

            // ボタンとラベルの位置を調整
            if (GUI.Button(new Rect(10, 10, 200, 30), "Switch to Selected Snapshot"))
            {
                SwitchToSelectedSnapshot();
            }

            // ボタンの下に現在のスナップショットを表示
            GUI.Label(new Rect(10, 50, 300, 30), "Current Snapshot: " + selectedSnapshot.ToString(), labelStyle);
        }

        /// <summary>
        /// スナップショットの切り替えをデバッグログに出力します。
        /// </summary>
        private void UpdateSnapshotDisplay()
        {
            Debug.Log("Switched to snapshot: " + selectedSnapshot.ToString());
        }
    }
}
