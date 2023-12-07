using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Util;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;

namespace King.K3.BD.App.Common.ServicePlugIn.Material
{
    [Description("获取云之家审批信息")]
    [HotUpdate]
    public class GetYunzhijia : AbstractListPlugIn
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="timeStamp"></param>13位时间戳格式日期
        /// <returns></returns>
        //13位时间戳转日期
        private static DateTime ConvertStringToDateTime(string timeStamp)
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long lTime = long.Parse(timeStamp + "0000");
            TimeSpan toNow = new TimeSpan(lTime);
            return dtStart.Add(toNow);
        }

        public override void BarItemClick(BarItemClickEventArgs e)
        {
            base.BarItemClick(e);




            if (e.BarItemKey.Equals("King_yunzhijia"))
            {
                yunzhijiaInterface yzjif = new yunzhijiaInterface();
                KingdeeWebApi kdapi = new KingdeeWebApi();

                int result = kdapi.SelectYwy(this.Context, this.Context.UserName);

                IOperationResult opResult = new OperationResult();

                if (result == 0)
                {
                    opResult.OperateResult.Add(new OperateResult()
                    {
                        Name = "信息提示",
                        //成功状态，提示
                        Message = "没有权限!",
                        SuccessStatus = false
                    });

                }
                else
                {
                    //opResult.OperateResult.Add(new OperateResult()
                    //{
                    //    Name = "信息提示",
                    //    //成功状态，提示
                    //    Message = "有权限",
                    //    SuccessStatus = true
                    //});

                    //定义时间2020-12-15 10:00:00
                    DateTime time = new DateTime(2020, 12, 15, 10, 00, 00);

                    // 获取accessToken
                    string token = yzjif.GetToken();

                    //获取流程列表接口
                    JObject findFlows = yzjif.FindFlows(token, kdapi.SelectYzjUserId(this.Context, this.Context.UserName));
                    //JArray flows = null;

                    if ((int)findFlows["data"]["total"] > 0)
                    {
                        JArray flows = (JArray)findFlows["data"]["list"];

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
                        string[] KFCGY = new string[flows.Count];//采购员
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
                        string[] MOQ = new string[flows.Count];//最小订货量
                        string[] MPQ = new string[flows.Count];//最小包装量
                        string[] LT = new string[flows.Count];//LT
                        DateTime[] applyTimes = new DateTime[flows.Count];//申请日期
                                                                          //数组赋值
                        for (int i = 0; i < flows.Count; i++)
                        {

                            flowInstId[i] = flows[i]["flowInstId"].ToString();
                            formCodeId[i] = flows[i]["formCodeId"].ToString();
                            formDefId[i] = flows[i]["formDefId"].ToString();
                            formInstId[i] = flows[i]["formInstId"].ToString();

                            //获取单据实例信息
                            JObject getFormCodeId = yzjif.ViewFormInst(token, formInstId[i], formCodeId[i]);
                            JObject FormCodeId = (JObject)getFormCodeId["data"]["formInfo"]["widgetMap"];


                            //判断云之家物料申请日期是否大于2020-12-15 10:00:00
                            if (ConvertStringToDateTime(FormCodeId["_S_DATE"]["value"].ToString()) > time)
                            {
                                FMaterialGroup[i] = FormCodeId["Pw_3"]["value"][0]["title"].ToString();//物料分组
                                FMaterialGroup[i] = FMaterialGroup[i].Substring(FMaterialGroup[i].IndexOf("(") + 1, FMaterialGroup[i].IndexOf(")") - 1 - FMaterialGroup[i].IndexOf("("));
                                //靶材尺寸是否为空
                                if (FMaterialGroup[i] != "04.07.0000")
                                {
                                    FNumber[i] = FormCodeId["Te_7"]["value"].ToString();//物料编码
                                    FName[i] = FormCodeId["Ta_2"]["value"].ToString();//物料名称
                                    FSpecification[i] = FormCodeId["Ta_3"]["value"].ToString();//规格型号
                                    FMnemonicCode[i] = FormCodeId["Te_34"]["value"].ToString();//助记码
                                    if (FormCodeId["Ps_0"]["personInfo"].ToString() != "[]")
                                    {
                                        F_CGY[i] = FormCodeId["Ps_0"]["personInfo"][0]["name"].ToString();//后端采购员
                                    }
                                    else
                                    {
                                        KFCGY[i] = null;
                                    }
                                    if (FormCodeId["Ps_2"]["personInfo"].ToString() != "[]")
                                    {
                                        KFCGY[i] = FormCodeId["Ps_2"]["personInfo"][0]["name"].ToString();//前端采购员
                                    }
                                    else
                                    {
                                        KFCGY[i] = null;
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
                                    deptInfo[i] = FormCodeId["_S_DEPT"]["deptInfo"][0]["longName"].ToString();//云之家申请人所属公司
                                    deptInfo[i] = deptInfo[i].Substring(deptInfo[i].IndexOf("(") + 1, deptInfo[i].IndexOf(")") - 1 - deptInfo[i].IndexOf("("));
                                    applyTimes[i] = ConvertStringToDateTime(FormCodeId["_S_DATE"]["value"].ToString());//申请日期
                                    MPQ[i] = FormCodeId["Te_36"]["value"].ToString();
                                    MOQ[i] = FormCodeId["Te_35"]["value"].ToString();
                                    LT[i] = FormCodeId["Te_37"]["value"].ToString();



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
                                            if (FCategoryID[i].Equals("原材料(包含外购、代加工)"))
                                            {
                                                FErpClsID[i] = "1";
                                            }
                                            else
                                            {
                                                FErpClsID[i] = "2";
                                            }
                                            break;
                                        case "客供料":
                                            FErpClsID[i] = "1";
                                            break;
                                        case "资产":
                                            FErpClsID[i] = "10";
                                            break;
                                        case "费用":
                                            if (FMaterialGroup[i].Substring(0, 2) == "12")
                                            {
                                                FErpClsID[i] = "6";
                                            }
                                            else
                                            {
                                                FErpClsID[i] = "11";
                                            }
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

                                    //调用金蝶云接口，查询计量单位编码
                                    FBaseUnitId[i] = kdapi.SelectUnitNumber(this.Context, FBaseUnitId[i]);

                                    //查询云之家物料是否已在系统创建
                                    if (kdapi.SelectMaterial(this.Context, YZJSERIALNUMBER[i]) == 0)
                                    {
                                        //金蝶云创建物料，并返回物料编码
                                        FNumber[i] = kdapi.DraftMaterial(this.Context, FNumber[i], FName[i], FSpecification[i], FMnemonicCode[i], FMaterialGroup[i], F_CGY[i], FErpClsID[i], FBaseUnitId[i], FCategoryID[i], FLENGTH[i], FWIDTH[i], FIsKitting[i], FExpPeriod[i], FormDefId[i], YZJSERIALNUMBER[i], flowInstId[i], formCodeId[i], formDefId[i], formInstId[i], deptInfo[i], MPQ[i], MOQ[i], LT[i], KFCGY[i]);
                                        opResult.OperateResult.Add(new OperateResult()
                                        {
                                            Name = "信息提示",
                                            //成功状态，提示
                                            Message = YZJSERIALNUMBER[i] + "创建成功!",
                                            SuccessStatus = true
                                        });
                                    }
                                    else
                                    {
                                        opResult.OperateResult.Add(new OperateResult()
                                        {
                                            Name = "信息提示",
                                            //成功状态，提示
                                            Message = YZJSERIALNUMBER[i] + "已在系统创建!",
                                            SuccessStatus = false
                                        });
                                    }
                                }
                                else
                                {
                                    opResult.OperateResult.Add(new OperateResult()
                                    {
                                        Name = "信息提示",
                                        //成功状态，提示
                                        Message = YZJSERIALNUMBER[i] + "云之家该物料为靶材料号!",
                                        SuccessStatus = false
                                    });
                                }
                            }
                            else
                            {
                                opResult.OperateResult.Add(new OperateResult()
                                {
                                    Name = "信息提示",
                                    //成功状态，提示
                                    Message = YZJSERIALNUMBER[i] + "云之家物料申请日期位2020-12-15之前，无法抓取!",
                                    SuccessStatus = false
                                });
                            }




                        }
                        //调用金蝶云WebApi查询物料编码是否已存在

                    }
                    else
                    {
                        opResult.OperateResult.Add(new OperateResult()
                        {
                            Name = "信息提示",
                            //成功状态，提示
                            Message = "云之家没有流程!",
                            SuccessStatus = false
                        });
                    }
                }

                this.View.ShowOperateResult(opResult.OperateResult);
                this.View.Refresh();
            }
        }
    }
}
