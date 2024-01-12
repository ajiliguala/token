using System;
using System.ComponentModel;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.FormElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;

namespace Kingdee.CX.PlugIn
{
	// Token: 0x0200000B RID: 11
	[Description("付款申请单审核修改SRM单据状态")]
	[HotUpdate]
	public class CN_PAYAPPLY_Service : AbstractOperationServicePlugIn
	{
		// Token: 0x06000070 RID: 112 RVA: 0x00004328 File Offset: 0x00002528
		public override void OnPreparePropertys(PreparePropertysEventArgs e)
		{
			base.OnPreparePropertys(e);
			e.FieldKeys.Add("FTHIRDBILLNO");
		}

		// Token: 0x06000071 RID: 113 RVA: 0x00004344 File Offset: 0x00002544
		public override void EndOperationTransaction(EndOperationTransactionArgs e)
		{
			base.EndOperationTransaction(e);
			bool flag = base.FormOperation.OperationId == FormOperation.Operation_Audit;
			if (flag)
			{
				foreach (DynamicObject dynamicObject in e.DataEntitys)
				{
					string text = Helper.ToStr(dynamicObject["FTHIRDBILLNO"], 0);
					bool flag2 = !string.IsNullOrEmpty(text);
					if (flag2)
					{
						SRMStatus srmstatus = new SRMStatus();
						srmstatus.exectime = "付款申请审核完成";
						srmstatus.company = "芜湖长信科技股份有限公司";
						srmstatus.vendid = "";
						srmstatus.stmtnums = "";
						srmstatus.paynums = text;
						UpdateSrmStatus_Service.SendToSrm(base.Context, srmstatus);
					}
				}
			}
		}
	}
}
