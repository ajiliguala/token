using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using Kingdee.BOS;
using Kingdee.BOS.BusinessEntity.BillType;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.Operation;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.ConvertElement;
using Kingdee.BOS.Core.Metadata.ConvertElement.ServiceArgs;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Core.Permission;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Core.Validation;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.BD.ServiceHelper;
using Kingdee.K3.Core.BD;
using Kingdee.K3.Core.BD.ServiceArgs;
using Kingdee.K3.Core.MFG;
using Kingdee.K3.Core.MFG.ENG.BomExpand;
using Kingdee.K3.Core.MFG.ENG.ParamOption;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.Core.MFG.EntityHelper.SqlMeta;
using Kingdee.K3.Core.MFG.EnumConst;
using Kingdee.K3.Core.MFG.PRD.MO;
using Kingdee.K3.Core.MFG.Utils;
using Kingdee.K3.Core.SCM.STK.SP;
using Kingdee.K3.MFG.Common.BusinessEntity.PRD;
using Kingdee.K3.MFG.ServiceHelper;
using Kingdee.K3.MFG.ServiceHelper.ENG;
using Kingdee.K3.MFG.ServiceHelper.PRD;
using Kingdee.K3.MFG.ServiceHelper.SUB;
using Microsoft.CSharp.RuntimeBinder;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm
{
	// Token: 0x020000AE RID: 174
	[Description("产品结构视图插件")]
	public class ProductStructureView : AbstractDynamicFormPlugIn
	{
		// Token: 0x06000C09 RID: 3081 RVA: 0x00089F04 File Offset: 0x00088104
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
			try
			{
				this.InitializeBomQueryOption();
				this.View.GetControl<EntryGrid>("FBottomEntity").SetGridSelectedType(2);
				this.View.GetControl<EntryGrid>("FBottomEntity").SetFireRowChangeEvent(true);
				if (!this.IsParentMoView)
				{
					Attribute[] array = new Attribute[]
					{
						new SimplePropertyAttribute
						{
							Alias = "ftargetorgid_reg"
						},
						new AutoExpandToResultAttribute(true)
					};
					BomForwardSourceDynamicRow.RegisterSimpleProperty("targetorgid_reg", typeof(long), 0, false, array);
					Attribute[] array2 = new Attribute[]
					{
						new SimplePropertyAttribute
						{
							Alias = "fworkshopid_reg"
						},
						new AutoExpandToResultAttribute(true)
					};
					BomForwardSourceDynamicRow.RegisterSimpleProperty("workshopid_reg", typeof(long), 0, false, array2);
				}
				this.OnInitialize_ForSP(e);
				this.OnInitialize_ForMO(e);
				this.OnInitialize_ForSTK(e);
				this.OnInitialize_ForSAL(e);
			}
			catch (Exception ex)
			{
				this.View.ShowErrMessage(ex.Message, "", 0);
			}
		}

		// Token: 0x06000C0A RID: 3082 RVA: 0x0008A030 File Offset: 0x00088230
		public override void BeforeBindData(EventArgs e)
		{
			base.BeforeBindData(e);
			this.BeforeBindData_ForSP(e);
			this.BeforeBindData_ForMO(e);
			this.BeforeBindData_ForSTK(e);
			this.BeforeBindData_ForSAL(e);
		}

		// Token: 0x06000C0B RID: 3083 RVA: 0x0008A055 File Offset: 0x00088255
		public override void EntityRowDoubleClick(EntityRowClickEventArgs e)
		{
			base.EntityRowDoubleClick(e);
			this.EntityRowDoubleClick_ForSP(e);
			this.EntityRowDoubleClick_ForMO(e);
			this.EntityRowDoubleClick_ForSTK(e);
			this.EntityRowDoubleClick_ForSAL(e);
		}

		// Token: 0x06000C0C RID: 3084 RVA: 0x0008A07C File Offset: 0x0008827C
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			string barItemKey;
			if ((barItemKey = e.BarItemKey) != null && barItemKey == "tbRefresh")
			{
				e.Cancel = this.DoRefresh(null);
			}
			this.BarItemClick_ForSP(e);
			this.BarItemClick_ForMO(e);
			this.BarItemClick_ForSTK(e);
			this.BarItemClick_ForSAL(e);
		}

		// Token: 0x06000C0D RID: 3085 RVA: 0x0008A0C9 File Offset: 0x000882C9
		public override void BeforeUpdateValue(BeforeUpdateValueEventArgs e)
		{
			base.BeforeUpdateValue(e);
			this.BeforeUpdateValue_ForMo(e);
		}

		// Token: 0x06000C0E RID: 3086 RVA: 0x0008A0D9 File Offset: 0x000882D9
		public override void DataChanged(DataChangedEventArgs e)
		{
			base.DataChanged(e);
			this.LoadReferenceFieldValue(e.Field.Key, e.NewValue);
			this.DataChanged_ForMo(e);
			this.DataChanged_ForSTK(e);
			this.DataChanged_ForSAL(e);
		}

		// Token: 0x06000C0F RID: 3087 RVA: 0x0008A110 File Offset: 0x00088310
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			base.BeforeF7Select(e);
			Field field = this.View.Model.BusinessInfo.GetField(e.FieldKey);
			if (field == null || !this.View.StyleManager.GetEnabled(field))
			{
				return;
			}
			string fieldKey;
			if ((fieldKey = e.FieldKey) != null)
			{
				if (!(fieldKey == "FQkParentMaterialId"))
				{
					if (!(fieldKey == "FQkBomId"))
					{
						if (fieldKey == "FPrdOrgId")
						{
							this.BeforeF7Select_ForMo(e);
							return;
						}
						if (fieldKey == "FMOBillType")
						{
							this.BeforeF7Select_ForBillType(e);
							return;
						}
						if (!(fieldKey == "FEnTrustOrgId"))
						{
							return;
						}
						this.BeforeF7Select_ForEnTrustOrg(e);
					}
					else
					{
						BaseDataField baseDataField = ObjectUtils.CreateCopy(this.View.BillBusinessInfo.GetField("FBomId2")) as BaseDataField;
						if (baseDataField != null)
						{
							baseDataField.EntityKey = "FBillHead";
							baseDataField.Entity = this.View.BillBusinessInfo.GetEntity("FBillHead");
							baseDataField.OrgFieldKey = "FUseOrgId";
							string value = MFGBillUtil.GetValue<string>(this.Model, "FQkParentMaterialId", -1, null, null);
							if (!string.IsNullOrWhiteSpace(value))
							{
								baseDataField.Filter = StringUtils.JoinFilterString(baseDataField.Filter, string.Format(" (FMATERIALID.FNumber = '{0}') ", value), "AND");
							}
							string filter = baseDataField.Filter;
							baseDataField.Filter = (ObjectUtils.IsNullOrEmptyOrWhiteSpace(filter) ? " (FBOMCATEGORY='1') " : (filter + " AND (FBOMCATEGORY='1') "));
							MFGCommonUtil.DoF7ButtonClick(this.View, e.FieldKey, null, baseDataField, false, "FNumber", true, 2, 0);
							return;
						}
					}
				}
				else
				{
					if (this.IsParentSTKView)
					{
						this.BeforeF7Select_ForSTK(e);
						return;
					}
					if (this.IsParentSALView)
					{
						this.BeforeF7Select_ForSAL(e);
						return;
					}
					BaseDataField baseDataField2 = ObjectUtils.CreateCopy(this.View.BillBusinessInfo.GetField("FMaterialId2")) as BaseDataField;
					if (baseDataField2 != null)
					{
						baseDataField2.EntityKey = "FBillHead";
						baseDataField2.Entity = this.View.BillBusinessInfo.GetEntity("FBillHead");
						baseDataField2.OrgFieldKey = "FUseOrgId";
						string text = StringUtils.JoinFilterString(baseDataField2.Filter, " FIsMainPrd = '1' ", "AND");
						if (this.View.ParentFormView != null)
						{
							text = StringUtils.JoinFilterString(text, (this.View.ParentFormView.BillBusinessInfo.GetForm().Id == "SUB_SUBREQORDER") ? " FIsSubContract = '1' " : " FIsProduce = '1' ", "AND");
						}
						baseDataField2.Filter = text;
						MFGCommonUtil.DoF7ButtonClick(this.View, e.FieldKey, null, baseDataField2, false, "FNumber", true, 1, 0);
						return;
					}
				}
			}
		}

		// Token: 0x06000C10 RID: 3088 RVA: 0x0008A3A9 File Offset: 0x000885A9
		public override void Dispose()
		{
			base.Dispose();
			BomExpandServiceHelper.ClearBomExpandResult(base.Context, this.MemBomQueryOption);
		}

		// Token: 0x06000C11 RID: 3089 RVA: 0x0008A3C4 File Offset: 0x000885C4
		public override void AfterBindData(EventArgs e)
		{
			base.AfterBindData(e);
			SplitContainer splitContainer = (SplitContainer)this.View.GetControl("FSpliteContainer");
			splitContainer.HideSecondPanel(true);
			this.AfterBindData_ForMo(e);
		}

		// Token: 0x06000C12 RID: 3090 RVA: 0x0008A3FC File Offset: 0x000885FC
		private void LoadReferenceFieldValue(string FieldKey, object NewValue)
		{
			if (FieldKey != null)
			{
				if (!(FieldKey == "FQkParentMaterialId"))
				{
					if (!(FieldKey == "FQkBomId"))
					{
						return;
					}
					string text = null;
					if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(NewValue))
					{
						string value = MFGBillUtil.GetValue<string>(this.View.Model, "FQkParentMaterialId", -1, null, null);
						BaseDataField field = this.View.BillBusinessInfo.GetField("FMaterialId2") as BaseDataField;
						DynamicObjectCollection dynamicObjectCollection = this.LoadReference(field, value, "BD_MATERIAL", new List<string>
						{
							"fmaterialid"
						}, "", false);
						if (dynamicObjectCollection != null && dynamicObjectCollection.Count<DynamicObject>() == 1)
						{
							BaseDataField field2 = this.View.BillBusinessInfo.GetField("FBomId2") as BaseDataField;
							dynamicObjectCollection = this.LoadReference(field2, NewValue.ToString(), "ENG_BOM", new List<string>
							{
								"FID"
							}, string.Format("FMaterialId = {0}", DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObjectCollection[0], "FMaterialId", 0L)), false);
							if (dynamicObjectCollection != null && dynamicObjectCollection.Count<DynamicObject>() == 1)
							{
								text = DataEntityExtend.GetDynamicObjectItemValue<string>(dynamicObjectCollection[0], "FName", null);
								DynamicObject bomObjById = this.GetBomObjById(DataEntityExtend.GetDynamicValue<long>(dynamicObjectCollection[0], "FID", 0L));
								if (bomObjById != null)
								{
									long dynamicValue = DataEntityExtend.GetDynamicValue<long>(bomObjById, "FUNITID_Id", 0L);
									this.View.Model.SetValue("FUnitId", dynamicValue);
								}
							}
						}
					}
					this.View.Model.SetValue("FQkBomName", text);
				}
				else
				{
					this.View.Model.SetValue("FQkParentMaterialName", string.Empty);
					this.View.Model.SetValue("FQkParentMaterialSpec", string.Empty);
					this.View.Model.SetValue("FUnitId", 0);
					this.View.Model.SetValue("FQkBomId", string.Empty);
					this.View.Model.SetValue("FQkBomName", string.Empty);
					BaseDataField field3 = this.View.BillBusinessInfo.GetField("FMaterialId2") as BaseDataField;
					DynamicObjectCollection dynamicObjectCollection2 = null;
					if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(NewValue))
					{
						dynamicObjectCollection2 = this.LoadReference(field3, NewValue.ToString(), "BD_MATERIAL", new List<string>
						{
							"fmaterialid"
						}, string.Format(" FIsMainPrd = '1' AND  {0} = '1' ", (this.View.ParentFormView.BillBusinessInfo.GetForm().Id == "SUB_SUBREQORDER") ? "FIsSubContract" : "FIsProduce"), false);
					}
					if (dynamicObjectCollection2 != null && dynamicObjectCollection2.Count<DynamicObject>() == 1)
					{
						this.HeadMtrlId = DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObjectCollection2[0], "FMaterialId", 0L);
						this.View.Model.SetValue("FQkParentMaterialName", DataEntityExtend.GetDynamicObjectItemValue<string>(dynamicObjectCollection2[0], "FName", null));
						this.View.Model.SetValue("FQkParentMaterialSpec", DataEntityExtend.GetDynamicObjectItemValue<string>(dynamicObjectCollection2[0], "FSpecification", null));
						long num = (this.View.ParentFormView.BillBusinessInfo.GetForm().Id == "SUB_SUBREQORDER") ? DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObjectCollection2[0], "FSubconUnitId", 0L) : DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObjectCollection2[0], "FProduceUnitId", 0L);
						num = ((num > 0L) ? num : DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObjectCollection2[0], "FStoreUnitID", 0L));
						this.View.Model.SetValue("FUnitId", num);
						long dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObjectCollection2[0], "FMaterialId", 0L);
						long[] materialMasterAndUserOrgId = MaterialServiceHelper.GetMaterialMasterAndUserOrgId(base.Context, dynamicObjectItemValue);
						long defaultBomKey = BOMServiceHelper.GetDefaultBomKey(this.View.Context, materialMasterAndUserOrgId[0], materialMasterAndUserOrgId[1], 0L, (this.View.ParentFormView.BillBusinessInfo.GetForm().Id == "SUB_SUBREQORDER") ? 3 : 2);
						DynamicObject bomObjById2 = this.GetBomObjById(defaultBomKey);
						if (bomObjById2 != null)
						{
							this.View.Model.SetValue("FQkBomId", DataEntityExtend.GetDynamicObjectItemValue<string>(bomObjById2, "Number", null));
							string empty = string.Empty;
							DataEntityExtend.GetDynamicObjectItemValue<LocaleValue>(bomObjById2, "Name", null).TryGetValue(base.Context.UserLocale.LCID, ref empty);
							this.View.Model.SetValue("FQkBomName", empty);
							return;
						}
					}
				}
			}
		}

		// Token: 0x06000C13 RID: 3091 RVA: 0x0008A894 File Offset: 0x00088A94
		private bool DoRefresh(Func<bool> preAction = null)
		{
			if (preAction != null && !preAction())
			{
				return false;
			}
			string value = MFGBillUtil.GetValue<string>(this.Model, "FQkParentMaterialId", -1, null, null);
			string value2 = MFGBillUtil.GetValue<string>(this.Model, "FQkBomId", -1, null, null);
			if (ObjectUtils.IsNullOrEmptyOrWhiteSpace(value) && ObjectUtils.IsNullOrEmptyOrWhiteSpace(value2))
			{
				this.View.ShowErrMessage(ResManager.LoadKDString("请录入过滤条件！", "015072000003215", 7, new object[0]), "", 0);
				return false;
			}
			if (MFGBillUtil.GetValue<decimal>(this.Model, "FQty", -1, 0m, null) <= 0m)
			{
				this.View.ShowMessage(ResManager.LoadKDString("请录入套数后，重新刷新！", "015072000002223", 7, new object[0]), 0);
				return false;
			}
			if (this.IsParentMoView)
			{
				if (ObjectUtils.IsNullOrEmptyOrWhiteSpace(value))
				{
					this.View.ShowMessage(ResManager.LoadKDString("请录入产品编码，重新刷新！", "015072000003216", 7, new object[0]), 0);
					return false;
				}
				BaseDataField field = this.View.BillBusinessInfo.GetField("FMaterialId2") as BaseDataField;
				DynamicObjectCollection dynamicObjectCollection = this.LoadReference(field, value, "BD_MATERIAL", new List<string>
				{
					"fmaterialid"
				}, " FIsMainPrd = '1' AND  FIsProduce = '1' ", false);
				if (dynamicObjectCollection != null && dynamicObjectCollection.Count<DynamicObject>() == 1)
				{
					int dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<int>(dynamicObjectCollection[0], "FErpClsID", 0);
					if (dynamicObjectItemValue == 9 && ObjectUtils.IsNullOrEmptyOrWhiteSpace(value2))
					{
						this.View.ShowMessage(string.Format(ResManager.LoadKDString("物料{0}为配置件，请指定BOM版本！", "015072000003217", 7, new object[0]), value), 0);
						return false;
					}
				}
			}
			this.FillBomChildData();
			this.SetChildPrdOrgIdBySupplyOrgId();
			return true;
		}

		// Token: 0x06000C14 RID: 3092 RVA: 0x0008AA38 File Offset: 0x00088C38
		private void SelFieldsVisible(List<string> lstField, bool visible)
		{
			if (ListUtils.IsEmpty<string>(lstField))
			{
				return;
			}
			foreach (string text in lstField)
			{
				this.View.StyleManager.SetVisible(text, null, visible);
			}
		}

		// Token: 0x06000C15 RID: 3093 RVA: 0x0008AA9C File Offset: 0x00088C9C
		private void LayoutQuickField(List<string> lstField)
		{
			int num = 0;
			int num2 = 0;
			foreach (string text in lstField)
			{
				if (num2 % 3 == 0)
				{
					num++;
				}
				int num3 = num2 % 3 + 1;
				FieldAppearance fieldAppearance = this.View.LayoutInfo.GetFieldAppearance(text);
				fieldAppearance.Width = new LocaleValue(318.ToString(), base.Context.UserLocale.LCID);
				fieldAppearance.Height = new LocaleValue(21.ToString(), base.Context.UserLocale.LCID);
				fieldAppearance.LabelWidth = new LocaleValue(100.ToString(), base.Context.UserLocale.LCID);
				fieldAppearance.Left = new LocaleValue((4 + (num3 - 1) * 334).ToString(), base.Context.UserLocale.LCID);
				fieldAppearance.Top = new LocaleValue((8 + (num - 1) * 25).ToString(), base.Context.UserLocale.LCID);
				num2++;
			}
		}

		// Token: 0x06000C16 RID: 3094 RVA: 0x0008AC00 File Offset: 0x00088E00
		private DynamicObjectCollection LoadReference(BaseDataField field, string fnumberValue, string formId, List<string> selectItems = null, string filter = null, bool isSearchByLike = true)
		{
			if (selectItems == null)
			{
				selectItems = new List<string>();
			}
			selectItems.AddRange(from w in field.RefPropertyKeys
			select w.Key);
			QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter
			{
				FormId = field.LookUpObject.FormId,
				SelectItems = SelectorItemInfo.CreateItems(string.Join(",", selectItems)),
				FilterClauseWihtKey = string.Format("{0} like '%{1}%'  AND FDocumentStatus = 'C' AND FForbidStatus = 'A' ", field.LookUpObject.NumberFieldName, fnumberValue)
			};
			if (!string.IsNullOrWhiteSpace(filter))
			{
				queryBuilderParemeter.FilterClauseWihtKey = StringUtils.JoinFilterString(queryBuilderParemeter.FilterClauseWihtKey, filter, "AND");
			}
			if (!isSearchByLike)
			{
				queryBuilderParemeter.FilterClauseWihtKey = StringUtils.JoinFilterString(queryBuilderParemeter.FilterClauseWihtKey, string.Format("{0} = '{1}'", field.LookUpObject.NumberFieldName, fnumberValue), "AND");
			}
			List<SqlParam> list = new List<SqlParam>();
			if (MFGServiceHelper.GetBaseDataPolicyType(base.Context, formId) != 1)
			{
				queryBuilderParemeter.FilterClauseWihtKey = StringUtils.JoinFilterString(queryBuilderParemeter.FilterClauseWihtKey, this.AddFieldFilter("FUseOrgId", "FUseOrgId", list, "="), "AND");
			}
			return QueryServiceHelper.GetDynamicObjectCollection(this.View.Context, queryBuilderParemeter, list);
		}

		// Token: 0x06000C17 RID: 3095 RVA: 0x0008AD3C File Offset: 0x00088F3C
		private DynamicObject GetBomObjById(long bomId)
		{
			FormMetadata formMetaData = MetaDataServiceHelper.GetFormMetaData(base.Context, "ENG_BOM");
			return BusinessDataServiceHelper.LoadFromCache(this.View.Context, new object[]
			{
				bomId
			}, formMetaData.BusinessInfo.GetDynamicObjectType()).FirstOrDefault<DynamicObject>();
		}

		// Token: 0x17000079 RID: 121
		// (get) Token: 0x06000C18 RID: 3096 RVA: 0x0008AD8B File Offset: 0x00088F8B
		// (set) Token: 0x06000C19 RID: 3097 RVA: 0x0008AD93 File Offset: 0x00088F93
		protected MemBomExpandOption_ForPSV MemBomQueryOption { get; set; }

		// Token: 0x06000C1A RID: 3098 RVA: 0x0008AD9C File Offset: 0x00088F9C
		protected virtual void InitializeBomQueryOption()
		{
			DateTime sysDate = MFGServiceHelper.GetSysDate(base.Context);
			MetaDataServiceHelper.GetFormMetaData(base.Context, "ENG_BOM");
			this.MemBomQueryOption = new MemBomExpandOption_ForPSV();
			this.MemBomQueryOption.ExpandLevelTo = 0;
			this.MemBomQueryOption.ValidDate = new DateTime?(sysDate);
			this.MemBomQueryOption.DeleteVirtualMaterial = false;
			this.MemBomQueryOption.ParentCsdYieldRate = false;
			this.MemBomQueryOption.ChildCsdYieldRate = true;
			this.MemBomQueryOption.Mode = 0;
			this.MemBomQueryOption.BomExpandCalType = 0;
			this.MemBomQueryOption.DeleteSkipRow = false;
		}

		// Token: 0x06000C1B RID: 3099 RVA: 0x0008AE38 File Offset: 0x00089038
		protected virtual void UpdateBomQueryOption()
		{
			this.MemBomQueryOption.BomExpandId = SequentialGuid.NewGuid().ToString();
			this.MemBomQueryOption.ExpandLevelTo = MFGBillUtil.GetValue<int>(this.Model, "FExpandLevel", -1, 0, null);
			this.MemBomQueryOption.ExpandVirtualMaterial = MFGBillUtil.GetValue<bool>(this.Model, "FIsExpandVirtualMtrl", -1, false, null);
			this.MemBomQueryOption.CsdSubstitution = MFGBillUtil.GetValue<bool>(this.Model, "FIsShowSubMtrl", -1, false, null);
			DateTime? dateTime = new DateTime?(MFGBillUtil.GetValue<DateTime>(this.Model, "FValidDate", -1, MFGServiceHelper.GetSysDate(base.Context), null));
			if (dateTime != null && dateTime >= new DateTime(1900, 1, 1))
			{
				this.MemBomQueryOption.ValidDate = new DateTime?(dateTime.Value);
			}
		}

		// Token: 0x06000C1C RID: 3100 RVA: 0x0008AF2C File Offset: 0x0008912C
		private DynamicObject BuildBomExpandSourceDataRow(DynamicObject bomData)
		{
			BomForwardSourceDynamicRow bomForwardSourceDynamicRow = BomForwardSourceDynamicRow.CreateInstance();
			long dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<long>(bomData, "FMATERIALID", 0L);
			if (dynamicObjectItemValue <= 0L)
			{
				dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<long>(bomData, "MaterialId_Id", 0L);
			}
			DynamicObject dynamicObject = MFGServiceHelper.GetDynamicObjectCollection(base.Context, new QueryBuilderParemeter
			{
				FormId = "BD_MATERIAL",
				SelectItems = SelectorItemInfo.CreateItems(new string[]
				{
					"FMaterialId",
					"FMasterId",
					"FBaseUnitId"
				}),
				FilterClauseWihtKey = string.Format("FMaterialId={0}", dynamicObjectItemValue)
			}, null).FirstOrDefault<DynamicObject>();
			decimal num = MFGBillUtil.GetValue<decimal>(this.Model, "FQty", -1, 1m, null);
			if (dynamicObject != null)
			{
				long dynamicObjectItemValue2 = DataEntityExtend.GetDynamicObjectItemValue<long>(bomData, "FUNITID", 0L);
				if (dynamicObjectItemValue2 <= 0L)
				{
					dynamicObjectItemValue2 = DataEntityExtend.GetDynamicObjectItemValue<long>(bomData, "FUnitId_Id", 0L);
				}
				long sourceUnitId = (MFGBillUtil.GetValue<long>(this.View.Model, "FUnitId", -1, 0L, null) > 0L) ? MFGBillUtil.GetValue<long>(this.View.Model, "FUnitId", -1, 0L, null) : dynamicObjectItemValue2;
				UnitConvert unitConvertRate = UnitConvertServiceHelper.GetUnitConvertRate(base.Context, new GetUnitConvertRateArgs
				{
					MaterialId = dynamicObjectItemValue,
					MasterId = DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObject, "FMasterId", 0L),
					SourceUnitId = sourceUnitId,
					DestUnitId = DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObject, "FBaseUnitId", 0L)
				});
				if (unitConvertRate != null)
				{
					num = unitConvertRate.ConvertQty(num, "");
				}
				bomForwardSourceDynamicRow.UnitId_Id = dynamicObjectItemValue2;
				bomForwardSourceDynamicRow.BaseUnitId_Id = DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObject, "FBaseUnitId", 0L);
			}
			long dynamicObjectItemValue3 = DataEntityExtend.GetDynamicObjectItemValue<long>(bomData, "FID", 0L);
			if (dynamicObjectItemValue3 <= 0L)
			{
				dynamicObjectItemValue3 = DataEntityExtend.GetDynamicObjectItemValue<long>(bomData, "Id", 0L);
			}
			long auxPropId = 0L;
			if (this.IsParentMoView)
			{
				auxPropId = this.selBOMParamByMO.SpMoEntryLst.First<SimpleMoEntry>().auxPropId;
				string mtoNo = this.selBOMParamByMO.SpMoEntryLst.FirstOrDefault<SimpleMoEntry>().mtoNo;
				DataEntityExtend.SetDynamicObjectItemValue(bomForwardSourceDynamicRow.DataEntity, "MtoNo", mtoNo);
				bomForwardSourceDynamicRow.MtoNo = mtoNo;
			}
			bomForwardSourceDynamicRow.MaterialId_Id = dynamicObjectItemValue;
			bomForwardSourceDynamicRow.BomId_Id = dynamicObjectItemValue3;
			bomForwardSourceDynamicRow.AuxPropId = auxPropId;
			bomForwardSourceDynamicRow.NeedQty = num;
			bomForwardSourceDynamicRow.SupplyOrgId_Id = DataEntityExtend.GetDynamicObjectItemValue<long>(bomData, "FUseOrgId", 0L);
			bomForwardSourceDynamicRow.NeedDate = new DateTime?(MFGBillUtil.GetValue<DateTime>(this.Model, "FValidDate", -1, default(DateTime), null));
			List<Enums.Enu_CalendarOwnerLayerType> list = new List<Enums.Enu_CalendarOwnerLayerType>
			{
				2,
				1
			};
			foreach (Enums.Enu_CalendarOwnerLayerType enu_CalendarOwnerLayerType in list)
			{
				long dynamicObjectItemValue4 = DataEntityExtend.GetDynamicObjectItemValue<long>(this.Model.DataObject, (enu_CalendarOwnerLayerType == 2) ? "WORKSHOPID_Id" : "UseOrgId_Id", 0L);
				DynamicObject workCalendarData = CalendarServiceHelper.GetWorkCalendarData(this.View.Context, dynamicObjectItemValue4, enu_CalendarOwnerLayerType);
				if (workCalendarData != null)
				{
					bomForwardSourceDynamicRow.WorkCalId_Id = DataEntityExtend.GetDynamicObjectItemValue<long>(workCalendarData, "Id", 0L);
					break;
				}
			}
			bomForwardSourceDynamicRow.TimeUnit = 1.ToString();
			if (!this.IsParentMoView)
			{
				DataEntityExtend.SetDynamicObjectItemValue(bomForwardSourceDynamicRow.DataEntity, "targetorgid_reg", this._TargetOrgId);
				bomForwardSourceDynamicRow.SrcFormId_Id = "ENG_BOM";
			}
			return bomForwardSourceDynamicRow.DataEntity;
		}

		// Token: 0x06000C1D RID: 3101 RVA: 0x0008B2B0 File Offset: 0x000894B0
		private DynamicObject ReBuildBomExpandSourceDataRow(DynamicObject sourceData, bool isMo)
		{
			DynamicObjectCollection dynamicValue = DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(sourceData, "TreeEntity", null);
			long entryId = this.selBOMParamByMO.SpMoEntryLst.FirstOrDefault<SimpleMoEntry>().entryId;
			DynamicObject dynamicObject = (from w in dynamicValue
			where DataEntityExtend.GetDynamicValue<long>(w, "Id", 0L) == entryId
			select w).FirstOrDefault<DynamicObject>();
			BomForwardSourceDynamicRow bomForwardSourceDynamicRow = BomForwardSourceDynamicRow.CreateInstance();
			bomForwardSourceDynamicRow.MaterialId_Id = DataEntityExtend.GetDynamicValue<long>(dynamicObject, "MaterialId_Id", 0L);
			bomForwardSourceDynamicRow.AuxPropId = DataEntityExtend.GetDynamicValue<long>(dynamicObject, "AuxPropId_Id", 0L);
			bomForwardSourceDynamicRow.BomId_Id = DataEntityExtend.GetDynamicValue<long>(dynamicObject, "BomId_Id", 0L);
			bomForwardSourceDynamicRow.NeedQty = DataEntityExtend.GetDynamicValue<long>(dynamicObject, "BaseUnitQty", 0L);
			bomForwardSourceDynamicRow.NeedDate = new DateTime?(MFGBillUtil.GetValue<DateTime>(this.Model, "FValidDate", -1, default(DateTime), null));
			List<Enums.Enu_CalendarOwnerLayerType> list = new List<Enums.Enu_CalendarOwnerLayerType>
			{
				2,
				1
			};
			foreach (Enums.Enu_CalendarOwnerLayerType enu_CalendarOwnerLayerType in list)
			{
				long dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<long>(this.Model.DataObject, (enu_CalendarOwnerLayerType == 2) ? "WORKSHOPID_Id" : "UseOrgId_Id", 0L);
				DynamicObject workCalendarData = CalendarServiceHelper.GetWorkCalendarData(this.View.Context, dynamicObjectItemValue, enu_CalendarOwnerLayerType);
				if (workCalendarData != null)
				{
					bomForwardSourceDynamicRow.WorkCalId_Id = DataEntityExtend.GetDynamicObjectItemValue<long>(workCalendarData, "Id", 0L);
					break;
				}
			}
			string mtoNo = this.selBOMParamByMO.SpMoEntryLst.FirstOrDefault<SimpleMoEntry>().mtoNo;
			DataEntityExtend.SetDynamicObjectItemValue(bomForwardSourceDynamicRow.DataEntity, "MtoNo", mtoNo);
			bomForwardSourceDynamicRow.MtoNo = mtoNo;
			bomForwardSourceDynamicRow.BaseUnitId_Id = DataEntityExtend.GetDynamicValue<long>(dynamicObject, "BaseUnitId_Id", 0L);
			bomForwardSourceDynamicRow.UnitId_Id = DataEntityExtend.GetDynamicValue<long>(dynamicObject, "UnitId_Id", 0L);
			bomForwardSourceDynamicRow.DemandOrgId_Id = (isMo ? DataEntityExtend.GetDynamicValue<long>(sourceData, "PrdOrgId_Id", 0L) : DataEntityExtend.GetDynamicValue<long>(sourceData, "SubOrgId_ID", 0L));
			bomForwardSourceDynamicRow.SupplyOrgId_Id = (isMo ? DataEntityExtend.GetDynamicValue<long>(sourceData, "PrdOrgId_Id", 0L) : DataEntityExtend.GetDynamicValue<long>(sourceData, "SubOrgId_ID", 0L));
			bomForwardSourceDynamicRow.TimeUnit = 1.ToString();
			return bomForwardSourceDynamicRow.DataEntity;
		}

		// Token: 0x06000C1E RID: 3102 RVA: 0x0008B4EC File Offset: 0x000896EC
		protected virtual List<DynamicObject> GetBomChildData(List<DynamicObject> lstExpandSource, MemBomExpandOption_ForPSV memBomExpandOption)
		{
			return BomQueryServiceHelper.GetBomQueryForwardResult_ForPSVConvert(base.Context, lstExpandSource, memBomExpandOption);
		}

		// Token: 0x06000C1F RID: 3103 RVA: 0x0008B564 File Offset: 0x00089764
		protected virtual void FillBomChildData()
		{
			this.UpdateBomQueryOption();
			this.Model.DeleteEntryData("FBottomEntity");
			List<DynamicObject> list = new List<DynamicObject>();
			if (this.IsParentMoView)
			{
				string bomNumber = MFGBillUtil.GetValue<string>(this.View.Model, "FQkBomId", -1, null, null);
				if (ObjectUtils.IsNullOrEmptyOrWhiteSpace(bomNumber))
				{
					string value = MFGBillUtil.GetValue<string>(this.View.Model, "FQkParentMaterialId", -1, null, null);
					BaseDataField field = this.View.BillBusinessInfo.GetField("FMaterialId2") as BaseDataField;
					DynamicObjectCollection dynamicObjectCollection = this.LoadReference(field, value, "BD_MATERIAL", new List<string>
					{
						"fmaterialid"
					}, " FIsMainPrd = '1' AND  FIsProduce = '1' ", false);
					if (dynamicObjectCollection != null && dynamicObjectCollection.Count<DynamicObject>() == 1 && DataEntityExtend.GetDynamicObjectItemValue<int>(dynamicObjectCollection[0], "FErpClsID", 0) != 9)
					{
						long num = this.HeadMtrlId = DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObjectCollection[0], "FMaterialId", 0L);
						long[] materialMasterAndUserOrgId = MaterialServiceHelper.GetMaterialMasterAndUserOrgId(base.Context, num);
						long defaultBomKey = BOMServiceHelper.GetDefaultBomKey(this.View.Context, materialMasterAndUserOrgId[0], materialMasterAndUserOrgId[1], 0L, 2);
						DynamicObject bomObjById = this.GetBomObjById(defaultBomKey);
						if (bomObjById != null)
						{
							list.Add(bomObjById);
						}
					}
				}
				else
				{
					DynamicObject item = (from w in this.GetBomInfo()
					where DataEntityExtend.GetDynamicObjectItemValue<string>(w, "FNumber", null) == bomNumber
					select w).FirstOrDefault<DynamicObject>();
					list.Add(item);
				}
			}
			else
			{
				list.AddRange(this.GetBomInfo());
			}
			List<DynamicObject> list2 = (from w in this.PrepareBomOpenSourceData(list)
			where DataEntityExtend.GetDynamicObjectItemValue<long>(w, "MaterialId_Id", 0L) > 0L
			select w).ToList<DynamicObject>();
			if (list2 == null || list2.Count <= 0)
			{
				this.View.ShowMessage(ResManager.LoadKDString("没有符合条件的数据！", "015072000002225", 7, new object[0]), 0);
				return;
			}
			if (this.IsParentMoView && !ObjectUtils.IsNullOrEmpty(this.selBOMParamByMO) && this.selBOMParamByMO.BomExpandSource == "2")
			{
				this.MemBomQueryOption.IsShowOutSource = true;
			}
			this.View.Model.BeginIniti();
			List<DynamicObject> list3 = this.GetBomChildData(list2, this.MemBomQueryOption);
			if (this.IsParentMoView && this.selBOMParamByMO.BomExpandSource == "2")
			{
				List<DynamicObject> ppbomchildData = this.GetPPBOMChildData();
				list3 = (from w in list3
				where DataEntityExtend.GetDynamicValue<int>(w, "BomLevel", 0) == 0
				select w).ToList<DynamicObject>();
				if (!ListUtils.IsEmpty<DynamicObject>(list3))
				{
					string dynamicValue = DataEntityExtend.GetDynamicValue<string>(list3.FirstOrDefault<DynamicObject>(), "EntryId", null);
					foreach (DynamicObject dynamicObject in ppbomchildData)
					{
						int dynamicValue2 = DataEntityExtend.GetDynamicValue<int>(dynamicObject, "BomLevel", 0);
						DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "BomLevel", dynamicValue2 + 1);
						if (dynamicValue2 == 0)
						{
							DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "ParentEntryId", dynamicValue);
						}
						list3.Add(dynamicObject);
					}
				}
			}
			this.bomQueryChildItems = list3;
			List<string> list4 = (from c in list3
			where !ObjectUtils.IsNullOrEmptyOrWhiteSpace(DataEntityExtend.GetDynamicObjectItemValue<string>(c, "ParentEntryId", null))
			select DataEntityExtend.GetDynamicObjectItemValue<string>(c, "ParentEntryId", null)).ToList<string>();
			foreach (DynamicObject dynamicObject2 in list3)
			{
				if (!list4.Contains(DataEntityExtend.GetDynamicObjectItemValue<string>(dynamicObject2, "EntryId", null)))
				{
					dynamicObject2["RowType"] = "16";
				}
				DynamicObject dynamicObject3 = dynamicObject2["MaterialId"] as DynamicObject;
				int num2 = OtherExtend.ConvertTo<int>(((DynamicObjectCollection)dynamicObject3["MaterialBase"])[0]["ErpClsId"], 0);
				if (num2 == 3)
				{
					dynamicObject2["GenerateType"] = "1";
				}
				if (num2 == 2 || num2 == 5 || num2 == 9)
				{
					dynamicObject2["GenerateType"] = "0";
				}
			}
			EntryEntity entryEntity = this.Model.BillBusinessInfo.GetEntryEntity("FBottomEntity");
			if (list3 != null && list3.Count > 0)
			{
				DynamicObjectCollection entityDataObject = this.Model.GetEntityDataObject(entryEntity);
				foreach (DynamicObject item2 in list3)
				{
					entityDataObject.Add(item2);
				}
			}
			this.View.Model.EndIniti();
			if (this.IsParentSPView)
			{
				this.SetRowsDefSelect();
				this.ChangeDataRowColor(-1);
			}
			else if (this.IsParentSTKView)
			{
				this.SetRowsDefSelectForSTK();
				this.ChangeDataRowColorForSTK(-1);
			}
			else if (this.IsParentSALView)
			{
				this.SetRowsDefSelectForSAL();
				this.ChangeDataRowColorForSAL(-1);
			}
			this.View.UpdateView("FBottomEntity");
			this.View.SetEntityFocusRow("FBottomEntity", 0);
		}

		// Token: 0x06000C20 RID: 3104 RVA: 0x0008BAF0 File Offset: 0x00089CF0
		public override void EntryBarItemClick(BarItemClickEventArgs e)
		{
			base.EntryBarItemClick(e);
			string a;
			if ((a = e.BarItemKey.ToUpperInvariant()) != null)
			{
				if (!(a == "TBEXPANDALLROWS"))
				{
					return;
				}
				TreeEntryGrid control = this.View.GetControl<TreeEntryGrid>("FBottomEntity");
				Entity entity = this.View.BusinessInfo.GetEntity("FBottomEntity");
				DynamicObjectCollection entityDataObject = this.View.Model.GetEntityDataObject(entity);
				if (!ListUtils.IsEmpty<DynamicObject>(entityDataObject))
				{
					Dictionary<string, IGrouping<string, DynamicObject>> bomQueryForwardEntryGroupDatas = (from g in entityDataObject
					group g by DataEntityExtend.GetDynamicValue<string>(g, "EntryId", null)).ToDictionary((IGrouping<string, DynamicObject> d) => d.Key);
					Dictionary<string, IGrouping<string, DynamicObject>> bomQueryForwardEntryGroups = (from g in this.bomQueryChildItems
					group g by DataEntityExtend.GetDynamicValue<string>(g, "ParentEntryId", null)).ToDictionary((IGrouping<string, DynamicObject> d) => d.Key);
					int entryCurrentRowIndex = this.View.Model.GetEntryCurrentRowIndex("FBottomEntity");
					DynamicObject entityDataObject2 = this.View.Model.GetEntityDataObject(entity, entryCurrentRowIndex);
					string currEntryId = entityDataObject2["EntryId"].ToString();
					this.ExpandedAllRows(entryCurrentRowIndex, currEntryId, this.bomQueryChildItems, bomQueryForwardEntryGroups, bomQueryForwardEntryGroupDatas, control, entity, entityDataObject);
					this.View.SetEntityFocusRow("FBottomEntity", entryCurrentRowIndex);
				}
			}
		}

		// Token: 0x06000C21 RID: 3105 RVA: 0x0008BC64 File Offset: 0x00089E64
		private void ExpandedAllRows(int currRowIndex, string currEntryId, List<DynamicObject> bomQueryForwardEntrys, Dictionary<string, IGrouping<string, DynamicObject>> bomQueryForwardEntryGroups, Dictionary<string, IGrouping<string, DynamicObject>> bomQueryForwardEntryGroupDatas, TreeEntryGrid treeEntryGrid, Entity entry, DynamicObjectCollection bomQueryEntryDatas)
		{
			IGrouping<string, DynamicObject> source = null;
			if (!bomQueryForwardEntryGroups.TryGetValue(currEntryId, out source))
			{
				return;
			}
			IEnumerable<DynamicObject> enumerable = source.ToList<DynamicObject>();
			if (!ListUtils.IsEmpty<DynamicObject>(enumerable))
			{
				treeEntryGrid.ExpandedRow(currRowIndex);
				foreach (DynamicObject dynamicObject in enumerable)
				{
					string dynamicValue = DataEntityExtend.GetDynamicValue<string>(dynamicObject, "EntryId", null);
					if (!bomQueryForwardEntryGroupDatas.TryGetValue(dynamicValue, out source))
					{
						this.View.Model.CreateNewEntryRow(entry, -1, dynamicObject);
					}
					int currRowIndex2 = bomQueryEntryDatas.IndexOf(dynamicObject);
					this.ExpandedAllRows(currRowIndex2, dynamicValue, bomQueryForwardEntrys, bomQueryForwardEntryGroups, bomQueryForwardEntryGroupDatas, treeEntryGrid, entry, bomQueryEntryDatas);
				}
			}
		}

		// Token: 0x06000C22 RID: 3106 RVA: 0x0008BD1C File Offset: 0x00089F1C
		private IEnumerable<DynamicObject> GetBomInfo()
		{
			string text = string.Empty;
			List<SqlParam> list = new List<SqlParam>();
			text = StringUtils.JoinFilterString(text, this.AddFieldFilter("FQkBomId", "FNumber", list, "like"), "AND");
			text = StringUtils.JoinFilterString(text, this.AddFieldFilter("FQkParentMaterialId", "FMATERIALID.FNumber", list, "="), "AND");
			IEnumerable<DynamicObject> result = new DynamicObject[0];
			List<long> list2 = new List<long>();
			if (this.selBOMParamByMO != null)
			{
				list2.Add(this.selBOMParamByMO.MainOrgId);
			}
			else
			{
				list2.Add(MFGBillUtil.GetValue<long>(this.View.Model, "FUseOrgId", -1, 0L, null));
			}
			if (!string.IsNullOrWhiteSpace(text))
			{
				result = MFGServiceHelper.GetDynamicObjectCollection(base.Context, new QueryBuilderParemeter
				{
					FormId = "ENG_BOM",
					IsolationOrgList = list2,
					IsDistincted = true,
					IsShowApproved = true,
					IsShowUsed = true,
					FilterClauseWihtKey = text + " AND (FBOMCATEGORY='1') ",
					SelectItems = SelectorItemInfo.CreateItems(new string[]
					{
						"FID",
						"FNumber",
						"FMATERIALID",
						"FUNITID",
						"FUseOrgId"
					})
				}, list);
			}
			return result;
		}

		// Token: 0x06000C23 RID: 3107 RVA: 0x0008BE64 File Offset: 0x0008A064
		private string AddFieldFilter(string key, string mappingKey, List<SqlParam> lstParams, string compareOp = "like")
		{
			if (lstParams == null)
			{
				lstParams = new List<SqlParam>();
			}
			Field field = this.View.BusinessInfo.GetField(key);
			object obj = this.Model.GetValue(key);
			if (ObjectUtils.IsNullOrEmptyOrWhiteSpace(obj) || field == null)
			{
				return string.Empty;
			}
			SqlFieldItem sqlFieldItem = SysBclExtend.ToSqlFieldItem(field);
			if (obj is DynamicObject)
			{
				obj = DataEntityExtend.GetDynamicObjectItemValue<long>(obj as DynamicObject, "Id", 0L);
			}
			if (StringUtils.EqualsIgnoreCase(compareOp, "like") && obj.ToString().Split(new char[]
			{
				';'
			}).Count<string>() > 1)
			{
				lstParams.Add(new SqlParam(string.Format("@{0}_P", key), sqlFieldItem.DbType, string.Format("%{0}%", obj)));
				return string.Format(" ({0} {1} @{2}_P OR {0} IN ('{3}') ) ", new object[]
				{
					mappingKey,
					compareOp,
					key,
					string.Join("','", obj.ToString().Split(new char[]
					{
						';'
					}).ToList<string>())
				});
			}
			if (StringUtils.EqualsIgnoreCase(compareOp, "like"))
			{
				obj = string.Format("%{0}%", obj);
			}
			lstParams.Add(new SqlParam(string.Format("@{0}_P", key), sqlFieldItem.DbType, obj));
			return string.Format(" {0} {1} @{2}_P ", mappingKey, compareOp, key);
		}

		// Token: 0x06000C24 RID: 3108 RVA: 0x0008BFC8 File Offset: 0x0008A1C8
		private List<DynamicObject> PrepareBomOpenSourceData(IEnumerable<DynamicObject> lstBomData)
		{
			List<DynamicObject> list = new List<DynamicObject>();
			int num = lstBomData.Count<DynamicObject>();
			if (lstBomData.Count<DynamicObject>() > 20)
			{
				this.View.ShowMessage(string.Format(ResManager.LoadKDString("正在计算{0}个BOM的子项信息，这个可能需要等待一些时间……", "015072000002226", 7, new object[0]), num), 0);
			}
			foreach (DynamicObject bomData in lstBomData)
			{
				list.Add(this.BuildBomExpandSourceDataRow(bomData));
			}
			if (this.IsParentMoView && ListUtils.IsEmpty<DynamicObject>(list) && this.selBOMParamByMO.BomExpandSource == "2")
			{
				bool isMo = this.View.ParentFormView.BillBusinessInfo.GetForm().Id == "PRD_MO";
				foreach (DynamicObject sourceData in this.selBOMParamByMO.sourceDatas)
				{
					list.Add(this.ReBuildBomExpandSourceDataRow(sourceData, isMo));
				}
			}
			return list;
		}

		// Token: 0x06000C25 RID: 3109 RVA: 0x0008C10C File Offset: 0x0008A30C
		public List<DynamicObject> GetPPBOMChildData()
		{
			bool flag = this.View.ParentFormView.BillBusinessInfo.GetForm().Id == "PRD_MO";
			List<DynamicObject> result = new List<DynamicObject>();
			if (ObjectUtils.IsNullOrEmpty(this.selBOMParamByMO))
			{
				return result;
			}
			long entryId = this.selBOMParamByMO.SpMoEntryLst.FirstOrDefault<SimpleMoEntry>().entryId;
			QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter();
			queryBuilderParemeter.FormId = (flag ? "PRD_PPBOM" : "SUB_PPBOM");
			queryBuilderParemeter.SelectItems = new List<SelectorItemInfo>
			{
				new SelectorItemInfo("FID")
			};
			queryBuilderParemeter.FilterClauseWihtKey = string.Format(flag ? " FMOENTRYID={0} " : "FSUBREQENTRYID={0}", entryId);
			DynamicObjectCollection dynamicObjectCollection = QueryServiceHelper.GetDynamicObjectCollection(this.View.Context, queryBuilderParemeter, null);
			if (ListUtils.IsEmpty<DynamicObject>(dynamicObjectCollection))
			{
				return result;
			}
			long dynamicValue = DataEntityExtend.GetDynamicValue<long>(dynamicObjectCollection.FirstOrDefault<DynamicObject>(), "FID", 0L);
			FormMetadata formMetadata = (FormMetadata)MetaDataServiceHelper.Load(base.Context, queryBuilderParemeter.FormId, true);
			DynamicObject[] array = BusinessDataServiceHelper.Load(base.Context, new object[]
			{
				dynamicValue
			}, formMetadata.BusinessInfo.GetDynamicObjectType());
			if (ListUtils.IsEmpty<DynamicObject>(array))
			{
				return result;
			}
			List<DynamicObject> lstExpandSource = this.BuildSourceDatas(array, flag);
			return this.GetBomChildData(lstExpandSource, this.MemBomQueryOption);
		}

		// Token: 0x06000C26 RID: 3110 RVA: 0x0008C26C File Offset: 0x0008A46C
		private List<DynamicObject> BuildSourceDatas(DynamicObject[] ppbomData, bool isMo)
		{
			List<DynamicObject> list = new List<DynamicObject>();
			FormMetadata formMetadata = MetaDataServiceHelper.Load(base.Context, "ENG_BomExpandBill", true) as FormMetadata;
			Entity entity = formMetadata.BusinessInfo.GetEntity("FBomSource");
			List<Enums.Enu_CalendarOwnerLayerType> list2 = new List<Enums.Enu_CalendarOwnerLayerType>
			{
				2,
				1
			};
			long workCalId_Id = 0L;
			foreach (Enums.Enu_CalendarOwnerLayerType enu_CalendarOwnerLayerType in list2)
			{
				long dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<long>(this.Model.DataObject, (enu_CalendarOwnerLayerType == 2) ? "WORKSHOPID_Id" : "UseOrgId_Id", 0L);
				DynamicObject workCalendarData = CalendarServiceHelper.GetWorkCalendarData(this.View.Context, dynamicObjectItemValue, enu_CalendarOwnerLayerType);
				if (workCalendarData != null)
				{
					workCalId_Id = DataEntityExtend.GetDynamicObjectItemValue<long>(workCalendarData, "Id", 0L);
					break;
				}
			}
			DynamicObjectCollection dynamicValue = DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(ppbomData[0], "PPBomEntry", null);
			foreach (DynamicObject dynamicObject in dynamicValue)
			{
				BomForwardSourceDynamicRow bomForwardSourceDynamicRow = new DynamicObject(entity.DynamicObjectType);
				PPBomBillView.PPBomEntry ppbomEntry = dynamicObject;
				PPBomBillView ppbomBillView = (DynamicObject)dynamicObject.Parent;
				long num = isMo ? ppbomBillView.PrdOrgId_Id : DataEntityExtend.GetDynamicValue<long>((DynamicObject)dynamicObject.Parent, "SubOrgId_ID", 0L);
				long num2 = (ppbomEntry.ChildSupplyOrgId_Id > 0L) ? ppbomEntry.ChildSupplyOrgId_Id : num;
				if (ppbomEntry.BOMID_Id <= 0L && ppbomEntry.MaterialID.MaterialBaseList.FirstOrDefault<PPBomBillView.MaterialBase>().ErpClsID != 9.ToString())
				{
					ppbomEntry.BOMID_Id = BOMServiceHelper.GetDefaultBomKey(base.Context, ppbomEntry.MaterialID.MsterID, num2, ppbomEntry.AuxPropID_Id, 2);
				}
				bomForwardSourceDynamicRow.MaterialId_Id = ppbomEntry.MaterialID_Id;
				bomForwardSourceDynamicRow.BomId_Id = ppbomEntry.BOMID_Id;
				bomForwardSourceDynamicRow.AuxPropId = ppbomEntry.AuxPropID_Id;
				bomForwardSourceDynamicRow.NeedQty = ppbomEntry.BaseNeedQty;
				bomForwardSourceDynamicRow.WorkCalId_Id = ppbomEntry.WorkCalId_Id;
				bomForwardSourceDynamicRow.NeedDate = ppbomEntry.NeedDate;
				bomForwardSourceDynamicRow.BaseUnitId_Id = ppbomEntry.BaseUnitID_Id;
				bomForwardSourceDynamicRow.UnitId_Id = ppbomEntry.UnitID_Id;
				bomForwardSourceDynamicRow.SrcEntryId = ppbomEntry.Id;
				bomForwardSourceDynamicRow.SrcInterId = ppbomBillView.Id;
				bomForwardSourceDynamicRow.SrcFormId_Id = (isMo ? "FFORMID" : "FFORMID");
				bomForwardSourceDynamicRow.DemandOrgId_Id = num;
				bomForwardSourceDynamicRow.SupplyOrgId_Id = num2;
				bomForwardSourceDynamicRow.ScrapRate = ppbomEntry.ScrapRate;
				bomForwardSourceDynamicRow.FixScrapRate = ppbomEntry.BaseFixScrapQTY;
				bomForwardSourceDynamicRow.WorkCalId_Id = workCalId_Id;
				bomForwardSourceDynamicRow.TimeUnit = 1.ToString();
				list.Add(bomForwardSourceDynamicRow.DataEntity);
			}
			return list;
		}

		// Token: 0x1700007A RID: 122
		// (get) Token: 0x06000C27 RID: 3111 RVA: 0x0008C578 File Offset: 0x0008A778
		// (set) Token: 0x06000C28 RID: 3112 RVA: 0x0008C580 File Offset: 0x0008A780
		private bool IsDoPush
		{
			get
			{
				return this._isDoPush;
			}
			set
			{
				this._isDoPush = value;
			}
		}

		// Token: 0x1700007B RID: 123
		// (get) Token: 0x06000C29 RID: 3113 RVA: 0x0008C589 File Offset: 0x0008A789
		// (set) Token: 0x06000C2A RID: 3114 RVA: 0x0008C591 File Offset: 0x0008A791
		public OperateOption Option { get; private set; }

		// Token: 0x06000C2B RID: 3115 RVA: 0x0008C5D8 File Offset: 0x0008A7D8
		public ListSelectedRowCollection GetSelectRowInfo(bool getAllSelectRow = true, int row = 0, int generateType = -1, DynamicObjectCollection dymObj = null)
		{
			EntryEntity entryEntity = this.Model.BillBusinessInfo.GetEntryEntity("FBottomEntity");
			dymObj = ((dymObj == null) ? this.Model.GetEntityDataObject(entryEntity) : dymObj);
			IEnumerable<DynamicObject> enumerable;
			if (generateType >= 0)
			{
				enumerable = from w in dymObj
				where DataEntityExtend.GetDynamicObjectItemValue<bool>(w, "IsSelect", false) && DataEntityExtend.GetDynamicObjectItemValue<int>(w, "GenerateType", 0) == generateType
				select w;
			}
			else
			{
				enumerable = from w in dymObj
				where DataEntityExtend.GetDynamicObjectItemValue<bool>(w, "IsSelect", false)
				select w;
			}
			if ((getAllSelectRow && ListUtils.IsEmpty<DynamicObject>(enumerable)) || (!getAllSelectRow && (row < 0 || row >= dymObj.Count)))
			{
				return null;
			}
			ListSelectedRowCollection listSelectedRowCollection = new ListSelectedRowCollection();
			if (getAllSelectRow)
			{
				using (IEnumerator<DynamicObject> enumerator = enumerable.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						DynamicObject dynamicObject = enumerator.Current;
						listSelectedRowCollection.Add(new ListSelectedRow(DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObject, "Id", 0L).ToString(), DataEntityExtend.GetDynamicObjectItemValue<string>(dynamicObject, "EntryId", null).ToString(), DataEntityExtend.GetDynamicObjectItemValue<int>(dynamicObject, "Seq", 0), "ENG_BOM")
						{
							EntryEntityKey = "FBomResult"
						});
					}
					return listSelectedRowCollection;
				}
			}
			listSelectedRowCollection.Add(new ListSelectedRow(DataEntityExtend.GetDynamicObjectItemValue<long>(dymObj[row], "Id", 0L).ToString(), DataEntityExtend.GetDynamicObjectItemValue<string>(dymObj[row], "EntryId", null).ToString(), DataEntityExtend.GetDynamicObjectItemValue<int>(dymObj[row], "Seq", 0), "ENG_BOM")
			{
				EntryEntityKey = "FBomResult"
			});
			return listSelectedRowCollection;
		}

		// Token: 0x06000C2C RID: 3116 RVA: 0x0008C830 File Offset: 0x0008AA30
		private void DoPush(string sourceFormId, string targetFormId, ListSelectedRow[] selectedRows, List<long> prdOrgIds)
		{
			if (selectedRows == null || selectedRows.Length == 0)
			{
				return;
			}
			List<ConvertRuleElement> convertRules = ConvertServiceHelper.GetConvertRules(this.View.Context, sourceFormId, targetFormId);
			ConvertRuleElement convertRuleElement = convertRules.FirstOrDefault((ConvertRuleElement t) => t.IsDefault);
			if (convertRuleElement == null)
			{
				this.View.ShowMessage(ResManager.LoadKDString("规则不存在或者没有指定“默认”转换规则！", "015072000002227", 7, new object[0]), 0);
				return;
			}
			List<ValidationErrorInfo> list = new List<ValidationErrorInfo>();
			List<DynamicObject> list2 = new List<DynamicObject>();
			DynamicObjectCollection source = (DynamicObjectCollection)this.View.Model.DataObject["BomChild"];
			Dictionary<long, IGrouping<long, DynamicObject>> dictionary = (from w in source
			where DataEntityExtend.GetDynamicObjectItemValue<int>(w, "BomLevel", 0) != 0
			select w into g
			group g by DataEntityExtend.GetDynamicObjectItemValue<long>(g, "PrdOrgId_Id", 0L)).ToDictionary((IGrouping<long, DynamicObject> d) => d.Key);
			foreach (long num in prdOrgIds)
			{
				IGrouping<long, DynamicObject> source2;
				if (dictionary.TryGetValue(num, out source2))
				{
					List<IGrouping<object, DynamicObject>> list3 = (from g in source2
					group g by g["MOBILLTYPE_Id"]).ToList<IGrouping<object, DynamicObject>>();
					foreach (IGrouping<object, DynamicObject> grouping in list3)
					{
						List<IGrouping<object, DynamicObject>> list4 = new List<IGrouping<object, DynamicObject>>();
						list4.Add(grouping);
						List<IGrouping<object, DynamicObject>> list5 = (from f in grouping
						where DataEntityExtend.GetDynamicValue<long>(f, "EnTrustOrgId_Id", 0L) != 0L
						select f into g
						group g by g["EnTrustOrgId_Id"]).ToList<IGrouping<object, DynamicObject>>();
						bool flag = false;
						if (!ListUtils.IsEmpty<IGrouping<object, DynamicObject>>(list5))
						{
							list4 = list5;
							flag = true;
						}
						foreach (IGrouping<object, DynamicObject> source3 in list4)
						{
							List<string> subEntryIds = (from s in source3
							select DataEntityExtend.GetDynamicObjectItemValue<string>(s, "EntryId", null)).ToList<string>();
							IEnumerable<ListSelectedRow> source4 = from w in selectedRows
							where subEntryIds.Contains(w.EntryPrimaryKeyValue)
							select w;
							if (source4.Count<ListSelectedRow>() > 0)
							{
								ListSelectedRow[] array = source4.ToArray<ListSelectedRow>();
								Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
								if (flag)
								{
									dictionary2.Add("EntrustOrgId", DataEntityExtend.GetDynamicValue<DynamicObject>(source3.FirstOrDefault<DynamicObject>(), "EntrustOrgId", null));
								}
								dictionary2.Add("BomExpandId", this.MemBomQueryOption.BomExpandId);
								dictionary2.Add("selBOMParamByMO", this.selBOMParamByMO);
								PushArgs pushArgs = new PushArgs(convertRuleElement, array)
								{
									TargetBillTypeId = Convert.ToString(grouping.Key),
									TargetOrgId = num,
									CustomParams = dictionary2
								};
								if (this.Option == null)
								{
									this.Option = OperateOption.Create();
								}
								ConvertOperationResult convertOperationResult = ConvertServiceHelper.Push(this.View.Context, pushArgs, this.Option);
								if (convertOperationResult.ValidationErrors != null && convertOperationResult.ValidationErrors.Count > 0)
								{
									list.AddRange(convertOperationResult.ValidationErrors);
									this.splitErrors.AddRange(convertOperationResult.ValidationErrors);
								}
								else
								{
									List<DynamicObject> list6 = (from p in convertOperationResult.TargetDataEntities
									select p.DataEntity).ToList<DynamicObject>();
									if (StringUtils.EqualsIgnoreCase(targetFormId, "PRD_MO"))
									{
										WorkCenterServiceHelper.UpdateMOREMWorkShopId(base.Context, list6);
									}
									foreach (DynamicObject dynamicObject in list6)
									{
										if (StringUtils.EqualsIgnoreCase(targetFormId, "PRD_MO"))
										{
											this.SetRoutingId(dynamicObject);
											this.SetIsEnableSchedule(dynamicObject);
										}
										if (StringUtils.EqualsIgnoreCase(targetFormId, "SUB_SUBREQORDER"))
										{
											this.SetSubIsEnableSchedule(dynamicObject);
											SubReqServiceHelper.UpdateNoStockInQty(base.Context, new List<DynamicObject>
											{
												dynamicObject
											});
										}
										this.SetPickStatus(dynamicObject);
									}
									DBServiceHelper.LoadReferenceObject(this.View.Context, list6.ToArray(), list6.First<DynamicObject>().DynamicObjectType, true);
									list2.AddRange(list6);
								}
							}
						}
					}
					if (list.Count > 0)
					{
						this.View.ShowWarnningMessage(string.Join(";\r\n", from t in list
						select t.Message), "", 0, null, 1);
						return;
					}
				}
			}
			this.ShowResult(list2.ToArray(), targetFormId, list);
		}

		// Token: 0x06000C2D RID: 3117 RVA: 0x0008CD84 File Offset: 0x0008AF84
		private void SetIsEnableSchedule(DynamicObject billData)
		{
			bool dynamicValue = DataEntityExtend.GetDynamicValue<bool>(billData, "IsRework", false);
			if (dynamicValue)
			{
				return;
			}
			DynamicObjectCollection dynamicObjectCollection = billData["TreeEntity"] as DynamicObjectCollection;
			if (ListUtils.IsEmpty<DynamicObject>(dynamicObjectCollection))
			{
				return;
			}
			for (int i = 0; i < dynamicObjectCollection.Count; i++)
			{
				DynamicObject dynamicObject = dynamicObjectCollection[i];
				int dynamicValue2 = DataEntityExtend.GetDynamicValue<int>(dynamicObject, "ProductType", 0);
				if (dynamicValue2 == 1)
				{
					DataEntityExtend.GetDynamicValue<long>(dynamicObject, "WorkShopID_Id", 0L);
					DynamicObject dynamicValue3 = DataEntityExtend.GetDynamicValue<DynamicObject>(dynamicObject, "MaterialId", null);
					bool dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<bool>(DataEntityExtend.GetDynamicObjectItemValue<DynamicObjectCollection>(dynamicValue3, "MaterialProduce", null).FirstOrDefault<DynamicObject>(), "IsEnableSchedule", false);
					DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "IsEnableSchedule", dynamicObjectItemValue);
				}
			}
		}

		// Token: 0x06000C2E RID: 3118 RVA: 0x0008CE38 File Offset: 0x0008B038
		private void SetSubIsEnableSchedule(DynamicObject billData)
		{
			bool dynamicValue = DataEntityExtend.GetDynamicValue<bool>(billData, "IsRework", false);
			if (dynamicValue)
			{
				return;
			}
			DynamicObjectCollection dynamicObjectCollection = billData["TreeEntity"] as DynamicObjectCollection;
			if (ListUtils.IsEmpty<DynamicObject>(dynamicObjectCollection))
			{
				return;
			}
			for (int i = 0; i < dynamicObjectCollection.Count; i++)
			{
				DynamicObject dynamicObject = dynamicObjectCollection[i];
				int dynamicValue2 = DataEntityExtend.GetDynamicValue<int>(dynamicObject, "ProductType", 0);
				if (dynamicValue2 == 1)
				{
					DynamicObject dynamicValue3 = DataEntityExtend.GetDynamicValue<DynamicObject>(dynamicObject, "MaterialId", null);
					bool dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<bool>(DataEntityExtend.GetDynamicObjectItemValue<DynamicObjectCollection>(dynamicValue3, "MaterialPurchase", null).FirstOrDefault<DynamicObject>(), "IsEnableScheduleSub", false);
					DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "IsEnableSchedule", dynamicObjectItemValue);
				}
			}
		}

		// Token: 0x06000C2F RID: 3119 RVA: 0x0008CEDC File Offset: 0x0008B0DC
		private void SetRoutingId(DynamicObject billData)
		{
			long dynamicValue = DataEntityExtend.GetDynamicValue<long>(billData, "PrdOrgId_Id", 0L);
			string dynamicValue2 = DataEntityExtend.GetDynamicValue<string>(billData, "BillType_Id", null);
			DynamicObjectCollection dynamicObjectCollection = billData["TreeEntity"] as DynamicObjectCollection;
			if (ListUtils.IsEmpty<DynamicObject>(dynamicObjectCollection))
			{
				return;
			}
			for (int i = 0; i < dynamicObjectCollection.Count; i++)
			{
				DynamicObject dynamicObject = dynamicObjectCollection[i];
				long num = DataEntityExtend.GetDynamicValue<long>(dynamicObject, "RoutingId_Id", 0L);
				if (num <= 0L)
				{
					int dynamicValue3 = DataEntityExtend.GetDynamicValue<int>(dynamicObject, "ProductType", 0);
					if (dynamicValue3 == 1)
					{
						long dynamicValue4 = DataEntityExtend.GetDynamicValue<long>(dynamicObject, "WorkShopID_Id", 0L);
						long dynamicValue5 = DataEntityExtend.GetDynamicValue<long>(dynamicObject, "MaterialId_Id", 0L);
						decimal dynamicValue6 = DataEntityExtend.GetDynamicValue<decimal>(dynamicObject, "Qty", 0m);
						DateTime dynamicValue7 = DataEntityExtend.GetDynamicValue<DateTime>(dynamicObject, "PlanStartDate", default(DateTime));
						long dynamicValue8 = DataEntityExtend.GetDynamicValue<long>(dynamicObject, "BomId_Id", 0L);
						long dynamicValue9 = DataEntityExtend.GetDynamicValue<long>(dynamicObject, "AuxPropId_Id", 0L);
						long dynamicValue10 = DataEntityExtend.GetDynamicValue<long>(dynamicObject, "UnitId_Id", 0L);
						num = RouteServiceHelper.GetDefaultRoutingId(base.Context, dynamicValue, dynamicValue4, dynamicValue5, dynamicValue8, dynamicValue9, dynamicValue6, dynamicValue7, null, dynamicValue2, dynamicValue10);
						DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "RoutingId_Id", num);
					}
				}
			}
			MOServiceHelper.UpdateNoStockInQty(base.Context, new List<DynamicObject>
			{
				billData
			});
			DBServiceHelper.LoadReferenceObject(this.View.Context, dynamicObjectCollection.ToArray<DynamicObject>(), dynamicObjectCollection.DynamicCollectionItemPropertyType, true);
		}

		// Token: 0x06000C30 RID: 3120 RVA: 0x0008D054 File Offset: 0x0008B254
		private void SetPickStatus(DynamicObject billData)
		{
			DynamicObjectCollection dynamicObjectCollection = billData["TreeEntity"] as DynamicObjectCollection;
			foreach (DynamicObject dynamicObject in dynamicObjectCollection)
			{
				int dynamicValue = DataEntityExtend.GetDynamicValue<int>(dynamicObject, "ProductType", 0);
				if (dynamicValue == 1)
				{
					DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "PickMtrlStatus", 1);
				}
			}
		}

		// Token: 0x06000C31 RID: 3121 RVA: 0x0008D0C8 File Offset: 0x0008B2C8
		private void SetMOEnTrustOrgId(DynamicObject billData, object billTypeKey, object rowKey)
		{
			if (billTypeKey.Equals(rowKey))
			{
				return;
			}
			DataEntityExtend.SetDynamicObjectItemValue(billData, "ENTrustOrgId_Id", OtherExtend.ConvertTo<long>(rowKey, 0L));
			DataEntityExtend.SetDynamicObjectItemValue(billData, "IsEntrust", true);
		}

		// Token: 0x06000C32 RID: 3122 RVA: 0x0008D110 File Offset: 0x0008B310
		private string GetDefaultBillType(string formId)
		{
			QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter
			{
				FormId = "BOS_BillType",
				SelectItems = SelectorItemInfo.CreateItems("FBillTypeId,FIsDefault"),
				FilterClauseWihtKey = string.Format(" FBillFormID=@formId", new object[0])
			};
			List<SqlParam> list = new List<SqlParam>
			{
				new SqlParam("@formId", 0, formId)
			};
			DynamicObjectCollection dynamicObjectCollection = QueryServiceHelper.GetDynamicObjectCollection(base.Context, queryBuilderParemeter, list);
			if (dynamicObjectCollection.Count == 0)
			{
				return string.Empty;
			}
			DynamicObject dynamicObject = dynamicObjectCollection.FirstOrDefault((DynamicObject x) => OtherExtend.ConvertTo<bool>(x["FIsDefault"], false));
			if (dynamicObject != null)
			{
				return dynamicObject["FBillTypeId"].ToString();
			}
			return dynamicObjectCollection.FirstOrDefault<DynamicObject>()["FBillTypeId"].ToString();
		}

		// Token: 0x06000C33 RID: 3123 RVA: 0x0008D1E4 File Offset: 0x0008B3E4
		public virtual void ShowResult(DynamicObject[] objs, string targetFormId, List<ValidationErrorInfo> errInfo)
		{
			if (!this.IsParentMoView)
			{
				return;
			}
			DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter
			{
				FormId = "ENG_BillMiddle",
				ParentPageId = this.View.PageId
			};
			string pageId = string.Empty;
			string pageId2 = string.Empty;
			if (StringUtils.EqualsIgnoreCase(targetFormId, "PRD_MO"))
			{
				pageId = Convert.ToString(SequentialGuid.NewGuid());
				dynamicFormShowParameter.PageId = pageId;
			}
			if (StringUtils.EqualsIgnoreCase(targetFormId, "SUB_SUBREQORDER"))
			{
				pageId2 = Convert.ToString(SequentialGuid.NewGuid());
				dynamicFormShowParameter.PageId = pageId2;
			}
			dynamicFormShowParameter.OpenStyle.ShowType = 7;
			dynamicFormShowParameter.CustomComplexParams.Add("billDatas", objs);
			dynamicFormShowParameter.CustomComplexParams.Add("fromId", targetFormId);
			this.View.ShowForm(dynamicFormShowParameter);
		}

		// Token: 0x1700007C RID: 124
		// (get) Token: 0x06000C34 RID: 3124 RVA: 0x0008D2AC File Offset: 0x0008B4AC
		private bool IsParentMoView
		{
			get
			{
				return this.View.ParentFormView != null && new List<string>
				{
					"PRD_MO",
					"SUB_SUBREQORDER",
					"BOS_List"
				}.Contains(this.View.ParentFormView.BillBusinessInfo.GetForm().Id);
			}
		}

		// Token: 0x1700007D RID: 125
		// (get) Token: 0x06000C35 RID: 3125 RVA: 0x0008D30F File Offset: 0x0008B50F
		// (set) Token: 0x06000C36 RID: 3126 RVA: 0x0008D317 File Offset: 0x0008B517
		private SelBOMParamByMO selBOMParamByMO { get; set; }

		// Token: 0x06000C37 RID: 3127 RVA: 0x0008D320 File Offset: 0x0008B520
		private void OnInitialize_ForMO(InitializeEventArgs e)
		{
			if (!this.IsParentMoView)
			{
				return;
			}
			if (this.View.ParentFormView != null)
			{
				object obj;
				this.View.ParentFormView.Session.TryGetValue("SelInStockBillParam", out obj);
				if (obj != null && obj is SelBOMParamByMO)
				{
					this.selBOMParamByMO = (obj as SelBOMParamByMO);
					this.View.ParentFormView.Session["SelInStockBillParam"] = null;
				}
			}
			this.RegisterEntityRule_ForPrdMo();
		}

		// Token: 0x06000C38 RID: 3128 RVA: 0x0008D398 File Offset: 0x0008B598
		private void BeforeBindData_ForMO(EventArgs e)
		{
			if (!this.IsParentMoView)
			{
				return;
			}
			this.SelFieldsVisible(new List<string>
			{
				"FIsShowPurchaseMtrl",
				"FIsExpandVirtualMtrl",
				"FValidDate",
				"FGenerateType",
				"FMOBillType"
			}, true);
			this.SelFieldsVisible(new List<string>
			{
				"FIsExpandPurchaseMtrl",
				"FExpandLevel",
				"FWorkShopID",
				"FSupplyOrgId",
				"FSTOCKID",
				"FSTOCKLOCID",
				"FOWNERTYPEID"
			}, false);
			if (this.selBOMParamByMO != null)
			{
				this.Model.SetValue("FUseOrgId", this.selBOMParamByMO.MainOrgId);
				this.IsDoPush = this.selBOMParamByMO.IsNewBill;
				if (!ListUtils.IsEmpty<SimpleMoEntry>(this.selBOMParamByMO.SpMoEntryLst) && !string.IsNullOrWhiteSpace(this.selBOMParamByMO.SpMoEntryLst.First<SimpleMoEntry>().mtrlNumber))
				{
					this.View.Model.SetValue("FQkParentMaterialId", this.selBOMParamByMO.SpMoEntryLst.First<SimpleMoEntry>().mtrlNumber);
					this.LoadReferenceFieldValue("FQkParentMaterialId", this.selBOMParamByMO.SpMoEntryLst.First<SimpleMoEntry>().mtrlNumber);
					this.View.Model.SetValue("FQkBomId", this.selBOMParamByMO.SpMoEntryLst.First<SimpleMoEntry>().bomNumber);
					this.LoadReferenceFieldValue("FQkBomId", this.selBOMParamByMO.SpMoEntryLst.First<SimpleMoEntry>().bomNumber);
					this.View.Model.SetValue("FUnitId", this.selBOMParamByMO.SpMoEntryLst.First<SimpleMoEntry>().unitId);
					if (this.selBOMParamByMO.SpMoEntryLst.First<SimpleMoEntry>().qty > 0m)
					{
						this.View.Model.SetValue("FQty", this.selBOMParamByMO.SpMoEntryLst.First<SimpleMoEntry>().qty);
					}
					this.View.Model.SetValue("FValidDate", this.selBOMParamByMO.SpMoEntryLst.First<SimpleMoEntry>().planStartDate);
					this.View.Model.SetValue("FWorkShopID", this.selBOMParamByMO.SpMoEntryLst.First<SimpleMoEntry>().workShopId);
					this.View.StyleManager.SetEnabled("FWorkShopID", null, this.selBOMParamByMO.SpMoEntryLst.First<SimpleMoEntry>().workShopId <= 0L);
					this.DoRefresh(null);
				}
			}
		}

		// Token: 0x06000C39 RID: 3129 RVA: 0x0008D66A File Offset: 0x0008B86A
		private void AfterBindData_ForMo(EventArgs e)
		{
			if (!this.IsParentMoView)
			{
				return;
			}
			this.SetChildEntrustEnable(0, 0, true);
		}

		// Token: 0x06000C3A RID: 3130 RVA: 0x0008D680 File Offset: 0x0008B880
		private void EntityRowDoubleClick_ForMO(EntityRowClickEventArgs e)
		{
			if (!this.IsParentMoView)
			{
				return;
			}
			EntryEntity entryEntity = this.Model.BillBusinessInfo.GetEntryEntity("FBottomEntity");
			if (string.Equals(e.Key, entryEntity.Key, StringComparison.OrdinalIgnoreCase) && e.Row >= 0 && e.Row < this.View.Model.GetEntryRowCount(entryEntity.Key) && this.View.StyleManager.GetEnabled(this.View.BusinessInfo.GetField("FIsSelect"), this.Model.GetEntityDataObject(entryEntity, e.Row)))
			{
				long value = MFGBillUtil.GetValue<long>(this.View.Model, "FPrdOrgId", e.Row, 0L, null);
				if (value != this.selBOMParamByMO.MainOrgId)
				{
					this.IsDoPush = true;
				}
				if (this.IsDoPush)
				{
					ListSelectedRowCollection selectRowInfo = this.GetSelectRowInfo(false, e.Row, -1, null);
					this.ReturnDataToParentByMo(selectRowInfo.ToArray<ListSelectedRow>(), new List<long>
					{
						value
					}, false);
					return;
				}
				List<DynamicObject> lstSelRows = new List<DynamicObject>
				{
					this.View.Model.GetEntityDataObject(entryEntity).ElementAt(e.Row)
				};
				this.ReturnDataToMoParentBySelect(lstSelRows, false);
			}
		}

		// Token: 0x06000C3B RID: 3131 RVA: 0x0008D7CC File Offset: 0x0008B9CC
		private void BarItemClick_ForMO(BarItemClickEventArgs e)
		{
			if (!this.IsParentMoView)
			{
				return;
			}
			string barItemKey;
			if ((barItemKey = e.BarItemKey) != null)
			{
				if (barItemKey == "tbReturn")
				{
					this.ReturnDataForMo();
					return;
				}
				if (!(barItemKey == "tbReturnAndClose"))
				{
					return;
				}
				this.ReturnDataForMo();
				if (this.lstErrorBomItems.Count <= 0 && this.splitErrors.Count <= 0)
				{
					this.View.Close();
				}
			}
		}

		// Token: 0x06000C3C RID: 3132 RVA: 0x0008D87C File Offset: 0x0008BA7C
		private void ReturnDataForMo()
		{
			ListSelectedRowCollection selectRowInfo = this.GetSelectRowInfo(true, 0, -1, null);
			if (selectRowInfo == null || selectRowInfo.Count <= 0)
			{
				return;
			}
			List<string> entryIds = (from s in selectRowInfo
			select s.EntryPrimaryKeyValue).ToList<string>();
			DynamicObjectCollection source = (DynamicObjectCollection)this.View.Model.DataObject["BomChild"];
			List<long> prdOrgIds = (from w in source
			where entryIds.Contains(DataEntityExtend.GetDynamicObjectItemValue<string>(w, "EntryId", null))
			select w into g
			group g by DataEntityExtend.GetDynamicObjectItemValue<long>(g, "PrdOrgId_Id", 0L) into s
			select s.Key).ToList<long>();
			MFGBillUtil.GetValue<long>(this.View.Model, "FUseOrgId", -1, 0L, null);
			if (this.selBOMParamByMO.IsNewBill)
			{
				this.ReturnDataToParentByMo(selectRowInfo.ToArray<ListSelectedRow>(), prdOrgIds, false);
				return;
			}
			this.ReturnDataToDraw(prdOrgIds);
		}

		// Token: 0x06000C3D RID: 3133 RVA: 0x0008DA0C File Offset: 0x0008BC0C
		private void ReturnDataToDraw(List<long> prdOrgIds)
		{
			IDynamicFormView parentFormView = this.View.ParentFormView;
			int generateType = 1;
			int entryRowGenerateType = 0;
			if (parentFormView != null && parentFormView.BillBusinessInfo.GetForm().Id == "SUB_SUBREQORDER")
			{
				generateType = 0;
				entryRowGenerateType = 1;
			}
			ListSelectedRowCollection listSelectedRowCollection = this.GetSelectRowInfo(true, 0, generateType, null);
			EntryEntity entryEntity = this.Model.BillBusinessInfo.GetEntryEntity("FBottomEntity");
			List<DynamicObject> list = (from w in this.View.Model.GetEntityDataObject(entryEntity)
			where DataEntityExtend.GetDynamicObjectItemValue<bool>(w, "IsSelect", false) && DataEntityExtend.GetDynamicObjectItemValue<int>(w, "GenerateType", 0) == entryRowGenerateType && DataEntityExtend.GetDynamicValue<int>(w, "BomLevel", 0) > 0
			select w).ToList<DynamicObject>();
			listSelectedRowCollection = (listSelectedRowCollection ?? new ListSelectedRowCollection());
			if (!ListUtils.IsEmpty<DynamicObject>(list))
			{
				IEnumerable<IGrouping<long, DynamicObject>> source = from x in list
				group x by OtherExtend.ConvertTo<long>(x["PrdOrgId_Id"], 0L);
				IGrouping<long, DynamicObject> grouping = source.FirstOrDefault((IGrouping<long, DynamicObject> x) => x.Key == this.selBOMParamByMO.MainOrgId);
				List<IGrouping<long, DynamicObject>> list2 = (from x in source
				where x.Key != this.selBOMParamByMO.MainOrgId
				select x).ToList<IGrouping<long, DynamicObject>>();
				foreach (IGrouping<long, DynamicObject> grouping2 in list2)
				{
					foreach (DynamicObject dynamicObject in grouping2)
					{
						listSelectedRowCollection.Add(new ListSelectedRow(DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObject, "Id", 0L).ToString(), DataEntityExtend.GetDynamicObjectItemValue<string>(dynamicObject, "EntryId", null).ToString(), DataEntityExtend.GetDynamicObjectItemValue<int>(dynamicObject, "Seq", 0), "ENG_BOM")
						{
							EntryEntityKey = "FBomResult"
						});
					}
				}
				Dictionary<long, long> dictionary = new Dictionary<long, long>();
				object obj;
				this.selBOMParamByMO.BillHeadKeyValue.TryGetValue("FBillType", out obj);
				List<DynamicObject> list3 = new List<DynamicObject>();
				if (!ListUtils.IsEmpty<DynamicObject>(grouping))
				{
					foreach (DynamicObject dynamicObject2 in grouping)
					{
						if (StringUtils.EqualsIgnoreCase(DataEntityExtend.GetDynamicObjectItemValue<string>(dynamicObject2, "MOBILLTYPE_Id", null), OtherExtend.ConvertTo<string>(obj, null)) && ((this.selBOMParamByMO.IsEntrust && DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObject2, "OWNERID_Id", 0L) == this.selBOMParamByMO.ENTrustOrgId) || DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObject2, "OWNERID_Id", 0L) == 0L || !this.selBOMParamByMO.IsEntrust))
						{
							list3.Add(dynamicObject2);
							dictionary.Add(DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObject2, "Id", 0L), this.selBOMParamByMO.MainOrgId);
						}
						else
						{
							listSelectedRowCollection.Add(new ListSelectedRow(DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObject2, "Id", 0L).ToString(), DataEntityExtend.GetDynamicObjectItemValue<string>(dynamicObject2, "EntryId", null).ToString(), DataEntityExtend.GetDynamicObjectItemValue<int>(dynamicObject2, "Seq", 0), "ENG_BOM")
							{
								EntryEntityKey = "FBomResult"
							});
						}
					}
				}
				if (grouping != null)
				{
					ProductStructureViewServiceHelper.UpdateSupplyOrgId(this.View.Context, dictionary);
					this.ReturnDataToMoParentBySelect(list3, false);
				}
			}
			if (listSelectedRowCollection != null && !ListUtils.IsEmpty<ListSelectedRow>(listSelectedRowCollection))
			{
				this.ReturnDataToParentByMo(listSelectedRowCollection.ToArray<ListSelectedRow>(), prdOrgIds, false);
			}
		}

		// Token: 0x06000C3E RID: 3134 RVA: 0x0008DDC4 File Offset: 0x0008BFC4
		private void ReturnDataToMoParentBySelect(List<DynamicObject> lstSelRows, bool isReturnAndClose = false)
		{
			if (ListUtils.IsEmpty<DynamicObject>(lstSelRows) || this.MemBomQueryOption == null)
			{
				this.View.ShowMessage(ResManager.LoadKDString("请选择分录！", "015072000002224", 7, new object[0]), 0);
				return;
			}
			this.selBOMParamByMO.ListSelRows = lstSelRows;
			ProductStructureViewServiceHelper.UpdateBomExpandTmpTableSourceInfo(this.View.Context, "T_ENG_BOMEXPANDRESULT", (from w in lstSelRows
			select DataEntityExtend.GetDynamicObjectItemValue<long>(w, "BomEntryId", 0L)).ToList<long>());
			if (this.View.ParentFormView != null && this.View.ParentFormView is IDynamicFormViewService)
			{
				this.View.ReturnToParentWindow(this.selBOMParamByMO);
				(this.View.ParentFormView as IDynamicFormViewService).CustomEvents(this.View.PageId, "CustomSelBill", "returnData");
				this.View.SendDynamicFormAction(this.View.ParentFormView);
				if (isReturnAndClose)
				{
					this.View.Close();
				}
			}
		}

		// Token: 0x06000C3F RID: 3135 RVA: 0x0008DECC File Offset: 0x0008C0CC
		private void BeforeF7Select_ForMo(BeforeF7SelectEventArgs e)
		{
			if (!this.IsParentMoView || MFGServiceHelper.GetBaseDataPolicyType(base.Context, "BD_MATERIAL") == 1)
			{
				return;
			}
			e.EnableUICache = false;
			long value = MFGBillUtil.GetValue<long>(this.View.Model, "FMaterialId2", e.Row, 0L, null);
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(" FORGID IN ");
			stringBuilder.AppendLine(" (SELECT TM.FUSEORGID FROM T_BD_MATERIAL TM ");
			stringBuilder.AppendLine(" INNER JOIN T_BD_MATERIALBASE TMB ON TM.FMATERIALID=TMB.FMATERIALID ");
			stringBuilder.AppendLine(string.Format(" AND EXISTS(SELECT 1 FROM T_BD_MATERIAL TMN WHERE TM.FMASTERID=TMN.FMASTERID AND TMN.FMATERIALID={0})) ", value));
			if (ObjectUtils.IsNullOrEmptyOrWhiteSpace(e.ListFilterParameter.Filter))
			{
				e.ListFilterParameter.Filter = stringBuilder.ToString();
				return;
			}
			IRegularFilterParameter listFilterParameter = e.ListFilterParameter;
			listFilterParameter.Filter = listFilterParameter.Filter + " AND " + stringBuilder.ToString();
		}

		// Token: 0x06000C40 RID: 3136 RVA: 0x0008DFB4 File Offset: 0x0008C1B4
		private void BeforeF7Select_ForBillType(BeforeF7SelectEventArgs e)
		{
			if (!this.IsParentMoView || MFGServiceHelper.GetBaseDataPolicyType(base.Context, "BD_MATERIAL") == 1)
			{
				return;
			}
			e.EnableUICache = false;
			string value = MFGBillUtil.GetValue<string>(this.View.Model, "FGenerateType", e.Row, null, null);
			StringBuilder stringBuilder = new StringBuilder();
			if (value == "0")
			{
				stringBuilder.Append(" FBillFormID='PRD_MO'");
				stringBuilder.Append(string.Format(" AND FNUMBER IN ('{0}')", string.Join("','", from o in this.MOBillTypes
				select o.Number)));
			}
			else if (value == "1")
			{
				stringBuilder.Append(" FBillFormID='SUB_SUBREQORDER'");
				stringBuilder.Append(string.Format(" AND FNUMBER IN ('{0}')", string.Join("','", from o in this.SubBillTypes
				select o.Number)));
			}
			else
			{
				stringBuilder.Append(" 1!=1");
			}
			e.ListFilterParameter.Filter = stringBuilder.ToString();
		}

		// Token: 0x06000C41 RID: 3137 RVA: 0x0008E0E4 File Offset: 0x0008C2E4
		private void BeforeF7Select_ForEnTrustOrg(BeforeF7SelectEventArgs e)
		{
			if (!this.IsParentMoView || MFGServiceHelper.GetBaseDataPolicyType(base.Context, "BD_MATERIAL") == 1)
			{
				return;
			}
			e.EnableUICache = false;
			long value = MFGBillUtil.GetValue<long>(this.View.Model, "FPrdOrgId", e.Row, 0L, null);
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendFormat("EXISTS\r\n(select *\r\n  from t_org_bizrelation tob\r\n inner join t_org_bizrelationentry tobe\r\n    on tob.fbizrelationid = tobe.fbizrelationid\r\n where fbrtypeid = 109 and tobe.forgid =t0.FOrgId and tobe.frelationorgid = {0}) AND t0.FOrgId<>{0}", value);
			e.ListFilterParameter.Filter = stringBuilder.ToString();
		}

		// Token: 0x1700007E RID: 126
		// (get) Token: 0x06000C42 RID: 3138 RVA: 0x0008E15C File Offset: 0x0008C35C
		// (set) Token: 0x06000C43 RID: 3139 RVA: 0x0008E164 File Offset: 0x0008C364
		private List<SimpleBillType> SubBillTypes { get; set; }

		// Token: 0x1700007F RID: 127
		// (get) Token: 0x06000C44 RID: 3140 RVA: 0x0008E16D File Offset: 0x0008C36D
		// (set) Token: 0x06000C45 RID: 3141 RVA: 0x0008E175 File Offset: 0x0008C375
		private string DefaultSubBillType { get; set; }

		// Token: 0x06000C46 RID: 3142 RVA: 0x0008E19C File Offset: 0x0008C39C
		private void InitSubBillType(DynamicObject dyObj, dynamic dymic)
		{
			this.SubBillTypes = BusinessDataServiceHelper.GetBillTypeInfos(base.Context, "SUB_SUBREQORDER", "SubType", "1", "SubReqOrderParamSetting");
			List<SimpleBillType> billTypeInfos = BusinessDataServiceHelper.GetBillTypeInfos(base.Context, "SUB_SUBREQORDER", "SubType", "2", "SubReqOrderParamSetting");
			this.SubBillTypes.AddRange(billTypeInfos);
			List<SimpleBillType> list = new List<SimpleBillType>();
			list = (from o in this.SubBillTypes
			where o.IsDefault.Value
			select o).ToList<SimpleBillType>();
			string empty = string.Empty;
			if (!ListUtils.IsEmpty<SimpleBillType>(list))
			{
				this.DefaultSubBillType = list.FirstOrDefault<SimpleBillType>().Id;
				return;
			}
			if (!ListUtils.IsEmpty<SimpleBillType>(this.SubBillTypes))
			{
				this.DefaultSubBillType = this.SubBillTypes.FirstOrDefault<SimpleBillType>().Id;
			}
		}

		// Token: 0x17000080 RID: 128
		// (get) Token: 0x06000C47 RID: 3143 RVA: 0x0008E271 File Offset: 0x0008C471
		// (set) Token: 0x06000C48 RID: 3144 RVA: 0x0008E279 File Offset: 0x0008C479
		private List<SimpleBillType> MOBillTypes { get; set; }

		// Token: 0x17000081 RID: 129
		// (get) Token: 0x06000C49 RID: 3145 RVA: 0x0008E282 File Offset: 0x0008C482
		// (set) Token: 0x06000C4A RID: 3146 RVA: 0x0008E28A File Offset: 0x0008C48A
		private string DefaultMOBillType { get; set; }

		// Token: 0x06000C4B RID: 3147 RVA: 0x0008E2B0 File Offset: 0x0008C4B0
		private void InitMOBillType(DynamicObject dyObj, dynamic dymic)
		{
			this.MOBillTypes = BusinessDataServiceHelper.GetBillTypeInfos(base.Context, "PRD_MO", "ProductType", "1", "MoBillTypeParaSetting");
			List<SimpleBillType> billTypeInfos = BusinessDataServiceHelper.GetBillTypeInfos(base.Context, "PRD_MO", "ProductType", "2", "MoBillTypeParaSetting");
			this.MOBillTypes.AddRange(billTypeInfos);
			List<SimpleBillType> list = new List<SimpleBillType>();
			list = (from o in this.MOBillTypes
			where o.IsDefault.Value
			select o).ToList<SimpleBillType>();
			if (!ListUtils.IsEmpty<SimpleBillType>(list))
			{
				this.DefaultMOBillType = list.FirstOrDefault<SimpleBillType>().Id;
				return;
			}
			this.DefaultMOBillType = this.MOBillTypes.FirstOrDefault<SimpleBillType>().Id;
		}

		// Token: 0x06000C4C RID: 3148 RVA: 0x0008E374 File Offset: 0x0008C574
		private void BeforeUpdateValue_ForMo(BeforeUpdateValueEventArgs e)
		{
			if (!this.IsParentMoView)
			{
				return;
			}
			string key;
			if ((key = e.Key) != null)
			{
				if (!(key == "FQkParentMaterialId"))
				{
					if (key == "FPrdOrgId")
					{
						e.Cancel = !this.BeforeUpdateValue_ForMoPrdOrgId(e);
						return;
					}
					if (!(key == "FGenerateType"))
					{
						return;
					}
					DynamicObject dynamicObject = this.View.Model.GetValue("FMaterialId2", e.Row) as DynamicObject;
					string text = string.Empty;
					if (dynamicObject != null)
					{
						object arg = dynamicObject;
						if (e.Value.ToString() == "0")
						{
							if (ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site73 == null)
							{
								ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site73 = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsTrue, typeof(ProductStructureView), new CSharpArgumentInfo[]
								{
									CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
								}));
							}
							Func<CallSite, object, bool> target = ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site73.Target;
							CallSite <>p__Site = ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site73;
							if (ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site74 == null)
							{
								ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site74 = CallSite<Func<CallSite, object, object>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.Not, typeof(ProductStructureView), new CSharpArgumentInfo[]
								{
									CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
								}));
							}
							Func<CallSite, object, object> target2 = ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site74.Target;
							CallSite <>p__Site2 = ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site74;
							if (ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site75 == null)
							{
								ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site75 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "IsProduce", typeof(ProductStructureView), new CSharpArgumentInfo[]
								{
									CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
								}));
							}
							Func<CallSite, object, object> target3 = ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site75.Target;
							CallSite <>p__Site3 = ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site75;
							if (ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site76 == null)
							{
								ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site76 = CallSite<Func<CallSite, object, int, object>>.Create(Binder.GetIndex(CSharpBinderFlags.None, typeof(ProductStructureView), new CSharpArgumentInfo[]
								{
									CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
									CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null)
								}));
							}
							Func<CallSite, object, int, object> target4 = ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site76.Target;
							CallSite <>p__Site4 = ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site76;
							if (ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site77 == null)
							{
								ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site77 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.ResultIndexed, "MaterialBase", typeof(ProductStructureView), new CSharpArgumentInfo[]
								{
									CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
								}));
							}
							object obj = target2(<>p__Site2, target3(<>p__Site3, target4(<>p__Site4, ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site77.Target(ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site77, arg), 0)));
							if (ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site78 == null)
							{
								ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site78 = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsFalse, typeof(ProductStructureView), new CSharpArgumentInfo[]
								{
									CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
								}));
							}
							object arg3;
							if (!ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site78.Target(ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site78, obj))
							{
								if (ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site79 == null)
								{
									ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site79 = CallSite<Func<CallSite, object, object, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.BinaryOperationLogical, ExpressionType.And, typeof(ProductStructureView), new CSharpArgumentInfo[]
									{
										CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
										CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
									}));
								}
								Func<CallSite, object, object, object> target5 = ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site79.Target;
								CallSite <>p__Site5 = ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site79;
								object arg2 = obj;
								if (ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site7a == null)
								{
									ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site7a = CallSite<Func<CallSite, object, string, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.NotEqual, typeof(ProductStructureView), new CSharpArgumentInfo[]
									{
										CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
										CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null)
									}));
								}
								Func<CallSite, object, string, object> target6 = ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site7a.Target;
								CallSite <>p__Site7a = ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site7a;
								if (ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site7b == null)
								{
									ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site7b = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "ErpClsId", typeof(ProductStructureView), new CSharpArgumentInfo[]
									{
										CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
									}));
								}
								Func<CallSite, object, object> target7 = ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site7b.Target;
								CallSite <>p__Site7b = ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site7b;
								if (ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site7c == null)
								{
									ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site7c = CallSite<Func<CallSite, object, int, object>>.Create(Binder.GetIndex(CSharpBinderFlags.None, typeof(ProductStructureView), new CSharpArgumentInfo[]
									{
										CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
										CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null)
									}));
								}
								Func<CallSite, object, int, object> target8 = ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site7c.Target;
								CallSite <>p__Site7c = ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site7c;
								if (ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site7d == null)
								{
									ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site7d = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.ResultIndexed, "MaterialBase", typeof(ProductStructureView), new CSharpArgumentInfo[]
									{
										CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
									}));
								}
								arg3 = target5(<>p__Site5, arg2, target6(<>p__Site7a, target7(<>p__Site7b, target8(<>p__Site7c, ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site7d.Target(ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site7d, arg), 0)), "1"));
							}
							else
							{
								arg3 = obj;
							}
							if (target(<>p__Site, arg3))
							{
								if (ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site7e == null)
								{
									ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site7e = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "Number", typeof(ProductStructureView), new CSharpArgumentInfo[]
									{
										CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
									}));
								}
								object arg4 = ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site7e.Target(ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site7e, arg);
								if (ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site7f == null)
								{
									ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site7f = CallSite<Action<CallSite, IDynamicFormView, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.ResultDiscarded, "ShowErrMessage", null, typeof(ProductStructureView), new CSharpArgumentInfo[]
									{
										CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
										CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
									}));
								}
								Action<CallSite, IDynamicFormView, object> target9 = ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site7f.Target;
								CallSite <>p__Site7f = ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site7f;
								IDynamicFormView view = this.View;
								if (ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site80 == null)
								{
									ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site80 = CallSite<Func<CallSite, Type, string, object, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.None, "Format", null, typeof(ProductStructureView), new CSharpArgumentInfo[]
									{
										CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.IsStaticType, null),
										CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
										CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
									}));
								}
								target9(<>p__Site7f, view, ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site80.Target(ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site80, typeof(string), ResManager.LoadKDString("物料{0}未勾选允许生产，订单类型不能为生产.", "015072000014404", 7, new object[0]), arg4));
								e.Cancel = true;
								return;
							}
							DynamicObjectCollection dynamicObjectCollection = (DynamicObjectCollection)dynamicObject["MaterialProduce"];
							if (dynamicObjectCollection.Count > 0)
							{
								if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(DataEntityExtend.GetDynamicValue<string>(dynamicObjectCollection[0], "ProduceBillType_Id", null)))
								{
									text = DataEntityExtend.GetDynamicValue<string>(dynamicObjectCollection[0], "ProduceBillType_Id", null);
								}
								else if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(DataEntityExtend.GetDynamicValue<string>(dynamicObjectCollection[0], "OrgTrustBillType_Id", null)))
								{
									text = DataEntityExtend.GetDynamicValue<string>(dynamicObjectCollection[0], "OrgTrustBillType_Id", null);
								}
							}
							if (ObjectUtils.IsNullOrEmptyOrWhiteSpace(text))
							{
								text = this.DefaultMOBillType;
							}
							this.View.Model.SetValue("FMOBillType", text, e.Row);
							return;
						}
						else if (e.Value.ToString() == "1")
						{
							if (ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site81 == null)
							{
								ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site81 = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsTrue, typeof(ProductStructureView), new CSharpArgumentInfo[]
								{
									CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
								}));
							}
							Func<CallSite, object, bool> target10 = ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site81.Target;
							CallSite <>p__Site6 = ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site81;
							if (ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site82 == null)
							{
								ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site82 = CallSite<Func<CallSite, object, object>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.Not, typeof(ProductStructureView), new CSharpArgumentInfo[]
								{
									CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
								}));
							}
							Func<CallSite, object, object> target11 = ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site82.Target;
							CallSite <>p__Site7 = ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site82;
							if (ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site83 == null)
							{
								ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site83 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "IsSubContract", typeof(ProductStructureView), new CSharpArgumentInfo[]
								{
									CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
								}));
							}
							Func<CallSite, object, object> target12 = ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site83.Target;
							CallSite <>p__Site8 = ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site83;
							if (ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site84 == null)
							{
								ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site84 = CallSite<Func<CallSite, object, int, object>>.Create(Binder.GetIndex(CSharpBinderFlags.None, typeof(ProductStructureView), new CSharpArgumentInfo[]
								{
									CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
									CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null)
								}));
							}
							Func<CallSite, object, int, object> target13 = ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site84.Target;
							CallSite <>p__Site9 = ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site84;
							if (ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site85 == null)
							{
								ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site85 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.ResultIndexed, "MaterialBase", typeof(ProductStructureView), new CSharpArgumentInfo[]
								{
									CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
								}));
							}
							object obj2 = target11(<>p__Site7, target12(<>p__Site8, target13(<>p__Site9, ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site85.Target(ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site85, arg), 0)));
							if (ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site86 == null)
							{
								ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site86 = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsFalse, typeof(ProductStructureView), new CSharpArgumentInfo[]
								{
									CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
								}));
							}
							object arg6;
							if (!ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site86.Target(ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site86, obj2))
							{
								if (ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site87 == null)
								{
									ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site87 = CallSite<Func<CallSite, object, object, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.BinaryOperationLogical, ExpressionType.And, typeof(ProductStructureView), new CSharpArgumentInfo[]
									{
										CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
										CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
									}));
								}
								Func<CallSite, object, object, object> target14 = ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site87.Target;
								CallSite <>p__Site10 = ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site87;
								object arg5 = obj2;
								if (ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site88 == null)
								{
									ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site88 = CallSite<Func<CallSite, object, string, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.NotEqual, typeof(ProductStructureView), new CSharpArgumentInfo[]
									{
										CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
										CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null)
									}));
								}
								Func<CallSite, object, string, object> target15 = ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site88.Target;
								CallSite <>p__Site11 = ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site88;
								if (ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site89 == null)
								{
									ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site89 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "ErpClsId", typeof(ProductStructureView), new CSharpArgumentInfo[]
									{
										CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
									}));
								}
								Func<CallSite, object, object> target16 = ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site89.Target;
								CallSite <>p__Site12 = ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site89;
								if (ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site8a == null)
								{
									ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site8a = CallSite<Func<CallSite, object, int, object>>.Create(Binder.GetIndex(CSharpBinderFlags.None, typeof(ProductStructureView), new CSharpArgumentInfo[]
									{
										CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
										CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null)
									}));
								}
								Func<CallSite, object, int, object> target17 = ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site8a.Target;
								CallSite <>p__Site8a = ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site8a;
								if (ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site8b == null)
								{
									ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site8b = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.ResultIndexed, "MaterialBase", typeof(ProductStructureView), new CSharpArgumentInfo[]
									{
										CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
									}));
								}
								arg6 = target14(<>p__Site10, arg5, target15(<>p__Site11, target16(<>p__Site12, target17(<>p__Site8a, ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site8b.Target(ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site8b, arg), 0)), "1"));
							}
							else
							{
								arg6 = obj2;
							}
							if (target10(<>p__Site6, arg6))
							{
								if (ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site8c == null)
								{
									ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site8c = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "Number", typeof(ProductStructureView), new CSharpArgumentInfo[]
									{
										CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
									}));
								}
								object arg7 = ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site8c.Target(ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site8c, arg);
								if (ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site8d == null)
								{
									ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site8d = CallSite<Action<CallSite, IDynamicFormView, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.ResultDiscarded, "ShowErrMessage", null, typeof(ProductStructureView), new CSharpArgumentInfo[]
									{
										CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
										CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
									}));
								}
								Action<CallSite, IDynamicFormView, object> target18 = ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site8d.Target;
								CallSite <>p__Site8d = ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site8d;
								IDynamicFormView view2 = this.View;
								if (ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site8e == null)
								{
									ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site8e = CallSite<Func<CallSite, Type, string, object, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.None, "Format", null, typeof(ProductStructureView), new CSharpArgumentInfo[]
									{
										CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.IsStaticType, null),
										CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
										CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
									}));
								}
								target18(<>p__Site8d, view2, ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site8e.Target(ProductStructureView.<BeforeUpdateValue_ForMo>o__SiteContainer72.<>p__Site8e, typeof(string), ResManager.LoadKDString("物料{0}未勾选允许委外，订单类型不能为委外.", "015072000014405", 7, new object[0]), arg7));
								e.Cancel = true;
								return;
							}
							DynamicObjectCollection dynamicObjectCollection = (DynamicObjectCollection)dynamicObject["MaterialSubcon"];
							if (dynamicObjectCollection.Count > 0)
							{
								text = DataEntityExtend.GetDynamicValue<string>(dynamicObjectCollection[0], "SUBBILLTYPE_Id", null);
							}
							if (ObjectUtils.IsNullOrEmptyOrWhiteSpace(text))
							{
								text = this.DefaultSubBillType;
							}
							this.View.Model.SetValue("FMOBillType", text, e.Row);
							return;
						}
						else
						{
							this.View.Model.SetValue("FMOBillType", "", e.Row);
						}
					}
				}
				else
				{
					string[] array = Convert.ToString(e.Value).Split(new char[]
					{
						';'
					});
					if (array != null && array.Length > 1)
					{
						e.Cancel = true;
						this.View.Model.SetValue("FQkParentMaterialId", array[0]);
						return;
					}
				}
			}
		}

		// Token: 0x06000C4D RID: 3149 RVA: 0x0008EF64 File Offset: 0x0008D164
		private void DataChanged_ForMo(DataChangedEventArgs e)
		{
			if (!this.IsParentMoView)
			{
				return;
			}
			string key;
			if ((key = e.Field.Key) != null)
			{
				if (!(key == "FPrdOrgId"))
				{
					if (key == "FMOBillType")
					{
						this.DataChanged_ForMOBillType(e);
						return;
					}
					if (!(key == "FEnTrustOrgId"))
					{
						return;
					}
					this.View.Model.SetValue("FOWNERTYPEID", "BD_OwnerOrg", e.Row);
					this.View.Model.SetValue("FOWNERID", e.NewValue, e.Row);
				}
				else
				{
					this.DataChanged_ForMoPrdOrgId(e);
					EntryEntity entryEntity = this.Model.BillBusinessInfo.GetEntryEntity("FBottomEntity");
					DynamicObjectCollection entityDataObject = this.View.Model.GetEntityDataObject(entryEntity);
					if (ListUtils.IsEmpty<DynamicObject>(entityDataObject))
					{
						return;
					}
					DynamicObject dynamicObject = entityDataObject[e.Row];
					if (ObjectUtils.IsNullOrEmptyOrWhiteSpace(DataEntityExtend.GetDynamicValue<string>(dynamicObject, "MOBILLTYPE_Id", null)))
					{
						this.View.Model.SetValue("FOWNERTYPEID", null, e.Row);
						this.View.Model.SetValue("FOWNERID", null, e.Row);
						return;
					}
					string dynamicValue = DataEntityExtend.GetDynamicValue<string>(dynamicObject, "MOBILLTYPE_Id", null);
					DynamicObject dynamicObject2 = BusinessDataServiceHelper.LoadBillTypePara(this.View.Context, "MoBillTypeParaSetting", dynamicValue, true);
					bool dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<bool>(dynamicObject2, "IsEntrust", false);
					if (dynamicObjectItemValue)
					{
						return;
					}
					this.View.Model.SetValue("FOWNERTYPEID", "BD_OwnerOrg", e.Row);
					this.View.Model.SetValue("FOWNERID", e.NewValue, e.Row);
					return;
				}
			}
		}

		// Token: 0x06000C4E RID: 3150 RVA: 0x0008F544 File Offset: 0x0008D744
		private void RegisterEntityRule_ForPrdMo()
		{
			if (!this.IsParentMoView)
			{
				return;
			}
			this.View.RuleContainer.AddPluginRule("FBottomEntity", 2, new Action<DynamicObject, object>(this.InitMOBillType), new string[0]);
			this.View.RuleContainer.AddPluginRule("FBottomEntity", 2, new Action<DynamicObject, object>(this.InitSubBillType), new string[0]);
			this.View.RuleContainer.AddPluginRule("FBottomEntity", 7, new Action<DynamicObject, object>(this.LockCheckBox_ForPrdMo), new string[]
			{
				"FMaterialId2",
				"FGenerateType"
			});
			if (this.MemBomQueryOption != null)
			{
				this.MemBomQueryOption.IsShowOutSource = false;
			}
			this.View.RuleContainer.AddPluginRule("FBillHead", 3, delegate(DynamicObject dataEntity, dynamic dymc)
			{
				if (ProductStructureView.<RegisterEntityRule_ForPrdMo>o__SiteContainer8f.<>p__Site90 == null)
				{
					ProductStructureView.<RegisterEntityRule_ForPrdMo>o__SiteContainer8f.<>p__Site90 = CallSite<Action<CallSite, IStyleManager, string, object, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.ResultDiscarded, "SetEnabled", null, typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.Constant, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
					}));
				}
				Action<CallSite, IStyleManager, string, object, object> target = ProductStructureView.<RegisterEntityRule_ForPrdMo>o__SiteContainer8f.<>p__Site90.Target;
				CallSite <>p__Site = ProductStructureView.<RegisterEntityRule_ForPrdMo>o__SiteContainer8f.<>p__Site90;
				IStyleManager styleManager = this.View.StyleManager;
				string arg = "FIsExpandVirtualMtrl";
				object arg2 = null;
				if (ProductStructureView.<RegisterEntityRule_ForPrdMo>o__SiteContainer8f.<>p__Site91 == null)
				{
					ProductStructureView.<RegisterEntityRule_ForPrdMo>o__SiteContainer8f.<>p__Site91 = CallSite<Func<CallSite, object, object>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.Not, typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
					}));
				}
				Func<CallSite, object, object> target2 = ProductStructureView.<RegisterEntityRule_ForPrdMo>o__SiteContainer8f.<>p__Site91.Target;
				CallSite <>p__Site2 = ProductStructureView.<RegisterEntityRule_ForPrdMo>o__SiteContainer8f.<>p__Site91;
				if (ProductStructureView.<RegisterEntityRule_ForPrdMo>o__SiteContainer8f.<>p__Site92 == null)
				{
					ProductStructureView.<RegisterEntityRule_ForPrdMo>o__SiteContainer8f.<>p__Site92 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "IsShowPurchaseMtrl", typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
					}));
				}
				Func<CallSite, object, object> target3 = ProductStructureView.<RegisterEntityRule_ForPrdMo>o__SiteContainer8f.<>p__Site92.Target;
				CallSite <>p__Site3 = ProductStructureView.<RegisterEntityRule_ForPrdMo>o__SiteContainer8f.<>p__Site92;
				if (ProductStructureView.<RegisterEntityRule_ForPrdMo>o__SiteContainer8f.<>p__Site93 == null)
				{
					ProductStructureView.<RegisterEntityRule_ForPrdMo>o__SiteContainer8f.<>p__Site93 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "ActiveObject", typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
					}));
				}
				target(<>p__Site, styleManager, arg, arg2, target2(<>p__Site2, target3(<>p__Site3, ProductStructureView.<RegisterEntityRule_ForPrdMo>o__SiteContainer8f.<>p__Site93.Target(ProductStructureView.<RegisterEntityRule_ForPrdMo>o__SiteContainer8f.<>p__Site93, dymc))));
				if (ProductStructureView.<RegisterEntityRule_ForPrdMo>o__SiteContainer8f.<>p__Site94 == null)
				{
					ProductStructureView.<RegisterEntityRule_ForPrdMo>o__SiteContainer8f.<>p__Site94 = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsTrue, typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
					}));
				}
				Func<CallSite, object, bool> target4 = ProductStructureView.<RegisterEntityRule_ForPrdMo>o__SiteContainer8f.<>p__Site94.Target;
				CallSite <>p__Site4 = ProductStructureView.<RegisterEntityRule_ForPrdMo>o__SiteContainer8f.<>p__Site94;
				if (ProductStructureView.<RegisterEntityRule_ForPrdMo>o__SiteContainer8f.<>p__Site95 == null)
				{
					ProductStructureView.<RegisterEntityRule_ForPrdMo>o__SiteContainer8f.<>p__Site95 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "IsShowPurchaseMtrl", typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
					}));
				}
				Func<CallSite, object, object> target5 = ProductStructureView.<RegisterEntityRule_ForPrdMo>o__SiteContainer8f.<>p__Site95.Target;
				CallSite <>p__Site5 = ProductStructureView.<RegisterEntityRule_ForPrdMo>o__SiteContainer8f.<>p__Site95;
				if (ProductStructureView.<RegisterEntityRule_ForPrdMo>o__SiteContainer8f.<>p__Site96 == null)
				{
					ProductStructureView.<RegisterEntityRule_ForPrdMo>o__SiteContainer8f.<>p__Site96 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "ActiveObject", typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
					}));
				}
				if (target4(<>p__Site4, target5(<>p__Site5, ProductStructureView.<RegisterEntityRule_ForPrdMo>o__SiteContainer8f.<>p__Site96.Target(ProductStructureView.<RegisterEntityRule_ForPrdMo>o__SiteContainer8f.<>p__Site96, dymc))))
				{
					if (ProductStructureView.<RegisterEntityRule_ForPrdMo>o__SiteContainer8f.<>p__Site97 == null)
					{
						ProductStructureView.<RegisterEntityRule_ForPrdMo>o__SiteContainer8f.<>p__Site97 = CallSite<Func<CallSite, object, bool, object>>.Create(Binder.SetMember(CSharpBinderFlags.None, "IsExpandVirtualMtrl", typeof(ProductStructureView), new CSharpArgumentInfo[]
						{
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null)
						}));
					}
					Func<CallSite, object, bool, object> target6 = ProductStructureView.<RegisterEntityRule_ForPrdMo>o__SiteContainer8f.<>p__Site97.Target;
					CallSite <>p__Site6 = ProductStructureView.<RegisterEntityRule_ForPrdMo>o__SiteContainer8f.<>p__Site97;
					if (ProductStructureView.<RegisterEntityRule_ForPrdMo>o__SiteContainer8f.<>p__Site98 == null)
					{
						ProductStructureView.<RegisterEntityRule_ForPrdMo>o__SiteContainer8f.<>p__Site98 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "ActiveObject", typeof(ProductStructureView), new CSharpArgumentInfo[]
						{
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
						}));
					}
					target6(<>p__Site6, ProductStructureView.<RegisterEntityRule_ForPrdMo>o__SiteContainer8f.<>p__Site98.Target(ProductStructureView.<RegisterEntityRule_ForPrdMo>o__SiteContainer8f.<>p__Site98, dymc), true);
					this.View.Model.DataObject["IsExpandVirtualMtrl"] = true;
					this.View.UpdateView("FIsExpandVirtualMtrl");
				}
				if (this.MemBomQueryOption != null)
				{
					MemBomExpandOption_ForPSV memBomQueryOption = this.MemBomQueryOption;
					if (ProductStructureView.<RegisterEntityRule_ForPrdMo>o__SiteContainer8f.<>p__Site99 == null)
					{
						ProductStructureView.<RegisterEntityRule_ForPrdMo>o__SiteContainer8f.<>p__Site99 = CallSite<Func<CallSite, object, bool>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(bool), typeof(ProductStructureView)));
					}
					Func<CallSite, object, bool> target7 = ProductStructureView.<RegisterEntityRule_ForPrdMo>o__SiteContainer8f.<>p__Site99.Target;
					CallSite <>p__Site7 = ProductStructureView.<RegisterEntityRule_ForPrdMo>o__SiteContainer8f.<>p__Site99;
					if (ProductStructureView.<RegisterEntityRule_ForPrdMo>o__SiteContainer8f.<>p__Site9a == null)
					{
						ProductStructureView.<RegisterEntityRule_ForPrdMo>o__SiteContainer8f.<>p__Site9a = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "IsShowPurchaseMtrl", typeof(ProductStructureView), new CSharpArgumentInfo[]
						{
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
						}));
					}
					Func<CallSite, object, object> target8 = ProductStructureView.<RegisterEntityRule_ForPrdMo>o__SiteContainer8f.<>p__Site9a.Target;
					CallSite <>p__Site9a = ProductStructureView.<RegisterEntityRule_ForPrdMo>o__SiteContainer8f.<>p__Site9a;
					if (ProductStructureView.<RegisterEntityRule_ForPrdMo>o__SiteContainer8f.<>p__Site9b == null)
					{
						ProductStructureView.<RegisterEntityRule_ForPrdMo>o__SiteContainer8f.<>p__Site9b = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "ActiveObject", typeof(ProductStructureView), new CSharpArgumentInfo[]
						{
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
						}));
					}
					memBomQueryOption.IsShowOutSource = target7(<>p__Site7, target8(<>p__Site9a, ProductStructureView.<RegisterEntityRule_ForPrdMo>o__SiteContainer8f.<>p__Site9b.Target(ProductStructureView.<RegisterEntityRule_ForPrdMo>o__SiteContainer8f.<>p__Site9b, dymc)));
				}
			}, new string[]
			{
				"FIsShowPurchaseMtrl"
			});
		}

		// Token: 0x06000C4F RID: 3151 RVA: 0x0008F630 File Offset: 0x0008D830
		private void LockCheckBox_ForPrdMo(DynamicObject dataEntity, dynamic row)
		{
			Field field = this.View.BusinessInfo.GetField("FIsSelect");
			if (ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Site9e == null)
			{
				ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Site9e = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsTrue, typeof(ProductStructureView), new CSharpArgumentInfo[]
				{
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
				}));
			}
			Func<CallSite, object, bool> target = ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Site9e.Target;
			CallSite <>p__Site9e = ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Site9e;
			if (ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Site9f == null)
			{
				ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Site9f = CallSite<Func<CallSite, object, object, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.NotEqual, typeof(ProductStructureView), new CSharpArgumentInfo[]
				{
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.Constant, null)
				}));
			}
			Func<CallSite, object, object, object> target2 = ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Site9f.Target;
			CallSite <>p__Site9f = ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Site9f;
			if (ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitea0 == null)
			{
				ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitea0 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "MaterialId", typeof(ProductStructureView), new CSharpArgumentInfo[]
				{
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
				}));
			}
			Func<CallSite, object, object> target3 = ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitea0.Target;
			CallSite <>p__Sitea = ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitea0;
			if (ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitea1 == null)
			{
				ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitea1 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "ActiveObject", typeof(ProductStructureView), new CSharpArgumentInfo[]
				{
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
				}));
			}
			object obj = target2(<>p__Site9f, target3(<>p__Sitea, ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitea1.Target(ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitea1, row)), null);
			if (ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitea2 == null)
			{
				ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitea2 = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsFalse, typeof(ProductStructureView), new CSharpArgumentInfo[]
				{
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
				}));
			}
			object arg10;
			if (!ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitea2.Target(ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitea2, obj))
			{
				if (ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitea3 == null)
				{
					ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitea3 = CallSite<Func<CallSite, object, object, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.BinaryOperationLogical, ExpressionType.And, typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
					}));
				}
				Func<CallSite, object, object, object> target4 = ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitea3.Target;
				CallSite <>p__Sitea2 = ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitea3;
				object arg = obj;
				if (ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitea4 == null)
				{
					ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitea4 = CallSite<Func<CallSite, object, string, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.NotEqual, typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null)
					}));
				}
				Func<CallSite, object, string, object> target5 = ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitea4.Target;
				CallSite <>p__Sitea3 = ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitea4;
				if (ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitea5 == null)
				{
					ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitea5 = CallSite<Func<CallSite, Type, object, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.None, "ToString", null, typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.IsStaticType, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
					}));
				}
				Func<CallSite, Type, object, object> target6 = ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitea5.Target;
				CallSite <>p__Sitea4 = ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitea5;
				Type typeFromHandle = typeof(Convert);
				if (ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitea6 == null)
				{
					ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitea6 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "ForbidStatus", typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
					}));
				}
				Func<CallSite, object, object> target7 = ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitea6.Target;
				CallSite <>p__Sitea5 = ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitea6;
				if (ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitea7 == null)
				{
					ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitea7 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "MaterialId", typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
					}));
				}
				Func<CallSite, object, object> target8 = ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitea7.Target;
				CallSite <>p__Sitea6 = ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitea7;
				if (ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitea8 == null)
				{
					ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitea8 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "ActiveObject", typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
					}));
				}
				object obj2 = target5(<>p__Sitea3, target6(<>p__Sitea4, typeFromHandle, target7(<>p__Sitea5, target8(<>p__Sitea6, ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitea8.Target(ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitea8, row)))), "A");
				if (ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitea9 == null)
				{
					ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitea9 = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsTrue, typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
					}));
				}
				object obj3;
				if (!ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitea9.Target(ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitea9, obj2))
				{
					if (ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Siteaa == null)
					{
						ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Siteaa = CallSite<Func<CallSite, object, object, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.BinaryOperationLogical, ExpressionType.Or, typeof(ProductStructureView), new CSharpArgumentInfo[]
						{
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
						}));
					}
					Func<CallSite, object, object, object> target9 = ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Siteaa.Target;
					CallSite <>p__Siteaa = ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Siteaa;
					object arg2 = obj2;
					if (ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Siteab == null)
					{
						ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Siteab = CallSite<Func<CallSite, object, string, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.NotEqual, typeof(ProductStructureView), new CSharpArgumentInfo[]
						{
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null)
						}));
					}
					Func<CallSite, object, string, object> target10 = ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Siteab.Target;
					CallSite <>p__Siteab = ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Siteab;
					if (ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Siteac == null)
					{
						ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Siteac = CallSite<Func<CallSite, Type, object, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.None, "ToString", null, typeof(ProductStructureView), new CSharpArgumentInfo[]
						{
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.IsStaticType, null),
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
						}));
					}
					Func<CallSite, Type, object, object> target11 = ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Siteac.Target;
					CallSite <>p__Siteac = ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Siteac;
					Type typeFromHandle2 = typeof(Convert);
					if (ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitead == null)
					{
						ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitead = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "DocumentStatus", typeof(ProductStructureView), new CSharpArgumentInfo[]
						{
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
						}));
					}
					Func<CallSite, object, object> target12 = ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitead.Target;
					CallSite <>p__Sitead = ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitead;
					if (ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Siteae == null)
					{
						ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Siteae = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "MaterialId", typeof(ProductStructureView), new CSharpArgumentInfo[]
						{
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
						}));
					}
					Func<CallSite, object, object> target13 = ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Siteae.Target;
					CallSite <>p__Siteae = ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Siteae;
					if (ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Siteaf == null)
					{
						ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Siteaf = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "ActiveObject", typeof(ProductStructureView), new CSharpArgumentInfo[]
						{
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
						}));
					}
					obj3 = target9(<>p__Siteaa, arg2, target10(<>p__Siteab, target11(<>p__Siteac, typeFromHandle2, target12(<>p__Sitead, target13(<>p__Siteae, ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Siteaf.Target(ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Siteaf, row)))), "C"));
				}
				else
				{
					obj3 = obj2;
				}
				object obj4 = obj3;
				if (ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Siteb0 == null)
				{
					ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Siteb0 = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsTrue, typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
					}));
				}
				object obj6;
				if (!ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Siteb0.Target(ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Siteb0, obj4))
				{
					if (ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Siteb1 == null)
					{
						ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Siteb1 = CallSite<Func<CallSite, object, object, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.BinaryOperationLogical, ExpressionType.Or, typeof(ProductStructureView), new CSharpArgumentInfo[]
						{
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
						}));
					}
					Func<CallSite, object, object, object> target14 = ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Siteb1.Target;
					CallSite <>p__Siteb = ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Siteb1;
					object arg3 = obj4;
					if (ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Siteb2 == null)
					{
						ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Siteb2 = CallSite<Func<CallSite, object, object>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.Not, typeof(ProductStructureView), new CSharpArgumentInfo[]
						{
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
						}));
					}
					Func<CallSite, object, object> target15 = ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Siteb2.Target;
					CallSite <>p__Siteb2 = ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Siteb2;
					if (ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Siteb3 == null)
					{
						ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Siteb3 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "IsProduce", typeof(ProductStructureView), new CSharpArgumentInfo[]
						{
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
						}));
					}
					Func<CallSite, object, object> target16 = ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Siteb3.Target;
					CallSite <>p__Siteb3 = ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Siteb3;
					if (ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Siteb4 == null)
					{
						ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Siteb4 = CallSite<Func<CallSite, object, int, object>>.Create(Binder.GetIndex(CSharpBinderFlags.None, typeof(ProductStructureView), new CSharpArgumentInfo[]
						{
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null)
						}));
					}
					Func<CallSite, object, int, object> target17 = ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Siteb4.Target;
					CallSite <>p__Siteb4 = ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Siteb4;
					if (ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Siteb5 == null)
					{
						ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Siteb5 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.ResultIndexed, "MaterialBase", typeof(ProductStructureView), new CSharpArgumentInfo[]
						{
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
						}));
					}
					Func<CallSite, object, object> target18 = ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Siteb5.Target;
					CallSite <>p__Siteb5 = ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Siteb5;
					if (ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Siteb6 == null)
					{
						ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Siteb6 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "MaterialId", typeof(ProductStructureView), new CSharpArgumentInfo[]
						{
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
						}));
					}
					Func<CallSite, object, object> target19 = ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Siteb6.Target;
					CallSite <>p__Siteb6 = ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Siteb6;
					if (ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Siteb7 == null)
					{
						ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Siteb7 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "ActiveObject", typeof(ProductStructureView), new CSharpArgumentInfo[]
						{
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
						}));
					}
					object obj5 = target15(<>p__Siteb2, target16(<>p__Siteb3, target17(<>p__Siteb4, target18(<>p__Siteb5, target19(<>p__Siteb6, ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Siteb7.Target(ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Siteb7, row))), 0)));
					if (ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Siteb8 == null)
					{
						ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Siteb8 = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsFalse, typeof(ProductStructureView), new CSharpArgumentInfo[]
						{
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
						}));
					}
					object arg5;
					if (!ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Siteb8.Target(ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Siteb8, obj5))
					{
						if (ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Siteb9 == null)
						{
							ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Siteb9 = CallSite<Func<CallSite, object, object, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.BinaryOperationLogical, ExpressionType.And, typeof(ProductStructureView), new CSharpArgumentInfo[]
							{
								CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
								CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
							}));
						}
						Func<CallSite, object, object, object> target20 = ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Siteb9.Target;
						CallSite <>p__Siteb7 = ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Siteb9;
						object arg4 = obj5;
						if (ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Siteba == null)
						{
							ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Siteba = CallSite<Func<CallSite, object, object>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.Not, typeof(ProductStructureView), new CSharpArgumentInfo[]
							{
								CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
							}));
						}
						Func<CallSite, object, object> target21 = ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Siteba.Target;
						CallSite <>p__Siteba = ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Siteba;
						if (ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitebb == null)
						{
							ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitebb = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "IsSubContract", typeof(ProductStructureView), new CSharpArgumentInfo[]
							{
								CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
							}));
						}
						Func<CallSite, object, object> target22 = ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitebb.Target;
						CallSite <>p__Sitebb = ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitebb;
						if (ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitebc == null)
						{
							ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitebc = CallSite<Func<CallSite, object, int, object>>.Create(Binder.GetIndex(CSharpBinderFlags.None, typeof(ProductStructureView), new CSharpArgumentInfo[]
							{
								CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
								CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null)
							}));
						}
						Func<CallSite, object, int, object> target23 = ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitebc.Target;
						CallSite <>p__Sitebc = ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitebc;
						if (ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitebd == null)
						{
							ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitebd = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.ResultIndexed, "MaterialBase", typeof(ProductStructureView), new CSharpArgumentInfo[]
							{
								CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
							}));
						}
						Func<CallSite, object, object> target24 = ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitebd.Target;
						CallSite <>p__Sitebd = ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitebd;
						if (ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitebe == null)
						{
							ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitebe = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "MaterialId", typeof(ProductStructureView), new CSharpArgumentInfo[]
							{
								CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
							}));
						}
						Func<CallSite, object, object> target25 = ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitebe.Target;
						CallSite <>p__Sitebe = ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitebe;
						if (ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitebf == null)
						{
							ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitebf = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "ActiveObject", typeof(ProductStructureView), new CSharpArgumentInfo[]
							{
								CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
							}));
						}
						arg5 = target20(<>p__Siteb7, arg4, target21(<>p__Siteba, target22(<>p__Sitebb, target23(<>p__Sitebc, target24(<>p__Sitebd, target25(<>p__Sitebe, ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitebf.Target(ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitebf, row))), 0))));
					}
					else
					{
						arg5 = obj5;
					}
					obj6 = target14(<>p__Siteb, arg3, arg5);
				}
				else
				{
					obj6 = obj4;
				}
				object obj7 = obj6;
				if (ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitec0 == null)
				{
					ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitec0 = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsTrue, typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
					}));
				}
				object arg9;
				if (!ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitec0.Target(ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitec0, obj7))
				{
					if (ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitec1 == null)
					{
						ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitec1 = CallSite<Func<CallSite, object, object, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.BinaryOperationLogical, ExpressionType.Or, typeof(ProductStructureView), new CSharpArgumentInfo[]
						{
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
						}));
					}
					Func<CallSite, object, object, object> target26 = ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitec1.Target;
					CallSite <>p__Sitec = ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitec1;
					object arg6 = obj7;
					if (ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitec2 == null)
					{
						ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitec2 = CallSite<Func<CallSite, object, string, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.NotEqual, typeof(ProductStructureView), new CSharpArgumentInfo[]
						{
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null)
						}));
					}
					Func<CallSite, object, string, object> target27 = ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitec2.Target;
					CallSite <>p__Sitec2 = ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitec2;
					if (ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitec3 == null)
					{
						ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitec3 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "GenerateType", typeof(ProductStructureView), new CSharpArgumentInfo[]
						{
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
						}));
					}
					Func<CallSite, object, object> target28 = ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitec3.Target;
					CallSite <>p__Sitec3 = ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitec3;
					if (ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitec4 == null)
					{
						ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitec4 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "ActiveObject", typeof(ProductStructureView), new CSharpArgumentInfo[]
						{
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
						}));
					}
					object obj8 = target27(<>p__Sitec2, target28(<>p__Sitec3, ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitec4.Target(ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitec4, row)), "0");
					if (ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitec5 == null)
					{
						ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitec5 = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsFalse, typeof(ProductStructureView), new CSharpArgumentInfo[]
						{
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
						}));
					}
					object arg8;
					if (!ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitec5.Target(ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitec5, obj8))
					{
						if (ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitec6 == null)
						{
							ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitec6 = CallSite<Func<CallSite, object, object, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.BinaryOperationLogical, ExpressionType.And, typeof(ProductStructureView), new CSharpArgumentInfo[]
							{
								CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
								CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
							}));
						}
						Func<CallSite, object, object, object> target29 = ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitec6.Target;
						CallSite <>p__Sitec4 = ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitec6;
						object arg7 = obj8;
						if (ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitec7 == null)
						{
							ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitec7 = CallSite<Func<CallSite, object, string, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.NotEqual, typeof(ProductStructureView), new CSharpArgumentInfo[]
							{
								CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
								CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null)
							}));
						}
						Func<CallSite, object, string, object> target30 = ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitec7.Target;
						CallSite <>p__Sitec5 = ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitec7;
						if (ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitec8 == null)
						{
							ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitec8 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "GenerateType", typeof(ProductStructureView), new CSharpArgumentInfo[]
							{
								CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
							}));
						}
						Func<CallSite, object, object> target31 = ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitec8.Target;
						CallSite <>p__Sitec6 = ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitec8;
						if (ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitec9 == null)
						{
							ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitec9 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "ActiveObject", typeof(ProductStructureView), new CSharpArgumentInfo[]
							{
								CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
							}));
						}
						arg8 = target29(<>p__Sitec4, arg7, target30(<>p__Sitec5, target31(<>p__Sitec6, ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitec9.Target(ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitec9, row)), "1"));
					}
					else
					{
						arg8 = obj8;
					}
					arg9 = target26(<>p__Sitec, arg6, arg8);
				}
				else
				{
					arg9 = obj7;
				}
				arg10 = target4(<>p__Sitea2, arg, arg9);
			}
			else
			{
				arg10 = obj;
			}
			if (target(<>p__Site9e, arg10))
			{
				this.Model.SetValue(field, dataEntity, false);
				if (ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Siteca == null)
				{
					ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Siteca = CallSite<Action<CallSite, IStyleManager, Field, object, string, bool>>.Create(Binder.InvokeMember(CSharpBinderFlags.ResultDiscarded, "SetEnabled", null, typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null)
					}));
				}
				Action<CallSite, IStyleManager, Field, object, string, bool> target32 = ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Siteca.Target;
				CallSite <>p__Siteca = ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Siteca;
				IStyleManager styleManager = this.View.StyleManager;
				Field arg11 = field;
				if (ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitecb == null)
				{
					ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitecb = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "ActiveObject", typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
					}));
				}
				target32(<>p__Siteca, styleManager, arg11, ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitecb.Target(ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitecb, row), field.PropertyName, false);
			}
			else
			{
				if (ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitecc == null)
				{
					ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitecc = CallSite<Action<CallSite, IStyleManager, Field, object, string, bool>>.Create(Binder.InvokeMember(CSharpBinderFlags.ResultDiscarded, "SetEnabled", null, typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null)
					}));
				}
				Action<CallSite, IStyleManager, Field, object, string, bool> target33 = ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitecc.Target;
				CallSite <>p__Sitecc = ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitecc;
				IStyleManager styleManager2 = this.View.StyleManager;
				Field arg12 = field;
				if (ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitecd == null)
				{
					ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitecd = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "ActiveObject", typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
					}));
				}
				target33(<>p__Sitecc, styleManager2, arg12, ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitecd.Target(ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitecd, row), field.PropertyName, true);
			}
			if (ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitece == null)
			{
				ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitece = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsTrue, typeof(ProductStructureView), new CSharpArgumentInfo[]
				{
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
				}));
			}
			Func<CallSite, object, bool> target34 = ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitece.Target;
			CallSite <>p__Sitece = ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitece;
			if (ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitecf == null)
			{
				ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitecf = CallSite<Func<CallSite, ProductStructureView, object, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.InvokeSimpleName, "IsNeedLock_ForMo", null, typeof(ProductStructureView), new CSharpArgumentInfo[]
				{
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
				}));
			}
			Func<CallSite, ProductStructureView, object, object> target35 = ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitecf.Target;
			CallSite <>p__Sitecf = ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sitecf;
			if (ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sited0 == null)
			{
				ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sited0 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "ActiveObject", typeof(ProductStructureView), new CSharpArgumentInfo[]
				{
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
				}));
			}
			if (target34(<>p__Sitece, target35(<>p__Sitecf, this, ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sited0.Target(ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sited0, row))))
			{
				if (ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sited1 == null)
				{
					ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sited1 = CallSite<Action<CallSite, IStyleManager, Field, object, string, bool>>.Create(Binder.InvokeMember(CSharpBinderFlags.ResultDiscarded, "SetEnabled", null, typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null)
					}));
				}
				Action<CallSite, IStyleManager, Field, object, string, bool> target36 = ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sited1.Target;
				CallSite <>p__Sited = ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sited1;
				IStyleManager styleManager3 = this.View.StyleManager;
				Field arg13 = field;
				if (ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sited2 == null)
				{
					ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sited2 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "ActiveObject", typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
					}));
				}
				target36(<>p__Sited, styleManager3, arg13, ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sited2.Target(ProductStructureView.<LockCheckBox_ForPrdMo>o__SiteContainer9d.<>p__Sited2, row), field.PropertyName, false);
			}
		}

		// Token: 0x06000C50 RID: 3152 RVA: 0x00090900 File Offset: 0x0008EB00
		private void ReturnDataToParentByMo(ListSelectedRow[] selRows, List<long> prdOrgIds, bool isReturnAndClose = false)
		{
			if (ListUtils.IsEmpty<ListSelectedRow>(selRows))
			{
				this.View.ShowMessage(ResManager.LoadKDString("选择数据行集合为空！", "015072000002228", 7, new object[0]), 0);
				return;
			}
			Dictionary<long, long> dictionary = new Dictionary<long, long>();
			DynamicObjectCollection source = (DynamicObjectCollection)this.View.Model.DataObject["BomChild"];
			List<long> permissionOrg = MFGServiceHelper.GetPermissionOrg(base.Context, new BusinessObject
			{
				Id = "PRD_MO"
			}, "fce8b1aca2144beeb3c6655eaf78bc34");
			List<long> permissionOrg2 = MFGServiceHelper.GetPermissionOrg(base.Context, new BusinessObject
			{
				Id = "SUB_SUBREQORDER"
			}, "fce8b1aca2144beeb3c6655eaf78bc34");
			List<long> list = new List<long>();
			List<long> list2 = new List<long>();
			StringBuilder stringBuilder = new StringBuilder();
			StringBuilder stringBuilder2 = new StringBuilder();
			StringBuilder stringBuilder3 = new StringBuilder();
			List<DynamicObject> list3 = new List<DynamicObject>();
			List<ListSelectedRow> list4 = new List<ListSelectedRow>();
			List<ListSelectedRow> list5 = new List<ListSelectedRow>();
			prdOrgIds = new List<long>();
			List<long> list6 = new List<long>();
			FormMetadata formMetadata = MetaDataServiceHelper.Load(base.Context, "BD_MATERIAL", true) as FormMetadata;
			List<string> lstBillTypeId = (from p in source
			where StringUtils.EqualsIgnoreCase("0", DataEntityExtend.GetDynamicValue<string>(p, "GenerateType", null)) && !ObjectUtils.IsNullOrEmpty(DataEntityExtend.GetDynamicValue<string>(p, "MOBILLTYPE_Id", null))
			select p into d
			select DataEntityExtend.GetDynamicValue<string>(d, "MOBILLTYPE_Id", null)).Distinct<string>().ToList<string>();
			Dictionary<string, DynamicObject> dictionary2 = ProductStructureView.CreateMoBillTypeDicCache(base.Context, lstBillTypeId);
			this.lstErrorBomItems.Clear();
			for (int i = 0; i < selRows.Length; i++)
			{
				ListSelectedRow selRow = selRows[i];
				DynamicObject dynamicObject = source.FirstOrDefault((DynamicObject w) => DataEntityExtend.GetDynamicObjectItemValue<string>(w, "EntryId", null) == selRow.EntryPrimaryKeyValue);
				if (dynamicObject != null)
				{
					int dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<int>(dynamicObject, "GenerateType", 0);
					DynamicObject dynamicObjectItemValue2 = DataEntityExtend.GetDynamicObjectItemValue<DynamicObject>(dynamicObject, "PrdOrgId", null);
					if (ObjectUtils.IsNullOrEmpty(DataEntityExtend.GetDynamicValue<string>(dynamicObject, "MOBILLTYPE_Id", null)))
					{
						this.lstErrorBomItems.Add(DataEntityExtend.GetDynamicValue<DynamicObject>(dynamicObject, "MaterialId", null));
					}
					else if (dynamicObjectItemValue == 0)
					{
						DynamicObject dynamicObject2 = null;
						string dynamicValue = DataEntityExtend.GetDynamicValue<string>(dynamicObject, "MOBILLTYPE_Id", null);
						dictionary2.TryGetValue(dynamicValue, out dynamicObject2);
						bool dynamicValue2 = DataEntityExtend.GetDynamicValue<bool>(dynamicObject2, "IsEntrust", false);
						if (dynamicValue2)
						{
							long dynamicValue3 = DataEntityExtend.GetDynamicValue<long>(dynamicObject, "EnTrustOrgId_Id", 0L);
							long dynamicValue4 = DataEntityExtend.GetDynamicValue<long>(dynamicObject, "PrdOrgId_Id", 0L);
							if (dynamicValue3 == dynamicValue4)
							{
								list3.Add(DataEntityExtend.GetDynamicValue<DynamicObject>(dynamicObject, "MaterialId", null));
								string dynamicValue5 = DataEntityExtend.GetDynamicValue<string>(DataEntityExtend.GetDynamicValue<DynamicObject>(dynamicObject, "MaterialId", null), "Number", null);
								stringBuilder3.AppendFormat(ResManager.LoadKDString("物料{0},组织间委托类生产订单的生产/委外组织与委托组织不允许相同!", "015072000040024", 7, new object[0]), dynamicValue5);
								goto IL_614;
							}
							if (dynamicValue3 <= 0L)
							{
								list3.Add(DataEntityExtend.GetDynamicValue<DynamicObject>(dynamicObject, "MaterialId", null));
								string dynamicValue6 = DataEntityExtend.GetDynamicValue<string>(DataEntityExtend.GetDynamicValue<DynamicObject>(dynamicObject, "MaterialId", null), "Number", null);
								stringBuilder3.AppendFormat(ResManager.LoadKDString("物料{0},组织间委托类生产订单的委托组织不允许为空!", "015072000040025", 7, new object[0]), dynamicValue6);
								goto IL_614;
							}
							long dynamicValue7 = DataEntityExtend.GetDynamicValue<long>(dynamicObject, "OWNERID_Id", 0L);
							if (dynamicValue7 <= 0L)
							{
								list3.Add(DataEntityExtend.GetDynamicValue<DynamicObject>(dynamicObject, "MaterialId", null));
								string dynamicValue8 = DataEntityExtend.GetDynamicValue<string>(DataEntityExtend.GetDynamicValue<DynamicObject>(dynamicObject, "MaterialId", null), "Number", null);
								stringBuilder3.AppendFormat(ResManager.LoadKDString("物料{0},组织间委托类生产订单的货主不允许为空!", "015072000012094", 7, new object[0]), dynamicValue8);
								goto IL_614;
							}
						}
					}
					long dynamicObjectItemValue3 = DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObjectItemValue2, "Id", 0L);
					if (dynamicObjectItemValue == 0 && !permissionOrg.Contains(dynamicObjectItemValue3))
					{
						if (!list.Contains(dynamicObjectItemValue3))
						{
							list.Add(dynamicObjectItemValue3);
							string empty = string.Empty;
							LocaleValue dynamicObjectItemValue4 = DataEntityExtend.GetDynamicObjectItemValue<LocaleValue>(dynamicObjectItemValue2, "Name", null);
							dynamicObjectItemValue4.TryGetValue(base.Context.UserLocale.LCID, ref empty);
							stringBuilder.AppendLine(string.Format(ResManager.LoadKDString("当前用户在[{0}]组织下无生产订单新增权限！", "015072000002229", 7, new object[0]), empty));
						}
					}
					else if (dynamicObjectItemValue == 1 && !permissionOrg2.Contains(dynamicObjectItemValue3))
					{
						if (!list2.Contains(dynamicObjectItemValue3))
						{
							list2.Add(dynamicObjectItemValue3);
							string empty2 = string.Empty;
							LocaleValue dynamicObjectItemValue5 = DataEntityExtend.GetDynamicObjectItemValue<LocaleValue>(dynamicObjectItemValue2, "Name", null);
							dynamicObjectItemValue5.TryGetValue(base.Context.UserLocale.LCID, ref empty2);
							stringBuilder2.AppendLine(string.Format(ResManager.LoadKDString("当前用户在[{0}]组织下无委外订单新增权限！", "015072000014406", 7, new object[0]), empty2));
						}
					}
					else
					{
						DynamicObject dynamicObjectItemValue6 = DataEntityExtend.GetDynamicObjectItemValue<DynamicObject>(dynamicObject, "MaterialId", null);
						long dynamicObjectItemValue7 = DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObjectItemValue6, "MsterId", 0L);
						object baseDataPkId = BaseDataServiceHelper.GetBaseDataPkId(base.Context, formMetadata.BusinessInfo, "BD_MATERIAL", dynamicObjectItemValue7, dynamicObjectItemValue3);
						long num = 0L;
						if (baseDataPkId != null && baseDataPkId is DynamicObject)
						{
							DynamicObject dynamicObject3 = baseDataPkId as DynamicObject;
							num = (long)Convert.ToInt32(dynamicObject3["Id"]);
						}
						if (num == 0L)
						{
							string empty3 = string.Empty;
							LocaleValue dynamicObjectItemValue8 = DataEntityExtend.GetDynamicObjectItemValue<LocaleValue>(dynamicObjectItemValue2, "Name", null);
							dynamicObjectItemValue8.TryGetValue(base.Context.UserLocale.LCID, ref empty3);
							stringBuilder.AppendLine(string.Format(ResManager.LoadKDString("物料{0}在{1}组织下不存在!", "015072000012583", 7, new object[0]), DataEntityExtend.GetDynamicObjectItemValue<string>(dynamicObjectItemValue6, "Number", null), empty3));
						}
						else
						{
							if (num != (long)Convert.ToInt32(dynamicObjectItemValue6["Id"]))
							{
								DynamicObject dynamicObject3 = BusinessDataServiceHelper.LoadSingle(base.Context, num, dynamicObjectItemValue6.DynamicObjectType, null);
								DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "MaterialId", dynamicObject3);
								DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "MaterialId_Id", num);
							}
							dictionary.Add(DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObject, "Id", 0L), dynamicObjectItemValue3);
							if (dynamicObjectItemValue == 0)
							{
								if (!prdOrgIds.Contains(dynamicObjectItemValue3))
								{
									prdOrgIds.Add(dynamicObjectItemValue3);
								}
								list4.Add(selRow);
							}
							else if (dynamicObjectItemValue == 1)
							{
								if (!list6.Contains(dynamicObjectItemValue3))
								{
									list6.Add(dynamicObjectItemValue3);
								}
								list5.Add(selRow);
							}
						}
					}
				}
				IL_614:;
			}
			StringBuilder stringBuilder4 = new StringBuilder();
			if (this.lstErrorBomItems.Count > 0)
			{
				this.View.ShowErrMessage(string.Format(ResManager.LoadKDString("{0}的单据类型必录", "015072000018959", 7, new object[0]), string.Join(";", from s in this.lstErrorBomItems
				select DataEntityExtend.GetDynamicValue<string>(s, "Number", null))), "", 0);
				return;
			}
			if (stringBuilder3.Length > 0)
			{
				this.lstErrorBomItems.AddRange(list3);
				this.View.ShowErrMessage(stringBuilder3.ToString(), "", 0);
				return;
			}
			if (list4.Count == 0)
			{
				stringBuilder4.Append(stringBuilder.ToString());
			}
			if (list5.Count == 0)
			{
				stringBuilder4.Append(stringBuilder2.ToString());
			}
			if (stringBuilder4.Length > 0)
			{
				this.View.ParentFormView.ShowErrMessage(stringBuilder4.ToString(), "", 0);
				this.View.SendDynamicFormAction(this.View.ParentFormView);
			}
			if (dictionary.Count < 1)
			{
				return;
			}
			ProductStructureViewServiceHelper.UpdateSupplyOrgId(this.View.Context, dictionary);
			this.DoPush("ENG_BomExpandBill", "PRD_MO", list4.ToArray(), prdOrgIds);
			this.DoPush("ENG_BomExpandBill", "SUB_SUBREQORDER", list5.ToArray(), list6);
			if (isReturnAndClose)
			{
				this.View.Close();
			}
		}

		// Token: 0x06000C51 RID: 3153 RVA: 0x000910A0 File Offset: 0x0008F2A0
		private bool BeforeUpdateValue_ForMoPrdOrgId(BeforeUpdateValueEventArgs e)
		{
			bool flag = true;
			return ((e.Value is DynamicObject && e.Value is DynamicObject) || e.Value is long) && flag;
		}

		// Token: 0x06000C52 RID: 3154 RVA: 0x000910DC File Offset: 0x0008F2DC
		private void DataChanged_ForMoPrdOrgId(DataChangedEventArgs e)
		{
			long num = Convert.ToInt64(e.NewValue);
			long num2 = MFGBillUtil.GetValue<long>(this.View.Model, "FMaterialId2", e.Row, 0L, null);
			int value = MFGBillUtil.GetValue<int>(this.View.Model, "FBomLevel", e.Row, 0, null);
			if (e.Row == 0)
			{
				if (num <= 0L)
				{
					num = this.selBOMParamByMO.MainOrgId;
				}
				this.SetHeadMtrlBom(num, this.HeadMtrlId);
			}
			else
			{
				this.DelChildItems(value, e.Row);
			}
			if (num <= 0L)
			{
				num = this.GetParentPrdOrgId(e.Row);
			}
			DynamicObjectCollection mtrlInfoByPrdOrgId = this.GetMtrlInfoByPrdOrgId(num, num2);
			if (mtrlInfoByPrdOrgId != null && mtrlInfoByPrdOrgId.Count > 0)
			{
				DynamicObject dynamicObject = mtrlInfoByPrdOrgId.FirstOrDefault<DynamicObject>();
				num2 = DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObject, "FMaterialId", 0L);
				this.View.Model.SetValue("FMaterialId2", num2, e.Row);
				long defaultBomKey = BOMServiceHelper.GetDefaultBomKey(this.View.Context, DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObject, "FMasterId", 0L), num, 0L, 2);
				string dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<string>(dynamicObject, "FErpClsID", null);
				if (dynamicObjectItemValue == "1")
				{
					this.View.Model.SetValue("FGenerateType", "", e.Row);
				}
				if (defaultBomKey <= 0L)
				{
					this.View.Model.SetValue("FBomId2", 0, e.Row);
					return;
				}
				this.View.Model.SetValue("FBomId2", defaultBomKey, e.Row);
				this.SetNewChildItems(value, num, dynamicObject, e.Row);
				this.SetChildPrdOrgId(value, num, e.Row, false);
				this.SetChildEntrustEnable(0, 0, true);
			}
		}

		// Token: 0x06000C53 RID: 3155 RVA: 0x000912A4 File Offset: 0x0008F4A4
		private void DataChanged_ForMOBillType(DataChangedEventArgs e)
		{
			string text = Convert.ToString(e.NewValue);
			DynamicObject dynamicObject = BusinessDataServiceHelper.LoadBillTypePara(this.View.Context, "MoBillTypeParaSetting", text, true);
			bool dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<bool>(dynamicObject, "IsEntrust", false);
			if (dynamicObjectItemValue)
			{
				this.View.GetFieldEditor("FEnTrustOrgId", e.Row).SetEnabled("", true);
				this.View.Model.SetValue("FOWNERTYPEID", null, e.Row);
				this.View.Model.SetValue("FOWNERID", null, e.Row);
				return;
			}
			this.View.Model.SetValue("FEnTrustOrgId", null, e.Row);
			this.View.GetFieldEditor("FEnTrustOrgId", e.Row).SetEnabled("", false);
			EntryEntity entryEntity = this.Model.BillBusinessInfo.GetEntryEntity("FBottomEntity");
			DynamicObjectCollection entityDataObject = this.View.Model.GetEntityDataObject(entryEntity);
			if (ListUtils.IsEmpty<DynamicObject>(entityDataObject))
			{
				return;
			}
			DynamicObject dynamicObject2 = entityDataObject[e.Row];
			this.View.Model.SetValue("FOWNERTYPEID", "BD_OwnerOrg", e.Row);
			this.View.Model.SetValue("FOWNERID", DataEntityExtend.GetDynamicValue<long>(dynamicObject2, "PrdOrgId_Id", 0L), e.Row);
		}

		// Token: 0x06000C54 RID: 3156 RVA: 0x0009140C File Offset: 0x0008F60C
		private void SetHeadMtrlBom(long prdOrgId, long mtrlId)
		{
			int entryRowCount = this.View.Model.GetEntryRowCount("FBottomEntity");
			for (int i = entryRowCount - 1; i > 0; i--)
			{
				this.View.Model.DeleteEntryRow("FBottomEntity", i);
			}
			DynamicObjectCollection mtrlInfoByPrdOrgId = this.GetMtrlInfoByPrdOrgId(prdOrgId, mtrlId);
			if (mtrlInfoByPrdOrgId != null && mtrlInfoByPrdOrgId.Count > 0)
			{
				DynamicObject dynamicObject = mtrlInfoByPrdOrgId.FirstOrDefault<DynamicObject>();
				mtrlId = DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObject, "FMaterialId", 0L);
				this.View.Model.SetValue("FQkParentMaterialName", DataEntityExtend.GetDynamicObjectItemValue<string>(dynamicObject, "FName", null));
				this.View.Model.SetValue("FQkParentMaterialSpec", DataEntityExtend.GetDynamicObjectItemValue<string>(dynamicObject, "FSpecification", null));
				long defaultBomKey = BOMServiceHelper.GetDefaultBomKey(this.View.Context, DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObject, "FMasterId", 0L), prdOrgId, 0L, 2);
				DynamicObject bomObjById = this.GetBomObjById(defaultBomKey);
				if (bomObjById != null)
				{
					this.selBOMParamByMO.MainOrgId = prdOrgId;
					this.View.Model.SetValue("FQkBomId", DataEntityExtend.GetDynamicObjectItemValue<string>(bomObjById, "Number", null));
					string empty = string.Empty;
					DataEntityExtend.GetDynamicObjectItemValue<LocaleValue>(bomObjById, "Name", null).TryGetValue(base.Context.UserLocale.LCID, ref empty);
					this.View.Model.SetValue("FQkBomName", empty);
					return;
				}
				this.View.Model.SetValue("FQkBomId", string.Empty);
			}
		}

		// Token: 0x06000C55 RID: 3157 RVA: 0x00091584 File Offset: 0x0008F784
		private void SetNewChildItems(int bomLevel, long prdOrgId, DynamicObject mtrlObj, int row)
		{
			this.UpdateBomQueryOption();
			List<DynamicObject> list = new List<DynamicObject>();
			long defaultBomKey = BOMServiceHelper.GetDefaultBomKey(this.View.Context, DataEntityExtend.GetDynamicObjectItemValue<long>(mtrlObj, "FMasterId", 0L), prdOrgId, 0L, 2);
			list.Add(this.GetBomObjById(defaultBomKey));
			if (list == null || list.Count<DynamicObject>() <= 0)
			{
				this.View.Model.SetValue("FBomId2", 0, row);
				return;
			}
			List<DynamicObject> list2 = this.PrepareBomOpenSourceData(list);
			if (list2 == null || list2.Count <= 0)
			{
				return;
			}
			this.View.Model.BeginIniti();
			List<DynamicObject> bomChildData = this.GetBomChildData(list2, this.MemBomQueryOption);
			EntryEntity entryEntity = this.Model.BillBusinessInfo.GetEntryEntity("FBottomEntity");
			if (bomChildData != null && bomChildData.Count > 0)
			{
				DynamicObjectCollection entityDataObject = this.Model.GetEntityDataObject(entryEntity);
				string dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<string>(bomChildData.First<DynamicObject>(), "EntryId", null);
				if (row == 0)
				{
					this.View.Model.SetValue("FEntryId", dynamicObjectItemValue, 0);
				}
				string value = MFGBillUtil.GetValue<string>(this.View.Model, "FEntryId", row, null, null);
				foreach (DynamicObject dynamicObject in bomChildData)
				{
					if (!(DataEntityExtend.GetDynamicObjectItemValue<string>(dynamicObject, "EntryId", null) == dynamicObjectItemValue))
					{
						if (DataEntityExtend.GetDynamicObjectItemValue<string>(dynamicObject, "ParentEntryId", null) == dynamicObjectItemValue)
						{
							DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "ParentEntryId", value);
						}
						int num = bomLevel + DataEntityExtend.GetDynamicObjectItemValue<int>(dynamicObject, "BomLevel", 0);
						DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "BomLevel", num);
						entityDataObject.Add(dynamicObject);
					}
				}
			}
			this.View.Model.EndIniti();
			this.View.UpdateView("FBottomEntity");
			this.View.SetEntityFocusRow("FBottomEntity", row);
		}

		// Token: 0x06000C56 RID: 3158 RVA: 0x000917C0 File Offset: 0x0008F9C0
		private void SetChildPrdOrgId(int bomLevel, long prdOrgId, int row, bool refreshAllChilds = true)
		{
			if (!this.IsParentMoView)
			{
				return;
			}
			prdOrgId = ((prdOrgId == 0L) ? this.selBOMParamByMO.MainOrgId : prdOrgId);
			EntryEntity entryEntity = this.Model.BillBusinessInfo.GetEntryEntity("FBottomEntity");
			DynamicObjectCollection entityDataObject = this.View.Model.GetEntityDataObject(entryEntity);
			List<DynamicObject> list = new List<DynamicObject>();
			if (refreshAllChilds)
			{
				list.AddRange((from w in entityDataObject
				where DataEntityExtend.GetDynamicObjectItemValue<int>(w, "BomLevel", 0) == bomLevel
				select w).ToList<DynamicObject>());
			}
			else
			{
				list.Add(entityDataObject[row]);
			}
			this.View.Model.BeginIniti();
			foreach (DynamicObject dynamicObject in list)
			{
				string dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<string>(dynamicObject, "EntryId", null);
				List<string> bomChItemEntryIds = this.GetChItemsByEntryId(bomLevel, dynamicObjectItemValue, entityDataObject);
				List<DynamicObject> list2 = (from w in entityDataObject
				where bomChItemEntryIds.Contains(DataEntityExtend.GetDynamicObjectItemValue<string>(w, "EntryId", null))
				select w).ToList<DynamicObject>();
				foreach (DynamicObject dynamicObject2 in list2)
				{
					int rowIndex = this.View.Model.GetRowIndex(entryEntity, dynamicObject2);
					this.View.Model.SetValue("FPrdOrgId", prdOrgId, rowIndex);
				}
				if (DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObject, "PrdOrgId_Id", (long)row) <= 0L)
				{
					int rowIndex2 = this.View.Model.GetRowIndex(entryEntity, dynamicObject);
					this.View.Model.SetValue("FPrdOrgId", prdOrgId, rowIndex2);
				}
			}
			this.View.Model.EndIniti();
			this.View.UpdateView("FBottomEntity");
		}

		// Token: 0x06000C57 RID: 3159 RVA: 0x00091A2C File Offset: 0x0008FC2C
		private void SetChildEntrustEnable(int bomLevel, int row, bool refreshAllChilds = true)
		{
			if (!this.IsParentMoView)
			{
				return;
			}
			EntryEntity entryEntity = this.Model.BillBusinessInfo.GetEntryEntity("FBottomEntity");
			DynamicObjectCollection entityDataObject = this.View.Model.GetEntityDataObject(entryEntity);
			List<DynamicObject> list = new List<DynamicObject>();
			if (refreshAllChilds)
			{
				list.AddRange((from w in entityDataObject
				where DataEntityExtend.GetDynamicObjectItemValue<int>(w, "BomLevel", 0) == bomLevel
				select w).ToList<DynamicObject>());
			}
			else
			{
				list.Add(entityDataObject[row]);
			}
			this.View.Model.BeginIniti();
			foreach (DynamicObject dynamicObject in list)
			{
				this.SetEntrustEnable(entryEntity, dynamicObject);
				string dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<string>(dynamicObject, "EntryId", null);
				List<string> bomChItemEntryIds = this.GetChItemsByEntryId(bomLevel, dynamicObjectItemValue, entityDataObject);
				List<DynamicObject> list2 = (from w in entityDataObject
				where bomChItemEntryIds.Contains(DataEntityExtend.GetDynamicObjectItemValue<string>(w, "EntryId", null))
				select w).ToList<DynamicObject>();
				foreach (DynamicObject bomChItem in list2)
				{
					this.SetEntrustEnable(entryEntity, bomChItem);
				}
			}
			this.View.Model.EndIniti();
		}

		// Token: 0x06000C58 RID: 3160 RVA: 0x00091BB4 File Offset: 0x0008FDB4
		private void SetEntrustEnable(EntryEntity bomEntity, DynamicObject bomChItem)
		{
			int rowIndex = this.View.Model.GetRowIndex(bomEntity, bomChItem);
			string dynamicValue = DataEntityExtend.GetDynamicValue<string>(bomChItem, "MOBILLTYPE_Id", null);
			DynamicObject dynamicObject = BusinessDataServiceHelper.LoadBillTypePara(this.View.Context, "MoBillTypeParaSetting", dynamicValue, true);
			bool dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<bool>(dynamicObject, "IsEntrust", false);
			if (dynamicObjectItemValue)
			{
				this.View.GetFieldEditor("FEnTrustOrgId", rowIndex).SetEnabled("", true);
				return;
			}
			this.View.GetFieldEditor("FEnTrustOrgId", rowIndex).SetEnabled("", false);
		}

		// Token: 0x06000C59 RID: 3161 RVA: 0x00091C74 File Offset: 0x0008FE74
		private void SetChildPrdOrgIdBySupplyOrgId()
		{
			if (!this.IsParentMoView)
			{
				return;
			}
			long mainOrgId = this.selBOMParamByMO.MainOrgId;
			EntryEntity entryEntity = this.Model.BillBusinessInfo.GetEntryEntity("FBottomEntity");
			DynamicObjectCollection entityDataObject = this.View.Model.GetEntityDataObject(entryEntity);
			List<DynamicObject> list = new List<DynamicObject>();
			list.AddRange((from w in entityDataObject
			where DataEntityExtend.GetDynamicObjectItemValue<int>(w, "BomLevel", 0) == 0
			select w).ToList<DynamicObject>());
			this.View.Model.BeginIniti();
			foreach (DynamicObject dynamicObject in list)
			{
				this.SetMOBillType(entryEntity, dynamicObject);
				this.SetMOOwnerId(entryEntity, dynamicObject);
				string dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<string>(dynamicObject, "EntryId", null);
				List<string> bomChItemEntryIds = this.GetChItemsByEntryId(0, dynamicObjectItemValue, entityDataObject);
				List<DynamicObject> list2 = (from w in entityDataObject
				where bomChItemEntryIds.Contains(DataEntityExtend.GetDynamicObjectItemValue<string>(w, "EntryId", null))
				select w).ToList<DynamicObject>();
				foreach (DynamicObject dynamicObject2 in list2)
				{
					this.SetMOBillType(entryEntity, dynamicObject2);
					this.SetMOOwnerId(entryEntity, dynamicObject);
					int rowIndex = this.View.Model.GetRowIndex(entryEntity, dynamicObject2);
					this.View.Model.SetValue("FPrdOrgId", MFGBillUtil.GetValue<long>(this.View.Model, "FSupplyOrgId", rowIndex, 0L, null), rowIndex);
				}
				if (DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObject, "PrdOrgId_Id", 0L) <= 0L)
				{
					int rowIndex2 = this.View.Model.GetRowIndex(entryEntity, dynamicObject);
					this.View.Model.SetValue("FPrdOrgId", mainOrgId, rowIndex2);
				}
			}
			this.View.Model.EndIniti();
			this.View.UpdateView("FBottomEntity");
		}

		// Token: 0x06000C5A RID: 3162 RVA: 0x00091EAC File Offset: 0x000900AC
		private void SetMOBillType(EntryEntity entryEntity, DynamicObject docRow)
		{
			DynamicObject dynamicObject = (DynamicObject)docRow["MaterialId"];
			string text = string.Empty;
			if (Convert.ToString(docRow["GenerateType"]) == "0")
			{
				DynamicObjectCollection dynamicObjectCollection = (DynamicObjectCollection)dynamicObject["MaterialProduce"];
				if (dynamicObjectCollection.Count > 0)
				{
					if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(DataEntityExtend.GetDynamicValue<string>(dynamicObjectCollection[0], "ProduceBillType_Id", null)))
					{
						text = DataEntityExtend.GetDynamicValue<string>(dynamicObjectCollection[0], "ProduceBillType_Id", null);
					}
					else if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(DataEntityExtend.GetDynamicValue<string>(dynamicObjectCollection[0], "OrgTrustBillType_Id", null)))
					{
						text = DataEntityExtend.GetDynamicValue<string>(dynamicObjectCollection[0], "OrgTrustBillType_Id", null);
					}
				}
				if (ObjectUtils.IsNullOrEmptyOrWhiteSpace(text))
				{
					text = this.DefaultMOBillType;
				}
			}
			else if (Convert.ToString(docRow["GenerateType"]) == "1")
			{
				DynamicObjectCollection dynamicObjectCollection = (DynamicObjectCollection)dynamicObject["MaterialSubcon"];
				if (dynamicObjectCollection.Count > 0)
				{
					text = DataEntityExtend.GetDynamicValue<string>(dynamicObjectCollection[0], "SUBBILLTYPE_Id", null);
				}
				if (ObjectUtils.IsNullOrEmptyOrWhiteSpace(text))
				{
					text = this.DefaultSubBillType;
				}
			}
			this.View.Model.SetValue("FMOBillType", text, this.View.Model.GetRowIndex(entryEntity, docRow));
		}

		// Token: 0x06000C5B RID: 3163 RVA: 0x00091FF4 File Offset: 0x000901F4
		private void SetMOOwnerId(EntryEntity entryEntity, DynamicObject docRow)
		{
			long dynamicValue = DataEntityExtend.GetDynamicValue<long>(docRow, "OWNERID_Id", 0L);
			if (dynamicValue > 0L)
			{
				return;
			}
			int rowIndex = this.View.Model.GetRowIndex(entryEntity, docRow);
			this.View.Model.SetValue("FOWNERTYPEID", this.selBOMParamByMO.SpMoEntryLst.First<SimpleMoEntry>().ownerTypeId, rowIndex);
			this.View.Model.SetValue("FOWNERID", this.selBOMParamByMO.SpMoEntryLst.First<SimpleMoEntry>().ownerId, rowIndex);
		}

		// Token: 0x06000C5C RID: 3164 RVA: 0x00092084 File Offset: 0x00090284
		private void DelChildItems(int bomLevel, int row)
		{
			int entryRowCount = this.View.Model.GetEntryRowCount("FBottomEntity");
			DynamicObjectCollection dynamicObjectCollection = (DynamicObjectCollection)this.View.Model.DataObject["BomChild"];
			string value = MFGBillUtil.GetValue<string>(this.View.Model, "FEntryId", row, null, null);
			this.View.Model.SetValue("FRowType", "16", row);
			List<string> chItemsByEntryId = this.GetChItemsByEntryId(bomLevel, value, dynamicObjectCollection);
			if (chItemsByEntryId.Count <= 0)
			{
				return;
			}
			this.View.Model.BeginIniti();
			for (int i = entryRowCount - 1; i >= row; i--)
			{
				if (DataEntityExtend.GetDynamicObjectItemValue<int>(dynamicObjectCollection[i], "BomLevel", 0) > bomLevel && chItemsByEntryId.Contains(DataEntityExtend.GetDynamicObjectItemValue<string>(dynamicObjectCollection[i], "EntryId", null)))
				{
					this.View.Model.DeleteEntryRow("FBottomEntity", i);
				}
			}
			this.View.Model.EndIniti();
			this.View.UpdateView("FBottomEntity");
		}

		// Token: 0x06000C5D RID: 3165 RVA: 0x000921DC File Offset: 0x000903DC
		private List<string> GetChItemsByEntryId(int bomLevel, string entryId, DynamicObjectCollection bomItems)
		{
			List<string> list = new List<string>();
			List<DynamicObject> list2 = (from item in bomItems
			where DataEntityExtend.GetDynamicObjectItemValue<int>(item, "BomLevel", 0) > bomLevel
			orderby DataEntityExtend.GetDynamicObjectItemValue<int>(item, "BomLevel", 0) descending
			select item).ToList<DynamicObject>();
			Dictionary<string, IGrouping<string, DynamicObject>> bomChItemsOdDic = (from g in list2
			group g by DataEntityExtend.GetDynamicValue<string>(g, "EntryId", null)).ToDictionary((IGrouping<string, DynamicObject> d) => d.Key);
			foreach (DynamicObject dynamicObject in list2)
			{
				if (this.IsChItemByEntryId(entryId, dynamicObject, bomChItemsOdDic))
				{
					list.Add(DataEntityExtend.GetDynamicObjectItemValue<string>(dynamicObject, "EntryId", null));
				}
			}
			return list;
		}

		// Token: 0x06000C5E RID: 3166 RVA: 0x000922D8 File Offset: 0x000904D8
		private bool IsChItemByEntryId(string entryId, DynamicObject bomChItem, Dictionary<string, IGrouping<string, DynamicObject>> bomChItemsOdDic)
		{
			string dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<string>(bomChItem, "ParentEntryId", null);
			if (entryId == dynamicObjectItemValue)
			{
				return true;
			}
			IGrouping<string, DynamicObject> source;
			if (!bomChItemsOdDic.TryGetValue(dynamicObjectItemValue, out source))
			{
				return false;
			}
			DynamicObject dynamicObject = source.FirstOrDefault<DynamicObject>();
			return dynamicObject != null && this.IsChItemByEntryId(entryId, dynamicObject, bomChItemsOdDic);
		}

		// Token: 0x06000C5F RID: 3167 RVA: 0x0009234C File Offset: 0x0009054C
		private long GetParentPrdOrgId(int row)
		{
			long num = 0L;
			int bomLevel = MFGBillUtil.GetValue<int>(this.View.Model, "FBomLevel", row, 0, null);
			string b = MFGBillUtil.GetValue<string>(this.View.Model, "FParentEntryId", row, null, null);
			DynamicObjectCollection source = (DynamicObjectCollection)this.View.Model.DataObject["BomChild"];
			List<DynamicObject> list = (from item in source
			where DataEntityExtend.GetDynamicObjectItemValue<int>(item, "BomLevel", 0) < bomLevel
			orderby DataEntityExtend.GetDynamicObjectItemValue<int>(item, "BomLevel", 0) descending
			select item).ToList<DynamicObject>();
			foreach (DynamicObject dynamicObject in list)
			{
				if (!(DataEntityExtend.GetDynamicObjectItemValue<string>(dynamicObject, "EntryId", null) != b))
				{
					num = DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObject, "PrdOrgId_Id", 0L);
					if (num > 0L)
					{
						break;
					}
					b = DataEntityExtend.GetDynamicObjectItemValue<string>(dynamicObject, "ParentEntryId", null);
				}
			}
			if (num <= 0L)
			{
				num = this.selBOMParamByMO.MainOrgId;
			}
			return num;
		}

		// Token: 0x06000C60 RID: 3168 RVA: 0x0009247C File Offset: 0x0009067C
		private DynamicObjectCollection GetMtrlInfoByPrdOrgId(long prdOrgId, long mtrlId)
		{
			string filterClauseWihtKey = string.Format(" FUSEORGID={0} \r\nAND EXISTS(SELECT 1 FROM T_BD_MATERIAL TMN WHERE FMASTERID=TMN.FMASTERID AND TMN.FMATERIALID={1}) ", prdOrgId, mtrlId);
			QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter
			{
				FormId = "BD_MATERIAL",
				FilterClauseWihtKey = filterClauseWihtKey,
				SelectItems = SelectorItemInfo.CreateItems(new string[]
				{
					"FMasterId",
					"FMaterialId AS FMaterialId",
					"FNumber",
					"FName",
					"FErpClsId",
					"FSpecification"
				})
			};
			return MFGServiceHelper.GetDynamicObjectCollection(base.Context, queryBuilderParemeter, null);
		}

		// Token: 0x06000C61 RID: 3169 RVA: 0x0009250C File Offset: 0x0009070C
		private bool IsNeedLock_ForMo(dynamic row)
		{
			if (ProductStructureView.<IsNeedLock_ForMo>o__SiteContainer101.<>p__Site102 == null)
			{
				ProductStructureView.<IsNeedLock_ForMo>o__SiteContainer101.<>p__Site102 = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsTrue, typeof(ProductStructureView), new CSharpArgumentInfo[]
				{
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
				}));
			}
			Func<CallSite, object, bool> target = ProductStructureView.<IsNeedLock_ForMo>o__SiteContainer101.<>p__Site102.Target;
			CallSite <>p__Site = ProductStructureView.<IsNeedLock_ForMo>o__SiteContainer101.<>p__Site102;
			if (ProductStructureView.<IsNeedLock_ForMo>o__SiteContainer101.<>p__Site103 == null)
			{
				ProductStructureView.<IsNeedLock_ForMo>o__SiteContainer101.<>p__Site103 = CallSite<Func<CallSite, object, string, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.Equal, typeof(ProductStructureView), new CSharpArgumentInfo[]
				{
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null)
				}));
			}
			Func<CallSite, object, string, object> target2 = ProductStructureView.<IsNeedLock_ForMo>o__SiteContainer101.<>p__Site103.Target;
			CallSite <>p__Site2 = ProductStructureView.<IsNeedLock_ForMo>o__SiteContainer101.<>p__Site103;
			if (ProductStructureView.<IsNeedLock_ForMo>o__SiteContainer101.<>p__Site104 == null)
			{
				ProductStructureView.<IsNeedLock_ForMo>o__SiteContainer101.<>p__Site104 = CallSite<Func<CallSite, Type, object, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.None, "ToString", null, typeof(ProductStructureView), new CSharpArgumentInfo[]
				{
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.IsStaticType, null),
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
				}));
			}
			Func<CallSite, Type, object, object> target3 = ProductStructureView.<IsNeedLock_ForMo>o__SiteContainer101.<>p__Site104.Target;
			CallSite <>p__Site3 = ProductStructureView.<IsNeedLock_ForMo>o__SiteContainer101.<>p__Site104;
			Type typeFromHandle = typeof(Convert);
			if (ProductStructureView.<IsNeedLock_ForMo>o__SiteContainer101.<>p__Site105 == null)
			{
				ProductStructureView.<IsNeedLock_ForMo>o__SiteContainer101.<>p__Site105 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "BomLevel", typeof(ProductStructureView), new CSharpArgumentInfo[]
				{
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
				}));
			}
			object obj = target2(<>p__Site2, target3(<>p__Site3, typeFromHandle, ProductStructureView.<IsNeedLock_ForMo>o__SiteContainer101.<>p__Site105.Target(ProductStructureView.<IsNeedLock_ForMo>o__SiteContainer101.<>p__Site105, row)), "0");
			if (ProductStructureView.<IsNeedLock_ForMo>o__SiteContainer101.<>p__Site106 == null)
			{
				ProductStructureView.<IsNeedLock_ForMo>o__SiteContainer101.<>p__Site106 = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsFalse, typeof(ProductStructureView), new CSharpArgumentInfo[]
				{
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
				}));
			}
			object arg2;
			if (!ProductStructureView.<IsNeedLock_ForMo>o__SiteContainer101.<>p__Site106.Target(ProductStructureView.<IsNeedLock_ForMo>o__SiteContainer101.<>p__Site106, obj))
			{
				if (ProductStructureView.<IsNeedLock_ForMo>o__SiteContainer101.<>p__Site107 == null)
				{
					ProductStructureView.<IsNeedLock_ForMo>o__SiteContainer101.<>p__Site107 = CallSite<Func<CallSite, object, object, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.BinaryOperationLogical, ExpressionType.And, typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
					}));
				}
				Func<CallSite, object, object, object> target4 = ProductStructureView.<IsNeedLock_ForMo>o__SiteContainer101.<>p__Site107.Target;
				CallSite <>p__Site4 = ProductStructureView.<IsNeedLock_ForMo>o__SiteContainer101.<>p__Site107;
				object arg = obj;
				if (ProductStructureView.<IsNeedLock_ForMo>o__SiteContainer101.<>p__Site108 == null)
				{
					ProductStructureView.<IsNeedLock_ForMo>o__SiteContainer101.<>p__Site108 = CallSite<Func<CallSite, object, int, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.LessThanOrEqual, typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null)
					}));
				}
				Func<CallSite, object, int, object> target5 = ProductStructureView.<IsNeedLock_ForMo>o__SiteContainer101.<>p__Site108.Target;
				CallSite <>p__Site5 = ProductStructureView.<IsNeedLock_ForMo>o__SiteContainer101.<>p__Site108;
				if (ProductStructureView.<IsNeedLock_ForMo>o__SiteContainer101.<>p__Site109 == null)
				{
					ProductStructureView.<IsNeedLock_ForMo>o__SiteContainer101.<>p__Site109 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "BomEntryId", typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
					}));
				}
				arg2 = target4(<>p__Site4, arg, target5(<>p__Site5, ProductStructureView.<IsNeedLock_ForMo>o__SiteContainer101.<>p__Site109.Target(ProductStructureView.<IsNeedLock_ForMo>o__SiteContainer101.<>p__Site109, row), 0));
			}
			else
			{
				arg2 = obj;
			}
			return target(<>p__Site, arg2);
		}

		// Token: 0x06000C62 RID: 3170 RVA: 0x000927C8 File Offset: 0x000909C8
		private static Dictionary<string, DynamicObject> CreateMoBillTypeDicCache(Context ctx, List<string> lstBillTypeId)
		{
			Dictionary<string, DynamicObject> dictionary = new Dictionary<string, DynamicObject>();
			if (ListUtils.IsEmpty<string>(lstBillTypeId))
			{
				return dictionary;
			}
			foreach (string text in lstBillTypeId)
			{
				DynamicObject value = BusinessDataServiceHelper.LoadBillTypePara(ctx, "MoBillTypeParaSetting", text, true);
				dictionary.Add(text, value);
			}
			return dictionary;
		}

		// Token: 0x17000082 RID: 130
		// (get) Token: 0x06000C63 RID: 3171 RVA: 0x00092838 File Offset: 0x00090A38
		private bool IsParentSaleOrderView
		{
			get
			{
				return this.View.ParentFormView != null && this.View.ParentFormView.BillBusinessInfo.GetForm().Id == "SAL_SaleOrder";
			}
		}

		// Token: 0x17000083 RID: 131
		// (get) Token: 0x06000C64 RID: 3172 RVA: 0x0009286D File Offset: 0x00090A6D
		private bool IsParentSALView
		{
			get
			{
				return this.IsParentSaleOrderView;
			}
		}

		// Token: 0x17000084 RID: 132
		// (get) Token: 0x06000C65 RID: 3173 RVA: 0x00092875 File Offset: 0x00090A75
		// (set) Token: 0x06000C66 RID: 3174 RVA: 0x0009287D File Offset: 0x00090A7D
		private SelBomBillParam selSalBomBillParam { get; set; }

		// Token: 0x06000C67 RID: 3175 RVA: 0x00092888 File Offset: 0x00090A88
		private void OnInitialize_ForSAL(InitializeEventArgs e)
		{
			if (!this.IsParentSALView)
			{
				return;
			}
			if (this.View.ParentFormView != null)
			{
				object obj;
				this.View.ParentFormView.Session.TryGetValue("SelInStockBillParam", out obj);
				if (obj != null && obj is SelBomBillParam)
				{
					this.selSalBomBillParam = (obj as SelBomBillParam);
					this.View.ParentFormView.Session["SelInStockBillParam"] = null;
				}
			}
			this.RegisterEntityRule_ForSAL();
		}

		// Token: 0x06000C68 RID: 3176 RVA: 0x00092900 File Offset: 0x00090B00
		private void BeforeBindData_ForSAL(EventArgs e)
		{
			if (!this.IsParentSALView)
			{
				return;
			}
			this.SelFieldsVisible(new List<string>
			{
				"FIsExpandPurchaseMtrl",
				"FIsExpandVirtualMtrl"
			}, true);
			this.SelFieldsVisible(new List<string>
			{
				"FIsShowPurchaseMtrl",
				"FExpandLevel",
				"FValidDate",
				"FWorkShopID",
				"FPrdOrgId",
				"FGenerateType",
				"FUnitId",
				"FMOBillType"
			}, false);
			if (this.selSalBomBillParam != null)
			{
				this.Model.SetValue("FUseOrgId", this.selSalBomBillParam.PrdOrgId);
				this._TargetOrgId = this.selSalBomBillParam.StockOrgId;
			}
		}

		// Token: 0x06000C69 RID: 3177 RVA: 0x000929EC File Offset: 0x00090BEC
		private void EntityRowDoubleClick_ForSAL(EntityRowClickEventArgs e)
		{
			if (!this.IsParentSALView)
			{
				return;
			}
			EntryEntity entryEntity = this.Model.BillBusinessInfo.GetEntryEntity("FBottomEntity");
			if (string.Equals(e.Key, entryEntity.Key, StringComparison.OrdinalIgnoreCase) && e.Row >= 0 && e.Row < this.View.Model.GetEntryRowCount(entryEntity.Key) && this.View.StyleManager.GetEnabled(this.View.BusinessInfo.GetField("FIsSelect"), this.Model.GetEntityDataObject(entryEntity, e.Row)))
			{
				List<DynamicObject> lstSelRows = (from w in new List<DynamicObject>
				{
					this.View.Model.GetEntityDataObject(entryEntity).ElementAt(e.Row)
				}
				where DataEntityExtend.GetDynamicObjectItemValue<long>(w, "BomEntryId", 0L) > 0L
				select w).ToList<DynamicObject>();
				this.ReturnDataToSALParentBySelect(lstSelRows, false);
			}
		}

		// Token: 0x06000C6A RID: 3178 RVA: 0x00092AF0 File Offset: 0x00090CF0
		private void BarItemClick_ForSAL(BarItemClickEventArgs e)
		{
			if (!this.IsParentSALView)
			{
				return;
			}
			string barItemKey;
			if ((barItemKey = e.BarItemKey) != null)
			{
				if (barItemKey == "tbReturn")
				{
					this.ReturnDataForSAL();
					return;
				}
				if (!(barItemKey == "tbReturnAndClose"))
				{
					return;
				}
				this.ReturnDataForSAL();
				this.View.Close();
			}
		}

		// Token: 0x06000C6B RID: 3179 RVA: 0x00092B68 File Offset: 0x00090D68
		private void ReturnDataForSAL()
		{
			EntryEntity entryEntity = this.Model.BillBusinessInfo.GetEntryEntity("FBottomEntity");
			List<DynamicObject> lstSelRows = (from w in this.View.Model.GetEntityDataObject(entryEntity)
			where DataEntityExtend.GetDynamicObjectItemValue<bool>(w, "IsSelect", false) && DataEntityExtend.GetDynamicObjectItemValue<long>(w, "BomEntryId", 0L) > 0L
			select w).ToList<DynamicObject>();
			this.ReturnDataToSALParentBySelect(lstSelRows, false);
		}

		// Token: 0x06000C6C RID: 3180 RVA: 0x00092BCC File Offset: 0x00090DCC
		private void ReturnDataToSALParentBySelect(List<DynamicObject> lstSelRows, bool isReturnAndClose = false)
		{
			if (ListUtils.IsEmpty<DynamicObject>(lstSelRows) || this.MemBomQueryOption == null)
			{
				this.View.ShowMessage(ResManager.LoadKDString("请选择分录！", "015072000002224", 7, new object[0]), 0);
				return;
			}
			SelBill0ption selBill0ption = new SelBill0ption
			{
				FormId = this.View.BusinessInfo.GetForm().Id,
				ListSelRows = lstSelRows
			};
			if (this.View.ParentFormView != null && this.View.ParentFormView is IDynamicFormViewService)
			{
				this.View.ReturnToParentWindow(selBill0ption);
				(this.View.ParentFormView as IDynamicFormViewService).CustomEvents(this.View.PageId, "CustomSelBill", "returnData");
				this.View.SendDynamicFormAction(this.View.ParentFormView);
				if (isReturnAndClose)
				{
					this.View.Close();
				}
			}
		}

		// Token: 0x06000C6D RID: 3181 RVA: 0x00092CB0 File Offset: 0x00090EB0
		private void BeforeF7Select_ForSAL(BeforeF7SelectEventArgs e)
		{
			if (!this.IsParentSALView)
			{
				return;
			}
			string fieldKey;
			if ((fieldKey = e.FieldKey) != null)
			{
				if (!(fieldKey == "FQkParentMaterialId"))
				{
					return;
				}
				BaseDataField baseDataField = ObjectUtils.CreateCopy(this.View.BillBusinessInfo.GetField("FMaterialId2")) as BaseDataField;
				if (baseDataField != null)
				{
					baseDataField.EntityKey = "FBillHead";
					baseDataField.Entity = this.View.BillBusinessInfo.GetEntity("FBillHead");
					baseDataField.OrgFieldKey = "FUseOrgId";
					string filter = StringUtils.JoinFilterString(baseDataField.Filter, " FIsMainPrd = '1' ", "AND");
					baseDataField.Filter = filter;
					MFGCommonUtil.DoF7ButtonClick(this.View, e.FieldKey, null, baseDataField, false, "FNumber", true, 1, 0);
				}
			}
		}

		// Token: 0x06000C6E RID: 3182 RVA: 0x00092D70 File Offset: 0x00090F70
		private void DataChanged_ForSAL(DataChangedEventArgs e)
		{
			if (!this.IsParentSALView)
			{
				return;
			}
			string key;
			if ((key = e.Field.Key) != null)
			{
				if (!(key == "FQkParentMaterialId"))
				{
					return;
				}
				BaseDataField field = this.View.BillBusinessInfo.GetField("FMaterialId2") as BaseDataField;
				DynamicObjectCollection dynamicObjectCollection = null;
				if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(e.NewValue))
				{
					dynamicObjectCollection = this.LoadReference(field, e.NewValue.ToString(), "BD_MATERIAL", new List<string>
					{
						"fmaterialid"
					}, " FIsMainPrd = '1' ", false);
				}
				if (dynamicObjectCollection != null && dynamicObjectCollection.Count == 1)
				{
					DynamicObject dynamicObject = dynamicObjectCollection[0];
					this.HeadMtrlId = DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObject, "FMaterialId", 0L);
					this.View.Model.SetValue("FQkParentMaterialName", DataEntityExtend.GetDynamicObjectItemValue<string>(dynamicObject, "FName", null));
					this.View.Model.SetValue("FQkParentMaterialSpec", DataEntityExtend.GetDynamicObjectItemValue<string>(dynamicObject, "FSpecification", null));
					long num = DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObject, "FProduceUnitId", 0L);
					num = ((num > 0L) ? num : DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObject, "FStoreUnitID", 0L));
					this.View.Model.SetValue("FUnitId", num);
					long dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObject, "FMaterialId", 0L);
					long[] materialMasterAndUserOrgId = MaterialServiceHelper.GetMaterialMasterAndUserOrgId(base.Context, dynamicObjectItemValue);
					long defaultBomKey = BOMServiceHelper.GetDefaultBomKey(this.View.Context, materialMasterAndUserOrgId[0], materialMasterAndUserOrgId[1], 0L, 2);
					DynamicObject bomObjById = this.GetBomObjById(defaultBomKey);
					if (bomObjById != null)
					{
						this.View.Model.SetValue("FQkBomId", DataEntityExtend.GetDynamicObjectItemValue<string>(bomObjById, "Number", null));
						string empty = string.Empty;
						DataEntityExtend.GetDynamicObjectItemValue<LocaleValue>(bomObjById, "Name", null).TryGetValue(base.Context.UserLocale.LCID, ref empty);
						this.View.Model.SetValue("FQkBomName", empty);
					}
				}
			}
		}

		// Token: 0x06000C6F RID: 3183 RVA: 0x00092F64 File Offset: 0x00091164
		private void RegisterEntityRule_ForSAL()
		{
			if (!this.IsParentSALView)
			{
				return;
			}
			this.View.RuleContainer.AddPluginRule("FBottomEntity", 7, new Action<DynamicObject, object>(this.LockCheckBox_ForSAL), new string[]
			{
				"FBomLevel",
				"FBomEntryId",
				"FMaterialId2",
				"FBomId2",
				"FISSUETYPE"
			});
			this.View.RuleContainer.AddPluginRule("FBottomEntity", 7, new Action<DynamicObject, object>(this.LockCheckBoxByMaterialTypeForSAL), new string[]
			{
				"FMaterialType"
			});
		}

		// Token: 0x06000C70 RID: 3184 RVA: 0x00093000 File Offset: 0x00091200
		private void LockCheckBox_ForSAL(DynamicObject dataEntity, dynamic row)
		{
			if (ProductStructureView.<LockCheckBox_ForSAL>o__SiteContainer113.<>p__Site114 == null)
			{
				ProductStructureView.<LockCheckBox_ForSAL>o__SiteContainer113.<>p__Site114 = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsTrue, typeof(ProductStructureView), new CSharpArgumentInfo[]
				{
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
				}));
			}
			Func<CallSite, object, bool> target = ProductStructureView.<LockCheckBox_ForSAL>o__SiteContainer113.<>p__Site114.Target;
			CallSite <>p__Site = ProductStructureView.<LockCheckBox_ForSAL>o__SiteContainer113.<>p__Site114;
			if (ProductStructureView.<LockCheckBox_ForSAL>o__SiteContainer113.<>p__Site115 == null)
			{
				ProductStructureView.<LockCheckBox_ForSAL>o__SiteContainer113.<>p__Site115 = CallSite<Func<CallSite, ProductStructureView, object, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.InvokeSimpleName, "IsNeedLockForSAL", null, typeof(ProductStructureView), new CSharpArgumentInfo[]
				{
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
				}));
			}
			Func<CallSite, ProductStructureView, object, object> target2 = ProductStructureView.<LockCheckBox_ForSAL>o__SiteContainer113.<>p__Site115.Target;
			CallSite <>p__Site2 = ProductStructureView.<LockCheckBox_ForSAL>o__SiteContainer113.<>p__Site115;
			if (ProductStructureView.<LockCheckBox_ForSAL>o__SiteContainer113.<>p__Site116 == null)
			{
				ProductStructureView.<LockCheckBox_ForSAL>o__SiteContainer113.<>p__Site116 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "ActiveObject", typeof(ProductStructureView), new CSharpArgumentInfo[]
				{
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
				}));
			}
			if (target(<>p__Site, target2(<>p__Site2, this, ProductStructureView.<LockCheckBox_ForSAL>o__SiteContainer113.<>p__Site116.Target(ProductStructureView.<LockCheckBox_ForSAL>o__SiteContainer113.<>p__Site116, row))))
			{
				Field field = this.View.BusinessInfo.GetField("FIsSelect");
				if (ProductStructureView.<LockCheckBox_ForSAL>o__SiteContainer113.<>p__Site117 == null)
				{
					ProductStructureView.<LockCheckBox_ForSAL>o__SiteContainer113.<>p__Site117 = CallSite<Action<CallSite, IStyleManager, Field, object, string, bool>>.Create(Binder.InvokeMember(CSharpBinderFlags.ResultDiscarded, "SetEnabled", null, typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null)
					}));
				}
				Action<CallSite, IStyleManager, Field, object, string, bool> target3 = ProductStructureView.<LockCheckBox_ForSAL>o__SiteContainer113.<>p__Site117.Target;
				CallSite <>p__Site3 = ProductStructureView.<LockCheckBox_ForSAL>o__SiteContainer113.<>p__Site117;
				IStyleManager styleManager = this.View.StyleManager;
				Field arg = field;
				if (ProductStructureView.<LockCheckBox_ForSAL>o__SiteContainer113.<>p__Site118 == null)
				{
					ProductStructureView.<LockCheckBox_ForSAL>o__SiteContainer113.<>p__Site118 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "ActiveObject", typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
					}));
				}
				target3(<>p__Site3, styleManager, arg, ProductStructureView.<LockCheckBox_ForSAL>o__SiteContainer113.<>p__Site118.Target(ProductStructureView.<LockCheckBox_ForSAL>o__SiteContainer113.<>p__Site118, row), field.PropertyName, false);
			}
		}

		// Token: 0x06000C71 RID: 3185 RVA: 0x000931FC File Offset: 0x000913FC
		private void LockCheckBoxByMaterialTypeForSAL(DynamicObject dataEntity, dynamic row)
		{
			if (ProductStructureView.<LockCheckBoxByMaterialTypeForSAL>o__SiteContainer119.<>p__Site11a == null)
			{
				ProductStructureView.<LockCheckBoxByMaterialTypeForSAL>o__SiteContainer119.<>p__Site11a = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsTrue, typeof(ProductStructureView), new CSharpArgumentInfo[]
				{
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
				}));
			}
			Func<CallSite, object, bool> target = ProductStructureView.<LockCheckBoxByMaterialTypeForSAL>o__SiteContainer119.<>p__Site11a.Target;
			CallSite <>p__Site11a = ProductStructureView.<LockCheckBoxByMaterialTypeForSAL>o__SiteContainer119.<>p__Site11a;
			if (ProductStructureView.<LockCheckBoxByMaterialTypeForSAL>o__SiteContainer119.<>p__Site11b == null)
			{
				ProductStructureView.<LockCheckBoxByMaterialTypeForSAL>o__SiteContainer119.<>p__Site11b = CallSite<Func<CallSite, ProductStructureView, object, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.InvokeSimpleName, "IsNeedLockForSAL", null, typeof(ProductStructureView), new CSharpArgumentInfo[]
				{
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
				}));
			}
			Func<CallSite, ProductStructureView, object, object> target2 = ProductStructureView.<LockCheckBoxByMaterialTypeForSAL>o__SiteContainer119.<>p__Site11b.Target;
			CallSite <>p__Site11b = ProductStructureView.<LockCheckBoxByMaterialTypeForSAL>o__SiteContainer119.<>p__Site11b;
			if (ProductStructureView.<LockCheckBoxByMaterialTypeForSAL>o__SiteContainer119.<>p__Site11c == null)
			{
				ProductStructureView.<LockCheckBoxByMaterialTypeForSAL>o__SiteContainer119.<>p__Site11c = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "ActiveObject", typeof(ProductStructureView), new CSharpArgumentInfo[]
				{
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
				}));
			}
			if (target(<>p__Site11a, target2(<>p__Site11b, this, ProductStructureView.<LockCheckBoxByMaterialTypeForSAL>o__SiteContainer119.<>p__Site11c.Target(ProductStructureView.<LockCheckBoxByMaterialTypeForSAL>o__SiteContainer119.<>p__Site11c, row))))
			{
				Field field = this.View.BusinessInfo.GetField("FIsSelect");
				if (ProductStructureView.<LockCheckBoxByMaterialTypeForSAL>o__SiteContainer119.<>p__Site11d == null)
				{
					ProductStructureView.<LockCheckBoxByMaterialTypeForSAL>o__SiteContainer119.<>p__Site11d = CallSite<Action<CallSite, IStyleManager, Field, object, string, bool>>.Create(Binder.InvokeMember(CSharpBinderFlags.ResultDiscarded, "SetEnabled", null, typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null)
					}));
				}
				Action<CallSite, IStyleManager, Field, object, string, bool> target3 = ProductStructureView.<LockCheckBoxByMaterialTypeForSAL>o__SiteContainer119.<>p__Site11d.Target;
				CallSite <>p__Site11d = ProductStructureView.<LockCheckBoxByMaterialTypeForSAL>o__SiteContainer119.<>p__Site11d;
				IStyleManager styleManager = this.View.StyleManager;
				Field arg = field;
				if (ProductStructureView.<LockCheckBoxByMaterialTypeForSAL>o__SiteContainer119.<>p__Site11e == null)
				{
					ProductStructureView.<LockCheckBoxByMaterialTypeForSAL>o__SiteContainer119.<>p__Site11e = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "ActiveObject", typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
					}));
				}
				target3(<>p__Site11d, styleManager, arg, ProductStructureView.<LockCheckBoxByMaterialTypeForSAL>o__SiteContainer119.<>p__Site11e.Target(ProductStructureView.<LockCheckBoxByMaterialTypeForSAL>o__SiteContainer119.<>p__Site11e, row), field.PropertyName, false);
			}
		}

		// Token: 0x06000C72 RID: 3186 RVA: 0x000933F8 File Offset: 0x000915F8
		private bool IsNeedLockForSAL(dynamic row)
		{
			if (ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site120 == null)
			{
				ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site120 = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsTrue, typeof(ProductStructureView), new CSharpArgumentInfo[]
				{
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
				}));
			}
			Func<CallSite, object, bool> target = ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site120.Target;
			CallSite <>p__Site = ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site120;
			bool isParentSALView = this.IsParentSALView;
			object arg2;
			if (isParentSALView)
			{
				if (ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site121 == null)
				{
					ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site121 = CallSite<Func<CallSite, bool, object, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.BinaryOperationLogical, ExpressionType.And, typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
					}));
				}
				Func<CallSite, bool, object, object> target2 = ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site121.Target;
				CallSite <>p__Site2 = ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site121;
				bool arg = isParentSALView;
				if (ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site122 == null)
				{
					ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site122 = CallSite<Func<CallSite, object, string, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.Equal, typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null)
					}));
				}
				Func<CallSite, object, string, object> target3 = ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site122.Target;
				CallSite <>p__Site3 = ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site122;
				if (ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site123 == null)
				{
					ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site123 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "MaterialType", typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
					}));
				}
				arg2 = target2(<>p__Site2, arg, target3(<>p__Site3, ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site123.Target(ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site123, row), Convert.ToString(2)));
			}
			else
			{
				arg2 = isParentSALView;
			}
			if (target(<>p__Site, arg2))
			{
				return true;
			}
			if (ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site124 == null)
			{
				ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site124 = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsTrue, typeof(ProductStructureView), new CSharpArgumentInfo[]
				{
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
				}));
			}
			Func<CallSite, object, bool> target4 = ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site124.Target;
			CallSite <>p__Site4 = ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site124;
			if (ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site125 == null)
			{
				ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site125 = CallSite<Func<CallSite, object, string, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.Equal, typeof(ProductStructureView), new CSharpArgumentInfo[]
				{
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null)
				}));
			}
			Func<CallSite, object, string, object> target5 = ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site125.Target;
			CallSite <>p__Site5 = ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site125;
			if (ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site126 == null)
			{
				ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site126 = CallSite<Func<CallSite, Type, object, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.None, "ToString", null, typeof(ProductStructureView), new CSharpArgumentInfo[]
				{
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.IsStaticType, null),
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
				}));
			}
			Func<CallSite, Type, object, object> target6 = ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site126.Target;
			CallSite <>p__Site6 = ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site126;
			Type typeFromHandle = typeof(Convert);
			if (ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site127 == null)
			{
				ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site127 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "BomLevel", typeof(ProductStructureView), new CSharpArgumentInfo[]
				{
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
				}));
			}
			object obj = target5(<>p__Site5, target6(<>p__Site6, typeFromHandle, ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site127.Target(ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site127, row)), "0");
			if (ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site128 == null)
			{
				ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site128 = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsFalse, typeof(ProductStructureView), new CSharpArgumentInfo[]
				{
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
				}));
			}
			object obj2;
			if (!ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site128.Target(ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site128, obj))
			{
				if (ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site129 == null)
				{
					ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site129 = CallSite<Func<CallSite, object, object, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.BinaryOperationLogical, ExpressionType.And, typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
					}));
				}
				Func<CallSite, object, object, object> target7 = ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site129.Target;
				CallSite <>p__Site7 = ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site129;
				object arg3 = obj;
				if (ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site12a == null)
				{
					ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site12a = CallSite<Func<CallSite, object, int, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.LessThanOrEqual, typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null)
					}));
				}
				Func<CallSite, object, int, object> target8 = ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site12a.Target;
				CallSite <>p__Site12a = ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site12a;
				if (ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site12b == null)
				{
					ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site12b = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "BomEntryId", typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
					}));
				}
				obj2 = target7(<>p__Site7, arg3, target8(<>p__Site12a, ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site12b.Target(ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site12b, row), 0));
			}
			else
			{
				obj2 = obj;
			}
			object obj3 = obj2;
			if (ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site12c == null)
			{
				ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site12c = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsTrue, typeof(ProductStructureView), new CSharpArgumentInfo[]
				{
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
				}));
			}
			object obj6;
			if (!ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site12c.Target(ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site12c, obj3))
			{
				if (ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site12d == null)
				{
					ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site12d = CallSite<Func<CallSite, object, object, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.BinaryOperationLogical, ExpressionType.Or, typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
					}));
				}
				Func<CallSite, object, object, object> target9 = ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site12d.Target;
				CallSite <>p__Site12d = ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site12d;
				object arg4 = obj3;
				if (ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site12e == null)
				{
					ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site12e = CallSite<Func<CallSite, object, object, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.NotEqual, typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.Constant, null)
					}));
				}
				Func<CallSite, object, object, object> target10 = ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site12e.Target;
				CallSite <>p__Site12e = ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site12e;
				if (ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site12f == null)
				{
					ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site12f = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "MaterialId", typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
					}));
				}
				object obj4 = target10(<>p__Site12e, ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site12f.Target(ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site12f, row), null);
				if (ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site130 == null)
				{
					ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site130 = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsFalse, typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
					}));
				}
				object arg8;
				if (!ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site130.Target(ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site130, obj4))
				{
					if (ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site131 == null)
					{
						ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site131 = CallSite<Func<CallSite, object, object, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.BinaryOperationLogical, ExpressionType.And, typeof(ProductStructureView), new CSharpArgumentInfo[]
						{
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
						}));
					}
					Func<CallSite, object, object, object> target11 = ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site131.Target;
					CallSite <>p__Site8 = ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site131;
					object arg5 = obj4;
					if (ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site132 == null)
					{
						ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site132 = CallSite<Func<CallSite, object, string, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.NotEqual, typeof(ProductStructureView), new CSharpArgumentInfo[]
						{
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null)
						}));
					}
					Func<CallSite, object, string, object> target12 = ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site132.Target;
					CallSite <>p__Site9 = ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site132;
					if (ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site133 == null)
					{
						ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site133 = CallSite<Func<CallSite, Type, object, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.None, "ToString", null, typeof(ProductStructureView), new CSharpArgumentInfo[]
						{
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.IsStaticType, null),
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
						}));
					}
					Func<CallSite, Type, object, object> target13 = ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site133.Target;
					CallSite <>p__Site10 = ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site133;
					Type typeFromHandle2 = typeof(Convert);
					if (ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site134 == null)
					{
						ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site134 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "ForbidStatus", typeof(ProductStructureView), new CSharpArgumentInfo[]
						{
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
						}));
					}
					Func<CallSite, object, object> target14 = ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site134.Target;
					CallSite <>p__Site11 = ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site134;
					if (ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site135 == null)
					{
						ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site135 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "MaterialId", typeof(ProductStructureView), new CSharpArgumentInfo[]
						{
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
						}));
					}
					object obj5 = target12(<>p__Site9, target13(<>p__Site10, typeFromHandle2, target14(<>p__Site11, ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site135.Target(ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site135, row))), "A");
					if (ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site136 == null)
					{
						ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site136 = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsTrue, typeof(ProductStructureView), new CSharpArgumentInfo[]
						{
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
						}));
					}
					object arg7;
					if (!ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site136.Target(ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site136, obj5))
					{
						if (ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site137 == null)
						{
							ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site137 = CallSite<Func<CallSite, object, object, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.BinaryOperationLogical, ExpressionType.Or, typeof(ProductStructureView), new CSharpArgumentInfo[]
							{
								CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
								CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
							}));
						}
						Func<CallSite, object, object, object> target15 = ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site137.Target;
						CallSite <>p__Site12 = ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site137;
						object arg6 = obj5;
						if (ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site138 == null)
						{
							ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site138 = CallSite<Func<CallSite, object, string, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.NotEqual, typeof(ProductStructureView), new CSharpArgumentInfo[]
							{
								CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
								CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null)
							}));
						}
						Func<CallSite, object, string, object> target16 = ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site138.Target;
						CallSite <>p__Site13 = ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site138;
						if (ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site139 == null)
						{
							ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site139 = CallSite<Func<CallSite, Type, object, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.None, "ToString", null, typeof(ProductStructureView), new CSharpArgumentInfo[]
							{
								CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.IsStaticType, null),
								CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
							}));
						}
						Func<CallSite, Type, object, object> target17 = ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site139.Target;
						CallSite <>p__Site14 = ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site139;
						Type typeFromHandle3 = typeof(Convert);
						if (ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site13a == null)
						{
							ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site13a = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "DocumentStatus", typeof(ProductStructureView), new CSharpArgumentInfo[]
							{
								CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
							}));
						}
						Func<CallSite, object, object> target18 = ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site13a.Target;
						CallSite <>p__Site13a = ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site13a;
						if (ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site13b == null)
						{
							ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site13b = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "MaterialId", typeof(ProductStructureView), new CSharpArgumentInfo[]
							{
								CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
							}));
						}
						arg7 = target15(<>p__Site12, arg6, target16(<>p__Site13, target17(<>p__Site14, typeFromHandle3, target18(<>p__Site13a, ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site13b.Target(ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site13b, row))), "C"));
					}
					else
					{
						arg7 = obj5;
					}
					arg8 = target11(<>p__Site8, arg5, arg7);
				}
				else
				{
					arg8 = obj4;
				}
				obj6 = target9(<>p__Site12d, arg4, arg8);
			}
			else
			{
				obj6 = obj3;
			}
			object obj7 = obj6;
			if (ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site13c == null)
			{
				ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site13c = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsTrue, typeof(ProductStructureView), new CSharpArgumentInfo[]
				{
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
				}));
			}
			object arg10;
			if (!ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site13c.Target(ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site13c, obj7))
			{
				if (ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site13d == null)
				{
					ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site13d = CallSite<Func<CallSite, object, object, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.BinaryOperationLogical, ExpressionType.Or, typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
					}));
				}
				Func<CallSite, object, object, object> target19 = ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site13d.Target;
				CallSite <>p__Site13d = ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site13d;
				object arg9 = obj7;
				if (ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site13e == null)
				{
					ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site13e = CallSite<Func<CallSite, object, string, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.Equal, typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null)
					}));
				}
				Func<CallSite, object, string, object> target20 = ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site13e.Target;
				CallSite <>p__Site13e = ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site13e;
				if (ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site13f == null)
				{
					ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site13f = CallSite<Func<CallSite, Type, object, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.None, "ToString", null, typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.IsStaticType, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
					}));
				}
				Func<CallSite, Type, object, object> target21 = ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site13f.Target;
				CallSite <>p__Site13f = ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site13f;
				Type typeFromHandle4 = typeof(Convert);
				if (ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site140 == null)
				{
					ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site140 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "ISSUETYPE", typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
					}));
				}
				arg10 = target19(<>p__Site13d, arg9, target20(<>p__Site13e, target21(<>p__Site13f, typeFromHandle4, ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site140.Target(ProductStructureView.<IsNeedLockForSAL>o__SiteContainer11f.<>p__Site140, row)), "7"));
			}
			else
			{
				arg10 = obj7;
			}
			return target4(<>p__Site4, arg10);
		}

		// Token: 0x06000C73 RID: 3187 RVA: 0x00093F9C File Offset: 0x0009219C
		private void SetRowsDefSelectForSAL()
		{
			if (!this.IsParentSALView)
			{
				return;
			}
			EntryEntity entryEntity = this.Model.BillBusinessInfo.GetEntryEntity("FBottomEntity");
			DynamicObjectCollection entityDataObject = this.View.Model.GetEntityDataObject(entryEntity);
			List<DynamicObject> list = (from o in entityDataObject
			orderby DataEntityExtend.GetDynamicValue<int>(o, "BomLevel", 0)
			select o).ToList<DynamicObject>();
			List<string> list2 = new List<string>();
			using (List<DynamicObject>.Enumerator enumerator = list.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					DynamicObject oneRow = enumerator.Current;
					string dynamicValue = DataEntityExtend.GetDynamicValue<string>(oneRow, "EntryId", null);
					int dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<int>(oneRow, "BomLevel", 0);
					bool dynamicObjectItemValue2 = DataEntityExtend.GetDynamicObjectItemValue<bool>(oneRow, "IsSkip", false);
					if (!this.IsNeedLockForSAL(oneRow) && dynamicObjectItemValue >= 1)
					{
						if (this.MemBomQueryOption.ExpandVirtualMaterial)
						{
							DynamicObject dynamicObject = (from w in list
							where DataEntityExtend.GetDynamicObjectItemValue<string>(w, "EntryId", null) == DataEntityExtend.GetDynamicObjectItemValue<string>(oneRow, "ParentEntryId", null)
							select w).FirstOrDefault<DynamicObject>();
							string item = (dynamicObject == null) ? string.Empty : DataEntityExtend.GetDynamicValue<string>(dynamicObject, "EntryId", null);
							if (list2.Contains(item))
							{
								list2.Add(dynamicValue);
							}
							else if (!dynamicObjectItemValue2)
							{
								oneRow["IsSelect"] = true;
								list2.Add(dynamicValue);
							}
						}
						else
						{
							oneRow["IsSelect"] = true;
						}
					}
				}
			}
		}

		// Token: 0x06000C74 RID: 3188 RVA: 0x00094158 File Offset: 0x00092358
		private void ChangeDataRowColorForSAL(int row = -1)
		{
			int i = row;
			int num = i + 1;
			List<KeyValuePair<int, string>> list = new List<KeyValuePair<int, string>>();
			if (row == -1)
			{
				i = 0;
				num = this.Model.GetEntryRowCount("FBottomEntity");
			}
			while (i < num)
			{
				if (this.selBomBillParam != null && MFGBillUtil.GetValue<int>(this.Model, "FBomEntryId", i, 0, null) > 0 && ((MFGBillUtil.GetValue<DynamicObject>(this.Model, "FSupplyOrgId", i, null, null) != null && this.selBomBillParam.StockOrgId != Convert.ToInt64(MFGBillUtil.GetValue<DynamicObject>(this.Model, "FSupplyOrgId", i, null, null)["Id"])) || this.selBomBillParam.OwnerType != MFGBillUtil.GetValue<string>(this.Model, "FOWNERTYPEID", i, null, null) || (MFGBillUtil.GetValue<DynamicObject>(this.Model, "FOWNERID", i, null, null) != null && this.selBomBillParam.OwnerId != Convert.ToInt64(MFGBillUtil.GetValue<DynamicObject>(this.Model, "FOWNERID", i, null, null)["Id"]))))
				{
					list.Add(new KeyValuePair<int, string>(i, "#CCCCCC"));
				}
				i++;
			}
			this.View.GetControl<EntryGrid>("FBottomEntity").SetRowBackcolor(list);
		}

		// Token: 0x17000085 RID: 133
		// (get) Token: 0x06000C75 RID: 3189 RVA: 0x00094293 File Offset: 0x00092493
		private bool IsParentSpPickView
		{
			get
			{
				return this.View.ParentFormView != null && this.View.ParentFormView.BillBusinessInfo.GetForm().Id == "SP_PickMtrl";
			}
		}

		// Token: 0x17000086 RID: 134
		// (get) Token: 0x06000C76 RID: 3190 RVA: 0x000942C8 File Offset: 0x000924C8
		private bool IsParentSpReturnView
		{
			get
			{
				return this.View.ParentFormView != null && this.View.ParentFormView.BillBusinessInfo.GetForm().Id == "SP_ReturnMtrl";
			}
		}

		// Token: 0x17000087 RID: 135
		// (get) Token: 0x06000C77 RID: 3191 RVA: 0x000942FD File Offset: 0x000924FD
		private bool IsParentSPView
		{
			get
			{
				return this.IsParentSpPickView || this.IsParentSpReturnView;
			}
		}

		// Token: 0x17000088 RID: 136
		// (get) Token: 0x06000C78 RID: 3192 RVA: 0x0009430F File Offset: 0x0009250F
		// (set) Token: 0x06000C79 RID: 3193 RVA: 0x00094317 File Offset: 0x00092517
		private SelBomBillParam selBomBillParam { get; set; }

		// Token: 0x06000C7A RID: 3194 RVA: 0x00094320 File Offset: 0x00092520
		private void OnInitialize_ForSP(InitializeEventArgs e)
		{
			if (!this.IsParentSPView)
			{
				return;
			}
			if (this.View.ParentFormView != null)
			{
				object obj;
				this.View.ParentFormView.Session.TryGetValue("SelInStockBillParam", out obj);
				if (obj != null && obj is SelBomBillParam)
				{
					this.selBomBillParam = (obj as SelBomBillParam);
					this.View.ParentFormView.Session["SelInStockBillParam"] = null;
				}
			}
			this.RegisterEntityRule_ForSP();
			this.RegisterEntityRule_ForSpPick();
		}

		// Token: 0x06000C7B RID: 3195 RVA: 0x000943A0 File Offset: 0x000925A0
		private void BeforeBindData_ForSP(EventArgs e)
		{
			if (!this.IsParentSPView)
			{
				return;
			}
			this.SelFieldsVisible(new List<string>
			{
				"FIsExpandPurchaseMtrl",
				"FIsExpandVirtualMtrl"
			}, true);
			this.SelFieldsVisible(new List<string>
			{
				"FIsShowPurchaseMtrl",
				"FExpandLevel",
				"FValidDate",
				"FWorkShopID",
				"FPrdOrgId",
				"FGenerateType",
				"FUnitId",
				"FMOBillType"
			}, false);
			if (this.selBomBillParam != null)
			{
				this.Model.SetValue("FUseOrgId", this.selBomBillParam.PrdOrgId);
				this._TargetOrgId = this.selBomBillParam.StockOrgId;
			}
		}

		// Token: 0x06000C7C RID: 3196 RVA: 0x0009448C File Offset: 0x0009268C
		private void EntityRowDoubleClick_ForSP(EntityRowClickEventArgs e)
		{
			if (!this.IsParentSPView)
			{
				return;
			}
			EntryEntity entryEntity = this.Model.BillBusinessInfo.GetEntryEntity("FBottomEntity");
			if (string.Equals(e.Key, entryEntity.Key, StringComparison.OrdinalIgnoreCase) && e.Row >= 0 && e.Row < this.View.Model.GetEntryRowCount(entryEntity.Key) && this.View.StyleManager.GetEnabled(this.View.BusinessInfo.GetField("FIsSelect"), this.Model.GetEntityDataObject(entryEntity, e.Row)))
			{
				List<DynamicObject> lstSelRows = (from w in new List<DynamicObject>
				{
					this.View.Model.GetEntityDataObject(entryEntity).ElementAt(e.Row)
				}
				where DataEntityExtend.GetDynamicObjectItemValue<long>(w, "BomEntryId", 0L) > 0L
				select w).ToList<DynamicObject>();
				this.ReturnDataToSPParentBySelect(lstSelRows, false);
			}
		}

		// Token: 0x06000C7D RID: 3197 RVA: 0x00094590 File Offset: 0x00092790
		private void BarItemClick_ForSP(BarItemClickEventArgs e)
		{
			if (!this.IsParentSPView)
			{
				return;
			}
			string barItemKey;
			if ((barItemKey = e.BarItemKey) != null)
			{
				if (barItemKey == "tbReturn")
				{
					this.ReturnDataForSP();
					return;
				}
				if (!(barItemKey == "tbReturnAndClose"))
				{
					return;
				}
				this.ReturnDataForSP();
				this.View.Close();
			}
		}

		// Token: 0x06000C7E RID: 3198 RVA: 0x00094608 File Offset: 0x00092808
		private void ReturnDataForSP()
		{
			EntryEntity entryEntity = this.Model.BillBusinessInfo.GetEntryEntity("FBottomEntity");
			List<DynamicObject> lstSelRows = (from w in this.View.Model.GetEntityDataObject(entryEntity)
			where DataEntityExtend.GetDynamicObjectItemValue<bool>(w, "IsSelect", false) && DataEntityExtend.GetDynamicObjectItemValue<long>(w, "BomEntryId", 0L) > 0L
			select w).ToList<DynamicObject>();
			this.ReturnDataToSPParentBySelect(lstSelRows, false);
		}

		// Token: 0x06000C7F RID: 3199 RVA: 0x0009466C File Offset: 0x0009286C
		private void ReturnDataToSPParentBySelect(List<DynamicObject> lstSelRows, bool isReturnAndClose = false)
		{
			if (ListUtils.IsEmpty<DynamicObject>(lstSelRows) || this.MemBomQueryOption == null)
			{
				this.View.ShowMessage(ResManager.LoadKDString("请选择分录！", "015072000002224", 7, new object[0]), 0);
				return;
			}
			SelBill0ption selBill0ption = new SelBill0ption
			{
				FormId = this.View.BusinessInfo.GetForm().Id,
				ListSelRows = lstSelRows
			};
			if (this.View.ParentFormView != null && this.View.ParentFormView is IDynamicFormViewService)
			{
				this.View.ReturnToParentWindow(selBill0ption);
				(this.View.ParentFormView as IDynamicFormViewService).CustomEvents(this.View.PageId, "CustomSelBill", "returnData");
				this.View.SendDynamicFormAction(this.View.ParentFormView);
				if (isReturnAndClose)
				{
					this.View.Close();
				}
			}
		}

		// Token: 0x06000C80 RID: 3200 RVA: 0x00094750 File Offset: 0x00092950
		private void RegisterEntityRule_ForSpPick()
		{
			if (!this.IsParentSpPickView)
			{
				return;
			}
			this.View.RuleContainer.AddPluginRule("FBottomEntity", 7, new Action<DynamicObject, object>(this.LockCheckBoxByMaterialType), new string[]
			{
				"FMaterialType"
			});
		}

		// Token: 0x06000C81 RID: 3201 RVA: 0x0009479C File Offset: 0x0009299C
		private void LockCheckBoxByMaterialType(DynamicObject dataEntity, dynamic row)
		{
			if (ProductStructureView.<LockCheckBoxByMaterialType>o__SiteContainer14f.<>p__Site150 == null)
			{
				ProductStructureView.<LockCheckBoxByMaterialType>o__SiteContainer14f.<>p__Site150 = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsTrue, typeof(ProductStructureView), new CSharpArgumentInfo[]
				{
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
				}));
			}
			Func<CallSite, object, bool> target = ProductStructureView.<LockCheckBoxByMaterialType>o__SiteContainer14f.<>p__Site150.Target;
			CallSite <>p__Site = ProductStructureView.<LockCheckBoxByMaterialType>o__SiteContainer14f.<>p__Site150;
			if (ProductStructureView.<LockCheckBoxByMaterialType>o__SiteContainer14f.<>p__Site151 == null)
			{
				ProductStructureView.<LockCheckBoxByMaterialType>o__SiteContainer14f.<>p__Site151 = CallSite<Func<CallSite, ProductStructureView, object, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.InvokeSimpleName, "IsNeedLock", null, typeof(ProductStructureView), new CSharpArgumentInfo[]
				{
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
				}));
			}
			Func<CallSite, ProductStructureView, object, object> target2 = ProductStructureView.<LockCheckBoxByMaterialType>o__SiteContainer14f.<>p__Site151.Target;
			CallSite <>p__Site2 = ProductStructureView.<LockCheckBoxByMaterialType>o__SiteContainer14f.<>p__Site151;
			if (ProductStructureView.<LockCheckBoxByMaterialType>o__SiteContainer14f.<>p__Site152 == null)
			{
				ProductStructureView.<LockCheckBoxByMaterialType>o__SiteContainer14f.<>p__Site152 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "ActiveObject", typeof(ProductStructureView), new CSharpArgumentInfo[]
				{
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
				}));
			}
			if (target(<>p__Site, target2(<>p__Site2, this, ProductStructureView.<LockCheckBoxByMaterialType>o__SiteContainer14f.<>p__Site152.Target(ProductStructureView.<LockCheckBoxByMaterialType>o__SiteContainer14f.<>p__Site152, row))))
			{
				Field field = this.View.BusinessInfo.GetField("FIsSelect");
				if (ProductStructureView.<LockCheckBoxByMaterialType>o__SiteContainer14f.<>p__Site153 == null)
				{
					ProductStructureView.<LockCheckBoxByMaterialType>o__SiteContainer14f.<>p__Site153 = CallSite<Action<CallSite, IStyleManager, Field, object, string, bool>>.Create(Binder.InvokeMember(CSharpBinderFlags.ResultDiscarded, "SetEnabled", null, typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null)
					}));
				}
				Action<CallSite, IStyleManager, Field, object, string, bool> target3 = ProductStructureView.<LockCheckBoxByMaterialType>o__SiteContainer14f.<>p__Site153.Target;
				CallSite <>p__Site3 = ProductStructureView.<LockCheckBoxByMaterialType>o__SiteContainer14f.<>p__Site153;
				IStyleManager styleManager = this.View.StyleManager;
				Field arg = field;
				if (ProductStructureView.<LockCheckBoxByMaterialType>o__SiteContainer14f.<>p__Site154 == null)
				{
					ProductStructureView.<LockCheckBoxByMaterialType>o__SiteContainer14f.<>p__Site154 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "ActiveObject", typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
					}));
				}
				target3(<>p__Site3, styleManager, arg, ProductStructureView.<LockCheckBoxByMaterialType>o__SiteContainer14f.<>p__Site154.Target(ProductStructureView.<LockCheckBoxByMaterialType>o__SiteContainer14f.<>p__Site154, row), field.PropertyName, false);
			}
		}

		// Token: 0x06000C82 RID: 3202 RVA: 0x00094998 File Offset: 0x00092B98
		private void RegisterEntityRule_ForSP()
		{
			if (!this.IsParentSPView)
			{
				return;
			}
			this.View.RuleContainer.AddPluginRule("FBottomEntity", 7, new Action<DynamicObject, object>(this.LockCheckBox_ForSP), new string[]
			{
				"FBomLevel",
				"FBomEntryId",
				"FMaterialId2",
				"FBomId2",
				"FISSUETYPE"
			});
		}

		// Token: 0x06000C83 RID: 3203 RVA: 0x00094A04 File Offset: 0x00092C04
		private void LockCheckBox_ForSP(DynamicObject dataEntity, dynamic row)
		{
			if (ProductStructureView.<LockCheckBox_ForSP>o__SiteContainer155.<>p__Site156 == null)
			{
				ProductStructureView.<LockCheckBox_ForSP>o__SiteContainer155.<>p__Site156 = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsTrue, typeof(ProductStructureView), new CSharpArgumentInfo[]
				{
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
				}));
			}
			Func<CallSite, object, bool> target = ProductStructureView.<LockCheckBox_ForSP>o__SiteContainer155.<>p__Site156.Target;
			CallSite <>p__Site = ProductStructureView.<LockCheckBox_ForSP>o__SiteContainer155.<>p__Site156;
			if (ProductStructureView.<LockCheckBox_ForSP>o__SiteContainer155.<>p__Site157 == null)
			{
				ProductStructureView.<LockCheckBox_ForSP>o__SiteContainer155.<>p__Site157 = CallSite<Func<CallSite, ProductStructureView, object, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.InvokeSimpleName, "IsNeedLock", null, typeof(ProductStructureView), new CSharpArgumentInfo[]
				{
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
				}));
			}
			Func<CallSite, ProductStructureView, object, object> target2 = ProductStructureView.<LockCheckBox_ForSP>o__SiteContainer155.<>p__Site157.Target;
			CallSite <>p__Site2 = ProductStructureView.<LockCheckBox_ForSP>o__SiteContainer155.<>p__Site157;
			if (ProductStructureView.<LockCheckBox_ForSP>o__SiteContainer155.<>p__Site158 == null)
			{
				ProductStructureView.<LockCheckBox_ForSP>o__SiteContainer155.<>p__Site158 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "ActiveObject", typeof(ProductStructureView), new CSharpArgumentInfo[]
				{
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
				}));
			}
			if (target(<>p__Site, target2(<>p__Site2, this, ProductStructureView.<LockCheckBox_ForSP>o__SiteContainer155.<>p__Site158.Target(ProductStructureView.<LockCheckBox_ForSP>o__SiteContainer155.<>p__Site158, row))))
			{
				Field field = this.View.BusinessInfo.GetField("FIsSelect");
				if (ProductStructureView.<LockCheckBox_ForSP>o__SiteContainer155.<>p__Site159 == null)
				{
					ProductStructureView.<LockCheckBox_ForSP>o__SiteContainer155.<>p__Site159 = CallSite<Action<CallSite, IStyleManager, Field, object, string, bool>>.Create(Binder.InvokeMember(CSharpBinderFlags.ResultDiscarded, "SetEnabled", null, typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null)
					}));
				}
				Action<CallSite, IStyleManager, Field, object, string, bool> target3 = ProductStructureView.<LockCheckBox_ForSP>o__SiteContainer155.<>p__Site159.Target;
				CallSite <>p__Site3 = ProductStructureView.<LockCheckBox_ForSP>o__SiteContainer155.<>p__Site159;
				IStyleManager styleManager = this.View.StyleManager;
				Field arg = field;
				if (ProductStructureView.<LockCheckBox_ForSP>o__SiteContainer155.<>p__Site15a == null)
				{
					ProductStructureView.<LockCheckBox_ForSP>o__SiteContainer155.<>p__Site15a = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "ActiveObject", typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
					}));
				}
				target3(<>p__Site3, styleManager, arg, ProductStructureView.<LockCheckBox_ForSP>o__SiteContainer155.<>p__Site15a.Target(ProductStructureView.<LockCheckBox_ForSP>o__SiteContainer155.<>p__Site15a, row), field.PropertyName, false);
			}
		}

		// Token: 0x06000C84 RID: 3204 RVA: 0x00094C00 File Offset: 0x00092E00
		private bool IsNeedLock(dynamic row)
		{
			if (ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site15c == null)
			{
				ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site15c = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsTrue, typeof(ProductStructureView), new CSharpArgumentInfo[]
				{
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
				}));
			}
			Func<CallSite, object, bool> target = ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site15c.Target;
			CallSite <>p__Site15c = ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site15c;
			bool isParentSpPickView = this.IsParentSpPickView;
			object arg2;
			if (isParentSpPickView)
			{
				if (ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site15d == null)
				{
					ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site15d = CallSite<Func<CallSite, bool, object, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.BinaryOperationLogical, ExpressionType.And, typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
					}));
				}
				Func<CallSite, bool, object, object> target2 = ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site15d.Target;
				CallSite <>p__Site15d = ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site15d;
				bool arg = isParentSpPickView;
				if (ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site15e == null)
				{
					ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site15e = CallSite<Func<CallSite, object, string, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.Equal, typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null)
					}));
				}
				Func<CallSite, object, string, object> target3 = ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site15e.Target;
				CallSite <>p__Site15e = ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site15e;
				if (ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site15f == null)
				{
					ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site15f = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "MaterialType", typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
					}));
				}
				arg2 = target2(<>p__Site15d, arg, target3(<>p__Site15e, ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site15f.Target(ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site15f, row), Convert.ToString(2)));
			}
			else
			{
				arg2 = isParentSpPickView;
			}
			if (target(<>p__Site15c, arg2))
			{
				return true;
			}
			if (ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site160 == null)
			{
				ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site160 = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsTrue, typeof(ProductStructureView), new CSharpArgumentInfo[]
				{
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
				}));
			}
			Func<CallSite, object, bool> target4 = ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site160.Target;
			CallSite <>p__Site = ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site160;
			if (ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site161 == null)
			{
				ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site161 = CallSite<Func<CallSite, object, string, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.Equal, typeof(ProductStructureView), new CSharpArgumentInfo[]
				{
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null)
				}));
			}
			Func<CallSite, object, string, object> target5 = ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site161.Target;
			CallSite <>p__Site2 = ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site161;
			if (ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site162 == null)
			{
				ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site162 = CallSite<Func<CallSite, Type, object, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.None, "ToString", null, typeof(ProductStructureView), new CSharpArgumentInfo[]
				{
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.IsStaticType, null),
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
				}));
			}
			Func<CallSite, Type, object, object> target6 = ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site162.Target;
			CallSite <>p__Site3 = ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site162;
			Type typeFromHandle = typeof(Convert);
			if (ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site163 == null)
			{
				ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site163 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "BomLevel", typeof(ProductStructureView), new CSharpArgumentInfo[]
				{
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
				}));
			}
			object obj = target5(<>p__Site2, target6(<>p__Site3, typeFromHandle, ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site163.Target(ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site163, row)), "0");
			if (ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site164 == null)
			{
				ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site164 = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsFalse, typeof(ProductStructureView), new CSharpArgumentInfo[]
				{
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
				}));
			}
			object obj2;
			if (!ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site164.Target(ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site164, obj))
			{
				if (ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site165 == null)
				{
					ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site165 = CallSite<Func<CallSite, object, object, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.BinaryOperationLogical, ExpressionType.And, typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
					}));
				}
				Func<CallSite, object, object, object> target7 = ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site165.Target;
				CallSite <>p__Site4 = ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site165;
				object arg3 = obj;
				if (ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site166 == null)
				{
					ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site166 = CallSite<Func<CallSite, object, int, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.LessThanOrEqual, typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null)
					}));
				}
				Func<CallSite, object, int, object> target8 = ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site166.Target;
				CallSite <>p__Site5 = ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site166;
				if (ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site167 == null)
				{
					ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site167 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "BomEntryId", typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
					}));
				}
				obj2 = target7(<>p__Site4, arg3, target8(<>p__Site5, ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site167.Target(ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site167, row), 0));
			}
			else
			{
				obj2 = obj;
			}
			object obj3 = obj2;
			if (ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site168 == null)
			{
				ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site168 = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsTrue, typeof(ProductStructureView), new CSharpArgumentInfo[]
				{
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
				}));
			}
			object obj6;
			if (!ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site168.Target(ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site168, obj3))
			{
				if (ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site169 == null)
				{
					ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site169 = CallSite<Func<CallSite, object, object, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.BinaryOperationLogical, ExpressionType.Or, typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
					}));
				}
				Func<CallSite, object, object, object> target9 = ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site169.Target;
				CallSite <>p__Site6 = ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site169;
				object arg4 = obj3;
				if (ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site16a == null)
				{
					ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site16a = CallSite<Func<CallSite, object, object, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.NotEqual, typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.Constant, null)
					}));
				}
				Func<CallSite, object, object, object> target10 = ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site16a.Target;
				CallSite <>p__Site16a = ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site16a;
				if (ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site16b == null)
				{
					ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site16b = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "MaterialId", typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
					}));
				}
				object obj4 = target10(<>p__Site16a, ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site16b.Target(ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site16b, row), null);
				if (ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site16c == null)
				{
					ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site16c = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsFalse, typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
					}));
				}
				object arg8;
				if (!ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site16c.Target(ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site16c, obj4))
				{
					if (ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site16d == null)
					{
						ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site16d = CallSite<Func<CallSite, object, object, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.BinaryOperationLogical, ExpressionType.And, typeof(ProductStructureView), new CSharpArgumentInfo[]
						{
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
						}));
					}
					Func<CallSite, object, object, object> target11 = ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site16d.Target;
					CallSite <>p__Site16d = ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site16d;
					object arg5 = obj4;
					if (ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site16e == null)
					{
						ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site16e = CallSite<Func<CallSite, object, string, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.NotEqual, typeof(ProductStructureView), new CSharpArgumentInfo[]
						{
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null)
						}));
					}
					Func<CallSite, object, string, object> target12 = ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site16e.Target;
					CallSite <>p__Site16e = ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site16e;
					if (ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site16f == null)
					{
						ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site16f = CallSite<Func<CallSite, Type, object, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.None, "ToString", null, typeof(ProductStructureView), new CSharpArgumentInfo[]
						{
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.IsStaticType, null),
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
						}));
					}
					Func<CallSite, Type, object, object> target13 = ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site16f.Target;
					CallSite <>p__Site16f = ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site16f;
					Type typeFromHandle2 = typeof(Convert);
					if (ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site170 == null)
					{
						ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site170 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "ForbidStatus", typeof(ProductStructureView), new CSharpArgumentInfo[]
						{
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
						}));
					}
					Func<CallSite, object, object> target14 = ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site170.Target;
					CallSite <>p__Site7 = ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site170;
					if (ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site171 == null)
					{
						ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site171 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "MaterialId", typeof(ProductStructureView), new CSharpArgumentInfo[]
						{
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
						}));
					}
					object obj5 = target12(<>p__Site16e, target13(<>p__Site16f, typeFromHandle2, target14(<>p__Site7, ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site171.Target(ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site171, row))), "A");
					if (ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site172 == null)
					{
						ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site172 = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsTrue, typeof(ProductStructureView), new CSharpArgumentInfo[]
						{
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
						}));
					}
					object arg7;
					if (!ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site172.Target(ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site172, obj5))
					{
						if (ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site173 == null)
						{
							ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site173 = CallSite<Func<CallSite, object, object, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.BinaryOperationLogical, ExpressionType.Or, typeof(ProductStructureView), new CSharpArgumentInfo[]
							{
								CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
								CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
							}));
						}
						Func<CallSite, object, object, object> target15 = ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site173.Target;
						CallSite <>p__Site8 = ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site173;
						object arg6 = obj5;
						if (ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site174 == null)
						{
							ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site174 = CallSite<Func<CallSite, object, string, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.NotEqual, typeof(ProductStructureView), new CSharpArgumentInfo[]
							{
								CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
								CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null)
							}));
						}
						Func<CallSite, object, string, object> target16 = ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site174.Target;
						CallSite <>p__Site9 = ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site174;
						if (ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site175 == null)
						{
							ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site175 = CallSite<Func<CallSite, Type, object, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.None, "ToString", null, typeof(ProductStructureView), new CSharpArgumentInfo[]
							{
								CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.IsStaticType, null),
								CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
							}));
						}
						Func<CallSite, Type, object, object> target17 = ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site175.Target;
						CallSite <>p__Site10 = ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site175;
						Type typeFromHandle3 = typeof(Convert);
						if (ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site176 == null)
						{
							ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site176 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "DocumentStatus", typeof(ProductStructureView), new CSharpArgumentInfo[]
							{
								CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
							}));
						}
						Func<CallSite, object, object> target18 = ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site176.Target;
						CallSite <>p__Site11 = ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site176;
						if (ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site177 == null)
						{
							ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site177 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "MaterialId", typeof(ProductStructureView), new CSharpArgumentInfo[]
							{
								CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
							}));
						}
						arg7 = target15(<>p__Site8, arg6, target16(<>p__Site9, target17(<>p__Site10, typeFromHandle3, target18(<>p__Site11, ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site177.Target(ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site177, row))), "C"));
					}
					else
					{
						arg7 = obj5;
					}
					arg8 = target11(<>p__Site16d, arg5, arg7);
				}
				else
				{
					arg8 = obj4;
				}
				obj6 = target9(<>p__Site6, arg4, arg8);
			}
			else
			{
				obj6 = obj3;
			}
			object obj7 = obj6;
			if (ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site178 == null)
			{
				ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site178 = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsTrue, typeof(ProductStructureView), new CSharpArgumentInfo[]
				{
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
				}));
			}
			object arg10;
			if (!ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site178.Target(ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site178, obj7))
			{
				if (ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site179 == null)
				{
					ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site179 = CallSite<Func<CallSite, object, object, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.BinaryOperationLogical, ExpressionType.Or, typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
					}));
				}
				Func<CallSite, object, object, object> target19 = ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site179.Target;
				CallSite <>p__Site12 = ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site179;
				object arg9 = obj7;
				if (ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site17a == null)
				{
					ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site17a = CallSite<Func<CallSite, object, string, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.Equal, typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null)
					}));
				}
				Func<CallSite, object, string, object> target20 = ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site17a.Target;
				CallSite <>p__Site17a = ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site17a;
				if (ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site17b == null)
				{
					ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site17b = CallSite<Func<CallSite, Type, object, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.None, "ToString", null, typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.IsStaticType, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
					}));
				}
				Func<CallSite, Type, object, object> target21 = ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site17b.Target;
				CallSite <>p__Site17b = ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site17b;
				Type typeFromHandle4 = typeof(Convert);
				if (ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site17c == null)
				{
					ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site17c = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "ISSUETYPE", typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
					}));
				}
				arg10 = target19(<>p__Site12, arg9, target20(<>p__Site17a, target21(<>p__Site17b, typeFromHandle4, ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site17c.Target(ProductStructureView.<IsNeedLock>o__SiteContainer15b.<>p__Site17c, row)), "7"));
			}
			else
			{
				arg10 = obj7;
			}
			return target4(<>p__Site, arg10);
		}

		// Token: 0x06000C85 RID: 3205 RVA: 0x000957A4 File Offset: 0x000939A4
		private void SetRowsDefSelect()
		{
			if (!this.IsParentSPView)
			{
				return;
			}
			EntryEntity entryEntity = this.Model.BillBusinessInfo.GetEntryEntity("FBottomEntity");
			DynamicObjectCollection entityDataObject = this.View.Model.GetEntityDataObject(entryEntity);
			List<DynamicObject> list = (from o in entityDataObject
			orderby DataEntityExtend.GetDynamicValue<int>(o, "BomLevel", 0)
			select o).ToList<DynamicObject>();
			List<string> list2 = new List<string>();
			using (List<DynamicObject>.Enumerator enumerator = list.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					DynamicObject oneRow = enumerator.Current;
					string dynamicValue = DataEntityExtend.GetDynamicValue<string>(oneRow, "EntryId", null);
					int dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<int>(oneRow, "BomLevel", 0);
					bool dynamicObjectItemValue2 = DataEntityExtend.GetDynamicObjectItemValue<bool>(oneRow, "IsSkip", false);
					if (!this.IsNeedLock(oneRow) && dynamicObjectItemValue >= 1)
					{
						if (this.MemBomQueryOption.ExpandVirtualMaterial)
						{
							DynamicObject dynamicObject = (from w in list
							where DataEntityExtend.GetDynamicObjectItemValue<string>(w, "EntryId", null) == DataEntityExtend.GetDynamicObjectItemValue<string>(oneRow, "ParentEntryId", null)
							select w).FirstOrDefault<DynamicObject>();
							string item = (dynamicObject == null) ? string.Empty : DataEntityExtend.GetDynamicValue<string>(dynamicObject, "EntryId", null);
							if (list2.Contains(item))
							{
								list2.Add(dynamicValue);
							}
							else if (!dynamicObjectItemValue2)
							{
								oneRow["IsSelect"] = true;
								list2.Add(dynamicValue);
							}
						}
						else if (dynamicObjectItemValue == 1)
						{
							oneRow["IsSelect"] = true;
						}
					}
				}
			}
		}

		// Token: 0x06000C86 RID: 3206 RVA: 0x00095990 File Offset: 0x00093B90
		private bool IsVisualMtrl(DynamicObjectCollection fullRows, DynamicObject row)
		{
			if (DataEntityExtend.GetDynamicObjectItemValue<string>(DataEntityExtend.GetDynamicObjectItemValue<DynamicObjectCollection>(DataEntityExtend.GetDynamicObjectItemValue<DynamicObject>(row, "MaterialId", null), "MaterialBase", null)[0], "ErpClsID", null) == "5")
			{
				if (DataEntityExtend.GetDynamicObjectItemValue<long>(row, "BomLevel", 0L) == 1L)
				{
					return true;
				}
				this.IsVisualMtrl(fullRows, (from w in fullRows
				where DataEntityExtend.GetDynamicObjectItemValue<string>(w, "EntryId", null) == DataEntityExtend.GetDynamicObjectItemValue<string>(row, "ParentEntryId", null)
				select w).FirstOrDefault<DynamicObject>());
			}
			return false;
		}

		// Token: 0x06000C87 RID: 3207 RVA: 0x00095A24 File Offset: 0x00093C24
		private void ChangeDataRowColor(int row = -1)
		{
			int i = row;
			int num = i + 1;
			List<KeyValuePair<int, string>> list = new List<KeyValuePair<int, string>>();
			if (row == -1)
			{
				i = 0;
				num = this.Model.GetEntryRowCount("FBottomEntity");
			}
			while (i < num)
			{
				if (this.selBomBillParam != null && MFGBillUtil.GetValue<int>(this.Model, "FBomEntryId", i, 0, null) > 0 && ((MFGBillUtil.GetValue<DynamicObject>(this.Model, "FSupplyOrgId", i, null, null) != null && this.selBomBillParam.StockOrgId != Convert.ToInt64(MFGBillUtil.GetValue<DynamicObject>(this.Model, "FSupplyOrgId", i, null, null)["Id"])) || this.selBomBillParam.OwnerType != MFGBillUtil.GetValue<string>(this.Model, "FOWNERTYPEID", i, null, null) || (MFGBillUtil.GetValue<DynamicObject>(this.Model, "FOWNERID", i, null, null) != null && this.selBomBillParam.OwnerId != Convert.ToInt64(MFGBillUtil.GetValue<DynamicObject>(this.Model, "FOWNERID", i, null, null)["Id"]))))
				{
					list.Add(new KeyValuePair<int, string>(i, "#CCCCCC"));
				}
				i++;
			}
			this.View.GetControl<EntryGrid>("FBottomEntity").SetRowBackcolor(list);
		}

		// Token: 0x17000089 RID: 137
		// (get) Token: 0x06000C88 RID: 3208 RVA: 0x00095B5F File Offset: 0x00093D5F
		private bool IsParentMisAppView
		{
			get
			{
				return this.View.ParentFormView != null && this.View.ParentFormView.BillBusinessInfo.GetForm().Id == "STK_OutStockApply";
			}
		}

		// Token: 0x1700008A RID: 138
		// (get) Token: 0x06000C89 RID: 3209 RVA: 0x00095B94 File Offset: 0x00093D94
		private bool IsParentMisDelView
		{
			get
			{
				return this.View.ParentFormView != null && this.View.ParentFormView.BillBusinessInfo.GetForm().Id == "STK_MisDelivery";
			}
		}

		// Token: 0x1700008B RID: 139
		// (get) Token: 0x06000C8A RID: 3210 RVA: 0x00095BC9 File Offset: 0x00093DC9
		private bool IsParentMisCellView
		{
			get
			{
				return this.View.ParentFormView != null && this.View.ParentFormView.BillBusinessInfo.GetForm().Id == "STK_MISCELLANEOUS";
			}
		}

		// Token: 0x1700008C RID: 140
		// (get) Token: 0x06000C8B RID: 3211 RVA: 0x00095BFE File Offset: 0x00093DFE
		private bool IsParentTransAppView
		{
			get
			{
				return this.View.ParentFormView != null && this.View.ParentFormView.BillBusinessInfo.GetForm().Id == "STK_TRANSFERAPPLY";
			}
		}

		// Token: 0x1700008D RID: 141
		// (get) Token: 0x06000C8C RID: 3212 RVA: 0x00095C33 File Offset: 0x00093E33
		private bool IsParentTransDirectView
		{
			get
			{
				return this.View.ParentFormView != null && this.View.ParentFormView.BillBusinessInfo.GetForm().Id == "STK_TransferDirect";
			}
		}

		// Token: 0x1700008E RID: 142
		// (get) Token: 0x06000C8D RID: 3213 RVA: 0x00095C68 File Offset: 0x00093E68
		private bool IsParentTransOutView
		{
			get
			{
				return this.View.ParentFormView != null && this.View.ParentFormView.BillBusinessInfo.GetForm().Id == "STK_TRANSFEROUT";
			}
		}

		// Token: 0x1700008F RID: 143
		// (get) Token: 0x06000C8E RID: 3214 RVA: 0x00095C9D File Offset: 0x00093E9D
		private bool IsParentSTKView
		{
			get
			{
				return this.IsParentMisAppView || this.IsParentMisDelView || this.IsParentMisCellView || this.IsParentTransAppView || this.IsParentTransDirectView || this.IsParentTransOutView;
			}
		}

		// Token: 0x17000090 RID: 144
		// (get) Token: 0x06000C8F RID: 3215 RVA: 0x00095CCF File Offset: 0x00093ECF
		// (set) Token: 0x06000C90 RID: 3216 RVA: 0x00095CD7 File Offset: 0x00093ED7
		private SelBomBillParam selStkBomBillParam { get; set; }

		// Token: 0x06000C91 RID: 3217 RVA: 0x00095CE0 File Offset: 0x00093EE0
		private void OnInitialize_ForSTK(InitializeEventArgs e)
		{
			if (!this.IsParentSTKView)
			{
				return;
			}
			if (this.View.ParentFormView != null)
			{
				object obj;
				this.View.ParentFormView.Session.TryGetValue("SelInStockBillParam", out obj);
				if (obj != null && obj is SelBomBillParam)
				{
					this.selStkBomBillParam = (obj as SelBomBillParam);
					this.View.ParentFormView.Session["SelInStockBillParam"] = null;
				}
			}
			this.RegisterEntityRule_ForSTK();
		}

		// Token: 0x06000C92 RID: 3218 RVA: 0x00095D58 File Offset: 0x00093F58
		private void BeforeBindData_ForSTK(EventArgs e)
		{
			if (!this.IsParentSTKView)
			{
				return;
			}
			this.SelFieldsVisible(new List<string>
			{
				"FIsExpandPurchaseMtrl",
				"FIsExpandVirtualMtrl"
			}, true);
			this.SelFieldsVisible(new List<string>
			{
				"FIsShowPurchaseMtrl",
				"FExpandLevel",
				"FValidDate",
				"FWorkShopID",
				"FPrdOrgId",
				"FGenerateType",
				"FUnitId",
				"FMOBillType"
			}, false);
			if (this.selStkBomBillParam != null)
			{
				this.Model.SetValue("FUseOrgId", this.selStkBomBillParam.PrdOrgId);
				this._TargetOrgId = this.selStkBomBillParam.StockOrgId;
			}
		}

		// Token: 0x06000C93 RID: 3219 RVA: 0x00095E44 File Offset: 0x00094044
		private void EntityRowDoubleClick_ForSTK(EntityRowClickEventArgs e)
		{
			if (!this.IsParentSTKView)
			{
				return;
			}
			EntryEntity entryEntity = this.Model.BillBusinessInfo.GetEntryEntity("FBottomEntity");
			if (string.Equals(e.Key, entryEntity.Key, StringComparison.OrdinalIgnoreCase) && e.Row >= 0 && e.Row < this.View.Model.GetEntryRowCount(entryEntity.Key) && this.View.StyleManager.GetEnabled(this.View.BusinessInfo.GetField("FIsSelect"), this.Model.GetEntityDataObject(entryEntity, e.Row)))
			{
				List<DynamicObject> lstSelRows = (from w in new List<DynamicObject>
				{
					this.View.Model.GetEntityDataObject(entryEntity).ElementAt(e.Row)
				}
				where DataEntityExtend.GetDynamicObjectItemValue<long>(w, "BomEntryId", 0L) > 0L
				select w).ToList<DynamicObject>();
				this.ReturnDataToSTKParentBySelect(lstSelRows, false);
			}
		}

		// Token: 0x06000C94 RID: 3220 RVA: 0x00095F48 File Offset: 0x00094148
		private void BarItemClick_ForSTK(BarItemClickEventArgs e)
		{
			if (!this.IsParentSTKView)
			{
				return;
			}
			string barItemKey;
			if ((barItemKey = e.BarItemKey) != null)
			{
				if (barItemKey == "tbReturn")
				{
					this.ReturnDataForSTK();
					return;
				}
				if (!(barItemKey == "tbReturnAndClose"))
				{
					return;
				}
				this.ReturnDataForSTK();
				this.View.Close();
			}
		}

		// Token: 0x06000C95 RID: 3221 RVA: 0x00095F9C File Offset: 0x0009419C
		private void BeforeF7Select_ForSTK(BeforeF7SelectEventArgs e)
		{
			if (!this.IsParentSTKView)
			{
				return;
			}
			string fieldKey;
			if ((fieldKey = e.FieldKey) != null)
			{
				if (!(fieldKey == "FQkParentMaterialId"))
				{
					return;
				}
				BaseDataField baseDataField = ObjectUtils.CreateCopy(this.View.BillBusinessInfo.GetField("FMaterialId2")) as BaseDataField;
				if (baseDataField != null)
				{
					baseDataField.EntityKey = "FBillHead";
					baseDataField.Entity = this.View.BillBusinessInfo.GetEntity("FBillHead");
					baseDataField.OrgFieldKey = "FUseOrgId";
					string filter = StringUtils.JoinFilterString(baseDataField.Filter, " FIsMainPrd = '1' ", "AND");
					baseDataField.Filter = filter;
					MFGCommonUtil.DoF7ButtonClick(this.View, e.FieldKey, null, baseDataField, false, "FNumber", true, 1, 0);
				}
			}
		}

		// Token: 0x06000C96 RID: 3222 RVA: 0x0009605C File Offset: 0x0009425C
		private void DataChanged_ForSTK(DataChangedEventArgs e)
		{
			if (!this.IsParentSTKView)
			{
				return;
			}
			string key;
			if ((key = e.Field.Key) != null)
			{
				if (!(key == "FQkParentMaterialId"))
				{
					return;
				}
				BaseDataField field = this.View.BillBusinessInfo.GetField("FMaterialId2") as BaseDataField;
				DynamicObjectCollection dynamicObjectCollection = null;
				if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(e.NewValue))
				{
					dynamicObjectCollection = this.LoadReference(field, e.NewValue.ToString(), "BD_MATERIAL", new List<string>
					{
						"fmaterialid"
					}, " FIsMainPrd = '1' ", false);
				}
				if (dynamicObjectCollection != null && dynamicObjectCollection.Count<DynamicObject>() == 1)
				{
					this.HeadMtrlId = DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObjectCollection[0], "FMaterialId", 0L);
					this.View.Model.SetValue("FQkParentMaterialName", DataEntityExtend.GetDynamicObjectItemValue<string>(dynamicObjectCollection[0], "FName", null));
					this.View.Model.SetValue("FQkParentMaterialSpec", DataEntityExtend.GetDynamicObjectItemValue<string>(dynamicObjectCollection[0], "FSpecification", null));
					long num = DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObjectCollection[0], "FProduceUnitId", 0L);
					num = ((num > 0L) ? num : DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObjectCollection[0], "FStoreUnitID", 0L));
					this.View.Model.SetValue("FUnitId", num);
					long dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObjectCollection[0], "FMaterialId", 0L);
					long[] materialMasterAndUserOrgId = MaterialServiceHelper.GetMaterialMasterAndUserOrgId(base.Context, dynamicObjectItemValue);
					long defaultBomKey = BOMServiceHelper.GetDefaultBomKey(this.View.Context, materialMasterAndUserOrgId[0], materialMasterAndUserOrgId[1], 0L, 2);
					DynamicObject bomObjById = this.GetBomObjById(defaultBomKey);
					if (bomObjById != null)
					{
						this.View.Model.SetValue("FQkBomId", DataEntityExtend.GetDynamicObjectItemValue<string>(bomObjById, "Number", null));
						string empty = string.Empty;
						DataEntityExtend.GetDynamicObjectItemValue<LocaleValue>(bomObjById, "Name", null).TryGetValue(base.Context.UserLocale.LCID, ref empty);
						this.View.Model.SetValue("FQkBomName", empty);
					}
				}
			}
		}

		// Token: 0x06000C97 RID: 3223 RVA: 0x0009628C File Offset: 0x0009448C
		private void ReturnDataForSTK()
		{
			EntryEntity entryEntity = this.Model.BillBusinessInfo.GetEntryEntity("FBottomEntity");
			List<DynamicObject> lstSelRows = (from w in this.View.Model.GetEntityDataObject(entryEntity)
			where DataEntityExtend.GetDynamicObjectItemValue<bool>(w, "IsSelect", false) && DataEntityExtend.GetDynamicObjectItemValue<long>(w, "BomEntryId", 0L) > 0L
			select w).ToList<DynamicObject>();
			this.ReturnDataToSTKParentBySelect(lstSelRows, false);
		}

		// Token: 0x06000C98 RID: 3224 RVA: 0x000962F0 File Offset: 0x000944F0
		private void ReturnDataToSTKParentBySelect(List<DynamicObject> lstSelRows, bool isReturnAndClose = false)
		{
			if (ListUtils.IsEmpty<DynamicObject>(lstSelRows) || this.MemBomQueryOption == null)
			{
				this.View.ShowMessage(ResManager.LoadKDString("请选择分录！", "015072000002224", 7, new object[0]), 0);
				return;
			}
			SelBill0ption selBill0ption = new SelBill0ption
			{
				FormId = this.View.BusinessInfo.GetForm().Id,
				ListSelRows = lstSelRows
			};
			if (this.View.ParentFormView != null && this.View.ParentFormView is IDynamicFormViewService)
			{
				this.View.ReturnToParentWindow(selBill0ption);
				(this.View.ParentFormView as IDynamicFormViewService).CustomEvents(this.View.PageId, "CustomSelBill", "returnData");
				this.View.SendDynamicFormAction(this.View.ParentFormView);
				if (isReturnAndClose)
				{
					this.View.Close();
				}
			}
		}

		// Token: 0x06000C99 RID: 3225 RVA: 0x000963D4 File Offset: 0x000945D4
		private void RegisterEntityRule_ForSTK()
		{
			if (!this.IsParentSTKView)
			{
				return;
			}
			this.View.RuleContainer.AddPluginRule("FBottomEntity", 7, new Action<DynamicObject, object>(this.LockCheckBox_ForSTK), new string[]
			{
				"FBomLevel",
				"FBomEntryId",
				"FMaterialId2",
				"FBomId2",
				"FISSUETYPE"
			});
			this.View.RuleContainer.AddPluginRule("FBottomEntity", 7, new Action<DynamicObject, object>(this.LockCheckBoxByMaterialTypeForSTK), new string[]
			{
				"FMaterialType"
			});
		}

		// Token: 0x06000C9A RID: 3226 RVA: 0x00096470 File Offset: 0x00094670
		private void LockCheckBox_ForSTK(DynamicObject dataEntity, dynamic row)
		{
			if (ProductStructureView.<LockCheckBox_ForSTK>o__SiteContainer190.<>p__Site191 == null)
			{
				ProductStructureView.<LockCheckBox_ForSTK>o__SiteContainer190.<>p__Site191 = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsTrue, typeof(ProductStructureView), new CSharpArgumentInfo[]
				{
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
				}));
			}
			Func<CallSite, object, bool> target = ProductStructureView.<LockCheckBox_ForSTK>o__SiteContainer190.<>p__Site191.Target;
			CallSite <>p__Site = ProductStructureView.<LockCheckBox_ForSTK>o__SiteContainer190.<>p__Site191;
			if (ProductStructureView.<LockCheckBox_ForSTK>o__SiteContainer190.<>p__Site192 == null)
			{
				ProductStructureView.<LockCheckBox_ForSTK>o__SiteContainer190.<>p__Site192 = CallSite<Func<CallSite, ProductStructureView, object, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.InvokeSimpleName, "IsNeedLockForSTK", null, typeof(ProductStructureView), new CSharpArgumentInfo[]
				{
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
				}));
			}
			Func<CallSite, ProductStructureView, object, object> target2 = ProductStructureView.<LockCheckBox_ForSTK>o__SiteContainer190.<>p__Site192.Target;
			CallSite <>p__Site2 = ProductStructureView.<LockCheckBox_ForSTK>o__SiteContainer190.<>p__Site192;
			if (ProductStructureView.<LockCheckBox_ForSTK>o__SiteContainer190.<>p__Site193 == null)
			{
				ProductStructureView.<LockCheckBox_ForSTK>o__SiteContainer190.<>p__Site193 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "ActiveObject", typeof(ProductStructureView), new CSharpArgumentInfo[]
				{
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
				}));
			}
			if (target(<>p__Site, target2(<>p__Site2, this, ProductStructureView.<LockCheckBox_ForSTK>o__SiteContainer190.<>p__Site193.Target(ProductStructureView.<LockCheckBox_ForSTK>o__SiteContainer190.<>p__Site193, row))))
			{
				Field field = this.View.BusinessInfo.GetField("FIsSelect");
				if (ProductStructureView.<LockCheckBox_ForSTK>o__SiteContainer190.<>p__Site194 == null)
				{
					ProductStructureView.<LockCheckBox_ForSTK>o__SiteContainer190.<>p__Site194 = CallSite<Action<CallSite, IStyleManager, Field, object, string, bool>>.Create(Binder.InvokeMember(CSharpBinderFlags.ResultDiscarded, "SetEnabled", null, typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null)
					}));
				}
				Action<CallSite, IStyleManager, Field, object, string, bool> target3 = ProductStructureView.<LockCheckBox_ForSTK>o__SiteContainer190.<>p__Site194.Target;
				CallSite <>p__Site3 = ProductStructureView.<LockCheckBox_ForSTK>o__SiteContainer190.<>p__Site194;
				IStyleManager styleManager = this.View.StyleManager;
				Field arg = field;
				if (ProductStructureView.<LockCheckBox_ForSTK>o__SiteContainer190.<>p__Site195 == null)
				{
					ProductStructureView.<LockCheckBox_ForSTK>o__SiteContainer190.<>p__Site195 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "ActiveObject", typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
					}));
				}
				target3(<>p__Site3, styleManager, arg, ProductStructureView.<LockCheckBox_ForSTK>o__SiteContainer190.<>p__Site195.Target(ProductStructureView.<LockCheckBox_ForSTK>o__SiteContainer190.<>p__Site195, row), field.PropertyName, false);
			}
		}

		// Token: 0x06000C9B RID: 3227 RVA: 0x0009666C File Offset: 0x0009486C
		private void LockCheckBoxByMaterialTypeForSTK(DynamicObject dataEntity, dynamic row)
		{
			if (ProductStructureView.<LockCheckBoxByMaterialTypeForSTK>o__SiteContainer196.<>p__Site197 == null)
			{
				ProductStructureView.<LockCheckBoxByMaterialTypeForSTK>o__SiteContainer196.<>p__Site197 = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsTrue, typeof(ProductStructureView), new CSharpArgumentInfo[]
				{
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
				}));
			}
			Func<CallSite, object, bool> target = ProductStructureView.<LockCheckBoxByMaterialTypeForSTK>o__SiteContainer196.<>p__Site197.Target;
			CallSite <>p__Site = ProductStructureView.<LockCheckBoxByMaterialTypeForSTK>o__SiteContainer196.<>p__Site197;
			if (ProductStructureView.<LockCheckBoxByMaterialTypeForSTK>o__SiteContainer196.<>p__Site198 == null)
			{
				ProductStructureView.<LockCheckBoxByMaterialTypeForSTK>o__SiteContainer196.<>p__Site198 = CallSite<Func<CallSite, ProductStructureView, object, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.InvokeSimpleName, "IsNeedLockForSTK", null, typeof(ProductStructureView), new CSharpArgumentInfo[]
				{
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
				}));
			}
			Func<CallSite, ProductStructureView, object, object> target2 = ProductStructureView.<LockCheckBoxByMaterialTypeForSTK>o__SiteContainer196.<>p__Site198.Target;
			CallSite <>p__Site2 = ProductStructureView.<LockCheckBoxByMaterialTypeForSTK>o__SiteContainer196.<>p__Site198;
			if (ProductStructureView.<LockCheckBoxByMaterialTypeForSTK>o__SiteContainer196.<>p__Site199 == null)
			{
				ProductStructureView.<LockCheckBoxByMaterialTypeForSTK>o__SiteContainer196.<>p__Site199 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "ActiveObject", typeof(ProductStructureView), new CSharpArgumentInfo[]
				{
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
				}));
			}
			if (target(<>p__Site, target2(<>p__Site2, this, ProductStructureView.<LockCheckBoxByMaterialTypeForSTK>o__SiteContainer196.<>p__Site199.Target(ProductStructureView.<LockCheckBoxByMaterialTypeForSTK>o__SiteContainer196.<>p__Site199, row))))
			{
				Field field = this.View.BusinessInfo.GetField("FIsSelect");
				if (ProductStructureView.<LockCheckBoxByMaterialTypeForSTK>o__SiteContainer196.<>p__Site19a == null)
				{
					ProductStructureView.<LockCheckBoxByMaterialTypeForSTK>o__SiteContainer196.<>p__Site19a = CallSite<Action<CallSite, IStyleManager, Field, object, string, bool>>.Create(Binder.InvokeMember(CSharpBinderFlags.ResultDiscarded, "SetEnabled", null, typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null)
					}));
				}
				Action<CallSite, IStyleManager, Field, object, string, bool> target3 = ProductStructureView.<LockCheckBoxByMaterialTypeForSTK>o__SiteContainer196.<>p__Site19a.Target;
				CallSite <>p__Site19a = ProductStructureView.<LockCheckBoxByMaterialTypeForSTK>o__SiteContainer196.<>p__Site19a;
				IStyleManager styleManager = this.View.StyleManager;
				Field arg = field;
				if (ProductStructureView.<LockCheckBoxByMaterialTypeForSTK>o__SiteContainer196.<>p__Site19b == null)
				{
					ProductStructureView.<LockCheckBoxByMaterialTypeForSTK>o__SiteContainer196.<>p__Site19b = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "ActiveObject", typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
					}));
				}
				target3(<>p__Site19a, styleManager, arg, ProductStructureView.<LockCheckBoxByMaterialTypeForSTK>o__SiteContainer196.<>p__Site19b.Target(ProductStructureView.<LockCheckBoxByMaterialTypeForSTK>o__SiteContainer196.<>p__Site19b, row), field.PropertyName, false);
			}
		}

		// Token: 0x06000C9C RID: 3228 RVA: 0x00096868 File Offset: 0x00094A68
		private bool IsNeedLockForSTK(dynamic row)
		{
			if (ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site19d == null)
			{
				ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site19d = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsTrue, typeof(ProductStructureView), new CSharpArgumentInfo[]
				{
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
				}));
			}
			Func<CallSite, object, bool> target = ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site19d.Target;
			CallSite <>p__Site19d = ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site19d;
			bool isParentSTKView = this.IsParentSTKView;
			object arg2;
			if (isParentSTKView)
			{
				if (ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site19e == null)
				{
					ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site19e = CallSite<Func<CallSite, bool, object, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.BinaryOperationLogical, ExpressionType.And, typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
					}));
				}
				Func<CallSite, bool, object, object> target2 = ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site19e.Target;
				CallSite <>p__Site19e = ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site19e;
				bool arg = isParentSTKView;
				if (ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site19f == null)
				{
					ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site19f = CallSite<Func<CallSite, object, string, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.Equal, typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null)
					}));
				}
				Func<CallSite, object, string, object> target3 = ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site19f.Target;
				CallSite <>p__Site19f = ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site19f;
				if (ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1a0 == null)
				{
					ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1a0 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "MaterialType", typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
					}));
				}
				arg2 = target2(<>p__Site19e, arg, target3(<>p__Site19f, ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1a0.Target(ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1a0, row), Convert.ToString(2)));
			}
			else
			{
				arg2 = isParentSTKView;
			}
			if (target(<>p__Site19d, arg2))
			{
				return true;
			}
			if (ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1a1 == null)
			{
				ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1a1 = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsTrue, typeof(ProductStructureView), new CSharpArgumentInfo[]
				{
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
				}));
			}
			Func<CallSite, object, bool> target4 = ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1a1.Target;
			CallSite <>p__Site1a = ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1a1;
			if (ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1a2 == null)
			{
				ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1a2 = CallSite<Func<CallSite, object, string, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.Equal, typeof(ProductStructureView), new CSharpArgumentInfo[]
				{
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null)
				}));
			}
			Func<CallSite, object, string, object> target5 = ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1a2.Target;
			CallSite <>p__Site1a2 = ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1a2;
			if (ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1a3 == null)
			{
				ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1a3 = CallSite<Func<CallSite, Type, object, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.None, "ToString", null, typeof(ProductStructureView), new CSharpArgumentInfo[]
				{
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.IsStaticType, null),
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
				}));
			}
			Func<CallSite, Type, object, object> target6 = ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1a3.Target;
			CallSite <>p__Site1a3 = ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1a3;
			Type typeFromHandle = typeof(Convert);
			if (ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1a4 == null)
			{
				ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1a4 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "BomLevel", typeof(ProductStructureView), new CSharpArgumentInfo[]
				{
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
				}));
			}
			object obj = target5(<>p__Site1a2, target6(<>p__Site1a3, typeFromHandle, ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1a4.Target(ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1a4, row)), "0");
			if (ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1a5 == null)
			{
				ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1a5 = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsFalse, typeof(ProductStructureView), new CSharpArgumentInfo[]
				{
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
				}));
			}
			object obj2;
			if (!ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1a5.Target(ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1a5, obj))
			{
				if (ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1a6 == null)
				{
					ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1a6 = CallSite<Func<CallSite, object, object, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.BinaryOperationLogical, ExpressionType.And, typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
					}));
				}
				Func<CallSite, object, object, object> target7 = ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1a6.Target;
				CallSite <>p__Site1a4 = ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1a6;
				object arg3 = obj;
				if (ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1a7 == null)
				{
					ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1a7 = CallSite<Func<CallSite, object, int, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.LessThanOrEqual, typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null)
					}));
				}
				Func<CallSite, object, int, object> target8 = ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1a7.Target;
				CallSite <>p__Site1a5 = ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1a7;
				if (ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1a8 == null)
				{
					ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1a8 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "BomEntryId", typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
					}));
				}
				obj2 = target7(<>p__Site1a4, arg3, target8(<>p__Site1a5, ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1a8.Target(ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1a8, row), 0));
			}
			else
			{
				obj2 = obj;
			}
			object obj3 = obj2;
			if (ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1a9 == null)
			{
				ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1a9 = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsTrue, typeof(ProductStructureView), new CSharpArgumentInfo[]
				{
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
				}));
			}
			object arg9;
			if (!ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1a9.Target(ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1a9, obj3))
			{
				if (ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1aa == null)
				{
					ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1aa = CallSite<Func<CallSite, object, object, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.BinaryOperationLogical, ExpressionType.Or, typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
					}));
				}
				Func<CallSite, object, object, object> target9 = ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1aa.Target;
				CallSite <>p__Site1aa = ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1aa;
				object arg4 = obj3;
				if (ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1ab == null)
				{
					ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1ab = CallSite<Func<CallSite, object, object, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.NotEqual, typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.Constant, null)
					}));
				}
				Func<CallSite, object, object, object> target10 = ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1ab.Target;
				CallSite <>p__Site1ab = ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1ab;
				if (ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1ac == null)
				{
					ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1ac = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "MaterialId", typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
					}));
				}
				object obj4 = target10(<>p__Site1ab, ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1ac.Target(ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1ac, row), null);
				if (ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1ad == null)
				{
					ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1ad = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsFalse, typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
					}));
				}
				object arg8;
				if (!ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1ad.Target(ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1ad, obj4))
				{
					if (ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1ae == null)
					{
						ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1ae = CallSite<Func<CallSite, object, object, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.BinaryOperationLogical, ExpressionType.And, typeof(ProductStructureView), new CSharpArgumentInfo[]
						{
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
						}));
					}
					Func<CallSite, object, object, object> target11 = ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1ae.Target;
					CallSite <>p__Site1ae = ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1ae;
					object arg5 = obj4;
					if (ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1af == null)
					{
						ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1af = CallSite<Func<CallSite, object, string, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.NotEqual, typeof(ProductStructureView), new CSharpArgumentInfo[]
						{
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null)
						}));
					}
					Func<CallSite, object, string, object> target12 = ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1af.Target;
					CallSite <>p__Site1af = ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1af;
					if (ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1b0 == null)
					{
						ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1b0 = CallSite<Func<CallSite, Type, object, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.None, "ToString", null, typeof(ProductStructureView), new CSharpArgumentInfo[]
						{
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.IsStaticType, null),
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
						}));
					}
					Func<CallSite, Type, object, object> target13 = ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1b0.Target;
					CallSite <>p__Site1b = ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1b0;
					Type typeFromHandle2 = typeof(Convert);
					if (ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1b1 == null)
					{
						ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1b1 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "ForbidStatus", typeof(ProductStructureView), new CSharpArgumentInfo[]
						{
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
						}));
					}
					Func<CallSite, object, object> target14 = ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1b1.Target;
					CallSite <>p__Site1b2 = ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1b1;
					if (ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1b2 == null)
					{
						ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1b2 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "MaterialId", typeof(ProductStructureView), new CSharpArgumentInfo[]
						{
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
						}));
					}
					object obj5 = target12(<>p__Site1af, target13(<>p__Site1b, typeFromHandle2, target14(<>p__Site1b2, ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1b2.Target(ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1b2, row))), "A");
					if (ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1b3 == null)
					{
						ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1b3 = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsTrue, typeof(ProductStructureView), new CSharpArgumentInfo[]
						{
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
						}));
					}
					object arg7;
					if (!ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1b3.Target(ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1b3, obj5))
					{
						if (ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1b4 == null)
						{
							ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1b4 = CallSite<Func<CallSite, object, object, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.BinaryOperationLogical, ExpressionType.Or, typeof(ProductStructureView), new CSharpArgumentInfo[]
							{
								CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
								CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
							}));
						}
						Func<CallSite, object, object, object> target15 = ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1b4.Target;
						CallSite <>p__Site1b3 = ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1b4;
						object arg6 = obj5;
						if (ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1b5 == null)
						{
							ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1b5 = CallSite<Func<CallSite, object, string, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.NotEqual, typeof(ProductStructureView), new CSharpArgumentInfo[]
							{
								CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
								CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null)
							}));
						}
						Func<CallSite, object, string, object> target16 = ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1b5.Target;
						CallSite <>p__Site1b4 = ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1b5;
						if (ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1b6 == null)
						{
							ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1b6 = CallSite<Func<CallSite, Type, object, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.None, "ToString", null, typeof(ProductStructureView), new CSharpArgumentInfo[]
							{
								CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.IsStaticType, null),
								CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
							}));
						}
						Func<CallSite, Type, object, object> target17 = ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1b6.Target;
						CallSite <>p__Site1b5 = ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1b6;
						Type typeFromHandle3 = typeof(Convert);
						if (ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1b7 == null)
						{
							ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1b7 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "DocumentStatus", typeof(ProductStructureView), new CSharpArgumentInfo[]
							{
								CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
							}));
						}
						Func<CallSite, object, object> target18 = ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1b7.Target;
						CallSite <>p__Site1b6 = ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1b7;
						if (ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1b8 == null)
						{
							ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1b8 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "MaterialId", typeof(ProductStructureView), new CSharpArgumentInfo[]
							{
								CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
							}));
						}
						arg7 = target15(<>p__Site1b3, arg6, target16(<>p__Site1b4, target17(<>p__Site1b5, typeFromHandle3, target18(<>p__Site1b6, ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1b8.Target(ProductStructureView.<IsNeedLockForSTK>o__SiteContainer19c.<>p__Site1b8, row))), "C"));
					}
					else
					{
						arg7 = obj5;
					}
					arg8 = target11(<>p__Site1ae, arg5, arg7);
				}
				else
				{
					arg8 = obj4;
				}
				arg9 = target9(<>p__Site1aa, arg4, arg8);
			}
			else
			{
				arg9 = obj3;
			}
			return target4(<>p__Site1a, arg9);
		}

		// Token: 0x06000C9D RID: 3229 RVA: 0x00097210 File Offset: 0x00095410
		private bool IsNotNeedSelectForSTK(dynamic row)
		{
			if (ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1ba == null)
			{
				ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1ba = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsTrue, typeof(ProductStructureView), new CSharpArgumentInfo[]
				{
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
				}));
			}
			Func<CallSite, object, bool> target = ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1ba.Target;
			CallSite <>p__Site1ba = ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1ba;
			bool isParentSTKView = this.IsParentSTKView;
			object arg2;
			if (isParentSTKView)
			{
				if (ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1bb == null)
				{
					ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1bb = CallSite<Func<CallSite, bool, object, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.BinaryOperationLogical, ExpressionType.And, typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
					}));
				}
				Func<CallSite, bool, object, object> target2 = ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1bb.Target;
				CallSite <>p__Site1bb = ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1bb;
				bool arg = isParentSTKView;
				if (ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1bc == null)
				{
					ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1bc = CallSite<Func<CallSite, object, string, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.Equal, typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null)
					}));
				}
				Func<CallSite, object, string, object> target3 = ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1bc.Target;
				CallSite <>p__Site1bc = ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1bc;
				if (ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1bd == null)
				{
					ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1bd = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "MaterialType", typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
					}));
				}
				arg2 = target2(<>p__Site1bb, arg, target3(<>p__Site1bc, ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1bd.Target(ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1bd, row), Convert.ToString(2)));
			}
			else
			{
				arg2 = isParentSTKView;
			}
			if (target(<>p__Site1ba, arg2))
			{
				return true;
			}
			if (ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1be == null)
			{
				ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1be = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsTrue, typeof(ProductStructureView), new CSharpArgumentInfo[]
				{
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
				}));
			}
			Func<CallSite, object, bool> target4 = ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1be.Target;
			CallSite <>p__Site1be = ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1be;
			if (ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1bf == null)
			{
				ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1bf = CallSite<Func<CallSite, object, string, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.Equal, typeof(ProductStructureView), new CSharpArgumentInfo[]
				{
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null)
				}));
			}
			Func<CallSite, object, string, object> target5 = ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1bf.Target;
			CallSite <>p__Site1bf = ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1bf;
			if (ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1c0 == null)
			{
				ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1c0 = CallSite<Func<CallSite, Type, object, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.None, "ToString", null, typeof(ProductStructureView), new CSharpArgumentInfo[]
				{
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.IsStaticType, null),
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
				}));
			}
			Func<CallSite, Type, object, object> target6 = ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1c0.Target;
			CallSite <>p__Site1c = ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1c0;
			Type typeFromHandle = typeof(Convert);
			if (ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1c1 == null)
			{
				ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1c1 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "BomLevel", typeof(ProductStructureView), new CSharpArgumentInfo[]
				{
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
				}));
			}
			object obj = target5(<>p__Site1bf, target6(<>p__Site1c, typeFromHandle, ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1c1.Target(ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1c1, row)), "0");
			if (ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1c2 == null)
			{
				ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1c2 = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsFalse, typeof(ProductStructureView), new CSharpArgumentInfo[]
				{
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
				}));
			}
			object obj2;
			if (!ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1c2.Target(ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1c2, obj))
			{
				if (ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1c3 == null)
				{
					ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1c3 = CallSite<Func<CallSite, object, object, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.BinaryOperationLogical, ExpressionType.And, typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
					}));
				}
				Func<CallSite, object, object, object> target7 = ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1c3.Target;
				CallSite <>p__Site1c2 = ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1c3;
				object arg3 = obj;
				if (ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1c4 == null)
				{
					ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1c4 = CallSite<Func<CallSite, object, int, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.LessThanOrEqual, typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null)
					}));
				}
				Func<CallSite, object, int, object> target8 = ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1c4.Target;
				CallSite <>p__Site1c3 = ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1c4;
				if (ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1c5 == null)
				{
					ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1c5 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "BomEntryId", typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
					}));
				}
				obj2 = target7(<>p__Site1c2, arg3, target8(<>p__Site1c3, ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1c5.Target(ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1c5, row), 0));
			}
			else
			{
				obj2 = obj;
			}
			object obj3 = obj2;
			if (ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1c6 == null)
			{
				ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1c6 = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsTrue, typeof(ProductStructureView), new CSharpArgumentInfo[]
				{
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
				}));
			}
			object obj6;
			if (!ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1c6.Target(ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1c6, obj3))
			{
				if (ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1c7 == null)
				{
					ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1c7 = CallSite<Func<CallSite, object, object, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.BinaryOperationLogical, ExpressionType.Or, typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
					}));
				}
				Func<CallSite, object, object, object> target9 = ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1c7.Target;
				CallSite <>p__Site1c4 = ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1c7;
				object arg4 = obj3;
				if (ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1c8 == null)
				{
					ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1c8 = CallSite<Func<CallSite, object, object, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.NotEqual, typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.Constant, null)
					}));
				}
				Func<CallSite, object, object, object> target10 = ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1c8.Target;
				CallSite <>p__Site1c5 = ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1c8;
				if (ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1c9 == null)
				{
					ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1c9 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "MaterialId", typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
					}));
				}
				object obj4 = target10(<>p__Site1c5, ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1c9.Target(ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1c9, row), null);
				if (ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1ca == null)
				{
					ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1ca = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsFalse, typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
					}));
				}
				object arg8;
				if (!ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1ca.Target(ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1ca, obj4))
				{
					if (ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1cb == null)
					{
						ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1cb = CallSite<Func<CallSite, object, object, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.BinaryOperationLogical, ExpressionType.And, typeof(ProductStructureView), new CSharpArgumentInfo[]
						{
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
						}));
					}
					Func<CallSite, object, object, object> target11 = ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1cb.Target;
					CallSite <>p__Site1cb = ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1cb;
					object arg5 = obj4;
					if (ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1cc == null)
					{
						ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1cc = CallSite<Func<CallSite, object, string, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.NotEqual, typeof(ProductStructureView), new CSharpArgumentInfo[]
						{
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null)
						}));
					}
					Func<CallSite, object, string, object> target12 = ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1cc.Target;
					CallSite <>p__Site1cc = ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1cc;
					if (ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1cd == null)
					{
						ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1cd = CallSite<Func<CallSite, Type, object, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.None, "ToString", null, typeof(ProductStructureView), new CSharpArgumentInfo[]
						{
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.IsStaticType, null),
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
						}));
					}
					Func<CallSite, Type, object, object> target13 = ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1cd.Target;
					CallSite <>p__Site1cd = ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1cd;
					Type typeFromHandle2 = typeof(Convert);
					if (ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1ce == null)
					{
						ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1ce = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "ForbidStatus", typeof(ProductStructureView), new CSharpArgumentInfo[]
						{
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
						}));
					}
					Func<CallSite, object, object> target14 = ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1ce.Target;
					CallSite <>p__Site1ce = ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1ce;
					if (ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1cf == null)
					{
						ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1cf = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "MaterialId", typeof(ProductStructureView), new CSharpArgumentInfo[]
						{
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
						}));
					}
					object obj5 = target12(<>p__Site1cc, target13(<>p__Site1cd, typeFromHandle2, target14(<>p__Site1ce, ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1cf.Target(ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1cf, row))), "A");
					if (ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1d0 == null)
					{
						ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1d0 = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsTrue, typeof(ProductStructureView), new CSharpArgumentInfo[]
						{
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
						}));
					}
					object arg7;
					if (!ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1d0.Target(ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1d0, obj5))
					{
						if (ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1d1 == null)
						{
							ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1d1 = CallSite<Func<CallSite, object, object, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.BinaryOperationLogical, ExpressionType.Or, typeof(ProductStructureView), new CSharpArgumentInfo[]
							{
								CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
								CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
							}));
						}
						Func<CallSite, object, object, object> target15 = ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1d1.Target;
						CallSite <>p__Site1d = ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1d1;
						object arg6 = obj5;
						if (ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1d2 == null)
						{
							ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1d2 = CallSite<Func<CallSite, object, string, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.NotEqual, typeof(ProductStructureView), new CSharpArgumentInfo[]
							{
								CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
								CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null)
							}));
						}
						Func<CallSite, object, string, object> target16 = ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1d2.Target;
						CallSite <>p__Site1d2 = ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1d2;
						if (ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1d3 == null)
						{
							ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1d3 = CallSite<Func<CallSite, Type, object, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.None, "ToString", null, typeof(ProductStructureView), new CSharpArgumentInfo[]
							{
								CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.IsStaticType, null),
								CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
							}));
						}
						Func<CallSite, Type, object, object> target17 = ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1d3.Target;
						CallSite <>p__Site1d3 = ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1d3;
						Type typeFromHandle3 = typeof(Convert);
						if (ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1d4 == null)
						{
							ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1d4 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "DocumentStatus", typeof(ProductStructureView), new CSharpArgumentInfo[]
							{
								CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
							}));
						}
						Func<CallSite, object, object> target18 = ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1d4.Target;
						CallSite <>p__Site1d4 = ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1d4;
						if (ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1d5 == null)
						{
							ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1d5 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "MaterialId", typeof(ProductStructureView), new CSharpArgumentInfo[]
							{
								CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
							}));
						}
						arg7 = target15(<>p__Site1d, arg6, target16(<>p__Site1d2, target17(<>p__Site1d3, typeFromHandle3, target18(<>p__Site1d4, ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1d5.Target(ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1d5, row))), "C"));
					}
					else
					{
						arg7 = obj5;
					}
					arg8 = target11(<>p__Site1cb, arg5, arg7);
				}
				else
				{
					arg8 = obj4;
				}
				obj6 = target9(<>p__Site1c4, arg4, arg8);
			}
			else
			{
				obj6 = obj3;
			}
			object obj7 = obj6;
			if (ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1d6 == null)
			{
				ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1d6 = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsTrue, typeof(ProductStructureView), new CSharpArgumentInfo[]
				{
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
				}));
			}
			object arg10;
			if (!ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1d6.Target(ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1d6, obj7))
			{
				if (ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1d7 == null)
				{
					ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1d7 = CallSite<Func<CallSite, object, object, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.BinaryOperationLogical, ExpressionType.Or, typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
					}));
				}
				Func<CallSite, object, object, object> target19 = ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1d7.Target;
				CallSite <>p__Site1d5 = ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1d7;
				object arg9 = obj7;
				if (ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1d8 == null)
				{
					ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1d8 = CallSite<Func<CallSite, object, string, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.Equal, typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null)
					}));
				}
				Func<CallSite, object, string, object> target20 = ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1d8.Target;
				CallSite <>p__Site1d6 = ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1d8;
				if (ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1d9 == null)
				{
					ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1d9 = CallSite<Func<CallSite, Type, object, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.None, "ToString", null, typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.IsStaticType, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
					}));
				}
				Func<CallSite, Type, object, object> target21 = ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1d9.Target;
				CallSite <>p__Site1d7 = ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1d9;
				Type typeFromHandle4 = typeof(Convert);
				if (ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1da == null)
				{
					ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1da = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "ISSUETYPE", typeof(ProductStructureView), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
					}));
				}
				arg10 = target19(<>p__Site1d5, arg9, target20(<>p__Site1d6, target21(<>p__Site1d7, typeFromHandle4, ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1da.Target(ProductStructureView.<IsNotNeedSelectForSTK>o__SiteContainer1b9.<>p__Site1da, row)), "7"));
			}
			else
			{
				arg10 = obj7;
			}
			return target4(<>p__Site1be, arg10);
		}

		// Token: 0x06000C9E RID: 3230 RVA: 0x00097DB4 File Offset: 0x00095FB4
		private void SetRowsDefSelectForSTK()
		{
			if (!this.IsParentSTKView)
			{
				return;
			}
			EntryEntity entryEntity = this.Model.BillBusinessInfo.GetEntryEntity("FBottomEntity");
			DynamicObjectCollection entityDataObject = this.View.Model.GetEntityDataObject(entryEntity);
			List<DynamicObject> list = (from o in entityDataObject
			orderby DataEntityExtend.GetDynamicValue<int>(o, "BomLevel", 0)
			select o).ToList<DynamicObject>();
			List<string> list2 = new List<string>();
			using (List<DynamicObject>.Enumerator enumerator = list.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					DynamicObject oneRow = enumerator.Current;
					string dynamicValue = DataEntityExtend.GetDynamicValue<string>(oneRow, "EntryId", null);
					int dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<int>(oneRow, "BomLevel", 0);
					bool dynamicObjectItemValue2 = DataEntityExtend.GetDynamicObjectItemValue<bool>(oneRow, "IsSkip", false);
					if (!this.IsNotNeedSelectForSTK(oneRow) && dynamicObjectItemValue >= 1)
					{
						if (this.MemBomQueryOption.ExpandVirtualMaterial)
						{
							DynamicObject dynamicObject = (from w in list
							where DataEntityExtend.GetDynamicObjectItemValue<string>(w, "EntryId", null) == DataEntityExtend.GetDynamicObjectItemValue<string>(oneRow, "ParentEntryId", null)
							select w).FirstOrDefault<DynamicObject>();
							string item = (dynamicObject == null) ? string.Empty : DataEntityExtend.GetDynamicValue<string>(dynamicObject, "EntryId", null);
							if (list2.Contains(item))
							{
								list2.Add(dynamicValue);
							}
							else if (!dynamicObjectItemValue2)
							{
								oneRow["IsSelect"] = true;
								list2.Add(dynamicValue);
							}
						}
						else if (dynamicObjectItemValue == 1)
						{
							oneRow["IsSelect"] = true;
						}
					}
				}
			}
		}

		// Token: 0x06000C9F RID: 3231 RVA: 0x00097F74 File Offset: 0x00096174
		private void ChangeDataRowColorForSTK(int row = -1)
		{
			int i = row;
			int num = i + 1;
			List<KeyValuePair<int, string>> list = new List<KeyValuePair<int, string>>();
			if (row == -1)
			{
				i = 0;
				num = this.Model.GetEntryRowCount("FBottomEntity");
			}
			while (i < num)
			{
				if (this.selBomBillParam != null && MFGBillUtil.GetValue<int>(this.Model, "FBomEntryId", i, 0, null) > 0 && ((MFGBillUtil.GetValue<DynamicObject>(this.Model, "FSupplyOrgId", i, null, null) != null && this.selBomBillParam.StockOrgId != Convert.ToInt64(MFGBillUtil.GetValue<DynamicObject>(this.Model, "FSupplyOrgId", i, null, null)["Id"])) || this.selBomBillParam.OwnerType != MFGBillUtil.GetValue<string>(this.Model, "FOWNERTYPEID", i, null, null) || (MFGBillUtil.GetValue<DynamicObject>(this.Model, "FOWNERID", i, null, null) != null && this.selBomBillParam.OwnerId != Convert.ToInt64(MFGBillUtil.GetValue<DynamicObject>(this.Model, "FOWNERID", i, null, null)["Id"]))))
				{
					list.Add(new KeyValuePair<int, string>(i, "#CCCCCC"));
				}
				i++;
			}
			this.View.GetControl<EntryGrid>("FBottomEntity").SetRowBackcolor(list);
		}

		// Token: 0x0400059D RID: 1437
		private const string COLOR_YELLOW = "#F8F88B";

		// Token: 0x0400059E RID: 1438
		private const string PARENTVIEW_PARAMNAME = "SelInStockBillParam";

		// Token: 0x0400059F RID: 1439
		private long _TargetOrgId;

		// Token: 0x040005A0 RID: 1440
		private long HeadMtrlId;

		// Token: 0x040005A1 RID: 1441
		protected List<DynamicObject> bomQueryChildItems = new List<DynamicObject>();

		// Token: 0x040005A2 RID: 1442
		private bool _isDoPush = true;

		// Token: 0x040005A3 RID: 1443
		private List<DynamicObject> lstErrorBomItems = new List<DynamicObject>();

		// Token: 0x040005A4 RID: 1444
		private List<ValidationErrorInfo> splitErrors = new List<ValidationErrorInfo>();

		// Token: 0x020001C5 RID: 453
		[CompilerGenerated]
		private static class <BeforeUpdateValue_ForMo>o__SiteContainer72
		{
			// Token: 0x040008D8 RID: 2264
			public static CallSite<Func<CallSite, object, bool>> <>p__Site73;

			// Token: 0x040008D9 RID: 2265
			public static CallSite<Func<CallSite, object, object>> <>p__Site74;

			// Token: 0x040008DA RID: 2266
			public static CallSite<Func<CallSite, object, object>> <>p__Site75;

			// Token: 0x040008DB RID: 2267
			public static CallSite<Func<CallSite, object, int, object>> <>p__Site76;

			// Token: 0x040008DC RID: 2268
			public static CallSite<Func<CallSite, object, object>> <>p__Site77;

			// Token: 0x040008DD RID: 2269
			public static CallSite<Func<CallSite, object, bool>> <>p__Site78;

			// Token: 0x040008DE RID: 2270
			public static CallSite<Func<CallSite, object, object, object>> <>p__Site79;

			// Token: 0x040008DF RID: 2271
			public static CallSite<Func<CallSite, object, string, object>> <>p__Site7a;

			// Token: 0x040008E0 RID: 2272
			public static CallSite<Func<CallSite, object, object>> <>p__Site7b;

			// Token: 0x040008E1 RID: 2273
			public static CallSite<Func<CallSite, object, int, object>> <>p__Site7c;

			// Token: 0x040008E2 RID: 2274
			public static CallSite<Func<CallSite, object, object>> <>p__Site7d;

			// Token: 0x040008E3 RID: 2275
			public static CallSite<Func<CallSite, object, object>> <>p__Site7e;

			// Token: 0x040008E4 RID: 2276
			public static CallSite<Action<CallSite, IDynamicFormView, object>> <>p__Site7f;

			// Token: 0x040008E5 RID: 2277
			public static CallSite<Func<CallSite, Type, string, object, object>> <>p__Site80;

			// Token: 0x040008E6 RID: 2278
			public static CallSite<Func<CallSite, object, bool>> <>p__Site81;

			// Token: 0x040008E7 RID: 2279
			public static CallSite<Func<CallSite, object, object>> <>p__Site82;

			// Token: 0x040008E8 RID: 2280
			public static CallSite<Func<CallSite, object, object>> <>p__Site83;

			// Token: 0x040008E9 RID: 2281
			public static CallSite<Func<CallSite, object, int, object>> <>p__Site84;

			// Token: 0x040008EA RID: 2282
			public static CallSite<Func<CallSite, object, object>> <>p__Site85;

			// Token: 0x040008EB RID: 2283
			public static CallSite<Func<CallSite, object, bool>> <>p__Site86;

			// Token: 0x040008EC RID: 2284
			public static CallSite<Func<CallSite, object, object, object>> <>p__Site87;

			// Token: 0x040008ED RID: 2285
			public static CallSite<Func<CallSite, object, string, object>> <>p__Site88;

			// Token: 0x040008EE RID: 2286
			public static CallSite<Func<CallSite, object, object>> <>p__Site89;

			// Token: 0x040008EF RID: 2287
			public static CallSite<Func<CallSite, object, int, object>> <>p__Site8a;

			// Token: 0x040008F0 RID: 2288
			public static CallSite<Func<CallSite, object, object>> <>p__Site8b;

			// Token: 0x040008F1 RID: 2289
			public static CallSite<Func<CallSite, object, object>> <>p__Site8c;

			// Token: 0x040008F2 RID: 2290
			public static CallSite<Action<CallSite, IDynamicFormView, object>> <>p__Site8d;

			// Token: 0x040008F3 RID: 2291
			public static CallSite<Func<CallSite, Type, string, object, object>> <>p__Site8e;
		}

		// Token: 0x020001C6 RID: 454
		[CompilerGenerated]
		private static class <RegisterEntityRule_ForPrdMo>o__SiteContainer8f
		{
			// Token: 0x040008F4 RID: 2292
			public static CallSite<Action<CallSite, IStyleManager, string, object, object>> <>p__Site90;

			// Token: 0x040008F5 RID: 2293
			public static CallSite<Func<CallSite, object, object>> <>p__Site91;

			// Token: 0x040008F6 RID: 2294
			public static CallSite<Func<CallSite, object, object>> <>p__Site92;

			// Token: 0x040008F7 RID: 2295
			public static CallSite<Func<CallSite, object, object>> <>p__Site93;

			// Token: 0x040008F8 RID: 2296
			public static CallSite<Func<CallSite, object, bool>> <>p__Site94;

			// Token: 0x040008F9 RID: 2297
			public static CallSite<Func<CallSite, object, object>> <>p__Site95;

			// Token: 0x040008FA RID: 2298
			public static CallSite<Func<CallSite, object, object>> <>p__Site96;

			// Token: 0x040008FB RID: 2299
			public static CallSite<Func<CallSite, object, bool, object>> <>p__Site97;

			// Token: 0x040008FC RID: 2300
			public static CallSite<Func<CallSite, object, object>> <>p__Site98;

			// Token: 0x040008FD RID: 2301
			public static CallSite<Func<CallSite, object, bool>> <>p__Site99;

			// Token: 0x040008FE RID: 2302
			public static CallSite<Func<CallSite, object, object>> <>p__Site9a;

			// Token: 0x040008FF RID: 2303
			public static CallSite<Func<CallSite, object, object>> <>p__Site9b;
		}

		// Token: 0x020001C7 RID: 455
		[CompilerGenerated]
		private static class <LockCheckBox_ForPrdMo>o__SiteContainer9d
		{
			// Token: 0x04000900 RID: 2304
			public static CallSite<Func<CallSite, object, bool>> <>p__Site9e;

			// Token: 0x04000901 RID: 2305
			public static CallSite<Func<CallSite, object, object, object>> <>p__Site9f;

			// Token: 0x04000902 RID: 2306
			public static CallSite<Func<CallSite, object, object>> <>p__Sitea0;

			// Token: 0x04000903 RID: 2307
			public static CallSite<Func<CallSite, object, object>> <>p__Sitea1;

			// Token: 0x04000904 RID: 2308
			public static CallSite<Func<CallSite, object, bool>> <>p__Sitea2;

			// Token: 0x04000905 RID: 2309
			public static CallSite<Func<CallSite, object, object, object>> <>p__Sitea3;

			// Token: 0x04000906 RID: 2310
			public static CallSite<Func<CallSite, object, string, object>> <>p__Sitea4;

			// Token: 0x04000907 RID: 2311
			public static CallSite<Func<CallSite, Type, object, object>> <>p__Sitea5;

			// Token: 0x04000908 RID: 2312
			public static CallSite<Func<CallSite, object, object>> <>p__Sitea6;

			// Token: 0x04000909 RID: 2313
			public static CallSite<Func<CallSite, object, object>> <>p__Sitea7;

			// Token: 0x0400090A RID: 2314
			public static CallSite<Func<CallSite, object, object>> <>p__Sitea8;

			// Token: 0x0400090B RID: 2315
			public static CallSite<Func<CallSite, object, bool>> <>p__Sitea9;

			// Token: 0x0400090C RID: 2316
			public static CallSite<Func<CallSite, object, object, object>> <>p__Siteaa;

			// Token: 0x0400090D RID: 2317
			public static CallSite<Func<CallSite, object, string, object>> <>p__Siteab;

			// Token: 0x0400090E RID: 2318
			public static CallSite<Func<CallSite, Type, object, object>> <>p__Siteac;

			// Token: 0x0400090F RID: 2319
			public static CallSite<Func<CallSite, object, object>> <>p__Sitead;

			// Token: 0x04000910 RID: 2320
			public static CallSite<Func<CallSite, object, object>> <>p__Siteae;

			// Token: 0x04000911 RID: 2321
			public static CallSite<Func<CallSite, object, object>> <>p__Siteaf;

			// Token: 0x04000912 RID: 2322
			public static CallSite<Func<CallSite, object, bool>> <>p__Siteb0;

			// Token: 0x04000913 RID: 2323
			public static CallSite<Func<CallSite, object, object, object>> <>p__Siteb1;

			// Token: 0x04000914 RID: 2324
			public static CallSite<Func<CallSite, object, object>> <>p__Siteb2;

			// Token: 0x04000915 RID: 2325
			public static CallSite<Func<CallSite, object, object>> <>p__Siteb3;

			// Token: 0x04000916 RID: 2326
			public static CallSite<Func<CallSite, object, int, object>> <>p__Siteb4;

			// Token: 0x04000917 RID: 2327
			public static CallSite<Func<CallSite, object, object>> <>p__Siteb5;

			// Token: 0x04000918 RID: 2328
			public static CallSite<Func<CallSite, object, object>> <>p__Siteb6;

			// Token: 0x04000919 RID: 2329
			public static CallSite<Func<CallSite, object, object>> <>p__Siteb7;

			// Token: 0x0400091A RID: 2330
			public static CallSite<Func<CallSite, object, bool>> <>p__Siteb8;

			// Token: 0x0400091B RID: 2331
			public static CallSite<Func<CallSite, object, object, object>> <>p__Siteb9;

			// Token: 0x0400091C RID: 2332
			public static CallSite<Func<CallSite, object, object>> <>p__Siteba;

			// Token: 0x0400091D RID: 2333
			public static CallSite<Func<CallSite, object, object>> <>p__Sitebb;

			// Token: 0x0400091E RID: 2334
			public static CallSite<Func<CallSite, object, int, object>> <>p__Sitebc;

			// Token: 0x0400091F RID: 2335
			public static CallSite<Func<CallSite, object, object>> <>p__Sitebd;

			// Token: 0x04000920 RID: 2336
			public static CallSite<Func<CallSite, object, object>> <>p__Sitebe;

			// Token: 0x04000921 RID: 2337
			public static CallSite<Func<CallSite, object, object>> <>p__Sitebf;

			// Token: 0x04000922 RID: 2338
			public static CallSite<Func<CallSite, object, bool>> <>p__Sitec0;

			// Token: 0x04000923 RID: 2339
			public static CallSite<Func<CallSite, object, object, object>> <>p__Sitec1;

			// Token: 0x04000924 RID: 2340
			public static CallSite<Func<CallSite, object, string, object>> <>p__Sitec2;

			// Token: 0x04000925 RID: 2341
			public static CallSite<Func<CallSite, object, object>> <>p__Sitec3;

			// Token: 0x04000926 RID: 2342
			public static CallSite<Func<CallSite, object, object>> <>p__Sitec4;

			// Token: 0x04000927 RID: 2343
			public static CallSite<Func<CallSite, object, bool>> <>p__Sitec5;

			// Token: 0x04000928 RID: 2344
			public static CallSite<Func<CallSite, object, object, object>> <>p__Sitec6;

			// Token: 0x04000929 RID: 2345
			public static CallSite<Func<CallSite, object, string, object>> <>p__Sitec7;

			// Token: 0x0400092A RID: 2346
			public static CallSite<Func<CallSite, object, object>> <>p__Sitec8;

			// Token: 0x0400092B RID: 2347
			public static CallSite<Func<CallSite, object, object>> <>p__Sitec9;

			// Token: 0x0400092C RID: 2348
			public static CallSite<Action<CallSite, IStyleManager, Field, object, string, bool>> <>p__Siteca;

			// Token: 0x0400092D RID: 2349
			public static CallSite<Func<CallSite, object, object>> <>p__Sitecb;

			// Token: 0x0400092E RID: 2350
			public static CallSite<Action<CallSite, IStyleManager, Field, object, string, bool>> <>p__Sitecc;

			// Token: 0x0400092F RID: 2351
			public static CallSite<Func<CallSite, object, object>> <>p__Sitecd;

			// Token: 0x04000930 RID: 2352
			public static CallSite<Func<CallSite, object, bool>> <>p__Sitece;

			// Token: 0x04000931 RID: 2353
			public static CallSite<Func<CallSite, ProductStructureView, object, object>> <>p__Sitecf;

			// Token: 0x04000932 RID: 2354
			public static CallSite<Func<CallSite, object, object>> <>p__Sited0;

			// Token: 0x04000933 RID: 2355
			public static CallSite<Action<CallSite, IStyleManager, Field, object, string, bool>> <>p__Sited1;

			// Token: 0x04000934 RID: 2356
			public static CallSite<Func<CallSite, object, object>> <>p__Sited2;
		}

		// Token: 0x020001D0 RID: 464
		[CompilerGenerated]
		private static class <IsNeedLock_ForMo>o__SiteContainer101
		{
			// Token: 0x0400093F RID: 2367
			public static CallSite<Func<CallSite, object, bool>> <>p__Site102;

			// Token: 0x04000940 RID: 2368
			public static CallSite<Func<CallSite, object, string, object>> <>p__Site103;

			// Token: 0x04000941 RID: 2369
			public static CallSite<Func<CallSite, Type, object, object>> <>p__Site104;

			// Token: 0x04000942 RID: 2370
			public static CallSite<Func<CallSite, object, object>> <>p__Site105;

			// Token: 0x04000943 RID: 2371
			public static CallSite<Func<CallSite, object, bool>> <>p__Site106;

			// Token: 0x04000944 RID: 2372
			public static CallSite<Func<CallSite, object, object, object>> <>p__Site107;

			// Token: 0x04000945 RID: 2373
			public static CallSite<Func<CallSite, object, int, object>> <>p__Site108;

			// Token: 0x04000946 RID: 2374
			public static CallSite<Func<CallSite, object, object>> <>p__Site109;
		}

		// Token: 0x020001D1 RID: 465
		[CompilerGenerated]
		private static class <LockCheckBox_ForSAL>o__SiteContainer113
		{
			// Token: 0x04000947 RID: 2375
			public static CallSite<Func<CallSite, object, bool>> <>p__Site114;

			// Token: 0x04000948 RID: 2376
			public static CallSite<Func<CallSite, ProductStructureView, object, object>> <>p__Site115;

			// Token: 0x04000949 RID: 2377
			public static CallSite<Func<CallSite, object, object>> <>p__Site116;

			// Token: 0x0400094A RID: 2378
			public static CallSite<Action<CallSite, IStyleManager, Field, object, string, bool>> <>p__Site117;

			// Token: 0x0400094B RID: 2379
			public static CallSite<Func<CallSite, object, object>> <>p__Site118;
		}

		// Token: 0x020001D2 RID: 466
		[CompilerGenerated]
		private static class <LockCheckBoxByMaterialTypeForSAL>o__SiteContainer119
		{
			// Token: 0x0400094C RID: 2380
			public static CallSite<Func<CallSite, object, bool>> <>p__Site11a;

			// Token: 0x0400094D RID: 2381
			public static CallSite<Func<CallSite, ProductStructureView, object, object>> <>p__Site11b;

			// Token: 0x0400094E RID: 2382
			public static CallSite<Func<CallSite, object, object>> <>p__Site11c;

			// Token: 0x0400094F RID: 2383
			public static CallSite<Action<CallSite, IStyleManager, Field, object, string, bool>> <>p__Site11d;

			// Token: 0x04000950 RID: 2384
			public static CallSite<Func<CallSite, object, object>> <>p__Site11e;
		}

		// Token: 0x020001D3 RID: 467
		[CompilerGenerated]
		private static class <IsNeedLockForSAL>o__SiteContainer11f
		{
			// Token: 0x04000951 RID: 2385
			public static CallSite<Func<CallSite, object, bool>> <>p__Site120;

			// Token: 0x04000952 RID: 2386
			public static CallSite<Func<CallSite, bool, object, object>> <>p__Site121;

			// Token: 0x04000953 RID: 2387
			public static CallSite<Func<CallSite, object, string, object>> <>p__Site122;

			// Token: 0x04000954 RID: 2388
			public static CallSite<Func<CallSite, object, object>> <>p__Site123;

			// Token: 0x04000955 RID: 2389
			public static CallSite<Func<CallSite, object, bool>> <>p__Site124;

			// Token: 0x04000956 RID: 2390
			public static CallSite<Func<CallSite, object, string, object>> <>p__Site125;

			// Token: 0x04000957 RID: 2391
			public static CallSite<Func<CallSite, Type, object, object>> <>p__Site126;

			// Token: 0x04000958 RID: 2392
			public static CallSite<Func<CallSite, object, object>> <>p__Site127;

			// Token: 0x04000959 RID: 2393
			public static CallSite<Func<CallSite, object, bool>> <>p__Site128;

			// Token: 0x0400095A RID: 2394
			public static CallSite<Func<CallSite, object, object, object>> <>p__Site129;

			// Token: 0x0400095B RID: 2395
			public static CallSite<Func<CallSite, object, int, object>> <>p__Site12a;

			// Token: 0x0400095C RID: 2396
			public static CallSite<Func<CallSite, object, object>> <>p__Site12b;

			// Token: 0x0400095D RID: 2397
			public static CallSite<Func<CallSite, object, bool>> <>p__Site12c;

			// Token: 0x0400095E RID: 2398
			public static CallSite<Func<CallSite, object, object, object>> <>p__Site12d;

			// Token: 0x0400095F RID: 2399
			public static CallSite<Func<CallSite, object, object, object>> <>p__Site12e;

			// Token: 0x04000960 RID: 2400
			public static CallSite<Func<CallSite, object, object>> <>p__Site12f;

			// Token: 0x04000961 RID: 2401
			public static CallSite<Func<CallSite, object, bool>> <>p__Site130;

			// Token: 0x04000962 RID: 2402
			public static CallSite<Func<CallSite, object, object, object>> <>p__Site131;

			// Token: 0x04000963 RID: 2403
			public static CallSite<Func<CallSite, object, string, object>> <>p__Site132;

			// Token: 0x04000964 RID: 2404
			public static CallSite<Func<CallSite, Type, object, object>> <>p__Site133;

			// Token: 0x04000965 RID: 2405
			public static CallSite<Func<CallSite, object, object>> <>p__Site134;

			// Token: 0x04000966 RID: 2406
			public static CallSite<Func<CallSite, object, object>> <>p__Site135;

			// Token: 0x04000967 RID: 2407
			public static CallSite<Func<CallSite, object, bool>> <>p__Site136;

			// Token: 0x04000968 RID: 2408
			public static CallSite<Func<CallSite, object, object, object>> <>p__Site137;

			// Token: 0x04000969 RID: 2409
			public static CallSite<Func<CallSite, object, string, object>> <>p__Site138;

			// Token: 0x0400096A RID: 2410
			public static CallSite<Func<CallSite, Type, object, object>> <>p__Site139;

			// Token: 0x0400096B RID: 2411
			public static CallSite<Func<CallSite, object, object>> <>p__Site13a;

			// Token: 0x0400096C RID: 2412
			public static CallSite<Func<CallSite, object, object>> <>p__Site13b;

			// Token: 0x0400096D RID: 2413
			public static CallSite<Func<CallSite, object, bool>> <>p__Site13c;

			// Token: 0x0400096E RID: 2414
			public static CallSite<Func<CallSite, object, object, object>> <>p__Site13d;

			// Token: 0x0400096F RID: 2415
			public static CallSite<Func<CallSite, object, string, object>> <>p__Site13e;

			// Token: 0x04000970 RID: 2416
			public static CallSite<Func<CallSite, Type, object, object>> <>p__Site13f;

			// Token: 0x04000971 RID: 2417
			public static CallSite<Func<CallSite, object, object>> <>p__Site140;
		}

		// Token: 0x020001D5 RID: 469
		[CompilerGenerated]
		private static class <LockCheckBoxByMaterialType>o__SiteContainer14f
		{
			// Token: 0x04000973 RID: 2419
			public static CallSite<Func<CallSite, object, bool>> <>p__Site150;

			// Token: 0x04000974 RID: 2420
			public static CallSite<Func<CallSite, ProductStructureView, object, object>> <>p__Site151;

			// Token: 0x04000975 RID: 2421
			public static CallSite<Func<CallSite, object, object>> <>p__Site152;

			// Token: 0x04000976 RID: 2422
			public static CallSite<Action<CallSite, IStyleManager, Field, object, string, bool>> <>p__Site153;

			// Token: 0x04000977 RID: 2423
			public static CallSite<Func<CallSite, object, object>> <>p__Site154;
		}

		// Token: 0x020001D6 RID: 470
		[CompilerGenerated]
		private static class <LockCheckBox_ForSP>o__SiteContainer155
		{
			// Token: 0x04000978 RID: 2424
			public static CallSite<Func<CallSite, object, bool>> <>p__Site156;

			// Token: 0x04000979 RID: 2425
			public static CallSite<Func<CallSite, ProductStructureView, object, object>> <>p__Site157;

			// Token: 0x0400097A RID: 2426
			public static CallSite<Func<CallSite, object, object>> <>p__Site158;

			// Token: 0x0400097B RID: 2427
			public static CallSite<Action<CallSite, IStyleManager, Field, object, string, bool>> <>p__Site159;

			// Token: 0x0400097C RID: 2428
			public static CallSite<Func<CallSite, object, object>> <>p__Site15a;
		}

		// Token: 0x020001D7 RID: 471
		[CompilerGenerated]
		private static class <IsNeedLock>o__SiteContainer15b
		{
			// Token: 0x0400097D RID: 2429
			public static CallSite<Func<CallSite, object, bool>> <>p__Site15c;

			// Token: 0x0400097E RID: 2430
			public static CallSite<Func<CallSite, bool, object, object>> <>p__Site15d;

			// Token: 0x0400097F RID: 2431
			public static CallSite<Func<CallSite, object, string, object>> <>p__Site15e;

			// Token: 0x04000980 RID: 2432
			public static CallSite<Func<CallSite, object, object>> <>p__Site15f;

			// Token: 0x04000981 RID: 2433
			public static CallSite<Func<CallSite, object, bool>> <>p__Site160;

			// Token: 0x04000982 RID: 2434
			public static CallSite<Func<CallSite, object, string, object>> <>p__Site161;

			// Token: 0x04000983 RID: 2435
			public static CallSite<Func<CallSite, Type, object, object>> <>p__Site162;

			// Token: 0x04000984 RID: 2436
			public static CallSite<Func<CallSite, object, object>> <>p__Site163;

			// Token: 0x04000985 RID: 2437
			public static CallSite<Func<CallSite, object, bool>> <>p__Site164;

			// Token: 0x04000986 RID: 2438
			public static CallSite<Func<CallSite, object, object, object>> <>p__Site165;

			// Token: 0x04000987 RID: 2439
			public static CallSite<Func<CallSite, object, int, object>> <>p__Site166;

			// Token: 0x04000988 RID: 2440
			public static CallSite<Func<CallSite, object, object>> <>p__Site167;

			// Token: 0x04000989 RID: 2441
			public static CallSite<Func<CallSite, object, bool>> <>p__Site168;

			// Token: 0x0400098A RID: 2442
			public static CallSite<Func<CallSite, object, object, object>> <>p__Site169;

			// Token: 0x0400098B RID: 2443
			public static CallSite<Func<CallSite, object, object, object>> <>p__Site16a;

			// Token: 0x0400098C RID: 2444
			public static CallSite<Func<CallSite, object, object>> <>p__Site16b;

			// Token: 0x0400098D RID: 2445
			public static CallSite<Func<CallSite, object, bool>> <>p__Site16c;

			// Token: 0x0400098E RID: 2446
			public static CallSite<Func<CallSite, object, object, object>> <>p__Site16d;

			// Token: 0x0400098F RID: 2447
			public static CallSite<Func<CallSite, object, string, object>> <>p__Site16e;

			// Token: 0x04000990 RID: 2448
			public static CallSite<Func<CallSite, Type, object, object>> <>p__Site16f;

			// Token: 0x04000991 RID: 2449
			public static CallSite<Func<CallSite, object, object>> <>p__Site170;

			// Token: 0x04000992 RID: 2450
			public static CallSite<Func<CallSite, object, object>> <>p__Site171;

			// Token: 0x04000993 RID: 2451
			public static CallSite<Func<CallSite, object, bool>> <>p__Site172;

			// Token: 0x04000994 RID: 2452
			public static CallSite<Func<CallSite, object, object, object>> <>p__Site173;

			// Token: 0x04000995 RID: 2453
			public static CallSite<Func<CallSite, object, string, object>> <>p__Site174;

			// Token: 0x04000996 RID: 2454
			public static CallSite<Func<CallSite, Type, object, object>> <>p__Site175;

			// Token: 0x04000997 RID: 2455
			public static CallSite<Func<CallSite, object, object>> <>p__Site176;

			// Token: 0x04000998 RID: 2456
			public static CallSite<Func<CallSite, object, object>> <>p__Site177;

			// Token: 0x04000999 RID: 2457
			public static CallSite<Func<CallSite, object, bool>> <>p__Site178;

			// Token: 0x0400099A RID: 2458
			public static CallSite<Func<CallSite, object, object, object>> <>p__Site179;

			// Token: 0x0400099B RID: 2459
			public static CallSite<Func<CallSite, object, string, object>> <>p__Site17a;

			// Token: 0x0400099C RID: 2460
			public static CallSite<Func<CallSite, Type, object, object>> <>p__Site17b;

			// Token: 0x0400099D RID: 2461
			public static CallSite<Func<CallSite, object, object>> <>p__Site17c;
		}

		// Token: 0x020001DA RID: 474
		[CompilerGenerated]
		private static class <LockCheckBox_ForSTK>o__SiteContainer190
		{
			// Token: 0x040009A0 RID: 2464
			public static CallSite<Func<CallSite, object, bool>> <>p__Site191;

			// Token: 0x040009A1 RID: 2465
			public static CallSite<Func<CallSite, ProductStructureView, object, object>> <>p__Site192;

			// Token: 0x040009A2 RID: 2466
			public static CallSite<Func<CallSite, object, object>> <>p__Site193;

			// Token: 0x040009A3 RID: 2467
			public static CallSite<Action<CallSite, IStyleManager, Field, object, string, bool>> <>p__Site194;

			// Token: 0x040009A4 RID: 2468
			public static CallSite<Func<CallSite, object, object>> <>p__Site195;
		}

		// Token: 0x020001DB RID: 475
		[CompilerGenerated]
		private static class <LockCheckBoxByMaterialTypeForSTK>o__SiteContainer196
		{
			// Token: 0x040009A5 RID: 2469
			public static CallSite<Func<CallSite, object, bool>> <>p__Site197;

			// Token: 0x040009A6 RID: 2470
			public static CallSite<Func<CallSite, ProductStructureView, object, object>> <>p__Site198;

			// Token: 0x040009A7 RID: 2471
			public static CallSite<Func<CallSite, object, object>> <>p__Site199;

			// Token: 0x040009A8 RID: 2472
			public static CallSite<Action<CallSite, IStyleManager, Field, object, string, bool>> <>p__Site19a;

			// Token: 0x040009A9 RID: 2473
			public static CallSite<Func<CallSite, object, object>> <>p__Site19b;
		}

		// Token: 0x020001DC RID: 476
		[CompilerGenerated]
		private static class <IsNeedLockForSTK>o__SiteContainer19c
		{
			// Token: 0x040009AA RID: 2474
			public static CallSite<Func<CallSite, object, bool>> <>p__Site19d;

			// Token: 0x040009AB RID: 2475
			public static CallSite<Func<CallSite, bool, object, object>> <>p__Site19e;

			// Token: 0x040009AC RID: 2476
			public static CallSite<Func<CallSite, object, string, object>> <>p__Site19f;

			// Token: 0x040009AD RID: 2477
			public static CallSite<Func<CallSite, object, object>> <>p__Site1a0;

			// Token: 0x040009AE RID: 2478
			public static CallSite<Func<CallSite, object, bool>> <>p__Site1a1;

			// Token: 0x040009AF RID: 2479
			public static CallSite<Func<CallSite, object, string, object>> <>p__Site1a2;

			// Token: 0x040009B0 RID: 2480
			public static CallSite<Func<CallSite, Type, object, object>> <>p__Site1a3;

			// Token: 0x040009B1 RID: 2481
			public static CallSite<Func<CallSite, object, object>> <>p__Site1a4;

			// Token: 0x040009B2 RID: 2482
			public static CallSite<Func<CallSite, object, bool>> <>p__Site1a5;

			// Token: 0x040009B3 RID: 2483
			public static CallSite<Func<CallSite, object, object, object>> <>p__Site1a6;

			// Token: 0x040009B4 RID: 2484
			public static CallSite<Func<CallSite, object, int, object>> <>p__Site1a7;

			// Token: 0x040009B5 RID: 2485
			public static CallSite<Func<CallSite, object, object>> <>p__Site1a8;

			// Token: 0x040009B6 RID: 2486
			public static CallSite<Func<CallSite, object, bool>> <>p__Site1a9;

			// Token: 0x040009B7 RID: 2487
			public static CallSite<Func<CallSite, object, object, object>> <>p__Site1aa;

			// Token: 0x040009B8 RID: 2488
			public static CallSite<Func<CallSite, object, object, object>> <>p__Site1ab;

			// Token: 0x040009B9 RID: 2489
			public static CallSite<Func<CallSite, object, object>> <>p__Site1ac;

			// Token: 0x040009BA RID: 2490
			public static CallSite<Func<CallSite, object, bool>> <>p__Site1ad;

			// Token: 0x040009BB RID: 2491
			public static CallSite<Func<CallSite, object, object, object>> <>p__Site1ae;

			// Token: 0x040009BC RID: 2492
			public static CallSite<Func<CallSite, object, string, object>> <>p__Site1af;

			// Token: 0x040009BD RID: 2493
			public static CallSite<Func<CallSite, Type, object, object>> <>p__Site1b0;

			// Token: 0x040009BE RID: 2494
			public static CallSite<Func<CallSite, object, object>> <>p__Site1b1;

			// Token: 0x040009BF RID: 2495
			public static CallSite<Func<CallSite, object, object>> <>p__Site1b2;

			// Token: 0x040009C0 RID: 2496
			public static CallSite<Func<CallSite, object, bool>> <>p__Site1b3;

			// Token: 0x040009C1 RID: 2497
			public static CallSite<Func<CallSite, object, object, object>> <>p__Site1b4;

			// Token: 0x040009C2 RID: 2498
			public static CallSite<Func<CallSite, object, string, object>> <>p__Site1b5;

			// Token: 0x040009C3 RID: 2499
			public static CallSite<Func<CallSite, Type, object, object>> <>p__Site1b6;

			// Token: 0x040009C4 RID: 2500
			public static CallSite<Func<CallSite, object, object>> <>p__Site1b7;

			// Token: 0x040009C5 RID: 2501
			public static CallSite<Func<CallSite, object, object>> <>p__Site1b8;
		}

		// Token: 0x020001DD RID: 477
		[CompilerGenerated]
		private static class <IsNotNeedSelectForSTK>o__SiteContainer1b9
		{
			// Token: 0x040009C6 RID: 2502
			public static CallSite<Func<CallSite, object, bool>> <>p__Site1ba;

			// Token: 0x040009C7 RID: 2503
			public static CallSite<Func<CallSite, bool, object, object>> <>p__Site1bb;

			// Token: 0x040009C8 RID: 2504
			public static CallSite<Func<CallSite, object, string, object>> <>p__Site1bc;

			// Token: 0x040009C9 RID: 2505
			public static CallSite<Func<CallSite, object, object>> <>p__Site1bd;

			// Token: 0x040009CA RID: 2506
			public static CallSite<Func<CallSite, object, bool>> <>p__Site1be;

			// Token: 0x040009CB RID: 2507
			public static CallSite<Func<CallSite, object, string, object>> <>p__Site1bf;

			// Token: 0x040009CC RID: 2508
			public static CallSite<Func<CallSite, Type, object, object>> <>p__Site1c0;

			// Token: 0x040009CD RID: 2509
			public static CallSite<Func<CallSite, object, object>> <>p__Site1c1;

			// Token: 0x040009CE RID: 2510
			public static CallSite<Func<CallSite, object, bool>> <>p__Site1c2;

			// Token: 0x040009CF RID: 2511
			public static CallSite<Func<CallSite, object, object, object>> <>p__Site1c3;

			// Token: 0x040009D0 RID: 2512
			public static CallSite<Func<CallSite, object, int, object>> <>p__Site1c4;

			// Token: 0x040009D1 RID: 2513
			public static CallSite<Func<CallSite, object, object>> <>p__Site1c5;

			// Token: 0x040009D2 RID: 2514
			public static CallSite<Func<CallSite, object, bool>> <>p__Site1c6;

			// Token: 0x040009D3 RID: 2515
			public static CallSite<Func<CallSite, object, object, object>> <>p__Site1c7;

			// Token: 0x040009D4 RID: 2516
			public static CallSite<Func<CallSite, object, object, object>> <>p__Site1c8;

			// Token: 0x040009D5 RID: 2517
			public static CallSite<Func<CallSite, object, object>> <>p__Site1c9;

			// Token: 0x040009D6 RID: 2518
			public static CallSite<Func<CallSite, object, bool>> <>p__Site1ca;

			// Token: 0x040009D7 RID: 2519
			public static CallSite<Func<CallSite, object, object, object>> <>p__Site1cb;

			// Token: 0x040009D8 RID: 2520
			public static CallSite<Func<CallSite, object, string, object>> <>p__Site1cc;

			// Token: 0x040009D9 RID: 2521
			public static CallSite<Func<CallSite, Type, object, object>> <>p__Site1cd;

			// Token: 0x040009DA RID: 2522
			public static CallSite<Func<CallSite, object, object>> <>p__Site1ce;

			// Token: 0x040009DB RID: 2523
			public static CallSite<Func<CallSite, object, object>> <>p__Site1cf;

			// Token: 0x040009DC RID: 2524
			public static CallSite<Func<CallSite, object, bool>> <>p__Site1d0;

			// Token: 0x040009DD RID: 2525
			public static CallSite<Func<CallSite, object, object, object>> <>p__Site1d1;

			// Token: 0x040009DE RID: 2526
			public static CallSite<Func<CallSite, object, string, object>> <>p__Site1d2;

			// Token: 0x040009DF RID: 2527
			public static CallSite<Func<CallSite, Type, object, object>> <>p__Site1d3;

			// Token: 0x040009E0 RID: 2528
			public static CallSite<Func<CallSite, object, object>> <>p__Site1d4;

			// Token: 0x040009E1 RID: 2529
			public static CallSite<Func<CallSite, object, object>> <>p__Site1d5;

			// Token: 0x040009E2 RID: 2530
			public static CallSite<Func<CallSite, object, bool>> <>p__Site1d6;

			// Token: 0x040009E3 RID: 2531
			public static CallSite<Func<CallSite, object, object, object>> <>p__Site1d7;

			// Token: 0x040009E4 RID: 2532
			public static CallSite<Func<CallSite, object, string, object>> <>p__Site1d8;

			// Token: 0x040009E5 RID: 2533
			public static CallSite<Func<CallSite, Type, object, object>> <>p__Site1d9;

			// Token: 0x040009E6 RID: 2534
			public static CallSite<Func<CallSite, object, object>> <>p__Site1da;
		}
	}
}
