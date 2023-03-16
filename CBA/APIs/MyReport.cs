namespace CBA.APIs
{
    public class MyReport
    {
        public class InOutPerson
        {
            public string getIn { get; set; } = "";
            public string getOut { get; set; } = "";
        }

        public class ItemInHours
        {
            public string hours { get; set; } = "";
            public List<InOutPerson> data { get; set; } = new List<InOutPerson>();
        }

        public class ItemCount
        {
            public List<string> group { get; set; } = new List<string>();
            public List<ItemInHours> items { get; set; } = new List<ItemInHours>();
        }




    }
}
