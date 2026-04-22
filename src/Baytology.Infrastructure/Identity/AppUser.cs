using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Baytology.Infrastructure.Identity;

public class AppUser : IdentityUser
{
    public bool IsDeleted { get; set; }
}