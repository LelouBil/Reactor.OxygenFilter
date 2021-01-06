using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Reactor.Greenhouse.Setup.Provider.Configurations;

namespace TestProject
{
    public class ProviderConfigConverter : JsonConverter<IProviderConfig>
    {
        private const string TypeProperty = "Type";

        private JsonSerializer privateSerializer = JsonSerializer.CreateDefault();

        public override void WriteJson(JsonWriter writer, IProviderConfig value, JsonSerializer serializer)
        {
            JObject token;
            token = JObject.FromObject(value);

            token.AddFirst(new JProperty(TypeProperty,value.GetType().FullName));
            
            token.WriteTo(writer);
        }
        
        public override IProviderConfig ReadJson(JsonReader reader, Type objectType, IProviderConfig existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            if(hasExistingValue) return existingValue;
            var obj = JToken.Load(reader);
            if (obj is JObject jobj)
            {
                var concreteType = Type.GetType(jobj.Property(TypeProperty)?.Value.ToString() ??
                                                throw new InvalidOperationException());

                return privateSerializer.Deserialize(obj.CreateReader(), concreteType) as IProviderConfig;
            }

            return null;
        }
    }
}