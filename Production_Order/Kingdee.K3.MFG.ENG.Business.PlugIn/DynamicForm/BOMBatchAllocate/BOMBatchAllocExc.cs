using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using Kingdee.K3.Core.MFG.EnumConst;
using Kingdee.K3.Core.MFG.Utils;
using Kingdee.K3.MFG.ServiceHelper.ENG;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm.BOMBatchAllocate
{
	// Token: 0x0200005D RID: 93
	[Description("批量分配执行单")]
	public class BOMBatchAllocExc : AbstractDynamicFormPlugIn
	{
		// Token: 0x060006F6 RID: 1782 RVA: 0x00051F2C File Offset: 0x0005012C
		public override void OnInitialize(InitializeEventArgs e)
		{
			object customParameter = this.View.OpenParameter.GetCustomParameter("allocQueue", true);
			if (customParameter != null)
			{
				this.allocQueue = (customParameter as List<DynamicObject>);
			}
			else
			{
				this.allocQueue = new List<DynamicObject>();
			}
			object customParameter2 = this.View.OpenParameter.GetCustomParameter("calResult", true);
			if (customParameter2 != null)
			{
				this.calResult = (customParameter2 as DynamicObjectCollection);
			}
			this._isAutoAudit = OtherExtend.ConvertTo<bool>(this.View.OpenParameter.GetCustomParameter("IsAutoAudit"), false);
		}

		// Token: 0x060006F7 RID: 1783 RVA: 0x00052B08 File Offset: 0x00050D08
		public override void BeforeBindData(EventArgs e)
		{
			Entity entity = this.View.BusinessInfo.GetEntity("FAllocateQueue");
			DynamicObjectCollection allocQueueCollection = this.Model.GetEntityDataObject(entity);
			this.Model.ClearNoDataRow();
			foreach (DynamicObject item in this.allocQueue)
			{
				allocQueueCollection.Add(item);
			}
			Entity entity2 = this.View.BusinessInfo.GetEntity("FResult");
			DynamicObjectCollection entityDataObject = this.Model.GetEntityDataObject(entity2);
			if (this.calResult != null)
			{
				foreach (DynamicObject item2 in this.calResult)
				{
					entityDataObject.Add(item2);
				}
			}
			this.LockParentViewBtn(true);
			ProgressBar control = this.View.GetControl<ProgressBar>("FProgressBar");
			control.Enabled = true;
			control.Start(5);
			this.View.UpdateView("FProgressBar");
			FormMetadata bomMeta = MetaDataServiceHelper.Load(base.Context, "ENG_BOM", true) as FormMetadata;
			FormUtils.RunTask(this.View, delegate()
			{
				this._isProcessing = true;
				HashSet<string> hashSet = new HashSet<string>();
				try
				{
					this._progressMsg.Add(ResManager.LoadKDString("开始进行分配操作……", "015072000012069", 7, new object[0]));
					this._progressValue = 30;
					Dictionary<string, IGrouping<string, DynamicObject>> dictionary = (from x in this.calResult
					group x by x["Key1"].ToString()).ToDictionary((IGrouping<string, DynamicObject> x) => x.Key);
					Dictionary<string, IGrouping<string, DynamicObject>> dictionary2 = (from x in this.calResult
					group x by x["ROWID"].ToString()).ToDictionary((IGrouping<string, DynamicObject> x) => x.Key);
					foreach (DynamicObject dynamicObject in allocQueueCollection)
					{
						DynamicObject dynamicObject2 = dynamicObject["BOMID1"] as DynamicObject;
						DynamicObject dynamicObject3 = dynamicObject["CreateOrgId1"] as DynamicObject;
						LocaleValue localeValue = dynamicObject3["Name"] as LocaleValue;
						DynamicObject dynamicObject4 = dynamicObject["tgtOrgId1"] as DynamicObject;
						LocaleValue localeValue2 = dynamicObject4["Name"] as LocaleValue;
						this._progressMsg.Add(string.Format(ResManager.LoadKDString("创建组织【{1}】版本号【{0}】的物料清单正在进行分配,分配目标组织为【{2}】……", "015072000012070", 7, new object[0]), dynamicObject2["Number"], localeValue[this.Context.UserLocale.LCID], localeValue2[this.Context.UserLocale.LCID]));
						this._progressValue += 70 / allocQueueCollection.Count;
						if (DataEntityExtend.GetDynamicValue<string>(dynamicObject, "AllocStatus", null) == CONST_ENG_BOMBatchAllocate.Enu_AllocStatus.Allocated)
						{
							this._progressMsg.Add(string.Format(ResManager.LoadKDString("创建组织【{1}】版本号【{0}】的物料清单已经分配至目标组织……", "015072000012071", 7, new object[0]), dynamicObject2["Number"], localeValue));
							string key = dynamicObject["Key"].ToString();
							IGrouping<string, DynamicObject> grouping = dictionary[key];
							using (IEnumerator<DynamicObject> enumerator4 = grouping.GetEnumerator())
							{
								while (enumerator4.MoveNext())
								{
									DynamicObject dynamicObject5 = enumerator4.Current;
									dynamicObject5["AllocateResult"] = 0;
								}
								continue;
							}
						}
						string text = dynamicObject["Key"].ToString();
						if (hashSet.Contains(text))
						{
							this._progressMsg.Add(string.Format(ResManager.LoadKDString("创建组织【{1}】版本号【{0}】的物料清单分配操作中止，中止原因：下级BOM出现异常……", "015072000012072", 7, new object[0]), dynamicObject2["Number"], localeValue));
							IGrouping<string, DynamicObject> grouping2 = dictionary[text];
							using (IEnumerator<DynamicObject> enumerator5 = grouping2.GetEnumerator())
							{
								while (enumerator5.MoveNext())
								{
									DynamicObject dynamicObject6 = enumerator5.Current;
									dynamicObject6["AllocateResult"] = 2;
									string dynamicValue = DataEntityExtend.GetDynamicValue<string>(dynamicObject6, "ParentRowId", null);
									if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(dynamicValue))
									{
										IGrouping<string, DynamicObject> source = null;
										if (dictionary2.TryGetValue(dynamicValue, out source))
										{
											string item3 = source.FirstOrDefault<DynamicObject>()["Key1"].ToString();
											if (!hashSet.Contains(item3))
											{
												hashSet.Add(item3);
											}
										}
									}
								}
								continue;
							}
						}
						long num = OtherExtend.ConvertTo<long>(dynamicObject["tgtOrgId1_Id"], 0L);
						long num2 = OtherExtend.ConvertTo<long>(dynamicObject["createOrgId1_Id"], 0L);
						long num3 = OtherExtend.ConvertTo<long>(dynamicObject2["msterID"], 0L);
						IOperationResult operationResult = BOMServiceHelper.Allocate(this.Context, bomMeta, num3, num, num2, localeValue2[this.Context.UserLocale.LCID]);
						if (!operationResult.IsSuccess)
						{
							this._progressMsg.Add(string.Format(ResManager.LoadKDString("创建组织【{1}】版本号【{0}】的物料清单分配操作失败，异常信息如下：", "015072000012290", 7, new object[0]), dynamicObject2["Number"], localeValue));
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
							this._progressMsg.AddRange(list);
							IGrouping<string, DynamicObject> grouping3 = dictionary[text];
							this._dctOptrResult.Add(text, operationResult);
							using (IEnumerator<DynamicObject> enumerator6 = grouping3.GetEnumerator())
							{
								while (enumerator6.MoveNext())
								{
									DynamicObject dynamicObject7 = enumerator6.Current;
									dynamicObject7["AllocateResult"] = 1;
									dynamicObject7["Remark"] = string.Join(";", from x in operationResult.OperateResult
									select x.Message);
									string dynamicValue2 = DataEntityExtend.GetDynamicValue<string>(dynamicObject7, "ParentRowId", null);
									if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(dynamicValue2))
									{
										IGrouping<string, DynamicObject> source2 = null;
										if (dictionary2.TryGetValue(dynamicValue2, out source2))
										{
											string item4 = source2.FirstOrDefault<DynamicObject>()["Key1"].ToString();
											if (!hashSet.Contains(item4))
											{
												hashSet.Add(item4);
											}
										}
									}
								}
								continue;
							}
						}
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
							this._progressMsg.Add(string.Format(ResManager.LoadKDString("创建组织【{1}】版本号【{0}】的物料清单分配完毕……", "015072000012074", 7, new object[0]), dynamicObject2["Number"], localeValue));
							this.successBomIds.Add(OtherExtend.ConvertTo<long>(operationResult.SuccessDataEnity.First<DynamicObject>()["Id"], 0L));
							IGrouping<string, DynamicObject> grouping4 = dictionary[text];
							using (IEnumerator<DynamicObject> enumerator7 = grouping4.GetEnumerator())
							{
								while (enumerator7.MoveNext())
								{
									DynamicObject dynamicObject8 = enumerator7.Current;
									dynamicObject8["AllocateResult"] = 0;
								}
								continue;
							}
						}
						this._progressMsg.Add(string.Format(ResManager.LoadKDString("创建组织【{1}】编码为【{0}】的物料清单提交审核失败，异常信息如下：", "015072000033344", 7, new object[0]), dynamicObject2["Number"], localeValue));
						this._progressMsg.AddRange(from x in optrChain.Result.OperateResult
						select x.Message);
						IGrouping<string, DynamicObject> grouping5 = dictionary[text];
						this._dctOptrResult.Add(text, operationResult);
						foreach (DynamicObject dynamicObject9 in grouping5)
						{
							dynamicObject9["AllocateResult"] = 1;
							dynamicObject9["Remark"] = string.Join(";", from x in optrChain.Result.OperateResult
							select x.Message);
							string dynamicValue3 = DataEntityExtend.GetDynamicValue<string>(dynamicObject9, "ParentRowId", null);
							if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(dynamicValue3))
							{
								IGrouping<string, DynamicObject> source3 = null;
								if (dictionary2.TryGetValue(dynamicValue3, out source3))
								{
									string item5 = source3.FirstOrDefault<DynamicObject>()["Key1"].ToString();
									if (!hashSet.Contains(item5))
									{
										hashSet.Add(item5);
									}
								}
							}
						}
					}
				}
				catch (Exception ex)
				{
					Logger.Error("ENG", ex.Message, ex);
					this._progressMsg.Add(ex.Message);
					this._progressMsg.Add(ex.StackTrace);
					throw new Exception(string.Format("执行分配期间发生异常，异常信息：{0}   \r\n                    请查看系统日志了解详细异常信息", ex.Message));
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
				this.LockParentViewBtn(false);
			}, delegate()
			{
				this._progressMsg.Add(ResManager.LoadKDString("分配操作已全部完成!", "015072000012741", 7, new object[0]));
				this._progressValue = 100;
				this.View.UpdateView("FResult");
				ProgressBar control2 = this.View.GetControl<ProgressBar>("FProgressBar");
				control2.Enabled = false;
				control2.Start(100000);
				this.View.UpdateView("FProgressBar");
				this.LockParentViewBtn(false);
				this._isProcessing = false;
				this.ShowAfterAllocateBomList();
			});
		}

		// Token: 0x060006F8 RID: 1784 RVA: 0x00052C9C File Offset: 0x00050E9C
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

		// Token: 0x060006F9 RID: 1785 RVA: 0x00052D08 File Offset: 0x00050F08
		public override void AuthPermissionBeforeF7Select(AuthPermissionBeforeF7SelectEventArgs e)
		{
			string fieldKey;
			if ((fieldKey = e.FieldKey) != null)
			{
				if (!(fieldKey == "FBOMID1") && !(fieldKey == "FBOMID"))
				{
					return;
				}
				e.IsIsolationOrg = false;
			}
		}

		// Token: 0x060006FA RID: 1786 RVA: 0x00052D41 File Offset: 0x00050F41
		public override void BeforeClosed(BeforeClosedEventArgs e)
		{
			if (this._isProcessing)
			{
				e.Cancel = true;
				this.View.ShowErrMessage(ResManager.LoadKDString("正在执行分配，请勿关闭", "015072000012077", 7, new object[0]), "", 0);
			}
		}

		// Token: 0x060006FB RID: 1787 RVA: 0x00052D7C File Offset: 0x00050F7C
		public override void AfterBarItemClick(AfterBarItemClickEventArgs e)
		{
			string barItemKey;
			if ((barItemKey = e.BarItemKey) != null)
			{
				if (!(barItemKey == "tbExport"))
				{
					return;
				}
				this.ExportToTxtFile();
			}
		}

		// Token: 0x060006FC RID: 1788 RVA: 0x00052DA8 File Offset: 0x00050FA8
		private void LockParentViewBtn(bool isLock = true)
		{
			if (this.View.ParentFormView == null)
			{
				return;
			}
			this.View.ParentFormView.GetControl<Button>("FPrevious").Enabled = !isLock;
			this.View.ParentFormView.GetControl<Button>("FFinish").Enabled = !isLock;
			this.View.SendDynamicFormAction(this.View.ParentFormView);
		}

		// Token: 0x060006FD RID: 1789 RVA: 0x00052E18 File Offset: 0x00051018
		private void ShowAfterAllocateBomList()
		{
			if (ListUtils.IsEmpty<long>(this.successBomIds))
			{
				return;
			}
			List<long> permissionOrg = PermissionServiceHelper.GetPermissionOrg(base.Context, new BusinessObject
			{
				Id = "ENG_BOM"
			}, "6e44119a58cb4a8e86f6c385e14a17ad");
			ListShowParameter listShowParameter = new ListShowParameter();
			listShowParameter.FormId = "ENG_BOM";
			listShowParameter.PermissionItemId = "6e44119a58cb4a8e86f6c385e14a17ad";
			listShowParameter.ParentPageId = this.View.ParentFormView.PageId;
			listShowParameter.OpenStyle.ShowType = 7;
			listShowParameter.IsShowFilter = false;
			listShowParameter.MutilListUseOrgId = string.Join<long>(",", permissionOrg);
			listShowParameter.ExtJoinTables.Add(new ExtJoinTableDescription
			{
				FieldName = "FID",
				JoinOption = 1,
				ScourceKey = "FID",
				TableName = string.Format("table(fn_StrSplit(@bomid,',',1))", new object[0]),
				TableNameAs = "ts"
			});
			SqlParam item = new SqlParam("@bomid", 161, this.successBomIds.Distinct<long>().ToArray<long>());
			listShowParameter.SqlParams.Add(item);
			this.View.ShowForm(listShowParameter);
		}

		// Token: 0x060006FE RID: 1790 RVA: 0x00052F3C File Offset: 0x0005113C
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

		// Token: 0x060006FF RID: 1791 RVA: 0x000530CC File Offset: 0x000512CC
		private string GetWebAppRootUrl()
		{
			Uri uri;
			if (Uri.TryCreate(HttpContext.Current.Request.ApplicationPath, UriKind.Absolute, out uri))
			{
				return uri.AbsoluteUri;
			}
			return "";
		}

		// Token: 0x0400031A RID: 794
		private List<DynamicObject> allocQueue;

		// Token: 0x0400031B RID: 795
		private DynamicObjectCollection calResult;

		// Token: 0x0400031C RID: 796
		private bool _isAutoAudit;

		// Token: 0x0400031D RID: 797
		private string _taskId;

		// Token: 0x0400031E RID: 798
		private bool _catchException;

		// Token: 0x0400031F RID: 799
		private bool _isProcessing;

		// Token: 0x04000320 RID: 800
		private Dictionary<string, IOperationResult> _dctOptrResult = new Dictionary<string, IOperationResult>();

		// Token: 0x04000321 RID: 801
		private List<string> _progressMsg = new List<string>();

		// Token: 0x04000322 RID: 802
		private List<long> successBomIds = new List<long>();

		// Token: 0x04000323 RID: 803
		private int _progressValue;
	}
}
