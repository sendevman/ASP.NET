﻿using System.Web.Mvc;
using Abp.Application.Authorization;

namespace Abp.Web.Mvc.Authorization
{
    /// <summary>
    /// This attribute is used on an action of an MVC <see cref="Controller"/>
    /// to make that action usable only by authorized users.
    /// </summary>
    public class AbpAuthorizeAttribute : AuthorizeAttribute, IFeatureBasedAuthorization
    {
        /// <summary>
        /// A list of features to authorize.
        /// A user is authorized if any of the features is allowed.
        /// </summary>
        public string[] Features { get; set; }

        /// <param name="singleFeature">
        /// A shortcut to create a AbpAuthorizeAttribute that has only one feature.
        /// If more than one feature is added, <see cref="Features"/> should be used.
        /// </param>
        public AbpAuthorizeAttribute(string singleFeature = null)
        {
            if (!string.IsNullOrEmpty(singleFeature))
            {
                Features = new[] { singleFeature };
            }
            else
            {
                Features = new string[0];
            }
        }

        protected override bool AuthorizeCore(System.Web.HttpContextBase httpContext)
        {
            if (!base.AuthorizeCore(httpContext))
            {
                return false;
            }

            return true;
        }
    }
}
