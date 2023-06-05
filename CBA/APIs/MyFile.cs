using CBA.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using RestSharp;
using Serilog;
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

        /* public async Task<byte[]>? getImageChanged(byte[] data)
         {
             var client = new RestClient("http://office.stvg.vn:59073/image");
             var request = new RestRequest();
             request.Method = Method.Post;
             request.AddHeader("Content-Type", "multipart/form-data");
             request.AddFile("file", data, String.Format("{0}.jpg", DateTime.Now.Ticks));
             request.Timeout = 10000;
             RestResponse response = await client.ExecuteAsync(request);

             if (response.StatusCode == System.Net.HttpStatusCode.OK)
             {
                 return response.RawBytes;
             }
             else
             {
                 return null;
             }
         }*/
    }
}
