using CBA.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Logical;
using RestSharp;
using Serilog;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Security.Cryptography.Pkcs;
using static CBA.APIs.MyReport;
using static CBA.Program;

namespace CBA.APIs
{
    public class MyFile
    {
        public MyFile()
        {
        }

        private string createKey(string file)
        {
            string key = file + "|" + DateTime.Now.Ticks.ToString();
            return String.Concat(key.Select(x => ((int)x).ToString("x")));
        }

        public async Task<string> saveFileAsync(string file, byte[] data)
        {
            using (DataContext context = new DataContext())
            {
                string error_ID = "";
                try
                {
                    string code = createKey(file);
                    string link_file = "/Data/" + code + ".file";
                    try
                    {
                        await File.WriteAllBytesAsync(link_file, data);
                    }
                    catch (Exception ex)
                    {
                        code = "";
                    }
                    if (string.IsNullOrEmpty(code))
                    {
                        return code;
                    }

                    SqlFile m_file = new SqlFile();
                    m_file.ID = DateTime.Now.Ticks;
                    m_file.key = code;
                    m_file.link = link_file;
                    m_file.name = file;
                    m_file.time = DateTime.Now.ToUniversalTime();
                    context.files!.Add(m_file);

                    error_ID = m_file.ID.ToString();

                    int rows = await context.SaveChangesAsync();
                    if (rows > 0)
                    {
                        return code;
                    }
                    else
                    {
                        return "";
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(error_ID);
                    Log.Error(e.ToString());
                    return "";
                }
            }

        }

        public byte[]? readFile(string code)
        {
            using (DataContext context = new DataContext())
            {
                List<SqlFile> files = context.files!.Where(s => s.key.CompareTo(code) == 0).ToList();
                if (files.Count <= 0)
                {
                    return null;
                }
                byte[] data = File.ReadAllBytes(files[0].link);
                return data;
            }
        }

        public string getFileConfig()
        {
            try
            {
                string filePath = string.Format("Configs/configSql.json");
                string data = File.ReadAllText(filePath);
                return data;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return "";
            }
        }

        public bool createConfig(string m_file)
        {
            string path = "./Configs";
            string fileName = m_file + ".json";
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                string link = Path.Combine(path, fileName);
                ItemHost tmp = new ItemHost();

                string data = "Host=192.100.1.11:5432;Database=db_stvg_cba;Username=postgres;Password=stvg";
                tmp.host.Add(data);

                //data = "Host=office.stvg.vn:59061;Database=db_stvg_cba;Username=postgres;Password=stvg";
                //tmp.host.Add(data);
                string file = JsonConvert.SerializeObject(tmp);
                File.WriteAllText(link, file);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        //public class ItemTarger
        //{
        //    public string target { get; set; } = "";
        //    public string date { get; set; } = "";
        //}

        public void initCreateTargetFile()
        {
            try
            {
                string path = "Targets";
                List<string> tmps = new List<string>();
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);

                    string link = Path.Combine(path, "mytarget.txt");

                    string item = "24-06-2023 : 2000";
                    tmps.Add(item);

                    string data = JsonConvert.SerializeObject(tmps);

                    File.WriteAllText(link, data);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }

        public string getFileTarget(string month, DateTime time)
        {
            try
            {
                string m_time = time.ToLocalTime().ToString("dd-MM-yyyy");

                string filePath = string.Format("Targets/mytarget.txt");
                string data = File.ReadAllText(filePath);
                List<string>? items = JsonConvert.DeserializeObject<List<string>>(data);
                string m_data = "";
                if(items != null)
                {
                    foreach (string item in items)
                    {
                        string[] c = item.Split(":");
                        if (c[0].Trim().CompareTo(m_time) == 0)
                        {
                            m_data = c[1].Trim();
                            break;
                        }
                    }
                    return m_data;
                }
                else
                {
                    return "";
                }    
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                return "";
            }
        }
    }
}
