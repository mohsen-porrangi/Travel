using UserManagement.API.Infrastructure.Data.Models;

namespace UserManagement.API.Infrastructure.Seeders
{
    public static class PermissionSeeder
    {
        public static List<Permission> Seed()
        {
            return new List<Permission>
        {
            new() { Module = "User", Action = "View", Description = "مشاهده کاربران" },
            new() { Module = "User", Action = "Edit", Description = "ویرایش کاربران" },
            new() { Module = "User", Action = "Delete", Description = "حذف کاربران" },

            new() { Module = "Role", Action = "Create", Description = "ایجاد نقش" },
            new() { Module = "Role", Action = "Edit", Description = "ویرایش نقش" },
            new() { Module = "Role", Action = "Delete", Description = "حذف نقش" },
            new() { Module = "Role", Action = "AssignPermission", Description = "اختصاص دسترسی به نقش" },

            new() { Module = "Flight", Action = "View", Description = "مشاهده پروازها" },
            new() { Module = "Flight", Action = "Create", Description = "ایجاد پرواز" },

            new() { Module = "Wallet", Action = "View", Description = "مشاهده کیف‌پول" },
            new() { Module = "Wallet", Action = "Charge", Description = "شارژ کیف‌پول" }
        };
        }
    }
}