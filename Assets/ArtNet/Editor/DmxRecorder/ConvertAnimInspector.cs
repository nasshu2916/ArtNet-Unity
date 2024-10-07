using System.Collections.Generic;
using System.Linq;
using ArtNet.Packets;
using UnityEditor;
using UnityEngine;

namespace ArtNet.Editor.DmxRecorder
{
    [CustomEditor(typeof(ConvertAnim))]
    public class ConvertAnimInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var convertAnim = target as ConvertAnim;
            if (!convertAnim) return;

            GUILayout.Space(5);

            EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                if (GUILayout.Button("Convert"))
                {
                    Convert(convertAnim);
                }
            }
            EditorGUILayout.EndVertical();
        }

        private static void Convert(ConvertAnim convertAnim)
        {
            var binary = convertAnim.binary;
            if (!binary)
            {
                Debug.LogError("Binary is null");
                return;
            }

            var bytes = binary.bytes;
            var groupedUniversePackets = RecordData.Deserialize(bytes).OrderBy(x => x.time).ToList().GroupBy(x => x
                .packet
                .Universe);

            foreach (var group in groupedUniversePackets)
            {
                var universe = group.Key;

                var curves = ConvertAnimationCurves(group.ToList());
                var clip = new AnimationClip
                {
                    name = $"Universe{universe + 1}"
                };
                for (var i = 0; i < curves.Length; i++)
                {
                    if (curves[i].keys.Length == 0) continue;
                    clip.SetCurve("", typeof(DmxData), $"Ch{i + 1:D3}", curves[i]);
                }

                SaveAnimationClip(clip, $"Assets/Universe{universe + 1}.anim");
            }
        }

        private static AnimationCurve[] ConvertAnimationCurves(
            IReadOnlyCollection<(int time, DmxPacket packet)> dmxPackets)
        {
            var dmxKeys = new List<(int, byte)>[512];
            for (var i = 0; i < 512; i++)
            {
                dmxKeys[i] = new List<(int, byte)>();
            }

            foreach (var (time, packet) in dmxPackets)
            {
                for (var i = 0; i < packet.Length; i++)
                {
                    var value = packet.Dmx[i];
                    var keys = dmxKeys[i];
                    if (keys.Count > 1 && keys[^1].Item2 == value && keys[^2].Item2 == value)
                    {
                        keys.RemoveAt(keys.Count - 1);
                    }

                    keys.Add((time, value));
                }
            }

            return dmxKeys.Select(keys =>
                new AnimationCurve(keys.Select(x => new Keyframe(x.Item1 / 1000f, x.Item2)).ToArray())).ToArray();
        }

        private static void SaveAnimationClip(AnimationClip clip, string path)
        {
            AssetDatabase.CreateAsset(clip, path);
            AssetDatabase.SaveAssets();
            Debug.Log("AnimationClip saved to " + path);
            AssetDatabase.Refresh();
        }
    }
}
