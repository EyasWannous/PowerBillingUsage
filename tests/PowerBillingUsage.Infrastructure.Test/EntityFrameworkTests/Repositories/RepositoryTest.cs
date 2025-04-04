using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using PowerBillingUsage.Domain.Abstractions.Helpers;
using PowerBillingUsage.Domain.Abstractions.Services;
using PowerBillingUsage.Domain.Bills;
using PowerBillingUsage.Domain.Enums;
using PowerBillingUsage.Infrastructure.EntityFramework;
using PowerBillingUsage.Infrastructure.EntityFramework.Repositories;
using PowerBillingUsage.Infrastructure.Helpers;

namespace PowerBillingUsage.Infrastructure.Test.EntityFrameworkTests.Repositories;

public class RepositoryTest
{
    private static readonly DateTime DefaultStartDate = new(2025, 01, 01, 20, 10, 00);
    private static readonly DateTime DefaultEndDate = new(2025, 01, 31, 03, 15, 00);

    private readonly PowerBillingUsageWriteDbContext _context;
    private readonly Repository<Bill, BillId> _billRepository;
    private readonly IHybridCacheService _cacheService;
    private readonly ICacheKeyHelper<Bill> _cacheKeyHelper;
    private readonly ReadRepository<BillReadModel, BillId> _billReadRepository;
    private readonly PowerBillingUsageReadDbContext _readContext;
    private readonly ICacheKeyHelper<BillReadModel> _readModelCacheKeyHelper;


    public static IEnumerable<object[]> GetValidatedResidentialBillingData =>
    [
        [
            new Bill(
                id: new(Guid.NewGuid()),
                billingTypeValue: BillingType.Residential.Value,
                startAt: DefaultStartDate,
                endAt: DefaultEndDate,
                breakDowns: [
                    new BillDetail(new(Guid.NewGuid()), "Up to 160 KWh", 160, 0.05m, 8.00m),
                    new BillDetail(new(Guid.NewGuid()), "Up to 300 KWh", 140, 0.10m, 14.00m),
                    new BillDetail(new(Guid.NewGuid()), "Up to 500 KWh", 150, 0.12m, 18.00m),
                ]
            )
        ]
    ];

    public RepositoryTest()
    {
        var databaseName = Guid.NewGuid().ToString();
        var inMemoryDbRoot = new InMemoryDatabaseRoot();

        var options = new DbContextOptionsBuilder<PowerBillingUsageWriteDbContext>()
            .UseInMemoryDatabase(databaseName: databaseName, databaseRoot: inMemoryDbRoot)
            .Options;

        _context = new PowerBillingUsageWriteDbContext(options);

        _cacheService = Mock.Of<IHybridCacheService>();
        _cacheKeyHelper = new CacheKeyHelper<Bill>();

        _billRepository = new Repository<Bill, BillId>(_context, _cacheService, _cacheKeyHelper);

        var readOptions = new DbContextOptionsBuilder<PowerBillingUsageReadDbContext>()
            .UseInMemoryDatabase(databaseName: databaseName, databaseRoot: inMemoryDbRoot)
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
            .Options;

        _readContext = new PowerBillingUsageReadDbContext(readOptions);

        _readModelCacheKeyHelper = new CacheKeyHelper<BillReadModel>();

        _billReadRepository = new ReadRepository<BillReadModel, BillId>(_readContext, _cacheService, _readModelCacheKeyHelper);

    }

    [Theory]
    [MemberData(nameof(GetValidatedResidentialBillingData))]
    public async Task GetQueryableWithIncludedDetailsShouldIncludeEverythignInBill(Bill bill)
    {
        await _billRepository.InsertAsync(bill);
        await _billRepository.SaveChangesAsync();

        var query = await _billRepository.GetQueryableWithDetailsAsync(x => x.BreakDowns);

        var billFromRepo = await query.FirstOrDefaultAsync(x => x.Id.Equals(bill.Id));
        Assert.NotNull(billFromRepo);
        Assert.NotEmpty(billFromRepo.BreakDowns);

        billFromRepo.Id.Should().Be(bill.Id);
        billFromRepo.BillingTypeValue.Should().Be(bill.BillingTypeValue);
        billFromRepo.StartAt.Should().Be(bill.StartAt);
        billFromRepo.EndAt.Should().Be(bill.EndAt);

        billFromRepo.BreakDowns.Should().HaveCount(bill.BreakDowns.Count);
        for (int i = 0; i < billFromRepo.BreakDowns.Count; i++)
        {
            billFromRepo.BreakDowns[i].Id.Should().Be(bill.BreakDowns[i].Id);
            billFromRepo.BreakDowns[i].TierName.Should().Be(bill.BreakDowns[i].TierName);
            billFromRepo.BreakDowns[i].Consumption.Should().Be(bill.BreakDowns[i].Consumption);
            billFromRepo.BreakDowns[i].Rate.Should().Be(bill.BreakDowns[i].Rate);
            billFromRepo.BreakDowns[i].Total.Should().Be(bill.BreakDowns[i].Total);
        }
    }


    [Theory]
    [MemberData(nameof(GetValidatedResidentialBillingData))]
    public async Task GetPaginateListFromReadRepository(Bill bill)
    {
        var billReadModel = MapBillToBillReadModel(bill);
        await _readContext.Bills.AddAsync(billReadModel);
        await _readContext.SaveChangesAsync();

        var query = await _billReadRepository.GetQueryableWithDetailsAsync(x => x.BreakDowns);
        var billFromQuery = await query.FirstOrDefaultAsync(x => x.Id.Equals(bill.Id));

        var billFromReadRepo = await _billReadRepository.GetByIdAsync(bill.Id);

        Assert.NotNull(billFromQuery);
        Assert.NotEmpty(billFromQuery.BreakDowns);
    }
    private BillReadModel MapBillToBillReadModel(Bill bill)
    {
        return new BillReadModel
        {
            Id = bill.Id,
            BillingTypeValue = bill.BillingTypeValue,
            StartAt = bill.StartAt,
            EndAt = bill.EndAt,
            BreakDowns = [.. bill.BreakDowns.Select(d => new BillDetailReadModel
            {
                Id = d.Id,
                TierName = d.TierName,
                Consumption = d.Consumption,
                Rate = d.Rate,
                Total = d.Total
            })]
        };
    }

}

