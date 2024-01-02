using System;
using System.ComponentModel;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;

namespace King.K3.BD.App.Common.ServicePlugIn.Material
{
	// Token: 0x02000007 RID: 7
	[Description("物料分配操作自动更新供应来源")]
	[HotUpdate]
	public class Allocate : AbstractOperationServicePlugIn
	{
		// Token: 0x0600001B RID: 27 RVA: 0x00003DEC File Offset: 0x00001FEC
		public override void AfterExecuteOperationTransaction(AfterExecuteOperationTransaction e)
		{
			foreach (DynamicObject dynamicObject in e.DataEntitys)
			{
				base.AfterExecuteOperationTransaction(e);
				string str = Convert.ToString(dynamicObject["Number"]);
				string text = "/*dialect*/\r\nUPDATE T_BD_MATERIALPLAN SET  FSUPPLYSOURCEID = \r\nCASE \r\nWHEN FUSEORGID = 1023812 THEN  1194398  \r\nWHEN FUSEORGID = 1023805 THEN  1194394 END  \r\nFROM T_BD_MATERIALPLAN as mlp\r\nINNER JOIN T_BD_MATERIAL AS ml ON ml.FMATERIALID = mlp.FMATERIALID\r\nINNER JOIN T_BD_MATERIALBASE AS mlb ON mlb.FMATERIALID = ml.FMATERIALID\r\nWHERE ml.FNUMBER = '" + str + "'  AND FERPCLSID = 1";
				DBUtils.Execute(base.Context, text);
			}
		}
	}
}
