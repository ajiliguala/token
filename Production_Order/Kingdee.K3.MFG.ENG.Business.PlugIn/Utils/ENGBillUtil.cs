using System;
using System.Collections.Generic;
using System.Linq;
using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Core.Permission;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.MFG.ENG.BomExpand;
using Kingdee.K3.Core.MFG.ENG.ParamOption;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.MFG.ServiceHelper;
using Kingdee.K3.MFG.ServiceHelper.ENG;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.Utils
{
	// Token: 0x020000D4 RID: 212
	public static class ENGBillUtil
	{
		// Token: 0x06000EE2 RID: 3810 RVA: 0x000AC7E4 File Offset: 0x000AA9E4
		public static void SetPrdOrgField(Context ctx, OrgField orgField, string formId, string permItem)
		{
			List<long> permtedOrgList = ENGBillUtil.GetPermtedOrgList(ctx, formId, permItem);
			if (permtedOrgList.Count > 0)
			{
				orgField.Filter = string.Format("FORGID IN ({0})", string.Join<long>(",", permtedOrgList));
				return;
			}
			orgField.Filter = "FORGID = 0";
		}

		// Token: 0x06000EE3 RID: 3811 RVA: 0x000AC82C File Offset: 0x000AAA2C
		private static List<long> GetPermtedOrgList(Context ctx, string formId, string permItem)
		{
			List<long> list = new List<long>();
			List<SelectorItemInfo> list2 = new List<SelectorItemInfo>();
			list2.Add(new SelectorItemInfo("FORGID"));
			string filterClauseWihtKey = " FDOCUMENTSTATUS = 'C' AND FFORBIDSTATUS = 'A' AND FORGFUNCTIONS LIKE '%104%' ";
			QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter
			{
				FormId = "ORG_Organizations",
				SelectItems = list2,
				FilterClauseWihtKey = filterClauseWihtKey,
				RequiresDataPermission = false
			};
			DynamicObjectCollection dynamicObjectCollection = QueryServiceHelper.GetDynamicObjectCollection(ctx, queryBuilderParemeter, null);
			List<long> permissionOrg = PermissionServiceHelper.GetPermissionOrg(ctx, new BusinessObject
			{
				Id = formId
			}, permItem);
			foreach (DynamicObject dynamicObject in dynamicObjectCollection)
			{
				if (permissionOrg.Contains(Convert.ToInt64(dynamicObject["FORGID"])))
				{
					list.Add(Convert.ToInt64(dynamicObject["FORGID"]));
				}
			}
			return list;
		}

		// Token: 0x06000EE4 RID: 3812 RVA: 0x000AC96C File Offset: 0x000AAB6C
		public static void ShowBomIntegrity(this IDynamicFormView view, List<long> bomIds, List<long> allMaterialIds)
		{
			if (ListUtils.IsEmpty<long>(allMaterialIds))
			{
				return;
			}
			List<long> list = new List<long>();
			List<DynamicObject> list2 = new List<DynamicObject>();
			OperateOption operateOption = OperateOption.Create();
			if (!ListUtils.IsEmpty<long>(bomIds))
			{
				long[] array = bomIds.Distinct<long>().ToArray<long>();
				string sqlWithCardinality = StringUtils.GetSqlWithCardinality(array.Length, "@BomIds", 1, true);
				QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter
				{
					FormId = "ENG_BOM",
					SelectItems = SelectorItemInfo.CreateItems("FID,FMaterialId,FUseOrgId,FUnitId,FBaseUnitId")
				};
				queryBuilderParemeter.ExtJoinTables.Add(new ExtJoinTableDescription
				{
					FieldName = "FID",
					ScourceKey = "FID",
					TableName = sqlWithCardinality,
					TableNameAs = "TT"
				});
				List<SqlParam> list3 = new List<SqlParam>();
				list3.Add(new SqlParam("@BomIds", 161, array));
				DynamicObjectCollection dynamicObjectCollection = QueryServiceHelper.GetDynamicObjectCollection(view.Context, queryBuilderParemeter, list3);
				if (!ListUtils.IsEmpty<DynamicObject>(dynamicObjectCollection))
				{
					foreach (DynamicObject dynamicObject in dynamicObjectCollection)
					{
						BomForwardSourceDynamicRow bomForwardSourceDynamicRow = BomForwardSourceDynamicRow.CreateInstance();
						bomForwardSourceDynamicRow.BomId_Id = DataEntityExtend.GetDynamicValue<long>(dynamicObject, "FID", 0L);
						bomForwardSourceDynamicRow.MaterialId_Id = DataEntityExtend.GetDynamicValue<long>(dynamicObject, "FMaterialId", 0L);
						bomForwardSourceDynamicRow.DemandOrgId_Id = DataEntityExtend.GetDynamicValue<long>(dynamicObject, "FUseOrgId", 0L);
						bomForwardSourceDynamicRow.UnitId_Id = DataEntityExtend.GetDynamicValue<long>(dynamicObject, "FUnitId", 0L);
						bomForwardSourceDynamicRow.BaseUnitId_Id = DataEntityExtend.GetDynamicValue<long>(dynamicObject, "FBaseUnitId", 0L);
						list2.Add(bomForwardSourceDynamicRow.DataEntity);
						allMaterialIds.Add(bomForwardSourceDynamicRow.MaterialId_Id);
					}
					MemBomExpandOption memBomExpandOption = new MemBomExpandOption();
					memBomExpandOption.BomExpandId = Guid.NewGuid().ToString();
					memBomExpandOption.SysDate = MFGServiceHelper.GetSysDate(view.Context);
					memBomExpandOption.IsConvertUnitQty = false;
					memBomExpandOption.BomExpandCalType = 2;
					memBomExpandOption.Mode = 1;
					memBomExpandOption.CsdSubstitution = true;
					memBomExpandOption.DeleteVirtualMaterial = false;
					memBomExpandOption.ExpandLevelTo = 0;
					memBomExpandOption.CsdMtrlPlanStrategy = true;
					memBomExpandOption.DeleteSkipRow = false;
					memBomExpandOption.DeleteExpireMaterial = false;
					memBomExpandOption.ExpandVirtualMaterial = true;
					memBomExpandOption.IsExpandForbidden = false;
					memBomExpandOption.ValidDate = new DateTime?(memBomExpandOption.SysDate);
					DynamicObject dynamicObject2 = BomExpandServiceHelper.ExpandBomForwardMen(view.Context, list2, memBomExpandOption);
					DynamicObjectCollection dynamicObjectCollection2 = dynamicObject2["BomExpandResult"] as DynamicObjectCollection;
					long[] array2 = (from s in dynamicObjectCollection2
					select DataEntityExtend.GetDynamicValue<long>(DataEntityExtend.GetDynamicValue<DynamicObject>(s, "MaterialId", null), "MsterId", 0L)).Distinct<long>().ToArray<long>();
					if (!ListUtils.IsEmpty<long>(array2))
					{
						string sqlWithCardinality2 = StringUtils.GetSqlWithCardinality(array2.Length, "@FMtrlMastID", 1, true);
						QueryBuilderParemeter queryBuilderParemeter2 = new QueryBuilderParemeter
						{
							FormId = "BD_MATERIAL",
							SelectItems = SelectorItemInfo.CreateItems("FMATERIALID,FMASTERID,FUSEORGID")
						};
						queryBuilderParemeter2.ExtJoinTables.Add(new ExtJoinTableDescription
						{
							FieldName = "FID",
							ScourceKey = "FMASTERID",
							TableName = sqlWithCardinality2,
							TableNameAs = "TT"
						});
						List<SqlParam> list4 = new List<SqlParam>();
						list4.Add(new SqlParam("@FMtrlMastID", 161, array2));
						DynamicObjectCollection dynamicObjectCollection3 = QueryServiceHelper.GetDynamicObjectCollection(view.Context, queryBuilderParemeter2, list4);
						Dictionary<long, IGrouping<long, DynamicObject>> dictionary = (from g in dynamicObjectCollection3
						group g by DataEntityExtend.GetDynamicValue<long>(g, "FMASTERID", 0L)).ToDictionary((IGrouping<long, DynamicObject> d) => d.Key);
						foreach (DynamicObject dynamicObject3 in dynamicObjectCollection2)
						{
							long dynamicValue = DataEntityExtend.GetDynamicValue<long>(dynamicObject3, "BomId_Id", 0L);
							DynamicObject dynamicValue2 = DataEntityExtend.GetDynamicValue<DynamicObject>(dynamicObject3, "BomId", null);
							long supplyOrgId = DataEntityExtend.GetDynamicValue<long>(dynamicObject3, "SupplyOrgId_Id", 0L);
							long dynamicValue3 = DataEntityExtend.GetDynamicValue<long>(dynamicObject3, "MaterialId_Id", 0L);
							DynamicObject dynamicValue4 = DataEntityExtend.GetDynamicValue<DynamicObject>(dynamicObject3, "MaterialId", null);
							long key = (dynamicValue4 == null) ? dynamicValue3 : DataEntityExtend.GetDynamicValue<long>(dynamicValue4, "MsterId", 0L);
							IGrouping<long, DynamicObject> source;
							if (dynamicValue2 != null)
							{
								dynamicValue3 = DataEntityExtend.GetDynamicValue<long>(dynamicValue2, "MaterialId_Id", 0L);
							}
							else if (supplyOrgId > 0L && dictionary.TryGetValue(key, out source))
							{
								DynamicObject dynamicObject4 = (from w in source
								where DataEntityExtend.GetDynamicValue<long>(w, "FUSEORGID", 0L) == supplyOrgId
								select w).FirstOrDefault<DynamicObject>();
								if (dynamicObject4 != null)
								{
									dynamicValue3 = DataEntityExtend.GetDynamicValue<long>(dynamicObject4, "FMATERIALID", 0L);
								}
							}
							allMaterialIds.Add(dynamicValue3);
							if (dynamicValue == 0L && !ObjectUtils.IsNullOrEmpty(dynamicValue2))
							{
								dynamicValue = DataEntityExtend.GetDynamicValue<long>(dynamicValue2, "Id", 0L);
							}
							if (dynamicValue > 0L)
							{
								list.Add(dynamicValue);
							}
						}
					}
				}
			}
			operateOption.SetVariableValue("materialIds", allMaterialIds.Distinct<long>().ToList<long>());
			if (!ListUtils.IsEmpty<long>(list))
			{
				operateOption.SetVariableValue("bomIds", list.Distinct<long>().ToList<long>());
			}
			DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
			dynamicFormShowParameter.FormId = "PLN_MAINTAINLLC";
			dynamicFormShowParameter.ParentPageId = view.PageId;
			dynamicFormShowParameter.MultiSelect = false;
			if (view.Session.ContainsKey("isFromBOM"))
			{
				view.Session["isFromBOM"] = true;
			}
			else
			{
				view.Session.Add("isFromBOM", true);
			}
			if (view.Session.ContainsKey("MaterialIdsForIntegrityCheck"))
			{
				view.Session["MaterialIdsForIntegrityCheck"] = operateOption;
			}
			else
			{
				view.Session.Add("MaterialIdsForIntegrityCheck", operateOption);
			}
			view.ShowForm(dynamicFormShowParameter);
		}
	}
}
