using CBA.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Npgsql;
using Serilog;
using System;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using static CBA.APIs.MyGroup;
using static System.Reflection.Metadata.BlobBuilder;

namespace CBA.APIs
{
    public class MyReport
    {
        public class ItemPersonForPlot
        {
            public string code { get; set; } = "";
            public string name { get; set; } = "";
            public string group { get; set; } = "";
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

                List<SqlGroup> groups = context.groups!.Where(s => s.isdeleted == false).ToList();
                if (groups.Count > 0)
                {
                    foreach (SqlGroup m_group in groups)
                    {
                        string item = m_group.name;
                        tmp.groups.Add(item);
                    }
                }
                tmp.groups.Add("");

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
                                m_tmp.group = m_face.person.group.name;
                                m_tmp.time = m_face.createdTime;
                                face_persons.Add(m_tmp);
                            }
                            else
                            {
                                m_tmp.group = "";
                                m_tmp.time = m_face.createdTime;
                                face_persons.Add(m_tmp);
                            }

                        }


                    }
                }


                if (face_persons.Count > 0)
                {
                    if(tmp.groups.Count > 0)
                    {
                        ItemCounts item = getData(time, start.ToUniversalTime(), stop.ToUniversalTime(), tmp.groups, face_persons);
                        for (int i = 0; i < tmp.groups.Count; i++)
                        {
                            int totalcount = 0;
                            foreach (ItemCountHours tmp_count in item.data)
                            {
                                totalcount += tmp_count.number[i];
                            }
                            item.totalCount.Add(totalcount);
                        }
                        tmp.item = item;
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
                        
                    }
                }
                try
                {
                    foreach(string m_time in m_times)
                    {

                        ItemCountsByDay item = new ItemCountsByDay();

                        DateTime time = DateTime.ParseExact(m_time, "dd-MM-yyyy", null);
                        ItemCountsPlotInDay mCount = getCountHour(time);
                        item.date = m_time;
                        item.totalCount = mCount.item.totalCount;
                        tmp.items.Add(item);
                        tmp.groups = mCount.groups;

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

                
                ItemCountPersons m_item = new ItemCountPersons();
                m_item.date = begin.ToLocalTime().ToString("dd-MM-yyyy");
                do
                {
                    List<string> count_days = new List<string>();
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
                            List<string> count_hour = new List<string>();
                            List<ItemFaceOfPerson> m_logs = logs.Where(s => s.group.CompareTo(group) == 0).ToList();
                            foreach (ItemFaceOfPerson m_log in m_logs)
                            {
                                if (DateTime.Compare(hourBegin, m_log.time) <= 0 && DateTime.Compare(hourEnd, m_log.time) > 0)
                                {
                                    string? tempHours = count_hour.Where(s => s.CompareTo(m_log.person.code) == 0).FirstOrDefault(); // Count person In Day
                                    if (tempHours == null)
                                    {
                                        count_hour.Add(m_log.person.code);
                                    }
                                }
                            }
                            item.number.Add(count_hour.Count);
                            //Console.WriteLine(string.Format("{0} Hour, Count  : {1} --- Group : {2} ",item.hour, index, group));

                        }

                        m_item.data.Add(item);
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

                List<SqlPerson> sqlPersons = context.persons!.Where(s => DateTime.Compare(start.ToUniversalTime(), s.lastestTime) <= 0 && DateTime.Compare(stop.ToUniversalTime(), s.lastestTime) > 0 && s.isdeleted == false).ToList();
                
                List<SqlGroup> groups = context.groups!.Where(s => s.isdeleted == false).ToList();
                if (groups.Count > 0)
                {
                    foreach (SqlGroup m_group in groups)
                    {
                        string item = m_group.name;
                        tmp.groups.Add(item);
                    }
                }
                tmp.groups.Add("");

                List<ItemFaceOfPerson> face_persons = new List<ItemFaceOfPerson>();

                List<SqlFace>? faces = context.faces!.Where(s => DateTime.Compare(start.ToUniversalTime(), s.createdTime) <= 0 && DateTime.Compare(stop.ToUniversalTime(), s.createdTime) > 0 && s.isdeleted == false).Include(s => s.person!).ThenInclude(s => s.group).ToList();
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
                            item.group = m_face.person.group.name;
                            item.time = m_face.createdTime;
                        }
                        else
                        {
                            item.group = "";
                            item.time = m_face.createdTime;
                        }

                        face_persons.Add(item);

                    }


                }

                if (face_persons.Count > 0)
                {
                    if(tmp.groups.Count > 0)
                    {
                        ItemCountPersons item = getDataPersons(time, start.ToUniversalTime(), stop.ToUniversalTime(), tmp.groups, face_persons);
                        foreach (string temp in tmp.groups)
                        {
                            List<string> codePesons = new List<string>();
                            List<ItemFaceOfPerson> gPersons = face_persons.Where(s => s.group.CompareTo(temp) == 0).ToList();
                            foreach (ItemFaceOfPerson mPerson in gPersons)
                            {
                                string? mCount = codePesons.Where(s => s.CompareTo(mPerson.person.code) == 0).FirstOrDefault();
                                if (mCount == null)
                                {
                                    codePesons.Add(mPerson.person.code);
                                }
                            }
                            //if(temp.CompareTo("") == 0)
                            //{
                            //    foreach(string view in codePesons)
                            //    {
                            //        Console.WriteLine(view);
                            //    }
                            //}
                            item.totalCount.Add(codePesons.Count);
                        }
                       
                        tmp.item = item;
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

                List<string> m_times = new List<string>();

                ItemPersonsPlotForDates tmp = new ItemPersonsPlotForDates();

                List<SqlFace>? faces = context.faces!.Where(s => DateTime.Compare(dateBegin.ToUniversalTime(), s.createdTime) <= 0 && DateTime.Compare(dateEnd.ToUniversalTime(), s.createdTime) > 0).Include(s => s.person!).ThenInclude(s => s.group).ToList();
                foreach (SqlFace m_face in faces)
                {
                    string? tmpTime = m_times.Where(s => s.CompareTo(m_face.createdTime.ToLocalTime().ToString("dd-MM-yyyy")) == 0).FirstOrDefault();
                    if (tmpTime == null)
                    {
                        m_times.Add(m_face.createdTime.ToLocalTime().ToString("dd-MM-yyyy"));
                    }
                }
                try
                {
                    foreach (string m_time in m_times)
                    {
                        DateTime time = DateTime.ParseExact(m_time, "dd-MM-yyyy", null);
                        ItemTotalPersons temp = new ItemTotalPersons();

                        ItemPersonsHoursPlot countHours = getPersonsHours(time);
                        temp.date = m_time;
                        temp.totalCount = countHours.item.totalCount;
                        tmp.items.Add(temp);
                        tmp.groups = countHours.groups;

                    }
                    return tmp;
                }
                catch(Exception ex)
                {
                    Log.Error(ex.ToString());
                    return new ItemPersonsPlotForDates();
                }
            }
        }

        /// <summary>
        /// Plot persons with Devices
        /// </summary>

        public class ItemPersonForDevice
        {
            public string code { get; set; } = "";
            public string name { get; set; } = "";
        }

        public class ItemFaceForDevice
        {
            public ItemPersonForDevice person { get; set; } = new ItemPersonForDevice();

            public string device { get; set; } = "";

            public DateTime time { get; set; }
        }

        public class ItemCountWithDevice
        {
            public string hour { get; set; } = "";
            public List<int> number { get; set; } = new List<int>();
        }
        public class ItemCountPersonsWithDevice
        {
            public string date { get; set; } = "";
            public List<ItemCountWithDevice> data { get; set; } = new List<ItemCountWithDevice>();
            public List<int> totalCount { get; set; } = new List<int>();
        }

        public ItemCountPersonsWithDevice getDataPersonsWithDevice(DateTime timeCheck, DateTime begin, DateTime end, List<string> devices, List<ItemFaceForDevice> logs)
        {
            using (DataContext context = new DataContext())
            {
                List<ItemPersonForPlot> list_persons = new List<ItemPersonForPlot>();


                ItemCountPersonsWithDevice m_item = new ItemCountPersonsWithDevice();
                m_item.date = begin.ToLocalTime().ToString("dd-MM-yyyy");
                do
                {
                    for (int i = 0; i < 24; i++)
                    {
                        //Console.WriteLine(i);
                        ItemCountWithDevice item = new ItemCountWithDevice();
                        item.hour = i.ToString();
                        DateTime hourBegin = begin.AddHours(i);
                        DateTime hourEnd = hourBegin.AddHours(1);
                        timeCheck = hourEnd;
                        foreach (string device in devices)
                        {
                            List<string> count_hour = new List<string>();
                            List<ItemFaceForDevice> m_logs = logs.Where(s => s.device.CompareTo(device) == 0).ToList();
                            foreach (ItemFaceForDevice m_log in m_logs)
                            {
                                if (DateTime.Compare(hourBegin, m_log.time) <= 0 && DateTime.Compare(hourEnd, m_log.time) > 0)
                                {
                                    string? tempHours = count_hour.Where(s => s.CompareTo(m_log.person.code) == 0).FirstOrDefault(); // Count person In Day
                                    if (tempHours == null)
                                    {
                                        count_hour.Add(m_log.person.code);
                                    }
                                }
                            }
                            item.number.Add(count_hour.Count);
                            //Console.WriteLine(string.Format("{0} Hour, Count  : {1} --- Group : {2} ",item.hour, index, group));

                        }
                        m_item.data.Add(item);
                    }
                }
                while (DateTime.Compare(timeCheck, end) < 0);
                return m_item;

            }
        }


        public class ItemInfoPlot
        {
            public List<string> devices { get; set; } = new List<string>();
            public ItemCountPersonsWithDevice item { get; set; } = new ItemCountPersonsWithDevice();
        }

        public ItemInfoPlot getCountWithDevice(DateTime time)
        {
            using (DataContext context = new DataContext())
            {
                DateTime start = new DateTime(time.Year, time.Month, time.Day, 00, 00, 00);
                DateTime stop = start.AddDays(1);
                time = start.ToUniversalTime();

                ItemInfoPlot tmp = new ItemInfoPlot();

                List<SqlDevice> devices = context.devices!.Where(s => s.isdeleted == false).ToList();
                if (devices.Count > 0)
                {
                    foreach (SqlDevice m_device in devices)
                    {
                        string item = m_device.name;
                        tmp.devices.Add(item);
                    }
                }

                List<ItemFaceForDevice> myfaces = new List<ItemFaceForDevice>();

                List<SqlFace>? faces = context.faces!.Where(s => DateTime.Compare(start.ToUniversalTime(), s.createdTime) <= 0 && DateTime.Compare(stop.ToUniversalTime(), s.createdTime) > 0).Include(s => s.person!).ThenInclude(s => s.group).ToList();
                foreach (SqlFace m_face in faces)
                {
                    ItemFaceForDevice item = new ItemFaceForDevice();
                    if (m_face.person != null)
                    {

                        ItemPersonForDevice person = new ItemPersonForDevice();
                        person.code = m_face.person.code;
                        person.name = m_face.person.name;

                        item.person = person;

                        if (m_face.device != null)
                        {
                            item.device = m_face.device.name;
                            item.time = m_face.createdTime;
                            myfaces.Add(item);
                        }
                    }
                }

                if (myfaces.Count > 0)
                {
                    if (tmp.devices.Count > 0)
                    {
                        ItemCountPersonsWithDevice item = getDataPersonsWithDevice(time, start.ToUniversalTime(), stop.ToUniversalTime(), tmp.devices, myfaces);
                        foreach (string temp in tmp.devices)
                        {
                            List<string> codePesons = new List<string>();
                            List<ItemFaceForDevice> myPersons = myfaces.Where(s => s.device.CompareTo(temp) == 0).ToList();
                            foreach (ItemFaceForDevice mPerson in myPersons)
                            {
                                string? mCount = codePesons.Where(s => s.CompareTo(mPerson.person.code) == 0).FirstOrDefault();
                                if (mCount == null)
                                {
                                    codePesons.Add(mPerson.person.code);
                                }
                            }
                            item.totalCount.Add(codePesons.Count);
                        }
                        tmp.item = item;
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

        public class ItemTotalCountsWithDevice
        {
            public string date { get; set; } = "";
            public List<int> totalCount { get; set; } = new List<int>();
        }

        public class ItemInfoPlotForDates
        {
            public List<string> devices { get; set; } = new List<string>();
            public List<ItemTotalCountsWithDevice> items { get; set; } = new List<ItemTotalCountsWithDevice>();
        }

        public ItemInfoPlotForDates getCountWithDeviceForDates(DateTime begin, DateTime end)
        {
            using (DataContext context = new DataContext())
            {
                DateTime dateBegin = new DateTime(begin.Year, begin.Month, begin.Day, 00, 00, 00);
                DateTime dateEnd = new DateTime(end.Year, end.Month, end.Day, 00, 00, 00);
                dateEnd = dateEnd.AddDays(1);

                List<string> m_times = new List<string>();

                ItemInfoPlotForDates tmp = new ItemInfoPlotForDates();

                List<SqlFace>? faces = context.faces!.Where(s => DateTime.Compare(dateBegin.ToUniversalTime(), s.createdTime) <= 0 && DateTime.Compare(dateEnd.ToUniversalTime(), s.createdTime) > 0).Include(s => s.person!).ThenInclude(s => s.group).ToList();
                foreach (SqlFace m_face in faces)
                {
                    string? tmpTime = m_times.Where(s => s.CompareTo(m_face.createdTime.ToLocalTime().ToString("dd-MM-yyyy")) == 0).FirstOrDefault();
                    if (tmpTime == null)
                    {
                        m_times.Add(m_face.createdTime.ToLocalTime().ToString("dd-MM-yyyy"));
                    }
                }
                try
                {
                    foreach (string m_time in m_times)
                    {
                        DateTime time = DateTime.ParseExact(m_time, "dd-MM-yyyy", null);
                        ItemTotalCountsWithDevice temp = new ItemTotalCountsWithDevice();

                        ItemInfoPlot countHours = getCountWithDevice(time);
                        temp.date = m_time;
                        temp.totalCount = countHours.item.totalCount;
                        tmp.items.Add(temp);
                        tmp.devices = countHours.devices;

                    }
                    return tmp;
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                    return new ItemInfoPlotForDates();
                }
            }
        }

    }
}
