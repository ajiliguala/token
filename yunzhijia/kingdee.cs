using Kingdee.BOS.WebApi.Client;
using Newtonsoft.Json.Linq;
using System;

namespace yunzhijia
{
    class Kingdee
    {
        //调用金蝶云WebApi保存物料
        public string SaveMaterial(string FNumber, string FName, string FSpecification, string FMnemonicCode, string FMaterialGroupID, string F_CGY, string FErpClsID, string FBaseUnitId, string FCategoryID, string FLENGTH, string FWIDTH, string FIsKitting, string FExpPeriod, string FormDefId, string YZJSERIALNUMBER, string flowInstId, string formCodeId, string formDefId, string formInstId, string deptInfo)
        {
            // 使用webapi引用组件Kingdee.BOS.WebApi.Client.dll
            K3CloudApiClient client = new K3CloudApiClient("https://whcxkj.ik3cloud.com/k3cloud/");
            var loginResult = client.ValidateLogin("20190918173757209", "圣凯", "token.123", 2052);
            var resultType = JObject.Parse(loginResult)["LoginResultType"].Value<int>();
            //登录结果类型等于1，代表登录成功

            string resutlFnumber = "";
            if (resultType == 1)
            {
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


                Model["Model"] = Material;

                Material["FCreateOrgId"] = FCreateOrg;
                Material["FUseOrgId"] = FUseOrg;
                Material["SubHeadEntity5"] = MaterialProduceData;
                Material["SubHeadEntity1"] = MaterialStockData;
                Material["SubHeadEntity"] = MaterialBaseData;

                FCreateOrg["FNumber"] = "999";
                FUseOrg["FNumber"] = "999";
               
                Material["FNumber"] = FNumber;//物料编码
                Material["FName"] = FName;//物料名称
                Material["FSpecification"] = FSpecification;//规格型号
                Material["FMnemonicCode"] = FMnemonicCode;//助记码
                Material["FMaterialGroup"] = FMaterialGroup;//物料分组
                Material["F_CGY"] = F_CGY;//采购员
                Material["F_KING_YZJSERIALNUMBER"] = YZJSERIALNUMBER;//云之家流水号
                Material["F_KING_flowInstId"] = flowInstId;//云之家流水号
                Material["F_KING_formCodeId"] = formCodeId;//云之家流水号
                Material["F_KING_formDefId"] = formDefId;//云之家流水号
                Material["F_KING_formInstId"] = formInstId;//云之家流水号
                Material["F_KING_APDEPT"] = deptInfo; //云之家申请人所属部门

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
                                                             //——————————————————————————————————————————————————————————————————————

                Console.WriteLine(resultType);


                JObject result = JObject.Parse(client.Draft("BD_MATERIAL", Convert.ToString(Model)));

                resutlFnumber = result["Result"]["ResponseStatus"]["SuccessEntitys"][0]["Number"].ToString();
            }

            switch (FormDefId)
            {
                case "无":
                    break;
                case "JL":
                    resutlFnumber = resutlFnumber + "-JL";
                    break;
                case "LL":
                    resutlFnumber = resutlFnumber + "-LL";
                    break;
                case "JK":
                    resutlFnumber = resutlFnumber + "-JK";
                    break;
                case "CK":
                    resutlFnumber = resutlFnumber + "-CK";
                    break;
                default:
                    break;
            }
            return resutlFnumber;
        }

        //调用金蝶云WebApi查询物料编码是否已存在
        //public int SelectMaterialNumber(string FNumber)
        //{
        //    K3CloudApiClient client = new K3CloudApiClient("https://whcxkj.test.ik3cloud.com/k3cloud/");
        //    var loginResult = client.ValidateLogin("20190918173832507", "圣凯", "token.123", 2052);
        //    var resultType = JObject.Parse(loginResult)["LoginResultType"].Value<int>();

        //    int result = 0;

        //    if (resultType == 1)
        //    {
        //        JObject model = new JObject();

        //        model["FormId"] = "BD_MATERIAL";
        //        model["FieldKeys"] = "FNUMBER";
        //        model["FilterString"] = "FNUMBER = '" + FNumber + "'";
        //        //client.ExecuteBillQuery(Convert.ToString(model));

        //        result = client.ExecuteBillQuery(Convert.ToString(model)).Count;
        //    }
        //    return result;
        //}

        //调用金蝶云WebApi查询单位编码
        public string SelectUnitNumber(string FBaseUnitId)
        {
            K3CloudApiClient client = new K3CloudApiClient("https://whcxkj.ik3cloud.com/k3cloud/");
            var loginResult = client.ValidateLogin("20190918173757209", "圣凯", "token.123", 2052);
            var resultType = JObject.Parse(loginResult)["LoginResultType"].Value<int>();

            string result = null;

            if (resultType == 1)
            {
                JObject model = new JObject();

                model["FormId"] = "BD_UNIT";
                model["FieldKeys"] = "FNUMBER";
                model["FilterString"] = "FNAME = '" + FBaseUnitId + "'";
                //client.ExecuteBillQuery(Convert.ToString(model));

                result = client.ExecuteBillQuery(Convert.ToString(model))[0][0].ToString();
            }
            return result;
        }
    }
}
