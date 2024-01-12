using System;
using System.ComponentModel;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.FormElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;

namespace Kingdee.CX.PlugIn
{
	// Token: 0x02000007 RID: 7
	[Description("财务应付审核修改SRM单据状态")]
	[HotUpdate]
	public class AP_Payable_Service : AbstractOperationServicePlugIn
	{
		// Token: 0x06000060 RID: 96 RVA: 0x00003522 File Offset: 0x00001722
		public override void OnPreparePropertys(PreparePropertysEventArgs e)
		{
			base.OnPreparePropertys(e);
			e.FieldKeys.Add("FSUPPLIERID");
			e.FieldKeys.Add("FTHIRDBILLNO");
			e.FieldKeys.Add("FSetAccountType");
		}

		// Token: 0x06000061 RID: 97 RVA: 0x00003560 File Offset: 0x00001760
		public override void EndOperationTransaction(EndOperationTransactionArgs e)
		{
			base.EndOperationTransaction(e);
			bool flag = base.FormOperation.OperationId == FormOperation.Operation_Audit;
			if (flag)
			{
				foreach (DynamicObject dynamicObject in e.DataEntitys)
				{
					string a = Helper.ToStr(dynamicObject["FSetAccountType"], 0);
					string text = Helper.ToStr(dynamicObject["FTHIRDBILLNO"], 0);
					bool flag2 = a == "3" && !string.IsNullOrEmpty(text);
					if (flag2)
					{
						SRMStatus srmstatus = new SRMStatus();
						srmstatus.exectime = "财务应付审核完成";
						srmstatus.company = "芜湖长信科技股份有限公司";
						DynamicObject dynamicObject2 = dynamicObject["SUPPLIERID"] as DynamicObject;
						bool flag3 = !dynamicObject2.IsNullOrEmptyOrWhiteSpace();
						if (flag3)
						{
							srmstatus.vendid = Helper.ToStr(dynamicObject2["Number"], 0);
						}
						srmstatus.stmtnums = text;
						srmstatus.paynums = "";
						UpdateSrmStatus_Service.SendToSrm(base.Context, srmstatus);
					}
				}
			}
		}
	}
}
