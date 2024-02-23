using System;
using System.ComponentModel;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm
{
	// Token: 0x02000059 RID: 89
	[Description("替代方案批量设置用户参数表单插件")]
	public class BatchSubStitueUserParamEdit : AbstractDynamicFormPlugIn
	{
		// Token: 0x0600069B RID: 1691 RVA: 0x0004E16C File Offset: 0x0004C36C
		public override void AfterBindData(EventArgs e)
		{
			base.AfterBindData(e);
			bool enabled = (bool)this.Model.GetValue("FISUpdateVersion");
			this.View.GetControl("FNewBomDStatus").Enabled = enabled;
		}

		// Token: 0x0600069C RID: 1692 RVA: 0x0004E1AC File Offset: 0x0004C3AC
		public override void DataChanged(DataChangedEventArgs e)
		{
			base.DataChanged(e);
			string a;
			if ((a = e.Field.Key.ToUpperInvariant()) != null)
			{
				if (!(a == "FISUPDATEVERSION"))
				{
					return;
				}
				bool enabled = (bool)this.Model.GetValue("FISUpdateVersion");
				this.View.GetControl("FNewBomDStatus").Enabled = enabled;
			}
		}
	}
}
