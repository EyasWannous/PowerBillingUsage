using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Moq;
using PowerBillingUsage.Domain.Abstractions.Helpers;
using PowerBillingUsage.Domain.Abstractions.RegisteringDependencies;
using PowerBillingUsage.Domain.Abstractions.Repositories;
using PowerBillingUsage.Domain.Abstractions.Services;
using PowerBillingUsage.Domain.Bills;
using PowerBillingUsage.Domain.Enums;
using PowerBillingUsage.Infrastructure.EntityFramework.Repositories;
using PowerBillingUsage.Infrastructure.EntityFramework;
using PowerBillingUsage.Infrastructure.Helpers;
using System.Linq.Expressions;
using FluentAssertions;

namespace PowerBillingUsage.Infrastructure.Test.EntityFrameworkTests.RepositoriesTests;


public class ReadRepositoryTests
{
    private static readonly DateTime DefaultStartDate = new(2025, 01, 01, 20, 10, 00);
    private static readonly DateTime DefaultEndDate = new(2025, 01, 31, 03, 15, 00);

    private readonly PowerBillingUsageReadDbContext _readContext;
    private readonly ReadRepository<BillReadModel, BillId> _readRepository;
    private readonly Mock<IHybridCacheService> _mockCacheService;
    private readonly ICacheKeyHelper<BillReadModel> _cacheKeyHelper;

    public ReadRepositoryTests()
    {
        var databaseName = Guid.NewGuid().ToString();
        var inMemoryDbRoot = new InMemoryDatabaseRoot();

        var options = new DbContextOptionsBuilder<PowerBillingUsageReadDbContext>()
            .UseInMemoryDatabase(databaseName: databaseName, databaseRoot: inMemoryDbRoot)
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
            .Options;

        _readContext = new PowerBillingUsageReadDbContext(options);

        _mockCacheService = new Mock<IHybridCacheService>();
        _cacheKeyHelper = new CacheKeyHelper<BillReadModel>();

        _readRepository = new ReadRepository<BillReadModel, BillId>(_readContext, _mockCacheService.Object, _cacheKeyHelper);

        SeedDatabase().Wait();
    }

    private async Task SeedDatabase()
    {
        var bills = new List<BillReadModel>
        {
            CreateSampleBillReadModel(),
            CreateSampleBillReadModel(),
            CreateSampleBillReadModel()
        };

        foreach (var bill in bills)
        {
            await _readContext.Set<BillReadModel>().AddAsync(bill);
        }

        await _readContext.SaveChangesAsync();
    }

    private BillReadModel CreateSampleBillReadModel()
    {
        return new BillReadModel
        {
            Id = new(Guid.NewGuid()),
            BillingTypeValue = BillingType.Residential.Value,
            StartAt = DefaultStartDate,
            EndAt = DefaultEndDate,
            BreakDowns =
            [
                new BillDetailReadModel
                {
                    Id = new(Guid.NewGuid()),
                    TierName = "Up to 160 KWh",
                    Consumption = 160,
                    Rate = 0.05m,
                    Total = 8.00m
                },
                new BillDetailReadModel
                {
                    Id = new(Guid.NewGuid()),
                    TierName = "Up to 300 KWh",
                    Consumption = 140,
                    Rate = 0.10m,
                    Total = 14.00m
                },
                new BillDetailReadModel
                {
                    Id = new(Guid.NewGuid()),
                    TierName = "Up to 500 KWh",
                    Consumption = 150,
                    Rate = 0.12m,
                    Total = 18.00m
                }
            ]
        };
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnReadModel()
    {
        var bill = await _readContext.Set<BillReadModel>().FirstAsync();
        var keyOne = _cacheKeyHelper.MakeKeyOne(bill.Id);

        _mockCacheService
            .Setup(x => x.GetOrCreateAsync(
                keyOne,
                It.IsAny<Func<CancellationToken, ValueTask<BillReadModel?>>>(),
                HybridCacheEntryFlags.None,
                It.Is<IEnumerable<string>>(tags => tags.Contains(keyOne)),
                It.IsAny<TimeSpan?>(),
                null,
                It.IsAny<CancellationToken>()))
            .Returns(async (
                string key,
                Func<CancellationToken, ValueTask<BillReadModel?>> factory,
                HybridCacheEntryFlags flags,
                IEnumerable<string>? tags,
                TimeSpan? expiration,
                TimeSpan? localExpiration,
                CancellationToken cancellationToken) =>
            {
                return await factory(cancellationToken);
            });

        var result = await _readRepository.GetByIdAsync(bill.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(bill.Id);
        result.BillingTypeValue.Should().Be(bill.BillingTypeValue);
    }

    [Fact]
    public async Task GetListAsync_ShouldReturnAllReadModels()
    {
        // Arrange
        _mockCacheService
            .Setup(x => x.GetOrCreateAsync<IEnumerable<BillReadModel>>(
                _cacheKeyHelper.KeyAll,
                It.IsAny<Func<CancellationToken, ValueTask<IEnumerable<BillReadModel>>>>(),
                HybridCacheEntryFlags.None,
                It.Is<IEnumerable<string>>(tags => tags.Contains(_cacheKeyHelper.KeyAll)),
                It.IsAny<TimeSpan?>(),
                null,
                It.IsAny<CancellationToken>()))
            .Returns(async (
                string key,
                Func<CancellationToken, ValueTask<IEnumerable<BillReadModel>>> factory,
                HybridCacheEntryFlags flags,
                IEnumerable<string>? tags,
                TimeSpan? expiration,
                TimeSpan? localExpiration,
                CancellationToken cancellationToken) =>
            {
                return await factory(cancellationToken);
            });

        var result = await _readRepository.GetListAsync();

        result.Should().NotBeNull();
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetQueryableWithDetailsAsync_ShouldIncludeNavigationProperties()
    {
        // Arrange
        var navigationProps = new List<Expression<Func<BillReadModel, object>>> { b => b.BreakDowns };

        // Act
        var result = await _readRepository.GetQueryableWithDetailsAsync(navigationProps);
        var bills = await result.ToListAsync();

        // Assert
        bills.Should().NotBeNull();
        bills.Should().HaveCount(3);

        // Check that breakdowns are included
        foreach (var bill in bills)
        {
            bill.BreakDowns.Should().NotBeNull();
            bill.BreakDowns.Should().HaveCountGreaterThan(0);
        }
    }
}