using CBA.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Serilog;
using System;
using static CBA.APIs.MyGroup;

namespace CBA.APIs
{
    public class MyReport
    {
        public class ItemPersonForPlot
        {
            public string code { get; set; } = "";
            public string name { get; set; } = "";
        }

        public class ItemFaceOfPerson
        {
            public ItemPersonForPlot person { get; set; } = new ItemPersonForPlot();

            public string group { get; set; } = "";

            public DateTime time { get; set; }
        }

        public class ItemCountHours
        {
            public string hour { get; set; } = "";
            public List<int> number { get; set; } = new List<int>();
        }

        public class ItemCountsPlot
        {
            public List<string> groups { get; set; } = new List<string>();
            public string date { get; set; } = "";
            public List<ItemCountHours> data { get; set; } = new List<ItemCountHours>();
            public int maxCount { get; set; } = 0;
            public List<int> totalCount { get; set; } = new List<int>();
        }

        public ItemCountsPlot getCountHour(DateTime time)
        {

            using (DataContext context = new DataContext())
            {
                DateTime start = new DateTime(time.Year, time.Month, time.Day, 00, 00, 00);
                DateTime end = start.AddDays(1);
                DateTime timeEnd = end.ToUniversalTime();

                ItemCountsPlot tmp = new ItemCountsPlot();
                tmp.date = time.ToString("dd-MM-yyyy");


                List<ItemFaceOfPerson> face_persons = new List<ItemFaceOfPerson>();

                List<SqlFace>? faces = context.faces!.Where(s => DateTime.Compare(start.ToUniversalTime(), s.createdTime) <= 0 && DateTime.Compare(end.ToUniversalTime(), s.createdTime) > 0).Include(s => s.person!).ThenInclude(s => s.group).ToList();
                if (faces.Count > 0)
                {

                    foreach (SqlFace m_face in faces)
                    {
                        ItemFaceOfPerson item = new ItemFaceOfPerson();
                        if (m_face.person != null)
                        {

                            ItemPersonForPlot person = new ItemPersonForPlot();
                            person.code = m_face.person.code;
                            person.name = m_face.person.name;

                            item.person = person;

                            if (m_face.person.group != null)
                            {
                                string? tmpGroup = tmp.groups!.Where(s => s.CompareTo(m_face.person.group.name) == 0).FirstOrDefault();
                                if (string.IsNullOrEmpty(tmpGroup))
                                {
                                    tmp.groups.Add(m_face.person.group.name);
                                }



                                item.group = m_face.person.group.name;
                                item.time = m_face.createdTime;
                                face_persons.Add(item);
                            }
                            else
                            {
                                if (tmp.groups.Count < 1)
                                {
                                    tmp.groups.Add("");
                                }
                                item.group = "";
                                item.time = m_face.createdTime;
                                face_persons.Add(item);
                            }

                        }


                    }
                }
                try
                {


                    do
                    {
                        for (int i = 0; i < 24; i++)
                        {
                            //Console.WriteLine(i);
                            ItemCountHours item = new ItemCountHours();
                            item.hour = i.ToString();
                            DateTime hourBegin = start.AddHours(i).ToUniversalTime();
                            DateTime hourEnd = hourBegin.AddHours(1);
                            time = hourEnd;
                            //Console.WriteLine(string.Format("Time start : {0} ---- Time End : {1}", hourBegin.ToLocalTime(), hourEnd.ToLocalTime()));

                            List<ItemFaceOfPerson> m_list = new List<ItemFaceOfPerson>();
                            foreach (string group in tmp.groups)
                            {
                                int index = 0;
                                foreach (ItemFaceOfPerson m_log in face_persons)
                                {
                                    if (m_log.group.CompareTo(group) == 0 && DateTime.Compare(hourBegin, m_log.time) <= 0 && DateTime.Compare(hourEnd, m_log.time) > 0)
                                    {
                                        index++;
                                        //Console.WriteLine(m_log.person.code);
                                        //Console.WriteLine(string.Format(" time : {0} - person : {1} - Count : {2} - Group : {3}", m_log.time.ToLocalTime(), m_log.person.code, index, group));
                                        //Thread.Sleep(1);

                                    }
                                }
                                item.number.Add(index);
                            }
                            tmp.data.Add(item);
                        }
                        for (int i = 0; i < tmp.groups.Count; i++)
                        {
                            int totalcount = 0;
                            foreach (ItemCountHours tmp_count in tmp.data)
                            {
                                totalcount += tmp_count.number[i];
                            }
                            tmp.totalCount.Add(totalcount);
                        }

                    }
                    while (DateTime.Compare(time, timeEnd) < 0);
                    return tmp;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return new ItemCountsPlot();
                }

            }
        }

        public List<ItemCountsPlot> getCountDate(DateTime begin, DateTime end)
        {
            List<ItemCountsPlot> itemCounts = new List<ItemCountsPlot>();

            using (DataContext context = new DataContext())
            {
                DateTime timeEnd = end.AddDays(1);
                List<SqlFace>? logs = context.faces!.Where(s => DateTime.Compare(begin.ToUniversalTime(), s.createdTime) <= 0 && DateTime.Compare(timeEnd.ToUniversalTime(), s.createdTime) >= 0).ToList();
                //.Where(s => DateTime.Compare(time_begin.ToUniversalTime(), s.time) <= 0 && DateTime.Compare(time_end.ToUniversalTime(), s.time) >= 0)
                if (logs.Count > 0)
                {
                    List<string> dates = new List<string>();
                    foreach (SqlFace log in logs)
                    {
                        DateTime time = log.createdTime.ToLocalTime();

                        string? tmp = dates.Where(s => s.CompareTo(time.Date.ToString("dd-MM-yyyy")) == 0).FirstOrDefault();
                        if (tmp == null)
                        {
                            //Console.WriteLine(time.Date.ToString("dd-MM-yyyy"));
                            dates.Add(time.Date.ToString("dd-MM-yyyy"));
                            
                        }

                       
                    }
                    foreach (string temp in dates)
                    {
                        DateTime time_input = DateTime.MinValue;
                        try
                        {
                            time_input = DateTime.ParseExact(temp, "dd-MM-yyyy", null);
                        }
                        catch (Exception e)
                        {
                            time_input = DateTime.MinValue;
                        }
                       // Console.WriteLine(string.Format("Time test : {0}", time_input));
                        ItemCountsPlot item = getCountHour(time_input);
                        itemCounts.Add(item);
                    }
                }
                return itemCounts;
            }
        }

        public class ItemPersonHours
        {
            public string hour { get; set; } = "";
            public List<int> number { get; set; } = new List<int>();
        }

        public class ItemPersonsPlot
        {
            public List<string> groups { get; set; } = new List<string>();
            public string date { get; set; } = "";
            public List<ItemPersonHours> data { get; set; } = new List<ItemPersonHours>();
            public List<int> totalCount { get; set; } = new List<int>();
        }

        public ItemPersonsPlot getPersonsHours(DateTime time)
        {
            using(DataContext context = new DataContext())
            {
                DateTime start = new DateTime(time.Year, time.Month, time.Day, 00, 00, 00);
                DateTime end = start.AddDays(1);
                DateTime timeEnd = end.ToUniversalTime();

                ItemPersonsPlot tmp = new ItemPersonsPlot();
                tmp.date = time.ToString("dd-MM-yyyy");


                List<ItemFaceOfPerson> face_persons = new List<ItemFaceOfPerson>();

                List<SqlFace>? faces = context.faces!.Where(s => DateTime.Compare(start.ToUniversalTime(), s.createdTime) <= 0 && DateTime.Compare(end.ToUniversalTime(), s.createdTime) > 0).Include(s => s.person!).ThenInclude(s => s.group).ToList();
                if (faces.Count > 0)
                {

                    foreach (SqlFace m_face in faces)
                    {
                        ItemFaceOfPerson item = new ItemFaceOfPerson();
                        if (m_face.person != null)
                        {

                            ItemPersonForPlot person = new ItemPersonForPlot();
                            person.code = m_face.person.code;
                            person.name = m_face.person.name;

                            item.person = person;

                            if (m_face.person.group != null)
                            {
                                string? tmpGroup = tmp.groups!.Where(s => s.CompareTo(m_face.person.group.name) == 0).FirstOrDefault();
                                if (string.IsNullOrEmpty(tmpGroup))
                                {
                                    tmp.groups.Add(m_face.person.group.name);
                                }

                                item.group = m_face.person.code;
                                item.time = m_face.createdTime;
                                face_persons.Add(item);
                            }
                            else
                            {
                                if (tmp.groups.Count < 1)
                                {
                                    tmp.groups.Add("");
                                }
                                item.group = "";
                                item.time = m_face.createdTime;
                                face_persons.Add(item);
                            }
                            
                        }

                        
                    }
                }
                List<ItemPersonForPlot> list_persons = new List<ItemPersonForPlot>();

                List<SqlPerson>? persons = context.persons!.Where(s => s.isdeleted == false).Include(s => s.group).AsNoTracking().ToList();
                if(persons.Count > 0)
                {
                    foreach(SqlPerson m_person in persons)
                    {
                        ItemPersonForPlot item = new ItemPersonForPlot();
                        item.code = m_person.code;
                        item.name = m_person.name;
                        list_persons.Add(item);
                    }    
                }    

                
                try
                {
                    

                    do
                    {
                        for (int i = 0; i < 24; i++)
                        {
                            //Console.WriteLine(i);
                            ItemPersonHours item = new ItemPersonHours();
                            item.hour = i.ToString();
                            DateTime hourBegin = start.AddHours(i).ToUniversalTime();
                            DateTime hourEnd = hourBegin.AddHours(1);
                            time = hourEnd;
                            //Console.WriteLine(string.Format("Time start : {0} ---- Time End : {1}", hourBegin, hourEnd));

                            List<ItemFaceOfPerson> m_list = new List<ItemFaceOfPerson>();
                            foreach (string group in tmp.groups)
                            {
                                
                                foreach (ItemFaceOfPerson m_log in face_persons)
                                {
                                    if (m_log.group.CompareTo(group) == 0 && DateTime.Compare(hourBegin, m_log.time) <= 0 && DateTime.Compare(hourEnd, m_log.time) > 0)
                                    {
                                        m_list.Add(m_log);
                                    }
                                }

                                int index = 0;
                                if (m_list.Count > 0)
                                {                                   
                                    foreach (ItemPersonForPlot m_person in list_persons)
                                    {
                                        List<ItemFaceOfPerson>? group_persons = m_list.Where(s => s.person.code.CompareTo(m_person.code) == 0).ToList();
                                        if (group_persons.Count > 1)
                                        {
                                            index++;
                                            ////Console.WriteLine(group_persons[0].person.code);
                                            //Console.WriteLine(string.Format(" time : {0} - person : {1} - Count : {2} - Group : {3}", group_persons[group_persons.Count - 1].time, group_persons[0].person.code, index, group));
                                            //Thread.Sleep(1);
                                        }
                                    }
                                }
                                item.number.Add(index);
                            }
                            tmp.data.Add(item);
                        }
                        for (int i = 0; i < tmp.groups.Count; i++)
                        {
                            int totalcount = 0;
                            foreach (ItemPersonHours tmp_count in tmp.data)
                            {
                                totalcount += tmp_count.number[i];
                            }
                            tmp.totalCount.Add(totalcount);
                        }

                    }
                    while (DateTime.Compare(time, timeEnd) < 0);
                    return tmp;
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex);
                    return new ItemPersonsPlot();
                }
            }
        }

        public List<ItemPersonsPlot> getCountPersonForDate(DateTime begin, DateTime end)
        {
            List<ItemPersonsPlot> items = new List<ItemPersonsPlot>();

            using (DataContext context = new DataContext())
            {
                DateTime timeEnd = end.AddDays(1);
                List<SqlFace>? logs = context.faces!.Where(s => DateTime.Compare(begin.ToUniversalTime(), s.createdTime) <= 0 && DateTime.Compare(timeEnd.ToUniversalTime(), s.createdTime) >= 0).ToList();
                //.Where(s => DateTime.Compare(time_begin.ToUniversalTime(), s.time) <= 0 && DateTime.Compare(time_end.ToUniversalTime(), s.time) >= 0)
                if (logs.Count > 0)
                {
                    List<string> dates = new List<string>();
                    foreach (SqlFace log in logs)
                    {
                        DateTime time = log.createdTime.ToLocalTime();

                        string? tmp = dates.Where(s => s.CompareTo(time.Date.ToString("dd-MM-yyyy")) == 0).FirstOrDefault();
                        if (tmp == null)
                        {
                            //Console.WriteLine(time.Date.ToString("dd-MM-yyyy"));
                            dates.Add(time.Date.ToString("dd-MM-yyyy"));

                        }


                    }
                    foreach (string temp in dates)
                    {
                        DateTime time_input = DateTime.MinValue;
                        try
                        {
                            time_input = DateTime.ParseExact(temp, "dd-MM-yyyy", null);
                        }
                        catch (Exception e)
                        {
                            time_input = DateTime.MinValue;
                        }
                        // Console.WriteLine(string.Format("Time test : {0}", time_input));
                        ItemPersonsPlot item = getPersonsHours(time_input);
                        items.Add(item);
                    }
                }
                return items;
            }
        }

    }
}
