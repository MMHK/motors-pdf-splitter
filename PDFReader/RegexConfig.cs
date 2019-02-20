using System.Collections.Generic;

namespace PDFReader
{
    public class RegexConfig
    {
        public string Pattern;

        protected Dictionary<string, string> data = new Dictionary<string, string>();

        public void setParam(string key, string value)
        {
            data[key] = value;
        }

        public string getParam(string key)
        {
            string value = "";
            return data.TryGetValue(key, out value) ? value : null;
        }

        public Dictionary<string, string> GetAll()
        {
            return data;
        }
    }
}