using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args.WizardForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.DynamicForm.PlugIn.WizardForm;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.NetworkCtrl;
using Kingdee.BOS.Core.Validation;
using Kingdee.BOS.KDThread;
using Kingdee.BOS.Log;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.MFG.ENG.MaterialSyncBom;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.Core.MFG.PLN.MrpEntity;
using Kingdee.K3.Core.MFG.Utils;
using Kingdee.K3.MFG.ServiceHelper.ENG;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm.WizardForm
{
	// Token: 0x020000B8 RID: 184
	[Description("物料信息更新同步物料清单")]
	public class MateInfoEditSyncBomWizardPlugin : AbstractWizardFormPlugIn
	{
		// Token: 0x06000D7D RID: 3453 RVA: 0x0009E7FE File Offset: 0x0009C9FE
		protected void ButtonClick_FNext(ButtonClickEventArgs e)
		{
			base.View.CurrentWizardStep.ContainerKey != "";
		}

		// Token: 0x17000096 RID: 150
		// (get) Token: 0x06000D7E RID: 3454 RVA: 0x0009E81B File Offset: 0x0009CA1B
		protected string MateInfoEditFilter_PageId
		{
			get
			{
				return string.Format("{0}_MateInfoEditFilter", base.View.PageId);
			}
		}

		// Token: 0x17000097 RID: 151
		// (get) Token: 0x06000D7F RID: 3455 RVA: 0x0009E832 File Offset: 0x0009CA32
		protected string BomEditSync_PageId
		{
			get
			{
				return string.Format("{0}_BomEditSync", base.View.PageId);
			}
		}

		// Token: 0x17000098 RID: 152
		// (get) Token: 0x06000D80 RID: 3456 RVA: 0x0009E849 File Offset: 0x0009CA49
		protected string Result_PageId
		{
			get
			{
				return string.Format("{0}_EditResultSync", base.View.PageId);
			}
		}

		// Token: 0x06000D81 RID: 3457 RVA: 0x0009E860 File Offset: 0x0009CA60
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
			this.computeId = SequentialGuid.NewGuid().ToString();
			this.useOrgId = Convert.ToInt64(e.Paramter.GetCustomParameter("UseOrgId"));
			this.materialIds = (e.Paramter.GetCustomParameter("MaterialId") as List<string>);
			this.AddButtonClickEventHandler();
			this.AddStepChangingEventHandler();
			this.AddStepChangedEventHandler();
		}

		// Token: 0x06000D82 RID: 3458 RVA: 0x0009E8D8 File Offset: 0x0009CAD8
		public override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
			dynamicFormShowParameter.PageId = this.MateInfoEditFilter_PageId;
			dynamicFormShowParameter.ParentPageId = base.View.PageId;
			dynamicFormShowParameter.OpenStyle.ShowType = 3;
			dynamicFormShowParameter.OpenStyle.TagetKey = "FPanelStep1";
			dynamicFormShowParameter.FormId = "ENG_MATERIALEDITFILTER";
			dynamicFormShowParameter.CustomParams.Add("UseOrgId", this.useOrgId.ToString());
			dynamicFormShowParameter.CustomComplexParams.Add("MaterialIds", this.materialIds);
			base.View.ShowForm(dynamicFormShowParameter);
		}

		// Token: 0x06000D83 RID: 3459 RVA: 0x0009E973 File Offset: 0x0009CB73
		public override void AfterBindData(EventArgs e)
		{
			base.AfterBindData(e);
			base.View.GetControl<Panel>("FProgressPanel").Visible = false;
			base.View.GetControl<ProgressBar>("FProgressBar").Visible = false;
		}

		// Token: 0x06000D84 RID: 3460 RVA: 0x0009E9A8 File Offset: 0x0009CBA8
		public override void WizardStepChanging(WizardStepChangingEventArgs e)
		{
			base.WizardStepChanging(e);
			if (e.OldWizardStep == null)
			{
				return;
			}
			Action<WizardStepChangingEventArgs> action = null;
			string key = string.Format("{0}_{1}", e.OldWizardStep.ContainerKey, e.UpDownEnum);
			if (this._stepChangingEventHandler.TryGetValue(key, out action) && action != null)
			{
				action(e);
			}
		}

		// Token: 0x06000D85 RID: 3461 RVA: 0x0009EA04 File Offset: 0x0009CC04
		public override void WizardStepChanged(WizardStepChangedEventArgs e)
		{
			base.WizardStepChanged(e);
			Action<WizardStepChangedEventArgs> action = null;
			string key = string.Format("{0}_{1}", e.WizardStep.ContainerKey, e.UpDownEnum);
			if (this._stepChangedEventHandler.TryGetValue(key, out action) && action != null)
			{
				action(e);
			}
		}

		// Token: 0x06000D86 RID: 3462 RVA: 0x0009EA58 File Offset: 0x0009CC58
		public override void ButtonClick(ButtonClickEventArgs e)
		{
			base.ButtonClick(e);
			Action<ButtonClickEventArgs> action = null;
			if (this._buttonClickEventHandlers.TryGetValue(e.Key, out action) && action != null)
			{
				action(e);
			}
		}

		// Token: 0x06000D87 RID: 3463 RVA: 0x0009EA90 File Offset: 0x0009CC90
		public override void OnQueryProgressValue(QueryProgressValueEventArgs e)
		{
			base.OnQueryProgressValue(e);
			if (this.catchThreadException)
			{
				string text = string.Empty;
				if (!string.IsNullOrWhiteSpace(this.errorMessage))
				{
					text = string.Format(ResManager.LoadKDString("计算出现错误:{0},请重新进行运算...", "015072000015034", 7, new object[0]), this.errorMessage);
				}
				else
				{
					text = ResManager.LoadKDString("计算出现错误:null,请重新进行运算...", "015072000015035", 7, new object[0]);
				}
				e.Caption = ResManager.LoadKDString(text, "015072000014407", 7, new object[0]);
				e.Value = 100;
				return;
			}
			if (!base.View.GetControl<ProgressBar>("FProgressBar").Visible)
			{
				e.Value = 0;
				e.Caption = "     ";
				return;
			}
			IOperationResult calcProgressRate = MaterialEditSyncBomServiceHelper.GetCalcProgressRate(base.Context, this.computeId);
			MrpProgressContext mrpProgressContext = calcProgressRate.FuncResult as MrpProgressContext;
			if (mrpProgressContext != null)
			{
				int num = Convert.ToInt32(Math.Floor(mrpProgressContext.ProgressValue));
				if (num <= e.Value)
				{
					num = ((e.Value + 1 < 100) ? (e.Value + 1) : e.Value);
				}
				if (num > e.Value || e.Value == 100)
				{
					e.Value = num;
				}
				e.Caption = mrpProgressContext.ProgressMessage;
			}
			if (e.Value >= 100)
			{
				if (this.isSaveSimResult)
				{
					base.View.GetControl<Button>("FCancel").Enabled = false;
					base.View.GetControl<Button>("FFinish").Enabled = true;
				}
				else
				{
					IDynamicFormView view = base.View.GetView(this.BomEditSync_PageId);
					if (!ObjectUtils.IsNullOrEmpty(view))
					{
						view.Refresh();
						base.View.SendDynamicFormAction(view);
					}
					base.View.GetControl<Button>("FPrevious").Enabled = true;
				}
			}
			if (string.IsNullOrWhiteSpace(e.Caption))
			{
				e.Caption = "     ";
			}
		}

		// Token: 0x06000D88 RID: 3464 RVA: 0x0009EC61 File Offset: 0x0009CE61
		public override void BeforeClosed(BeforeClosedEventArgs e)
		{
			base.BeforeClosed(e);
			if (this.simResult != null && this.simResult.FuncResult != null)
			{
				BomBatchEditServiceHelper.ReleaseRunResult(base.Context, (List<NetworkCtrlResult>)this.simResult.FuncResult, this.computeId);
			}
		}

		// Token: 0x06000D89 RID: 3465 RVA: 0x0009ECA0 File Offset: 0x0009CEA0
		public override void EntryButtonCellClick(EntryButtonCellClickEventArgs e)
		{
			DynamicObjectCollection dynamicValue = DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(base.View.Model.DataObject, "TreeEntity", null);
			if (ListUtils.IsEmpty<DynamicObject>(dynamicValue))
			{
				return;
			}
			string dynamicValue2 = DataEntityExtend.GetDynamicValue<string>(dynamicValue[e.Row], "BOMID", null);
			BillShowParameter billShowParameter = new BillShowParameter
			{
				FormId = "ENG_BOM",
				Status = 1,
				PKey = dynamicValue2
			};
			base.View.ShowForm(billShowParameter);
		}

		// Token: 0x06000D8A RID: 3466 RVA: 0x0009ED1A File Offset: 0x0009CF1A
		private void AddButtonClickEventHandler()
		{
			this._buttonClickEventHandlers.AddOrUpdate("FNext", new Action<ButtonClickEventArgs>(this.ButtonClick_FNext), (string key, Action<ButtonClickEventArgs> instance) => instance);
		}

		// Token: 0x06000D8B RID: 3467 RVA: 0x0009ED58 File Offset: 0x0009CF58
		private void AddStepChangingEventHandler()
		{
			this.AddStepChangingEventHandler("FPanelStep1", new Action<WizardStepChangingEventArgs>(this.StepChanging_FPanelStep1_Down), 1);
			this.AddStepChangingEventHandler("FPanelStep2", new Action<WizardStepChangingEventArgs>(this.StepChanging_FPanelStep2_Down), 1);
			this.AddStepChangingEventHandler("FPanelStep2", new Action<WizardStepChangingEventArgs>(this.StepChanging_FPanelStep2_Up), 2);
		}

		// Token: 0x06000D8C RID: 3468 RVA: 0x0009EDB0 File Offset: 0x0009CFB0
		private void AddStepChangingEventHandler(string stepKey, Action<WizardStepChangingEventArgs> func, int stepDirection = 1)
		{
			this._stepChangingEventHandler.AddOrUpdate(string.Format("{0}_{1}", stepKey, stepDirection), func, (string key, Action<WizardStepChangingEventArgs> instance) => instance);
		}

		// Token: 0x06000D8D RID: 3469 RVA: 0x0009EDF0 File Offset: 0x0009CFF0
		private void AddStepChangedEventHandler()
		{
			this.AddStepChangedEventHandler("FPanelStep2", new Action<WizardStepChangedEventArgs>(this.StepChanged_FPanelStep1_Down), 1);
			this.AddStepChangedEventHandler("FPanelStep3", new Action<WizardStepChangedEventArgs>(this.StepChanged_FPanelStep2_Down), 1);
			this.AddStepChangedEventHandler("FPanelStep1", new Action<WizardStepChangedEventArgs>(this.StepChanged_FPanelStep1_Up), 2);
		}

		// Token: 0x06000D8E RID: 3470 RVA: 0x0009EE48 File Offset: 0x0009D048
		private void AddStepChangedEventHandler(string stepKey, Action<WizardStepChangedEventArgs> func, int stepDirection = 1)
		{
			this._stepChangedEventHandler.AddOrUpdate(string.Format("{0}_{1}", stepKey, stepDirection), func, (string key, Action<WizardStepChangedEventArgs> instance) => instance);
		}

		// Token: 0x06000D8F RID: 3471 RVA: 0x0009EEF8 File Offset: 0x0009D0F8
		private void StepChanged_FPanelStep1_Down(WizardStepChangedEventArgs e)
		{
			this.computeId = SequentialGuid.NewGuid().ToString();
			this.catchThreadException = false;
			this.simResult = null;
			ListShowParameter listShowParameter = new ListShowParameter();
			listShowParameter.PageId = this.BomEditSync_PageId;
			listShowParameter.FormId = "ENG_BBEBOM";
			listShowParameter.ListFilterParameter.Filter = string.Format(" FCOMPUTEID = '{0}' ", this.computeId);
			listShowParameter.ListType = 1;
			listShowParameter.IsShowApproved = false;
			listShowParameter.IsShowUsed = false;
			listShowParameter.IsShowFilter = false;
			listShowParameter.IsShowQuickFilter = false;
			listShowParameter.MultiSelect = true;
			listShowParameter.OpenStyle.ShowType = 3;
			listShowParameter.OpenStyle.TagetKey = "FPanelStep2";
			listShowParameter.UseOrgId = this.useOrgId;
			bool dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<bool>(this.mateInfoEditFilterParamter.CustomFilter, "EditMtrlFilter", false);
			if (dynamicObjectItemValue)
			{
				DynamicObjectCollection dynamicObjectItemValue2 = DataEntityExtend.GetDynamicObjectItemValue<DynamicObjectCollection>(this.mateInfoEditFilterParamter.CustomFilter, "MaterialIdChild", null);
				if (!ListUtils.IsEmpty<DynamicObject>(dynamicObjectItemValue2))
				{
					IEnumerable<long> enumerable = from i in dynamicObjectItemValue2
					select DataEntityExtend.GetDynamicObjectItemValue<long>(i, "MaterialIdChild_Id", 0L);
					if (!ListUtils.IsEmpty<long>(enumerable))
					{
						FormMetadata formMetadata = (FormMetadata)MetaDataServiceHelper.Load(base.View.Context, "ENG_BBEBOM", true);
						Entity entity = formMetadata.BusinessInfo.GetEntity("FTreeEntity");
						listShowParameter.ListFilterParameter.Filter = StringUtils.JoinFilterString(listShowParameter.ListFilterParameter.Filter, string.Format(" {0}.FMATERIALID IN ({1})", entity.TableAlias, string.Join<long>(",", enumerable)), "AND");
					}
				}
			}
			base.View.ShowForm(listShowParameter);
			base.View.GetControl<Panel>("FProgressPanel").Visible = true;
			base.View.GetControl<ProgressBar>("FProgressBar").Visible = true;
			base.View.GetControl<ProgressBar>("FProgressBar").Start(1);
			try
			{
				MainWorker.QuequeTask(base.View.Context, new Action(this.RunSimulationBom), delegate(AsynResult result)
				{
					if (!result.Success)
					{
						this.errorMessage = this.FormatException(result.Exception.InnerException);
						this.catchThreadException = true;
						return;
					}
					IDynamicFormView view = base.View.GetView(this.BomEditSync_PageId);
					if (view != null)
					{
						view.Refresh();
						if (base.View != null)
						{
							base.View.SendDynamicFormAction(view);
						}
					}
				});
			}
			catch (Exception ex)
			{
				Logger.Error("MateInfoEditSyncBomWizardPlugin_StepChanged_FPanelStep1_Down", ex.Message, ex);
				throw ex;
			}
		}

		// Token: 0x06000D90 RID: 3472 RVA: 0x0009F13C File Offset: 0x0009D33C
		public override void CustomEvents(CustomEventsArgs e)
		{
			base.CustomEvents(e);
			if (e.EventName == "EntitySelect" && this.isSuccess)
			{
				base.View.GetControl<Button>("FNext").Enabled = true;
				this.isSave = true;
				this.isSuccess = false;
			}
		}

		// Token: 0x06000D91 RID: 3473 RVA: 0x0009F1F8 File Offset: 0x0009D3F8
		private void StepChanged_FPanelStep2_Down(WizardStepChangedEventArgs e)
		{
			base.View.GetControl<ProgressBar>("FProgressBar").SetValue(0);
			(base.View as IDynamicFormViewService).ButtonClick("FNext", "");
			base.View.GetControl<Panel>("FProgressPanel").Visible = true;
			base.View.GetControl<ProgressBar>("FProgressBar").Visible = true;
			base.View.GetControl<ProgressBar>("FProgressBar").Start(1);
			base.View.GetControl<Button>("FPrevious").Enabled = false;
			base.View.GetControl<Button>("FNext").Enabled = false;
			base.View.GetControl<Button>("FCancel").Enabled = false;
			base.View.GetControl<Button>("FFinish").Enabled = false;
			this.computeId = SequentialGuid.NewGuid().ToString();
			this.catchThreadException = false;
			this.saveSimResult = null;
			this.isSaveSimResult = true;
			this.isSave = false;
			try
			{
				MainWorker.QuequeTask(base.View.Context, new Action(this.SaveSimulationBom), delegate(AsynResult result)
				{
					if (!result.Success)
					{
						this.errorMessage = this.FormatException(result.Exception.InnerException);
						base.View.GetControl<Button>("FCancel").Enabled = false;
						base.View.GetControl<Button>("FFinish").Enabled = true;
						this.catchThreadException = true;
						return;
					}
					this.ShowSaveResult();
				});
			}
			catch (Exception ex)
			{
				Logger.Error("MateInfoEditSyncBomWizardPlugin_StepChanged_FPanelStep2_Down", ex.Message, ex);
				throw ex;
			}
		}

		// Token: 0x06000D92 RID: 3474 RVA: 0x0009F360 File Offset: 0x0009D560
		private void StepChanged_FPanelStep1_Up(WizardStepChangedEventArgs e)
		{
			this.CloseChildView(this.BomEditSync_PageId);
			this.isSave = false;
			base.View.GetControl<Panel>("FProgressPanel").Visible = false;
			base.View.GetControl<ProgressBar>("FProgressBar").Visible = false;
			if (this.simResult != null && this.simResult.FuncResult != null)
			{
				BomBatchEditServiceHelper.ReleaseRunResult(base.Context, (List<NetworkCtrlResult>)this.simResult.FuncResult, this.computeId);
			}
		}

		// Token: 0x06000D93 RID: 3475 RVA: 0x0009F3E4 File Offset: 0x0009D5E4
		private void CloseChildView(string pageId)
		{
			IDynamicFormView view = base.View.GetView(pageId);
			if (view != null)
			{
				this.isSave = false;
				view.Model.DataChanged = false;
				view.Close();
				base.View.SendDynamicFormAction(view);
			}
		}

		// Token: 0x06000D94 RID: 3476 RVA: 0x0009F428 File Offset: 0x0009D628
		private void RunSimulationBom()
		{
			try
			{
				MaterialSyncBomOption materialSyncBomOption = new MaterialSyncBomOption();
				materialSyncBomOption.ComputeId = this.computeId;
				materialSyncBomOption.UseOrgId = this.useOrgId;
				materialSyncBomOption.EditFieldKeys = this.canEditFieldKey;
				materialSyncBomOption.OrmFieldKeys = this.canEditFieldProp;
				materialSyncBomOption.SyncEditBomFilter = this.mateInfoEditFilterParamter;
				this.simResult = MaterialEditSyncBomServiceHelper.RunSimulationBom(base.Context, materialSyncBomOption);
				this.ecnBomItem = materialSyncBomOption.EcnBomItem;
				if (!ListUtils.IsEmpty<DynamicObject>(this.simResult.SuccessDataEnity))
				{
					this.isSuccess = true;
				}
			}
			catch (Exception ex)
			{
				Logger.Error("MateInfoEditSyncBomWizardPlugin_RunSimulationBom", ex.Message, ex);
				throw ex;
			}
		}

		// Token: 0x06000D95 RID: 3477 RVA: 0x0009F50C File Offset: 0x0009D70C
		public void SaveSimulationBom()
		{
			try
			{
				IDynamicFormView view = base.View.GetView(this.BomEditSync_PageId);
				List<long> list = new List<long>();
				list = (from i in (view as IListView).SelectedRowsInfo
				select Convert.ToInt64(i.PrimaryKeyValue)).Distinct<long>().ToList<long>();
				List<long> first = (from i in (view as IListView).CurrentPageRowsInfo
				select Convert.ToInt64(i.PrimaryKeyValue)).Distinct<long>().ToList<long>();
				MaterialSyncBomOption materialSyncBomOption = new MaterialSyncBomOption();
				materialSyncBomOption.ComputeId = this.computeId;
				materialSyncBomOption.EditFieldKeys = this.canEditFieldKey;
				materialSyncBomOption.OrmFieldKeys = this.canEditFieldProp;
				materialSyncBomOption.UseOrgId = this.useOrgId;
				materialSyncBomOption.EcnBomItem = this.ecnBomItem;
				materialSyncBomOption.AuthEcnNewPermission = true;
				materialSyncBomOption.SyncEditBomFilter = this.mateInfoEditFilterParamter;
				this.saveSimResult = MaterialEditSyncBomServiceHelper.SaveSimulationBomResult(base.Context, list, materialSyncBomOption, (List<NetworkCtrlResult>)this.simResult.FuncResult);
				this.Bom2EcnDatas = materialSyncBomOption.Bom2EcnContent;
				this.AuthEcnNewPermission = materialSyncBomOption.AuthEcnNewPermission;
				List<long> list2 = first.Except(list).ToList<long>();
				List<object> dBomIds = new List<object>();
				list2.ForEach(delegate(long bomId)
				{
					dBomIds.Add(bomId);
				});
				OperateOption operateOption = OperateOption.Create();
				OperateOptionUtils.SetValidateFlag(operateOption, false);
				FormMetadata formMetadata = (FormMetadata)MetaDataServiceHelper.Load(base.Context, "ENG_BBEBOM", true);
				BusinessDataServiceHelper.Delete(base.Context, formMetadata.BusinessInfo, dBomIds.ToArray(), operateOption, "");
			}
			catch (Exception ex)
			{
				Logger.Error("MateInfoEditSyncBomWizardPlugin_SaveSimulationBom", ex.Message, ex);
				throw ex;
			}
		}

		// Token: 0x06000D96 RID: 3478 RVA: 0x0009F6EC File Offset: 0x0009D8EC
		private string FormatException(Exception ex)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (ex == null)
			{
				return stringBuilder.ToString();
			}
			stringBuilder.AppendFormat("source:{0}\r\n", ex.Source);
			stringBuilder.AppendFormat("message:{0}\r\n", ex.Message);
			stringBuilder.AppendFormat("stacktrace:{0}\r\n", ex.StackTrace);
			if (ex.InnerException != null)
			{
				stringBuilder.AppendLine("\r\ninner exception:");
				stringBuilder.AppendLine(this.FormatException(ex.InnerException));
			}
			return stringBuilder.ToString();
		}

		// Token: 0x06000D97 RID: 3479 RVA: 0x0009F76C File Offset: 0x0009D96C
		private void ShowSaveResult()
		{
			base.View.GetView(this.MateInfoEditFilter_PageId);
			List<DynamicObject> list = new List<DynamicObject>();
			Entity entryEntity = this.Model.BusinessInfo.GetEntryEntity("FTreeEntity");
			DynamicObjectCollection entityDataObject = this.Model.GetEntityDataObject(entryEntity);
			list = DataEntityExtend.GetDynamicObjectItemValue<DynamicObjectCollection>(this.mateInfoEditFilterParamter.CustomFilter, "MaterialIdChild", null).ToList<DynamicObject>();
			int num = 0;
			base.View.Model.DeleteEntryData("FTreeEntity");
			if (this.saveSimResult != null && this.saveSimResult.SuccessDataEnity != null && this.saveSimResult.SuccessDataEnity.Count<DynamicObject>() >= 1)
			{
				foreach (DynamicObject dynamicObject in this.saveSimResult.SuccessDataEnity)
				{
					foreach (DynamicObject dynamicObject2 in list)
					{
						List<long> list2 = DataEntityExtend.GetDynamicObjectColumnValues<long>(DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(dynamicObject, "TreeEntity", null), "MATERIALIDCHILD_Id").ToList<long>();
						long dynamicValue = DataEntityExtend.GetDynamicValue<long>(DataEntityExtend.GetDynamicValue<DynamicObject>(dynamicObject2, "MaterialIdChild", null), "Id", 0L);
						if (list2.Contains(dynamicValue))
						{
							this.CreateNewResultRow(dynamicObject, num, dynamicObject2, entityDataObject);
							num++;
						}
					}
				}
			}
			if (this.saveSimResult != null && !ListUtils.IsEmpty<ValidationErrorInfo>(this.saveSimResult.ValidationErrors))
			{
				foreach (ValidationErrorInfo validationErrorInfo in this.saveSimResult.ValidationErrors)
				{
					base.View.Model.CreateNewEntryRow("FTreeEntity");
					base.View.Model.SetValue("FMaterialNo", string.Empty, num);
					base.View.Model.SetValue("FBillNo", string.Empty, num);
					base.View.Model.SetValue("FEditContent", validationErrorInfo.Message, num);
					num++;
				}
			}
			if (!ListUtils.IsEmpty<DynamicObject>(this.Bom2EcnDatas))
			{
				this.CreateEcnResultRow(this.Bom2EcnDatas, num, list, entityDataObject);
			}
			else if (!this.AuthEcnNewPermission)
			{
				string text = ResManager.LoadKDString("当前用户没有工程变更单的新增权限", "015072000012095", 7, new object[0]);
				base.View.Model.CreateNewEntryRow("FTreeEntity");
				base.View.Model.SetValue("FMaterialNo", string.Empty, num);
				base.View.Model.SetValue("FBillNo", string.Empty, num);
				base.View.Model.SetValue("FEditContent", text, num);
			}
			base.View.UpdateView("FTreeEntity");
		}

		// Token: 0x06000D98 RID: 3480 RVA: 0x0009FA70 File Offset: 0x0009DC70
		private void CreateNewResultRow(DynamicObject bomData, int row, DynamicObject material, DynamicObjectCollection detailDataEntities)
		{
			string arg = string.Empty;
			List<long> list = DataEntityExtend.GetDynamicObjectColumnValues<long>(DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(bomData, "TreeEntity", null), "MaterialIdChild_Id").ToList<long>();
			if (list.Contains(DataEntityExtend.GetDynamicValue<long>(DataEntityExtend.GetDynamicValue<DynamicObject>(material, "MaterialIdChild", null), "Id", 0L)))
			{
				arg = string.Format(ResManager.LoadKDString("物料编码为【{0}】的子项", "015072000014410", 7, new object[0]), DataEntityExtend.GetDynamicValue<string>(DataEntityExtend.GetDynamicValue<DynamicObject>(material, "MaterialIdChild", null), "Number", null));
			}
			string arg2 = ResManager.LoadKDString("修改", "015072000014413", 7, new object[0]);
			string text = string.Format(ResManager.LoadKDString("版本号为【{0}】物料清单已成功{1}{2}；", "015072000014415", 7, new object[0]), bomData["Number"], arg2, arg);
			base.View.Model.CreateNewEntryRow("FTreeEntity");
			base.View.Model.SetValue("FMaterialId", DataEntityExtend.GetDynamicValue<long>(DataEntityExtend.GetDynamicValue<DynamicObject>(material, "MaterialIdChild", null), "Id", 0L), row);
			base.View.Model.SetValue("FBomNo", bomData["Number"], row);
			base.View.Model.SetValue("FEditContent", text, row);
			base.View.Model.SetValue("FBomID", bomData["ID"], row);
		}

		// Token: 0x06000D99 RID: 3481 RVA: 0x0009FBD4 File Offset: 0x0009DDD4
		private void CreateEcnResultRow(List<DynamicObject> bom2EcnData, int row, List<DynamicObject> newMaterials, DynamicObjectCollection detailDataEntities)
		{
			string text = string.Empty;
			foreach (DynamicObject dynamicObject in bom2EcnData)
			{
				text = string.Format(ResManager.LoadKDString("版本号为【{0}】物料清单已成功生成工程变更单", "015072000012096", 7, new object[0]), dynamicObject["Number"]);
				base.View.Model.CreateNewEntryRow("FTreeEntity");
				base.View.Model.SetValue("FEditContent", text, row);
				base.View.Model.SetValue("FBomNo", dynamicObject["Number"], row);
				base.View.Model.SetValue("FBomID", dynamicObject["ID"], row);
				row++;
			}
		}

		// Token: 0x06000D9A RID: 3482 RVA: 0x0009FCFC File Offset: 0x0009DEFC
		private void StepChanging_FPanelStep1_Down(WizardStepChangingEventArgs e)
		{
			IDynamicFormView view = base.View.GetView(this.MateInfoEditFilter_PageId);
			if (!this.ValidateMateInfoFilterView(view))
			{
				e.Cancel = true;
				return;
			}
			DynamicObject dataObject = view.Model.DataObject;
			if (dataObject == null)
			{
				base.View.ShowMessage(ResManager.LoadKDString("请选择使用组织", "0151515153499000013265", 7, new object[0]), 0);
				e.Cancel = true;
			}
			this.canEditFieldKey = (from i in DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(dataObject, "FEntity", null)
			where DataEntityExtend.GetDynamicValue<bool>(i, "IsSync", false)
			select DataEntityExtend.GetDynamicValue<string>(i, "FieldKey", null)).ToList<string>();
			this.canEditFieldProp = (from i in DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(dataObject, "FEntity", null)
			where DataEntityExtend.GetDynamicValue<bool>(i, "IsSync", false)
			select DataEntityExtend.GetDynamicValue<string>(i, "FieldProp", null)).ToList<string>();
			(base.View.GetView(this.MateInfoEditFilter_PageId) as IDynamicFormViewService).ButtonClick("FBtnOK", "");
			object obj = null;
			base.View.Session.TryGetValue("returnData", out obj);
			if (obj is FilterParameter)
			{
				this.mateInfoEditFilterParamter = (obj as FilterParameter);
			}
		}

		// Token: 0x06000D9B RID: 3483 RVA: 0x0009FE80 File Offset: 0x0009E080
		private void StepChanging_FPanelStep2_Down(WizardStepChangingEventArgs e)
		{
			if (!this.isSave)
			{
				e.Cancel = true;
				return;
			}
			IDynamicFormView view = base.View.GetView(this.BomEditSync_PageId);
			if (view == null)
			{
				base.View.ShowMessage(ResManager.LoadKDString("请至少选择一条数据进行保存。", "0151515153499000013266", 7, new object[0]), 0);
				e.Cancel = true;
				return;
			}
			List<string> list = (from i in (view as IListView).CurrentPageRowsInfo
			where i.Selected
			select i.PrimaryKeyValue).ToList<string>();
			if (list == null || list.Count < 1)
			{
				base.View.ShowMessage(ResManager.LoadKDString("请至少选择一条数据进行保存。", "0151515153499000013266", 7, new object[0]), 0);
				e.Cancel = true;
			}
		}

		// Token: 0x06000D9C RID: 3484 RVA: 0x0009FF66 File Offset: 0x0009E166
		private void StepChanging_FPanelStep2_Up(WizardStepChangingEventArgs e)
		{
			this.CloseChildView(this.BomEditSync_PageId);
		}

		// Token: 0x06000D9D RID: 3485 RVA: 0x0009FF74 File Offset: 0x0009E174
		private bool ValidateMateInfoFilterView(IDynamicFormView materialFilterView)
		{
			if (MFGBillUtil.GetValue<long>(materialFilterView.Model, "FOrgId", -1, 0L, null) == 0L)
			{
				base.View.ShowMessage(ResManager.LoadKDString("请录入使用组织！", "015072000014416", 7, new object[0]), 0);
				return false;
			}
			return true;
		}

		// Token: 0x04000636 RID: 1590
		protected string computeId = string.Empty;

		// Token: 0x04000637 RID: 1591
		private long useOrgId;

		// Token: 0x04000638 RID: 1592
		private List<string> materialIds = new List<string>();

		// Token: 0x04000639 RID: 1593
		private List<string> canEditFieldKey;

		// Token: 0x0400063A RID: 1594
		private List<string> canEditFieldProp;

		// Token: 0x0400063B RID: 1595
		protected FilterParameter mateInfoEditFilterParamter = new FilterParameter();

		// Token: 0x0400063C RID: 1596
		private bool catchThreadException;

		// Token: 0x0400063D RID: 1597
		private string errorMessage = string.Empty;

		// Token: 0x0400063E RID: 1598
		private bool isSaveSimResult;

		// Token: 0x0400063F RID: 1599
		protected bool isSave;

		// Token: 0x04000640 RID: 1600
		protected bool isSuccess;

		// Token: 0x04000641 RID: 1601
		private IOperationResult simResult;

		// Token: 0x04000642 RID: 1602
		private IOperationResult saveSimResult;

		// Token: 0x04000643 RID: 1603
		private List<DynamicObject> ecnBomItem;

		// Token: 0x04000644 RID: 1604
		private List<DynamicObject> Bom2EcnDatas;

		// Token: 0x04000645 RID: 1605
		private bool AuthEcnNewPermission = true;

		// Token: 0x04000646 RID: 1606
		private ConcurrentDictionary<string, Action<ButtonClickEventArgs>> _buttonClickEventHandlers = new ConcurrentDictionary<string, Action<ButtonClickEventArgs>>(new IgnoreCaseStringComparer());

		// Token: 0x04000647 RID: 1607
		private ConcurrentDictionary<string, Action<WizardStepChangingEventArgs>> _stepChangingEventHandler = new ConcurrentDictionary<string, Action<WizardStepChangingEventArgs>>(new IgnoreCaseStringComparer());

		// Token: 0x04000648 RID: 1608
		private ConcurrentDictionary<string, Action<WizardStepChangedEventArgs>> _stepChangedEventHandler = new ConcurrentDictionary<string, Action<WizardStepChangedEventArgs>>(new IgnoreCaseStringComparer());
	}
}
