using PowerBillingUsage.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace PowerBillingUsage.API.DTOs;

public class CalculateBillDto
{
    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Consumption must be a non-negative value.")]
    public int Consumption { get; set; }

    [Required]
    public DateTime StartAt { get; set; }

    [Required]
    public DateTime EndAt { get; set; }

    [Required]
    [Range(1, 2)]
    public int BillingTypeValue { get; set; } = BillingType.Residential.Value;
}