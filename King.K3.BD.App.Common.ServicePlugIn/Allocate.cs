using Kingdee.BOS.App.Data;
//服务端
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using System;
//校验器
using System.ComponentModel;

namespace King.K3.BD.App.Common.ServicePlugIn.Material
{
    [Description("物料分配操作自动更新供应来源")]
    [Kingdee.BOS.Util.HotUpdate]
    public class Allocate : AbstractOperationServicePlugIn
    {
        public override void AfterExecuteOperationTransaction(AfterExecuteOperationTransaction e)
        {

            foreach (DynamicObject entity in e.DataEntitys)
            {
                base.AfterExecuteOperationTransaction(e);
                string FNUMBER = Convert.ToString(entity["Number"]);
                string sql = $@"/*dialect*/
UPDATE T_BD_MATERIALPLAN SET  FSUPPLYSOURCEID = 
CASE 
WHEN FUSEORGID = 1023812 THEN  1194398  
WHEN FUSEORGID = 1023805 THEN  1194394 END  
FROM T_BD_MATERIALPLAN as mlp
INNER JOIN T_BD_MATERIAL AS ml ON ml.FMATERIALID = mlp.FMATERIALID
INNER JOIN T_BD_MATERIALBASE AS mlb ON mlb.FMATERIALID = ml.FMATERIALID
WHERE ml.FNUMBER = '{FNUMBER}'  AND FERPCLSID = 1";
                DBUtils.Execute(this.Context, sql);
            }
        }
    }
}
