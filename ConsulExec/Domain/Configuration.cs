using System.IO;
using Newtonsoft.Json;

namespace ConsulExec.Domain
{
    public class Configuration
    {
        public static Configuration ReadFrom(TextReader Reader)
        {
            return JsonConvert.DeserializeObject<Configuration>(Reader.ReadToEnd(), JsonSettings);
        }

        public string[] MruCommands;
        public ConnectionOptions[] Connections;
        public StartupOptions[] Starup;

        public void SaveTo(TextWriter Writer)
        {
            var json = JsonConvert.SerializeObject(this, JsonSettings);
            Writer.Write(json);
        }

        private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Objects,
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects
        };
    }
}
