using Microsoft.EntityFrameworkCore;
using TigerBooking.Domain.Entities.TbAdmin;
using TigerBooking.Domain.Interfaces.TbAdmin;
using TigerBooking.Infrastructure.Data;

namespace TigerBooking.Infrastructure.Repositories;

/// <summary>
/// SocialAuth 리포지토리 구현체
/// TbAdmin 스키마의 social_auth 테이블에 대한 데이터 액세스 제공
/// </summary>
public class SocialAuthRepository : ISocialAuthRepository
{
    private readonly TbAdminDbContext _context;

    public SocialAuthRepository(TbAdminDbContext context)
    {
        _context = context;
    }

    public async Task<SocialAuth?> GetByProviderAndUserIdAsync(string provider, string providerUserId, CancellationToken cancellationToken = default)
    {
        return await _context.SocialAuths
            .Where(sa => sa.Provider == provider && sa.ProviderUserId == providerUserId && !sa.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<SocialAuth?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.SocialAuths
            .Where(sa => sa.Id == id && !sa.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<SocialAuth> CreateAsync(SocialAuth socialAuth, CancellationToken cancellationToken = default)
    {
        _context.SocialAuths.Add(socialAuth);
        await _context.SaveChangesAsync(cancellationToken);
        return socialAuth;
    }

    public async Task<SocialAuth> UpdateAsync(SocialAuth socialAuth, CancellationToken cancellationToken = default)
    {
        _context.SocialAuths.Update(socialAuth);
        await _context.SaveChangesAsync(cancellationToken);
        return socialAuth;
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var socialAuth = await GetByIdAsync(id, cancellationToken);
        if (socialAuth != null)
        {
            socialAuth.IsDeleted = true;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
