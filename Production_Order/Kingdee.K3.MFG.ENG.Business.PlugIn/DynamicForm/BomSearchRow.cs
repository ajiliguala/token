using System;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Resource;
using Kingdee.K3.Core.MFG.Utils;
using Kingdee.K3.MFG.BusinessCommon.BillPlugIn;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm
{
	// Token: 0x0200007C RID: 124
	public class BomSearchRow : AbstractMFGDynamicFormPlugIn
	{
		// Token: 0x06000941 RID: 2369 RVA: 0x0006DC20 File Offset: 0x0006BE20
		public override void ButtonClick(ButtonClickEventArgs e)
		{
			base.ButtonClick(e);
			string key;
			if ((key = e.Key) != null)
			{
				if (key == "FBTNFINDNEXT" || key == "FBTNFINDPREV")
				{
					this.Find(e.Key);
					return;
				}
				if (!(key == "FBTNCLOSE"))
				{
					return;
				}
				this.View.Close();
			}
		}

		// Token: 0x06000942 RID: 2370 RVA: 0x0006DC80 File Offset: 0x0006BE80
		private void Find(string eventKey)
		{
			string value = MFGBillUtil.GetValue<string>(this.View.Model, "FSearch", -1, null, null);
			if (string.IsNullOrWhiteSpace(value))
			{
				this.View.ShowMessage(ResManager.LoadKDString("请输入查找内容", "015072000012079", 7, new object[0]), 0);
				return;
			}
			(this.View.ParentFormView as IDynamicFormViewService).CustomEvents(this.View.PageId, eventKey, value);
			this.View.SendAynDynamicFormAction(this.View.ParentFormView);
		}
	}
}
