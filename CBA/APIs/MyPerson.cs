using CBA.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Diagnostics;
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
        public class ItemDateData
        {
            public string date { get; set; } = "";
            public string data { get; set; } = "";
        }

        public class ListInfoLogsPage
        {
            public int total { get; set; } = 0;
            public int page { get; set; } = 0;
            public string data { get; set; } = "";

        }


        public ListInfoLogsPage getListPersonHistory(DateTime begin, DateTime end, int index, int total)
        {
            using (DataContext context = new DataContext())
            {
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();

                DateTime dateBegin = new DateTime(begin.Year, begin.Month, begin.Day, 00, 00, 00);
                DateTime dateEnd = new DateTime(end.Year, end.Month, end.Day, 00, 00, 00);
                dateEnd = dateEnd.AddDays(1);
                List<string> times = new List<string>();
                ListInfoLogsPage info = new ListInfoLogsPage();

                List<SqlLogPerson> logs = context.logs!.Where(s => DateTime.Compare(dateBegin.ToUniversalTime(), s.time) <= 0 && DateTime.Compare(dateEnd.ToUniversalTime(), s.time) > 0)
                                                      .Include(s => s.person!).ThenInclude(s => s.level)
                                                      .Include(s => s.person!).ThenInclude(s => s.group)
                                                      .Include(s => s.person!).ThenInclude(s => s.faces)
                                                      .Include(s => s.device)
                                                      .OrderByDescending(s => s.time)
                                                      .ToList();
                

                if (logs.Count < 1)
                {
                    return new ListInfoLogsPage();
                }

                
                do
                {
                    times.Add(dateBegin.ToString("dd-MM-yyyy"));
                    dateBegin = dateBegin.AddDays(1);
                }while(DateTime.Compare(dateBegin, dateEnd) < 0);
                int count = 0;
                foreach (string m_time in times)
                {
                    List<ItemPerson> persons = new List<ItemPerson>();

                    ItemDateData m_data = new ItemDateData();
                    m_data.date = m_time;

                    DateTime time_input = DateTime.MinValue;
                    try
                    {
                        time_input = DateTime.ParseExact(m_time, "dd-MM-yyyy", null);
                    }
                    catch (Exception e)
                    {
                        time_input = DateTime.MinValue;
                    }
                    DateTime start = time_input;
                    DateTime stop = time_input.AddDays(1);
                    List<SqlLogPerson> mLogs = logs.Where(s => DateTime.Compare(start.ToUniversalTime(), s.time) <= 0 && DateTime.Compare(stop.ToUniversalTime(), s.time) > 0).ToList();
                    foreach (SqlLogPerson m_log in mLogs)
                    {
                        if (m_log.person != null)
                        {
                            ItemPerson? tmpPerson = persons.Where(s => s.code.CompareTo(m_log.person.code) == 0).FirstOrDefault();
                            if (tmpPerson == null)
                            {
                                ItemPerson item = new ItemPerson();
                                item.code = m_log.person.code;
                                item.codeSystem = m_log.person.codeSystem;
                                item.name = m_log.person.name;
                                item.gender = m_log.person.gender;
                                item.age = m_log.person.age;
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
                                else
                                {
                                    string timeStart = "24-03-2023 11:00:00";
                                    DateTime time = DateTime.ParseExact(timeStart, "dd-MM-yyyy HH:mm:ss", null);
                                    if (DateTime.Compare(m_log.person.createdTime.ToLocalTime(), time) < 0)
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

                                item.createdTime = m_log.person.createdTime.ToLocalTime().ToString("dd-MM-yyyy HH:mm:ss");
                                item.lastestTime = m_log.person.lastestTime.ToLocalTime().ToString("dd-MM-yyyy HH:mm:ss");
                                persons.Add(item);
                            }
                            else
                            {
                                ItemFaceForPerson itemFace = new ItemFaceForPerson();
                                itemFace.image = m_log.image;
                                itemFace.time = m_log.time.ToLocalTime().ToString("dd-MM-yyyy HH:mm:ss");

                                if (m_log.device != null)
                                {
                                    itemFace.device = m_log.device.code;
                                }

                                tmpPerson.faces.Add(itemFace);
                            }
                        }
                    }
                    count += persons.Count();
                    info.data += JsonConvert.SerializeObject(persons);
                }
                List<ItemPerson>? m_totalData = JsonConvert.DeserializeObject<List<ItemPerson>>(info.data);

                /*stopWatch.Stop();
                TimeSpan ts = stopWatch.Elapsed;
                string elapsedTime = string.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
                Console.WriteLine(string.Format("getlogs : {0}", elapsedTime));*/

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
