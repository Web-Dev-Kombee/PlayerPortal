using Microsoft.Extensions.DependencyInjection;

namespace PlayerPortal.Data.SqlDb
{
    public abstract class SqlDbRepository<TDbContext>
    {
        private readonly IServiceProvider _serviceProvider;

        protected SqlDbRepository(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected RepositoryScope<TDbContext> GetScope()
        {
            var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<TDbContext>();
            return new RepositoryScope<TDbContext>(scope, db);
        }

        protected class RepositoryScope<T> : IDisposable
        {
            private readonly IServiceScope _scope;

            public RepositoryScope(IServiceScope scope, T db)
            {
                _scope = scope;
                Db = db;
            }
            public T Db { get; }
            public void Dispose() => _scope.Dispose();
        }
    }
}
