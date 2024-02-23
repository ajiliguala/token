using System;
using System.ComponentModel;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.Base
{
	// Token: 0x02000014 RID: 20
	[Description("模具编辑表单插件")]
	public class MouldEdit : BaseControlEdit
	{
		// Token: 0x06000227 RID: 551 RVA: 0x0001A09C File Offset: 0x0001829C
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			base.BeforeF7Select(e);
			string filter = string.Empty;
			string a;
			if ((a = e.FieldKey.ToUpper()) != null)
			{
				if (a == "FMOULDMODELID")
				{
					filter = "FDOCUMENTSTATUS='C' AND FFORBIDSTATUS='A' AND FIsAsset = '1'";
					e.ListFilterParameter.Filter = filter;
					return;
				}
				if (!(a == "FUSEDEPARTID"))
				{
					return;
				}
				filter = "FDOCUMENTSTATUS='C' AND FFORBIDSTATUS='A' AND FDeptProperty = '4866f13a3a3940b9b2fe47895a6e7cbe'";
				e.ListFilterParameter.Filter = filter;
			}
		}
	}
}
