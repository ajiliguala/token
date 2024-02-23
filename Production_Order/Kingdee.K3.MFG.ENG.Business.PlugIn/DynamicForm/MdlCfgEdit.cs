using System;
using System.ComponentModel;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.JSON;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.Util;
using Kingdee.BOS.VerificationHelper.Verifiers;
using Kingdee.K3.Core.MFG.ENG.ProductModel;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.MFG.ServiceHelper.ENG;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm
{
	// Token: 0x020000A6 RID: 166
	[Description("模型配置")]
	public class MdlCfgEdit : AbstractDynamicFormPlugIn
	{
		// Token: 0x17000076 RID: 118
		// (get) Token: 0x06000BC8 RID: 3016 RVA: 0x0008715B File Offset: 0x0008535B
		// (set) Token: 0x06000BC9 RID: 3017 RVA: 0x00087163 File Offset: 0x00085363
		public string PanelViewId { get; set; }

		// Token: 0x17000077 RID: 119
		// (get) Token: 0x06000BCA RID: 3018 RVA: 0x0008716C File Offset: 0x0008536C
		protected IDynamicFormView PanelView
		{
			get
			{
				return this.View.GetView(this.PanelViewId);
			}
		}

		// Token: 0x17000078 RID: 120
		// (get) Token: 0x06000BCB RID: 3019 RVA: 0x0008717F File Offset: 0x0008537F
		// (set) Token: 0x06000BCC RID: 3020 RVA: 0x00087187 File Offset: 0x00085387
		public JSONObject PrdMdlJObject
		{
			get
			{
				return this._prdMdlJObject;
			}
			set
			{
				this._prdMdlJObject = value;
			}
		}

		// Token: 0x06000BCD RID: 3021 RVA: 0x00087190 File Offset: 0x00085390
		public override void PreOpenForm(PreOpenFormEventArgs e)
		{
			base.PreOpenForm(e);
			FeatureVerifier.CheckFeature(e.Context, "PDB");
		}

		// Token: 0x06000BCE RID: 3022 RVA: 0x000871AC File Offset: 0x000853AC
		public override void BeforeBindData(EventArgs e)
		{
			long num = Convert.ToInt64(this.View.OpenParameter.GetCustomParameter("UserOrgId"));
			long num2 = Convert.ToInt64(this.View.OpenParameter.GetCustomParameter("ProductModelId"));
			this.View.Model.SetValue("FOrgId", (num == 0L) ? base.Context.CurrentOrganizationInfo.ID : num);
			this.View.Model.SetValue("FModelNumber", num2);
			if (num2 > 0L)
			{
				this.ShowDynamicPanel();
			}
		}

		// Token: 0x06000BCF RID: 3023 RVA: 0x00087248 File Offset: 0x00085448
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			string a;
			if ((a = e.FieldKey.ToUpper()) != null)
			{
				if (!(a == "FMODELNUMBER"))
				{
					return;
				}
				long dynamicValue = DataEntityExtend.GetDynamicValue<long>(this.Model.DataObject, "OrgId_Id", 0L);
				IRegularFilterParameter listFilterParameter = e.ListFilterParameter;
				listFilterParameter.Filter += string.Format(" {0}FUseOrgId={1} ", ObjectUtils.IsNullOrEmptyOrWhiteSpace(e.ListFilterParameter.Filter) ? "" : "And ", dynamicValue);
			}
		}

		// Token: 0x06000BD0 RID: 3024 RVA: 0x000872CE File Offset: 0x000854CE
		public override void DataChanged(DataChangedEventArgs e)
		{
			base.DataChanged(e);
		}

		// Token: 0x06000BD1 RID: 3025 RVA: 0x000872D8 File Offset: 0x000854D8
		public override void ButtonClick(ButtonClickEventArgs e)
		{
			string key;
			if ((key = e.Key) != null)
			{
				if (key == "FREFRESH")
				{
					this.ShowDynamicPanel();
					return;
				}
				if (!(key == "FOK"))
				{
					if (!(key == "FCANCEL"))
					{
						return;
					}
					this.View.Close();
				}
				else
				{
					if (this.PanelView == null)
					{
						this.View.ShowErrMessage("", ResManager.LoadKDString("模型配置数据包获取失败!", "015072000025084", 7, new object[0]), 0);
						return;
					}
					JSONObject jsonobject = MdlCfgServiceHelper.BuildCfgDataObj(base.Context, this.PanelView.Model.DataObject);
					IOperationResult operationResult = MdlCfgServiceHelper.ValidateDynFlds(base.Context, this.PrdMdlJObject, jsonobject);
					if (!operationResult.IsSuccess)
					{
						this.View.ShowOperateResult(operationResult.OperateResult, "BOS_BatchTips");
						return;
					}
					object customParameter = this.View.OpenParameter.GetCustomParameter("optParam", true);
					bool flag = customParameter != null;
					DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter
					{
						FormId = "ENG_MDLCFGEXEC",
						PageId = Guid.NewGuid().ToString(),
						ParentPageId = this.View.ParentFormView.PageId
					};
					MdlCfgOption mdlCfgOption = new MdlCfgOption();
					mdlCfgOption.CfgBillObj = jsonobject;
					mdlCfgOption.Context = base.Context;
					dynamicFormShowParameter.CustomComplexParams.Add("option", mdlCfgOption);
					dynamicFormShowParameter.CustomParams.Add("MdlCfgParameter", KDObjectConverter.SerializeObject(this.PrdMdlJObject));
					if (flag)
					{
						dynamicFormShowParameter.CustomParams.Add("optOption", customParameter.ToString());
					}
					dynamicFormShowParameter.OpenStyle.ShowType = 7;
					this.View.ParentFormView.ShowForm(dynamicFormShowParameter);
					this.View.SendDynamicFormAction(this.View.ParentFormView);
					this.View.Close();
					return;
				}
			}
		}

		// Token: 0x06000BD2 RID: 3026 RVA: 0x000874C0 File Offset: 0x000856C0
		private void ShowDynamicPanel()
		{
			long dynamicValue = DataEntityExtend.GetDynamicValue<long>(this.Model.DataObject, "ModelNumber_Id", 0L);
			if (dynamicValue == 0L)
			{
				this.View.ShowErrMessage("", ResManager.LoadKDString("产品模型不能为空", "015072000025085", 7, new object[0]), 0);
				return;
			}
			IDynamicFormView panelView = this.PanelView;
			this.PrdMdlJObject = MdlCfgServiceHelper.BuildDynFieldMdlFromPrdModeling(base.Context, dynamicValue);
			if (panelView != null)
			{
				panelView.InvokeFormOperation("Close");
				this.View.SendDynamicFormAction(panelView);
			}
			string text = Guid.NewGuid().ToString();
			DynamicObject dynamicObject = this.Model.GetValue("FModelNumber") as DynamicObject;
			string text2 = DataEntityExtend.GetDynamicValue<string>(dynamicObject, "FixFormId_Id", null);
			text2 = (ObjectUtils.IsNullOrEmptyOrWhiteSpace(text2) ? "ENG_MDLCFGPANEL" : text2);
			DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter
			{
				FormId = text2,
				PageId = text
			};
			dynamicFormShowParameter.OpenStyle.ShowType = 3;
			dynamicFormShowParameter.OpenStyle.TagetKey = "FDymPanel";
			dynamicFormShowParameter.CustomParams.Add("MdlCfgParameter", KDObjectConverter.SerializeObject(this.PrdMdlJObject));
			dynamicFormShowParameter.CustomParams.Add("MdlNumber", DataEntityExtend.GetDynamicValue<string>(this.Model.DataObject, "ModelNumber_Id", null));
			dynamicFormShowParameter.CustomParams.Add("OrgId", DataEntityExtend.GetDynamicValue<string>(this.Model.DataObject, "OrgId_Id", null));
			object customParameter = this.View.OpenParameter.GetCustomParameter("optParam", false);
			dynamicFormShowParameter.CustomParams.Add("IsContainer", "1");
			bool flag = customParameter != null;
			if (flag)
			{
				JSONObject jsonobject = KDObjectConverter.DeserializeObject<JSONObject>(customParameter.ToString());
				string @string = jsonobject.GetString("AuxPtyKey");
				int num = jsonobject.GetInt("EntrySeq") - 1;
				DynamicObject value = this.View.ParentFormView.Model.GetValue(@string, num) as DynamicObject;
				dynamicFormShowParameter.CustomComplexParams.Add("AuxPtyObj", value);
			}
			this.View.ShowForm(dynamicFormShowParameter, new Action<FormResult>(this.GetFormResult));
			this.PanelViewId = text;
		}

		// Token: 0x06000BD3 RID: 3027 RVA: 0x000876F2 File Offset: 0x000858F2
		private void GetFormResult(FormResult result)
		{
		}

		// Token: 0x04000585 RID: 1413
		private IDynamicFormView _panelView;

		// Token: 0x04000586 RID: 1414
		private JSONObject _prdMdlJObject;
	}
}
