using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;

namespace yunzhijia
{
    class yunzhijiaInterface
    {
        public static long ConvertDateTimeToInt(System.DateTime time)
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1, 0, 0, 0, 0));
            long t = (time.Ticks - startTime.Ticks) / 10000;   //除10000调整为13位      
            return t;
        }

        private static string Post(string url, string content)
        {

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/json";
            string result = "";
            byte[] data = Encoding.UTF8.GetBytes(content);
            request.ContentLength = data.Length;
            using (Stream reqStream = request.GetRequestStream())
            {
                reqStream.Write(data, 0, data.Length);
                reqStream.Close();
            }
            HttpWebResponse resp = (HttpWebResponse)request.GetResponse();
            Stream stream = resp.GetResponseStream();
            //获取响应内容
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                result = reader.ReadToEnd();
            }
            return result;
        }


        // 获取accessToken
        public JObject GetToken()
        {
            //创建JSON
            JObject jo = new JObject();
            jo["appId"] = "SP1663384";
            jo["eid"] = "1663384";
            jo["secret"] = "JE2S9DAvYzHDtr8FD6oHei4L07n8Qc";
            jo["timestamp"] = ConvertDateTimeToInt(DateTime.Now).ToString();
            jo["scope"] = "team";
            //POST请求
            string result = Post("https://yunzhijia.com/gateway/oauth2/token/getAccessToken", Convert.ToString(jo));
            JObject obresult = JObject.Parse(result);
            //接口调用成功数据
            return obresult;
        }
        // 获取单据实例接口
        public JObject ViewFormIns(string token, string formInstId, string formCodeId)
        {
            using (var httpClient = new HttpClient())
            {


                //post
                var url = new Uri("https://yunzhijia.com/gateway/workflow/form/thirdpart/viewFormInst?accessToken=" + token);
                var body = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "formInstId",formInstId},
                    { "formCodeId",formCodeId}
                });
                // response
                var response = httpClient.PostAsync(url, body).Result;
                var data = response.Content.ReadAsStringAsync().Result;
                JObject jo = JObject.Parse(data);
                return jo;//接口调用成功数据
            }
        }

        // 获取流程列表接口
        public JObject FindFlows(string token)
        {
            JObject jo = new JObject();
            JArray approvers = new JArray();
            JArray formCodeIds = new JArray();
            JArray status = new JArray();
            /*  approvers.Add("王陈辰");
              approvers.Add("圣凯");
              approvers.Add("汪海燕");
              approvers.Add("李源");
              approvers.Add("王杰");
              approvers.Add("房百龙");*/
            approvers.Add("5d91b7cfe4b01b007c703bb7");
            //formCodeIds.Add("ad98a2c602ee4aefa91c164885edbcb7");//云之家测试
            status.Add("RUNNING");
            formCodeIds.Add("1d87e21742bd4bf5a190aa330c34f100");//物料/
            jo["approvers"] = approvers;
            jo["formCodeIds"] = formCodeIds;
            jo["pageSize"] = "100";
            jo["status"] = status;

            string result = Post("https://yunzhijia.com/gateway/workflow/form/thirdpart/findFlows?accessToken=" + token, Convert.ToString(jo));
            JObject obresult = JObject.Parse(result);
            //接口调用成功数据
            return obresult;

        }

        // 获取单据实例接口
        public JObject GetTemplates(string token)
        {
            using (var httpClient = new HttpClient())
            {


                //post
                var url = new Uri("https://yunzhijia.com/gateway/workflow/form/thirdpart/getTemplates?accessToken=" + token);
                var body = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    //{ "identifyKey", "3716f5fc19434ead8c2e62a8a4c0271f"},

                });
                // response
                var response = httpClient.PostAsync(url, body).Result;
                var data = response.Content.ReadAsStringAsync().Result;
                JObject jo = JObject.Parse(data);
                return jo;//接口调用成功数据
            }
        }

        // 修改单据实例
        public string ModifyInst(string token, string formCodeId, string formDefId, string formInstId, string widgetValue)
        {
            JObject jo = new JObject();
            JObject jo2 = new JObject();

            jo["creator"] = "5d91b7cfe4b01b007c703bb7";
            jo["formCodeId"] = formCodeId;
            jo["formDefId"] = formDefId;
            jo["formInstId"] = formInstId;
            jo2["Te_7"] = widgetValue;
            jo["widgetValue"] = jo2;
            string result = Post("https://yunzhijia.com/gateway/workflow/form/thirdpart/modifyInst?accessToken=" + token, Convert.ToString(jo));
            //接口调用成功数据
            return result;

        }

        // 查询审批节点审批人信息接口
        public string GetFlowDetailById(string token)
        {
            JObject jo = new JObject();
            JObject jo2 = new JObject();

            jo["flowInstId"] = "5fd58808e6df1500014149db";
            jo["activityCodeId"] = "3716f5fc19434ead8c2e62a8a4c0271f";
            jo["activityType"] = "5fd587ee02c1040001a71df5";
            jo["predictFlag"] = "5fd58808e6df1500014149db";
            string result = Post("http://www.yunzhijia.com/gateway/workflow/form/thirdpart/getFlowDetailById?accessToken=" + token, Convert.ToString(jo));
            //接口调用成功数据
            return result;

        }

        // 同意接口用来对审批人在当前节点执行同意操作，接口效果等同在审批内点击同意
        public string Agree(string token, string flowInstId, string formCodeId, string formDefId, string formInstId, string value)
        {
            JObject jo = new JObject();
            JObject jo2 = new JObject();
            JObject jo3 = new JObject();
            JObject jo4 = new JObject();

            jo["approver"] = "5d91b7cfe4b01b007c703bb7";

            jo2["flowInstId"] = flowInstId;
            jo2["opinion"] = "同意";
            jo["flow"] = jo2;

            jo3["formCodeId"] = formCodeId;
            jo3["formDefId"] = formDefId;
            jo3["formInstId"] = formInstId;
            jo4["Te_7"] = value;
            jo3["widgetValue"] = jo4;
            jo["form"] = jo3;



            string result = Post("http://www.yunzhijia.com/gateway/workflow/form/thirdpart/agree?accessToken=" + token, Convert.ToString(jo));
            //接口调用成功数据
            return result;

        }

        // 同意接口用来对审批人在当前节点执行同意操作，接口效果等同在审批内点击同意
        public string RetrunBack(string token, string flowInstId, string formCodeId, string formDefId, string formInstId, string value)
        {
            JObject jo = new JObject();
            JObject jo2 = new JObject();
            JObject jo3 = new JObject();
            JObject jo4 = new JObject();

            jo["approver"] = "5d91b7cfe4b01b007c703bb7";

            jo2["flowInstId"] = flowInstId;
            jo2["opinion"] = "不同意";
            jo2["returnStartActivity"] = true;
            jo["flow"] = jo2;

            //jo3["formCodeId"] = formCodeId;
            //jo3["formDefId"] = formDefId;
            //jo3["formInstId"] = formInstId;
            jo4["Te_7"] = value;
            jo3["widgetValue"] = jo4;
            jo["form"] = jo3;



            string result = Post("http://www.yunzhijia.com/gateway/workflow/form/thirdpart/return?accessToken=" + token, Convert.ToString(jo));
            //接口调用成功数据
            return result;

        }

        // 获取互联模板接口
        public string GetByGroupId(string token)
        {
            JObject jo = new JObject();
            JObject jo2 = new JObject();
            JObject jo3 = new JObject();
            JObject jo4 = new JObject();




            string result = Post("https://yunzhijia.com/gateway/workflow/form/thirdpart/getByGroupId?accessToken=" + token, Convert.ToString(jo));
            //接口调用成功数据
            return result;

        }
    }
}


