using CBA.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Serilog;
using System;
using System.Reflection.Emit;
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

        public class ItemCounts
        {
            public string date { get; set; } = "";
            public List<ItemCountHours> data { get; set; } = new List<ItemCountHours>();
            public List<int> totalCount { get; set; } = new List<int>();
        }

        public class ItemCountsPlotInDay
        {
            public List<string> groups { get; set; } = new List<string>();
            public ItemCounts item { get; set; } = new ItemCounts();
        }

        public ItemCounts getData(DateTime timeCheck, DateTime begin, DateTime end, List<string> groups, List<ItemFaceOfPerson> logs)
        { 
            ItemCounts item = new ItemCounts();
            item.date = begin.ToLocalTime().ToString("dd-MM-yyyy");
            do
            {

                for (int i = 0; i < 24; i++)
                {
                    //Console.WriteLine(i);
                    ItemCountHours m_item = new ItemCountHours();
                    m_item.hour = i.ToString();
                    DateTime hourStart = begin.AddHours(i);
                    DateTime hourStop = hourStart.AddHours(1);
                    timeCheck = hourStop;

                    //Console.WriteLine(string.Format("Time start : {0} ---- Time Stop : {1}", hourStart.ToLocalTime(), hourStop.ToLocalTime()));

                    foreach (string group in groups)
                    {
                        int index = 0;
                        List<ItemFaceOfPerson>? gLogs = logs.Where(s => s.group.CompareTo(group) == 0).ToList();
                        if(gLogs.Count > 0)
                        {
                            foreach(ItemFaceOfPerson person in gLogs)
                            {
                                if (DateTime.Compare(hourStart, person.time) <= 0 && DateTime.Compare(hourStop, person.time) > 0)
                                {
                                    index++;
                                }
                            }    
                            
                        }
                        m_item.number.Add(index);
                        //Console.WriteLine(string.Format(" timeEnd : {0} - Count : {1} - Group : {2}", hourStop.ToLocalTime(), index, group));
                    }
                    item.data.Add(m_item);
                }


            }
            while (DateTime.Compare(timeCheck, end) < 0);

            for (int i = 0; i < groups.Count; i++)
            {
                int totalcount = 0;
                foreach (ItemCountHours tmp_count in item.data)
                {
                    totalcount += tmp_count.number[i];
                }
                item.totalCount.Add(totalcount);
            }

            return item;
        }

        public ItemCountsPlotInDay getCountHour(DateTime time)
        {

            using (DataContext context = new DataContext())
            {
                DateTime start = new DateTime(time.Year, time.Month, time.Day, 00, 00, 00);
                DateTime stop = start.AddDays(1);
                time = start.ToUniversalTime();
                ItemCountsPlotInDay tmp = new ItemCountsPlotInDay();

                List<ItemFaceOfPerson> face_persons = new List<ItemFaceOfPerson>();

                List<SqlFace>? faces = context.faces!.Where(s => DateTime.Compare(start.ToUniversalTime(), s.createdTime) <= 0 && DateTime.Compare(stop.ToUniversalTime(), s.createdTime) > 0).Include(s => s.person!).ThenInclude(s => s.group).ToList();
                if (faces.Count > 0)
                {

                    foreach (SqlFace m_face in faces)
                    {
                        ItemFaceOfPerson m_tmp = new ItemFaceOfPerson();
                        if (m_face.person != null)
                        {

                            ItemPersonForPlot person = new ItemPersonForPlot();
                            person.code = m_face.person.code;
                            person.name = m_face.person.name;

                            m_tmp.person = person;

                            if (m_face.person.group != null)
                            {
                                string? tmpGroup = tmp.groups!.Where(s => s.CompareTo(m_face.person.group.name) == 0).FirstOrDefault();
                                if (string.IsNullOrEmpty(tmpGroup))
                                {
                                    tmp.groups.Add(m_face.person.group.name);
                                }



                                m_tmp.group = m_face.person.group.name;
                                m_tmp.time = m_face.createdTime;
                                face_persons.Add(m_tmp);
                            }
                            else
                            {
                                if(tmp.groups.Count > 0)
                                {
                                    string? tmpGroup = tmp.groups.Where(s => s.CompareTo("") == 0).FirstOrDefault();
                                    if(tmpGroup == null)
                                    {
                                        tmp.groups.Add("");
                                    }    
                                }  
                                else
                                {
                                    tmp.groups.Add("");
                                }    
                                m_tmp.group = "";
                                m_tmp.time = m_face.createdTime;
                                face_persons.Add(m_tmp);
                            }

                        }


                    }
                }

                
                if(face_persons.Count > 0)
                {
                    if(tmp.groups.Count > 0)
                    {
                        ItemCounts item = getData(time, start.ToUniversalTime(), stop.ToUniversalTime(), tmp.groups, face_persons);
                        if(item != null)
                        {
                            tmp.item = item;
                        }
                    }
                }    

                return tmp;


            }
        }


        public class ItemCountsByDay
        {
            public string date { get; set; } = "";
            public List<int> totalCount { get; set; } = new List<int>();
        }

        public class ItemCountsPlotByDay
        {
            public List<string> groups { get; set; } = new List<string>();
            public List<ItemCountsByDay> items { get; set; } = new List<ItemCountsByDay>();
        }

        public ItemCountsPlotByDay getCountDates(DateTime begin, DateTime end)
        {

            using (DataContext context = new DataContext())
            {
                DateTime dateBegin = new DateTime(begin.Year, begin.Month, begin.Day, 00, 00, 00);
                DateTime dateEnd = new DateTime(end.Year, end.Month, end.Day, 00, 00, 00); 
                dateEnd = dateEnd.AddDays(1);

                ItemCountsPlotByDay tmp = new ItemCountsPlotByDay();
                List<string> m_times = new List<string>();

                List<ItemFaceOfPerson> face_persons = new List<ItemFaceOfPerson>();

                List<SqlFace>? faces = context.faces!.Where(s => DateTime.Compare(dateBegin.ToUniversalTime(), s.createdTime) <= 0 && DateTime.Compare(dateEnd.ToUniversalTime(), s.createdTime) > 0).Include(s => s.person!).ThenInclude(s => s.group).ToList();
                if (faces.Count > 0)
                {
                    
                    foreach (SqlFace m_face in faces)
                    {
                        string? tmpTime = m_times.Where(s => s.CompareTo(m_face.createdTime.ToLocalTime().ToString("dd-MM-yyyy")) == 0).FirstOrDefault();
                        if(tmpTime == null)
                        {
                            m_times.Add(m_face.createdTime.ToLocalTime().ToString("dd-MM-yyyy"));
                        }    
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
                                if (tmp.groups.Count > 0)
                                {
                                    string? tmpGroup = tmp.groups.Where(s => s.CompareTo("") == 0).FirstOrDefault();
                                    if (tmpGroup == null)
                                    {
                                        tmp.groups.Add("");
                                    }
                                }
                                else
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
                    foreach(string m_time in m_times)
                    {
                        
                        DateTime time = DateTime.ParseExact(m_time, "dd-MM-yyyy", null);
                        DateTime start = new DateTime(time.Year, time.Month, time.Day, 00, 00, 00);
                        DateTime stop = start.AddDays(1);
                        time = start.ToUniversalTime();
                        if (face_persons.Count > 0)
                        {
                            if (tmp.groups.Count > 0)
                            {
                                ItemCounts data = getData(time, start.ToUniversalTime(), stop.ToUniversalTime(), tmp.groups, face_persons);
                                if (data != null)
                                {
                                    ItemCountsByDay item = new ItemCountsByDay();
                                    item.date = m_time;
                                    item.totalCount = data.totalCount;
                                    tmp.items.Add(item);
                                }
                            }
                        }
                            
                    }
                    return tmp;

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return new ItemCountsPlotByDay();
                }


            }
        }

        public class ItemPersonHours
        {
            public string hour { get; set; } = "";
            public List<int> number { get; set; } = new List<int>();
        }

        public class ItemCountPersons
        {
            public string date { get; set; } = "";
            public List<ItemPersonHours> data { get; set; } = new List<ItemPersonHours>();
            public List<int> totalCount { get; set; } = new List<int>();
        }

        public ItemCountPersons getDataPersons(DateTime timeCheck, DateTime begin, DateTime end, List<string> groups, List<ItemFaceOfPerson> logs)
        {
            using (DataContext context = new DataContext())
            {
                List<ItemPersonForPlot> list_persons = new List<ItemPersonForPlot>();

                List<SqlPerson>? persons = context.persons!.Where(s => s.isdeleted == false).Include(s => s.group).AsNoTracking().ToList();
                if (persons.Count > 0)
                {
                    foreach (SqlPerson m_person in persons)
                    {
                        ItemPersonForPlot item = new ItemPersonForPlot();
                        item.code = m_person.code;
                        item.name = m_person.name;
                        list_persons.Add(item);
                    }
                }
                ItemCountPersons m_item = new ItemCountPersons();
                m_item.date = begin.ToLocalTime().ToString("dd-MM-yyyy");
                do
                {

                    for (int i = 0; i < 24; i++)
                    {
                        //Console.WriteLine(i);
                        ItemPersonHours item = new ItemPersonHours();
                        item.hour = i.ToString();
                        DateTime hourBegin = begin.AddHours(i);
                        DateTime hourEnd = hourBegin.AddHours(1);
                        timeCheck = hourEnd;

                        foreach (string group in groups)
                        {
                            int index = 0;
                            List<ItemFaceOfPerson> mylist = new List<ItemFaceOfPerson>();

                            List<ItemFaceOfPerson> gLogs = logs.Where(s => s.group.CompareTo(group) == 0).ToList();// Filter following group....
                            if(gLogs.Count > 0)
                            {
                                foreach (ItemFaceOfPerson m_log in gLogs)
                                {
                                    if (DateTime.Compare(hourBegin, m_log.time) <= 0 && DateTime.Compare(hourEnd, m_log.time) > 0)
                                    {
                                        mylist.Add(m_log);
                                    }
                                }
                            }    
                            if (mylist.Count > 0)
                            {
                                List<string> count_persons = new List<string>();

                                foreach (ItemFaceOfPerson m_person in mylist)
                                {
                                    ItemPersonForPlot? person = list_persons.Where(s => s.code.CompareTo(m_person.person.code) == 0).FirstOrDefault();
                                    if (person != null)
                                    {
                                        index++;

                                        //string? tempCode = count_persons.Where(s => s.CompareTo(person.code) == 0).FirstOrDefault();
                                        //if(tempCode == null)
                                        //{
                                        //    count_persons.Add(person.code);
                                        //}
                                        
                                    }
                                }
                                //if (count_persons.Count > 0)
                                //{
                                //    Console.WriteLine(string.Format("Time start : {0} ---- Time End : {1} |||||||||||| Group : {2}", hourBegin.ToLocalTime(), hourEnd.ToLocalTime(), group));
                                //    Console.WriteLine(string.Format("Count : {0} <----> Num_Index : {1}", count_persons.Count, index));
                                //}

                            }
                            item.number.Add(index);
                        }
                        m_item.data.Add(item);
                    }
                    for (int i = 0; i < groups.Count; i++)
                    {
                        int totalcount = 0;
                        if (m_item.data.Count > 0)
                        {
                            foreach (ItemPersonHours tmp_count in m_item.data)
                            {
                                totalcount += tmp_count.number[i];
                            }
                            m_item.totalCount.Add(totalcount);
                        }
                    }
                }
                while (DateTime.Compare(timeCheck, end) < 0);
                return m_item;

            }
        }


        public class ItemPersonsHoursPlot
        {
            public List<string> groups { get; set; } = new List<string>();
            public ItemCountPersons item { get; set; } = new ItemCountPersons();
        }

        public ItemPersonsHoursPlot getPersonsHours(DateTime time)
        {
            using(DataContext context = new DataContext())
            {
                DateTime start = new DateTime(time.Year, time.Month, time.Day, 00, 00, 00);
                DateTime stop = start.AddDays(1);
                time = start.ToUniversalTime();

                ItemPersonsHoursPlot tmp = new ItemPersonsHoursPlot();


                List<ItemFaceOfPerson> face_persons = new List<ItemFaceOfPerson>();

                List<SqlFace>? faces = context.faces!.Where(s => DateTime.Compare(start.ToUniversalTime(), s.createdTime) <= 0 && DateTime.Compare(stop.ToUniversalTime(), s.createdTime) > 0).Include(s => s.person!).ThenInclude(s => s.group).ToList();
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
                                if (tmp.groups.Count > 0)
                                {
                                    string? tmpGroup = tmp.groups.Where(s => s.CompareTo("") == 0).FirstOrDefault();
                                    if (tmpGroup == null)
                                    {
                                        tmp.groups.Add("");
                                    }
                                }
                                else
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

                if(face_persons.Count > 0)
                {
                    if(tmp.groups.Count > 0)
                    {
                        ItemCountPersons m_item = getDataPersons(time, start.ToUniversalTime(), stop.ToUniversalTime(), tmp.groups, face_persons);
                        if (m_item != null)
                        {
                            tmp.item = m_item;
                        }
                    }    
                }    
                return tmp;


                
                //try
                //{
                    

                    
                //}
                //catch(Exception ex)
                //{
                //    Console.WriteLine(ex);
                //    return new ItemPersonsPlot();
                //}
            }
        }

        public class ItemTotalPersons
        {
            public string date { get; set; } = "";
            public List<int> totalCount { get; set; } = new List<int>();
        }

        public class ItemPersonsPlotForDates
        {
            public List<string> groups { get; set; } = new List<string>();
            public List<ItemTotalPersons> items { get; set; } = new List<ItemTotalPersons>();
        }

        public ItemPersonsPlotForDates getCountPersonForDate(DateTime begin, DateTime end)
        {
            using (DataContext context = new DataContext())
            {
                DateTime dateBegin = new DateTime(begin.Year, begin.Month, begin.Day, 00, 00, 00);
                DateTime dateEnd = new DateTime(end.Year, end.Month, end.Day, 00, 00, 00);
                dateEnd = dateEnd.AddDays(1);
                ItemPersonsPlotForDates tmp = new ItemPersonsPlotForDates();
                List<string> m_times = new List<string>();

                List<ItemFaceOfPerson> face_persons = new List<ItemFaceOfPerson>();

                List<SqlFace>? faces = context.faces!.Where(s => DateTime.Compare(dateBegin.ToUniversalTime(), s.createdTime) <= 0 && DateTime.Compare(dateEnd.ToUniversalTime(), s.createdTime) > 0).Include(s => s.person!).ThenInclude(s => s.group).ToList();
                if (faces.Count > 0)
                {

                    foreach (SqlFace m_face in faces)
                    {
                        string? tmpTime = m_times.Where(s => s.CompareTo(m_face.createdTime.ToLocalTime().ToString("dd-MM-yyyy")) == 0).FirstOrDefault();
                        if(tmpTime == null)
                        {
                            m_times.Add(m_face.createdTime.ToLocalTime().ToString("dd-MM-yyyy"));
                        }    
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
                                if (tmp.groups.Count > 0)
                                {
                                    string? tmpGroup = tmp.groups.Where(s => s.CompareTo("") == 0).FirstOrDefault();
                                    if (tmpGroup == null)
                                    {
                                        tmp.groups.Add("");
                                    }
                                }
                                else
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

                foreach(string m_time in m_times)
                {
                    DateTime time = DateTime.ParseExact(m_time, "dd-MM-yyyy", null);
                    DateTime start = new DateTime(time.Year, time.Month, time.Day, 00, 00, 00);
                    DateTime stop = start.AddDays(1);
                    time = start.ToUniversalTime();
                    if (face_persons.Count > 0)
                    {
                        if (tmp.groups.Count > 0)
                        {
                            ItemCountPersons item = getDataPersons(time, start.ToUniversalTime(), stop.ToUniversalTime(), tmp.groups, face_persons);
                            if (item != null)
                            {
                                ItemTotalPersons temp = new ItemTotalPersons();
                                temp.date = m_time;
                                temp.totalCount = item.totalCount;
                                tmp.items.Add(temp);
                            }
                        }
                    }
                }
                return tmp;
            }
        }
    }
}
