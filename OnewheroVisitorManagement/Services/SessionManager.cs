using OnewheroVisitorManagement.Models;

namespace OnewheroVisitorManagement.Services
{
    public static class SessionManager
    {
        public static User CurrentUser { get; set; }

        public static bool IsLoggedIn => CurrentUser != null;

        public static bool IsAdmin => CurrentUser?.Role == "Admin";

        public static void Logout()
        {
            CurrentUser = null;
        }
    }
}