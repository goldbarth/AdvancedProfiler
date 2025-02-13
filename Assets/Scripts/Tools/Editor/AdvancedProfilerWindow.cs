using System;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using Tools.SerializedSettings;
using UnityEditor;
using UnityEngine;

namespace Tools.Editor
{
    public class AdvancedProfilerWindow : OdinEditorWindow
    {
        [MenuItem("Tools/Advanced Profiler", false, -9000)]
        public static void OpenWindow()
        {
            var window = GetWindow<AdvancedProfilerWindow>();
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(800, 500);
            window.minSize = new Vector2(800, 500);
            window.maxSize = new Vector2(1600, 1000);
            
            // Get the type name and remove the "Window" part for the title
            var title = window.GetType().GetNiceName();
            title = title[..title.IndexOf("Window", StringComparison.Ordinal)];
            window.titleContent = new GUIContent(title);
            
            window.Show();
        }
        
        [TabGroup("Profiling")]
        [Title("Profiling Settings", "Configure the Profiling settings for the Advanced Profiler", TitleAlignments.Centered)]
        [InlineProperty]
        [HideLabel]
        public ProfilingSettings ProfilingSettings = new ProfilingSettings();
        
        [TabGroup("Network")]
        [Title("Network Settings", "Configure the Network settings for the Advanced Profiler", TitleAlignments.Centered)]
        [InlineProperty]
        [HideLabel]
        public NetworkSettings NetworkSettings;
        
        [TabGroup("Logs")]
        [Title("Log Settings", "Configure the Log settings for the Advanced Profiler", TitleAlignments.Centered)]
        [InlineProperty]
        [HideLabel]
        public LogSettings LogSettings;
        
        protected override void OnEnable()
        {
            base.OnEnable();
            ProfilingSettings.Initialize();
        }
        
        protected override void OnDestroy()
        {
            base.OnDestroy();
            ProfilingSettings.Dispose();
        }
    }
}