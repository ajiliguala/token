using System;
using System.ComponentModel;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;

namespace Kingdee.Wychl.Token.Business
{
	// Token: 0x0200001B RID: 27
	[Description("生产入库单、生产退料单审核更新不良品拆解单插件")]
	public class proUnAudit : AbstractOperationServicePlugIn
	{
		// Token: 0x06000055 RID: 85 RVA: 0x00009249 File Offset: 0x00007449
		public override void OnPreparePropertys(PreparePropertysEventArgs e)
		{
			base.OnPreparePropertys(e);
			e.FieldKeys.Add("FEntity");
			e.FieldKeys.Add("FApproveDate");
			e.FieldKeys.Add("F_PAEZ_BILLID");
		}

		// Token: 0x06000056 RID: 86 RVA: 0x00009288 File Offset: 0x00007488
		public override void AfterExecuteOperationTransaction(AfterExecuteOperationTransaction e)
		{
			base.AfterExecuteOperationTransaction(e);
			foreach (DynamicObject dynamicObject in e.DataEntitys)
			{
				DynamicObjectCollection dynamicObjectCollection = dynamicObject["Entity"] as DynamicObjectCollection;
				string text = string.Empty;
				foreach (DynamicObject dynamicObject2 in dynamicObjectCollection)
				{
					bool flag = !ObjectUtils.IsNullOrEmptyOrWhiteSpace(dynamicObject2["F_PAEZ_BILLID"]);
					if (flag)
					{
						text = text + dynamicObject2["F_PAEZ_BILLID"].ToString() + ",";
					}
				}
				bool flag2 = !ObjectUtils.IsNullOrEmptyOrWhiteSpace(text);
				if (flag2)
				{
					text = text.TrimEnd(new char[]
					{
						','
					});
					string text2 = string.Concat(new object[]
					{
						"/*dialect*/update PAEZ_BLPCJ set F_PAEZ_STATUS='C',FAUDITDATE='",
						dynamicObject["ApproveDate"],
						"' where FID IN (",
						text,
						")"
					});
					DBServiceHelper.Execute(base.Context, text2);
				}
			}
		}
	}
}
