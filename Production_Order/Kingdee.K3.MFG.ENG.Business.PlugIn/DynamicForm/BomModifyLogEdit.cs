using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Kingdee.BOS;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Core.Permission;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.BD.ServiceHelper;
using Kingdee.K3.Core.MFG.ENG.ParamOption;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.Core.MFG.Utils;
using Kingdee.K3.MFG.ServiceHelper;
using Kingdee.K3.MFG.ServiceHelper.ENG;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm
{
	// Token: 0x02000074 RID: 116
	[Description("BOM修改日志插件")]
	public class BomModifyLogEdit : AbstractDynamicFormPlugIn
	{
		// Token: 0x06000835 RID: 2101 RVA: 0x00061940 File Offset: 0x0005FB40
		public override void PreOpenForm(PreOpenFormEventArgs e)
		{
			bool flag = Convert.ToBoolean(MFGServiceHelper.GetSystemProfile<int>(e.Context, e.Context.CurrentOrganizationInfo.ID, "MFG_EngParameter", "IsUseBomModifyLog", 0));
			IDynamicFormView parentFormView = e.ParentView.ParentFormView;
			if (!ObjectUtils.IsNullOrEmpty(parentFormView))
			{
				string id = parentFormView.BillBusinessInfo.GetForm().Id;
				if (id.Equals("ENG_BOM"))
				{
					long num = Convert.ToInt64(e.OpenParameter.GetCustomParameter("HeadUseOrgId"));
					flag = Convert.ToBoolean(MFGServiceHelper.GetSystemProfile<int>(e.Context, num, "MFG_EngParameter", "IsUseBomModifyLog", 0));
				}
			}
			if (!flag)
			{
				e.Cancel = true;
				e.CancelMessage = ResManager.LoadKDString("工程数据参数【启用BOM修改日志】勾选后，才允许使用BOM修改日志查询功能！", "0151515153499000014048", 7, new object[0]);
			}
		}

		// Token: 0x06000836 RID: 2102 RVA: 0x00061A04 File Offset: 0x0005FC04
		public override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			DateTime systemDateTime = TimeServiceHelper.GetSystemDateTime(base.Context);
			DateTime dateTime = default(DateTime);
			this.InitPrdOrgId("FHeadUseOrgId");
			dateTime = systemDateTime.AddDays(-7.0).Date;
			IDynamicFormView parentFormView = this.View.ParentFormView;
			if (!ObjectUtils.IsNullOrEmpty(parentFormView))
			{
				string id = parentFormView.BillBusinessInfo.GetForm().Id;
				if (id.Equals("ENG_BOM"))
				{
					long num = Convert.ToInt64(this.View.OpenParameter.GetCustomParameter("HeadUseOrgId"));
					long num2 = Convert.ToInt64(this.View.OpenParameter.GetCustomParameter("HeadBomId"));
					long num3 = Convert.ToInt64(this.View.OpenParameter.GetCustomParameter("ParMaterialId"));
					this.View.Model.SetValue("FHeadUseOrgId", num);
					this.View.Model.SetValue("FHeadBomId", num2);
					this.View.Model.SetValue("FParMaterialId", num3);
					dateTime = systemDateTime.AddMonths(-3).Date;
					this.View.Model.SetValue("FBeginModifyStartDate", dateTime);
					string empty = string.Empty;
					if (!this.RefreashVerify(out empty))
					{
						this.View.ShowErrMessage(empty, "", 0);
						return;
					}
					this.FillEntitys();
				}
			}
			this.View.Model.SetValue("FBeginModifyStartDate", dateTime);
			this.View.Model.SetValue("FHeadModifyType", "1,2,3");
		}

		// Token: 0x06000837 RID: 2103 RVA: 0x00061BC0 File Offset: 0x0005FDC0
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			base.BarItemClick(e);
			string empty = string.Empty;
			string barItemKey;
			if ((barItemKey = e.BarItemKey) != null)
			{
				if (!(barItemKey == "tbRefresh"))
				{
					if (barItemKey == "tbExit")
					{
						this.View.Close();
						return;
					}
					if (!(barItemKey == "tbExport"))
					{
						return;
					}
					if (!this.RefreashVerify(out empty))
					{
						this.View.ShowErrMessage(empty, "", 0);
						e.Cancel = true;
					}
				}
				else
				{
					if (!this.RefreashVerify(out empty))
					{
						this.View.ShowErrMessage(empty, "", 0);
						e.Cancel = true;
						return;
					}
					this.FillEntitys();
					return;
				}
			}
		}

		// Token: 0x06000838 RID: 2104 RVA: 0x00061C6C File Offset: 0x0005FE6C
		public override void BeforeDoOperation(BeforeDoOperationEventArgs e)
		{
			string a;
			if ((a = e.Operation.FormOperation.Id.ToUpperInvariant()) != null)
			{
				if (!(a == "ENTITYEXPORT"))
				{
					return;
				}
				if (!this.ValidatePermission(e))
				{
					e.Cancel = true;
				}
			}
		}

		// Token: 0x06000839 RID: 2105 RVA: 0x00061CC0 File Offset: 0x0005FEC0
		public override void BeforeEntityExport(BeforeEntityExportArgs e)
		{
			base.BeforeEntityExport(e);
			BomModifyLogOption bomModifyLogOption = new BomModifyLogOption();
			List<int> headBomId = new List<int>();
			DynamicObjectCollection value = MFGBillUtil.GetValue<DynamicObjectCollection>(this.View.Model, "FHeadBomId", -1, null, null);
			if (!ListUtils.IsEmpty<DynamicObject>(value))
			{
				headBomId = (from f in value
				select DataEntityExtend.GetDynamicValue<int>(f, "HeadBomId_Id", 0)).ToList<int>();
			}
			bomModifyLogOption.HeadBomId = headBomId;
			List<string> headModifyType = new List<string>();
			string value2 = MFGBillUtil.GetValue<string>(this.View.Model, "FHeadModifyType", -1, null, null);
			if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(value2))
			{
				headModifyType = value2.Split(new char[]
				{
					','
				}).ToList<string>();
			}
			bomModifyLogOption.HeadModifyType = headModifyType;
			bomModifyLogOption.HeadUseOrgId = MFGBillUtil.GetValue<int>(this.View.Model, "FHeadUseOrgId", -1, 0, null);
			bomModifyLogOption.HeadModifierId = MFGBillUtil.GetValue<int>(this.View.Model, "FHeadModifierId", -1, 0, null);
			bomModifyLogOption.BeginModifyDate = MFGBillUtil.GetValue<DateTime>(this.View.Model, "FBeginModifyStartDate", -1, default(DateTime), null);
			bomModifyLogOption.EndModifyDate = MFGBillUtil.GetValue<DateTime>(this.View.Model, "FEndModifyDate", -1, default(DateTime), null).AddDays(1.0).AddMilliseconds(-1.0);
			bomModifyLogOption.ParMaterialId = MFGBillUtil.GetValue<int>(this.View.Model, "FParMaterialId", -1, 0, null);
			bomModifyLogOption.ChildMaterialId = MFGBillUtil.GetValue<int>(this.View.Model, "FChildMaterialId", -1, 0, null);
			IOperationResult entityData = BomModifyLogServiceHelper.GetEntityData(base.Context, bomModifyLogOption);
			List<DynamicObject> value3 = (List<DynamicObject>)entityData.FuncResult;
			e.DataSource = new Dictionary<string, List<DynamicObject>>
			{
				{
					"FEntity",
					value3
				}
			};
		}

		// Token: 0x0600083A RID: 2106 RVA: 0x00061EA4 File Offset: 0x000600A4
		public override void DataChanged(DataChangedEventArgs e)
		{
			string key;
			if ((key = e.Field.Key) != null)
			{
				if (!(key == "FParMaterialId"))
				{
					return;
				}
				if (e.NewValue == null)
				{
					this.View.Model.SetValue("FHeadBomId", null);
					return;
				}
				long[] materialMasterAndUserOrgId = MaterialServiceHelper.GetMaterialMasterAndUserOrgId(base.Context, Convert.ToInt64(e.NewValue));
				long value = MFGBillUtil.GetValue<long>(this.View.Model, "FHeadUseOrgId", -1, 0L, null);
				long hightVersionBomKey = BOMServiceHelper.GetHightVersionBomKey(base.Context, materialMasterAndUserOrgId[0], value);
				this.View.Model.SetValue("FHeadBomId", hightVersionBomKey);
			}
		}

		// Token: 0x0600083B RID: 2107 RVA: 0x00061F64 File Offset: 0x00060164
		public override void BeforeSetItemValueByNumber(BeforeSetItemValueByNumberArgs e)
		{
			base.BeforeSetItemValueByNumber(e);
			string baseDataFieldKey;
			if ((baseDataFieldKey = e.BaseDataFieldKey) != null)
			{
				if (!(baseDataFieldKey == "FHeadBomId"))
				{
					if (!(baseDataFieldKey == "FHeadUseOrgId"))
					{
						return;
					}
					List<long> permissionOrg = this.GetPermissionOrg("ENG_BomModifyLog");
					Dictionary<long, object> paramter = SystemParameterServiceHelper.GetParamter(base.Context, permissionOrg, 0L, "MFG_EngParameter", "IsUseBomModifyLog");
					List<long> list = (from x in paramter
					where Convert.ToBoolean(x.Value)
					select x.Key).ToList<long>();
					if (!ListUtils.IsEmpty<long>(list))
					{
						if (string.IsNullOrWhiteSpace(e.Filter))
						{
							e.Filter = string.Format(" FORGID IN ({0}) ", string.Join<long>(",", list));
						}
						else
						{
							e.Filter += string.Format(" AND FORGID IN ({0})", string.Join<long>(",", list));
						}
					}
					bool flag = MFGServiceHelper.IsEnabledForbidOrgQuery(base.Context);
					if (flag)
					{
						e.IsShowUsed = false;
					}
				}
				else
				{
					e.IsShowApproved = false;
					e.IsShowUsed = false;
					long value = MFGBillUtil.GetValue<long>(this.View.Model, "FParMaterialId", -1, 0L, null);
					if (value != 0L)
					{
						if (string.IsNullOrWhiteSpace(e.Filter))
						{
							e.Filter = string.Format(" FMATERIALID = {0} ", value);
							return;
						}
						e.Filter += string.Format(" AND FMATERIALID = {0} ", value);
						return;
					}
				}
			}
		}

		// Token: 0x0600083C RID: 2108 RVA: 0x00062110 File Offset: 0x00060310
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			base.BeforeF7Select(e);
			string fieldKey;
			if ((fieldKey = e.FieldKey) != null)
			{
				if (!(fieldKey == "FHeadBomId"))
				{
					if (!(fieldKey == "FHeadUseOrgId"))
					{
						return;
					}
					List<long> permissionOrg = this.GetPermissionOrg("ENG_BomModifyLog");
					Dictionary<long, object> paramter = SystemParameterServiceHelper.GetParamter(base.Context, permissionOrg, 0L, "MFG_EngParameter", "IsUseBomModifyLog");
					List<long> list = (from x in paramter
					where Convert.ToBoolean(x.Value)
					select x.Key).ToList<long>();
					if (!ListUtils.IsEmpty<long>(list))
					{
						if (string.IsNullOrWhiteSpace(e.ListFilterParameter.Filter))
						{
							e.ListFilterParameter.Filter = string.Format(" FORGID IN ({0}) ", string.Join<long>(",", list));
						}
						else
						{
							IRegularFilterParameter listFilterParameter = e.ListFilterParameter;
							listFilterParameter.Filter += string.Format(" AND FORGID IN ({0})", string.Join<long>(",", list));
						}
					}
					bool flag = MFGServiceHelper.IsEnabledForbidOrgQuery(base.Context);
					if (flag)
					{
						e.IsShowUsed = false;
					}
				}
				else
				{
					e.IsShowApproved = false;
					e.IsShowUsed = false;
					long value = MFGBillUtil.GetValue<long>(this.View.Model, "FParMaterialId", -1, 0L, null);
					if (value != 0L)
					{
						if (string.IsNullOrWhiteSpace(e.ListFilterParameter.Filter))
						{
							e.ListFilterParameter.Filter = string.Format(" FMATERIALID = {0} ", value);
							return;
						}
						IRegularFilterParameter listFilterParameter2 = e.ListFilterParameter;
						listFilterParameter2.Filter += string.Format(" AND FMATERIALID = {0} ", value);
						return;
					}
				}
			}
		}

		// Token: 0x0600083D RID: 2109 RVA: 0x000622C6 File Offset: 0x000604C6
		public override void AfterBindData(EventArgs e)
		{
			base.AfterBindData(e);
			this.SetModifyTypeItems();
		}

		// Token: 0x0600083E RID: 2110 RVA: 0x000622D8 File Offset: 0x000604D8
		private List<long> GetPermissionOrg(string formId)
		{
			BusinessObject businessObject = new BusinessObject
			{
				Id = formId,
				SubSystemId = this.View.Model.SubSytemId
			};
			return PermissionServiceHelper.GetPermissionOrg(base.Context, businessObject, "6e44119a58cb4a8e86f6c385e14a17ad");
		}

		// Token: 0x0600083F RID: 2111 RVA: 0x0006232C File Offset: 0x0006052C
		private void FillEntitys()
		{
			BomModifyLogOption bomModifyLogOption = new BomModifyLogOption();
			List<int> headBomId = new List<int>();
			DynamicObjectCollection value = MFGBillUtil.GetValue<DynamicObjectCollection>(this.View.Model, "FHeadBomId", -1, null, null);
			if (!ListUtils.IsEmpty<DynamicObject>(value))
			{
				headBomId = (from f in value
				select DataEntityExtend.GetDynamicValue<int>(f, "HeadBomId_Id", 0)).ToList<int>();
			}
			bomModifyLogOption.HeadBomId = headBomId;
			List<string> headModifyType = new List<string>();
			string value2 = MFGBillUtil.GetValue<string>(this.View.Model, "FHeadModifyType", -1, null, null);
			if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(value2))
			{
				headModifyType = value2.Split(new char[]
				{
					','
				}).ToList<string>();
			}
			bomModifyLogOption.HeadModifyType = headModifyType;
			bomModifyLogOption.HeadUseOrgId = MFGBillUtil.GetValue<int>(this.View.Model, "FHeadUseOrgId", -1, 0, null);
			bomModifyLogOption.HeadModifierId = MFGBillUtil.GetValue<int>(this.View.Model, "FHeadModifierId", -1, 0, null);
			bomModifyLogOption.BeginModifyDate = MFGBillUtil.GetValue<DateTime>(this.View.Model, "FBeginModifyStartDate", -1, default(DateTime), null);
			bomModifyLogOption.EndModifyDate = MFGBillUtil.GetValue<DateTime>(this.View.Model, "FEndModifyDate", -1, default(DateTime), null).AddDays(1.0).AddMilliseconds(-1.0);
			bomModifyLogOption.ParMaterialId = MFGBillUtil.GetValue<int>(this.View.Model, "FParMaterialId", -1, 0, null);
			bomModifyLogOption.ChildMaterialId = MFGBillUtil.GetValue<int>(this.View.Model, "FChildMaterialId", -1, 0, null);
			IOperationResult entityData = BomModifyLogServiceHelper.GetEntityData(base.Context, bomModifyLogOption);
			List<DynamicObject> list = (List<DynamicObject>)entityData.FuncResult;
			Entity entity = this.View.BusinessInfo.GetEntity("FEntity");
			DynamicObjectCollection entityDataObject = this.View.Model.GetEntityDataObject(entity);
			entityDataObject.Clear();
			if (ListUtils.IsEmpty<DynamicObject>(list))
			{
				this.View.ShowMessage(ResManager.LoadKDString("查询BOM修改日志为空!", "0151515153499030038265", 7, new object[0]), 0);
				this.View.UpdateView("FEntity");
				return;
			}
			if (!entityData.IsSuccess)
			{
				this.View.UpdateView("FEntity");
				FormUtils.ShowOperationResult(this.View, entityData, null);
				return;
			}
			this.Model.BeginIniti();
			foreach (DynamicObject item in list)
			{
				entityDataObject.Add(item);
			}
			this.Model.EndIniti();
			this.View.UpdateView("FEntity");
		}

		// Token: 0x06000840 RID: 2112 RVA: 0x000625EC File Offset: 0x000607EC
		private void InitPrdOrgId(string fieldKey)
		{
			if (base.Context.CurrentOrganizationInfo.FunctionIds.Contains(104L))
			{
				this.View.Model.SetValue(fieldKey, base.Context.CurrentOrganizationInfo.ID);
			}
		}

		// Token: 0x06000841 RID: 2113 RVA: 0x0006263C File Offset: 0x0006083C
		private bool RefreashVerify(out string errMsg)
		{
			bool result = true;
			StringBuilder stringBuilder = new StringBuilder();
			if (MFGBillUtil.GetValue<long>(this.View.Model, "FHeadUseOrgId", -1, 0L, null) == 0L)
			{
				stringBuilder.AppendLine(ResManager.LoadKDString("请至少选择一个使用组织。", "0151515153499000014049", 7, new object[0]));
				result = false;
			}
			errMsg = stringBuilder.ToString();
			return result;
		}

		// Token: 0x06000842 RID: 2114 RVA: 0x00062698 File Offset: 0x00060898
		private bool ValidatePermission(BeforeDoOperationEventArgs e)
		{
			bool flag = MFGCommonUtil.AuthPermissionBeforeShowF7Form(this.View, this.View.BillBusinessInfo.GetForm().Id, e.Operation.FormOperation.PermissionItemId);
			if (!flag)
			{
				this.View.ShowMessage(string.Format(ResManager.LoadKDString("没有BOM修改日志的{0}权限！", "0151515153499000014051", 7, new object[0]), e.Operation.FormOperation.OperationName[this.View.Context.UserLocale.LCID]), 0);
				flag = false;
			}
			return flag;
		}

		// Token: 0x06000843 RID: 2115 RVA: 0x00062760 File Offset: 0x00060960
		private void SetModifyTypeItems()
		{
			List<string> valueLst = new List<string>
			{
				"1",
				"2",
				"3"
			};
			ComboField comboField = this.View.BusinessInfo.GetField("FHeadModifyType") as ComboField;
			DynamicObject enumObject = comboField.EnumObject;
			IEnumerable<DynamicObject> enumerable = (from f in DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(enumObject, "Items", null)
			where valueLst.Contains(DataEntityExtend.GetDynamicValue<string>(f, "Value", null))
			select f into g
			orderby DataEntityExtend.GetDynamicValue<int>(g, "Seq", 0)
			select g).ToList<DynamicObject>();
			List<EnumItem> list = new List<EnumItem>();
			foreach (DynamicObject dynamicObject in enumerable)
			{
				list.Add(new EnumItem
				{
					Value = DataEntityExtend.GetDynamicValue<string>(dynamicObject, "Value", null),
					Caption = DataEntityExtend.GetDynamicValue<LocaleValue>(dynamicObject, "Caption", null),
					Seq = DataEntityExtend.GetDynamicValue<int>(dynamicObject, "Seq", 0)
				});
			}
			ComboFieldEditor control = this.View.GetControl<ComboFieldEditor>("FHeadModifyType");
			control.SetComboItems(list);
		}
	}
}
