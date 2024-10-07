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

        private void Convert(ConvertAnim convertAnim)
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

                var curves = ConvertAnimationCurves(group);
                var clip = new AnimationClip
                {
                    name = $"Universe{universe}"
                };
                for (var i = 0; i < 512; i++)
                {
                    clip.SetCurve("", typeof(DmxData), $"Ch{i + 1:D3}", curves[i]);
                }

                SaveAnimationClip(clip, $"Assets/Universe{universe}.anim");
            }

            Debug.Log("Finish Convert");
        }

        private AnimationCurve[] ConvertAnimationCurves(IEnumerable<(int time, DmxPacket packet)> dmxPackets)
        {
            var curves = new AnimationCurve[512];

            for (var i = 0; i < 512; i++)
            {
                var keys = dmxPackets.Where(x => x.packet.Dmx.Length > i).Select(x => new Keyframe(x.time / 1000f, x
                    .packet.Dmx[i])).ToArray();
                var curve = new AnimationCurve(keys);
                curves[i] = curve;
            }
            return curves;
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
