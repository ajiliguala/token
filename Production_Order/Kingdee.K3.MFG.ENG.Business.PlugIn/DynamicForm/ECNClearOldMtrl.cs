using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Kingdee.BOS;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.NetworkCtrl;
using Kingdee.BOS.Core.Permission;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.BD.ServiceHelper;
using Kingdee.K3.Core.BD;
using Kingdee.K3.Core.BD.ServiceArgs;
using Kingdee.K3.Core.MFG.ENG.ECN;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.Core.MFG.Utils;
using Kingdee.K3.MFG.ServiceHelper;
using Kingdee.K3.MFG.ServiceHelper.ENG;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm
{
	// Token: 0x0200008C RID: 140
	[Description("ECN旧料清理")]
	public class ECNClearOldMtrl : AbstractDynamicFormPlugIn
	{
		// Token: 0x06000A6F RID: 2671 RVA: 0x00078EA4 File Offset: 0x000770A4
		public override void BeforeBindData(EventArgs e)
		{
			base.BeforeBindData(e);
			if (base.Context.CurrentOrganizationInfo.FunctionIds.Contains(103L))
			{
				this.Model.SetValue("FStockOrgIds", base.Context.CurrentOrganizationInfo.ID);
			}
			this.SetDefStockStatusValue();
		}

		// Token: 0x06000A70 RID: 2672 RVA: 0x00078F00 File Offset: 0x00077100
		public override void BeforeDoOperation(BeforeDoOperationEventArgs e)
		{
			base.BeforeDoOperation(e);
			if ((e.Operation.FormOperation.IsEqualOperation("Print") || e.Operation.FormOperation.IsEqualOperation("EntityExport")) && !MFGCommonUtil.AuthPermissionBeforeShowF7Form(this.View, this.View.BillBusinessInfo.GetForm().Id, e.Operation.FormOperation.PermissionItemId))
			{
				this.View.ShowErrMessage(string.Format(ResManager.LoadKDString("没有旧料清理的{0}权限", "015072000012084", 7, new object[0]), e.Operation.FormOperation.OperationName[base.Context.UserLocale.LCID]), "", 0);
				e.Cancel = true;
			}
		}

		// Token: 0x06000A71 RID: 2673 RVA: 0x00078FD4 File Offset: 0x000771D4
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			base.BarItemClick(e);
			if (StringUtils.EqualsIgnoreCase(e.BarItemKey, "tbBScheduleClearMtrl"))
			{
				long value = MFGBillUtil.GetValue<long>(this.View.Model, "FChangeOrgId", -1, 0L, null);
				object value2 = this.View.Model.GetValue("FStockOrgIds");
				object value3 = this.View.Model.GetValue("FStockStatus");
				DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
				dynamicFormShowParameter.FormId = "ENG_ClearOldMtrlScheduleParam";
				dynamicFormShowParameter.ParentPageId = this.View.PageId;
				dynamicFormShowParameter.CustomParams.Add("ENG_ClearOldMtrlScheduleParam_ChangeOrgId", value.ToString());
				dynamicFormShowParameter.CustomParams.Add("ENG_ClearOldMtrlScheduleParam_Name", ResManager.LoadKDString("旧料调度清理", "015072000016562", 7, new object[0]));
				dynamicFormShowParameter.CustomParams.Add("ENG_ClearOldMtrlScheduleParam_Description", string.Empty);
				dynamicFormShowParameter.CustomParams.Add("ENG_ClearOldMtrlScheduleParam_PluginClass", "Kingdee.K3.MFG.ENG.App.Core.BacksageSchedule.ClearOldMtrlSchedule,Kingdee.K3.MFG.ENG.App.Core");
				dynamicFormShowParameter.CustomComplexParams.Add("ENG_ClearOldMtrlScheduleParam_StockOrgIds", value2);
				dynamicFormShowParameter.CustomComplexParams.Add("ENG_ClearOldMtrlScheduleParam_StockStatus", value3);
				this.View.ShowForm(dynamicFormShowParameter);
			}
		}

		// Token: 0x06000A72 RID: 2674 RVA: 0x000790FC File Offset: 0x000772FC
		public override void AfterBarItemClick(AfterBarItemClickEventArgs e)
		{
			base.AfterBarItemClick(e);
			string barItemKey;
			if ((barItemKey = e.BarItemKey) != null)
			{
				if (barItemKey == "tbRefresh")
				{
					this.DoRefresh(false);
					return;
				}
				if (barItemKey == "tbBuild")
				{
					this.DoBuild();
					return;
				}
				if (!(barItemKey == "tbQuit"))
				{
					return;
				}
				this.DoQuit();
			}
		}

		// Token: 0x06000A73 RID: 2675 RVA: 0x00079158 File Offset: 0x00077358
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			base.BeforeF7Select(e);
			string filter = string.Empty;
			string fieldKey;
			if ((fieldKey = e.FieldKey) != null)
			{
				if (fieldKey == "FStockOrgIds")
				{
					filter = " CHARINDEX('103', FORGFUNCTIONS)>0";
					e.ListFilterParameter.Filter = this.SqlAppendAnd(e.ListFilterParameter.Filter, filter);
					return;
				}
				if (fieldKey == "FECNNo")
				{
					this.ShowECNListForm(e);
					return;
				}
				if (fieldKey == "FParentMaterialIds")
				{
					filter = this.SetParentMaterilIdFilterString();
					e.ListFilterParameter.Filter = this.SqlAppendAnd(e.ListFilterParameter.Filter, filter);
					return;
				}
				if (fieldKey == "FChildMaterialIds")
				{
					filter = this.SetChildMaterilIdFilterString();
					e.ListFilterParameter.Filter = this.SqlAppendAnd(e.ListFilterParameter.Filter, filter);
					return;
				}
				if (!(fieldKey == "FChangeOrgId"))
				{
					return;
				}
				if (MFGServiceHelper.IsEnabledForbidOrgQuery(base.Context))
				{
					e.IsShowUsed = false;
				}
			}
		}

		// Token: 0x06000A74 RID: 2676 RVA: 0x0007924B File Offset: 0x0007744B
		public override void EntityRowClick(EntityRowClickEventArgs e)
		{
			base.EntityRowClick(e);
			if (StringUtils.EqualsIgnoreCase(e.Key, "FEntity"))
			{
				this.LoadECNStockInfo(e.Row);
				this.LoadPredictInStockInfo(e.Row);
			}
		}

		// Token: 0x06000A75 RID: 2677 RVA: 0x00079280 File Offset: 0x00077480
		public override void EntryButtonCellClick(EntryButtonCellClickEventArgs e)
		{
			base.EntryButtonCellClick(e);
			string fieldKey;
			if ((fieldKey = e.FieldKey) != null)
			{
				if (!(fieldKey == "FECNNUMBER"))
				{
					return;
				}
				string text = string.Empty;
				string value = MFGBillUtil.GetValue<string>(this.View.Model, "FECNNumber", e.Row, "", null);
				if (!string.IsNullOrWhiteSpace(value))
				{
					BillShowParameter billShowParameter = new BillShowParameter();
					billShowParameter.FormId = "ENG_ECNOrder";
					billShowParameter.OpenStyle.ShowType = 6;
					billShowParameter.Status = 1;
					if (this.billKeyCache.TryGetValue(value, out text))
					{
						billShowParameter.PKey = text;
					}
					else
					{
						text = this.QueryEcnIdByBillNo(value);
						if (ObjectUtils.IsNullOrEmptyOrWhiteSpace(text))
						{
							this.View.ShowMessage(string.Format(ResManager.LoadKDString("工程变更单号{0}对应的单据不存在！", "015072030038266", 7, new object[0]), value), 0);
							return;
						}
						billShowParameter.PKey = text;
					}
					this.View.ShowForm(billShowParameter);
				}
			}
		}

		// Token: 0x06000A76 RID: 2678 RVA: 0x0007936D File Offset: 0x0007756D
		public override void BeforeClosed(BeforeClosedEventArgs e)
		{
			base.BeforeClosed(e);
			this.CommitNetworkCtrl();
		}

		// Token: 0x06000A77 RID: 2679 RVA: 0x0007937C File Offset: 0x0007757C
		public override void BeforeSetItemValueByNumber(BeforeSetItemValueByNumberArgs e)
		{
			base.BeforeSetItemValueByNumber(e);
			string baseDataFieldKey;
			if ((baseDataFieldKey = e.BaseDataFieldKey) != null)
			{
				if (!(baseDataFieldKey == "FStockOrgIds"))
				{
					return;
				}
				if (MFGServiceHelper.IsEnabledForbidOrgQuery(base.Context))
				{
					e.IsShowUsed = false;
				}
			}
		}

		// Token: 0x06000A78 RID: 2680 RVA: 0x00079514 File Offset: 0x00077714
		private void ShowECNListForm(BeforeF7SelectEventArgs e)
		{
			ListSelBillShowParameter listSelBillShowParameter = new ListSelBillShowParameter
			{
				FormId = "ENG_ECNOrder",
				PermissionItemId = "6e44119a58cb4a8e86f6c385e14a17ad",
				IsShowApproved = true,
				IsLookUp = true,
				MultiSelect = true,
				ListFilterParameter = new ListRegularFilterParameter
				{
					Filter = this.SetECNNoFilterString(),
					OrderBy = " FBILLNO DESC"
				}
			};
			this.View.ShowForm(listSelBillShowParameter, delegate(FormResult result)
			{
				if (result != null && result.ReturnData != null && (result.ReturnData as ListSelectedRowCollection).Count > 0)
				{
					ListSelectedRowCollection listSelectedRowCollection = (ListSelectedRowCollection)result.ReturnData;
					List<string> list = new List<string>();
					Dictionary<string, string> dictionary = new Dictionary<string, string>();
					foreach (ListSelectedRow listSelectedRow in listSelectedRowCollection)
					{
						if (!list.Contains(listSelectedRow.BillNo))
						{
							list.Add(listSelectedRow.BillNo);
							dictionary.Add(listSelectedRow.BillNo, listSelectedRow.PrimaryKeyValue);
						}
					}
					int num = 100;
					object obj = 100;
					if (e.CustomParams.TryGetValue("maxCount", out obj))
					{
						num = OtherExtend.ConvertTo<int>(obj, 0);
					}
					if (list.Count > num)
					{
						this.View.ShowErrMessage(ResManager.LoadKDString("选择的工程变更单请勿超过10张。", "015072000019429", 7, new object[0]), "", 0);
						return;
					}
					this.selectedECNBillNos = list;
					this.billKeyCache = dictionary;
					this.Model.SetValue("FECNNo", string.Join(";", list));
				}
			});
		}

		// Token: 0x06000A79 RID: 2681 RVA: 0x000795A4 File Offset: 0x000777A4
		private string SetECNNoFilterString()
		{
			return string.Format(" FCHANGEORGID={0} AND FECNSTATUS='1' AND FCHANGETYPE='0' AND FISECNCLEAROLDMTRL='0'", DataEntityExtend.GetDynamicValue<long>(this.Model.DataObject, "ChangeOrgId_Id", 0L));
		}

		// Token: 0x06000A7A RID: 2682 RVA: 0x000795CC File Offset: 0x000777CC
		private string SetParentMaterilIdFilterString()
		{
			return string.Format(" FUSEORGID={0} AND FISMAINPRD='1' AND FISECN='1' ", DataEntityExtend.GetDynamicValue<long>(this.Model.DataObject, "ChangeOrgId_Id", 0L));
		}

		// Token: 0x06000A7B RID: 2683 RVA: 0x000795F4 File Offset: 0x000777F4
		private string SetChildMaterilIdFilterString()
		{
			return string.Format(" FUSEORGID={0} ", DataEntityExtend.GetDynamicValue<long>(this.Model.DataObject, "ChangeOrgId_Id", 0L));
		}

		// Token: 0x06000A7C RID: 2684 RVA: 0x0007961C File Offset: 0x0007781C
		private string SqlAppendAnd(string sql, string filter)
		{
			if (string.IsNullOrWhiteSpace(filter))
			{
				return sql;
			}
			return sql + (string.IsNullOrWhiteSpace(sql) ? "" : " AND ") + filter;
		}

		// Token: 0x06000A7D RID: 2685 RVA: 0x00079644 File Offset: 0x00077844
		private void SetDefStockStatusValue()
		{
			if (this.sysStockStatusArr == null)
			{
				this.InitStockStatus();
			}
			PermissionAuthResult permissionAuthResult = PermissionServiceHelper.FuncPermissionAuth(this.View.Context, new BusinessObject
			{
				Id = "BD_StockStatus"
			}, "6e44119a58cb4a8e86f6c385e14a17ad");
			if (permissionAuthResult.Passed)
			{
				DynamicObjectCollection dynamicObjectCollection = this.Model.DataObject["StockStatus"] as DynamicObjectCollection;
				dynamicObjectCollection.Clear();
				foreach (DynamicObject dynamicObject in this.sysStockStatusArr)
				{
					DynamicObject dynamicObject2 = new DynamicObject(dynamicObjectCollection.DynamicCollectionItemPropertyType);
					dynamicObject2["StockStatus_Id"] = dynamicObject["Id"];
					dynamicObject2["StockStatus"] = dynamicObject;
					dynamicObjectCollection.Add(dynamicObject2);
				}
			}
		}

		// Token: 0x06000A7E RID: 2686 RVA: 0x0007970C File Offset: 0x0007790C
		private void InitStockStatus()
		{
			List<SelectorItemInfo> list = new List<SelectorItemInfo>();
			list.Add(new SelectorItemInfo("FSTOCKSTATUSID AS Id"));
			list.Add(new SelectorItemInfo("FNUMBER AS Number"));
			list.Add(new SelectorItemInfo("FNAME AS Name"));
			list.Add(new SelectorItemInfo("FMASTERID AS msterID"));
			OQLFilter oqlfilter = new OQLFilter();
			oqlfilter.Add(new OQLFilterHeadEntityItem
			{
				FilterString = " FSYSDEFAULT='1' AND FTYPE IN('0','1','4') "
			});
			this.sysStockStatusArr = BusinessDataServiceHelper.LoadFromCache(base.Context, "BD_StockStatus", list, oqlfilter);
		}

		// Token: 0x06000A7F RID: 2687 RVA: 0x000797A4 File Offset: 0x000779A4
		private void DoBuild()
		{
			Entity entity = this.View.BusinessInfo.GetEntity("FEntity");
			DynamicObjectCollection entityDataObject = this.View.Model.GetEntityDataObject(entity);
			List<DynamicObject> list = (from w in entityDataObject
			where DataEntityExtend.GetDynamicValue<bool>(w, "CheckBox", false)
			select w).ToList<DynamicObject>();
			if (ListUtils.IsEmpty<DynamicObject>(list))
			{
				this.View.ShowErrMessage(ResManager.LoadKDString("请先选择需要清理的旧料！", "015072000012085", 7, new object[0]), "", 0);
				return;
			}
			List<ECNClearOldMtrlBuildParam> list2 = new List<ECNClearOldMtrlBuildParam>();
			foreach (DynamicObject dynamicObject in list)
			{
				list2.Add(new ECNClearOldMtrlBuildParam
				{
					BomId = DataEntityExtend.GetDynamicValue<long>(dynamicObject, "BomId_Id", 0L),
					ECNNumber = DataEntityExtend.GetDynamicValue<string>(dynamicObject, "ECNNumber", null),
					MaterialId = DataEntityExtend.GetDynamicValue<long>(dynamicObject, "ChildMaterialId_Id", 0L),
					BomEntryId = DataEntityExtend.GetDynamicValue<long>(dynamicObject, "BomEntryId", 0L),
					ECNBillId = DataEntityExtend.GetDynamicValue<long>(dynamicObject, "ECNBillId", 0L),
					ECNEntryId = DataEntityExtend.GetDynamicValue<long>(dynamicObject, "ECNEntryId", 0L)
				});
			}
			long dynamicValue = DataEntityExtend.GetDynamicValue<long>(this.Model.DataObject, "ChangeOrgId_Id", 0L);
			IOperationResult operationResult = ECNClearOldMtrlServiceHelper.BuildSelectTran(base.Context, dynamicValue, list2, true);
			this.View.ShowOperateResult(operationResult.OperateResult, "BOS_BatchTips");
			this.DoRefresh(true);
		}

		// Token: 0x06000A80 RID: 2688 RVA: 0x00079950 File Offset: 0x00077B50
		private void DoRefresh(bool flag = false)
		{
			if (this.LoadECNOldMtrls(flag))
			{
				this.LoadECNStockInfo(0);
				this.LoadPredictInStockInfo(0);
			}
		}

		// Token: 0x06000A81 RID: 2689 RVA: 0x00079969 File Offset: 0x00077B69
		private void DoQuit()
		{
			this.CommitNetworkCtrl();
		}

		// Token: 0x06000A82 RID: 2690 RVA: 0x000799A4 File Offset: 0x00077BA4
		private bool LoadECNOldMtrls(bool flag)
		{
			DateTime dateTime = DataEntityExtend.GetDynamicValue<DateTime>(this.Model.DataObject, "ChangeBeginDate", default(DateTime));
			DateTime dateTime2 = DataEntityExtend.GetDynamicValue<DateTime>(this.Model.DataObject, "ChangeEndDate", default(DateTime));
			if (dateTime > dateTime2)
			{
				this.View.ShowErrMessage(ResManager.LoadKDString("变更的结束日期必须大于等于开始日期", "015072000012087", 7, new object[0]), "", 0);
				return false;
			}
			DateTime dateTime3 = new DateTime(1900, 1, 1);
			if (dateTime < dateTime3)
			{
				dateTime = dateTime3;
			}
			if (dateTime2 < dateTime3)
			{
				dateTime2 = dateTime3;
			}
			ECNClearOldMtrlOption ecnclearOldMtrlOption = new ECNClearOldMtrlOption
			{
				Ctx = base.Context,
				ChangeOrgId = DataEntityExtend.GetDynamicValue<long>(this.Model.DataObject, "ChangeOrgId_Id", 0L),
				StockOrgIds = this.GetMutilSelectedIds("StockOrgIds"),
				StockStatus = this.GetMutilSelectedIds("StockStatus"),
				ChangeBillNos = this.selectedECNBillNos,
				ParentMaterialIds = this.GetMutilSelectedIds("ParentMaterialIds"),
				ChildMaterialIds = this.GetMutilSelectedIds("ChildMaterialIds"),
				ChangeBeginDate = dateTime,
				ChangeEndDate = dateTime2
			};
			DynamicObjectCollection ecnoldMaterialsStockQty = ECNClearOldMtrlServiceHelper.GetECNOldMaterialsStockQty(ecnclearOldMtrlOption);
			if (ListUtils.IsEmpty<DynamicObject>(ecnoldMaterialsStockQty))
			{
				this.View.Model.DeleteEntryData("FEntity");
				this.View.UpdateView("FEntity");
				return false;
			}
			List<long> bomIds = (from s in ecnoldMaterialsStockQty
			select DataEntityExtend.GetDynamicValue<long>(s, "BomId", 0L)).Distinct<long>().ToList<long>();
			List<long> successPkIds = this.StartNetworkCtrl(bomIds, ecnoldMaterialsStockQty, flag);
			List<DynamicObject> list = (from w in ecnoldMaterialsStockQty
			where successPkIds.Contains(DataEntityExtend.GetDynamicValue<long>(w, "BomId", 0L))
			select w).ToList<DynamicObject>();
			Entity entity = this.View.BusinessInfo.GetEntity("FEntity");
			int num = 0;
			if (list.Count <= 0)
			{
				this.View.Model.DeleteEntryData("FEntity");
				this.View.UpdateView("FEntity");
				return false;
			}
			this.View.Model.BeginIniti();
			this.View.Model.DeleteEntryData("FEntity");
			this.View.Model.BatchCreateNewEntryRow("FEntity", list.Count);
			foreach (DynamicObject dynamicObject in list)
			{
				MFGCommonUtil.SetDyFormViewFieldsValue(this.View, entity, dynamicObject, num);
				num++;
			}
			this.View.Model.EndIniti();
			this.View.UpdateView("FEntity");
			return true;
		}

		// Token: 0x06000A83 RID: 2691 RVA: 0x00079C80 File Offset: 0x00077E80
		private void LoadECNStockInfo(int row)
		{
			Entity entity = this.View.BusinessInfo.GetEntity("FEntity");
			DynamicObject entityDataObject = this.View.Model.GetEntityDataObject(entity, row);
			long dynamicValue = DataEntityExtend.GetDynamicValue<long>(entityDataObject, "ChildMaterialId_Id", 0L);
			long[] materialMasterAndUserOrgId = MaterialServiceHelper.GetMaterialMasterAndUserOrgId(base.Context, dynamicValue);
			if (dynamicValue <= 0L)
			{
				this.View.Model.DeleteEntryData("FEntityStock");
				this.View.UpdateView("FEntityStock");
				return;
			}
			long dynamicValue2 = DataEntityExtend.GetDynamicValue<long>(entityDataObject, "ChildBomId_Id", 0L);
			string text = DataEntityExtend.GetDynamicValue<string>(entityDataObject, "BomIsEnable", null);
			if (!ObjectUtils.IsNullOrEmpty(text) && text.ToUpperInvariant() == "TRUE")
			{
				text = "1";
			}
			else
			{
				text = "0";
			}
			long dynamicValue3 = DataEntityExtend.GetDynamicValue<long>(entityDataObject, "AuxPropId_Id", 0L);
			ECNClearStockInfoOption ecnclearStockInfoOption = new ECNClearStockInfoOption
			{
				Ctx = base.Context,
				StockOrgIds = this.GetMutilSelectedIds("StockOrgIds"),
				StockStatus = this.GetMutilSelectedIds("StockStatus"),
				MaterialId = materialMasterAndUserOrgId[0],
				BomId = dynamicValue2,
				AuxPropId = dynamicValue3,
				BomIsEnable = text
			};
			DynamicObjectCollection ecnstockInfo = ECNClearOldMtrlServiceHelper.GetECNStockInfo(ecnclearStockInfoOption);
			if (ListUtils.IsEmpty<DynamicObject>(ecnstockInfo))
			{
				this.View.Model.DeleteEntryData("FEntityStock");
				this.View.UpdateView("FEntityStock");
				return;
			}
			List<GetUnitConvertRateArgs> list = new List<GetUnitConvertRateArgs>();
			int num = 0;
			foreach (DynamicObject dynamicObject in ecnstockInfo)
			{
				list.Add(new GetUnitConvertRateArgs
				{
					PrimaryKey = (long)num++,
					MasterId = DataEntityExtend.GetDynamicValue<long>(dynamicObject, "MaterialId", 0L),
					SourceUnitId = DataEntityExtend.GetDynamicValue<long>(dynamicObject, "BaseStockUnitID", 0L),
					DestUnitId = DataEntityExtend.GetDynamicValue<long>(dynamicObject, "StockUnitID", 0L)
				});
			}
			Dictionary<long, UnitConvert> unitConvertRateList = UnitConvertServiceHelper.GetUnitConvertRateList(base.Context, list);
			Entity entity2 = this.View.BusinessInfo.GetEntity("FEntityStock");
			int num2 = 0;
			this.View.Model.BeginIniti();
			this.View.Model.DeleteEntryData("FEntityStock");
			this.View.Model.BatchCreateNewEntryRow("FEntityStock", ecnstockInfo.Count);
			foreach (DynamicObject dynamicObject2 in ecnstockInfo)
			{
				UnitConvert unitConvert;
				if (unitConvertRateList.TryGetValue((long)num2, out unitConvert))
				{
					dynamicObject2["Qty"] = unitConvert.ConvertQty(DataEntityExtend.GetDynamicValue<decimal>(dynamicObject2, "BaseStockQty", 0m), "");
				}
				MFGCommonUtil.SetDyFormViewFieldsValue(this.View, entity2, dynamicObject2, num2);
				num2++;
			}
			this.View.Model.EndIniti();
			this.View.UpdateView("FEntityStock");
		}

		// Token: 0x06000A84 RID: 2692 RVA: 0x00079FCC File Offset: 0x000781CC
		private List<long> GetMutilSelectedIds(string key)
		{
			return (from s in DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(this.Model.DataObject, key, null)
			select DataEntityExtend.GetDynamicValue<long>(s, string.Format("{0}_Id", key), 0L)).ToList<long>();
		}

		// Token: 0x06000A85 RID: 2693 RVA: 0x0007A04C File Offset: 0x0007824C
		private List<long> StartNetworkCtrl(List<long> bomIds, DynamicObjectCollection ecnClearOldMtrls, bool flag)
		{
			this.CommitNetworkCtrl();
			string text = ResManager.LoadKDString("修改", "015072000014413", 7, new object[0]);
			string text2 = string.Format(" FMetaObjectID = '{0}' and FoperationID = 'fae2446c-66e3-4cfe-83ab-5c7c1409d177'  and ftype={1}  and FStart = '1'  ", "ENG_BOM", 6);
			NetworkCtrlObject networkCtrlObject = NetworkCtrlServiceHelper.GetNetCtrlList(base.Context, text2).FirstOrDefault<NetworkCtrlObject>();
			List<NetWorkRunTimeParam> list = new List<NetWorkRunTimeParam>();
			foreach (long num in bomIds)
			{
				list.Add(new NetWorkRunTimeParam
				{
					BillName = new LocaleValue(ResManager.LoadKDString("旧料清理", "015072000017887", 7, new object[0]), 2052),
					InterID = num.ToString(),
					OperationDesc = ResManager.LoadKDString("物料清单", "015072000018136", 7, new object[0]) + "-BillNo-" + text,
					OperationName = new LocaleValue(text, 2052)
				});
			}
			this.networkCtrlResults = NetworkCtrlServiceHelper.BatchBeginNetCtrl(base.Context, networkCtrlObject, list, false);
			List<long> list2 = new List<long>();
			string empty = string.Empty;
			OperateResultCollection operateResultCollection = new OperateResultCollection();
			using (List<NetworkCtrlResult>.Enumerator enumerator2 = this.networkCtrlResults.GetEnumerator())
			{
				while (enumerator2.MoveNext())
				{
					NetworkCtrlResult r = enumerator2.Current;
					if (r.StartSuccess)
					{
						list2.Add(Convert.ToInt64(r.InterID));
					}
					else
					{
						OperateResult operateResult = new OperateResult();
						operateResult.Name = (from f in ecnClearOldMtrls
						where DataEntityExtend.GetDynamicValue<long>(f, "BomId", 0L) == Convert.ToInt64(r.InterID)
						select DataEntityExtend.GetDynamicValue<string>(f, "BomVersion", null)).FirstOrDefault<string>();
						operateResult.Message = r.Message;
						operateResult.SuccessStatus = false;
						operateResultCollection.Add(operateResult);
					}
				}
			}
			if (!ListUtils.IsEmpty<OperateResult>(operateResultCollection) && !flag)
			{
				this.View.ShowOperateResult(operateResultCollection, "BOS_BatchTips");
			}
			return list2;
		}

		// Token: 0x06000A86 RID: 2694 RVA: 0x0007A29C File Offset: 0x0007849C
		protected void CommitNetworkCtrl()
		{
			NetworkCtrlServiceHelper.BatchCommitNetCtrl(base.Context, this.networkCtrlResults);
		}

		// Token: 0x06000A87 RID: 2695 RVA: 0x0007A2B0 File Offset: 0x000784B0
		private string QueryEcnIdByBillNo(string billNo)
		{
			QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter
			{
				FormId = "ENG_ECNOrder",
				SelectItems = SelectorItemInfo.CreateItems("FID"),
				FilterClauseWihtKey = string.Format("FBillNo='{0}' and FChangeOrgId={1}", billNo, DataEntityExtend.GetDynamicValue<long>(this.Model.DataObject, "ChangeOrgId_Id", 0L))
			};
			DynamicObjectCollection dynamicObjectCollection = QueryServiceHelper.GetDynamicObjectCollection(base.Context, queryBuilderParemeter, null);
			if (ListUtils.IsEmpty<DynamicObject>(dynamicObjectCollection))
			{
				return null;
			}
			return DataEntityExtend.GetDynamicValue<string>(dynamicObjectCollection.FirstOrDefault<DynamicObject>(), "FID", null);
		}

		// Token: 0x06000A88 RID: 2696 RVA: 0x0007A338 File Offset: 0x00078538
		private void LoadPredictInStockInfo(int row)
		{
			Entity entity = this.View.BusinessInfo.GetEntity("FEntity");
			DynamicObject entityDataObject = this.View.Model.GetEntityDataObject(entity, row);
			long dynamicValue = DataEntityExtend.GetDynamicValue<long>(entityDataObject, "ChildMaterialId_Id", 0L);
			if (dynamicValue <= 0L)
			{
				this.View.Model.DeleteEntryData("FPredictInstockEntity");
				this.View.UpdateView("FPredictInstockEntity");
				return;
			}
			long[] materialMasterAndUserOrgId = MaterialServiceHelper.GetMaterialMasterAndUserOrgId(base.Context, dynamicValue);
			List<long> mutilSelectedIds = this.GetMutilSelectedIds("StockOrgIds");
			List<long> mutilSelectedIds2 = this.GetMutilSelectedIds("StockStatus");
			long num = DataEntityExtend.GetDynamicValue<long>(entityDataObject, "ChildBomId_Id", 0L);
			if (!DataEntityExtend.GetDynamicValue<bool>(entityDataObject, "BomIsEnable", false))
			{
				num = 0L;
			}
			long dynamicValue2 = DataEntityExtend.GetDynamicValue<long>(entityDataObject, "AuxPropId_Id", 0L);
			List<DynamicObject> predictInStockInfo = ECNClearOldMtrlServiceHelper.GetPredictInStockInfo(base.Context, new List<long>
			{
				materialMasterAndUserOrgId[0]
			}, mutilSelectedIds, mutilSelectedIds2, num, dynamicValue2);
			if (ListUtils.IsEmpty<DynamicObject>(predictInStockInfo))
			{
				this.View.Model.DeleteEntryData("FPredictInstockEntity");
				this.View.UpdateView("FPredictInstockEntity");
				return;
			}
			Entity entity2 = this.View.BusinessInfo.GetEntity("FPredictInstockEntity");
			int num2 = 0;
			this.View.Model.BeginIniti();
			this.View.Model.DeleteEntryData("FPredictInstockEntity");
			this.View.Model.BatchCreateNewEntryRow("FPredictInstockEntity", predictInStockInfo.Count);
			foreach (DynamicObject dynamicObject in predictInStockInfo)
			{
				MFGCommonUtil.SetDyFormViewFieldsValue(this.View, entity2, dynamicObject, num2++);
			}
			this.View.Model.EndIniti();
			this.View.UpdateView("FPredictInstockEntity");
		}

		// Token: 0x040004FE RID: 1278
		private const string ecnNumberKey = "FECNNUMBER";

		// Token: 0x040004FF RID: 1279
		private DynamicObject[] sysStockStatusArr;

		// Token: 0x04000500 RID: 1280
		private List<string> selectedECNBillNos = new List<string>();

		// Token: 0x04000501 RID: 1281
		private Dictionary<string, string> billKeyCache = new Dictionary<string, string>();

		// Token: 0x04000502 RID: 1282
		private List<NetworkCtrlResult> networkCtrlResults = new List<NetworkCtrlResult>();
	}
}
