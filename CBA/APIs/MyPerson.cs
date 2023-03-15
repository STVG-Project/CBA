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

        public class ItemFaceForPerson
        {
            public string image { get; set; } = "";
            public string createdTime { get; set; } = "";
           // public ItemDeviceForFace device { get; set; } = new ItemDeviceForFace();
        }

        public class ItemPerson
        {
            public string code { get; set; } = "";
            public string codeSytem { get; set; } = "";
            public string name { get; set; } = "";
            public string gender { get; set; } = "";
            public int age { get; set; } = 0;
            public ItemGroupForPerson? group { get; set; } = null;
            public List<ItemFaceForPerson> faces { get; set; } = new List<ItemFaceForPerson>();
            public string createdTime { get; set; } = "";
            public string lastestTime { get; set; } = "";

        }

        public List<ItemPerson> getListPerson()
        {
            using (DataContext context = new DataContext())
            {
                List<ItemPerson> list = new List<ItemPerson>();
                List<SqlPerson> persons = context.persons!.Where(s => s.isdeleted == false).Include(s => s.faces!).ThenInclude(s => s.device).Include(s => s.group).Include(s => s.faces).ToList();
                if (persons.Count > 0)
                {
                    foreach (SqlPerson person in persons)
                    {
                        ItemPerson item = new ItemPerson();
                        item.code = person.code;
                        item.codeSytem = person.codeSystem;
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
                            foreach (SqlFace tmp in person.faces)
                            {
                                ItemFaceForPerson itemFace = new ItemFaceForPerson();

                                itemFace.image = tmp.image;
                                itemFace.createdTime = tmp.createdTime.ToLocalTime().ToString("dd-MM-yyyy HH:mm:ss");
                               /* if (tmp.device != null)
                                {
                                    itemFace.device.code = tmp.device.code;
                                    itemFace.device.name = tmp.device.name;
                                    itemFace.device.des = tmp.device.des;
                                }*/
                                item.faces.Add(itemFace);

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
