// copyright-holders:K.Ito
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using zanac.MAmidiMEmo.ComponentModel;

namespace zanac.MAmidiMEmo.Instruments
{

    public static class SimpleSerializer
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="props"></param>
        /// <returns></returns>
        public static string SerializeProps<T>(T obj, params string[] props)
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                foreach (string m in props)
                {
                    if (sb.Length != 0)
                        sb.Append(",");
                    var f = getPropertyInfo(obj, m);
                    var val = f.Property.GetValue(f.Owner);
                    if(val != null)
                        sb.Append(val.ToString());
                }
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(Exception))
                    throw;
                else if (ex.GetType() == typeof(SystemException))
                    throw;


                System.Windows.Forms.MessageBox.Show(ex.ToString());
            }

            return sb.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="props"></param>
        /// <returns></returns>
        public static void DeserializeProps<T>(T obj, string serializeData, params string[] props)
        {
            try
            {
                serializeData = serializeData.Replace("\r", "").Replace("\n", "");
                var vals = serializeData.Split(new char[] { ',', ' ', '\t' }).GetEnumerator();
                foreach (string m in props)
                {
                    if (!vals.MoveNext())
                        break;
                    var v = (string)vals.Current;
                    v = v.Trim();

                    var f = getPropertyInfo(obj, m);
                    try
                    {
                        if (string.IsNullOrEmpty(v))
                        {
                            f.Property.SetValue(f.Owner, null);
                        }
                        else
                        {
                            switch (f.Property.PropertyType)
                            {
                                case Type t when t == typeof(int):
                                    f.Property.SetValue(f.Owner, int.Parse(v));
                                    break;
                                case Type t when t == typeof(uint):
                                    f.Property.SetValue(f.Owner, uint.Parse(v));
                                    break;
                                case Type t when t == typeof(short):
                                    f.Property.SetValue(f.Owner, short.Parse(v));
                                    break;
                                case Type t when t == typeof(ushort):
                                    f.Property.SetValue(f.Owner, ushort.Parse(v));
                                    break;
                                case Type t when t == typeof(byte):
                                    f.Property.SetValue(f.Owner, byte.Parse(v));
                                    break;
                                case Type t when t == typeof(sbyte):
                                    f.Property.SetValue(f.Owner, sbyte.Parse(v));
                                    break;

                                case Type t when t == typeof(int?):
                                    f.Property.SetValue(f.Owner, int.Parse(v));
                                    break;
                                case Type t when t == typeof(uint?):
                                    f.Property.SetValue(f.Owner, uint.Parse(v));
                                    break;
                                case Type t when t == typeof(short?):
                                    f.Property.SetValue(f.Owner, short.Parse(v));
                                    break;
                                case Type t when t == typeof(ushort?):
                                    f.Property.SetValue(f.Owner, ushort.Parse(v));
                                    break;
                                case Type t when t == typeof(byte?):
                                    f.Property.SetValue(f.Owner, byte.Parse(v));
                                    break;
                                case Type t when t == typeof(sbyte?):
                                    f.Property.SetValue(f.Owner, sbyte.Parse(v));
                                    break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ex.GetType() == typeof(Exception))
                            throw;
                        else if (ex.GetType() == typeof(SystemException))
                            throw;


                        System.Windows.Forms.MessageBox.Show(ex.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(Exception))
                    throw;
                else if (ex.GetType() == typeof(SystemException))
                    throw;


                System.Windows.Forms.MessageBox.Show(ex.ToString());
            }
        }

        private static InstancePropertyInfo getPropertyInfo(object inst, string propertyName)
        {
            object obj = inst;
            object lobj = obj;

            // Split property name to parts (propertyName could be hierarchical, like obj.subobj.subobj.property
            string[] propertyNameParts = propertyName.Split('.');

            PropertyInfo pi = null;
            foreach (string propertyNamePart in propertyNameParts)
            {
                if (obj == null)
                    return null;

                // propertyNamePart could contain reference to specific 
                // element (by index) inside a collection
                if (!propertyNamePart.Contains("["))
                {
                    pi = obj.GetType().GetProperty(propertyNamePart);
                    if (pi == null)
                        return null;
                    lobj = obj;
                    obj = pi.GetValue(obj, null);
                }
                else
                {   // propertyNamePart is areference to specific element 
                    // (by index) inside a collection
                    // like AggregatedCollection[123]
                    //   get collection name and element index
                    int indexStart = propertyNamePart.IndexOf("[") + 1;
                    string collectionPropertyName = propertyNamePart.Substring(0, indexStart - 1);
                    int collectionElementIndex = Int32.Parse(propertyNamePart.Substring(indexStart, propertyNamePart.Length - indexStart - 1));
                    //   get collection object
                    pi = obj.GetType().GetProperty(collectionPropertyName);
                    if (pi == null)
                        return null;
                    object unknownCollection = pi.GetValue(obj, null);
                    //   try to process the collection as array
                    if (unknownCollection.GetType().IsArray)
                    {
                        object[] collectionAsArray = unknownCollection as object[];
                        lobj = obj;
                        obj = collectionAsArray[collectionElementIndex];
                    }
                    else
                    {
                        //   try to process the collection as IList
                        System.Collections.IList collectionAsList = unknownCollection as System.Collections.IList;
                        if (collectionAsList != null)
                        {
                            lobj = obj;
                            obj = collectionAsList[collectionElementIndex];
                        }
                        else
                        {
                            // ??? Unsupported collection type
                        }
                    }
                }
            }

            return new InstancePropertyInfo(lobj, pi);
        }

    }
}
