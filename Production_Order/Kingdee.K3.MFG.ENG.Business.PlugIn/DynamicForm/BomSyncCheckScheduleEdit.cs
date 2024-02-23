using System;
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
	// Token: 0x0200007D RID: 125
	[Description("物料清单调度同步检查")]
	public class BomSyncCheckScheduleEdit : AbstractDynamicFormPlugIn
	{
		// Token: 0x17000054 RID: 84
		// (get) Token: 0x06000944 RID: 2372 RVA: 0x0006DD11 File Offset: 0x0006BF11
		// (set) Token: 0x06000945 RID: 2373 RVA: 0x0006DD19 File Offset: 0x0006BF19
		private DynamicObject TargetData { get; set; }

		// Token: 0x06000946 RID: 2374 RVA: 0x0006DD24 File Offset: 0x0006BF24
		public override void BeforeBindData(EventArgs e)
		{
			base.BeforeBindData(e);
			object customParameter = this.View.OpenParameter.GetCustomParameter("ENG_BomSyncCheckScheduleParam_CreateOrgId", true);
			object customParameter2 = this.View.OpenParameter.GetCustomParameter("ENG_BomSyncCheckScheduleParam_UseOrgId", true);
			object customParameter3 = this.View.OpenParameter.GetCustomParameter("ENG_BomSyncCheckScheduleParam_Name", true);
			object customParameter4 = this.View.OpenParameter.GetCustomParameter("ENG_BomSyncCheckScheduleParam_PluginClass", true);
			this.View.Model.SetValue("FCreateOrgId", customParameter);
			this.View.Model.SetValue("FUseOrgId", customParameter2);
			this.View.Model.SetValue("FNAME", customParameter3);
			this.View.Model.SetValue("FSCHEDULECLASS", customParameter4);
			this.TargetData = this.GetClearOldScheduleData();
			this.SetScheduleInfo();
		}

		// Token: 0x06000947 RID: 2375 RVA: 0x0006DE00 File Offset: 0x0006C000
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

		// Token: 0x06000948 RID: 2376 RVA: 0x0006DE50 File Offset: 0x0006C050
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

		// Token: 0x06000949 RID: 2377 RVA: 0x0006DEFC File Offset: 0x0006C0FC
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
					long num2 = 0L;
					long.TryParse(Convert.ToString(jsonobject["createOrgId"]), out num);
					long.TryParse(Convert.ToString(jsonobject["useOrgId"]), out num2);
					this.View.Model.SetValue("FCreateOrgId", num);
					this.View.Model.SetValue("FUseOrgId", num2);
				}
			}
		}

		// Token: 0x0600094A RID: 2378 RVA: 0x0006E080 File Offset: 0x0006C280
		private void BuildSchedule()
		{
			FormMetadata formMetadata = (FormMetadata)MetaDataServiceHelper.Load(base.Context, "BOS_SCHEDULETYPE", true);
			DynamicObject dataObject = this.View.Model.DataObject;
			if (DataEntityExtend.GetDynamicValue<long>(dataObject, "CreateOrgId_Id", 0L) == DataEntityExtend.GetDynamicValue<long>(dataObject, "useOrgId_Id", 0L))
			{
				this.View.ShowErrMessage(ResManager.LoadKDString("创建组织和使用组织不能一致！", "0151515153499000016552", 7, new object[0]), "", 0);
				return;
			}
			string dynamicValue = DataEntityExtend.GetDynamicValue<string>(dataObject, "EXECUTEINTERVALUNIT", null);
			if (string.IsNullOrWhiteSpace(dynamicValue) || StringUtils.EqualsIgnoreCase(dynamicValue, "6"))
			{
				this.View.ShowErrMessage(ResManager.LoadKDString("执行单位不可为空或当前不支持Cron表达式！", "0151515153499000016553", 7, new object[0]), "", 0);
				return;
			}
			int dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<int>(dataObject, "EXECUTEINTERVAL", 0);
			if (StringUtils.EqualsIgnoreCase(dynamicValue, "1") && dynamicObjectItemValue < 30)
			{
				this.View.ShowErrMessage(ResManager.LoadKDString("为确保机器性能，执行单位为分请设置执行间隔时间大于30分钟！", "0151515153499000016554", 7, new object[0]), "", 0);
				return;
			}
			if (dynamicObjectItemValue <= 0)
			{
				this.View.ShowErrMessage(ResManager.LoadKDString("执行间隔时间不可以为非正数或为空！", "0151515153499000016555", 7, new object[0]), "", 0);
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
			jsonobject.Put("createOrgId", DataEntityExtend.GetDynamicValue<long>(dataObject, "CreateOrgId_Id", 0L));
			jsonobject.Put("useOrgId", DataEntityExtend.GetDynamicValue<long>(dataObject, "useOrgId_Id", 0L));
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
