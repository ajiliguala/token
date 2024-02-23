using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.ControlElement;
using Kingdee.BOS.JSON;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.MFG.ServiceHelper.ENG;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm
{
	// Token: 0x020000A5 RID: 165
	[Description("模型配置动态面板插件")]
	public class MdlCfgDynPanelEdit : AbstractDynamicFormPlugIn
	{
		// Token: 0x17000075 RID: 117
		// (get) Token: 0x06000BB5 RID: 2997 RVA: 0x000867C5 File Offset: 0x000849C5
		// (set) Token: 0x06000BB6 RID: 2998 RVA: 0x000867CD File Offset: 0x000849CD
		public JSONObject MdlCfgObj
		{
			get
			{
				return this._mdlCfgObj;
			}
			set
			{
				this._mdlCfgObj = value;
			}
		}

		// Token: 0x06000BB7 RID: 2999 RVA: 0x000867D6 File Offset: 0x000849D6
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
		}

		// Token: 0x06000BB8 RID: 3000 RVA: 0x000867E0 File Offset: 0x000849E0
		public override void OnSetBusinessInfo(SetBusinessInfoArgs e)
		{
			object customParameter = this.View.OpenParameter.GetCustomParameter("MdlCfgParameter");
			if (customParameter == null)
			{
				this._isUnitTest = true;
				return;
			}
			JSONObject jsonobject = KDObjectConverter.DeserializeObject<JSONObject>(OtherExtend.ConvertTo<string>(customParameter, null));
			if (jsonobject == null)
			{
				this._isUnitTest = true;
				return;
			}
			Tuple<BusinessInfo, LayoutInfo, LayoutInfo> tuple = MdlCfgServiceHelper.BuildDynPanelBusiness(base.Context, this.View.OpenParameter.FormMetaData, jsonobject);
			this._businessInfo = tuple.Item1;
			this._layoutInfo = tuple.Item2;
			this._panellayoutInfo = tuple.Item3;
			e.BusinessInfo = (e.BillBusinessInfo = this._businessInfo);
		}

		// Token: 0x06000BB9 RID: 3001 RVA: 0x0008687C File Offset: 0x00084A7C
		public override void OnSetLayoutInfo(SetLayoutInfoArgs e)
		{
			if (this._isUnitTest)
			{
				return;
			}
			Container control = this.View.GetControl<Container>("FPanel");
			base.OnSetLayoutInfo(e);
			control.AddControls(this._panellayoutInfo);
			e.LayoutInfo = this._layoutInfo;
		}

		// Token: 0x06000BBA RID: 3002 RVA: 0x000868F0 File Offset: 0x00084AF0
		public override void BeforeBindData(EventArgs e)
		{
			if (this._isUnitTest)
			{
				return;
			}
			object customParameter = this.View.OpenParameter.GetCustomParameter("MdlNumber", true);
			FormMetadata formMetadata = MetaDataServiceHelper.Load(base.Context, "ENG_PRODUCTMODEL", true) as FormMetadata;
			this.PrdMdlObject = BusinessDataServiceHelper.LoadSingle(base.Context, customParameter, formMetadata.BusinessInfo.GetDynamicObjectType(), null);
			this.PrdModelVariables = (from x in this.PrdMdlObject["Entity"] as DynamicObjectCollection
			group x by string.Format("F{0}", DataEntityExtend.GetDynamicValue<long>(x, "ModelVarId_Id", 0L))).ToDictionary((IGrouping<string, DynamicObject> x) => x.Key, (IGrouping<string, DynamicObject> v) => v.First<DynamicObject>());
			object customParameter2 = this.View.OpenParameter.GetCustomParameter("OrgId", true);
			this.Model.SetValue("FOrgId", OtherExtend.ConvertTo<long>(customParameter2, 0L));
			this.Model.SetValue("FModelNumber", OtherExtend.ConvertTo<long>(customParameter, 0L));
			object customParameter3 = this.View.OpenParameter.GetCustomParameter("MdlCfgParameter");
			this.MdlCfgObj = KDObjectConverter.DeserializeObject<JSONObject>(OtherExtend.ConvertTo<string>(customParameter3, null));
			JSONArray jsonarray = KDObjectConverter.DeserializeObject<JSONArray>(this.MdlCfgObj.Get("dynFlds").ToString());
			DynamicObject dynamicObject = this.View.OpenParameter.GetCustomParameter("AuxPtyObj") as DynamicObject;
			bool flag = dynamicObject != null;
			foreach (object obj in jsonarray)
			{
				JSONObject jsonobject = KDObjectConverter.DeserializeObject<JSONObject>(obj.ToString());
				if (flag)
				{
					this.SetDefaultValue(dynamicObject, jsonobject);
				}
				else
				{
					this.SetDefaultValue(jsonobject);
				}
			}
		}

		// Token: 0x06000BBB RID: 3003 RVA: 0x00086AEC File Offset: 0x00084CEC
		public override void AfterBindData(EventArgs e)
		{
			if (this.View.OpenParameter.GetCustomParameter("IsContainer") != null)
			{
				this.View.GetMainMenu().RemoveMenuBarItems(new List<string>
				{
					"tbConfirm",
					"tbCancel"
				});
			}
		}

		// Token: 0x06000BBC RID: 3004 RVA: 0x00086B40 File Offset: 0x00084D40
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			string a;
			if ((a = e.BarItemKey.ToUpper()) != null)
			{
				if (!(a == "TBCONFIRM"))
				{
					if (!(a == "TBCANCEL"))
					{
						return;
					}
				}
				else
				{
					this.View.ReturnToParentWindow(this.Model.DataObject);
				}
			}
		}

		// Token: 0x06000BBD RID: 3005 RVA: 0x00086B90 File Offset: 0x00084D90
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			string modelVarFilter = this.GetModelVarFilter(e.FieldKey);
			if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(modelVarFilter))
			{
				e.ListFilterParameter.Filter = (ObjectUtils.IsNullOrEmptyOrWhiteSpace(e.ListFilterParameter.Filter) ? modelVarFilter : (" And " + modelVarFilter));
			}
		}

		// Token: 0x06000BBE RID: 3006 RVA: 0x00086BE0 File Offset: 0x00084DE0
		private string GetModelVarFilter(string fieldKey)
		{
			string result = "";
			DynamicObject dynamicObject;
			if (this.PrdModelVariables.TryGetValue(fieldKey, out dynamicObject))
			{
				string dynamicValue = DataEntityExtend.GetDynamicValue<string>(dynamicObject, "SettingType", null);
				long dynamicValue2 = DataEntityExtend.GetDynamicValue<long>(dynamicObject, "Id", 0L);
				string a;
				if ((a = dynamicValue) != null)
				{
					if (!(a == "1"))
					{
						if (a == "2")
						{
							result = string.Format("not exists(\r\nselect 1 from T_ENG_PRDMODELVARVALUESET T1 WHERE T1.FENTRYID = {0} AND T1.FASSISTANTDATAVALUEID = T0.FENTRYID\r\n)", dynamicValue2);
						}
					}
					else
					{
						result = string.Format("exists(\r\nselect 1 from T_ENG_PRDMODELVARVALUESET T1 WHERE T1.FENTRYID = {0} AND T1.FASSISTANTDATAVALUEID = T0.FENTRYID\r\n)", dynamicValue2);
					}
				}
			}
			return result;
		}

		// Token: 0x06000BBF RID: 3007 RVA: 0x00086C68 File Offset: 0x00084E68
		public override void BeforeSetItemValueByNumber(BeforeSetItemValueByNumberArgs e)
		{
			string modelVarFilter = this.GetModelVarFilter(e.BaseDataFieldKey);
			if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(modelVarFilter))
			{
				e.Filter = (ObjectUtils.IsNullOrEmptyOrWhiteSpace(e.Filter) ? modelVarFilter : (" And " + modelVarFilter));
			}
		}

		// Token: 0x06000BC0 RID: 3008 RVA: 0x00086CAC File Offset: 0x00084EAC
		private void SetDefaultValue(JSONObject df)
		{
			string text = "";
			if (df.TryGetValue<string>("defValue", "", ref text))
			{
				string @string = df.GetString("fieldType");
				string text2 = "F" + df.Get("id").ToString();
				string a;
				if ((a = @string) != null)
				{
					if (a == "AssistantField" || a == "BaseDataField")
					{
						((IDynamicFormViewService)this.View).SetItemValueByNumber(text2, text, 0);
						return;
					}
					if (a == "TextField")
					{
						this.Model.SetValue(text2, text);
						return;
					}
					if (a == "DecimalField")
					{
						this.Model.SetValue(text2, OtherExtend.ConvertTo<decimal>(text, 0m));
						return;
					}
					if (!(a == "CheckBoxField"))
					{
						return;
					}
					this.Model.SetValue(text2, OtherExtend.ConvertTo<bool>(text, false));
				}
			}
		}

		// Token: 0x06000BC1 RID: 3009 RVA: 0x00086DA0 File Offset: 0x00084FA0
		private void SetDefaultValue(DynamicObject auxPtyValue, JSONObject df)
		{
			string text = "";
			string @string = df.GetString("fieldType");
			string text2 = "F" + df.Get("id").ToString();
			df.TryGetValue<string>("defValue", "", ref text);
			string text3 = "";
			string a;
			if ((a = @string) != null)
			{
				if (a == "AssistantField")
				{
					long num = OtherExtend.ConvertTo<long>(df.Get("auxPropTypeId"), 0L);
					if (num != 0L)
					{
						DynamicObject dynamicObject = auxPtyValue["F" + num] as DynamicObject;
						if (dynamicObject != null)
						{
							text = DataEntityExtend.GetDynamicValue<string>(dynamicObject, "FNumber", null);
						}
					}
					((IDynamicFormViewService)this.View).SetItemValueByNumber(text2, text, 0);
					return;
				}
				if (a == "BaseDataField")
				{
					long num = OtherExtend.ConvertTo<long>(df.Get("auxPropTypeId"), 0L);
					if (num != 0L)
					{
						DynamicObject dynamicObject2 = auxPtyValue["F" + num] as DynamicObject;
						if (dynamicObject2 != null)
						{
							text = DataEntityExtend.GetDynamicValue<string>(dynamicObject2, "Number", null);
						}
					}
					((IDynamicFormViewService)this.View).SetItemValueByNumber(text2, text, 0);
					return;
				}
				if (a == "TextField")
				{
					if (this.TryGetSplitValue(auxPtyValue, df, out text3))
					{
						int num2 = OtherExtend.ConvertTo<int>(df.Get("editLen"), 0);
						if (text3.Length <= num2)
						{
							text = text3;
						}
					}
					this.Model.SetValue(text2, text);
					return;
				}
				if (a == "DecimalField")
				{
					decimal num3 = OtherExtend.ConvertTo<decimal>(text, 0m);
					if (this.TryGetSplitValue(auxPtyValue, df, out text3))
					{
						decimal num4 = 0m;
						if (decimal.TryParse(text3, out num4))
						{
							int num5 = OtherExtend.ConvertTo<int>(df.Get("editLen"), 0);
							if (OtherExtend.ConvertTo<decimal>(decimal.Divide(num4, OtherExtend.ConvertTo<decimal>(Math.Pow(10.0, (double)(num5 + 1)), 0m)), 0m) < 1m)
							{
								num3 = decimal.Round(num4, OtherExtend.ConvertTo<int>(df.Get("precision"), 0));
							}
						}
					}
					this.Model.SetValue(text2, num3);
					return;
				}
				if (!(a == "CheckBoxField"))
				{
					return;
				}
				bool flag = OtherExtend.ConvertTo<bool>(text, false);
				if (this.TryGetSplitValue(auxPtyValue, df, out text3))
				{
					if (text3 == "Y" || text3 == ResManager.LoadKDString("是", "015072000039151", 7, new object[0]))
					{
						flag = true;
					}
					else if (text3 == "N" || text3 == ResManager.LoadKDString("否", "015072000039152", 7, new object[0]))
					{
						flag = false;
					}
				}
				this.Model.SetValue(text2, flag);
			}
		}

		// Token: 0x06000BC2 RID: 3010 RVA: 0x00087084 File Offset: 0x00085284
		private bool TryGetSplitValue(DynamicObject auxPtyValue, JSONObject df, out string newValue)
		{
			bool result = false;
			newValue = "";
			long num = OtherExtend.ConvertTo<long>(df.Get("auxPropTypeId"), 0L);
			if (num != 0L)
			{
				string text = OtherExtend.ConvertTo<string>(auxPtyValue["F" + num], string.Empty);
				string @string = df.GetString("split");
				int num2 = OtherExtend.ConvertTo<int>(df.Get("span"), 0);
				if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(@string))
				{
					string[] array = text.Split(new string[]
					{
						@string
					}, StringSplitOptions.RemoveEmptyEntries);
					if (array.Count<string>() >= num2 && num2 > 0)
					{
						result = true;
						newValue = array[num2 - 1].Trim();
					}
				}
				else if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(text))
				{
					result = true;
					newValue = text.Trim();
				}
			}
			return result;
		}

		// Token: 0x06000BC3 RID: 3011 RVA: 0x0008714A File Offset: 0x0008534A
		public override void DataChanged(DataChangedEventArgs e)
		{
			base.DataChanged(e);
		}

		// Token: 0x0400057B RID: 1403
		private BusinessInfo _businessInfo;

		// Token: 0x0400057C RID: 1404
		private LayoutInfo _layoutInfo;

		// Token: 0x0400057D RID: 1405
		private LayoutInfo _panellayoutInfo;

		// Token: 0x0400057E RID: 1406
		private JSONObject _mdlCfgObj;

		// Token: 0x0400057F RID: 1407
		protected DynamicObject PrdMdlObject;

		// Token: 0x04000580 RID: 1408
		private bool _isUnitTest;

		// Token: 0x04000581 RID: 1409
		protected Dictionary<string, DynamicObject> PrdModelVariables;
	}
}
