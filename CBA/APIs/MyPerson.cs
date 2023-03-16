using CBA.Models;
using Microsoft.EntityFrameworkCore;
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

        public List<ItemPerson> getListPerson()
        {
            using (DataContext context = new DataContext())
            {
                List<ItemPerson> list = new List<ItemPerson>();
                List<SqlPerson> persons = context.persons!.Where(s => s.isdeleted == false).Include(s => s.faces!).ThenInclude(s => s.device).Include(s => s.group).Include(s => s.faces!).ThenInclude(s => s.device).OrderByDescending(s => s.createdTime).ToList();
                if (persons.Count > 0)
                {
                    foreach (SqlPerson person in persons)
                    {
                        ItemPerson item = new ItemPerson();
                        item.code = person.code;
                        item.codeSystem = person.codeSystem;
                        item.name = person.name;
                        item.gender = person.gender;
                        item.age = person.age;
                        if (person.group != null)
                        {
                            item.group.code = person.group.code;
                            item.group.name = person.group.name;
                            item.group.des = person.group.des;
                        }

                        if (person.faces != null)
                        {
                            List<SqlFace>? tmpFace = person.faces!.OrderBy(s => s.createdTime).ToList();
                            if(tmpFace.Count > 0)
                            {
                                item.image = tmpFace[0].image;

                                foreach (SqlFace tmp in tmpFace)
                                {
                                    ItemFaceForPerson itemFace = new ItemFaceForPerson();

                                    itemFace.image = tmp.image;
                                    itemFace.time = tmp.createdTime.ToLocalTime().ToString("dd-MM-yyyy HH:mm:ss");

                                    if(tmp.device != null)
                                    {
                                        itemFace.device = tmp.device.code;
                                    }

                                    item.faces.Add(itemFace);

                                }
                            }    
                            
                        }

                        item.createdTime = person.createdTime.ToLocalTime().ToString("dd-MM-yyyy HH:mm:ss");
                        item.lastestTime = person.lastestTime.ToLocalTime().ToString("dd-MM-yyyy HH:mm:ss");

                        list.Add(item);
                    }
                }
                return list;
            }
        }
    }
}
