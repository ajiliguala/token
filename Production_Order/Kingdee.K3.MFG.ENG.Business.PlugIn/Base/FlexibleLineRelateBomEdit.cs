using System;
using System.ComponentModel;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.Base
{
	// Token: 0x0200001E RID: 30
	[Description("柔性产线与物料关系表单插件")]
	public class FlexibleLineRelateBomEdit : BaseControlEdit
	{
		// Token: 0x060002A7 RID: 679 RVA: 0x0001F578 File Offset: 0x0001D778
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			base.BeforeF7Select(e);
			e.CustomParams.Add("WorkCenterType", "D");
		}
	}
}
