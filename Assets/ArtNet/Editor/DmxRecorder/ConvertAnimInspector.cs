using System.Collections.Generic;
using System.IO;
using System.Linq;
using ArtNet.Common;
using ArtNet.Editor.DmxRecorder.IO;
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
                if (GUILayout.Button("ConvertAnim"))
                {
                    ConvertAnim(convertAnim);
                }

                GUILayout.Space(5);

                if (GUILayout.Button("ConvertPacket"))
                {
                    ConvertPacket(convertAnim);
                }
            }
            EditorGUILayout.EndVertical();
        }

        private static void ConvertAnim(ConvertAnim convertAnim)
        {
            var binary = convertAnim.binary;
            if (!binary)
            {
                Debug.LogError("Binary is null");
                return;
            }
            if (string.IsNullOrEmpty(convertAnim.OutputDirectory))
            {
                Debug.LogError("Output directory is null or empty");
                return;
            }

            var bytes = binary.bytes;
            var universeData = BinaryDmx.Deserialize(bytes);

            var output = convertAnim.OutputDirectory + "/ArtNetDmx.anim";
            AnimationClipDmx.Export(universeData, output);

            ArtNetLogger.DevLogDebug("Conversion complete");
        }

        private static void ConvertPacket(ConvertAnim convertAnim)
        {
            var binary = convertAnim.binary;
            if (!binary)
            {
                Debug.LogError("Binary is null");
                return;
            }
            if (string.IsNullOrEmpty(convertAnim.OutputDirectory))
            {
                Debug.LogError("Output directory is null or empty");
                return;
            }

            var timelineSettingPath = convertAnim.OutputDirectory + "/ArtNetDmx.anim";
            if (AssetDatabase.LoadAssetAtPath(timelineSettingPath, typeof(AnimationClip)) is not AnimationClip artNetDmxClip)
            {
                Debug.LogError("DmxTimelineSetting is null");
                return;
            }

            var timelineConverter = new TimelineConverter(artNetDmxClip);
            var universeDataList = timelineConverter.ToUniverseData();
            var dmxPackets = new List<(long, DmxPacket)>();
            byte sequence = 0;
            foreach (var universeData in universeDataList)
            {
                var packet = new DmxPacket
                {
                    Sequence = sequence++,
                    Universe = (ushort) universeData.Universe,
                    Dmx = universeData.Values
                };
                dmxPackets.Add((universeData.Time, packet));

                if (sequence >= 255)
                {
                    sequence = 0;
                }
                else
                {
                    sequence++;
                }
            }

            var dmxUniverseData = dmxPackets.Select(packet =>
                new UniverseData(packet.Item1, packet.Item2.Universe, packet.Item2.Dmx)).ToList();

            var path = convertAnim.OutputDirectory + "/DmxPackets.bytes";
            var exists = File.Exists(path);
            BinaryDmx.Export(dmxUniverseData, path, convertAnim.IsCompressBinary);

            var message = exists ? "Data updated" : "Data stored";
            ArtNetLogger.DevLogDebug($"ArtNet Recorder: {message} at {path}");
            ArtNetLogger.DevLogDebug("Conversion complete");
        }
    }
}
