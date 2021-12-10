
using Microsoft.AspNetCore.Identity;

public class SeedApplicationDbContext
{
    /// <summary>
    /// 种子数据
    /// </summary>
    public const string default_username = "admin";
    public const string default_email = "admin@test.com";
    public const string defaul_password = "P@ssw0rd123";
    public const string default_role = "Administrator";
    /// <summary>
    /// 种子数据
    /// </summary>
    /// <param name="userManager"></param>
    /// <param name="roleManager"></param>
    /// <returns></returns>
    public static async Task SeedEssentialsAsync(UserManager<AppUser> userManager, RoleManager<IdentityRole<Guid>> roleManager)
    {
        //生成自定义的用户种子数据
        var defaultUser = new AppUser
        {
            UserName = default_username,
            Email = default_email,
            EmailConfirmed = true
        };
        //不存在则创建
        if (userManager.Users.All(u => u.Id != defaultUser.Id))
        {
            //权限域种子数据
            await roleManager.CreateAsync(new IdentityRole<Guid>("Administrator"));
            await roleManager.CreateAsync(new IdentityRole<Guid>("Moderator"));
            await roleManager.CreateAsync(new IdentityRole<Guid>("User"));


            await userManager.CreateAsync(defaultUser, defaul_password);
            await userManager.AddToRoleAsync(defaultUser, default_role.ToString());
        }
    }
}
