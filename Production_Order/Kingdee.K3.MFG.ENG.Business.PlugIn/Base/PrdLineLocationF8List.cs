using System;
using System.ComponentModel;
using Kingdee.BOS.Core.List.PlugIn.Args;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.Base
{
	// Token: 0x0200002D RID: 45
	[Description("生产线工位F8_列表插件")]
	public class PrdLineLocationF8List : BaseControlList
	{
		// Token: 0x0600030B RID: 779 RVA: 0x00023758 File Offset: 0x00021958
		public override void PrepareFilterParameter(FilterArgs e)
		{
			if (!e.SortString.ToUpper().Contains("FPRODUCTLINEID.FNUMBER"))
			{
				e.AppendQueryOrderby("FProductLineId.FNumber");
			}
			if (!e.SortString.ToUpper().Contains("FLOCATIONCODE"))
			{
				e.AppendQueryOrderby("FLocationCode");
			}
		}
	}
}
