using System;
using System.Collections.Generic;
using System.Linq;
using Kingdee.BOS;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.MFG.ENG.BomExpand;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.MFG.BusinessCommon;
using Kingdee.K3.MFG.ServiceHelper.ENG;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.ECNControler
{
	// Token: 0x020000D1 RID: 209
	public class SyncPPBomControler : AbstractItemControler
	{
		// Token: 0x06000E9B RID: 3739 RVA: 0x000A989C File Offset: 0x000A7A9C
		public override void DoOperation()
		{
			if (!DataEntityExtend.GetDynamicValue<bool>(base.Model.DataObject, "IsUpdatePPBom", false))
			{
				base.View.ShowErrMessage("", ResManager.LoadKDString("未勾选同步用料清单，不需要做同步操作", "015072000018155", 7, new object[0]), 0);
				return;
			}
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

		// Token: 0x06000E9C RID: 3740 RVA: 0x000A99F0 File Offset: 0x000A7BF0
		private void DoOperationOnBill()
		{
			List<long> list = new List<long>();
			List<long> list2 = new List<long>();
			List<DynamicObject> list3 = new List<DynamicObject>();
			DynamicObjectCollection source = base.Model.DataObject["TreeEntity"] as DynamicObjectCollection;
			object[] array = (from x in source
			where DataEntityExtend.GetDynamicValue<bool>(x, "IsNeedSync", false)
			select x into row
			select row["BomVersion_Id"]).Distinct<object>().ToArray<object>();
			if (ListUtils.IsEmpty<object>(array))
			{
				base.View.ShowErrMessage("", ResManager.LoadKDString("没有需要做同步用料清单的数据", "015072000018469", 7, new object[0]), 0);
				return;
			}
			list3.AddRange(BusinessDataServiceHelper.Load(base.View.Context, array, base.BomMeta.BusinessInfo.GetDynamicObjectType()));
			list3 = (from w in list3
			where DataEntityExtend.GetDynamicValue<string>(w, "DocumentStatus", null) == "C" && DataEntityExtend.GetDynamicValue<string>(w, "ForbidStatus", null) == "A"
			select w).ToList<DynamicObject>();
			if (list3.Count == 0)
			{
				base.View.ShowErrMessage(ResManager.LoadKDString("选择的BOM数据状态不为已审核或禁用状态为禁用，请重新选择数据", "015072000018123", 7, new object[0]), "", 0);
				return;
			}
			list = (from x in list3
			select DataEntityExtend.GetDynamicValue<long>(x, "Id", 0L)).Distinct<long>().ToList<long>();
			list2 = (from x in list3
			select DataEntityExtend.GetDynamicValue<long>(x, "UseOrgId_Id", 0L)).Distinct<long>().ToList<long>();
			FormMetadata formMetadata = (FormMetadata)MetaDataServiceHelper.Load(base.View.Context, "ENG_BOM_BILLPARAM", true);
			DynamicObject dynamicObject = UserParamterServiceHelper.Load(base.View.Context, formMetadata.BusinessInfo, base.View.Context.UserId, "ENG_BOM", "UserParameter");
			string dynamicValue = DataEntityExtend.GetDynamicValue<string>(dynamicObject, "UpdateRange", null);
			string dynamicValue2 = DataEntityExtend.GetDynamicValue<string>(dynamicObject, "ConSultDate", null);
			bool dynamicValue3 = DataEntityExtend.GetDynamicValue<bool>(dynamicObject, "IsSkipExpand", false);
			List<long> bomMasterIds = new List<long>();
			if (dynamicValue == "2")
			{
				bomMasterIds = (from w in list3
				where DataEntityExtend.GetDynamicValue<long>(w, "UseOrgId_Id", 0L) == DataEntityExtend.GetDynamicValue<long>(w, "CreateOrgId_Id", 0L)
				select w into s
				select DataEntityExtend.GetDynamicValue<long>(s, "MsterId", 0L)).ToList<long>();
				List<DynamicObject> allocatedBOM = this.GetAllocatedBOM(bomMasterIds);
				if (!ListUtils.IsEmpty<DynamicObject>(allocatedBOM))
				{
					list2.AddRange((from s in allocatedBOM
					select DataEntityExtend.GetDynamicValue<long>(s, "UseOrgId_Id", 0L)).ToList<long>().Except(list2));
					list.AddRange((from s in allocatedBOM
					select DataEntityExtend.GetDynamicValue<long>(s, "Id", 0L)).ToList<long>().Except(list));
					foreach (DynamicObject dynamicObject2 in allocatedBOM)
					{
						long bomId = DataEntityExtend.GetDynamicValue<long>(dynamicObject2, "Id", 0L);
						if (!list3.Any((DynamicObject a) => DataEntityExtend.GetDynamicValue<long>(a, "Id", 0L) == bomId))
						{
							list3.Add(dynamicObject2);
						}
					}
				}
			}
			List<long> first = new List<long>();
			List<long> bomBackwardByVirtualBom = BomSyncBackwardUtil.GetBomBackwardByVirtualBom(base.View.Context, list3, ref first);
			if (!ListUtils.IsEmpty<long>(bomBackwardByVirtualBom))
			{
				list.AddRange(bomBackwardByVirtualBom.Except(list));
				list2.AddRange(first.Except(list2));
				DynamicObject[] source2 = BusinessDataServiceHelper.Load(base.View.Context, (from i in list.Distinct<long>()
				select i).ToArray<object>(), base.View.BusinessInfo.GetDynamicObjectType());
				list3 = source2.ToList<DynamicObject>();
			}
			DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
			dynamicFormShowParameter.OpenStyle.ShowType = 4;
			dynamicFormShowParameter.FormId = "ENG_SYNSUPDATEPPBOM";
			dynamicFormShowParameter.CustomComplexParams.Add("BomData", list3);
			dynamicFormShowParameter.CustomComplexParams.Add("BomId", list);
			dynamicFormShowParameter.CustomComplexParams.Add("UserOrgId", list2);
			dynamicFormShowParameter.CustomComplexParams.Add("IsShowPrdList", true);
			dynamicFormShowParameter.CustomComplexParams.Add("IsShowSubList", true);
			dynamicFormShowParameter.CustomComplexParams.Add("ConSultDate", dynamicValue2);
			dynamicFormShowParameter.CustomComplexParams.Add("IsSkipExpand", dynamicValue3);
			dynamicFormShowParameter.CustomComplexParams.Add("IsShowPlnList", true);
			base.View.ShowForm(dynamicFormShowParameter);
		}

		// Token: 0x06000E9D RID: 3741 RVA: 0x000A9EDC File Offset: 0x000A80DC
		private void DoOperationOnList()
		{
			new List<string>();
			new List<long>();
			new List<DynamicObject>();
			ListSelectedRowCollection selectedRowsInfo = ((IListView)base.View).SelectedRowsInfo;
			QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter
			{
				FormId = "ENG_ECNOrder",
				SelectItems = SelectorItemInfo.CreateItems(new string[]
				{
					"FID",
					"FDocumentStatus",
					"FEffectStatus",
					"FTreeEntity_FEntryId",
					"FUpdateVersion",
					"FIsNeedSync"
				})
			};
			queryBuilderParemeter.ExtJoinTables.Add(new ExtJoinTableDescription
			{
				FieldName = "FID",
				TableName = "TABLE(fn_StrSplit(@ECNID,',',1))",
				TableNameAs = "TS",
				ScourceKey = "FID"
			});
			long[] array = (from x in selectedRowsInfo
			select OtherExtend.ConvertTo<long>(x.PrimaryKeyValue, 0L)).Distinct<long>().ToArray<long>();
			List<SqlParam> list = new List<SqlParam>
			{
				new SqlParam("@ECNID", 161, array)
			};
			QueryServiceHelper.GetDynamicObjectCollection(base.View.Context, queryBuilderParemeter, list);
		}

		// Token: 0x06000E9E RID: 3742 RVA: 0x000AA014 File Offset: 0x000A8214
		private List<DynamicObject> GetAllocatedBOM(List<long> bomMasterIds)
		{
			FormMetadata formMetadata = (FormMetadata)MetaDataServiceHelper.Load(base.View.Context, "ENG_BOM", true);
			QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter();
			queryBuilderParemeter.BusinessInfo = formMetadata.BusinessInfo;
			queryBuilderParemeter.ExtJoinTables.Add(new ExtJoinTableDescription
			{
				TableName = "table(fn_StrSplit(@msterId,',',1))",
				TableNameAs = "tms",
				FieldName = "FID",
				ScourceKey = "FMASTERID"
			});
			queryBuilderParemeter.FilterClauseWihtKey = "FCreateOrgId<>FUseOrgId";
			queryBuilderParemeter.SqlParams.Add(new SqlParam("@msterId", 161, bomMasterIds.Distinct<long>().ToArray<long>()));
			return BusinessDataServiceHelper.Load(base.View.Context, queryBuilderParemeter.BusinessInfo.GetDynamicObjectType(), queryBuilderParemeter).ToList<DynamicObject>();
		}

		// Token: 0x06000E9F RID: 3743 RVA: 0x000AA0E0 File Offset: 0x000A82E0
		private List<DynamicObject> BuildBomExpandSourceData(IEnumerable<long> materialIds)
		{
			List<DynamicObject> list = new List<DynamicObject>();
			if (!ListUtils.IsEmpty<long>(materialIds))
			{
				foreach (long materialId_Id in materialIds)
				{
					BomBackwardSourceDynamicRow bomBackwardSourceDynamicRow = BomBackwardSourceDynamicRow.CreateInstance();
					bomBackwardSourceDynamicRow.MaterialId_Id = materialId_Id;
					bomBackwardSourceDynamicRow.AuxPropId = 0L;
					list.Add(bomBackwardSourceDynamicRow.DataEntity);
				}
			}
			return list;
		}

		// Token: 0x06000EA0 RID: 3744 RVA: 0x000AA220 File Offset: 0x000A8420
		private void GetBomResultsByBomId(KeyValuePair<string, List<DynamicObject>> bomResultDatas)
		{
			string[] array = bomResultDatas.Key.Split(new string[]
			{
				"^_^"
			}, StringSplitOptions.None);
			Convert.ToInt64(array[0]);
			long msterId = Convert.ToInt64(array[1]);
			long bomId = Convert.ToInt64(array[2]);
			long num = Convert.ToInt64(array[3]);
			long hightVersionBomKey = BOMServiceHelper.GetHightVersionBomKey(base.View.Context, msterId, num, 0L);
			List<DynamicObject> list = (from w in bomResultDatas.Value
			where Convert.ToInt64(w["BomLevel"]) == 1L
			select w).ToList<DynamicObject>();
			List<long> list2 = new List<long>();
			List<DynamicObject> list3 = new List<DynamicObject>();
			Dictionary<long, IGrouping<long, DynamicObject>> bomExpandResult = (from x in bomResultDatas.Value
			group x by DataEntityExtend.GetDynamicValue<long>(x, "ParentEntryId", 0L)).ToDictionary((IGrouping<long, DynamicObject> x) => x.Key);
			foreach (DynamicObject dynamicObject in list)
			{
				long dynamicValue = DataEntityExtend.GetDynamicValue<long>(dynamicObject, "EntryId", 0L);
				DynamicObject dynamicValue2 = DataEntityExtend.GetDynamicValue<DynamicObject>(dynamicObject, "BomId", null);
				DynamicObjectCollection dynamicValue3 = DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(dynamicValue2, "TreeEntity", null);
				List<long> list4 = new List<long>();
				if (hightVersionBomKey == bomId)
				{
					if (!dynamicValue3.Any((DynamicObject a) => DataEntityExtend.GetDynamicValue<long>(DataEntityExtend.GetDynamicValue<DynamicObject>(a, "MATERIALIDCHILD", null), "msterID", 0L) == msterId && (DataEntityExtend.GetDynamicValue<long>(a, "BOMID_Id", 0L) == 0L || DataEntityExtend.GetDynamicValue<long>(a, "BOMID_Id", 0L) == bomId)))
					{
						list3.Add(dynamicObject);
						list4.Add(dynamicValue);
						list2.AddRange(list4);
						this.dgFindBom(list4, list2, bomExpandResult, list3);
					}
				}
				else if (!dynamicValue3.Any((DynamicObject a) => DataEntityExtend.GetDynamicValue<long>(DataEntityExtend.GetDynamicValue<DynamicObject>(a, "MATERIALIDCHILD", null), "msterID", 0L) == msterId && DataEntityExtend.GetDynamicValue<long>(a, "BOMID_Id", 0L) == bomId))
				{
					list3.Add(dynamicObject);
					list4.Add(dynamicValue);
					list2.AddRange(list4);
					this.dgFindBom(list4, list2, bomExpandResult, list3);
				}
			}
			foreach (DynamicObject item in list3)
			{
				bomResultDatas.Value.Remove(item);
			}
		}

		// Token: 0x06000EA1 RID: 3745 RVA: 0x000AA500 File Offset: 0x000A8700
		private void FindParentBom(List<DynamicObject> bomResultDatas)
		{
			if (ListUtils.IsEmpty<DynamicObject>(bomResultDatas))
			{
				return;
			}
			List<DynamicObject> list = (from w in bomResultDatas
			where Convert.ToInt16(w["BomLevel"]) > 0
			select w).ToList<DynamicObject>();
			List<long> list2 = new List<long>();
			List<Tuple<long, long, long>> list3 = new List<Tuple<long, long, long>>();
			foreach (DynamicObject dynamicObject in list)
			{
				DynamicObject dynamicValue = DataEntityExtend.GetDynamicValue<DynamicObject>(dynamicObject, "BomId", null);
				DynamicObject dynamicValue2 = DataEntityExtend.GetDynamicValue<DynamicObject>(dynamicObject, "MaterialId2", null);
				long useOrgId = DataEntityExtend.GetDynamicValue<long>(dynamicValue, "UseOrgId_Id", 0L);
				long msterId = DataEntityExtend.GetDynamicValue<long>(dynamicValue2, "MsterId", 0L);
				long auxPropId = DataEntityExtend.GetDynamicValue<long>(dynamicValue, "ParentAuxPropId_Id", 0L);
				if (!list3.Any((Tuple<long, long, long> p) => p.Item1 == msterId && p.Item2 == useOrgId && p.Item3 == auxPropId))
				{
					list3.Add(new Tuple<long, long, long>(msterId, useOrgId, auxPropId));
				}
			}
			List<DynamicObject> source = BOMServiceHelper.GetHightVersionBom(base.View.Context, list3).ToList<DynamicObject>();
			List<long> list4 = (from s in source
			select Convert.ToInt64(s["Id"])).ToList<long>();
			List<DynamicObject> list5 = new List<DynamicObject>();
			foreach (DynamicObject dynamicObject2 in from o in list
			orderby o["BomLevel"]
			select o)
			{
				long dynamicValue3 = DataEntityExtend.GetDynamicValue<long>(dynamicObject2, "EntryId", 0L);
				if (list2.Contains(dynamicValue3))
				{
					list5.Add(dynamicObject2);
				}
				DataEntityExtend.GetDynamicValue<DynamicObject>(dynamicObject2, "MaterialId2", null);
				long dynamicValue4 = DataEntityExtend.GetDynamicValue<long>(dynamicObject2, "BomId_ID", 0L);
				if (!list4.Contains(dynamicValue4) && !list2.Contains(dynamicValue3))
				{
					this.findParentBomByBomId(dynamicValue3, dynamicValue4, list2, bomResultDatas, list5);
				}
			}
			foreach (DynamicObject item in list5)
			{
				bomResultDatas.Remove(item);
			}
		}

		// Token: 0x06000EA2 RID: 3746 RVA: 0x000AA7F8 File Offset: 0x000A89F8
		private void findParentBomByBomId(long rowId, long bomId, List<long> bomRowIds, List<DynamicObject> bomExpandResult, List<DynamicObject> removeBomDatas)
		{
			List<DynamicObject> source = (from w in bomExpandResult
			where DataEntityExtend.GetDynamicValue<long>(w, "ParentEntryId", 0L) == rowId
			select w).ToList<DynamicObject>();
			List<long> list = new List<long>();
			List<DynamicObject> list2 = (from s in source
			select DataEntityExtend.GetDynamicValue<DynamicObject>(s, "BomId", null)).ToList<DynamicObject>();
			Dictionary<long, List<DynamicObject>> dictionary = (from i in source
			group i by DataEntityExtend.GetDynamicValue<long>(i, "BomId_Id", 0L)).ToDictionary((IGrouping<long, DynamicObject> k) => k.Key, (IGrouping<long, DynamicObject> v) => v.ToList<DynamicObject>());
			foreach (DynamicObject dynamicObject in list2)
			{
				DynamicObjectCollection dynamicValue = DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(dynamicObject, "TreeEntity", null);
				if (!dynamicValue.Any((DynamicObject w) => DataEntityExtend.GetDynamicValue<long>(w, "BOMID_Id", 0L) == bomId))
				{
					long dynamicValue2 = DataEntityExtend.GetDynamicValue<long>(dynamicObject, "Id", 0L);
					List<DynamicObject> source2 = null;
					if (dictionary.TryGetValue(dynamicValue2, out source2))
					{
						IEnumerable<long> collection = from i in source2
						select DataEntityExtend.GetDynamicValue<long>(i, "EntryId", 0L);
						list.AddRange(collection);
					}
				}
			}
			if (ListUtils.IsEmpty<long>(list))
			{
				return;
			}
			bomRowIds.AddRange(list);
			Dictionary<long, IGrouping<long, DynamicObject>> bomExpandResult2 = (from x in bomExpandResult
			group x by DataEntityExtend.GetDynamicValue<long>(x, "ParentEntryId", 0L)).ToDictionary((IGrouping<long, DynamicObject> x) => x.Key);
			this.dgFindBom(list, bomRowIds, bomExpandResult2, removeBomDatas);
		}

		// Token: 0x06000EA3 RID: 3747 RVA: 0x000AA9FC File Offset: 0x000A8BFC
		private void dgFindBom(List<long> rowIds, List<long> bomRowIds, Dictionary<long, IGrouping<long, DynamicObject>> bomExpandResult, List<DynamicObject> removeBomDatas)
		{
			foreach (long key in rowIds)
			{
				List<long> list = new List<long>();
				IGrouping<long, DynamicObject> grouping;
				if (bomExpandResult.TryGetValue(key, out grouping))
				{
					removeBomDatas.AddRange(grouping);
					list = (from x in grouping.ToList<DynamicObject>()
					select DataEntityExtend.GetDynamicValue<long>(x, "EntryId", 0L)).ToList<long>();
					list = list.Except(rowIds).ToList<long>();
				}
				bomRowIds.AddRange(list);
				this.dgFindBom(list, bomRowIds, bomExpandResult, removeBomDatas);
			}
		}

		// Token: 0x06000EA4 RID: 3748 RVA: 0x000AAAC4 File Offset: 0x000A8CC4
		private Dictionary<long, List<DynamicObject>> GetEveryMtrlBomResult(IEnumerable<DynamicObject> bomQueryResult)
		{
			Dictionary<long, List<DynamicObject>> dictionary = new Dictionary<long, List<DynamicObject>>();
			if (!ListUtils.IsEmpty<DynamicObject>(bomQueryResult))
			{
				Dictionary<string, IGrouping<string, DynamicObject>> dicQueryDatas = (from i in bomQueryResult
				group i by DataEntityExtend.GetDynamicValue<string>(i, "ParentEntryId", null)).ToDictionary((IGrouping<string, DynamicObject> d) => d.Key);
				foreach (DynamicObject dynamicObject in bomQueryResult)
				{
					if (DataEntityExtend.GetDynamicValue<int>(dynamicObject, "BomLevel", 0) == 0)
					{
						string dynamicValue = DataEntityExtend.GetDynamicValue<string>(dynamicObject, "EntryId", null);
						long dynamicValue2 = DataEntityExtend.GetDynamicValue<long>(dynamicObject, "MaterialId2_Id", 0L);
						if (dictionary.ContainsKey(dynamicValue2))
						{
							dictionary[dynamicValue2].Add(dynamicObject);
						}
						else
						{
							dictionary.Add(dynamicValue2, new List<DynamicObject>
							{
								dynamicObject
							});
						}
						this.GetBomGroupResult(dynamicValue, dynamicValue2, dicQueryDatas, dictionary);
					}
				}
			}
			return dictionary;
		}

		// Token: 0x06000EA5 RID: 3749 RVA: 0x000AABCC File Offset: 0x000A8DCC
		private void GetBomGroupResult(string rowId, long materialId, Dictionary<string, IGrouping<string, DynamicObject>> dicQueryDatas, Dictionary<long, List<DynamicObject>> result)
		{
			IGrouping<string, DynamicObject> grouping;
			if (dicQueryDatas.TryGetValue(rowId, out grouping))
			{
				foreach (DynamicObject dynamicObject in grouping)
				{
					if (result.ContainsKey(materialId))
					{
						result[materialId].Add(dynamicObject);
					}
					else
					{
						result.Add(materialId, new List<DynamicObject>
						{
							dynamicObject
						});
					}
					string dynamicValue = DataEntityExtend.GetDynamicValue<string>(dynamicObject, "EntryId", null);
					this.GetBomGroupResult(dynamicValue, materialId, dicQueryDatas, result);
				}
			}
		}

		// Token: 0x06000EA6 RID: 3750 RVA: 0x000AAC80 File Offset: 0x000A8E80
		private void GetBomByMtrlErpClsID(IEnumerable<DynamicObject> lstQueryResult, List<long> addBomDatas, List<long> addBomUseOrgIds)
		{
			Dictionary<string, IGrouping<string, DynamicObject>> dicQueryResult = (from i in lstQueryResult
			group i by DataEntityExtend.GetDynamicValue<string>(i, "ParentEntryId", null)).ToDictionary((IGrouping<string, DynamicObject> d) => d.Key);
			foreach (DynamicObject dynamicObject in lstQueryResult)
			{
				if (DataEntityExtend.GetDynamicValue<int>(dynamicObject, "BomLevel", 0) == 0)
				{
					string dynamicValue = DataEntityExtend.GetDynamicValue<string>(dynamicObject, "EntryId", null);
					this.QueryChildBomEntry(dynamicValue, dicQueryResult, addBomDatas, addBomUseOrgIds);
				}
			}
		}

		// Token: 0x06000EA7 RID: 3751 RVA: 0x000AAD2C File Offset: 0x000A8F2C
		private void QueryChildBomEntry(string rowId, Dictionary<string, IGrouping<string, DynamicObject>> dicQueryResult, List<long> bomIds, List<long> bomUseOrgIds)
		{
			IGrouping<string, DynamicObject> grouping;
			if (dicQueryResult.TryGetValue(rowId, out grouping))
			{
				foreach (DynamicObject dynamicObject in grouping)
				{
					string dynamicValue = DataEntityExtend.GetDynamicValue<string>(DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(DataEntityExtend.GetDynamicValue<DynamicObject>(dynamicObject, "MaterialId2", null), "MaterialBase", null).FirstOrDefault<DynamicObject>(), "ErpClsID", null);
					if (StringUtils.EqualsIgnoreCase(dynamicValue, "5"))
					{
						bomIds.Add(DataEntityExtend.GetDynamicValue<long>(dynamicObject, "BomId_Id", 0L));
						bomUseOrgIds.Add(DataEntityExtend.GetDynamicValue<long>(DataEntityExtend.GetDynamicValue<DynamicObject>(dynamicObject, "BomId", null), "UseOrgId_Id", 0L));
						string dynamicValue2 = DataEntityExtend.GetDynamicValue<string>(dynamicObject, "EntryId", null);
						this.QueryChildBomEntry(dynamicValue2, dicQueryResult, bomIds, bomUseOrgIds);
					}
					else
					{
						bomIds.Add(DataEntityExtend.GetDynamicValue<long>(dynamicObject, "BomId_Id", 0L));
						bomUseOrgIds.Add(DataEntityExtend.GetDynamicValue<long>(DataEntityExtend.GetDynamicValue<DynamicObject>(dynamicObject, "BomId", null), "UseOrgId_Id", 0L));
					}
				}
			}
		}
	}
}
