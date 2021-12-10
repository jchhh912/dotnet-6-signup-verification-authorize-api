using Microsoft.AspNetCore.Identity;
using Swashbuckle.AspNetCore.SwaggerUI;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddFrastructure(builder.Configuration);


//var userManager = builder.Services.BuildServiceProvider().GetRequiredService<UserManager<AppUser>>();
//var roleManager = builder.Services.BuildServiceProvider().GetRequiredService<RoleManager<IdentityRole<Guid>>>();
//await SeedApplicationDbContext.SeedEssentialsAsync(userManager, roleManager);
builder.Services.AddControllers();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.DefaultModelsExpandDepth(-1);
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.RoutePrefix = "swagger";
        options.DisplayRequestDuration();
        options.DocExpansion(DocExpansion.None);
    });
}
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});
app.Run();
