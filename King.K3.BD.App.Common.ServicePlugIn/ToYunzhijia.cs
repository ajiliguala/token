using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;

namespace King.K3.BD.App.Common.ServicePlugIn.Material
{
    [Description("物料审核时，同步审批云之家")]
    [HotUpdate]
    public class ToYunzhijia : AbstractOperationServicePlugIn
    {
        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
            e.FieldKeys.Add("F_KING_YZJSERIALNUMBER");
            e.FieldKeys.Add("F_KING_FLOWINSTID");
            e.FieldKeys.Add("F_KING_FORMCODEID");
            e.FieldKeys.Add("F_KING_FORMDEFID");
            e.FieldKeys.Add("F_KING_FORMINSTID");
            e.FieldKeys.Add("FNumber");
            e.FieldKeys.Add("F_King_ISApprover");
        }

        public override void BeginOperationTransaction(BeginOperationTransactionArgs e)
        {
            base.BeginOperationTransaction(e);

            KingdeeWebApi kdapi = new KingdeeWebApi();
            yunzhijiaInterface yzjif = new yunzhijiaInterface();
            string token = yzjif.GetToken();

            foreach (DynamicObject entity in e.DataEntitys)
            {
                if (entity["F_King_ISApprover"].ToString() == "False")
                {
                    string YZJSERIALNUMBER = entity["F_KING_YZJSERIALNUMBER"].ToString();
                    string FLOWINSTID = entity["F_KING_FLOWINSTID"].ToString();
                    string FORMCODEID = entity["F_KING_FORMCODEID"].ToString();
                    string FORMDEFID = entity["F_KING_FORMDEFID"].ToString();
                    string FORMINSTID = entity["F_KING_FORMINSTID"].ToString();
                    string FNUMBER = entity["Number"].ToString();
                    if (FLOWINSTID.IsNullOrEmptyOrWhiteSpace() || FORMCODEID.IsNullOrEmptyOrWhiteSpace() || FORMDEFID.IsNullOrEmptyOrWhiteSpace() || FORMINSTID.IsNullOrEmptyOrWhiteSpace())
                    {


                    }
                    else
                    {
                        String yzjUserId = kdapi.SelectYzjUserId(this.Context, this.Context.UserName).ToString();
                        //yzjif.ModifyInst(token, yzjUserId, FORMCODEID, FORMDEFID, FORMINSTID, FNUMBER);
                        //JObject findFlows = yzjif.FindFlows(token, kdapi.SelectYzjUserId(this.Context, this.Context.UserName));
                        string result = yzjif.Agree(token, FLOWINSTID, FORMCODEID, FORMDEFID, FORMINSTID, FNUMBER, yzjUserId);
                        JObject obresult = JObject.Parse(result);
                        if (obresult["success"].ToString().Equals("False"))
                        {

                        }
                        else
                        {
                            entity["F_King_ISApprover"] = 1;
                        }

                    }
                }



            }
        }
    }

}
