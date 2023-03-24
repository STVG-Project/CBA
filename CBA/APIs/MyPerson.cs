using CBA.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using System;
using static CBA.APIs.MyFace;

namespace CBA.APIs
{
    public class MyPerson
    {

        public async Task<bool> editPerson(string code, string name, string codeSystem)
        {
            if (string.IsNullOrEmpty(codeSystem))
            {
                return false;
            }
            using (DataContext context = new DataContext())
            {

                SqlPerson? m_person = context.persons!.Where(s => s.isdeleted == false && s.codeSystem.CompareTo(codeSystem) == 0).FirstOrDefault();
                if (m_person == null)
                {
                    return false;
                }

                if (!string.IsNullOrEmpty(code))
                {
                    m_person.code = code;
                }
                if (!string.IsNullOrEmpty(name))
                {
                    m_person.name = name;
                }



                int rows = await context.SaveChangesAsync();
                return true;
            }
        }

        public class ItemGroupForPerson
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

        public class ItemAgeLevelForPerson
        {
            public string code { get; set; } = "";
            public string name { get; set; } = "";

        }

        public class ItemDeviceForFace
        {
            public string code { get; set; } = "";
            public string name { get; set; } = "";
            public string des { get; set; } = "";
        }
        

        public class ItemPerson
        {
            public string code { get; set; } = "";
            public string codeSystem { get; set; } = "";
            public string name { get; set; } = "";
            public string gender { get; set; } = "";
            public int age { get; set; } = 0;
            public string image { get; set; } = "";
            public ItemGroupForPerson group { get; set; } = new ItemGroupForPerson(); 
            public List<ItemFaceForPerson> faces { get; set; } = new List<ItemFaceForPerson>();
            public ItemAgeLevelForPerson level { get; set; } = new ItemAgeLevelForPerson();
            public string createdTime { get; set; } = "";
            public string lastestTime { get; set; } = "";

        }
        public class ListPersonPage
        {
            public int total { get; set; } = 0;
            public int page { get; set; } = 0;
            public List<ItemPerson> persons { get; set; } = new List<ItemPerson>();
        }

        public ListPersonPage getListPerson(int page, int count)
        {
            using (DataContext context = new DataContext())
            {
                ListPersonPage list = new ListPersonPage();

                
                List<SqlPerson> persons = context.persons!.Where(s => s.isdeleted == false)
                                                          .Include(s => s.faces!).ThenInclude(s => s.device)
                                                          .Include(s => s.level)
                                                          .Include(s => s.group).Include(s => s.faces!).ThenInclude(s => s.device)
                                                          .OrderByDescending(s => s.lastestTime)
                                                          .ToList();
                if (persons.Count> 0)
                {
                    if (page > persons.Count)
                    {
                        return new ListPersonPage();
                    }
                    list.total = persons.Count();
                    list.page = page;
                    for (int i = 0; i < count; i++)
                    {
                        int index = page + i;
                        if (index > persons.Count)
                        {
                            break;
                        }

                        ItemPerson item = new ItemPerson();
                        item.code = persons[index - 1].code;
                        item.codeSystem = persons[index - 1].codeSystem;
                        item.name = persons[index - 1].name;
                        item.gender = persons[index - 1].gender;
                        item.age = persons[index - 1].age;
                        if (persons[index - 1].group != null)
                        {
                            item.group.code = persons[index - 1].group!.code;
                            item.group.name = persons[index - 1].group!.name;
                            item.group.des = persons[index - 1].group!.des;
                        }

                        if (persons[index - 1].faces != null)
                        {
                            List<SqlFace>? tmpFace = persons[index - 1].faces!.OrderByDescending(s => s.createdTime).ToList();
                            if (tmpFace.Count > 0)
                            {
                                item.image = tmpFace[tmpFace.Count - 1].image;

                                foreach (SqlFace tmp in tmpFace)
                                {
                                    ItemFaceForPerson itemFace = new ItemFaceForPerson();

                                    itemFace.image = tmp.image;
                                    itemFace.time = tmp.createdTime.ToLocalTime().ToString("dd-MM-yyyy HH:mm:ss");

                                    if (tmp.device != null)
                                    {
                                        itemFace.device = tmp.device.code;
                                    }

                                    item.faces.Add(itemFace);

                                }
                            }
                        }

                        if (persons[index -1].level != null)
                        {
                            item.level.code = persons[index - 1].level!.code;
                            item.level.name = persons[index - 1].level!.name;
                        }
                        else
                        {
                            string timeStart = "24-03-2023 11:00:00";
                            DateTime time = DateTime.ParseExact(timeStart, "dd-MM-yyyy HH:mm:ss", null);
                            if (DateTime.Compare(persons[index - 1].createdTime.ToLocalTime(), time) < 0)
                            {
                                item.level.code = "NA";
                                item.level.name = "Not yet Update Range Ages";
                            }
                            else
                            {
                                item.level.code = "NA";
                                item.level.name = "Out Range Ages";
                            }
                        }    

                        item.createdTime = persons[index - 1].createdTime.ToLocalTime().ToString("dd-MM-yyyy HH:mm:ss");
                        item.lastestTime = persons[index - 1].lastestTime.ToLocalTime().ToString("dd-MM-yyyy HH:mm:ss");
                        list.persons.Add(item);    

                    }
                }
                return list;

            }
        }

        public class ItemPersonsHistory
        {
            public string device { get; set; } = "";
            public string data { get; set; } = "";
        }

        public string getListPersonHistory(DateTime begin, DateTime end)
        {
            using (DataContext context = new DataContext())
            {
                DateTime dateBegin = new DateTime(begin.Year, begin.Month, begin.Day, 00, 00, 00);
                DateTime dateEnd = new DateTime(end.Year, end.Month, end.Day, 00, 00, 00);
                dateEnd = dateEnd.AddDays(1);

                List<ItemPerson> items = new List<ItemPerson>();

                List<SqlPerson> persons = context.persons!.Where(s => DateTime.Compare(dateBegin.ToUniversalTime(), s.createdTime) <= 0 && DateTime.Compare(dateEnd.ToUniversalTime(), s.createdTime) > 0 && s.isdeleted == false)
                                                          .Include(s => s.group)
                                                          .Include(s => s.faces!).ThenInclude(s => s.device)
                                                          .Include(s => s.level)
                                                          .OrderByDescending(s => s.createdTime)
                                                          .ToList();
                if (persons.Count > 0)
                {
                    foreach (SqlPerson m_person in persons)
                    {
                        ItemPerson item = new ItemPerson();
                        item.code = m_person.code;
                        item.codeSystem = m_person.codeSystem;
                        item.name = m_person.name;
                        item.gender = m_person.gender;
                        item.age = m_person.age;
                        if (m_person.group != null)
                        {
                            item.group.code = m_person.group!.code;
                            item.group.name = m_person.group!.name;
                            item.group.des = m_person.group!.des;
                        }

                        if (m_person.faces != null)
                        {
                            List<SqlFace>? tmpFace = m_person.faces!.OrderByDescending(s => s.createdTime).ToList();
                            if (tmpFace.Count > 0)
                            {
                                item.image = tmpFace[tmpFace.Count - 1].image;

                                foreach (SqlFace tmp in tmpFace)
                                {
                                    ItemFaceForPerson itemFace = new ItemFaceForPerson();

                                    itemFace.image = tmp.image;
                                    itemFace.time = tmp.createdTime.ToLocalTime().ToString("dd-MM-yyyy HH:mm:ss");

                                    if (tmp.device != null)
                                    {
                                        itemFace.device = tmp.device.code;
                                    }

                                    item.faces.Add(itemFace);
                                }
                            }
                        }

                        if (m_person.level != null)
                        {
                            item.level.code = m_person.level!.code;
                            item.level.name = m_person.level!.name;
                        }
                        else
                        {
                            string timeStart = "24-03-2023 11:00:00";
                            DateTime time = DateTime.ParseExact(timeStart, "dd-MM-yyyy HH:mm:ss", null);
                            if (DateTime.Compare(m_person.createdTime.ToLocalTime(), time) < 0)
                            {
                                item.level.code = "NA";
                                item.level.name = "Not yet Update Range Ages";
                            }
                            else
                            {
                                item.level.code = "NA";
                                item.level.name = "Out Range Ages";
                            }    
                        }

                        item.createdTime = m_person.createdTime.ToLocalTime().ToString("dd-MM-yyyy HH:mm:ss");
                        item.lastestTime = m_person.lastestTime.ToLocalTime().ToString("dd-MM-yyyy HH:mm:ss");
                        items.Add(item);
                    }
                }
                string temp = JsonConvert.SerializeObject(items);
                return temp;

            }
        }

    }
}
