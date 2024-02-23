using System;
using System.Collections.Generic;
using System.ComponentModel;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Resource;
using Kingdee.BOS.Util;
using Kingdee.K3.MFG.ServiceHelper.ENG;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm
{
	// Token: 0x020000B0 RID: 176
	[Description("工艺路线批改_表单插件")]
	public class RouteBatchEdit : AbstractDynamicFormPlugIn
	{
		// Token: 0x06000CDB RID: 3291 RVA: 0x00098838 File Offset: 0x00096A38
		public override void ButtonClick(ButtonClickEventArgs e)
		{
			base.ButtonClick(e);
			string a;
			if ((a = e.Key.ToUpper()) != null)
			{
				if (a == "FBTNCONFIRM")
				{
					this.DoConfirm(e);
					return;
				}
				if (!(a == "FBTNCANCEL"))
				{
					return;
				}
				this.DoCancel(e);
			}
		}

		// Token: 0x06000CDC RID: 3292 RVA: 0x000988B4 File Offset: 0x00096AB4
		private void DoConfirm(ButtonClickEventArgs e)
		{
			string fieldName = this.View.Model.GetValue("FFieldName") as string;
			object value = this.View.Model.GetValue("FFieldValue");
			decimal fieldValue = -1m;
			if (value != null)
			{
				fieldValue = Convert.ToDecimal(value);
			}
			if (ObjectUtils.IsNullOrEmptyOrWhiteSpace(fieldName))
			{
				this.View.ShowErrMessage(ResManager.LoadKDString("修改字段名称不能为空！", "015072000025098", 7, new object[0]), "", 0);
				e.Cancel = true;
			}
			if (fieldValue < 0m)
			{
				this.View.ShowErrMessage(ResManager.LoadKDString("修改字段值不能为空！", "015072000025099", 7, new object[0]), "", 0);
				e.Cancel = true;
			}
			List<long> lstSubEntryIds = this.View.OpenParameter.GetCustomParameter("fPKEntryIds") as List<long>;
			this.View.ShowMessage(ResManager.LoadKDString("您确定要对所选字段进行批量修改？", "015072000025100", 7, new object[0]), 1, delegate(MessageBoxResult cfm)
			{
				if (cfm != 1)
				{
					return;
				}
				this.DoExecuteBulkEdit(fieldName, fieldValue, lstSubEntryIds);
			}, "", 0);
		}

		// Token: 0x06000CDD RID: 3293 RVA: 0x000989F0 File Offset: 0x00096BF0
		private void DoExecuteBulkEdit(string fieldName, decimal fieldValue, List<long> lstSubEntryIds)
		{
			RouteServiceHelper.BulkEditActivityQty(base.Context, fieldName, fieldValue, lstSubEntryIds);
			this.View.ShowMessage(ResManager.LoadKDString("批改成功", "015072000025101", 7, new object[0]), 0);
			this.View.OpenParameter.SetCustomParameter("DataChanged", true);
		}

		// Token: 0x06000CDE RID: 3294 RVA: 0x00098A48 File Offset: 0x00096C48
		private void DoCancel(ButtonClickEventArgs e)
		{
			this.ListRefresh();
			this.View.Close();
		}

		// Token: 0x06000CDF RID: 3295 RVA: 0x00098A5C File Offset: 0x00096C5C
		private void ListRefresh()
		{
			object customParameter = this.View.OpenParameter.GetCustomParameter("DataChanged", true);
			if (customParameter != null && (bool)customParameter)
			{
				this.View.ParentFormView.Refresh();
				this.View.SendDynamicFormAction(this.View.ParentFormView);
			}
		}
	}
}
