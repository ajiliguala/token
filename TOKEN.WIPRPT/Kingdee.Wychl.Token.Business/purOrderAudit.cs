using System;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;

namespace Kingdee.Wychl.Token.Business
{
	// Token: 0x0200001C RID: 28
	public class purOrderAudit : AbstractOperationServicePlugIn
	{
		// Token: 0x06000058 RID: 88 RVA: 0x000093C9 File Offset: 0x000075C9
		public override void OnPreparePropertys(PreparePropertysEventArgs e)
		{
			base.OnPreparePropertys(e);
			e.FieldKeys.Add("FPOOrderFinance");
			e.FieldKeys.Add("FBillAllAmount");
		}

		// Token: 0x06000059 RID: 89 RVA: 0x000093F8 File Offset: 0x000075F8
		public override void AfterExecuteOperationTransaction(AfterExecuteOperationTransaction e)
		{
			base.AfterExecuteOperationTransaction(e);
			foreach (DynamicObject dynamicObject in e.DataEntitys)
			{
				serviceHelper.CreteItem(base.Context, dynamicObject["id"].ToString());
			}
		}
	}
}
