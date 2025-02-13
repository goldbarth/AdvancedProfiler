using System;
using UnityEngine;

namespace Tools.SerializedSettings
{
    [Serializable]
    public abstract class BaseSettings
    {
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
    }
}