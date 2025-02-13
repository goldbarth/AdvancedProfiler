using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

#if UNITY_2020_1_OR_NEWER
using Unity.Profiling;
#endif

namespace Tools.SerializedSettings
{
    [ExecuteAlways]
    [Serializable]
    public class ProfilingSettings : BaseSettings
    {
        // Maximale Anzahl gespeicherter Werte
        private const int HistoryLength = 100;
        private const int FpsHistoryLength = 100;
        
        // Verlauf der CPU-Nutzung und FPS
        private readonly Queue<float> _cpuHistory = new(HistoryLength);
        private readonly Queue<float> _fpsHistory = new(FpsHistoryLength);
        
        private bool _displayFPS;
        private bool _displayCPU;
        private bool _displayMemory;

        public bool DisplayFPS
        {
            get => _displayFPS;
            set => _displayFPS = value;
        }
        
        public bool DisplayCPU
        {
            get => _displayCPU;
            set => _displayCPU = value;
        }

        public bool DisplayMemory
        {
            get => _displayMemory;
            set => _displayMemory = value;
        }
        
        private ProfilerRecorder _cpuRecorder;
        
        public void Initialize()
        {
            EditorApplication.update += UpdateMetricsAndRepaint;
#if UNITY_2020_1_OR_NEWER
            StartRecording();
#endif
            Debug.Log("ProfilingSettings initialized.");
        }
        
        public void Dispose()
        {
            EditorApplication.update -= UpdateMetricsAndRepaint;
#if UNITY_2020_1_OR_NEWER
            _cpuRecorder.Dispose();
#endif
            Debug.Log("ProfilingSettings disposed.");
        }
        
        public void StartRecording()
        {
            // Starte einen Recorder, der z.B. die Zeit auf dem Hauptthread misst
            _cpuRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Main Thread");
        }
        
        private void UpdateMetricsAndRepaint()
        {
            UpdateMetrics();  // Hier werden Deine echten Messwerte aktualisiert
        }

        // Aktualisiere die Werte – je nachdem, wie Du das in Deinem Setup handhaben willst,
        // könntest Du dies in einem Editor-Update oder in einer separaten Update()-Methode machen.
        public void UpdateMetrics()
        {
            // FPS-Wert aktualisieren
            CurrentFPS = 1.0f / Time.unscaledDeltaTime;
            _fpsHistory.Enqueue(CurrentFPS);
            if (_fpsHistory.Count > FpsHistoryLength)
            {
                _fpsHistory.Dequeue();
            }
            
            // CPU-Wert aktualisieren
#if UNITY_2020_1_OR_NEWER
            if (_cpuRecorder is { Valid: true, Count: > 0 })
            {
                long sum = 0;
                for (int i = 0; i < _cpuRecorder.Count; i++)
                {
                    sum += _cpuRecorder.GetSample(i).Value;
                }
                var avgCpuUsageMicro = sum / (float)_cpuRecorder.Count;
                var avgCpuUsageMs = avgCpuUsageMicro / 1000f;
                CpuUsage = avgCpuUsageMs;
            }
#endif

            // CPU-Wert zum Verlauf hinzufügen
            _cpuHistory.Enqueue(CpuUsage);
            if (_cpuHistory.Count > HistoryLength)
            {
                _cpuHistory.Dequeue();
            }
        }
        
        #region Editor Exposure
        
        [PropertyOrder(1)]
        [BoxGroup("Profiling Settings", CenterLabel = true)]
        [Button("$DisplayFPSName"), GUIColor("DisplayFPSColor")]
        private void DisplayFPSButton()
        {
            _displayFPS = !_displayFPS;
        }
        
        [PropertyOrder(1)]
        [BoxGroup("Profiling Settings")]
        [OnInspectorGUI] private void Space1() => GUILayout.Space(5);
        
        [PropertyOrder(1)]
        [BoxGroup("Profiling Settings", CenterLabel = true)]
        [Button("$DisplayCPUName"), GUIColor("DisplayCPUColor")]
        private void DisplayCPUButton()
        {
            _displayCPU = !_displayCPU;
        }
        
        [PropertyOrder(1)]
        [BoxGroup("Profiling Settings")]
        [OnInspectorGUI] private void Space2() => GUILayout.Space(5);

        [PropertyOrder(1)]
        [BoxGroup("Profiling Settings")]
        [Button("$DisplayMemoryName"), GUIColor("DisplayMemoryColor")]
        private void DisplayMemoryButton()
        {
            _displayMemory = !_displayMemory;
        }
        
        [PropertyOrder(4)]
        [BoxGroup("Metrics")]
        [LabelText("Garbage Collection (ms)")]
        public float GarbageCollectionTime = 0.0f;

        [PropertyOrder(5)]
        [BoxGroup("Metrics")]
        [LabelText("Draw Calls")]
        public int DrawCalls = 0;

        [PropertyOrder(6)]
        [BoxGroup("Metrics")]
        [LabelText("Physics calculations (ms)")]
        public float PhysicsCalculationTime = 0.0f;
        
        [PropertyOrder(7)]
        [ShowIf("DisplayCPU")]
        [BoxGroup("Metrics/CPUGraph", LabelText = "CPU Usage Graph")]
        [LabelText("CPU Utilization")]
        [ReadOnly, SuffixLabel("ms", Overlay = true)]
        public float CpuUsage;
        
        [PropertyOrder(7)]
        [ShowIf("DisplayCPU")]
        [BoxGroup("Metrics/CPUGraph", LabelText = "CPU Usage Graph")]
        [OnInspectorGUI] private void Space3() => GUILayout.Space(5);

        // Graphische Darstellung: Hier wird ein einfacher Graph der CPU-Nutzung gezeichnet
        [PropertyOrder(7)]
        [ShowIf("DisplayCPU")]
        [BoxGroup("Metrics/CPUGraph", LabelText = "CPU Usage Graph")]
        [ShowInInspector]
        [OnInspectorGUI]
        private void DrawCpuGraph()
        {
            var rect = GUILayoutUtility.GetRect(100, 100);
            var backgroundColor = new Color(0.12f, 0.12f, 0.12f);
            EditorGUI.DrawRect(rect, backgroundColor);
            
            if (Event.current.type == EventType.Repaint)
            {
                Handles.BeginGUI();
                Handles.color = Color.green;
                var count = _cpuHistory.Count;
                if (count < 2)
                {
                    Handles.EndGUI();
                    return;
                }

                // Ermittle den maximalen CPU-Wert aus dem Verlauf.
                // Setze mindestens einen Default-Wert (z.B. 16ms), falls alle Werte sehr niedrig sind.
                var maxCpu = Mathf.Max(16.6f, _cpuHistory.Max());

                var points = new Vector3[count];
                var historyArray = _cpuHistory.ToArray();

                for (int i = 0; i < count; i++)
                {
                    // Normalisiere den aktuellen Wert relativ zum maximalen Wert der History.
                    // Dadurch wird der höchste gemessene Wert als 100% (also am oberen Rand) angezeigt.
                    var normalizedValue = Mathf.Clamp01(historyArray[i] / maxCpu);
                    var x = rect.x + (rect.width / (count - 1)) * i;
                    var y = rect.y + rect.height * (1 - normalizedValue);
                    points[i] = new Vector3(x, y, 0);
                }
                
                Handles.DrawAAPolyLine(2, points);
                Handles.EndGUI();
                
                DrawGuideLines(rect);
            }
        }

        [PropertyOrder(8)]
        [ShowIf("DisplayFPS")]
        [BoxGroup("Metrics/FPSGraph", LabelText = "FPS Graph")]
        [LabelText("FPS")]
        [ReadOnly, SuffixLabel("FPS", Overlay = true)]
        public float CurrentFPS;
        
        [PropertyOrder(8)]
        [ShowIf("DisplayFPS")]
        [BoxGroup("Metrics/FPSGraph", LabelText = "FPS Graph")]
        [OnInspectorGUI] private void Space4() => GUILayout.Space(5);
        
        [PropertyOrder(8)]
        [ShowIf("DisplayFPS")]
        [BoxGroup("Metrics/FPSGraph", LabelText = "FPS Graph")]
        [OnInspectorGUI]
        private void DrawFpsGraph()
        {
            // Erzeuge ein Rechteck als Zeichenfläche
            var rect = GUILayoutUtility.GetRect(100, 100);
            EditorGUI.DrawRect(rect, new Color(0.12f, 0.12f, 0.12f));

            if (Event.current.type == EventType.Repaint)
            {
                Handles.BeginGUI();
                Handles.color = Color.cyan;
        
                int count = _fpsHistory.Count;
                if (count < 2)
                {
                    Handles.EndGUI();
                    return;
                }

                // Ermittle den maximalen FPS-Wert aus der History.
                // Alternativ kannst Du hier einen festen Wert wählen, z.B. 100 FPS.
                float maxFps = Mathf.Max(60f, _fpsHistory.Max());
                Vector3[] points = new Vector3[count];
                float[] fpsArray = _fpsHistory.ToArray();

                for (int i = 0; i < count; i++)
                {
                    // Normalisiere den FPS-Wert relativ zum maximalen Wert (als 100%)
                    float normalizedFps = Mathf.Clamp01(fpsArray[i] / maxFps);
                    float x = rect.x + (rect.width / (count - 1)) * i;
                    // Bei FPS kann man sich vorstellen: Höhere Werte => weiter oben im Graphen
                    float y = rect.y + rect.height * (1 - normalizedFps);
                    points[i] = new Vector3(x, y, 0);
                }

                // Zeichne die Linie des Graphen
                Handles.DrawAAPolyLine(2, points);
                Handles.EndGUI();

                // Optional: Zeichne Hilfslinien zur Orientierung
                DrawGuideLines(rect);
            }
        }
        
        private void DrawGuideLines(Rect rect)
        {
            // Obere Linie (100% – max FPS)
            Handles.color = Color.red;
            Handles.DrawLine(new Vector3(rect.x, rect.y, 0), new Vector3(rect.x + rect.width, rect.y, 0));
    
            // 75%
            Handles.color = new Color(1f, 0.5f, 0f);
            float y75 = rect.y + rect.height * 0.25f;
            Handles.DrawLine(new Vector3(rect.x, y75, 0), new Vector3(rect.x + rect.width, y75, 0));
    
            // 50%
            Handles.color = Color.green;
            float y50 = rect.y + rect.height * 0.5f;
            Handles.DrawLine(new Vector3(rect.x, y50, 0), new Vector3(rect.x + rect.width, y50, 0));
    
            // 25%
            Handles.color = Color.yellow;
            float y25 = rect.y + rect.height * 0.75f;
            Handles.DrawLine(new Vector3(rect.x, y25, 0), new Vector3(rect.x + rect.width, y25, 0));
        }
        
        #endregion
        
        #region Button Utils
        
        private string DisplayFPSName => DisplayFPS ? "Display FPS : ON" : "Display FPS : OFF";
        private string DisplayCPUName => DisplayCPU ? "CPU Display : ON" : "Display CPU : OFF";
        private string DisplayMemoryName => DisplayMemory ? "Memory Display : ON" : "Display Memory : OFF";

        private Color DisplayFPSColor => GetButtonColor(DisplayFPS);
        private Color DisplayCPUColor => GetButtonColor(DisplayCPU);
        private Color DisplayMemoryColor => GetButtonColor(DisplayMemory);
        
        #endregion
    }
}