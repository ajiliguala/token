using System;
using System.Collections.Generic;
using System.ComponentModel;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Permission;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;

namespace Kingdee.Wychl.Token.Business
{
	// Token: 0x02000012 RID: 18
	[Description("报表组织隔离权限插件")]
	public class orgFilter : AbstractDynamicFormPlugIn
	{
		// Token: 0x06000036 RID: 54 RVA: 0x00004A54 File Offset: 0x00002C54
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			base.BeforeF7Select(e);
			bool flag = StringUtils.EqualsIgnoreCase(e.FieldKey, "F_ORGID");
			if (flag)
			{
				List<Organization> userOrg = PermissionServiceHelper.GetUserOrg(this.View.Context);
				bool flag2 = userOrg.Count > 0;
				if (flag2)
				{
					string text = string.Empty;
					foreach (Organization organization in userOrg)
					{
						text = text + organization.Id.ToString() + ",";
					}
					e.ListFilterParameter.Filter = "FORGID in(" + text.TrimEnd(new char[]
					{
						','
					}) + ")";
				}
				else
				{
					this.View.ShowErrMessage("没有组织权限！", "", 0);
					e.ListFilterParameter.Filter = " 1=2";
				}
			}
		}
	}
}
