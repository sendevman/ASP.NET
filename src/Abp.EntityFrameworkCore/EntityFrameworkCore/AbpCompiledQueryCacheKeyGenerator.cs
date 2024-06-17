using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;

namespace Abp.EntityFrameworkCore;

public class AbpCompiledQueryCacheKeyGenerator : ICompiledQueryCacheKeyGenerator
{
    protected ICompiledQueryCacheKeyGenerator InnerCompiledQueryCacheKeyGenerator { get; }
    protected ICurrentDbContext CurrentContext { get; }
    protected AbpEfCoreCurrentDbContext AbpEfCoreCurrentDbContext { get; }

    public AbpCompiledQueryCacheKeyGenerator(
        ICompiledQueryCacheKeyGenerator innerCompiledQueryCacheKeyGenerator,
        ICurrentDbContext currentContext,
        AbpEfCoreCurrentDbContext abpEfCoreCurrentDbContext)
    {
        InnerCompiledQueryCacheKeyGenerator = innerCompiledQueryCacheKeyGenerator;
        CurrentContext = currentContext;
        AbpEfCoreCurrentDbContext = abpEfCoreCurrentDbContext;
    }

    public virtual object GenerateCacheKey(Expression query, bool async)
    {
        var cacheKey = InnerCompiledQueryCacheKeyGenerator.GenerateCacheKey(query, async);
        if (CurrentContext.Context is AbpDbContext abpDbContext)
        {
            AbpEfCoreCurrentDbContext.Current.Value = abpDbContext;
            return new AbpCompiledQueryCacheKey(cacheKey, abpDbContext.GetGlobalFilterCompiledQueryCacheKey());
        }

        return cacheKey;
    }

    private readonly struct AbpCompiledQueryCacheKey : IEquatable<AbpCompiledQueryCacheKey>
    {
        private readonly object _compiledQueryCacheKey;
        private readonly string _currentFilterCacheKey;

        public AbpCompiledQueryCacheKey(object compiledQueryCacheKey, string currentFilterCacheKey)
        {
            _compiledQueryCacheKey = compiledQueryCacheKey;
            _currentFilterCacheKey = currentFilterCacheKey;
        }

        public override bool Equals(object obj)
        {
            return obj is AbpCompiledQueryCacheKey key && Equals(key);
        }

        public bool Equals(AbpCompiledQueryCacheKey other)
        {
            return _compiledQueryCacheKey.Equals(other._compiledQueryCacheKey) &&
                   _currentFilterCacheKey == other._currentFilterCacheKey;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_compiledQueryCacheKey, _currentFilterCacheKey);
        }
    }
}
