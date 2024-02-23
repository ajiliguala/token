using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.MFG.ServiceHelper.ENG;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm
{
	// Token: 0x020000B1 RID: 177
	[Description("工艺路线批量字段修改_表单插件")]
	public class RouteBatchFieldsEdit : AbstractDynamicFormPlugIn
	{
		// Token: 0x06000CE1 RID: 3297 RVA: 0x00098ABC File Offset: 0x00096CBC
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

		// Token: 0x06000CE2 RID: 3298 RVA: 0x00098B0C File Offset: 0x00096D0C
		public override void DataChanged(DataChangedEventArgs e)
		{
			base.DataChanged(e);
			string key;
			if ((key = e.Field.Key) != null)
			{
				if (key == "FWorkCenterId")
				{
					this.WorkCenterIdDataChanged(e);
					return;
				}
				if (key == "FOptCtrlCodeId")
				{
					this.SetDefault();
					return;
				}
				if (!(key == "FSupplier"))
				{
					return;
				}
				this.SupplierDataChanged(e);
			}
		}

		// Token: 0x06000CE3 RID: 3299 RVA: 0x00098B70 File Offset: 0x00096D70
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			base.BeforeF7Select(e);
			long num = Convert.ToInt64(this.View.OpenParameter.GetCustomParameter("FProOrgId"));
			long num2 = Convert.ToInt64(this.View.Model.DataObject["PurchaseOrgId_Id"]);
			this.CancelIsolation(e);
			string fieldKey;
			if ((fieldKey = e.FieldKey) != null)
			{
				if (fieldKey == "FPurchaseOrgId")
				{
					ListShowParameter listShowParameter = (ListShowParameter)e.DynamicFormShowParameter;
					listShowParameter.ExtJoinTables.Add(new ExtJoinTableDescription
					{
						FieldName = "FID",
						JoinOption = 1,
						ScourceKey = "FORGID",
						TableName = "table(fn_StrSplit(@purOrg,',',1))",
						TableNameAs = "Tmp"
					});
					SqlParam item = new SqlParam("@purOrg", 161, this.GetFilterPurOrgId().Distinct<long>().ToArray<long>());
					listShowParameter.SqlParams.Add(item);
					return;
				}
				if (fieldKey == "FWorkCenterId" || fieldKey == "FProcessId" || fieldKey == "FScheduleResourceId")
				{
					e.ListFilterParameter.Filter = string.Format(" {0}FUseOrgId={1} ", ObjectUtils.IsNullOrEmptyOrWhiteSpace(e.ListFilterParameter.Filter) ? "" : "And ", num);
					return;
				}
				if (!(fieldKey == "FSupplier"))
				{
					if (!(fieldKey == "FPriceList"))
					{
						return;
					}
					if (num2 != 0L)
					{
						e.ListFilterParameter.Filter = string.Format(" {0}FCreateOrgId={1} ", ObjectUtils.IsNullOrEmptyOrWhiteSpace(e.ListFilterParameter.Filter) ? "" : "And ", num2);
					}
				}
				else if (num2 != 0L)
				{
					e.ListFilterParameter.Filter = string.Format(" {0}FUseOrgId={1} ", ObjectUtils.IsNullOrEmptyOrWhiteSpace(e.ListFilterParameter.Filter) ? "" : "And ", num2);
					return;
				}
			}
		}

		// Token: 0x06000CE4 RID: 3300 RVA: 0x00098D74 File Offset: 0x00096F74
		private void CancelIsolation(BeforeF7SelectEventArgs e)
		{
			(e.DynamicFormShowParameter as ListShowParameter).IsIsolationOrg = false;
		}

		// Token: 0x06000CE5 RID: 3301 RVA: 0x00098D88 File Offset: 0x00096F88
		private List<long> GetFilterPurOrgId()
		{
			List<long> list = new List<long>();
			long num = Convert.ToInt64(this.View.OpenParameter.GetCustomParameter("FProOrgId"));
			string text = "select B.FRelationOrgID from t_org_bizrelation A inner join t_org_bizrelationEntry B on A.fbizrelationid=B.fbizrelationid  where  A.FBRTypeId=104 and B.FOrgId=@ProOrgId";
			List<SqlParam> list2 = new List<SqlParam>
			{
				new SqlParam("@ProOrgId", 12, num)
			};
			using (IDataReader dataReader = DBServiceHelper.ExecuteReader(base.Context, text, list2))
			{
				while (dataReader.Read())
				{
					list.Add(Convert.ToInt64(dataReader["FRelationOrgID"]));
				}
			}
			list.Add(num);
			return list;
		}

		// Token: 0x06000CE6 RID: 3302 RVA: 0x00098E38 File Offset: 0x00097038
		private void WorkCenterIdDataChanged(DataChangedEventArgs e)
		{
			this.CleanActivities(e.Row);
			if (e.NewValue != null)
			{
				for (int i = 0; i < 6; i++)
				{
					this.Model.SetValue("FCheck_ActivityQty" + Convert.ToString(i + 1), false, e.Row);
				}
				FormMetadata formMetadata = MetaDataServiceHelper.Load(base.Context, "ENG_WorkCenter", true) as FormMetadata;
				DynamicObject dynamicObject = BusinessDataServiceHelper.LoadSingle(base.Context, e.NewValue, formMetadata.BusinessInfo.GetDynamicObjectType(), null);
				DynamicObjectCollection dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<DynamicObjectCollection>(dynamicObject, "WorkCenterBaseActivity", null);
				DataEntityExtend.GetDynamicObjectItemValue<DynamicObjectCollection>(dynamicObject, "WorkCenterScheduling", null);
				foreach (DynamicObject dynamicObject2 in dynamicObjectItemValue)
				{
					long dynamicObjectItemValue2 = DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObject2, "BaseActivityID_Id", 0L);
					decimal dynamicObjectItemValue3 = DataEntityExtend.GetDynamicObjectItemValue<decimal>(dynamicObject2, "DefaultValue", 0m);
					long dynamicObjectItemValue4 = DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObject2, "TimeUnit_Id", 0L);
					long dynamicObjectItemValue5 = DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObject2, "ActFormula_Id", 0L);
					long dynamicObjectItemValue6 = DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObject2, "ActRepFormula_Id", 0L);
					int num = dynamicObjectItemValue.IndexOf(dynamicObject2);
					this.Model.SetValue("FActivity" + Convert.ToString(num + 1) + "Id", dynamicObjectItemValue2, e.Row);
					this.Model.SetValue("FActivity" + Convert.ToString(num + 1) + "Qty", dynamicObjectItemValue3, e.Row);
					this.Model.SetValue("FActivity" + Convert.ToString(num + 1) + "UnitID", dynamicObjectItemValue4, e.Row);
					this.Model.SetValue("FActivity" + Convert.ToString(num + 1) + "FormulaId", dynamicObjectItemValue5, e.Row);
					this.Model.SetValue("FActivity" + Convert.ToString(num + 1) + "RepFormulaId", dynamicObjectItemValue6, e.Row);
					this.Model.SetValue("FCheck_ActivityQty" + Convert.ToString(num + 1), true, e.Row);
				}
			}
		}

		// Token: 0x06000CE7 RID: 3303 RVA: 0x000990A4 File Offset: 0x000972A4
		private void SetDefault()
		{
			DynamicObject dynamicObject = (DynamicObject)this.Model.GetValue("FOptCtrlCodeId", 0);
			if (dynamicObject != null && dynamicObject["ProcessingMode"].ToString() != "10")
			{
				long num = Convert.ToInt64(this.View.OpenParameter.GetCustomParameter("FProOrgId"));
				long num2 = Convert.ToInt64(this.View.OpenParameter.GetCustomParameter("operUnitId"));
				string text = " select FCURRENCYID  from T_BD_CURRENCY where fcode='CNY'";
				long num3 = DBServiceHelper.ExecuteScalar<long>(base.Context, text, 0L, null);
				this.Model.SetValue("FPurchaseOrgId", num);
				this.Model.SetValue("FOutSrcCurrency", num3);
				this.Model.SetValue("FOperUnitID", num2);
				this.Model.SetValue("FValuationUnitID", num2);
			}
		}

		// Token: 0x06000CE8 RID: 3304 RVA: 0x00099194 File Offset: 0x00097394
		private void CleanActivities(int rowIndex)
		{
			for (int i = 0; i < 6; i++)
			{
				this.Model.SetValue("FActivity" + Convert.ToString(i + 1) + "Id", null, rowIndex);
				this.Model.SetValue("FActivity" + Convert.ToString(i + 1) + "Qty", null, rowIndex);
				this.Model.SetValue("FActivity" + Convert.ToString(i + 1) + "UnitID", null, rowIndex);
				this.Model.SetValue("FActivity" + Convert.ToString(i + 1) + "FormulaId", null, rowIndex);
				this.Model.SetValue("FActivity" + Convert.ToString(i + 1) + "RepFormulaId", null, rowIndex);
			}
		}

		// Token: 0x06000CE9 RID: 3305 RVA: 0x00099268 File Offset: 0x00097468
		private void SupplierDataChanged(DataChangedEventArgs e)
		{
			if (e.NewValue == null || string.IsNullOrEmpty(e.NewValue.ToString()) || "0".Equals(e.NewValue.ToString()))
			{
				this.Model.SetValue("FTaxRate", 0, e.Row);
				return;
			}
			if (Convert.ToDecimal(this.Model.GetValue("FTaxRate", e.Row)) == 0m)
			{
				FormMetadata formMetadata = MetaDataServiceHelper.Load(base.Context, "BD_Supplier", true) as FormMetadata;
				DynamicObject dynamicObject = BusinessDataServiceHelper.LoadSingle(base.Context, e.NewValue, formMetadata.BusinessInfo.GetDynamicObjectType(), null);
				DynamicObject dynamicObject2 = DataEntityExtend.GetDynamicObjectItemValue<DynamicObjectCollection>(dynamicObject, "SupplierFinance", null).FirstOrDefault<DynamicObject>();
				DynamicObject dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<DynamicObject>(dynamicObject2, "TaxRateId", null);
				if (dynamicObjectItemValue != null)
				{
					decimal dynamicObjectItemValue2 = DataEntityExtend.GetDynamicObjectItemValue<decimal>(dynamicObjectItemValue, "TaxRate", 0m);
					this.Model.SetValue("FTaxRate", dynamicObjectItemValue2, e.Row);
					decimal d = Convert.ToDecimal(this.Model.GetValue("FOutSrcTaxPrice", e.Row));
					decimal num = d / ++(dynamicObjectItemValue2 / 100m);
					this.Model.SetValue("FOutSrcPrice", num, e.Row);
					decimal d2 = Convert.ToDecimal(this.Model.GetValue("FScrapTaxPrice", e.Row));
					decimal num2 = d2 / ++(d2 / 100m);
					this.Model.SetValue("FScrapPrice", num2, e.Row);
					decimal d3 = Convert.ToDecimal(this.Model.GetValue("FMatScrapTaxPrice", e.Row));
					decimal num3 = d3 / ++(d3 / 100m);
					this.Model.SetValue("FMatScrapPrice", num3, e.Row);
				}
			}
		}

		// Token: 0x06000CEA RID: 3306 RVA: 0x000994A8 File Offset: 0x000976A8
		private void DoConfirm(ButtonClickEventArgs e)
		{
			this.View.Model.GetValue("FFieldName");
			this.View.Model.GetValue("FFieldValue");
			List<long> lstSubEntryIds = this.View.OpenParameter.GetCustomParameter("fPKEntryIds") as List<long>;
			List<long> lstHeadId = this.View.OpenParameter.GetCustomParameter("lstRouteHead") as List<long>;
			DynamicObject routeBatchFields = this.View.Model.DataObject;
			this.View.ShowMessage(ResManager.LoadKDString("您确定要对所选字段进行批量修改？", "002014030004369", 2, new object[0]), 1, delegate(MessageBoxResult cfm)
			{
				if (cfm != 1)
				{
					return;
				}
				this.DoExecuteBulkFeildsEdit(routeBatchFields, lstSubEntryIds, lstHeadId);
			}, "", 0);
		}

		// Token: 0x06000CEB RID: 3307 RVA: 0x00099578 File Offset: 0x00097778
		private void DoExecuteBulkFeildsEdit(DynamicObject routeBatchFields, List<long> lstSubEntryIds, List<long> lstHeadId)
		{
			RouteServiceHelper.BulkEditBatchFields(base.Context, routeBatchFields, lstSubEntryIds, lstHeadId);
			this.View.ShowMessage(ResManager.LoadKDString("批改成功", "015072000025101", 7, new object[0]), 0);
			this.View.OpenParameter.SetCustomParameter("DataChanged", true);
			this.ListRefresh();
			this.View.Close();
		}

		// Token: 0x06000CEC RID: 3308 RVA: 0x000995E1 File Offset: 0x000977E1
		private void DoCancel(ButtonClickEventArgs e)
		{
			this.ListRefresh();
			this.View.Close();
		}

		// Token: 0x06000CED RID: 3309 RVA: 0x000995F4 File Offset: 0x000977F4
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
