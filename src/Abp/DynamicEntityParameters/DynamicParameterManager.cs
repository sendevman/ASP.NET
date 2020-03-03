﻿using System;
using System.Threading.Tasks;
using System.Transactions;
using Abp.Dependency;
using Abp.Domain.Uow;
using Abp.Runtime.Caching;

namespace Abp.DynamicEntityParameters
{
    public class DynamicParameterManager : IDynamicParameterManager, ITransientDependency
    {
        private readonly ICacheManager _cacheManager;
        private readonly IDynamicParameterStore _dynamicParameterStore;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IDynamicEntityParameterDefinitionManager _dynamicEntityParameterDefinitionManager;

        public const string CacheName = "AbpZeroDynamicParameterCache";
        private ITypedCache<int, DynamicParameter> DynamicParameterCache => _cacheManager.GetCache<int, DynamicParameter>(CacheName);

        public DynamicParameterManager(
            ICacheManager cacheManager,
            IDynamicParameterStore dynamicParameterStore,
            IUnitOfWorkManager unitOfWorkManager,
            IDynamicEntityParameterDefinitionManager dynamicEntityParameterDefinitionManager
            )
        {
            _cacheManager = cacheManager;
            _dynamicParameterStore = dynamicParameterStore;
            _unitOfWorkManager = unitOfWorkManager;
            _dynamicEntityParameterDefinitionManager = dynamicEntityParameterDefinitionManager;
        }

        public virtual DynamicParameter Get(int id)
        {
            return DynamicParameterCache.Get(id, () => _dynamicParameterStore.Get(id));
        }

        public virtual Task<DynamicParameter> GetAsync(int id)
        {
            return DynamicParameterCache.GetAsync(id, (i) => _dynamicParameterStore.GetAsync(id));
        }

        protected virtual void CheckInputType(string inputType)
        {
            if (!_dynamicEntityParameterDefinitionManager.ContainsInputType(inputType))
            {
                throw new ApplicationException($"Input type is invalid, if you want to use \"{inputType}\" input type, define it in DynamicEntityParameterDefinitionProvider.");
            }
        }

        public virtual void Add(DynamicParameter dynamicParameter)
        {
            CheckInputType(dynamicParameter.InputType);

            using (var uow = _unitOfWorkManager.Begin(TransactionScopeOption.RequiresNew))
            {
                _dynamicParameterStore.Add(dynamicParameter);
                uow.Complete();
            }

            DynamicParameterCache.Set(dynamicParameter.Id, dynamicParameter);
        }

        public virtual async Task AddAsync(DynamicParameter dynamicParameter)
        {
            CheckInputType(dynamicParameter.InputType);

            using (var uow = _unitOfWorkManager.Begin(TransactionScopeOption.RequiresNew))
            {
                await _dynamicParameterStore.AddAsync(dynamicParameter);
                await uow.CompleteAsync();
            }

            await DynamicParameterCache.SetAsync(dynamicParameter.Id, dynamicParameter);
        }

        public virtual void Update(DynamicParameter dynamicParameter)
        {
            CheckInputType(dynamicParameter.InputType);

            using (var uow = _unitOfWorkManager.Begin(TransactionScopeOption.RequiresNew))
            {
                _dynamicParameterStore.Update(dynamicParameter);
                uow.Complete();
            }

            DynamicParameterCache.Set(dynamicParameter.Id, dynamicParameter);
        }

        public virtual async Task UpdateAsync(DynamicParameter dynamicParameter)
        {
            CheckInputType(dynamicParameter.InputType);

            using (var uow = _unitOfWorkManager.Begin(TransactionScopeOption.RequiresNew))
            {
                await _dynamicParameterStore.UpdateAsync(dynamicParameter);
                await uow.CompleteAsync();
            }

            await DynamicParameterCache.SetAsync(dynamicParameter.Id, dynamicParameter);
        }

        public virtual void Delete(int id)
        {
            using (var uow = _unitOfWorkManager.Begin(TransactionScopeOption.RequiresNew))
            {
                _dynamicParameterStore.Delete(id);
                uow.Complete();
            }

            DynamicParameterCache.Remove(id);
        }

        public virtual async Task DeleteAsync(int id)
        {
            using (var uow = _unitOfWorkManager.Begin(TransactionScopeOption.RequiresNew))
            {
                await _dynamicParameterStore.DeleteAsync(id);
                await uow.CompleteAsync();
            }

            await DynamicParameterCache.RemoveAsync(id);
        }
    }
}
