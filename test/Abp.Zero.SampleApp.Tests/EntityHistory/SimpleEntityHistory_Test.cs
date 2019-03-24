﻿using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.EntityHistory;
using Abp.Events.Bus.Entities;
using Abp.Extensions;
using Abp.Json;
using Abp.Threading;
using Abp.Timing;
using Abp.Zero.SampleApp.EntityHistory;
using Castle.MicroKernel.Registration;
using NSubstitute;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using Xunit;

namespace Abp.Zero.SampleApp.Tests.EntityHistory
{
    public class SimpleEntityHistory_Test : SampleAppTestBase
    {
        private readonly IRepository<Blog> _blogRepository;
        private readonly IRepository<Post, Guid> _postRepository;
        private readonly IRepository<Comment> _commentRepository;

        private IEntityHistoryStore _entityHistoryStore;

        public SimpleEntityHistory_Test()
        {
            _blogRepository = Resolve<IRepository<Blog>>();
            _postRepository = Resolve<IRepository<Post, Guid>>();
            _commentRepository = Resolve<IRepository<Comment>>();

            Resolve<IEntityHistoryConfiguration>().IsEnabledForAnonymousUsers = true;
        }

        protected override void PreInitialize()
        {
            base.PreInitialize();
            _entityHistoryStore = Substitute.For<IEntityHistoryStore>();
            LocalIocManager.IocContainer.Register(
                Component.For<IEntityHistoryStore>().Instance(_entityHistoryStore).LifestyleSingleton()
                );
        }

        #region CASES WRITE HISTORY

        [Fact]
        public void Should_Write_History_For_Tracked_Entities_Create()
        {
            /* Blog has Audited attribute. */

            var blog2Id = CreateBlogAndGetId();

            _entityHistoryStore.Received().SaveAsync(Arg.Is<EntityChangeSet>(
                s => s.EntityChanges.Count == 1 &&
                     s.EntityChanges[0].ChangeTime == s.EntityChanges[0].EntityEntry.As<DbEntityEntry>().Entity.As<IHasCreationTime>().CreationTime &&
                     s.EntityChanges[0].ChangeType == EntityChangeType.Created &&
                     s.EntityChanges[0].EntityId == blog2Id.ToJsonString(false, false) &&
                     s.EntityChanges[0].EntityTypeFullName == typeof(Blog).FullName &&
                     s.EntityChanges[0].PropertyChanges.Count == 2 && // Blog.Name, Blog.Url

                     // Check "who did this change"
                     s.ImpersonatorTenantId == AbpSession.ImpersonatorTenantId &&
                     s.ImpersonatorUserId == AbpSession.ImpersonatorUserId &&
                     s.TenantId == AbpSession.TenantId &&
                     s.UserId == AbpSession.UserId
            ));
        }

        [Fact]
        public void Should_Write_History_For_Tracked_Entities_Create_To_Database()
        {
            // Forward calls from substitute to implementation
            var entityHistoryStore = Resolve<EntityHistoryStore>();
            _entityHistoryStore.When(x => x.SaveAsync(Arg.Any<EntityChangeSet>()))
                .Do(callback => AsyncHelper.RunSync(() =>
                    entityHistoryStore.SaveAsync(callback.Arg<EntityChangeSet>()))
                );

            UsingDbContext((context) =>
            {
                context.EntityChanges.Count(e => e.TenantId == null).ShouldBe(0);
                context.EntityChangeSets.Count(e => e.TenantId == null).ShouldBe(0);
                context.EntityPropertyChanges.Count(e => e.TenantId == null).ShouldBe(0);
            });

            var justNow = Clock.Now;
            var blog2Id = CreateBlogAndGetId();

            UsingDbContext((context) =>
            {
                context.EntityChanges.Count(e => e.TenantId == null).ShouldBe(1);
                context.EntityChangeSets.Count(e => e.TenantId == null).ShouldBe(1);
                context.EntityChangeSets.Single().CreationTime.ShouldBeGreaterThan(justNow);
                context.EntityPropertyChanges.Count(e => e.TenantId == null).ShouldBe(2);
            });
        }

        [Fact]
        public void Should_Write_History_For_Tracked_Entities_Update()
        {
            /* Blog has Audited attribute. */

            var newValue = "http://testblog1-changed.myblogs.com";
            var originalValue = UpdateBlogUrlAndGetOriginalValue(newValue);

            _entityHistoryStore.Received().SaveAsync(Arg.Is<EntityChangeSet>(
                s => s.EntityChanges.Count == 1 &&
                     s.EntityChanges[0].ChangeType == EntityChangeType.Updated &&
                     s.EntityChanges[0].EntityId == s.EntityChanges[0].EntityEntry.As<DbEntityEntry>().Entity.As<IEntity>().Id.ToJsonString(false, false) &&
                     s.EntityChanges[0].EntityTypeFullName == typeof(Blog).FullName &&
                     s.EntityChanges[0].PropertyChanges.Count == 1 &&
                     s.EntityChanges[0].PropertyChanges.FirstOrDefault().NewValue == newValue.ToJsonString(false, false) &&
                     s.EntityChanges[0].PropertyChanges.FirstOrDefault().OriginalValue == originalValue.ToJsonString(false, false) &&
                     s.EntityChanges[0].PropertyChanges.FirstOrDefault().PropertyName == nameof(Blog.Url) &&
                     s.EntityChanges[0].PropertyChanges.FirstOrDefault().PropertyTypeFullName == typeof(Blog).GetProperty(nameof(Blog.Url)).PropertyType.FullName
            ));
        }

        [Fact]
        public void Should_Write_History_For_Tracked_Property_Foreign_Key()
        {
            /* Post.BlogId has Audited attribute. */

            var blogId = CreateBlogAndGetId();
            Guid post1Id;

            using (var uow = Resolve<IUnitOfWorkManager>().Begin())
            {
                var blog2 = _blogRepository.Single(b => b.Id == 2);
                var post1 = _postRepository.Single(p => p.Body == "test-post-1-body");
                post1Id = post1.Id;

                // Change foreign key by assigning navigation property
                post1.Blog = blog2;
                _postRepository.Update(post1);

                uow.Complete();
            }

            _entityHistoryStore.Received().SaveAsync(Arg.Is<EntityChangeSet>(
                s => s.EntityChanges.Count == 1 &&
                     s.EntityChanges[0].ChangeType == EntityChangeType.Updated &&
                     s.EntityChanges[0].EntityId == post1Id.ToJsonString(false, false) &&
                     s.EntityChanges[0].EntityTypeFullName == typeof(Post).FullName &&
                     s.EntityChanges[0].PropertyChanges.Count == 1 // Post.BlogId
            ));
        }

        [Fact]
        public void Should_Write_History_For_Tracked_Property_Foreign_Key_Collection()
        {
            using (var uow = Resolve<IUnitOfWorkManager>().Begin())
            {
                var blog1 = _blogRepository.Single(b => b.Name == "test-blog-1");
                var post5 = new Post { Blog = blog1, Title = "test-post-5-title", Body = "test-post-5-body" };

                // Change navigation property by adding into collection
                blog1.Posts.Add(post5);
                _blogRepository.Update(blog1);

                uow.Complete();
            }

            _entityHistoryStore.Received().SaveAsync(Arg.Is<EntityChangeSet>(
                s => s.EntityChanges.Count == 2 &&
                     s.EntityChanges[0].ChangeType == EntityChangeType.Created &&
                     s.EntityChanges[0].EntityTypeFullName == typeof(Post).FullName &&
                     s.EntityChanges[0].PropertyChanges.Count == 1 && // Post.BlogId

                     s.EntityChanges[1].ChangeType == EntityChangeType.Updated &&
                     s.EntityChanges[1].EntityTypeFullName == typeof(Blog).FullName &&
                     s.EntityChanges[1].PropertyChanges.Count == 0
            ));
        }

        [Fact]
        public void Should_Write_History_For_Tracked_Property_Foreign_Key_Shadow()
        {
            /* Comment has Audited attribute. */

            var post1KeyValue = new Dictionary<string, object>();
            var post2KeyValue = new Dictionary<string, object>();

            using (var uow = Resolve<IUnitOfWorkManager>().Begin())
            {
                var post2 = _postRepository.Single(p => p.Body == "test-post-2-body");
                post2KeyValue.Add("Id", post2.Id);

                var comment1 = _commentRepository.Single(c => c.Content == "test-comment-1-content");
                post1KeyValue.Add("Id", comment1.Post.Id);

                // Change foreign key by assigning navigation property
                comment1.Post = post2;
                _commentRepository.Update(comment1);

                uow.Complete();
            }

            _entityHistoryStore.Received().SaveAsync(Arg.Is<EntityChangeSet>(
                s => s.EntityChanges.Count == 1 &&
                     s.EntityChanges[0].PropertyChanges.Count == 1 &&
                     s.EntityChanges[0].PropertyChanges.First().PropertyName == "Post" &&
                     s.EntityChanges[0].PropertyChanges.First().PropertyTypeFullName == typeof(Post).FullName &&
                     s.EntityChanges[0].PropertyChanges.First().NewValue == post2KeyValue.ToJsonString(false, false) &&
                     s.EntityChanges[0].PropertyChanges.First().OriginalValue == post1KeyValue.ToJsonString(false, false)
            ));
        }

        [Fact]
        public void Should_Write_History_But_Not_For_Property_If_Disabled_History_Tracking()
        {
            /* Blog.Name has DisableAuditing attribute. */

            using (var uow = Resolve<IUnitOfWorkManager>().Begin())
            {
                var blog1 = _blogRepository.Single(b => b.Name == "test-blog-1");

                blog1.Name = null;
                _blogRepository.Update(blog1);

                uow.Complete();
            }

            _entityHistoryStore.Received().SaveAsync(Arg.Is<EntityChangeSet>(
                s => s.EntityChanges.Count == 1 &&
                     s.EntityChanges[0].ChangeType == EntityChangeType.Updated &&
                     s.EntityChanges[0].EntityId == s.EntityChanges[0].EntityEntry.As<DbEntityEntry>().Entity.As<IEntity>().Id.ToJsonString(false, false) &&
                     s.EntityChanges[0].EntityTypeFullName == typeof(Blog).FullName &&
                     s.EntityChanges[0].PropertyChanges.Count == 0
            ));
        }

        #endregion

        #region CASES DON'T WRITE HISTORY

        [Fact]
        public void Should_Not_Write_History_If_Disabled()
        {
            Resolve<IEntityHistoryConfiguration>().IsEnabled = false;

            /* Blog has Audited attribute. */

            var newValue = "http://testblog1-changed.myblogs.com";
            var originalValue = UpdateBlogUrlAndGetOriginalValue(newValue);

            _entityHistoryStore.DidNotReceive().SaveAsync(Arg.Any<EntityChangeSet>());
        }

        [Fact]
        public void Should_Not_Write_History_If_Property_Has_No_Audited_Attribute()
        {
            /* Post.Body does not have Audited attribute. */

            using (var uow = Resolve<IUnitOfWorkManager>().Begin())
            {
                var post1 = _postRepository.Single(p => p.Body == "test-post-1-body");

                post1.Body = null;
                _postRepository.Update(post1);

                uow.Complete();
            }

            _entityHistoryStore.DidNotReceive().SaveAsync(Arg.Any<EntityChangeSet>());
        }

        #endregion

        private int CreateBlogAndGetId()
        {
            int blog2Id;

            using (var uow = Resolve<IUnitOfWorkManager>().Begin())
            {
                var blog2 = new Blog("test-blog-2", "http://testblog2.myblogs.com");

                blog2Id = _blogRepository.InsertAndGetId(blog2);

                uow.Complete();
            }

            return blog2Id;
        }

        private string UpdateBlogUrlAndGetOriginalValue(string newValue)
        {
            string originalValue;

            using (var uow = Resolve<IUnitOfWorkManager>().Begin())
            {
                var blog1 = _blogRepository.Single(b => b.Name == "test-blog-1");
                originalValue = blog1.Url;

                blog1.ChangeUrl(newValue);
                _blogRepository.Update(blog1);

                uow.Complete();
            }

            return originalValue;
        }
    }

    #region Helpers

    internal static class IEnumerableExtensions
    {
        internal static EntityPropertyChange FirstOrDefault(this IEnumerable<EntityPropertyChange> enumerable)
        {
            var enumerator = enumerable.GetEnumerator();
            enumerator.MoveNext();
            return enumerator.Current;
        }
    }

    #endregion
}
