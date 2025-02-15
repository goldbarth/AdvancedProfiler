using System;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using Tools.SerializedSettings;
using Tools.SerializedSettings.Base;
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
        
        private ISettings _settings;
        
        [TabGroup("Profiling")]
        [Title("Profiling Settings", "Configure the Profiling settings for the Advanced Profiler", TitleAlignments.Centered)]
        [InlineProperty]
        [HideLabel]
        public ProfilingSettings ProfilingSettings = new ProfilingSettings();
        
        [TabGroup("Network")]
        [Title("Network Settings", "Configure the Network settings for the Advanced Profiler", TitleAlignments.Centered)]
        [InlineProperty]
        [HideLabel]
        public NetworkSettings NetworkSettings = new NetworkSettings();
        
        [TabGroup("Logs")]
        [Title("Log Settings", "Configure the Log settings for the Advanced Profiler", TitleAlignments.Centered)]
        [InlineProperty]
        [HideLabel]
        public LogSettings LogSettings = new LogSettings();
        
        protected override void OnEnable()
        {
            base.OnEnable();
            _settings = new CompositeSettings(
                ProfilingSettings, 
                NetworkSettings, 
                LogSettings
                );
            _settings.Initialize();
            EditorApplication.update += OnEditorUpdate;
        }
        
        protected override void OnDestroy()
        {
            base.OnDestroy();
            _settings.Dispose();
            EditorApplication.update -= OnEditorUpdate;
        }
        
        private void OnEditorUpdate()
        {
            Repaint();
        }
    }
}