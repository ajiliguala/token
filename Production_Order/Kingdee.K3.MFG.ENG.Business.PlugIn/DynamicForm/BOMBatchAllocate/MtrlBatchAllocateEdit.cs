using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using Kingdee.BOS;
using Kingdee.BOS.BusinessEntity.Organizations;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Permission;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Core.Validation;
using Kingdee.BOS.Log;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.Core.MFG.Utils;
using Kingdee.K3.MFG.ServiceHelper.ENG;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm.BOMBatchAllocate
{
	// Token: 0x02000063 RID: 99
	public class MtrlBatchAllocateEdit : AbstractDynamicFormPlugIn
	{
		// Token: 0x06000727 RID: 1831 RVA: 0x0005380C File Offset: 0x00051A0C
		public override void BeforeBindData(EventArgs e)
		{
			object customParameter = this.View.OpenParameter.GetCustomParameter("NeedAllocMtrls");
			if (customParameter == null)
			{
				return;
			}
			this._isAutoAudit = OtherExtend.ConvertTo<bool>(this.View.OpenParameter.GetCustomParameter("IsAutoAudit"), false);
			List<string> list = customParameter as List<string>;
			list = list.Distinct<string>().ToList<string>();
			List<long> msterIds = list.Select(delegate(string x)
			{
				string[] array2 = x.Split(new string[]
				{
					"_"
				}, StringSplitOptions.RemoveEmptyEntries);
				return OtherExtend.ConvertTo<long>(array2[0], 0L);
			}).ToList<long>();
			Dictionary<long, long> dctMaterMaterialId = this.GetDctMaterMaterialId(msterIds);
			this.Model.ClearNoDataRow();
			Entity entity = this.View.BusinessInfo.GetEntity("FEntity");
			Dictionary<long, BaseDataControlPolicy> dictionary = new Dictionary<long, BaseDataControlPolicy>();
			foreach (string text in list)
			{
				string[] array = text.Split(new string[]
				{
					"_"
				}, StringSplitOptions.RemoveEmptyEntries);
				long key = OtherExtend.ConvertTo<long>(array[0], 0L);
				long targetOrgId = OtherExtend.ConvertTo<long>(array[1], 0L);
				int num = 0;
				DataEntityExtend.CreateNewEntryRow(this.Model, entity, -1, ref num);
				this.Model.SetValue("FMaterialId", dctMaterMaterialId[key], num);
				DynamicObject dynamicObject = this.Model.GetValue("FMaterialId", num) as DynamicObject;
				long num2 = OtherExtend.ConvertTo<long>(dynamicObject["CreateOrgId_Id"], 0L);
				this.Model.SetValue("FCreateOrgId", num2, num);
				this.Model.SetValue("FUseOrgId", targetOrgId, num);
				BaseDataControlPolicy baseDataControlPolicy = null;
				if (DataEntityExtend.GetDynamicValue<string>(dynamicObject, "DocumentStatus", null) != "C" || DataEntityExtend.GetDynamicValue<string>(dynamicObject, "ForbidStatus", null) != "A")
				{
					this.Model.SetValue("FAllocStatus", "4", num);
				}
				if (!dictionary.TryGetValue(num2, out baseDataControlPolicy))
				{
					baseDataControlPolicy = OrganizationServiceHelper.GetBaseDataControlPolicyDObj(base.Context, num2, "BD_MATERIAL");
					if (baseDataControlPolicy == null)
					{
						this.Model.SetValue("FAllocStatus", "3", num);
						continue;
					}
					dictionary.Add(num2, baseDataControlPolicy);
				}
				DynamicObjectCollection source = baseDataControlPolicy.DataEntity["TargetOrgEntrys"] as DynamicObjectCollection;
				if (!source.Any((DynamicObject x) => OtherExtend.ConvertTo<long>(x["TargetOrg_Id"], 0L) == targetOrgId))
				{
					this.Model.SetValue("FAllocStatus", "3", num);
				}
				this.Model.SetValue("FKey", text, num);
			}
		}

		// Token: 0x06000728 RID: 1832 RVA: 0x00053AD8 File Offset: 0x00051CD8
		public override void AfterBarItemClick(AfterBarItemClickEventArgs e)
		{
			string barItemKey;
			if ((barItemKey = e.BarItemKey) != null)
			{
				if (barItemKey == "tbExecute")
				{
					this.DoAlloc();
					return;
				}
				if (!(barItemKey == "tbExport"))
				{
					return;
				}
				this.ExportToTxtFile();
			}
		}

		// Token: 0x06000729 RID: 1833 RVA: 0x00053B18 File Offset: 0x00051D18
		public override void AuthPermissionBeforeF7Select(AuthPermissionBeforeF7SelectEventArgs e)
		{
			string fieldKey;
			if ((fieldKey = e.FieldKey) != null)
			{
				if (!(fieldKey == "FMaterialId"))
				{
					return;
				}
				e.IsIsolationOrg = false;
			}
		}

		// Token: 0x0600072A RID: 1834 RVA: 0x00053B44 File Offset: 0x00051D44
		public override void BeforeSetItemValueByNumber(BeforeSetItemValueByNumberArgs e)
		{
			string baseDataFieldKey;
			if ((baseDataFieldKey = e.BaseDataFieldKey) != null)
			{
				if (!(baseDataFieldKey == "FMaterialId"))
				{
					return;
				}
				e.IsShowUsed = false;
				e.IsShowApproved = false;
			}
		}

		// Token: 0x0600072B RID: 1835 RVA: 0x00053B78 File Offset: 0x00051D78
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			string fieldKey;
			if ((fieldKey = e.FieldKey) != null)
			{
				if (!(fieldKey == "FMaterialId"))
				{
					return;
				}
				e.IsShowApproved = false;
				e.IsShowUsed = false;
			}
		}

		// Token: 0x0600072C RID: 1836 RVA: 0x00053BAC File Offset: 0x00051DAC
		public override void OnQueryProgressValue(QueryProgressValueEventArgs e)
		{
			if (e.Value >= 100 || this._catchException)
			{
				return;
			}
			int num = this._progressMsg.Count - 30;
			num = ((num < 0) ? 0 : num);
			string text = string.Join(Environment.NewLine, this._progressMsg.Skip(num));
			this.Model.SetValue("FRemarks", text);
			e.Value = this._progressValue;
		}

		// Token: 0x0600072D RID: 1837 RVA: 0x00053C40 File Offset: 0x00051E40
		private Dictionary<long, long> GetDctMaterMaterialId(List<long> msterIds)
		{
			QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter
			{
				FormId = "BD_MATERIAL",
				SelectItems = SelectorItemInfo.CreateItems(new string[]
				{
					"FMasterId",
					"FMaterialId"
				}),
				FilterClauseWihtKey = "FUseOrgId=FCreateOrgId"
			};
			queryBuilderParemeter.ExtJoinTables.Add(new ExtJoinTableDescription
			{
				FieldName = "FID",
				TableName = "table(fn_StrSplit(@ids,',',1))",
				TableNameAs = "ts",
				ScourceKey = "FMasterId"
			});
			DynamicObjectCollection dynamicObjectCollection = QueryServiceHelper.GetDynamicObjectCollection(base.Context, queryBuilderParemeter, new List<SqlParam>
			{
				new SqlParam("@ids", 161, msterIds.Distinct<long>().ToArray<long>())
			});
			Dictionary<long, long> result = new Dictionary<long, long>();
			if (!ListUtils.IsEmpty<DynamicObject>(dynamicObjectCollection))
			{
				result = dynamicObjectCollection.ToDictionary((DynamicObject x) => OtherExtend.ConvertTo<long>(x["FMasterId"], 0L), (DynamicObject v) => OtherExtend.ConvertTo<long>(v["FMaterialId"], 0L));
			}
			return result;
		}

		// Token: 0x0600072E RID: 1838 RVA: 0x000543B8 File Offset: 0x000525B8
		private void DoAlloc()
		{
			Entity entity = this.View.BusinessInfo.GetEntity("FEntity");
			DynamicObjectCollection allocList = this.Model.GetEntityDataObject(entity);
			if (allocList.Any((DynamicObject x) => DataEntityExtend.GetDynamicValue<string>(x, "AllocStatus", null) == "4"))
			{
				this.View.ShowErrMessage("", ResManager.LoadKDString("存在未审核或已禁用的物料，请预先处理完毕。", "015072000033843", 7, new object[0]), 0);
				return;
			}
			foreach (DynamicObject dynamicObject in allocList)
			{
				if (OtherExtend.ConvertTo<string>(dynamicObject["AllocStatus"], null) == "3")
				{
					this.View.ShowErrMessage(ResManager.LoadKDString("未设置物料分配关系，请设置物料分配关系！", "015072000013256", 7, new object[0]), "", 0);
					return;
				}
			}
			this.View.ShowMessage(ResManager.LoadKDString("开始执行分配操作，请切换至执行进度页签内查看分配的详细信息。", "015072000012293", 7, new object[0]), 0);
			this._progressValue = 0;
			this._catchException = false;
			this._progressMsg.Clear();
			this._isProcessing = true;
			this._successMtrlId.Clear();
			ProgressBar control = this.View.GetControl<ProgressBar>("FProgressBar");
			control.Enabled = true;
			control.Start(5);
			this.View.UpdateView("FProgressBar");
			FormMetadata bomMeta = MetaDataServiceHelper.Load(base.Context, "BD_MATERIAL", true) as FormMetadata;
			allocList.ToDictionary((DynamicObject x) => OtherExtend.ConvertTo<string>(x["Key"], null));
			FormUtils.RunTask(this.View, delegate()
			{
				try
				{
					this._progressMsg.Add(ResManager.LoadKDString("开始进行分配操作……", "015072000012069", 7, new object[0]));
					this._progressValue = 30;
					foreach (DynamicObject dynamicObject2 in allocList)
					{
						DynamicObject dynamicObject3 = dynamicObject2["MaterialId"] as DynamicObject;
						DynamicObject dynamicObject4 = dynamicObject3["CreateOrgId"] as DynamicObject;
						LocaleValue localeValue = dynamicObject4["Name"] as LocaleValue;
						DynamicObject dynamicObject5 = dynamicObject2["UseOrgId"] as DynamicObject;
						LocaleValue localeValue2 = dynamicObject5["Name"] as LocaleValue;
						this._progressMsg.Add(string.Format(ResManager.LoadKDString("创建组织【{1}】编码为【{0}】的物料正在进行分配,分配目标组织为【{2}】……", "015072000012294", 7, new object[0]), dynamicObject3["Number"], localeValue[this.Context.UserLocale.LCID], localeValue2[this.Context.UserLocale.LCID]));
						this._progressValue += 70 / allocList.Count;
						long num = OtherExtend.ConvertTo<long>(dynamicObject2["UseOrgId_Id"], 0L);
						long num2 = OtherExtend.ConvertTo<long>(dynamicObject3["createOrgId_Id"], 0L);
						long num3 = OtherExtend.ConvertTo<long>(dynamicObject3["Id"], 0L);
						new List<string>();
						IOperationResult operationResult = BOMServiceHelper.Allocate(this.Context, bomMeta, num3, num, num2, localeValue2[this.Context.UserLocale.LCID]);
						if (operationResult.IsSuccess)
						{
							OptrChain optrChain = new OptrChain
							{
								BizInfo = bomMeta.BusinessInfo,
								Datas = operationResult.SuccessDataEnity.ToList<DynamicObject>(),
								Result = new OperationResult
								{
									IsSuccess = true
								}
							};
							optrChain.Init();
							if (this._isAutoAudit)
							{
								optrChain.ChianLinks.Dequeue();
							}
							else
							{
								optrChain.ChianLinks.Clear();
							}
							optrChain.DoOperations(this.Context);
							if (optrChain.Result.IsSuccess)
							{
								this._progressMsg.Add(string.Format(ResManager.LoadKDString("创建组织【{1}】编码为【{0}】的物料分配完毕……", "015072000012295", 7, new object[0]), dynamicObject3["Number"], localeValue));
								this._successMtrlId.Add(OtherExtend.ConvertTo<long>(operationResult.SuccessDataEnity.First<DynamicObject>()["Id"], 0L));
								dynamicObject2["AllocStatus"] = "1";
							}
							else
							{
								this._progressMsg.Add(string.Format(ResManager.LoadKDString("创建组织【{1}】编码为【{0}】的物料提交审核失败，异常信息如下：", "015072000033345", 7, new object[0]), dynamicObject3["Number"], localeValue));
								this._progressMsg.AddRange(from x in optrChain.Result.OperateResult
								select x.Message);
								dynamicObject2["AllocStatus"] = "2";
							}
						}
						else
						{
							List<string> list = new List<string>();
							list.AddRange(from x in operationResult.ValidationErrors
							select x.Message);
							List<OperateResult> list2 = (from x in operationResult.OperateResult
							where !x.SuccessStatus
							select x).ToList<OperateResult>();
							if (!ListUtils.IsEmpty<OperateResult>(list2))
							{
								list.AddRange(from x in list2
								select x.Message);
							}
							this._progressMsg.Add(string.Format(ResManager.LoadKDString("创建组织【{1}】编码为【{0}】的物料分配失败，异常信息如下：", "015072000012296", 7, new object[0]), dynamicObject3["Number"], localeValue));
							this._progressMsg.AddRange(list);
							dynamicObject2["AllocStatus"] = "2";
						}
					}
				}
				catch (Exception ex)
				{
					Logger.Error("ENG", ex.Message, ex);
					this._progressMsg.Add(ex.Message);
					this._progressMsg.Add(ex.StackTrace);
					throw new Exception(string.Format("执行批量分配期间发生异常，异常信息：{0}   \r\n                    请查看系统日志了解详细异常信息", ex.Message));
				}
			}, delegate(string message)
			{
				this.View.ShowErrMessage(ResManager.LoadKDString("执行分配期间发生异常，请查看系统日志了解详细异常信息", "015072000012075", 7, new object[0]), "", 0);
				this._catchException = true;
				this._isProcessing = false;
				ProgressBar control2 = this.View.GetControl<ProgressBar>("FProgressBar");
				control2.Enabled = false;
				control2.Start(100000);
				this.View.UpdateView("FProgressBar");
			}, delegate()
			{
				this._progressMsg.Add(ResManager.LoadKDString("分配操作已全部完成!", "015072000012741", 7, new object[0]));
				this._progressValue = 100;
				this.View.UpdateView("FEntity");
				this._isProcessing = false;
				ProgressBar control2 = this.View.GetControl<ProgressBar>("FProgressBar");
				control2.Enabled = false;
				control2.Start(100000);
				this.View.UpdateView("FProgressBar");
				this.ShowAfterAllocateBomList();
			});
		}

		// Token: 0x0600072F RID: 1839 RVA: 0x000545C4 File Offset: 0x000527C4
		private void ShowAfterAllocateBomList()
		{
			if (ListUtils.IsEmpty<long>(this._successMtrlId))
			{
				return;
			}
			List<long> permissionOrg = PermissionServiceHelper.GetPermissionOrg(base.Context, new BusinessObject
			{
				Id = "BD_MATERIAL"
			}, "6e44119a58cb4a8e86f6c385e14a17ad");
			ListShowParameter listShowParameter = new ListShowParameter();
			listShowParameter.FormId = "BD_MATERIAL";
			listShowParameter.PermissionItemId = "6e44119a58cb4a8e86f6c385e14a17ad";
			listShowParameter.ParentPageId = this.View.ParentFormView.PageId;
			listShowParameter.OpenStyle.ShowType = 7;
			listShowParameter.IsShowFilter = false;
			listShowParameter.MutilListUseOrgId = string.Join<long>(",", permissionOrg);
			listShowParameter.ExtJoinTables.Add(new ExtJoinTableDescription
			{
				FieldName = "FID",
				JoinOption = 1,
				ScourceKey = "FMATERIALID",
				TableName = string.Format("table(fn_StrSplit(@mtrlId,',',1))", new object[0]),
				TableNameAs = "ts"
			});
			SqlParam item = new SqlParam("@mtrlId", 161, this._successMtrlId.Distinct<long>().ToArray<long>());
			listShowParameter.SqlParams.Add(item);
			this.View.ShowForm(listShowParameter);
		}

		// Token: 0x06000730 RID: 1840 RVA: 0x000546E8 File Offset: 0x000528E8
		private void ExportToTxtFile()
		{
			if (ListUtils.IsEmpty<string>(this._progressMsg))
			{
				this.View.ShowMessage(ResManager.LoadKDString("没有需要导出的日志数据", "015072000012291", 7, new object[0]), 0);
				return;
			}
			string text = HttpContext.Current.Server.MapPath(KeyConst.TEMPFILEPATH);
			string text2 = string.Format("{0}_{1}.txt", ResManager.LoadKDString("物料批量分配记录", "015072000012292", 7, new object[0]), DateTime.Now.ToString("yyyyMMddHHmmssffff"));
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			string path = Path.Combine(text, text2);
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
				HttpUtility.UrlEncode(text2)
			});
			DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
			dynamicFormShowParameter.FormId = "BOS_FileDownLoad";
			dynamicFormShowParameter.CustomParams.Add("url", value);
			dynamicFormShowParameter.CustomParams.Add("linktext", text2);
			this.View.ShowForm(dynamicFormShowParameter);
		}

		// Token: 0x06000731 RID: 1841 RVA: 0x00054878 File Offset: 0x00052A78
		private string GetWebAppRootUrl()
		{
			Uri uri;
			if (Uri.TryCreate(HttpContext.Current.Request.ApplicationPath, UriKind.Absolute, out uri))
			{
				return uri.AbsoluteUri;
			}
			return "";
		}

		// Token: 0x04000332 RID: 818
		private int _progressValue;

		// Token: 0x04000333 RID: 819
		private bool _catchException;

		// Token: 0x04000334 RID: 820
		private bool _isProcessing;

		// Token: 0x04000335 RID: 821
		private List<long> _successMtrlId = new List<long>();

		// Token: 0x04000336 RID: 822
		private bool _isAutoAudit;

		// Token: 0x04000337 RID: 823
		private List<string> _progressMsg = new List<string>();
	}
}
