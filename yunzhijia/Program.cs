using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Text;

namespace yunzhijia
{
    class Program
    {



        public static long ConvertDateTimeToInt(System.DateTime time)
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1, 0, 0, 0, 0));
            long t = (time.Ticks - startTime.Ticks) / 10000;   //除10000调整为13位      
            return t;
        }

        //13位时间戳转日期
        private static DateTime ConvertStringToDateTime(string timeStamp)
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long lTime = long.Parse(timeStamp + "0000");
            TimeSpan toNow = new TimeSpan(lTime);
            return dtStart.Add(toNow);
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



        static void Main(string[] args)
        {


            yunzhijiaInterface yzjif = new yunzhijiaInterface();
            Kingdee kd = new Kingdee();
            DateTime time = new DateTime(2020, 12, 15, 10, 00, 00);

            //---------------------------------------------------------------------------------------------------

            // 获取accessToken
            JObject accesstoken = yzjif.GetToken();
            string token = Convert.ToString(accesstoken["data"]["accessToken"]);

            //---------------------------------------------------------------------------------------------------

            //获取流程列表接口
            JObject findFlows = yzjif.FindFlows(token);
            JArray flows = null;
            if ((int)findFlows["data"]["total"] > 0 )
            {
                flows = (JArray)findFlows["data"]["list"];
            
            

            
                //定义需要抓取的字段
                string[] flowInstId = new string[flows.Count];
                string[] formCodeId = new string[flows.Count];
                string[] formDefId = new string[flows.Count];
                string[] formInstId = new string[flows.Count];
                //定义需要抓取的字段
                string[] FNumber = new string[flows.Count];//物料编码
                string[] FName = new string[flows.Count];//物料名称
                string[] FSpecification = new string[flows.Count];//规格型号
                string[] FMnemonicCode = new string[flows.Count];//助记码
                string[] FMaterialGroup = new string[flows.Count];//物料分组
                string[] F_CGY = new string[flows.Count];//采购员
                string[] FErpClsID = new string[flows.Count];//物料属性
                string[] FBaseUnitId = new string[flows.Count];//基本单位
                string[] FCategoryID = new string[flows.Count];//存货类别
                string[] FLENGTH = new string[flows.Count];//长
                string[] FWIDTH = new string[flows.Count];//宽
                string[] FIsKitting = new string[flows.Count];//是否关键件
                string[] FExpPeriod = new string[flows.Count];//保质期天数
                string[] FormDefId = new string[flows.Count]; //进出口后缀
                string[] YZJSERIALNUMBER = new string[flows.Count];//云之家流水号
                string[] deptInfo = new string[flows.Count];//申请人所属部门
                DateTime[] applyTimes = new DateTime[flows.Count];//申请日期
                //数组赋值
                for (int i = 0; i < flows.Count; i++)
                {

                    flowInstId[i] = flows[i]["flowInstId"].ToString();
                    formCodeId[i] = flows[i]["formCodeId"].ToString();
                    formDefId[i] = flows[i]["formDefId"].ToString();
                    formInstId[i] = flows[i]["formInstId"].ToString();

                    Console.WriteLine(flowInstId[i]);
                    Console.WriteLine(formCodeId[i]);
                    Console.WriteLine(formDefId[i]);
                    Console.WriteLine(formInstId[i]);



                }

                //-----------------------------------------------------------------------------------------------------------------------------------------------------------

                for (int i = 0; i < flows.Count; i++)
                {

                    //获取单据实例信息
                    JObject getFormCodeId = yzjif.ViewFormIns(token, formInstId[i], formCodeId[i]);
                    JObject FormCodeId = (JObject)getFormCodeId["data"]["formInfo"]["widgetMap"];


                    //判断云之家物料编码是否为空，为空执行
                    if (ConvertStringToDateTime(FormCodeId["_S_DATE"]["value"].ToString()) > time)
                    {
                        if (FormCodeId["Te_30"]["value"].ToString().Equals(""))
                        {
                            FNumber[i] = FormCodeId["Te_7"]["value"].ToString();//物料编码
                            FName[i] = FormCodeId["Ta_2"]["value"].ToString();//物料名称
                            FSpecification[i] = FormCodeId["Ta_3"]["value"].ToString();//规格型号
                            FMnemonicCode[i] = FormCodeId["Te_34"]["value"].ToString();//助记码
                            FMaterialGroup[i] = FormCodeId["Pw_3"]["value"][0]["title"].ToString();//物料分组
                            FMaterialGroup[i] = FMaterialGroup[i].Substring(FMaterialGroup[i].IndexOf("(") + 1, FMaterialGroup[i].IndexOf(")") - 1 - FMaterialGroup[i].IndexOf("("));
                            if (FormCodeId["Ps_0"]["personInfo"].ToString() != "[]")
                            {
                                F_CGY[i] = FormCodeId["Ps_0"]["personInfo"][0]["name"].ToString();//后端采购员
                            }
                            else
                            {
                                F_CGY[i] = null;
                            }
                            FErpClsID[i] = FormCodeId["Pw_6"]["value"][0]["title"].ToString();//物料属性
                            FBaseUnitId[i] = FormCodeId["Pw_5"]["value"][0]["title"].ToString();//计量单位
                            FCategoryID[i] = FormCodeId["Pw_7"]["value"][0]["title"].ToString();//存货类别
                            FLENGTH[i] = FormCodeId["Te_31"]["value"].ToString();//长
                            FWIDTH[i] = FormCodeId["Te_32"]["value"].ToString();//宽
                            FIsKitting[i] = FormCodeId["Ra_4"]["value"].ToString();//是否关键件
                            FExpPeriod[i] = FormCodeId["Nu_9"]["value"].ToString();//保质期天数
                            FormDefId[i] = FormCodeId["Pw_4"]["value"][0]["title"].ToString();//进出口后缀
                            YZJSERIALNUMBER[i] = FormCodeId["_S_SERIAL"]["value"].ToString();//云之家流水号
                            deptInfo[i] = FormCodeId["_S_DEPT"]["deptInfo"][0]["longName"].ToString();
                            deptInfo[i] = deptInfo[i].Substring(deptInfo[i].IndexOf("(") + 1, deptInfo[i].IndexOf(")") - 1 - deptInfo[i].IndexOf("("));
                            applyTimes[i] = ConvertStringToDateTime(FormCodeId["_S_DATE"]["value"].ToString());



                            //物料属性判断转换
                            switch (FErpClsID[i])
                            {
                                case "外购（经过商务审核）":
                                    FErpClsID[i] = "1";
                                    break;
                                case "辅材/定制件（无需商务审核）":
                                    FErpClsID[i] = "1";
                                    break;
                                case "自制":
                                    FErpClsID[i] = "2";
                                    break;
                                case "委外":
                                    FErpClsID[i] = "3";
                                    break;
                                case "TFT":
                                    FErpClsID[i] = "2";
                                    break;
                                case "客供料":
                                    FErpClsID[i] = "1";
                                    break;
                                case "资产":
                                    FErpClsID[i] = "10";
                                    break;
                                case "费用":
                                    FErpClsID[i] = "11";
                                    break;
                                default:
                                    break;
                            }

                            //是否关键件判断转换
                            if (FIsKitting[i].Equals("AaBaCcDd"))
                            {
                                FIsKitting[i] = "true";
                            }
                            else
                            {
                                FIsKitting[i] = "false";
                            }

                            //存货类别判断转换
                            switch (FCategoryID[i])
                            {
                                case "原材料(包含外购、代加工)":
                                    FCategoryID[i] = "CHLB01_SYS";
                                    break;
                                case "辅材（客供料包材专用）":
                                    FCategoryID[i] = "CHLB02_SYS";
                                    break;
                                case "自制半成品":
                                    FCategoryID[i] = "CHLB03_SYS";
                                    break;
                                case "自制成品":
                                    FCategoryID[i] = "CHLB05_SYS";
                                    break;
                                case "委外半成品":
                                    FCategoryID[i] = "CHLB04_SYS";
                                    break;
                                case "资产":
                                    FCategoryID[i] = "CHLB07_SYS";
                                    break;
                                case "费用":
                                    FCategoryID[i] = "CHLB06_SYS";
                                    break;
                                case "外协加工件":
                                    FCategoryID[i] = "CHLB01_SYS";
                                    break;
                                default:
                                    break;
                            }

                            //调用云之家接口，查询计量单位编码
                            FBaseUnitId[i] = kd.SelectUnitNumber(FBaseUnitId[i]);

                            //金蝶云创建物料，并返回物料编码
                            FNumber[i] = kd.SaveMaterial(FNumber[i], FName[i], FSpecification[i], FMnemonicCode[i], FMaterialGroup[i], F_CGY[i], FErpClsID[i], FBaseUnitId[i], FCategoryID[i], FLENGTH[i], FWIDTH[i], FIsKitting[i], FExpPeriod[i], FormDefId[i], YZJSERIALNUMBER[i], flowInstId[i], formCodeId[i], formDefId[i], formInstId[i], deptInfo[i]);

                            //云之家填写物料编码
                            //yzjif.ModifyInst(token, formCodeId[i], formDefId[i], formInstId[i], FNumber[i]);

                            //Console.WriteLine("物料编码:" + FNumber[i]);
                            //Console.WriteLine("物料名称:" + FName[i]);
                            //Console.WriteLine("规格型号:" + FSpecification[i]);
                            //Console.WriteLine("助记码:" + FMnemonicCode[i]);
                            //Console.WriteLine("物料分组:" + FMaterialGroup[i]);
                            //Console.WriteLine("采购员:" + F_CGY[i]);
                            //Console.WriteLine("物料属性:" + FErpClsID[i]);
                            //Console.WriteLine("基本单位:" + FBaseUnitId[i]);
                            //Console.WriteLine("存货类别:" + FCategoryID[i]);
                            //Console.WriteLine("长:" + FLENGTH[i]);
                            //Console.WriteLine("宽:" + FWIDTH[i]);
                            //Console.WriteLine("是否关键件:" + FIsKitting[i]);
                            //Console.WriteLine("保质期天数:" + FExpPeriod[i]);
                            Console.WriteLine(YZJSERIALNUMBER[i] + "创建成功!");
                        }
                        else
                        {
                            Console.WriteLine(YZJSERIALNUMBER[i] + "云之家该物料为靶材料号");
                        }
                    }
                    else
                    {
                        Console.WriteLine(YZJSERIALNUMBER[i] + "云之家物料申请日期位2020-12-15之前，无法抓取");
                    }




                }
                //调用金蝶云WebApi查询物料编码是否已存在

            }
            else
            {
                Console.WriteLine("云之家没有流程");
            }
            //-----------------------------------------------------------------------------------------------------------------------------------------------------------




            //-----------------------------------------------------------------------------------------------------------------------------------------------------------

            //修改单据实例
            //string modifyInst = null;
            //for (int i = 0; i < flows.Count; i++)
            //{
            //    if (formCodeId[i].Equals("3716f5fc19434ead8c2e62a8a4c0271f"))
            //    {
            //        modifyInst = yzjif.ModifyInst(token, formCodeId[i], formDefId[i], formInstId[i], "物料代码");
            //    }
            //}

            //---------------------------------------------------------------------------------------------------------------------------------------------

            //云之家同意操作
            //string agree = null;
            //for (int i = 0; i < flows.Count; i++)
            //{
            //    if (formCodeId[i].Equals("3716f5fc19434ead8c2e62a8a4c0271f"))
            //    {         
            //         agree = yzjif.Agree(token, flowInstId[i], formCodeId[i], formDefId[i], formInstId[i], "物料代码");
            //    }
            //}

            //-----------------------------------------------------------------------------------------------------------------------------------------------------------

            //云之家不同意操作
            //string retrunback = null;
            //for (int i = 0; i < flows.count; i++)
            //{
            //    if (formcodeid[i].equals("3716f5fc19434ead8c2e62a8a4c0271f"))
            //    {
            //        retrunback = yzjif.retrunback(token, flowinstid[i], formcodeid[i], formdefid[i], forminstid[i], "物料代码");
            //    }
            //}

            //-----------------------------------------------------------------------------------------------------------------------------------------------------------


            //Console.WriteLine(token);
            //Console.WriteLine(modifyInst);
            //Console.WriteLine(findFlows);
            //Console.WriteLine(getFormCodeId);
            //Console.WriteLine(retrunBack);
            Console.WriteLine("按任意按键结束");
            Console.ReadLine();
        }
    }
}



