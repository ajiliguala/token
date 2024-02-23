using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Orm.Metadata.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.BD.ServiceHelper;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.Core.MFG.Utils;
using Kingdee.K3.MFG.ENG.Business.PlugIn.Base;
using Kingdee.K3.MFG.ServiceHelper.ENG;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.Bill
{
	// Token: 0x02000055 RID: 85
	[Description("ECR表单插件")]
	public class ECRApplyEdit : BaseControlEdit
	{
		// Token: 0x06000647 RID: 1607 RVA: 0x0004AE84 File Offset: 0x00049084
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			base.BeforeF7Select(e);
			string fieldKey;
			if ((fieldKey = e.FieldKey) != null)
			{
				if (fieldKey == "FBomId")
				{
					e.DynamicFormShowParameter.MultiSelect = false;
					DynamicObjectCollection dynamicObjectCollection = base.View.Model.DataObject["Entity"] as DynamicObjectCollection;
					DynamicObject dynamicObject = dynamicObjectCollection[e.Row]["MaterialId"] as DynamicObject;
					string text;
					if (dynamicObject == null)
					{
						text = string.Format("FMATERIALID={0}", 0);
					}
					long dynamicValue = DataEntityExtend.GetDynamicValue<long>(dynamicObject, "msterID", 0L);
					long value = MFGBillUtil.GetValue<long>(base.View.Model, "FChangeOrgId", e.Row, 0L, null);
					long dynamicValue2 = DataEntityExtend.GetDynamicValue<long>(dynamicObjectCollection[e.Row], "AuxPropId_Id", 0L);
					List<long> list = new List<long>();
					if (dynamicValue2 > 0L)
					{
						list = BOMServiceHelper.GetApprovedBomIdByOrgId(base.View.Context, dynamicValue, value, dynamicValue2);
					}
					else
					{
						DynamicObject dynamicObject2 = dynamicObjectCollection[e.Row]["AuxPropId"] as DynamicObject;
						list = BOMServiceHelper.GetApprovedBomIdByOrgId(base.View.Context, dynamicValue, value, dynamicObject2);
					}
					if (!ListUtils.IsEmpty<long>(list))
					{
						text = string.Format(" FID IN ({0}) ", string.Join<long>(",", list));
					}
					else
					{
						text = string.Format(" FID={0}", 0);
					}
					e.ListFilterParameter.Filter = base.SqlAppendAnd(e.ListFilterParameter.Filter, text);
					return;
				}
				if (!(fieldKey == "FMaterialId"))
				{
					return;
				}
				if (string.IsNullOrEmpty(e.ListFilterParameter.Filter))
				{
					e.ListFilterParameter.Filter = "FMATERIALID IN (SELECT FMATERIALID FROM t_BD_MaterialProduce WHERE FISECN='1' AND FISMAINPRD='1') AND FDOCUMENTSTATUS='C' AND FFORBIDSTATUS='A' ";
					return;
				}
				e.ListFilterParameter.Filter = "FMATERIALID IN (SELECT FMATERIALID FROM t_BD_MaterialProduce WHERE FISECN='1' AND FISMAINPRD='1') AND FDOCUMENTSTATUS='C' AND FFORBIDSTATUS='A' AND " + e.ListFilterParameter.Filter;
			}
		}

		// Token: 0x06000648 RID: 1608 RVA: 0x0004B060 File Offset: 0x00049260
		public override void DataChanged(DataChangedEventArgs e)
		{
			base.DataChanged(e);
			string key = e.Field.Key;
			string key2;
			if ((key2 = e.Field.Key) != null)
			{
				if (key2 == "FAuxPropId")
				{
					DynamicObject newAuxpropData = e.OldValue as DynamicObject;
					this.AuxpropDataChanged(newAuxpropData, e.Row);
					return;
				}
				if (!(key2 == "FMaterialId"))
				{
					return;
				}
				base.View.Model.SetValue("FAuxPropId", 0, e.Row);
				base.View.Model.SetValue("FBomId", 0, e.Row);
				if (!MFGBillUtil.GetUserParam<bool>(base.View, "AutoSetBom", false))
				{
					return;
				}
				DynamicObject value = MFGBillUtil.GetValue<DynamicObject>(base.View.Model, "FMaterialId", e.Row, null, null);
				if (ObjectUtils.IsNullOrEmpty(value))
				{
					return;
				}
				DynamicObjectCollection dynamicValue = DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(value, "MaterialBase", null);
				if (ListUtils.IsEmpty<DynamicObject>(dynamicValue))
				{
					return;
				}
				DynamicObject dynamicObject = dynamicValue.FirstOrDefault<DynamicObject>();
				string dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<string>(dynamicObject, "ErpClsID", null);
				if (!StringUtils.EqualsIgnoreCase(dynamicObjectItemValue, "9") && !StringUtils.EqualsIgnoreCase(dynamicObjectItemValue, "2") && !StringUtils.EqualsIgnoreCase(dynamicObjectItemValue, "3") && !StringUtils.EqualsIgnoreCase(dynamicObjectItemValue, "1"))
				{
					return;
				}
				long dynamicValue2 = DataEntityExtend.GetDynamicValue<long>(value, "msterID", 0L);
				long value2 = MFGBillUtil.GetValue<long>(base.View.Model, "FChangeOrgId", e.Row, 0L, null);
				long value3 = MFGBillUtil.GetValue<long>(base.View.Model, "FAuxPropId", e.Row, 0L, null);
				long hightVersionBomKey = BOMServiceHelper.GetHightVersionBomKey(base.Context, dynamicValue2, value2, value3);
				base.View.Model.SetValue("FBomId", hightVersionBomKey, e.Row);
			}
		}

		// Token: 0x06000649 RID: 1609 RVA: 0x0004B234 File Offset: 0x00049434
		private void AuxpropDataChanged(int row)
		{
			DynamicObjectCollection dynamicObjectCollection = base.View.Model.DataObject["Entity"] as DynamicObjectCollection;
			long dynamicValue = DataEntityExtend.GetDynamicValue<long>(dynamicObjectCollection[row], "AuxPropId_Id", 0L);
			if (dynamicValue == this.lastAuxpropId)
			{
				return;
			}
			long value = MFGBillUtil.GetValue<long>(base.View.Model, "FBomId", row, 0L, null);
			long value2 = MFGBillUtil.GetValue<long>(base.View.Model, "FMaterialId", row, 0L, null);
			long value3 = MFGBillUtil.GetValue<long>(this.Model, "FChangeOrgId", 0, 0L, null);
			long[] materialMasterAndUserOrgId = MaterialServiceHelper.GetMaterialMasterAndUserOrgId(base.Context, value2);
			long defaultBomKey = BOMServiceHelper.GetDefaultBomKey(base.View.Context, materialMasterAndUserOrgId[0], value3, dynamicValue, 2);
			if (defaultBomKey != value)
			{
				base.View.Model.SetValue("FBomId", defaultBomKey, row);
			}
			this.lastAuxpropId = dynamicValue;
			base.View.UpdateView("FEntity", row);
		}

		// Token: 0x0600064A RID: 1610 RVA: 0x0004B32C File Offset: 0x0004952C
		private void AuxpropDataChanged(DynamicObject newAuxpropData, int row)
		{
			object obj = base.View.Model.DataObject["Entity"];
			long value = MFGBillUtil.GetValue<long>(base.View.Model, "FBomId", row, 0L, null);
			long value2 = MFGBillUtil.GetValue<long>(base.View.Model, "FMaterialId", row, 0L, null);
			long value3 = MFGBillUtil.GetValue<long>(this.Model, "FChangeOrgId", 0, 0L, null);
			long[] materialMasterAndUserOrgId = MaterialServiceHelper.GetMaterialMasterAndUserOrgId(base.Context, value2);
			long defaultBomKey = BOMServiceHelper.GetDefaultBomKey(base.View.Context, materialMasterAndUserOrgId[0], value3, newAuxpropData, 2);
			if (defaultBomKey != value)
			{
				base.View.Model.SetValue("FBomId", defaultBomKey, row);
			}
			base.View.UpdateView("FEntity", row);
		}

		// Token: 0x0600064B RID: 1611 RVA: 0x0004B3F8 File Offset: 0x000495F8
		public override void BeforeFlexSelect(BeforeFlexSelectEventArgs e)
		{
			base.BeforeFlexSelect(e);
			if (StringUtils.EqualsIgnoreCase(e.FieldKey, "FAuxPropId"))
			{
				DynamicObjectCollection dynamicObjectCollection = base.View.Model.DataObject["Entity"] as DynamicObjectCollection;
				this.lastAuxpropId = DataEntityExtend.GetDynamicValue<long>(dynamicObjectCollection[e.Row], "AuxPropId_Id", 0L);
			}
		}

		// Token: 0x0600064C RID: 1612 RVA: 0x0004B45C File Offset: 0x0004965C
		public override void AfterShowFlexForm(AfterShowFlexFormEventArgs e)
		{
			base.AfterShowFlexForm(e);
			if (e.Result == 1 && StringUtils.EqualsIgnoreCase(e.FlexField.Key, "FAuxPropId"))
			{
				this.AuxpropDataChanged(e.Row);
			}
		}

		// Token: 0x17000032 RID: 50
		// (get) Token: 0x0600064D RID: 1613 RVA: 0x0004B491 File Offset: 0x00049691
		protected override string PushEnityKey
		{
			get
			{
				return base.GetFirstEntityKey();
			}
		}

		// Token: 0x0600064E RID: 1614 RVA: 0x0004B49C File Offset: 0x0004969C
		public override void AuthPermissionBeforeF7Select(AuthPermissionBeforeF7SelectEventArgs e)
		{
			base.AuthPermissionBeforeF7Select(e);
			string fieldKey;
			if ((fieldKey = e.FieldKey) != null)
			{
				if (!(fieldKey == "FAnalyisisDeptId"))
				{
					return;
				}
				e.IsIsolationOrg = false;
			}
		}

		// Token: 0x0600064F RID: 1615 RVA: 0x0004B4CF File Offset: 0x000496CF
		public override void BeforeUpdateValue(BeforeUpdateValueEventArgs e)
		{
			base.BeforeUpdateValue(e);
			if (StringUtils.EqualsIgnoreCase(e.Key, "FMaterialId"))
			{
				this.validateMaterial(e);
			}
		}

		// Token: 0x06000650 RID: 1616 RVA: 0x0004B4F4 File Offset: 0x000496F4
		private void validateMaterial(BeforeUpdateValueEventArgs e)
		{
			FormMetadata formMetadata = (FormMetadata)MetaDataServiceHelper.Load(base.Context, "BD_MATERIAL", true);
			List<string> list = new List<string>();
			list.Add("FIsECN");
			DynamicObjectType dynamicObjectType = formMetadata.BusinessInfo.GetSubBusinessInfo(list).GetDynamicObjectType();
			List<DynamicObject> list2 = new List<DynamicObject>();
			if (e.Value is DynamicObject)
			{
				list2.Add((DynamicObject)e.Value);
			}
			else if (e.Value is long)
			{
				list2 = BusinessDataServiceHelper.Load(base.Context, new object[]
				{
					e.Value
				}, dynamicObjectType).ToList<DynamicObject>();
			}
			if (ListUtils.IsEmpty<DynamicObject>(list2))
			{
				return;
			}
			DynamicObjectCollection dynamicValue = DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(list2.FirstOrDefault<DynamicObject>(), "MaterialProduce", null);
			if (!DataEntityExtend.GetDynamicValue<bool>(dynamicValue.FirstOrDefault<DynamicObject>(), "IsECN", false))
			{
				e.Cancel = true;
				base.View.ShowWarnningMessage(ResManager.LoadKDString("工程变更申请单只能录入启用ECN管理的物料!", "0151515151774000013635", 7, new object[0]), "", 0, null, 1);
			}
		}

		// Token: 0x040002C7 RID: 711
		private long lastAuxpropId;
	}
}
