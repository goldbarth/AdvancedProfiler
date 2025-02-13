using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tools.SerializedSettings
{
    public class NetworkSettings : BaseSettings
    {
        private bool _trackPackets;
        private bool _trackConnections;

        public bool TrackPackets
        {
            get => _trackPackets;
            set => _trackPackets = value;
        }
        
        public bool TrackConnections
        {
            get => _trackConnections;
            set => _trackConnections = value;
        }
        
        #region Editor Exposure

        [PropertyOrder(0)]
        [BoxGroup("Network Settings", CenterLabel = true)]
        [Button("$TrackPacketsName"), GUIColor("TrackPacketsColor")]
        private void TrackPacketsButton()
        {
            _trackPackets = !_trackPackets;
        }
        
        [PropertyOrder(1)]
        [BoxGroup("Network Settings")]
        [OnInspectorGUI] private void Space1() => GUILayout.Space(5);

        [PropertyOrder(2)]
        [BoxGroup("Network Settings")]
        [Button("$TrackConnectionsName"), GUIColor("TrackConnectionsColor")]
        private void TrackConnectionsButton()
        {
            _trackConnections = !_trackConnections;
        }

        #endregion

        #region Button Utils
        
        private string TrackPacketsName => TrackPackets ? "Track Packets : ON" : "Track Packets : OFF";
        private string TrackConnectionsName => TrackConnections ? "Track Connections : ON" : "Track Connections : OFF";
        
        private Color TrackPacketsColor => GetButtonColor(TrackPackets);
        private Color TrackConnectionsColor => GetButtonColor(TrackConnections);
        
        #endregion
    }
}