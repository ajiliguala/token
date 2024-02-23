using System;
using System.ComponentModel;
using System.Text;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.Base
{
	// Token: 0x02000015 RID: 21
	[Description("模具产品组合编辑表单插件")]
	public class MouldProdMixEdit : BaseControlEdit
	{
		// Token: 0x06000229 RID: 553 RVA: 0x0001A110 File Offset: 0x00018310
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			base.BeforeF7Select(e);
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder = stringBuilder.Append("FDOCUMENTSTATUS='C' AND FFORBIDSTATUS='A'");
			string a;
			if ((a = e.FieldKey.ToUpper()) != null)
			{
				if (a == "FMOULDTYPEID")
				{
					stringBuilder.Append("AND FIsAsset = '1'");
					e.ListFilterParameter.Filter = stringBuilder.ToString();
					return;
				}
				if (a == "FPROCESSID")
				{
					e.ListFilterParameter.Filter = stringBuilder.ToString();
					return;
				}
				if (!(a == "FPRODUCTID"))
				{
					return;
				}
				stringBuilder.Append("AND FErpClsID = '2'");
				e.ListFilterParameter.Filter = stringBuilder.ToString();
			}
		}
	}
}
