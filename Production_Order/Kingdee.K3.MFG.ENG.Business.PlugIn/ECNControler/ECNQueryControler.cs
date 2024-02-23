using System;
using System.Collections.Generic;
using System.Linq;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Resource;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.MFG.ENG.ParamOption;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.Core.MFG.Utils;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.ECNControler
{
	// Token: 0x020000C6 RID: 198
	public class ECNQueryControler : AbstractItemControler
	{
		// Token: 0x06000E4D RID: 3661 RVA: 0x000A5997 File Offset: 0x000A3B97
		public override void DoOperation()
		{
			if (base.View is IBillView)
			{
				this.DoOperationOnBill();
				return;
			}
			if (base.View is IListView)
			{
				this.DoOperationOnList();
			}
		}

		// Token: 0x06000E4E RID: 3662 RVA: 0x000A59D0 File Offset: 0x000A3BD0
		private void DoOperationOnList()
		{
			if (!this.ValidatePermission())
			{
				base.View.ShowMessage(ResManager.LoadKDString("变更影响查询在该组织下无查看权！", "015072000019436", 7, new object[0]), 0);
				return;
			}
			ListSelectedRowCollection selectedRowsInfo = ((IListView)base.View).SelectedRowsInfo;
			if (ListUtils.IsEmpty<ListSelectedRow>(selectedRowsInfo))
			{
				return;
			}
			List<long> list = (from x in selectedRowsInfo
			select x.MainOrgId).Distinct<long>().ToList<long>();
			if (list.Count<long>() > 1)
			{
				base.View.ShowErrMessage("", ResManager.LoadKDString("查询失败,请选择相同变更组织下的工程变更单!", "015072000019430", 7, new object[0]), 0);
				return;
			}
			if (selectedRowsInfo.Count > 10)
			{
				base.View.ShowErrMessage("", ResManager.LoadKDString("选择的工程变更单请勿超过10张。", "015072000019429", 7, new object[0]), 0);
				return;
			}
			ECNQueryOption ecnqueryOption = new ECNQueryOption();
			ecnqueryOption.billOrgIds = list;
			ecnqueryOption.ECNOrgId = list.First<long>();
			ecnqueryOption.Context = base.View.Context;
			ecnqueryOption.SortMode = "1";
			ecnqueryOption.SourceBills = (from x in selectedRowsInfo
			select x.BillNo).Distinct<string>().ToList<string>();
			this.ShowForm(ecnqueryOption);
		}

		// Token: 0x06000E4F RID: 3663 RVA: 0x000A5B24 File Offset: 0x000A3D24
		private bool ValidatePermission()
		{
			bool flag = MFGCommonUtil.AuthPermissionBeforeShowF7Form(base.View, "ENG_ECNQuery", "6e44119a58cb4a8e86f6c385e14a17ad");
			if (!flag)
			{
				flag = false;
			}
			return flag;
		}

		// Token: 0x06000E50 RID: 3664 RVA: 0x000A5B50 File Offset: 0x000A3D50
		private void DoOperationOnBill()
		{
			if (!this.ValidatePermission())
			{
				base.View.ShowMessage(ResManager.LoadKDString("变更影响查询在该组织下无查看权！", "015072000019436", 7, new object[0]), 0);
				return;
			}
			ECNQueryOption ecnqueryOption = new ECNQueryOption();
			ecnqueryOption.billOrgIds = new List<long>();
			ecnqueryOption.ECNOrgId = DataEntityExtend.GetDynamicValue<long>(base.Model.DataObject, "ChangeOrgId_Id", 0L);
			ecnqueryOption.billOrgIds.Add(ecnqueryOption.ECNOrgId);
			ecnqueryOption.SortMode = "1";
			ecnqueryOption.SourceBills = new List<string>
			{
				DataEntityExtend.GetDynamicValue<string>(base.Model.DataObject, "BillNo", null)
			};
			ecnqueryOption.Context = base.View.Context;
			this.ShowForm(ecnqueryOption);
		}

		// Token: 0x06000E51 RID: 3665 RVA: 0x000A5C14 File Offset: 0x000A3E14
		private void ShowForm(ECNQueryOption option)
		{
			DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter
			{
				FormId = "ENG_ECNEffect",
				PageId = Guid.NewGuid().ToString()
			};
			dynamicFormShowParameter.OpenStyle.ShowType = 7;
			dynamicFormShowParameter.CustomComplexParams.Add("QueryOption", option);
			base.View.ShowForm(dynamicFormShowParameter);
		}
	}
}
