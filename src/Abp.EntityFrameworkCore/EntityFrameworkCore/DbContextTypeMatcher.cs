using System;
using System.Collections.Generic;
using System.Linq;
using Abp.Collections.Extensions;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.MultiTenancy;

namespace Abp.EntityFrameworkCore
{
    public class DbContextTypeMatcher : IDbContextTypeMatcher, ISingletonDependency
    {
        private readonly ICurrentUnitOfWorkProvider _currentUnitOfWorkProvider;
        private readonly Dictionary<Type, List<Type>> _dbContextTypes;

        public DbContextTypeMatcher(ICurrentUnitOfWorkProvider currentUnitOfWorkProvider)
        {
            _currentUnitOfWorkProvider = currentUnitOfWorkProvider;
            _dbContextTypes = new Dictionary<Type, List<Type>>();
        }

        public void Populate(Type[] dbContextTypes)
        {
            foreach (var dbContextType in dbContextTypes)
            {
                var types = new List<Type>();

                AddWithBaseTypes(dbContextType, types);

                foreach (var type in types)
                {
                    Add(type, dbContextType);
                }
            }
        }

        //TODO: GetConcreteType method can be optimized by extracting/caching MultiTenancySideAttribute attributes for DbContexes.

        public virtual Type GetConcreteType(Type sourceDbContextType)
        {
            //TODO: This can also get MultiTenancySide to filter dbcontexes

            //Get possible concrete types for given DbContext type
            var allTargetTypes = _dbContextTypes.GetOrDefault(sourceDbContextType);

            if (allTargetTypes.IsNullOrEmpty())
            {
                //Not found any target type, return the given type if it's not abstract
                if (sourceDbContextType.IsAbstract)
                {
                    throw new AbpException("Could not find a concrete implementation of given DbContext type: " + sourceDbContextType.AssemblyQualifiedName);
                }

                return sourceDbContextType;
            }

            if (allTargetTypes.Count == 1)
            {
                //Only one type does exists, return it
                return allTargetTypes[0];
            }

            CheckCurrentUow();

            var currentTenancySide = GetCurrentTenancySide();

            var multiTenancySideContexes = GetMultiTenancySideContextTypes(allTargetTypes, currentTenancySide);

            if (multiTenancySideContexes.Count == 1)
            {
                return multiTenancySideContexes[0];
            }

            if (multiTenancySideContexes.Count > 1)
            {
                return GetDbContextTypeWithoutAutoRepositoryTypesAttribute(multiTenancySideContexes, sourceDbContextType, currentTenancySide);
            }

            return GetDbContextTypeWithoutAutoRepositoryTypesAttribute(allTargetTypes, sourceDbContextType, currentTenancySide);
        }

        private void CheckCurrentUow()
        {
            if (_currentUnitOfWorkProvider.Current == null)
            {
                throw new AbpException("GetConcreteType method should be called in a UOW.");
            }
        }

        private MultiTenancySides GetCurrentTenancySide()
        {
            return _currentUnitOfWorkProvider.Current.GetTenantId() == null
                       ? MultiTenancySides.Host
                       : MultiTenancySides.Tenant;
        }

        private static List<Type> GetMultiTenancySideContextTypes(List<Type> dbContextTypes, MultiTenancySides tenancySide)
        {
            return dbContextTypes.Where(type =>
            {
                var attrs = type.GetCustomAttributes(typeof(MultiTenancySideAttribute), true);
                if (attrs.IsNullOrEmpty())
                {
                    return false;
                }

                return ((MultiTenancySideAttribute)attrs[0]).Side.HasFlag(tenancySide);
            }).ToList();
        }

        private static Type GetDbContextTypeWithoutAutoRepositoryTypesAttribute(List<Type> dbContextTypes, Type sourceDbContextType, MultiTenancySides tenancySide)
        {
            var filteredTypes = dbContextTypes
                .Where(type => !type.IsDefined(typeof(AutoRepositoryTypesAttribute), true))
                .ToList();

            if (filteredTypes.Count == 1)
            {
                return filteredTypes[0];
            }

            throw new AbpException(string.Format(
                "Found more than one concrete type for given DbContext Type ({0}) define MultiTenancySideAttribute with {1}. Found types: {2}.",
                sourceDbContextType,
                tenancySide,
                dbContextTypes.Select(c => c.AssemblyQualifiedName).JoinAsString(", ")
                ));
        }

        private static void AddWithBaseTypes(Type dbContextType, List<Type> types)
        {
            types.Add(dbContextType);
            if (dbContextType != typeof(AbpDbContext))
            {
                AddWithBaseTypes(dbContextType.BaseType, types);
            }
        }

        private void Add(Type sourceDbContextType, Type targetDbContextType)
        {
            if (!_dbContextTypes.ContainsKey(sourceDbContextType))
            {
                _dbContextTypes[sourceDbContextType] = new List<Type>();
            }

            _dbContextTypes[sourceDbContextType].Add(targetDbContextType);
        }
    }
}