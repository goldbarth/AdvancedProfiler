using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Tools.SerializedSettings.Base
{
    public abstract class BaseSettings : ISettings
    {
        public virtual void Initialize()
        {
            EditorApplication.update += Update;
        }
        public virtual void Dispose()
        {
            EditorApplication.update -= Update;
        }
        
        protected abstract void Update();
        
        protected void TrimQueue(Queue<float> queue, int maxCapacity)
        {
            while(queue.Count > maxCapacity)
            {
                queue.Dequeue();
            }
        }
        
        protected virtual Color GetButtonColor(bool isActive)
        {
            return isActive ? GetButtonOnColor() : GetButtonOffColor();
        }

        protected virtual Color GetButtonOnColor()
        {
            return new Color(0.6f, 1.0f, 0.6f);
        }

        protected virtual Color GetButtonOffColor()
        {
            return new Color(1.0f, 0.6f, 0.6f);
        }
        
        protected virtual void DrawGraph<T>(
            Queue<T> history,
            Rect rect,
            Color lineColor,
            float maxValue,
            Func<T, float> valueSelector,
            Color backgroundColor = default)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            EditorGUI.DrawRect(rect, backgroundColor != default 
                ? backgroundColor 
                : new Color(0.12f, 0.12f, 0.12f));

            Handles.BeginGUI();
            Handles.color = lineColor;
        
            var count = history.Count;
            if (count < 2)
            {
                Handles.EndGUI();
                return;
            }
        
            var points = new Vector3[count];
            var historyArray = history.ToArray();
        
            for (int i = 0; i < count; i++)
            {
                var normalizedValue = Mathf.Clamp01(valueSelector(historyArray[i]) / maxValue);
                var x = rect.x + (rect.width / (count - 1)) * i;
                var y = rect.y + rect.height * (1 - normalizedValue);
                points[i] = new Vector3(x, y, 0);
            }
        
            Handles.DrawAAPolyLine(2, points);
            Handles.EndGUI();
        
            DrawGuideLines(rect);
        }
        
        protected virtual void DrawGuideLines(Rect rect)
        {
            // 100%
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
    }
}