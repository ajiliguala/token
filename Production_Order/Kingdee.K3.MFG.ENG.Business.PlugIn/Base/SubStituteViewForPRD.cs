using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Kingdee.BOS;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.Core.MFG.Utils;
using Kingdee.K3.MFG.Common.BusinessEntity.ENG;
using Kingdee.K3.MFG.ServiceHelper;
using Kingdee.K3.MFG.ServiceHelper.ENG;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.Base
{
	// Token: 0x020000D2 RID: 210
	[Description("替代方案设置视图插件")]
	public class SubStituteViewForPRD : SubStituteView
	{
		// Token: 0x06000EC6 RID: 3782 RVA: 0x000AAE5C File Offset: 0x000A905C
		public override void BeforeBindData(EventArgs e)
		{
			this.dictDelError = (base.View.OpenParameter.GetCustomParameter("DeleteValidation") as Dictionary<string, string>);
			this.pushedIds = (base.View.OpenParameter.GetCustomParameter("PushedIds") as HashSet<string>);
			if (this.BomChItems == null || this.BomChItems.Count<DynamicObject>() <= 0)
			{
				return;
			}
			base.View.Model.SetValue("FUseOrgId", DataEntityExtend.GetDynamicValue<long>(this.BomChItems.First<DynamicObject>().Parent as DynamicObject, "PrdOrgId_Id", 0L));
			base.DeleteNullRows("FEntityMainItems", "EntityMainItems", "MaterialID_Id");
			int num = (from w in this.BomChItems
			where !ObjectUtils.IsNullOrEmptyOrWhiteSpace(DataEntityExtend.GetDynamicValue<string>(w, "ParentRowId", null))
			select w).Count<DynamicObject>();
			if (num > 0)
			{
				base.DeleteNullRows("FEntity", "Entity", "SubMaterialID_Id");
			}
			this.BindBomChItems();
			if (!ListUtils.IsEmpty<KeyValuePair<string, string>>(this.dictDelError))
			{
				base.View.StyleManager.SetEnabled("FReplaceNO", "locked", false);
			}
		}

		// Token: 0x06000EC7 RID: 3783 RVA: 0x000AAF9C File Offset: 0x000A919C
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
					base.View.Model.SetValue("FMaterialID", DataEntityExtend.GetDynamicValue<long>(dynamicObject, "MaterialID_Id", 0L), num);
					base.View.Model.SetValue("FAuxPropID", DataEntityExtend.GetDynamicValue<long>(dynamicObject, "AuxPropID_Id", 0L), num);
					base.View.Model.SetValue("FAuxPropID", DataEntityExtend.GetDynamicValue<DynamicObject>(dynamicObject, "AuxPropID", null), num);
					base.View.Model.SetValue("FMainSupplyOrgId", DataEntityExtend.GetDynamicValue<long>(dynamicObject, "ChildSupplyOrgId_Id", 0L), num);
					base.View.Model.SetValue("FBomId", DataEntityExtend.GetDynamicValue<long>(dynamicObject, "BOMID_Id", 0L), num);
					base.View.Model.SetValue("FBaseUnitID", DataEntityExtend.GetDynamicValue<long>(dynamicObject, "BaseUnitID_Id", 0L), num);
					base.View.Model.SetValue("FUnitID", DataEntityExtend.GetDynamicValue<long>(dynamicObject, "UnitID_Id", 0L), num);
					base.View.Model.SetValue("FNumerator", DataEntityExtend.GetDynamicValue<decimal>(dynamicObject, "Numerator", 0m), num);
					base.View.Model.SetValue("FDenominator", DataEntityExtend.GetDynamicValue<decimal>(dynamicObject, "Denominator", 0m), num);
					if (dynamicValue2 == "3" && flag)
					{
						base.View.Model.BeginIniti();
						base.View.Model.SetValue("FNETDEMANDRATE", DataEntityExtend.GetDynamicValue<decimal>(dynamicObject, "UseRate", 0m), num);
						base.View.Model.EndIniti();
						base.View.UpdateView("FNETDEMANDRATE", num);
					}
					num++;
				}
				else
				{
					string dynamicValue4 = DataEntityExtend.GetDynamicValue<string>(dynamicObject, "RowId", null);
					base.View.Model.SetValue("FPriority", DataEntityExtend.GetDynamicValue<int>(dynamicObject, "ReplacePriority", 0), num2);
					base.View.Model.SetValue("FSubBOMRowId", dynamicValue4, num2);
					base.View.Model.SetValue("FSubMaterialID", DataEntityExtend.GetDynamicValue<long>(dynamicObject, "MaterialID_Id", 0L), num2);
					base.View.Model.SetValue("FSubIsKeyItem", DataEntityExtend.GetDynamicValue<bool>(dynamicObject, "IsKeyItem", false), num2);
					base.View.Model.SetValue("FMemo", DataEntityExtend.GetDynamicValue<LocaleValue>(dynamicObject, "MEMO", null), num2);
					base.View.Model.SetValue("FSubAuxPropID", DataEntityExtend.GetDynamicValue<long>(dynamicObject, "AuxPropID_Id", 0L), num2);
					base.View.Model.SetValue("FSubAuxPropID", DataEntityExtend.GetDynamicValue<DynamicObject>(dynamicObject, "AuxPropID", null), num2);
					base.View.Model.SetValue("FSubSupplyOrgId", DataEntityExtend.GetDynamicValue<long>(dynamicObject, "ChildSupplyOrgId_Id", 0L), num2);
					base.View.Model.SetValue("FSubBomId", DataEntityExtend.GetDynamicValue<long>(dynamicObject, "BOMID_Id", 0L), num2);
					base.View.Model.SetValue("FSubBaseUnitID", DataEntityExtend.GetDynamicValue<long>(dynamicObject, "BaseUnitID_Id", 0L), num2);
					base.View.Model.SetValue("FSubUnitID", DataEntityExtend.GetDynamicValue<long>(dynamicObject, "UnitID_Id", 0L), num2);
					base.View.Model.SetValue("FSubNumerator", DataEntityExtend.GetDynamicValue<decimal>(dynamicObject, "Numerator", 0m), num2);
					base.View.Model.SetValue("FSubDenominator", DataEntityExtend.GetDynamicValue<decimal>(dynamicObject, "Denominator", 0m), num2);
					base.View.GetFieldEditor("FSubMaterialID", num2).Enabled = !this.pushedIds.Contains(dynamicValue4);
					bool dynamicValue5 = DataEntityExtend.GetDynamicValue<bool>(dynamicObject, "IsKeyItem", false);
					if (dynamicValue2 == "3" && dynamicValue5)
					{
						base.View.Model.BeginIniti();
						base.View.Model.SetValue("FSUBNETDEMANDRATE", DataEntityExtend.GetDynamicValue<decimal>(dynamicObject, "UseRate", 0m), num2);
						base.View.Model.EndIniti();
						base.View.UpdateView("FSUBNETDEMANDRATE", num2);
					}
					num2++;
				}
			}
		}

		// Token: 0x06000EC8 RID: 3784 RVA: 0x000AB630 File Offset: 0x000A9830
		public override void BeforeDeleteRow(BeforeDeleteRowEventArgs e)
		{
			string text = "";
			string entityKey;
			if ((entityKey = e.EntityKey) != null)
			{
				if (!(entityKey == "FEntity"))
				{
					if (entityKey == "FEntityMainItems")
					{
						text = OtherExtend.ConvertTo<string>(this.Model.GetValue("FBOMRowId", e.Row), null);
					}
				}
				else
				{
					text = OtherExtend.ConvertTo<string>(this.Model.GetValue("FSubBOMRowId", e.Row), null);
				}
			}
			string text2;
			if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(text) && this.dictDelError.TryGetValue(text, out text2))
			{
				base.View.ShowErrMessage("", text2, 0);
				e.Cancel = true;
			}
		}

		// Token: 0x06000EC9 RID: 3785 RVA: 0x000ABA50 File Offset: 0x000A9C50
		protected override void BeforeMainMaterialF7(BeforeF7SelectEventArgs e)
		{
			e.Cancel = true;
			long dynamicValue = DataEntityExtend.GetDynamicValue<long>(this.BomChItems.First<DynamicObject>().Parent as DynamicObject, "Id", 0L);
			Entity entryEntity = base.View.BusinessInfo.GetEntryEntity("FEntityMainItems");
			DynamicObjectCollection entityDataObject = base.View.Model.GetEntityDataObject(entryEntity);
			List<string> values = (from m in entityDataObject
			where !ObjectUtils.IsNullOrEmptyOrWhiteSpace(m["BOMRowId"])
			select m into s
			select DataEntityExtend.GetDynamicValue<string>(s, "BOMRowId", null)).ToList<string>();
			ListSelBillShowParameter listSelBillShowParameter = new ListSelBillShowParameter
			{
				FormId = "PRD_PPBOM",
				PermissionItemId = "6e44119a58cb4a8e86f6c385e14a17ad",
				ParentPageId = base.View.PageId,
				IsLookUp = true
			};
			IRegularFilterParameter listFilterParameter = listSelBillShowParameter.ListFilterParameter;
			listFilterParameter.Filter += string.Format(" {0}FId={1} AND FRowId NOT IN ('{2}') AND FMATERIALTYPE = '1' AND (ISNULL(FReplacePolicy,'0')='0' OR FReplacePolicy=' ') AND EXISTS(SELECT 1 FROM T_BD_MATERIALBASE TMB WHERE TMB.FMATERIALID=t1.FMATERIALID AND TMB.FERPCLSID NOT IN('4','5')) ", ObjectUtils.IsNullOrEmptyOrWhiteSpace(listSelBillShowParameter.ListFilterParameter.Filter) ? string.Empty : " And ", dynamicValue, string.Join("','", values));
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
					foreach (DynamicObject dynamicObject in bomOtherChs)
					{
						if (num == base.View.Model.GetEntryRowCount("FEntityMainItems"))
						{
							base.View.Model.CreateNewEntryRow("FEntityMainItems");
						}
						base.View.Model.SetValue("FBOMRowId", DataEntityExtend.GetDynamicValue<string>(dynamicObject, "FRowId", null), num);
						base.View.Model.SetValue("FMaterialID", DataEntityExtend.GetDynamicValue<long>(dynamicObject, "FMaterialID2", 0L), num);
						base.View.Model.SetValue("FAuxPropID", DataEntityExtend.GetDynamicValue<long>(dynamicObject, "FAuxPropID", 0L), num);
						base.View.Model.SetValue("FMainSupplyOrgId", DataEntityExtend.GetDynamicValue<long>(dynamicObject, "FChildSupplyOrgId", 0L), num);
						base.View.Model.SetValue("FBomId", DataEntityExtend.GetDynamicValue<long>(dynamicObject, "FBomId2", 0L), num);
						base.View.Model.SetValue("FBaseUnitID", DataEntityExtend.GetDynamicValue<long>(dynamicObject, "FBaseUnitID1", 0L), num);
						base.View.Model.SetValue("FUnitID", DataEntityExtend.GetDynamicValue<long>(dynamicObject, "FUnitID2", 0L), num);
						base.View.Model.SetValue("FNumerator", DataEntityExtend.GetDynamicValue<decimal>(dynamicObject, "FNumerator", 0m), num);
						base.View.Model.SetValue("FDenominator", DataEntityExtend.GetDynamicValue<decimal>(dynamicObject, "FDenominator", 0m), num);
						base.View.Model.SetValue("FBaseNumerator", DataEntityExtend.GetDynamicValue<decimal>(dynamicObject, "FBaseNumerator", 0m), num);
						base.View.Model.SetValue("FBaseDenominator", DataEntityExtend.GetDynamicValue<decimal>(dynamicObject, "FBASEDENOMINATOR", 0m), num);
						string value = MFGBillUtil.GetValue<string>(base.View.Model, "FReplaceType", -1, null, null);
						bool value2 = MFGBillUtil.GetValue<bool>(base.View.Model, "FIsKeyItem", -1, false, null);
						if (value == "3" && value2)
						{
							base.View.Model.SetValue("FNETDEMANDRATE", DataEntityExtend.GetDynamicValue<decimal>(dynamicObject, "FUseRate", 0m), num);
						}
						num++;
					}
				}
			});
		}

		// Token: 0x06000ECA RID: 3786 RVA: 0x000ABBC0 File Offset: 0x000A9DC0
		private DynamicObjectCollection GetBomOtherChs(List<long> bomEntryId)
		{
			QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter
			{
				FormId = "PRD_PPBOM",
				SelectItems = SelectorItemInfo.CreateItems(new string[]
				{
					"FENTITY_FENTRYID AS FENTRYID",
					"FRowId",
					"FMaterialID2",
					"FBaseUnitID1",
					"FUnitID2",
					"FBomId2",
					"FAuxPropID",
					"FNumerator",
					"FDenominator",
					"FBaseNumerator",
					"FBASEDENOMINATOR",
					"FUseRate",
					"FChildSupplyOrgId"
				})
			};
			ExtJoinTableDescription item = new ExtJoinTableDescription
			{
				TableName = "table(fn_StrSplit(@spPKValue,',',1))",
				TableNameAs = "sp",
				FieldName = "FId",
				ScourceKey = "FENTITY_FENTRYID"
			};
			queryBuilderParemeter.ExtJoinTables.Add(item);
			queryBuilderParemeter.SqlParams.Add(new SqlParam("@spPKValue", 161, bomEntryId.Distinct<long>().ToArray<long>()));
			return MFGServiceHelper.GetDynamicObjectCollection(base.Context, queryBuilderParemeter, null);
		}

		// Token: 0x06000ECB RID: 3787 RVA: 0x000ABE4C File Offset: 0x000AA04C
		protected override bool ValidateData()
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
			if ((from w in list2
			where DataEntityExtend.GetDynamicValue<DateTime>(w, "ExpireDate", default(DateTime)) < DataEntityExtend.GetDynamicValue<DateTime>(w, "EffectDate", default(DateTime))
			select w).Count<DynamicObject>() > 0)
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
				stringBuilder.AppendLine("替代方式不为按比例，主物料或替代料中存在净需求比例大于零的数据");
			}
			decimal d = dynamicValue.Sum((DynamicObject s) => DataEntityExtend.GetDynamicValue<decimal>(s, "NETDEMANDRATE", 0m));
			decimal d2 = dynamicValue2.Sum((DynamicObject s) => DataEntityExtend.GetDynamicValue<decimal>(s, "SUBNETDEMANDRATE", 0m));
			if (value3 == "3" && d + d2 != 100m)
			{
				stringBuilder.AppendLine("替代方式为按比例，主物料或替代料中净需求比例之和不等于100%");
			}
			if (stringBuilder.Length > 0)
			{
				base.View.ShowErrMessage(stringBuilder.ToString(), "", 0);
				return false;
			}
			base.View.Model.DataChanged = false;
			return true;
		}

		// Token: 0x040006BE RID: 1726
		private Dictionary<string, string> dictDelError = new Dictionary<string, string>();

		// Token: 0x040006BF RID: 1727
		private HashSet<string> pushedIds = new HashSet<string>();
	}
}
