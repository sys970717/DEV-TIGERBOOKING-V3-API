using System.ComponentModel.DataAnnotations;

namespace TigerBooking.Application.DTOs.TbAdmin;

/// <summary>
/// 채널 목록 조회 요청 DTO
/// </summary>
public class GetChannelsRequestDto
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public bool? IsActive { get; set; }
    public bool ParentOnly { get; set; } = false;
    public long? ParentId { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
    public DateOnly? DateFrom { get; set; }
    public DateOnly? DateTo { get; set; }
}

/// <summary>
/// 채널 목록 조회 응답 DTO
/// </summary>
public class GetChannelsResponseDto
{
    public IEnumerable<ChannelDto> Items { get; set; } = new List<ChannelDto>();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int Total { get; set; }
}

/// <summary>
/// 채널 생성 요청 DTO
/// </summary>
public class CreateChannelRequestDto
{
    public long? ParentChannelId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public DateOnly? ContractDate { get; set; }

    [Range(0, 1)]
    public decimal Ratio { get; set; } = 0;

    public int SortOrder { get; set; } = 0;

    public bool IsActive { get; set; } = true;
}

/// <summary>
/// 채널 수정 요청 DTO
/// </summary>
public class UpdateChannelRequestDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public DateOnly? ContractDate { get; set; }

    [Range(0, 1)]
    public decimal Ratio { get; set; } = 0;

    public int SortOrder { get; set; } = 0;

    public bool IsActive { get; set; } = true;
}

/// <summary>
/// 채널 응답 DTO
/// </summary>
public class ChannelDto
{
    public long Id { get; set; }
    public long? ParentChannelId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateOnly? ContractDate { get; set; }
    public decimal Ratio { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;
}
