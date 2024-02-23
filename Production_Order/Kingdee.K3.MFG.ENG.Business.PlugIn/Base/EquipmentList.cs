using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.List.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Core.Validation;
using Kingdee.BOS.JSON;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.BOS.VerificationHelper;
using Kingdee.K3.MFG.ServiceHelper.DI;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.Base
{
	// Token: 0x0200004D RID: 77
	public class EquipmentList : BaseControlList
	{
		// Token: 0x17000029 RID: 41
		// (get) Token: 0x0600054A RID: 1354 RVA: 0x0003FC77 File Offset: 0x0003DE77
		private List<int> lstCurSelId
		{
			get
			{
				return (from p in this.ListView.SelectedRowsInfo
				select Convert.ToInt32(p.PrimaryKeyValue)).Distinct<int>().ToList<int>();
			}
		}

		// Token: 0x1700002A RID: 42
		// (get) Token: 0x0600054B RID: 1355 RVA: 0x0003FCD8 File Offset: 0x0003DED8
		private Dictionary<int, string> dctCurSelIdNum
		{
			get
			{
				var source = (from p in this.ListView.SelectedRowsInfo
				select new
				{
					Id = Convert.ToInt32(p.PrimaryKeyValue),
					Number = p.Number
				}).Distinct();
				return source.ToDictionary(p => p.Id, p => p.Number);
			}
		}

		// Token: 0x0600054C RID: 1356 RVA: 0x0003FD58 File Offset: 0x0003DF58
		public override void PrepareFilterParameter(FilterArgs e)
		{
			if (ObjectUtils.IsNullOrEmptyOrWhiteSpace(e.SortString))
			{
				Field billNoField = this.View.BillBusinessInfo.GetBillNoField();
				if (billNoField != null)
				{
					e.SortString = billNoField.Key;
				}
			}
			base.PrepareFilterParameter(e);
		}

		// Token: 0x0600054D RID: 1357 RVA: 0x0003FDA8 File Offset: 0x0003DFA8
		public override void BeforeDoOperation(BeforeDoOperationEventArgs e)
		{
			base.BeforeDoOperation(e);
			string a;
			if ((a = e.Operation.FormOperation.Operation.ToString().ToUpper()) != null)
			{
				if (!(a == "CHANGELOG"))
				{
					if (!(a == "CHANGELOGDELETE"))
					{
						if (!(a == "TYPEMODIFY"))
						{
							if (!(a == "SETWHITELIST"))
							{
								return;
							}
							LicenseVerifier.CheckViewOnlyOperation(base.Context, ResManager.LoadKDString("同步白名单", "015072000014190", 7, new object[0]));
							Action<MessageBoxResult> action = delegate(MessageBoxResult yesRes)
							{
								if (6 == yesRes)
								{
									this.SetWhiteList();
								}
							};
							string arg = ResManager.LoadKDString("白名单将按设备中的设置全部重新同步Scada系统，确定要同步吗？", "015072000035378", 7, new object[0]);
							string text = string.Format(ResManager.LoadKDString("提示:{0}", "015072000035413", 7, new object[0]), arg);
							this.View.ShowMessage("", 4, action, text, 0);
						}
						else
						{
							LicenseVerifier.CheckViewOnlyOperation(base.Context, ResManager.LoadKDString("批改", "015072030034325", 7, new object[0]));
							if (this.ListView.SelectedRowsInfo.Count == 0)
							{
								this.View.ShowErrMessage(this.errMsg, "", 0);
								e.Cancel = true;
								return;
							}
							this.ShowModify("ENG_TypeModification");
							return;
						}
					}
					else
					{
						if (this.ListView.SelectedRowsInfo.Count == 0)
						{
							this.View.ShowErrMessage(this.errMsg, "", 0);
							e.Cancel = true;
							return;
						}
						this.ShowForm("ENG_ChangeLogDelete");
						return;
					}
				}
				else
				{
					LicenseVerifier.CheckViewOnlyOperation(base.Context, ResManager.LoadKDString("状态变更", "015072030034324", 7, new object[0]));
					if (this.ListView.SelectedRowsInfo.Count == 0)
					{
						this.View.ShowErrMessage(this.errMsg, "", 0);
						e.Cancel = true;
						return;
					}
					this.ShowList1("ENG_EqmStatusChgLogDym");
					return;
				}
			}
		}

		// Token: 0x0600054E RID: 1358 RVA: 0x0003FF98 File Offset: 0x0003E198
		private void ShowList(string formId)
		{
			string[] array = this.ListView.SelectedRowsInfo.GetPrimaryKeyValues();
			array = array.Distinct<string>().ToArray<string>();
			int num = array.Count<string>();
			long[] array2 = new long[num];
			for (int i = 0; i < num; i++)
			{
				array2[i] = Convert.ToInt64(array[i]);
			}
			ListShowParameter listShowParameter = new ListShowParameter();
			listShowParameter.FormId = formId;
			listShowParameter.ParentPageId = this.View.PageId;
			listShowParameter.IsLookUp = false;
			listShowParameter.IsIsolationOrg = true;
			listShowParameter.UseOrgId = base.Context.CurrentOrganizationInfo.ID;
			listShowParameter.IsShowUsed = true;
			listShowParameter.OpenStyle.ShowType = 6;
			listShowParameter.Height = 600;
			listShowParameter.Width = 900;
			listShowParameter.ExtJoinTables.Add(new ExtJoinTableDescription
			{
				FieldName = "FID",
				JoinOption = 1,
				ScourceKey = "FEquipmentId",
				TableName = "table(fn_StrSplit(@FID, ',' , 1))",
				TableNameAs = "TMP"
			});
			SqlParam item = new SqlParam("@FID", 161, array2);
			listShowParameter.SqlParams.Add(item);
			this.View.ShowForm(listShowParameter);
		}

		// Token: 0x0600054F RID: 1359 RVA: 0x000400D8 File Offset: 0x0003E2D8
		private void ShowList1(string formId)
		{
			string[] array = this.ListView.SelectedRowsInfo.GetPrimaryKeyValues();
			array = array.Distinct<string>().ToArray<string>();
			int num = array.Count<string>();
			long[] array2 = new long[num];
			for (int i = 0; i < num; i++)
			{
				array2[i] = Convert.ToInt64(array[i]);
			}
			DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
			dynamicFormShowParameter.FormId = formId;
			dynamicFormShowParameter.ParentPageId = this.View.PageId;
			dynamicFormShowParameter.OpenStyle.ShowType = 6;
			dynamicFormShowParameter.OpenStyle.ShowType = 6;
			dynamicFormShowParameter.Height = 600;
			dynamicFormShowParameter.Width = 900;
			dynamicFormShowParameter.CustomComplexParams["eqmIds"] = array2.ToList<long>();
			dynamicFormShowParameter.CustomComplexParams["IsOpenByEqm"] = true;
			this.View.ShowForm(dynamicFormShowParameter);
		}

		// Token: 0x06000550 RID: 1360 RVA: 0x000401B8 File Offset: 0x0003E3B8
		private void ShowForm(string formId)
		{
			DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
			dynamicFormShowParameter.FormId = formId;
			dynamicFormShowParameter.ParentPageId = this.View.PageId;
			dynamicFormShowParameter.OpenStyle.ShowType = 6;
			if (this.ListView.SelectedRowsInfo.Count > 0)
			{
				string[] primaryKeyValues = this.ListView.SelectedRowsInfo.GetPrimaryKeyValues();
				dynamicFormShowParameter.CustomParams.Add("EquipmentIds", string.Join(",", primaryKeyValues));
			}
			this.View.ShowForm(dynamicFormShowParameter);
		}

		// Token: 0x06000551 RID: 1361 RVA: 0x000402F8 File Offset: 0x0003E4F8
		private void ShowModify(string formId)
		{
			DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
			dynamicFormShowParameter.FormId = formId;
			dynamicFormShowParameter.ParentPageId = this.View.PageId;
			dynamicFormShowParameter.OpenStyle.ShowType = 6;
			List<KeyValuePair<object, object>> pkentryIdValues = this.ListView.SelectedRowsInfo.GetPKEntryIdValues();
			dynamicFormShowParameter.CustomComplexParams.Add("DicMap", pkentryIdValues);
			this.View.ShowForm(dynamicFormShowParameter, delegate(FormResult result)
			{
				if (result != null && result.ReturnData != null)
				{
					List<ValidationErrorInfo> list = (List<ValidationErrorInfo>)result.ReturnData;
					OperateResultCollection operateResultCollection = new OperateResultCollection();
					foreach (ValidationErrorInfo validationErrorInfo in list)
					{
						operateResultCollection.Add(new OperateResult
						{
							Message = validationErrorInfo.Message,
							SuccessStatus = false,
							Name = validationErrorInfo.Title
						});
					}
					if (operateResultCollection.Count<OperateResult>() > 0)
					{
						this.View.ShowOperateResult(operateResultCollection, "BOS_BatchTips");
					}
				}
				this.ListView.Refresh();
			});
		}

		// Token: 0x06000552 RID: 1362 RVA: 0x0004036C File Offset: 0x0003E56C
		private void SetWhiteList()
		{
			DateTime systemDateTime = TimeServiceHelper.GetSystemDateTime(base.Context);
			Dictionary<string, Dictionary<string, List<Tuple<string, bool, bool>>>> whiteList = DICommonServericeHelper.GetWhiteList(base.Context);
			StringBuilder stringBuilder = new StringBuilder();
			StringBuilder stringBuilder2 = new StringBuilder();
			StringBuilder stringBuilder3 = new StringBuilder();
			for (int i = 0; i < whiteList.Count; i++)
			{
				StringBuilder stringBuilder4 = new StringBuilder();
				string text = whiteList.Keys.ElementAt(i);
				stringBuilder4.AppendFormat("{0}[", text);
				Dictionary<string, List<Tuple<string, bool, bool>>> dictionary = whiteList[text];
				JSONObject jsonobject = new JSONObject();
				JSONArray jsonarray = new JSONArray();
				JSONObject jsonobject2 = new JSONObject();
				JSONArray jsonarray2 = new JSONArray();
				for (int j = 0; j < dictionary.Count; j++)
				{
					string text2 = dictionary.Keys.ElementAt(j);
					JSONObject jsonobject3 = new JSONObject();
					stringBuilder4.Append(text2);
					List<Tuple<string, bool, bool>> list = dictionary[text2];
					JSONArray jsonarray3 = new JSONArray();
					stringBuilder4.Append("(");
					for (int k = 0; k < list.Count; k++)
					{
						Tuple<string, bool, bool> tuple = list.ElementAt(k);
						if (tuple.Item2)
						{
							JSONObject jsonobject4 = new JSONObject();
							jsonobject4.Add("Name", tuple.Item1);
							jsonobject4.Add("Type", "0");
							jsonarray3.Add(jsonobject4);
							stringBuilder4.Append(tuple.Item1);
						}
						if (tuple.Item3)
						{
							JSONObject jsonobject5 = new JSONObject();
							jsonobject5.Add("Name", tuple.Item1);
							jsonobject5.Add("Type", "1");
							jsonarray3.Add(jsonobject5);
							stringBuilder4.Append(tuple.Item1);
						}
						if (k < list.Count - 1)
						{
							stringBuilder4.Append(",");
						}
					}
					jsonobject3.Add("Tags", jsonarray3);
					stringBuilder4.Append(") ");
					if (j < dictionary.Count - 1)
					{
						stringBuilder4.Append(",");
					}
					jsonobject3.Add("Name", text2);
					jsonarray2.Add(jsonobject3);
				}
				jsonobject2.Add("Name", text);
				jsonobject2.Add("Devices", jsonarray2);
				jsonarray.Add(jsonobject2);
				jsonobject.Add("Projects", jsonarray);
				stringBuilder4.Append("]");
				if (i < whiteList.Count - 1)
				{
					stringBuilder4.Append(",");
				}
				string text3 = KDObjectConverter.SerializeObject(jsonobject);
				string text4 = DICommonServericeHelper.InterfaceCall(base.Context, text, "E", "POST", text3);
				JSONObject jsonobject6 = null;
				try
				{
					jsonobject6 = JSONObject.Parse(text4);
				}
				catch
				{
					this.View.ShowErrMessage(ResManager.LoadKDString("同步失败，可能是设备联网的WEB服务连接失败！", "015072000013090", 7, new object[0]), "", 0);
					return;
				}
				if (jsonobject6 != null)
				{
					JSONObject jsonobject7 = (JSONObject)jsonobject6.Get("Result");
					string text5 = Convert.ToString(jsonobject7.Get("Ret"));
					if (text5.Equals("0"))
					{
						if (stringBuilder.Length > 0)
						{
							stringBuilder.Append(",");
						}
						stringBuilder.Append(stringBuilder4.ToString());
					}
					else
					{
						string empty = string.Empty;
						jsonobject7.TryGetValue<string>("Message", string.Empty, ref empty);
						stringBuilder3.AppendLine(empty);
					}
				}
				else
				{
					if (stringBuilder2.Length > 0)
					{
						stringBuilder2.Append(",");
					}
					stringBuilder2.Append(stringBuilder4.ToString());
				}
			}
			string text6 = string.Empty;
			if (stringBuilder.Length > 0 && stringBuilder2.Length > 0)
			{
				text6 = string.Format(ResManager.LoadKDString("同步白名单成功的工程、设备、点如下：{0}，不成功的工程、设备、点如下：{1}", "015072000035377", 7, new object[0]), stringBuilder.ToString(), stringBuilder2.ToString());
			}
			else if (stringBuilder.Length > 0)
			{
				text6 = string.Format(ResManager.LoadKDString("同步白名单成功的工程、设备、点如下：{0}", "015072000035376", 7, new object[0]), stringBuilder.ToString());
			}
			else if (stringBuilder2.Length > 0)
			{
				text6 = string.Format(ResManager.LoadKDString("同步白名单不成功的工程、设备、点如下：{1}", "015072000035375", 7, new object[0]), stringBuilder.ToString(), stringBuilder2.ToString());
			}
			else
			{
				text6 = ResManager.LoadKDString("不存在需要同步的点信息！", "015072000033734", 7, new object[0]);
			}
			if (stringBuilder3.Length > 0)
			{
				text6 = text6 + "\r\n" + stringBuilder3.ToString();
			}
			DICommonServericeHelper.DISaveLog(base.Context, 0L, text6, "C", "B", systemDateTime);
			this.View.ShowMessage(text6, 0);
		}

		// Token: 0x04000253 RID: 595
		private const string FKey_FEquipmentID = "FRESNUMBER";

		// Token: 0x04000254 RID: 596
		private const string FKey_FNumber = "FNUMBER";

		// Token: 0x04000255 RID: 597
		private const string FKey_FID = "FID";

		// Token: 0x04000256 RID: 598
		private bool firstDoOperation = true;

		// Token: 0x04000257 RID: 599
		private string errMsg = string.Format(ResManager.LoadKDString("请至少选择一行数据！", "015072000012056", 7, new object[0]), new object[0]);

		// Token: 0x0200004E RID: 78
		public class KeyItem
		{
			// Token: 0x0600055A RID: 1370 RVA: 0x0004083A File Offset: 0x0003EA3A
			public static string GetKey(ListSelectedRow item)
			{
				return item.PrimaryKeyValue;
			}

			// Token: 0x0400025C RID: 604
			public long Id;

			// Token: 0x0400025D RID: 605
			public string Number;
		}
	}
}
