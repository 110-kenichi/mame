// copyright-holders:K.Ito
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace zanac.MAmidiMEmo.ComponentModel
{

    /// <summary>
    /// クラスのTypeConverterをJSONでシリアライズする時は無効にするコンバータ
    /// (JSON文字列をクラスのTypeConverterが受け取ってエラーになってしまう)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class NoTypeConverterJsonConverter<T> : JsonConverter
    {
        protected static readonly IContractResolver Resolver = new NoTypeConverterContractResolver();

        private static readonly JsonSerializerSettings jsonSettings = new JsonSerializerSettings { ContractResolver = Resolver, TypeNameHandling = TypeNameHandling.Auto, SerializationBinder = Program.SerializationBinder };

        class NoTypeConverterContractResolver : DefaultContractResolver
        {
            protected override JsonContract CreateContract(Type objectType)
            {
                if (typeof(T).IsAssignableFrom(objectType))
                {
                    var contract = this.CreateObjectContract(objectType);
                    contract.Converter = null; // Also null out the converter to prevent infinite recursion.
                    return contract;
                }
                return base.CreateContract(objectType);
            }

            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                JsonProperty property = base.CreateProperty(member, memberSerialization);

                MethodInfo shouldSerializeMethodInfo = member.DeclaringType.GetMethod("ShouldSerialize" + member.Name);
                if (shouldSerializeMethodInfo != null)
                {
                    property.ShouldSerialize = o =>
                    {
                        return (bool)shouldSerializeMethodInfo.Invoke(o, null);
                    };
                }
                else
                {
                    property.ShouldSerialize = o =>
                    {
                        var val = property.ValueProvider.GetValue(o);
                        if (val == null)
                            return false;

                        DefaultValueAttribute attr = (DefaultValueAttribute)member.GetCustomAttribute(typeof(DefaultValueAttribute));
                        if(attr != null)
                            return !val.Equals(attr.Value);

                        return true;
                    };

                }

                return property;
            }
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(T).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return JsonSerializer.CreateDefault(jsonSettings).Deserialize(reader, objectType);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            JsonSerializer.CreateDefault(jsonSettings).Serialize(writer, value);
        }
    }

    /// <summary>
    /// クラスのTypeConverterをJSONでシリアライズする時は無効にするコンバータ
    /// (JSON文字列をクラスのTypeConverterが受け取ってエラーになってしまう)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class NoTypeConverterJsonConverterObject<T> : NoTypeConverterJsonConverter<T>
    {
        private static readonly JsonSerializerSettings jsonSettings = new JsonSerializerSettings { ContractResolver = Resolver, TypeNameHandling = TypeNameHandling.Objects, SerializationBinder = Program.SerializationBinder };

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var js = Program.JsonAutoSettings;

            return JsonSerializer.CreateDefault(Program.JsonAutoSettings).Deserialize(reader, objectType);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            JsonSerializer.CreateDefault(jsonSettings).Serialize(writer, value);
        }
    }
}
