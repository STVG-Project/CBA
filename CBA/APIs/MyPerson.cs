using CBA.Models;
using Microsoft.EntityFrameworkCore;
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

                
                List<SqlPerson> persons = context.persons!.Where(s => s.isdeleted == false).Include(s => s.faces!).ThenInclude(s => s.device).Include(s => s.group).Include(s => s.faces!).ThenInclude(s => s.device).OrderByDescending(s => s.createdTime).ToList();
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
                            List<SqlFace>? tmpFace = persons[index - 1].faces!.OrderBy(s => s.createdTime).ToList();
                            if (tmpFace.Count > 0)
                            {
                                item.image = tmpFace[0].image;

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

                        item.createdTime = persons[index - 1].createdTime.ToLocalTime().ToString("dd-MM-yyyy HH:mm:ss");
                        item.lastestTime = persons[index - 1].lastestTime.ToLocalTime().ToString("dd-MM-yyyy HH:mm:ss");
                        list.persons.Add(item);    

                    }
                }
                return list;

            }
        }
    }
}
