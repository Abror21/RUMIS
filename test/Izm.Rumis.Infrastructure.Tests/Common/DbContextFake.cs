using Izm.Rumis.Application.Contracts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Izm.Rumis.Infrastructure.Tests.Common
{
    internal class DbContextFake : AppDbContext
    {
        public DbContextFake(
            DbContextOptions<AppDbContext> options,
            ICurrentUserService currentUserService,
            IMediator mediator
            ) : base(options, currentUserService, mediator) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // https://github.com/dotnet/efcore/issues/11926
            // example how to fake array property
            //modelBuilder
            //    .Entity<Domain.Entities.XXX>()
            //    .Property(e => e.Array)
            //    .HasConversion(
            //        v => new ArrayWrapper<string>(v),
            //        v => v.Values);
        }

        private struct ArrayWrapper
        {
            public ArrayWrapper(int[] values)
                => Values = values;

            public int[] Values { get; }
        }

        private struct ArrayWrapper<T>
        {
            public ArrayWrapper(T[] values)
                => Values = values;

            public T[] Values { get; }
        }
    }
}
