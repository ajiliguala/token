using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Util;
using System.ComponentModel;

namespace King.K3.BD.App.Common.ServicePlugIn.Material
{
    [Description("物料删除时，同步退回云之家")]
    [HotUpdate]
    public class DelectMaterial : AbstractOperationServicePlugIn
    {
        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
            e.FieldKeys.Add("F_KING_flowInstId");
        }
        public override void BeginOperationTransaction(BeginOperationTransactionArgs e)
        {
            base.BeginOperationTransaction(e);

            //foreach (DynamicObject entity in e.DataEntitys)
            //{

            //    string flowInstId = entity["F_KING_flowInstId"].ToString();
            //    String yzjUserId = kdapi.SelectYzjUserId(this.Context, this.Context.UserName).ToString();
            //    yzjif.RetrunBack(token, flowInstId, yzjUserId);
            //}
        }
    }
}
