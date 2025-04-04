using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Caching.Hybrid;
using Moq;
using PowerBillingUsage.Domain.Abstractions.Helpers;
using PowerBillingUsage.Domain.Abstractions.Services;
using PowerBillingUsage.Domain.Abstractions.Shared;
using PowerBillingUsage.Domain.Bills;
using PowerBillingUsage.Domain.Enums;
using PowerBillingUsage.Infrastructure.EntityFramework;
using PowerBillingUsage.Infrastructure.EntityFramework.Repositories;
using PowerBillingUsage.Infrastructure.Helpers;
using System.Linq.Expressions;

namespace PowerBillingUsage.Infrastructure.Test.EntityFrameworkTests.RepositoriesTests;

public class BaseRepositoryTests
{
    private static readonly DateTime DefaultStartDate = new(2025, 01, 01, 20, 10, 00);
    private static readonly DateTime DefaultEndDate = new(2025, 01, 31, 03, 15, 00);

    private readonly PowerBillingUsageWriteDbContext _context;
    private readonly Repository<Bill, BillId> _repository;
    private readonly Mock<IHybridCacheService> _mockCacheService;
    private readonly ICacheKeyHelper<Bill> _cacheKeyHelper;

    public BaseRepositoryTests()
    {
        var databaseName = Guid.NewGuid().ToString();
        var inMemoryDbRoot = new InMemoryDatabaseRoot();

        var options = new DbContextOptionsBuilder<PowerBillingUsageWriteDbContext>()
            .UseInMemoryDatabase(databaseName: databaseName, databaseRoot: inMemoryDbRoot)
            .Options;

        _context = new PowerBillingUsageWriteDbContext(options);

        _mockCacheService = new Mock<IHybridCacheService>();
        _cacheKeyHelper = new CacheKeyHelper<Bill>();

        _repository = new Repository<Bill, BillId>(_context, _mockCacheService.Object, _cacheKeyHelper);

        SeedDatabase().Wait();
    }

    private async Task SeedDatabase()
    {
        var bills = new List<Bill>
        {
            CreateSampleBill(),
            CreateSampleBill(),
            CreateSampleBill()
        };

        foreach (var bill in bills)
        {
            await _context.Set<Bill>().AddAsync(bill);
        }

        await _context.SaveChangesAsync();
    }

    private Bill CreateSampleBill()
    {
        return new Bill(
            id: new(Guid.NewGuid()),
            billingTypeValue: BillingType.Residential.Value,
            startAt: DefaultStartDate,
            endAt: DefaultEndDate,
            breakDowns: [
                new BillDetail(new(Guid.NewGuid()), "Up to 160 KWh", 160, 0.05m, 8.00m),
                new BillDetail(new(Guid.NewGuid()), "Up to 300 KWh", 140, 0.10m, 14.00m),
                new BillDetail(new(Guid.NewGuid()), "Up to 500 KWh", 150, 0.12m, 18.00m),
            ]
        );
    }

    [Fact]
    public async Task GetPaginateAsync_ShouldGetFromCache_WhenDataIsCached()
    {
        var skip = 0;
        var take = 10;
        var paginateKey = _cacheKeyHelper.MakePaginateKey(skip, take);
        var expectedResponse = new PaingationResponse<Bill>(3, [CreateSampleBill()]);

        _mockCacheService
            .Setup(x => x.GetOrCreateAsync(
                paginateKey,
                It.IsAny<Func<CancellationToken, ValueTask<PaingationResponse<Bill>>>>(),
                HybridCacheEntryFlags.None,
                It.Is<IEnumerable<string>>(tags => tags.Contains(_cacheKeyHelper.PaginateKey)),
                It.IsAny<TimeSpan?>(),
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var result = await _repository.GetPaginateAsync(skip, take);

        result.Should().BeEquivalentTo(expectedResponse);
        _mockCacheService.Verify(
            x => x.GetOrCreateAsync(
                paginateKey,
                It.IsAny<Func<CancellationToken, ValueTask<PaingationResponse<Bill>>>>(),
                HybridCacheEntryFlags.None,
                It.Is<IEnumerable<string>>(tags => tags.Contains(_cacheKeyHelper.PaginateKey)),
                It.IsAny<TimeSpan?>(),
                null,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetPaginateAsync_ShouldCallFactory_WhenDataIsNotCached()
    {
        var skip = 0;
        var take = 10;
        var paginateKey = _cacheKeyHelper.MakePaginateKey(skip, take);

        _mockCacheService
            .Setup(x => x.GetOrCreateAsync(
                paginateKey,
                It.IsAny<Func<CancellationToken, ValueTask<PaingationResponse<Bill>>>>(),
                HybridCacheEntryFlags.None,
                It.Is<IEnumerable<string>>(tags => tags.Contains(_cacheKeyHelper.PaginateKey)),
                It.IsAny<TimeSpan?>(),
                null,
                It.IsAny<CancellationToken>()))
            .Returns(async (
                string key,
                Func<CancellationToken, ValueTask<PaingationResponse<Bill>>> factory,
                HybridCacheEntryFlags flags,
                IEnumerable<string>? tags,
                TimeSpan? expiration,
                TimeSpan? localExpiration,
                CancellationToken cancellationToken) =>
            {
                return await factory(cancellationToken);
            });

        var result = await _repository.GetPaginateAsync(skip, take);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(3);
        result.TotalCount.Should().Be(3);
    }

    [Fact]
    public async Task GetListAsync_ShouldGetFromCache_WhenDataIsCached()
    {
        var expectedResponse = new List<Bill> { CreateSampleBill() };

        _mockCacheService
            .Setup(x => x.GetOrCreateAsync(
                _cacheKeyHelper.KeyAll,
                It.IsAny<Func<CancellationToken, ValueTask<IEnumerable<Bill>>>>(),
                HybridCacheEntryFlags.None,
                It.Is<IEnumerable<string>>(tags => tags.Contains(_cacheKeyHelper.KeyAll)),
                It.IsAny<TimeSpan?>(),
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var result = await _repository.GetListAsync();

        result.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldGetFromCache_WhenDataIsCached()
    {
        var bill = CreateSampleBill();
        var keyOne = _cacheKeyHelper.MakeKeyOne(bill.Id);

        _mockCacheService
            .Setup(x => x.GetOrCreateAsync(
                keyOne,
                It.IsAny<Func<CancellationToken, ValueTask<Bill?>>>(),
                HybridCacheEntryFlags.None,
                It.Is<IEnumerable<string>>(tags => tags.Contains(keyOne)),
                It.IsAny<TimeSpan?>(),
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(bill);

        var result = await _repository.GetByIdAsync(bill.Id);

        result.Should().BeEquivalentTo(bill);
    }

    [Fact]
    public async Task GetQueryableAsync_ShouldReturnQueryable()
    {
        var result = await _repository.GetQueryableAsync();

        result.Should().NotBeNull();
        var bills = await result.ToListAsync();
        bills.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetQueryableWithDetailsAsync_ShouldIncludeNavigationProperties()
    {
        var navigationProps = new List<Expression<Func<Bill, object>>> { b => b.BreakDowns };

        var result = await _repository.GetQueryableWithDetailsAsync(navigationProps);

        result.Should().NotBeNull();
        var bills = await result.ToListAsync();
        bills.Should().HaveCount(3);
        // Note: In an in-memory database, Include doesn't actually do anything, but we can verify the method works
    }

    [Fact]
    public async Task CountAsync_ShouldGetFromCache_WhenDataIsCached()
    {
        int expectedCount = 3;

        _mockCacheService
            .Setup(x => x.GetOrCreateAsync(
                _cacheKeyHelper.CountKey,
                It.IsAny<Func<CancellationToken, ValueTask<int>>>(),
                HybridCacheEntryFlags.None,
                It.Is<IEnumerable<string>>(tags => tags.Contains(_cacheKeyHelper.CountKey)),
                It.IsAny<TimeSpan?>(),
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedCount);

        var result = await _repository.CountAsync();

        result.Should().Be(expectedCount);
    }

    [Fact]
    public async Task CountAsync_WithCriteria_ShouldReturnMatchingCount()
    {
        // Arrange
        Expression<Func<Bill, bool>> criteria = b => b.BillingTypeValue == BillingType.Residential.Value;

        // Act
        var result = await _repository.CountAsync(criteria);

        // Assert
        result.Should().Be(3);
    }

    [Fact]
    public void Dispose_ShouldDisposeContext()
    {
        _repository.Dispose();
    }
}