using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Dynamic;
using System.Linq;
using System.Runtime.Serialization;
using zanac.MAmidiMEmo.ComponentModel;
using zanac.MAmidiMEmo.Gui;

namespace zanac.MAmidiMEmo.Instruments.Vst
{

    [JsonConverter(typeof(VstSettingsJsonConverter<VstSettings>))]
    [TypeConverter(typeof(CustomExpandableObjectConverter))]
    [DataContract]
    [Editor(typeof(VstUITypeEditor), typeof(UITypeEditor))]
    public class VstSettings : DynamicObject, INotifyPropertyChanged, ICustomTypeDescriptor
    {
        public VstPluginContextWrapper VstPluginContext
        {
            get;
            set;
        }

        private readonly Dictionary<string, object> dynamicProperties =
            new Dictionary<string, object>();

        public IEnumerable<KeyValuePair<string, object>> GetAllRawProperties()
        {
            return dynamicProperties.ToArray();
        }

        public override bool TryGetMember(System.Dynamic.GetMemberBinder binder, out object result)
        {
            var memberName = binder.Name;
            if (VstPluginContext != null && dynamicProperties.ContainsKey(memberName))
            {
                result = 0.0f;
                lock (InstrumentBase.VstPluginContextLockObject)
                {
                    if (VstPluginContext != null)
                    {
                        if (VstPluginContext.VstParameterIndexes.ContainsKey(memberName))
                        {
                            int idx = VstPluginContext.VstParameterIndexes[memberName];
                            result = VstPluginContext.Context.PluginCommandStub.GetParameter(idx);
                        }
                    }
                }
                return true;
            }
            return dynamicProperties.TryGetValue(memberName, out result);
        }

        public override bool TrySetMember(System.Dynamic.SetMemberBinder binder, object value)
        {
            var memberName = binder.Name;
            dynamicProperties[memberName] = value;
            if (VstPluginContext != null)
            {
                lock (InstrumentBase.VstPluginContextLockObject)
                {
                    if (VstPluginContext != null)
                    {
                        if (VstPluginContext.VstParameterIndexes.ContainsKey(memberName))
                        {
                            int idx = VstPluginContext.VstParameterIndexes[memberName];
                            float val = (float)value;
                            if (val < 0)
                                val = 0;
                            else if (val > 1)
                                val = 1;
                            VstPluginContext.Context.PluginCommandStub.SetParameter(idx, val);
                        }
                    }
                }
                return true;
            }
            NotifyToRefreshAllProperties();
            return true;
        }

        #region Implementation of ICustomTypeDescriptor

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
        {
            return GetProperties(new Attribute[0]);
        }

        public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            IEnumerable<VstDynamicPropertyDescriptor> properties = dynamicProperties
                .Select(pair => new VstDynamicPropertyDescriptor(this,
                    pair.Key, pair.Value.GetType(), attributes));
            List<VstDynamicPropertyDescriptor> list = new List<VstDynamicPropertyDescriptor>();
            foreach (VstDynamicPropertyDescriptor property in properties)
                list.Add(property);
            return new PropertyDescriptorCollection(list.ToArray());
        }

        public object GetPropertyOwner(PropertyDescriptor pd)
        {
            return this;
        }

        public string GetClassName()
        {
            return GetType().Name;
        }
        #endregion

        #region Hide not implemented members

        public AttributeCollection GetAttributes()
        {
            return TypeDescriptor.GetAttributes(this, true);
        }

        public string GetComponentName()
        {
            throw new NotImplementedException();
        }

        public TypeConverter GetConverter()
        {
            return TypeDescriptor.GetConverter(this, true);
        }

        public EventDescriptor GetDefaultEvent()
        {
            return TypeDescriptor.GetDefaultEvent(this, true);
        }

        public PropertyDescriptor GetDefaultProperty()
        {
            return TypeDescriptor.GetDefaultProperty(this, true);
        }

        public object GetEditor(Type editorBaseType)
        {
            return TypeDescriptor.GetEditor(this, editorBaseType, true);
        }

        public EventDescriptorCollection GetEvents()
        {
            return TypeDescriptor.GetEvents(this, true);
        }

        public EventDescriptorCollection GetEvents(Attribute[] attributes)
        {
            return TypeDescriptor.GetEvents(this, attributes, true);
        }
        #endregion

        #region Implementation of INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged == null)
                return;

            var eventArgs = new PropertyChangedEventArgs(propertyName);
            PropertyChanged(this, eventArgs);
        }

        private void NotifyToRefreshAllProperties()
        {
            OnPropertyChanged(string.Empty);
        }

        #endregion

        private class VstDynamicPropertyDescriptor : PropertyDescriptor
        {
            private readonly VstSettings vstSettingsObject;
            private readonly Type propertyType;

            public VstDynamicPropertyDescriptor(VstSettings vstSettingsObject,
                string propertyName, Type propertyType, Attribute[] propertyAttributes)
                : base(propertyName, propertyAttributes)
            {
                this.vstSettingsObject = vstSettingsObject;
                this.propertyType = propertyType;
            }

            public override bool CanResetValue(object component)
            {
                return true;
            }

            public override object GetValue(object component)
            {
                return vstSettingsObject.GetPropertyValue(Name);
            }

            public override void ResetValue(object component)
            {
            }

            public override void SetValue(object component, object value)
            {
                vstSettingsObject.SetPropertyValue(Name, value);
            }

            public override bool ShouldSerializeValue(object component)
            {
                return false;
            }

            public override Type ComponentType
            {
                get { return typeof(VstSettings); }
            }

            public override bool IsReadOnly
            {
                get { return false; }
            }

            public override Type PropertyType
            {
                get { return propertyType; }
            }

            public override AttributeCollection Attributes
            {
                get
                {
                    var attrs = base.Attributes;
                    List<Attribute> list = new List<Attribute>();
                    foreach (Attribute attr in attrs)
                        list.Add(attr);

                    list.Add(new EditorAttribute(typeof(DoubleSlideEditor), typeof(UITypeEditor)));
                    list.Add(new DoubleSlideParametersAttribute(0, 1, 0.01, true));
                    AttributeCollection ac = new AttributeCollection(list.ToArray());
                    return ac;
                }
            }
        }

        // オブジェクトの指定された名前のプロパティの値を取得
        public object Eval(string propertyName, out bool isSucceeded)
        {
            var propertyInfo = GetType().GetProperty(propertyName);
            if (propertyInfo == null)
            {
                isSucceeded = false;
                return null;
            }
            isSucceeded = true;
            return propertyInfo.GetValue(this, null);
        }

        // オブジェクトの指定された名前のプロパティの値を設定
        public void SetPropertyValue(string propertyName, object value)
        {
            TrySetMember(new VstSetMemberBinder(propertyName), value);
        }

        public object GetPropertyValue(string propertyName)
        {
            object result;
            return TryGetMember(new VstGetMemberBinder(propertyName), out result) ? result : null;
        }

        private class VstSetMemberBinder : System.Dynamic.SetMemberBinder
        {
            public VstSetMemberBinder(string name) : base(name, false)
            {
            }

            public override DynamicMetaObject FallbackSetMember(DynamicMetaObject target, DynamicMetaObject value, DynamicMetaObject errorSuggestion)
            {
                return null;
            }
        }

        private class VstGetMemberBinder : System.Dynamic.GetMemberBinder
        {
            public VstGetMemberBinder(string name) : base(name, false)
            {
            }

            public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
            {
                return null;
            }
        }
    }


    /// <summary>
    /// クラスのTypeConverterをJSONでシリアライズする時は無効にするコンバータ
    /// (JSON文字列をクラスのTypeConverterが受け取ってエラーになってしまう)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class VstSettingsJsonConverter<T> : JsonConverter
    {
        protected static readonly IContractResolver Resolver = new NoTypeConverterContractResolver();

        private static readonly JsonSerializerSettings jsonSettings = new JsonSerializerSettings { ContractResolver = Resolver, TypeNameHandling = TypeNameHandling.Auto, DefaultValueHandling = DefaultValueHandling.Ignore, SerializationBinder = Program.SerializationBinder };

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
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(T).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var obj = new VstSettings();

            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.EndObject) break;
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    var prop = reader.Value?.ToString();
                    reader.Read();
                    float val = (float)((double)reader.Value);
                    obj.SetPropertyValue(prop, val);
                }
            }

            return obj;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var obj = value as VstSettings;
            if (obj == null)
                return;

            writer.WriteStartObject();
            foreach (var kvp in obj.GetAllRawProperties())
            {
                writer.WritePropertyName(kvp.Key);
                var val = obj.GetPropertyValue(kvp.Key);
                JToken.FromObject(val).WriteTo(writer);
            }
            writer.WriteEndObject();
        }
    }

}
