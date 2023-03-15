using CBA.Models;
using Microsoft.EntityFrameworkCore;

namespace CBA.APIs
{
    public class MyUser
    {
        public MyUser()
        {
        }
        public async Task initAsync()
        {
            using (DataContext context = new DataContext())
            {
                SqlUser? user = context.users!.Where(s => s.user.CompareTo("admin") == 0).FirstOrDefault();
                if (user == null)
                {
                    SqlUser item = new SqlUser();
                    item.ID = DateTime.Now.Ticks;
                    item.user = "admin";
                    item.username = "admin";
                    item.password = "123456";
                    item.role = context.roles!.Where(s => s.isdeleted == false && s.code.CompareTo("admin") == 0).FirstOrDefault();
                    item.token = "1234567890";
                    item.displayName = "admin";
                    item.des = "admin";
                    item.phoneNumber = "123456";
                    item.isdeleted = false;
                    context.users!.Add(item);
                }

                user = context.users!.Where(s => s.user.CompareTo("manager") == 0).FirstOrDefault();
                if (user == null)
                {
                    SqlUser item = new SqlUser();
                    item.ID = DateTime.Now.Ticks;
                    item.user = "manager";
                    item.username = "manager";
                    item.password = "123456";
                    item.role = context.roles!.Where(s => s.isdeleted == false && s.code.CompareTo("manager") == 0).FirstOrDefault();
                    item.token = createToken();
                    item.displayName = "manager";
                    item.des = "manager";
                    item.phoneNumber = "123456789";
                    item.isdeleted = false;
                    context.users!.Add(item);
                }

                user = context.users!.Where(s => s.user.CompareTo("viewer") == 0).FirstOrDefault();
                if (user == null)
                {
                    SqlUser item = new SqlUser();
                    item.ID = DateTime.Now.Ticks;
                    item.user = "viewer";
                    item.username = "viewer";
                    item.password = "123456";
                    item.role = context.roles!.Where(s => s.isdeleted == false && s.code.CompareTo("viewer") == 0).FirstOrDefault();
                    item.token = createToken();
                    item.displayName = "viewer";
                    item.des = "viewer";
                    item.phoneNumber = "123456789";
                    item.isdeleted = false;
                    context.users!.Add(item);
                }

                user = context.users!.Where(s => s.user.CompareTo("stvg") == 0).FirstOrDefault();
                if (user == null)
                {
                    SqlUser item = new SqlUser();
                    item.ID = DateTime.Now.Ticks;
                    item.user = "stvg";
                    item.username = "stvg";
                    item.password = "123456";
                    item.role = context.roles!.Where(s => s.isdeleted == false && s.code.CompareTo("specialist") == 0).FirstOrDefault();
                    item.token = createToken();
                    item.displayName = "stvg";
                    item.des = "stvg";
                    item.phoneNumber = "123456789";
                    item.isdeleted = false;
                    context.users!.Add(item);
                }

                int rows = await context.SaveChangesAsync();
            }
        }
        /*public long checkAdmin(string token)
        {
            using (DataContext context = new DataContext())
            {
                SqlUser? user = context.users!.Where(s => s.isdeleted == false && s.token.CompareTo(token) == 0).Include(s => s.role).FirstOrDefault();
                if (user == null)
                {
                    return -1;
                }
                if (user.role!.code.CompareTo("admin") != 0)
                {
                    return -1;
                }
                return 0;
            }
        }
*/

        public long checkUser(string token)
        {
            using (DataContext context = new DataContext())
            {
                SqlUser? user = context.users!.Where(s => s.isdeleted == false && s.token.CompareTo(token) == 0).Include(s => s.role).FirstOrDefault();
                if (user == null)
                {
                    return -1;
                }
                if (user.role!.code.CompareTo("viewer") == 0)
                {
                    return -1;
                }
                return 0;
            }
        }


        public class InfoUserSystem
        {
            public string user { get; set; } = "";
            public string token { get; set; } = "";
            public string role { get; set; } = "";
        }

        public InfoUserSystem login(string username, string password)
        {
            using (DataContext context = new DataContext())
            {
                SqlUser? user = context.users!.Where(s => s.isdeleted == false && s.username.CompareTo(username) == 0 && s.password.CompareTo(password) == 0).Include(s => s.role).AsNoTracking().FirstOrDefault();
                if (user == null)
                {
                    return new InfoUserSystem();
                }
                InfoUserSystem info = new InfoUserSystem();
                info.user = user.user;
                info.token = user.token;
                info.role = user.role!.code;
                return info;
            }
        }

        private string createToken()
        {
            using (DataContext context = new DataContext())
            {
                string token = DataContext.randomString(64);
                while (true)
                {
                    SqlUser? user = context.users!.Where(s => s.token.CompareTo(token) == 0).AsNoTracking().FirstOrDefault();
                    if (user == null)
                    {
                        break;
                    }
                    token = DataContext.randomString(64);
                }
                return token;
            }
        }

        public async Task<bool> createUserAsync(string token, string user, string username, string password, string displayName, string phoneNumber, string des, string role)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(user) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(role) || string.IsNullOrEmpty(displayName) || string.IsNullOrEmpty(phoneNumber))
            {
                return false;
            }

            using (var context = new DataContext())
            {
                SqlUser? own_user = context.users!.Where(s => s.isdeleted == false && s.token.CompareTo(token) == 0).Include(s => s.role).FirstOrDefault();
                if (own_user == null)
                {
                    return false;
                }
                if (own_user.role == null)
                {
                    return false;
                }

                SqlUser? tmp = context.users!.Where(s => s.isdeleted == false && (s.user.CompareTo(user) == 0 || s.username.CompareTo(username) == 0)).FirstOrDefault();
                if (tmp != null)
                {
                    return false;
                }
                SqlRole? m_role = context.roles!.Where(s => s.isdeleted == false && s.code.CompareTo(role) == 0).FirstOrDefault();
                if (m_role == null)
                {
                    return false;
                }
                SqlUser new_user = new SqlUser();
                new_user.ID = DateTime.Now.Ticks;
                new_user.user = user;
                new_user.username = username;
                new_user.password = password;
                new_user.role = m_role;
                new_user.des = des;
                new_user.isdeleted = false;
                new_user.displayName = displayName;
                new_user.phoneNumber = phoneNumber;
                new_user.token = createToken();
                context.users!.Add(new_user);
                int rows = await context.SaveChangesAsync();
                return true;
            }
        }

        public async Task<bool> editUserAsync(string token, string user, string password, string displayName, string phoneNumber, string des, string role)
        {
            if (string.IsNullOrEmpty(user))
            {
                return false;
            }
            using (DataContext context = new DataContext())
            {
                SqlUser? own_user = context.users!.Where(s => s.isdeleted == false && s.token.CompareTo(token) == 0).Include(s => s.role).FirstOrDefault();
                if (own_user == null)
                {
                    return false;
                }
                if (own_user.role == null)
                {
                    return false;
                }
                if (own_user.role!.code.CompareTo("admin") != 0)
                {
                    if (user.CompareTo(own_user.user) != 0)
                    {
                        return false;
                    }
                    own_user.des = des;

                    if (!string.IsNullOrEmpty(password))
                    {
                        own_user.password = password;
                        own_user.token = createToken();
                    }
                    if (!string.IsNullOrEmpty(displayName))
                    {
                        own_user.displayName = displayName;
                    }
                    if (!string.IsNullOrEmpty(phoneNumber))
                    {
                        own_user.phoneNumber = phoneNumber;
                    }

                }
                else
                {
                    SqlUser? tmp = context.users!.Where(s => s.user.CompareTo(user) == 0 && s.isdeleted == false).FirstOrDefault();
                    if (tmp == null)
                    {
                        return false;
                    }
                    tmp.des = des;


                    SqlRole? m_role = context.roles!.Where(s => s.isdeleted == false && s.code.CompareTo(role) == 0).FirstOrDefault();
                    if (m_role != null)
                    {
                        tmp.role = m_role;
                    }
                    if (!string.IsNullOrEmpty(password))
                    {
                        tmp.password = password;
                        tmp.token = createToken();
                    }
                    if (!string.IsNullOrEmpty(password))
                    {
                        own_user.password = password;
                        own_user.token = createToken();
                    }
                    if (!string.IsNullOrEmpty(displayName))
                    {
                        own_user.displayName = displayName;
                    }
                    if (!string.IsNullOrEmpty(phoneNumber))
                    {
                        own_user.phoneNumber = phoneNumber;
                    }
                }

                int rows = await context.SaveChangesAsync();
                if (rows > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public async Task<bool> deleteUserAsync(string token, string user)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(user))
            {
                return false;
            }
            using (DataContext context = new DataContext())
            {
                SqlUser? own_user = context.users!.Where(s => s.isdeleted == false && s.token.CompareTo(token) == 0).Include(s => s.role).AsNoTracking().FirstOrDefault();
                if (own_user == null)
                {
                    return false;
                }
                if (own_user.role == null)
                {
                    return false;
                }


                SqlUser? tmp = context.users!.Where(s => s.user.CompareTo(user) == 0 && s.isdeleted == false).FirstOrDefault();
                if (tmp == null)
                {
                    return false;
                }

                tmp.isdeleted = true;

                int rows = await context.SaveChangesAsync();
                if (rows > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }



        public async Task<string> setAvatarAsync(string token, byte[] file)
        {
            if (string.IsNullOrEmpty(token))
            {
                return "";
            }
            using (DataContext context = new DataContext())
            {
                SqlUser? user = context.users!.Where(s => s.isdeleted == false && s.token.CompareTo(token) == 0).FirstOrDefault();
                if (user == null)
                {
                    return "";
                }

                string code = await Program.api_file.saveFileAsync(string.Format("{0}.jpg", DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss")), file);
                if (string.IsNullOrEmpty(code))
                {
                    return "";
                }


                user.avatar = code;

                int rows = await context.SaveChangesAsync();
                if (rows > 0)
                {
                    return code;
                }
                else
                {
                    return "";
                }
            }
        }

      

        
/*
        public class ItemGroupForUser
        {
            public string code { get; set; } = "";
            public string name { get; set; } = "";
            public string des { get; set; } = "";
        }*/

        public class ItemUser
        {
            public string user { get; set; } = "";
            public string username { get; set; } = "";
            public string displayName { get; set; } = "";
            public string numberPhone { get; set; } = "";
            public string avatar { get; set; } = "";
            public string des { get; set; } = "";
            public string role { get; set; } = "";
        }

        public List<ItemUser> listUser(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return new List<ItemUser>();
            }
            using (DataContext context = new DataContext())
            {
                SqlUser? own_user = context.users!.Where(s => s.isdeleted == false && s.token.CompareTo(token) == 0).Include(s => s.role).FirstOrDefault();
                if (own_user == null)
                {
                    return new List<ItemUser>();
                }
                if (own_user.role == null)
                {
                    return new List<ItemUser>();
                }

                List<SqlUser> users = context.users!.Where(s => s.isdeleted == false).Include(s => s.role).ToList();
                List<ItemUser> items = new List<ItemUser>();
                foreach (SqlUser user in users)
                {
                    ItemUser item = new ItemUser();
                    item.user = user.user;
                    item.username = user.username;
                    item.des = user.des;
                    item.displayName = user.displayName;
                    item.numberPhone = user.phoneNumber;
                    item.avatar = user.avatar;
                    if (user.role != null)
                    {
                        item.role = user.role.name;
                    }

                   
                    items.Add(item);
                }
                return items;
            }
        }

    }
}
