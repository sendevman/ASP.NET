using System;
using System.Collections.Generic;
using Abp.Timing;
using Abp.Zero.SampleApp.EntityHistory;
using Abp.Zero.SampleApp.EntityHistory.EFCore;
using Abp.Zero.SampleApp.EntityHistory.Nhibernate;
using NHibernate;

namespace Abp.Zero.SampleApp.NHibernate.TestDatas
{
    public class InitialBlogBuilder
    {
        private readonly ISession _session;

        public InitialBlogBuilder(ISession session)
        {
            _session = session;
        }

        public void Build()
        {
            SaveBlogs();
            SaveAdvertisements();
        }


        private void SaveAdvertisements()
        {
            _session.Save(new NhAdvertisement
            {
                Banner = "test-advertisement-1"
            });
        }

        private void SaveBlogs()
        {
            var blog1 = new NhBlog
            {
                Name = "test-blog-1",
                More = new NhBlogEx
                {
                    BloggerName = "blogger-1"
                }
            };

            _session.Save(blog1);

            var blog2 = new NhBlog
            {
                Name = "test-blog-2",
            };
            blog2.ChangeUrl("http://testblog2.myblogs.com");
            _session.Save(blog2);
        }
    }
}