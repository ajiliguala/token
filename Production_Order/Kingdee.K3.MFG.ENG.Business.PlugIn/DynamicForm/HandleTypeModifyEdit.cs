using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Validation;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.MFG.Utils;
using Kingdee.K3.MFG.BusinessCommon.BillPlugIn;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm
{
	// Token: 0x0200009C RID: 156
	[Description("设备列表处理类型批量修改（动态表单）处理类")]
	public class HandleTypeModifyEdit : AbstractMFGDynamicFormPlugIn
	{
		// Token: 0x06000B1B RID: 2843 RVA: 0x0007F002 File Offset: 0x0007D202
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
			this.dicMap = (List<KeyValuePair<object, object>>)e.Paramter.GetCustomParameter("DicMap");
		}

		// Token: 0x06000B1C RID: 2844 RVA: 0x0007F028 File Offset: 0x0007D228
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			base.BarItemClick(e);
			if (e.BarItemKey.ToUpper().Equals("TBMODIFY"))
			{
				string text = this.View.Model.GetValue("FBatchField").ToString();
				string key;
				switch (key = text)
				{
				case "A":
				{
					string text2 = this.View.Model.GetValue("FHandleType").ToString();
					decimal num2 = 0m;
					if (text2.Equals("B") || text2.Equals("F"))
					{
						num2 = MFGBillUtil.GetValue<decimal>(this.View.Model, "FUpperLimitValue1", -1, 0m, null);
						if (num2 <= 0m)
						{
							this.View.ShowErrMessage(ResManager.LoadKDString("批量报工或批量报工-不良品时，上限值必录！", "015072000039521", 7, new object[0]), "", 0);
							return;
						}
					}
					this.HandleBatchModifyForProcessType(text2, num2, this.handleType_Title);
					return;
				}
				case "B":
				{
					bool value = MFGBillUtil.GetValue<bool>(this.View.Model, "FAutoPush", -1, false, null);
					this.HandleBatchModify("IsAutoPush", value, this.autoPush_Title);
					return;
				}
				case "C":
				{
					bool value2 = MFGBillUtil.GetValue<bool>(this.View.Model, "FAutoAlert", -1, false, null);
					DynamicObject dynamicObject = null;
					if (value2)
					{
						dynamicObject = MFGBillUtil.GetValue<DynamicObject>(this.View.Model, "FAlertType", -1, null, null);
						if (ObjectUtils.IsNullOrEmptyOrWhiteSpace(dynamicObject))
						{
							this.View.ShowErrMessage(ResManager.LoadKDString("自动报警时，设备事件类型必录！", "015072000036558", 7, new object[0]), "", 0);
							return;
						}
					}
					this.HandleBatchModifyForAlert(value2, dynamicObject, this.autoAlert_Title);
					return;
				}
				case "D":
				{
					object value3 = this.View.Model.GetValue("FMeterUnitName");
					if (ObjectUtils.IsNullOrEmptyOrWhiteSpace(value3))
					{
						this.View.ShowErrMessage(ResManager.LoadKDString("仪表显示单位不能为空！", "015072000036559", 7, new object[0]), "", 0);
						return;
					}
					LocaleValue value4 = new LocaleValue(value3.ToString());
					this.HandleBatchModify("MeterUnitName", value4, this.meterUnitName_Title);
					return;
				}
				case "F":
				{
					decimal value5 = MFGBillUtil.GetValue<decimal>(this.View.Model, "FUpperLimitValue", -1, 0m, null);
					this.HandleBatchModify("UpperLimitValue", value5, this.upperLimitValue_Title);
					return;
				}
				case "G":
				{
					int value6 = MFGBillUtil.GetValue<int>(this.View.Model, "FAutoDeleteDay", -1, 0, null);
					this.HandleBatchModify("AutoDeleteDay", value6, this.autoDeleteDay_Title);
					return;
				}
				case "H":
				{
					int value7 = MFGBillUtil.GetValue<int>(this.View.Model, "FAutoDeleteItem", -1, 0, null);
					this.HandleBatchModify("AutoDeleteItem", value7, this.autoDeleteItem_Title);
					return;
				}
				case "I":
				{
					bool value8 = MFGBillUtil.GetValue<bool>(this.View.Model, "FAutoGather", -1, false, null);
					this.HandleHeadModify("IsAutoGather", value8, this.autoGather_Title);
					return;
				}
				case "J":
				{
					bool value9 = MFGBillUtil.GetValue<bool>(this.View.Model, "FCalOEE", -1, false, null);
					this.HandleHeadModify("IsCalOEE", value9, this.calOEE_Title);
					return;
				}
				case "K":
				{
					bool value10 = MFGBillUtil.GetValue<bool>(this.View.Model, "FDeviceNetWorking", -1, false, null);
					this.HandleHeadModify("IsDeviceNetWorking", value10, this.deviceNetWorking_Title);
					return;
				}
				case "L":
				{
					bool value11 = MFGBillUtil.GetValue<bool>(this.View.Model, "FMonitoring", -1, false, null);
					this.HandleHeadModify("IsMonitoring", value11, this.monitoring_Title);
					return;
				}
				case "M":
				{
					bool value12 = MFGBillUtil.GetValue<bool>(this.View.Model, "FLinkageStart", -1, false, null);
					this.HandleHeadModify("IsLinkageStart", value12, this.linkageStart_Title);
					return;
				}
				case "N":
				{
					string value13 = MFGBillUtil.GetValue<string>(this.View.Model, "FMonitorDisOrder", -1, null, null);
					if (ObjectUtils.IsNullOrEmptyOrWhiteSpace(value13))
					{
						this.View.ShowErrMessage(ResManager.LoadKDString("监控显示顺序不能为空！", "015072000018983", 7, new object[0]), "", 0);
						return;
					}
					this.HandleHeadModify("MonitorDisOrder", value13, this.monitorDisOrder_Title);
					break;
				}

					return;
				}
			}
		}

		// Token: 0x06000B1D RID: 2845 RVA: 0x0007F540 File Offset: 0x0007D740
		private void HandleHeadModify(string key, object value, string title)
		{
			FormMetadata formMetadata = (FormMetadata)MetaDataServiceHelper.Load(base.Context, "ENG_Equipment", true);
			List<DynamicObject> list = BusinessDataServiceHelper.Load(base.Context, (from o in this.dicMap
			select o.Key).Distinct<object>().ToArray<object>(), formMetadata.BusinessInfo.GetDynamicObjectType()).ToList<DynamicObject>();
			List<DynamicObject> list2 = new List<DynamicObject>();
			List<DynamicObject> list3 = new List<DynamicObject>();
			foreach (DynamicObject dynamicObject in list)
			{
				if (dynamicObject["ForbidStatus"].ToString().Equals("B"))
				{
					list2.Add(dynamicObject);
				}
				else
				{
					dynamicObject[key] = value;
					list3.Add(dynamicObject);
				}
			}
			IOperationResult operationResult = new OperationResult();
			if (list3.Count > 0)
			{
				operationResult = BusinessDataServiceHelper.Save(base.Context, formMetadata.BusinessInfo, list3.ToArray(), OperateOption.Create(), "Save");
			}
			foreach (DynamicObject dynamicObject2 in list2)
			{
				ValidationErrorInfo item = new ValidationErrorInfo("FNumber", dynamicObject2["Id"].ToString(), 0, 0, dynamicObject2["Id"].ToString(), string.Format(this.msg, dynamicObject2["Number"].ToString()), title, 2);
				operationResult.ValidationErrors.Add(item);
			}
			this.View.ReturnToParentWindow(operationResult.ValidationErrors);
			this.View.Close();
		}

		// Token: 0x06000B1E RID: 2846 RVA: 0x0007F760 File Offset: 0x0007D960
		private void HandleBatchModify(string key, object value, string title)
		{
			FormMetadata formMetadata = (FormMetadata)MetaDataServiceHelper.Load(base.Context, "ENG_Equipment", true);
			List<DynamicObject> list = BusinessDataServiceHelper.Load(base.Context, (from o in this.dicMap
			select o.Key).Distinct<object>().ToArray<object>(), formMetadata.BusinessInfo.GetDynamicObjectType()).ToList<DynamicObject>();
			List<DynamicObject> list2 = new List<DynamicObject>();
			List<DynamicObject> list3 = new List<DynamicObject>();
			List<DynamicObject> list4 = new List<DynamicObject>();
			using (List<DynamicObject>.Enumerator enumerator = list.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					DynamicObject equm = enumerator.Current;
					if (equm["ForbidStatus"].ToString().Equals("B"))
					{
						list2.Add(equm);
					}
					else
					{
						List<long> list5 = (from o in this.dicMap
						where o.Key.ToString().Equals(equm["Id"].ToString())
						select o into v
						select Convert.ToInt64(v.Value)).ToList<long>();
						DynamicObjectCollection dynamicObjectCollection = equm["MeterEntity"] as DynamicObjectCollection;
						if (dynamicObjectCollection.Count == 0)
						{
							list4.Add(equm);
						}
						else
						{
							foreach (DynamicObject dynamicObject in dynamicObjectCollection)
							{
								if (list5.Contains((long)dynamicObject["Id"]))
								{
									dynamicObject[key] = value;
								}
							}
							list3.Add(equm);
						}
					}
				}
			}
			IOperationResult operationResult = new OperationResult();
			if (list3.Count > 0)
			{
				operationResult = BusinessDataServiceHelper.Save(base.Context, formMetadata.BusinessInfo, list3.ToArray(), OperateOption.Create(), "Save");
			}
			foreach (DynamicObject dynamicObject2 in list2)
			{
				ValidationErrorInfo item = new ValidationErrorInfo("FNumber", dynamicObject2["Id"].ToString(), 0, 0, dynamicObject2["Id"].ToString(), string.Format(this.msg, dynamicObject2["Number"].ToString()), title, 2);
				operationResult.ValidationErrors.Add(item);
			}
			foreach (DynamicObject dynamicObject3 in list4)
			{
				ValidationErrorInfo item2 = new ValidationErrorInfo("FNumber", dynamicObject3["Id"].ToString(), 0, 0, dynamicObject3["Id"].ToString(), string.Format(this.strMsg, dynamicObject3["Number"].ToString()), title, 2);
				operationResult.ValidationErrors.Add(item2);
			}
			this.View.ReturnToParentWindow(operationResult.ValidationErrors);
			this.View.Close();
		}

		// Token: 0x06000B1F RID: 2847 RVA: 0x0007FB3C File Offset: 0x0007DD3C
		private void HandleBatchModifyForProcessType(string processType, decimal upperLimitValue, string title)
		{
			FormMetadata formMetadata = (FormMetadata)MetaDataServiceHelper.Load(base.Context, "ENG_Equipment", true);
			List<DynamicObject> list = BusinessDataServiceHelper.Load(base.Context, (from o in this.dicMap
			select o.Key).Distinct<object>().ToArray<object>(), formMetadata.BusinessInfo.GetDynamicObjectType()).ToList<DynamicObject>();
			List<DynamicObject> list2 = new List<DynamicObject>();
			List<DynamicObject> list3 = new List<DynamicObject>();
			List<DynamicObject> list4 = new List<DynamicObject>();
			using (List<DynamicObject>.Enumerator enumerator = list.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					DynamicObject equm = enumerator.Current;
					if (equm["ForbidStatus"].ToString().Equals("B"))
					{
						list2.Add(equm);
					}
					else
					{
						List<long> list5 = (from o in this.dicMap
						where o.Key.ToString().Equals(equm["Id"].ToString())
						select o into v
						select Convert.ToInt64(v.Value)).ToList<long>();
						DynamicObjectCollection dynamicObjectCollection = equm["MeterEntity"] as DynamicObjectCollection;
						if (dynamicObjectCollection.Count == 0)
						{
							list4.Add(equm);
						}
						else
						{
							foreach (DynamicObject dynamicObject in dynamicObjectCollection)
							{
								if (list5.Contains((long)dynamicObject["Id"]))
								{
									dynamicObject["ProcessType"] = processType;
									dynamicObject["UpperLimitValue"] = upperLimitValue;
								}
							}
							list3.Add(equm);
						}
					}
				}
			}
			IOperationResult operationResult = new OperationResult();
			if (list3.Count > 0)
			{
				operationResult = BusinessDataServiceHelper.Save(base.Context, formMetadata.BusinessInfo, list3.ToArray(), OperateOption.Create(), "Save");
			}
			foreach (DynamicObject dynamicObject2 in list2)
			{
				ValidationErrorInfo item = new ValidationErrorInfo("FNumber", dynamicObject2["Id"].ToString(), 0, 0, dynamicObject2["Id"].ToString(), string.Format(this.msg, dynamicObject2["Number"].ToString()), title, 2);
				operationResult.ValidationErrors.Add(item);
			}
			foreach (DynamicObject dynamicObject3 in list4)
			{
				ValidationErrorInfo item2 = new ValidationErrorInfo("FNumber", dynamicObject3["Id"].ToString(), 0, 0, dynamicObject3["Id"].ToString(), string.Format(this.strMsg, dynamicObject3["Number"].ToString()), title, 2);
				operationResult.ValidationErrors.Add(item2);
			}
			this.View.ReturnToParentWindow(operationResult.ValidationErrors);
			this.View.Close();
		}

		// Token: 0x06000B20 RID: 2848 RVA: 0x0007FF30 File Offset: 0x0007E130
		private void HandleBatchModifyForAlert(bool autoAlert, DynamicObject alertType, string title)
		{
			FormMetadata formMetadata = (FormMetadata)MetaDataServiceHelper.Load(base.Context, "ENG_Equipment", true);
			List<DynamicObject> list = BusinessDataServiceHelper.Load(base.Context, (from o in this.dicMap
			select o.Key).Distinct<object>().ToArray<object>(), formMetadata.BusinessInfo.GetDynamicObjectType()).ToList<DynamicObject>();
			List<DynamicObject> list2 = new List<DynamicObject>();
			List<DynamicObject> list3 = new List<DynamicObject>();
			using (List<DynamicObject>.Enumerator enumerator = list.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					DynamicObject equm = enumerator.Current;
					if (equm["ForbidStatus"].ToString().Equals("B"))
					{
						list2.Add(equm);
					}
					else
					{
						List<long> list4 = (from o in this.dicMap
						where o.Key.ToString().Equals(equm["Id"].ToString())
						select o into v
						select Convert.ToInt64(v.Value)).ToList<long>();
						DynamicObjectCollection dynamicObjectCollection = equm["MeterEntity"] as DynamicObjectCollection;
						foreach (DynamicObject dynamicObject in dynamicObjectCollection)
						{
							if (list4.Contains((long)dynamicObject["Id"]))
							{
								dynamicObject["IsAlert"] = autoAlert;
								dynamicObject["AlertTypeId"] = alertType;
								if (autoAlert)
								{
									dynamicObject["AlertTypeId_Id"] = Convert.ToInt64(alertType["Id"]);
								}
								else
								{
									dynamicObject["AlertTypeId_Id"] = 0;
								}
							}
						}
						list3.Add(equm);
					}
				}
			}
			IOperationResult operationResult = new OperationResult();
			if (list3.Count > 0)
			{
				operationResult = BusinessDataServiceHelper.Save(base.Context, formMetadata.BusinessInfo, list3.ToArray(), OperateOption.Create(), "Save");
			}
			foreach (DynamicObject dynamicObject2 in list2)
			{
				ValidationErrorInfo item = new ValidationErrorInfo("FNumber", dynamicObject2["Id"].ToString(), 0, 0, dynamicObject2["Id"].ToString(), string.Format(this.msg, dynamicObject2["Number"].ToString()), title, 2);
				operationResult.ValidationErrors.Add(item);
			}
			this.View.ReturnToParentWindow(operationResult.ValidationErrors);
			this.View.Close();
		}

		// Token: 0x04000530 RID: 1328
		private List<KeyValuePair<object, object>> dicMap;

		// Token: 0x04000531 RID: 1329
		private string handleType_Title = ResManager.LoadKDString("处理类型", "015072000036548", 7, new object[0]);

		// Token: 0x04000532 RID: 1330
		private string autoPush_Title = ResManager.LoadKDString("自动推送", "015072000036549", 7, new object[0]);

		// Token: 0x04000533 RID: 1331
		private string autoAlert_Title = ResManager.LoadKDString("自动报警", "015072000036550", 7, new object[0]);

		// Token: 0x04000534 RID: 1332
		private string meterUnitName_Title = ResManager.LoadKDString("仪表显示单位", "015072000036551", 7, new object[0]);

		// Token: 0x04000535 RID: 1333
		private string upperLimitValue_Title = ResManager.LoadKDString("上限值", "015072000036552", 7, new object[0]);

		// Token: 0x04000536 RID: 1334
		private string autoDeleteDay_Title = ResManager.LoadKDString("自动删除(天)", "015072000036553", 7, new object[0]);

		// Token: 0x04000537 RID: 1335
		private string autoDeleteItem_Title = ResManager.LoadKDString("自动删除(条)", "015072000036554", 7, new object[0]);

		// Token: 0x04000538 RID: 1336
		private string autoGather_Title = ResManager.LoadKDString("报工自动采集", "015072000018976", 7, new object[0]);

		// Token: 0x04000539 RID: 1337
		private string calOEE_Title = ResManager.LoadKDString("计算OEE", "015072000018977", 7, new object[0]);

		// Token: 0x0400053A RID: 1338
		private string deviceNetWorking_Title = ResManager.LoadKDString("设备联网", "015072000018978", 7, new object[0]);

		// Token: 0x0400053B RID: 1339
		private string monitoring_Title = ResManager.LoadKDString("看板显示", "015072000018979", 7, new object[0]);

		// Token: 0x0400053C RID: 1340
		private string linkageStart_Title = ResManager.LoadKDString("仅设备联动开工", "015072000018980", 7, new object[0]);

		// Token: 0x0400053D RID: 1341
		private string monitorDisOrder_Title = ResManager.LoadKDString("监控显示顺序", "015072000018981", 7, new object[0]);

		// Token: 0x0400053E RID: 1342
		private string msg = ResManager.LoadKDString("设备{0}已被禁用，不能进行批改操作！", "015072000036555", 7, new object[0]);

		// Token: 0x0400053F RID: 1343
		private string strMsg = ResManager.LoadKDString("设备{0}不存在仪表信息，不能进行批改操作！", "015072000036556", 7, new object[0]);
	}
}
