using CBA.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace CBA.APIs
{
    public class MyFace
    {

        public async Task<bool> createFace(int age, string gender, byte[] image, string device, string codeSystem)
        {
            //string codefile = await Program.api_file.saveFileAsync(DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss.image"), image);
            int totalAge = 0;
            string codefile = Encoding.Unicode.GetString(image);

            if (string.IsNullOrEmpty(gender) || string.IsNullOrEmpty(codeSystem) || string.IsNullOrEmpty(codefile))
            {
                return false;
            }
            using (DataContext context = new DataContext())
            {
                /*SqlFace? face = context.faces!.Where(s => s.isdeleted == false).Include(s => s.person).FirstOrDefault();
                if (face != null)
                {
                    return false;
                }*/


                SqlFace? face = new SqlFace();
                face.ID = DateTime.Now.Ticks;
                face.age = age;
                face.gender = gender;
                face.person = context.persons!.Where(s => s.isdeleted == false && s.codeSystem.CompareTo(codeSystem) == 0).Include(s => s.faces).FirstOrDefault();
                if (face.person == null)
                {
                    SqlPerson tmp = new SqlPerson();
                    tmp.ID = DateTime.Now.Ticks;
                    tmp.codeSystem = codeSystem;
                    tmp.code = "person " + codeSystem;
                    tmp.gender = gender;
                    tmp.age = face.age;
                    tmp.createdTime = DateTime.Now.ToUniversalTime();
                    tmp.lastestTime = DateTime.Now.ToUniversalTime();
                    tmp.isdeleted = false;

                    face.person = tmp;
                    context.persons!.Add(tmp);
                }
                else
                {
                    foreach (SqlFace item in face.person.faces!)
                    {
                        totalAge += item.age;
                    }
                    face.person.age = (totalAge + age) / (face.person.faces.Count + 1);
                    face.person.lastestTime = DateTime.Now.ToUniversalTime();
                }

                face.createdTime = DateTime.Now.ToUniversalTime();
                face.image = codefile;
                face.device = context.devices!.Where(s => s.isdeleted == false && s.code.CompareTo(device) == 0).FirstOrDefault();
                face.isdeleted = false;


                context.faces!.Add(face);

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

        public async Task<bool> updateAge(string codeSystem)
        {
            int totalAge = 0;
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

                foreach (SqlFace item in m_person.faces!)
                {
                    totalAge += item.age;
                }
                m_person.age = totalAge / m_person.faces.Count;
                m_person.lastestTime = DateTime.Now.ToUniversalTime();
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


        public class ItemDeviceForFace
        {
            public string code { get; set; } = "";
            public string name { get; set; } = "";
            public string des { get; set; } = "";
        }

        /* public class ItemPersonForFace
         {
             public string code { get; set; } = "";
             public string name { get; set; } = "";
             public string des { get; set; } = "";
         }*/

        public class ItemFace
        {
            public string ID { get; set; } = "";
            public int age { get; set; } = 0;
            public string gender { get; set; } = "";
            public string image { get; set; } = "";
            public ItemDeviceForFace device { get; set; } = new ItemDeviceForFace();
        }

        public List<ItemFace> getListFace()
        {
            using (DataContext context = new DataContext())
            {
                List<ItemFace> list = new List<ItemFace>();
                List<SqlFace> faces = context.faces!.Where(s => s.isdeleted == false).Include(s => s.device).ToList();
                if (faces.Count > 0)
                {
                    foreach (SqlFace face in faces)
                    {
                        ItemFace item = new ItemFace();
                        item.ID = face.ID.ToString();
                        item.age = face.age;
                        item.gender = face.gender;
                        item.image = face.image;

                        if (face.device != null)
                        {
                            item.device.code = face.device.code;
                            item.device.name = face.device.name;
                            item.device.des = face.device.des;
                        }

                        list.Add(item);
                    }
                }
                return list;
            }
        }
    }
}
