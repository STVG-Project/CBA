using CBA.Models;
using Microsoft.EntityFrameworkCore;

namespace CBA.APIs
{
    public class MyReport
    {
        public class InOutPerson
        {
            public string hour { get; set; } = "";
            public string number { get; set; } = "";
        }

        public class ItemInHours
        {
            public string date { get; set; } = "";
            public List<InOutPerson> data { get; set; } = new List<InOutPerson>();
        }

        public class ItemCount
        {
            public List<string> groups { get; set; } = new List<string>();
            public List<ItemInHours> items { get; set; } = new List<ItemInHours>();
        }



        public ItemCount showPlotCount(string begin, string end)
        {
            DateTime time_begin = DateTime.MinValue;
            try
            {
                time_begin = DateTime.ParseExact(begin, "dd-MM-yyyy", null);
            }
            catch (Exception e)
            {
                time_begin = DateTime.MinValue;
            }

            DateTime time_end = DateTime.MaxValue;
            try
            {
                time_end = DateTime.ParseExact(end, "dd-MM-yyyy", null);
            }
            catch (Exception e)
            {
                time_end = DateTime.MaxValue;
            }

            using (DataContext context = new DataContext())
            {

                List<SqlLogPerson>? m_logs = context.logs!.Include(s => s.device).Include(s => s.person).ThenInclude(s => s!.group).ToList();
             //.Where(s => DateTime.Compare(time_begin.ToUniversalTime(), s.time) <= 0 && DateTime.Compare(time_end.ToUniversalTime(), s.time) >= 0)
                if (m_logs == null)
                {
                    return new ItemCount();
                }
                ItemCount temp = new ItemCount();
                foreach (SqlLogPerson item in m_logs)
                {
                    if (item.person != null)
                    {
                        if(item.person.group != null)
                        {
                            temp.groups.Add(item.person.group.code);
                        }
                        else
                        {
                            temp.groups.Add("");
                        }
                        //ItemInHours itemInHours = new ItemInHours();
                    }
                }
                return temp;
                
            }
            

        }


    }
}
