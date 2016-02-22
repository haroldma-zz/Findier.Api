using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using Findier.Api.Infrastructure;

namespace Findier.Api.Services
{
    public class DtoService
    {
        private readonly ILifetimeScope _lifetimeScope;

        public DtoService(ILifetimeScope lifetimeScope)
        {
            _lifetimeScope = lifetimeScope;
        }

        public async Task<TT> CreateAsync<T, TT>(T entry)
        {
            var provider = GetProvider<T, TT>();
            return await provider.CreateAsync(entry);
        }

        public async Task<List<TT>> CreateListAsync<T, TT>(List<T> entries)
        {
            var list = new List<TT>();

            if (entries.Count == 0)
            {
                return list;
            }

            var provider = GetProvider<T, TT>();

            var dtos = new List<TT>();
            foreach (var entry in entries)
            {
                dtos.Add(await provider.CreateAsync(entry));
            }
            return dtos;
        }

        protected IDtoProvider<T, TT> GetProvider<T, TT>()
        {
            IDtoProvider<T, TT> provider;
            if (!_lifetimeScope.TryResolve(out provider))
            {
                throw new DtoProviderNotFoundException($"A DTO provider for {typeof (T)} was not found.");
            }
            return provider;
        }

        internal class DtoProviderException : Exception
        {
            public DtoProviderException(string msg) : base(msg)
            {
            }
        }

        internal class DtoProviderNotFoundException : DtoProviderException
        {
            public DtoProviderNotFoundException(string msg) : base(msg)
            {
            }
        }
    }
}