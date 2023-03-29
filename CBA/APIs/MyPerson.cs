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
                Stopwatch sw = new Stopwatch();
                sw.Start();
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
                sw.Stop();
                Log.Information(string.Format("getReport : {0} ms", sw.ElapsedMilliseconds));
                return list;

            }
        }
        public class ItemBufferPerSon
        {
            public string codeSystem { get; set; } = "";
            public string code { get; set; } = "";
            public string name { get; set; } = "";
            public string gender { get; set; } = "";
            public int age { get; set; } = 0;
            public ItemGroupForPerson group { get; set; } = new ItemGroupForPerson();
            public ItemAgeLevelForPerson level { get; set; } = new ItemAgeLevelForPerson();
            public DateTime createdTime { get; set; }
        }
        public class ItemBufferImage
        {
            public string image { get; set; } = "";
            public string time { get; set; } = "";
            public string device { get; set; } = "";
        }
        public class ItemBuffer
        {
            public long idPerson { get; set; } = 0;
            public string date { get; set; } = "";
            public ItemBufferPerSon person { get; set; } = new ItemBufferPerSon();
            public DateTime lastestTime { get; set; }
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
            public List<ItemLogPersons> items { get; set; } = new List<ItemLogPersons>();

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


                List<SqlLogPerson> totallogs = context.logs!.Include(s => s.person!).ThenInclude(s => s.group)
                                                        .Include(s => s.person!).ThenInclude(s => s.faces)
                                                        .Include(s => s.person!).ThenInclude(s => s.level)
                                                        .Include(s => s.device)
                                                        .ToList();

                List<SqlLogPerson> logs = totallogs.Where(s => DateTime.Compare(dateBegin.ToUniversalTime(), s.time) <= 0 && DateTime.Compare(dateEnd.ToUniversalTime(), s.time) > 0)
                                                       .OrderByDescending(s => s.time)
                                                       .ToList();
                sw.Stop();
                Console.WriteLine(string.Format("getReport : step1: {0} ms", sw.ElapsedMilliseconds));

                sw.Start();
                List<ItemBuffer> m_buffer = new List<ItemBuffer>();
                foreach (SqlLogPerson m_log in logs)
                {
                    ItemBuffer itemBuffer = new ItemBuffer();

                    itemBuffer.idPerson = m_log.person!.ID;
                    itemBuffer.date = m_log.time.ToLocalTime().ToString("dd-MM-yyyy");
                    if (m_log.person != null)
                    {
                        itemBuffer.person.codeSystem = m_log.person.codeSystem;
                        itemBuffer.person.code = m_log.person.code;
                        itemBuffer.person.name = m_log.person.name;
                        itemBuffer.person.gender = m_log.person.gender;
                        itemBuffer.person.age = m_log.person.age;



                        if (m_log.person.group != null)
                        {
                            ItemGroupForPerson itemGroup = new ItemGroupForPerson();
                            itemGroup.name = m_log.person.group.name;
                            itemGroup.code = m_log.person.group.code;
                            itemGroup.des = m_log.person.group.des;

                            itemBuffer.person.group = itemGroup;
                        }

                        if (m_log.person.level != null)
                        {
                            ItemAgeLevelForPerson itemLevel = new ItemAgeLevelForPerson();

                            itemLevel.code = m_log.person.level.code;
                            itemLevel.name = m_log.person.level.name;

                            itemBuffer.person.level = itemLevel;
                        }

                        itemBuffer.person.createdTime = m_log.person.createdTime;

                    }
                    itemBuffer.lastestTime = m_log.time;

                    m_buffer.Add(itemBuffer);
                }

                sw.Stop();
                Console.WriteLine(string.Format("getReport : step2: {0} ms", sw.ElapsedMilliseconds));

                sw.Restart();

                m_buffer = m_buffer.OrderBy(s => s.idPerson).ThenByDescending(s => s.lastestTime).ToList();
                DateTime start = DateTime.Now;


                while (m_buffer.Count > 0)
                {
                    TimeSpan time = DateTime.Now.Subtract(start);

                    for (int i = 0; i < m_buffer.Count; i++)
                    {
                        //Console.WriteLine(string.Format("getReport : code : {0}  --- lastestTime : {1}", m_buffer[i].code, m_buffer[i].lastestTime));
                        //sw.Start();

                        ItemLogPersons? m_item = items.Where(s => s.code.CompareTo(m_buffer[i].person.code) == 0 && s.date.CompareTo(m_buffer[i].date) == 0).FirstOrDefault();
                        if (m_item == null)
                        {
                            ItemLogPersons item = new ItemLogPersons();
                            item.code = m_buffer[i].person!.code;
                            item.codeSystem = m_buffer[i].person!.codeSystem;
                            item.name = m_buffer[i].person!.name;
                            item.gender = m_buffer[i].person!.gender;
                            item.age = m_buffer[i].person!.age;
                            item.date = m_buffer[i].date;


                            if (m_buffer[i].person!.group != null)
                            {
                                item.group.code = m_buffer[i].person!.group.code;
                                item.group.name = m_buffer[i].person!.group.name;
                                item.group.des = m_buffer[i].person!.group.des;
                            }

                            List<SqlLogPerson>? m_image = totallogs.Where(s => s.person!.ID == m_buffer[i].idPerson).OrderByDescending(s => s.time).ToList();
                            SqlPerson? tmp = m_image[m_image.Count - 1].person;
                            if (tmp != null)
                            {
                                if (tmp.faces != null)
                                {
                                    item.image = tmp.faces[tmp.faces.Count - 1].image;

                                    foreach (SqlFace tmpFace in tmp.faces)
                                    {

                                        DateTime timeFace = new DateTime(m_buffer[i].lastestTime.Year, m_buffer[i].lastestTime.Month, m_buffer[i].lastestTime.Day, 00, 00, 00);
                                        timeFace = timeFace.AddDays(1);

                                        if (DateTime.Compare(tmpFace.createdTime, timeFace) < 0)
                                        {
                                            ItemFaceForPerson itemFace = new ItemFaceForPerson();

                                            itemFace.image = tmpFace.image;
                                            itemFace.time = tmpFace.createdTime.ToLocalTime().ToString("dd-MM-yyyy HH:mm:ss");

                                            if (tmpFace.device != null)
                                            {
                                                itemFace.device = tmpFace.device.code;
                                            }

                                            item.faces.Add(itemFace);
                                        }
                                    }
                                }
                            }

                            //sw.Stop();
                            //Console.WriteLine(string.Format("getReport : step3: {0} ms", sw.ElapsedMilliseconds));


                            if (m_buffer[i].person!.level != null)
                            {
                                item.level.code = m_buffer[i].person!.level.code;
                                item.level.name = m_buffer[i].person!.level.name;
                            }
                            else
                            {
                                string timeStart = "24-03-2023 11:00:00";
                                DateTime checkTime = DateTime.ParseExact(timeStart, "dd-MM-yyyy HH:mm:ss", null);
                                if (DateTime.Compare(m_buffer[i].person!.createdTime, checkTime.ToUniversalTime()) < 0)
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

                            item.createdTime = m_buffer[i].person!.createdTime.ToLocalTime().ToString("dd-MM-yyyy HH:mm:ss");
                            item.lastestTime = m_buffer[i].lastestTime.ToLocalTime().ToString("dd-MM-yyyy HH:mm:ss");

                            items.Add(item);

                            //sw.Stop();
                            //Log.Information(string.Format("getReport : step3 : {0} ms", sw.ElapsedMilliseconds));

                        }
                        if (time.Milliseconds > 100)
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
                    info.items.Add(m_person);

                }
                /* sw.Stop();
                 Log.Information(string.Format("getReport : step4 : {0} ms", sw.ElapsedMilliseconds));*/
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
