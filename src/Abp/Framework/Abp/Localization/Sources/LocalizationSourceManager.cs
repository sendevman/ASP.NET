using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Configuration;
using Abp.Dependency;
using Abp.Startup.Configuration;

namespace Abp.Localization.Sources
{
    /// <summary>
    /// This class is used to manage localization sources by implementing <see cref="ILocalizationSourceManager"/>. See <see cref="ILocalizationSource"/>.
    /// </summary>
    public class LocalizationSourceManager : ILocalizationSourceManager, ISingletonDependency
    {
        private static bool IsEnabled
        {
            get
            {
                return AbpConfiguration.Instance.Localization.IsEnabled;
            }
        }

        private readonly IDictionary<string, ILocalizationSource> _sources;
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public LocalizationSourceManager()
        {
            _sources = new Dictionary<string, ILocalizationSource>();
        }

        public void RegisterSource(ILocalizationSource source)
        {
            if (!IsEnabled)
            {
                return;
            }

            if (_sources.ContainsKey(source.Name))
            {
                throw new AbpException("There is already a source with name: " + source.Name);
            }

            _sources[source.Name] = source;
            source.Initialize();
        }

        public ILocalizationSource GetSource(string name)
        {
            if (!IsEnabled)
            {
                return NullLocalizationSource.Instance;
            }

            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            
            ILocalizationSource source;
            if (!_sources.TryGetValue(name, out source))
            {
                throw new AbpException("Can not find a source with name: " + name);
            }

            return source;
        }

        public IReadOnlyList<ILocalizationSource> GetAllSources()
        {
            return _sources.Values.ToImmutableList();
        }
    }
}