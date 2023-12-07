using Kingdee.BOS.Util;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;

namespace King.K3.BD.App.Common.ServicePlugIn.Material
{
    [HotUpdate]
    public class yunzhijiaInterface
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="time"></param>//需转13位时间戳的时间
        /// <returns></returns>
        public static long ConvertDateTimeToInt(System.DateTime time)
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1, 0, 0, 0, 0));
            long t = (time.Ticks - startTime.Ticks) / 10000;   //除10000调整为13位      
            return t;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>//接口地址
        /// <param name="content"></param>//请求内容
        /// <returns></returns>
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
        public string GetToken()
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
            string token = Convert.ToString(obresult["data"]["accessToken"]);
            //接口调用成功数据
            return token;
        }
        /// <summary>
        /// 获取单据实例接口
        /// </summary>
        /// <param name="token"></param>//接口密令
        /// <param name="formInstId"></param>//审批实例ID
        /// <param name="formCodeId"></param>//审批模板ID
        /// <returns></returns>
        public JObject ViewFormInst(string token, string formInstId, string formCodeId)
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
        /// <summary>
        /// 获取流程列表接口
        /// </summary>
        /// <param name="token"></param>//接口密令
        /// <param name="Name"></param>//当前审批人员ID
        /// <returns></returns>
        public JObject FindFlows(string token, string Name)
        {
            JObject jo = new JObject();
            JArray approvers = new JArray();
            JArray formCodeIds = new JArray();
            JArray status = new JArray();
            approvers.Add(Name);
            status.Add("RUNNING");
            //formCodeIds.Add("ad98a2c602ee4aefa91c164885edbcb7");//云之家测试
            formCodeIds.Add("1d87e21742bd4bf5a190aa330c34f100");//物料
            jo["approvers"] = approvers;
            jo["formCodeIds"] = formCodeIds;
            jo["pageSize"] = "100";
            jo["status"] = status;

            string result = Post("https://yunzhijia.com/gateway/workflow/form/thirdpart/findFlows?accessToken=" + token, Convert.ToString(jo));
            JObject obresult = JObject.Parse(result);
            //接口调用成功数据
            return obresult;

        }
        /// <summary>
        /// 同意接口用来对审批人在当前节点执行同意操作，接口效果等同在审批内点击同意
        /// </summary>
        /// <param name="token"></param>//接口密令
        /// <param name="flowInstId"></param>// 流程实例id
        /// <param name="formCodeId"></param>//审批实例ID
        /// <param name="formDefId"></param>//表单模版ID
        /// <param name="formInstId"></param>//审批模板ID
        /// <param name="value"></param>//物料编码
        /// <param name="yzjUserId"></param>//云之家审核人ID
        /// <returns></returns>
        public string Agree(string token, string flowInstId, string formCodeId, string formDefId, string formInstId, string value, string yzjUserId)
        {
            JObject jo = new JObject();
            JObject jo2 = new JObject();
            JObject jo3 = new JObject();
            JObject jo4 = new JObject();

            jo["approver"] = yzjUserId;

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


        /// <summary>
        /// 退回节点接口用来将审批退回到某个节点，目前只支持退回到发起节点，接口效果等同在审批内点击退回
        /// </summary>
        /// <param name="token"></param>//接口密令
        /// <param name="flowInstId"></param>//流程实例id
        /// <param name="yzjUserId"></param>//云之家审核人ID
        /// <returns></returns>
        public string RetrunBack(string token, string flowInstId, string yzjUserId)
        {
            JObject jo = new JObject();
            JObject jo2 = new JObject();

            jo["approver"] = yzjUserId;

            jo2["flowInstId"] = flowInstId;
            jo2["opinion"] = "不同意";
            jo2["returnStartActivity"] = true;
            jo["flow"] = jo2;

            string result = Post("http://www.yunzhijia.com/gateway/workflow/form/thirdpart/return?accessToken=" + token, Convert.ToString(jo));
            //接口调用成功数据
            return result;

        }

        /// <summary>
        /// 表单修改接口
        /// </summary>
        /// <returns></returns>
        public void ModifyInst(string token, string yzjUserId, string formCodeId, string formDefId, string formInstId, string Number)
        {
            JObject jo = new JObject();
            JObject jo2 = new JObject();
            

            jo["creator"] = yzjUserId;
            jo["formCodeId"] = formCodeId;
            jo["formDefId"] = formDefId;
            jo["formInstId"] = formInstId;
            jo["widgetValue"] = jo2;

            jo2["Te_7"] = Number;


            string result = Post("https://yunzhijia.com/gateway/workflow/form/thirdpart/modifyInst?accessToken="+token, Convert.ToString(jo));
        }

    }
}


