using System.ComponentModel.DataAnnotations.Schema;
using TigerBooking.Domain.Common.Entities;

namespace TigerBooking.Domain.Entities.TbAdmin;

/// <summary>
/// 채널 엔티티 - 서비스 운영 단위를 관리하는 엔티티
/// 루트 채널과 1-depth 서브 채널을 지원
/// </summary>
[Table("channel")]
public class Channel : BaseEntity
{
    /// <summary>
    /// 부모 채널 ID (루트 채널의 경우 NULL)
    /// </summary>
    [Column("parent_channel_id")]
    public long? ParentChannelId { get; set; }

    /// <summary>
    /// 채널 코드 - 루트는 전역 유니크, 서브는 동일 부모 내 유니크
    /// </summary>
    [Column("code")]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 채널명
    /// </summary>
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 사용 여부 (기본값: true)
    /// </summary>
    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// 계약일자
    /// </summary>
    [Column("contract_date")]
    public DateOnly? ContractDate { get; set; }

    /// <summary>
    /// 비율 (0~1 권장)
    /// </summary>
    [Column("ratio")]
    public decimal Ratio { get; set; } = 0;

    /// <summary>
    /// 정렬 우선순위 (오름차순)
    /// </summary>
    [Column("sort_order")]
    public int SortOrder { get; set; } = 0;

    // Navigation properties (EF Core에서 관계 설정용)
    /// <summary>
    /// 부모 채널
    /// </summary>
    public virtual Channel? ParentChannel { get; set; }

    /// <summary>
    /// 자식 채널들 (서브 채널)
    /// </summary>
    public virtual ICollection<Channel> SubChannels { get; set; } = new List<Channel>();

    // BaseEntity의 필드들을 DB 컬럼에 맞게 매핑
    [Column("created_tz")]
    public new DateTime CreatedAt { get; set; }

    [Column("updated_tz")]
    public new DateTime UpdatedAt { get; set; }

    [Column("created_by")]
    public new string CreatedBy { get; set; } = string.Empty;

    [Column("updated_by")]
    public new string UpdatedBy { get; set; } = string.Empty;

    [Column("is_deleted")]
    public new bool IsDeleted { get; set; }

    [Column("deleted_tz")]
    public new DateTime? DeletedAt { get; set; }

    [Column("deleted_by")]
    public new string? DeletedBy { get; set; }
}
