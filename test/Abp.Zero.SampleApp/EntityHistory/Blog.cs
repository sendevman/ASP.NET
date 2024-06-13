﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Auditing;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace Abp.Zero.SampleApp.EntityHistory
{
    [Audited]
    public class Blog : AggregateRoot, IHasCreationTime
    {
        [DisableAuditing]
        public virtual string Name { get; set; }

        public virtual string Url { get; protected set; }

        public virtual DateTime CreationTime { get; set; }

        public virtual BlogEx More { get; set; }

        public virtual ICollection<Post> Posts { get; set; }

        public Blog()
        {

        }

        public Blog(string name, string url, string bloggerName)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentNullException(nameof(url));
            }

            Name = name;
            Url = url;
            More = new BlogEx { BloggerName = bloggerName };
        }

        public virtual void ChangeUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentNullException(nameof(url));
            }

            var oldUrl = Url;
            Url = url;
        }
    }

    [ComplexType]
    public class BlogEx
    {
        public virtual string BloggerName { get; set; }
    }
}
