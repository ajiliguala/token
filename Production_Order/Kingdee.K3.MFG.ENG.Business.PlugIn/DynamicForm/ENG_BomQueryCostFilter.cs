using System;
using System.ComponentModel;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.CommonFilter.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm
{
	// Token: 0x02000094 RID: 148
	[Description("BOM成本查询过滤")]
	public class ENG_BomQueryCostFilter : AbstractCommonFilterPlugIn
	{
		// Token: 0x06000AC5 RID: 2757 RVA: 0x0007C12B File Offset: 0x0007A32B
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
			(this.Model as ICommonFilterModelService).FormId = "ENG_BomQueryCost";
		}

		// Token: 0x04000510 RID: 1296
		private const string ParentForm_TopEntityKey = "FTopEntity";
	}
}
