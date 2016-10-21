using System;
using Newtonsoft.Json;

namespace U2F.Core.Models
{
    public abstract class BaseModel
    {
        public String ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static T FromJson<T>(String json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
