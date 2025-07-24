using Newtonsoft.Json;

namespace TMHCC.UITest
{
    public static class ConfigLoader
    {
        public static Config LoadConfig(string path)
        {
            using (StreamReader r = new StreamReader(path))
            {
                string json = r.ReadToEnd();
                return JsonConvert.DeserializeObject<Config>(json);
            }
        }
    }

}
