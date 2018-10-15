using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Cache;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Net.Security;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Demo_CsharpBetweenSrvAndWebAPI
{
    public class webapi_operation
    {
        public string Enroll(bool https_en)
        {
            return WebAPI_get_enroll_minutiae(https_en);
        }

        public string Verify(bool https_en)
        {
            return WebAPI_get_minutiae(https_en);
        }

        public string Identify(bool https_en)
        {
            return WebAPI_get_minutiae(https_en);
        }
        public string Delete(bool https_en, string id, int fp_idx)
        {
            return WebAPI_get_delete_data(https_en, id, fp_idx);
        }

        //this function is only for first time load fpservice.exe take more time, so we launch it at very beggining
        private void WebAPI_load_fp_srv(bool https_en)
        {
            //bool https_en = checkBox_https.Enabled;
            string route = "/api/load_fp_srv";

            string json_out = PostJson2WebAPI(https_en, route, "");
        }

        private string WebAPI_get_minutiae(bool https_en)
        {
            //bool https_en = checkBox_https.Enabled;
            string route = "/api/get_minutiae";

            string ret = PostJson2WebAPI(https_en, route, "");

            return ret;
        }

        //this function is specified session key for Startek format "2018-08-20-14-50-00           "
        private string WebAPI_set_session_key(bool https_en)
        {
            string route = "/api/set_session_key ";

            //modify key as you need
            string startek_sessionkey = StartekSessionKey();

            string json_str = "{sessionkey:\"" + startek_sessionkey + "\"}";

            string ret = PostJson2WebAPI(https_en, route, json_str);

            return ret;
        }

        private string WebAPI_get_enroll_minutiae(bool https_en)
        {
            string route = "/api/get_enroll_minutiae";

            string ret = PostJson2WebAPI(https_en, route, "");

            return ret;
        }

        private string WebAPI_get_delete_data(bool https_en, string id, int fp_idx)
        {
            //bool https_en = checkBox_https.Enabled;
            //string id = richTextBox_id.Text;

            //int fp_idx;
            //int.TryParse(comboBox_finger.SelectedValue.ToString(), out fp_idx);

            string route = "/api/get_delete_data";

            string json_str = "{clientUserId:\"" + id + "\", fpIndex:" + fp_idx.ToString() + "}";
            string ret = PostJson2WebAPI(https_en, route, json_str);

            return ret;
        }

        private string PostJson2WebAPI(bool https_en, string route, string json_string)
        {
            string protocol = "";

            if (https_en)
            {
                protocol = "https://localhost:5888";

                //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Ssl3|SecurityProtocolType.Tls12;

                //must support tls 1.2 for WebAPI use, thus .netframe work 4.6 or higher is necessary
                //also must ignore self signed certification for localhost can only get self signed CA.
                ServicePointManager.ServerCertificateValidationCallback =
                delegate { return true; };
            }
            else
            {
                protocol = "http://localhost:5887";
            }

            //var httpWebRequest = (HttpWebRequest)WebRequest.Create("http://localhost:5887/api/load_fp_srv");
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(protocol + route);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";
            httpWebRequest.Accept = "application/json";
            httpWebRequest.CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
            httpWebRequest.UserAgent = "myapp";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string json = json_string;

                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                string result_ = result.ToString();
                return result;
            }

        }

        private string StartekSessionKey()
        {
            string str_utc = TimeZoneInfo.ConvertTimeToUtc(DateTime.Now).ToString("yyyy-MM-dd-hh-mm-ss");// use UTC time as session key
            string str_pad_blank = str_utc.PadRight(32, ' ');

            char[] charValues = str_pad_blank.ToCharArray();
            string hexOutput = "";
            foreach (char _eachChar in charValues)
            {
                // Get the integral value of the character.
                int value = Convert.ToInt32(_eachChar);
                // Convert the decimal value to a hexadecimal value in string form.
                hexOutput += String.Format("{0:X}", value);
            }

            return hexOutput;
        }
    }
}
