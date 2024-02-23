using System;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.CommonFilter.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm
{
	// Token: 0x02000079 RID: 121
	public class BomQueryForwardFilter : AbstractCommonFilterPlugIn
	{
		// Token: 0x0600092D RID: 2349 RVA: 0x0006D218 File Offset: 0x0006B418
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
			(this.Model as ICommonFilterModelService).FormId = "ENG_BomQueryForward2";
		}

		// Token: 0x04000464 RID: 1124
		private const string ParentForm_TopEntityKey = "FTopEntity";
	}
}
