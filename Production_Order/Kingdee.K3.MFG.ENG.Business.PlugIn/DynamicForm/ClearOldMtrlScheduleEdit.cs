using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Core.Validation;
using Kingdee.BOS.JSON;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.Core.MFG.Utils;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm
{
	// Token: 0x02000086 RID: 134
	[Description("旧料调度清理 - 表单插件")]
	public class ClearOldMtrlScheduleEdit : AbstractDynamicFormPlugIn
	{
		// Token: 0x17000068 RID: 104
		// (get) Token: 0x06000A3E RID: 2622 RVA: 0x000772BC File Offset: 0x000754BC
		// (set) Token: 0x06000A3F RID: 2623 RVA: 0x000772C4 File Offset: 0x000754C4
		private DynamicObject TargetData { get; set; }

		// Token: 0x06000A40 RID: 2624 RVA: 0x000772D0 File Offset: 0x000754D0
		public override void BeforeBindData(EventArgs e)
		{
			base.BeforeBindData(e);
			object customParameter = this.View.OpenParameter.GetCustomParameter("ENG_ClearOldMtrlScheduleParam_ChangeOrgId", true);
			object customParameter2 = this.View.OpenParameter.GetCustomParameter("ENG_ClearOldMtrlScheduleParam_Name", true);
			object customParameter3 = this.View.OpenParameter.GetCustomParameter("ENG_ClearOldMtrlScheduleParam_Description", true);
			object customParameter4 = this.View.OpenParameter.GetCustomParameter("ENG_ClearOldMtrlScheduleParam_PluginClass", true);
			object customParameter5 = this.View.OpenParameter.GetCustomParameter("ENG_ClearOldMtrlScheduleParam_StockOrgIds", true);
			object customParameter6 = this.View.OpenParameter.GetCustomParameter("ENG_ClearOldMtrlScheduleParam_StockStatus", true);
			this.View.Model.SetValue("FChangeOrgId", customParameter);
			this.View.Model.SetValue("FNAME", customParameter2);
			this.View.Model.SetValue("FDESCRIPTION", customParameter3);
			this.View.Model.SetValue("FSCHEDULECLASS", customParameter4);
			this.View.Model.SetValue("FStockOrgIds", customParameter5);
			this.View.Model.SetValue("FStockStatus", customParameter6);
			this.TargetData = this.GetClearOldScheduleData();
			this.SetScheduleInfo();
		}

		// Token: 0x06000A41 RID: 2625 RVA: 0x00077408 File Offset: 0x00075608
		public override void ButtonClick(ButtonClickEventArgs e)
		{
			base.ButtonClick(e);
			string a;
			if ((a = e.Key.ToUpperInvariant()) != null)
			{
				if (a == "FBTNCONFIRM")
				{
					this.BuildSchedule();
					return;
				}
				if (!(a == "FBTNCANCEL"))
				{
					return;
				}
				this.View.Close();
			}
		}

		// Token: 0x06000A42 RID: 2626 RVA: 0x00077458 File Offset: 0x00075658
		private DynamicObject GetClearOldScheduleData()
		{
			string value = MFGBillUtil.GetValue<string>(this.View.Model, "FSCHEDULECLASS", -1, null, null);
			string filterClauseWihtKey = string.Format(" FSCHEDULECLASS='{0}' ", value);
			FormMetadata formMetadata = (FormMetadata)MetaDataServiceHelper.Load(base.Context, "BOS_SCHEDULETYPE", true);
			DynamicObject[] array = BusinessDataServiceHelper.Load(this.View.Context, formMetadata.BusinessInfo.GetDynamicObjectType(), new QueryBuilderParemeter
			{
				BusinessInfo = formMetadata.BusinessInfo,
				FilterClauseWihtKey = filterClauseWihtKey
			});
			DynamicObject result;
			if (!ListUtils.IsEmpty<DynamicObject>(array))
			{
				result = array.FirstOrDefault<DynamicObject>();
			}
			else
			{
				result = new DynamicObject(formMetadata.BusinessInfo.GetDynamicObjectType());
			}
			return result;
		}

		// Token: 0x06000A43 RID: 2627 RVA: 0x00077504 File Offset: 0x00075704
		private void SetScheduleInfo()
		{
			DynamicObject dynamicObject = DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(this.TargetData, "SCHEDULEINFO", null).FirstOrDefault<DynamicObject>();
			if (dynamicObject != null)
			{
				this.View.Model.SetValue("FEXECUTEINTERVAL", DataEntityExtend.GetDynamicValue<int>(dynamicObject, "EXECUTEINTERVAL", 0), 0);
				this.View.Model.SetValue("FEXECUTEINTERVALUNIT", DataEntityExtend.GetDynamicValue<string>(dynamicObject, "EXECUTEINTERVALUNIT", null), 0);
				this.View.Model.SetValue("FEXECUTETIME", DataEntityExtend.GetDynamicValue<DateTime>(dynamicObject, "EXECUTETIME", default(DateTime)), 0);
				this.View.Model.SetValue("FBEGINTIME", DataEntityExtend.GetDynamicValue<DateTime>(dynamicObject, "BEGINTIME", default(DateTime)), 0);
				this.View.Model.SetValue("FENDTIME", DataEntityExtend.GetDynamicValue<DateTime>(dynamicObject, "ENDTIME", default(DateTime)), 0);
				JSONObject jsonobject = KDObjectConverter.DeserializeObject<JSONObject>(DataEntityExtend.GetDynamicValue<string>(dynamicObject, "Parameters", null));
				if (jsonobject != null)
				{
					long num = 0L;
					bool flag = false;
					bool flag2 = false;
					bool flag3 = false;
					bool flag4 = false;
					object obj = jsonobject["stockOrgIds_Id"];
					object obj2 = jsonobject["stockStatus_Id"];
					long.TryParse(Convert.ToString(jsonobject["changeOrgId"]), out num);
					bool.TryParse(Convert.ToString(jsonobject["currOrgZero"]), out flag);
					bool.TryParse(Convert.ToString(jsonobject["otherOrgZero"]), out flag2);
					jsonobject.TryGetValue<bool>("currOrgPredictQtyZero", false, ref flag3);
					jsonobject.TryGetValue<bool>("otherOrgPredictQtyZero", false, ref flag4);
					this.View.Model.SetValue("FChangeOrgId", num);
					this.View.Model.SetValue("FStockOrgIds", obj);
					this.View.Model.SetValue("FStockStatus", obj2);
					this.View.Model.SetValue("FCurrOrgZero", flag);
					this.View.Model.SetValue("FOtherOrgZero", flag2);
					this.View.Model.SetValue("FCurrOrgPredictQtyZero", flag3);
					this.View.Model.SetValue("FOtherOrgPredictQtyZero", flag4);
				}
			}
		}

		// Token: 0x06000A44 RID: 2628 RVA: 0x00077784 File Offset: 0x00075984
		private void BuildSchedule()
		{
			FormMetadata formMetadata = (FormMetadata)MetaDataServiceHelper.Load(base.Context, "BOS_SCHEDULETYPE", true);
			DynamicObject dataObject = this.View.Model.DataObject;
			string dynamicValue = DataEntityExtend.GetDynamicValue<string>(dataObject, "EXECUTEINTERVALUNIT", null);
			if (string.IsNullOrWhiteSpace(dynamicValue) || StringUtils.EqualsIgnoreCase(dynamicValue, "6"))
			{
				this.View.ShowErrMessage(ResManager.LoadKDString("执行单位不可为空或当前不支持Cron表达式！", "0151515153499000016553", 7, new object[0]), "", 0);
				return;
			}
			int dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<int>(dataObject, "EXECUTEINTERVAL", 0);
			if (StringUtils.EqualsIgnoreCase(dynamicValue, "1") && dynamicObjectItemValue < 30)
			{
				this.View.ShowErrMessage(ResManager.LoadKDString("为确保机器性能，执行单位为分请设置执行时间大于30分钟！", "0151515153499000016560", 7, new object[0]), "", 0);
				return;
			}
			if (dynamicObjectItemValue <= 0)
			{
				this.View.ShowErrMessage(ResManager.LoadKDString("执行时间不可以为非正数或为空！", "0151515153499000016561", 7, new object[0]), "", 0);
				return;
			}
			DataEntityExtend.SetDynamicObjectItemValue(this.TargetData, "SCHEDULECLASS", DataEntityExtend.GetDynamicValue<string>(dataObject, "SCHEDULECLASS", null));
			DataEntityExtend.SetDynamicObjectItemValue(this.TargetData, "CREATEUSERID_Id", base.Context.UserId);
			DataEntityExtend.SetDynamicObjectItemValue(this.TargetData, "CREATETIME", DateTime.Now);
			DataEntityExtend.SetDynamicObjectItemValue(this.TargetData, "NAME", DataEntityExtend.GetDynamicValue<LocaleValue>(dataObject, "NAME", null));
			DataEntityExtend.SetDynamicObjectItemValue(this.TargetData, "DESCRIPTION", DataEntityExtend.GetDynamicValue<LocaleValue>(dataObject, "DESCRIPTION", null));
			DynamicObjectCollection dynamicValue2 = DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(this.TargetData, "SCHEDULEINFO", null);
			DynamicObject dynamicObject;
			if (ListUtils.IsEmpty<DynamicObject>(dynamicValue2))
			{
				Entity entity = formMetadata.BusinessInfo.GetEntity("SubHeadEntity");
				dynamicObject = new DynamicObject(entity.DynamicObjectType);
				DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "FAutoRecoveryTime", 1440);
				dynamicValue2.Add(dynamicObject);
			}
			else
			{
				dynamicObject = dynamicValue2.First<DynamicObject>();
			}
			DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "EXECUTEINTERVAL", DataEntityExtend.GetDynamicValue<int>(dataObject, "EXECUTEINTERVAL", 0));
			DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "EXECUTEINTERVALUNIT", DataEntityExtend.GetDynamicValue<string>(dataObject, "EXECUTEINTERVALUNIT", null));
			DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "EXECUTETIME", DataEntityExtend.GetDynamicValue<DateTime>(dataObject, "EXECUTETIME", default(DateTime)));
			DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "BEGINTIME", DataEntityExtend.GetDynamicValue<DateTime>(dataObject, "BEGINTIME", default(DateTime)));
			DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "ISASYNCJOB", false);
			DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "STATUS", "0");
			DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "ENDTIME", DataEntityExtend.GetDynamicValue<DateTime>(dataObject, "ENDTIME", default(DateTime)));
			DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "FISAutoExecute", true);
			JSONObject jsonobject = new JSONObject();
			jsonobject.Put("changeOrgId", DataEntityExtend.GetDynamicValue<long>(dataObject, "ChangeOrgId_Id", 0L));
			jsonobject.Put("currOrgZero", DataEntityExtend.GetDynamicValue<bool>(dataObject, "CurrOrgZero", false));
			jsonobject.Put("otherOrgZero", DataEntityExtend.GetDynamicValue<bool>(dataObject, "OtherOrgZero", false));
			jsonobject.Put("currOrgPredictQtyZero", DataEntityExtend.GetDynamicValue<bool>(dataObject, "currOrgPredictQtyZero", false));
			jsonobject.Put("otherOrgPredictQtyZero", DataEntityExtend.GetDynamicValue<bool>(dataObject, "OtherOrgPredictQtyZero", false));
			DynamicObjectCollection dynamicValue3 = DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(dataObject, "StockOrgIds", null);
			DynamicObjectCollection dynamicValue4 = DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(dataObject, "StockStatus", null);
			List<long> list;
			if (!ListUtils.IsEmpty<DynamicObject>(dynamicValue3))
			{
				list = (from i in dynamicValue3
				select DataEntityExtend.GetDynamicValue<long>(i, "StockOrgIds_Id", 0L)).ToList<long>();
			}
			else
			{
				list = new List<long>();
			}
			List<long> list2 = list;
			List<long> list3;
			if (!ListUtils.IsEmpty<DynamicObject>(dynamicValue4))
			{
				list3 = (from i in dynamicValue4
				select DataEntityExtend.GetDynamicValue<long>(i, "StockStatus_Id", 0L)).ToList<long>();
			}
			else
			{
				list3 = new List<long>();
			}
			List<long> list4 = list3;
			jsonobject.Put("stockOrgIds_Id", list2);
			jsonobject.Put("stockStatus_Id", list4);
			string text = jsonobject.ToJSONString();
			DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "Parameters", text);
			IOperationResult operationResult = BusinessDataServiceHelper.Save(base.Context, formMetadata.BusinessInfo, this.TargetData, null, "");
			if (operationResult.IsSuccess)
			{
				string arg = ResManager.LoadKDString("创建执行计划成功！", "015065000020339", 7, new object[0]);
				if (!ListUtils.IsEmpty<OperateResult>(operationResult.OperateResult))
				{
					arg = operationResult.OperateResult.First<OperateResult>().Message;
				}
				if (this.View.ParentFormView != null)
				{
					this.View.ParentFormView.ShowMessage(string.Format(ResManager.LoadKDString("{0} 请到基础管理->公共设置->其他->执行计划列表查看", "015065000020340", 7, new object[0]), arg), 0);
					this.View.SendAynDynamicFormAction(this.View.ParentFormView);
				}
				this.View.Close();
				return;
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(ResManager.LoadKDString("创建执行计划失败！", "015065000020341", 7, new object[0]));
			foreach (ValidationErrorInfo validationErrorInfo in operationResult.ValidationErrors)
			{
				stringBuilder.AppendLine(validationErrorInfo.Message);
			}
			foreach (OperateResult operateResult in operationResult.OperateResult)
			{
				if (!operateResult.SuccessStatus)
				{
					stringBuilder.AppendLine(operateResult.Message);
				}
			}
			this.View.ShowErrMessage(stringBuilder.ToString(), "", 0);
		}
	}
}
