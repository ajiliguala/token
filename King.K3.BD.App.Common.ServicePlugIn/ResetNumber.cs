using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using System.ComponentModel;

namespace King.K3.BD.App.Common.ServicePlugIn.CardSplit
{
    [Description("卡片拆分重置卡片编码")]
    [Kingdee.BOS.Util.HotUpdate]
    public class ResetNumber : AbstractOperationServicePlugIn
    {
        public override void BeginOperationTransaction(BeginOperationTransactionArgs e)
        {

            base.BeginOperationTransaction(e);
            foreach (DynamicObject entity in e.DataEntitys)
            {
                if (entity["ASSETORGID_Id"].ToString().Equals("1") || entity["ASSETORGID_Id"].ToString().Equals("1431804"))
                {
                    var Newentity = entity["NewEntity"] as DynamicObjectCollection;

                    for (int i = 0; i < Newentity.Count; i++)
                    {
                        Newentity[i]["Number"] = entity["Number"] + "-拆" + (i + 1).ToString().PadLeft(4, '0');

                        var NewDetailEntity = Newentity[i]["NewDetailEntity"] as DynamicObjectCollection;
                        for (int j = 0; j < NewDetailEntity.Count; j++)
                        {
                            NewDetailEntity[j]["NewAssetNO"] = Newentity[i]["Number"];
                        }
                    }
                }
            }
        }
    }
}
