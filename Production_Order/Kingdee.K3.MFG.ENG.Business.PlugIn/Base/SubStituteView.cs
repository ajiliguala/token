using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Kingdee.BOS;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.Core.MFG.Utils;
using Kingdee.K3.MFG.Common.BusinessEntity.ENG;
using Kingdee.K3.MFG.ServiceHelper;
using Kingdee.K3.MFG.ServiceHelper.ENG;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.Base
{
	// Token: 0x02000035 RID: 53
	[Description("替代方案设置视图插件")]
	public class SubStituteView : BaseControlEdit
	{
		// Token: 0x060003C2 RID: 962 RVA: 0x0002EB84 File Offset: 0x0002CD84
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
			if (base.View.ParentFormView != null)
			{
				object obj;
				base.View.ParentFormView.Session.TryGetValue("SelBomChItems", out obj);
				if (obj != null)
				{
					this.BomChItems = (obj as IEnumerable<DynamicObject>);
					base.View.ParentFormView.Session["SelBomChItems"] = null;
				}
			}
			base.View.SetFormTitle(new LocaleValue(ResManager.LoadKDString("替代设置", "015072000002184", 7, new object[0]), base.View.Context.UserLocale.LCID));
		}

		// Token: 0x060003C3 RID: 963 RVA: 0x0002EC40 File Offset: 0x0002CE40
		public override void BeforeBindData(EventArgs e)
		{
			base.BeforeBindData(e);
			if (this.BomChItems == null || this.BomChItems.Count<DynamicObject>() <= 0)
			{
				return;
			}
			base.View.Model.SetValue("FUseOrgId", DataEntityExtend.GetDynamicValue<long>(this.BomChItems.First<DynamicObject>().Parent as DynamicObject, "UseOrgId_Id", 0L));
			this.DeleteNullRows("FEntityMainItems", "EntityMainItems", "MaterialID_Id");
			bool flag = this.BomChItems.Any((DynamicObject w) => !ObjectUtils.IsNullOrEmptyOrWhiteSpace(DataEntityExtend.GetDynamicValue<string>(w, "ParentRowId", null)));
			if (flag)
			{
				this.DeleteNullRows("FEntity", "Entity", "SubMaterialID_Id");
			}
			this.BindBomChItems();
		}

		// Token: 0x060003C4 RID: 964 RVA: 0x0002ED04 File Offset: 0x0002CF04
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			base.BeforeF7Select(e);
			string text = string.Empty;
			string fieldKey;
			switch (fieldKey = e.FieldKey)
			{
			case "FMaterialID":
				this.BeforeMainMaterialF7(e);
				break;
			case "FSubMaterialID":
				this.BeforeSubMaterialF7(e);
				break;
			case "FReplaceNO":
				text = this.BeforeReplaceNoF7(e);
				break;
			case "FBomId":
				e.DynamicFormShowParameter.MultiSelect = false;
				e.IsShowApproved = false;
				e.ListFilterParameter.Filter = this.GetBomId2Filter(e.ListFilterParameter.Filter, e.Row, "FMaterialID", "FMainSupplyOrgId");
				break;
			case "FMainSupplyOrgId":
				e.ListFilterParameter.Filter = this.GetChildSupplyOrgFilter(e.ListFilterParameter.Filter, e.Row);
				break;
			case "FSubBomId":
				e.DynamicFormShowParameter.MultiSelect = false;
				e.IsShowApproved = false;
				e.ListFilterParameter.Filter = this.GetBomId2Filter(e.ListFilterParameter.Filter, e.Row, "FSubMaterialID", "FSubSupplyOrgId");
				break;
			case "FSubSupplyOrgId":
				e.ListFilterParameter.Filter = this.GetChildSupplyOrgFilter(e.ListFilterParameter.Filter, e.Row);
				break;
			}
			if (ObjectUtils.IsNullOrEmptyOrWhiteSpace(text))
			{
				return;
			}
			if (ObjectUtils.IsNullOrEmptyOrWhiteSpace(e.ListFilterParameter.Filter))
			{
				e.ListFilterParameter.Filter = text;
				return;
			}
			IRegularFilterParameter listFilterParameter = e.ListFilterParameter;
			listFilterParameter.Filter = listFilterParameter.Filter + " AND " + text;
		}

		// Token: 0x060003C5 RID: 965 RVA: 0x0002EF04 File Offset: 0x0002D104
		public override void DataChanged(DataChangedEventArgs e)
		{
			base.DataChanged(e);
			string key;
			if ((key = e.Field.Key) != null)
			{
				if (!(key == "FReplaceNO"))
				{
					if (!(key == "FSubMaterialID"))
					{
						return;
					}
					MFGBillUtil.SetEffectDate(base.View, "FEFFECTDATE", e.Row, 0L);
				}
				else
				{
					if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(e.NewValue))
					{
						this.SetMainInfo(Convert.ToString(e.NewValue));
						base.View.Model.DeleteEntryData("FEntity");
						this.SetRepInfo(Convert.ToString(e.NewValue));
						base.View.GetBarItem("FEntityMainItems", "tbNewMainItemEntryRow").Enabled = false;
						base.View.GetBarItem("FEntityMainItems", "tbDelMainItemEntryRow").Enabled = false;
						base.View.LockField("FMaterialID", false);
						return;
					}
					if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(e.OldValue) && ObjectUtils.IsNullOrEmptyOrWhiteSpace(e.NewValue))
					{
						this.DeleteMainEntryIds();
						base.View.Model.DeleteEntryData("FEntity");
						base.View.GetBarItem("FEntityMainItems", "tbNewMainItemEntryRow").Enabled = true;
						base.View.GetBarItem("FEntityMainItems", "tbDelMainItemEntryRow").Enabled = true;
						base.View.LockField("FMaterialID", true);
						return;
					}
				}
			}
		}

		// Token: 0x060003C6 RID: 966 RVA: 0x0002F06C File Offset: 0x0002D26C
		public override void BeforeSetItemValueByNumber(BeforeSetItemValueByNumberArgs e)
		{
			string baseDataFieldKey;
			if ((baseDataFieldKey = e.BaseDataFieldKey) != null)
			{
				if (!(baseDataFieldKey == "FMaterialID"))
				{
					return;
				}
				e.Cancel = true;
			}
		}

		// Token: 0x060003C7 RID: 967 RVA: 0x0002F098 File Offset: 0x0002D298
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			base.BarItemClick(e);
			string barItemKey;
			if ((barItemKey = e.BarItemKey) != null)
			{
				if (!(barItemKey == "tbReturnBom"))
				{
					if (barItemKey == "tbSaveAs")
					{
						this.SaveAs();
						return;
					}
					if (!(barItemKey == "tbClose"))
					{
						return;
					}
					base.View.Model.DataChanged = false;
				}
				else
				{
					if (this.ValidateData())
					{
						this.DeleteNullRows("FEntityMainItems", "EntityMainItems", "MaterialID_Id");
						this.DeleteNullRows("FEntity", "Entity", "SubMaterialID_Id");
						base.View.ReturnToParentWindow(base.View.Model.DataObject);
						base.View.Close();
						return;
					}
					e.Cancel = true;
					return;
				}
			}
		}

		// Token: 0x060003C8 RID: 968 RVA: 0x0002F15A File Offset: 0x0002D35A
		public override void BeforeClosed(BeforeClosedEventArgs e)
		{
			base.BeforeClosed(e);
			base.View.Model.DataChanged = false;
		}

		// Token: 0x060003C9 RID: 969 RVA: 0x0002F18C File Offset: 0x0002D38C
		private void BindBomChItems()
		{
			SubstitutionBillView substitutionBillView = new SubstitutionBillView(base.View.Model.DataObject);
			string dynamicValue = DataEntityExtend.GetDynamicValue<string>(this.BomChItems.First<DynamicObject>(), "ReplacePolicy", null);
			string dynamicValue2 = DataEntityExtend.GetDynamicValue<string>(this.BomChItems.First<DynamicObject>(), "ReplaceType", null);
			if (!string.IsNullOrWhiteSpace(dynamicValue))
			{
				substitutionBillView.ReplacePolicy = dynamicValue.ToString();
			}
			if (!string.IsNullOrWhiteSpace(dynamicValue2))
			{
				substitutionBillView.ReplaceType = dynamicValue2;
			}
			object parent = this.BomChItems.First<DynamicObject>().Parent;
			int num = 0;
			int num2 = 0;
			int num3 = (from x in this.BomChItems
			where !ObjectUtils.IsNullOrEmptyOrWhiteSpace(DataEntityExtend.GetDynamicValue<string>(x, "ParentRowId", null))
			select x).Count<DynamicObject>();
			if (num3 != 0)
			{
				base.View.Model.BatchCreateNewEntryRow("FEntity", num3);
			}
			foreach (DynamicObject dynamicObject in this.BomChItems)
			{
				if (ObjectUtils.IsNullOrEmptyOrWhiteSpace(DataEntityExtend.GetDynamicValue<string>(dynamicObject, "ParentRowId", null)))
				{
					base.View.Model.CreateNewEntryRow("FEntityMainItems");
					int dynamicValue3 = DataEntityExtend.GetDynamicValue<int>(dynamicObject, "ReplacePriority", 0);
					if (dynamicValue3 > 0)
					{
						base.View.Model.SetValue("FMainPriority", dynamicValue3, num);
					}
					bool flag = this.BomChItems.Count<DynamicObject>() == 1 || DataEntityExtend.GetDynamicValue<bool>(dynamicObject, "IskeyItem", false);
					base.View.Model.SetValue("FIsKeyItem", flag, num);
					base.View.Model.SetValue("FBOMRowId", DataEntityExtend.GetDynamicValue<string>(dynamicObject, "RowId", null), num);
					base.View.Model.SetValue("FMaterialID", DataEntityExtend.GetDynamicValue<long>(dynamicObject, "MATERIALIDCHILD_Id", 0L), num);
					base.View.Model.SetValue("FAuxPropID", DataEntityExtend.GetDynamicValue<long>(dynamicObject, "AuxPropId_Id", 0L), num);
					base.View.Model.SetValue("FAuxPropID", DataEntityExtend.GetDynamicValue<DynamicObject>(dynamicObject, "AuxPropId", null), num);
					base.View.Model.SetValue("FMainSupplyOrgId", DataEntityExtend.GetDynamicValue<long>(dynamicObject, "ChildSupplyOrgId_Id", 0L), num);
					base.View.Model.SetValue("FBomId", DataEntityExtend.GetDynamicValue<long>(dynamicObject, "BOMID_Id", 0L), num);
					base.View.Model.SetValue("FBaseUnitID", DataEntityExtend.GetDynamicValue<long>(dynamicObject, "ChildBaseUnitID_Id", 0L), num);
					base.View.Model.SetValue("FUnitID", DataEntityExtend.GetDynamicValue<long>(dynamicObject, "CHILDUNITID_Id", 0L), num);
					base.View.Model.SetValue("FNumerator", DataEntityExtend.GetDynamicValue<decimal>(dynamicObject, "NUMERATOR", 0m), num);
					base.View.Model.SetValue("FDenominator", DataEntityExtend.GetDynamicValue<decimal>(dynamicObject, "DENOMINATOR", 0m), num);
					base.View.Model.SetValue("FBaseNumerator", DataEntityExtend.GetDynamicValue<decimal>(dynamicObject, "BaseNumerator", 0m), num);
					base.View.Model.SetValue("FBaseDenominator", DataEntityExtend.GetDynamicValue<decimal>(dynamicObject, "BaseDenominator", 0m), num);
					base.View.Model.BeginIniti();
					base.View.Model.SetValue("FNETDEMANDRATE", DataEntityExtend.GetDynamicValue<decimal>(dynamicObject, "NETDEMANDRATE", 0m), num);
					base.View.Model.EndIniti();
					base.View.UpdateView("FNETDEMANDRATE", num);
					num++;
				}
				else
				{
					base.View.Model.SetValue("FPriority", DataEntityExtend.GetDynamicValue<int>(dynamicObject, "ReplacePriority", 0), num2);
					base.View.Model.SetValue("FSubBOMRowId", DataEntityExtend.GetDynamicValue<string>(dynamicObject, "RowId", null), num2);
					base.View.Model.SetValue("FSubMaterialID", DataEntityExtend.GetDynamicValue<long>(dynamicObject, "MATERIALIDCHILD_Id", 0L), num2);
					base.View.Model.SetValue("FSubIsKeyItem", DataEntityExtend.GetDynamicValue<bool>(dynamicObject, "IskeyItem", false), num2);
					base.View.Model.SetValue("FEffectDate", DataEntityExtend.GetDynamicValue<DateTime>(dynamicObject, "EFFECTDATE", default(DateTime)), num2);
					base.View.Model.SetValue("FExpireDate", DataEntityExtend.GetDynamicValue<DateTime>(dynamicObject, "EXPIREDATE", default(DateTime)), num2);
					base.View.Model.SetValue("FMemo", DataEntityExtend.GetDynamicValue<LocaleValue>(dynamicObject, "MEMO", null), num2);
					base.View.Model.SetValue("FSubAuxPropID", DataEntityExtend.GetDynamicValue<long>(dynamicObject, "AuxPropId_Id", 0L), num2);
					base.View.Model.SetValue("FSubAuxPropID", DataEntityExtend.GetDynamicValue<DynamicObject>(dynamicObject, "AuxPropId", null), num2);
					base.View.Model.SetValue("FSubSupplyOrgId", DataEntityExtend.GetDynamicValue<long>(dynamicObject, "ChildSupplyOrgId_Id", 0L), num2);
					base.View.Model.SetValue("FSubBomId", DataEntityExtend.GetDynamicValue<long>(dynamicObject, "BOMID_Id", 0L), num2);
					base.View.Model.SetValue("FSubBaseUnitID", DataEntityExtend.GetDynamicValue<long>(dynamicObject, "ChildBaseUnitID_Id", 0L), num2);
					base.View.Model.SetValue("FSubUnitID", DataEntityExtend.GetDynamicValue<long>(dynamicObject, "CHILDUNITID_Id", 0L), num2);
					base.View.Model.SetValue("FSubNumerator", DataEntityExtend.GetDynamicValue<decimal>(dynamicObject, "NUMERATOR", 0m), num2);
					base.View.Model.SetValue("FSubDenominator", DataEntityExtend.GetDynamicValue<decimal>(dynamicObject, "DENOMINATOR", 0m), num2);
					base.View.Model.SetValue("FSubBaseNumerator", DataEntityExtend.GetDynamicValue<decimal>(dynamicObject, "BaseNumerator", 0m), num2);
					base.View.Model.SetValue("FSubBaseDenominator", DataEntityExtend.GetDynamicValue<decimal>(dynamicObject, "BaseDenominator", 0m), num2);
					base.View.Model.BeginIniti();
					base.View.Model.SetValue("FSUBNETDEMANDRATE", DataEntityExtend.GetDynamicValue<decimal>(dynamicObject, "NETDEMANDRATE", 0m), num2);
					base.View.Model.EndIniti();
					base.View.UpdateView("FSUBNETDEMANDRATE", num2);
					num2++;
				}
			}
		}

		// Token: 0x060003CA RID: 970 RVA: 0x0002FC44 File Offset: 0x0002DE44
		protected virtual void BeforeMainMaterialF7(BeforeF7SelectEventArgs e)
		{
			e.Cancel = true;
			long dynamicValue = DataEntityExtend.GetDynamicValue<long>(this.BomChItems.First<DynamicObject>().Parent as DynamicObject, "Id", 0L);
			Entity entryEntity = base.View.BusinessInfo.GetEntryEntity("FEntityMainItems");
			DynamicObjectCollection entityDataObject = base.View.Model.GetEntityDataObject(entryEntity);
			List<string> list = (from w in entityDataObject
			where !ObjectUtils.IsNullOrEmptyOrWhiteSpace(DataEntityExtend.GetDynamicValue<string>(w, "BOMRowId", null))
			select w into s
			select DataEntityExtend.GetDynamicValue<string>(s, "BOMRowId", null)).ToList<string>();
			DynamicObject dynamicObject = this.BomChItems.First<DynamicObject>().Parent as DynamicObject;
			string dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<string>(dynamicObject, "BOMCATEGORY", null);
			string dynamicObjectItemValue2 = DataEntityExtend.GetDynamicObjectItemValue<string>(DataEntityExtend.GetDynamicObjectItemValue<DynamicObjectCollection>(DataEntityExtend.GetDynamicObjectItemValue<DynamicObject>(dynamicObject, "MATERIALID", null), "MaterialBase", null).First<DynamicObject>(), "ErpClsID", null);
			ListSelBillShowParameter listSelBillShowParameter = new ListSelBillShowParameter();
			if (!StringUtils.EqualsIgnoreCase(dynamicObjectItemValue, "1") || StringUtils.EqualsIgnoreCase(dynamicObjectItemValue2, "4"))
			{
				listSelBillShowParameter = SubStituteViewServiceHelper.GetBomBillSelShowParam(base.Context, list, dynamicValue, base.View.PageId);
			}
			else
			{
				listSelBillShowParameter = SubStituteViewServiceHelper.GetBomBillSelShowParamForPZ(base.Context, list, dynamicValue, base.View.PageId);
			}
			listSelBillShowParameter.UseOrgId = MFGBillUtil.GetValue<long>(base.View.Model, "FUseOrgId", -1, 0L, null);
			base.View.ShowForm(listSelBillShowParameter, delegate(FormResult result)
			{
				if (result != null && result.ReturnData != null && (result.ReturnData as ListSelectedRowCollection).Count > 0)
				{
					ListSelectedRowCollection source = (ListSelectedRowCollection)result.ReturnData;
					List<long> bomEntryId = (from s in source
					select Convert.ToInt64(s.EntryPrimaryKeyValue)).ToList<long>();
					DynamicObjectCollection bomOtherChs = this.GetBomOtherChs(bomEntryId);
					int num = base.View.Model.GetEntryRowCount("FEntityMainItems") - 1;
					foreach (DynamicObject dynamicObject2 in bomOtherChs)
					{
						if (num == base.View.Model.GetEntryRowCount("FEntityMainItems"))
						{
							base.View.Model.CreateNewEntryRow("FEntityMainItems");
						}
						base.View.Model.SetValue("FBOMRowId", DataEntityExtend.GetDynamicValue<string>(dynamicObject2, "FRowId", null), num);
						base.View.Model.SetValue("FMaterialID", DataEntityExtend.GetDynamicValue<long>(dynamicObject2, "FMATERIALIDCHILD", 0L), num);
						base.View.Model.SetValue("FAuxPropID", DataEntityExtend.GetDynamicValue<long>(dynamicObject2, "FAuxPropId", 0L), num);
						base.View.Model.SetValue("FMainSupplyOrgId", DataEntityExtend.GetDynamicValue<long>(dynamicObject2, "FChildSupplyOrgId", 0L), num);
						base.View.Model.SetValue("FBomId", DataEntityExtend.GetDynamicValue<long>(dynamicObject2, "FBOMID", 0L), num);
						base.View.Model.SetValue("FBaseUnitID", DataEntityExtend.GetDynamicValue<long>(dynamicObject2, "FChildBaseUnitID", 0L), num);
						base.View.Model.SetValue("FUnitID", DataEntityExtend.GetDynamicValue<long>(dynamicObject2, "FCHILDUNITID", 0L), num);
						base.View.Model.SetValue("FNumerator", DataEntityExtend.GetDynamicValue<decimal>(dynamicObject2, "FNUMERATOR", 0m), num);
						base.View.Model.SetValue("FDenominator", DataEntityExtend.GetDynamicValue<decimal>(dynamicObject2, "FDENOMINATOR", 0m), num);
						base.View.Model.SetValue("FBaseNumerator", DataEntityExtend.GetDynamicValue<decimal>(dynamicObject2, "FBaseNumerator", 0m), num);
						base.View.Model.SetValue("FBaseDenominator", DataEntityExtend.GetDynamicValue<decimal>(dynamicObject2, "FBaseDenominator", 0m), num);
						base.View.Model.SetEntryPKValue("Id", DataEntityExtend.GetDynamicValue<decimal>(dynamicObject2, "FSTEntryId", 0m), num);
						base.View.Model.SetValue("FNETDEMANDRATE", DataEntityExtend.GetDynamicValue<decimal>(dynamicObject2, "FNETDEMANDRATE", 0m), num);
						num++;
					}
				}
			});
		}

		// Token: 0x060003CB RID: 971 RVA: 0x0002FDD0 File Offset: 0x0002DFD0
		protected virtual void BeforeSubMaterialF7(BeforeF7SelectEventArgs e)
		{
			if (StringUtils.EqualsIgnoreCase(base.View.ParentFormView.BillBusinessInfo.GetForm().Id, "ENG_BOM"))
			{
				DynamicObject dynamicObject = this.BomChItems.First<DynamicObject>().Parent as DynamicObject;
				string dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<string>(dynamicObject, "BOMCATEGORY", null);
				string dynamicObjectItemValue2 = DataEntityExtend.GetDynamicObjectItemValue<string>(DataEntityExtend.GetDynamicObjectItemValue<DynamicObjectCollection>(DataEntityExtend.GetDynamicObjectItemValue<DynamicObject>(dynamicObject, "MATERIALID", null), "MaterialBase", null).First<DynamicObject>(), "ErpClsID", null);
				new ListSelBillShowParameter();
				if (!StringUtils.EqualsIgnoreCase(dynamicObjectItemValue, "1") || StringUtils.EqualsIgnoreCase(dynamicObjectItemValue2, "4"))
				{
					if (ObjectUtils.IsNullOrEmptyOrWhiteSpace(e.ListFilterParameter.Filter))
					{
						e.ListFilterParameter.Filter = " FErpClsID <> '9'";
						return;
					}
					e.ListFilterParameter.Filter = StringUtils.JoinFilterString(e.ListFilterParameter.Filter, " FErpClsID <> '9'", "AND");
				}
			}
		}

		// Token: 0x060003CC RID: 972 RVA: 0x0002FECC File Offset: 0x0002E0CC
		private string BeforeReplaceNoF7(BeforeF7SelectEventArgs e)
		{
			if (!((DynamicObjectCollection)base.View.Model.DataObject["EntityMainItems"]).Any((DynamicObject w) => DataEntityExtend.GetDynamicValue<long>(w, "MaterialID_Id", 0L) > 0L))
			{
				base.View.ShowErrMessage(ResManager.LoadKDString("主物料至少选择一个物料！", "015072000002186", 7, new object[0]), "", 0);
				e.Cancel = true;
				return string.Empty;
			}
			return SubStituteViewServiceHelper.GetSubStituteFilter(base.Context, base.View.Model.DataObject);
		}

		// Token: 0x060003CD RID: 973 RVA: 0x0002FF70 File Offset: 0x0002E170
		private DynamicObjectCollection GetBomOtherChs(List<long> bomEntryId)
		{
			QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter
			{
				FormId = "ENG_BOM",
				SelectItems = SelectorItemInfo.CreateItems(new string[]
				{
					"FTREEENTITY_FENTRYID AS FENTRYID",
					"FRowId",
					"FMATERIALIDCHILD",
					"FChildBaseUnitID",
					"FCHILDUNITID",
					"FBOMID",
					"FAuxPropId",
					"FNUMERATOR",
					"FDENOMINATOR",
					"FBaseNumerator",
					"FBaseDenominator",
					"FSTEntryId",
					"FNETDEMANDRATE",
					"FChildSupplyOrgId"
				})
			};
			ExtJoinTableDescription item = new ExtJoinTableDescription
			{
				TableName = "table(fn_StrSplit(@spPKValue,',',1))",
				TableNameAs = "sp",
				FieldName = "FId",
				ScourceKey = "FTREEENTITY_FENTRYID"
			};
			queryBuilderParemeter.ExtJoinTables.Add(item);
			queryBuilderParemeter.SqlParams.Add(new SqlParam("@spPKValue", 161, bomEntryId.Distinct<long>().ToArray<long>()));
			return MFGServiceHelper.GetDynamicObjectCollection(base.Context, queryBuilderParemeter, null);
		}

		// Token: 0x060003CE RID: 974 RVA: 0x0003009C File Offset: 0x0002E29C
		private void SetMainInfo(string replaceId)
		{
			DynamicObject dynamicObject = BusinessDataServiceHelper.LoadSingle(base.Context, replaceId, base.View.BusinessInfo.GetDynamicObjectType(), null);
			DynamicObjectCollection dynamicValue = DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(dynamicObject, "EntityMainItems", null);
			DynamicObjectCollection dynamicValue2 = DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(base.View.Model.DataObject, "EntityMainItems", null);
			foreach (DynamicObject dynamicObject2 in dynamicValue2)
			{
				foreach (DynamicObject dynamicObject3 in dynamicValue)
				{
					if (DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObject3, "MaterialID_Id", 0L) == DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObject2, "MaterialID_Id", 0L))
					{
						DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "Id", DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObject3, "Id", 0L));
						DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "NETDEMANDRATE", DataEntityExtend.GetDynamicValue<decimal>(dynamicObject3, "NETDEMANDRATE", 0m));
					}
				}
			}
		}

		// Token: 0x060003CF RID: 975 RVA: 0x000301D8 File Offset: 0x0002E3D8
		private void DeleteMainEntryIds()
		{
			List<DynamicObject> list = DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(base.View.Model.DataObject, "EntityMainItems", null).ToList<DynamicObject>();
			list.ForEach(delegate(DynamicObject x)
			{
				DataEntityExtend.SetDynamicObjectItemValue(x, "Id", 0);
			});
		}

		// Token: 0x060003D0 RID: 976 RVA: 0x00030238 File Offset: 0x0002E438
		private void SetRepInfo(string replaceId)
		{
			DynamicObject dynamicObject = BusinessDataServiceHelper.LoadSingle(base.Context, replaceId, base.View.BusinessInfo.GetDynamicObjectType(), null);
			base.View.Model.SetValue("FReplacePolicy", DataEntityExtend.GetDynamicValue<string>(dynamicObject, "ReplacePolicy", null));
			base.View.Model.SetValue("FReplaceType", DataEntityExtend.GetDynamicValue<string>(dynamicObject, "ReplaceType", null));
			DynamicObjectCollection dynamicValue = DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(dynamicObject, "EntityMainItems", null);
			DynamicObjectCollection dynamicValue2 = DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(dynamicObject, "Entity", null);
			(from i in dynamicValue
			where DataEntityExtend.GetDynamicValue<bool>(i, "IsKeyItem", false)
			select i).FirstOrDefault<DynamicObject>();
			SubStituteViewServiceHelper.CalSubsMaterialQty(base.Context, base.View.Model.DataObject, dynamicValue, dynamicValue2);
			base.View.UpdateView("FEntityMainItems");
			int num = 0;
			foreach (DynamicObject dynamicObject2 in dynamicValue2)
			{
				base.View.Model.CreateNewEntryRow("FEntity");
				base.View.Model.SetValue("FPriority", DataEntityExtend.GetDynamicValue<int>(dynamicObject2, "Priority", 0), num);
				base.View.Model.SetValue("FEffectDate", DataEntityExtend.GetDynamicValue<DateTime>(dynamicObject2, "EffectDate", default(DateTime)), num);
				base.View.Model.SetValue("FExpireDate", DataEntityExtend.GetDynamicValue<DateTime>(dynamicObject2, "ExpireDate", default(DateTime)), num);
				base.View.Model.SetValue("FMemo", DataEntityExtend.GetDynamicValue<LocaleValue>(dynamicObject2, "MEMO", null), num);
				base.View.Model.SetValue("FSubMaterialID", DataEntityExtend.GetDynamicValue<long>(dynamicObject2, "SubMaterialID_Id", 0L), num);
				base.View.Model.SetValue("FSubAuxPropID", DataEntityExtend.GetDynamicValue<long>(dynamicObject2, "SubAuxPropID_Id", 0L), num);
				base.View.Model.SetValue("FSubAuxPropID", DataEntityExtend.GetDynamicValue<DynamicObject>(dynamicObject2, "SubAuxPropID", null), num);
				base.View.Model.SetValue("FSubSupplyOrgId", DataEntityExtend.GetDynamicValue<long>(dynamicObject2, "SubSupplyOrgId_Id", 0L), num);
				base.View.Model.SetValue("FSubBomId", DataEntityExtend.GetDynamicValue<long>(dynamicObject2, "SubBomId_Id", 0L), num);
				base.View.Model.SetValue("FSubBaseUnitID", DataEntityExtend.GetDynamicValue<long>(dynamicObject2, "SubBaseUnitID_Id", 0L), num);
				base.View.Model.SetValue("FSubIsKeyItem", DataEntityExtend.GetDynamicValue<bool>(dynamicObject2, "SubIsKeyItem", false), num);
				base.View.Model.SetValue("FSubUnitID", DataEntityExtend.GetDynamicValue<long>(dynamicObject2, "SubUnitID_Id", 0L), num);
				base.View.Model.SetValue("FSubNumerator", DataEntityExtend.GetDynamicValue<decimal>(dynamicObject2, "SubNumerator", 0m), num);
				base.View.Model.SetValue("FSubDenominator", DataEntityExtend.GetDynamicValue<decimal>(dynamicObject2, "SubDenominator", 0m), num);
				DynamicObject dynamicObject3 = DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(base.View.Model.DataObject, "Entity", null)[num];
				DataEntityExtend.SetDynamicObjectItemValue(dynamicObject3, "Id", DataEntityExtend.GetDynamicValue<long>(dynamicObject2, "Id", 0L));
				base.View.Model.SetValue("FSUBNETDEMANDRATE", DataEntityExtend.GetDynamicValue<decimal>(dynamicObject2, "SUBNETDEMANDRATE", 0m), num);
				num++;
			}
			base.View.UpdateView("FEntity");
		}

		// Token: 0x060003D1 RID: 977 RVA: 0x000307D4 File Offset: 0x0002E9D4
		protected virtual bool ValidateData()
		{
			StringBuilder stringBuilder = new StringBuilder();
			List<DynamicObject> list = (from w in (DynamicObjectCollection)base.View.Model.DataObject["EntityMainItems"]
			where DataEntityExtend.GetDynamicValue<long>(w, "MaterialID_Id", 0L) > 0L
			select w).ToList<DynamicObject>();
			IEnumerable<DynamicObject> source = from w in list
			where DataEntityExtend.GetDynamicValue<bool>(w, "IsKeyItem", false)
			select w;
			IEnumerable<DynamicObject> source2 = from w in list
			where DataEntityExtend.GetDynamicValue<decimal>(w, "Numerator", 0m) <= 0m || DataEntityExtend.GetDynamicValue<decimal>(w, "Denominator", 0m) <= 0m
			select w;
			if (list.Count<DynamicObject>() <= 0)
			{
				base.View.ShowErrMessage(ResManager.LoadKDString("主物料至少选择一个物料！", "015072000002186", 7, new object[0]), "", 0);
				return false;
			}
			if (source.Count<DynamicObject>() <= 0 || source.Count<DynamicObject>() > 1)
			{
				stringBuilder.AppendLine(ResManager.LoadKDString("主物料有且只能有一个替代主料！", "015072000002187", 7, new object[0]));
			}
			if (source2.Count<DynamicObject>() > 0)
			{
				stringBuilder.AppendLine(ResManager.LoadKDString("主物料分子和分母必须大于0！", "015072000003212", 7, new object[0]));
			}
			int value = MFGBillUtil.GetValue<int>(base.View.Model, "FMainPriority", 0, 0, null);
			string value2 = string.Empty;
			List<DynamicObject> list2 = (from w in (DynamicObjectCollection)base.View.Model.DataObject["Entity"]
			where DataEntityExtend.GetDynamicValue<long>(w, "SubMaterialID_Id", 0L) > 0L
			select w).ToList<DynamicObject>();
			if (list2.Count<DynamicObject>() <= 0)
			{
				base.View.ShowErrMessage(ResManager.LoadKDString("替代物料至少选择一个物料！", "015072000002188", 7, new object[0]), "", 0);
				return false;
			}
			IEnumerable<DynamicObject> source3 = from w in list2
			where DataEntityExtend.GetDynamicValue<decimal>(w, "SubNumerator", 0m) <= 0m || DataEntityExtend.GetDynamicValue<decimal>(w, "SubDenominator", 0m) <= 0m
			select w;
			if (source3.Count<DynamicObject>() > 0)
			{
				stringBuilder.AppendLine(ResManager.LoadKDString("替代物料分子和分母必须大于0！", "015072000003213", 7, new object[0]));
			}
			IEnumerable<IGrouping<int, DynamicObject>> enumerable = from g in list2
			group g by DataEntityExtend.GetDynamicValue<int>(g, "Priority", 0);
			foreach (IGrouping<int, DynamicObject> grouping in enumerable)
			{
				if (grouping.Key == value)
				{
					value2 = ResManager.LoadKDString("主物料和替代物料优先级重复！", "015072000002189", 7, new object[0]);
					stringBuilder.AppendLine(value2);
				}
				IEnumerable<DynamicObject> source4 = from w in grouping
				where DataEntityExtend.GetDynamicValue<bool>(w, "SubIsKeyItem", false)
				select w;
				if (source4.Count<DynamicObject>() <= 0 || source4.Count<DynamicObject>() > 1)
				{
					stringBuilder.AppendLine(ResManager.LoadKDString("优先级相同的替代物料有且只能有一个替代主料！", "015072000002190", 7, new object[0]));
				}
			}
			if (list2.Any((DynamicObject w) => ObjectUtils.IsNullOrEmptyOrWhiteSpace(DataEntityExtend.GetDynamicValue<string>(w, "EffectDate", null))))
			{
				stringBuilder.AppendLine(ResManager.LoadKDString("替代物料生效日期必录！", "015072000002191", 7, new object[0]));
			}
			if (list2.Any((DynamicObject w) => ObjectUtils.IsNullOrEmptyOrWhiteSpace(DataEntityExtend.GetDynamicValue<string>(w, "ExpireDate", null))))
			{
				stringBuilder.AppendLine(ResManager.LoadKDString("替代物料失效日期必录！", "015072000002192", 7, new object[0]));
			}
			if (list2.Any((DynamicObject w) => DataEntityExtend.GetDynamicValue<DateTime>(w, "ExpireDate", default(DateTime)) < DataEntityExtend.GetDynamicValue<DateTime>(w, "EffectDate", default(DateTime))))
			{
				stringBuilder.AppendLine(ResManager.LoadKDString("替代物料失效日期必须晚于生效日期！", "015072000002193", 7, new object[0]));
			}
			stringBuilder.Append(SubStituteViewServiceHelper.CheckMainItemUnique(base.Context, list, list2));
			if (stringBuilder.Length > 0)
			{
				stringBuilder.AppendLine();
			}
			stringBuilder.Append(SubStituteViewServiceHelper.CheckSubsItemUnique(base.Context, list2));
			string value3 = MFGBillUtil.GetValue<string>(base.View.Model, "FReplaceType", -1, null, null);
			DynamicObjectCollection dynamicValue = DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(base.View.Model.DataObject, "EntityMainItems", null);
			DynamicObjectCollection dynamicValue2 = DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(base.View.Model.DataObject, "Entity", null);
			bool flag = dynamicValue.Any((DynamicObject a) => DataEntityExtend.GetDynamicValue<decimal>(a, "NETDEMANDRATE", 0m) > 0m);
			bool flag2 = dynamicValue2.Any((DynamicObject a) => DataEntityExtend.GetDynamicValue<decimal>(a, "SUBNETDEMANDRATE", 0m) > 0m);
			if (value3 != "3" && (flag || flag2))
			{
				stringBuilder.AppendLine(ResManager.LoadKDString("替代方式不为按比例，主物料或替代料中存在净需求比例大于零的数据", "015072030044746", 7, new object[0]));
			}
			decimal d = dynamicValue.Sum((DynamicObject s) => DataEntityExtend.GetDynamicValue<decimal>(s, "NETDEMANDRATE", 0m));
			decimal d2 = dynamicValue2.Sum((DynamicObject s) => DataEntityExtend.GetDynamicValue<decimal>(s, "SUBNETDEMANDRATE", 0m));
			if (value3 == "3" && d + d2 != 100m)
			{
				stringBuilder.AppendLine(ResManager.LoadKDString("替代方式为按比例，主物料或替代料中净需求比例之和不等于100%", "015072030044747", 7, new object[0]));
			}
			if (stringBuilder.Length > 0)
			{
				base.View.ShowErrMessage(stringBuilder.ToString(), "", 0);
				return false;
			}
			base.View.Model.DataChanged = false;
			return true;
		}

		// Token: 0x060003D2 RID: 978 RVA: 0x00030D88 File Offset: 0x0002EF88
		protected void DeleteNullRows(string keyEntity, string ormEntity, string nullField)
		{
			DynamicObjectCollection source = (DynamicObjectCollection)base.View.Model.DataObject[ormEntity];
			List<DynamicObject> list = (from o in source.AsEnumerable<DynamicObject>()
			where DataEntityExtend.GetDynamicValue<long>(o, nullField, 0L) <= 0L
			select o).ToList<DynamicObject>();
			for (int i = list.Count - 1; i >= 0; i--)
			{
				int num = DataEntityExtend.GetDynamicValue<int>(list.ElementAt(i), "Seq", 0) - 1;
				base.View.Model.DeleteEntryRow(keyEntity, num);
			}
		}

		// Token: 0x060003D3 RID: 979 RVA: 0x00030E18 File Offset: 0x0002F018
		private void SaveAs()
		{
			this.DeleteNullRows("FEntityMainItems", "EntityMainItems", "MaterialID_Id");
			this.DeleteNullRows("FEntity", "Entity", "SubMaterialID_Id");
			BillShowParameter billShowParameter = new BillShowParameter();
			billShowParameter.OpenStyle.ShowType = 6;
			billShowParameter.FormId = "ENG_Substitution";
			billShowParameter.Status = 0;
			billShowParameter.ParentPageId = base.View.PageId;
			DynamicObject value = OrmUtils.Clone(base.View.Model.DataObject, false, true) as DynamicObject;
			base.View.Session["NewDataObject"] = value;
			base.View.ShowForm(billShowParameter);
		}

		// Token: 0x060003D4 RID: 980 RVA: 0x00030EC4 File Offset: 0x0002F0C4
		private string GetChildSupplyOrgFilter(string filter, int row)
		{
			if (filter == null)
			{
				filter = string.Empty;
			}
			long value = MFGBillUtil.GetValue<long>(base.View.Model, "FUseOrgId", -1, 0L, null);
			List<long> list = new List<long>
			{
				102L,
				112L,
				101L,
				104L,
				103L,
				109L
			};
			List<long> list2 = new List<long>();
			List<long> orgByBizRelationOrgs = MFGServiceHelper.GetOrgByBizRelationOrgs(base.Context, value, list);
			if (orgByBizRelationOrgs == null || orgByBizRelationOrgs.Count < 1)
			{
				return filter;
			}
			list2.AddRange(orgByBizRelationOrgs);
			list2.Add(value);
			filter += ((filter.Length > 0) ? (" AND " + string.Format("FORGID in ({0})", string.Join<long>(",", list2))) : string.Format("FORGID in ({0})", string.Join<long>(",", list2)));
			return filter;
		}

		// Token: 0x060003D5 RID: 981 RVA: 0x00030FB4 File Offset: 0x0002F1B4
		private string GetBomId2Filter(string filter, int row, string materialField, string supplyOrgField)
		{
			if (filter == null)
			{
				filter = string.Empty;
			}
			string text = string.Empty;
			text = " FMATERIALID=0 ";
			DynamicObject value = MFGBillUtil.GetValue<DynamicObject>(base.View.Model, materialField, row, null, null);
			if (value != null)
			{
				long dynamicValue = DataEntityExtend.GetDynamicValue<long>(value, "MsterId", 0L);
				long value2 = MFGBillUtil.GetValue<long>(base.View.Model, supplyOrgField, row, 0L, null);
				FormMetadata formMetadata = MetaDataServiceHelper.Load(base.Context, "BD_MATERIAL", true) as FormMetadata;
				object baseDataPkId = BaseDataServiceHelper.GetBaseDataPkId(base.Context, formMetadata.BusinessInfo, "BD_MATERIAL", dynamicValue, value2);
				if (baseDataPkId != null && baseDataPkId is DynamicObject)
				{
					text = string.Format(" FMATERIALID={0} ", Convert.ToInt64(((DynamicObject)baseDataPkId)[0]));
				}
				else if (baseDataPkId != null && baseDataPkId is long)
				{
					text = string.Format(" FMATERIALID={0} ", Convert.ToInt64(baseDataPkId));
				}
			}
			filter += ((filter.Length > 0) ? (" AND " + text) : text);
			return filter;
		}

		// Token: 0x04000199 RID: 409
		protected IEnumerable<DynamicObject> BomChItems;
	}
}
