using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace PowerBillingUsage.Application.DTOs;

public record GetPaginateListDto
{
    [DefaultValue(0)]
    public int Skip { get; set; }

    [Range(0, 100)]
    [DefaultValue(100)]
    public int Take { get; set; }

}
