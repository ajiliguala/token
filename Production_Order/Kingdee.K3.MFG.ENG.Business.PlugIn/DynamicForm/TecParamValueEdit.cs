using System;
using System.ComponentModel;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.JSON;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.MFG.EntityHelper;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm
{
	// Token: 0x020000BB RID: 187
	[HotUpdate]
	[Description("工艺参数json录入_表单插件")]
	public class TecParamValueEdit : AbstractDynamicFormPlugIn
	{
		// Token: 0x06000DD5 RID: 3541 RVA: 0x000A190C File Offset: 0x0009FB0C
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
			this.currentRow = (this.View.OpenParameter.GetCustomParameter("currentRow") as DynamicObject);
			this.documentStatus = (ObjectUtils.IsNullOrEmptyOrWhiteSpace(this.View.OpenParameter.GetCustomParameter("documentStatus")) ? "" : Convert.ToString(this.View.OpenParameter.GetCustomParameter("documentStatus")));
		}

		// Token: 0x06000DD6 RID: 3542 RVA: 0x000A1984 File Offset: 0x0009FB84
		public override void AfterBindData(EventArgs e)
		{
			base.AfterBindData(e);
			if (this.currentRow != null)
			{
				this.View.Model.SetValue("FParamValue", DataEntityExtend.GetDynamicObjectItemValue<string>(this.currentRow, "ParamValue", null));
				this.View.UpdateView("FParamValue");
			}
		}

		// Token: 0x06000DD7 RID: 3543 RVA: 0x000A19D8 File Offset: 0x0009FBD8
		public override void ButtonClick(ButtonClickEventArgs e)
		{
			base.ButtonClick(e);
			string a;
			if ((a = e.Key.ToUpper()) != null)
			{
				if (a == "FBTNCONFIRM")
				{
					this.DoConfirm(e);
					return;
				}
				if (a == "FBTNCANCEL")
				{
					this.DoCancel(e);
					return;
				}
				if (!(a == "FBTNINPUTJSON"))
				{
					return;
				}
				this.OpenJsonForm();
			}
		}

		// Token: 0x06000DD8 RID: 3544 RVA: 0x000A1A84 File Offset: 0x0009FC84
		private void OpenJsonForm()
		{
			object obj = this.View.Model.GetValue("FParamValue") ?? "";
			string text = obj.ToString();
			try
			{
				JSONObject.Parse(text);
			}
			catch (Exception)
			{
				string text2 = ResManager.LoadKDString("请确认json格式正确", "0151515153499000018036", 7, new object[0]);
				this.View.ShowErrMessage(text2, "", 0);
				return;
			}
			DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
			dynamicFormShowParameter.OpenStyle.ShowType = 6;
			dynamicFormShowParameter.ParentPageId = this.View.PageId;
			dynamicFormShowParameter.FormId = "SFC_JsonInput";
			dynamicFormShowParameter.CustomComplexParams.Add("documentStatus", this.documentStatus);
			dynamicFormShowParameter.CustomComplexParams.Add("jsonstr", text);
			this.View.ShowForm(dynamicFormShowParameter, delegate(FormResult returnValue)
			{
				if (returnValue.ReturnData != null)
				{
					string text3 = returnValue.ReturnData.ToString();
					this.View.Model.SetValue("FParamValue", text3);
					this.View.UpdateView("FParamValue");
				}
			});
		}

		// Token: 0x06000DD9 RID: 3545 RVA: 0x000A1B6C File Offset: 0x0009FD6C
		private void DoConfirm(ButtonClickEventArgs e)
		{
			this.ReturnJsonValue();
		}

		// Token: 0x06000DDA RID: 3546 RVA: 0x000A1B74 File Offset: 0x0009FD74
		private void DoCancel(ButtonClickEventArgs e)
		{
			this.ReturnNullValue();
		}

		// Token: 0x06000DDB RID: 3547 RVA: 0x000A1B7C File Offset: 0x0009FD7C
		private void ReturnJsonValue()
		{
			object obj = this.View.Model.GetValue("FParamValue") ?? "";
			string text = obj.ToString();
			this.View.ReturnToParentWindow(text);
			this.View.Close();
		}

		// Token: 0x06000DDC RID: 3548 RVA: 0x000A1BC8 File Offset: 0x0009FDC8
		private void ReturnNullValue()
		{
			string text = null;
			this.View.ReturnToParentWindow(text);
			this.View.Close();
		}

		// Token: 0x04000669 RID: 1641
		private DynamicObject currentRow;

		// Token: 0x0400066A RID: 1642
		private string documentStatus;
	}
}
