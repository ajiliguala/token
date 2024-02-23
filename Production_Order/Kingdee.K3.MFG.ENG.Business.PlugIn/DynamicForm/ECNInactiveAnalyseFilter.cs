using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.CommonFilter.PlugIn;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.Core.MFG.Utils;
using Kingdee.K3.MFG.ServiceHelper;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm
{
	// Token: 0x0200008E RID: 142
	[Description("ECN呆滞分析过滤")]
	public class ECNInactiveAnalyseFilter : AbstractCommonFilterPlugIn
	{
		// Token: 0x06000A9C RID: 2716 RVA: 0x0007ACEE File Offset: 0x00078EEE
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
		}

		// Token: 0x06000A9D RID: 2717 RVA: 0x0007ACF8 File Offset: 0x00078EF8
		public override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			this.Model.SetValue("FEffectiveStart", DateTime.Now.AddMonths(-1));
			this.Model.SetValue("FChangeOrgId", base.Context.CurrentOrganizationInfo.ID);
			this.Model.SetValue("FStockOrgId", base.Context.CurrentOrganizationInfo.ID);
		}

		// Token: 0x06000A9E RID: 2718 RVA: 0x0007AD7C File Offset: 0x00078F7C
		public override void AuthPermissionBeforeF7Select(AuthPermissionBeforeF7SelectEventArgs e)
		{
			base.AuthPermissionBeforeF7Select(e);
			string fieldKey;
			if ((fieldKey = e.FieldKey) != null)
			{
				if (!(fieldKey == "FStockIdFrom") && !(fieldKey == "FStockIdTo"))
				{
					return;
				}
				e.IsIsolationOrg = false;
			}
		}

		// Token: 0x06000A9F RID: 2719 RVA: 0x0007ADDC File Offset: 0x00078FDC
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			base.BeforeF7Select(e);
			string filter = e.ListFilterParameter.Filter;
			string fieldKey;
			if ((fieldKey = e.FieldKey) != null)
			{
				if (!(fieldKey == "FChangeOrgId"))
				{
					if (!(fieldKey == "FStockOrgId"))
					{
						if (!(fieldKey == "FNumber"))
						{
							if (!(fieldKey == "FStockIdFrom") && !(fieldKey == "FStockIdTo"))
							{
								return;
							}
							DynamicObjectCollection value = MFGBillUtil.GetValue<DynamicObjectCollection>(this.Model, "FStockOrgId", -1, null, null);
							if (!ListUtils.IsEmpty<DynamicObject>(value))
							{
								long[] values = (from s in value
								select DataEntityExtend.GetDynamicValue<long>(s, "StockOrgId_Id", 0L)).Distinct<long>().ToArray<long>();
								string text = string.Format("FUseOrgId in ({0}) ", string.Join<long>(",", values));
								e.ListFilterParameter.Filter = (ObjectUtils.IsNullOrEmptyOrWhiteSpace(e.ListFilterParameter.Filter) ? text : ("AND " + text));
								return;
							}
							e.Cancel = true;
							this.View.ShowMessage(ResManager.LoadKDString("请先选择库存组织！", "0151515153499000012707", 7, new object[0]), 0);
						}
						else
						{
							DynamicObjectCollection value2 = MFGBillUtil.GetValue<DynamicObjectCollection>(this.Model, "FChangeOrgId", -1, null, null);
							if (!ListUtils.IsEmpty<DynamicObject>(value2))
							{
								IEnumerable<long> values2 = from s in value2
								select DataEntityExtend.GetDynamicValue<long>(s, "ChangeOrgId_Id", 0L);
								string filter2 = string.Format("FChangeOrgId in ({0}) AND FECNSTATUS='1' ", string.Join<long>(",", values2));
								this.GetBillNoByF7(e, "ENG_ECNOrder", e.FieldKey, null, filter2);
								return;
							}
							this.View.ShowMessage(ResManager.LoadKDString("请先选择变更组织！", "0151515153499000012706", 7, new object[0]), 0);
							return;
						}
					}
					else
					{
						IRegularFilterParameter listFilterParameter = e.ListFilterParameter;
						listFilterParameter.Filter += (string.IsNullOrEmpty(filter) ? "FORGFUNCTIONS LIKE '%103%' " : " AND FORGFUNCTIONS LIKE '%103%' ");
						if (MFGServiceHelper.IsEnabledForbidOrgQuery(base.Context))
						{
							e.IsShowUsed = false;
							return;
						}
					}
				}
				else
				{
					IRegularFilterParameter listFilterParameter2 = e.ListFilterParameter;
					listFilterParameter2.Filter += (string.IsNullOrEmpty(filter) ? "FORGFUNCTIONS LIKE '%104%' " : " AND FORGFUNCTIONS LIKE '%104%' ");
					if (MFGServiceHelper.IsEnabledForbidOrgQuery(base.Context))
					{
						e.IsShowUsed = false;
						return;
					}
				}
			}
		}

		// Token: 0x06000AA0 RID: 2720 RVA: 0x0007B02C File Offset: 0x0007922C
		public override void BeforeSetItemValueByNumber(BeforeSetItemValueByNumberArgs e)
		{
			base.BeforeSetItemValueByNumber(e);
			string a;
			if ((a = e.BaseDataFieldKey.ToUpperInvariant()) != null)
			{
				if (!(a == "FSTOCKORGID") && !(a == "FCHANGEORGID"))
				{
					return;
				}
				if (MFGServiceHelper.IsEnabledForbidOrgQuery(base.Context))
				{
					e.IsShowUsed = false;
				}
			}
		}

		// Token: 0x06000AA1 RID: 2721 RVA: 0x0007B110 File Offset: 0x00079310
		protected void GetBillNoByF7(BeforeF7SelectEventArgs e, string formId, string selectColumn, string prdOrgId, string filter = "")
		{
			if (ObjectUtils.IsNullOrEmptyOrWhiteSpace(formId))
			{
				return;
			}
			ListSelBillShowParameter listSelBillShowParameter = new ListSelBillShowParameter
			{
				FormId = formId,
				PermissionItemId = "6e44119a58cb4a8e86f6c385e14a17ad",
				ParentPageId = this.View.PageId,
				IsShowApproved = e.IsShowApproved,
				IsLookUp = true,
				ListFilterParameter = new ListRegularFilterParameter
				{
					Filter = filter,
					OrderBy = " FBILLNO DESC "
				}
			};
			this.View.ShowForm(listSelBillShowParameter, delegate(FormResult result)
			{
				if (result.ReturnData != null && result.ReturnData is ListSelectedRowCollection)
				{
					ListSelectedRowCollection source = (ListSelectedRowCollection)result.ReturnData;
					this.View.Model.SetValue(selectColumn, string.Join(";", (from w in source
					select w.BillNo).Distinct<string>().ToArray<string>()));
				}
			});
		}
	}
}
