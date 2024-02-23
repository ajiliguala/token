using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using Kingdee.BOS;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Core.Validation;
using Kingdee.BOS.JSON;
using Kingdee.BOS.Log;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.MFG.ENG.ProductModel;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.Core.MFG.Utils;
using Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm.BOMBatchAllocate;
using Kingdee.K3.MFG.ServiceHelper;
using Kingdee.K3.MFG.ServiceHelper.ENG;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm
{
	// Token: 0x020000A7 RID: 167
	public class MdlCfgExecEdit : AbstractDynamicFormPlugIn
	{
		// Token: 0x06000BD5 RID: 3029 RVA: 0x000876FC File Offset: 0x000858FC
		public override void AfterBindData(EventArgs e)
		{
			object customParameter = this.View.OpenParameter.GetCustomParameter("option");
			if (customParameter == null)
			{
				return;
			}
			object customParameter2 = this.View.OpenParameter.GetCustomParameter("optOption", true);
			if (customParameter2 != null)
			{
				this.IsFromOptr = true;
				this.OptOption = KDObjectConverter.DeserializeObject<JSONObject>(customParameter2.ToString());
			}
			this.inputOption = (customParameter as MdlCfgOption);
			this.MdlCfgParameter = KDObjectConverter.DeserializeObject<JSONObject>(this.View.OpenParameter.GetCustomParameter("MdlCfgParameter").ToString());
			string text = string.Format(this.MdlCfgParameter.GetString("prdMdlNum") + "_" + SequentialGuid.NewNativeGuid().ToString(), new object[0]);
			IOperationResult operationResult = BGProgressServiceHelper.CreateNewProgressLog(base.Context, text);
			this.progressDetailMeta = (MetaDataServiceHelper.Load(base.Context, "MFG_ProgressDetail", true) as FormMetadata);
			if (operationResult.IsSuccess)
			{
				this.inputOption.ComputeNo = text;
				this.inputOption.ComputeId = operationResult.SuccessDataEnity.FirstOrDefault<DynamicObject>()["Id"].ToString();
				this.inputOption.IsWriteRuleLog = true;
				this.DoCfg();
				return;
			}
			FormUtils.ShowOperationResult(this.View, operationResult, null);
		}

		// Token: 0x06000BD6 RID: 3030 RVA: 0x00087F88 File Offset: 0x00086188
		private void DoCfg()
		{
			this._progressValue = 0;
			this._catchException = false;
			this._progressMsg.Clear();
			this._isProcessing = true;
			ProgressBar control = this.View.GetControl<ProgressBar>("FProgressBar");
			control.Enabled = true;
			control.Start(2);
			FormUtils.RunTask(this.View, delegate()
			{
				try
				{
					this.outputOption = MdlCfgServiceHelper.ExecuteStandAbstrategy(base.Context, this.inputOption);
					object customParameter = this.View.OpenParameter.GetCustomParameter("isExistCfgBill", true);
					object customParameter2 = this.View.OpenParameter.GetCustomParameter("cfgBillId", true);
					if (this.outputOption.IsSuccess)
					{
						if (customParameter != null && customParameter.ToString() == "1")
						{
							this.outputOption.CfgBillObj.Put("cfgBillId", OtherExtend.ConvertTo<long>(customParameter2, 0L));
							this.outputOption.CfgBillObj.Put("status", "1");
							this.SetMsg(this.outputOption, ResManager.LoadKDString("保存配置清单", "015072000025086", 7, new object[0]), string.Format(ResManager.LoadKDString("正在更新编码为{0}配置清单的状态", "015072000025087", 7, new object[0]), this.View.OpenParameter.GetCustomParameter("cfgBillNo").ToString()), "0");
							MdlCfgServiceHelper.UpdateCfgBillStatus(base.Context, this.outputOption);
						}
						else
						{
							long num;
							if (this.outputOption.CfgBillObj.TryGetValue<long>("bomId", 0L, ref num) && num != 0L)
							{
								this.outputOption.CfgBillObj.Put("status", "1");
							}
							if (this.IsFromOptr)
							{
								string @string = this.OptOption.GetString("FormId");
								string string2 = this.OptOption.GetString("BillNo");
								int @int = this.OptOption.GetInt("EntrySeq");
								this.outputOption.CfgBillObj.Put("srcFormId", @string);
								this.outputOption.CfgBillObj.Put("billNo", string2);
								this.outputOption.CfgBillObj.Put("seq", @int);
							}
							this.DeliverMdlVersion(this.outputOption);
						}
					}
					else if (customParameter == null || !(customParameter.ToString() == "1"))
					{
						if (this.IsFromOptr)
						{
							string string3 = this.OptOption.GetString("FormId");
							string string4 = this.OptOption.GetString("BillNo");
							int int2 = this.OptOption.GetInt("EntrySeq");
							this.inputOption.CfgBillObj.Put("srcFormId", string3);
							this.inputOption.CfgBillObj.Put("billNo", string4);
							this.inputOption.CfgBillObj.Put("seq", int2);
						}
						this.inputOption.CfgBillObj.Put("status", "0");
						this.inputOption.CfgBillObj.Put("bomId", 0);
						this.DeliverMdlVersion(this.inputOption);
					}
				}
				catch (Exception ex)
				{
					this.SetException(this.inputOption, ex);
					throw ex;
				}
			}, delegate(string message)
			{
				this.View.ShowErrMessage(ResManager.LoadKDString("执行出现异常", "015072000025088", 7, new object[0]), "", 0);
				this._isProcessing = false;
			}, delegate()
			{
				this._isProcessing = false;
				ProgressBar control2 = this.View.GetControl<ProgressBar>("FProgressBar");
				control2.Start(100000);
				this.View.UpdateView("FProgressBar");
				this._progressMsg.Add(ResManager.LoadKDString("任务执行完成", "015072000025089", 7, new object[0]));
				this._progressValue = 100;
				if (this.outputOption.IsSuccess && this.IsFromOptr)
				{
					string @string = this.OptOption.GetString("MaterialKey");
					string string2 = this.OptOption.GetString("AuxPtyKey");
					string string3 = this.OptOption.GetString("BomIdKey");
					string string4 = this.OptOption.GetString("OrgIdKey");
					int num = this.OptOption.GetInt("EntrySeq");
					num--;
					MdlCfgGenResult mdlCfgGenResult = (from x in this.outputOption.CfgResults
					where ObjectUtils.IsNullOrEmptyOrWhiteSpace(x.RowId)
					select x).FirstOrDefault<MdlCfgGenResult>();
					long num2 = OtherExtend.ConvertTo<long>(this.outputOption.CfgBillObj.Get("bomId"), 0L);
					long dynamicValue = DataEntityExtend.GetDynamicValue<long>(mdlCfgGenResult.BOMObj, "ParentAuxPropId_Id", 0L);
					long dynamicValue2 = DataEntityExtend.GetDynamicValue<long>(mdlCfgGenResult.BOMObj, "MaterialId_Id", 0L);
					if (DataEntityExtend.GetDynamicValue<bool>(mdlCfgGenResult.PrdMdlObj, "IsChooseMtrl", false))
					{
						DynamicObject dynamicObject = mdlCfgGenResult.BOMObj["MaterialId"] as DynamicObject;
						long dynamicValue3 = DataEntityExtend.GetDynamicValue<long>(dynamicObject, "UseOrgId_Id", 0L);
						BaseDataField baseDataField = this.View.ParentFormView.BusinessInfo.GetField(@string) as BaseDataField;
						DynamicObject dynamicObject2 = this.View.ParentFormView.Model.GetValue(baseDataField.OrgFieldKey) as DynamicObject;
						long dynamicValue4 = DataEntityExtend.GetDynamicValue<long>(dynamicObject2, "Id", 0L);
						if (dynamicValue3 != dynamicValue4 && !this.TryGetAllocedMtrl(base.Context, dynamicValue4, dynamicValue2, out dynamicValue2))
						{
							LocaleValue dynamicValue5 = DataEntityExtend.GetDynamicValue<LocaleValue>(dynamicObject2, "Name", null);
							IOperationResult operationResult = BOMServiceHelper.Allocate(base.Context, this.outputOption.MaterialMeta, dynamicValue2, dynamicValue4, dynamicValue3, dynamicValue5[base.Context.UserLocale.LCID]);
							if (operationResult.IsSuccess)
							{
								OptrChain optrChain = new OptrChain
								{
									BizInfo = this.outputOption.MaterialMeta.BusinessInfo,
									Datas = operationResult.SuccessDataEnity.ToList<DynamicObject>(),
									Result = new OperationResult
									{
										IsSuccess = true
									}
								};
								optrChain.Init();
								optrChain.DoOperations(base.Context);
								if (optrChain.Result.IsSuccess)
								{
									dynamicValue2 = DataEntityExtend.GetDynamicValue<long>(operationResult.SuccessDataEnity.FirstOrDefault<DynamicObject>(), "Id", 0L);
								}
								else
								{
									this.View.ShowErrMessage(string.Format(ResManager.LoadKDString("创建组织【{1}】编码为【{0}】的物料提交审核失败，异常信息如下：", "015072000033345", 7, new object[0]), dynamicObject["Number"], dynamicValue5[base.Context.UserLocale.LCID]), "", 0);
								}
							}
						}
						this.View.ParentFormView.Model.SetValue(@string, dynamicValue2, num);
						if (this.View.ParentFormView.BusinessInfo.GetForm().Id == "SAL_SaleOrder")
						{
							this.View.ParentFormView.Model.SetValue(string4, dynamicValue3, num);
						}
					}
					this.View.ParentFormView.Model.SetValue(string3, num2, num);
					this.View.ParentFormView.Model.SetValue(string2, dynamicValue, num);
					this.View.SendDynamicFormAction(this.View.ParentFormView);
				}
			});
		}

		// Token: 0x06000BD7 RID: 3031 RVA: 0x00088004 File Offset: 0x00086204
		private bool TryGetAllocedMtrl(Context ctx, long useOrgId, long mtrlId, out long allocedId)
		{
			QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter
			{
				FormId = "BD_MATERIAL",
				SelectItems = SelectorItemInfo.CreateItems("FMaterialID"),
				FilterClauseWihtKey = "FUseOrgId=@useOrgId and FMasterId=@masterId"
			};
			List<SqlParam> list = new List<SqlParam>
			{
				new SqlParam("@useOrgId", 12, useOrgId),
				new SqlParam("masterId", 12, mtrlId)
			};
			DynamicObjectCollection dynamicObjectCollection = QueryServiceHelper.GetDynamicObjectCollection(ctx, queryBuilderParemeter, list);
			allocedId = (ListUtils.IsEmpty<DynamicObject>(dynamicObjectCollection) ? mtrlId : DataEntityExtend.GetDynamicValue<long>(dynamicObjectCollection.FirstOrDefault<DynamicObject>(), "FMaterialID", 0L));
			return !ListUtils.IsEmpty<DynamicObject>(dynamicObjectCollection);
		}

		// Token: 0x06000BD8 RID: 3032 RVA: 0x000880B0 File Offset: 0x000862B0
		private void DeliverMdlVersion(MdlCfgOption option)
		{
			this.SetMsg(option, ResManager.LoadKDString("保存配置清单", "015072000025086", 7, new object[0]), ResManager.LoadKDString("正在保存配置清单", "015072000025090", 7, new object[0]), "0");
			JSONObject jsonobject = MdlCfgServiceHelper.DeliverNewMdlVersion(base.Context, this.MdlCfgParameter, option.CfgBillObj);
			if (jsonobject.GetBool("isSuccess"))
			{
				this.SetMsg(option, ResManager.LoadKDString("保存配置清单", "015072000025086", 7, new object[0]), string.Format(ResManager.LoadKDString("生成新的模型配置清单，编码：{0}", "015072000025091", 7, new object[0]), jsonobject.GetString("cfgBillNo")), "0");
				return;
			}
			this.SetMsg(option, ResManager.LoadKDString("保存配置清单", "015072000025086", 7, new object[0]), ResManager.LoadKDString("模型配置清单生成失败", "015072000025092", 7, new object[0]), "0");
			IOperationResult operationResult = jsonobject.Get("saveResult") as SaveOperationResult;
			foreach (ValidationErrorInfo validationErrorInfo in operationResult.ValidationErrors)
			{
				this.SetMsg(option, validationErrorInfo.Message, "2", "0");
			}
			foreach (OperateResult operateResult in operationResult.OperateResult)
			{
				if (!operateResult.SuccessStatus)
				{
					this.SetMsg(option, operateResult.Message, "2", "0");
				}
			}
		}

		// Token: 0x06000BD9 RID: 3033 RVA: 0x0008825C File Offset: 0x0008645C
		private void SetMsg(MdlCfgOption inputOption, string period, string msg, string msgType = "0")
		{
			DynamicObject dynamicObject = new DynamicObject(this.progressDetailMeta.BusinessInfo.GetDynamicObjectType());
			dynamicObject["ComputeNo"] = inputOption.ComputeNo;
			dynamicObject["ProgressMsgId"] = inputOption.ComputeId;
			dynamicObject["period"] = period;
			dynamicObject["message"] = msg;
			dynamicObject["MessageType"] = msgType;
			dynamicObject["DetailTime"] = DateTime.Now;
			BGProgressServiceHelper.SetProgressDetail(base.Context, dynamicObject);
		}

		// Token: 0x06000BDA RID: 3034 RVA: 0x000882E8 File Offset: 0x000864E8
		private void SetException(MdlCfgOption inputOption, Exception ex)
		{
			Logger.Error("ENG", ex.Message, ex);
			DynamicObject dynamicObject = new DynamicObject(this.progressDetailMeta.BusinessInfo.GetDynamicObjectType());
			dynamicObject["ComputeNo"] = inputOption.ComputeNo;
			dynamicObject["ProgressMsgId"] = inputOption.ComputeId;
			dynamicObject["period"] = ResManager.LoadKDString("生成模型配置", "015072000025093", 7, new object[0]);
			dynamicObject["message"] = ex.Message;
			dynamicObject["MessageType"] = "2";
			dynamicObject["Exceptions"] = ex.StackTrace;
			dynamicObject["DetailTime"] = DateTime.Now;
			BGProgressServiceHelper.SetProgressDetail(base.Context, dynamicObject);
		}

		// Token: 0x06000BDB RID: 3035 RVA: 0x000883D8 File Offset: 0x000865D8
		public override void OnQueryProgressValue(QueryProgressValueEventArgs e)
		{
			if (e.Value >= 100)
			{
				return;
			}
			string computeNo = this.inputOption.ComputeNo;
			DynamicObject[] array = BGProgressServiceHelper.GetProgressDetails(base.Context, computeNo, 25);
			if (!ListUtils.IsEmpty<DynamicObject>(array))
			{
				array = (from x in array
				orderby DataEntityExtend.GetDynamicValue<DateTime>(x, "DetailTime", default(DateTime))
				select x).ToArray<DynamicObject>();
				this._progressMsg.Clear();
				foreach (DynamicObject dynamicObject in array)
				{
					string dynamicValue = DataEntityExtend.GetDynamicValue<string>(dynamicObject, "MessageType", null);
					string text = "";
					string text2 = "";
					string a;
					if ((a = dynamicValue) != null)
					{
						if (!(a == "0"))
						{
							if (!(a == "1"))
							{
								if (a == "2")
								{
									text = ResManager.LoadKDString("异常中断", "015072000025096", 7, new object[0]);
									text2 = DataEntityExtend.GetDynamicValue<string>(dynamicObject, "Exceptions", null);
								}
							}
							else
							{
								text = ResManager.LoadKDString("警告", "015072000025095", 7, new object[0]);
							}
						}
						else
						{
							text = ResManager.LoadKDString("提示", "015072000025094", 7, new object[0]);
						}
					}
					this._progressMsg.Add(string.Format("{3}:[{0}]{1}{2}", new object[]
					{
						text,
						DataEntityExtend.GetDynamicValue<string>(dynamicObject, "message", null),
						text2,
						DataEntityExtend.GetDynamicValue<string>(dynamicObject, "period", null)
					}));
				}
			}
			string text3 = string.Join(Environment.NewLine, this._progressMsg);
			this.Model.SetValue("FRemarks", text3);
			this._progressValue = BGProgressServiceHelper.GetProgressValue(base.Context, computeNo);
			e.Value = this._progressValue;
		}

		// Token: 0x06000BDC RID: 3036 RVA: 0x000885A4 File Offset: 0x000867A4
		public override void AfterBarItemClick(AfterBarItemClickEventArgs e)
		{
			string barItemKey;
			if ((barItemKey = e.BarItemKey) != null)
			{
				if (barItemKey == "tbExport")
				{
					this.ExportToTxtFile();
					return;
				}
				if (!(barItemKey == "tbOpenLogs"))
				{
					return;
				}
				string arg = "";
				if (this.inputOption != null)
				{
					arg = this.inputOption.ComputeId;
				}
				ListShowParameter listShowParameter = new ListShowParameter
				{
					FormId = "MFG_ProgressDetail",
					PageId = SequentialGuid.NewGuid().ToString()
				};
				listShowParameter.ListFilterParameter.Filter = string.Format("{0} FProgressMsgId='{1}'", ObjectUtils.IsNullOrEmptyOrWhiteSpace(listShowParameter.ListFilterParameter.Filter) ? "" : (listShowParameter.ListFilterParameter.Filter + " And "), arg);
				this.View.ShowForm(listShowParameter);
			}
		}

		// Token: 0x06000BDD RID: 3037 RVA: 0x00088678 File Offset: 0x00086878
		public override void BeforeClosed(BeforeClosedEventArgs e)
		{
			if (this._isProcessing)
			{
				e.Cancel = true;
				this.View.ShowMessage(ResManager.LoadKDString("模型配置正在执行中，请稍等", "015072000025097", 7, new object[0]), 0);
				return;
			}
			this.View.ReturnToParentWindow(this.outputOption);
		}

		// Token: 0x06000BDE RID: 3038 RVA: 0x000887C0 File Offset: 0x000869C0
		private void ExportToTxtFile()
		{
			DynamicObject[] msgDetails = BGProgressServiceHelper.GetProgressDetails(base.Context, this.inputOption.ComputeNo, -1);
			if (!ListUtils.IsEmpty<DynamicObject>(msgDetails))
			{
				msgDetails = (from x in msgDetails
				orderby DataEntityExtend.GetDynamicValue<DateTime>(x, "DetailTime", default(DateTime))
				select x).ToArray<DynamicObject>();
				this._progressMsg.Clear();
				foreach (DynamicObject dynamicObject in msgDetails)
				{
					string dynamicValue = DataEntityExtend.GetDynamicValue<string>(dynamicObject, "MessageType", null);
					string text = "";
					string text2 = "";
					string a;
					if ((a = dynamicValue) != null)
					{
						if (!(a == "0"))
						{
							if (!(a == "1"))
							{
								if (a == "2")
								{
									text = ResManager.LoadKDString("异常中断", "015072000025096", 7, new object[0]);
									text2 = DataEntityExtend.GetDynamicValue<string>(dynamicObject, "Exceptions", null);
								}
							}
							else
							{
								text = ResManager.LoadKDString("警告", "015072000025095", 7, new object[0]);
							}
						}
						else
						{
							text = ResManager.LoadKDString("提示", "015072000025094", 7, new object[0]);
						}
					}
					this._progressMsg.Add(string.Format("[{4}]{3}:[{0}]{1}{2}", new object[]
					{
						text,
						DataEntityExtend.GetDynamicValue<string>(dynamicObject, "message", null),
						text2,
						DataEntityExtend.GetDynamicValue<string>(dynamicObject, "period", null),
						DataEntityExtend.GetDynamicValue<DateTime>(dynamicObject, "DetailTime", default(DateTime))
					}));
				}
			}
			if (ListUtils.IsEmpty<string>(this._progressMsg))
			{
				this.View.ShowMessage(ResManager.LoadKDString("没有需要导出的日志数据", "015072000012291", 7, new object[0]), 0);
				return;
			}
			string text3 = HttpContext.Current.Server.MapPath(KeyConst.TEMPFILEPATH);
			string text4 = string.Format("{0}_{1}.txt", ResManager.LoadKDString("模型配置执行记录", "015072000017382", 7, new object[0]), DateTime.Now.ToString("yyyyMMddHHmmssffff"));
			if (!Directory.Exists(text3))
			{
				Directory.CreateDirectory(text3);
			}
			string path = Path.Combine(text3, text4);
			if (File.Exists(path))
			{
				File.Delete(path);
			}
			using (FileStream fileStream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.Read))
			{
				BinaryWriter binaryWriter = new BinaryWriter(fileStream);
				byte[] bytes = Encoding.Default.GetBytes(string.Join(Environment.NewLine, this._progressMsg));
				binaryWriter.Write(bytes);
				binaryWriter.Close();
				fileStream.Close();
			}
			string value = string.Concat(new string[]
			{
				this.GetWebAppRootUrl(),
				"/",
				KeyConst.TEMPFILEPATH,
				"/",
				HttpUtility.UrlEncode(text4)
			});
			DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
			dynamicFormShowParameter.FormId = "BOS_FileDownLoad";
			dynamicFormShowParameter.CustomParams.Add("url", value);
			dynamicFormShowParameter.CustomParams.Add("linktext", text4);
			this.View.ShowForm(dynamicFormShowParameter, delegate(FormResult x)
			{
				object returnData = x.ReturnData;
				this.View.ShowMessage(ResManager.LoadKDString("需要清除本次计算输出的日志吗？", "015072000017383", 7, new object[0]), 4, delegate(MessageBoxResult mbResult)
				{
					if (mbResult == 6 && !ListUtils.IsEmpty<DynamicObject>(msgDetails))
					{
						BusinessDataServiceHelper.Delete(this.Context, (from msd in msgDetails
						select msd["Id"]).ToArray<object>(), this.progressDetailMeta.BusinessInfo.GetDynamicObjectType());
					}
				}, "", 3);
			});
		}

		// Token: 0x06000BDF RID: 3039 RVA: 0x00088B20 File Offset: 0x00086D20
		private string GetWebAppRootUrl()
		{
			Uri uri;
			if (Uri.TryCreate(HttpContext.Current.Request.ApplicationPath, UriKind.Absolute, out uri))
			{
				return uri.AbsoluteUri;
			}
			return "";
		}

		// Token: 0x04000588 RID: 1416
		private List<DynamicObject> allocQueue;

		// Token: 0x04000589 RID: 1417
		private DynamicObjectCollection calResult;

		// Token: 0x0400058A RID: 1418
		private string _taskId;

		// Token: 0x0400058B RID: 1419
		private bool _catchException;

		// Token: 0x0400058C RID: 1420
		private bool _isProcessing;

		// Token: 0x0400058D RID: 1421
		private List<string> _progressMsg = new List<string>();

		// Token: 0x0400058E RID: 1422
		private int _progressValue;

		// Token: 0x0400058F RID: 1423
		private JSONObject MdlCfgParameter;

		// Token: 0x04000590 RID: 1424
		private MdlCfgOption outputOption;

		// Token: 0x04000591 RID: 1425
		private MdlCfgOption inputOption;

		// Token: 0x04000592 RID: 1426
		private bool IsFromOptr;

		// Token: 0x04000593 RID: 1427
		private JSONObject OptOption;

		// Token: 0x04000594 RID: 1428
		private FormMetadata progressDetailMeta;
	}
}
