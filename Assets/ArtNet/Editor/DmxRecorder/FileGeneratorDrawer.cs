using System.IO;
using ArtNet.Editor.DmxRecorder.Util;
using UnityEditor;
using UnityEngine;

namespace ArtNet.Editor.DmxRecorder
{
    [CustomPropertyDrawer(typeof(FileGenerator))]
    internal class FileGeneratorDrawer : TargetedPropertyDrawer<FileGenerator>
    {
        private static class Contents
        {
            internal static readonly GUIContent FileNameLabel = new()
            {
                text = "File Name",
                tooltip =
                    "The name of the file to record to.\n" +
                    "If using wildcards, they will be replaced with dynamic values. (use the \"+ Wildcards\" button)"
            };

            internal static readonly GUIContent AddWildcardButton = new()
            {
                text = "+ Wildcards",
                tooltip = "Add a wildcard to the file name. Wildcards are replaced with values."
            };

            internal static readonly GUIContent PathSelectButton = new()
            {
                text = "...",
                tooltip = "Select the output location"
            };

            internal static readonly GUIContent OpenDirectoryButton = new()
            {
                image = IconHelper.FolderOpen,
                tooltip = "Open Directory"
            };
        }

        private SerializedProperty _fileName;
        private SerializedProperty _directory;

        private static readonly GUIStyle PathPreviewStyle = new(GUI.skin.label)
        {
            wordWrap = true,
            stretchHeight = true,
            stretchWidth = true,
            padding = new RectOffset(20, 0, 0, 0),
            clipping = TextClipping.Overflow
        };

        protected override void Initialize(SerializedProperty property)
        {
            base.Initialize(property);

            _fileName = property.FindPropertyRelative("_fileName");
            _directory = property.FindPropertyRelative("_directory");
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Initialize(property);

            EditorGUI.BeginProperty(position, label, property);

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PropertyField(_fileName, Contents.FileNameLabel);

                if (GUILayout.Button(Contents.AddWildcardButton, EditorStyles.popup, GUILayout.Width(90)))
                {
                    GUI.FocusControl(null);
                    var menu = new GenericMenu();

                    foreach (var w in Target.Wildcards)
                    {
                        var pattern = w.Pattern;
                        menu.AddItem(new GUIContent(w.Label),
                            false,
                            () =>
                            {
                                _fileName.stringValue += pattern;
                                _fileName.serializedObject.ApplyModifiedProperties();
                            });
                    }

                    menu.ShowAsContext();
                }
            }


            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Assets Path", GUILayout.Width(EditorGUIUtility.labelWidth));

                var restRect = GUILayoutUtility.GetRect(GUIContent.none, GUI.skin.textField);
                var labelRect = new Rect(restRect.x, restRect.y, 50, restRect.height);
                GUI.Label(labelRect, "Assets" + Path.DirectorySeparatorChar, EditorStyles.label);
                var directoryInputRect = new Rect(
                    restRect.x + 55,
                    restRect.y,
                    restRect.width - 55,
                    restRect.height
                );
                _directory.stringValue = EditorGUI.TextField(directoryInputRect, _directory.stringValue);

                if (GUILayout.Button(Contents.PathSelectButton, EditorStyles.miniButton, GUILayout.Width(30)))
                {
                    var outputDirectory = Target.OutputDirectory();
                    var newDirectory = EditorUtility.OpenFolderPanel("Select Folder", outputDirectory, "");
                    GUI.FocusControl(null);

                    if (!string.IsNullOrEmpty(newDirectory))
                    {
                        if (!newDirectory.StartsWith(Application.dataPath))
                        {
                            EditorUtility.DisplayDialog("Invalid Path",
                                "Selected path " + newDirectory + " is not within the project's Assets folder.",
                                "Ok");
                        }
                        else
                        {
                            newDirectory = newDirectory[Application.dataPath.Length..];
                            if (newDirectory.StartsWith("/") || newDirectory.StartsWith("\\"))
                            {
                                newDirectory = newDirectory[1..];
                            }

                            _directory.stringValue = newDirectory;
                            _directory.serializedObject.ApplyModifiedProperties();
                        }
                    }

                    GUIUtility.ExitGUI();
                }
            }


            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PrefixLabel(" ");

                var layoutOptions = new[]
                {
                    GUILayout.ExpandWidth(true),
                    GUILayout.ExpandHeight(true)
                };

                var outputPath = Target.AbsolutePath();
                var rect = GUILayoutUtility.GetRect(new GUIContent(outputPath), PathPreviewStyle, layoutOptions);
                EditorGUI.SelectableLabel(rect, outputPath, PathPreviewStyle);

                if (GUILayout.Button(Contents.OpenDirectoryButton, EditorStyles.miniButton, GUILayout.Width(30)))
                {
                    var outputDir = Target.OutputDirectory();
                    var dir = new DirectoryInfo(outputDir);
                    if (!dir.Exists) dir.Create();

                    EditorUtility.RevealInFinder(outputDir);
                }
            }

            EditorGUI.EndProperty();
        }
    }
}
