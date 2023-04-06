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

        public class DataRaw
        {
            public long ID { get; set; }
            public int age { get; set; } = 0;
            public string gender { get; set; } = "";
            public string image { get; set; } = "";
            public DataPersonRaw person { get; set; } = new DataPersonRaw();
            public DataDeviceRaw device { get; set; } = new DataDeviceRaw();
            public DateTime createdTime { get; set; }
        }

        public class DataPersonRaw
        {
            public long ID { get; set; }
            public string code { get; set; } = "";
            public string codeSystem { get; set; } = "";
            public string name { get; set; } = "";
            public string des { get; set; } = "";
            public string gender { get; set; } = "";
            public int age { get; set; } = 0;
            public DataGroupRaw group { get; set; } = new DataGroupRaw();
            public DataLevelRaw level { get; set; } = new DataLevelRaw();
            public DateTime lastestTime { get; set; }
            public DateTime createdTime { get; set; }
        }
       
        public class DataGroupRaw
        {
            public long ID { get; set; }
            public string code { get; set; } = "";
            public string name { get; set; } = "";
            public string des { get; set; } = "";
        }
        public class DataLevelRaw
        {
            public long ID { get; set; }
            public string code { get; set; } = "";
            public string name { get; set; } = "";
            public string des { get; set; } = "";
            public int low { get; set; } = int.MinValue;
            public int high { get; set; } = int.MaxValue;
        }
        public class DataDeviceRaw
        {
            public long ID { get; set; }
            public string code { get; set; } = "";
            public string name { get; set; } = "";
            public string des { get; set; } = "";
        }

        public List<DataRaw> getRawData(DateTime begin, DateTime end)
        {
            using (DataContext context = new DataContext())
            {
                List<SqlFace>? faces = context.faces!.Where(s => DateTime.Compare(begin.ToUniversalTime(), s.createdTime) <= 0 && DateTime.Compare(end.ToUniversalTime(), s.createdTime) > 0 && s.isdeleted == false)
                                                    .Include(s => s.person!).ThenInclude(s => s.group)
                                                    .Include(s => s.person!).ThenInclude(s => s.level)
                                                    .Include(s => s.device)
                                                    .ToList();
                List<DataRaw> datas = new List<DataRaw>();
                foreach(SqlFace face in faces)
                {
                    DataRaw item = new DataRaw();
                    item.ID = face.ID;
                    item.age = face.age;
                    item.gender = face.gender;
                    item.createdTime = face.createdTime.ToLocalTime();
                    item.image = face.image;
                    if(face.person != null)
                    {
                        if(face.person.isdeleted == false)
                        {
                            item.person.ID = face.person.ID;
                            item.person.name = face.person.name;
                            item.person.code = face.person.code;
                            item.person.codeSystem = face.person.codeSystem;
                            item.person.des = face.person.des;
                            item.person.gender = face.person.gender;
                            item.person.age = face.person.age;
                            item.person.createdTime = face.person.createdTime.ToLocalTime();
                            item.person.lastestTime = face.person.lastestTime.ToLocalTime();
                            if(face.person.group != null)
                            {
                                if(face.person.group.isdeleted == false)
                                {
                                    item.person.group.ID = face.person.group.ID;
                                    item.person.group.code = face.person.group.code;
                                    item.person.group.name = face.person.group.name;
                                    item.person.group.des = face.person.group.des;
                                }
                            }

                            if (face.person.level != null)
                            {
                                if (face.person.level.isdeleted == false)
                                {
                                    item.person.level.ID = face.person.level.ID;
                                    item.person.level.code = face.person.level.code;
                                    item.person.level.name = face.person.level.name;
                                    item.person.level.des = face.person.level.des;
                                    item.person.level.low = face.person.level.low;
                                    item.person.level.high = face.person.level.high;
                                }
                            }
                        }
                    }
                    if (face.device != null)
                    {
                        item.device.ID = face.device.ID;
                        item.device.code = face.device.code;
                        item.device.name = face.device.name;
                        item.device.des = face.device.des;
                    }
                    datas.Add(item);
                }
                return datas;
            }
        }

        public ItemCountsPlotInDay calcGetCountHour(DateTime time, List<DataRaw> datas)
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

                List<DataRaw> faces = datas.Where(s => DateTime.Compare(begin, s.createdTime) <= 0 && DateTime.Compare(end, s.createdTime) > 0).ToList();
                if (faces.Count > 0)
                {

                    foreach (DataRaw m_face in faces)
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

                ItemCounts item = new ItemCounts();
                item.date = begin.ToLocalTime().ToString("dd-MM-yyyy");
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
                                if (DateTime.Compare(hourStart, m_person.time) <= 0 && DateTime.Compare(hourStop, m_person.time) > 0)
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

        public ItemCountsPlotInDay getCountHourV2(DateTime time)
        {
            DateTime begin = new DateTime(time.Year, time.Month, time.Day, 00, 00, 00);
            DateTime end = begin.AddDays(1);
            List<DataRaw> datas = getRawData(begin, end);
            return calcGetCountHour(time, datas);
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
        public ItemCountsPlotByDay getCountDatesV2(DateTime begin, DateTime end)
        {
            using (DataContext context = new DataContext())
            {
                DateTime dateBegin = new DateTime(begin.Year, begin.Month, begin.Day, 00, 00, 00);
                DateTime dateEnd = new DateTime(end.Year, end.Month, end.Day, 00, 00, 00);
                dateEnd = dateEnd.AddDays(1);
                List<DataRaw> datas = getRawData(dateBegin, dateEnd);

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
                        ItemCountsPlotInDay m_counts = calcGetCountHour(time, datas);
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

        public ItemPersonsHoursPlot calcGetCountPersonHour(DateTime time, List<DataRaw> datas)
        {
            using (DataContext context = new DataContext())
            {
                DateTime begin = new DateTime(time.Year, time.Month, time.Day, 00, 00, 00);
                DateTime end = begin.AddDays(1);

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

                List<DataRaw> faces = datas.Where(s => DateTime.Compare(begin, s.createdTime) <= 0 && DateTime.Compare(end, s.createdTime) > 0).ToList();
                foreach (DataRaw m_face in faces)
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
                for (int i = 0; i < 24; i++)
                {

                    //Console.WriteLine(i);
                    ItemPersonHours item = new ItemPersonHours();
                    item.hour = i.ToString();
                    DateTime hourStart = begin.AddHours(i);
                    DateTime hourStop = hourStart.AddHours(1);

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

        public ItemPersonsHoursPlot getPersonsHoursV2(DateTime time)
        {
            DateTime begin = new DateTime(time.Year, time.Month, time.Day, 00, 00, 00);
            DateTime end = begin.AddDays(1);
            List<DataRaw> datas = getRawData(begin, end);
            return calcGetCountPersonHour(time, datas);
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
                List<DataRaw> datas = getRawData(begin, end);

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

                        ItemPersonsHoursPlot countHours = calcGetCountPersonHour(time, datas);
                        temp.date = m_time;
                        temp.totalCount = countHours.item.totalCount;
                        tmp.items.Add(temp);
                        tmp.groups = countHours.groups;
                        tmp.totals = new List<int>();
                        for (int i = 0; i < tmp.groups.Count; i++)
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
        public ItemPersonsPlotForDates getCountPersonForDateV2(DateTime begin, DateTime end)
        {
            using (DataContext context = new DataContext())
            {
                DateTime dateBegin = new DateTime(begin.Year, begin.Month, begin.Day, 00, 00, 00);
                DateTime dateEnd = new DateTime(end.Year, end.Month, end.Day, 00, 00, 00);
                dateEnd = dateEnd.AddDays(1);
                List<DataRaw> datas = getRawData(dateBegin, dateEnd);

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
                        for (int i = 0; i < tmp.groups.Count; i++)
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

        public ItemInfoPlot calcGetCountPersonDevice(DateTime time, List<DataRaw> datas)
        {
            using (DataContext context = new DataContext())
            {
                DateTime begin = new DateTime(time.Year, time.Month, time.Day, 00, 00, 00);
                DateTime end = begin.AddDays(1);
                ItemInfoPlot tmp = new ItemInfoPlot();

                List<SqlDevice> devices = context.devices!.Where(s => s.isdeleted == false).OrderBy(s => s.ID).ToList();
                if (devices.Count > 0)
                {
                    foreach (SqlDevice m_device in devices)
                    {
                        string item = m_device.name;
                        tmp.devices.Add(item);
                    }
                }

                List<ItemBufferForDevice> buffers = new List<ItemBufferForDevice>();

                List<DataRaw> faces = datas.Where(s => DateTime.Compare(begin, s.createdTime) <= 0 && DateTime.Compare(end, s.createdTime) > 0).ToList();
                foreach (DataRaw m_face in faces)
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
                    for (int i = 0; i < 24; i++)
                    {
                        //Console.WriteLine(i);
                        ItemCountWithDevice item = new ItemCountWithDevice();
                        item.hour = i.ToString();
                        DateTime hourStart = begin.AddHours(i);
                        DateTime hourStop = hourStart.AddHours(1);

                        foreach (string device in tmp.devices)
                        {
                            List<string> numbers = new List<string>();
                            List<ItemBufferForDevice> m_logs = buffers.Where(s => s.device.CompareTo(device) == 0).OrderBy(s => s.time).ToList();
                            foreach (ItemBufferForDevice m_log in m_logs)
                            {
                                if (DateTime.Compare(hourStart, m_log.time) <= 0 && DateTime.Compare(hourStop, m_log.time) > 0)
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
        public ItemInfoPlot getCountWithDevice(DateTime time)
        {
            using (DataContext context = new DataContext())
            {
                DateTime begin = new DateTime(time.Year, time.Month, time.Day, 00, 00, 00);
                DateTime end = begin.AddDays(1);
                DateTime timeHours = begin;
                ItemInfoPlot tmp = new ItemInfoPlot();

                List<SqlDevice> devices = context.devices!.Where(s => s.isdeleted == false).OrderBy(s => s.ID).ToList();
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
        public ItemInfoPlot getCountWithDeviceV2(DateTime time)
        {
            DateTime begin = new DateTime(time.Year, time.Month, time.Day, 00, 00, 00);
            DateTime end = begin.AddDays(1);
            List<DataRaw> datas = getRawData(begin, end);
            return calcGetCountPersonDevice(time, datas);
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
                List<DataRaw> datas = getRawData(dateBegin, dateEnd);

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
        public ItemInfoPlotForDates getCountWithDeviceForDatesV2(DateTime begin, DateTime end)
        {
            using (DataContext context = new DataContext())
            {
                DateTime dateBegin = new DateTime(begin.Year, begin.Month, begin.Day, 00, 00, 00);
                DateTime dateEnd = new DateTime(end.Year, end.Month, end.Day, 00, 00, 00);
                dateEnd = dateEnd.AddDays(1);
                List<DataRaw> datas = getRawData(dateBegin, dateEnd);

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

                        ItemInfoPlot countHours = calcGetCountPersonDevice(time, datas);
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
        public ItemCountWithAge calcGetCountPersonLevel(DateTime time, List<DataRaw> datas)
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


                List<DataRaw> faces = datas.Where(s => DateTime.Compare(begin, s.createdTime) <= 0 && DateTime.Compare(end, s.createdTime) > 0).ToList();
                if (faces.Count > 0)
                {

                    foreach (DataRaw itemFace in faces)
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
                ItemData data = new ItemData();

                data.date = time.ToString("dd-MM-yyyy");
                for (int i = 0; i < 24; i++)
                {
                    ItemHours hour = new ItemHours();

                    hour.hour = i.ToString();

                    DateTime hourStart = begin.AddHours(i);
                    DateTime hourStop = hourStart.AddHours(1);
                    foreach (string item in tmp.levels)
                    {
                        List<string> persons = new List<string>();

                        List<ItemBufferForAge>? mPersons = buffers.Where(s => s.level.CompareTo(item) == 0).OrderBy(s => s.time).ToList();
                        if (mPersons.Count > 0)
                        {

                            foreach (ItemBufferForAge m_person in mPersons)
                            {
                                if (DateTime.Compare(hourStart, m_person.time) <= 0 && DateTime.Compare(hourStop, m_person.time) > 0)
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
        public ItemCountWithAge showPlotLevelV2(DateTime time)
        {
            DateTime begin = new DateTime(time.Year, time.Month, time.Day, 00, 00, 00);
            DateTime end = begin.AddDays(1);
            List<DataRaw> datas = getRawData(begin, end);
            return calcGetCountPersonLevel(time, datas);
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
        public ItemInfoPlotForAge showPlotLevelForDatesV2(DateTime begin, DateTime end)
        {
            DateTime dateBegin = new DateTime(begin.Year, begin.Month, begin.Day, 00, 00, 00);
            DateTime dateEnd = new DateTime(end.Year, end.Month, end.Day, 00, 00, 00);
            dateEnd = dateEnd.AddDays(1);
            List<DataRaw> datas = getRawData(dateBegin, dateEnd);

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
                    DateTime time = DateTime.ParseExact(m_time, "dd-MM-yyyy", null);

                    ItemCountWithAge item = calcGetCountPersonLevel(time, datas);
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
        public class ItemGenderPerson
        {
            public string person { get; set; } = "";
            public string gender { get; set; } = "";
            public int count { get; set; } = 0;

        }
        public class ItemBufferForGender
        {
            public string person { get; set; } = "";
            public string gender { get; set; } = "";
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
        public ItemCountWithGender calcGetCountPersonGender(DateTime time, List<DataRaw> datas)
        {
            DateTime begin = new DateTime(time.Year, time.Month, time.Day, 00, 00, 00);
            DateTime end = begin.AddDays(1);
            using (DataContext context = new DataContext())
            {
                ItemCountWithGender tmp = new ItemCountWithGender();

                tmp.genders.Add("0");
                tmp.genders.Add("1");
                tmp.genders.Add("2");
                List<ItemGenderPerson> mCodes = new List<ItemGenderPerson>();
                List<ItemGenderPerson> mPersons = new List<ItemGenderPerson>();
                List<ItemBufferForGender> buffers = new List<ItemBufferForGender>();
                List<DataRaw> faces = datas.Where(s => DateTime.Compare(begin, s.createdTime) <= 0 && DateTime.Compare(end, s.createdTime) > 0).ToList();
                if (faces.Count > 0)
                {
                    foreach (DataRaw face in faces)
                    {
                        ItemBufferForGender itemBuffer = new ItemBufferForGender();

                        if (face.person != null)
                        {
                            itemBuffer.person = face.person.code;

                            ItemGenderPerson? person = mCodes.Where(s => s.person.CompareTo(face.person.code) == 0 && s.gender.CompareTo(face.gender) == 0).FirstOrDefault();

                            if (person == null)
                            {
                                ItemGenderPerson tmpPerson = new ItemGenderPerson();

                                tmpPerson.person = face.person.code;
                                tmpPerson.gender = face.gender;
                                tmpPerson.count = 1;
                                mCodes.Add(tmpPerson);
                            }
                            else
                            {
                                person.count += 1;
                            }

                        }

                        itemBuffer.gender = face.gender;
                        itemBuffer.time = face.createdTime;

                        buffers.Add(itemBuffer);

                    }
                }
                mPersons = mCodes.OrderBy(s => s.person).ThenByDescending(s => s.gender).ToList();
                List<int> counts = new List<int> { 0, 0, 0 };

                foreach (ItemGenderPerson m_person in mPersons)
                {
                    List<ItemGenderPerson> tmpPersons = mPersons.Where(s => s.person.CompareTo(m_person.person) == 0).ToList();
                    if (tmpPersons.Count > 1)
                    {

                        foreach (string m_gender in tmp.genders)
                        {
                            int index = tmp.genders.IndexOf(m_gender);
                            if (m_person.gender.CompareTo(m_gender) == 0)
                            {
                                counts[index] = m_person.count;

                            }
                        }

                        if (tmpPersons[tmpPersons.Count - 1].gender.CompareTo(m_person.gender) == 0)
                        {
                            int num = counts.Max();
                            int indexNum = counts.IndexOf(num);
                            string temp = tmp.genders[indexNum];


                            foreach (ItemBufferForGender itemBuffer in buffers)
                            {
                                if (itemBuffer.person.CompareTo(m_person.person) == 0 && itemBuffer.gender.CompareTo(temp) != 0)
                                {
                                    //Console.WriteLine("Truoc: " + itemBuffer.gender);
                                    itemBuffer.gender = temp;
                                    //Console.WriteLine("Sau: " + itemBuffer.gender);
                                }
                            }
                            ItemGenderPerson? tmpGender = tmpPersons.Where(s => s.gender.CompareTo(temp) != 0).FirstOrDefault();
                            if (tmpGender != null)
                            {
                                int index = mCodes.FindIndex(s => s.person.CompareTo(tmpGender.person) == 0);
                                mCodes.Remove(mCodes[index]);
                            }
                            counts = new List<int> { 0, 0, 0 };
                        }

                    }

                }



                buffers = buffers.OrderBy(s => s.person).ThenByDescending(s => s.gender).ToList();
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

                            List<ItemBufferForGender>? mGenders = buffers.Where(s => s.gender.CompareTo(gender) == 0).ToList();
                            foreach (ItemBufferForGender m_gender in mGenders)
                            {
                                if (DateTime.Compare(hourStart.ToUniversalTime(), m_gender.time) <= 0 && DateTime.Compare(hourStop.ToUniversalTime(), m_gender.time) > 0)
                                {
                                    string? m_person = persons.Where(s => s.CompareTo(m_gender.person) == 0).FirstOrDefault();
                                    if (m_person == null)
                                    {

                                        persons.Add(m_gender.person);
                                    }
                                }


                            }
                            hour.number.Add(persons.Count);
                        }
                        data.data.Add(hour);
                    }
                } while (DateTime.Compare(hourTime, end) < 0);
                foreach (string m_item in tmp.genders)
                {
                    List<ItemGenderPerson> mCounts = mCodes.Where(s => s.gender.CompareTo(m_item) == 0).OrderBy(s => s.gender).ToList();

                    data.totalCount.Add(mCounts.Count);
                }
                tmp.item = data;
                return tmp;
            }
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
                List<ItemGenderPerson> mCodes = new List<ItemGenderPerson>();
                List<ItemGenderPerson> mPersons = new List<ItemGenderPerson>();
                List<ItemBufferForGender> buffers = new List<ItemBufferForGender>();
                List<SqlFace>? faces = context.faces!.Where(s => s.isdeleted == false && (DateTime.Compare(begin.ToUniversalTime(), s.createdTime) <= 0 && DateTime.Compare(end.ToUniversalTime(), s.createdTime) > 0)).Include(s => s.person).ToList();
                if (faces.Count > 0)
                {
                    foreach (SqlFace face in faces)
                    {
                        ItemBufferForGender itemBuffer = new ItemBufferForGender();

                        if (face.person != null)
                        {
                            itemBuffer.person = face.person.code;

                            ItemGenderPerson? person = mCodes.Where(s => s.person.CompareTo(face.person.code) == 0 && s.gender.CompareTo(face.gender) == 0).FirstOrDefault();

                            if (person == null)
                            {
                                ItemGenderPerson tmpPerson = new ItemGenderPerson();

                                tmpPerson.person = face.person.code;
                                tmpPerson.gender = face.gender;
                                tmpPerson.count = 1;
                                mCodes.Add(tmpPerson);
                            }
                            else
                            {
                                person.count += 1; 
                            }

                        }

                        itemBuffer.gender = face.gender;
                        itemBuffer.time = face.createdTime;

                        buffers.Add(itemBuffer);

                    }
                }
                mPersons = mCodes.OrderBy(s => s.person).ThenByDescending(s => s.gender).ToList();
                List<int> counts = new List<int> { 0, 0, 0};

                foreach (ItemGenderPerson m_person in mPersons)
                {
                    List<ItemGenderPerson> tmpPersons = mPersons.Where(s => s.person.CompareTo(m_person.person) == 0).ToList();
                    if(tmpPersons.Count > 1)
                    {

                        foreach (string m_gender in tmp.genders)
                        {
                            int index = tmp.genders.IndexOf(m_gender);
                            if (m_person.gender.CompareTo(m_gender) == 0)
                            {
                                counts[index] = m_person.count;

                            }
                        }
                        
                        if (tmpPersons[tmpPersons.Count - 1].gender.CompareTo(m_person.gender) == 0)
                        {
                            int num = counts.Max();
                            int indexNum = counts.IndexOf(num);
                            string temp = tmp.genders[indexNum];
                            
                            
                            foreach (ItemBufferForGender itemBuffer in buffers)
                            {
                                if (itemBuffer.person.CompareTo(m_person.person) == 0 && itemBuffer.gender.CompareTo(temp) != 0)
                                {
                                    //Console.WriteLine("Truoc: " + itemBuffer.gender);
                                    itemBuffer.gender = temp;
                                    //Console.WriteLine("Sau: " + itemBuffer.gender);
                                }
                            }
                            ItemGenderPerson? tmpGender = tmpPersons.Where(s => s.gender.CompareTo(temp) != 0).FirstOrDefault();
                            if (tmpGender != null)
                            {
                                int index = mCodes.FindIndex(s => s.person.CompareTo(tmpGender.person) == 0);
                                mCodes.Remove(mCodes[index]);
                            }
                            counts = new List<int> { 0, 0, 0 };
                        }
                        
                    }
                   
                }

                

                buffers = buffers.OrderBy(s => s.person).ThenByDescending(s => s.gender).ToList();
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

                            List<ItemBufferForGender>? mGenders = buffers.Where(s => s.gender.CompareTo(gender) == 0).ToList();
                            foreach (ItemBufferForGender m_gender in mGenders)
                            {
                                if (DateTime.Compare(hourStart.ToUniversalTime(), m_gender.time) <= 0 && DateTime.Compare(hourStop.ToUniversalTime(), m_gender.time) > 0)
                                {
                                    string? m_person = persons.Where(s => s.CompareTo(m_gender.person) == 0).FirstOrDefault();
                                    if (m_person == null)
                                    {

                                        persons.Add(m_gender.person);
                                    }
                                }
                                

                            }
                            hour.number.Add(persons.Count);
                        }
                        data.data.Add(hour);
                    }
                } while (DateTime.Compare(hourTime, end) < 0);
                foreach (string m_item in tmp.genders)
                { 
                    List<ItemGenderPerson> mCounts = mCodes.Where(s => s.gender.CompareTo(m_item) == 0).OrderBy(s => s.gender).ToList();
                   
                    data.totalCount.Add(mCounts.Count);
                }
                tmp.item = data;
                return tmp;
            }
        }
        public ItemCountWithGender showPlotGenderV2(DateTime time)
        {
            DateTime begin = new DateTime(time.Year, time.Month, time.Day, 00, 00, 00);
            DateTime end = begin.AddDays(1);
            List<DataRaw> datas = getRawData(begin, end);
            return calcGetCountPersonGender(time, datas);
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
        public ItemInfoPlotForGender showPlotGenderForDatesV2(DateTime begin, DateTime end)
        {
            DateTime dateBegin = new DateTime(begin.Year, begin.Month, begin.Day, 00, 00, 00);
            DateTime dateEnd = new DateTime(end.Year, end.Month, end.Day, 00, 00, 00);
            dateEnd = dateEnd.AddDays(1);
            List<DataRaw> datas = getRawData(dateBegin, dateEnd);

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
                    DateTime time = DateTime.ParseExact(m_time, "dd-MM-yyyy", null);

                    ItemCountWithGender item = calcGetCountPersonGender(time, datas);
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
