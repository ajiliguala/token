using System;
using System.ComponentModel;
using Kingdee.BOS.Core.CommonFilter.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm
{
	// Token: 0x02000097 RID: 151
	[Description("设备状态变更日志动态表单-过滤")]
	public class EqmStatusChgLogListDymFilter : AbstractCommonFilterPlugIn
	{
		// Token: 0x06000AEF RID: 2799 RVA: 0x0007D911 File Offset: 0x0007BB11
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
		}

		// Token: 0x06000AF0 RID: 2800 RVA: 0x0007D91A File Offset: 0x0007BB1A
		public override void ButtonClick(ButtonClickEventArgs e)
		{
			base.ButtonClick(e);
		}
	}
}
