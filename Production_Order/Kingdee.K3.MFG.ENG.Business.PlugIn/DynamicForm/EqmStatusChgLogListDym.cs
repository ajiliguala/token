using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Kingdee.BOS;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.List.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Core.Permission;
using Kingdee.BOS.Model.ReportFilter;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Orm.Metadata.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.MFG.Utils;
using Kingdee.K3.MFG.Common.BusinessEntity.SFC.SFCDymObjManager;
using Kingdee.K3.MFG.ENG.Business.PlugIn.Base;
using Kingdee.K3.MFG.ServiceHelper;
using Kingdee.K3.MFG.ServiceHelper.DI;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm
{
	// Token: 0x02000096 RID: 150
	[Description("设备状态变更日志（动态表单）处理类")]
	[HotUpdate]
	public class EqmStatusChgLogListDym : BaseControlList
	{
		// Token: 0x1700006D RID: 109
		// (get) Token: 0x06000ACC RID: 2764 RVA: 0x0007C4BB File Offset: 0x0007A6BB
		protected FormMetadata EqmStatusMetadata
		{
			get
			{
				if (this.eqmStatusMetadata == null)
				{
					this.eqmStatusMetadata = SFCEquipmentManager.Instance.GetFormMetadata(base.Context);
				}
				return this.eqmStatusMetadata;
			}
		}

		// Token: 0x1700006E RID: 110
		// (get) Token: 0x06000ACD RID: 2765 RVA: 0x0007C4E1 File Offset: 0x0007A6E1
		protected FormMetadata EqmChgStatusLogMetadata
		{
			get
			{
				if (this.eqmChgStatusLogMetadata == null)
				{
					this.eqmChgStatusLogMetadata = SFCEqmStatusChgLogManager.Instance.GetFormMetadata(base.Context);
				}
				return this.eqmChgStatusLogMetadata;
			}
		}

		// Token: 0x06000ACE RID: 2766 RVA: 0x0007C508 File Offset: 0x0007A708
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
			if (this.currOrgId == 0L)
			{
				this.currOrgId = base.Context.CurrentOrganizationInfo.ID;
			}
			List<long> permissionOrg = this.GetPermissionOrg();
			if (!permissionOrg.Contains(this.currOrgId) && this.currOrgId > 0L)
			{
				this.currOrgId = 0L;
				this.View.ShowErrMessage(ResManager.LoadKDString("没有当前组织的业务权限，请重新选择!", "0151515153499000013567", 7, new object[0]), "", 0);
			}
			this.lstEqmIds = (this.View.OpenParameter.GetCustomParameter("eqmIds") as List<long>);
			object customParameter = this.View.OpenParameter.GetCustomParameter("IsOpenByEqm");
			if (customParameter != null)
			{
				this.IsOpenByEqm = Convert.ToBoolean(customParameter);
			}
			else if (this.lstEqmIds != null)
			{
				this.lstEqmIds.Clear();
			}
			this.filterParam = this.GetNextEntrySchemeFilter();
			if (this.filterParam != null)
			{
				this.filterString = this.filterParam.FilterString;
			}
		}

		// Token: 0x06000ACF RID: 2767 RVA: 0x0007C609 File Offset: 0x0007A809
		public override void PrepareFilterParameter(FilterArgs e)
		{
			base.PrepareFilterParameter(e);
		}

		// Token: 0x06000AD0 RID: 2768 RVA: 0x0007C612 File Offset: 0x0007A812
		public override void BeforeBindData(EventArgs e)
		{
			base.BeforeBindData(e);
		}

		// Token: 0x06000AD1 RID: 2769 RVA: 0x0007C61B File Offset: 0x0007A81B
		public override void AfterBindData(EventArgs e)
		{
			base.AfterBindData(e);
		}

		// Token: 0x06000AD2 RID: 2770 RVA: 0x0007C624 File Offset: 0x0007A824
		public override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (this.IsFilter)
			{
				return;
			}
			this.LoadData();
		}

		// Token: 0x06000AD3 RID: 2771 RVA: 0x0007C6C4 File Offset: 0x0007A8C4
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			base.BarItemClick(e);
			string a;
			if ((a = e.BarItemKey.ToUpper()) != null && !(a == "TBREFRESH"))
			{
				if (a == "TBFILTER")
				{
					MFGBillUtil.ShowFilterForm(this.View, this.GetBillName(), null, delegate(FormResult filterResult)
					{
						if (filterResult.ReturnData is FilterParameter)
						{
							this.IsFilter = true;
							this.filterParam = (filterResult.ReturnData as FilterParameter);
							this.filterString = this.filterParam.FilterString;
							this.LoadEqmLogDataByFilter();
							SplitContainer splitContainer = (SplitContainer)this.View.GetControl("FSpliteContainer");
							splitContainer.HideFirstPanel(false);
							splitContainer.HideSecondPanel(false);
							EqmStatusChgLogListDym.ShowOrHideField(this.View, this.filterParam);
							this.IsFilter = false;
						}
					}, this.GetFilterName(), 0);
					e.Cancel = true;
					return;
				}
				if (!(a == "TBBULKMODIFY"))
				{
					return;
				}
				this.ShowF7Op();
			}
		}

		// Token: 0x06000AD4 RID: 2772 RVA: 0x0007C749 File Offset: 0x0007A949
		public override void AfterEntryBarItemClick(AfterBarItemClickEventArgs e)
		{
			base.AfterEntryBarItemClick(e);
		}

		// Token: 0x06000AD5 RID: 2773 RVA: 0x0007C752 File Offset: 0x0007A952
		public override void EntityRowClick(EntityRowClickEventArgs e)
		{
			base.EntityRowClick(e);
			if (e.Key.Equals("FEntityDev", StringComparison.OrdinalIgnoreCase))
			{
				if (this.IsFilter)
				{
					return;
				}
				this.LoadEqmLogData();
			}
		}

		// Token: 0x06000AD6 RID: 2774 RVA: 0x0007C77D File Offset: 0x0007A97D
		public override void EntityRowDoubleClick(EntityRowClickEventArgs e)
		{
			base.EntityRowDoubleClick(e);
		}

		// Token: 0x06000AD7 RID: 2775 RVA: 0x0007C788 File Offset: 0x0007A988
		private void LoadData()
		{
			if (this.currOrgId <= 0L)
			{
				this.currOrgId = 0L;
				this.View.ShowErrMessage(ResManager.LoadKDString("没有设置设备监控权限，请设置！", "0151515153499000013568", 7, new object[0]), "", 0);
				return;
			}
			if (!this.IsOpenByEqm)
			{
				this.lstEqmIds = DICommonServericeHelper.GetEquipmentByUserAuth(base.Context, base.Context.UserId, this.currOrgId).ToList<long>();
			}
			if (this.lstEqmIds != null && this.lstEqmIds.Count > 0)
			{
				this.BindDataToDevEntry(this.lstEqmIds);
				return;
			}
			this.currOrgId = 0L;
			this.View.ShowErrMessage(ResManager.LoadKDString("没有设置设备监控权限，请设置！", "0151515153499000013568", 7, new object[0]), "", 0);
		}

		// Token: 0x06000AD8 RID: 2776 RVA: 0x0007C850 File Offset: 0x0007AA50
		private void LoadEqmLogData()
		{
			if (this.IsFilter)
			{
				return;
			}
			int num;
			DynamicObject currentDevDym = this.GetCurrentDevDym(out num);
			if (num < 0 || currentDevDym == null)
			{
				return;
			}
			if (this.filterParam == null)
			{
				string text = string.Format("FEquipmentId = {0} ", currentDevDym["DEquipmentId_Id"]);
				this.filterParam = new FilterParameter();
				this.filterParam.FilterString = text;
				this.filterString = "";
			}
			this.LoadEqmLogDataByFilter();
			SplitContainer splitContainer = (SplitContainer)this.View.GetControl("FSpliteContainer");
			splitContainer.HideFirstPanel(false);
			splitContainer.HideSecondPanel(false);
			EqmStatusChgLogListDym.ShowOrHideField(this.View, this.filterParam);
		}

		// Token: 0x06000AD9 RID: 2777 RVA: 0x0007C8F4 File Offset: 0x0007AAF4
		private FilterParameter GetNextEntrySchemeFilter()
		{
			return this.GetDirectInScheme();
		}

		// Token: 0x06000ADA RID: 2778 RVA: 0x0007C90C File Offset: 0x0007AB0C
		protected FilterParameter GetDirectInScheme()
		{
			FilterParameter result = null;
			string nextEntrySchemeId = UserParamterServiceHelper.GetNextEntrySchemeId(base.Context, "ENG_EqmStatusChgLogDym");
			if (nextEntrySchemeId != null && nextEntrySchemeId != string.Empty)
			{
				FilterMetaData cachedFilterMetaData = FormMetaDataCache.GetCachedFilterMetaData(base.Context);
				FormMetadata cachedFormMetaData = FormMetaDataCache.GetCachedFormMetaData(base.Context, "ENG_EqmStatusChgLogDym");
				FormMetadata cachedFormMetaData2 = FormMetaDataCache.GetCachedFormMetaData(base.Context, "ENG_EqmStatusChgLog_Filter");
				IResourceServiceProvider formServiceProvider = cachedFormMetaData2.BusinessInfo.GetForm().GetFormServiceProvider(false);
				SysReportFilterModel sysReportFilterModel = new SysReportFilterModel();
				sysReportFilterModel.SetContext(base.Context, cachedFormMetaData2.BusinessInfo, formServiceProvider);
				sysReportFilterModel.FormId = cachedFormMetaData2.BusinessInfo.GetForm().Id;
				sysReportFilterModel.FilterObject.FilterMetaData = cachedFilterMetaData;
				sysReportFilterModel.InitFieldList(cachedFormMetaData, cachedFormMetaData2);
				sysReportFilterModel.GetSchemeList();
				sysReportFilterModel.Load(nextEntrySchemeId);
				result = sysReportFilterModel.GetFilterParameter();
			}
			return result;
		}

		// Token: 0x06000ADB RID: 2779 RVA: 0x0007C9EC File Offset: 0x0007ABEC
		private DateTime? GetUserNow(FilterObject filter)
		{
			bool flag = false;
			foreach (FilterRow filterRow in filter.FilterRows)
			{
				flag = (filterRow.FilterField.FieldType == 58 || filterRow.FilterField.FieldType == 189 || filterRow.FilterField.FieldType == 61);
				if (flag)
				{
					break;
				}
			}
			if (flag)
			{
				return new DateTime?(MFGServiceHelper.GetSysDate(base.Context));
			}
			return null;
		}

		// Token: 0x06000ADC RID: 2780 RVA: 0x0007CA90 File Offset: 0x0007AC90
		private void BindDataToDevEntry(List<long> lstEqmIds)
		{
			string text = string.Format("FID IN ({0}) ", string.Join<long>(",", lstEqmIds));
			OQLFilter oqlfilter = OQLFilter.CreateHeadEntityFilter(text);
			oqlfilter.Add(new OQLFilterHeadEntityItem
			{
				FilterString = ""
			});
			DynamicObject[] array = BusinessDataServiceHelper.Load(base.Context, this.EqmStatusMetadata.BusinessInfo, null, oqlfilter);
			DynamicObject dynamicObject;
			int num;
			this.View.Model.TryGetEntryCurrentRow("FEntityDev", ref dynamicObject, ref num);
			DynamicObjectCollection dynamicObjectCollection = this.View.Model.DataObject["EntityDev"] as DynamicObjectCollection;
			dynamicObjectCollection.Clear();
			DynamicObjectType dynamicCollectionItemPropertyType = dynamicObjectCollection.DynamicCollectionItemPropertyType;
			foreach (DynamicObject dynamicObject2 in array)
			{
				DynamicObject dynamicObject3 = new DynamicObject(dynamicCollectionItemPropertyType);
				dynamicObject3["DEquipmentId"] = dynamicObject2;
				dynamicObject3["DEquipmentId_Id"] = dynamicObject2["Id"];
				dynamicObjectCollection.Add(dynamicObject3);
			}
			if (num == -1)
			{
				num = 0;
			}
			this.View.UpdateView("FEntityDev");
		}

		// Token: 0x06000ADD RID: 2781 RVA: 0x0007CBA4 File Offset: 0x0007ADA4
		private List<long> GetPermissionOrg()
		{
			BusinessObject businessObject = new BusinessObject
			{
				Id = SFCEqmStatusChgLogManager.Instance.FormId,
				PermissionControl = this.View.BillBusinessInfo.GetForm().SupportPermissionControl,
				SubSystemId = this.View.ParentFormView.Model.SubSytemId
			};
			return MFGServiceHelper.GetPermissionOrg(base.Context, businessObject, "6e44119a58cb4a8e86f6c385e14a17ad");
		}

		// Token: 0x06000ADE RID: 2782 RVA: 0x0007CC14 File Offset: 0x0007AE14
		private DynamicObject GetCurrentDevDym(out int rowIndex)
		{
			rowIndex = -1;
			DynamicObject result;
			this.View.Model.TryGetEntryCurrentRow("FEntityDev", ref result, ref rowIndex);
			return result;
		}

		// Token: 0x06000ADF RID: 2783 RVA: 0x0007CC40 File Offset: 0x0007AE40
		private DynamicObject GetCurrentLogDym(out int rowIndex)
		{
			rowIndex = -1;
			DynamicObject result;
			this.View.Model.TryGetEntryCurrentRow("FEntityLog", ref result, ref rowIndex);
			return result;
		}

		// Token: 0x06000AE0 RID: 2784 RVA: 0x0007CC6A File Offset: 0x0007AE6A
		protected virtual string GetBillName()
		{
			return "ENG_EqmStatusChgLogDym";
		}

		// Token: 0x06000AE1 RID: 2785 RVA: 0x0007CC71 File Offset: 0x0007AE71
		protected virtual string GetFilterName()
		{
			return "ENG_EqmStatusChgLog_Filter";
		}

		// Token: 0x06000AE2 RID: 2786 RVA: 0x0007CC78 File Offset: 0x0007AE78
		protected void LoadEqmLogDataByFilter()
		{
			this.Model.BeginIniti();
			this.Model.DeleteEntryData("FEntityLog");
			this.GetLogInfo();
			this.Model.EndIniti();
			this.View.UpdateView("FEntityLog");
		}

		// Token: 0x06000AE3 RID: 2787 RVA: 0x0007CCB6 File Offset: 0x0007AEB6
		private void GetLogInfo()
		{
			this.GetLogData();
			if (this.lstBomHeadData != null)
			{
				this.FillEqmLogData(this.lstBomHeadData.ToArray());
			}
		}

		// Token: 0x06000AE4 RID: 2788 RVA: 0x0007CCD8 File Offset: 0x0007AED8
		private void GetLogData()
		{
			int num;
			DynamicObject currentDevDym = this.GetCurrentDevDym(out num);
			if (num < 0 || currentDevDym == null)
			{
				this.View.ShowErrMessage(ResManager.LoadKDString("请选择设备！", "0151515153499000013587", 7, new object[0]), "", 0);
				return;
			}
			string str = string.Format("FEquipmentId = {0} ", currentDevDym["DEquipmentId_Id"]);
			if (this.filterParam == null)
			{
				this.filterParam = new FilterParameter();
				this.filterParam.FilterString = str;
				this.filterString = "";
			}
			this.filterParam.FilterString = str + (string.IsNullOrWhiteSpace(this.filterString) ? "" : (" AND " + this.filterString));
			this.lstBomHeadData = DICommonServericeHelper.GetEqmStatusChgLogQueryItems(base.Context, this.filterParam, this.View.BillBusinessInfo.GetForm().Id);
		}

		// Token: 0x06000AE5 RID: 2789 RVA: 0x0007CDC0 File Offset: 0x0007AFC0
		private void FillEqmLogData(DynamicObject[] loginfos)
		{
			DynamicObjectCollection dynamicObjectCollection = this.View.Model.DataObject["EntityLog"] as DynamicObjectCollection;
			dynamicObjectCollection.Clear();
			DynamicObjectType dynamicCollectionItemPropertyType = dynamicObjectCollection.DynamicCollectionItemPropertyType;
			foreach (DynamicObject dynamicObject in loginfos)
			{
				DynamicObject dynamicObject2 = new DynamicObject(dynamicCollectionItemPropertyType);
				dynamicObject2["ChangeTime"] = dynamicObject["ChangeTime"];
				dynamicObject2["ModifyDate"] = dynamicObject["ModifyDate"];
				dynamicObject2["EndChangeTime"] = dynamicObject["EndChangeTime"];
				dynamicObject2["TimeInterval"] = dynamicObject["TimeInterval"];
				dynamicObject2["OriginalStatus"] = dynamicObject["OriginalStatus"];
				dynamicObject2["ChangedStatus"] = dynamicObject["ChangedStatus"];
				dynamicObject2["ExpType"] = dynamicObject["ExpType"];
				dynamicObject2["ExpType_Id"] = dynamicObject["ExpType_Id"];
				dynamicObject2["ChangerId"] = dynamicObject["ChangerId"];
				dynamicObject2["ChangerId_Id"] = dynamicObject["ChangerId_Id"];
				dynamicObject2["ChangeType"] = dynamicObject["ChangeType"];
				dynamicObject2["MOBillNO"] = dynamicObject["MOBillNO"];
				dynamicObject2["MOSeq"] = dynamicObject["MOSeq"];
				dynamicObject2["OpPlanBillNo"] = dynamicObject["OpPlanBillNo"];
				dynamicObject2["OpSeq"] = dynamicObject["OpSeq"];
				dynamicObject2["OpNo"] = dynamicObject["OpNo"];
				dynamicObject2["ProcessId"] = dynamicObject["ProcessId"];
				dynamicObject2["ProcessId_Id"] = dynamicObject["ProcessId_Id"];
				dynamicObject2["EquipmentId"] = dynamicObject["EquipmentId"];
				dynamicObject2["EquipmentId_Id"] = dynamicObject["EquipmentId_Id"];
				dynamicObject2["TaskId"] = dynamicObject["TaskId"];
				dynamicObject2["Number"] = dynamicObject["Number"];
				dynamicObject2["TrackNo"] = dynamicObject["TrackNo"];
				dynamicObject2["FaultCode"] = dynamicObject["FaultCode"];
				dynamicObject2["Description"] = dynamicObject["Description"];
				dynamicObject2["OrgId"] = dynamicObject["OrgId"];
				dynamicObject2["OrgId_Id"] = dynamicObject["OrgId_Id"];
				dynamicObject2["LogId"] = dynamicObject["LogId"];
				dynamicObjectCollection.Add(dynamicObject2);
			}
			this.View.UpdateView("FEntityLog");
		}

		// Token: 0x06000AE6 RID: 2790 RVA: 0x0007D0C4 File Offset: 0x0007B2C4
		public static void ShowOrHideField(IDynamicFormView view, FilterParameter filterParam)
		{
			if (filterParam == null)
			{
				return;
			}
			using (List<Field>.Enumerator enumerator = view.BillBusinessInfo.GetFieldList().GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Field field = enumerator.Current;
					if (!"FEntityDev".Equals(field.EntityKey) && !(field.Entity is HeadEntity))
					{
						try
						{
							bool visible = false;
							if (filterParam.ColumnInfo.Count == 0 && (filterParam.SelectedEntities == null || filterParam.SelectedEntities.Count == 0))
							{
								break;
							}
							ColumnField columnField = filterParam.ColumnInfo.FirstOrDefault((ColumnField item) => item.Key.Equals(field.Key, StringComparison.InvariantCultureIgnoreCase));
							if (columnField != null)
							{
								visible = columnField.Visible;
							}
							Control control = view.GetControl(field.Key);
							if (control != null)
							{
								control.Visible = visible;
							}
						}
						catch
						{
						}
					}
				}
			}
		}

		// Token: 0x06000AE7 RID: 2791 RVA: 0x0007D348 File Offset: 0x0007B548
		private void ShowF7Op()
		{
			if (!this.ValidateSelectDatas())
			{
				return;
			}
			new BeforeF7SelectEventArgs("ENG_EQMStatusAlertTypeBatEdit", "FAlertTypeId");
			ListShowParameter listShowParameter = new ListShowParameter();
			listShowParameter.FormId = "SFC_AlertType";
			listShowParameter.PageId = "SFC_AlertType";
			listShowParameter.ParentPageId = this.View.PageId;
			listShowParameter.IsLookUp = true;
			listShowParameter.ListType = 2;
			listShowParameter.ListFilterParameter.Filter = "FIsSysPreset != '1' AND FUseOrgId= @UserOrgId and FEffectStatus = @ChangeStatus ";
			long num = Convert.ToInt64(this.lstModifyDatas[0]["OrgId_Id"]);
			string text = Convert.ToString(this.lstModifyDatas[0]["ChangedStatus"]);
			listShowParameter.SqlParams.Add(new SqlParam("@UserOrgId", 12, num));
			listShowParameter.SqlParams.Add(new SqlParam("@ChangeStatus", 16, text));
			listShowParameter.CustomComplexParams["IsBacth"] = true;
			listShowParameter.CustomComplexParams["LogList"] = this.lstModifyDatas;
			this.View.ShowForm(listShowParameter, delegate(FormResult result)
			{
				if (result != null && result.ReturnData != null && !"Clear".Equals(result.ReturnData.ToString()))
				{
					IOperationResult operationResult = new OperationResult();
					foreach (DynamicObject dynamicObject in this.lstModifyDatas)
					{
						OperateResult operateResult = new OperateResult();
						operateResult.SuccessStatus = true;
						string value = string.Format(ResManager.LoadKDString("工序计划：{0}-{1}-{2}", "0151515153499000013572", 7, new object[0]), Convert.ToString(dynamicObject["OpPlanBillNo"]), Convert.ToString(dynamicObject["OpSeq"]), Convert.ToString(dynamicObject["OpNo"]));
						operateResult.Name = string.Format(ResManager.LoadKDString("变更时间：{0}", "0151515153499000013573", 7, new object[0]), Convert.ToDateTime(dynamicObject["ChangeTime"]).ToString());
						operateResult.Message = Convert.ToString(value);
						operateResult.PKValue = Convert.ToInt64(dynamicObject["Id"]);
						operateResult.MessageType = -1;
						operationResult.OperateResult.Add(operateResult);
					}
					FormUtils.ShowOperationResult(this.View, operationResult, null);
					this.UpdateModifyLog();
				}
			});
		}

		// Token: 0x06000AE8 RID: 2792 RVA: 0x0007D46C File Offset: 0x0007B66C
		private bool ValidateSelectDatas()
		{
			bool result = true;
			string text = null;
			this.GetSelectedLogList();
			if (this.lstSelectDatas.Count <= 0)
			{
				this.View.ShowErrMessage(ResManager.LoadKDString("至少选中一行数据！", "0151515153499000013569", 7, new object[0]), "", 0);
				result = false;
			}
			else
			{
				new StringBuilder();
				string text2 = null;
				this.lstModifyDatas.Clear();
				foreach (DynamicObject dynamicObject in this.lstSelectDatas)
				{
					long num = Convert.ToInt64(dynamicObject["LogId"]);
					DynamicObject dynamicObject2 = BusinessDataServiceHelper.Load(base.Context, new object[]
					{
						num
					}, this.EqmChgStatusLogMetadata.BusinessInfo.GetDynamicObjectType()).FirstOrDefault<DynamicObject>();
					DynamicObject dynamicObject3 = dynamicObject2["ExpType"] as DynamicObject;
					if (dynamicObject3 != null)
					{
						long num2 = Convert.ToInt64(dynamicObject3["msterID"]);
						if (num2 == 10001L || num2 == 10002L)
						{
							text = ResManager.LoadKDString("不能批改开工或完工待机事件！", "0151515153499000013570", 7, new object[0]);
							result = false;
							break;
						}
					}
					string text3 = Convert.ToString(dynamicObject2["ChangedStatus"]);
					if (string.IsNullOrWhiteSpace(text2))
					{
						text2 = text3;
					}
					else if (text2 != text3)
					{
						text = ResManager.LoadKDString("选中行的变更后状态必须相同才能批改！", "0151515153499000013571", 7, new object[0]);
						result = false;
						break;
					}
					this.lstModifyDatas.Add(dynamicObject2);
				}
				if (!string.IsNullOrWhiteSpace(text))
				{
					this.View.ShowErrMessage(text, "", 0);
				}
			}
			return result;
		}

		// Token: 0x06000AE9 RID: 2793 RVA: 0x0007D648 File Offset: 0x0007B848
		private List<DynamicObject> GetSelectedLogList()
		{
			this.lstSelectDatas.Clear();
			DynamicObjectCollection source = this.View.Model.DataObject["EntityLog"] as DynamicObjectCollection;
			List<DynamicObject> result = (from o in source
			where Convert.ToBoolean(o["CheckBox"])
			select o).ToList<DynamicObject>();
			this.lstSelectDatas = result;
			return result;
		}

		// Token: 0x06000AEA RID: 2794 RVA: 0x0007D6D4 File Offset: 0x0007B8D4
		private void UpdateModifyLog()
		{
			if (this.lstModifyDatas == null)
			{
				return;
			}
			FormMetadata formMetadata = MetaDataServiceHelper.Load(base.Context, "SFC_AlertType", true) as FormMetadata;
			List<string> list = new List<string>();
			foreach (DynamicObject dynamicObject in this.lstModifyDatas)
			{
				long Id = Convert.ToInt64(dynamicObject["Id"]);
				DynamicObject dynamicObject2 = (from o in this.lstSelectDatas
				where Convert.ToInt64(o["LogId"]) == Id
				select o).FirstOrDefault<DynamicObject>();
				if (dynamicObject2 != null)
				{
					DynamicObject dynamicObject3 = BusinessDataServiceHelper.Load(base.Context, new object[]
					{
						Id
					}, this.EqmChgStatusLogMetadata.BusinessInfo.GetDynamicObjectType()).FirstOrDefault<DynamicObject>();
					dynamicObject2["ModifyDate"] = dynamicObject3["ModifyDate"];
					dynamicObject2["ExpType_Id"] = dynamicObject3["ExpType_Id"];
					dynamicObject2["ExpType"] = dynamicObject3["ExpType"];
					DynamicObject dynamicObject4 = BusinessDataServiceHelper.Load(base.Context, new object[]
					{
						Convert.ToInt64(dynamicObject3["ExpType_Id"])
					}, formMetadata.BusinessInfo.GetDynamicObjectType()).FirstOrDefault<DynamicObject>();
					if (Convert.ToString(dynamicObject4["SusGather"]) == "True")
					{
						list.Add(string.Format("UPDATE T_ENG_EQUIPMENT SET FISAUTOGATHER='0' WHERE FID='{0}'", Convert.ToInt64(dynamicObject3["EquipmentId_Id"])));
					}
					else
					{
						list.Add(string.Format("UPDATE T_ENG_EQUIPMENT SET FISAUTOGATHER='1' WHERE FID='{0}'", Convert.ToInt64(dynamicObject3["EquipmentId_Id"])));
					}
				}
			}
			DBServiceHelper.ExecuteBatch(base.Context, list);
			this.View.UpdateView("FEntityLog");
		}

		// Token: 0x04000512 RID: 1298
		private string PageLogId;

		// Token: 0x04000513 RID: 1299
		private List<DynamicObject> lstSelectDatas = new List<DynamicObject>();

		// Token: 0x04000514 RID: 1300
		private List<DynamicObject> lstModifyDatas = new List<DynamicObject>();

		// Token: 0x04000515 RID: 1301
		private FormMetadata orgMetaData;

		// Token: 0x04000516 RID: 1302
		private FormMetadata eqmStatusMetadata;

		// Token: 0x04000517 RID: 1303
		private FormMetadata eqmChgStatusLogMetadata;

		// Token: 0x04000518 RID: 1304
		private long currOrgId;

		// Token: 0x04000519 RID: 1305
		private List<long> lstEqmIds;

		// Token: 0x0400051A RID: 1306
		private int _selectRowIndex;

		// Token: 0x0400051B RID: 1307
		private int _selectPreRowIndex;

		// Token: 0x0400051C RID: 1308
		protected FilterParameter filterParam;

		// Token: 0x0400051D RID: 1309
		private string filterString = "";

		// Token: 0x0400051E RID: 1310
		private bool IsFilter;

		// Token: 0x0400051F RID: 1311
		private bool IsOpenByEqm;

		// Token: 0x04000520 RID: 1312
		private DynamicObjectCollection eqmCollection;

		// Token: 0x04000521 RID: 1313
		private List<DynamicObject> lstBomHeadData;
	}
}
