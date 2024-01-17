using System;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Util;

namespace Kingdee.Wychl.Token.Business
{
	// Token: 0x02000005 RID: 5
	public class batchAddBill : AbstractDynamicFormPlugIn
	{
		// Token: 0x0600000E RID: 14 RVA: 0x00002938 File Offset: 0x00000B38
		public override void ButtonClick(ButtonClickEventArgs e)
		{
			base.ButtonClick(e);
			bool flag = !e.Key.ToUpperInvariant().Equals("FOK");
			if (flag)
			{
				bool flag2 = e.Key.ToUpperInvariant().Equals("FCANCEL");
				if (flag2)
				{
					this.View.Close();
				}
			}
			else
			{
				bool flag3 = ObjectUtils.IsNullOrEmptyOrWhiteSpace(this.Model.GetValue("FSTART"));
				if (flag3)
				{
					this.View.ShowMessage("请录入开始日期", 0);
				}
				bool flag4 = Convert.ToInt32(this.Model.GetValue("FCOUNT")) < 1;
				if (flag4)
				{
					this.View.ShowMessage("请录入新增行数", 0);
				}
				batchParm batchParm = new batchParm();
				batchParm.startDate = Convert.ToDateTime(this.Model.GetValue("FSTART"));
				batchParm.days = Convert.ToInt32(this.Model.GetValue("FCOUNT"));
				this.View.ReturnToParentWindow(batchParm);
				this.View.Close();
			}
		}
	}
}
