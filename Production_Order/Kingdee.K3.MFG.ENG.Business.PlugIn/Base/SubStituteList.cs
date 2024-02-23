using System;
using Kingdee.BOS.Core.List.PlugIn.Args;
using Kingdee.BOS.Util;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.Base
{
	// Token: 0x0200003F RID: 63
	public class SubStituteList : BaseControlList
	{
		// Token: 0x0600046F RID: 1135 RVA: 0x0003744C File Offset: 0x0003564C
		public override void PrepareFilterParameter(FilterArgs e)
		{
			base.PrepareFilterParameter(e);
			string text = Convert.ToString(this.View.OpenParameter.GetCustomParameter("IsSelectPLM"));
			if (StringUtils.EqualsIgnoreCase(text, "1"))
			{
				string text2 = " FREPLACESOURCE='1' ";
				e.FilterString = StringUtils.JoinFilterString(e.FilterString, text2, "AND");
				return;
			}
			string text3 = " FREPLACESOURCE='0' ";
			e.FilterString = StringUtils.JoinFilterString(e.FilterString, text3, "AND");
		}
	}
}
