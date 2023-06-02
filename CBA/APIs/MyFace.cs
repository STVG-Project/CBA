using CBA.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Serilog;
using System.Text;
using System.Text.RegularExpressions;
using static CBA.APIs.MyUser;
using static CBA.Program;

namespace CBA.APIs
{
    public class MyFace
    {
        public bool flag = false;

        /* public class ItemPersonDetect
         {
             public string codeSystem { get; set; } = "";
             public string name { get; set; } = "";
             public string gender { get; set; } = "";
             public int age { get; set; } = 0;
         }*/
        public class ItemDetectPerson
        {
            public string codeSystem { get; set; } = "";
            public string name { get; set; } = "";
            public string gender { get; set; } = "";
            public int age { get; set; } = 0;
            public string image { get; set; } = "";
            public string time { get; set; } = "";
            public string group { get; set; } = "";
            public string device { get; set; } = "";
            public bool alert { get; set; } = false; 
        }
        public async Task<bool> createFace(int age, string gender, byte[] image, string device, string codeSystem)
        {
            string codefile = await Program.api_file.saveFileAsync(DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss.image"), image);

            if (string.IsNullOrEmpty(gender) || string.IsNullOrEmpty(codeSystem) || string.IsNullOrEmpty(codefile) || string.IsNullOrEmpty(device))
            {
                return false;
            }

             
            
            using (DataContext context = new DataContext())
            {
                SqlPerson? sqlPerson = context.persons!.Where(s => s.isdeleted == false && s.codeSystem.CompareTo(codeSystem) == 0).Include(s => s.faces).Include(s => s.group).FirstOrDefault();
                if (sqlPerson == null)
                {
                    sqlPerson = new SqlPerson();
                    sqlPerson.ID = DateTime.Now.Ticks;
                    sqlPerson.codeSystem = codeSystem;
                    sqlPerson.code = "identify_" + codeSystem;
                    sqlPerson.gender = gender;
                    sqlPerson.age = age;
                    sqlPerson.createdTime = DateTime.Now.ToUniversalTime();
                    sqlPerson.lastestTime = DateTime.Now.ToUniversalTime();
                    sqlPerson.isdeleted = false;
                    sqlPerson.level = context.ages!.Where(s => (s.low <= sqlPerson.age && s.high >= sqlPerson.age) && s.isdeleted == false).FirstOrDefault();

                    context.persons!.Add(sqlPerson);
                    await context.SaveChangesAsync();

                }
                else
                {
                    if (sqlPerson.faces != null)
                    {
                        if (sqlPerson.faces.Count > 1)
                        {
                            int totalAge = 0;
                            foreach (SqlFace item in sqlPerson.faces)
                            {
                                totalAge += item.age;

                            }
                            sqlPerson.age = totalAge / sqlPerson.faces.Count;
                            sqlPerson.level = context.ages!.Where(s => (s.low <= sqlPerson.age && s.high >= sqlPerson.age) && s.isdeleted == false).FirstOrDefault();

                            sqlPerson.lastestTime = DateTime.Now.ToUniversalTime();
                            await context.SaveChangesAsync();

                        }
                    }
                }

                SqlDevice? sqlDevice = context.devices!.Where(s => s.isdeleted == false && s.code.CompareTo(device) == 0).FirstOrDefault();
                if (sqlDevice == null)
                {
                    sqlDevice = new SqlDevice();
                    sqlDevice.ID = DateTime.Now.Ticks;
                    sqlDevice.code = device;
                    sqlDevice.name = "tb_" + device;
                    sqlDevice.des = "tb_" + device;
                    sqlDevice.isdeleted = false;

                    context.devices!.Add(sqlDevice);
                    await context.SaveChangesAsync();

                }

                try
                {
                    SqlFace face = new SqlFace();
                    face.ID = DateTime.Now.Ticks;
                    face.gender = gender;
                    face.person = sqlPerson;
                    face.age = age;
                    face.device = sqlDevice;
                    face.image = codefile;
                    face.createdTime = DateTime.Now.ToUniversalTime();
                    face.isdeleted = false;

                    context.faces!.Add(face);
                    int rows = await context.SaveChangesAsync();
                    if (rows > 0)
                    {
                        Log.Information("Create new face !!!");
                    }

                    SqlLogPerson log = new SqlLogPerson();
                    log.ID = DateTime.Now.Ticks;
                    log.person = sqlPerson;
                    log.device = sqlDevice;
                    log.image = codefile;
                    log.note = string.Format("Person arrived : {0}", sqlPerson!.codeSystem);
                    log.time = DateTime.Now.ToUniversalTime();

                    context.logs!.Add(log);
                    await context.SaveChangesAsync();
                }
                catch(Exception ex)
                {
                    Log.Error(ex.ToString());
                }

                


                try
                {
                    if (sqlPerson != null)
                    {
                     
                        ItemDetectPerson itemDetect = new ItemDetectPerson();
                        itemDetect.codeSystem = codeSystem;
                        itemDetect.name = sqlPerson.name;
                        itemDetect.gender = sqlPerson.gender;
                        itemDetect.age = sqlPerson.age;
                        itemDetect.image = codefile;
                        if(sqlPerson.group != null && sqlPerson.group.code.CompareTo("BL") == 0)
                        {
                            itemDetect.alert = true;
                            itemDetect.group = sqlPerson.group.name;
                        }
                        else
                        {
                            itemDetect.alert = false;
                            itemDetect.group = sqlPerson.group?.name;
                        }
                        itemDetect.device = sqlDevice!.name;
                        itemDetect.time = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

                        List<HttpNotification>? notifications = Program.httpNotifications.ToList();
                        foreach (HttpNotification notification in notifications)
                        {
                            Log.Debug("Person Arrived : {0}", itemDetect.codeSystem);
                            notification.messagers.Add(JsonConvert.SerializeObject(itemDetect));
                        }
                    }

                }
                catch(Exception ex)
                {
                    Log.Error(String.Format("Notification of Person : {0}**************************{1}", codeSystem, ex.ToString()));
                }
                return true;
            }
        }

        public async Task<bool> setConvertFace(string s1, string s2)
        {
            if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2))
            {
                return false;
            }
            using (DataContext context = new DataContext())
            {
                try
                {
                    SqlPerson? person_S1 = context.persons!.Where(s => s.codeSystem.CompareTo(s1) == 0).Include(s => s.faces).Include(s => s.group).FirstOrDefault();
                    if (person_S1 == null)
                    {
                        return false;
                    }

                    SqlPerson? person_S2 = context.persons!.Where(s => s.codeSystem.CompareTo(s2) == 0).Include(s => s.faces).FirstOrDefault();
                    if (person_S2 == null)
                    {
                        return false;
                    }


                    if (DateTime.Compare(person_S1.faces![0].createdTime, person_S2.faces![0].createdTime) < 0)
                    {

                        foreach (SqlFace item in person_S2.faces)
                        {
                            person_S1.faces.Add(item);
                        }
                        person_S2.faces = new List<SqlFace>();
                        person_S2.faces = person_S1.faces;
                    }
                    else
                    {

                        foreach (SqlFace item in person_S1.faces)
                        {
                            person_S2.faces.Add(item);
                        }
                    }


                    List<SqlLogPerson> logs = context.logs!.Where(s => s.person!.ID == person_S1.ID).ToList();
                    if (logs.Count > 0)
                    {
                        foreach (SqlLogPerson log in logs)
                        {
                            if (log.note.CompareTo(String.Format("Person arrived : {0}", person_S1.code)) == 0)
                            {
                                log.note = String.Format("Person arrived : {0} :", person_S2.code);
                            }
                            log.person = person_S2;
                        }
                    }

                    if (person_S1.group != null)
                    {
                        if (person_S2.group == null)
                        {
                            bool flag = await Program.api_group.SetPersonAsync(person_S2.code, person_S1.group.code);
                            if (flag)
                            {
                                Console.WriteLine("Done !!!");
                            }
                            else
                            {
                                Console.Write("Clean Group Fail");
                                return false;
                            }
                        }
                       
                    }
                    person_S1.isdeleted = true;                    

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
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                    return false;
                }
            }
        }


        public class ItemLog
        {
            public string person { get; set; } = "";
            public string device { get; set; } = "";
            public string image { get; set; } = "";
            public string note { get; set; } = "";
            public string time { get; set; } = "";
        }

        public List<ItemLog> getListHistoryForPerson(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return new List<ItemLog>();
            }

            List<ItemLog> list = new List<ItemLog>();

            using (DataContext context = new DataContext())
            {
                SqlPerson? m_person = context.persons!.Where(s => s.code.CompareTo(code) == 0).FirstOrDefault();
                if (m_person == null)
                {
                    return new List<ItemLog>();
                }

                List<SqlLogPerson>? logs = context.logs!.Include(s => s.person).Where(s => s.person!.ID == m_person.ID).Include(s => s.person).Include(s => s.device).OrderByDescending(s => s.time).ToList();
                if (logs.Count > 0)
                {
                    foreach (SqlLogPerson log in logs)
                    {
                        ItemLog tmp = new ItemLog();
                        tmp.person = log.person!.code;
                        tmp.device = log.device!.code;
                        tmp.image = log.image;
                        tmp.note = log.note;
                        tmp.time = log.time.ToLocalTime().ToString("dd-MM-yyyy HH:mm:ss");

                        list.Add(tmp);
                    }
                }
                return list;
            }
        }

        //public List<ItemDetectPerson> getListPersonArrived(string token)
        //{
        //    using(DataContext context = new DataContext())
        //    {
        //        SqlUser? m_user = context.users!.Where(s => s.isdeleted == false && s.token.CompareTo(token) == 0).AsNoTracking().FirstOrDefault();
        //        if(m_user == null)
        //        {
        //            return new List<ItemDetectPerson>();
        //        }

        //        List<ItemDetectPerson> items = new List<ItemDetectPerson>();
        //        List<string> groups = new List<string>();
        //        groups.Add("0");
        //        groups.Add("1");
        //        groups.Add("2");
        //        foreach (string m_group in groups)
        //        {
        //            if (Program.httpNotifications.Count > 0)
        //            {
        //                HttpNotification? notification = Program.httpNotifications.Where(s => s.group.CompareTo(m_group) == 0).FirstOrDefault();
        //                if (notification == null)
        //                {
        //                    continue;
        //                }
        //                if (notification.infos.Count > 0)
        //                {
        //                    foreach (ItemDetectPerson m_item in notification.infos)
        //                    {
        //                        items.Add(m_item);
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                Console.WriteLine("No connection !!!");
        //            }
        //        }
        //        if(items.Count == 0)
        //        {
        //            Console.WriteLine("Person not yet arrived !!!");
        //        }
                
        //        return items;
        //    }    
        //}
    }
}
