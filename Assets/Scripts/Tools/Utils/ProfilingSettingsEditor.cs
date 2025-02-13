using Sirenix.OdinInspector.Editor;
using Tools.SerializedSettings;
using UnityEditor;

namespace Tools.Utils
{
    // [CustomEditor(typeof(ProfilingSettings))]
    public class ProfilingSettingsEditor : OdinEditor
    {
        protected override void OnEnable()
        {
            EditorApplication.update += UpdateInspector;
        }

        protected override void OnDisable()
        {
            EditorApplication.update -= UpdateInspector;
        }

        protected virtual void UpdateInspector()
        {
            Repaint();
        }
    }
}