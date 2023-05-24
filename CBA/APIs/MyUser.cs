using CBA.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using static CBA.APIs.MyPerson;
using static CBA.Program;

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
                    item.username = "backend";
                    item.password = "1234";
                    item.role = context.roles!.Where(s => s.isdeleted == false && s.code.CompareTo("system") == 0).FirstOrDefault();
                    item.token = "00001111";
                    item.displayName = "stvg";
                    item.des = "stvg";
                    item.phoneNumber = "123456789";
                    item.isdeleted = false;
                    context.users!.Add(item);
                }

                int rows = await context.SaveChangesAsync();
            }
        }

        

        public async Task callUserLoginAsync(CancellationToken cancellationToken, string id)
        {
            await Task.Delay(100);
            Thread t = new Thread(async () =>
            {
                while (true)
                {
                    Program.CacheForUser? m_cache = Program.caches.Where(s => s.id.CompareTo(id) == 0).FirstOrDefault();
                    if (m_cache == null)
                    {
                        break;
                    }

                    if (cancellationToken.IsCancellationRequested)
                    {
                        Console.WriteLine("Cancel !!!");
                        Program.caches.Remove(m_cache);
                        break;
                    }
                    if (!m_cache.flag)
                    {
                        Console.WriteLine("Check connection !!!");
                        Program.caches.Remove(m_cache);
                        break;
                    }
                    await Task.Delay(1000);
                }
            });
            t.Start();
        }
        public CacheForUser checkUserLogin(string token)
        {
            using (DataContext context = new DataContext())
            {
                SqlUser? user = context.users!.Where(s => s.isdeleted == false && s.token.CompareTo(token) == 0).Include(s => s.role).FirstOrDefault();
                if (user == null)
                {
                    return new CacheForUser();
                }


                string id = DateTime.Now.Ticks.ToString();
                Program.CacheForUser httpcache = new Program.CacheForUser();
                httpcache.id = id;
                httpcache.flag = true;
                Program.caches.Add(httpcache);

                return httpcache;
            }
        }

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
                return user.ID;
            }
        }

        public long checkSys(string token)
        {
            using (DataContext context = new DataContext())
            {
                SqlUser? user = context.users!.Where(s => s.isdeleted == false && s.token.CompareTo(token) == 0).Include(s => s.role).FirstOrDefault();
                if (user == null)
                {
                    return -1;
                }
                
                if (user.role!.code.CompareTo("system") == 0 || user.token.CompareTo("1234567890") == 0)
                {
                    return user.ID;
                }
                return -1;
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
                SqlUser? m_user = context.users!.Where(s => s.user.CompareTo(user) == 0 && s.isdeleted == false).Include(s => s.role).FirstOrDefault();
                if (m_user == null)
                {
                    return false;
                }

                if (m_user.role!.code.CompareTo("system") == 0)
                {
                    return false;
                }

                if (!string.IsNullOrEmpty(password))
                {
                    m_user.password = password;
                    m_user.token = createToken();

                }


                if (!string.IsNullOrEmpty(displayName))
                {
                    m_user.displayName = displayName;
                }
                if (!string.IsNullOrEmpty(phoneNumber))
                {
                    m_user.phoneNumber = phoneNumber;
                }

                if (!string.IsNullOrEmpty(des))
                {
                    m_user.des = des;
                }

                if (!string.IsNullOrEmpty(role))
                {
                    SqlRole? m_role = context.roles!.Where(s => s.isdeleted == false && s.code.CompareTo(role) == 0).FirstOrDefault();
                    if (m_role != null)
                    {
                        m_user.role = m_role;
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
                    if (user.role!.code.CompareTo("system") != 0)
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
                }
                return items;
            }
        }

        public class ItemPersonDetect
        {
            public string codeSystem { get; set; } = "";
            public string name { get; set; } = "";
            public string gender { get; set; } = "";
            public int age { get; set; } = 0;
        }

        public class DataLogRaw
        {
            public ItemPersonDetect person { get; set; } = new ItemPersonDetect();
            public string device { get; set; } = "";
            public string group { get; set; } = "";
            public string image { get; set; } = "";
            public string note { get; set; } = "";
            public DateTime time { get; set; }
        }

        public List<DataLogRaw> getDataRaw(DateTime begin, DateTime end)
        {
            List<DataLogRaw> list = new List<DataLogRaw>();
            using (DataContext context = new DataContext())
            {
                List<SqlLogPerson> datas = context.logs!.Include(s => s.person!).ThenInclude(s => s.group)
                                                        .Where(s => DateTime.Compare(begin, s.time) <= 0
                                                                    && DateTime.Compare(end, s.time) >= 0)
                                                        .Include(s => s.device).OrderByDescending(s => s.time)
                                                        .ToList();
                if(datas.Count > 0)
                {
                    foreach (SqlLogPerson item in datas)
                    {
                        DataLogRaw tmp = new DataLogRaw();
                        tmp.person.codeSystem = item.person!.codeSystem;
                        tmp.person.name = item.person.name;
                        tmp.person.gender = item.person.gender;
                        tmp.person.age = item.person.age;
                        if(item.person!.group != null)
                        {
                            tmp.group = item.person.group.code;
                        }
                        else
                        {
                            tmp.group = "0";
                        }

                        tmp.image = item.image;
                        tmp.note = item.note;
                        tmp.time = item.time;

                        if (item.device != null)
                        {
                            tmp.device = item.device.name;
                        }
                        
                        list.Add(tmp);
                    }
                }
                return list;
            }
        }
      
        public class ItemDetect
        {
            public ItemPersonDetect person { get; set; } = new ItemPersonDetect();
            public string image { get; set; } = "";
            public string time { get; set; } = "";
            public string note { get; set; } = "";
            public string group { get; set; } = "";
            public string device { get; set; } = "";
        }
        public List<ItemDetect> detectBlackList(string group)
        {
            DateTime m_end = DateTime.Now.ToUniversalTime();
            m_end = m_end.AddHours(-3);
            DateTime m_begin = m_end.AddMinutes(-3);
            List<ItemDetect> list = new List<ItemDetect>();
            List<DataLogRaw> datas = getDataRaw(m_begin, m_end);
            List<DataLogRaw> m_datas = datas.Where(s => s.group.CompareTo(group) == 0).ToList();
            foreach (DataLogRaw item in m_datas)
            {
                ItemDetect tmp = new ItemDetect();
                tmp.person.codeSystem = item.person.codeSystem;
                tmp.person.name = item.person.name;
                tmp.person.gender = item.person.gender;
                tmp.person.age = item.person.age;

                tmp.image = item.image;
                tmp.group = item.group;
                tmp.time = item.time.ToLocalTime().ToString("dd-MM-yyyy HH:mm:ss");
                
                tmp.device = item.device;

                list.Add(tmp);
            }
            return list;
        }
    }
}
