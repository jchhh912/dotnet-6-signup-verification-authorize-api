
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class BaseDbContext : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>
{
    public BaseDbContext(DbContextOptions<BaseDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        ///自定义生成表名
        ///用户数据核心存储
        builder.Entity<AppUser>(b => { b.ToTable("AppUsers"); });
        //用户权限表
        builder.Entity<IdentityUserRole<Guid>>(b => { b.ToTable("AppUserRoles"); });
        //登录信息表
        builder.Entity<IdentityUserLogin<Guid>>(b => { b.ToTable("AppUserLogins"); });
        //用户声明表
        builder.Entity<IdentityUserClaim<Guid>>(b => { b.ToTable("AppUserClaims"); });
        //登录外部Token储存表
        builder.Entity<IdentityUserToken<Guid>>(b => { b.ToTable("AppUserTokens"); });
        //权限表
        builder.Entity<IdentityRole<Guid>>(b => { b.ToTable("AppRoles"); });
        //权限详情表
        //builder.Entity<IdentityRole<string>>(b => { b.ToTable("AppRoles"); });
        //角色生命表
        builder.Entity<IdentityRoleClaim<Guid>>(b => { b.ToTable("AppRoleClaims"); });
    }
}