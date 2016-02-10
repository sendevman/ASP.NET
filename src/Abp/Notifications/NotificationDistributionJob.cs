﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.BackgroundJobs;
using Abp.Configuration;
using Abp.Dependency;
using Abp.Domain.Uow;
using Abp.Extensions;
using Abp.Threading;
using Castle.Core.Internal;

namespace Abp.Notifications
{
    /// <summary>
    /// This background job distributes notifications to users.
    /// </summary>
    public class NotificationDistributionJob : BackgroundJob<NotificationDistributionJobArgs>, ITransientDependency
    {
        /// <summary>
        /// Referece to <see cref="IRealTimeNotifier"/>.
        /// </summary>
        public IRealTimeNotifier RealTimeNotifier { get; set; }

        private readonly INotificationDefinitionManager _notificationDefinitionManager;
        private readonly INotificationStore _notificationStore;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationDistributionJob"/> class.
        /// </summary>
        public NotificationDistributionJob(
            INotificationDefinitionManager notificationDefinitionManager,
            INotificationStore notificationStore)
        {
            _notificationDefinitionManager = notificationDefinitionManager;
            _notificationStore = notificationStore;

            RealTimeNotifier = NullRealTimeNotifier.Instance;
        }

        public override void Execute(NotificationDistributionJobArgs args)
        {
            AsyncHelper.RunSync(() => ExecuteAsync(args));
        }

        private async Task ExecuteAsync(NotificationDistributionJobArgs args)
        {
            var notificationInfo = await _notificationStore.GetNotificationOrNullAsync(args.NotificationId);
            if (notificationInfo == null)
            {
                Logger.Warn("NotificationDistributionJob can not continue since could not found notification by id: " + args.NotificationId);
                return;
            }

            Notification notification;

            try
            {
                notification = notificationInfo.ToNotification();
            }
            catch (Exception)
            {
                Logger.Warn("NotificationDistributionJob can not continue since could not convert NotificationInfo to Notification for NotificationId: " + args.NotificationId);
                return;
            }

            var userIds = await GetUserIds(notificationInfo);

            var userNotificationInfos = userIds
                .Select(userId => new UserNotificationInfo(userId, notificationInfo.Id))
                .ToList();

            await SaveUserNotifications(userNotificationInfos);

            try
            {
                var userNotifications = userNotificationInfos
                    .Select(uni => uni.ToUserNotification(notification))
                    .ToArray();

                await RealTimeNotifier.SendNotificationsAsync(userNotifications);
            }
            catch (Exception ex)
            {
                Logger.Warn(ex.ToString(), ex);
            }
        }

        [UnitOfWork]
        protected virtual async Task<long[]> GetUserIds(NotificationInfo notificationInfo)
        {
            List<long> userIds;

            if (!notificationInfo.UserIds.IsNullOrEmpty())
            {
                //Directly get from UserIds
                userIds = notificationInfo
                    .UserIds
                    .Split(",")
                    .Select(uidAsStr => uidAsStr.To<long>())
                    .Where(uid => SettingManager.GetSettingValueForUser<bool>(NotificationSettingNames.ReceiveNotifications, null, uid))
                    .ToList();
            }
            else
            {
                //Get subscribed users

                var tenantIds = GetTenantIds(notificationInfo);

                List<NotificationSubscriptionInfo> subscriptions;

                using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant))
                {
                    if (tenantIds.IsNullOrEmpty())
                    {
                        //Get all subscribed users of all tenants
                        subscriptions = await _notificationStore.GetSubscriptionsAsync(
                            notificationInfo.NotificationName,
                            notificationInfo.EntityTypeName,
                            notificationInfo.EntityId
                            );
                    }
                    else
                    {
                        //Get all subscribed users of specified tenant(s)
                        subscriptions = await _notificationStore.GetSubscriptionsAsync(
                            tenantIds,
                            notificationInfo.NotificationName,
                            notificationInfo.EntityTypeName,
                            notificationInfo.EntityId
                            );
                    }

                    //Remove invalid subscriptions
                    var invalidSubscriptions = new Dictionary<Guid, NotificationSubscriptionInfo>();

                    foreach (var subscription in subscriptions)
                    {
                        if (!await _notificationDefinitionManager.IsAvailableAsync(notificationInfo.NotificationName, subscription.TenantId, subscription.UserId) ||
                            !SettingManager.GetSettingValueForUser<bool>(NotificationSettingNames.ReceiveNotifications, subscription.TenantId, subscription.UserId))
                        {
                            invalidSubscriptions[subscription.Id] = subscription;
                        }
                    }

                    subscriptions.RemoveAll(s => invalidSubscriptions.ContainsKey(s.Id));
                }

                //Get user ids
                userIds = subscriptions
                    .Select(s => s.UserId)
                    .ToList();
            }

            if (!notificationInfo.ExcludedUserIds.IsNullOrEmpty())
            {
                //Exclude specified users.
                var excludedUserIds = notificationInfo
                    .ExcludedUserIds
                    .Split(",")
                    .Select(uidAsStr => uidAsStr.To<long>())
                    .ToList();

                userIds.RemoveAll(uid => excludedUserIds.Contains(uid));
            }

            return userIds.ToArray();
        }

        private static int?[] GetTenantIds(NotificationInfo notificationInfo)
        {
            if (notificationInfo.TenantIds.IsNullOrEmpty())
            {
                return null;
            }

            return notificationInfo
                .TenantIds
                .Split(",")
                .Select(uidAsStr => uidAsStr == "null" ? (int?)null : (int?)uidAsStr.To<int>())
                .ToArray();
        }

        [UnitOfWork]
        protected virtual async Task SaveUserNotifications(IEnumerable<UserNotificationInfo> userNotifications)
        {
            foreach (var userNotification in userNotifications)
            {
                await _notificationStore.InsertUserNotificationAsync(userNotification);
            }

            await CurrentUnitOfWork.SaveChangesAsync(); //To get Ids of the notifications
        }
    }
}
