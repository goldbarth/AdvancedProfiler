using System.Collections.Generic;

namespace Tools.SerializedSettings.Base
{
    /// <summary>
    /// Composite settings that allows to combine multiple settings into one.
    /// </summary>
    public class CompositeSettings : ISettings
    {
        private readonly List<ISettings> _settings = new();

        public CompositeSettings(params ISettings[] settings)
        {
            _settings.AddRange(settings);
        }

        public void Initialize()
        {
            foreach (var setting in _settings)
            {
                setting.Initialize();
            }
        }

        public void Dispose()
        {
            foreach (var setting in _settings)
            {
                setting.Dispose();
            }
        }
    }
}