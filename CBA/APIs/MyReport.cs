using CBA.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Npgsql;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Logical;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using Serilog;
using System;
using System.Drawing;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using static CBA.APIs.MyGroup;
using static System.Reflection.Metadata.BlobBuilder;
using Log = Serilog.Log;

namespace CBA.APIs
{
    public class MyReport
    {
        public class ItemBufferForGroups
        {
            public string person { get; set; } = "";

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

        public ItemCountsPlotInDay getCountHour(DateTime time)
        {

            using (DataContext context = new DataContext())
            {
                DateTime begin = new DateTime(time.Year, time.Month, time.Day, 00, 00, 00);
                DateTime end = begin.AddDays(1);
                DateTime timeHours = begin;

                ItemCountsPlotInDay tmp = new ItemCountsPlotInDay();

                List<ItemBufferForGroups> buffers = new List<ItemBufferForGroups>();

                List<SqlGroup> groups = context.groups!.Where(s => s.isdeleted == false).ToList();
                if (groups.Count > 0)
                {
                    foreach (SqlGroup m_group in groups)
                    {
                        tmp.groups.Add(m_group.name);
                    }
                }

                tmp.groups.Add("");

                List<SqlFace>? faces = context.faces!.Where(s => DateTime.Compare(begin.ToUniversalTime(), s.createdTime) <= 0 && DateTime.Compare(end.ToUniversalTime(), s.createdTime) > 0 && s.isdeleted == false).Include(s => s.person!).ThenInclude(s => s.group).ToList();
                if (faces.Count > 0)
                {

                    foreach (SqlFace m_face in faces)
                    {
                        ItemBufferForGroups m_buffer = new ItemBufferForGroups();

                        if (m_face.person != null)
                        {

                            m_buffer.person = m_face.person.code;

                            if (m_face.person.group != null)
                            {
                                m_buffer.group = m_face.person.group.name;
                            }
                            else
                            {
                                m_buffer.group = "";
                            }
                        }
                        m_buffer.time = m_face.createdTime;
                        buffers.Add(m_buffer);

                    }
                }


                //ItemCounts itemCount = getData(time, start.ToUniversalTime(), stop.ToUniversalTime(), tmp.groups, face_persons);
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
                        timeHours = hourStop;

                        //Console.WriteLine(string.Format("Time start : {0} ---- Time Stop : {1}", hourStart.ToLocalTime(), hourStop.ToLocalTime()));

                        foreach (string group in tmp.groups)
                        {
                            int index = 0;
                            List<ItemBufferForGroups>? persons = buffers.Where(s => s.group.CompareTo(group) == 0).OrderBy(s => s.time).ToList();
                            if (persons.Count > 0)
                            {
                                foreach (ItemBufferForGroups m_person in persons)
                                {
                                    if (DateTime.Compare(hourStart.ToUniversalTime(), m_person.time) <= 0 && DateTime.Compare(hourStop.ToUniversalTime(), m_person.time) > 0)
                                    {
                                        index++;
                                       //Console.WriteLine(string.Format(" timeEnd : {0} - Count : {1} - Group : {2}", hourStop.ToLocalTime(), index, group));

                                    }
                                }

                            }
                            m_item.number.Add(index);
                        }
                        item.data.Add(m_item);
                    }


                }
                while (DateTime.Compare(timeHours, end) < 0);

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
            public List<int> totals { get; set; } = new List<int>();

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
                do
                {
                    m_times.Add(dateBegin.ToString("dd-MM-yyyy"));
                    dateBegin = dateBegin.AddDays(1);

                } while (DateTime.Compare(dateBegin, dateEnd) < 0);

                try
                {
                    foreach (string m_time in m_times)
                    {

                        ItemCountsByDay item = new ItemCountsByDay();

                        DateTime time = DateTime.ParseExact(m_time, "dd-MM-yyyy", null);
                        ItemCountsPlotInDay m_counts = getCountHour(time);
                        item.date = m_time;
                        item.totalCount = m_counts.item.totalCount;
                        tmp.items.Add(item);
                        tmp.groups = m_counts.groups;
                        tmp.totals = new List<int>();
                        for (int i = 0; i < tmp.groups.Count; i++)
                        {
                            tmp.totals.Add(0);
                            foreach (ItemCountsByDay m_item in tmp.items)
                            {
                                tmp.totals[i] += m_item.totalCount[i];
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

        public class ItemPersonsHoursPlot
        {
            public List<string> groups { get; set; } = new List<string>();
            public ItemCountPersons item { get; set; } = new ItemCountPersons();
        }

        public ItemPersonsHoursPlot getPersonsHours(DateTime time)
        {
            using (DataContext context = new DataContext())
            {
                DateTime begin = new DateTime(time.Year, time.Month, time.Day, 00, 00, 00);
                DateTime end = begin.AddDays(1);
                DateTime timeHours = begin;

                ItemPersonsHoursPlot tmp = new ItemPersonsHoursPlot();

                List<SqlGroup> groups = context.groups!.Where(s => s.isdeleted == false).ToList();
                if (groups.Count > 0)
                {
                    foreach (SqlGroup m_group in groups)
                    {
                        tmp.groups.Add(m_group.name);
                    }
                }
                tmp.groups.Add("");

                List<ItemBufferForGroups> buffers = new List<ItemBufferForGroups>();

                List<SqlFace>? faces = context.faces!.Where(s => DateTime.Compare(begin.ToUniversalTime(), s.createdTime) <= 0 && DateTime.Compare(end.ToUniversalTime(), s.createdTime) > 0 && s.isdeleted == false).Include(s => s.person!).ThenInclude(s => s.group).ToList();
                foreach (SqlFace m_face in faces)
                {
                    ItemBufferForGroups m_buffer = new ItemBufferForGroups();
                    if (m_face.person != null)
                    {
                        m_buffer.person = m_face.person.code;

                        if (m_face.person.group != null)
                        {
                            m_buffer.group = m_face.person.group.name;
                        }
                        else
                        {
                            m_buffer.group = "";
                        }
                    }
                    m_buffer.time = m_face.createdTime;

                    buffers.Add(m_buffer);
                }

                //ItemCountPersons item = getDataPersons(time, start.ToUniversalTime(), stop.ToUniversalTime(), tmp.groups, face_persons);

                ItemCountPersons m_item = new ItemCountPersons();
                m_item.date = begin.ToLocalTime().ToString("dd-MM-yyyy");
                do
                {
                    for (int i = 0; i < 24; i++)
                    {

                        //Console.WriteLine(i);
                        ItemPersonHours item = new ItemPersonHours();
                        item.hour = i.ToString();
                        DateTime hourStart = begin.AddHours(i);
                        DateTime hourStop = hourStart.AddHours(1);
                        timeHours = hourStop;

                        foreach (string group in tmp.groups)
                        {
                            List<string> numbers = new List<string>();


                            List<ItemBufferForGroups> perrsons = buffers.Where(s => s.group.CompareTo(group) == 0).OrderBy(s => s.time).ToList();
                            foreach (ItemBufferForGroups m_person in perrsons)
                            {
                                if (DateTime.Compare(hourStart.ToUniversalTime(), m_person.time) <= 0 && DateTime.Compare(hourStop.ToUniversalTime(), m_person.time) > 0)
                                {
                                    string? tempHours = numbers.Where(s => s.CompareTo(m_person.person) == 0).FirstOrDefault(); // Count person In Day
                                    if (tempHours == null)
                                    {
                                        numbers.Add(m_person.person);
                                    }
                                }
                            }
                            item.number.Add(numbers.Count);
                            //Console.WriteLine(string.Format("{0} Hour, Count  : {1} --- Group : {2} ",item.hour, index, group));

                        }

                        m_item.data.Add(item);
                    }
                }
                while (DateTime.Compare(timeHours, end) < 0);

                foreach (string temp in tmp.groups)
                {
                    List<string> codes = new List<string>();
                    List<ItemBufferForGroups> bufPersons = buffers.Where(s => s.group.CompareTo(temp) == 0).ToList();
                    foreach (ItemBufferForGroups mPerson in bufPersons)
                    {
                        string? mCount = codes.Where(s => s.CompareTo(mPerson.person) == 0).FirstOrDefault();
                        if (mCount == null)
                        {
                            codes.Add(mPerson.person);
                        }
                    }

                    m_item.totalCount.Add(codes.Count);
                }

                tmp.item = m_item;
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
            public List<int> totals { get; set; } = new List<int>();

        }

        public ItemPersonsPlotForDates getCountPersonForDate(DateTime begin, DateTime end)
        {
            using (DataContext context = new DataContext())
            {
                DateTime dateBegin = new DateTime(begin.Year, begin.Month, begin.Day, 00, 00, 00);
                DateTime dateEnd = new DateTime(end.Year, end.Month, end.Day, 00, 00, 00);
                dateEnd = dateEnd.AddDays(1);

                List<string> m_times = new List<string>();

                do
                {
                    m_times.Add(dateBegin.ToString("dd-MM-yyyy"));
                    dateBegin = dateBegin.AddDays(1);

                } while (DateTime.Compare(dateBegin, dateEnd) < 0);
                try
                {
                    ItemPersonsPlotForDates tmp = new ItemPersonsPlotForDates();
                    foreach (string m_time in m_times)
                    {
                        DateTime time = DateTime.ParseExact(m_time, "dd-MM-yyyy", null);
                        ItemTotalPersons temp = new ItemTotalPersons();

                        ItemPersonsHoursPlot countHours = getPersonsHours(time);
                        temp.date = m_time;
                        temp.totalCount = countHours.item.totalCount;
                        tmp.items.Add(temp);
                        tmp.groups = countHours.groups;
                        tmp.totals = new List<int>();
                        for(int i = 0; i < tmp.groups.Count; i++)
                        {
                            tmp.totals.Add(0);
                            foreach (ItemTotalPersons m_item in tmp.items)
                            {
                                tmp.totals[i] += m_item.totalCount[i];
                            }
                        }
                    }
                    return tmp;
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                    return new ItemPersonsPlotForDates();
                }
            }
        }

        /// <summary>
        /// Plot persons with Devices
        /// </summary>

        public class ItemBufferForDevice
        {
            public string person { get; set; } = "";

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

        public class ItemInfoPlot
        {
            public List<string> devices { get; set; } = new List<string>();
            public ItemCountPersonsWithDevice item { get; set; } = new ItemCountPersonsWithDevice();
        }

        public ItemInfoPlot getCountWithDevice(DateTime time)
        {
            using (DataContext context = new DataContext())
            {
                DateTime begin = new DateTime(time.Year, time.Month, time.Day, 00, 00, 00);
                DateTime end = begin.AddDays(1);
                DateTime timeHours = begin;
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

                List<ItemBufferForDevice> buffers = new List<ItemBufferForDevice>();

                List<SqlFace>? faces = context.faces!.Where(s => DateTime.Compare(begin.ToUniversalTime(), s.createdTime) <= 0 && DateTime.Compare(end.ToUniversalTime(), s.createdTime) > 0).Include(s => s.person!).ThenInclude(s => s.group).ToList();
                foreach (SqlFace m_face in faces)
                {
                    ItemBufferForDevice m_buffer = new ItemBufferForDevice();
                    if (m_face.person != null)
                    {
                        m_buffer.person = m_face.person.code;
                    }

                    if (m_face.device != null)
                    {
                        m_buffer.device = m_face.device.name;
                    }

                    m_buffer.time = m_face.createdTime;

                    buffers.Add(m_buffer);
                }


                if (tmp.devices.Count > 0)
                {
                    ItemCountPersonsWithDevice m_item = new ItemCountPersonsWithDevice();
                    m_item.date = begin.ToLocalTime().ToString("dd-MM-yyyy");
                    do
                    {
                        for (int i = 0; i < 24; i++)
                        {
                            //Console.WriteLine(i);
                            ItemCountWithDevice item = new ItemCountWithDevice();
                            item.hour = i.ToString();
                            DateTime hourStart = begin.AddHours(i);
                            DateTime hourStop = hourStart.AddHours(1);
                            timeHours = hourStop;

                            foreach (string device in tmp.devices)
                            {
                                List<string> numbers = new List<string>();
                                List<ItemBufferForDevice> m_logs = buffers.Where(s => s.device.CompareTo(device) == 0).OrderBy(s => s.time).ToList();
                                foreach (ItemBufferForDevice m_log in m_logs)
                                {
                                    if (DateTime.Compare(hourStart.ToUniversalTime(), m_log.time) <= 0 && DateTime.Compare(hourStop.ToUniversalTime(), m_log.time) > 0)
                                    {
                                        string? tempHours = numbers.Where(s => s.CompareTo(m_log.person) == 0).FirstOrDefault(); // Count person In Day
                                        if (tempHours == null)
                                        {
                                            numbers.Add(m_log.person);
                                        }
                                    }
                                }
                                item.number.Add(numbers.Count);
                                //Console.WriteLine(string.Format("{0} Hour, Count  : {1} --- Group : {2} ",item.hour, index, group));

                            }
                            m_item.data.Add(item);
                        }
                    }
                    while (DateTime.Compare(timeHours, end) < 0);

                    foreach (string temp in tmp.devices)
                    {
                        List<string> codePesons = new List<string>();
                        List<ItemBufferForDevice> bufPersons = buffers.Where(s => s.device.CompareTo(temp) == 0).ToList();
                        foreach (ItemBufferForDevice mPerson in bufPersons)
                        {
                            string? mCount = codePesons.Where(s => s.CompareTo(mPerson.person) == 0).FirstOrDefault();
                            if (mCount == null)
                            {
                                codePesons.Add(mPerson.person);
                            }
                        }
                        m_item.totalCount.Add(codePesons.Count);
                    }
                    tmp.item = m_item;
                }

                return tmp;
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
            public List<int> totals { get; set; } = new List<int>();
        }

        public ItemInfoPlotForDates getCountWithDeviceForDates(DateTime begin, DateTime end)
        {
            using (DataContext context = new DataContext())
            {
                DateTime dateBegin = new DateTime(begin.Year, begin.Month, begin.Day, 00, 00, 00);
                DateTime dateEnd = new DateTime(end.Year, end.Month, end.Day, 00, 00, 00);
                dateEnd = dateEnd.AddDays(1);
                List<string> m_times = new List<string>();
                do
                {
                    m_times.Add(dateBegin.ToString("dd-MM-yyyy"));
                    dateBegin = dateBegin.AddDays(1);

                } while (DateTime.Compare(dateBegin, dateEnd) < 0);
                try
                {
                    ItemInfoPlotForDates tmp = new ItemInfoPlotForDates();

                    foreach (string m_time in m_times)
                    {
                        DateTime time = DateTime.ParseExact(m_time, "dd-MM-yyyy", null);
                        ItemTotalCountsWithDevice temp = new ItemTotalCountsWithDevice();

                        ItemInfoPlot countHours = getCountWithDevice(time);
                        temp.date = m_time;
                        temp.totalCount = countHours.item.totalCount;
                        tmp.items.Add(temp);
                        tmp.devices = countHours.devices;
                        tmp.totals = new List<int>();
                        for (int i = 0; i < tmp.devices.Count; i++)
                        {
                            tmp.totals.Add(0);
                            foreach (ItemTotalCountsWithDevice m_item in tmp.items)
                            {
                                tmp.totals[i] += m_item.totalCount[i];
                            }
                        }
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


        public class ItemBufferForAge
        {
            public string person { get; set; } = "";
            public string level { get; set; } = "";
            public DateTime time { get; set; }

        }

        public class ItemHours
        {
            public string hour { get; set; } = "";
            public List<int> number { get; set; } = new List<int>();
        }

        public class ItemData
        {
            public string date { get; set; } = "";
            public List<ItemHours> data { get; set; } = new List<ItemHours>();
            public List<int> totalCount { get; set; } = new List<int>();
        }
        public class ItemCountWithAge
        {
            public List<string> levels { get; set; } = new List<string>();
            public ItemData item { get; set; } = new ItemData();

        }

        public ItemCountWithAge showPlotLevel(DateTime time)
        {
            DateTime begin = new DateTime(time.Year, time.Month, time.Day, 00, 00, 00);
            DateTime end = begin.AddDays(1);
            using (DataContext context = new DataContext())
            {
                ItemCountWithAge tmp = new ItemCountWithAge();

                List<SqlAgeLevel>? levels = context.ages!.Where(s => s.isdeleted == false).OrderByDescending(s => s.high).ToList();
                if (levels.Count > 0)
                {
                    foreach (SqlAgeLevel m_level in levels)
                    {
                        tmp.levels.Add(m_level.name);
                    }
                }
                tmp.levels.Add("");

                List<ItemBufferForAge> buffers = new List<ItemBufferForAge>();


                List<SqlFace> faces = context.faces!.Where(s => s.isdeleted == false && (DateTime.Compare(begin.ToUniversalTime(), s.createdTime) <= 0 && DateTime.Compare(end.ToUniversalTime(), s.createdTime) > 0)).Include(s => s.person!).ThenInclude(s => s.level).ToList();
                if (faces.Count > 0)
                {

                    foreach (SqlFace itemFace in faces)
                    {
                        ItemBufferForAge itemBuffer = new ItemBufferForAge();

                        if (itemFace.person != null)
                        {
                            itemBuffer.person = itemFace.person.code;
                            if (itemFace.person.level != null)
                            {
                                itemBuffer.level = itemFace.person.level.name;
                            }
                            else
                            {
                                itemBuffer.level = "";
                            }
                        }

                        itemBuffer.time = itemFace.createdTime;

                        buffers.Add(itemBuffer);
                    }

                }
                DateTime hourTime = begin;
                ItemData data = new ItemData();

                data.date = time.ToString("dd-MM-yyyy");
                do
                {
                    for (int i = 0; i < 24; i++)
                    {
                        ItemHours hour = new ItemHours();

                        hour.hour = i.ToString();

                        DateTime hourStart = begin.AddHours(i);
                        DateTime hourStop = hourStart.AddHours(1);
                        hourTime = hourStop;
                        foreach (string item in tmp.levels)
                        {
                            List<string> persons = new List<string>();

                            List<ItemBufferForAge>? mPersons = buffers.Where(s => s.level.CompareTo(item) == 0).OrderBy(s => s.time).ToList();
                            if (mPersons.Count > 0)
                            {

                                foreach (ItemBufferForAge m_person in mPersons)
                                {
                                    if (DateTime.Compare(hourStart.ToUniversalTime(), m_person.time) <= 0 && DateTime.Compare(hourStop.ToUniversalTime(), m_person.time) > 0)
                                    {
                                        string? tmpPerson = persons.Where(s => s.CompareTo(m_person.person) == 0).FirstOrDefault();
                                        if (tmpPerson == null)
                                        {
                                            persons.Add(m_person.person);
                                        }

                                    }
                                }
                            }
                            hour.number.Add(persons.Count);

                        }
                        data.data.Add(hour);
                    }

                } while (DateTime.Compare(hourTime, end) < 0);

                foreach (string m_item in tmp.levels)
                {
                    List<string> mCounts = new List<string>();

                    List<ItemBufferForAge>? mBuffers = buffers.Where(s => s.level.CompareTo(m_item) == 0).ToList();
                    if (mBuffers.Count > 0)
                    {
                        foreach (ItemBufferForAge m_buffer in mBuffers)
                        {
                            string? tempCode = mCounts.Where(s => s.CompareTo(m_buffer.person) == 0).FirstOrDefault();
                            if (tempCode == null)
                            {
                                mCounts.Add(m_buffer.person);
                            }

                        }
                    }
                    data.totalCount.Add(mCounts.Count);
                }
                tmp.item = data;

                return tmp;
            }
        }

        public class ItemTotalCountsWithAge
        {
            public string date { get; set; } = "";
            public List<int> totalCount { get; set; } = new List<int>();
        }
        public class ItemInfoPlotForAge
        {
            public List<string> levels { get; set; } = new List<string>();
            public List<ItemTotalCountsWithAge> items { get; set; } = new List<ItemTotalCountsWithAge>();

            public List<int> totals { get; set; } = new List<int>();
        }

        public ItemInfoPlotForAge showPlotLevelForDates(DateTime begin, DateTime end)
        {
            DateTime dateBegin = new DateTime(begin.Year, begin.Month, begin.Day, 00, 00, 00);
            DateTime dateEnd = new DateTime(end.Year, end.Month, end.Day, 00, 00, 00);
            dateEnd = dateEnd.AddDays(1);

            List<string> times = new List<string>();

            do
            {
                times.Add(dateBegin.ToString("dd-MM-yyyy"));
                dateBegin = dateBegin.AddDays(1);
            } while (DateTime.Compare(dateBegin, dateEnd) < 0);

            using (DataContext context = new DataContext())
            {
                ItemInfoPlotForAge tmp = new ItemInfoPlotForAge();

                foreach (string m_time in times)
                {
                    DateTime time_input = DateTime.MinValue;
                    try
                    {
                        time_input = DateTime.ParseExact(m_time, "dd-MM-yyyy", null);
                    }
                    catch (Exception e)
                    {
                        time_input = DateTime.MinValue;
                    }
                    ItemCountWithAge item = showPlotLevel(time_input);
                    ItemTotalCountsWithAge data = new ItemTotalCountsWithAge();

                    data.date = m_time;
                    data.totalCount = item.item.totalCount;
                    tmp.levels = item.levels;
                    tmp.items.Add(data);
                    tmp.totals = new List<int>();
                    for (int i = 0; i < tmp.levels.Count; i++)
                    {
                        tmp.totals.Add(0);
                        foreach (ItemTotalCountsWithAge m_item in tmp.items)
                        {
                            tmp.totals[i] += m_item.totalCount[i];
                        }
                    }

                }
                return tmp;
            }
        }

        public class ItemBufferForGender
        {
            public string person { get; set; } = "";
            public string gender { get; set; } = "";
            public List<ItemTimeGender> times { get; set; } = new List<ItemTimeGender>();
            public int total { get; set; } = 0;
        }

        public class ItemTimeGender
        {
            public DateTime time { get; set; }
        }


        public class ItemDataWithGender
        {
            public string date { get; set; } = "";
            public List<ItemHours> data { get; set; } = new List<ItemHours>();
            public List<int> totalCount { get; set; } = new List<int>();
        }
        public class ItemCountWithGender
        {
            public List<string> genders { get; set; } = new List<string>();
            public ItemDataWithGender item { get; set; } = new ItemDataWithGender();
        }

        public ItemCountWithGender showPlotGender(DateTime time)
        {
            DateTime begin = new DateTime(time.Year, time.Month, time.Day, 00, 00, 00);
            DateTime end = begin.AddDays(1);
            using (DataContext context = new DataContext())
            {
                ItemCountWithGender tmp = new ItemCountWithGender();

                tmp.genders.Add("0");
                tmp.genders.Add("1");
                tmp.genders.Add("2");

                List<string> mCodes = new List<string>();
                List<ItemBufferForGender> buffers = new List<ItemBufferForGender>();
                List<SqlFace>? faces = context.faces!.Where(s => s.isdeleted == false && (DateTime.Compare(begin.ToUniversalTime(), s.createdTime) <= 0 && DateTime.Compare(end.ToUniversalTime(), s.createdTime) > 0)).Include(s => s.person).ToList();
                if (faces.Count > 0)
                {
                    List<ItemBufferForGender> mybuffers = new List<ItemBufferForGender>();

                    foreach (SqlFace face in faces)
                    {
                        ItemBufferForGender? m_person = mybuffers.Where(s => s.person.CompareTo(face.person!.code) == 0 && s.gender.CompareTo(face.gender) == 0).FirstOrDefault();
                        if (m_person == null)
                        {
                            ItemBufferForGender item = new ItemBufferForGender();
                            item.person = face.person!.code;
                            item.gender = face.gender;

                            ItemTimeGender i_gender = new ItemTimeGender();
                            i_gender.time = face.createdTime;
                            item.times.Add(i_gender);
                            item.total = item.times.Count();
                            mybuffers.Add(item);

                            string? t_person = mCodes.Where(s => s.CompareTo(face.person!.code) == 0).FirstOrDefault();
                            if (t_person == null)
                            {
                                mCodes.Add(face.person!.code);
                            }

                        }
                        else
                        {
                            ItemTimeGender i_gender = new ItemTimeGender();
                            i_gender.time = face.createdTime;
                            m_person.times.Add(i_gender);
                            m_person.total = m_person.times.Count();
                        }
                    }

                    foreach (string m_code in mCodes)
                    {
                        List<ItemTimeGender> tmpTimes = new List<ItemTimeGender>();

                        List<ItemBufferForGender> arr_Buffers = mybuffers.Where(s => s.person.CompareTo(m_code) == 0).ToList();
                        if (arr_Buffers.Count() > 1)
                        {
                            int num = arr_Buffers.Max(s => s.total);
                            List<ItemBufferForGender> mItems = arr_Buffers.OrderBy(s => s.gender).ThenByDescending(s => s.total).ToList();                           
                            foreach (ItemBufferForGender item in mItems)
                            {
                                if (item.total < num)
                                {
                                    foreach(ItemTimeGender m_time in item.times)
                                    {
                                        tmpTimes.Add(m_time);
                                    }
                                    mItems.Remove(item);
                                    arr_Buffers.Remove(item);
                                    if(mItems.Count < 2)
                                    {
                                        break;
                                    }    
                                }
                            }
                            if (mItems.Count() > 1)
                            {
                                for (int i = 0; i < mItems.Count() - 1; i++)
                                {
                                    foreach (ItemTimeGender m_time in mItems[i].times)
                                    {
                                        tmpTimes.Add(m_time);
                                    }
                                    arr_Buffers.Remove(mItems[i]);
                                }
                            }
                        }
                        if (tmpTimes.Count > 0)
                        {
                            foreach (ItemTimeGender m_tmp in tmpTimes)
                            {
                                arr_Buffers[0].times.Add(m_tmp);
                            }
                        }
                        buffers.Add(arr_Buffers[0]);
                    }
                }



                DateTime hourTime = begin;
                ItemDataWithGender data = new ItemDataWithGender();

                data.date = time.ToString("dd-MM-yyyy");

                do
                {
                    for (int i = 0; i < 24; i++)
                    {
                        ItemHours hour = new ItemHours();

                        hour.hour = i.ToString();

                        DateTime hourStart = begin.AddHours(i);
                        DateTime hourStop = hourStart.AddHours(1);
                        hourTime = hourStop;

                        foreach (string gender in tmp.genders)
                        {
                            List<string> persons = new List<string>();

                            List<ItemBufferForGender>? mGenders = buffers.Where(s => s.gender.CompareTo(gender) == 0).OrderBy(s => s.times.Max(s => s.time)).ToList();
                            foreach (ItemBufferForGender m_gender in mGenders)
                            {
                                ItemTimeGender? m_person = m_gender.times.Where(s => DateTime.Compare(hourStart.ToUniversalTime(), s.time) <= 0 && DateTime.Compare(hourStop.ToUniversalTime(), s.time) > 0).FirstOrDefault();
                                if(m_person != null)
                                {
                                    persons.Add(m_gender.person);
                                    //if(i == 14)
                                    //{
                                    //    Console.WriteLine(string.Format("Time : {0} - Code : {1}", m_person.time.ToLocalTime(), m_gender.person));
                                    //}    
                                }
                            }
                            hour.number.Add(persons.Count);
                        }
                        data.data.Add(hour);
                    }
                } while (DateTime.Compare(hourTime, end) < 0);
                foreach (string m_item in tmp.genders)
                {
                    List<string> mCounts = new List<string>();

                    List<ItemBufferForGender>? tmpBuffers = buffers.Where(s => s.gender.CompareTo(m_item) == 0).ToList();
                    if (tmpBuffers.Count > 0)
                    {
                        foreach (ItemBufferForGender m_buffer in tmpBuffers)
                        {
                            string? tempCode = mCounts.Where(s => s.CompareTo(m_buffer.person) == 0).FirstOrDefault();
                            if (tempCode == null)
                            {
                                mCounts.Add(m_buffer.person);
                            }

                        }
                    }
                    data.totalCount.Add(mCounts.Count);
                }
                tmp.item = data;
                return tmp;
            }
        }
        public class ItemTotalCountsWithGender
        {
            public string date { get; set; } = "";
            public List<int> totalCount { get; set; } = new List<int>();
        }
        public class ItemInfoPlotForGender
        {
            public List<string> genders { get; set; } = new List<string>();
            public List<ItemTotalCountsWithGender> items { get; set; } = new List<ItemTotalCountsWithGender>();
            public List<int> totals { get; set; } = new List<int>();
        }
        public ItemInfoPlotForGender showPlotGenderForDates(DateTime begin, DateTime end)
        {
            DateTime dateBegin = new DateTime(begin.Year, begin.Month, begin.Day, 00, 00, 00);
            DateTime dateEnd = new DateTime(end.Year, end.Month, end.Day, 00, 00, 00);
            dateEnd = dateEnd.AddDays(1);

            List<string> times = new List<string>();

            do
            {
                times.Add(dateBegin.ToString("dd-MM-yyyy"));
                dateBegin = dateBegin.AddDays(1);
            } while (DateTime.Compare(dateBegin, dateEnd) < 0);

            using (DataContext context = new DataContext())
            {
                ItemInfoPlotForGender tmp = new ItemInfoPlotForGender();

                foreach (string m_time in times)
                {
                    DateTime time_input = DateTime.MinValue;
                    try
                    {
                        time_input = DateTime.ParseExact(m_time, "dd-MM-yyyy", null);
                    }
                    catch (Exception e)
                    {
                        time_input = DateTime.MinValue;
                    }
                    ItemCountWithGender item = showPlotGender(time_input);
                    ItemTotalCountsWithGender data = new ItemTotalCountsWithGender();

                    data.date = item.item.date;
                    data.totalCount = item.item.totalCount;
                    tmp.items.Add(data);
                    tmp.genders = item.genders;
                    tmp.totals = new List<int>();
                    for (int i = 0; i < tmp.genders.Count; i++)
                    {
                        tmp.totals.Add(0);
                        foreach (ItemTotalCountsWithGender m_item in tmp.items)
                        {
                            tmp.totals[i] += m_item.totalCount[i];
                        }
                    }
                }
                return tmp;
            }
        }
    }
}
