using CBA.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql.Replication.PgOutput.Messages;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using Serilog;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;
using static CBA.APIs.MyPerson;
using Log = Serilog.Log;

namespace CBA.APIs
{
    public class MyAgeLevel
    {
        public MyAgeLevel() { }

        public class CacheAges
        {
            public long id { get; set; } = 0;
            public DateTime begin { get; set; }
            public DateTime end { get; set; }
            public List<ItemCacheAge> data { get; set; } = new List<ItemCacheAge>();
            public DateTime create { get; set; }
        }
        List<CacheAges> cacheAges = new List<CacheAges>();

        public class ItemCacheAge
        {
            public string code { get; set; } = "";
            public int age { get; set; } = 0;
            public SqlAgeLevel? level { get; set; }
        }
        

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


        //public async Task initAsync()
        //{
        //    using (DataContext context = new DataContext())
        //    {
        //        SqlAgeLevel? ages = context.ages!.Where(s => s.code.CompareTo("lv1") == 0 && s.isdeleted == false).FirstOrDefault();
        //        if (ages == null)
        //        {
        //            SqlAgeLevel item = new SqlAgeLevel();
        //            item.ID = DateTime.Now.Ticks;
        //            item.code = "init";
        //            item.name = "Level Init";
        //            item.des = "Level Init";
        //            item.low = 0;
        //            item.high = 0;
        //            item.isdeleted = false;

        //            context.ages!.Add(item);
        //        }

        //        int rows = await context.SaveChangesAsync();
        //    }
        //}

        public bool checkAge(int num)
        {
            using (DataContext context = new DataContext())
            {
                SqlAgeLevel? age = context.ages!.Where(s => s.low <= num && s.high >= num && s.isdeleted == false).FirstOrDefault();
                if(age != null)
                {
                    return true;    
                }
                return false;
            }    
        }

        public bool clearCache()
        {
            if (Program.api_person.cacheHistoryPersons.Count > 0)
            {
                //Console.WriteLine(" Remove cache History");
                Program.api_person.cacheHistoryPersons.Clear();
                Console.WriteLine(" Remove cache History, Done !!!");
            }

            Thread.Sleep(1000);

            if (Program.api_person.cacheListAllPersons.Count > 0)
            {
                //Console.WriteLine(" Remove cache List Person");
                Program.api_person.cacheListAllPersons.Clear();

                Console.WriteLine(" Remove cache List Person, Done !!!");
            }
            return true;

        }

        public async Task<bool> checkAgeLevel()
        {
            using(DataContext context = new DataContext())
            {
                List<SqlPerson> sqlPersons = context.persons!.Where(s => s.isdeleted == false).Include(s => s.level).ToList();
                if (sqlPersons.Count > 0)
                {
                    List<SqlAgeLevel> ages = context.ages!.Where(s => s.isdeleted == false).ToList();
                    if (ages.Count > 0)
                    {
                        foreach (SqlAgeLevel tmp in ages)
                        {
                            //Console.WriteLine("Age Level sqlPerson !!!");
                            List<SqlPerson>? persons = sqlPersons.Where(s => tmp.low <= s.age && tmp.high >= s.age).ToList();
                            if (persons.Count > 0)
                            {
                                foreach (SqlPerson m_person in persons)
                                {
                                    m_person.level = tmp;
                                    sqlPersons.Remove(m_person);

                                }
                            }
                        }
                        if (sqlPersons.Count > 0)
                        {
                            //Console.WriteLine("Out Range Set Null !!!");
                            foreach (SqlPerson temp in sqlPersons)
                            {
                                SqlAgeLevel? tmp_age = ages.Where(s => s.low <= temp.age && s.high >= temp.age).FirstOrDefault();
                                if (tmp_age == null)
                                {
                                    temp.level = null;
                                }
                            }
                        }
                        int rows_age = await context.SaveChangesAsync();
                    }
                }
                return true;
            }    
        }
        public async Task<bool> createAgeLevel(string code, string name, string des, int low, int high)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(code))
            {
                return false;
            }
            bool flag = false;
            using (DataContext context = new DataContext())
            {
                SqlAgeLevel? m_age = context.ages!.Where(s => s.isdeleted == false && s.code.CompareTo(code) == 0).FirstOrDefault();
                if (m_age != null)
                {
                    return false;
                }

                if (checkAge(low) || checkAge(high))
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
                    flag = true;
                }    
                else
                {
                    flag =  false;
                }    
            }
            try
            {
                if(flag)
                {
                    if (clearCache())
                    {
                        flag = await checkAgeLevel();
                        if (flag)
                        {
                            return flag;
                        }
                        else
                        {
                            return false;
                        }    
                    }  
                }
                return flag;
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                return false;
            }
        }


        public async Task<bool> editAgeLevel(string code, string name, string des, int low, int high)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(code) || string.IsNullOrEmpty(des))
            {
                return false;
            }

            bool flag = false;
            using (DataContext context = new DataContext())
            {
                SqlAgeLevel? m_age = context.ages!.Where(s => s.isdeleted == false && s.code.CompareTo(code) == 0 ).FirstOrDefault();
                if (m_age == null)
                {
                    return false;
                }

                m_age.name = name;
                m_age.des = des;
                
                if(low < high)
                {
                    if (m_age.low > low)
                    {
                        if (!checkAge(low))
                        {
                            m_age.low = low;
                        }
                    }
                    else
                    {
                        m_age.low = low;
                    }
                    if (m_age.high < high)
                    {
                        if (!checkAge(high))
                        {
                            m_age.high = high;
                        }
                    }
                    else
                    {
                        m_age.high = high;
                    }
                
                }
                
                int rows = await context.SaveChangesAsync();
                flag = true;
            }
            try
            {
                if (flag)
                {
                    if (clearCache())
                    {
                        flag = await checkAgeLevel();
                        if (flag)
                        {
                            return flag;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                return flag;
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                return false;
            }
        }

        public async Task<bool> deleteAgeLevel(string code)
        {
            bool flag = false;
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
                    flag = true;
                }
                else
                {
                    flag = false;
                }
            }
            try
            {
                if (flag)
                {
                    if (clearCache())
                    {
                        flag = await checkAgeLevel();
                        if (flag)
                        {
                            return flag;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                return flag;
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                return false;
            }
        }
    }
}
