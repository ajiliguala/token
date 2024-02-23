using System;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.CommonFilter.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm
{
	// Token: 0x0200007B RID: 123
	public class BomQueryIntegrationFilter : AbstractCommonFilterPlugIn
	{
		// Token: 0x0600093F RID: 2367 RVA: 0x0006DBF8 File Offset: 0x0006BDF8
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
			(this.Model as ICommonFilterModelService).FormId = "ENG_BomQueryIntegration";
		}

		// Token: 0x0400046D RID: 1133
		private const string ParentForm_TopEntityKey = "FTopEntity";
	}
}
