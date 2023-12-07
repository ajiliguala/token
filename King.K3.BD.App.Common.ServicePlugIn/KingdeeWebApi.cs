using Kingdee.BOS;
using Kingdee.BOS.Util;
using Kingdee.BOS.WebApi.FormService;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace King.K3.BD.App.Common.ServicePlugIn.Material
{
    [HotUpdate]
    public class KingdeeWebApi
    {
        /// <summary>
        /// 调用金蝶云WebApi暂存存物料
        /// </summary>
        /// <param name="context"></param>//当前对象
        /// <param name="FNumber"></param>//物料编码
        /// <param name="FName"></param>//物料名称
        /// <param name="FSpecification"></param>//规格型号
        /// <param name="FMnemonicCode"></param>//助记码
        /// <param name="FMaterialGroupID"></param>//物料分组
        /// <param name="F_CGY"></param>//采购员
        /// <param name="FErpClsID"></param>//物料属性
        /// <param name="FBaseUnitId"></param>//基本单位
        /// <param name="FCategoryID"></param>存货类别
        /// <param name="FLENGTH"></param>//长
        /// <param name="FWIDTH"></param>//宽
        /// <param name="FIsKitting"></param>//是否关机键
        /// <param name="FExpPeriod"></param>保质期天数
        /// <param name="FormDefId"></param>//进出口后缀
        /// <param name="YZJSERIALNUMBER"></param>//云之家流水号
        /// <param name="flowInstId"></param>//流程实例id
        /// <param name="formCodeId"></param>//审批实例ID
        /// <param name="formDefId"></param>//表单模版ID
        /// <param name="formInstId"></param>//审批模板ID
        /// <param name="deptInfo"></param>//申请人所属部门
        /// <returns></returns>
        public string DraftMaterial(Context context, string FNumber, string FName, string FSpecification, string FMnemonicCode, string FMaterialGroupID, string F_CGY, string FErpClsID, string FBaseUnitId, string FCategoryID, string FLENGTH, string FWIDTH, string FIsKitting, string FExpPeriod, string FormDefId, string YZJSERIALNUMBER, string flowInstId, string formCodeId, string formDefId, string formInstId, string deptInfo, string MPQ, string MOQ, string LT, string KFCGY)
        {
            switch (FormDefId)
            {
                case "无":
                    FormDefId = null;
                    break;
                case "JL":
                    FormDefId = "-JL";
                    break;
                case "LL":
                    FormDefId = "-LL";
                    break;
                case "JK":
                    FormDefId = "-JK";
                    break;
                case "CK":
                    FormDefId = "-CK";
                    break;
                default:
                    break;
            }
            //——————————————————————————————————————————————————————————————————————
            //构建JSON
            JObject Material = new JObject();//Model
            JObject Model = new JObject();
            JObject MaterialBaseData = new JObject();
            JObject FCreateOrg = new JObject();
            JObject FUseOrg = new JObject();
            JObject MaterialStockData = new JObject();
            JObject MaterialProduceData = new JObject();
            JObject FBaseUnit = new JObject();
            JObject FCategory = new JObject();
            JObject FMaterialGroup = new JObject();
            JObject MaterialPlanDate = new JObject();


            Model["Model"] = Material;

            Material["FCreateOrgId"] = FCreateOrg;
            Material["FUseOrgId"] = FUseOrg;
            Material["SubHeadEntity5"] = MaterialProduceData;
            Material["SubHeadEntity1"] = MaterialStockData;
            Material["SubHeadEntity"] = MaterialBaseData;
            Material["SubHeadEntity4"] = MaterialPlanDate;

            FCreateOrg["FNumber"] = "999";
            FUseOrg["FNumber"] = "999";

            Material["FNumber"] = FNumber;//物料编码
            Material["FName"] = FName;//物料名称
            Material["FSpecification"] = FSpecification;//规格型号
            Material["FMnemonicCode"] = FMnemonicCode;//助记码
            Material["FMaterialGroup"] = FMaterialGroup;//物料分组
            Material["F_CGY"] = F_CGY;//采购员
            Material["F_KFCGY"] = KFCGY;//采购员
            Material["F_KING_YZJSERIALNUMBER"] = YZJSERIALNUMBER;//云之家流水号
            Material["F_KING_flowInstId"] = flowInstId;//云之家流水号
            Material["F_KING_formCodeId"] = formCodeId;//云之家流水号
            Material["F_KING_formDefId"] = formDefId;//云之家流水号
            Material["F_KING_formInstId"] = formInstId;//云之家流水号
            Material["F_KING_APDEPT"] = deptInfo; //云之家申请人所属部门
            Material["F_KING_EMPEXPSUFFIX"] = FormDefId;//进出口后缀

            MaterialBaseData["FErpClsID"] = FErpClsID;//物料属性
            MaterialBaseData["FBaseUnitId"] = FBaseUnit;//基本单位
            MaterialBaseData["FCategoryID"] = FCategory;//存货类别
            MaterialBaseData["FLENGTH"] = FLENGTH;//长
            MaterialBaseData["FWIDTH"] = FWIDTH;//宽

            FBaseUnit["FNumber"] = FBaseUnitId;
            FCategory["FNumber"] = FCategoryID;
            FMaterialGroup["FNumber"] = FMaterialGroupID;


            MaterialProduceData["FIsKitting"] = FIsKitting;//是否关键件

            MaterialStockData["FIsBatchManage"] = "true";//是否启用保质期
            MaterialStockData["FExpPeriod"] = FExpPeriod;//保质期天数

            MaterialPlanDate["FMinPOQty"] = MOQ;
            MaterialPlanDate["FIncreaseQty"] = MPQ;
            MaterialPlanDate["FFixLeadTime"] = LT;
            //——————————————————————————————————————————————————————————————————————


            JObject result = JObject.Parse(JsonConvert.SerializeObject(WebApiServiceCall.Draft(context, "BD_MATERIAL", Convert.ToString(Model))));

            string resutlFnumber = result["Result"]["ResponseStatus"]["SuccessEntitys"][0]["Number"].ToString();



            return resutlFnumber;
        }

        /// <summary>
        /// 调用金蝶云WebApi查询云之家流水号是否已存在
        /// </summary>
        /// <param name="context"></param>//当前对象
        /// <param name="F_KING_YZJSERIALNUMBER"></param>//云之家流水号
        /// <returns></returns>
        public int SelectMaterial(Context context, string F_KING_YZJSERIALNUMBER)
        {


            JObject model = new JObject();

            model["FormId"] = "BD_MATERIAL";
            model["FieldKeys"] = "F_KING_YZJSERIALNUMBER";
            model["FilterString"] = "F_KING_YZJSERIALNUMBER = '" + F_KING_YZJSERIALNUMBER + "'";
            //client.ExecuteBillQuery(Convert.ToString(model));

            int result = WebApiServiceCall.ExecuteBillQuery(context, Convert.ToString(model)).Count;

            return result;
        }

        /// <summary>
        /// 调用金蝶云WebApi查询单位编码
        /// </summary>
        /// <param name="context"></param>//当前对象
        /// <param name="FBaseUnitId"></param>//基本单位
        /// <returns></returns>
        public string SelectUnitNumber(Context context, string FBaseUnitId)
        {
            JObject model = new JObject();

            model["FormId"] = "BD_UNIT";
            model["FieldKeys"] = "FNUMBER";
            model["FilterString"] = "FNAME = '" + FBaseUnitId + "' AND FForbidStatus  = 'A' AND FDocumentStatus = 'C'  AND FIsBaseUnit = '1'";
            //client.ExecuteBillQuery(Convert.ToString(model));

            string result = WebApiServiceCall.ExecuteBillQuery(context, Convert.ToString(model))[0][0].ToString();

            return result;
        }

        /// <summary>
        /// 调用金蝶云WebApi查询金蝶云用户对应的云之家内码
        /// </summary>
        /// <param name="context"></param>//当前对象
        /// <param name="FUserName"></param>//用户ID
        /// <returns></returns>
        public string SelectYzjUserId(Context context, string FUserName)
        {
            string userId = null;

            JObject model = new JObject();
            model["FormId"] = "SEC_XTUser";
            model["FieldKeys"] = "FOpenID";
            model["FilterString"] = "FUserName = '" + FUserName + "'";

            var obj = WebApiServiceCall.ExecuteBillQuery(context, model.ToString());
            userId = obj[0][0].ToString();

            return userId;
        }

        /// <summary>
        /// 调用金蝶云WebApi查询金蝶云物料员是否存在
        /// </summary>
        /// <param name="context"></param>//当前对象
        /// <param name="FName"></param>//物料员
        /// <returns></returns>
        public int SelectYwy(Context context, string FName)
        {
            //string FName = null;

            JObject model = new JObject();
            model["FormId"] = "BD_OPERATOR";
            model["FieldKeys"] = "FOPERATORTYPE";
            model["FilterString"] = "FNAME = '" + FName + "'AND FOPERATORTYPE = 'WLY'";

            var result = WebApiServiceCall.ExecuteBillQuery(context, model.ToString());

            return result.Count;
        }
    }
}
