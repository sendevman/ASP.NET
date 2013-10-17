using System;
using System.Collections.Generic;

namespace Abp.WebApi.Controllers.Dynamic
{
    /// <summary>
    /// Used to store a controller information.
    /// </summary>
    internal class DynamicApiControllerInfo
    {
        public string AreaName { get; set; }

        /// <summary>
        /// Name of the controller.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Controller type.
        /// </summary>
        public Type Type { get; private set; }

        /// <summary>
        /// Proxied type.
        /// </summary>
        public Type ProxiedType { get; private set; }

        /// <summary>
        /// All actions of the controller.
        /// </summary>
        public IDictionary<string, DynamicApiActionInfo> Actions { get; private set; }

        /// <summary>
        /// Creates a new <see cref="DynamicApiControllerInfo"/> instance.
        /// </summary>
        /// <param name="areaName">Area name (can be null for default)</param>
        /// <param name="name">Name of the controller</param>
        /// <param name="type">Controller type</param>
        /// <param name="proxiedType">Proxied type</param>
        public DynamicApiControllerInfo(string areaName, string name, Type type, Type proxiedType)
        {
            AreaName = areaName;
            Name = name;
            Type = type;
            ProxiedType = proxiedType;
            Actions = new Dictionary<string, DynamicApiActionInfo>();
        }
    }
}