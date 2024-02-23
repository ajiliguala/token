using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Kingdee.BOS;
using Kingdee.BOS.BusinessEntity.BaseData;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Contracts.BaseData;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DependencyRules;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.Interaction;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Log;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.ControlElement;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Core.Metadata.FormElement;
using Kingdee.BOS.Core.Permission;
using Kingdee.BOS.Core.Validation;
using Kingdee.BOS.KDThread;
using Kingdee.BOS.Log;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.BOS.Web.Bill;
using Kingdee.BOS.Web.Import;
using Kingdee.K3.Core.BD;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.MFG.ServiceHelper;
using Kingdee.K3.MFG.ServiceHelper.ENG;
using Microsoft.CSharp.RuntimeBinder;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm
{
	// Token: 0x02000066 RID: 102
	[HotUpdate]
	public class BOMChangeFieldContent : AbstractDynamicFormPlugIn
	{
		// Token: 0x0600074D RID: 1869 RVA: 0x000557F0 File Offset: 0x000539F0
		public override void OnInitialize(InitializeEventArgs e)
		{
			this.GetFieldMetadata();
			base.OnInitialize(e);
			if (this._rules != null && this._rules.Count == 0)
			{
				foreach (BOSRule item in this.View.RuleContainer.Rules)
				{
					this._rules.Add(item);
				}
				this.View.RuleContainer.Rules.Clear();
			}
			if (this.fieldKey == "FSTOCKID" && this._rules != null && this._rules.Count > 0)
			{
				foreach (object obj in this._rules)
				{
					this.View.RuleContainer.Rules.Add(obj as BOSRule);
				}
			}
		}

		// Token: 0x0600074E RID: 1870 RVA: 0x00055904 File Offset: 0x00053B04
		private void GetFieldMetadata()
		{
			if (this._metadata != null)
			{
				return;
			}
			if (this.View.OpenParameter.GetCustomParameter("FormId") != null)
			{
				this.formId = this.View.OpenParameter.GetCustomParameter("FormId").ToString();
			}
			if (this.View.OpenParameter.GetCustomParameter("FieldKey") != null)
			{
				this.fieldKey = this.View.OpenParameter.GetCustomParameter("FieldKey").ToString();
			}
			if (this.View.OpenParameter.GetCustomParameter("EntryName") != null)
			{
				this._entryName = this.View.OpenParameter.GetCustomParameter("EntryName").ToString();
			}
			if (this.View.OpenParameter.GetCustomParameter("BOMBILLNO") != null)
			{
				this._billName = new LocaleValue();
				this._billName.Add(new KeyValuePair<int, string>(base.Context.UserLocale.LCID, Convert.ToString(this.View.OpenParameter.GetCustomParameter("BOMBILLNO"))));
			}
			this._metadata = this.GetFormMetadata(this.formId);
		}

		// Token: 0x0600074F RID: 1871 RVA: 0x00055A2D File Offset: 0x00053C2D
		private FormMetadata GetFormMetadata(string strFormId)
		{
			return FormMetaDataCache.GetCachedFormMetaData(this.View.Context, strFormId);
		}

		// Token: 0x06000750 RID: 1872 RVA: 0x00055A40 File Offset: 0x00053C40
		private void AddCtrl()
		{
			int lcid = this.View.Context.UserLocale.LCID;
			LayoutInfo layoutInfo = new LayoutInfo();
			Container control = this.View.GetControl<Container>("FPanel");
			int num = 0;
			for (int i = 0; i < this._fieldAppList.Count; i++)
			{
				FieldAppearance fieldAppearance = this._fieldAppList[i];
				if (i != 0)
				{
					fieldAppearance.Caption[lcid] = ResManager.LoadKDString("┗上级资料：", "0151515153499000021267", 7, new object[0]);
					num += this._fieldAppList[i - 1].Height + 10;
					if (base.Context.UserLocale.LCID == 1033)
					{
						fieldAppearance.LabelWidth[lcid] = "105";
					}
					else
					{
						fieldAppearance.LabelWidth[lcid] = "85";
					}
					if (fieldAppearance.Key == "FSTOCKLOCID" || fieldAppearance.Key == "FBACKFLUSHTYPE")
					{
						fieldAppearance.LabelWidth[lcid] = "0";
					}
				}
				else
				{
					fieldAppearance.LabelWidth[lcid] = "0";
				}
				fieldAppearance.Top[lcid] = num.ToString();
				fieldAppearance.Left[lcid] = "0";
				fieldAppearance.Width[lcid] = "250";
				fieldAppearance.EntityKey = null;
				fieldAppearance.Container = null;
				layoutInfo.Add(fieldAppearance);
			}
			control.AddControls(layoutInfo);
		}

		// Token: 0x06000751 RID: 1873 RVA: 0x00055BD4 File Offset: 0x00053DD4
		public override void AfterBindData(EventArgs e)
		{
			this.AddCtrl();
			if (this.fieldKey == "FISSUETYPE")
			{
				this.View.StyleManager.SetEnabled("FBACKFLUSHTYPE", "FBACKFLUSHTYPE", false);
				this.View.Model.SetValue("FBACKFLUSHTYPE", "");
				return;
			}
			this.fieldKey == "FSTOCKID";
		}

		// Token: 0x06000752 RID: 1874 RVA: 0x00055C40 File Offset: 0x00053E40
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			base.BeforeF7Select(e);
			string filter = e.ListFilterParameter.Filter;
			if (e.FieldKey == "FSUPPLYORG")
			{
				string text = " FORGFUNCTIONS LIKE '%103%' ";
				IRegularFilterParameter listFilterParameter = e.ListFilterParameter;
				listFilterParameter.Filter += (string.IsNullOrEmpty(filter) ? text : (" AND " + text));
				return;
			}
			if (e.FieldKey == "FChildSupplyOrgId")
			{
				List<long> list = new List<long>();
				DynamicObjectCollection bomBatchEditOrgs = BOMServiceHelper.GetBomBatchEditOrgs(base.Context);
				foreach (DynamicObject dynamicObject in bomBatchEditOrgs)
				{
					string dynamicValue = DataEntityExtend.GetDynamicValue<string>(dynamicObject, "FORGFUNCTIONS", null);
					if (!string.IsNullOrEmpty(dynamicValue))
					{
						string[] array = dynamicValue.Split(new char[]
						{
							','
						});
						foreach (string a in array)
						{
							if (a == "101" || a == "102" || a == "103" || a == "104")
							{
								list.Add(DataEntityExtend.GetDynamicValue<long>(dynamicObject, "FORGID", 0L));
							}
						}
					}
				}
				if (!ListUtils.IsEmpty<long>(list))
				{
					string text2 = string.Format(" FORGID IN({0}) ", string.Join<long>(",", list.Distinct<long>()));
					IRegularFilterParameter listFilterParameter2 = e.ListFilterParameter;
					listFilterParameter2.Filter += (string.IsNullOrEmpty(filter) ? text2 : (" AND " + text2));
					return;
				}
				string text3 = " 0=1 ";
				IRegularFilterParameter listFilterParameter3 = e.ListFilterParameter;
				listFilterParameter3.Filter += (string.IsNullOrEmpty(filter) ? text3 : (" AND " + text3));
				return;
			}
			else
			{
				if (e.FieldKey == "FBOMID")
				{
					e.IsShowApproved = false;
					string text4 = " FBOMCATEGORY='1' ";
					IRegularFilterParameter listFilterParameter4 = e.ListFilterParameter;
					listFilterParameter4.Filter += (string.IsNullOrEmpty(filter) ? text4 : (" AND " + text4));
					return;
				}
				if (e.FieldKey == "FSTOCKID")
				{
					string text5 = " FIsGYStock='0' ";
					IRegularFilterParameter listFilterParameter5 = e.ListFilterParameter;
					listFilterParameter5.Filter += (string.IsNullOrEmpty(filter) ? text5 : (" AND " + text5));
				}
				return;
			}
		}

		// Token: 0x06000753 RID: 1875 RVA: 0x00055ECC File Offset: 0x000540CC
		public override void OnSetBusinessInfo(SetBusinessInfoArgs e)
		{
			this.GetFieldMetadata();
			this._currentBillBusinessInfo = (BusinessInfo)ObjectUtils.CreateCopy(this.View.OpenParameter.FormMetaData.BusinessInfo);
			this.CreateField(this.fieldKey);
			e.BusinessInfo = this._currentBillBusinessInfo;
			e.BillBusinessInfo = this._currentBillBusinessInfo;
			base.OnSetBusinessInfo(e);
		}

		// Token: 0x06000754 RID: 1876 RVA: 0x00055F44 File Offset: 0x00054144
		private void CreateField(string strKey)
		{
			Field field = (Field)ObjectUtils.CreateCopy(this._metadata.BusinessInfo.GetField(strKey));
			if (!string.IsNullOrEmpty(this._entryName) && strKey.ToUpper() == "FMATERIALTYPE")
			{
				ComboField comboField = field as ComboField;
				if (comboField != null)
				{
					DynamicObject enumObject = comboField.EnumObject;
					DynamicObjectCollection dynamicObjectCollection = enumObject["Items"] as DynamicObjectCollection;
					DynamicObject item2 = dynamicObjectCollection.FirstOrDefault((DynamicObject item) => Convert.ToInt16(item["Value"]) == 3);
					dynamicObjectCollection.Remove(item2);
				}
			}
			field.EntityKey = this._currentBillBusinessInfo.GetEntity(0).Key;
			field.Entity = this._currentBillBusinessInfo.GetEntity(0);
			if (!ListUtils.IsEmpty<FormBusinessService>(field.UpdateActions))
			{
				field.UpdateActions.Clear();
			}
			BaseDataField baseDataField = field as BaseDataField;
			if (baseDataField != null)
			{
				baseDataField.Filter = "";
				if (baseDataField.AdvancedFilters != null)
				{
					baseDataField.AdvancedFilters.Clear();
				}
			}
			this._currentBillBusinessInfo.Add(field);
			this._fieldList.Add(field);
			if (!StringUtils.IsEmpty(field.ControlFieldKey) && !(field is DecimalField) && strKey != "FOWNERID")
			{
				this.CreateField(field.ControlFieldKey);
			}
			if (strKey == "FSTOCKID")
			{
				this.CreateField("FSTOCKLOCID");
			}
			if (strKey == "FISSUETYPE")
			{
				this.CreateField("FBACKFLUSHTYPE");
			}
			if (strKey == "FOWNERTYPEID")
			{
				this.CreateField("FOWNERID");
			}
			if (!(strKey == "FSTOCKLOCID") && !(strKey == "FISSUETYPE") && !(strKey == "FOWNERTYPEID") && field is RelatedFlexGroupField)
			{
				RelatedFlexGroupField relatedFlexGroupField = field as RelatedFlexGroupField;
				if (relatedFlexGroupField != null)
				{
					this.CreateField(relatedFlexGroupField.RelatedBaseDataFlexGroupField);
				}
			}
		}

		// Token: 0x06000755 RID: 1877 RVA: 0x00056120 File Offset: 0x00054320
		public override void OnSetLayoutInfo(SetLayoutInfoArgs e)
		{
			this.GetFieldMetadata();
			this._currentBillLayoutInfo = (LayoutInfo)ObjectUtils.CreateCopy(this.View.OpenParameter.FormMetaData.GetLayoutInfo());
			foreach (Field field in this._fieldList)
			{
				this.CreateFieldApp(field.Key);
			}
			e.BillLayoutInfo = this._currentBillLayoutInfo;
			e.LayoutInfo = this._currentBillLayoutInfo;
			base.OnSetLayoutInfo(e);
		}

		// Token: 0x06000756 RID: 1878 RVA: 0x000561C4 File Offset: 0x000543C4
		private void CreateFieldApp(string strKey)
		{
			FieldAppearance fieldAppearance = (FieldAppearance)ObjectUtils.CreateCopy(this._metadata.GetLayoutInfo().GetAppearance(strKey));
			if (fieldAppearance == null)
			{
				return;
			}
			if (strKey == "FOverControlMode" || strKey == "FSTOCKLOCID" || strKey == "FBACKFLUSHTYPE" || strKey == "FPROCESSID" || strKey == "FPOSITIONNO" || strKey == "FISGETSCRAP" || strKey == "FOWNERTYPEID" || strKey == "FOWNERID" || strKey == "FOptQueue" || strKey == "FISMinIssueQty" || strKey == "FIsCanChoose" || strKey == "FIsCanEdit" || strKey == "FIsCanReplace" || strKey == "FSupplyMode" || strKey == "FIsMrpRun")
			{
				fieldAppearance.Visible = 1023;
				if (strKey == "FSTOCKLOCID" || strKey == "FBACKFLUSHTYPE")
				{
					fieldAppearance.Locked = 0;
				}
			}
			fieldAppearance.Field = this._currentBillBusinessInfo.GetField(strKey);
			if (!string.IsNullOrEmpty(this._entryName) && strKey.ToUpper() == "FDENOMINATOR")
			{
				fieldAppearance.Field.DataScope = "0,999999999999";
			}
			if (!string.IsNullOrEmpty(this._entryName) && (strKey.ToUpperInvariant() == "FISGETSCRAP" || strKey.ToUpperInvariant() == "FISMINISSUEQTY" || strKey.ToUpperInvariant() == "FISCANCHOOSE" || strKey.ToUpperInvariant() == "FISCANEDIT" || strKey.ToUpperInvariant() == "FISCANREPLACE" || strKey.ToUpperInvariant() == "FISMRPRUN"))
			{
				fieldAppearance.Field.DefValue = false;
			}
			this._currentBillLayoutInfo.Add(fieldAppearance);
			this._fieldAppList.Add(fieldAppearance);
		}

		// Token: 0x06000757 RID: 1879 RVA: 0x000563D4 File Offset: 0x000545D4
		public override void BeforeDoOperation(BeforeDoOperationEventArgs e)
		{
			if (e.Operation.FormOperation.OperationId == FormOperation.Operation_Save && this._fieldList.Count > 0)
			{
				Field field = this._fieldList[0];
				object value = this.Model.GetValue(field.Key);
				if (ObjectUtils.IsNullOrEmpty(value) && field.IsMustInput())
				{
					this.View.ParentFormView.ShowErrMessage(string.Format(ResManager.LoadKDString("【{0}】为必录项，不允许为空！", "0151515153499030041401", 7, new object[0]), field.Name), "", 0);
				}
				else
				{
					this._formName = this._metadata.BusinessInfo.GetForm().Name;
					this._fieldName = this._metadata.BusinessInfo.GetField(this.fieldKey).Name;
					this.WriteOperateLog(base.Context, "批改", string.Format("批改{0}的{1}", this._formName, this._fieldName));
					this.DoBatchEdit();
				}
				e.Cancel = true;
			}
			base.BeforeDoOperation(e);
		}

		// Token: 0x06000758 RID: 1880 RVA: 0x000564F0 File Offset: 0x000546F0
		public override void AfterF7Select(AfterF7SelectEventArgs e)
		{
			if (this._fieldList.Count == 2 && StringUtils.EqualsIgnoreCase(e.FieldKey, this._fieldList[1].Key) && this._fieldList[0] is RelatedFlexGroupField)
			{
				this.Model.SetValue(this._fieldList[0].Key, null);
			}
			base.AfterF7Select(e);
		}

		// Token: 0x06000759 RID: 1881 RVA: 0x00056738 File Offset: 0x00054938
		private void DoBatchEdit()
		{
			this._batchEditItems.Clear();
			this._batchEditDetails.Clear();
			this._batchID = string.Empty;
			this._progressClosed = false;
			this._fieldValues.Clear();
			this._batchEditTime = DateTime.Now;
			this._pkEntryIdsDic.Clear();
			this.mtrMsterDic.Clear();
			Form form = this._metadata.BusinessInfo.GetForm();
			string id = form.Id;
			int supportPermissionControl = form.SupportPermissionControl;
			BusinessObject businessObject = new BusinessObject
			{
				PermissionControl = supportPermissionControl,
				Id = id
			};
			HashSet<string> hashSet = new HashSet<string>();
			HashSet<string> hashSet2 = new HashSet<string>();
			List<FieldAuthResult> allowFieldList = PermissionServiceHelper.GetAllowFieldList(base.Context, businessObject, 1);
			ListSelectedRowCollection selectedRowsInfo = ((IListView)this.View.ParentFormView.ParentFormView).SelectedRowsInfo;
			foreach (ListSelectedRow listSelectedRow in selectedRowsInfo)
			{
				string text = string.IsNullOrWhiteSpace(listSelectedRow.Number) ? listSelectedRow.BillNo : listSelectedRow.Number;
				this._batchEditItems[listSelectedRow.PrimaryKeyValue] = text.Replace("'", "''");
				if (!string.IsNullOrEmpty(this._entryName))
				{
					long key = Convert.ToInt64(listSelectedRow.PrimaryKeyValue);
					long item = Convert.ToInt64(listSelectedRow.EntryPrimaryKeyValue);
					if (this._pkEntryIdsDic.ContainsKey(key))
					{
						this._pkEntryIdsDic[key].Add(item);
					}
					else
					{
						this._pkEntryIdsDic.Add(key, new List<long>
						{
							item
						});
					}
				}
				foreach (FieldAuthResult fieldAuthResult in allowFieldList)
				{
					if (fieldAuthResult.Org.Id == listSelectedRow.MainOrgId)
					{
						hashSet = fieldAuthResult.AllFieldList;
						hashSet2 = fieldAuthResult.AllowFieldList;
						using (List<Field>.Enumerator enumerator3 = this._fieldList.GetEnumerator())
						{
							while (enumerator3.MoveNext())
							{
								Field field = enumerator3.Current;
								if (hashSet.Contains(field.Key) && !hashSet2.Contains(field.Key))
								{
									FormMetadata formMetadata = (FormMetadata)MetaDataServiceHelper.Load(this.View.Context, "ORG_Organizations", true);
									DynamicObject dynamicObject = BusinessDataServiceHelper.LoadSingle(this.View.Context, listSelectedRow.MainOrgId, formMetadata.BusinessInfo.GetDynamicObjectType(), null);
									this._batchEditItems.Remove(listSelectedRow.PrimaryKeyValue);
									StringBuilder stringBuilder = new StringBuilder();
									stringBuilder.AppendFormat(ResManager.LoadKDString("您在【{0}】组织下没有【{1}】的字段权限，请联系系统管理员！", "0151515153499000021270", 7, new object[0]), dynamicObject["Name"].ToString(), field.Name[base.Context.UserLocale.LCID]);
									string message = stringBuilder.ToString();
									this.AddFailDetail(listSelectedRow.PrimaryKeyValue, text, message);
									break;
								}
							}
							break;
						}
					}
				}
			}
			foreach (Field field2 in this._fieldList)
			{
				object value = this.Model.GetValue(field2.Key);
				this._fieldValues[field2.Key] = value;
			}
			this.ShowProgress();
			if (this._fieldList.Any((Field x) => x.Key == "FChildSupplyOrgId"))
			{
				IEnumerable<long> enumerable = (from x in selectedRowsInfo
				select Convert.ToInt64(x.EntryPrimaryKeyValue)).Distinct<long>();
				DynamicObjectCollection mtrlMster = BOMServiceHelper.GetMtrlMster(base.Context, enumerable);
				this.mtrMsterDic = (from x in mtrlMster
				group x by DataEntityExtend.GetDynamicValue<long>(x, "FMASTERID", 0L)).ToDictionary((IGrouping<long, DynamicObject> d) => d.Key);
			}
			MainWorker.QuequeTask(base.Context, delegate()
			{
				Stopwatch stopwatch = new Stopwatch();
				stopwatch.Start();
				this.UpdateData();
				stopwatch.Stop();
				Logger.Info("BOS", "Batch edit form name: " + this._metadata.BusinessInfo.GetForm().Name);
				Logger.Info("BOS", "Batch edit field name: " + this._metadata.BusinessInfo.GetField(this.fieldKey).Name);
				Logger.Info("BOS", "Batch edit total items: " + this._batchEditItems.Count);
				Logger.Info("BOS", "Batch edit elapsed milliseconds: " + stopwatch.ElapsedMilliseconds.ToString() + " ms");
			}, delegate(AsynResult asynResult)
			{
				this.View.ParentFormView.Session["ProcessRateValue"] = 100;
				this._batchID = this.SaveBatchEditLog();
				if (this._batchID == null)
				{
					this.View.ParentFormView.ShowMessage(ResManager.LoadKDString("批改日志保存失败。", "0151515153499000021271", 7, new object[0]), 0);
				}
				if (!this._progressClosed)
				{
					while (this.View.ParentFormView.GetView(this._progressPageID) == null)
					{
						Thread.Sleep(100);
					}
					this.View.ParentFormView.GetView(this._progressPageID).Close();
				}
				if (asynResult.Exception != null)
				{
					Logger.Error("BOS", "Batch edit exception", asynResult.Exception);
				}
			});
		}

		// Token: 0x0600075A RID: 1882 RVA: 0x00056C34 File Offset: 0x00054E34
		private void ShowProgress()
		{
			this._progressPageID = Guid.NewGuid().ToString();
			DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
			dynamicFormShowParameter.PageId = this._progressPageID;
			dynamicFormShowParameter.FormId = "BAS_BatchEditProgress";
			dynamicFormShowParameter.OpenStyle.ShowType = 6;
			dynamicFormShowParameter.HiddenCloseButton = false;
			this.View.ParentFormView.ShowForm(dynamicFormShowParameter, delegate(FormResult t)
			{
				this._progressClosed = true;
				if (!string.IsNullOrEmpty(this._batchID))
				{
					this.ShowResult(this._batchID);
				}
			});
			this.View.ParentFormView.Session["ProcessRateValue"] = 1;
		}

		// Token: 0x0600075B RID: 1883 RVA: 0x00056CC8 File Offset: 0x00054EC8
		private void ShowResult(string batchID)
		{
			BillShowParameter billShowParameter = new BillShowParameter();
			billShowParameter.PageId = Guid.NewGuid().ToString();
			billShowParameter.FormId = "BAS_BatchEditResult";
			billShowParameter.PKey = batchID;
			billShowParameter.Status = 1;
			this.View.ParentFormView.ShowForm(billShowParameter);
			IBatchEditService service = ServiceFactory.GetService<IBatchEditService>(base.Context);
			service.SetBactchEditLogHasRead(base.Context, batchID);
		}

		// Token: 0x0600075C RID: 1884 RVA: 0x00056D38 File Offset: 0x00054F38
		private BillOpenParameter CreateOpenParameter(object pkValue)
		{
			Form form = this._metadata.BusinessInfo.GetForm();
			BillOpenParameter billOpenParameter = new BillOpenParameter(form.Id, string.Empty);
			billOpenParameter.Context = base.Context;
			billOpenParameter.ServiceName = form.FormServiceName;
			billOpenParameter.PageId = Guid.NewGuid().ToString();
			billOpenParameter.FormMetaData = this._metadata;
			billOpenParameter.LayoutId = this._metadata.GetLayoutInfo().Id;
			billOpenParameter.Status = 2;
			billOpenParameter.PkValue = pkValue;
			billOpenParameter.CreateFrom = 0;
			billOpenParameter.ParentId = 0;
			billOpenParameter.GroupId = "";
			billOpenParameter.DefaultBillTypeId = null;
			billOpenParameter.DefaultBusinessFlowId = null;
			billOpenParameter.SetCustomParameter("ShowConfirmDialogWhenChangeOrg", false);
			List<AbstractDynamicFormPlugIn> list = form.CreateFormPlugIns();
			billOpenParameter.SetCustomParameter(FormConst.PlugIns, list);
			return billOpenParameter;
		}

		// Token: 0x0600075D RID: 1885 RVA: 0x00056E1C File Offset: 0x0005501C
		private BillView CreateView(string pkValue)
		{
			BillOpenParameter billOpenParameter = this.CreateOpenParameter(pkValue);
			IResourceServiceProvider formServiceProvider = this._metadata.BusinessInfo.GetForm().GetFormServiceProvider(true);
			ImportBillView importBillView = new ImportBillView();
			importBillView.Initialize(billOpenParameter, formServiceProvider);
			importBillView.LoadData();
			return importBillView;
		}

		// Token: 0x0600075E RID: 1886 RVA: 0x00056E98 File Offset: 0x00055098
		private void UpdateData()
		{
			List<string> checkedData = this.GetCheckedData();
			if (checkedData.Count == 0)
			{
				return;
			}
			this._fieldList.Reverse();
			List<DynamicObject> list = new List<DynamicObject>();
			List<Field> fieldList = this._metadata.BusinessInfo.GetFieldList();
			Field field = fieldList.Find((Field f) => f.ElementType == 27 && f.Entity is HeadEntity);
			Field field2 = fieldList.Find((Field f) => f.ElementType == 28 && f.Entity is HeadEntity);
			for (int i = 0; i < checkedData.Count; i++)
			{
				string text = checkedData[i];
				try
				{
					BillView billView = this.CreateView(text);
					if (this.SetFieldValue(billView, Convert.ToInt64(text)))
					{
						billView.Model.ClearNoDataRow();
						if (field != null && DynamicObjectUtils.Contains(billView.Model.DataObject, field.PropertyName + "_Id"))
						{
							billView.Model.DataObject[field.PropertyName + "_Id"] = base.Context.UserId;
						}
						if (field2 != null && DynamicObjectUtils.Contains(billView.Model.DataObject, field2.PropertyName))
						{
							billView.Model.DataObject[field2.PropertyName] = DateTime.Now;
						}
						list.Add(billView.Model.DataObject);
					}
					if ((i + 1) % 100 == 0 && list.Count > 0)
					{
						this.SaveObjects(list);
						list.Clear();
					}
					billView.CommitNetworkCtrl();
					billView.Close();
					if ((i + 1) % 10 == 0)
					{
						int num = (i + 1) * 100 / checkedData.Count;
						this.View.ParentFormView.Session["ProcessRateValue"] = ((num > 1) ? num : 1);
					}
				}
				catch (Exception ex)
				{
					if (this._batchEditItems.ContainsKey(text))
					{
						this.AddFailDetail(text, this._batchEditItems[text], ex.Message);
					}
				}
			}
			if (list.Count > 0)
			{
				this.SaveObjects(list);
			}
			MFGServiceHelper.ResetBomCache(base.Context);
		}

		// Token: 0x0600075F RID: 1887 RVA: 0x000570F0 File Offset: 0x000552F0
		private void SaveObjects(List<DynamicObject> saveObjs)
		{
			try
			{
				OperateOption operateOption = OperateOption.Create();
				OperateOptionUtils.SetIgnoreWarning(operateOption, true);
				OperateOptionExt.SetIgnoreInteractionFlag(operateOption, true);
				operateOption.SetVariableValue("IsBatchEdit", true);
				operateOption.SetVariableValue("BatchEditFields", this._fieldValues);
				DynamicObject[] array = saveObjs.ToArray();
				IOperationResult saveResult = BusinessDataServiceHelper.Save(base.Context, this._metadata.BusinessInfo, array, operateOption, "");
				this.AddOperationResult(saveResult, array);
			}
			catch (KDException ex)
			{
				Logger.Error("BOS", "Batch edit SaveObjects exception", ex);
			}
		}

		// Token: 0x06000760 RID: 1888 RVA: 0x000571C8 File Offset: 0x000553C8
		private bool SetFieldValue(BillView view, long pkID)
		{
			decimal num = 0m;
			long num2 = 0L;
			long num3 = 0L;
			long num4 = 0L;
			long num5 = 0L;
			long num6 = 0L;
			int num7 = -1;
			string text = "";
			string text2 = "0";
			int decimals = 0;
			string format = "分录“{0}”批改{1}失败，原因为“{2}”";
			IEnumerable<DynamicObject> substitueSchemeDataLst = null;
			Dictionary<string, UnitConvert> dicParentUnitConvert = new Dictionary<string, UnitConvert>();
			try
			{
				foreach (Field field in this._fieldList)
				{
					object obj = this._fieldValues[field.Key];
					if (string.IsNullOrEmpty(this._entryName) || (!(field.Key == "FSTOCKLOCID") && !(field.Key == "FBACKFLUSHTYPE") && !(field.Key == "FOWNERID")))
					{
						if (field is CombinedField)
						{
							string text3 = obj.ToString();
							object value = this.Model.GetValue(text3);
							view.Model.SetCombinedValue(field.Key, text3, value, 0);
						}
						else if (field is BaseDataField && (string.IsNullOrEmpty(this._entryName) || (!(field.Key == "FSTOCKID") && !(field.Key == "FPROCESSID") && !(field.Key == "FSUPPLYORG") && !(field.Key == "FChildSupplyOrgId") && !(field.Key == "FBOMID"))))
						{
							DynamicObject dynamicObject = obj as DynamicObject;
							BaseDataField baseDataField = field as BaseDataField;
							FormMetadata formMetadata = MetaDataServiceHelper.Load(base.Context, baseDataField.LookUpObject.FormId, true) as FormMetadata;
							string propertyName = formMetadata.BusinessInfo.GetField(formMetadata.BusinessInfo.GetForm().NumberFieldKey).PropertyName;
							string text4 = (dynamicObject == null) ? "" : (dynamicObject[propertyName] as string);
							view.Model.SetItemValueByNumber(field.Key, text4, 0);
							DynamicObject dynamicObject2 = view.Model.GetValue(field.Key) as DynamicObject;
							string a = (dynamicObject2 == null) ? "" : (dynamicObject2[propertyName] as string);
							if (a != text4)
							{
								view.Model.SetItemValueByID(field.Key, OrmUtils.GetPrimaryKeyValue(dynamicObject, true), 0);
							}
							dynamicObject2 = (view.Model.GetValue(field.Key) as DynamicObject);
							a = ((dynamicObject2 == null) ? "" : (dynamicObject2[propertyName] as string));
							if (a != text4)
							{
								string text5 = view.Model.GetPKValue().ToString();
								if (this._batchEditItems.ContainsKey(text5))
								{
									bool flag = formMetadata.BusinessInfo.GetForm().IsCanIssue == 1;
									string text6 = string.Format(ResManager.LoadKDString("“{0}”没有分发到该组织", "0151515153499030041405", 7, new object[0]), baseDataField.Name);
									string text7 = ResManager.LoadKDString("使用组织不匹配", "0151515153499000021273", 7, new object[0]);
									this.AddFailDetail(text5, this._batchEditItems[text5], flag ? text6 : text7);
								}
								return false;
							}
						}
						else if (field is RelatedFlexGroupField)
						{
							Field field2 = this._metadata.BusinessInfo.GetField(field.Key);
							DynamicObject rowObj = null;
							if (field2.Entity is HeadEntity)
							{
								rowObj = (view.Model.DataObject[field2.PropertyName] as DynamicObject);
							}
							else if (field2.Entity is SubHeadEntity)
							{
								rowObj = (view.Model.DataObject[field2.Entity.EntryName] as DynamicObjectCollection)[0];
							}
							this.SetFlexValue(field2 as RelatedFlexGroupField, rowObj, obj, 0, view.Model);
						}
						else
						{
							if (!string.IsNullOrEmpty(this._entryName))
							{
								DynamicObjectCollection entityDataObject = view.Model.GetEntityDataObject(view.BusinessInfo.GetEntity(this._entryName));
								if (field.PropertyName == "DENOMINATOR")
								{
									decimals = DataEntityExtend.GetDynamicValue<int>(view.Model.DataObject["FUNITID"] as DynamicObject, "Precision", 0);
									num = this.GetHeadUnitConvertRates(view, Convert.ToDecimal(obj));
								}
								else if (field.PropertyName == "NUMERATOR")
								{
									dicParentUnitConvert = this.GetDetailUnitConvertRates(entityDataObject);
								}
								else if (field.PropertyName == "MATERIALTYPE")
								{
									substitueSchemeDataLst = this.ReGetSubstitute(entityDataObject);
								}
								else if (field.PropertyName == "STOCKID")
								{
									DynamicObject dynamicObject3 = obj as DynamicObject;
									if (dynamicObject3 != null)
									{
										num2 = Convert.ToInt64((dynamicObject3["UseOrgId"] as DynamicObject)["Id"]);
										text = Convert.ToString(dynamicObject3["Number"]);
									}
									dynamicObject3 = (this._fieldValues["FSTOCKLOCID"] as DynamicObject);
									if (dynamicObject3 != null)
									{
										text2 = Convert.ToString(dynamicObject3["Id"]);
									}
								}
								else if (field.PropertyName == "PROCESSID")
								{
									DynamicObject dynamicObject4 = view.Model.DataObject["USEORGID"] as DynamicObject;
									if (dynamicObject4 != null && DynamicObjectUtils.Contains(dynamicObject4, "Id"))
									{
										num6 = Convert.ToInt64(dynamicObject4["Id"]);
									}
									dynamicObject4 = (obj as DynamicObject);
									if (dynamicObject4 != null && DynamicObjectUtils.Contains(dynamicObject4, "Id"))
									{
										num2 = Convert.ToInt64(dynamicObject4["Id"]);
										text = Convert.ToString(dynamicObject4["Number"]);
										DynamicObject[] array = BusinessDataServiceHelper.Load(base.Context, new object[]
										{
											num2
										}, (MetaDataServiceHelper.Load(base.Context, "ENG_Process", true) as FormMetadata).BusinessInfo.GetDynamicObjectType());
										num2 = 0L;
										if (array != null)
										{
											num2 = Convert.ToInt64(array[0]["UseOrgId_Id"].ToString());
										}
									}
								}
								else if (field.PropertyName == "BOMID")
								{
									DynamicObject dynamicObject5 = this._fieldValues["FBOMID"] as DynamicObject;
									if (dynamicObject5 != null && DynamicObjectUtils.Contains(dynamicObject5, "Id"))
									{
										DynamicObject[] array2 = BusinessDataServiceHelper.Load(base.Context, new object[]
										{
											dynamicObject5["Id"]
										}, (MetaDataServiceHelper.Load(base.Context, "ENG_BOM", true) as FormMetadata).BusinessInfo.GetDynamicObjectType());
										if (array2 != null)
										{
											num3 = Convert.ToInt64(array2[0]["UseOrgId_Id"].ToString());
											DynamicObject dynamicObject6 = array2[0]["MATERIALID"] as DynamicObject;
											if (dynamicObject6 != null && DynamicObjectUtils.Contains(dynamicObject6, "msterID"))
											{
												num4 = Convert.ToInt64(dynamicObject6["msterID"]);
											}
										}
									}
								}
								else if (field.PropertyName == "OWNERTYPEID")
								{
									string text8 = Convert.ToString(obj);
									if (text8 == "BD_Supplier" || text8 == "BD_Customer")
									{
										DynamicObject dynamicObject7 = view.Model.DataObject["USEORGID"] as DynamicObject;
										if (dynamicObject7 != null && DynamicObjectUtils.Contains(dynamicObject7, "Id"))
										{
											num6 = Convert.ToInt64(dynamicObject7["Id"]);
										}
										DynamicObject dynamicObject8 = this._fieldValues["FOWNERID"] as DynamicObject;
										if (dynamicObject8 != null && DynamicObjectUtils.Contains(dynamicObject8, "Id"))
										{
											DynamicObject[] array3 = BusinessDataServiceHelper.Load(base.Context, new object[]
											{
												dynamicObject8["Id"]
											}, (MetaDataServiceHelper.Load(base.Context, text8, true) as FormMetadata).BusinessInfo.GetDynamicObjectType());
											if (array3 != null)
											{
												num5 = Convert.ToInt64(array3[0]["UseOrgId_Id"].ToString());
											}
										}
									}
								}
								using (IEnumerator<DynamicObject> enumerator2 = entityDataObject.GetEnumerator())
								{
									while (enumerator2.MoveNext())
									{
										DynamicObject dynamicObject9 = enumerator2.Current;
										num7++;
										if (this._pkEntryIdsDic.ContainsKey(pkID))
										{
											List<long> list = this._pkEntryIdsDic[pkID];
											DynamicObject dynamicObject10 = dynamicObject9;
											if (DynamicObjectUtils.Contains(dynamicObject10, field.PropertyName) && list.Contains(Convert.ToInt64(dynamicObject10["Id"])))
											{
												DynamicObject dynamicObject3 = dynamicObject10["MATERIALIDCHILD"] as DynamicObject;
												string text9 = Convert.ToString(dynamicObject3["Number"]).Replace("'", "''");
												if (field.PropertyName == "SupplyType")
												{
													int num8 = (int)Convert.ToInt16((dynamicObject3["MaterialBase"] as DynamicObjectCollection)[0]["ErpClsID"]);
													if (num8 != 2 && num8 != 3)
													{
														this.AddFailDetail(pkID.ToString(), this._batchEditItems[pkID.ToString()], string.Format(format, text9, this._fieldName, ResManager.LoadKDString("非自制件和委外件，不可维护供应类型。", "0151515153499000018777", 7, new object[0])));
														continue;
													}
												}
												else if (field.PropertyName == "EFFECTDATE")
												{
													if (Convert.ToDateTime(obj) > Convert.ToDateTime(dynamicObject10["EXPIREDATE"]))
													{
														this.AddFailDetail(pkID.ToString(), this._batchEditItems[pkID.ToString()], string.Format(format, text9, this._fieldName, ResManager.LoadKDString("生效日期不能大于失效日期。", "0151515153499000018778", 7, new object[0])));
														continue;
													}
												}
												else if (field.PropertyName == "EXPIREDATE")
												{
													if (Convert.ToDateTime(obj) < Convert.ToDateTime(dynamicObject10["EFFECTDATE"]))
													{
														this.AddFailDetail(pkID.ToString(), this._batchEditItems[pkID.ToString()], string.Format(format, text9, this._fieldName, ResManager.LoadKDString("失效日期不能小于生效日期。", "0151515153499030038968", 7, new object[0])));
														continue;
													}
												}
												else if (field.PropertyName == "STOCKID")
												{
													DynamicObject dynamicObject11 = dynamicObject10["SUPPLYORG"] as DynamicObject;
													if (obj != null)
													{
														if (dynamicObject11 == null)
														{
															this.AddFailDetail(pkID.ToString(), this._batchEditItems[pkID.ToString()], string.Format(format, text9, this._fieldName, "发料组织为空，不允许录入默认发料仓库"));
															continue;
														}
														if (Convert.ToInt64(dynamicObject11["Id"]) != num2)
														{
															this.AddFailDetail(pkID.ToString(), this._batchEditItems[pkID.ToString()], string.Format(format, text9, this._fieldName, ResManager.LoadKDString("发料组织与仓库使用组织不一致。", "0151515153499030038969", 7, new object[0])));
															continue;
														}
													}
												}
												else if (field.PropertyName == "MATERIALTYPE")
												{
													if (this.bExistsSubstitute(dynamicObject10, substitueSchemeDataLst) || Convert.ToString(dynamicObject10["MATERIALTYPE"]) == "3")
													{
														this.AddFailDetail(pkID.ToString(), this._batchEditItems[pkID.ToString()], string.Format(format, text9, this._fieldName, ResManager.LoadKDString("存在替代关系，不可维护子项类型。", "0151515153499030038970", 7, new object[0])));
														continue;
													}
												}
												else if (field.PropertyName == "DENOMINATOR")
												{
													if (Convert.ToInt16((dynamicObject3["MaterialBase"] as DynamicObjectCollection)[0]["ErpClsID"]) != 1 && Convert.ToDecimal(obj) > 1000m)
													{
														this.AddFailDetail(pkID.ToString(), this._batchEditItems[pkID.ToString()], string.Format(format, text9, this._fieldName, ResManager.LoadKDString("非外购件分母不允许大于1000。", "0151515153499030038971", 7, new object[0])));
														continue;
													}
												}
												else if (field.PropertyName == "ISSkip")
												{
													int num9 = (int)Convert.ToInt16((dynamicObject3["MaterialBase"] as DynamicObjectCollection)[0]["ErpClsID"]);
													int num10 = (int)Convert.ToInt16((dynamicObject3["MaterialProduce"] as DynamicObjectCollection)[0]["IsMainPrd"]);
													if (num9 == 5)
													{
														this.AddFailDetail(pkID.ToString(), this._batchEditItems[pkID.ToString()], string.Format(format, text9, this._fieldName, ResManager.LoadKDString("为虚拟件，不支持修改“跳层”字段。", "0151515153499030039956", 7, new object[0])));
														continue;
													}
													if (num10 == 0)
													{
														this.AddFailDetail(pkID.ToString(), this._batchEditItems[pkID.ToString()], string.Format(format, text9, this._fieldName, ResManager.LoadKDString("不可为主产品，不支持修改“跳层”字段。", "0151515153499000019779", 7, new object[0])));
														continue;
													}
												}
												else if (field.PropertyName == "PROCESSID" && num6 != num2)
												{
													this.AddFailDetail(pkID.ToString(), this._batchEditItems[pkID.ToString()], string.Format(format, text9, this._fieldName, ResManager.LoadKDString("使用组织与作业使用组织不一致。", "0151515153499000019867", 7, new object[0])));
													continue;
												}
												if (field.PropertyName == "DENOMINATOR")
												{
													dynamicObject10[field.PropertyName] = Math.Round(Convert.ToDecimal(obj), decimals);
													dynamicObject10["BaseDenominator"] = num;
												}
												else if (field.PropertyName == "NUMERATOR")
												{
													decimals = DataEntityExtend.GetDynamicValue<int>(dynamicObject10["CHILDUNITID"] as DynamicObject, "Precision", 0);
													num = this.GetUnitConvertRates(dynamicObject10, dicParentUnitConvert, Convert.ToDecimal(obj));
													dynamicObject10[field.PropertyName] = Math.Round(Convert.ToDecimal(obj), decimals);
													dynamicObject10["BaseNumerator"] = num;
												}
												else if (field.PropertyName == "STOCKID")
												{
													view.Model.SetItemValueByNumber(field.Key, text, num7);
													dynamicObject10["STOCKLOCID_ID"] = text2;
												}
												else if (field.PropertyName == "ISSUETYPE")
												{
													dynamicObject10[field.PropertyName] = obj;
													dynamicObject10["BACKFLUSHTYPE"] = this._fieldValues["FBACKFLUSHTYPE"];
												}
												else if (field.PropertyName == "FIXSCRAPQTY")
												{
													decimals = DataEntityExtend.GetDynamicValue<int>(dynamicObject10["CHILDUNITID"] as DynamicObject, "Precision", 0);
													dynamicObject10[field.PropertyName] = Math.Round(Convert.ToDecimal(obj), decimals);
												}
												else if (field.PropertyName == "PROCESSID")
												{
													view.Model.SetItemValueByNumber(field.Key, text, num7);
												}
												else if (field.PropertyName == "DOSAGETYPE")
												{
													string a2 = Convert.ToString(obj);
													string a3 = Convert.ToString(dynamicObject10["DOSAGETYPE"]);
													if ((a3 == "1" || a3 == "2") && a2 == "3")
													{
														this.AddFailDetail(pkID.ToString(), this._batchEditItems[pkID.ToString()], string.Format(format, text9, this._fieldName, "原本用量类型是固定、变动不允许批改为阶梯"));
														continue;
													}
													if (a3 == "3" && (a2 == "1" || a2 == "2"))
													{
														DynamicObjectCollection dynamicObjectCollection = dynamicObject10["BOMCHILDLOTBASEDQTY"] as DynamicObjectCollection;
														dynamicObjectCollection.Clear();
													}
													dynamicObject10[field.PropertyName] = obj;
												}
												else if (field.PropertyName == "SUPPLYORG")
												{
													DynamicObject dynamicObject12 = obj as DynamicObject;
													if (dynamicObject12 != null && DynamicObjectUtils.Contains(dynamicObject12, "Id"))
													{
														long num11 = Convert.ToInt64(dynamicObject12["Id"]);
														long num12 = Convert.ToInt64(dynamicObject10["SUPPLYORG_Id"]);
														if (num11 != num12 || num11 == 0L)
														{
															dynamicObject10["STOCKLOCID_ID"] = 0;
															dynamicObject10["STOCKID_ID"] = 0;
															dynamicObject10["SUPPLYORG_Id"] = num11;
														}
													}
													else
													{
														dynamicObject10["STOCKLOCID_ID"] = 0;
														dynamicObject10["STOCKID_ID"] = 0;
														dynamicObject10["SUPPLYORG_Id"] = 0;
													}
												}
												else if (field.PropertyName == "POSITIONNO")
												{
													string text10 = Convert.ToString(obj);
													if (text10.Length > 2000)
													{
														this.AddFailDetail(pkID.ToString(), this._batchEditItems[pkID.ToString()], string.Format(format, text9, this._fieldName, "位置号长度不得超过2000"));
														continue;
													}
													dynamicObject10[field.PropertyName] = obj;
												}
												else if (field.PropertyName == "ChildSupplyOrgId")
												{
													DynamicObject dynamicObject13 = obj as DynamicObject;
													if (dynamicObject13 != null && DynamicObjectUtils.Contains(dynamicObject13, "Id"))
													{
														long newChildSupplyOrgId = Convert.ToInt64(dynamicObject13["Id"]);
														long num13 = Convert.ToInt64(view.Model.DataObject["UseOrgId_Id"]);
														if (newChildSupplyOrgId != num13)
														{
															List<long> list2 = new List<long>
															{
																102L,
																112L,
																101L,
																104L,
																103L,
																109L
															};
															new List<long>();
															List<long> orgByBizRelationOrgs = MFGServiceHelper.GetOrgByBizRelationOrgs(base.Context, num13, list2);
															if (!ListUtils.IsEmpty<long>(orgByBizRelationOrgs))
															{
																if (!orgByBizRelationOrgs.Contains(newChildSupplyOrgId))
																{
																	this.AddFailDetail(pkID.ToString(), this._batchEditItems[pkID.ToString()], string.Format(format, text9, this._fieldName, ResManager.LoadKDString("使用组织与供应组织间不存在以下任一组织业务关系：跨组织领料、委托保管、委托销售、委托采购、库存调拨、委托生产", "0151515153499000023934", 7, new object[0])));
																	continue;
																}
																bool flag2 = false;
																IGrouping<long, DynamicObject> source;
																if (this.mtrMsterDic.TryGetValue(DataEntityExtend.GetDynamicValue<long>(DataEntityExtend.GetDynamicValue<DynamicObject>(dynamicObject10, "MATERIALIDCHILD", null), "msterID", 0L), out source))
																{
																	if (source.Any((DynamicObject x) => DataEntityExtend.GetDynamicValue<long>(x, "FUSEORGID", 0L) == newChildSupplyOrgId))
																	{
																		flag2 = true;
																	}
																}
																if (!flag2)
																{
																	this.AddFailDetail(pkID.ToString(), this._batchEditItems[pkID.ToString()], string.Format(format, text9, this._fieldName, ResManager.LoadKDString("物料未分配", "0151515153499000023935", 7, new object[0])));
																	continue;
																}
															}
														}
														long num14 = Convert.ToInt64(dynamicObject10["ChildSupplyOrgId_Id"]);
														if (newChildSupplyOrgId != num14 || newChildSupplyOrgId == 0L)
														{
															dynamicObject10["BOMID_ID"] = 0;
														}
														dynamicObject10["ChildSupplyOrgId_Id"] = newChildSupplyOrgId;
													}
													else
													{
														dynamicObject10["BOMID_ID"] = 0;
														dynamicObject10["ChildSupplyOrgId_Id"] = 0;
													}
												}
												else if (field.PropertyName == "BOMID")
												{
													DynamicObject dynamicObject14 = obj as DynamicObject;
													bool flag3 = dynamicObject14 != null && DynamicObjectUtils.Contains(dynamicObject14, "Id");
													if (!flag3)
													{
														dynamicObject10["BOMID_ID"] = 0;
													}
													long num15 = Convert.ToInt64(dynamicObject10["ChildSupplyOrgId_Id"]);
													DynamicObject dynamicObject15 = dynamicObject10["MATERIALIDCHILD"] as DynamicObject;
													long num16 = 0L;
													if (dynamicObject15 != null && DynamicObjectUtils.Contains(dynamicObject15, "msterID"))
													{
														num16 = Convert.ToInt64(dynamicObject15["msterID"]);
													}
													if (flag3 && num3 != num15)
													{
														this.AddFailDetail(pkID.ToString(), this._batchEditItems[pkID.ToString()], string.Format(format, text9, this._fieldName, ResManager.LoadKDString("子项BOM版本的使用组织要等于供应组织", "0151515153499000023936", 7, new object[0])));
														continue;
													}
													if (flag3 && num4 != num16)
													{
														this.AddFailDetail(pkID.ToString(), this._batchEditItems[pkID.ToString()], string.Format(format, text9, this._fieldName, ResManager.LoadKDString("子项BOM版本一定要是选择的子项物料对应的BOM版本", "0151515153499000023937", 7, new object[0])));
														continue;
													}
													if (dynamicObject14 != null && DynamicObjectUtils.Contains(dynamicObject14, "Id"))
													{
														dynamicObject10["BOMID_ID"] = Convert.ToInt64(dynamicObject14["Id"]);
													}
												}
												else if (field.PropertyName == "OWNERTYPEID")
												{
													DynamicObject dynamicObject16 = this._fieldValues["FOWNERID"] as DynamicObject;
													if (dynamicObject16 != null)
													{
														if ((Convert.ToString(obj) == "BD_Supplier" || Convert.ToString(obj) == "BD_Customer") && num5 != num6)
														{
															this.AddFailDetail(pkID.ToString(), this._batchEditItems[pkID.ToString()], string.Format(format, text9, this._fieldName, "货主的使用组织不等于物料清单的使用组织"));
															continue;
														}
														dynamicObject10["OWNERID_ID"] = Convert.ToString(dynamicObject16["Id"]);
													}
													else
													{
														dynamicObject10["OWNERID_ID"] = 0;
													}
													dynamicObject10[field.PropertyName] = obj;
												}
												else if (field.PropertyName == "ISMinIssueQty")
												{
													string a4 = Convert.ToString(dynamicObject10["OverControlMode"]);
													if (a4 == "3")
													{
														this.AddFailDetail(pkID.ToString(), this._batchEditItems[pkID.ToString()], string.Format(format, text9, this._fieldName, "子项超发控制为不允许超发时，不允许将领料考虑最小发料批量批改为勾选"));
														continue;
													}
													dynamicObject10[field.PropertyName] = obj;
												}
												else if (field.PropertyName == "IsCanChoose" || field.PropertyName == "IsCanEdit" || field.PropertyName == "IsCanReplace")
												{
													string dynamicValue = DataEntityExtend.GetDynamicValue<string>(view.Model.DataObject, "BOMCATEGORY", null);
													if (dynamicValue != "2")
													{
														this.AddFailDetail(pkID.ToString(), this._batchEditItems[pkID.ToString()], string.Format(format, text9, this._fieldName, "BOM分类不为配置BOM，不允许批改" + this._fieldName));
														continue;
													}
													int curReplaceGroup = DataEntityExtend.GetDynamicValue<int>(dynamicObject10, "ReplaceGroup", 0);
													DynamicObjectCollection dynamicValue2 = DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(view.Model.DataObject, "TreeEntity", null);
													if (!ListUtils.IsEmpty<DynamicObject>(dynamicValue2))
													{
														if (dynamicValue2.Count((DynamicObject x) => DataEntityExtend.GetDynamicValue<int>(x, "ReplaceGroup", 0) == curReplaceGroup) > 1)
														{
															if (Convert.ToBoolean(obj) && (field.PropertyName == "IsCanEdit" || field.PropertyName == "IsCanReplace"))
															{
																this.AddFailDetail(pkID.ToString(), this._batchEditItems[pkID.ToString()], string.Format(format, text9, this._fieldName, "配置BOM子项存在替代关系时，不允许批改" + this._fieldName));
																continue;
															}
															if (Convert.ToBoolean(obj) && field.PropertyName == "IsCanChoose" && (!DataEntityExtend.GetDynamicValue<bool>(dynamicObject10, "IskeyItem", false) || DataEntityExtend.GetDynamicValue<string>(dynamicObject10, "MATERIALTYPE", null) != "1"))
															{
																this.AddFailDetail(pkID.ToString(), this._batchEditItems[pkID.ToString()], string.Format(format, text9, this._fieldName, "相同项次有且只有标准件的替代主料可以勾选" + this._fieldName));
																continue;
															}
														}
													}
													dynamicObject10[field.PropertyName] = obj;
												}
												else if (field.PropertyName == "IsMrpRun")
												{
													bool flag4 = false;
													DynamicObjectCollection dynamicObjectCollection2 = dynamicObject3["MaterialPlan"] as DynamicObjectCollection;
													if (!ListUtils.IsEmpty<DynamicObject>(dynamicObjectCollection2))
													{
														DynamicObject dynamicObject17 = dynamicObjectCollection2.FirstOrDefault<DynamicObject>();
														if (DataEntityExtend.GetDynamicValue<string>(dynamicObject17, "PlanningStrategy", null) == "0" || DataEntityExtend.GetDynamicValue<string>(dynamicObject17, "PlanningStrategy", null) == "1")
														{
															flag4 = true;
														}
													}
													if (!flag4 && Convert.ToBoolean(obj))
													{
														this.AddFailDetail(pkID.ToString(), this._batchEditItems[pkID.ToString()], string.Format(format, text9, this._fieldName, ResManager.LoadKDString("计划策略不为MRP或者MPS，不允许勾选MRP运算", "0151515153499000023938", 7, new object[0])));
														continue;
													}
													dynamicObject10[field.PropertyName] = obj;
												}
												else
												{
													dynamicObject10[field.PropertyName] = obj;
												}
												if (DynamicObjectUtils.Contains(dynamicObject10, "ChangeTime"))
												{
													dynamicObject10["ChangeTime"] = DateTime.Now;
												}
												this.AddSuccessDetail(pkID.ToString(), this._batchEditItems[pkID.ToString()], string.Format(ResManager.LoadKDString("{0}为“{1}”的{2}分录“{3}”批改{4}成功。", "0151515153499030038972", 7, new object[0]), new object[]
												{
													this._billName,
													this._batchEditItems[pkID.ToString()],
													this._formName,
													text9,
													this._fieldName
												}));
											}
										}
									}
									goto IL_1AFA;
								}
							}
							view.Model.SetValue(field.Key, obj);
						}
						IL_1AFA:
						view.InvokeFieldUpdateService(field.Key, 0);
					}
				}
			}
			catch (Exception ex)
			{
				string text11 = view.Model.GetPKValue().ToString();
				if (this._batchEditItems.ContainsKey(text11))
				{
					this.AddFailDetail(text11, this._batchEditItems[text11], ex.Message);
				}
				return false;
			}
			return true;
		}

		// Token: 0x06000761 RID: 1889 RVA: 0x00058D90 File Offset: 0x00056F90
		private decimal GetHeadUnitConvertRates(BillView view, decimal addDenominator)
		{
			Dictionary<string, UnitConvert> dictionary = new Dictionary<string, UnitConvert>();
			string format = "{0}_{1}_{2}";
			List<string> list = new List<string>();
			DynamicObject dynamicObject = view.Model.GetValue("FMATERIALID") as DynamicObject;
			if (dynamicObject == null)
			{
				return addDenominator;
			}
			Convert.ToInt64(dynamicObject["Id"]);
			long num = Convert.ToInt64(dynamicObject["msterID"]);
			dynamicObject = (view.Model.GetValue("FUNITID") as DynamicObject);
			if (dynamicObject == null)
			{
				return addDenominator;
			}
			long num2 = Convert.ToInt64(dynamicObject["Id"]);
			dynamicObject = (view.Model.GetValue("FBaseUnitId") as DynamicObject);
			if (dynamicObject == null)
			{
				return addDenominator;
			}
			long num3 = Convert.ToInt64(dynamicObject["Id"]);
			string text = string.Format(format, num, num2, num3);
			if (!list.Contains(text))
			{
				list.Add(text);
			}
			dictionary = MFGServiceHelper.GetUnitConvertRates(this.View.Context, list);
			UnitConvert unitConvert = dictionary[text];
			return unitConvert.ConvertQty(addDenominator, "");
		}

		// Token: 0x06000762 RID: 1890 RVA: 0x00058EA0 File Offset: 0x000570A0
		private Dictionary<string, UnitConvert> GetDetailUnitConvertRates(DynamicObjectCollection BOMEntryObjs)
		{
			List<string> list = new List<string>();
			foreach (DynamicObject dynamicObject in BOMEntryObjs)
			{
				DynamicObject dynamicObject2 = dynamicObject;
				string format = "{0}_{1}_{2}";
				DynamicObject dynamicObject3 = dynamicObject2["MATERIALIDCHILD"] as DynamicObject;
				if (dynamicObject3 != null)
				{
					Convert.ToInt64(dynamicObject3["Id"]);
					long num = Convert.ToInt64(dynamicObject3["msterID"]);
					dynamicObject3 = (dynamicObject2["CHILDUNITID"] as DynamicObject);
					if (dynamicObject3 != null)
					{
						long num2 = Convert.ToInt64(dynamicObject3["Id"]);
						dynamicObject3 = (dynamicObject2["ChildBaseUnitID"] as DynamicObject);
						if (dynamicObject3 != null)
						{
							long num3 = Convert.ToInt64(dynamicObject3["Id"]);
							string item = string.Format(format, num, num2, num3);
							if (!list.Contains(item))
							{
								list.Add(item);
							}
						}
					}
				}
			}
			return MFGServiceHelper.GetUnitConvertRates(this.View.Context, list);
		}

		// Token: 0x06000763 RID: 1891 RVA: 0x00058FCC File Offset: 0x000571CC
		private decimal GetUnitConvertRates(DynamicObject rowData, Dictionary<string, UnitConvert> dicParentUnitConvert, decimal addNumertor)
		{
			string format = "{0}_{1}_{2}";
			DynamicObject dynamicObject = rowData["MATERIALIDCHILD"] as DynamicObject;
			if (dynamicObject == null)
			{
				return addNumertor;
			}
			Convert.ToInt64(dynamicObject["Id"]);
			long num = Convert.ToInt64(dynamicObject["msterID"]);
			dynamicObject = (rowData["CHILDUNITID"] as DynamicObject);
			if (dynamicObject == null)
			{
				return addNumertor;
			}
			long num2 = Convert.ToInt64(dynamicObject["Id"]);
			dynamicObject = (rowData["ChildBaseUnitID"] as DynamicObject);
			if (dynamicObject == null)
			{
				return addNumertor;
			}
			long num3 = Convert.ToInt64(dynamicObject["Id"]);
			string key = string.Format(format, num, num2, num3);
			UnitConvert unitConvert = dicParentUnitConvert[key];
			return unitConvert.ConvertQty(addNumertor, "");
		}

		// Token: 0x06000764 RID: 1892 RVA: 0x00059098 File Offset: 0x00057298
		private void SetFlexValue(RelatedFlexGroupField flexField, DynamicObject rowObj, object value, int rowIndex, IBillModel model)
		{
			if (value == null)
			{
				return;
			}
			if (value is long || value is int)
			{
				rowObj[flexField.PropertyName] = BusinessDataServiceHelper.LoadSingle(this.Model.Context, value, flexField.RefFormDynamicObjectType, null);
				rowObj[flexField.PropertyName + "_Id"] = value;
				return;
			}
			BusinessInfo relateFlexBusinessInfo = flexField.RelateFlexBusinessInfo;
			string relatedBaseDataFlexGroupField = flexField.RelatedBaseDataFlexGroupField;
			if (string.IsNullOrWhiteSpace(relatedBaseDataFlexGroupField))
			{
				return;
			}
			string entityKey = flexField.EntityKey;
			BOSDynamicRow bosdynamicRow = new BOSDynamicRow(rowObj, entityKey, this._metadata.BusinessInfo);
			object arg;
			if (!bosdynamicRow.TryGetMember(relatedBaseDataFlexGroupField, ref arg))
			{
				return;
			}
			if (BOMChangeFieldContent.<SetFlexValue>o__SiteContainer1c.<>p__Site1d == null)
			{
				BOMChangeFieldContent.<SetFlexValue>o__SiteContainer1c.<>p__Site1d = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "ActiveObject", typeof(BOMChangeFieldContent), new CSharpArgumentInfo[]
				{
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
				}));
			}
			DynamicObject dynamicObject = BOMChangeFieldContent.<SetFlexValue>o__SiteContainer1c.<>p__Site1d.Target(BOMChangeFieldContent.<SetFlexValue>o__SiteContainer1c.<>p__Site1d, arg) as DynamicObject;
			if (dynamicObject == null)
			{
				return;
			}
			this.SetFlexItemValue(flexField, rowObj, value, rowIndex, model);
			this.SetFlexObjectId(flexField, rowObj, dynamicObject);
		}

		// Token: 0x06000765 RID: 1893 RVA: 0x000591A8 File Offset: 0x000573A8
		private void SetFlexItemValue(RelatedFlexGroupField flexField, DynamicObject rowObj, object value, int rowIndex, IBillModel model)
		{
			DynamicObject dynamicObject = value as DynamicObject;
			if (dynamicObject == null)
			{
				return;
			}
			foreach (Field field in flexField.RelateFlexBusinessInfo.GetFieldList())
			{
				if (!StringUtils.EqualsIgnoreCase(field.PropertyName, "opcode"))
				{
					object obj = dynamicObject[field.PropertyName];
					if (!ObjectUtils.IsNullOrEmpty(obj))
					{
						Field field2 = flexField.RelateFlexBusinessInfo.GetField(field.Key);
						if (field2 != null)
						{
							if (field2 is BaseDataField)
							{
								DynamicObject dynamicObject2 = obj as DynamicObject;
								if (dynamicObject2 != null)
								{
									object obj2 = dynamicObject2["Number"];
									if (!ObjectUtils.IsNullOrEmpty(obj2))
									{
										model.SetItemValueByNumber("$$" + flexField.Key + "__" + field.Key, obj2.ToString(), rowIndex);
									}
								}
							}
							else
							{
								model.SetValue("$$" + flexField.Key + "__" + field.Key, obj, rowIndex);
							}
						}
					}
				}
			}
		}

		// Token: 0x06000766 RID: 1894 RVA: 0x000592CC File Offset: 0x000574CC
		private void SetFlexObjectId(RelatedFlexGroupField flexField, DynamicObject rowObj, DynamicObject bdObj)
		{
			DynamicObject dynamicObject = flexField.DynamicProperty.GetValue(rowObj) as DynamicObject;
			bool flag = this.CheckFlexFieldIsNull(flexField, bdObj, dynamicObject);
			if (flag)
			{
				if (dynamicObject != null)
				{
					dynamicObject["Id"] = null;
				}
				flexField.DynamicProperty.SetValue(rowObj, null);
				flexField.RefIDDynamicProperty.SetValue(rowObj, null);
				return;
			}
			long flexDataId = FlexServiceHelper.GetFlexDataId(base.Context, dynamicObject, flexField.RelatedBDFlexItemLinkField);
			if (flexDataId < 0L)
			{
				return;
			}
			if (flexDataId > 0L)
			{
				dynamicObject["Id"] = flexDataId;
				flexField.RefIDDynamicProperty.SetValue(rowObj, flexDataId);
				return;
			}
			DynamicObject dynamicObject2 = OrmUtils.Clone(dynamicObject, false, true) as DynamicObject;
			DynamicObject dynamicObject3 = BusinessDataServiceHelper.Save(base.Context, dynamicObject2);
			dynamicObject[0] = dynamicObject3[0];
			flexField.RefIDDynamicProperty.SetValue(rowObj, dynamicObject3[0]);
		}

		// Token: 0x06000767 RID: 1895 RVA: 0x000593A8 File Offset: 0x000575A8
		private bool CheckFlexFieldIsNull(RelatedFlexGroupField flexField, DynamicObject bdObj, DynamicObject flexObj)
		{
			bool flag = true;
			int num;
			int num2;
			List<DynamicObject> inUseFlexItemInfo = this.GetInUseFlexItemInfo(flexField, bdObj, out num, out num2);
			if (inUseFlexItemInfo == null || inUseFlexItemInfo.Count == 0)
			{
				return flag;
			}
			if (flexObj == null)
			{
				return flag;
			}
			foreach (DynamicObject dynamicObject in inUseFlexItemInfo)
			{
				string text = dynamicObject["FFlexNumber"].ToString().Substring(1);
				bool flag2 = false;
				if (!string.IsNullOrWhiteSpace(text))
				{
					if (!flexObj.DynamicObjectType.Properties.ContainsKey(text))
					{
						flag2 = true;
						flag = (flag && flag2);
					}
					else
					{
						if (Convert.ToInt32(dynamicObject["FValueType"]) == 0 || Convert.ToInt32(dynamicObject["FValueType"]) == 3)
						{
							string text2 = text + "_Id";
							if (flexObj[text2] == null || flexObj[text2].ToString() == "" || Convert.ToInt32(flexObj[text2]) == 0)
							{
								flag2 = true;
							}
						}
						else if (Convert.ToInt32(dynamicObject["FValueType"]) == 1)
						{
							string text2 = text + "_Id";
							if (flexObj[text2] == null || flexObj[text2].ToString() == "")
							{
								flag2 = true;
							}
						}
						else
						{
							string text2 = text;
							string a = dynamicObject["FCustomDataType"].ToString();
							if (a == "1" || a == "5")
							{
								if (flexObj[text2] == null || Convert.ToInt32(flexObj[text2]) == 0)
								{
									flag2 = true;
								}
							}
							else if (a == "2" || a == "3" || a == "6")
							{
								if (flexObj[text2] == null || flexObj[text2].ToString() == "")
								{
									flag2 = true;
								}
							}
							else if (a == "4")
							{
								if (flexObj[text2] == null)
								{
									flag2 = true;
								}
							}
							else
							{
								flag2 = true;
							}
						}
						flag = (flag && flag2);
					}
				}
			}
			return flag;
		}

		// Token: 0x06000768 RID: 1896 RVA: 0x00059628 File Offset: 0x00057828
		private List<DynamicObject> GetInUseFlexItemInfo(RelatedFlexGroupField flexField, DynamicObject bdObj, out int flexItemId, out int flexItemMasterId)
		{
			UseFlexItemInfo useFlexItemInfo = this._cacheUseFlexItemInfoList.FirstOrDefault((UseFlexItemInfo p) => p.IsThisFlexGroup(flexField.Key, Convert.ToString(bdObj[0])));
			if (useFlexItemInfo == null)
			{
				BaseDataField baseDataField = this._metadata.BusinessInfo.GetField(flexField.RelatedBaseDataFlexGroupField) as BaseDataField;
				List<DynamicObject> inUseFlexItemInfo = baseDataField.Controller.GetInUseFlexItemInfo(base.Context, this._metadata.BusinessInfo, flexField, bdObj, "", ref flexItemId, ref flexItemMasterId);
				useFlexItemInfo = new UseFlexItemInfo
				{
					FlexGroupFieldKey = flexField.Key,
					ControlBaseDataId = Convert.ToString(bdObj[0]),
					FlexItemId = flexItemId,
					FlexItemMasterId = flexItemMasterId,
					FlexList = inUseFlexItemInfo
				};
				this._cacheUseFlexItemInfoList.Add(useFlexItemInfo);
			}
			flexItemId = useFlexItemInfo.FlexItemId;
			flexItemMasterId = useFlexItemInfo.FlexItemMasterId;
			return useFlexItemInfo.FlexList;
		}

		// Token: 0x06000769 RID: 1897 RVA: 0x00059730 File Offset: 0x00057930
		private List<string> GetCheckedData()
		{
			List<string> list = this._batchEditItems.Keys.ToList<string>();
			list = this.CheckBatchEditPermission(list);
			list = this.CheckEditPermission(list);
			foreach (Field field in this._fieldList)
			{
				if (this._metadata.BusinessInfo.GetForm().StrategyType == 2)
				{
					list = this.CheckControlStrategy(list, field);
				}
				if (field is BaseDataField)
				{
					try
					{
						list = this.CheckBaseDataUseOrg(list, field as BaseDataField);
					}
					catch (Exception ex)
					{
						Logger.Error("BOS", "Batch edit CheckBaseDataUseOrg exception", ex);
					}
				}
			}
			return list;
		}

		// Token: 0x0600076A RID: 1898 RVA: 0x00059814 File Offset: 0x00057A14
		private List<string> CheckBatchEditPermission(List<string> pkids)
		{
			if (pkids.Count == 0)
			{
				return pkids;
			}
			List<string> result;
			try
			{
				IFormOperation formOperation = this.View.ParentFormView.ParentFormView.GetFormOperation("BULKEDIT");
				if (formOperation == null || string.IsNullOrWhiteSpace(formOperation.FormOperation.PermissionItemId))
				{
					result = pkids;
				}
				else
				{
					List<PermissionAuthResult> source = this.View.ParentFormView.ParentFormView.Model.FuncPermissionAuth(pkids.ToArray(), formOperation.FormOperation.PermissionItemId, null, true);
					foreach (PermissionAuthResult permissionAuthResult in from r in source
					where !r.Passed
					select r)
					{
						if (this._batchEditItems.ContainsKey(permissionAuthResult.Id))
						{
							string message = ResManager.LoadKDString("没有批改权限", "0151515153499030041407", 7, new object[0]);
							this.AddFailDetail(permissionAuthResult.Id, this._batchEditItems[permissionAuthResult.Id], message);
						}
					}
					result = (from r in source
					where r.Passed
					select r.Id).ToList<string>();
				}
			}
			catch (Exception ex)
			{
				Logger.Error("BOS", "Batch edit CheckBatchEditPermission exception", ex);
				result = pkids;
			}
			return result;
		}

		// Token: 0x0600076B RID: 1899 RVA: 0x000599DC File Offset: 0x00057BDC
		private List<string> CheckEditPermission(List<string> pkids)
		{
			if (pkids.Count == 0)
			{
				return pkids;
			}
			IFormOperation formOperation = this.View.ParentFormView.ParentFormView.GetFormOperation(2);
			List<PermissionAuthResult> source = this.View.ParentFormView.ParentFormView.Model.FuncPermissionAuth(pkids.ToArray(), formOperation.FormOperation.PermissionItemId, null, true);
			foreach (PermissionAuthResult permissionAuthResult in from r in source
			where !r.Passed
			select r)
			{
				if (this._batchEditItems.ContainsKey(permissionAuthResult.Id))
				{
					string message = ResManager.LoadKDString("没有修改权限", "002014000004376", 2, new object[0]);
					this.AddFailDetail(permissionAuthResult.Id, this._batchEditItems[permissionAuthResult.Id], message);
				}
			}
			return (from r in source
			where r.Passed
			select r.Id).ToList<string>();
		}

		// Token: 0x0600076C RID: 1900 RVA: 0x00059B44 File Offset: 0x00057D44
		private List<string> CheckControlStrategy(List<string> pkids, Field field)
		{
			if (pkids.Count == 0)
			{
				return pkids;
			}
			string pkFieldName = this._metadata.BusinessInfo.GetForm().PkFieldName;
			string id = this._metadata.BusinessInfo.GetForm().Id;
			string useOrgFieldKey = this._metadata.BusinessInfo.GetForm().UseOrgFieldKey;
			string fieldName = this._metadata.BusinessInfo.GetField(useOrgFieldKey).FieldName;
			string createOrgFieldKey = this._metadata.BusinessInfo.GetForm().CreateOrgFieldKey;
			string fieldName2 = this._metadata.BusinessInfo.GetField(createOrgFieldKey).FieldName;
			string tableName = LocaleHelper.GetTableName(this._metadata.BusinessInfo.GetDynamicObjectType());
			string format = "\r\nSELECT t4.{6} \r\nFROM t_org_bdctrlpropentry t1\r\ninner join t_org_bdctrltorgentry t2 on t1.FTargetOrgEntryID = t2.FTargetOrgEntryID\r\ninner join t_org_bdctrlPolicy t3 on t2.FPolicyID=t3.FPolicyID\r\ninner join {0} t4 on t4.{5} = t2.FTARGETORGID AND t4.{4} = T3.FCREATEORGID AND t4.{5} <> t4.{4}\r\nWHERE t1.FLocked = 1 and t4.{6} in ({1}) and t3.FBASEDATATYPEID = '{2}' AND T1.FKEY = '{3}'";
			string text = string.Format(format, new object[]
			{
				tableName,
				string.Join(",", pkids),
				id,
				field.Key,
				fieldName2,
				fieldName,
				pkFieldName
			});
			using (DataSet dataSet = DBServiceHelper.ExecuteDataSet(base.Context, text))
			{
				if (dataSet.Tables.Count > 0)
				{
					EnumerableRowCollection<string> enumerableRowCollection = from row in dataSet.Tables[0].AsEnumerable()
					select row[pkFieldName].ToString();
					foreach (string text2 in enumerableRowCollection)
					{
						if (this._batchEditItems.ContainsKey(text2))
						{
							string message = ResManager.LoadKDString("控制策略中设置不可修改", "0151515153499030041408", 7, new object[0]);
							this.AddFailDetail(text2, this._batchEditItems[text2], message);
						}
					}
					pkids = pkids.Except(enumerableRowCollection).ToList<string>();
				}
			}
			return pkids;
		}

		// Token: 0x0600076D RID: 1901 RVA: 0x00059D50 File Offset: 0x00057F50
		private List<string> CheckBaseDataUseOrg(List<string> pkids, BaseDataField baseDataField)
		{
			if (pkids.Count == 0 || (!string.IsNullOrEmpty(this._entryName) && (baseDataField.Key == "FSTOCKID" || baseDataField.Key == "FOWNERID" || baseDataField.Key == "FBOMID")))
			{
				return pkids;
			}
			object value = this.Model.GetValue(baseDataField.Key);
			DynamicObject dynamicObject = value as DynamicObject;
			if (baseDataField.LookUpObject != null && BaseDataField.CheckOrgSeprStrategyType(baseDataField.LookUpObject.StrategyType) && dynamicObject != null)
			{
				FormMetadata formMetadata = MetaDataServiceHelper.Load(base.Context, baseDataField.LookUpObject.FormId, true) as FormMetadata;
				if (formMetadata.BusinessInfo.GetForm().IsCanIssue == 1)
				{
					return pkids;
				}
				DynamicObject[] array = BusinessDataServiceHelper.Load(base.Context, new object[]
				{
					dynamicObject["Id"]
				}, formMetadata.BusinessInfo.GetDynamicObjectType());
				string useOrgFieldKey = formMetadata.BusinessInfo.GetForm().UseOrgFieldKey;
				if (string.IsNullOrEmpty(useOrgFieldKey))
				{
					return pkids;
				}
				string propertyName = formMetadata.BusinessInfo.GetField(useOrgFieldKey).PropertyName;
				string b = (array.Length > 0) ? (array[0][propertyName] as DynamicObject)["Id"].ToString() : string.Empty;
				DynamicObject[] array2 = BusinessDataServiceHelper.Load(base.Context, pkids.ToArray(), this._metadata.BusinessInfo.GetDynamicObjectType());
				foreach (DynamicObject dynamicObject2 in array2)
				{
					string orgIdOfDynamicObject = this.GetOrgIdOfDynamicObject(dynamicObject2, baseDataField.OrgFieldKey);
					if (string.IsNullOrEmpty(orgIdOfDynamicObject) || orgIdOfDynamicObject != b)
					{
						string text = dynamicObject2["Id"].ToString();
						pkids.Remove(text);
						if (this._batchEditItems.ContainsKey(text))
						{
							string format = ResManager.LoadKDString("“{0}”的使用组织和“{1}”的{2}不匹配", "0151515153499030041409", 7, new object[0]);
							Field field = this._metadata.BusinessInfo.GetField(baseDataField.OrgFieldKey);
							this.AddFailDetail(text, this._batchEditItems[text], string.Format(format, baseDataField.Name, this._metadata.Name, field.Name));
						}
					}
				}
			}
			return pkids;
		}

		// Token: 0x0600076E RID: 1902 RVA: 0x00059FA8 File Offset: 0x000581A8
		private string GetOrgIdOfDynamicObject(DynamicObject item, string orgKey)
		{
			Field field = this._metadata.BusinessInfo.GetField(orgKey);
			if (field.Entity is HeadEntity)
			{
				if (!DynamicObjectUtils.Contains(item, field.PropertyName))
				{
					return string.Empty;
				}
				return (item[field.PropertyName] as DynamicObject)["Id"].ToString();
			}
			else
			{
				if (!(field.Entity is SubHeadEntity))
				{
					return string.Empty;
				}
				DynamicObjectCollection dynamicObjectCollection = item[field.Entity.EntryName] as DynamicObjectCollection;
				if (dynamicObjectCollection.Count > 0 && DynamicObjectUtils.Contains(dynamicObjectCollection[0], field.PropertyName) && dynamicObjectCollection[0][field.PropertyName] != null)
				{
					return (dynamicObjectCollection[0][field.PropertyName] as DynamicObject)["Id"].ToString();
				}
				return string.Empty;
			}
		}

		// Token: 0x0600076F RID: 1903 RVA: 0x0005A090 File Offset: 0x00058290
		private void AddFailDetail(string dataID, string dataNumber, string message)
		{
			BatchEditDetail item = new BatchEditDetail
			{
				DataID = dataID,
				DataNumber = dataNumber,
				Result = false,
				Message = message.Substring(0, Math.Min(100, message.Length))
			};
			this._batchEditDetails.Add(item);
		}

		// Token: 0x06000770 RID: 1904 RVA: 0x0005A0FC File Offset: 0x000582FC
		private void AddFailDetailEntry(string dataID, string dataNumber, string message)
		{
			this._batchEditDetails.RemoveAll((BatchEditDetail ed) => ed.DataID == dataID);
			BatchEditDetail item = new BatchEditDetail
			{
				DataID = dataID,
				DataNumber = dataNumber,
				Result = false,
				Message = message.Substring(0, Math.Min(100, message.Length))
			};
			this._batchEditDetails.Add(item);
		}

		// Token: 0x06000771 RID: 1905 RVA: 0x0005A178 File Offset: 0x00058378
		private void AddSuccessDetail(string dataID, string dataNumber)
		{
			BatchEditDetail item = new BatchEditDetail
			{
				DataID = dataID,
				DataNumber = dataNumber,
				Result = true
			};
			this._batchEditDetails.Add(item);
		}

		// Token: 0x06000772 RID: 1906 RVA: 0x0005A1B0 File Offset: 0x000583B0
		private void AddSuccessDetail(string dataID, string dataNumber, string message)
		{
			BatchEditDetail item = new BatchEditDetail
			{
				DataID = dataID,
				DataNumber = dataNumber,
				Result = true,
				Message = message.Substring(0, Math.Min(100, message.Length))
			};
			this._batchEditDetails.Add(item);
		}

		// Token: 0x06000773 RID: 1907 RVA: 0x0005A200 File Offset: 0x00058400
		private void AddOperationResult(IOperationResult saveResult, DynamicObject[] arrObjs)
		{
			List<ValidationErrorInfo> fatalErrorResults = saveResult.GetFatalErrorResults();
			foreach (ValidationErrorInfo validationErrorInfo in fatalErrorResults)
			{
				if (validationErrorInfo.DataEntityIndex < arrObjs.Length)
				{
					string text = OrmUtils.GetPrimaryKeyValue(arrObjs[validationErrorInfo.DataEntityIndex], true).ToString();
					if (string.IsNullOrEmpty(this._entryName))
					{
						this.AddFailDetail(text, this._batchEditItems[text], validationErrorInfo.Message);
					}
					else
					{
						this.AddFailDetailEntry(text, this._batchEditItems[text], validationErrorInfo.Message);
					}
				}
			}
			foreach (OperateResult operateResult in saveResult.OperateResult)
			{
				string text2 = operateResult.PKValue.ToString();
				if (this._batchEditItems.ContainsKey(text2))
				{
					if (operateResult.SuccessStatus)
					{
						if (string.IsNullOrEmpty(this._entryName))
						{
							this.AddSuccessDetail(text2, this._batchEditItems[text2]);
						}
					}
					else if (string.IsNullOrEmpty(this._entryName))
					{
						this.AddFailDetail(text2, this._batchEditItems[text2], operateResult.Message);
					}
					else
					{
						this.AddFailDetailEntry(text2, this._batchEditItems[text2], operateResult.Message);
					}
				}
			}
		}

		// Token: 0x06000774 RID: 1908 RVA: 0x0005A3AC File Offset: 0x000585AC
		private string SaveBatchEditLog()
		{
			BatchEdit batchEdit = new BatchEdit
			{
				FormID = this.formId,
				FormName = this._metadata.BusinessInfo.GetForm().Name,
				FieldKey = this.fieldKey,
				FieldName = this._metadata.BusinessInfo.GetField(this.fieldKey).Name,
				ModifierID = base.Context.UserId,
				ModifyDate = this._batchEditTime
			};
			using (Dictionary<string, string>.Enumerator enumerator = this._batchEditItems.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					KeyValuePair<string, string> item = enumerator.Current;
					if (!this._batchEditDetails.Exists(delegate(BatchEditDetail d)
					{
						string dataID = d.DataID;
						KeyValuePair<string, string> item3 = item;
						return dataID == item3.Key;
					}))
					{
						KeyValuePair<string, string> item4 = item;
						string key = item4.Key;
						KeyValuePair<string, string> item2 = item;
						this.AddFailDetail(key, item2.Value, ResManager.LoadKDString("批改失败", "0151515153499030041410", 7, new object[0]));
					}
				}
			}
			IBatchEditService service = ServiceFactory.GetService<IBatchEditService>(base.Context);
			return service.SaveBatchEditLog(base.Context, batchEdit, this._batchEditDetails);
		}

		// Token: 0x06000775 RID: 1909 RVA: 0x0005A508 File Offset: 0x00058708
		private void WriteOperateLog(Context ctx, string opName, string description)
		{
			LogServiceHelper.WriteLog(ctx, new LogObject
			{
				SubSystemId = this._metadata.BusinessInfo.GetForm().SubsysId,
				ObjectTypeId = this.formId,
				Environment = 3,
				OperateName = opName,
				Description = description
			});
		}

		// Token: 0x06000776 RID: 1910 RVA: 0x0005A560 File Offset: 0x00058760
		public override void DataChanged(DataChangedEventArgs e)
		{
			if (this.fieldKey == "FISSUETYPE" && e.Field.Key == "FISSUETYPE")
			{
				if (Convert.ToInt16(e.NewValue) == 2 || Convert.ToInt16(e.NewValue) == 4)
				{
					this.View.StyleManager.SetEnabled("FBACKFLUSHTYPE", "FBACKFLUSHTYPE", true);
				}
				else
				{
					this.View.StyleManager.SetEnabled("FBACKFLUSHTYPE", "FBACKFLUSHTYPE", false);
					this.View.Model.SetValue("FBACKFLUSHTYPE", "");
				}
				this.View.UpdateView("FBACKFLUSHTYPE");
				return;
			}
			if (this.fieldKey == "FSTOCKID" && e.Field.Key == "FSTOCKID")
			{
				this.View.Model.SetValue("FSTOCKLOCID", "");
			}
		}

		// Token: 0x06000777 RID: 1911 RVA: 0x0005A6A8 File Offset: 0x000588A8
		private IEnumerable<DynamicObject> ReGetSubstitute(DynamicObjectCollection BOMEntryObjs)
		{
			IEnumerable<DynamicObject> result = null;
			List<DynamicObject> list = (from w in BOMEntryObjs
			where DataEntityExtend.GetDynamicValue<long>(w, "MATERIALIDCHILD_Id", 0L) > 0L && DataEntityExtend.GetDynamicValue<int>(w, "MATERIALTYPE", 0) == 1 && !DataEntityExtend.GetDynamicValue<bool>(w, "IsSkip", false)
			select w).ToList<DynamicObject>();
			if (ListUtils.IsEmpty<DynamicObject>(list))
			{
				return result;
			}
			List<DynamicObject> list2 = (from w in list
			where DataEntityExtend.GetDynamicValue<string>(w, "EntrySource", null) == "1"
			select w).ToList<DynamicObject>();
			if (ListUtils.IsEmpty<DynamicObject>(list2))
			{
				return result;
			}
			return SubStituteViewServiceHelper.GetSubstitueSchemeData(base.Context, list2);
		}

		// Token: 0x06000778 RID: 1912 RVA: 0x0005A72C File Offset: 0x0005892C
		private bool bExistsSubstitute(DynamicObject rowData, IEnumerable<DynamicObject> substitueSchemeDataLst)
		{
			bool result = false;
			if (substitueSchemeDataLst == null)
			{
				return result;
			}
			foreach (DynamicObject dynamicObject in substitueSchemeDataLst)
			{
				DynamicObjectCollection dynamicValue = DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(dynamicObject, "EntityMainItems", null);
				DynamicObject dynamicObject2 = dynamicValue.FirstOrDefault<DynamicObject>();
				long dynamicValue2 = DataEntityExtend.GetDynamicValue<long>(dynamicObject2, "MaterialID_Id", 0L);
				long dynamicValue3 = DataEntityExtend.GetDynamicValue<long>(dynamicObject2, "AuxPropID_Id", 0L);
				long dynamicValue4 = DataEntityExtend.GetDynamicValue<long>(dynamicObject2, "BomId_Id", 0L);
				if (DataEntityExtend.GetDynamicValue<long>(rowData, "MATERIALIDCHILD_Id", 0L) == dynamicValue2 && DataEntityExtend.GetDynamicValue<long>(rowData, "AuxPropId_Id", 0L) == dynamicValue3 && DataEntityExtend.GetDynamicValue<long>(rowData, "BOMID_Id", 0L) == dynamicValue4)
				{
					result = true;
					break;
				}
			}
			return result;
		}

		// Token: 0x06000779 RID: 1913 RVA: 0x0005A7F8 File Offset: 0x000589F8
		public override void BeforeUpdateValue(BeforeUpdateValueEventArgs e)
		{
			base.BeforeUpdateValue(e);
			if (e.Key == "FSTOCKLOCID" && e.Value is string && !(this.View.Model.GetValue("FSTOCKLOCID") is DynamicObject))
			{
				e.Value = "";
				this.View.Model.SetValue("FSTOCKLOCID", null);
				this.View.UpdateView("FSTOCKLOCID");
			}
		}

		// Token: 0x0600077A RID: 1914 RVA: 0x0005A87C File Offset: 0x00058A7C
		public override void CustomEvents(CustomEventsArgs e)
		{
			string empty = string.Empty;
			string eventName;
			if ((eventName = e.EventName) != null)
			{
				if (!(eventName == "EnterKeyPressed"))
				{
					return;
				}
				string a;
				if ((a = e.Key.ToUpper()) != null)
				{
					if (!(a == "FSTOCKLOCID"))
					{
						return;
					}
					if (!(this.View.Model.GetValue("FSTOCKLOCID") is DynamicObject))
					{
						this.View.Model.SetValue("FSTOCKLOCID", null);
						this.View.UpdateView("FSTOCKLOCID");
					}
				}
			}
		}

		// Token: 0x04000347 RID: 839
		private List<Field> _fieldList = new List<Field>();

		// Token: 0x04000348 RID: 840
		private List<FieldAppearance> _fieldAppList = new List<FieldAppearance>();

		// Token: 0x04000349 RID: 841
		private LayoutInfo _currentBillLayoutInfo;

		// Token: 0x0400034A RID: 842
		private BusinessInfo _currentBillBusinessInfo;

		// Token: 0x0400034B RID: 843
		private string formId;

		// Token: 0x0400034C RID: 844
		private FormMetadata _metadata;

		// Token: 0x0400034D RID: 845
		private string fieldKey;

		// Token: 0x0400034E RID: 846
		private List<BatchEditDetail> _batchEditDetails = new List<BatchEditDetail>();

		// Token: 0x0400034F RID: 847
		private Dictionary<string, string> _batchEditItems = new Dictionary<string, string>();

		// Token: 0x04000350 RID: 848
		private string _progressPageID;

		// Token: 0x04000351 RID: 849
		private string _batchID;

		// Token: 0x04000352 RID: 850
		private bool _progressClosed;

		// Token: 0x04000353 RID: 851
		private Dictionary<string, object> _fieldValues = new Dictionary<string, object>();

		// Token: 0x04000354 RID: 852
		private DateTime _batchEditTime;

		// Token: 0x04000355 RID: 853
		private Dictionary<long, List<long>> _pkEntryIdsDic = new Dictionary<long, List<long>>();

		// Token: 0x04000356 RID: 854
		private string _entryName = "";

		// Token: 0x04000357 RID: 855
		private LocaleValue _billName;

		// Token: 0x04000358 RID: 856
		private LocaleValue _formName;

		// Token: 0x04000359 RID: 857
		private LocaleValue _fieldName;

		// Token: 0x0400035A RID: 858
		private List<object> _rules = new List<object>();

		// Token: 0x0400035B RID: 859
		private Dictionary<long, IGrouping<long, DynamicObject>> mtrMsterDic = new Dictionary<long, IGrouping<long, DynamicObject>>();

		// Token: 0x0400035C RID: 860
		private List<UseFlexItemInfo> _cacheUseFlexItemInfoList = new List<UseFlexItemInfo>();

		// Token: 0x02000168 RID: 360
		[CompilerGenerated]
		private static class <SetFlexValue>o__SiteContainer1c
		{
			// Token: 0x04000818 RID: 2072
			public static CallSite<Func<CallSite, object, object>> <>p__Site1d;
		}
	}
}
