using System;
using System.ComponentModel;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Resource;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm
{
	// Token: 0x020000AD RID: 173
	[Description("产品模型BOM分录移动插件")]
	public class ProductModelBomEntryMoveToEdit : AbstractDynamicFormPlugIn
	{
		// Token: 0x06000C07 RID: 3079 RVA: 0x00089D98 File Offset: 0x00087F98
		public override void ButtonClick(ButtonClickEventArgs e)
		{
			base.ButtonClick(e);
			string a;
			if ((a = e.Key.ToUpper()) != null)
			{
				if (!(a == "FOK"))
				{
					if (!(a == "FCANCEL"))
					{
						return;
					}
					FormResult formResult = new FormResult("FCANCEL");
					this.View.ReturnToParentWindow(formResult);
					this.View.Close();
				}
				else
				{
					int num = Convert.ToInt32(this.View.Model.GetValue("FMoveLine"));
					int num2 = Convert.ToInt32(this.View.OpenParameter.GetCustomParameter("currentIndex"));
					int num3 = Convert.ToInt32(this.View.OpenParameter.GetCustomParameter("maxIndex"));
					if (num == num2 + 1)
					{
						this.View.ShowErrMessage(ResManager.LoadKDString("输入的要移动至位置和原位置相同，请重新输入！", "0151515153499000016575", 7, new object[0]), "", 0);
						e.Cancel = true;
						return;
					}
					if (num < 1 || num > num3 + 1)
					{
						this.View.ShowErrMessage(string.Format(ResManager.LoadKDString("输入的要移动至位置不合法，请重新输入，当前可输入范围为{0}—{1}", "0151515153499000016576", 7, new object[0]), 1, num3 + 1), "", 0);
						e.Cancel = true;
						return;
					}
					FormResult formResult2 = new FormResult(num - 1);
					this.View.ReturnToParentWindow(formResult2);
					this.View.Close();
					return;
				}
			}
		}
	}
}
