using System;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Tools.Utils
{
    public class ReadOnlyTextFieldAttribute : Attribute { }
    
#if UNITY_EDITOR
    public class ReadOnlyTextSliderDrawer : OdinAttributeDrawer<ReadOnlyTextFieldAttribute, int>
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {
            // 1. Bereich aus PropertyRange holen
            int min = 0, max = 1;
            var rangeAttr = Property.GetAttribute<PropertyRangeAttribute>();
            if (rangeAttr != null)
            {
                min = (int)Convert.ToSingle(rangeAttr.Min);
                max = (int)Convert.ToSingle(rangeAttr.Max);
            }

            // 2. Gesamten Bereich holen
            var fullRect = EditorGUILayout.GetControlRect();
            
            // 3. Label zeichnen und den verbleibenden Bereich zurückbekommen
            var contentRect = EditorGUI.PrefixLabel(fullRect, label);

            // 4. Platz für Slider und Textfeld festlegen
            const float spacing = 5f;
            const float textFieldWidth = 50f;

            // Slider-Rechteck
            var sliderRect = contentRect;
            sliderRect.width -= (textFieldWidth + spacing);

            // Textfeld-Rechteck
            var textRect = new Rect(
                sliderRect.xMax + spacing, 
                sliderRect.y, 
                textFieldWidth, 
                sliderRect.height
            );

            // 5. Horizontalen Slider zeichnen (ohne Textfeld)
            var currentValue = ValueEntry.SmartValue;
            var newFloatValue = GUI.HorizontalSlider(
                sliderRect, 
                currentValue, 
                min, 
                max
            );

            // 6. Direktes Casting auf int (oder Rundung, wenn gewünscht)
            var newValue = Mathf.RoundToInt(newFloatValue);

            // 7. Das danebenstehende Textfeld schreibgeschützt anzeigen
            GUI.enabled = false;
            EditorGUI.IntField(textRect, newValue);
            GUI.enabled = true;

            // 8. Wert zuweisen
            ValueEntry.SmartValue = newValue;
        }
    }
#endif
}