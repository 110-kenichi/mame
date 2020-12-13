// copyright-holders:K.Ito
using System;
using Newtonsoft.Json.Linq;

//https://stackoverflow.com/questions/36884902/how-to-omit-ignore-skip-empty-object-literals-in-the-produced-json

public static class JsonHelper
{
    public static string SerializeToMinimalJson(object obj)
    {
        return JToken.FromObject(obj).RemoveEmptyChildren().ToString();
    }

    public static JToken RemoveEmptyChildren(this JToken token)
    {
        if (token.Type == JTokenType.Object)
        {
            JObject copy = new JObject();
            foreach (JProperty prop in token.Children<JProperty>())
            {
                JToken child = prop.Value;
                if (child.HasValues)
                {
                    child = child.RemoveEmptyChildren();
                }
                if (!child.IsEmptyOrDefault())
                {
                    copy.Add(prop.Name, child);
                }
            }
            return copy;
        }
        else if (token.Type == JTokenType.Array)
        {
            JArray copy = new JArray();
            foreach (JToken item in token.Children())
            {
                JToken child = item;
                if (child.HasValues)
                {
                    child = child.RemoveEmptyChildren();
                }
                if (!child.IsEmptyOrDefault())
                {
                    copy.Add(child);
                }
            }
            return copy;
        }
        return token;
    }

    public static bool IsEmptyOrDefault(this JToken token)
    {
        return (token.Type == JTokenType.Array && !token.HasValues) ||
               (token.Type == JTokenType.Object && !token.HasValues) ||
               (token.Type == JTokenType.String && token.ToString() == String.Empty) ||
               (token.Type == JTokenType.Boolean && token.Value<bool>() == false) ||
               (token.Type == JTokenType.Integer && token.Value<int>() == 0) ||
               (token.Type == JTokenType.Float && token.Value<double>() == 0.0) ||
               (token.Type == JTokenType.Null);
    }

}
