using CBA.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Diagnostics;
using OfficeOpenXml;
using System.Xml.Linq;
using static CBA.APIs.MyFace;
using System.Drawing;
using static CBA.APIs.MyPerson;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Logical;

namespace CBA.APIs
{
    public class MyPerson
    {
        ///////////////////////////////////////////////////////////////////////////////////////////
        public class CacheHistoryPerson
        {
            public long id { get; set; } = 0;
            public DateTime begin { get; set; }
            public DateTime end { get; set; }
            public List<ItemLogPersons> data { get; set; } = new List<ItemLogPersons>();
            public DateTime create { get; set; }
        }
        public List<CacheHistoryPerson> cacheHistoryPersons = new List<CacheHistoryPerson>();


        public class CacheListAllPerson
        {
            public long id { get; set; } = 0;
            public DateTime begin { get; set; }
            public DateTime end { get; set; }
            public List<ItemPerson> data { get; set; } = new List<ItemPerson>();
            public DateTime create { get; set; }
        }
        public List<CacheListAllPerson> cacheListAllPersons = new List<CacheListAllPerson>();

        public class ItemDataExcel
        {
            public string date { get; set; } = "";
            public List<ItemLogPersons> data { get; set; } = new List<ItemLogPersons>();
        }
        public class CacheExcel
        {
            public long id { get; set; } = 0;
            public DateTime begin { get; set; }
            public DateTime end { get; set; }
            public List<ItemDataExcel> items { get; set; } = new List<ItemDataExcel>();
            public DateTime create { get; set; }
        }
        List<CacheExcel> cacheExcels = new List<CacheExcel>();
        ///////////////////////////////////////////////////////////////////////////////////////////
        public MyPerson()
        {
            Thread t = new Thread(() =>
            {
                while (true)
                {
                    Thread.Sleep(1000);
                    for (int i = 0; i < cacheHistoryPersons.Count; i++)
                    {
                        TimeSpan time = DateTime.Now.Subtract(cacheHistoryPersons[i].create);
                        if (time.Minutes > 5.0)
                        {
                            cacheHistoryPersons.RemoveAt(i);
                            i--;
                        }
                    }

                    Thread.Sleep(1000);
                    for (int i = 0; i < cacheListAllPersons.Count; i++)
                    {
                        TimeSpan time = DateTime.Now.Subtract(cacheListAllPersons[i].create);
                        if (time.Minutes > 5.0)
                        {
                            cacheListAllPersons.RemoveAt(i);
                            i--;
                        }
                    }
                    Thread.Sleep(1000);
                    for (int i = 0; i < cacheExcels.Count; i++)
                    {
                        TimeSpan time = DateTime.Now.Subtract(cacheExcels[i].create);
                        if (time.Minutes > 5.0)
                        {
                            cacheExcels.RemoveAt(i);
                            i--;
                        }
                    }
                }
            });
            t.Start();
        }                    
        ///////////////////////////////////////////////////////////////////////////////////////////
        public async Task<bool> editPerson(string code, string name, string des, string codeSystem)
        {
            if (string.IsNullOrEmpty(codeSystem))
            {
                return false;
            }
            using (DataContext context = new DataContext())
            {

                SqlPerson? m_person = context.persons!.Where(s => s.isdeleted == false && s.codeSystem.CompareTo(codeSystem) == 0).FirstOrDefault();
                if (m_person == null)
                {
                    return false;
                }

                if (!string.IsNullOrEmpty(code))
                {
                    if (m_person.code.CompareTo(code) != 0)
                    {
                        m_person.code = code;
                    }
                }
                if (!string.IsNullOrEmpty(name))
                {
                    m_person.name = name;
                }
                m_person.des = des;

                int rows = await context.SaveChangesAsync();
                return true;
            }
        }

        public class ItemGroupForPerson
        {
            public string code { get; set; } = "";
            public string name { get; set; } = "";
            public string des { get; set; } = "";
        }

        public class ItemFaceForPerson
        {
            public string image { get; set; } = "";
            public string time { get; set; } = "";
            public string device { get; set; } = "";


        }

        public class ItemAgeLevelForPerson
        {
            public string code { get; set; } = "";
            public string name { get; set; } = "";

        }

        public class ItemDeviceForFace
        {
            public string code { get; set; } = "";
            public string name { get; set; } = "";
            public string des { get; set; } = "";
        }

        public class ItemBufferListPerSon
        {
            public SqlPerson? person { get; set; } = new SqlPerson();
            public string image { get; set; } = "";
            public List<ItemFaceForPerson> faces { get; set; } = new List<ItemFaceForPerson>();

        }
        public class ItemPerson
        {
            public string code { get; set; } = "";
            public string codeSystem { get; set; } = "";
            public string name { get; set; } = "";
            public string gender { get; set; } = "";
            public int age { get; set; } = 0;
            public string image { get; set; } = "";
            public ItemGroupForPerson group { get; set; } = new ItemGroupForPerson();
            public List<ItemFaceForPerson> faces { get; set; } = new List<ItemFaceForPerson>();
            public ItemAgeLevelForPerson level { get; set; } = new ItemAgeLevelForPerson();
            public string createdTime { get; set; } = "";
            public string lastestTime { get; set; } = "";

        }
        public class ListPersonPage
        {
            public int total { get; set; } = 0;
            public int page { get; set; } = 0;
            public List<ItemPerson> items { get; set; } = new List<ItemPerson>();
        }

        public ListPersonPage getListPerson(int index, int total)
        {
            DateTime time = DateTime.Now;
            DateTime begin = new DateTime(time.Year, time.Month, time.Day, 00, 00, 00);
            DateTime end = begin.AddDays(1);

            CacheListAllPerson? cache = cacheListAllPersons.Where(s => s.begin.CompareTo(begin) == 0 && s.end.CompareTo(end) == 0).FirstOrDefault();
            if (cache != null)
            {
                Console.WriteLine("Cache !!!");
                List<ItemPerson> items = new List<ItemPerson>();
                items.AddRange(cache.data);
                ListPersonPage info = new ListPersonPage();
                info.page = index;
                info.total = items.Count;
                if (index + total < items.Count)
                {
                    items.RemoveRange(0, index);
                    items.RemoveRange(total, items.Count - total);
                }
                else
                {
                    items.RemoveRange(0, index);
                }
                info.items = items;
                return info;
            }
            else
            {
                using (DataContext context = new DataContext())
                {

                    ListPersonPage info = new ListPersonPage();
                    info.page = index;
                    List<ItemPerson> items = new List<ItemPerson>();

                    List<SqlPerson> persons = context.persons!.Where(s => s.isdeleted == false)
                                                              .Include(s => s.faces!).ThenInclude(s => s.device)
                                                              .Include(s => s.level)
                                                              .Include(s => s.group).Include(s => s.faces!).ThenInclude(s => s.device)
                                                              .OrderByDescending(s => s.lastestTime)
                                                              .ToList();

                   
                    if (persons.Count > 0)
                    {
                        List<ItemBufferListPerSon> m_buffer = new List<ItemBufferListPerSon>();
                        foreach(SqlPerson m_person in persons)
                        {
                            ItemBufferListPerSon m_tmp = new ItemBufferListPerSon();
                            m_tmp.person = m_person;
                            if(m_person.faces != null)
                            {
                                List<SqlFace>? sqlfaces = m_person.faces!.Where(s => s.isdeleted == false).OrderByDescending(s => s.createdTime).ToList();
                                if(sqlfaces.Count > 0)
                                {
                                    m_tmp.image = sqlfaces[sqlfaces.Count - 1].image;
                                    foreach(SqlFace m_face in sqlfaces)
                                    {
                                        ItemFaceForPerson m_item = new ItemFaceForPerson();
                                        m_item.image = m_face.image;
                                        m_item.time = m_face.createdTime.ToLocalTime().ToString("dd-MM-yyyy HH:mm:ss");
                                        if(m_face.device != null)
                                        {
                                            m_item.device = m_face.device!.code;
                                        }
                                        m_tmp.faces.Add(m_item);
                                    }    
                                }

                            }    
                            m_buffer.Add(m_tmp);
                        }
                        foreach (ItemBufferListPerSon tmp in m_buffer)
                        {
                            ItemPerson item = new ItemPerson();
                            item.code = tmp.person!.code;
                            item.codeSystem = tmp.person!.codeSystem;
                            item.name = tmp.person!.name;
                            item.gender = tmp.person!.gender;
                            item.age = tmp.person!.age;
                            item.image = tmp.image;
                            if (tmp.person!.group != null)
                            {
                                item.group.code = tmp.person!.group!.code;
                                item.group.name = tmp.person!.group!.name;
                                item.group.des = tmp.person!.group!.des;
                            }
                            item.faces = tmp.faces;
                            if (tmp.person!.level != null)
                            {
                                item.level.code = tmp.person!.level!.code;
                                item.level.name = tmp.person!.level!.name;
                            }
                            else
                            {
                                item.level.code = "NA";
                                item.level.name = "Out Range Ages";
                            }
                            item.createdTime = tmp.person!.createdTime.ToLocalTime().ToString("dd-MM-yyyy HH:mm:ss");
                            item.lastestTime = tmp.person!.lastestTime.ToLocalTime().ToString("dd-MM-yyyy HH:mm:ss");
                            items.Add(item);
                        }


                        //
                        CacheListAllPerson t_item = new CacheListAllPerson();
                        t_item.id = DateTime.Now.Ticks;
                        t_item.begin = begin;
                        t_item.end = end;
                        t_item.data = new List<ItemPerson>();
                        t_item.data.AddRange(items);
                        t_item.create = DateTime.Now;
                        cacheListAllPersons.Add(t_item);
                        //
                        info.page = index;
                        info.total = items.Count;
                        if (index + total < persons.Count)
                        {
                            items.RemoveRange(0, index);
                            items.RemoveRange(total, items.Count - total);
                        }
                        else
                        {
                            items.RemoveRange(0, index);
                        }

                        info.items = items;


                    }
                    
                    return info;

                }
            }
            
        }
        public class ItemBufferPerSon
        {
            public string codeSystem { get; set; } = "";
            public string code { get; set; } = "";
            public string name { get; set; } = "";
            public string gender { get; set; } = "";
            public int age { get; set; } = 0;
            public ItemGroupForPerson group { get; set; } = new ItemGroupForPerson();
            public ItemAgeLevelForPerson level { get; set; } = new ItemAgeLevelForPerson();
            public DateTime createdTime { get; set; }
        }
        public class ItemBufferImage
        {
            public string image { get; set; } = "";
            public string time { get; set; } = "";
            public string device { get; set; } = "";
        }
        
        public class ItemLogPersons
        {
            public string code { get; set; } = "";
            public string codeSystem { get; set; } = "";
            public string name { get; set; } = "";
            public string gender { get; set; } = "";
            public int age { get; set; } = 0;
            public string image { get; set; } = "";
            public ItemGroupForPerson group { get; set; } = new ItemGroupForPerson();
            public List<ItemFaceForPerson> faces { get; set; } = new List<ItemFaceForPerson>();
            public ItemAgeLevelForPerson level { get; set; } = new ItemAgeLevelForPerson();
            public string date { get; set; } = "";
            public string createdTime { get; set; } = "";
            public string lastestTime { get; set; } = "";
        }

        public class ListInfoLogsPage
        {
            public int total { get; set; } = 0;
            public int page { get; set; } = 0;
            public List<ItemLogPersons> items { get; set; } = new List<ItemLogPersons>();

        }

        public ListInfoLogsPage getListPersonHistory(DateTime begin, DateTime end, int index, int number)
        {
            CacheHistoryPerson? cache = cacheHistoryPersons.Where(s => s.begin.CompareTo(begin) == 0 && s.end.CompareTo(end) == 0).FirstOrDefault();
            if (cache != null)
            {
                List<ItemLogPersons> items = new List<ItemLogPersons>();
                items.AddRange(cache.data);
                ListInfoLogsPage info = new ListInfoLogsPage();
                info.page = index;
                info.total = items.Count;
                if (index + number < items.Count)
                {
                    items.RemoveRange(0, index);
                    items.RemoveRange(number, items.Count - number);
                }
                else
                {
                    items.RemoveRange(0, index);
                }
                info.items = items;
                return info;
            }
            else
            {
                using (DataContext context = new DataContext())
                {
                    //Stopwatch sw = new Stopwatch();
                    //sw.Start();

                    DateTime dateBegin = new DateTime(begin.Year, begin.Month, begin.Day, 00, 00, 00);
                    DateTime dateEnd = new DateTime(end.Year, end.Month, end.Day, 00, 00, 00);
                    dateEnd = dateEnd.AddDays(1);

                    ListInfoLogsPage info = new ListInfoLogsPage();
                    List<ItemLogPersons> items = new List<ItemLogPersons>();
;

                    List<SqlLogPerson> logs = context.logs!.Where(s => DateTime.Compare(dateBegin.ToUniversalTime(), s.time) <= 0 && DateTime.Compare(dateEnd.ToUniversalTime(), s.time) > 0)
                                                           .Include(s => s.person!).ThenInclude(s => s.group)
                                                           .Include(s => s.person!).ThenInclude(s => s.faces!).ThenInclude(s => s.device)
                                                           .Include(s => s.person!).ThenInclude(s => s.level)
                                                           .Include(s => s.device)
                                                           .OrderByDescending(s => s.time)
                                                           .ToList();
                    
                    foreach (SqlLogPerson m_log in logs)
                    {
                        
                        if (m_log.person != null)
                        {
                            ItemLogPersons? m_person = items.Where(s => s.codeSystem.CompareTo(m_log.person.codeSystem) == 0 && s.date.CompareTo(m_log.time.ToLocalTime().ToString("dd-MM-yyyy")) == 0).FirstOrDefault();

                            if(m_person == null)
                            {
                                ItemLogPersons item = new ItemLogPersons();
                                item.date = m_log.time.ToLocalTime().ToString("dd-MM-yyyy");
                                item.codeSystem = m_log.person.codeSystem;
                                item.code = m_log.person.code;
                                item.name = m_log.person.name;
                                item.gender = m_log.person.gender;
                                item.age = m_log.person.age;

                                if(m_log.person.faces != null)
                                {
                                    List<SqlFace> tmp = m_log.person.faces.OrderByDescending(s => s.createdTime).ToList();
                                    item.image = m_log.person.faces[tmp.Count - 1].image;
                                   
                                }

                                ItemFaceForPerson tmpFace = new ItemFaceForPerson();
                                tmpFace.image = m_log.image;
                                tmpFace.time = m_log.time.ToLocalTime().ToString("dd-MM-yyyy HH:mm:ss");
                                tmpFace.device = m_log.device!.code;

                                item.faces.Add(tmpFace);


                                if (m_log.person.group != null)
                                {
                                    ItemGroupForPerson itemGroup = new ItemGroupForPerson();
                                    itemGroup.name = m_log.person.group.name;
                                    itemGroup.code = m_log.person.group.code;
                                    itemGroup.des = m_log.person.group.des;

                                    item.group = itemGroup;
                                }

                                if (m_log.person.level != null)
                                {
                                    ItemAgeLevelForPerson itemLevel = new ItemAgeLevelForPerson();

                                    itemLevel.code = m_log.person.level.code;
                                    itemLevel.name = m_log.person.level.name;

                                    item.level = itemLevel;
                                }

                                item.createdTime = m_log.person.createdTime.ToLocalTime().ToString("dd-MM-yyyy HH:mm:ss");
                                item.lastestTime = m_log.person.lastestTime.ToLocalTime().ToString("dd-MM-yyyy HH:mm:ss");
                                
                                items.Add(item);

                            }
                            else
                            {
                                ItemFaceForPerson tmpFace = new ItemFaceForPerson();
                                tmpFace.image = m_log.image;
                                tmpFace.time = m_log.time.ToLocalTime().ToString("dd-MM-yyyy HH:mm:ss");
                                tmpFace.device = m_log.device!.code;

                                m_person.faces.Add(tmpFace);
                            }
                        }


                    }

                   
                    //foreach(ItemLogPersons mItem in myItems)
                    //{
                    //    Console.WriteLine(string.Format("code {0} - Time : {1}", mItem.code, mItem.lastestTime));
                    //}    
                    //CacheHistoryPerson
                    //

                    CacheHistoryPerson t_item = new CacheHistoryPerson();
                    t_item.id = DateTime.Now.Ticks;
                    t_item.begin = begin;
                    t_item.end = end;
                    t_item.data = new List<ItemLogPersons>();
                    t_item.data.AddRange(items);
                    t_item.create = DateTime.Now;
                    cacheHistoryPersons.Add(t_item);
                    //

                    //CacheExcel
                    //

                    CacheExcel? cachExcel = cacheExcels.Where(s => s.begin.CompareTo(begin) == 0 && s.end.CompareTo(end) == 0).FirstOrDefault();
                    if(cachExcel == null)
                    {
                        CacheExcel ex_item = new CacheExcel();
                        ex_item.id = DateTime.Now.Ticks;
                        ex_item.begin = begin;
                        ex_item.end = end;
                        ex_item.items = new List<ItemDataExcel>();
                        do
                        {
                            ItemDataExcel m_excel = new ItemDataExcel();
                            m_excel.date = begin.ToLocalTime().ToString("dd-MM-yyyy");
                            m_excel.data = items.Where(s => s.date.CompareTo(begin.ToLocalTime().ToString("dd-MM-yyyy")) == 0).ToList();
                            ex_item.items.Add(m_excel);
                            begin = begin.AddDays(1);
                        }while(DateTime.Compare(begin, dateEnd) < 0);

                        ex_item.create = DateTime.Now;
                        cacheExcels.Add(ex_item);
                    }
                   

                    ///

                    info.page = index;
                    info.total = items.Count;
                    if (index + number < items.Count)
                    {
                        items.RemoveRange(0, index);
                        items.RemoveRange(number, items.Count - number);
                    }
                    else
                    {
                        items.RemoveRange(0, index);
                    }
                    info.items = items;

                    return info;
                }
            }
        }

        public MemoryStream exportExcel(DateTime begin, DateTime end, int total)
        {
            MemoryStream stream = new MemoryStream();
            using (var xlPackage = new ExcelPackage(stream))
            {
                ExcelWorksheet worksheet = xlPackage.Workbook.Worksheets.Add("Report");
                var customStyle = xlPackage.Workbook.Styles.CreateNamedStyle("CustomStyle");
                customStyle.Style.Font.UnderLine = true;
                customStyle.Style.Font.Color.SetColor(Color.Red);

                

                int startRow = 1;
                int row = startRow;
                if(begin.CompareTo(end) == 0)
                {
                    worksheet.Cells[row, 1].Value = string.Format("BÁO CÁO NGÀY {0} ", begin.ToString("dd-MM-yyyy"));
                }  
                else
                {
                    worksheet.Cells[row, 1].Value = string.Format("BÁO CÁO NGÀY {0} -  {1} ", begin.ToString("dd-MM-yyyy"), end.ToString("dd-MM-yyyy"));
                }    
                using (ExcelRange r = worksheet.Cells[row, 1, row, 6])
                {
                    r.Merge = true;
                    r.Style.Font.Color.SetColor(Color.Yellow);
                    r.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    r.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(23, 55, 93));
                }
                row++;

                worksheet.Cells[row, 1].Value = "Tên người";
                worksheet.Cells[row, 2].Value = "Giới tính";
                worksheet.Cells[row, 3].Value = "Nhóm";
                worksheet.Cells[row, 4].Value = "Số lần phát hiện";
                worksheet.Cells[row, 5].Value = "Thời gian tạo";
                worksheet.Cells[row, 6].Value = "Thời gian cập nhật";
                worksheet.Cells[row, 1, row, 6].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells[row, 1, row, 6].Style.Fill.BackgroundColor.SetColor(Color.Yellow);
                row++;

                List<ItemDataExcel> items = new List<ItemDataExcel>();
                CacheExcel? cache = cacheExcels.Where(s => s.begin.CompareTo(begin) == 0 && s.end.CompareTo(end) == 0).FirstOrDefault();
                if (cache != null)
                {
                    items = cache.items;
                }
                else
                {
                    ListInfoLogsPage tmp = getListPersonHistory(begin, end, 1, total);

                    items = cacheExcels.Where(s => s.begin.CompareTo(begin) == 0 && s.end.CompareTo(end) == 0).FirstOrDefault()!.items;
                }

                foreach (ItemDataExcel item in items)
                {
                    worksheet.Cells[row, 1].Value = item.date;
                    using (ExcelRange r = worksheet.Cells[row, 1, row, 6])
                    {
                        r.Merge = true;
                        r.Style.Font.Color.SetColor(Color.White);
                        r.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        r.Style.Fill.BackgroundColor.SetColor(Color.Green);
                    }
                    row++;
                    foreach(ItemLogPersons m_data in item.data)
                    {
                        worksheet.Cells[row, 1].Value = string.IsNullOrEmpty(m_data.name) ? "unknown" : m_data.name;
                        worksheet.Cells[row, 2].Value = m_data.gender;
                        worksheet.Cells[row, 3].Value = m_data.group.name;
                        worksheet.Cells[row, 4].Value = m_data.faces.Count;
                        worksheet.Cells[row, 5].Value = m_data.createdTime;
                        worksheet.Cells[row, 6].Value = m_data.lastestTime;
                        row++;
                    }
                    using (ExcelRange r = worksheet.Cells[row, 1, row, 6])
                    {
                        r.Merge = true;
                        r.Style.Font.Color.SetColor(Color.Green);
                        r.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        r.Style.Fill.BackgroundColor.SetColor(Color.Black);
                    }
                    row++;
                }
                row--;
                using (ExcelRange r = worksheet.Cells[1, 1, row, 6])
                {
                    //r.Style.ShrinkToFit = true;

                    r.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    r.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                    r.Style.WrapText = false;
                    r.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    r.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    r.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    r.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                }

                xlPackage.Workbook.Properties.Title = "GIGAMALL Report";
                xlPackage.Workbook.Properties.Author = "GIGAMALL";

                xlPackage.Save();
            }
            stream.Position = 0;
            return stream;
        }
    }
}
