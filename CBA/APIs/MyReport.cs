using CBA.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using Npgsql;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Logical;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using Serilog;
using System;
using System.Drawing;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using static CBA.APIs.MyDevice;
using static CBA.APIs.MyGroup;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
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
            //public long ID { get; set; }
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
            //public int low { get; set; } = int.MinValue;
            // public int high { get; set; } = int.MaxValue;
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
                foreach (SqlFace face in faces)
                {
                    DataRaw item = new DataRaw();
                    item.ID = face.ID;
                    item.age = face.age;
                    item.gender = face.gender;
                    item.createdTime = face.createdTime.ToLocalTime();
                    item.image = face.image;
                    if (face.person != null)
                    {
                        if (face.person.isdeleted == false)
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
                            if (face.person.group != null)
                            {
                                if (face.person.group.isdeleted == false)
                                {
                                    // item.person.group.ID = face.person.group.ID;
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
                                    //item.person.level.low = face.person.level.low;
                                    //item.person.level.high = face.person.level.high;
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

        

        /// <summary>
        /// 
        /// </summary>
        /// 



        /// <summary>
        /// ///////////////
        /// </summary>
        /// 



        public class ItemMonthPerson
        {
            public string time { get; set; } = "";
            public int person { get; set; } = 0;
        }

        public class ItemCountPersonGroup
        {
            public string group { get; set; } = "";
            public List<ItemMonthPerson> data { get; set; } = new List<ItemMonthPerson>();
        }

        public ItemCountPersonGroup getStatisticsCountPersonForYear(DateTime time, string group)
        {
            ItemCountPersonGroup item = new ItemCountPersonGroup();
            DateTime begin = new DateTime(time.Year, 1, 1, 0, 0, 0);
            DateTime end = begin.AddYears(1);

            List<DataRaw> raws = getRawData(begin, end);
            List<DataRaw> datas = raws.OrderBy(s => s.person.group.code).ThenBy(s => s.person.codeSystem).ToList();

            if (group.CompareTo("") == 0)
            {
                datas = datas.Where(s => s.person.group.code.CompareTo("") == 0).ToList();
            }
            else if (group.CompareTo("1") == 0)
            {
                datas = datas.Where(s => s.person.group.code.CompareTo("") != 0).ToList();
            }
            
            for (int i = 0; i < 12; i++)
            {
                ItemMonthPerson itemPerson = new ItemMonthPerson();

                itemPerson.time = (i + 1).ToString();
                DateTime monthStart = begin.AddMonths(i);
                DateTime monthStop = monthStart.AddMonths(1);
                List<DataRaw> tmp_datas = datas.Where(s => DateTime.Compare(monthStart, s.createdTime) <= 0 && DateTime.Compare(monthStop, s.createdTime) > 0).ToList();
                while (tmp_datas.Count > 0)
                {
                    itemPerson.person = 0;
                    string codePerson = tmp_datas[0].person.codeSystem;
                    itemPerson.person++;

                    for (int j = 0; j < tmp_datas.Count; j++)
                    {
                        if (tmp_datas[j].person.codeSystem.CompareTo(codePerson) != 0)
                        {
                            codePerson = tmp_datas[j].person.codeSystem;
                            itemPerson.person++;
                        }
                        tmp_datas.RemoveAt(0);
                        j--;
                    }
                }
                item.data.Add(itemPerson);
            }

            return item;
        }

        public ItemCountPersonGroup getStatisticsCountPersonForMonth(DateTime time, string group)
        {
            ItemCountPersonGroup item = new ItemCountPersonGroup();
            DateTime begin = new DateTime(time.Year, time.Month, 1, 0, 0, 0);
            DateTime end = begin.AddMonths(1);

            List<DataRaw> raws = getRawData(begin, end);
            List<DataRaw> datas = raws.OrderBy(s => s.person.group.code).ThenBy(s => s.person.codeSystem).ToList();

            if (group.CompareTo("") == 0)
            {
                datas = datas.Where(s => s.person.group.code.CompareTo("") == 0).ToList();
            }
            else if(group.CompareTo("1") == 0)
            {
                datas = datas.Where(s => s.person.group.code.CompareTo("") != 0).ToList();

            }
            
            for (int i = 0; i < DateTime.DaysInMonth(begin.Year, begin.Month); i++)
            {
                ItemMonthPerson itemPerson = new ItemMonthPerson();

                itemPerson.time = (i + 1).ToString();
                DateTime dayStart = begin.AddDays(i);
                DateTime dayStop = dayStart.AddDays(1);

                List<DataRaw> tmp_datas = datas.Where(s => DateTime.Compare(dayStart, s.createdTime) <= 0 && DateTime.Compare(dayStop, s.createdTime) > 0).ToList();
                while (tmp_datas.Count > 0)
                {
                    itemPerson.person = 0;
                    string codePerson = tmp_datas[0].person.codeSystem;
                    itemPerson.person++;
                    for (int j = 0; j < tmp_datas.Count; j++)
                    {
                        if (tmp_datas[j].person.codeSystem.CompareTo(codePerson) != 0)
                        {
                            codePerson = tmp_datas[j].person.codeSystem;

                            itemPerson.person++;
                        }
                      
                        tmp_datas.RemoveAt(0);
                        j--;
                    }
                }
                item.data.Add(itemPerson);
            }
            return item;
        }


        public class ItemDetailMonthPerson
        {
            public string month { get; set; } = "";
            public float totalCustomerPerMonth { get; set; } = 0f;
            public float averageOfWeekday { get; set; } = 0f;
            public float averageOfWeekend { get; set; } = 0f;
        }

        public class ItemComparePersonForGroup
        {

            public float compareMonth { get; set; } = 0f;
            public float compareDay { get; set; } = 0f;
            public float compareWeekend { get; set; } = 0f;
        }
        public string getDetailStatisticsForYear(DateTime time)
        {
            List<ItemDetailMonthPerson> item = new List<ItemDetailMonthPerson>();
            DateTime begin = new DateTime(time.Year, 1, 1, 0, 0, 0);
            DateTime end = begin.AddYears(1);

            List<DataRaw> raws = getRawData(begin, end);
            List<DataRaw> datas = raws.OrderBy(s => s.person.group.code).ThenBy(s => s.person.codeSystem).Where(s => s.person.group.code == "").ToList();

            for (int i = 0; i < 12; i++)
            {
                float countDay = 0;
                float countLastDay = 0;
                ItemDetailMonthPerson itemPerson = new ItemDetailMonthPerson();

                itemPerson.month = (i + 1).ToString();
                DateTime monthStart = begin.AddMonths(i);
                DateTime monthStop = monthStart.AddMonths(1);

                List<DataRaw> tmp_datas = datas.Where(s => DateTime.Compare(monthStart, s.createdTime) <= 0 && DateTime.Compare(monthStop, s.createdTime) > 0).ToList();
                while (tmp_datas.Count > 0)
                {
                    itemPerson.totalCustomerPerMonth = 0;
                    itemPerson.averageOfWeekday = 0;
                    itemPerson.averageOfWeekend = 0;
                    string codePerson = tmp_datas[0].person.codeSystem;
                    if (!string.IsNullOrEmpty(codePerson))
                    {
                        itemPerson.totalCustomerPerMonth++;
                    }
                    for (int j = 0; j < tmp_datas.Count; j++)
                    {
                        if (tmp_datas[j].person.codeSystem.CompareTo(codePerson) != 0)
                        {
                            codePerson = tmp_datas[j].person.codeSystem;

                            itemPerson.totalCustomerPerMonth++;
                            if (tmp_datas[j].createdTime.DayOfWeek == DayOfWeek.Monday || tmp_datas[j].createdTime.DayOfWeek == DayOfWeek.Tuesday || tmp_datas[j].createdTime.DayOfWeek == DayOfWeek.Wednesday || tmp_datas[j].createdTime.DayOfWeek == DayOfWeek.Thursday || tmp_datas[j].createdTime.DayOfWeek == DayOfWeek.Friday)
                            {
                                countDay++;
                            }
                            else
                            {
                                countLastDay++;
                            }

                        }

                        tmp_datas.RemoveAt(0);
                        j--;
                    }
                    itemPerson.averageOfWeekday = countDay / 5;
                    itemPerson.averageOfWeekend = countLastDay / 2;
                    countDay = 0;
                    countLastDay = 0;
                }
                item.Add(itemPerson);
            }

            string result = JsonConvert.SerializeObject(item);
            return result;

        }

        public ItemComparePersonForGroup getComparePersonMOM(DateTime begin, DateTime end)
        {
            ItemComparePersonForGroup item = new ItemComparePersonForGroup();

            List<ItemDetailMonthPerson>? list = JsonConvert.DeserializeObject<List<ItemDetailMonthPerson>>(getDetailStatisticsForYear(begin));

            if (list != null)
            {
                List<ItemDetailMonthPerson>? m_list = list.Where(s => s.month.CompareTo(begin.Month.ToString()) == 0 || s.month.CompareTo(end.Month.ToString()) == 0).ToList();

                if (m_list.Count > 0)
                {
                    item.compareMonth = (m_list[m_list.Count - 1].totalCustomerPerMonth - m_list[0].totalCustomerPerMonth) / m_list[0].totalCustomerPerMonth * 100;
                    item.compareDay = (m_list[m_list.Count - 1].averageOfWeekday - m_list[0].averageOfWeekday) / m_list[0].averageOfWeekday * 100;
                    item.compareWeekend = (m_list[m_list.Count - 1].averageOfWeekend - m_list[0].averageOfWeekend) / m_list[0].averageOfWeekend * 100;
                }

            }

            return item;
        }

        /// <summary>
        /// ////////
        /// </summary>
        /// <returns></returns>
        /// 

        public class ItemPersonForDevice
        {
            public string codeDevice { get; set; } = "";
            public int number { get; set; } = 0;
            public int count { get; set; } = 0;
        }

        public class ItemReportPersonForDevice
        {
            public string time { get; set; } = "";
            public List<int> number { get; set; } = new List<int>();
            public List<int> count { get; set; } = new List<int>();
        }
        public class ReportPersonForDevice
        {
            public List<string> devices { get; set; } = new List<string>();

            public List<ItemReportPersonForDevice> datas = new List<ItemReportPersonForDevice>();

        }

        public class ItemHourPersonDevice
        {
            public string hour { get; set; } = "";
            public List<ItemPersonForDevice> persons { get; set; } = new List<ItemPersonForDevice>();

        }

        public class ItemCountPersonDevice
        {
            public List<string> devices { get; set; } = new List<string>();
            public List<ItemHourPersonDevice> data { get; set; } = new List<ItemHourPersonDevice>();
            public List<ItemPersonForDevice> totalCount { get; set; } = new List<ItemPersonForDevice>();
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


        public ItemInfoPlot getStatisticsCountPersonForDevice(DateTime time)
        {
            ItemInfoPlot item = new ItemInfoPlot();
            DateTime begin = new DateTime(time.Year, time.Month, time.Day, 0, 0, 0);
            DateTime end = begin.AddDays(1);

            item.item.date = time.ToString("dd-MM-yyyy");

            List<DataRaw> raws = getRawData(begin, end);
            List<DataRaw> datas = raws.OrderBy(s => s.device.code).ThenBy(s => s.person.codeSystem).ToList();
            
            List<string> codes = new List<string>();
            using (DataContext context = new DataContext())
            {
                List<SqlDevice>? m_devices = context.devices!.Where(s => s.isdeleted == false).ToList();
                foreach (SqlDevice tmpDevice in m_devices)
                {
                    codes.Add(tmpDevice.code);
                    item.devices.Add(tmpDevice.name);
                }
            }
            for (int i = 0; i < 24; i++)
            {
                ItemCountWithDevice itemPerson = new ItemCountWithDevice();

                itemPerson.hour = i.ToString();
                DateTime hourStart = begin.AddHours(i);
                DateTime hourStop = hourStart.AddHours(1);

                ItemPersonForDevice tmp = new ItemPersonForDevice();

                List<DataRaw> tmp_datas = datas.Where(s => DateTime.Compare(hourStart, s.createdTime) <= 0 && DateTime.Compare(hourStop, s.createdTime) > 0).ToList();
                if(tmp_datas.Count < 1)
                {
                    foreach(string m_code in codes)
                    {
                        itemPerson.number.Add(tmp.number);
                    }
                }    
                while (tmp_datas.Count > 0)
                {
                    string codePerson = tmp_datas[0].person.codeSystem;
                    string codeDevice = tmp_datas[0].device.code;
                    foreach (string m_code in codes) 
                    {   
                        if(codeDevice.CompareTo(m_code) == 0)
                        {
                            tmp.number++;
                        }
                        else
                        {
                            tmp.number = 0;
                        }    

                        tmp.count = 0;
                        tmp.codeDevice = m_code;
                        
                        for (int j = 0; j < tmp_datas.Count; j++)
                        {
                            if (tmp.codeDevice.CompareTo(tmp_datas[j].device.code) == 0)
                            {
                                if (tmp_datas[j].person.codeSystem.CompareTo(codePerson) == 0)
                                {
                                    tmp.count++;
                                }
                                else
                                {
                                    codePerson = tmp_datas[j].person.codeSystem;
                                    tmp.number++;
                                    tmp.count++;
                                }
                                tmp_datas.RemoveAt(0);
                                j--;
                            }
                            else
                            {
                                break;
                            }
                        }
                        itemPerson.number.Add(tmp.number);
                    }
                }
                item.item.data.Add(itemPerson);
            }
            while (datas.Count > 0)
            {
                string codePerson = datas[0].person.codeSystem;
                string codeDevice = datas[0].device.code;
                foreach (string m_code in codes)
                {
                    ItemPersonForDevice tmp_device = new ItemPersonForDevice();
                    tmp_device.codeDevice = m_code;
                    if (codeDevice.CompareTo(m_code) == 0)
                    {
                        tmp_device.number++;
                    }
                    else
                    {
                        tmp_device.number = 0;
                    }
                    tmp_device.count = 0;

                    for (int a = 0; a < datas.Count; a++)
                    {
                        if (tmp_device.codeDevice.CompareTo(datas[a].device.code) == 0)
                        {
                            if (datas[a].person.codeSystem.CompareTo(codePerson) == 0)
                            {
                                tmp_device.count++;
                            }
                            else
                            {
                                codePerson = datas[a].person.codeSystem;
                                tmp_device.number++;
                                tmp_device.count++;
                            }
                            datas.RemoveAt(0);
                            a--;
                        }
                        else
                        {
                            break;
                        }
                    }
                    item.item.totalCount.Add(tmp_device.number);
                }    
            }
            return item;
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

        public ItemInfoPlotForDates getStatisticsPersonForDevice(DateTime begin, DateTime end)
        {
            DateTime _begin = new DateTime(begin.Year, begin.Month, begin.Day, 00, 00, 00);
            DateTime _end = new DateTime(end.Year, end.Month, end.Day, 00, 00, 00);
            _end = _end.AddDays(1.0);
            List<DataRaw> raws = getRawData(_begin, _end);

            ItemInfoPlotForDates report = new ItemInfoPlotForDates();
            List<long> idDevices = new List<long>();

            using (DataContext context = new DataContext())
            {
                List<SqlDevice> devices = context.devices!.Where(s => s.isdeleted == false).ToList();
                foreach (SqlDevice device in devices)
                {
                    idDevices.Add(device.ID);
                    report.devices.Add(device.name);
                }
            }
            //Console.WriteLine("getStatisticsPersonForDevice");
            while (DateTime.Compare(_begin, _end) < 0)
            {
                DateTime _tmp = _begin.AddDays(1.0);
                List<DataRaw> buffer = raws.Where(s => s.createdTime.CompareTo(_begin) >= 0 && s.createdTime.CompareTo(_tmp) < 0).ToList();
                buffer = buffer.OrderBy(s => s.device.ID).ThenBy(s => s.person.ID).ToList();
                ItemTotalCountsWithDevice data = new ItemTotalCountsWithDevice();
                data.date = _begin.ToString("dd-MM-yyyy");
                foreach (long tmp in idDevices)
                {
                    data.totalCount.Add(0);
                }

                while (buffer.Count > 0)
                {
                    long idDevice = buffer[0].device.ID;
                    long idPerson = buffer[0].person.ID;
                    
                    int index = -1;
                    for (int i = 0; i < idDevices.Count; i++)
                    {
                        if (idDevice == idDevices[i])
                        {
                            index = i;
                            break;
                        }
                    }

                    if(idDevice == buffer[0].device.ID)
                    {
                        data.totalCount[index]++;
                    }

                    for (int i = 0; i < buffer.Count; i++)
                    {
                        if (idDevice == buffer[i].device.ID)
                        {
                            if (idPerson == buffer[i].person.ID)
                            {
                              
                            }
                            else
                            {
                                if (index >= 0)
                                {
                                    idPerson = buffer[i].person.ID;
                                    
                                    data.totalCount[index]++;
                                }
                            }
                            buffer.RemoveAt(0);
                            i--;
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                report.items.Add(data);
                _begin = _tmp;
            }
            report.totals = new List<int>();
            for (int i = 0; i < report.devices.Count; i++)
            {
                report.totals.Add(0);
                foreach (ItemTotalCountsWithDevice item in report.items)
                {
                    report.totals[i] += item.totalCount[i];
                }
            }
            return report;
        }


        /// <summary>
        /// ///////////
        /// </summary>
        /// 


        public class ItemHourPersonLevel
        {
            public string hour { get; set; } = "";
            public List<ItemPersonForLevel> persons { get; set; } = new List<ItemPersonForLevel>();

        }

        public class ItemCountPersonLevel
        {
            public List<string> levels { get; set; } = new List<string>();
            public List<ItemHourPersonLevel> data { get; set; } = new List<ItemHourPersonLevel>();
            public List<ItemPersonForLevel> totalCount { get; set; } = new List<ItemPersonForLevel>();
        }
        public class ItemCountWithLevel
        {
            public string hour { get; set; } = "";
            public List<int> number { get; set; } = new List<int>();
        }
        public class ItemCountPersonsWithLevel
        {
            public string date { get; set; } = "";
            public List<ItemCountWithLevel> data { get; set; } = new List<ItemCountWithLevel>();
            public List<int> totalCount { get; set; } = new List<int>();
        }
        public class ItemInfoPlotLevel
        {
            public List<string> levels { get; set; } = new List<string>();
            public ItemCountPersonsWithLevel item { get; set; } = new ItemCountPersonsWithLevel();
        }

        public ItemInfoPlotLevel getStatisticsCountPersonForLevel(DateTime time)
        {
            ItemInfoPlotLevel item = new ItemInfoPlotLevel();
            DateTime begin = new DateTime(time.Year, time.Month, time.Day, 0, 0, 0);
            DateTime end = begin.AddDays(1);

            item.item.date = time.ToString("dd-MM-yyyy");
            List<DataRaw> raws = getRawData(begin, end);
            List<DataRaw> datas = raws.OrderBy(s => s.person.level.code).ThenBy(s => s.person.codeSystem).ToList();

            List<string> codes = new List<string>();

            using (DataContext context = new DataContext())
            {
                List<SqlAgeLevel>? m_levels = context.ages!.Where(s => s.isdeleted == false).ToList();
                foreach (SqlAgeLevel tmpLevel in m_levels)
                {
                    codes.Add(tmpLevel.code);
                    item.levels.Add(tmpLevel.name);
                }
                codes.Add("");
                item.levels.Add("");
            }
            for (int i = 0; i < 24; i++)
            {
                ItemCountWithLevel itemPerson = new ItemCountWithLevel();

                itemPerson.hour = i.ToString();
                DateTime hourStart = begin.AddHours(i);
                DateTime hourStop = hourStart.AddHours(1);
                ItemPersonForLevel tmp = new ItemPersonForLevel();
                List<DataRaw> tmp_datas = datas.Where(s => DateTime.Compare(hourStart, s.createdTime) <= 0 && DateTime.Compare(hourStop, s.createdTime) > 0).ToList();

                if (tmp_datas.Count < 1)
                {
                    foreach (string m_code in codes)
                    {
                        itemPerson.number.Add(tmp.number);
                    }
                }
                while (tmp_datas.Count > 0)
                {
                    string codePerson = tmp_datas[0].person.codeSystem;
                    string codeLevel = tmp_datas[0].person.level.code;

                    foreach (string m_code in codes)
                    {
                        if (codeLevel.CompareTo(m_code) == 0)
                        {
                            tmp.number++;
                        }
                        else
                        {
                            tmp.number = 0;
                        }

                        tmp.count = 0;
                        tmp.codeLevel = m_code;

                        for (int j = 0; j < tmp_datas.Count; j++)
                        {
                            if (tmp.codeLevel.CompareTo(tmp_datas[j].person.level.code) == 0)
                            {
                                if (tmp_datas[j].person.codeSystem.CompareTo(codePerson) == 0)
                                {
                                    tmp.count++;
                                }
                                else
                                {
                                    codePerson = tmp_datas[j].person.codeSystem;
                                    tmp.number++;
                                    tmp.count++;
                                }
                                tmp_datas.RemoveAt(0);
                                j--;
                            }
                            else
                            {
                                break;
                            }
                        }
                        itemPerson.number.Add(tmp.number);
                    }
                }
                item.item.data.Add(itemPerson);
            }
            while (datas.Count > 0)
            {
                string codePerson = datas[0].person.codeSystem;
                string codeLevel = datas[0].person.level.code;
                foreach (string m_code in codes)
                {
                    ItemPersonForLevel tmp_level = new ItemPersonForLevel();
                    tmp_level.codeLevel = m_code;
                    if (codeLevel.CompareTo(m_code) == 0)
                    {
                        tmp_level.number++;
                    }
                    else
                    {
                        tmp_level.number = 0;
                    }
                    tmp_level.count = 0;

                    for (int a = 0; a < datas.Count; a++)
                    {
                        if (tmp_level.codeLevel.CompareTo(datas[a].person.level.code) == 0)
                        {
                            if (datas[a].person.codeSystem.CompareTo(codePerson) == 0)
                            {
                                tmp_level.count++;
                            }
                            else
                            {
                                codePerson = datas[a].person.codeSystem;
                                tmp_level.number++;
                                tmp_level.count++;
                            }
                            datas.RemoveAt(0);
                            a--;
                        }
                        else
                        {
                            break;
                        }
                    }
                    item.item.totalCount.Add(tmp_level.number);
                }
            }
            return item;
        }

        /// <summary>
        /// 
        /// </summary>

        public class ItemPersonForLevel
        {
            public string codeLevel { get; set; } = "";
            public string nameLevel { get; set; } = "";
            public int number { get; set; } = 0;
            public int count { get; set; } = 0;
        }
        public class ItemReportPersonForLevel
        {
            public string time { get; set; } = "";
            public List<int> number { get; set; } = new List<int>();
            public List<int> count { get; set; } = new List<int>();
        }
        public class ReportPersonForLevel
        {
            public List<string> levels { get; set; } = new List<string>();
            public List<ItemReportPersonForLevel> datas { get; set; } = new List<ItemReportPersonForLevel>();
        }

        /*  public List<ItemPersonForLevel> getStatisticsPersonForLevelDate(DateTime time)
          {

              DateTime begin = new DateTime(time.Year, time.Month, time.Day, 00, 00, 00);
              DateTime end = begin.AddDays(1.0);

              List<DataRaw> raws = getRawData(begin, end);

              List<DataRaw> datas = raws.OrderBy(s => s.person.level.code).ThenBy(s => s.person.codeSystem).ToList();
              List<ItemPersonForLevel> list = new List<ItemPersonForLevel>();
              while (datas.Count > 0)
              {
                  string tmp = datas[0].person.level.name;
                  ItemPersonForLevel item = new ItemPersonForLevel();
                  item.codeLevel = datas[0].person.level.code;
                  item.nameLevel = datas[0].person.level.name;
                  item.number = 0;
                  item.count = 0;
                  string codePerson = datas[0].person.codeSystem;

                  for (int i = 0; i < datas.Count; i++)
                  {
                      if (item.codeLevel.CompareTo(datas[i].person.level.code) == 0)
                      {
                          if (datas[i].person.codeSystem.CompareTo(codePerson) == 0)
                          {
                              item.count++;
                          }
                          else
                          {
                              codePerson = datas[i].person.codeSystem;
                              item.number++;
                              item.count++;
                          }
                          datas.RemoveAt(0);
                          i--;
                      }
                      else
                      {
                          break;
                      }
                  }
                  list.Add(item);
              }

              return list;
          }*/
        public class ItemTotalCountsWithLevel
        {
            public string date { get; set; } = "";
            public List<int> totalCount { get; set; } = new List<int>();
        }

        public class ItemInfoPlotLevelForDates
        {
            public List<string> levels { get; set; } = new List<string>();
            public List<ItemTotalCountsWithLevel> items { get; set; } = new List<ItemTotalCountsWithLevel>();
            public List<int> totals { get; set; } = new List<int>();
        }
        public ItemInfoPlotLevelForDates getStatisticsPersonForLevel(DateTime begin, DateTime end)
        {

            DateTime _begin = new DateTime(begin.Year, begin.Month, begin.Day, 00, 00, 00);
            DateTime _end = new DateTime(end.Year, end.Month, end.Day, 00, 00, 00);
            _end = _end.AddDays(1.0);
            List<DataRaw> raws = getRawData(_begin, _end);

            ItemInfoPlotLevelForDates report = new ItemInfoPlotLevelForDates();
            List<long> idLevels = new List<long>();

            using (DataContext context = new DataContext())
            {
                List<SqlAgeLevel> levels = context.ages!.Where(s => s.isdeleted == false).ToList();
                foreach (SqlAgeLevel level in levels)
                {
                    idLevels.Add(level.ID);
                    report.levels.Add(level.name);
                }
                report.levels.Add("");
                idLevels.Add(0);
            }

            while (DateTime.Compare(_begin, _end) < 0)
            {
                DateTime _tmp = _begin.AddDays(1.0);
                List<DataRaw> buffer = raws.Where(s => s.createdTime.CompareTo(_begin) >= 0 && s.createdTime.CompareTo(_tmp) < 0).ToList();
                buffer = buffer.OrderBy(s => s.person.level.ID).ThenBy(s => s.person.ID).ToList();
                ItemTotalCountsWithLevel data = new ItemTotalCountsWithLevel();
                data.date = _begin.ToString("dd-MM-yyyy");
                foreach (long tmp in idLevels)
                {
                    data.totalCount.Add(0);
                }

                while (buffer.Count > 0)
                {
                    long idLevel = buffer[0].person.level.ID;
                    long idPerson = buffer[0].person.ID;

                    int index = -1;
                    for (int i = 0; i < idLevels.Count; i++)
                    {
                        if (idLevel == idLevels[i])
                        {
                            index = i;
                            break;
                        }
                    }

                    if (idLevel == buffer[0].person.level.ID)
                    {
                        data.totalCount[index]++;
                    }

                    for (int i = 0; i < buffer.Count; i++)
                    {
                        if (idLevel == buffer[i].person.level.ID)
                        {
                            if (idPerson == buffer[i].person.ID)
                            {

                            }
                            else
                            {
                                if (index >= 0)
                                {
                                    idPerson = buffer[i].person.ID;

                                    data.totalCount[index]++;
                                }
                            }
                            buffer.RemoveAt(0);
                            i--;
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                report.items.Add(data);
                _begin = _tmp;
              
            }
            report.totals = new List<int>();
            for (int i = 0; i < report.levels.Count; i++)
            {
                report.totals.Add(0);
                foreach (ItemTotalCountsWithLevel item in report.items)
                {
                    report.totals[i] += item.totalCount[i];
                }
            }
            return report;
        }

        /// <summary>
        /// ////////
        /// </summary>
        /// 

        /*  public class ItemHourPersonGender
          {
              public string hour { get; set; } = "";
              public List<ItemPersonForGender> persons { get; set; } = new List<ItemPersonForGender>();

          }

          public class ItemCountPersonGender
          {
              public List<string> genders { get; set; } = new List<string>();
              public List<ItemHourPersonGender> data { get; set; } = new List<ItemHourPersonGender>();
              public List<ItemPersonForGender> totalCount { get; set; } = new List<ItemPersonForGender>();
          }*/

        public class ItemCountWithGenderV2
        {
            public string hour { get; set; } = "";
            public List<int> number { get; set; } = new List<int>();
        }
        public class ItemCountPersonsWithGender
        {
            public string date { get; set; } = "";
            public List<ItemCountWithGenderV2> data { get; set; } = new List<ItemCountWithGenderV2>();
            public List<int> totalCount { get; set; } = new List<int>();
        }
        public class ItemInfoPlotGender
        {
            public List<string> genders { get; set; } = new List<string>();
            public ItemCountPersonsWithGender item { get; set; } = new ItemCountPersonsWithGender();
        }

        public ItemInfoPlotGender getStatisticsCountPersonForGender(DateTime time)
        {
            ItemInfoPlotGender item = new ItemInfoPlotGender();
            DateTime begin = new DateTime(time.Year, time.Month, time.Day, 0, 0, 0);
            DateTime end = begin.AddDays(1);

            item.item.date = time.ToString("dd-MM-yyyy");
            List<DataRaw> raws = getRawData(begin, end);
            List<DataRaw> datas = raws.OrderBy(s => s.person.gender).ThenBy(s => s.person.codeSystem).ToList();

            List<string> codes = new List<string>();

            item.genders.Add("0");
            codes.Add("0");
            item.genders.Add("1");
            codes.Add("1");
            item.genders.Add("2");
            codes.Add("2");

            for (int i = 0; i < 24; i++)
            {
                ItemCountWithGenderV2 itemPerson = new ItemCountWithGenderV2();

                itemPerson.hour = i.ToString();
                DateTime hourStart = begin.AddHours(i);
                DateTime hourStop = hourStart.AddHours(1);
                ItemPersonForGender tmp = new ItemPersonForGender();
                List<DataRaw> tmp_datas = datas.Where(s => DateTime.Compare(hourStart, s.createdTime) <= 0 && DateTime.Compare(hourStop, s.createdTime) > 0).ToList();

                if (tmp_datas.Count < 1)
                {
                    foreach (string m_code in codes)
                    {
                        itemPerson.number.Add(tmp.number);
                    }
                }
                while (tmp_datas.Count > 0)
                {
                    string codePerson = tmp_datas[0].person.codeSystem;
                    string codeGender = tmp_datas[0].person.gender;

                    foreach (string m_code in codes)
                    {
                        if (codeGender.CompareTo(m_code) == 0)
                        {
                            tmp.number++;
                        }
                        else
                        {
                            tmp.number = 0;
                        }

                        tmp.count = 0;
                        tmp.gender = m_code;

                        for (int j = 0; j < tmp_datas.Count; j++)
                        {
                            if (tmp.gender.CompareTo(tmp_datas[j].person.gender) == 0)
                            {
                                if (tmp_datas[j].person.codeSystem.CompareTo(codePerson) == 0)
                                {
                                    tmp.count++;
                                }
                                else
                                {
                                    codePerson = tmp_datas[j].person.codeSystem;
                                    tmp.number++;
                                    tmp.count++;
                                }
                                tmp_datas.RemoveAt(0);
                                j--;
                            }
                            else
                            {
                                break;
                            }
                        }
                        itemPerson.number.Add(tmp.number);
                    }
                }
                item.item.data.Add(itemPerson);
            }
            while (datas.Count > 0)
            {
                string codePerson = datas[0].person.codeSystem;
                string codeGender = datas[0].person.gender;
                foreach (string m_code in codes)
                {
                    ItemPersonForGender tmp_gender = new ItemPersonForGender();
                    tmp_gender.gender = m_code;
                    if (codeGender.CompareTo(m_code) == 0)
                    {
                        tmp_gender.number++;
                    }
                    else
                    {
                        tmp_gender.number = 0;
                    }
                    tmp_gender.count = 0;

                    for (int a = 0; a < datas.Count; a++)
                    {
                        if (tmp_gender.gender.CompareTo(datas[a].person.gender) == 0)
                        {
                            if (datas[a].person.codeSystem.CompareTo(codePerson) == 0)
                            {
                                tmp_gender.count++;
                            }
                            else
                            {
                                codePerson = datas[a].person.codeSystem;
                                tmp_gender.number++;
                                tmp_gender.count++;
                            }
                            datas.RemoveAt(0);
                            a--;
                        }
                        else
                        {
                            break;
                        }
                    }
                    item.item.totalCount.Add(tmp_gender.number);
                }
            }
            return item;
        }



        /// <summary>
        /// ////////
        /// </summary>
        /// 
        public class ItemPersonForGender
        {
            public string gender { get; set; } = "";
            public int number { get; set; } = 0;
            public int count { get; set; } = 0;
        }
        /*  public class ItemReportPersonForGender
          {
              public string time { get; set; } = "";
              public List<int> number { get; set; } = new List<int>();
              public List<int> count { get; set; } = new List<int>();
          }
          public class ReportPersonForGender
          {
              public List<string> genders { get; set; } = new List<string>();
              public List<ItemReportPersonForGender> datas { get; set; } = new List<ItemReportPersonForGender>();
          }*/
        public class ItemTotalCountsWithGenderV2
        {
            public string date { get; set; } = "";
            public List<int> totalCount { get; set; } = new List<int>();
        }

        public class ItemInfoPlotGenderForDates
        {
            public List<string> genders { get; set; } = new List<string>();
            public List<ItemTotalCountsWithGenderV2> items { get; set; } = new List<ItemTotalCountsWithGenderV2>();
            public List<int> totals { get; set; } = new List<int>();
        }

        public List<ItemPersonForGender> getStatisticsPersonForGenderDate(DateTime time)
        {

            DateTime begin = new DateTime(time.Year, time.Month, time.Day, 00, 00, 00);
            DateTime end = begin.AddDays(1.0);

            List<DataRaw> raws = getRawData(begin, end);

            List<DataRaw> datas = raws.OrderBy(s => s.person.gender).ThenBy(s => s.person.codeSystem).ToList();
            List<ItemPersonForGender> list = new List<ItemPersonForGender>();
            while (datas.Count > 0)
            {
                string tmp = datas[0].person.gender;
                ItemPersonForGender item = new ItemPersonForGender();
                item.gender = datas[0].person.gender;
                item.number = 0;
                item.count = 0;
                string codePerson = datas[0].person.codeSystem;
                
                for (int i = 0; i < datas.Count; i++)
                {
                    if (item.gender.CompareTo(datas[i].person.gender) == 0)
                    {
                        if (datas[i].person.codeSystem.CompareTo(codePerson) == 0)
                        {
                            item.count++;
                        }
                        else
                        {
                            codePerson = datas[i].person.codeSystem;
                            item.number++;
                            item.count++;
                        }
                        datas.RemoveAt(0);
                        i--;
                    }
                    else
                    {
                        break;
                    }
                }
                list.Add(item);
            }

            return list;
        }
        public ItemInfoPlotGenderForDates getStatisticsPersonForGender(DateTime begin, DateTime end)
        {
            DateTime _begin = new DateTime(begin.Year, begin.Month, begin.Day, 00, 00, 00);
            DateTime _end = new DateTime(end.Year, end.Month, end.Day, 00, 00, 00);
            _end = _end.AddDays(1.0);
            List<DataRaw> raws = getRawData(_begin, _end);

            ItemInfoPlotGenderForDates report = new ItemInfoPlotGenderForDates();
            List<string> genders = new List<string>();

            report.genders.Add("0");
            report.genders.Add("1");
            report.genders.Add("2");
            genders.Add("0");
            genders.Add("1");
            genders.Add("2");

            while (DateTime.Compare(_begin, _end) < 0)
            {
                DateTime _tmp = _begin.AddDays(1.0);
                List<DataRaw> buffer = raws.Where(s => s.createdTime.CompareTo(_begin) >= 0 && s.createdTime.CompareTo(_tmp) < 0).ToList();
                buffer = buffer.OrderBy(s => s.person.gender).ThenBy(s => s.person.ID).ToList();
                ItemTotalCountsWithGenderV2 data = new ItemTotalCountsWithGenderV2();
                data.date = _begin.ToString("dd-MM-yyyy");
                foreach (string tmp in genders)
                {
                    data.totalCount.Add(0);
                }

                while (buffer.Count > 0)
                {
                    string gender = buffer[0].person.gender;
                    long idPerson = buffer[0].person.ID;

                    int index = -1;
                    for (int i = 0; i < genders.Count; i++)
                    {
                        if (gender == genders[i])
                        {
                            index = i;
                            break;
                        }
                    }

                    if (gender == buffer[0].person.gender)
                    {
                        data.totalCount[index]++;
                    }

                    for (int i = 0; i < buffer.Count; i++)
                    {
                        if (gender == buffer[i].person.gender)
                        {
                            if (idPerson == buffer[i].person.ID)
                            {

                            }
                            else
                            {
                                if (index >= 0)
                                {
                                    idPerson = buffer[i].person.ID;

                                    data.totalCount[index]++;
                                }
                            }
                            buffer.RemoveAt(0);
                            i--;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
               
                report.items.Add(data);
                _begin = _tmp;
            }

            report.totals = new List<int>();
            for (int i = 0; i < report.genders.Count; i++)
            {
                report.totals.Add(0);
                foreach (ItemTotalCountsWithGenderV2 item in report.items)
                {
                    report.totals[i] += item.totalCount[i];
                }
            }
            return report;
        }





        /// <summary>
        /// 
        /// </summary>
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
                        m_buffer.person = m_face.person.codeSystem;

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
                        m_buffer.person = m_face.person.codeSystem;

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

        /// <summary>
        /// Plot persons with Devices
        /// </summary>

        public class ItemBufferForDevice
        {
            public string person { get; set; } = "";

            public string device { get; set; } = "";

            public DateTime time { get; set; }
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
                        m_buffer.person = m_face.person.codeSystem;
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
                        m_buffer.person = m_face.person.codeSystem;
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
                            itemBuffer.person = itemFace.person.codeSystem;
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
                            itemBuffer.person = face.person.codeSystem;

                            ItemGenderPerson? person = mCodes.Where(s => s.person.CompareTo(face.person.codeSystem) == 0 && s.gender.CompareTo(face.gender) == 0).FirstOrDefault();

                            if (person == null)
                            {
                                ItemGenderPerson tmpPerson = new ItemGenderPerson();

                                tmpPerson.person = face.person.codeSystem;
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
                ItemDataWithGender data = new ItemDataWithGender();

                data.date = time.ToString("dd-MM-yyyy");

                for (int i = 0; i < 24; i++)
                {
                    ItemHours hour = new ItemHours();

                    hour.hour = i.ToString();

                    DateTime hourStart = begin.AddHours(i);
                    DateTime hourStop = hourStart.AddHours(1);

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

        //public class ItemPersonDetect
        //{
        //    public string code { get; set; } = "";
        //    public string codeSystem { get; set; } = "";
        //    public string name { get; set; } = "";
        //    public string gender { get; set; } = "";
        //    public int age { get; set; } = 0;
        //}
        //public class ItemDetect
        //{
        //    public ItemPersonDetect person { get; set; } = new ItemPersonDetect();
        //    public string image { get; set; } = "";
        //    public string time { get; set; } = "";
        //    public ItemDevice device { get; set; } = new ItemDevice();
        //}
        //public List<ItemDetect> detectBlackList(DateTime begin, string code)
        //{
        //    DateTime m_begin = new DateTime(begin.Year, begin.Month, begin.Day, begin.Hour, begin.Minute, begin.Second);
        //    DateTime m_end = begin.AddSeconds(10);
        //    List<DataRaw> raws = getRawData(m_begin, m_end);
        //    List<ItemDetect> list = new List<ItemDetect>();
        //    using (DataContext context = new DataContext())
        //    {
        //        /* SqlGroup? m_group = context.groups!.Where(s => s.isdeleted == false && s.code.CompareTo(code) == 0).FirstOrDefault();
        //         if(m_group == null)
        //         {
        //             return new List<ItemDetect>(); 
        //         }*/
        //        List<SqlLogPerson> datas = context.logs!.Include(s => s.person!).ThenInclude(s => s.group)
        //                                                .Where(s => DateTime.Compare(begin.ToUniversalTime(), s.time) <= 0
        //                                                            && DateTime.Compare(m_end.ToUniversalTime(), s.time) > 0)
        //                                                .Include(s => s.device).OrderByDescending(s => s.time)
        //                                                .ToList();
        //        if (datas.Count < 1)
        //        {
        //            return new List<ItemDetect>();
        //        }
        //        foreach (SqlLogPerson item in datas)
        //        {
        //            if (item.person!.group != null)
        //            {
        //                if (item.person.group.code.CompareTo(code) == 0)
        //                {
        //                    ItemDetect tmp = new ItemDetect();
        //                    tmp.person.code = item.person.code;
        //                    tmp.person.codeSystem = item.person.codeSystem;
        //                    tmp.person.name = item.person.name;
        //                    tmp.person.gender = item.person.gender;
        //                    tmp.person.age = item.person.age;

        //                    tmp.image = item.image;
        //                    tmp.time = item.time.ToLocalTime().ToString("dd-MM-yyyy HH:mm:ss");

        //                    if (item.device != null)
        //                    {
        //                        tmp.device.code = item.device.code;
        //                        tmp.device.name = item.device.name;
        //                        tmp.device.des = item.device.des;
        //                    }

        //                    list.Add(tmp);
        //                }
        //                else
        //                {
        //                    return new List<ItemDetect>();
        //                }
        //            }
        //        }
        //        return list;
        //    }
        //}

    }
}
