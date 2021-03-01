using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;

namespace Acupuncture.CommonFunction.WritebleAppSettingFunction
{
    public class WritebleSettingSvc<T> :IWritebleSettingSvc<T> where T:class, new() 
    {
        private readonly string _section;
        private readonly string _file;
        //This is notification when T instance changes
        private readonly IOptionsMonitor<T> _options;
        private readonly IWebHostEnvironment _en;
        public WritebleSettingSvc(IWebHostEnvironment en,IOptionsMonitor<T> optons,
            string section,string file)
        {
            _options = optons;
            _en = en;
            _file = file;
            _section = section;
        }
        public T Value => _options.CurrentValue;
        public T Get(string name) => _options.Get(name);
        public bool Update(Action<T> applyChange)
        {
            var resultError = false;
            try
            {
                var fileProvider = _en.ContentRootFileProvider;
                var fileInfo = fileProvider.GetFileInfo(_file);
                var physicalPath = fileInfo.PhysicalPath;

                var jObject = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(physicalPath));
                var sectionObject = jObject.TryGetValue(_section, out JToken section) ?
                    JsonConvert.DeserializeObject<T>(section.ToString()) : (Value ?? new T());
                //This is delegate used to assign value method in controller
                applyChange(sectionObject);

                jObject[_section] = JObject.Parse(JsonConvert.SerializeObject(sectionObject));
                File.WriteAllText(physicalPath, JsonConvert.SerializeObject(jObject, Formatting.Indented));
                resultError = false;
               
            }
            catch (Exception ex)
            {
                Log.Error("An error occurred while seeding the database  {Error} {StackTrace} {InnerException} {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
                resultError = true;
            }

            return resultError;

        }

    }
}
