using System;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Util;

namespace Kingdee.Wychl.Token.Business
{
	// Token: 0x02000014 RID: 20
	public class PlanBill : AbstractBillPlugIn
	{
		// Token: 0x0600003D RID: 61 RVA: 0x00005550 File Offset: 0x00003750
		public override void EntryBarItemClick(BarItemClickEventArgs e)
		{
			base.EntryBarItemClick(e);
			bool flag = StringUtils.EqualsIgnoreCase(e.BarItemKey, "tbBatchAdd");
			if (flag)
			{
				DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
				dynamicFormShowParameter.FormId = "PCQE_BATCHADD";
				dynamicFormShowParameter.PageId = SequentialGuid.NewGuid().ToString();
				dynamicFormShowParameter.OpenStyle.ShowType = 6;
				dynamicFormShowParameter.Resizable = true;
				base.View.ShowForm(dynamicFormShowParameter, new Action<FormResult>(this.batch));
			}
		}

		// Token: 0x0600003E RID: 62 RVA: 0x000055D8 File Offset: 0x000037D8
		private void batch(FormResult result)
		{
			bool flag = result.ReturnData != null;
			if (flag)
			{
				batchParm batchParm = (batchParm)result.ReturnData;
				DateTime startDate = batchParm.startDate;
				int days = batchParm.days;
				for (int i = 0; i < days; i++)
				{
					base.View.Model.CreateNewEntryRow("FEntity");
					base.View.Model.SetValue("F_DATE", startDate.AddDays((double)i), i);
				}
			}
		}

		// Token: 0x0600003F RID: 63 RVA: 0x00005664 File Offset: 0x00003864
		public override void AfterDoOperation(AfterDoOperationEventArgs e)
		{
			base.AfterDoOperation(e);
			bool flag = StringUtils.EqualsIgnoreCase(e.Operation.Operation, "Submit");
			if (flag)
			{
				base.View.UpdateView();
			}
		}
	}
}
