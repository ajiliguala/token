using System;
using System.ComponentModel;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.K3.Core.MFG.Utils;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.Base
{
	// Token: 0x02000028 RID: 40
	[Description("生产线工位物料参数表单插件")]
	public class LineLocationBomParaEdit : BaseControlEdit
	{
		// Token: 0x060002FA RID: 762 RVA: 0x00022FB0 File Offset: 0x000211B0
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			base.BeforeF7Select(e);
			string fieldKey;
			if ((fieldKey = e.FieldKey) != null)
			{
				if (!(fieldKey == "FLineLocation"))
				{
					return;
				}
				string value = MFGBillUtil.GetValue<string>(this.Model, "FProductLineId", -1, null, null);
				if (!"".Equals(value) && value != null)
				{
					string text = string.Format(" t0.FPRODUCTLINEID = {0} ", value);
					e.ListFilterParameter.Filter = base.SqlAppendAnd(e.ListFilterParameter.Filter, text);
				}
			}
		}
	}
}
