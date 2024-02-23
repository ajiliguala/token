using System;
using System.ComponentModel;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Util;
using Kingdee.K3.MFG.BusinessCommon.BillPlugIn;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm
{
	// Token: 0x02000092 RID: 146
	[Description("工程管理参数表单构建插件")]
	public class ENGParameterBulider : AbstractMFGBillBuilderPlugIn
	{
		// Token: 0x06000ABD RID: 2749 RVA: 0x0007BCE5 File Offset: 0x00079EE5
		public override void CreateControl(CreateControlEventArgs e)
		{
			if (StringUtils.EqualsIgnoreCase(e.ControlAppearance.Key, "FBomModifyLogConfig"))
			{
				e.Control.Put("showFilterRow", true);
			}
			base.CreateControl(e);
		}
	}
}
