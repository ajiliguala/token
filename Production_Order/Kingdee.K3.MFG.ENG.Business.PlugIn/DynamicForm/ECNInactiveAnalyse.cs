using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Kingdee.BOS;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Validation;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.MFG.Common;
using Kingdee.K3.Core.MFG.ENG.ECN;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.Core.MFG.Utils;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm
{
	// Token: 0x0200008D RID: 141
	[Description("工程变更呆滞分析")]
	public class ECNInactiveAnalyse : AbstractDynamicFormPlugIn
	{
		// Token: 0x06000A8D RID: 2701 RVA: 0x0007A551 File Offset: 0x00078751
		public override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
		}

		// Token: 0x06000A8E RID: 2702 RVA: 0x0007A55A File Offset: 0x0007875A
		public override void AfterBindData(EventArgs e)
		{
			base.AfterBindData(e);
			this.ShowFilterForm();
		}

		// Token: 0x06000A8F RID: 2703 RVA: 0x0007A56C File Offset: 0x0007876C
		public override void AfterBarItemClick(AfterBarItemClickEventArgs e)
		{
			base.AfterBarItemClick(e);
			string barItemKey;
			if ((barItemKey = e.BarItemKey) != null)
			{
				if (barItemKey == "tbFilter")
				{
					this.ShowFilterForm();
					return;
				}
				if (!(barItemKey == "tbRefresh"))
				{
					return;
				}
				if (this.filterParam != null)
				{
					this.LoadAndFillData(this.filterParam);
					return;
				}
				this.View.ShowMessage(ResManager.LoadKDString("无数据，请选择过滤条件！", "0151515153499000012704", 7, new object[0]), 0);
			}
		}

		// Token: 0x06000A90 RID: 2704 RVA: 0x0007A5E3 File Offset: 0x000787E3
		public override void EntityRowDoubleClick(EntityRowClickEventArgs e)
		{
			base.EntityRowDoubleClick(e);
		}

		// Token: 0x06000A91 RID: 2705 RVA: 0x0007A5EC File Offset: 0x000787EC
		public override void EntryButtonCellClick(EntryButtonCellClickEventArgs e)
		{
			base.EntryButtonCellClick(e);
			if (StringUtils.EqualsIgnoreCase("FECNNumber", e.FieldKey))
			{
				DynamicObject entityDataObject = this.Model.GetEntityDataObject(this.View.BusinessInfo.GetEntity("FEntity"), e.Row);
				if (!ObjectUtils.IsNullOrEmpty(entityDataObject))
				{
					string dynamicValue = DataEntityExtend.GetDynamicValue<string>(entityDataObject, "ECNID", null);
					if (ObjectUtils.IsNullOrEmptyOrWhiteSpace(dynamicValue) || dynamicValue == "0")
					{
						return;
					}
					BillShowParameter billShowParameter = new BillShowParameter();
					billShowParameter.FormId = "ENG_ECNOrder";
					billShowParameter.ParentPageId = this.View.PageId;
					billShowParameter.Status = 1;
					billShowParameter.PKey = dynamicValue;
					this.View.ShowForm(billShowParameter);
				}
			}
		}

		// Token: 0x06000A92 RID: 2706 RVA: 0x0007A6CF File Offset: 0x000788CF
		private void ShowFilterForm()
		{
			MFGBillUtil.ShowFilterForm(this.View, "ENG_InactiveAnalyse", null, delegate(FormResult r)
			{
				if (r.ReturnData is FilterParameter)
				{
					this.filterParam = (r.ReturnData as FilterParameter);
					this.LoadAndFillData(this.filterParam);
				}
			}, "ENG_InactiveAnalyseFilter", 0);
		}

		// Token: 0x06000A93 RID: 2707 RVA: 0x0007A8AC File Offset: 0x00078AAC
		private void LoadAndFillData(FilterParameter param)
		{
			InactiveAnalyseOption inactiveAnalyseOption = this.BuildOption(param.CustomFilter);
			TaskProxyItem taskProxyItem = new TaskProxyItem();
			inactiveAnalyseOption.ExtendOption.SetVariableValue("taskId", taskProxyItem.TaskId);
			List<object> list = new List<object>
			{
				base.Context,
				inactiveAnalyseOption
			};
			taskProxyItem.Title = ResManager.LoadKDString("获取数据....", "0151515151805000013364", 7, new object[0]);
			taskProxyItem.ProgressQueryInterval = 1;
			taskProxyItem.ClassName = "Kingdee.K3.MFG.ENG.App.Core.ECN.ECNInactiveService,Kingdee.K3.MFG.ENG.App.Core";
			taskProxyItem.MethodName = "GetInactiveDatas";
			taskProxyItem.Parameters = list.ToArray();
			FormUtils.ShowLoadingForm(this.View, taskProxyItem, null, true, delegate(IOperationResult op)
			{
				this.FillHead(param.CustomFilter);
				DynamicObjectCollection entityDataObject = this.Model.GetEntityDataObject(this.View.BusinessInfo.GetEntity("FEntity"));
				entityDataObject.Clear();
				if (op.FuncResult != null && op.FuncResult is List<DynamicObject>)
				{
					List<DynamicObject> list2 = op.FuncResult as List<DynamicObject>;
					int num = 1;
					foreach (DynamicObject dynamicObject in list2)
					{
						DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "Seq", num++);
						entityDataObject.Add(dynamicObject);
					}
					this.View.UpdateView("FEntity");
					return;
				}
				if (!op.IsSuccess && !ListUtils.IsEmpty<ValidationErrorInfo>(op.GetFatalErrorResults()))
				{
					List<ValidationErrorInfo> fatalErrorResults = op.GetFatalErrorResults();
					StringBuilder stringBuilder = new StringBuilder();
					foreach (ValidationErrorInfo validationErrorInfo in fatalErrorResults)
					{
						string message = validationErrorInfo.Message;
						stringBuilder.AppendLine(message);
					}
					this.View.ShowErrMessage(stringBuilder.ToString(), "", 0);
					return;
				}
				this.View.ShowMessage(ResManager.LoadKDString("无数据，请重新选择过滤条件！", "0151515153499000012705", 7, new object[0]), 0);
				this.View.UpdateView("FEntity");
			});
		}

		// Token: 0x06000A94 RID: 2708 RVA: 0x0007A9D8 File Offset: 0x00078BD8
		private void FillHead(DynamicObject customFilter)
		{
			DynamicObjectCollection dynamicValue = DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(customFilter, "ChangeOrgId", null);
			List<string> values = (from s in dynamicValue
			select DataEntityExtend.GetDynamicValue<LocaleValue>(DataEntityExtend.GetDynamicValue<DynamicObject>(s, "ChangeOrgId", null), "Name", null)[base.Context.UserLocale.LCID]).ToList<string>();
			this.Model.SetValue("FChangeOrgId", string.Join(";", values));
			DynamicObjectCollection dynamicValue2 = DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(customFilter, "StockOrgId", null);
			List<string> values2 = (from s in dynamicValue2
			select DataEntityExtend.GetDynamicValue<LocaleValue>(DataEntityExtend.GetDynamicValue<DynamicObject>(s, "StockOrgId", null), "Name", null)[base.Context.UserLocale.LCID]).ToList<string>();
			this.Model.SetValue("FStockOrgId", string.Join(";", values2));
			string text = string.Empty;
			DynamicObject dynamicValue3 = DataEntityExtend.GetDynamicValue<DynamicObject>(customFilter, "StockIdFrom", null);
			DynamicObject dynamicValue4 = DataEntityExtend.GetDynamicValue<DynamicObject>(customFilter, "StockIdTo", null);
			if (!ObjectUtils.IsNullOrEmpty(dynamicValue3))
			{
				text = DataEntityExtend.GetDynamicValue<LocaleValue>(dynamicValue3, "Name", null)[base.Context.UserLocale.LCID] + "--";
			}
			if (!ObjectUtils.IsNullOrEmpty(dynamicValue4))
			{
				text += DataEntityExtend.GetDynamicValue<LocaleValue>(dynamicValue4, "Name", null)[base.Context.UserLocale.LCID];
			}
			this.Model.SetValue("FStocks", text);
			this.Model.SetValue("FChangeBeginDate", DataEntityExtend.GetDynamicValue<DateTime>(customFilter, "EffectiveStart", default(DateTime)));
			this.Model.SetValue("FChangeEndDate", DataEntityExtend.GetDynamicValue<DateTime>(customFilter, "EffectiveEnd", default(DateTime)));
		}

		// Token: 0x06000A95 RID: 2709 RVA: 0x0007AB78 File Offset: 0x00078D78
		protected InactiveAnalyseOption BuildOption(DynamicObject customFilter)
		{
			InactiveAnalyseOption inactiveAnalyseOption = new InactiveAnalyseOption();
			inactiveAnalyseOption.Ctx = base.Context;
			DynamicObjectCollection dynamicValue = DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(customFilter, "ChangeOrgId", null);
			inactiveAnalyseOption.ChangeOrgId = (from s in dynamicValue
			select DataEntityExtend.GetDynamicValue<long>(s, "ChangeOrgId_Id", 0L)).ToList<long>();
			DynamicObjectCollection dynamicValue2 = DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(customFilter, "StockOrgId", null);
			inactiveAnalyseOption.StockOrgId = (from s in dynamicValue2
			select DataEntityExtend.GetDynamicValue<long>(s, "StockOrgId_Id", 0L)).ToList<long>();
			inactiveAnalyseOption.StockIdFrom = DataEntityExtend.GetDynamicValue<long>(customFilter, "StockIdFrom_Id", 0L);
			inactiveAnalyseOption.StockIdTo = DataEntityExtend.GetDynamicValue<long>(customFilter, "StockIdTo_Id", 0L);
			inactiveAnalyseOption.ChangeBeginDate = DataEntityExtend.GetDynamicValue<DateTime>(customFilter, "EffectiveStart", default(DateTime));
			inactiveAnalyseOption.ChangeEndDate = DataEntityExtend.GetDynamicValue<DateTime>(customFilter, "EffectiveEnd", default(DateTime));
			string dynamicValue3 = DataEntityExtend.GetDynamicValue<string>(customFilter, "Number", null);
			if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(dynamicValue3))
			{
				inactiveAnalyseOption.ChangeBillNos = dynamicValue3.Split(new char[]
				{
					';'
				}).ToList<string>();
			}
			inactiveAnalyseOption.StorageDays = DataEntityExtend.GetDynamicValue<int>(customFilter, "StorageDays", 0);
			inactiveAnalyseOption.IsSelectedWhole = DataEntityExtend.GetDynamicValue<bool>(customFilter, "Checkbox", false);
			inactiveAnalyseOption.ExtendOption.SetVariableValue("SchemeId", DataEntityExtend.GetDynamicValue<long>(customFilter, "SchemeId_Id", 0L));
			return inactiveAnalyseOption;
		}

		// Token: 0x04000506 RID: 1286
		private FilterParameter filterParam;
	}
}
