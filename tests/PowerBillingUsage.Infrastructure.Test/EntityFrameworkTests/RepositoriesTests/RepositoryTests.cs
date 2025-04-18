using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Moq;
using PowerBillingUsage.Domain.Abstractions.Helpers;
using PowerBillingUsage.Domain.Abstractions.Repositories;
using PowerBillingUsage.Domain.Abstractions.Services;
using PowerBillingUsage.Domain.Bills;
using PowerBillingUsage.Domain.Enums;
using PowerBillingUsage.Infrastructure.EntityFramework.Repositories;
using PowerBillingUsage.Infrastructure.EntityFramework;
using PowerBillingUsage.Infrastructure.Helpers;
using FluentAssertions;

namespace PowerBillingUsage.Infrastructure.Test.EntityFrameworkTests.RepositoriesTests;

public class RepositoryTests
{
    private static readonly DateTime DefaultStartDate = new(2025, 01, 01, 20, 10, 00);
    private static readonly DateTime DefaultEndDate = new(2025, 01, 31, 03, 15, 00);

    private readonly PowerBillingUsageWriteDbContext _context;
    private readonly Repository<Bill, BillId> _repository;
    private readonly Mock<IHybridCacheService> _mockCacheService;
    private readonly ICacheKeyHelper<Bill> _cacheKeyHelper;

    public RepositoryTests()
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
    }

    [Fact]
    public void Repository_ShouldImplementCorrectInterfaces()
    {
        _repository.Should().BeAssignableTo<IRepository<Bill, BillId>>();
        _repository.Should().BeAssignableTo<IWriteRepository<Bill, BillId>>();
        _repository.Should().BeAssignableTo<IBaseRepository<Bill, BillId>>();
    }

    // Since Repository inherits from WriteRepository, we don't need to retest all methods
    // We'll just verify that it's properly set up with the correct dependencies

    [Fact]
    public async Task Repository_ShouldWorkWithAllFeatures()
    {
        var bill = CreateSampleBill();

        SetupCacheMocksForInsert(bill);

        var insertedBill = await _repository.InsertAsync(bill);
        await _repository.SaveChangesAsync();

        insertedBill.Should().Be(bill);

        var savedBill = await _context.Set<Bill>().FindAsync(bill.Id);
        savedBill.Should().NotBeNull();
        savedBill!.Id.Should().Be(bill.Id);

        SetupCacheMocksForGetById(bill);

        var retrievedBill = await _repository.GetByIdAsync(bill.Id);
        retrievedBill.Should().NotBeNull();
        retrievedBill!.Id.Should().Be(bill.Id);

        SetupCacheMocksForGetList();

        var bills = await _repository.GetListAsync();
        bills.Should().NotBeNull();
        bills.Should().HaveCount(1);

        _context.Entry(bill).State = EntityState.Detached;

        SetupCacheMocksForUpdate(bill);
        
        var updatedBill = new Bill(
            id: bill.Id,
            billingTypeValue: BillingType.Commercial.Value, // Changed from original
            startAt: DefaultStartDate,
            endAt: DefaultEndDate,
            breakDowns: bill.BreakDowns
        );

        var result = await _repository.UpdateAsync(updatedBill);
        await _repository.SaveChangesAsync();

        result.Should().Be(updatedBill);

        var updatedSavedBill = await _context.Set<Bill>().FindAsync(bill.Id);
        updatedSavedBill.Should().NotBeNull();
        updatedSavedBill!.BillingTypeValue.Should().Be(BillingType.Commercial.Value);

        SetupCacheMocksForDelete(bill);

        await _repository.DeleteAsync(bill.Id);
        await _repository.SaveChangesAsync();

        var deletedBill = await _context.Set<Bill>().FindAsync(bill.Id);
        deletedBill.Should().BeNull();
    }

    private Bill CreateSampleBill()
    {
        return new Bill(
            id: new BillId(Guid.NewGuid()),
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

    private void SetupCacheMocksForInsert(Bill bill)
    {
        var keyOne = _cacheKeyHelper.MakeKeyOne(bill.Id);

        _mockCacheService
            .Setup(x => x.RemoveAsync(_cacheKeyHelper.AllKey, It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        _mockCacheService
            .Setup(x => x.RemoveByTagsAsync(It.Is<List<string>>(tags => tags.Contains(_cacheKeyHelper.PaginateKeyTag)), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        _mockCacheService
            .Setup(x => x.GetOrCreateAsync(
                _cacheKeyHelper.CountKey,
                It.IsAny<Func<CancellationToken, ValueTask<int>>>(),
                HybridCacheEntryFlags.None,
                It.Is<IEnumerable<string>>(tags => tags.Contains(_cacheKeyHelper.CountKey)),
                It.IsAny<TimeSpan?>(),
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        _mockCacheService
            .Setup(x => x.RemoveAsync(_cacheKeyHelper.CountKey, It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        _mockCacheService
            .Setup(x => x.SetAsync(
                _cacheKeyHelper.CountKey,
                1,
                HybridCacheEntryFlags.None,
                It.Is<IEnumerable<string>>(tags => tags.Contains(_cacheKeyHelper.CountKey)),
                It.IsAny<TimeSpan?>(),
                null,
                It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        _mockCacheService
            .Setup(x => x.SetAsync(
                keyOne,
                bill,
                HybridCacheEntryFlags.None,
                It.Is<IEnumerable<string>>(tags => tags.Contains(keyOne)),
                It.IsAny<TimeSpan?>(),
                null,
                It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);
    }

    private void SetupCacheMocksForGetById(Bill bill)
    {
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
            .Returns(async (
                string key,
                Func<CancellationToken, ValueTask<Bill?>> factory,
                HybridCacheEntryFlags flags,
                IEnumerable<string>? tags,
                TimeSpan? expiration,
                TimeSpan? localExpiration,
                CancellationToken cancellationToken) =>
            {
                return await factory(cancellationToken);
            });
    }

    private void SetupCacheMocksForGetList()
    {
        _mockCacheService
            .Setup(x => x.GetOrCreateAsync<IEnumerable<Bill>>(
                _cacheKeyHelper.AllKey,
                It.IsAny<Func<CancellationToken, ValueTask<IEnumerable<Bill>>>>(),
                HybridCacheEntryFlags.None,
                It.Is<IEnumerable<string>>(tags => tags.Contains(_cacheKeyHelper.AllKey)),
                It.IsAny<TimeSpan?>(),
                null,
                It.IsAny<CancellationToken>()))
            .Returns(async (
                string key,
                Func<CancellationToken, ValueTask<IEnumerable<Bill>>> factory,
                HybridCacheEntryFlags flags,
                IEnumerable<string>? tags,
                TimeSpan? expiration,
                TimeSpan? localExpiration,
                CancellationToken cancellationToken) =>
            {
                return await factory(cancellationToken);
            });
    }

    private void SetupCacheMocksForUpdate(Bill bill)
    {
        var keyOne = _cacheKeyHelper.MakeKeyOne(bill.Id);

        _mockCacheService
            .Setup(x => x.RemoveAsync(_cacheKeyHelper.AllKey, It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        _mockCacheService
            .Setup(x => x.RemoveAsync(keyOne, It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        _mockCacheService
            .Setup(x => x.RemoveByTagsAsync(It.Is<List<string>>(tags => tags.Contains(_cacheKeyHelper.PaginateKeyTag)), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);
    }

    private void SetupCacheMocksForDelete(Bill bill)
    {
        var keyOne = _cacheKeyHelper.MakeKeyOne(bill.Id);

        _mockCacheService
            .Setup(x => x.RemoveAsync(_cacheKeyHelper.AllKey, It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        _mockCacheService
            .Setup(x => x.RemoveAsync(keyOne, It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        _mockCacheService
            .Setup(x => x.RemoveByTagsAsync(It.Is<List<string>>(tags => tags.Contains(_cacheKeyHelper.PaginateKeyTag)), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        _mockCacheService
            .Setup(x => x.GetOrCreateAsync(
                _cacheKeyHelper.CountKey,
                It.IsAny<Func<CancellationToken, ValueTask<int>>>(),
                HybridCacheEntryFlags.None,
                It.Is<IEnumerable<string>>(tags => tags.Contains(_cacheKeyHelper.CountKey)),
                It.IsAny<TimeSpan?>(),
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _mockCacheService
            .Setup(x => x.RemoveAsync(_cacheKeyHelper.CountKey, It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        _mockCacheService
            .Setup(x => x.SetAsync(
                _cacheKeyHelper.CountKey,
                0,
                HybridCacheEntryFlags.None,
                It.Is<IEnumerable<string>>(tags => tags.Contains(_cacheKeyHelper.CountKey)),
                It.IsAny<TimeSpan?>(),
                null,
                It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);
    }
}