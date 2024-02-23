using System;
using System.Collections.Generic;
using System.Linq;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.MFG.Common;
using Kingdee.K3.Core.MFG.ENG.ParamOption;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.Core.MFG.Utils;
using Kingdee.K3.MFG.ServiceHelper;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm
{
	// Token: 0x0200008F RID: 143
	public class ECNQueryEdit : AbstractDynamicFormPlugIn
	{
		// Token: 0x06000AA5 RID: 2725 RVA: 0x0007B1B8 File Offset: 0x000793B8
		public override void BeforeBindData(EventArgs e)
		{
			this.Model.SetValue("FEffectOrgIds", this.Model.DataObject["ChangeOrgId_Id"]);
			ECNQueryOption ecnqueryOption = this.View.OpenParameter.GetCustomParameter("QueryOption") as ECNQueryOption;
			if (ecnqueryOption == null)
			{
				return;
			}
			this.Model.SetValue("FChangeOrgId", ecnqueryOption.ECNOrgId);
			this.Model.SetValue("FEffectOrgIds", ecnqueryOption.billOrgIds);
			this.selectedECNBillIds = ecnqueryOption.SourceBills;
			this.Model.SetValue("FECNNo", string.Join(";", ecnqueryOption.SourceBills));
			this.PrepareQueryOption();
		}

		// Token: 0x06000AA6 RID: 2726 RVA: 0x0007B26C File Offset: 0x0007946C
		public override void AfterBarItemClick(AfterBarItemClickEventArgs e)
		{
			string barItemKey;
			if ((barItemKey = e.BarItemKey) != null)
			{
				if (!(barItemKey == "tbRefresh"))
				{
					return;
				}
				this.PrepareQueryOption();
			}
		}

		// Token: 0x06000AA7 RID: 2727 RVA: 0x0007B298 File Offset: 0x00079498
		public override void EntryButtonCellClick(EntryButtonCellClickEventArgs e)
		{
			string a;
			if ((a = e.FieldKey.ToUpper()) != null)
			{
				if (!(a == "FBILLNO"))
				{
					if (!(a == "FBILLNO1"))
					{
						return;
					}
					long num = OtherExtend.ConvertTo<long>(this.Model.GetValue("FBILLID1", e.Row), 0L);
					DynamicObject dynamicObject = this.Model.GetValue("FBILLTYPE1", e.Row) as DynamicObject;
					if (num > 0L && dynamicObject != null)
					{
						string dynamicValue = DataEntityExtend.GetDynamicValue<string>(dynamicObject, "Id", null);
						this.ViewBillsById(dynamicValue, num.ToString());
					}
				}
				else
				{
					long num2 = OtherExtend.ConvertTo<long>(this.Model.GetValue("FBILLID", e.Row), 0L);
					DynamicObject dynamicObject2 = this.Model.GetValue("FBillTypeId", e.Row) as DynamicObject;
					if (num2 > 0L && dynamicObject2 != null)
					{
						string dynamicValue2 = DataEntityExtend.GetDynamicValue<string>(dynamicObject2, "Id", null);
						this.ViewBillsById(dynamicValue2, num2.ToString());
						return;
					}
				}
			}
		}

		// Token: 0x06000AA8 RID: 2728 RVA: 0x0007B398 File Offset: 0x00079598
		private void ViewBillsById(string objectTypeId, string primaryKey)
		{
			string text = null;
			BillShowParameter billShowParameter = MFGCommonUtil.CreateBillShowParameterByPermission(base.Context, objectTypeId, primaryKey, ref text);
			if (billShowParameter != null)
			{
				billShowParameter.PageId = SequentialGuid.NewGuid().ToString();
				this.View.ShowForm(billShowParameter);
				return;
			}
			this.View.ShowMessage(text, 0);
		}

		// Token: 0x06000AA9 RID: 2729 RVA: 0x0007B404 File Offset: 0x00079604
		private void PrepareQueryOption()
		{
			ECNQueryOption ecnqueryOption = new ECNQueryOption();
			ecnqueryOption.ECNOrgId = OtherExtend.ConvertTo<long>(this.Model.DataObject["ChangeOrgId_Id"], 0L);
			DynamicObjectCollection dynamicObjectCollection = this.Model.DataObject["EffectOrgIds"] as DynamicObjectCollection;
			if (ListUtils.IsEmpty<DynamicObject>(dynamicObjectCollection))
			{
				ecnqueryOption.billOrgIds = new List<long>();
			}
			else
			{
				ecnqueryOption.billOrgIds = (from x in dynamicObjectCollection
				select OtherExtend.ConvertTo<long>(x["EffectOrgIds_Id"], 0L)).ToList<long>();
			}
			ecnqueryOption.SourceBills = this.selectedECNBillIds;
			ecnqueryOption.SortMode = OtherExtend.ConvertTo<string>(this.Model.DataObject["SortMode"], null);
			ecnqueryOption.Context = base.Context;
			ecnqueryOption.Result = new OperationResult();
			ecnqueryOption.BusinessInfo = this.View.BillBusinessInfo;
			ecnqueryOption.ChgBeginDate = OtherExtend.ConvertTo<DateTime>(this.Model.DataObject["ChgBeginDate"], default(DateTime));
			ecnqueryOption.ChgEndDate = OtherExtend.ConvertTo<DateTime>(this.Model.DataObject["ChgEndDate"], default(DateTime));
			this.GetQueryResult(ecnqueryOption);
		}

		// Token: 0x06000AAA RID: 2730 RVA: 0x0007B5A8 File Offset: 0x000797A8
		private void GetQueryResult(ECNQueryOption option)
		{
			TaskProxyItem taskProxyItem = new TaskProxyItem();
			List<object> list = new List<object>
			{
				base.Context,
				option
			};
			taskProxyItem.Parameters = list.ToArray();
			taskProxyItem.ClassName = "Kingdee.K3.MFG.ENG.App.Core.ECNOrderService,Kingdee.K3.MFG.ENG.App.Core";
			taskProxyItem.MethodName = "GetECNChangeQuery";
			taskProxyItem.ProgressQueryInterval = 1;
			taskProxyItem.Title = ResManager.LoadKDString("变更影响查询", "015072000019428", 7, new object[0]);
			FormUtils.ShowLoadingForm(this.View, taskProxyItem, null, true, delegate(IOperationResult op)
			{
				if (option.Result.IsSuccess)
				{
					this.FillData(option);
					return;
				}
				this.View.ShowOperateResult(option.Result.OperateResult, "BOS_BatchTips");
			});
		}

		// Token: 0x06000AAB RID: 2731 RVA: 0x0007B650 File Offset: 0x00079850
		private void FillData(ECNQueryOption option)
		{
			Entity entity = this.View.BillBusinessInfo.GetEntity("FEntity");
			DynamicObjectCollection entityDataObject = this.Model.GetEntityDataObject(entity);
			entityDataObject.Clear();
			option.ParentEntityDatas.ForEach(new Action<DynamicObject>(entityDataObject.Add));
			this.View.UpdateView("FEntity");
			Entity entity2 = this.View.BillBusinessInfo.GetEntity("FEntity1");
			DynamicObjectCollection entityDataObject2 = this.Model.GetEntityDataObject(entity2);
			entityDataObject2.Clear();
			option.ChildEntityDatas.ForEach(new Action<DynamicObject>(entityDataObject2.Add));
			this.View.UpdateView("FEntity1");
		}

		// Token: 0x06000AAC RID: 2732 RVA: 0x0007B700 File Offset: 0x00079900
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			string fieldKey;
			if ((fieldKey = e.FieldKey) != null)
			{
				if (!(fieldKey == "FChangeOrgId"))
				{
					if (fieldKey == "FECNNo")
					{
						this.ShowECNBillSelect();
						return;
					}
					if (!(fieldKey == "FEffectOrgIds"))
					{
						return;
					}
					this.GetValidEffectOrgFilterString(e);
					if (MFGServiceHelper.IsEnabledForbidOrgQuery(base.Context))
					{
						e.IsShowUsed = false;
					}
				}
				else
				{
					this.GetValidChangeOrgFilterString(e);
					if (MFGServiceHelper.IsEnabledForbidOrgQuery(base.Context))
					{
						e.IsShowUsed = false;
						return;
					}
				}
			}
		}

		// Token: 0x06000AAD RID: 2733 RVA: 0x0007B780 File Offset: 0x00079980
		public override void BeforeSetItemValueByNumber(BeforeSetItemValueByNumberArgs e)
		{
			base.BeforeSetItemValueByNumber(e);
			string a;
			if ((a = e.BaseDataFieldKey.ToUpperInvariant()) != null)
			{
				if (!(a == "FEFFECTORGIDS"))
				{
					return;
				}
				if (MFGServiceHelper.IsEnabledForbidOrgQuery(base.Context))
				{
					e.IsShowUsed = false;
				}
			}
		}

		// Token: 0x06000AAE RID: 2734 RVA: 0x0007B7C8 File Offset: 0x000799C8
		public override void BeforeUpdateValue(BeforeUpdateValueEventArgs e)
		{
			string key;
			if ((key = e.Key) != null)
			{
				key == "FECNNo";
			}
		}

		// Token: 0x06000AAF RID: 2735 RVA: 0x0007B7EC File Offset: 0x000799EC
		public override void DataChanged(DataChangedEventArgs e)
		{
			string key;
			if ((key = e.Key) != null)
			{
				if (!(key == "FChangeOrgId"))
				{
					return;
				}
				this.Model.SetValue("FEffectOrgIds", e.NewValue);
			}
		}

		// Token: 0x06000AB0 RID: 2736 RVA: 0x0007B828 File Offset: 0x00079A28
		private void GetValidEffectOrgFilterString(BeforeF7SelectEventArgs e)
		{
			string arg = string.Format("SELECT DISTINCT T5.FORGID FROM T_SEC_FUNCPERMISSIONENTRY T1 \r\nINNER JOIN T_SEC_FUNCPERMISSION T2 ON T1.FITEMID = T2.FITEMID\r\nINNER JOIN T_SEC_USERROLEMAP T4 ON T4.FROLEID = T2.FROLEID \r\nINNER JOIN T_SEC_USERORG T5 ON T4.FENTITYID = T5.FENTITYID\r\nINNER JOIN T_SEC_PERMISSIONITEM T6 ON T1.FPERMISSIONITEMID = T6.FITEMID\r\nWHERE  T5.FUSERID = {0} AND  T2.FOBJECTTYPEID = 'ENG_ECNEffect' AND T6.FITEMID = '6e44119a58cb4a8e86f6c385e14a17ad'", base.Context.UserId);
			IRegularFilterParameter listFilterParameter = e.ListFilterParameter;
			listFilterParameter.Filter += (ObjectUtils.IsNullOrEmptyOrWhiteSpace(e.ListFilterParameter.Filter) ? string.Format(" FOrgId IN({0}) ", arg) : string.Format(" AND FOrgId IN({0}) ", arg));
		}

		// Token: 0x06000AB1 RID: 2737 RVA: 0x0007B890 File Offset: 0x00079A90
		private void GetValidChangeOrgFilterString(BeforeF7SelectEventArgs e)
		{
			string arg = string.Format("SELECT DISTINCT T5.FORGID FROM T_SEC_FUNCPERMISSIONENTRY T1 \r\nINNER JOIN T_SEC_FUNCPERMISSION T2 ON T1.FITEMID = T2.FITEMID\r\nINNER JOIN T_SEC_USERROLEMAP T4 ON T4.FROLEID = T2.FROLEID \r\nINNER JOIN T_SEC_USERORG T5 ON T4.FENTITYID = T5.FENTITYID\r\nINNER JOIN T_SEC_PERMISSIONITEM T6 ON T1.FPERMISSIONITEMID = T6.FITEMID\r\nWHERE  T5.FUSERID = {0} AND  T2.FOBJECTTYPEID = 'ENG_ECNOrder' AND T6.FITEMID = '6e44119a58cb4a8e86f6c385e14a17ad'", base.Context.UserId);
			IRegularFilterParameter listFilterParameter = e.ListFilterParameter;
			listFilterParameter.Filter += (ObjectUtils.IsNullOrEmptyOrWhiteSpace(e.ListFilterParameter.Filter) ? string.Format(" FOrgId IN({0}) ", arg) : string.Format(" AND FOrgId IN({0}) ", arg));
		}

		// Token: 0x06000AB2 RID: 2738 RVA: 0x0007B9A8 File Offset: 0x00079BA8
		private void ShowECNBillSelect()
		{
			ListShowParameter listShowParameter = new ListShowParameter
			{
				FormId = "ENG_ECNOrder",
				PermissionItemId = "6e44119a58cb4a8e86f6c385e14a17ad",
				IsLookUp = true
			};
			IRegularFilterParameter listFilterParameter = listShowParameter.ListFilterParameter;
			listFilterParameter.Filter += string.Format(" {0} FChangeOrgId={1} AND FCANCELSTATUS = 'A'", ObjectUtils.IsNullOrEmptyOrWhiteSpace(listShowParameter.ListFilterParameter.Filter) ? "" : "AND", DataEntityExtend.GetDynamicValue<long>(this.Model.DataObject, "ChangeOrgId_Id", 0L));
			this.View.ShowForm(listShowParameter, delegate(FormResult result)
			{
				if (result == null)
				{
					return;
				}
				ListSelectedRowCollection listSelectedRowCollection = result.ReturnData as ListSelectedRowCollection;
				if (ObjectUtils.IsNullOrEmpty(listSelectedRowCollection))
				{
					return;
				}
				List<string> list = (from element in listSelectedRowCollection
				select element.BillNo).Distinct<string>().ToList<string>();
				if (list.Count > 10)
				{
					this.View.ShowErrMessage("", ResManager.LoadKDString("选择的工程变更单请勿超过10张。", "015072000019429", 7, new object[0]), 0);
					return;
				}
				this.selectedECNBillIds = list;
				this.Model.SetValue("FECNNo", string.Join(";", list));
			});
		}

		// Token: 0x0400050B RID: 1291
		private List<string> selectedECNBillIds = new List<string>();
	}
}
