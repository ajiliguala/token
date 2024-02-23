using System;
using System.ComponentModel;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.CommonFilter.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.K3.MFG.ServiceHelper;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm
{
	// Token: 0x0200008B RID: 139
	[Description("ECN清理日志-过滤界面")]
	public class ECNClearLogFilter : AbstractCommonFilterPlugIn
	{
		// Token: 0x06000A6C RID: 2668 RVA: 0x00078E36 File Offset: 0x00077036
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
			(this.Model as ICommonFilterModelService).FormId = "ENG_ECNClearLog";
		}

		// Token: 0x06000A6D RID: 2669 RVA: 0x00078E54 File Offset: 0x00077054
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			base.BeforeF7Select(e);
			string a;
			if ((a = e.FieldKey.ToUpperInvariant()) != null)
			{
				if (!(a == "FCHANGEORGID"))
				{
					return;
				}
				bool flag = MFGServiceHelper.IsEnabledForbidOrgQuery(base.Context);
				if (flag)
				{
					e.IsShowUsed = false;
				}
			}
		}
	}
}
