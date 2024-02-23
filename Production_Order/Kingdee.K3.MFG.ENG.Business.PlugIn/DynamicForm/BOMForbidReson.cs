using System;
using System.ComponentModel;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm
{
	// Token: 0x02000071 RID: 113
	[Description("BOM禁用原因插件")]
	public class BOMForbidReson : AbstractDynamicFormPlugIn
	{
		// Token: 0x06000826 RID: 2086 RVA: 0x00060E2C File Offset: 0x0005F02C
		public override void ButtonClick(ButtonClickEventArgs e)
		{
			base.ButtonClick(e);
			string a;
			if ((a = e.Key.ToUpperInvariant()) != null)
			{
				if (a == "FBTNOK")
				{
					string text = this.View.Model.GetValue("FForbidReason").ToString();
					FormResult formResult = new FormResult(text);
					this.View.ReturnToParentWindow(formResult);
					this.View.Close();
					return;
				}
				if (!(a == "FBTNCANCEL"))
				{
					return;
				}
				FormResult formResult2 = new FormResult("FBtnCancel");
				this.View.ReturnToParentWindow(formResult2);
				this.View.Close();
			}
		}
	}
}
