// Copyright (c) 2021 Snowflake Inc. All rights reserved.

// Licensed under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License.  You may obtain a copy of the License at

//   http://www.apache.org/licenses/LICENSE-2.0

// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied.  See the License for the
// specific language governing permissions and limitations
// under the License.

using System;
using Newtonsoft.Json.Linq;

namespace Snowflake.Powershell
{

    /// <summary>
    /// Helper functions for dealing with JSON tokens
    /// </summary>
    public class JSONHelper
    {
        public static bool isTokenNull(JToken jToken)
        {
            if (jToken == null)
            {
                return true;
            }
            else if (jToken.Type == JTokenType.Null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool isTokenPropertyNull(JToken jToken, string propertyName)
        {
            if (jToken == null)
            {
                return true;
            }
            else if (jToken.Type == JTokenType.Null)
            {
                return true;
            }
            else if (jToken[propertyName] == null)
            {
                return true;
            }
            else if (jToken[propertyName].Type == JTokenType.Null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static string getStringValueFromJToken(JToken jToken, string propertyName)
        {
            if (jToken == null)
            {
                return String.Empty;
            }
            else if (jToken.Type == JTokenType.Null)
            {
                return String.Empty;
            }
            else if (jToken[propertyName] == null)
            {
                return String.Empty;
            }
            else if (jToken[propertyName].Type == JTokenType.Null)
            {
                return String.Empty;
            }
            else
            {
                string value = jToken[propertyName].Value<string>();
                if (value == null)
                {
                    return String.Empty;
                }
                else
                {
                    return value;
                }
            }
        }

        public static DateTime getDateTimeValueFromJToken(JToken jToken, string propertyName)
        {
            if (jToken == null)
            {
                return DateTime.MinValue;
            }
            else if (jToken.Type == JTokenType.Null)
            {
                return DateTime.MinValue;
            }
            else if (jToken[propertyName] == null)
            {
                return DateTime.MinValue;
            }
            else if (jToken[propertyName].Type == JTokenType.Null)
            {
                return DateTime.MinValue;
            }
            else
            {
                DateTime value = jToken[propertyName].Value<DateTime>();

                return value;
            }
        }

        public static JToken getJTokenValueFromJToken(JToken jToken, string propertyName)
        {
            if (jToken == null)
            {
                return null;
            }
            else if (jToken.Type == JTokenType.Null)
            {
                return null;
            }
            else if (jToken[propertyName] == null)
            {
                return null;
            }
            else if (jToken[propertyName].Type == JTokenType.Null)
            {
                return jToken[propertyName];
            }
            else
            {
                return jToken[propertyName];
            }
        }

        public static string getStringValueOfObjectFromJToken(JToken jToken, string propertyName)
        {
            return getStringValueOfObjectFromJToken(jToken, propertyName, false);
        }

        public static string getStringValueOfObjectFromJToken(JToken jToken, string propertyName, bool singleLine)
        {
            if (jToken == null)
            {
                return String.Empty;
            }
            else if (jToken.Type == JTokenType.Null)
            {
                return String.Empty;
            }
            else if (jToken[propertyName] == null)
            {
                return String.Empty;
            }
            else if (jToken[propertyName].Type == JTokenType.Null)
            {
                return String.Empty;
            }
            else
            {
                try
                {
                    if (singleLine == true)
                    {
                        return jToken[propertyName].ToString(Newtonsoft.Json.Formatting.None);
                    }
                    else
                    {
                        {
                            return jToken[propertyName].ToString(Newtonsoft.Json.Formatting.Indented);
                        }
                    }
                }
                catch
                {
                    return String.Empty;
                }
            }
        }

        public static bool getBoolValueFromJToken(JToken jToken, string propertyName)
        {
            if (jToken == null)
            {
                return false;
            }
            else if (jToken.Type == JTokenType.Null)
            {
                return false;
            }
            else if (jToken[propertyName] == null)
            {
                return false;
            }
            else if (jToken[propertyName].Type == JTokenType.Null)
            {
                return false;
            }
            else
            {
                return jToken[propertyName].Value<bool>();
            }
        }

        public static long getLongValueFromJToken(JToken jToken, string propertyName)
        {
            if (jToken == null)
            {
                return 0;
            }
            else if (jToken.Type == JTokenType.Null)
            {
                return 0;
            }
            else if (jToken[propertyName] == null)
            {
                return 0;
            }
            else if (jToken[propertyName].Type == JTokenType.Null)
            {
                return 0;
            }
            else
            {
                return jToken[propertyName].Value<long>();
            }
        }

        public static int getIntValueFromJToken(JToken jToken, string propertyName)
        {
            if (jToken == null)
            {
                return 0;
            }
            else if (jToken.Type == JTokenType.Null)
            {
                return 0;
            }
            else if (jToken[propertyName] == null)
            {
                return 0;
            }
            else if (jToken[propertyName].Type == JTokenType.Null)
            {
                return 0;
            }
            else
            {
                return jToken[propertyName].Value<int>();
            }
        }

        public static double getDoubleValueFromJToken(JToken jToken, string propertyName)
        {
            if (jToken == null)
            {
                return 0;
            }
            else if (jToken.Type == JTokenType.Null)
            {
                return 0;
            }
            else if (jToken[propertyName] == null)
            {
                return 0;
            }
            else if (jToken[propertyName].Type == JTokenType.Null)
            {
                return 0;
            }
            else
            {
                return jToken[propertyName].Value<double>();
            }
        }

        public static long sumLongValuesInArray(JArray valuesArray)
        {
            long result = 0;
            foreach (JToken arrayToken in valuesArray)
            {
                if (isTokenNull(arrayToken) == false)
                {
                    result = result + (long)arrayToken;
                }
            }
            return result;
        }
    }
}