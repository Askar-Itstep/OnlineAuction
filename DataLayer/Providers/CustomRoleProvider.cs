using DataLayer.Entities;
using System;
using System.Linq;
using System.Web.Security;

namespace DataLayer.Providers
{
    public class CustomRoleProvider : RoleProvider
    {
        public override string ApplicationName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override void CreateRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            throw new NotImplementedException();
        }

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            throw new NotImplementedException();
        }

        public override string[] GetAllRoles()
        {
            throw new NotImplementedException();
        }

        public override string[] GetRolesForUser(string username)
        {
            string[] roles = new string[] { };
            if (username.Contains('@'))
            {
                username = username.Split('@')[0];
            }

            using (Model1 db = new Model1())
            {
                var userRoles = db.RoleAccountLinks.Include("Role").Include("Account").Where(u => u.Account.Email.Contains(username)).
                    Select(u => u.Role.RoleName).ToList();
                return userRoles.ToArray();
            }
        }

        public override string[] GetUsersInRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override bool IsUserInRole(string username, string roleName)
        {
            using (Model1 db = new Model1())
            {
                // Получаем пользователя          
                var userRoles = db.RoleAccountLinks.Include("Role").Include("Account")
                    .Where(u => u.Account.FullName == username).Where(u => u.Role.RoleName == roleName).ToList();
                if (userRoles != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override bool RoleExists(string roleName)
        {
            throw new NotImplementedException();
        }
    }
}
