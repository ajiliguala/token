using System;
using System.ComponentModel;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Resource;
using Kingdee.BOS.Util;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm
{
	// Token: 0x02000088 RID: 136
	[Description("工程变更单新增N行子项")]
	public class EcnAddNItemEdit : AbstractDynamicFormPlugIn
	{
		// Token: 0x06000A4B RID: 2635 RVA: 0x000780A0 File Offset: 0x000762A0
		public override void ButtonClick(ButtonClickEventArgs e)
		{
			base.ButtonClick(e);
			string a;
			if ((a = e.Key.ToUpperInvariant()) != null)
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
					int num = 0;
					object value = this.Model.GetValue("FNumber");
					if (ObjectUtils.IsNullOrEmptyOrWhiteSpace(value) || !int.TryParse(value.ToString(), out num) || num <= 0)
					{
						this.View.ShowErrMessage(ResManager.LoadKDString("输入数据不合法，请输入大于0的正整数！", "0151515153499000022395", 7, new object[0]), "", 0);
						e.Cancel = true;
						return;
					}
					if (num > 2000)
					{
						this.View.ShowErrMessage(ResManager.LoadKDString("请输入大于0小于2000的正整数！", "0151515153499030042479", 7, new object[0]), "", 0);
						e.Cancel = true;
						return;
					}
					FormResult formResult2 = new FormResult(num);
					this.View.ReturnToParentWindow(formResult2);
					this.View.Close();
					return;
				}
			}
		}
	}
}
