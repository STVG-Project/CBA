using CBA.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;
using static CBA.APIs.MyGroup;

namespace CBA.APIs
{
    public class MyReport
    {
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
            public List<int> totalCount { get; set; } = new List<int>();
        }

        /*public class ItemCountPerson
        {
            public List<string> groups { get; set; } = new List<string>();
            public List<ItemInHours> items { get; set; } = new List<ItemInHours>();
        }*/



        public ItemCountsPlot getCountHour(DateTime time)
        {

            using (DataContext context = new DataContext())
            {
                DateTime start = new DateTime(time.Year, time.Month, time.Day, 00, 00, 00);
                DateTime end = start.AddDays(1);
                List<SqlLogPerson>? logs = context.logs!.Where(s => DateTime.Compare(start.ToUniversalTime(), s.time) <= 0 && DateTime.Compare(end.ToUniversalTime(), s.time) > 0).Include(s => s.person!).ThenInclude(s => s.group).ToList();
                if (logs.Count < 1)
                {
                    return new ItemCountsPlot();
                }
                   
                ItemCountsPlot itemCount = new ItemCountsPlot();
                itemCount.date = time.ToString("dd-MM-yyyy");
                DateTime timeEnd = end.ToUniversalTime();
                List<string> code_groups = new List<string>();
                try
                {
                    foreach (SqlLogPerson m_log in logs)
                    {
                        if (m_log.person != null)
                        {
                            if (m_log.person.group != null)
                            {
                                string? tmpGroup = itemCount.groups!.Where(s => s.CompareTo(m_log.person.group.name) == 0).FirstOrDefault();
                                if (string.IsNullOrEmpty(tmpGroup))
                                {
                                    itemCount.groups.Add(m_log.person.group.name);
                                    code_groups.Add(m_log.person.group.code);
                                }
                            }
                            else
                            {
                                if (itemCount.groups.Count < 1)
                                {
                                    itemCount.groups.Add("");
                                    code_groups.Add("");
                                }
                            }
                        }
                    }
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
                            Console.WriteLine(string.Format("Time start : {0} ---- Time End : {1}",hourBegin, hourEnd));
                            
                            foreach (string group in code_groups)
                            {
                                int index = 0;
                                if (group.CompareTo("") == 0)
                                {
                                    List<SqlLogPerson>? m_logs = logs.Where(s => s.person!.group == null).OrderByDescending(s => s.time).ToList();
                                    if(m_logs.Count > 0)
                                    {
                                        foreach (SqlLogPerson m_log in m_logs)
                                        {
                                            if (DateTime.Compare(hourBegin, m_log.time) <= 0 && DateTime.Compare(hourEnd, m_log.time) > 0)
                                            {
                                                //Console.WriteLine(m_log.person!.code);
                                                //Console.WriteLine(string.Format(" hour : {0} - person : {1} - time : {2} - Group : {3}", item.hour, index, m_log.time, group));
                                                //Thread.Sleep(1);
                                                index++;
                                            }
                                        }
                                        item.number.Add(index);
                                    }    
                                    
                                }
                                else
                                {

                                    SqlGroup? tmpGroup = context.groups!.Where(s => s.code.CompareTo(group) == 0).FirstOrDefault();
                                    if(tmpGroup != null)
                                    {
                                        List<SqlLogPerson>? m_logs = logs.Where(s => s.person!.group == tmpGroup).OrderByDescending(s => s.time).ToList();
                                        if (m_logs.Count > 0)
                                        {
                                            foreach (SqlLogPerson m_log in m_logs)
                                            {
                                                if (DateTime.Compare(hourBegin, m_log.time) <= 0 && DateTime.Compare(hourEnd, m_log.time) > 0)
                                                {
                                                    //Console.WriteLine(m_log.person!.code);
                                                    //Console.WriteLine(string.Format(" hour : {0} - person : {1} - time : {2} - Group : {3}", item.hour, index, m_log.time, group));
                                                    //Thread.Sleep(1);
                                                    index++;
                                                }
                                            }
                                            item.number.Add(index);
                                        }
                                    }    
                                        
                                }
                            }
                            itemCount.data.Add(item);
                            
                            //Console.WriteLine(i);
                            //Console.WriteLine("Time up : " + time);
                            //Thread.Sleep(1);
                            
                        }
                        for (int i = 0; i < code_groups.Count; i++)
                        {
                            int totalcount = 0;
                            foreach (ItemCountHours tmp_count in itemCount.data)
                            {
                                totalcount += tmp_count.number[i];
                            }
                            itemCount.totalCount.Add(totalcount);
                        }

                    }
                    while (DateTime.Compare(time, timeEnd) < 0);
                    
                    return itemCount;
                    
                    
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return new ItemCountsPlot();
                }

            }
        }

        public List<ItemCountsPlot> getCountDate(DateTime begin, DateTime end)
        {
            List<ItemCountsPlot> itemCounts = new List<ItemCountsPlot>();

            using (DataContext context = new DataContext())
            {
                List<SqlLogPerson>? logs = context.logs!.Where(s => DateTime.Compare(begin.ToUniversalTime(), s.time) <= 0 && DateTime.Compare(end.ToUniversalTime(), s.time) > 0).ToList();
                //.Where(s => DateTime.Compare(time_begin.ToUniversalTime(), s.time) <= 0 && DateTime.Compare(time_end.ToUniversalTime(), s.time) >= 0)
                if (logs.Count > 0)
                {
                    foreach (SqlLogPerson log in logs)
                    {
                        DateTime time = log.time.ToLocalTime();
                        Console.WriteLine(time.Date);
                        ItemCountsPlot? tmp = itemCounts.Where(s => s.date.CompareTo(time.Date.ToString("dd-MM-yyyy")) == 0).FirstOrDefault();
                        if (tmp == null)
                        {
                            ItemCountsPlot item = getCountHour(time.Date);
                            itemCounts.Add(item);
                        }    
                    }
                }
                return itemCounts;
            }
        }
    }
}
