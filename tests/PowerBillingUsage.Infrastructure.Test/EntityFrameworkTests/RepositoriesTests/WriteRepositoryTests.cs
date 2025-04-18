using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Caching.Hybrid;
using Moq;
using PowerBillingUsage.Domain.Abstractions.Helpers;
using PowerBillingUsage.Domain.Abstractions.Services;
using PowerBillingUsage.Domain.Bills;
using PowerBillingUsage.Domain.Enums;
using PowerBillingUsage.Infrastructure.EntityFramework;
using PowerBillingUsage.Infrastructure.EntityFramework.Repositories;
using PowerBillingUsage.Infrastructure.Helpers;

namespace PowerBillingUsage.Infrastructure.Test.EntityFrameworkTests.RepositoriesTests;

public class WriteRepositoryTests
{
    private static readonly DateTime DefaultStartDate = new(2025, 01, 01, 20, 10, 00);
    private static readonly DateTime DefaultEndDate = new(2025, 01, 31, 03, 15, 00);

    private readonly PowerBillingUsageWriteDbContext _context;
    private readonly WriteRepository<Bill, BillId> _writeRepository;
    private readonly Mock<IHybridCacheService> _mockCacheService;
    private readonly ICacheKeyHelper<Bill> _cacheKeyHelper;

    public WriteRepositoryTests()
    {
        var databaseName = Guid.NewGuid().ToString();
        var inMemoryDbRoot = new InMemoryDatabaseRoot();

        var options = new DbContextOptionsBuilder<PowerBillingUsageWriteDbContext>()
            .UseInMemoryDatabase(databaseName: databaseName, databaseRoot: inMemoryDbRoot)
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _context = new PowerBillingUsageWriteDbContext(options);

        _mockCacheService = new Mock<IHybridCacheService>();
        _cacheKeyHelper = new CacheKeyHelper<Bill>();

        _writeRepository = new WriteRepository<Bill, BillId>(_context, _mockCacheService.Object, _cacheKeyHelper);
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
    public async Task InsertAsync_ShouldAddEntityAndUpdateCache()
    {
        var bill = CreateSampleBill();
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

        var result = await _writeRepository.InsertAsync(bill);
        await _writeRepository.SaveChangesAsync();

        result.Should().Be(bill);

        var savedBill = await _context.Set<Bill>().FindAsync(bill.Id);
        savedBill.Should().NotBeNull();
        savedBill!.Id.Should().Be(bill.Id);

        _mockCacheService.Verify(x => x.RemoveAsync(_cacheKeyHelper.AllKey, It.IsAny<CancellationToken>()), Times.Once);
        _mockCacheService.Verify(x => x.RemoveByTagsAsync(It.Is<List<string>>(tags => tags.Contains(_cacheKeyHelper.PaginateKeyTag)), It.IsAny<CancellationToken>()), Times.Once);
        _mockCacheService.Verify(x => x.GetOrCreateAsync(_cacheKeyHelper.CountKey, It.IsAny<Func<CancellationToken, ValueTask<int>>>(), HybridCacheEntryFlags.None, It.IsAny<IEnumerable<string>>(), It.IsAny<TimeSpan?>(), null, It.IsAny<CancellationToken>()), Times.Once);
        _mockCacheService.Verify(x => x.SetAsync(_cacheKeyHelper.CountKey, 1, HybridCacheEntryFlags.None, It.IsAny<IEnumerable<string>>(), It.IsAny<TimeSpan?>(), null, It.IsAny<CancellationToken>()), Times.Once);
        _mockCacheService.Verify(x => x.SetAsync(keyOne, bill, HybridCacheEntryFlags.None, It.IsAny<IEnumerable<string>>(), It.IsAny<TimeSpan?>(), null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveEntityAndUpdateCache()
    {
        var bill = CreateSampleBill();
        await _context.Set<Bill>().AddAsync(bill);
        await _context.SaveChangesAsync();

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

        await _writeRepository.DeleteAsync(bill.Id);
        await _writeRepository.SaveChangesAsync();

        var deletedBill = await _context.Set<Bill>().FindAsync(bill.Id);
        deletedBill.Should().BeNull();

        _mockCacheService.Verify(x => x.RemoveAsync(_cacheKeyHelper.AllKey, It.IsAny<CancellationToken>()), Times.Once);
        _mockCacheService.Verify(x => x.RemoveAsync(keyOne, It.IsAny<CancellationToken>()), Times.Once);
        _mockCacheService.Verify(x => x.RemoveByTagsAsync(It.Is<List<string>>(tags => tags.Contains(_cacheKeyHelper.PaginateKeyTag)), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateEntityAndInvalidateCache()
    {
        var bill = CreateSampleBill();
        await _context.Set<Bill>().AddAsync(bill);
        await _context.SaveChangesAsync();
        _context.Entry(bill).State = EntityState.Detached;

        var updatedBill = new Bill(
            id: bill.Id,
            billingTypeValue: BillingType.Commercial.Value, // Changed type
            startAt: DefaultStartDate,
            endAt: DefaultEndDate,
            breakDowns: bill.BreakDowns
        );

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

        var result = await _writeRepository.UpdateAsync(updatedBill);
        await _writeRepository.SaveChangesAsync();

        result.Should().Be(updatedBill);

        var savedBill = await _context.Set<Bill>().FindAsync(bill.Id);
        savedBill.Should().NotBeNull();
        savedBill!.BillingTypeValue.Should().Be(BillingType.Commercial.Value);

        _mockCacheService.Verify(x => x.RemoveAsync(_cacheKeyHelper.AllKey, It.IsAny<CancellationToken>()), Times.Once);
        _mockCacheService.Verify(x => x.RemoveAsync(keyOne, It.IsAny<CancellationToken>()), Times.Once);
        _mockCacheService.Verify(x => x.RemoveByTagsAsync(It.Is<List<string>>(tags => tags.Contains(_cacheKeyHelper.PaginateKeyTag)), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldPersistChangesToDatabase()
    {
        var bill = CreateSampleBill();
        await _context.Set<Bill>().AddAsync(bill);

        var result = await _writeRepository.SaveChangesAsync();

        result.Should().BeGreaterThan(0);
        var savedBill = await _context.Set<Bill>().FindAsync(bill.Id);
        savedBill.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteRangeAsync_ShouldRemoveEntitiesAndUpdateCache()
    {
        var bills = new List<Bill> { CreateSampleBill(), CreateSampleBill() };
        await _context.Set<Bill>().AddRangeAsync(bills);
        await _context.SaveChangesAsync();

        var billIds = bills.Select(b => b.Id).ToList();
        var keyOnes = billIds.Select(id => _cacheKeyHelper.MakeKeyOne(id)).ToList();

        _mockCacheService
            .Setup(x => x.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        _mockCacheService
            .Setup(x => x.RemoveByTagsAsync(It.Is<List<string>>(tags => tags.Contains(_cacheKeyHelper.PaginateKeyTag)), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        await _writeRepository.DeleteRangeAsync(billIds);
        await _writeRepository.SaveChangesAsync();

        var deletedBills = await _context.Set<Bill>().Where(x => billIds.Contains(x.Id)).ToListAsync();
        deletedBills.Should().BeEmpty();

        foreach (var keyOne in keyOnes)
        {
            _mockCacheService.Verify(x => x.RemoveAsync(keyOne, It.IsAny<CancellationToken>()), Times.Once);
        }
        _mockCacheService.Verify(x => x.RemoveAsync(_cacheKeyHelper.AllKey, It.IsAny<CancellationToken>()), Times.Once);
        _mockCacheService.Verify(x => x.RemoveByTagsAsync(It.Is<List<string>>(tags => tags.Contains(_cacheKeyHelper.PaginateKeyTag)), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAndSaveChangesAsync_ShouldRemoveEntityAndUpdateCacheInTransaction()
    {
        var bill = CreateSampleBill();
        await _context.Set<Bill>().AddAsync(bill);
        await _context.SaveChangesAsync();

        var keyOne = _cacheKeyHelper.MakeKeyOne(bill.Id);

        _mockCacheService
            .Setup(x => x.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        _mockCacheService
            .Setup(x => x.RemoveByTagsAsync(It.Is<List<string>>(tags => tags.Contains(_cacheKeyHelper.PaginateKeyTag)), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        var result = await _writeRepository.DeleteAndSaveChangesAsync(bill);

        result.Should().Be(1);
        var deletedBill = await _context.Set<Bill>().FindAsync(bill.Id);
        deletedBill.Should().BeNull();

        _mockCacheService.Verify(x => x.RemoveAsync(_cacheKeyHelper.AllKey, It.IsAny<CancellationToken>()), Times.Once);
        _mockCacheService.Verify(x => x.RemoveAsync(keyOne, It.IsAny<CancellationToken>()), Times.Once);
        _mockCacheService.Verify(x => x.RemoveByTagsAsync(It.Is<List<string>>(tags => tags.Contains(_cacheKeyHelper.PaginateKeyTag)), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteRangeAndSaveChangesAsync_ShouldRemoveEntitiesAndUpdateCacheInTransaction()
    {
        var bills = new List<Bill> { CreateSampleBill(), CreateSampleBill() };
        await _context.Set<Bill>().AddRangeAsync(bills);
        await _context.SaveChangesAsync();

        var billIds = bills.Select(b => b.Id).ToList();
        var keyOnes = billIds.Select(id => _cacheKeyHelper.MakeKeyOne(id)).ToList();

        _mockCacheService
            .Setup(x => x.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        _mockCacheService
            .Setup(x => x.RemoveByTagsAsync(It.Is<List<string>>(tags => tags.Contains(_cacheKeyHelper.PaginateKeyTag)), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        var result = await _writeRepository.DeleteRangeAndSaveChangesAsync(bills);

        result.Should().Be(2);
        var deletedBills = await _context.Set<Bill>().Where(x => billIds.Contains(x.Id)).ToListAsync();
        deletedBills.Should().BeEmpty();

        foreach (var keyOne in keyOnes)
        {
            _mockCacheService.Verify(x => x.RemoveAsync(keyOne, It.IsAny<CancellationToken>()), Times.Once);
        }
        _mockCacheService.Verify(x => x.RemoveAsync(_cacheKeyHelper.AllKey, It.IsAny<CancellationToken>()), Times.Once);
        _mockCacheService.Verify(x => x.RemoveByTagsAsync(It.Is<List<string>>(tags => tags.Contains(_cacheKeyHelper.PaginateKeyTag)), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteRangeAsync_ShouldThrowWhenNotAllItemsFound()
    {
        var existingBill = CreateSampleBill();
        await _context.Set<Bill>().AddAsync(existingBill);
        await _context.SaveChangesAsync();

        var nonExistingBillId = new BillId(Guid.NewGuid());
        var billIds = new List<BillId> { existingBill.Id, nonExistingBillId };

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _writeRepository.DeleteRangeAsync(billIds));
    }

    [Fact]
    public async Task DeleteRangeAndSaveChangesAsync_ShouldRollbackWhenNotAllItemsDeleted()
    {
        var bills = new List<Bill> { CreateSampleBill(), CreateSampleBill() };
        await _context.Set<Bill>().AddRangeAsync(bills);
        await _context.SaveChangesAsync();

        // Simulate a failure by passing only one item but both IDs
        var billIds = bills.Select(b => b.Id).ToList();
        var itemsToDelete = new List<Bill> { bills[0] };

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _writeRepository.DeleteRangeAndSaveChangesAsync(itemsToDelete));

        // Verify the transaction was rolled back (items still exist)
        var remainingBills = await _context.Set<Bill>().Where(x => billIds.Contains(x.Id)).ToListAsync();
        remainingBills.Should().HaveCount(2);
    }
}