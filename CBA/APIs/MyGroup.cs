using CBA.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;
using static CBA.APIs.MyPerson;

namespace CBA.APIs
{
    public class MyGroup
    {
        public MyGroup() { }
        public async Task initAsync()
        {
            using (DataContext context = new DataContext())
            {
                SqlGroup? group = context.groups!.Where(s => s.code.CompareTo("NV") == 0 && s.isdeleted == false).FirstOrDefault();
                if (group == null)
                {
                    SqlGroup item = new SqlGroup();
                    item.ID = DateTime.Now.Ticks;
                    item.code = "NV";
                    item.name = "Nhân viên";
                    item.des = "Nhân viên";
                    item.isdeleted = false;
                    context.groups!.Add(item);
                }

                group = context.groups!.Where(s => s.code.CompareTo("VIP") == 0 && s.isdeleted == false).FirstOrDefault();
                if (group == null)
                {
                    SqlGroup item = new SqlGroup();
                    item.ID = DateTime.Now.Ticks;
                    item.code = "VIP";
                    item.name = "Khách VIP";
                    item.des = "Khách VIP";
                    item.isdeleted = false;
                    context.groups!.Add(item);
                }

                int rows = await context.SaveChangesAsync();
            }
        }

        
        public async Task<bool> createGroup(string code, string name, string des)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(code) || string.IsNullOrEmpty(des))
            {
                return false;
            }
            using (DataContext context = new DataContext())
            {
                SqlGroup? group = context.groups!.Where(s => s.isdeleted == false && s.code.CompareTo(code) == 0).FirstOrDefault();
                if (group != null)
                {
                    return false;
                }

                group = new SqlGroup();
                group.ID = DateTime.Now.Ticks;
                group.code = code;
                group.name = name;
                group.des = des;
                group.isdeleted = false;

                context.groups!.Add(group);

                int rows = await context.SaveChangesAsync();
                bool flag = false;
                if (rows > 0)
                {
                    return flag;
                }
                else
                {
                    return false;
                }
            }
        }

        public async Task<bool> editGroup(string code, string name, string des)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(code) || string.IsNullOrEmpty(des))
            {
                return false;
            }
            using (DataContext context = new DataContext())
            {
                SqlGroup? group = context.groups!.Where(s => s.isdeleted == false && s.code.CompareTo(code) == 0).FirstOrDefault();
                if (group == null)
                {
                    return false;
                }

                group.name = name;
                group.des = des;

                int rows = await context.SaveChangesAsync();
                try
                {
                    bool flag = Program.api_age.clearCache();
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
                return true;
            }
        }

        public async Task<bool> deleteGroup(string code)
        {
            using (DataContext context = new DataContext())
            {
                SqlGroup? group = context.groups!.Where(s => s.isdeleted == false && s.code.CompareTo(code) == 0).FirstOrDefault();
                if (group == null)
                {
                    return false;
                }
                bool flag = false;

                if (group.persons != null)
                {
                    List<SqlPerson> tmp = group.persons.Where(s => s.isdeleted == false).ToList();
                    foreach(SqlPerson person in tmp)
                    {
                        flag = await cleanPersonAsync(person.code, code);
                        if(flag)
                        {
                            continue;
                        }    
                        else
                        {
                            return false;
                        }    
                    }    
                }    

                group.isdeleted = true;

                int rows = await context.SaveChangesAsync();
                if (rows > 0)
                {
                    try
                    {
                        flag = Program.api_age.clearCache();
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex.ToString());
                    }
                    return flag;
                }
                else
                {
                    return false;
                }
            }
        }


        public async Task<bool> SetPersonAsync(string person, string group)
        {
            using (DataContext context = new DataContext())
            {
                SqlGroup? m_group = context.groups!.Where(s => s.isdeleted == false && s.code.CompareTo(group) == 0).Include(s => s.persons).FirstOrDefault();

                if (m_group == null)
                {
                    return false;
                }

                SqlPerson? m_person = context.persons!.Where(s => s.isdeleted == false && s.codeSystem.CompareTo(person) == 0).FirstOrDefault();
                if (m_person == null)
                {
                    return false;
                }

                if(m_group.persons == null)
                {
                    m_group.persons = new List<SqlPerson>();
                }    

                SqlPerson? tmp = m_group.persons!.Where(s => s.ID == m_person.ID).FirstOrDefault();
                if(tmp == null)
                {
                    m_group.persons.Add(m_person);
                }
                bool flag = false;
                try
                {
                    flag = Program.api_age.clearCache();
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
                if(flag)
                {
                    int rows = await context.SaveChangesAsync();
                    if (rows > 0)
                    {
                        return flag;
                    }
                    else
                    {
                        return false;
                    }    
                }    
                else
                {
                    return false;
                }    
            }
        }

        public async Task<bool> cleanPersonAsync(string person, string group)
        {
            using (DataContext context = new DataContext())
            {
                SqlGroup? m_group = context.groups!.Where(s => s.isdeleted == false && s.code.CompareTo(group) == 0).Include(s => s.persons).FirstOrDefault();

                if (m_group == null)
                {
                    return false;
                }
                if(m_group.persons == null)
                {
                    return false;
                }    
                    

                SqlPerson? m_person = context.persons!.Where(s => s.isdeleted == false && s.codeSystem.CompareTo(person) == 0).FirstOrDefault();

                if (m_person == null)
                {
                    return false;
                }

                SqlPerson? tmp = m_group.persons!.Where(s => s.ID == m_person.ID).FirstOrDefault();
                if (tmp != null)
                {
                    m_group.persons.Remove(tmp);
                }


                bool flag = false;
                try
                {
                    flag = Program.api_age.clearCache();
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
                if (flag)
                {
                    int rows = await context.SaveChangesAsync();
                    if (rows > 0)
                    {
                        return flag;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
        }


        public class ItemGroup
        {
            public string code { get; set; } = "";
            public string name { get; set; } = "";
            public string des { get; set; } = "";

        }

        public List<ItemGroup> getListGroup()
        {
            using (DataContext context = new DataContext())
            {
                List<ItemGroup> list = new List<ItemGroup>();
                List<SqlGroup> groups = context.groups!.Where(s => s.isdeleted == false).ToList();
                if (groups.Count > 0)
                {
                    foreach (SqlGroup group in groups)
                    {
                        ItemGroup item = new ItemGroup();
                        item.code = group.code;
                        item.name = group.name;
                        item.des = group.des;

                        list.Add(item);
                    }
                }
                return list;
            }
        }
        public class ItemDeviceForFace
        {
            public string code { get; set; } = "";
            public string name { get; set; } = "";
            public string des { get; set; } = "";
        }

        public class ItemFaceForPerson
        {
            public string image { get; set; } = "";
            public string time { get; set; } = "";
            public string device { get; set; } = "";
           
        }

        public class ItemPersonForGroup
        {
            public string code { get; set; } = "";
            public string codeSystem { get; set; } = "";
            public string name { get; set; } = "";
            public string gender { get; set; } = "";
            public int age { get; set; } = 0;
            public string image { get; set; } = "";
            public List<ItemFaceForPerson> faces { get; set; } = new List<ItemFaceForPerson>();
        }

        public List<ItemPersonForGroup> getListPersonInGroup(string code)
        {
            using (DataContext context = new DataContext())
            {
                List<ItemPersonForGroup> list = new List<ItemPersonForGroup>();
                List<SqlGroup> groups = context.groups!.Where(s => s.isdeleted == false && s.code.CompareTo(code) == 0).Include(s => s.persons!).ThenInclude(s => s.faces!).ThenInclude(s => s.device).ToList();
                if (groups.Count > 0)
                {
                    foreach (SqlGroup group in groups)
                    {
                        if (group.persons != null)
                        {
                            foreach (SqlPerson item in group.persons)
                            {
                                ItemPersonForGroup tmp = new ItemPersonForGroup();

                                tmp.code = item.code;
                                tmp.codeSystem = item.codeSystem;
                                tmp.name = item.name;
                                tmp.gender = item.gender;
                                tmp.age = item.age;
                                if (item.faces != null)
                                {
                                    List<SqlFace>? tmpFace = item.faces!.OrderByDescending(s => s.createdTime).ToList();
                                    if(tmpFace.Count > 0)
                                    {
                                        tmp.image = tmpFace[tmpFace.Count - 1].image;
                                        foreach (SqlFace face in tmpFace)
                                        {
                                            ItemFaceForPerson itemFace = new ItemFaceForPerson();

                                            itemFace.time = face.createdTime.ToLocalTime().ToString("dd-MM-yyyy HH:mm:ss");
                                            itemFace.image = face.image;

                                            if (face.device != null)
                                            {
                                                itemFace.device = face.device.code;
                                            }

                                            tmp.faces.Add(itemFace);
                                        }
                                    }
                                }

                                list.Add(tmp);
                            }
                        }
                    }
                }
                return list;
            }
        }
    }
}
