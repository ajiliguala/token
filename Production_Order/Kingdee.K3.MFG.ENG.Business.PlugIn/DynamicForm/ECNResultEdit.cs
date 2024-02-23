using System;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.K3.MFG.ENG.Business.PlugIn.Base;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm
{
	// Token: 0x02000091 RID: 145
	public class ECNResultEdit : BaseControlEdit
	{
		// Token: 0x06000ABA RID: 2746 RVA: 0x0007BB6C File Offset: 0x00079D6C
		public override void CreateNewData(BizDataEventArgs e)
		{
			DynamicObject dynamicObject = base.View.OpenParameter.GetCustomParameter("OrderDataObject", true) as DynamicObject;
			if (dynamicObject == null)
			{
				return;
			}
			e.BizDataObject = dynamicObject;
			e.IsExecuteRule = false;
			base.View.RuleContainer.Suspend();
			base.View.OpenParameter.Status = 1;
		}

		// Token: 0x06000ABB RID: 2747 RVA: 0x0007BBC8 File Offset: 0x00079DC8
		public override void AfterBindData(EventArgs e)
		{
			base.View.GetMainBarItem("tbSplitNew").Enabled = false;
			base.View.GetMainBarItem("tbNew").Enabled = false;
			base.View.GetMainBarItem("tbCopy").Enabled = false;
			base.View.GetMainBarItem("tbSplitSave").Enabled = false;
			base.View.GetMainBarItem("tbSplitSubmit").Enabled = false;
			base.View.GetMainBarItem("tbSubmit").Enabled = false;
			base.View.GetMainBarItem("tbReject").Enabled = false;
			base.View.GetMainBarItem("tbSplitApprove").Enabled = false;
			base.View.GetMainBarItem("tbApprove").Enabled = false;
			base.View.GetMainBarItem("tbForbid").Enabled = false;
			base.View.GetMainBarItem("tbEnable").Enabled = false;
			base.View.GetMainBarItem("tbAllocate").Enabled = false;
		}
	}
}
