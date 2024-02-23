using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Kingdee.BOS;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.Interaction;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm
{
	// Token: 0x02000064 RID: 100
	[HotUpdate]
	[Description("物料清单批量修改字段")]
	public class BOMBulkEdit : AbstractDynamicFormPlugIn
	{
		// Token: 0x0600073A RID: 1850 RVA: 0x000548C8 File Offset: 0x00052AC8
		public override void OnInitialize(InitializeEventArgs e)
		{
			this.metadata = (FormMetadata)MetaDataServiceHelper.Load(this.View.Context, this.View.ParentFormView.BillBusinessInfo.GetForm().Id, true);
			string value = string.Format("{1}-[{0}]", this.metadata.GetLayoutInfo().GetFormAppearance().Caption, this.View.LayoutInfo.GetFormAppearance().Caption);
			LocaleValue localeValue = new LocaleValue();
			localeValue.Add(new KeyValuePair<int, string>(base.Context.UserLocale.LCID, value));
			this.View.SetFormTitle(localeValue);
			List<KeyValuePair<object, object>> list = this.View.OpenParameter.GetCustomParameter("fPKEntryIds") as List<KeyValuePair<object, object>>;
			Convert.ToInt64(list[0].Key);
			if (!string.IsNullOrEmpty(list[0].Value.ToString()))
			{
				long num = Convert.ToInt64(list[0].Value);
				if (num > 0L)
				{
					this.IsFilterEntity = true;
				}
			}
			this.AddFiledKey();
		}

		// Token: 0x0600073B RID: 1851 RVA: 0x00054A08 File Offset: 0x00052C08
		private void AddFiledKey()
		{
			string[] array = new string[]
			{
				"FSupplyType",
				"FMATERIALTYPE",
				"FSTOCKID",
				"FOverControlMode",
				"FNUMERATOR",
				"FNUMERATOR",
				"FDENOMINATOR",
				"FFIXSCRAPQTY",
				"FSCRAPRATE",
				"FMEMO",
				"FEFFECTDATE",
				"FEXPIREDATE",
				"FISSUETYPE",
				"FProxyTime",
				"FProxyTimeUnit",
				"FProxyIsKeyComponent",
				"FISSkip",
				"FProxyOper111",
				"FPROCESSID",
				"FDOSAGETYPE",
				"FPOSITIONNO",
				"FISGETSCRAP",
				"FBOMID",
				"FOWNERTYPEID",
				"FOptQueue",
				"FISMinIssueQty",
				"FIsCanChoose",
				"FIsCanEdit",
				"FIsCanReplace",
				"FIsMrpRun"
			};
			if (!base.Context.IsStandardEdition())
			{
				string[] second = new string[]
				{
					"FSUPPLYORG",
					"FChildSupplyOrgId",
					"FSupplyMode"
				};
				array = array.Concat(second).ToArray<string>();
			}
			List<FieldFuncControlInfo> fieldFuncControlInfos = MetaDataServiceHelper.GetFieldFuncControlInfos(this.View.Context, this.View.ParentFormView.BillBusinessInfo.GetForm().Id, "1");
			HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			foreach (FieldFuncControlInfo fieldFuncControlInfo in fieldFuncControlInfos)
			{
				hashSet.Add(fieldFuncControlInfo.FieldKey);
			}
			List<EnumItem> list = new List<EnumItem>();
			foreach (Field field in this.metadata.BusinessInfo.GetFieldList())
			{
				if (field.Key == "FBOMIDCoby")
				{
					this._billName = field.Name;
				}
				if (this.IsParent)
				{
					if ((field.FunControl & Field.FUNCONTROL_BULK_EDIT) == Field.FUNCONTROL_BULK_EDIT || StringUtils.EqualsIgnoreCase(field.Key, "FName"))
					{
						Appearance appearance = this.metadata.GetLayoutInfo().GetAppearance(field.Key);
						if ((appearance == null || ((appearance.Visible & 128) == 128 && (appearance.Locked & 2) != 2 && (!(this.metadata.Id == "BD_MATERIAL") || !(field.Key == "FBaseUnitId")))) && !hashSet.Contains(field.Key))
						{
							this._lstFields.Add(field);
							list.Add(new EnumItem
							{
								Value = field.Key,
								Caption = field.Name
							});
						}
					}
				}
				else if (array.Contains(field.Key))
				{
					this._lstFields.Add(field);
					list.Add(new EnumItem
					{
						Value = field.Key,
						Caption = field.Name
					});
				}
			}
			list = (from p in list
			orderby p.Caption[this.View.Context.UserLocale.LCID]
			select p).ToList<EnumItem>();
			if (list.Count != 0)
			{
				this.selectedFielKey = list.FirstOrDefault<EnumItem>().Value;
			}
			this.View.GetControl<ComboFieldEditor>("FCombo").SetComboItems(list);
		}

		// Token: 0x0600073C RID: 1852 RVA: 0x00054E1C File Offset: 0x0005301C
		public override void AfterBindData(EventArgs e)
		{
			if (this.View.Context.ClientType == 2 || this.View.Context.ClientType == 1)
			{
				this.View.GetControl("FCombo").SetCustomPropertyValue("Editable", true);
			}
			if (!StringUtils.IsEmpty(this.selectedFielKey))
			{
				this.AddCtrlForm(this.selectedFielKey);
				this.Model.SetValue("FCombo", this.selectedFielKey);
				this.View.UpdateView("FCombo");
			}
			base.AfterBindData(e);
		}

		// Token: 0x0600073D RID: 1853 RVA: 0x00054EB8 File Offset: 0x000530B8
		public override void DataChanged(DataChangedEventArgs e)
		{
			if (!(e.Field.Key == "FCombo"))
			{
				if (e.Field.Key == "FRadioGroup")
				{
					int num = (int)Convert.ToInt16(e.NewValue.ToString());
					if (num == 1)
					{
						if (!this.IsFilterEntity)
						{
							this.View.ShowErrMessage(ResManager.LoadKDString("请在列表过滤条件中勾选【子项明细】！", "0151515153499030038964", 7, new object[0]), "", 0);
							this.View.Model.SetValue("FRadioGroup", 0);
							this.View.Model.SetValue("FParentItem", 0);
							this.IsParent = true;
						}
						this.IsParent = false;
					}
					else
					{
						this.IsParent = true;
					}
					this.AddFiledKey();
					if (!StringUtils.IsEmpty(this.selectedFielKey))
					{
						this.Model.SetValue("FCombo", this.selectedFielKey);
						this.View.UpdateView("FCombo");
					}
				}
				return;
			}
			this.selectedFielKey = e.NewValue.ToString();
			this.AddCtrlForm(e.NewValue.ToString());
			this.field = e.Field;
			if (this.selectedFielKey == "FSTOCKID")
			{
				this.View.GetControl("FLabel11").Text = ResManager.LoadKDString("默认发料仓位", "015072000014382", 7, new object[0]);
				return;
			}
			if (this.selectedFielKey == "FISSUETYPE")
			{
				this.View.GetControl("FLabel11").Text = ResManager.LoadKDString("倒冲时机", "015072000014379", 7, new object[0]);
				return;
			}
			if (this.selectedFielKey == "FOWNERTYPEID")
			{
				this.View.GetControl("FLabel11").Text = "货主";
				return;
			}
			this.View.GetControl("FLabel11").Text = "";
		}

		// Token: 0x0600073E RID: 1854 RVA: 0x000550B8 File Offset: 0x000532B8
		private void AddCtrlForm(string strKey)
		{
			if (!string.IsNullOrWhiteSpace(this._fieldPageid))
			{
				IDynamicFormView view = this.View.GetView(this._fieldPageid);
				if (view != null)
				{
					view.Close();
					this.View.SendDynamicFormAction(view);
				}
			}
			if (!string.IsNullOrWhiteSpace(strKey))
			{
				DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
				if (!this.IsParent)
				{
					dynamicFormShowParameter.FormId = "ENG_BOMChangeFieldContent";
				}
				else
				{
					dynamicFormShowParameter.FormId = "BD_ChangeFieldContent";
				}
				dynamicFormShowParameter.ParentPageId = this.View.OpenParameter.PageId;
				dynamicFormShowParameter.OpenStyle.ShowType = 3;
				dynamicFormShowParameter.OpenStyle.TagetKey = "FPanel";
				this._fieldPageid = Guid.NewGuid().ToString();
				dynamicFormShowParameter.PageId = this._fieldPageid;
				dynamicFormShowParameter.CustomParams.Add("FormId", this.View.ParentFormView.BillBusinessInfo.GetForm().Id);
				dynamicFormShowParameter.CustomParams.Add("FieldKey", strKey);
				dynamicFormShowParameter.CustomParams.Add("EntryName", this.IsParent ? "" : "FTreeEntity");
				dynamicFormShowParameter.CustomParams.Add("BOMBILLNO", this._billName);
				this.View.ShowForm(dynamicFormShowParameter);
			}
		}

		// Token: 0x0600073F RID: 1855 RVA: 0x00055228 File Offset: 0x00053428
		public override void ButtonClick(ButtonClickEventArgs e)
		{
			if (this.selectedFielKey == "FSTOCKID" && !(this.View.GetView(this._fieldPageid).Model.GetValue("FSTOCKLOCID") is DynamicObject))
			{
				this.View.GetView(this._fieldPageid).Model.SetValue("FSTOCKLOCID", null);
				this.View.GetView(this._fieldPageid).UpdateView("FSTOCKLOCID");
			}
			string key;
			if ((key = e.Key) != null)
			{
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
					if (string.IsNullOrWhiteSpace(this.selectedFielKey))
					{
						this.View.ShowErrMessage(ResManager.LoadKDString("请选择所需要批量修改的字段", "0151515153499000021266", 7, new object[0]), "", 0);
						return;
					}
					string text = ResManager.LoadKDString("您确定要对所选字段进行批量修改？", "002014030004369", 2, new object[0]);
					this.View.ShowMessage(text, 1, delegate(MessageBoxResult cfm)
					{
						if (cfm == 1)
						{
							this.View.GetView(this._fieldPageid).InvokeFormOperation(5);
						}
					}, "", 0);
					return;
				}
			}
		}

		// Token: 0x06000740 RID: 1856 RVA: 0x00055352 File Offset: 0x00053552
		private void ListRefresh()
		{
			this.View.ParentFormView.Refresh();
			this.View.SendDynamicFormAction(this.View.ParentFormView);
		}

		// Token: 0x06000741 RID: 1857 RVA: 0x0005537A File Offset: 0x0005357A
		public override void BeforeClosed(BeforeClosedEventArgs e)
		{
			this.ListRefresh();
			base.BeforeClosed(e);
		}

		// Token: 0x06000742 RID: 1858 RVA: 0x0005538C File Offset: 0x0005358C
		private void Save()
		{
			object obj = null;
			obj = this.Model.GetValue(this.field.Key);
			FormMetadata formMetadata = MetaDataServiceHelper.Load(base.Context, "ENG_BOM", true) as FormMetadata;
			DynamicObject[] array = BusinessDataServiceHelper.Load(base.Context, this.fPIdsDic.ToArray(), formMetadata.BusinessInfo.GetDynamicObjectType());
			if (array != null && array.Count<DynamicObject>() > 0)
			{
				foreach (DynamicObject dynamicObject in array)
				{
					DynamicObjectCollection dynamicObjectCollection = (DynamicObjectCollection)dynamicObject["TreeEntity"];
					foreach (DynamicObject dynamicObject2 in dynamicObjectCollection)
					{
						DynamicObject dynamicObject3 = dynamicObject2;
						dynamicObject3["MEMO"] = obj;
					}
				}
				try
				{
					OperateOption operateOption = OperateOption.Create();
					OperateOptionUtils.SetIgnoreWarning(operateOption, true);
					OperateOptionExt.SetIgnoreInteractionFlag(operateOption, true);
					operateOption.SetVariableValue("IsBatchEdit", true);
					operateOption.SetVariableValue("BatchEditFields", this.selectedFielKey);
					BusinessDataServiceHelper.Save(base.Context, formMetadata.BusinessInfo, array, operateOption, "");
				}
				catch (KDException)
				{
				}
			}
		}

		// Token: 0x0400033D RID: 829
		private List<Field> _lstFields = new List<Field>();

		// Token: 0x0400033E RID: 830
		private FormMetadata metadata;

		// Token: 0x0400033F RID: 831
		private string selectedFielKey;

		// Token: 0x04000340 RID: 832
		private string _fieldPageid = string.Empty;

		// Token: 0x04000341 RID: 833
		private Field field;

		// Token: 0x04000342 RID: 834
		private List<string> fPIdsDic = new List<string>();

		// Token: 0x04000343 RID: 835
		private Dictionary<long, List<long>> fPKEntryIdsDic = new Dictionary<long, List<long>>();

		// Token: 0x04000344 RID: 836
		private bool IsFilterEntity;

		// Token: 0x04000345 RID: 837
		private bool IsParent = true;

		// Token: 0x04000346 RID: 838
		private LocaleValue _billName;
	}
}
