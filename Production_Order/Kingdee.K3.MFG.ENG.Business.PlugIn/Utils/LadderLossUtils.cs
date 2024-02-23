using System;
using System.Collections.Generic;
using System.Linq;
using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.Log;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Permission;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.BD.ServiceHelper;
using Kingdee.K3.Core.BD;
using Kingdee.K3.Core.BD.ServiceArgs;
using Kingdee.K3.Core.MFG.Utils;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.Utils
{
	// Token: 0x020000D5 RID: 213
	public static class LadderLossUtils
	{
		// Token: 0x06000EE8 RID: 3816 RVA: 0x000ACF64 File Offset: 0x000AB164
		public static DynamicObject GetDataObject(Context ctx, long Fid, string formId)
		{
			FormMetadata formMetadata = MetaDataServiceHelper.Load(ctx, formId, true) as FormMetadata;
			QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter();
			queryBuilderParemeter.BusinessInfo = formMetadata.BusinessInfo;
			queryBuilderParemeter.FormId = formId;
			queryBuilderParemeter.FilterClauseWihtKey = string.Format("FID = {0}", Fid);
			return BusinessDataServiceHelper.Load(ctx, formMetadata.BusinessInfo.GetDynamicObjectType(), queryBuilderParemeter).FirstOrDefault<DynamicObject>();
		}

		// Token: 0x06000EE9 RID: 3817 RVA: 0x000ACFC8 File Offset: 0x000AB1C8
		public static DynamicObject[] GetDataObject(Context ctx, object[] Fids, string formId)
		{
			FormMetadata formMetadata = MetaDataServiceHelper.Load(ctx, formId, true) as FormMetadata;
			return BusinessDataServiceHelper.Load(ctx, Fids, formMetadata.BusinessInfo.GetDynamicObjectType());
		}

		// Token: 0x06000EEA RID: 3818 RVA: 0x000ACFF8 File Offset: 0x000AB1F8
		public static DynamicObject[] GetLadderLoss(Context ctx, long fmaterialId)
		{
			FormMetadata formMetadata = MetaDataServiceHelper.Load(ctx, "ENG_LadderLoss", true) as FormMetadata;
			QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter();
			queryBuilderParemeter.BusinessInfo = formMetadata.BusinessInfo;
			queryBuilderParemeter.FormId = "ENG_LadderLoss";
			queryBuilderParemeter.FilterClauseWihtKey = string.Format("FMATERIALID = {0} and FUSEORGID = {1}", fmaterialId, ctx.CurrentOrganizationInfo.ID);
			return BusinessDataServiceHelper.Load(ctx, formMetadata.BusinessInfo.GetDynamicObjectType(), queryBuilderParemeter);
		}

		// Token: 0x06000EEB RID: 3819 RVA: 0x000AD08C File Offset: 0x000AB28C
		public static IOperationResult UpdateBom(Context ctx, Dictionary<string, object> returnData, DynamicObject dataObject, DynamicObjectCollection lossEntries)
		{
			new List<IOperationResult>();
			List<LadderLossUtils.DataList> list = returnData["datalist"] as List<LadderLossUtils.DataList>;
			List<DynamicObject> list2 = new List<DynamicObject>();
			FormMetadata formMetadata = MetaDataServiceHelper.Load(ctx, "ENG_BOM", true) as FormMetadata;
			object[] fids = (from x in list
			select x.idList).ToArray<object>();
			Dictionary<long, DynamicObject> dictionary = LadderLossUtils.GetDataObject(ctx, fids, "ENG_BOM").ToDictionary((DynamicObject x) => Convert.ToInt64(OrmUtils.GetPrimaryKeyValue(x, true)));
			Dictionary<long, DynamicObject> dictionary2 = new Dictionary<long, DynamicObject>();
			foreach (KeyValuePair<long, DynamicObject> keyValuePair in dictionary)
			{
				DynamicObjectCollection dynamicObjectCollection = keyValuePair.Value["TreeEntity"] as DynamicObjectCollection;
				foreach (DynamicObject dynamicObject in dynamicObjectCollection)
				{
					dictionary2.Add(Convert.ToInt64(OrmUtils.GetPrimaryKeyValue(dynamicObject, true)), dynamicObject);
				}
			}
			Dictionary<string, Dictionary<long, UnitConvert>> baseUnitConvertRateList = LadderLossUtils.GetBaseUnitConvertRateList(ctx, dictionary, dictionary2, list, dataObject);
			Dictionary<long, UnitConvert> dictionary3 = baseUnitConvertRateList["bomUnitConvert"];
			Dictionary<long, UnitConvert> dictionary4 = baseUnitConvertRateList["bomEntryUnitConvert"];
			Dictionary<long, UnitConvert> dictionary5 = baseUnitConvertRateList["bomEntryBaseUnitConvert"];
			for (int i = 0; i < list.Count; i++)
			{
				DynamicObject dynamicObject2 = dictionary[list[i].idList];
				object obj = dynamicObject2["MATERIALID"];
				object obj2 = dynamicObject2["FUNITID"];
				object obj3 = dynamicObject2["BaseUnitId"];
				object obj4 = dynamicObject2["TreeEntity"];
				DynamicObject dynamicObject3 = null;
				if (dictionary2.TryGetValue(list[i].entryList, out dynamicObject3))
				{
					DynamicObjectCollection dynamicObjectCollection2 = dynamicObject3["BOMCHILDLOTBASEDQTY"] as DynamicObjectCollection;
					dynamicObjectCollection2.Clear();
					UnitConvert unitConvert = null;
					dictionary3.TryGetValue(list[i].idList, out unitConvert);
					object obj5 = dynamicObject3["MATERIALIDCHILD"];
					object obj6 = dynamicObject3["CHILDUNITID"];
					object obj7 = dynamicObject3["ChildBaseUnitID"];
					UnitConvert unitConvert2 = null;
					dictionary4.TryGetValue(list[i].entryList, out unitConvert2);
					UnitConvert unitConvert3 = null;
					dictionary5.TryGetValue(list[i].entryList, out unitConvert3);
					foreach (DynamicObject dynamicObject4 in lossEntries)
					{
						DynamicObject dynamicObject5 = new DynamicObject(dynamicObjectCollection2.DynamicCollectionItemPropertyType);
						dynamicObject5["MATERIALIDLOTBASED_ID"] = (dataObject["MATERIALID_ID"].ToString() ?? "0");
						dynamicObject5["MATERIALIDLOTBASED"] = dataObject["MATERIALID"];
						dynamicObject5["STARTQTY"] = dynamicObject4["STARTQTY"];
						dynamicObject5["ENDQTY"] = dynamicObject4["ENDQTY"];
						dynamicObject5["UNITIDLOT"] = dynamicObject3["CHILDUNITID"];
						dynamicObject5["UNITIDLOT_ID"] = dynamicObject3["CHILDUNITID_Id"];
						dynamicObject5["BASEUNITIDLOT"] = dynamicObject3["ChildBaseUnitID"];
						dynamicObject5["BASEUNITIDLOT_ID"] = dynamicObject3["ChildBaseUnitID_ID"];
						dynamicObject5["FIXSCRAPQTYLOT"] = (ObjectUtils.IsNullOrEmpty(unitConvert2) ? dynamicObject4["FIXSCRAPQTY"] : unitConvert2.ConvertQty(Convert.ToDecimal(dynamicObject4["FIXSCRAPQTY"] ?? 0m), ""));
						dynamicObject5["SCRAPRATELOT"] = dynamicObject4["SCRAPRATE"];
						dynamicObject5["NOTELOT"] = dynamicObject4["NOTE"];
						dynamicObject5["SEQ"] = dynamicObject4["SEQ"];
						dynamicObject5["NUMERATORLOT"] = dynamicObject3["NUMERATOR"];
						dynamicObject5["DENOMINATORLOT"] = dynamicObject3["DENOMINATOR"];
						dynamicObject5["BASENUMERATORLOT"] = dynamicObject3["BaseNumerator"];
						dynamicObject5["BASEDENOMINATORLOT"] = dynamicObject3["BaseDenominator"];
						dynamicObject5["BASESTARTQTY"] = (ObjectUtils.IsNullOrEmpty(unitConvert) ? dynamicObject4["STARTQTY"] : unitConvert.ConvertQty(Convert.ToDecimal(dynamicObject4["STARTQTY"] ?? 0m), ""));
						dynamicObject5["BASEENDQTY"] = (ObjectUtils.IsNullOrEmpty(unitConvert) ? dynamicObject4["ENDQTY"] : unitConvert.ConvertQty(Convert.ToDecimal(dynamicObject4["ENDQTY"] ?? 0m), ""));
						dynamicObject5["BASEFIXSCRAPQTYLOT"] = (ObjectUtils.IsNullOrEmpty(unitConvert3) ? dynamicObject4["FIXSCRAPQTY"] : unitConvert3.ConvertQty(Convert.ToDecimal(dynamicObject4["FIXSCRAPQTY"] ?? 0m), ""));
						dynamicObjectCollection2.Add(dynamicObject5);
					}
					dynamicObject3["ECNChgDate"] = TimeServiceHelper.GetSystemDateTime(ctx);
					dynamicObject3["ChangeTime"] = TimeServiceHelper.GetSystemDateTime(ctx);
					if (Convert.ToInt64(dynamicObject3["DOSAGETYPE"]) != 3L)
					{
						dynamicObject3["DOSAGETYPE"] = 3;
					}
					if (!list2.Contains(dynamicObject2))
					{
						list2.Add(dynamicObject2);
					}
				}
			}
			return BusinessDataServiceHelper.Save(ctx, formMetadata.BusinessInfo, list2.ToArray(), null, "");
		}

		// Token: 0x06000EEC RID: 3820 RVA: 0x000AD768 File Offset: 0x000AB968
		public static List<IOperationResult> UpdateLadderLoss(Context ctx, IDynamicFormView view, Dictionary<string, object> returnData)
		{
			LadderLossUtils.<>c__DisplayClass12 CS$<>8__locals1 = new LadderLossUtils.<>c__DisplayClass12();
			List<IOperationResult> list = new List<IOperationResult>();
			CS$<>8__locals1.dataList = (returnData["datalist"] as List<LadderLossUtils.DataList>);
			FormMetadata formMetadata = MetaDataServiceHelper.Load(ctx, "ENG_LadderLoss", true) as FormMetadata;
			OperateOption operateOption = OperateOption.Create();
			List<DynamicObject> list2 = new List<DynamicObject>();
			List<DynamicObject> list3 = new List<DynamicObject>();
			int i;
			for (i = 0; i < CS$<>8__locals1.dataList.Count; i++)
			{
				DynamicObject[] ladderLoss = LadderLossUtils.GetLadderLoss(ctx, CS$<>8__locals1.dataList[i].materialidList);
				if (ladderLoss.Length == 1)
				{
					DynamicObject dataObject = LadderLossUtils.GetDataObject(ctx, CS$<>8__locals1.dataList[i].idList, "ENG_BOM");
					DynamicObjectCollection source = dataObject["TreeEntity"] as DynamicObjectCollection;
					DynamicObject dynamicObject = (from p in source
					where Convert.ToInt64(p["id"]) == CS$<>8__locals1.dataList[i].entryList
					select p).FirstOrDefault<DynamicObject>();
					DynamicObjectCollection dynamicObjectCollection = dynamicObject["BOMCHILDLOTBASEDQTY"] as DynamicObjectCollection;
					DynamicObjectCollection dynamicObjectCollection2 = ladderLoss[0]["LADDERLOSSENTRY"] as DynamicObjectCollection;
					dynamicObjectCollection2.Clear();
					foreach (DynamicObject dynamicObject2 in dynamicObjectCollection)
					{
						DynamicObject dynamicObject3 = new DynamicObject(dynamicObjectCollection2.DynamicCollectionItemPropertyType);
						dynamicObject3["SEQ"] = dynamicObject2["SEQ"];
						dynamicObject3["STARTQTY"] = dynamicObject2["STARTQTY"];
						dynamicObject3["ENDQTY"] = dynamicObject2["ENDQTY"];
						dynamicObject3["FIXSCRAPQTY"] = dynamicObject2["FIXSCRAPQTYLOT"];
						dynamicObject3["SCRAPRATE"] = dynamicObject2["SCRAPRATELOT"];
						dynamicObject3["NOTE"] = dynamicObject2["NOTELOT"];
						dynamicObjectCollection2.Add(dynamicObject3);
					}
					list2.Add(ladderLoss[0]);
				}
				else
				{
					DynamicObject dynamicObject4 = new DynamicObject(formMetadata.BusinessInfo.GetDynamicObjectType());
					dynamicObject4["MATERIALID_Id"] = CS$<>8__locals1.dataList[i].materialidList;
					dynamicObject4["FORBIDSTATUS"] = "A";
					dynamicObject4["CreateOrgId_Id"] = ctx.CurrentOrganizationInfo.ID;
					dynamicObject4["UseOrgId_Id"] = ctx.CurrentOrganizationInfo.ID;
					dynamicObject4["CreatorId_Id"] = ctx.UserId;
					dynamicObject4["CreateDate"] = TimeServiceHelper.GetSystemDateTime(ctx);
					dynamicObject4["MODIFIERID_Id"] = ctx.UserId;
					dynamicObject4["MODIFYDate"] = TimeServiceHelper.GetSystemDateTime(ctx);
					dynamicObject4["UnitID_Id"] = CS$<>8__locals1.dataList[i].unitList;
					dynamicObject4["DocumentStatus"] = "A";
					DynamicObject dataObject2 = LadderLossUtils.GetDataObject(ctx, CS$<>8__locals1.dataList[i].idList, "ENG_BOM");
					DynamicObjectCollection source2 = dataObject2["TreeEntity"] as DynamicObjectCollection;
					DynamicObject dynamicObject5 = (from p in source2
					where Convert.ToInt64(p["id"]) == CS$<>8__locals1.dataList[i].entryList
					select p).FirstOrDefault<DynamicObject>();
					DynamicObjectCollection dynamicObjectCollection3 = dynamicObject5["BOMCHILDLOTBASEDQTY"] as DynamicObjectCollection;
					DynamicObjectCollection dynamicObjectCollection4 = dynamicObject4["LADDERLOSSENTRY"] as DynamicObjectCollection;
					foreach (DynamicObject dynamicObject6 in dynamicObjectCollection3)
					{
						DynamicObject dynamicObject7 = new DynamicObject(dynamicObjectCollection4.DynamicCollectionItemPropertyType);
						dynamicObject7["SEQ"] = dynamicObject6["SEQ"];
						dynamicObject7["STARTQTY"] = dynamicObject6["STARTQTY"];
						dynamicObject7["ENDQTY"] = dynamicObject6["ENDQTY"];
						dynamicObject7["FIXSCRAPQTY"] = dynamicObject6["FIXSCRAPQTYLOT"];
						dynamicObject7["SCRAPRATE"] = dynamicObject6["SCRAPRATELOT"];
						dynamicObject7["NOTE"] = dynamicObject6["NOTELOT"];
						dynamicObjectCollection4.Add(dynamicObject7);
					}
					list3.Add(dynamicObject4);
				}
			}
			if (list2.Count > 0)
			{
				IOperationResult item = BusinessDataServiceHelper.Save(ctx, formMetadata.BusinessInfo, list2.ToArray(), null, "");
				list.Add(item);
			}
			if (list3.Count > 0)
			{
				bool userParam = MFGBillUtil.GetUserParam<bool>(view, "FSaveAndSubmit", false);
				bool userParam2 = MFGBillUtil.GetUserParam<bool>(view, "FSubmitAndAudit", false);
				IOperationResult operationResult = BusinessDataServiceHelper.Save(ctx, formMetadata.BusinessInfo, list3.ToArray(), null, "");
				if (userParam)
				{
					IEnumerable<object> source3 = from e in operationResult.OperateResult
					where e.SuccessStatus
					select e into x
					select x.PKValue;
					if (source3.Count<object>() > 0)
					{
						IOperationResult operationResult2 = BusinessDataServiceHelper.Submit(ctx, formMetadata.BusinessInfo, source3.ToArray<object>(), 6.ToString(), operateOption.Copy());
						if (userParam2)
						{
							IEnumerable<object> source4 = from e in operationResult2.OperateResult
							where e.SuccessStatus
							select e into x
							select x.PKValue;
							if (source4.Count<object>() > 0)
							{
								IOperationResult item2 = BusinessDataServiceHelper.Audit(ctx, formMetadata.BusinessInfo, source4.ToArray<object>(), operateOption.Copy());
								list.Add(item2);
							}
							else
							{
								list.Add(operationResult2);
							}
						}
						else
						{
							list.Add(operationResult2);
						}
					}
					else
					{
						list.Add(operationResult);
					}
				}
				else
				{
					list.Add(operationResult);
				}
			}
			return list;
		}

		// Token: 0x06000EED RID: 3821 RVA: 0x000ADE20 File Offset: 0x000AC020
		public static bool IsUpdatePermission(Context ctx)
		{
			PermissionAuthResult permissionAuthResult = PermissionServiceHelper.FuncPermissionAuth(ctx, new BusinessObject
			{
				Id = "ENG_LadderLoss"
			}, "6189e0d8d8f552");
			return permissionAuthResult.Passed;
		}

		// Token: 0x06000EEE RID: 3822 RVA: 0x000ADE54 File Offset: 0x000AC054
		public static bool IsGetDataPermission(Context ctx)
		{
			PermissionAuthResult permissionAuthResult = PermissionServiceHelper.FuncPermissionAuth(ctx, new BusinessObject
			{
				Id = "ENG_LadderLoss"
			}, "6189e22a05529e");
			return permissionAuthResult.Passed;
		}

		// Token: 0x06000EEF RID: 3823 RVA: 0x000ADE88 File Offset: 0x000AC088
		public static bool IsModifyPermisson(Context ctx, string formId)
		{
			PermissionAuthResult permissionAuthResult = PermissionServiceHelper.FuncPermissionAuth(ctx, new BusinessObject
			{
				Id = formId
			}, "f323992d896745fbaab4a2717c79ce2e");
			return permissionAuthResult.Passed;
		}

		// Token: 0x06000EF0 RID: 3824 RVA: 0x000ADEB8 File Offset: 0x000AC0B8
		public static void WriteOperateLog(Context ctx, string opName, string description, BusinessInfo businessInfo, string formId)
		{
			LogServiceHelper.WriteLog(ctx, new LogObject
			{
				SubSystemId = businessInfo.GetForm().SubsysId,
				ObjectTypeId = formId,
				Environment = 3,
				OperateName = opName,
				Description = description
			});
		}

		// Token: 0x06000EF1 RID: 3825 RVA: 0x000ADF18 File Offset: 0x000AC118
		private static Dictionary<string, Dictionary<long, UnitConvert>> GetBaseUnitConvertRateList(Context ctx, Dictionary<long, DynamicObject> bomDictionary, Dictionary<long, DynamicObject> bomEntryDictionary, List<LadderLossUtils.DataList> dataList, DynamicObject dataObject)
		{
			Dictionary<long, UnitConvert> value = new Dictionary<long, UnitConvert>();
			Dictionary<long, UnitConvert> value2 = new Dictionary<long, UnitConvert>();
			Dictionary<long, UnitConvert> value3 = new Dictionary<long, UnitConvert>();
			List<GetUnitConvertRateArgs> list = new List<GetUnitConvertRateArgs>();
			List<GetUnitConvertRateArgs> list2 = new List<GetUnitConvertRateArgs>();
			List<GetUnitConvertRateArgs> list3 = new List<GetUnitConvertRateArgs>();
			for (int i = 0; i < dataList.Count; i++)
			{
				DynamicObject dynamicObject = bomDictionary[dataList[i].idList];
				object obj = dynamicObject["TreeEntity"];
				DynamicObject dynamicObject2 = null;
				if (bomEntryDictionary.TryGetValue(dataList[i].entryList, out dynamicObject2))
				{
					DynamicObject dynamicObject3 = dynamicObject["MATERIALID"] as DynamicObject;
					DynamicObject dynamicObject4 = dynamicObject["FUNITID"] as DynamicObject;
					DynamicObject dynamicObject5 = dynamicObject["BaseUnitid"] as DynamicObject;
					DynamicObject dynamicObject6 = dynamicObject2["MATERIALIDCHILD"] as DynamicObject;
					DynamicObject dynamicObject7 = dynamicObject2["CHILDUNITID"] as DynamicObject;
					DynamicObject dynamicObject8 = dynamicObject2["ChildBaseUnitID"] as DynamicObject;
					List<long> list4 = (from s in list
					select s.PrimaryKey).ToList<long>();
					List<long> list5 = (from s in list2
					select s.PrimaryKey).ToList<long>();
					List<long> list6 = (from s in list3
					select s.PrimaryKey).ToList<long>();
					long num = Convert.ToInt64(OrmUtils.GetPrimaryKeyValue(dynamicObject, true));
					long num2 = Convert.ToInt64(OrmUtils.GetPrimaryKeyValue(dynamicObject2, true));
					if (!list4.Contains(num) && dynamicObject4["id"] != dynamicObject5["id"])
					{
						list.Add(new GetUnitConvertRateArgs
						{
							PrimaryKey = num,
							MaterialId = Convert.ToInt64(dynamicObject3["id"]),
							MasterId = Convert.ToInt64(dynamicObject3["msterID"]),
							SourceUnitId = Convert.ToInt64(dynamicObject4["id"]),
							DestUnitId = Convert.ToInt64(dynamicObject5["id"])
						});
					}
					if (!list5.Contains(num2) && dataObject["UnitID_Id"] != dynamicObject7["id"])
					{
						list2.Add(new GetUnitConvertRateArgs
						{
							PrimaryKey = num2,
							MaterialId = Convert.ToInt64(dynamicObject6["id"]),
							MasterId = Convert.ToInt64(dynamicObject6["msterID"]),
							SourceUnitId = Convert.ToInt64(dataObject["UnitID_Id"]),
							DestUnitId = Convert.ToInt64(dynamicObject7["id"])
						});
					}
					if (!list6.Contains(num2) && dataObject["UnitID_Id"] != dynamicObject8["id"])
					{
						list3.Add(new GetUnitConvertRateArgs
						{
							PrimaryKey = num2,
							MaterialId = Convert.ToInt64(dynamicObject6["id"]),
							MasterId = Convert.ToInt64(dynamicObject6["msterID"]),
							SourceUnitId = Convert.ToInt64(dataObject["UnitID_Id"]),
							DestUnitId = Convert.ToInt64(dynamicObject8["id"])
						});
					}
				}
			}
			if (!ListUtils.IsEmpty<GetUnitConvertRateArgs>(list))
			{
				value = UnitConvertServiceHelper.GetUnitConvertRateList(ctx, list);
			}
			if (!ListUtils.IsEmpty<GetUnitConvertRateArgs>(list2))
			{
				value2 = UnitConvertServiceHelper.GetUnitConvertRateList(ctx, list2);
			}
			if (!ListUtils.IsEmpty<GetUnitConvertRateArgs>(list3))
			{
				value3 = UnitConvertServiceHelper.GetUnitConvertRateList(ctx, list3);
			}
			return new Dictionary<string, Dictionary<long, UnitConvert>>
			{
				{
					"bomUnitConvert",
					value
				},
				{
					"bomEntryUnitConvert",
					value2
				},
				{
					"bomEntryBaseUnitConvert",
					value3
				}
			};
		}

		// Token: 0x020000D6 RID: 214
		public class DataList
		{
			// Token: 0x170000AC RID: 172
			// (get) Token: 0x06000EFD RID: 3837 RVA: 0x000AE2FB File Offset: 0x000AC4FB
			// (set) Token: 0x06000EFE RID: 3838 RVA: 0x000AE303 File Offset: 0x000AC503
			public long idList { get; set; }

			// Token: 0x170000AD RID: 173
			// (get) Token: 0x06000EFF RID: 3839 RVA: 0x000AE30C File Offset: 0x000AC50C
			// (set) Token: 0x06000F00 RID: 3840 RVA: 0x000AE314 File Offset: 0x000AC514
			public long entryList { get; set; }

			// Token: 0x170000AE RID: 174
			// (get) Token: 0x06000F01 RID: 3841 RVA: 0x000AE31D File Offset: 0x000AC51D
			// (set) Token: 0x06000F02 RID: 3842 RVA: 0x000AE325 File Offset: 0x000AC525
			public long materialidList { get; set; }

			// Token: 0x170000AF RID: 175
			// (get) Token: 0x06000F03 RID: 3843 RVA: 0x000AE32E File Offset: 0x000AC52E
			// (set) Token: 0x06000F04 RID: 3844 RVA: 0x000AE336 File Offset: 0x000AC536
			public long unitList { get; set; }
		}
	}
}
