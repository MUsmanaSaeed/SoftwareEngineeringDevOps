namespace SoftwareEngineeringDevOps.App.Auth
{
    public enum UserRole
    {
        Standard,
        Editor,
        Admin
    }

    public static class RoleHelper
    {
        public static UserRole GetRole(Users.IUserInfo user)
        {
            if (user.IsAdmin) return UserRole.Admin;
            if (user.IsEditor) return UserRole.Editor;
            return UserRole.Standard;
        }

        public static bool CanCreate(UserRole role) => role == UserRole.Admin || role == UserRole.Editor;
        public static bool CanEdit(UserRole role) => role == UserRole.Admin || role == UserRole.Editor;
        public static bool CanDelete(UserRole role) => role == UserRole.Admin;
        public static bool CanManageUsers(UserRole role) => role == UserRole.Admin;
    }
}
