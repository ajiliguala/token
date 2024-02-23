using System;
using System.Collections.Generic;
using System.ComponentModel;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Permission;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.K3.Core.MFG.ENG.ParamOption;
using Kingdee.K3.Core.MFG.Utils;
using Kingdee.K3.MFG.ServiceHelper;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.Base
{
	// Token: 0x02000049 RID: 73
	[Description("工作中心模板表单插件")]
	public class WorkCenterBaseEdit : BaseControlEdit
	{
		// Token: 0x060004FB RID: 1275 RVA: 0x0003CD73 File Offset: 0x0003AF73
		public override void AfterBindData(EventArgs e)
		{
			base.AfterBindData(e);
			this.SetIsSetWorkCal();
		}

		// Token: 0x060004FC RID: 1276 RVA: 0x0003CD84 File Offset: 0x0003AF84
		public override void ButtonClick(ButtonClickEventArgs e)
		{
			base.ButtonClick(e);
			string value = "FbtnViewWorkCalSetup".ToUpper();
			if (e.Key.Equals(value))
			{
				if (this.VaildateWCSViewPermission())
				{
					T_WorkCalSetupFormParam t_WorkCalSetupFormParam = new T_WorkCalSetupFormParam();
					t_WorkCalSetupFormParam.useOrg = MFGBillUtil.GetValue<long>(base.View.Model, "FUseOrgId", -1, 0L, null);
					t_WorkCalSetupFormParam.treeNodeId = new DiffCalendarOption.NodeInfo(3, (base.View.Model.GetPKValue() == null) ? 0L : Convert.ToInt64(base.View.Model.GetPKValue())).FullNodeId;
					MFGBillUtil.ShowForm(base.View, "ENG_WorkCalSetup", t_WorkCalSetupFormParam, delegate(FormResult result)
					{
					}, 0);
				}
				this.SetIsSetWorkCal();
			}
		}

		// Token: 0x060004FD RID: 1277 RVA: 0x0003CE58 File Offset: 0x0003B058
		private bool VaildateWCSViewPermission()
		{
			PermissionAuthResult permissionAuthResult = PermissionServiceHelper.FuncPermissionAuth(base.Context, new BusinessObject
			{
				Id = "ENG_WorkCalSetup",
				SubSystemId = base.View.Model.SubSytemId
			}, "6e44119a58cb4a8e86f6c385e14a17ad");
			if (!permissionAuthResult.Passed)
			{
				base.View.ShowErrMessage(ResManager.LoadKDString("需要配置工作日历设置的查看权限", "015072000001798", 7, new object[0]), ResManager.LoadKDString("没有查看权限", "015072000001801", 7, new object[0]), 0);
			}
			return permissionAuthResult.Passed;
		}

		// Token: 0x060004FE RID: 1278 RVA: 0x0003CEE4 File Offset: 0x0003B0E4
		private List<DynamicObject> GetWorkCalById(string workCenterID)
		{
			string text = string.Format("{0}='{1}' and {2} = {3}", new object[]
			{
				"FCalUserType",
				this.Model.BusinessInfo.GetForm().Id,
				"FCalUserId",
				workCenterID
			});
			List<SelectorItemInfo> list = new List<SelectorItemInfo>
			{
				new SelectorItemInfo("FID")
			};
			return MFGServiceHelper.GetBaseBillInfo(base.Context, "ENG_WorkCalCustom", list, text, "");
		}

		// Token: 0x060004FF RID: 1279 RVA: 0x0003CF68 File Offset: 0x0003B168
		private void SetIsSetWorkCal()
		{
			if (base.View.Model.GetPKValue() != null)
			{
				int num = Convert.ToInt32(base.View.Model.GetPKValue());
				if (num != 0)
				{
					List<DynamicObject> workCalById = this.GetWorkCalById(num.ToString());
					if (workCalById.Count > 0 && workCalById[0]["Id"] != null)
					{
						base.View.GetControl("FIsSetWorkCal").SetValue(true);
						return;
					}
					base.View.GetControl("FIsSetWorkCal").SetValue(false);
				}
			}
		}

		// Token: 0x04000235 RID: 565
		private const string FKey_FID = "FID";
	}
}
