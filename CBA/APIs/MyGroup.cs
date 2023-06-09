﻿using CBA.Models;
using Microsoft.EntityFrameworkCore;

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

                group = context.groups!.Where(s => s.code.CompareTo("LA") == 0 && s.isdeleted == false).FirstOrDefault();
                if (group == null)
                {
                    SqlGroup item = new SqlGroup();
                    item.ID = DateTime.Now.Ticks;
                    item.code = "LA";
                    item.name = "Khách lạ";
                    item.des = "Khách lạ";
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

        public async Task<bool> deleteGroup(string code)
        {
            using (DataContext context = new DataContext())
            {
                SqlGroup? group = context.groups!.Where(s => s.isdeleted == false && s.code.CompareTo(code) == 0).FirstOrDefault();
                if (group == null)
                {
                    return false;
                }

                group.isdeleted = true;

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


        public async Task<bool> SetPersonAsync(string person, string group)
        {
            using (DataContext context = new DataContext())
            {
                SqlGroup? m_group = context.groups!.Where(s => s.isdeleted == false && s.code.CompareTo(group) == 0).FirstOrDefault();

                if (m_group == null)
                {
                    return false;
                }

                SqlPerson? m_person = context.persons!.Where(s => s.isdeleted == false && s.codeSystem.CompareTo(person) == 0).FirstOrDefault();

                if (m_person == null)
                {
                    return false;
                }
                if (m_group.persons == null)
                {
                    m_group.persons = new List<SqlPerson>();
                }


                m_group.persons.Add(m_person);

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

        public async Task<bool> cleanPersonAsync(string person, string group)
        {
            using (DataContext context = new DataContext())
            {
                SqlGroup? m_group = context.groups!.Where(s => s.isdeleted == false && s.code.CompareTo(group) == 0).FirstOrDefault();

                if (m_group == null)
                {
                    return false;
                }

                SqlPerson? m_person = context.persons!.Where(s => s.isdeleted == false && s.codeSystem.CompareTo(person) == 0).FirstOrDefault();

                if (m_person == null)
                {
                    return false;
                }


                if (m_group.persons != null)
                {
                    SqlPerson? tmp = m_group.persons!.Where(s => s.code.CompareTo(person) == 0).FirstOrDefault();

                    if (tmp != null)
                    {
                        m_group.persons.Remove(tmp);
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


        public class ItemFaceForPerson
        {
            public string image { get; set; } = "";
            public string createdTime { get; set; } = "";
           
        }

        public class ItemPersonForGroup
        {
            public string code { get; set; } = "";
            public string name { get; set; } = "";
            public string gender { get; set; } = "";
            public int age { get; set; } = 0;
            public List<ItemFaceForPerson> faces { get; set; } = new List<ItemFaceForPerson>();
        }

        public List<ItemPersonForGroup> getListPersonInGroup(string code)
        {
            using (DataContext context = new DataContext())
            {
                List<ItemPersonForGroup> list = new List<ItemPersonForGroup>();
                List<SqlGroup> groups = context.groups!.Where(s => s.isdeleted == false && s.code.CompareTo(code) == 0).Include(s => s.persons!).ThenInclude(s => s.faces).ToList();
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
                                tmp.name = item.name;
                                tmp.gender = item.gender;
                                tmp.age = item.age;
                                if (item.faces != null)
                                {
                                    foreach (SqlFace face in item.faces)
                                    {
                                        ItemFaceForPerson itemFace = new ItemFaceForPerson();

                                        itemFace.createdTime = face.createdTime.ToLocalTime().ToString("dd-MM-yyyy HH:mm:ss");
                                        itemFace.image = face.image;

                                        tmp.faces.Add(itemFace);
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
