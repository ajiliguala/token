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
using Kingdee.BOS.Orm;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.MFG.ENG.BomBatchEdit;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.Core.MFG.PLN.MrpEntity;
using Kingdee.K3.Core.MFG.Utils;
using Kingdee.K3.MFG.Common.BusinessEntity.ENG;
using Kingdee.K3.MFG.ServiceHelper;
using Kingdee.K3.MFG.ServiceHelper.ENG;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm.WizardForm
{
	// Token: 0x020000B7 RID: 183
	[Description("BOM批量维护插件")]
	public class BomBatchEditWizardPlugIn : AbstractWizardFormPlugIn
	{
		// Token: 0x06000D38 RID: 3384 RVA: 0x0009BBAD File Offset: 0x00099DAD
		protected void ButtonClick_FNext(ButtonClickEventArgs e)
		{
			base.View.CurrentWizardStep.ContainerKey != "";
		}

		// Token: 0x06000D39 RID: 3385 RVA: 0x0009BBCA File Offset: 0x00099DCA
		protected void ButtonClick_FLog(ButtonClickEventArgs e)
		{
			base.View.ShowErrMessage(this._errorMessage, "", 0);
		}

		// Token: 0x17000091 RID: 145
		// (get) Token: 0x06000D3A RID: 3386 RVA: 0x0009BBE3 File Offset: 0x00099DE3
		protected string BBEFilter_PageId
		{
			get
			{
				return string.Format("{0}_BBEFilter", base.View.PageId);
			}
		}

		// Token: 0x17000092 RID: 146
		// (get) Token: 0x06000D3B RID: 3387 RVA: 0x0009BBFA File Offset: 0x00099DFA
		protected string BBEBomFilter_PageId
		{
			get
			{
				return string.Format("{0}_BBEBomFilter", base.View.PageId);
			}
		}

		// Token: 0x17000093 RID: 147
		// (get) Token: 0x06000D3C RID: 3388 RVA: 0x0009BC11 File Offset: 0x00099E11
		protected string BBEEidtContent_PageId
		{
			get
			{
				return string.Format("{0}_BBEChildEidt", base.View.PageId);
			}
		}

		// Token: 0x17000094 RID: 148
		// (get) Token: 0x06000D3D RID: 3389 RVA: 0x0009BC28 File Offset: 0x00099E28
		protected string BBEBom_PageId
		{
			get
			{
				return string.Format("{0}_BBEBom", base.View.PageId);
			}
		}

		// Token: 0x17000095 RID: 149
		// (get) Token: 0x06000D3E RID: 3390 RVA: 0x0009BC3F File Offset: 0x00099E3F
		protected string BBESynFilter_PageId
		{
			get
			{
				return string.Format("{0}_BBESynFilter", base.View.PageId);
			}
		}

		// Token: 0x06000D3F RID: 3391 RVA: 0x0009BC58 File Offset: 0x00099E58
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
			this._computeId = SequentialGuid.NewGuid().ToString();
			this.AddButtonClickEventHandler();
			this.AddStepChangingEventHandler();
			this.AddStepChangedEventHandler();
		}

		// Token: 0x06000D40 RID: 3392 RVA: 0x0009BC98 File Offset: 0x00099E98
		public override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
			dynamicFormShowParameter.PageId = this.BBEFilter_PageId;
			dynamicFormShowParameter.ParentPageId = base.View.PageId;
			dynamicFormShowParameter.OpenStyle.ShowType = 3;
			dynamicFormShowParameter.OpenStyle.TagetKey = "FPanelStep1";
			dynamicFormShowParameter.FormId = "ENG_BOMBATCHEDITFILTER";
			base.View.ShowForm(dynamicFormShowParameter);
		}

		// Token: 0x06000D41 RID: 3393 RVA: 0x0009BD02 File Offset: 0x00099F02
		public override void AfterBindData(EventArgs e)
		{
			base.AfterBindData(e);
			base.View.GetControl<Panel>("FPanelProg").Visible = false;
			base.View.GetControl<Button>("FLog").Enabled = false;
		}

		// Token: 0x06000D42 RID: 3394 RVA: 0x0009BD38 File Offset: 0x00099F38
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

		// Token: 0x06000D43 RID: 3395 RVA: 0x0009BD94 File Offset: 0x00099F94
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

		// Token: 0x06000D44 RID: 3396 RVA: 0x0009BDE8 File Offset: 0x00099FE8
		public override void ButtonClick(ButtonClickEventArgs e)
		{
			base.ButtonClick(e);
			Action<ButtonClickEventArgs> action = null;
			if (this._buttonClickEventHandlers.TryGetValue(e.Key, out action) && action != null)
			{
				action(e);
			}
		}

		// Token: 0x06000D45 RID: 3397 RVA: 0x0009BE20 File Offset: 0x0009A020
		public override void OnQueryProgressValue(QueryProgressValueEventArgs e)
		{
			base.OnQueryProgressValue(e);
			if (this._catchThreadException)
			{
				string text = string.Empty;
				if (!string.IsNullOrWhiteSpace(this._errorMessage))
				{
					text = string.Format(ResManager.LoadKDString("计算出现错误:{0},请重新进行运算...", "015072000015034", 7, new object[0]), this._errorMessage);
				}
				else
				{
					text = ResManager.LoadKDString("计算出现错误:null,请重新进行运算...", "015072000015035", 7, new object[0]);
				}
				e.Caption = ResManager.LoadKDString(text, "015072000014407", 7, new object[0]);
				e.Value = 100;
				return;
			}
			if (!base.View.GetControl<Panel>("FPanelProg").Visible)
			{
				e.Value = 0;
				e.Caption = "     ";
				return;
			}
			IOperationResult calcProgressRate = BomBatchEditServiceHelper.GetCalcProgressRate(base.Context, this._computeId);
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
				if (this._isSaveSimResult)
				{
					base.View.GetControl<Button>("FCancel").Enabled = false;
					base.View.GetControl<Button>("FFinish").Enabled = true;
					this.ShowSaveResult();
				}
				else
				{
					IDynamicFormView view = base.View.GetView(this.BBEBom_PageId);
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

		// Token: 0x06000D46 RID: 3398 RVA: 0x0009BFFA File Offset: 0x0009A1FA
		public override void BeforeClosed(BeforeClosedEventArgs e)
		{
			base.BeforeClosed(e);
			if (this._simResultResult != null && this._simResultResult.FuncResult != null)
			{
				BomBatchEditServiceHelper.ReleaseRunResult(base.Context, (List<NetworkCtrlResult>)this._simResultResult.FuncResult, this._computeId);
			}
		}

		// Token: 0x06000D47 RID: 3399 RVA: 0x0009C040 File Offset: 0x0009A240
		private void AddButtonClickEventHandler()
		{
			this._buttonClickEventHandlers.AddOrUpdate("FNext", new Action<ButtonClickEventArgs>(this.ButtonClick_FNext), (string key, Action<ButtonClickEventArgs> instance) => instance);
			this._buttonClickEventHandlers.AddOrUpdate("FLog", new Action<ButtonClickEventArgs>(this.ButtonClick_FLog), (string key, Action<ButtonClickEventArgs> instance) => instance);
		}

		// Token: 0x06000D48 RID: 3400 RVA: 0x0009C0C4 File Offset: 0x0009A2C4
		private void AddStepChangingEventHandler()
		{
			this.AddStepChangingEventHandler("FPanelStep1", new Action<WizardStepChangingEventArgs>(this.StepChanging_FPanelStep1), 1);
			this.AddStepChangingEventHandler("FPanelStep2", new Action<WizardStepChangingEventArgs>(this.StepChanging_FPanelStep2), 1);
			this.AddStepChangingEventHandler("FPanelStep3", new Action<WizardStepChangingEventArgs>(this.StepChanging_FPanelStep3), 1);
			this.AddStepChangingEventHandler("FPanelStep2", new Action<WizardStepChangingEventArgs>(this.StepChanging_FPanelStep2_Up), 2);
		}

		// Token: 0x06000D49 RID: 3401 RVA: 0x0009C134 File Offset: 0x0009A334
		private void AddStepChangingEventHandler(string stepKey, Action<WizardStepChangingEventArgs> func, int stepDirection = 1)
		{
			this._stepChangingEventHandler.AddOrUpdate(string.Format("{0}_{1}", stepKey, stepDirection), func, (string key, Action<WizardStepChangingEventArgs> instance) => instance);
		}

		// Token: 0x06000D4A RID: 3402 RVA: 0x0009C174 File Offset: 0x0009A374
		private void AddStepChangedEventHandler()
		{
			this.AddStepChangedEventHandler("FPanelStep2", new Action<WizardStepChangedEventArgs>(this.StepChanged_FPanelStep1_Down), 1);
			this.AddStepChangedEventHandler("FPanelStep3", new Action<WizardStepChangedEventArgs>(this.StepChanged_FPanelStep2_Down), 1);
			this.AddStepChangedEventHandler("FPanelStep6", new Action<WizardStepChangedEventArgs>(this.StepChanged_FPanelStep3_Down), 1);
			this.AddStepChangedEventHandler("FPanelStep1", new Action<WizardStepChangedEventArgs>(this.StepChanged_FPanelStep2_Up), 2);
			this.AddStepChangedEventHandler("FPanelStep2", new Action<WizardStepChangedEventArgs>(this.StepChanged_FPanelStep3_Up), 2);
			this.AddStepChangedEventHandler("FPanelStep3", new Action<WizardStepChangedEventArgs>(this.StepChanged_FPanelStep4_Up), 2);
			this.AddStepChangedEventHandler("FPanelStep4", new Action<WizardStepChangedEventArgs>(this.StepChanged_FPanelStep5_Up), 2);
			this.AddStepChangedEventHandler("FPanelStep5", new Action<WizardStepChangedEventArgs>(this.StepChanged_FPanelStep6_Up), 2);
		}

		// Token: 0x06000D4B RID: 3403 RVA: 0x0009C244 File Offset: 0x0009A444
		private void AddStepChangedEventHandler(string stepKey, Action<WizardStepChangedEventArgs> func, int stepDirection = 1)
		{
			this._stepChangedEventHandler.AddOrUpdate(string.Format("{0}_{1}", stepKey, stepDirection), func, (string key, Action<WizardStepChangedEventArgs> instance) => instance);
		}

		// Token: 0x06000D4C RID: 3404 RVA: 0x0009C284 File Offset: 0x0009A484
		public override void EntryButtonCellClick(EntryButtonCellClickEventArgs e)
		{
			DynamicObjectCollection dynamicValue = DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(base.View.Model.DataObject, "ResultEntity", null);
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

		// Token: 0x06000D4D RID: 3405 RVA: 0x0009C358 File Offset: 0x0009A558
		private void StepChanged_FPanelStep1_Down(WizardStepChangedEventArgs e)
		{
			IDynamicFormView view = base.View.GetView(this.BBEFilter_PageId);
			this._UseOrgId = MFGBillUtil.GetValue<long>(view.Model, "FOrgId", -1, 0L, null);
			this._IsSynPPbom = MFGBillUtil.GetValue<bool>(view.Model, "FSynToPpbom", -1, false, null);
			this._IsSynPLBom = MFGBillUtil.GetValue<bool>(view.Model, "FSynToPlbom", -1, false, null);
			this._EditType = MFGBillUtil.GetValue<string>(view.Model, "FEditType", -1, string.Empty, null);
			this._repItemAlterPolicy = MFGBillUtil.GetValue<string>(view.Model, "FRepItemAlterPolicy", -1, "0", null);
			this._ecnChangeType = MFGBillUtil.GetValue<string>(view.Model, "FEcnChangeType", -1, "1", null);
			string text = (this._EditType == "b") ? "FChangeEntity" : "FEntity";
			string opeKey = (this._EditType == "b") ? "IsCanChange" : "IsCanEdit";
			string appendKey = (this._EditType == "b") ? "IsCanAppendC" : "IsCanAppend";
			string text2 = (this._EditType == "b") ? "ChangeFieldKey" : "FieldKey";
			string text3 = (this._EditType == "b") ? "ChangeFieldProp" : "FFIELDPROP";
			Entity entity = view.BusinessInfo.GetEntity(text);
			DynamicObjectCollection entityDataObject = view.Model.GetEntityDataObject(entity);
			IEnumerable<DynamicObject> enumerable = from row in entityDataObject
			where Convert.ToBoolean(row[opeKey])
			select row;
			IEnumerable<DynamicObject> enumerable2 = from row in entityDataObject
			where Convert.ToBoolean(row[appendKey])
			select row;
			this._CanEditFieldKeys.Clear();
			this._CanEditFieldOrmKeys.Clear();
			this._CanAppendFieldKeys.Clear();
			this._CanAppendFieldOrmKeys.Clear();
			foreach (DynamicObject dynamicObject in enumerable)
			{
				this._CanEditFieldKeys.Add(Convert.ToString(dynamicObject[text2]));
				this._CanEditFieldOrmKeys.AddRange(Convert.ToString(dynamicObject[text3]).Split(new char[]
				{
					'*'
				}).ToList<string>());
			}
			foreach (DynamicObject dynamicObject2 in enumerable2)
			{
				if (!(dynamicObject2[text2].ToString().Split(new char[]
				{
					'*'
				}).First<string>() == "FBOMCHILDLOTBASEDQTY"))
				{
					this._CanAppendFieldKeys.Add(Convert.ToString(dynamicObject2[text2]));
					this._CanAppendFieldOrmKeys.AddRange(Convert.ToString(dynamicObject2[text3]).Split(new char[]
					{
						'*'
					}).ToList<string>());
				}
			}
			if (this._EditType == "b")
			{
				IEnumerable<DynamicObject> enumerable3 = from row in entityDataObject
				where !Convert.ToBoolean(row[opeKey])
				select row;
				this._CanNotChangeFieldKeys.Clear();
				this._CanNotChangeFieldOrmKeys.Clear();
				foreach (DynamicObject dynamicObject3 in enumerable3)
				{
					this._CanNotChangeFieldKeys.Add(Convert.ToString(dynamicObject3[text2]));
					this._CanNotChangeFieldOrmKeys.AddRange(Convert.ToString(dynamicObject3[text3]).Split(new char[]
					{
						'*'
					}).ToList<string>());
				}
			}
			IDynamicFormView view2 = base.View.GetView(this.BBEBomFilter_PageId);
			if (view2 != null)
			{
				view2.OpenParameter.SetCustomParameter("UseOrgId", this._UseOrgId.ToString());
				view2.Refresh();
				base.View.SendDynamicFormAction(view2);
			}
			else
			{
				FilterShowParameter filterShowParameter = new FilterShowParameter();
				filterShowParameter.PageId = this.BBEBomFilter_PageId;
				filterShowParameter.ParentPageId = base.View.PageId;
				filterShowParameter.OpenStyle.ShowType = 3;
				filterShowParameter.OpenStyle.TagetKey = "FPanelFilterIn2";
				filterShowParameter.FormId = "ENG_BBEBOMFILTER";
				filterShowParameter.BillFormId = "ENG_BOM";
				filterShowParameter.OpenStyle.CacheId = filterShowParameter.FormId;
				filterShowParameter.CustomParams.Add("UseOrgId", this._UseOrgId.ToString());
				base.View.ShowForm(filterShowParameter);
			}
			this.CloseChildView(this.BBEEidtContent_PageId);
			DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
			dynamicFormShowParameter.PageId = this.BBEEidtContent_PageId;
			if (this._EditType == "a" || this._EditType == "b" || this._EditType == "c" || this._EditType == "d")
			{
				dynamicFormShowParameter.FormId = "ENG_BBECONCHILD";
			}
			if (this._EditType == "m")
			{
				dynamicFormShowParameter.FormId = "ENG_BBECONPARENT";
			}
			dynamicFormShowParameter.ParentPageId = base.View.PageId;
			dynamicFormShowParameter.OpenStyle.ShowType = 3;
			dynamicFormShowParameter.OpenStyle.TagetKey = "FPanelEditAreaIn2";
			dynamicFormShowParameter.CustomParams.Add("UseOrgId", this._UseOrgId.ToString());
			dynamicFormShowParameter.CustomParams.Add("EditType", this._EditType);
			dynamicFormShowParameter.CustomParams.Add("EditPageId", this.BBEBomFilter_PageId);
			if (this._EditType == "d" || this._EditType == "m" || this._EditType == "b")
			{
				if (base.View.Session.ContainsKey("CanEditFieldKeys"))
				{
					base.View.Session["CanEditFieldKeys"] = this._CanEditFieldKeys;
				}
				else
				{
					base.View.Session.Add("CanEditFieldKeys", this._CanEditFieldKeys);
				}
				if (this._EditType == "d" || this._EditType == "b")
				{
					if (base.View.Session.ContainsKey("CanAppendFieldKeys"))
					{
						base.View.Session["CanAppendFieldKeys"] = this._CanAppendFieldKeys;
					}
					else
					{
						base.View.Session.Add("CanAppendFieldKeys", this._CanAppendFieldKeys);
					}
				}
			}
			base.View.ShowForm(dynamicFormShowParameter, delegate(FormResult finishedShow)
			{
				base.View.GetControl<Button>("FNext").Enabled = true;
			});
		}

		// Token: 0x06000D4E RID: 3406 RVA: 0x0009CA58 File Offset: 0x0009AC58
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

		// Token: 0x06000D4F RID: 3407 RVA: 0x0009CB44 File Offset: 0x0009AD44
		private void StepChanged_FPanelStep2_Down(WizardStepChangedEventArgs e)
		{
			this._computeId = SequentialGuid.NewGuid().ToString();
			this._catchThreadException = false;
			this._simResultResult = null;
			FormMetadata formMetadata = (FormMetadata)MetaDataServiceHelper.Load(base.View.Context, "ENG_BBEBOM", true);
			Entity entity = formMetadata.BusinessInfo.GetEntity("FTreeEntity");
			ListShowParameter listShowParameter = new ListShowParameter();
			listShowParameter.PageId = this.BBEBom_PageId;
			listShowParameter.FormId = "ENG_BBEBOM";
			listShowParameter.ListFilterParameter.Filter = string.Format(" FCOMPUTEID = '{0}' ", this._computeId);
			listShowParameter.ListType = 1;
			listShowParameter.IsShowApproved = false;
			listShowParameter.IsShowUsed = false;
			listShowParameter.IsShowFilter = false;
			listShowParameter.IsShowQuickFilter = false;
			listShowParameter.MultiSelect = true;
			listShowParameter.OpenStyle.ShowType = 3;
			listShowParameter.OpenStyle.TagetKey = "FPanelStep3";
			listShowParameter.UseOrgId = this._UseOrgId;
			listShowParameter.ListFilterParameter.OrderBy = string.Format("FNUMBER,{0}.FSeq ASC", entity.TableAlias);
			if (this._EditType == "a" || this._EditType == "b" || this._EditType == "d")
			{
				bool flag = Convert.ToBoolean(this._bomFilterParamter.CustomFilter["EditMtrlFilter"]);
				if (flag)
				{
					DynamicObject dataObject = base.View.GetView(this.BBEEidtContent_PageId).Model.DataObject;
					DynamicObjectCollection dynamicObjectCollection = dataObject["TreeEntity"] as DynamicObjectCollection;
					if (!ListUtils.IsEmpty<DynamicObject>(dynamicObjectCollection))
					{
						List<long> values = (from w in dynamicObjectCollection
						where Convert.ToInt64(w["MATERIALIDCHILD_Id"]) != 0L
						select w into s
						select Convert.ToInt64(s["MATERIALIDCHILD_Id"])).ToList<long>();
						IRegularFilterParameter listFilterParameter = listShowParameter.ListFilterParameter;
						listFilterParameter.Filter += string.Format(" AND {0}.FMATERIALID IN ({1}) ", entity.TableAlias, string.Join<long>(",", values));
					}
				}
			}
			base.View.ShowForm(listShowParameter);
			base.View.GetControl<Panel>("FPanelProg").Visible = true;
			base.View.GetControl<ProgressBar>("FProgressBar").Visible = true;
			base.View.GetControl<ProgressBar>("FProgressBar").Start(1);
			base.View.GetControl<Button>("FPrevious").Enabled = false;
			base.View.GetControl<Button>("FNext").Enabled = false;
			MainWorker.QuequeTask(base.View.Context, new Action(this.RunSimulationBom), delegate(AsynResult result)
			{
				if (!result.Success)
				{
					this._errorMessage = this.FormatException(result.Exception.InnerException);
					base.View.GetControl<Button>("FLog").Enabled = true;
					this._catchThreadException = true;
				}
			});
			IDynamicFormView view = base.View.GetView(this.BBEBom_PageId);
			if (view != null)
			{
				view.Refresh();
				base.View.SendDynamicFormAction(view);
			}
		}

		// Token: 0x06000D50 RID: 3408 RVA: 0x0009CE2C File Offset: 0x0009B02C
		public override void CustomEvents(CustomEventsArgs e)
		{
			base.CustomEvents(e);
			if (e.EventName == "EntitySelect" && this._IsSuccess)
			{
				base.View.GetControl<Button>("FNext").Enabled = true;
				this._IsSave = true;
				this._IsSuccess = false;
			}
		}

		// Token: 0x06000D51 RID: 3409 RVA: 0x0009CEF8 File Offset: 0x0009B0F8
		private void StepChanged_FPanelStep3_Down(WizardStepChangedEventArgs e)
		{
			base.View.GetControl<ProgressBar>("FProgressBar").SetValue(0);
			(base.View as IDynamicFormViewService).ButtonClick("FNext", "");
			base.View.GetControl<Panel>("FPanelProg").Visible = true;
			base.View.GetControl<ProgressBar>("FProgressBar").Start(1);
			base.View.GetControl<Button>("FPrevious").Enabled = false;
			base.View.GetControl<Button>("FNext").Enabled = false;
			base.View.GetControl<Button>("FCancel").Enabled = false;
			base.View.GetControl<Button>("FFinish").Enabled = false;
			this._computeId = SequentialGuid.NewGuid().ToString();
			this._catchThreadException = false;
			this._saveSimResult = null;
			this._isSaveSimResult = true;
			this._IsSave = false;
			MainWorker.QuequeTask(base.View.Context, new Action(this.SaveSimulationBom), delegate(AsynResult result)
			{
				if (!result.Success)
				{
					this._errorMessage = this.FormatException(result.Exception.InnerException);
					base.View.GetControl<Button>("FLog").Enabled = true;
					base.View.GetControl<Button>("FCancel").Enabled = false;
					base.View.GetControl<Button>("FFinish").Enabled = true;
					this._catchThreadException = true;
				}
			});
		}

		// Token: 0x06000D52 RID: 3410 RVA: 0x0009D01B File Offset: 0x0009B21B
		private void StepChanged_FPanelStep4_Down(WizardStepChangedEventArgs e)
		{
			(base.View as IDynamicFormViewService).ButtonClick("FNext", "");
		}

		// Token: 0x06000D53 RID: 3411 RVA: 0x0009D037 File Offset: 0x0009B237
		private void StepChanged_FPanelStep5_Down(WizardStepChangedEventArgs e)
		{
		}

		// Token: 0x06000D54 RID: 3412 RVA: 0x0009D039 File Offset: 0x0009B239
		private void StepChanged_FPanelStep2_Up(WizardStepChangedEventArgs e)
		{
			this.CloseChildView(this.BBEEidtContent_PageId);
			this._IsSave = false;
		}

		// Token: 0x06000D55 RID: 3413 RVA: 0x0009D050 File Offset: 0x0009B250
		private void StepChanged_FPanelStep3_Up(WizardStepChangedEventArgs e)
		{
			this.CloseChildView(this.BBEBom_PageId);
			this._IsSave = false;
			base.View.GetControl<Panel>("FPanelProg").Visible = false;
			base.View.GetControl<Button>("FNext").Text = ResManager.LoadKDString("下一步", "015072000014409", 7, new object[0]);
			base.View.GetControl<Button>("FNext").Enabled = true;
			base.View.GetControl<ProgressBar>("FProgressBar").SetValue(0);
			if (this._simResultResult != null && this._simResultResult.FuncResult != null)
			{
				BomBatchEditServiceHelper.ReleaseRunResult(base.Context, (List<NetworkCtrlResult>)this._simResultResult.FuncResult, this._computeId);
			}
		}

		// Token: 0x06000D56 RID: 3414 RVA: 0x0009D118 File Offset: 0x0009B318
		private void StepChanged_FPanelStep4_Up(WizardStepChangedEventArgs e)
		{
			this.CloseChildView(this.BBESynFilter_PageId);
			this._IsSave = false;
		}

		// Token: 0x06000D57 RID: 3415 RVA: 0x0009D12D File Offset: 0x0009B32D
		private void StepChanged_FPanelStep5_Up(WizardStepChangedEventArgs e)
		{
			this._IsSave = false;
		}

		// Token: 0x06000D58 RID: 3416 RVA: 0x0009D136 File Offset: 0x0009B336
		private void StepChanged_FPanelStep6_Up(WizardStepChangedEventArgs e)
		{
			this._IsSave = false;
		}

		// Token: 0x06000D59 RID: 3417 RVA: 0x0009D140 File Offset: 0x0009B340
		private void CloseChildView(string pageId)
		{
			IDynamicFormView view = base.View.GetView(pageId);
			if (view != null)
			{
				this._IsSave = false;
				view.Model.DataChanged = false;
				view.Close();
				base.View.SendDynamicFormAction(view);
			}
		}

		// Token: 0x06000D5A RID: 3418 RVA: 0x0009D184 File Offset: 0x0009B384
		private void RunSimulationBom()
		{
			BomBatchEditOption bomBatchEditOption = new BomBatchEditOption();
			bomBatchEditOption.ComputeId = this._computeId;
			bomBatchEditOption.EditType = this._EditType;
			bomBatchEditOption.EditFieldKeys = this._CanEditFieldKeys;
			bomBatchEditOption.EditBomScopeFilter = this._bomFilterParamter;
			bomBatchEditOption.EditContent = base.View.GetView(this.BBEEidtContent_PageId).Model.DataObject;
			bomBatchEditOption.UseOrgId = this._UseOrgId;
			bomBatchEditOption.repItemAlterPolicy = (ObjectUtils.IsNullOrEmptyOrWhiteSpace(this._repItemAlterPolicy) ? "0" : this._repItemAlterPolicy);
			bomBatchEditOption.ecnChangeType = (ObjectUtils.IsNullOrEmptyOrWhiteSpace(this._ecnChangeType) ? "1" : this._ecnChangeType);
			bomBatchEditOption.OrmAppendFieldKey = this._CanAppendFieldOrmKeys.Distinct<string>().ToList<string>();
			if (this._EditType == "b")
			{
				bomBatchEditOption.OrmFieldKeys = this._CanNotChangeFieldOrmKeys.Distinct<string>().ToList<string>();
				bomBatchEditOption.OrmChangeFieldKey = this._CanEditFieldOrmKeys.Distinct<string>().ToList<string>();
			}
			else
			{
				bomBatchEditOption.OrmFieldKeys = this._CanEditFieldOrmKeys.Distinct<string>().ToList<string>();
			}
			this._simResultResult = BomBatchEditServiceHelper.RunSimulationBom(base.Context, null, bomBatchEditOption);
			this._ecnBomItem = bomBatchEditOption.ecnBomItems;
			if (!ListUtils.IsEmpty<DynamicObject>(this._simResultResult.SuccessDataEnity))
			{
				this._IsSuccess = true;
			}
		}

		// Token: 0x06000D5B RID: 3419 RVA: 0x0009D310 File Offset: 0x0009B510
		private void SaveSimulationBom()
		{
			IDynamicFormView view = base.View.GetView(this.BBEBom_PageId);
			List<long> list = new List<long>();
			list = (from c in (view as IListView).SelectedRowsInfo
			select Convert.ToInt64(c.PrimaryKeyValue)).Distinct<long>().ToList<long>();
			List<long> first = (from c in (view as IListView).CurrentPageRowsInfo
			select Convert.ToInt64(c.PrimaryKeyValue)).Distinct<long>().ToList<long>();
			BomBatchEditOption bomBatchEditOption = new BomBatchEditOption();
			bomBatchEditOption.ComputeId = this._computeId;
			bomBatchEditOption.EditType = this._EditType;
			bomBatchEditOption.EditFieldKeys = this._CanEditFieldKeys;
			bomBatchEditOption.EditBomScopeFilter = this._bomFilterParamter;
			bomBatchEditOption.EditContent = base.View.GetView(this.BBEEidtContent_PageId).Model.DataObject;
			bomBatchEditOption.UseOrgId = this._UseOrgId;
			bomBatchEditOption.ecnBomItems = this._ecnBomItem;
			bomBatchEditOption.authEcnNewPermission = true;
			bomBatchEditOption.repItemAlterPolicy = (ObjectUtils.IsNullOrEmptyOrWhiteSpace(this._repItemAlterPolicy) ? "0" : this._repItemAlterPolicy);
			bomBatchEditOption.ecnChangeType = (ObjectUtils.IsNullOrEmptyOrWhiteSpace(this._ecnChangeType) ? "1" : this._ecnChangeType);
			bomBatchEditOption.OrmAppendFieldKey = this._CanAppendFieldOrmKeys.Distinct<string>().ToList<string>();
			if (this._EditType == "b")
			{
				bomBatchEditOption.OrmFieldKeys = this._CanNotChangeFieldOrmKeys.Distinct<string>().ToList<string>();
				bomBatchEditOption.OrmChangeFieldKey = this._CanEditFieldOrmKeys.Distinct<string>().ToList<string>();
			}
			else
			{
				bomBatchEditOption.OrmFieldKeys = this._CanEditFieldOrmKeys.Distinct<string>().ToList<string>();
			}
			this._saveSimResult = BomBatchEditServiceHelper.SaveSimulationBomResult(base.Context, list, bomBatchEditOption, (List<NetworkCtrlResult>)this._simResultResult.FuncResult);
			this._bom2EcnDatas = bomBatchEditOption.Bom2EcnContent;
			this._authEcnNewPermission = bomBatchEditOption.authEcnNewPermission;
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

		// Token: 0x06000D5C RID: 3420 RVA: 0x0009D5AC File Offset: 0x0009B7AC
		private void ShowSaveResult()
		{
			IDynamicFormView view = base.View.GetView(this.BBEEidtContent_PageId);
			List<DynamicObject> newMaterials = new List<DynamicObject>();
			string text = string.Empty;
			if (this._EditType == "a" || this._EditType == "b" || this._EditType == "c" || this._EditType == "d")
			{
				text = "FEditMaterialId";
				Entity entity = view.BusinessInfo.GetEntity("FTreeEntity");
				DynamicObjectCollection entityDataObject = view.Model.GetEntityDataObject(entity);
				if (!ListUtils.IsEmpty<DynamicObject>(entityDataObject))
				{
					newMaterials = (from c in entityDataObject
					where c["MATERIALIDCHILD"] != null
					select (DynamicObject)c["MATERIALIDCHILD"]).ToList<DynamicObject>();
				}
			}
			DynamicObject value = MFGBillUtil.GetValue<DynamicObject>(view.Model, text, -1, null, null);
			int num = 0;
			base.View.Model.DeleteEntryData("FResultEntity");
			if (this._saveSimResult != null && this._saveSimResult.SuccessDataEnity != null && this._saveSimResult.SuccessDataEnity.Count<DynamicObject>() >= 1)
			{
				foreach (DynamicObject bomData in this._saveSimResult.SuccessDataEnity)
				{
					this.CreateNewResultRow(bomData, value, num, newMaterials);
					num++;
				}
			}
			if (this._saveSimResult != null && !ListUtils.IsEmpty<ValidationErrorInfo>(this._saveSimResult.ValidationErrors))
			{
				foreach (ValidationErrorInfo validationErrorInfo in this._saveSimResult.ValidationErrors)
				{
					base.View.Model.CreateNewEntryRow("FResultEntity");
					base.View.Model.SetValue("FMaterialNo", string.Empty, num);
					base.View.Model.SetValue("FBillNo", string.Empty, num);
					base.View.Model.SetValue("FEditContent", validationErrorInfo.Message, num);
					num++;
				}
			}
			if (!ListUtils.IsEmpty<DynamicObject>(this._bom2EcnDatas))
			{
				this.CreateEcnResultRow(this._bom2EcnDatas, num, newMaterials);
			}
			else if (!this._authEcnNewPermission)
			{
				string text2 = ResManager.LoadKDString("当前用户没有工程变更单的新增权限", "015072000012095", 7, new object[0]);
				base.View.Model.CreateNewEntryRow("FResultEntity");
				base.View.Model.SetValue("FMaterialNo", string.Empty, num);
				base.View.Model.SetValue("FBillNo", string.Empty, num);
				base.View.Model.SetValue("FEditContent", text2, num);
			}
			base.View.UpdateView("FResultEntity");
		}

		// Token: 0x06000D5D RID: 3421 RVA: 0x0009D910 File Offset: 0x0009BB10
		private void CreateNewResultRow(DynamicObject bomData, DynamicObject editMaterial, int row, List<DynamicObject> newMaterials)
		{
			string arg = string.Empty;
			if (newMaterials != null && newMaterials.Count > 0)
			{
				List<string> values = (from c in newMaterials
				select string.Format(ResManager.LoadKDString("物料编码为【{0}】名称为【{1}】规格型号为【{2}】的子项", "0151515153499030041914", 7, new object[0]), c["Number"], c["Name"], c["Specification"])).ToList<string>();
				arg = string.Join("，", values);
			}
			string arg2 = string.Empty;
			if (this._EditType == "a")
			{
				arg2 = ResManager.LoadKDString("新增", "015072000014411", 7, new object[0]);
			}
			else if (this._EditType == "b")
			{
				arg2 = string.Format(ResManager.LoadKDString("将物料编码为【{0}】名称为【{1}】规格型号为【{2}】的子项更换为", "015072030041915", 7, new object[0]), editMaterial["Number"], editMaterial["Name"], editMaterial["Specification"]);
			}
			else if (this._EditType == "c")
			{
				arg2 = ResManager.LoadKDString("删除", "015072000002165", 7, new object[0]);
			}
			else if (this._EditType == "d")
			{
				arg2 = ResManager.LoadKDString("修改", "015072000014413", 7, new object[0]);
			}
			else if (this._EditType == "m")
			{
				arg2 = ResManager.LoadKDString("修改父项信息", "015072000014414", 7, new object[0]);
				editMaterial = (DynamicObject)bomData["MATERIALID"];
			}
			else
			{
				arg2 = "";
			}
			string text = string.Format(ResManager.LoadKDString("版本号为【{0}】物料清单已成功{1}{2}；", "015072000014415", 7, new object[0]), bomData["Number"], arg2, arg);
			base.View.Model.CreateNewEntryRow("FResultEntity");
			if (editMaterial != null)
			{
				base.View.Model.SetValue("FMaterialNo", editMaterial["Id"], row);
			}
			base.View.Model.SetValue("FBillNo", bomData["Number"], row);
			base.View.Model.SetValue("FEditContent", text, row);
			base.View.Model.SetValue("FBOMID", bomData["ID"], row);
		}

		// Token: 0x06000D5E RID: 3422 RVA: 0x0009DB44 File Offset: 0x0009BD44
		private void CreateEcnResultRow(List<DynamicObject> bom2EcnData, int row, List<DynamicObject> newMaterials)
		{
			string text = string.Empty;
			foreach (DynamicObject dynamicObject in bom2EcnData)
			{
				text = string.Format(ResManager.LoadKDString("版本号为【{0}】物料清单已成功生成工程变更单", "015072000012096", 7, new object[0]), dynamicObject["Number"]);
				foreach (DynamicObject dynamicObject2 in newMaterials)
				{
					base.View.Model.CreateNewEntryRow("FResultEntity");
					base.View.Model.SetValue("FMaterialNo", dynamicObject2["ID"], row);
					base.View.Model.SetValue("FBillNo", dynamicObject["Number"], row);
					base.View.Model.SetValue("FEditContent", text, row);
					base.View.Model.SetValue("FBOMID", dynamicObject["ID"], row);
					row++;
				}
			}
		}

		// Token: 0x06000D5F RID: 3423 RVA: 0x0009DCA8 File Offset: 0x0009BEA8
		private void StepChanging_FPanelStep1(WizardStepChangingEventArgs e)
		{
			IDynamicFormView view = base.View.GetView(this.BBEFilter_PageId);
			if (!this.ValidateBBEFilterView(view))
			{
				e.Cancel = true;
			}
		}

		// Token: 0x06000D60 RID: 3424 RVA: 0x0009DCD8 File Offset: 0x0009BED8
		private void StepChanging_FPanelStep2(WizardStepChangingEventArgs e)
		{
			bool flag = true;
			if (this._EditType == "a" || this._EditType == "b" || this._EditType == "c" || this._EditType == "d")
			{
				flag = this.ValidateBBEEditChildContentView(base.View.GetView(this.BBEEidtContent_PageId));
				if (flag)
				{
					flag = this.ValidateAuxProp(base.View.GetView(this.BBEEidtContent_PageId));
				}
			}
			if (!flag)
			{
				e.Cancel = true;
				return;
			}
			(base.View.GetView(this.BBEBomFilter_PageId) as IDynamicFormViewService).ButtonClick("FBtnOK", "");
			object obj = null;
			base.View.Session.TryGetValue("returnData", out obj);
			if (obj is FilterParameter)
			{
				this._bomFilterParamter = (obj as FilterParameter);
			}
		}

		// Token: 0x06000D61 RID: 3425 RVA: 0x0009DDC0 File Offset: 0x0009BFC0
		private void StepChanging_FPanelStep3(WizardStepChangingEventArgs e)
		{
			if (!this._IsSave)
			{
				e.Cancel = true;
			}
		}

		// Token: 0x06000D62 RID: 3426 RVA: 0x0009DDD1 File Offset: 0x0009BFD1
		private void StepChanging_FPanelStep2_Up(WizardStepChangingEventArgs e)
		{
			this.CloseChildView(this.BBEEidtContent_PageId);
		}

		// Token: 0x06000D63 RID: 3427 RVA: 0x0009DDE0 File Offset: 0x0009BFE0
		private bool ValidateBBEFilterView(IDynamicFormView childView)
		{
			if (childView == null)
			{
				return false;
			}
			if (MFGBillUtil.GetValue<long>(childView.Model, "FOrgId", -1, 0L, null) == 0L)
			{
				base.View.ShowMessage(ResManager.LoadKDString("请录入使用组织！", "015072000014416", 7, new object[0]), 0);
				return false;
			}
			if (MFGBillUtil.GetValue<string>(childView.Model, "FEditType", -1, string.Empty, null) == string.Empty)
			{
				base.View.ShowMessage(ResManager.LoadKDString("请选择批量维护类型！", "015072000014417", 7, new object[0]), 0);
				return false;
			}
			return true;
		}

		// Token: 0x06000D64 RID: 3428 RVA: 0x0009DED8 File Offset: 0x0009C0D8
		private bool ValidateBBEEditChildContentView(IDynamicFormView childView)
		{
			if (childView == null)
			{
				return false;
			}
			if (MFGBillUtil.GetValue<long>(childView.Model, "FEditMaterialId", -1, 0L, null) <= 0L && (this._EditType == "b" || this._EditType == "b"))
			{
				base.View.ShowMessage(ResManager.LoadKDString("请录入子项物料编码！", "015072000014418", 7, new object[0]), 0);
				return false;
			}
			Entity entity = childView.BusinessInfo.GetEntity("FTreeEntity");
			DynamicObjectCollection entityDataObject = childView.Model.GetEntityDataObject(entity);
			List<DynamicObject> list = (from c in entityDataObject
			where DataEntityExtend.GetDynamicObjectItemValue<long>(c, "MATERIALIDCHILD_Id", 0L) > 0L
			select c).ToList<DynamicObject>();
			if (list == null || list.Count <= 0)
			{
				base.View.ShowMessage(ResManager.LoadKDString("请至少录入一行数据！", "015072000014419", 7, new object[0]), 0);
				return false;
			}
			if (this._CanNotChangeFieldOrmKeys.Contains("IsMrpRun"))
			{
				DynamicObject dynamicObject = DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(DataEntityExtend.GetDynamicValue<DynamicObject>(childView.Model.DataObject, "EditMaterialId", null), "MaterialPlan", null).First<DynamicObject>();
				string dynamicValue = DataEntityExtend.GetDynamicValue<string>(dynamicObject, "PlanningStrategy", null);
				if (dynamicValue.Equals("0") || dynamicValue.Equals("1"))
				{
					foreach (DynamicObject dynamicObject2 in list)
					{
						DynamicObject dynamicObject3 = DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(DataEntityExtend.GetDynamicValue<DynamicObject>(dynamicObject2, "MATERIALIDCHILD", null), "MaterialPlan", null).First<DynamicObject>();
						string dynamicValue2 = DataEntityExtend.GetDynamicValue<string>(dynamicObject3, "PlanningStrategy", null);
						if (dynamicValue2.Equals("2"))
						{
							base.View.ShowMessage(ResManager.LoadKDString("原子项物料计划策略为MPS或MRP，更换列未勾选MRP运算，替换的子项计划策略不允许为无!", "015072000037258", 7, new object[0]), 0);
							return false;
						}
					}
				}
			}
			foreach (DynamicObject dynamicObject4 in list)
			{
				if (DataEntityExtend.GetDynamicObjectItemValue<decimal>(dynamicObject4, "DENOMINATOR", 0m) <= 0m)
				{
					base.View.ShowMessage(ResManager.LoadKDString("子项明细中，分母必须大于0 !", "015072000014420", 7, new object[0]), 0);
					return false;
				}
			}
			List<DynamicObject> list2 = (from c in list
			where DataEntityExtend.GetDynamicObjectItemValue<int>(c, "DOSAGETYPE", 0) == 3
			select c).ToList<DynamicObject>();
			StringBuilder stringBuilder = new StringBuilder();
			if (list2 != null && list2.Count > 0)
			{
				foreach (DynamicObject dynamicObject5 in list2)
				{
					DynamicObjectCollection dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<DynamicObjectCollection>(dynamicObject5, "BOMCHILDLOTBASEDQTY", null);
					if (dynamicObjectItemValue == null || dynamicObjectItemValue.Count < 1)
					{
						base.View.ShowMessage(ResManager.LoadKDString("用量类型为阶梯的子项，必须维护对应的阶梯用量分录！", "015072000014421", 7, new object[0]), 0);
						return false;
					}
					IEnumerable<DynamicObject> source = from d in dynamicObjectItemValue
					where DataEntityExtend.GetDynamicObjectItemValue<decimal>(d, "DENOMINATORLOT", 0m) <= 0m
					select d;
					if (source.Count<DynamicObject>() > 0)
					{
						base.View.ShowMessage(ResManager.LoadKDString("子项对应的阶梯用量分录中，分母必须大于0 !", "015072000014422", 7, new object[0]), 0);
						return false;
					}
					IEnumerable<DynamicObject> source2 = from d in dynamicObjectItemValue
					where DataEntityExtend.GetDynamicObjectItemValue<decimal>(d, "ENDQTY", 0m) <= 0m
					select d;
					if (source2.Count<DynamicObject>() > 0)
					{
						base.View.ShowMessage(ResManager.LoadKDString("子项对应的阶梯用量分录，不允截止数量小于0！", "015072000014423", 7, new object[0]), 0);
						return false;
					}
					string arg = "";
					bool flag = this.IsLotQtyOverlapped(dynamicObjectItemValue, out arg);
					if (flag)
					{
						stringBuilder.AppendLine(string.Format(ResManager.LoadKDString("第{0}行分录,{1}", "015071000003192", 7, new object[0]), DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObject5, "seq", 0L), arg));
					}
				}
			}
			if (!string.IsNullOrWhiteSpace(stringBuilder.ToString()))
			{
				base.View.ShowMessage(stringBuilder.ToString(), 0);
				return false;
			}
			return true;
		}

		// Token: 0x06000D65 RID: 3429 RVA: 0x0009E378 File Offset: 0x0009C578
		private bool IsLotQtyOverlapped(IEnumerable<DynamicObject> childLotQtyDataEntities, out string errorMsg)
		{
			errorMsg = "";
			if (childLotQtyDataEntities == null || childLotQtyDataEntities.Count<DynamicObject>() <= 0)
			{
				return false;
			}
			decimal d = 0m;
			IEnumerable<DynamicObject> enumerable = from dynamicobject in childLotQtyDataEntities
			orderby new BomDataView(dynamicobject).StartQty
			select dynamicobject;
			bool result = false;
			foreach (DynamicObject dynamicObject in enumerable)
			{
				if (d - Convert.ToDecimal(dynamicObject["STARTQTY"]) > 0m)
				{
					result = true;
					errorMsg = ResManager.LoadKDString("子项明细对应的[阶梯用量]子表体中起始用量和截止用量重叠，请检查！", "015071000002097", 7, new object[0]);
					break;
				}
				d = Convert.ToDecimal(dynamicObject["ENDQTY"]);
			}
			return result;
		}

		// Token: 0x06000D66 RID: 3430 RVA: 0x0009E454 File Offset: 0x0009C654
		private bool ValidateAuxProp(IDynamicFormView childView)
		{
			if (this._EditType == "c")
			{
				return true;
			}
			if ((this._EditType == "b" || this._EditType == "d") && !this._CanEditFieldOrmKeys.Contains("AuxPropId_Id"))
			{
				return true;
			}
			long value = MFGBillUtil.GetValue<long>(childView.Model, "FUseOrgId", -1, 0L, null);
			Dictionary<long, Dictionary<long, string>> dictionary = new Dictionary<long, Dictionary<long, string>>();
			Dictionary<long, string> auxPropIdByOrgId = FlexServiceHelper.GetAuxPropIdByOrgId(base.Context, value);
			if (!ListUtils.IsEmpty<KeyValuePair<long, string>>(auxPropIdByOrgId))
			{
				dictionary[value] = auxPropIdByOrgId;
			}
			if (ListUtils.IsEmpty<KeyValuePair<long, Dictionary<long, string>>>(dictionary))
			{
				return true;
			}
			FormMetadata formMetadata = (FormMetadata)MetaDataServiceHelper.Load(base.Context, "BD_FLEXSITEMDETAILV", true);
			Entity entity = childView.BusinessInfo.GetEntity("FTreeEntity");
			DynamicObjectCollection entityDataObject = childView.Model.GetEntityDataObject(entity);
			StringBuilder stringBuilder = new StringBuilder();
			bool flag = true;
			foreach (DynamicObject dynamicObject in entityDataObject)
			{
				DynamicObject dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<DynamicObject>(dynamicObject, "MATERIALIDCHILD", null);
				if (dynamicObjectItemValue != null)
				{
					long dynamicObjectItemValue2 = DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObjectItemValue, "UseOrgId_Id", 0L);
					Dictionary<long, string> dictionary2 = null;
					if (dynamicObjectItemValue2 > 0L && dictionary.TryGetValue(dynamicObjectItemValue2, out dictionary2))
					{
						List<string> list = MFGServiceHelper.CheckInputFlexColumn(base.Context, "MATERIALIDCHILD", "AuxPropId", true, dynamicObject, dictionary2, formMetadata.BusinessInfo);
						if (!ListUtils.IsEmpty<string>(list))
						{
							StringBuilder stringBuilder2 = new StringBuilder(string.Format(ResManager.LoadKDString("{0}物料{1}{2}的辅助属性维度 {3} 不能为空!", "015072000014424", 7, new object[0]), new object[]
							{
								(entity is HeadEntity) ? "" : string.Format(ResManager.LoadKDString("第{0}行", "015064000002269", 7, new object[0]), dynamicObject["Seq"].ToString()),
								dynamicObjectItemValue["Number"].ToString(),
								ResManager.LoadKDString("影响计划", "015064000002265", 7, new object[0]),
								SysBclExtend.JoinSpecLast(list, "、", ResManager.LoadKDString("和", "015064000003223", 7, new object[0]))
							}));
							stringBuilder.AppendLine(stringBuilder2.ToString());
							flag = false;
						}
					}
				}
			}
			if (!flag)
			{
				base.View.ShowMessage(stringBuilder.ToString(), 0);
			}
			return flag;
		}

		// Token: 0x06000D67 RID: 3431 RVA: 0x0009E6DC File Offset: 0x0009C8DC
		private void SetBBEBomFilterMtlId(IDynamicFormView childView)
		{
			long value = MFGBillUtil.GetValue<long>(base.View.GetView(this.BBEEidtContent_PageId).Model, "FEditMaterialId", -1, 0L, null);
			childView.Model.SetValue("FFilterMaterialId", value);
		}

		// Token: 0x0400060A RID: 1546
		protected string _computeId = string.Empty;

		// Token: 0x0400060B RID: 1547
		protected static object lockobjectstr = new object();

		// Token: 0x0400060C RID: 1548
		protected long _UseOrgId;

		// Token: 0x0400060D RID: 1549
		protected string _EditType = string.Empty;

		// Token: 0x0400060E RID: 1550
		protected bool _IsSynPPbom;

		// Token: 0x0400060F RID: 1551
		protected bool _IsSynPLBom;

		// Token: 0x04000610 RID: 1552
		protected bool _IsSave;

		// Token: 0x04000611 RID: 1553
		protected bool _IsSuccess;

		// Token: 0x04000612 RID: 1554
		protected List<string> _CanEditFieldKeys = new List<string>();

		// Token: 0x04000613 RID: 1555
		protected List<string> _CanEditFieldOrmKeys = new List<string>();

		// Token: 0x04000614 RID: 1556
		protected List<string> _CanAppendFieldKeys = new List<string>();

		// Token: 0x04000615 RID: 1557
		protected List<string> _CanAppendFieldOrmKeys = new List<string>();

		// Token: 0x04000616 RID: 1558
		protected List<string> _CanNotChangeFieldKeys = new List<string>();

		// Token: 0x04000617 RID: 1559
		protected List<string> _CanNotChangeFieldOrmKeys = new List<string>();

		// Token: 0x04000618 RID: 1560
		protected FilterParameter _bomFilterParamter = new FilterParameter();

		// Token: 0x04000619 RID: 1561
		private ConcurrentDictionary<string, Action<ButtonClickEventArgs>> _buttonClickEventHandlers = new ConcurrentDictionary<string, Action<ButtonClickEventArgs>>(new IgnoreCaseStringComparer());

		// Token: 0x0400061A RID: 1562
		private ConcurrentDictionary<string, Action<WizardStepChangingEventArgs>> _stepChangingEventHandler = new ConcurrentDictionary<string, Action<WizardStepChangingEventArgs>>(new IgnoreCaseStringComparer());

		// Token: 0x0400061B RID: 1563
		private ConcurrentDictionary<string, Action<WizardStepChangedEventArgs>> _stepChangedEventHandler = new ConcurrentDictionary<string, Action<WizardStepChangedEventArgs>>(new IgnoreCaseStringComparer());

		// Token: 0x0400061C RID: 1564
		private bool _catchThreadException;

		// Token: 0x0400061D RID: 1565
		private string _errorMessage = string.Empty;

		// Token: 0x0400061E RID: 1566
		private bool _isSaveSimResult;

		// Token: 0x0400061F RID: 1567
		private IOperationResult _simResultResult;

		// Token: 0x04000620 RID: 1568
		private IOperationResult _saveSimResult;

		// Token: 0x04000621 RID: 1569
		private List<DynamicObject> _bom2EcnDatas;

		// Token: 0x04000622 RID: 1570
		private List<DynamicObject> _ecnBomItem;

		// Token: 0x04000623 RID: 1571
		private bool _authEcnNewPermission = true;

		// Token: 0x04000624 RID: 1572
		private string _repItemAlterPolicy = string.Empty;

		// Token: 0x04000625 RID: 1573
		private string _ecnChangeType = string.Empty;
	}
}
