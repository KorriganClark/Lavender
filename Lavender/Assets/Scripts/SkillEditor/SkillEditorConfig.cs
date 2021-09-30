using Sirenix.OdinInspector;
using UnityEngine;
namespace Lavender
{
    public class SkillEditorConfig : SerializedScriptableObject
    {
        public LavenderCharacter Monster;
        public Mesh mesh;
        public Material material;
        public Rect position = new Rect(100, 75, 150, 799);
    }
}