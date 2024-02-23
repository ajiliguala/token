using System;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.List.PlugIn.Args;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.Base
{
	// Token: 0x0200003D RID: 61
	public class ShiftList : BaseControlList
	{
		// Token: 0x06000448 RID: 1096 RVA: 0x00035CF3 File Offset: 0x00033EF3
		public override void PrepareFilterParameter(FilterArgs e)
		{
			base.PrepareFilterParameter(e);
			if (this.View.OpenParameter is ListOpenParameter)
			{
				e.AppendQueryFilter(" FIsPrivate = '0'");
			}
		}
	}
}
