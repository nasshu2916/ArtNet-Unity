using UnityEngine;

namespace ArtNet
{

    public class ConvertAnim : MonoBehaviour
    {
        // Binary を入れる SerializedField
        public TextAsset binary;
        public string OutputDirectory;
        public bool IsCompressBinary;
    }
}
