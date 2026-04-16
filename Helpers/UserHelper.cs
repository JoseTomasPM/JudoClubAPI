using System.Security.Claims;

namespace JudoClubAPI.Helpers;

public static class UserHelper
{
    // Extrae el Id del usuario autenticado desde el token JWT
    public static int GetUserId(ClaimsPrincipal user)
        => int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    // Comprueba si el usuario autenticado es Admin
    public static bool IsAdmin(ClaimsPrincipal user)
        => user.IsInRole("Admin");
}