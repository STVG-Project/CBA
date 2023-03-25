using CBA.Models;

namespace CBA.APIs
{
    public class MyAgeLevel
    {
        public MyAgeLevel() { }

        public class ItemAge
        {
            public string code { get; set; } = "";
            public string name { get; set; } = "";
            public string des { get; set; } = "";
            public int low { get; set; } = 0;
            public int high { get; set; } = 0;

        }


        public List<ItemAge> getListAgeLevel()
        {
            using (DataContext context = new DataContext())
            {
                List<ItemAge> list = new List<ItemAge>();
                List<SqlAgeLevel> ages = context.ages!.Where(s => s.isdeleted == false).ToList();
                if (ages.Count > 0)
                {
                    foreach (SqlAgeLevel m_age in ages)
                    {
                        ItemAge item = new ItemAge();
                        item.code = m_age.code;
                        item.name = m_age.name;
                        item.des = m_age.des;
                        item.low = m_age.low;
                        item.high = m_age.high;

                        list.Add(item);
                    }
                }
                return list;
            }
        }


        public async Task initAsync()
        {
            using (DataContext context = new DataContext())
            {
                SqlAgeLevel? ages = context.ages!.Where(s => s.code.CompareTo("lv1") == 0 && s.isdeleted == false).FirstOrDefault();
                if (ages == null)
                {
                    SqlAgeLevel item = new SqlAgeLevel();
                    item.ID = DateTime.Now.Ticks;
                    item.code = "lv1";
                    item.name = "Level 1";
                    item.des = "Level 1";
                    item.low = 1;
                    item.high = 15;
                    item.isdeleted = false;

                    context.ages!.Add(item);
                }

                ages = context.ages!.Where(s => s.code.CompareTo("lv2") == 0 && s.isdeleted == false).FirstOrDefault();
                if (ages == null)
                {
                    SqlAgeLevel item = new SqlAgeLevel();
                    item.ID = DateTime.Now.Ticks;
                    item.code = "lv2";
                    item.name = "Level 2";
                    item.des = "Level 2";
                    item.low = 16;
                    item.high = 30;
                    item.isdeleted = false;

                    context.ages!.Add(item);
                }

                ages = context.ages!.Where(s => s.code.CompareTo("lv3") == 0 && s.isdeleted == false).FirstOrDefault();
                if (ages == null)
                {
                    SqlAgeLevel item = new SqlAgeLevel();
                    item.ID = DateTime.Now.Ticks;
                    item.code = "lv3";
                    item.name = "Level 3";
                    item.des = "Level 3";
                    item.low = 31;
                    item.high = 45;
                    item.isdeleted = false;

                    context.ages!.Add(item);
                }

                ages = context.ages!.Where(s => s.code.CompareTo("lv4") == 0 && s.isdeleted == false).FirstOrDefault();
                if (ages == null)
                {
                    SqlAgeLevel item = new SqlAgeLevel();
                    item.ID = DateTime.Now.Ticks;
                    item.code = "lv4";
                    item.name = "Level 4";
                    item.des = "Level 4";
                    item.low = 46;
                    item.high = 60;
                    item.isdeleted = false;

                    context.ages!.Add(item);
                }

                ages = context.ages!.Where(s => s.code.CompareTo("lv5") == 0 && s.isdeleted == false).FirstOrDefault();
                if (ages == null)
                {
                    SqlAgeLevel item = new SqlAgeLevel();
                    item.ID = DateTime.Now.Ticks;
                    item.code = "lv5";
                    item.name = "Level 5";
                    item.des = "Level 5";
                    item.low = 61;
                    item.high = 75;
                    item.isdeleted = false;

                    context.ages!.Add(item);
                }

                int rows = await context.SaveChangesAsync();
            }
        }
        public async Task<bool> createAgeLevel(string code, string name, string des, int low, int high)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(code))
            {
                return false;
            }
            using (DataContext context = new DataContext())
            {
                SqlAgeLevel? m_age = context.ages!.Where(s => s.isdeleted == false && s.code.CompareTo(code) == 0).FirstOrDefault();
                if (m_age != null)
                {
                    return false;
                }

                m_age = new SqlAgeLevel();
                m_age.ID = DateTime.Now.Ticks;
                m_age.code = code;
                m_age.name = name;
                m_age.des = des;
                m_age.low = low;
                m_age.high = high;
                m_age.isdeleted = false;

                context.ages!.Add(m_age);

                int rows = await context.SaveChangesAsync();
                if (rows > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public async Task<bool> editAgeLevel(string code, string name, string des, int low, int high)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(code) || string.IsNullOrEmpty(des))
            {
                return false;
            }
            using (DataContext context = new DataContext())
            {
                SqlAgeLevel? m_age = context.ages!.Where(s => s.isdeleted == false && s.code.CompareTo(code) == 0).FirstOrDefault();
                if (m_age == null)
                {
                    return false;
                }

                m_age.name = name;
                m_age.des = des;
                m_age.low = low;
                m_age.high = high;

                int rows = await context.SaveChangesAsync();
                if (rows > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public async Task<bool> deleteAgeLevel(string code)
        {
            using (DataContext context = new DataContext())
            {
                SqlAgeLevel? m_age = context.ages!.Where(s => s.isdeleted == false && s.code.CompareTo(code) == 0).FirstOrDefault();
                if (m_age == null)
                {
                    return false;
                }

                m_age.isdeleted = true;

                int rows = await context.SaveChangesAsync();
                if (rows > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}
