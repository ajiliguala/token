using System;
using System.ComponentModel;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.FormElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;

namespace Kingdee.CX.PlugIn
{
	// Token: 0x02000009 RID: 9
	[Description("付款单审核修改SRM单据状态")]
	[HotUpdate]
	public class AP_PAYBILL_Service : AbstractOperationServicePlugIn
	{
		// Token: 0x06000068 RID: 104 RVA: 0x00003C80 File Offset: 0x00001E80
		public override void OnPreparePropertys(PreparePropertysEventArgs e)
		{
			base.OnPreparePropertys(e);
			e.FieldKeys.Add("F_PCQE_SRMDH");
		}

		// Token: 0x06000069 RID: 105 RVA: 0x00003C9C File Offset: 0x00001E9C
		public override void EndOperationTransaction(EndOperationTransactionArgs e)
		{
			base.EndOperationTransaction(e);
			bool flag = base.FormOperation.OperationId == FormOperation.Operation_Audit;
			if (flag)
			{
				foreach (DynamicObject dynamicObject in e.DataEntitys)
				{
					string text = Helper.ToStr(dynamicObject["F_PCQE_SRMDH"], 0);
					bool flag2 = !string.IsNullOrEmpty(text);
					if (flag2)
					{
						SRMStatus srmstatus = new SRMStatus();
						srmstatus.exectime = "付款审核完成";
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
