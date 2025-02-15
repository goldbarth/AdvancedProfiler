using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Tools.SerializedSettings.Base;
using Tools.Utils;
using UnityEngine;

#if UNITY_2022_1_OR_NEWER
using Unity.Profiling;
#endif

namespace Tools.SerializedSettings
{
    internal enum RecorderCommand
    {
        Start,
        Stop
    }
    
    [ExecuteAlways]
    [Serializable]
    public class ProfilingSettings : BaseSettings
    {
        private readonly Queue<float> _fpsHistory = new();
        private readonly Queue<float> _cpuHistory = new();
        private readonly Queue<float> _memoryHistory = new();
        
        private ProfilerRecorder _cpuRecorder;
        
        private int _historyCapacity = 1000;
        
        private float _currentFPS;
        private float _avarageFPS;
        
        private float _currentCpuUsage;
        private float _averageCpuUsage;
        private float _cpuUsageTotal;
        private int _cpuSampleCount;
        
        private float _totalSystemMemory;
        private float _averageMemoryUsage;
        private float _currentMemoryUsage;
        
        private bool _displayFPS;
        private bool _displayCPU;
        private bool _displayMemory;
        
        private bool IsAnyGraphVisible => _displayFPS || _displayCPU || _displayMemory;

        #region Event Handlers

        public override void Initialize()
        {
            base.Initialize();
            ManageCpuProfiler(RecorderCommand.Start);
            GetTotalSystemMemory();
            Debug.Log("ProfilingSettings initialized.");
        }

        public override void Dispose()
        {
            base.Dispose();
            ManageCpuProfiler(RecorderCommand.Stop);
            Debug.Log("ProfilingSettings disposed.");
        }
        
        private void GetTotalSystemMemory()
        {
            _totalSystemMemory = SystemInfo.systemMemorySize;
        }
        
        private void ManageCpuProfiler(RecorderCommand command)
        {
#if UNITY_2022_1_OR_NEWER
            switch (command)
            {
                case RecorderCommand.Start:
                    _cpuRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Main Thread");
                    break;
                case RecorderCommand.Stop:
                    _cpuRecorder.Dispose();
                    break;
            }
#endif
        }

        protected override void Update()
        {
#if UNITY_2022_1_OR_NEWER
            // Update CPU value
            if (_cpuRecorder is { Valid: true, Count: > 0 })
            {
                long sum = 0;
                for (int i = 0; i < _cpuRecorder.Count; i++)
                {
                    sum += _cpuRecorder.GetSample(i).Value;
                }
                var avgCpuUsageMicro = sum / (float)_cpuRecorder.Count;
                _currentCpuUsage = avgCpuUsageMicro / 1000000f; // Microseconds to milliseconds (1e6)
            }
#endif
            // Add CPU value to history
            _cpuHistory.Enqueue(_currentCpuUsage);
            if (_cpuHistory.Count > HistoryCapacity)
            {
                _cpuHistory.Dequeue();
            }
            _averageCpuUsage = _cpuHistory.Average();
            
            // Update FPS value
            _currentFPS = 1.0f / Time.unscaledDeltaTime;
            _fpsHistory.Enqueue(_currentFPS);
            if (_fpsHistory.Count > HistoryCapacity)
            {
                _fpsHistory.Dequeue();
            }
            _avarageFPS = _fpsHistory.Average();
            
            // Update memory value (in MB)
            _currentMemoryUsage = (float)GC.GetTotalMemory(false) / (1024 * 1024);
            _memoryHistory.Enqueue(_currentMemoryUsage);
            if (_memoryHistory.Count > HistoryCapacity)
            {
                _memoryHistory.Dequeue();
            }
            _averageMemoryUsage = _memoryHistory.Average();
        }
        
        #endregion

        #region Buttons
        
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
        
        #endregion

        #region History Capacity
        
        [PropertyOrder(6)]
        [ShowIf("IsAnyGraphVisible")]
        [BoxGroup("Metrics")]
        [LabelText("History Capacity")]
        [ShowInInspector]
        [SuffixLabel("samples")]
        [ReadOnlyTextField, PropertyRange(1000, 10000)]
        public int HistoryCapacity 
        {
            get => _historyCapacity;
            set {
                _historyCapacity = value;
                
                // Clean up the queues if they currently contain more elements than permitted
                TrimQueue(_fpsHistory, _historyCapacity);
                TrimQueue(_cpuHistory, _historyCapacity);
                TrimQueue(_memoryHistory, _historyCapacity);
            }
        }
        
        #endregion
        
        #region FPS Graph
        
        [PropertyOrder(7)]
        [ShowIf("_displayFPS")]
        [BoxGroup("Metrics/FPSGraph", LabelText = "FPS Graph")]
        [LabelText("FPS")]
        [ShowInInspector]
        [ReadOnly, SuffixLabel("FPS", Overlay = true)]
        public string Fps => $"{_currentFPS:0.00}";
        
        [PropertyOrder(7)]
        [ShowIf("_displayFPS")]
        [BoxGroup("Metrics/FPSGraph", LabelText = "FPS Graph")]
        [OnInspectorGUI] private void Space3() => GUILayout.Space(5);
        
        [PropertyOrder(7)]
        [ShowIf("_displayFPS")]
        [BoxGroup("Metrics/FPSGraph", LabelText = "FPS Graph")]
        [LabelText("Average FPS")]
        [ShowInInspector]
        [ReadOnly, SuffixLabel("FPS", Overlay = true)]
        public string AverageFps => $"{_avarageFPS:0.00}";
        
        [PropertyOrder(7)]
        [ShowIf("_displayFPS")]
        [BoxGroup("Metrics/FPSGraph", LabelText = "FPS Graph")]
        [OnInspectorGUI] private void Space4() => GUILayout.Space(5);
        
        [PropertyOrder(7)]
        [ShowIf("_displayFPS")]
        [BoxGroup("Metrics/FPSGraph", LabelText = "FPS Graph")]
        [OnInspectorGUI]
        private void DrawFpsGraph()
        {
            var rect = GUILayoutUtility.GetRect(100, 100);
            var maxFps = Mathf.Max(60f, _fpsHistory.Max());
            DrawGraph(_fpsHistory, rect, Color.cyan, maxFps, fps => fps);
        }
        
        #endregion

        #region CPU Graph
        
        [PropertyOrder(8)]
        [ShowIf("_displayCPU")]
        [BoxGroup("Metrics/CPUGraph", LabelText = "CPU Usage Graph")]
        [LabelText("CPU Usage")]
        [ShowInInspector]
        [ReadOnly, SuffixLabel("ms", Overlay = true)]
        public string CpuUsage => $"{_currentCpuUsage:0.00} ({(_currentCpuUsage*100/16.6f):0.0}%)";
        
        [PropertyOrder(8)]
        [ShowIf("_displayCPU")]
        [BoxGroup("Metrics/CPUGraph", LabelText = "CPU Usage Graph")]
        [OnInspectorGUI] private void Space5() => GUILayout.Space(5);
        
        [PropertyOrder(8)]
        [ShowIf("_displayCPU")]
        [BoxGroup("Metrics/CPUGraph", LabelText = "CPU Graph")]
        [LabelText("Average CPU Usage")]
        [ShowInInspector]
        [ReadOnly, SuffixLabel("ms", Overlay = true)]
        public string AverageCpuUsage => $"{_averageCpuUsage:0.00} ({(_averageCpuUsage*100/16.6f):0.0}%)";
        
        [PropertyOrder(8)]
        [ShowIf("_displayCPU")]
        [BoxGroup("Metrics/CPUGraph", LabelText = "CPU Graph")]
        [OnInspectorGUI] private void Space6() => GUILayout.Space(5);
        
        [PropertyOrder(8)]
        [ShowIf("_displayCPU")]
        [BoxGroup("Metrics/CPUGraph", LabelText = "CPU Usage Graph")]
        [ShowInInspector]
        [OnInspectorGUI]
        private void DrawCpuGraph()
        {
            var rect = GUILayoutUtility.GetRect(100, 100);
            var maxCpu = Mathf.Max(16.6f, _cpuHistory.Max());
            DrawGraph(_cpuHistory, rect, Color.green, maxCpu, cpu => cpu);
        }
        
        #endregion

        #region Memory Graph
        
        [PropertyOrder(9)]
        [ShowIf("_displayMemory")]
        [BoxGroup("Metrics/MemoryGraph", LabelText = "Memory Graph")]
        [LabelText("Memory Usage")]
        [ShowInInspector]
        [ReadOnly, SuffixLabel("MiB", Overlay = true)]
        public string MemoryUsage => $"{_currentMemoryUsage:0.00} / {_totalSystemMemory:0.00} ({(_currentMemoryUsage/_totalSystemMemory*100):0.0}%)";
        
        [PropertyOrder(9)]
        [ShowIf("_displayMemory")]
        [BoxGroup("Metrics/MemoryGraph", LabelText = "Memory Graph")]
        [OnInspectorGUI] private void Space7() => GUILayout.Space(5);
        
        [PropertyOrder(9)]
        [ShowIf("_displayMemory")]
        [BoxGroup("Metrics/MemoryGraph", LabelText = "Memory Graph")]
        [LabelText("Average Memory Usage")]
        [ShowInInspector]
        [ReadOnly, SuffixLabel("MiB", Overlay = true)]
        public string AverageMemoryUsage => $"{_averageMemoryUsage:0.00} / {_totalSystemMemory:0.00} ({(_averageMemoryUsage/_totalSystemMemory*100):0.0}%)";
        
        [PropertyOrder(9)]
        [ShowIf("_displayMemory")]
        [BoxGroup("Metrics/MemoryGraph", LabelText = "Memory Graph")]
        [OnInspectorGUI]
        private void DrawMemoryGraph()
        {
            var rect = GUILayoutUtility.GetRect(100, 100);
            DrawGraph(_memoryHistory, rect, Color.magenta, _totalSystemMemory, mem => mem);
        }
        
        #endregion
        
        #region Button Utils
        
        private string DisplayFPSName => _displayFPS ? "Display FPS : ON" : "Display FPS : OFF";
        private string DisplayCPUName => _displayCPU ? "CPU Display : ON" : "Display CPU : OFF";
        private string DisplayMemoryName => _displayMemory ? "Memory Display : ON" : "Display Memory : OFF";

        private Color DisplayFPSColor => GetButtonColor(_displayFPS);
        private Color DisplayCPUColor => GetButtonColor(_displayCPU);
        private Color DisplayMemoryColor => GetButtonColor(_displayMemory);
        
        #endregion
    }
}