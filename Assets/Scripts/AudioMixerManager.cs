using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;

namespace dsgarage.Audio
{
    /// <summary>
    /// AudioMixerManager は、シングルトンとして AudioMixer のスナップショットを一元管理し、
    /// どこからでも簡単にスナップショットを切り替えることができるマネージャークラスです。
    /// </summary>
    public class AudioMixerManager : MonoBehaviour
    {
        // シングルトンインスタンス
        public static AudioMixerManager Instance { get; private set; }

        // AudioMixerとスナップショットのキャッシュ
        private Dictionary<string, AudioMixerSnapshot> snapshotCache = new Dictionary<string, AudioMixerSnapshot>();

        // デフォルトのスナップショットと起動時の適用フラグ
        public AudioSnapshots defaultSnapshot;
        public bool useDefaultSnapshotOnStart = true;

        // 遷移時間（フェード時間）
        [SerializeField] private float defaultTransitionTime = 1.0f;

        private void Awake()
        {
            // シングルトンのインスタンスを設定
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject); // シーン間でオブジェクトを保持
            }
            else
            {
                Destroy(gameObject); // 重複するインスタンスがある場合は削除
                return;
            }

            // 起動時にシーン内のAudioMixerを検索してキャッシュを構築
            BuildSnapshotCache();

            // 起動時にデフォルトのスナップショットを適用するかどうかを確認
            if (useDefaultSnapshotOnStart)
            {
                SwitchSnapshot(defaultSnapshot, defaultTransitionTime);
            }
        }

        /// <summary>
        /// シーン内の AudioMixer を検索し、スナップショットキャッシュを構築します。
        /// </summary>
        private void BuildSnapshotCache()
        {
            snapshotCache.Clear();

            // シーン内のすべての AudioMixer を検索
            AudioMixer[] mixersInScene = Resources.FindObjectsOfTypeAll<AudioMixer>();

            foreach (AudioMixer mixer in mixersInScene)
            {
                foreach (AudioSnapshots snapshot in System.Enum.GetValues(typeof(AudioSnapshots)))
                {
                    string snapshotName = snapshot.ToString();
                    string[] parts = snapshotName.Split('_');

                    if (parts.Length == 2 && parts[0] == mixer.name)
                    {
                        AudioMixerSnapshot mixerSnapshot = mixer.FindSnapshot(parts[1]);
                        if (mixerSnapshot != null)
                        {
                            snapshotCache[snapshotName] = mixerSnapshot;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// スナップショットを切り替える静的関数
        /// </summary>
        /// <param name="snapshotEnum">AudioSnapshots で指定するスナップショット</param>
        /// <param name="transitionTime">オプションの遷移時間（省略時は defaultTransitionTime を使用）</param>
        public static void SwitchSnapshot(AudioSnapshots snapshotEnum, float transitionTime = -1f)
        {
            if (Instance == null)
            {
                Debug.LogError("AudioMixerManager インスタンスが初期化されていません。");
                return;
            }

            // transitionTime が指定されていなければデフォルトの遷移時間を使用
            if (transitionTime < 0)
            {
                transitionTime = Instance.defaultTransitionTime;
            }

            // enum からキャッシュのキーとなる名前を取得
            string snapshotEnumName = snapshotEnum.ToString();
            if (Instance.snapshotCache.TryGetValue(snapshotEnumName, out AudioMixerSnapshot snapshot))
            {
                snapshot.audioMixer.TransitionToSnapshots(new[] { snapshot }, new float[] { 1.0f }, transitionTime);
                Debug.Log($"スナップショットに切り替えました: {snapshotEnumName} in AudioMixer: {snapshot.audioMixer.name}");
            }
            else
            {
                Debug.LogWarning($"スナップショット '{snapshotEnumName}' がキャッシュ内に見つかりません。");
            }
        }
    }
}
