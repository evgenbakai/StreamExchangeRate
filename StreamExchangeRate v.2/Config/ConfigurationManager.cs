using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;

namespace StreamExchangeRate_v._2.Config
{
    public static class ConfigurationManager
    {
        public static List<ObjJsonConfig> GetJson(string fileName)
        {
            List<ObjJsonConfig> listResult;
            try
            {
                listResult = JsonConvert.DeserializeObject<List<ObjJsonConfig>>(GetJsonString(fileName));
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return listResult;
        }

        private static string GetJsonString(string fileName)
        {
            string jsonStr = string.Empty;
            try
            {
                using (StreamReader streamReader = new StreamReader(fileName))
                {
                    while (!streamReader.EndOfStream)
                    {
                        jsonStr += streamReader.ReadLine() + "\n";
                    }
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return jsonStr;
        }
    }
}
