using System;
using System.Collections.Generic;
using System.Linq;
using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.NetworkCtrl;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.MFG.Common;
using Kingdee.K3.Core.MFG.ENG.BomAuxPtyBulkEdit;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.Core.MFG.Utils;
using Kingdee.K3.MFG.ServiceHelper.ENG;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm
{
	// Token: 0x0200006B RID: 107
	public class BOMAuxPtyBulkEdit : AbstractDynamicFormPlugIn
	{
		// Token: 0x060007DB RID: 2011 RVA: 0x0005D236 File Offset: 0x0005B436
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
		}

		// Token: 0x060007DC RID: 2012 RVA: 0x0005D23F File Offset: 0x0005B43F
		public override void AfterBindData(EventArgs e)
		{
			base.AfterBindData(e);
			this.initData();
		}

		// Token: 0x060007DD RID: 2013 RVA: 0x0005D250 File Offset: 0x0005B450
		public override void BeforeUpdateValue(BeforeUpdateValueEventArgs e)
		{
			base.BeforeUpdateValue(e);
			string a;
			if ((a = e.Key.ToUpperInvariant()) != null)
			{
				if (!(a == "FAUXPTYVALUE"))
				{
					return;
				}
				DynamicObject dynamicObject;
				if (this.materialAuxPtysDic.TryGetValue(this.selectedAuxFID, out dynamicObject))
				{
					string dynamicValue = DataEntityExtend.GetDynamicValue<string>(dynamicObject, "FVALUETYPE", null);
					if (dynamicValue == "2")
					{
						string text = Convert.ToString(e.Value);
						int dynamicValue2 = DataEntityExtend.GetDynamicValue<int>(dynamicObject, "FCUSTOMDATASTRMAXLEN", 0);
						if (text.Length > dynamicValue2)
						{
							this.View.ShowErrMessage(string.Format(ResManager.LoadKDString("录入的值长度大于了辅助属性的最大数据长度{0}", "015072000021495", 7, new object[0]), dynamicValue2), "", 0);
							e.Cancel = true;
						}
					}
				}
			}
		}

		// Token: 0x060007DE RID: 2014 RVA: 0x0005D310 File Offset: 0x0005B510
		public override void DataChanged(DataChangedEventArgs e)
		{
			base.DataChanged(e);
			if (e.Field.Key.ToUpperInvariant() == "FCOMBOAUXPTYS")
			{
				this.selectedAuxFID = Convert.ToInt64(e.NewValue);
				this.selectedAuxValueId = string.Empty;
				this.View.Model.SetValue("FAuxptyValue", string.Empty);
				this.SetTextFieldProperty();
			}
			if (e.Field.Key.ToUpperInvariant() == "FAUXPTYVALUE")
			{
				this.selectedAuxValueId = Convert.ToString(e.NewValue);
			}
		}

		// Token: 0x060007DF RID: 2015 RVA: 0x0005D3AC File Offset: 0x0005B5AC
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			base.BeforeF7Select(e);
			string fieldKey;
			if ((fieldKey = e.FieldKey) != null)
			{
				if (!(fieldKey == "FAuxptyValue"))
				{
					return;
				}
				this.LoadAuxPtyValues(e);
			}
		}

		// Token: 0x060007E0 RID: 2016 RVA: 0x0005D668 File Offset: 0x0005B868
		public override void ButtonClick(ButtonClickEventArgs e)
		{
			BOMAuxPtyBulkEdit.<>c__DisplayClass5 CS$<>8__locals1 = new BOMAuxPtyBulkEdit.<>c__DisplayClass5();
			CS$<>8__locals1.e = e;
			CS$<>8__locals1.<>4__this = this;
			base.ButtonClick(CS$<>8__locals1.e);
			if (!StringUtils.EqualsIgnoreCase(CS$<>8__locals1.e.Key, "FOK"))
			{
				CS$<>8__locals1.e.Cancel = true;
				this.View.Close();
				return;
			}
			DynamicObject materialAuxPtyObj;
			if (!this.materialAuxPtysDic.TryGetValue(this.selectedAuxFID, out materialAuxPtyObj) || string.IsNullOrEmpty(this.selectedAuxValueId))
			{
				this.View.ShowMessage("物料清单子项物料未启用选择的辅助属性或录入的辅助属性值为空，不支持批量修改", 0);
				CS$<>8__locals1.e.Cancel = true;
				return;
			}
			this.View.ShowMessage(ResManager.LoadKDString("您确定要对所选分录的辅助属性值进行批量修改？", "015072000018133", 7, new object[0]), 1, delegate(MessageBoxResult result)
			{
				if (result != 1)
				{
					CS$<>8__locals1.e.Cancel = true;
					return;
				}
				List<NetworkCtrlResult> networkCtrlResults;
				CS$<>8__locals1.<>4__this.StartNetworkCtrl("fae2446c-66e3-4cfe-83ab-5c7c1409d177", ResManager.LoadKDString("修改", "015072000014413", 7, new object[0]), out networkCtrlResults);
				IOperationResult operationResult;
				List<long> list = CS$<>8__locals1.<>4__this.ValidateNetWorkCtrl(networkCtrlResults, out operationResult);
				if (list.Count == 0)
				{
					CS$<>8__locals1.<>4__this.CommitNetworkCtrl(networkCtrlResults);
					FormUtils.ShowOperationResult(CS$<>8__locals1.<>4__this.View, operationResult, null);
					return;
				}
				Dictionary<long, List<long>> pkEntryIdValues = new Dictionary<long, List<long>>();
				list.ForEach(delegate(long pkId)
				{
					pkEntryIdValues.Add(pkId, CS$<>8__locals1.<>4__this.fPKEntryIdsDic[pkId]);
				});
				TaskProxyItem taskProxyItem = new TaskProxyItem();
				BomAuxPtyBulkEditOption item = new BomAuxPtyBulkEditOption
				{
					ComputeId = taskProxyItem.TaskId,
					FlexFid = CS$<>8__locals1.<>4__this.selectedAuxFID,
					FlexNumber = DataEntityExtend.GetDynamicObjectItemValue<string>(materialAuxPtyObj, "FFLEXNUMBER", null),
					FlexName = DataEntityExtend.GetDynamicObjectItemValue<string>(materialAuxPtyObj, "FNAME", null),
					NewFlexValue = MFGBillUtil.GetValue<string>(CS$<>8__locals1.<>4__this.View.Model, "FAuxptyValue", -1, null, null),
					NewFlexValueId = CS$<>8__locals1.<>4__this.selectedAuxValueId,
					PKEntryIdValues = pkEntryIdValues
				};
				List<object> list2 = new List<object>
				{
					CS$<>8__locals1.<>4__this.Context,
					item,
					operationResult
				};
				taskProxyItem.Parameters = list2.ToArray();
				taskProxyItem.ClassName = "Kingdee.K3.MFG.ENG.App.Core.BOMAuxPtyBulkEditService,Kingdee.K3.MFG.ENG.App.Core";
				taskProxyItem.MethodName = "BatchUpdateAuxPtyValue";
				taskProxyItem.ProgressQueryInterval = 1;
				taskProxyItem.Title = ResManager.LoadKDString("批量修改-[物料清单]", "015072000018134", 7, new object[0]);
				FormUtils.ShowLoadingForm(CS$<>8__locals1.<>4__this.View, taskProxyItem, null, true, delegate(IOperationResult op)
				{
					CS$<>8__locals1.<>4__this.CommitNetworkCtrl(networkCtrlResults);
				});
			}, "", 0);
		}

		// Token: 0x060007E1 RID: 2017 RVA: 0x0005D7D4 File Offset: 0x0005B9D4
		private void initData()
		{
			this.materialId = MFGBillUtil.GetParam<long>(this.View, "MaterialId", 0L);
			MFGBillUtil.GetParam<List<KeyValuePair<object, object>>>(this.View, "fPKEntryIds", null).ForEach(delegate(KeyValuePair<object, object> pkEntryId)
			{
				long key = Convert.ToInt64(pkEntryId.Key);
				long item = Convert.ToInt64(pkEntryId.Value);
				if (this.fPKEntryIdsDic.ContainsKey(key))
				{
					this.fPKEntryIdsDic[key].Add(item);
					return;
				}
				this.fPKEntryIdsDic.Add(key, new List<long>
				{
					item
				});
			});
			this.materialAuxPtysDic = BOMAuxPtyBulkEditServiceHepler.GetAuxPtysByMtrId(base.Context, this.materialId).ToDictionary((DynamicObject d) => DataEntityExtend.GetDynamicObjectItemValue<long>(d, "FID", 0L));
			this.materialAuxPtysValueDic = (from g in BOMAuxPtyBulkEditServiceHepler.GetAuxPtyValueByMtrlId(base.Context, this.materialId)
			group g by DataEntityExtend.GetDynamicObjectItemValue<long>(g, "FAUXPROPERTYID", 0L)).ToDictionary((IGrouping<long, DynamicObject> d) => d.Key);
			this.LoadFCombAuxPtys();
		}

		// Token: 0x060007E2 RID: 2018 RVA: 0x0005D8B8 File Offset: 0x0005BAB8
		private void LoadFCombAuxPtys()
		{
			List<EnumItem> list = new List<EnumItem>();
			foreach (KeyValuePair<long, DynamicObject> keyValuePair in this.materialAuxPtysDic)
			{
				list.Add(new EnumItem
				{
					Value = Convert.ToString(keyValuePair.Key),
					Caption = new LocaleValue(ResManager.LoadKDString("辅助属性.", "015072000018135", 7, new object[0]) + DataEntityExtend.GetDynamicObjectItemValue<string>(keyValuePair.Value, "FNAME", null), base.Context.UserLocale.LCID)
				});
			}
			if (list.Count != 0)
			{
				this.selectedAuxFID = Convert.ToInt64(list.FirstOrDefault<EnumItem>().Value);
				this.SetTextFieldProperty();
			}
			this.View.GetControl<ComboFieldEditor>("FCOMBOAUXPTYS").SetComboItems(list);
		}

		// Token: 0x060007E3 RID: 2019 RVA: 0x0005DA18 File Offset: 0x0005BC18
		private void LoadAuxPtyValues(BeforeF7SelectEventArgs e)
		{
			DynamicObject dynamicObject;
			if (!this.materialAuxPtysDic.TryGetValue(this.selectedAuxFID, out dynamicObject))
			{
				return;
			}
			string dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<string>(dynamicObject, "FVALUESOURCE", null);
			int dynamicObjectItemValue2 = DataEntityExtend.GetDynamicObjectItemValue<int>(dynamicObject, "FVALUETYPE", 0);
			ListShowParameter listShowParameter = new ListShowParameter();
			listShowParameter.ParentPageId = this.View.PageId;
			listShowParameter.IsShowApproved = true;
			listShowParameter.IsLookUp = true;
			listShowParameter.MultiSelect = false;
			listShowParameter.UseOrgId = base.Context.CurrentOrganizationInfo.ID;
			IGrouping<long, DynamicObject> grouping;
			this.materialAuxPtysValueDic.TryGetValue(this.selectedAuxFID, out grouping);
			if (!ListUtils.IsEmpty<DynamicObject>(grouping))
			{
				List<string> list = new List<string>();
				foreach (DynamicObject dynamicObject2 in grouping)
				{
					list.Add(DataEntityExtend.GetDynamicObjectItemValue<string>(dynamicObject2, "FAUXPTYNUMBER", null));
				}
				if (list.Count > 0)
				{
					if (listShowParameter.ExtJoinTables == null)
					{
						List<ExtJoinTableDescription> extJoinTables = new List<ExtJoinTableDescription>();
						listShowParameter.ExtJoinTables = extJoinTables;
					}
					listShowParameter.ExtJoinTables.Add(new ExtJoinTableDescription
					{
						FieldName = "FID",
						JoinOption = 1,
						ScourceKey = "FNUMBER",
						TableName = "table(fn_StrSplit(@FID,',',3))",
						TableNameAs = "FS"
					});
					if (listShowParameter.SqlParams == null)
					{
						List<SqlParam> sqlParams = new List<SqlParam>();
						listShowParameter.SqlParams = sqlParams;
					}
					SqlParam item = new SqlParam("@FID", 163, list.Distinct<string>().ToArray<string>());
					listShowParameter.SqlParams.Add(item);
				}
			}
			if (dynamicObjectItemValue2 == 1)
			{
				listShowParameter.FormId = "BOS_ASSISTANTDATA_SELECT";
				listShowParameter.ListFilterParameter.Filter = string.Format(" FID='{0}' ", dynamicObjectItemValue);
			}
			else
			{
				listShowParameter.FormId = dynamicObjectItemValue;
			}
			this.View.ShowForm(listShowParameter, delegate(FormResult result)
			{
				if (result != null && result.ReturnData != null && (result.ReturnData as ListSelectedRowCollection).Count > 0)
				{
					ListSelectedRowCollection listSelectedRowCollection = (ListSelectedRowCollection)result.ReturnData;
					this.View.Model.SetValue("FAuxptyValue", listSelectedRowCollection[0].Name);
					this.selectedAuxValueId = listSelectedRowCollection[0].PrimaryKeyValue;
				}
			});
		}

		// Token: 0x060007E4 RID: 2020 RVA: 0x0005DC04 File Offset: 0x0005BE04
		private void StartNetworkCtrl(string operationKey, string operationName, out List<NetworkCtrlResult> networkCtrlResults)
		{
			string text = string.Format(" FMetaObjectID = '{0}' and FoperationID = '{1}'  and ftype={2}  and FStart = '1'  ", "ENG_BOM", operationKey, 6);
			NetworkCtrlObject networkCtrlObject = NetworkCtrlServiceHelper.GetNetCtrlList(base.Context, text).FirstOrDefault<NetworkCtrlObject>();
			List<NetWorkRunTimeParam> list = new List<NetWorkRunTimeParam>();
			List<long> list2 = this.fPKEntryIdsDic.Keys.ToList<long>();
			foreach (long num in list2)
			{
				list.Add(new NetWorkRunTimeParam
				{
					BillName = new LocaleValue(ResManager.LoadKDString("物料清单", "015072000018136", 7, new object[0]), 2052),
					InterID = num.ToString(),
					OperationDesc = ResManager.LoadKDString("物料清单", "015072000018136", 7, new object[0]) + "-BillNo-" + operationName,
					OperationName = new LocaleValue(operationName, 2052)
				});
			}
			networkCtrlResults = NetworkCtrlServiceHelper.BatchBeginNetCtrl(base.Context, networkCtrlObject, list, false);
		}

		// Token: 0x060007E5 RID: 2021 RVA: 0x0005DD24 File Offset: 0x0005BF24
		protected void CommitNetworkCtrl(List<NetworkCtrlResult> networkCtrlResults)
		{
			NetworkCtrlServiceHelper.BatchCommitNetCtrl(base.Context, networkCtrlResults);
		}

		// Token: 0x060007E6 RID: 2022 RVA: 0x0005DD54 File Offset: 0x0005BF54
		private List<long> ValidateNetWorkCtrl(List<NetworkCtrlResult> networkCtrlResults, out IOperationResult operationResult)
		{
			List<long> result = (from w in networkCtrlResults
			where w.StartSuccess
			select w into s
			select Convert.ToInt64(s.InterID)).ToList<long>();
			List<NetworkCtrlResult> list = (from w in networkCtrlResults
			where !w.StartSuccess
			select w).ToList<NetworkCtrlResult>();
			operationResult = new OperationResult();
			foreach (NetworkCtrlResult networkCtrlResult in list)
			{
				OperateResult operateResult = new OperateResult();
				operateResult.Name = networkCtrlResult.ObjectName;
				operateResult.Message = networkCtrlResult.Message;
				operateResult.SuccessStatus = false;
				operateResult.PKValue = networkCtrlResult.InterID;
				operateResult.MessageType = -1;
				operationResult.OperateResult.Add(operateResult);
			}
			return result;
		}

		// Token: 0x060007E7 RID: 2023 RVA: 0x0005DE60 File Offset: 0x0005C060
		private void SetTextFieldProperty()
		{
			DynamicObject dynamicObject;
			if (this.materialAuxPtysDic.TryGetValue(this.selectedAuxFID, out dynamicObject))
			{
				string dynamicValue = DataEntityExtend.GetDynamicValue<string>(dynamicObject, "FVALUETYPE", null);
				if (dynamicValue == "2")
				{
					this.View.GetFieldEditor("FAuxptyValue", 0).SetCustomPropertyValue("editable", true);
					this.View.GetFieldEditor("FAuxptyValue", 0).SetCustomPropertyValue("showEditButton", false);
					return;
				}
				this.View.GetFieldEditor("FAuxptyValue", 0).SetCustomPropertyValue("editable", false);
				this.View.GetFieldEditor("FAuxptyValue", 0).SetCustomPropertyValue("showEditButton", true);
			}
		}

		// Token: 0x04000397 RID: 919
		private long materialId;

		// Token: 0x04000398 RID: 920
		private long selectedAuxFID;

		// Token: 0x04000399 RID: 921
		private string selectedAuxValueId;

		// Token: 0x0400039A RID: 922
		private Dictionary<long, List<long>> fPKEntryIdsDic = new Dictionary<long, List<long>>();

		// Token: 0x0400039B RID: 923
		private Dictionary<long, DynamicObject> materialAuxPtysDic;

		// Token: 0x0400039C RID: 924
		private Dictionary<long, IGrouping<long, DynamicObject>> materialAuxPtysValueDic;
	}
}
