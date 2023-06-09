﻿using CBA.Models;

namespace CBA.APIs
{
    public class MyDevice
    {
        public MyDevice() { }
        public async Task initAsync()
        {
            using (DataContext context = new DataContext())
            {
                SqlDevice? device = context.devices!.Where(s => s.code.CompareTo("dv1") == 0 && s.isdeleted == false).FirstOrDefault();
                if (device == null)
                {
                    SqlDevice item = new SqlDevice();
                    item.ID = DateTime.Now.Ticks;
                    item.code = "dv1";
                    item.name = "thiết bị 1";
                    item.des = "thiết bị 1";
                    item.isdeleted = false;
                    context.devices!.Add(item);
                }

                device = context.devices!.Where(s => s.code.CompareTo("dv2") == 0 && s.isdeleted == false).FirstOrDefault();
                if (device == null)
                {
                    SqlDevice item = new SqlDevice();
                    item.ID = DateTime.Now.Ticks;
                    item.code = "dv2";
                    item.name = "thiết bị 2";
                    item.des = "thiết bị 2";
                    item.isdeleted = false;
                    context.devices!.Add(item);
                }
              
                int rows = await context.SaveChangesAsync();
            }
        }
        public async Task<bool> createDevice(string code, string name, string des)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(code) || string.IsNullOrEmpty(des))
            {
                return false;
            }
            using (DataContext context = new DataContext())
            {
                SqlDevice? device = context.devices!.Where(s => s.isdeleted == false && s.code.CompareTo(code) == 0).FirstOrDefault();
                if (device != null)
                {
                    return false;
                }
              
                device = new SqlDevice();
                device.ID = DateTime.Now.Ticks;
                device.code = code;
                device.name = name;
                device.des = des;
                device.isdeleted = false;
               
                context.devices!.Add(device);

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

        public async Task<bool> editDevice(string code, string name, string des)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(code) || string.IsNullOrEmpty(des))
            {
                return false;
            }
            using (DataContext context = new DataContext())
            {
                SqlDevice? device = context.devices!.Where(s => s.isdeleted == false && s.code.CompareTo(code) == 0).FirstOrDefault();
                if (device == null)
                {
                    return false;
                }
              
                device.name = name;
                device.des = des;

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

        public async Task<bool> deleteDevice(string code)
        {
            using (DataContext context = new DataContext())
            {
                SqlDevice? device = context.devices!.Where(s => s.isdeleted == false && s.code.CompareTo(code) == 0).FirstOrDefault();
                if (device == null)
                {
                    return false;
                }

                device.isdeleted = true;

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

        public class ItemDevice
        {
            public string code { get; set; } = "";
            public string name { get; set; } = "";
            public string des { get; set; } = "";
           
        }

        public List<ItemDevice> getListDevice()
        {
            using (DataContext context = new DataContext())
            {
                List<ItemDevice> list = new List<ItemDevice>();
                List<SqlDevice> devices = context.devices!.Where(s => s.isdeleted == false).ToList();
                if (devices.Count > 0)
                {
                    foreach (SqlDevice device in devices)
                    {
                        ItemDevice item = new ItemDevice();
                        item.code = device.code;
                        item.name = device.name;
                        item.des = device.des;

                        list.Add(item);
                    }
                }
                return list;
            }
        }
    }
}
