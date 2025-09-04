using TigerBooking.Application.Common.Exceptions;
using TigerBooking.Application.DTOs.TbAdmin;
using TigerBooking.Application.Interfaces;
using TigerBooking.Domain.Entities.TbAdmin;
using TigerBooking.Domain.Interfaces.TbAdmin;

namespace TigerBooking.Application.Services;

/// <summary>
/// 채널 서비스 구현체
/// 채널 CRUD 작업과 비즈니스 로직을 처리
/// </summary>
public class ChannelService : IChannelService
{
    private readonly IChannelRepository _channelRepository;

    public ChannelService(IChannelRepository channelRepository)
    {
        _channelRepository = channelRepository;
    }

    public async Task<GetChannelsResponseDto> GetChannelsAsync(GetChannelsRequestDto request, CancellationToken cancellationToken = default)
    {
        // 페이지 크기 제한
        if (request.PageSize > 100) request.PageSize = 100;
        if (request.PageSize < 1) request.PageSize = 20;
        if (request.Page < 1) request.Page = 1;

        var (items, total) = await _channelRepository.GetChannelsAsync(
            request.Page,
            request.PageSize,
            request.IsActive,
            request.ParentOnly,
            request.ParentId,
            request.Code,
            request.Name,
            request.DateFrom,
            request.DateTo,
            cancellationToken);

        return new GetChannelsResponseDto
        {
            Items = items.Select(MapToDto),
            Page = request.Page,
            PageSize = request.PageSize,
            Total = total
        };
    }

    public async Task<ChannelDto?> GetChannelByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var channel = await _channelRepository.GetChannelByIdAsync(id, cancellationToken);
        return channel != null ? MapToDto(channel) : null;
    }

    public async Task<ChannelDto> CreateChannelAsync(CreateChannelRequestDto request, string createdBy, CancellationToken cancellationToken = default)
    {
        // 비즈니스 규칙 검증
        await ValidateCreateChannelAsync(request, cancellationToken);

        var channel = new Channel
        {
            ParentChannelId = request.ParentChannelId,
            Code = request.Code,
            Name = request.Name,
            IsActive = request.IsActive,
            ContractDate = request.ContractDate,
            Ratio = request.Ratio,
            SortOrder = request.SortOrder,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = createdBy,
            UpdatedBy = createdBy,
            IsDeleted = false
        };

        var createdChannel = await _channelRepository.CreateChannelAsync(channel, cancellationToken);
        return MapToDto(createdChannel);
    }

    public async Task<ChannelDto> UpdateChannelAsync(long id, UpdateChannelRequestDto request, string updatedBy, CancellationToken cancellationToken = default)
    {
        var channel = await _channelRepository.GetChannelByIdAsync(id, cancellationToken);
        if (channel == null)
        {
            throw new NotFoundException($"채널을 찾을 수 없습니다. ID: {id}");
        }

        // 업데이트
        channel.Name = request.Name;
        channel.IsActive = request.IsActive;
        channel.ContractDate = request.ContractDate;
        channel.Ratio = request.Ratio;
        channel.SortOrder = request.SortOrder;
        channel.UpdatedAt = DateTime.UtcNow;
        channel.UpdatedBy = updatedBy;

        var updatedChannel = await _channelRepository.UpdateChannelAsync(channel, cancellationToken);
        return MapToDto(updatedChannel);
    }

    public async Task DeleteChannelAsync(long id, string deletedBy, CancellationToken cancellationToken = default)
    {
        var channel = await _channelRepository.GetChannelByIdAsync(id, cancellationToken);
        if (channel == null)
        {
            throw new NotFoundException($"채널을 찾을 수 없습니다. ID: {id}");
        }

        // 자식 채널이 있는지 확인
        var subChannelCount = await _channelRepository.GetSubChannelCountAsync(id, cancellationToken);
        if (subChannelCount > 0)
        {
            throw new BadRequestException("하위 채널이 있는 채널은 삭제할 수 없습니다. 먼저 하위 채널을 삭제해주세요.");
        }

        await _channelRepository.DeleteChannelAsync(id, deletedBy, cancellationToken);
    }

    private async Task ValidateCreateChannelAsync(CreateChannelRequestDto request, CancellationToken cancellationToken)
    {
        // 코드 유니크 검증
        var existingChannel = await _channelRepository.GetChannelByCodeAsync(request.Code, request.ParentChannelId, cancellationToken);
        if (existingChannel != null)
        {
            if (request.ParentChannelId == null)
            {
                throw new BadRequestException($"루트 채널 코드 '{request.Code}'는 이미 사용 중입니다.");
            }
            else
            {
                throw new BadRequestException($"동일 부모 채널 내에서 코드 '{request.Code}'는 이미 사용 중입니다.");
            }
        }

        // 서브 채널의 경우 부모가 루트 채널인지 확인
        if (request.ParentChannelId.HasValue)
        {
            var isRootChannel = await _channelRepository.IsRootChannelAsync(request.ParentChannelId.Value, cancellationToken);
            if (!isRootChannel)
            {
                throw new BadRequestException("서브 채널의 부모는 반드시 루트 채널이어야 합니다.");
            }
        }
    }

    private static ChannelDto MapToDto(Channel channel)
    {
        return new ChannelDto
        {
            Id = channel.Id,
            ParentChannelId = channel.ParentChannelId,
            Code = channel.Code,
            Name = channel.Name,
            IsActive = channel.IsActive,
            ContractDate = channel.ContractDate,
            Ratio = channel.Ratio,
            SortOrder = channel.SortOrder,
            CreatedAt = channel.CreatedAt,
            UpdatedAt = channel.UpdatedAt,
            CreatedBy = channel.CreatedBy,
            UpdatedBy = channel.UpdatedBy
        };
    }
}
