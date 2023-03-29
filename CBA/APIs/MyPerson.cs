using CBA.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Diagnostics;
using System.Xml.Linq;
using static CBA.APIs.MyFace;

namespace CBA.APIs
{
    public class MyPerson
    {

        public async Task<bool> editPerson(string code, string name, string des, string codeSystem)
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
                    if (m_person.code.CompareTo(code) != 0)
                    {
                        m_person.code = code;
                    }
                }
                if (!string.IsNullOrEmpty(name))
                {
                    m_person.name = name;
                }
                m_person.des = des;

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

        public ListPersonPage getListPerson(int index, int count)
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
                if (persons.Count > 0)
                {
                    if (index > persons.Count)
                    {
                        return new ListPersonPage();
                    }
                    list.total = persons.Count();
                    list.page = index;
                    for (int i = 0; i < count; i++)
                    {
                        int number = index + i;
                        if (number > persons.Count)
                        {
                            break;
                        }

                        ItemPerson item = new ItemPerson();
                        item.code = persons[number - 1].code;
                        item.codeSystem = persons[number - 1].codeSystem;
                        item.name = persons[number - 1].name;
                        item.gender = persons[number - 1].gender;
                        item.age = persons[number - 1].age;
                        if (persons[number - 1].group != null)
                        {
                            item.group.code = persons[number - 1].group!.code;
                            item.group.name = persons[number - 1].group!.name;
                            item.group.des = persons[number - 1].group!.des;
                        }

                        if (persons[number - 1].faces != null)
                        {
                            List<SqlFace>? tmpFace = persons[number - 1].faces!.OrderByDescending(s => s.createdTime).ToList();
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

                        if (persons[number - 1].level != null)
                        {
                            item.level.code = persons[number - 1].level!.code;
                            item.level.name = persons[number - 1].level!.name;
                        }
                        else
                        {
                            string timeStart = "24-03-2023 11:00:00";
                            DateTime time = DateTime.ParseExact(timeStart, "dd-MM-yyyy HH:mm:ss", null);
                            if (DateTime.Compare(persons[number - 1].createdTime.ToLocalTime(), time) < 0)
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

                        item.createdTime = persons[number - 1].createdTime.ToLocalTime().ToString("dd-MM-yyyy HH:mm:ss");
                        item.lastestTime = persons[number - 1].lastestTime.ToLocalTime().ToString("dd-MM-yyyy HH:mm:ss");
                        list.persons.Add(item);

                    }
                }
                return list;

            }
        }
        public class ItemLogPersons
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
            public string date { get; set; } = "";
            public string createdTime { get; set; } = "";
            public string lastestTime { get; set; } = "";
        }

        public class ListInfoLogsPage
        {
            public int total { get; set; } = 0;
            public int page { get; set; } = 0;
            public List<ItemLogPersons> data { get; set; } = new List<ItemLogPersons>();

        }


        public ListInfoLogsPage getListPersonHistory(DateTime begin, DateTime end, int index, int total)
        {
            using (DataContext context = new DataContext())
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();

                DateTime dateBegin = new DateTime(begin.Year, begin.Month, begin.Day, 00, 00, 00);
                DateTime dateEnd = new DateTime(end.Year, end.Month, end.Day, 00, 00, 00);
                dateEnd = dateEnd.AddDays(1);

                ListInfoLogsPage info = new ListInfoLogsPage();
                info.page = index;
                List<ItemLogPersons> items = new List<ItemLogPersons>();


                List<SqlLogPerson> logs = context.logs!.Where(s => DateTime.Compare(dateBegin.ToUniversalTime(), s.time) <= 0 && DateTime.Compare(dateEnd.ToUniversalTime(), s.time) > 0)
                                                       .Include(s => s.person)
                                                       .Include(s => s.device)
                                                       .OrderByDescending(s => s.time)
                                                       .ToList();

                sw.Stop();
                Console.WriteLine(string.Format("getReport : step1 : {0} ms", sw.ElapsedMilliseconds));

                sw.Restart();
                List<ItemLogPersons> m_buffer = new List<ItemLogPersons>();
                foreach (SqlLogPerson m_log in logs)
                {
                    if (m_log.person != null)
                    {
                        ItemLogPersons item = new ItemLogPersons();
                        item.code = m_log.person.code;
                        item.codeSystem = m_log.person.codeSystem;
                        item.name = m_log.person.name;
                        item.gender = m_log.person.gender;
                        item.age = m_log.person.age;
                        item.date = m_log.time.ToLocalTime().ToString("dd-MM-yyyy");

                        if (m_log.person.group != null)
                        {
                            item.group.code = m_log.person.group.code;
                            item.group.name = m_log.person.group.name;
                            item.group.des = m_log.person.group.des;
                        }
                        if (m_log.person.faces != null)
                        {
                            SqlPerson? m_person = context.persons!.Where(s => s.isdeleted == false && s.code.CompareTo(m_log.person.code) == 0).FirstOrDefault();
                            if (m_person != null)
                            {
                                if (m_person.faces != null)
                                {
                                    List<SqlFace> list = m_person.faces!.OrderByDescending(s => s.createdTime).ToList();
                                    item.image = list[list.Count - 1].image;
                                }
                            }
                            // item.image = m_log.person.faces[m_log.person.faces.Count - 1].image;
                        }
                        ItemFaceForPerson itemFace = new ItemFaceForPerson();
                        itemFace.image = m_log.image;
                        itemFace.time = m_log.time.ToLocalTime().ToString("dd-MM-yyyy HH:mm:ss");

                        if (m_log.device != null)
                        {
                            itemFace.device = m_log.device.code;
                        }

                        item.faces.Add(itemFace);

                        if (m_log.person.level != null)
                        {
                            item.level.code = m_log.person.level!.code;
                            item.level.name = m_log.person.level!.name;
                        }


                        item.createdTime = m_log.person.createdTime.ToLocalTime().ToString("dd-MM-yyyy HH:mm:ss");
                        item.lastestTime = m_log.time.ToLocalTime().ToString("dd-MM-yyyy HH:mm:ss");

                        m_buffer.Add(item);
                    }
                }
                sw.Stop();
                Console.WriteLine(string.Format("getReport : step2 : {0} ms", sw.ElapsedMilliseconds));

                sw.Restart();

                m_buffer = m_buffer.OrderBy(s => s.code).ThenByDescending(s => s.lastestTime).ToList();
                DateTime start = DateTime.Now;

                while (m_buffer.Count > 0)
                {
                    TimeSpan time = DateTime.Now.Subtract(start);

                    for (int i = 0; i < m_buffer.Count; i++)
                    {
                        //Console.WriteLine(string.Format("getReport : code : {0}  --- lastestTime : {1}", m_buffer[i].code, m_buffer[i].lastestTime));

                        ItemLogPersons? m_item = items.Where(s => s.code.CompareTo(m_buffer[i].code) == 0 && s.date.CompareTo(m_buffer[i].date) == 0).FirstOrDefault();
                        if (m_item == null)
                        {
                            ItemLogPersons m_data = new ItemLogPersons();
                            m_data = m_buffer[i];
                            items.Add(m_data);

                        }
                        else
                        {
                            if (time.TotalSeconds > 5.0)
                            {
                                m_buffer.RemoveAt(i);
                                i--;

                                if (m_buffer.Count < 1)
                                {
                                    break;
                                }
                            }
                            
                        }
                    }
                }


                sw.Stop();
                Log.Information(string.Format("getReport : step3 : {0} ms", sw.ElapsedMilliseconds));

                sw.Restart();

                info.total = items.Count;
                for (int i = 0; i < total; i++)
                {
                    int number = index + i;
                    if (number > items.Count)
                    {
                        break;
                    }

                    ItemLogPersons m_person = new ItemLogPersons();
                    m_person = items[number - 1];
                    info.data.Add(m_person);

                }
                sw.Stop();
                Log.Information(string.Format("getReport : step4 : {0} ms", sw.ElapsedMilliseconds));
                return info;
            }
        }

        
        public async Task<bool> updateLastestTime(DateTime time)
        {
            using (DataContext context = new DataContext())
            {
                List<SqlPerson> persons = context.persons!.Where(s => DateTime.Compare(time.ToUniversalTime(), s.createdTime) > 0 && s.isdeleted == false).Include(s => s.faces).ToList();
                if (persons.Count > 0)
                {
                    foreach (SqlPerson m_person in persons)
                    {
                        if (m_person.faces != null)
                        {
                            List<SqlFace> tmp = m_person.faces.Where(s => s.isdeleted == false).OrderByDescending(s => s.createdTime).ToList();
                            if (tmp.Count > 1)
                            {
                                if (DateTime.Compare(m_person.lastestTime, tmp[0].createdTime) == 0)
                                {
                                    Console.WriteLine("Updated Time");
                                    Console.WriteLine(String.Format(" Updated for code : {0} *** LastestTime : {1}", m_person.code, m_person.lastestTime));

                                }
                                else
                                {
                                    m_person.lastestTime = tmp[0].createdTime;
                                    await context.SaveChangesAsync();

                                    Console.WriteLine(String.Format(" Update for code : {0} --- Time : {1} ", m_person.code, m_person.lastestTime));

                                }
                                //Console.WriteLine(String.Format(" Update for code : {0} *** OriTime : {1}", m_person.code, m_person.lastestTime));


                            }
                        }
                    }
                }
                return true;
            }
        }
    }
}
