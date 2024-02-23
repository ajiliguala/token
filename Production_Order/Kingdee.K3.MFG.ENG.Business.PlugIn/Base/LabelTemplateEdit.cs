using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using Kingdee.BOS;
using Kingdee.BOS.Core.Attachment;
using Kingdee.BOS.Core.Bill.PlugIn.Args;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.BarElement;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Core.Metadata.FormElement;
using Kingdee.BOS.Core.NotePrint;
using Kingdee.BOS.JSON;
using Kingdee.BOS.Model.ListFilter;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.ServiceHelper.Report;
using Kingdee.BOS.Util;
using Kingdee.BOS.Web.List;
using Kingdee.K3.Core.MFG.Utils;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.Base
{
	// Token: 0x02000025 RID: 37
	[Description("标签模板 - 单据插件")]
	public class LabelTemplateEdit : BaseControlEdit
	{
		// Token: 0x060002CD RID: 717 RVA: 0x00020A94 File Offset: 0x0001EC94
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
			base.View.GetControl<TabControl>("FTabPanel").SetFireSelChanged(true);
			this.InitialFilterMetaData();
			this.InitialFilterGrid();
			this._rptFilterFields = new List<string>
			{
				"FBoxNumber"
			};
		}

		// Token: 0x060002CE RID: 718 RVA: 0x00020AE4 File Offset: 0x0001ECE4
		public override void BeforeUpdateValue(BeforeUpdateValueEventArgs e)
		{
			if (this.isClose)
			{
				return;
			}
			string a;
			if ((a = e.Key.ToUpper()) != null)
			{
				if (!(a == "FFILTERGRID"))
				{
					if (!(a == "FTEMPLATETYPE"))
					{
						if (!(a == "FAPPLYBILL"))
						{
							return;
						}
						if ("B".Equals(Convert.ToString(base.View.Model.GetValue("FTemplateType"))))
						{
							this.BindNoteTemplates(Convert.ToString(e.Value));
						}
					}
					else if ("B".Equals(Convert.ToString(e.Value)))
					{
						this.BindNoteTemplates(Convert.ToString(base.View.Model.GetValue("FApplyBill")));
						return;
					}
				}
				else
				{
					string a2 = Convert.ToString(base.View.Model.DataObject["DocumentStatus"]);
					if (a2 != "B" && a2 != "C")
					{
						this._listFilterModel.PostBack(CommonFilterConst.ControlKey_FilterGrid, e.Value, 0);
						this._listFilterModel.Load(this._listFilterModel.SchemeEntity);
						string strFilterSetting = (e.Value != null) ? e.Value.ToString() : "";
						this.AnalyzeFilterSetting(strFilterSetting);
						return;
					}
				}
			}
		}

		// Token: 0x060002CF RID: 719 RVA: 0x00020C3C File Offset: 0x0001EE3C
		private void InitialFilterMetaData()
		{
			if (this._filterMetaData == null)
			{
				this._filterMetaData = CommonFilterServiceHelper.GetFilterMetaData(base.Context, "");
				JSONObject jsonobject = this._filterMetaData.ToJSONObject();
				jsonobject.TryGetValue(CommonFilterConst.JSONKey_CompareTypes, out this._compareTypes);
				jsonobject.TryGetValue(CommonFilterConst.JSONKey_Logics, out this._logicData);
			}
		}

		// Token: 0x060002D0 RID: 720 RVA: 0x00020C98 File Offset: 0x0001EE98
		private void InitialFilterGrid()
		{
			FilterGrid control = base.View.GetControl<FilterGrid>("FFilterGrid");
			control.SetCompareTypes(this._compareTypes);
			control.SetLogicData(this._logicData);
		}

		// Token: 0x060002D1 RID: 721 RVA: 0x00020CCE File Offset: 0x0001EECE
		private void FillFilterGridData(string formid)
		{
			this._listFilterModel = this.CreateListFilterModel(formid);
			this.InitialFilterGrid(this._listFilterModel);
		}

		// Token: 0x060002D2 RID: 722 RVA: 0x00020CEC File Offset: 0x0001EEEC
		private ListFilterModel CreateListFilterModel(string formid)
		{
			ListFilterModel listFilterModel = new ListFilterModel();
			FormMetadata cachedFormMetaData = FormMetaDataCache.GetCachedFormMetaData(base.Context, formid);
			listFilterModel.FilterObject.FilterMetaData = this._filterMetaData;
			listFilterModel.SetContext(base.Context, cachedFormMetaData.BusinessInfo, cachedFormMetaData.BusinessInfo.GetForm().GetFormServiceProvider(false));
			listFilterModel.InitFieldList(cachedFormMetaData, null);
			string selectEntryKey = this.GetSelectEntryKey(cachedFormMetaData.BusinessInfo);
			listFilterModel.FilterObject.SetSelectEntity(selectEntryKey);
			return listFilterModel;
		}

		// Token: 0x060002D3 RID: 723 RVA: 0x00020D64 File Offset: 0x0001EF64
		private void InitialFilterGrid(ListFilterModel listFilterModel)
		{
			FilterGrid control = base.View.GetControl<FilterGrid>("FFilterGrid");
			control.SetFilterFields(listFilterModel.FilterObject.GetAllFilterFieldList());
			string selectEntryKey = this.GetSelectEntryKey(listFilterModel.BusinessInfo);
			control.SetSelectEntities(selectEntryKey);
			DynamicObject dynamicObject = this.Model.GetValue("FUseOrgId") as DynamicObject;
			long num = 0L;
			if (dynamicObject != null)
			{
				num = Convert.ToInt64(dynamicObject["Id"]);
			}
			this._listFilterModel.IsolationOrgId = num;
			List<long> list = new List<long>();
			list.Add(num);
			this._listFilterModel.IsolationOrgList = list;
			string setting = base.View.Model.GetValue("FFilterSetting") as string;
			this._listFilterModel.FilterObject.Setting = setting;
			control.SetFilterRows(this._listFilterModel.FilterObject.GetFilterRows());
		}

		// Token: 0x060002D4 RID: 724 RVA: 0x00020E5C File Offset: 0x0001F05C
		protected virtual JSONArray GetFilterFields(ListFilterModel listFilterModel)
		{
			JSONArray jsonarray = new JSONArray();
			using (List<string>.Enumerator enumerator = this._rptFilterFields.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					string fieldKey = enumerator.Current;
					jsonarray.Add(listFilterModel.FilterObject.AllFilterFieldList.Find((FilterField p) => p.Key == fieldKey).GetJSONObject());
				}
			}
			return jsonarray;
		}

		// Token: 0x060002D5 RID: 725 RVA: 0x00020EE8 File Offset: 0x0001F0E8
		public override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (base.View.OpenParameter.Status.Equals(0))
			{
				base.View.Model.DeleteEntryData("FEntityMaterialType");
				base.View.Model.DeleteEntryData("FEntityMaterial");
			}
		}

		// Token: 0x060002D6 RID: 726 RVA: 0x00020F48 File Offset: 0x0001F148
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			base.BarItemClick(e);
			string barItemKey;
			if ((barItemKey = e.BarItemKey) != null)
			{
				if (!(barItemKey == "tbButton_DownLoadLable"))
				{
					return;
				}
				this.DownLoadFiles();
			}
		}

		// Token: 0x060002D7 RID: 727 RVA: 0x00020F7C File Offset: 0x0001F17C
		private void DownLoadFiles()
		{
			DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
			dynamicFormShowParameter.FormId = "SFC_DownLoadLabelTemplate";
			dynamicFormShowParameter.ParentPageId = base.View.PageId;
			base.View.ShowForm(dynamicFormShowParameter);
		}

		// Token: 0x060002D8 RID: 728 RVA: 0x00020FB8 File Offset: 0x0001F1B8
		public override void AfterBindData(EventArgs e)
		{
			IDynamicFormView view = base.View.GetView(base.View.PageId + "_ENG_Attachment_F7");
			if (base.View.Model.GetPKValue() == null || MFGBillUtil.GetValue<string>(base.View.Model, "FDocumentStatus", -1, null, null).Equals("Z"))
			{
				if (view != null)
				{
					this.CloseAttachment();
				}
			}
			else if (view != null)
			{
				this.RefreshAttachment(view);
			}
			else
			{
				this.InitAttachment(base.View.PageId);
			}
			string formid = Convert.ToString(this.Model.DataObject["ApplyBill"]);
			this.FillFilterGridData(formid);
			this.SetFiterItemsEnable();
			if (StringUtils.EqualsIgnoreCase("B", Convert.ToString(base.View.Model.GetValue("FTemplateType"))))
			{
				string formID = Convert.ToString(base.View.Model.GetValue("FApplyBill"));
				this.BindNoteTemplates(formID);
			}
			if (Convert.ToString(base.View.Model.DataObject["PrinterConnect"]).Equals("B"))
			{
				base.View.Model.SetValue("FTemplateType", "B");
				base.View.GetControl("FTemplateType").Enabled = false;
				base.View.GetControl("FPrintCount").Visible = false;
				base.View.GetControl("FIsVerticalPrint").Visible = false;
				base.View.GetControl<Container>("FTabPanel_P2").Visible = true;
				base.View.UpdateView("FTabPanel_P2");
			}
			else
			{
				base.View.Model.SetValue("FTemplateType", "A");
				base.View.GetControl("FTemplateType").Enabled = true;
				base.View.GetControl<Container>("FTabPanel_P2").Visible = false;
				base.View.UpdateView("FTabPanel_P2");
			}
			if (Convert.ToString(base.View.Model.DataObject["TemplateType"]).Equals("B"))
			{
				base.View.GetControl<Container>("FTab1_P0").Visible = false;
				base.View.GetControl<Container>("FTabAccessory").Visible = false;
				base.View.GetControl<Container>("FTabPanel_P1").Visible = false;
				base.View.GetControl<Container>("FTabPanel_P").Visible = false;
				base.View.UpdateView("FTemplateType");
				base.View.UpdateView("FTab1_P0");
				base.View.UpdateView("FTabAccessory");
				base.View.UpdateView("FTabPanel_P1");
				base.View.UpdateView("FTabPanel_P");
			}
		}

		// Token: 0x060002D9 RID: 729 RVA: 0x00021294 File Offset: 0x0001F494
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			string a;
			if ((a = e.FieldKey.ToUpperInvariant()) != null)
			{
				if (!(a == "FFIELDNAME"))
				{
					return;
				}
				string value = MFGBillUtil.GetValue<string>(base.View.Model, "FIsFlexiblePL", -1, null, null);
				string value2 = MFGBillUtil.GetValue<string>(base.View.Model, "FApplyBill", -1, null, null);
				if (!StringUtils.IsEmpty(value2))
				{
					string b = Convert.ToString(base.View.Model.GetValue("FISMAUNALINPUT", e.Row));
					if ("A" == b)
					{
						base.View.ShowMessage(ResManager.LoadKDString("固定值类型不允许选择字段，请手工录入！", "015072030038264", 7, new object[0]), 0);
						e.Cancel = true;
						return;
					}
					this.SelectFormField(value2, value, e.FieldKey, e.Row);
				}
			}
		}

		// Token: 0x060002DA RID: 730 RVA: 0x00021430 File Offset: 0x0001F630
		public override void BeforeDoOperation(BeforeDoOperationEventArgs e)
		{
			string a;
			if ((a = e.Operation.FormOperation.Operation.ToUpperInvariant()) != null)
			{
				if (!(a == "SAVE") && !(a == "SUBMIT"))
				{
					if (!(a == "CLOSE"))
					{
						return;
					}
					this.isClose = true;
				}
				else
				{
					Entity entity = base.View.Model.BusinessInfo.GetEntity("FEntity");
					List<DynamicObject> list = (from o in base.View.Model.GetEntityDataObject(entity)
					where (o["ParaName"] == null || o["ParaName"].ToString().Equals("")) && (o["FieldName"] == null || o["FieldName"].ToString().Equals(""))
					select o).ToList<DynamicObject>();
					foreach (DynamicObject item in list)
					{
						base.View.Model.GetEntityDataObject(entity).Remove(item);
					}
					list = base.View.Model.GetEntityDataObject(entity).ToList<DynamicObject>();
					int num = 1;
					foreach (DynamicObject dynamicObject in list)
					{
						dynamicObject["SEQ"] = num++;
					}
					base.View.UpdateView("FEntity");
					string value = MFGBillUtil.GetValue<string>(base.View.Model, "FApplyScope", -1, null, null);
					if (value.Equals("B"))
					{
						Entity entity2 = base.View.Model.BusinessInfo.GetEntity("FEntityMaterial");
						List<DynamicObject> list2 = (from o in base.View.Model.GetEntityDataObject(entity2)
						where (long)o["MaterialId_Id"] == 0L
						select o).ToList<DynamicObject>();
						foreach (DynamicObject item2 in list2)
						{
							base.View.Model.GetEntityDataObject(entity2).Remove(item2);
						}
						list2 = base.View.Model.GetEntityDataObject(entity2).ToList<DynamicObject>();
						int num2 = 1;
						foreach (DynamicObject dynamicObject2 in list2)
						{
							dynamicObject2["SEQ"] = num2++;
						}
						base.View.UpdateView("FEntityMaterial");
						int num3 = (from o in base.View.Model.GetEntityDataObject(entity2)
						select o["MaterialId_Id"] into v
						where (long)v != 0L
						select v).Count<object>();
						if (num3 <= 0)
						{
							string text = ResManager.LoadKDString("适用范围=物料，适用范围-物料 页签不能为空！", "015072000012057", 7, new object[0]);
							base.View.ShowErrMessage(text, "", 0);
							e.Cancel = true;
							return;
						}
					}
					else if (value.Equals("C"))
					{
						Entity entity3 = base.View.Model.BusinessInfo.GetEntity("FEntityMaterialType");
						List<DynamicObject> list3 = (from o in base.View.Model.GetEntityDataObject(entity3)
						where (long)o["MaterialType_Id"] == 0L
						select o).ToList<DynamicObject>();
						foreach (DynamicObject item3 in list3)
						{
							base.View.Model.GetEntityDataObject(entity3).Remove(item3);
						}
						list3 = base.View.Model.GetEntityDataObject(entity3).ToList<DynamicObject>();
						int num4 = 1;
						foreach (DynamicObject dynamicObject3 in list3)
						{
							dynamicObject3["SEQ"] = num4++;
						}
						base.View.UpdateView("FEntityMaterialType");
						int num5 = (from o in base.View.Model.GetEntityDataObject(entity3)
						select o["MaterialType_Id"] into v
						where (long)v != 0L
						select v).Count<object>();
						if (num5 <= 0)
						{
							string text2 = ResManager.LoadKDString("适用范围=物料分类，适用范围-物料分类 页签不能为空！", "015072000012058", 7, new object[0]);
							base.View.ShowErrMessage(text2, "", 0);
							e.Cancel = true;
							return;
						}
					}
					string value2 = MFGBillUtil.GetValue<string>(base.View.Model, "FDocumentStatus", -1, null, null);
					if (value2.Equals("Z"))
					{
						this.isDraft = true;
					}
					if (Convert.ToString(base.View.Model.GetValue("FTemplateType")).Equals("B") && ObjectUtils.IsNullOrEmptyOrWhiteSpace(base.View.Model.GetValue("FNoteTemplate")))
					{
						string text3 = ResManager.LoadKDString("套打模板不能为空！", "015072030037042", 7, new object[0]);
						base.View.ShowErrMessage(text3, "", 0);
						e.Cancel = true;
						return;
					}
					if (Convert.ToString(base.View.Model.DataObject["PrinterConnect"]).Equals("B"))
					{
						Entity entity4 = base.View.Model.BusinessInfo.GetEntity("FEntityPrint");
						List<DynamicObject> list4 = base.View.Model.GetEntityDataObject(entity4).ToList<DynamicObject>();
						if (list4.Count <= 0)
						{
							string text4 = ResManager.LoadKDString("打印机资源列表不能为空！", "015072030037043", 7, new object[0]);
							base.View.ShowErrMessage(text4, "", 0);
							e.Cancel = true;
							return;
						}
					}
				}
			}
		}

		// Token: 0x060002DB RID: 731 RVA: 0x00021AA8 File Offset: 0x0001FCA8
		public override void AfterDoOperation(AfterDoOperationEventArgs e)
		{
			base.AfterDoOperation(e);
			string a;
			if ((a = e.Operation.Operation.ToUpperInvariant()) != null)
			{
				if (!(a == "SUBMIT"))
				{
					if (!(a == "CANCELASSIGN"))
					{
						if (!(a == "AUDIT"))
						{
							if (!(a == "UNAUDIT"))
							{
								if (a == "SAVE")
								{
									if (base.View.Model.GetPKValue() != null && this.isDraft)
									{
										this.isDraft = false;
										this.InitAttachment(base.View.PageId);
										base.View.GetControl<TabControl>("FTabPanel").SelectedIndex = 0;
									}
									base.View.UpdateView("FDocumentStatus");
								}
							}
							else if (e.OperationResult.IsSuccess)
							{
								this.SetAttBtnNoable("B");
							}
						}
						else if (e.OperationResult.IsSuccess)
						{
							this.SetAttBtnNoable("A");
						}
					}
					else if (e.OperationResult.IsSuccess)
					{
						this.SetAttBtnNoable("B");
					}
				}
				else if (e.OperationResult.IsSuccess)
				{
					this.SetAttBtnNoable("A");
				}
			}
			this.SetFiterItemsEnable();
		}

		// Token: 0x060002DC RID: 732 RVA: 0x00021BEC File Offset: 0x0001FDEC
		public override void TabItemSelectedChange(TabItemSelectedChangeEventArgs e)
		{
			base.TabItemSelectedChange(e);
			string a;
			if ((a = e.TabKey.ToUpper()) != null)
			{
				if (!(a == "FTABACCESSORY"))
				{
					return;
				}
				string value = MFGBillUtil.GetValue<string>(base.View.Model, "FDocumentStatus", -1, null, null);
				if ((base.View.Model.GetPKValue() == null || value.Equals("Z")) && !Convert.ToString(base.View.Model.DataObject["TemplateType"]).Equals("B"))
				{
					string text = ResManager.LoadKDString("保存后才能使用附件功能，请先保存单据！", "015072000012059", 7, new object[0]);
					base.View.ShowErrMessage(text, "", 0);
					return;
				}
				this.SetAttBtnInVisible();
				if (value.Equals("B") || value.Equals("C"))
				{
					this.SetAttBtnNoable("A");
					return;
				}
				this.SetAttBtnNoable("B");
			}
		}

		// Token: 0x060002DD RID: 733 RVA: 0x00021CE4 File Offset: 0x0001FEE4
		public override void BeforeSave(BeforeSaveEventArgs e)
		{
			base.BeforeSave(e);
			string value = MFGBillUtil.GetValue<string>(base.View.Model, "FApplyScope", -1, null, null);
			if (value.Equals("B"))
			{
				base.View.Model.DeleteEntryData("FEntityMaterialType");
			}
			else if (value.Equals("C"))
			{
				base.View.Model.DeleteEntryData("FEntityMaterial");
			}
			bool value2 = MFGBillUtil.GetValue<bool>(base.View.Model, "FIsFlexiblePL", -1, false, null);
			string value3 = base.View.Model.GetValue("FApplyBill").ToString();
			if (value2 && "SFC_OperationReport".Equals(value3))
			{
				base.View.Model.SetValue("FFilterSetting", null);
				base.View.Model.SetValue("FFilterString", null);
			}
		}

		// Token: 0x060002DE RID: 734 RVA: 0x00021DC7 File Offset: 0x0001FFC7
		public override void AfterSave(AfterSaveEventArgs e)
		{
		}

		// Token: 0x060002DF RID: 735 RVA: 0x00021DCC File Offset: 0x0001FFCC
		public override void DataChanged(DataChangedEventArgs e)
		{
			base.DataChanged(e);
			string a;
			if ((a = e.Field.Key.ToUpperInvariant()) != null)
			{
				if (!(a == "FISFLEXIBLEPL"))
				{
					if (!(a == "FAPPLYBILL"))
					{
						if (!(a == "FTEMPLATETYPE"))
						{
							if (!(a == "FPRINTERCONNECT"))
							{
								return;
							}
							if (Convert.ToString(e.NewValue).Equals("B"))
							{
								base.View.Model.SetValue("FTemplateType", "B");
								base.View.GetControl("FTemplateType").Enabled = false;
								base.View.GetControl("FPrintCount").Visible = false;
								base.View.GetControl("FIsVerticalPrint").Visible = false;
								base.View.GetControl<Container>("FTabPanel_P2").Visible = true;
							}
							else
							{
								base.View.Model.SetValue("FTemplateType", "A");
								base.View.GetControl("FTemplateType").Enabled = true;
								base.View.GetControl("FPrintCount").Visible = true;
								base.View.GetControl("FIsVerticalPrint").Visible = true;
								base.View.GetControl<Container>("FTabPanel_P2").Visible = false;
							}
							base.View.UpdateView("FTemplateType");
							base.View.UpdateView("FPrintCount");
							base.View.UpdateView("FIsVerticalPrint");
							base.View.UpdateView("FTabPanel_P2");
						}
						else if (!Convert.ToString(e.NewValue).Equals(Convert.ToString(e.OldValue)))
						{
							base.View.Model.DeleteEntryData("FEntity");
							base.View.UpdateView("FEntity");
							if ("A".Equals(Convert.ToString(e.NewValue)))
							{
								base.View.Model.DataObject["NoteTemplate"] = "";
								base.View.UpdateView("FNoteTemplate");
							}
							if (Convert.ToString(e.NewValue).Equals("B"))
							{
								base.View.GetControl<Container>("FTab1_P0").Visible = false;
								base.View.GetControl<Container>("FTabAccessory").Visible = false;
								base.View.GetControl<Container>("FTabPanel_P1").Visible = false;
								base.View.GetControl<Container>("FTabPanel_P").Visible = false;
							}
							else
							{
								base.View.GetControl<Container>("FTab1_P0").Visible = true;
								base.View.GetControl<Container>("FTabAccessory").Visible = true;
								base.View.GetControl<Container>("FTabPanel_P1").Visible = true;
								base.View.GetControl<Container>("FTabPanel_P").Visible = true;
							}
							base.View.UpdateView("FTab1_P0");
							base.View.UpdateView("FTabAccessory");
							base.View.UpdateView("FTabPanel_P1");
							base.View.UpdateView("FTabPanel_P");
							return;
						}
					}
					else if (!Convert.ToString(e.NewValue).Equals(Convert.ToString(e.OldValue)))
					{
						base.View.Model.DeleteEntryData("FEntity");
						base.View.UpdateView("FEntity");
						string formid = Convert.ToString(e.NewValue);
						base.View.Model.SetValue("FFilterSetting", null);
						this.FillFilterGridData(formid);
						base.View.UpdateView("FFilterGrid");
						return;
					}
				}
				else
				{
					string value = base.View.Model.GetValue("FApplyBill").ToString();
					if (Convert.ToBoolean(e.NewValue) && "SFC_OperationReport".Equals(value))
					{
						EntryEntity entryEntity = base.View.BusinessInfo.GetEntryEntity("FEntity");
						DynamicObjectCollection entityDataObject = base.View.Model.GetEntityDataObject(entryEntity);
						foreach (DynamicObject dynamicObject in entityDataObject)
						{
							string text = ObjectUtils.IsNullOrEmptyOrWhiteSpace(dynamicObject["FieldOrm"]) ? "" : dynamicObject["FieldOrm"].ToString();
							List<string> list = new List<string>
							{
								"BoxNumber",
								"TotalRptQty",
								"OptRptEntry.Lot.Number",
								"OptRptEntry.MaterialId.Name",
								"OptRptEntry.MaterialId.Number",
								"OptRptEntry.MaterialId.Specification",
								"OptRptEntry.MoNumber",
								"OptRptEntry.MoRowNumber",
								"BillNo"
							};
							if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(text) && !list.Contains(text))
							{
								dynamicObject["FieldName"] = "";
								dynamicObject["FieldOrm"] = "";
							}
						}
						base.View.UpdateView("FEntity");
						return;
					}
				}
			}
		}

		// Token: 0x060002E0 RID: 736 RVA: 0x00022318 File Offset: 0x00020518
		private void InitAttachment(string viewPageId)
		{
			Form form = base.View.BillBusinessInfo.GetForm();
			string billNo = "";
			Field billNoField = base.View.BusinessInfo.GetBillNoField();
			if (billNoField != null)
			{
				object value = base.View.Model.GetValue(billNoField.Key);
				if (value != null)
				{
					billNo = value.ToString();
				}
			}
			AttachmentKey attachmentKey = new AttachmentKey();
			attachmentKey.BillType = form.Id;
			attachmentKey.BillNo = billNo;
			attachmentKey.BillInterID = base.View.Model.GetPKValue().ToString();
			attachmentKey.OperationStatus = base.View.OpenParameter.Status;
			attachmentKey.EntryKey = " ";
			attachmentKey.EntryInterID = "-1";
			attachmentKey.RowIndex = 0;
			attachmentKey.AttachmentCountFieldKeys = new List<string>
			{
				"FAttachmentCount"
			};
			string filter = string.Format("FBILLTYPE='{0}' and FINTERID='{1}' and FENTRYKEY='{2}' and FENTRYINTERID='{3}'", new object[]
			{
				attachmentKey.BillType,
				attachmentKey.BillInterID,
				attachmentKey.EntryKey,
				attachmentKey.EntryInterID
			});
			ListShowParameter listShowParameter = new ListShowParameter();
			listShowParameter.IsLookUp = false;
			listShowParameter.CustomParams.Add("AttachmentKey", AttachmentKey.ConvertToString(attachmentKey));
			listShowParameter.OpenStyle.ShowType = 3;
			listShowParameter.OpenStyle.TagetKey = "FAccessoryPanel";
			listShowParameter.Caption = ResManager.LoadKDString("附件管理", "015072000012060", 7, new object[0]);
			listShowParameter.FormId = "ENG_Attachment";
			listShowParameter.MultiSelect = false;
			listShowParameter.PageId = string.Format("{0}_{1}_F7", viewPageId, listShowParameter.FormId);
			listShowParameter.ListFilterParameter.Filter = filter;
			listShowParameter.IsShowQuickFilter = false;
			base.View.ShowForm(listShowParameter);
		}

		// Token: 0x060002E1 RID: 737 RVA: 0x000224F0 File Offset: 0x000206F0
		private void CloseAttachment()
		{
			if (base.View.PageId != null)
			{
				string text = string.Format("{0}_{1}_F7", base.View.PageId, "ENG_Attachment");
				IDynamicFormView view = base.View.GetView(text);
				if (view != null)
				{
					view.Close();
					base.View.SendDynamicFormAction(view);
				}
			}
		}

		// Token: 0x060002E2 RID: 738 RVA: 0x00022548 File Offset: 0x00020748
		private void RefreshAttachment(IDynamicFormView attView)
		{
			Form form = base.View.BillBusinessInfo.GetForm();
			string billNo = "";
			Field billNoField = base.View.BusinessInfo.GetBillNoField();
			if (billNoField != null)
			{
				object value = base.View.Model.GetValue(billNoField.Key);
				if (value != null)
				{
					billNo = value.ToString();
				}
			}
			AttachmentKey attachmentKey = new AttachmentKey();
			attachmentKey.BillType = form.Id;
			attachmentKey.BillNo = billNo;
			attachmentKey.BillInterID = base.View.Model.GetPKValue().ToString();
			attachmentKey.OperationStatus = base.View.OpenParameter.Status;
			attachmentKey.EntryKey = " ";
			attachmentKey.EntryInterID = "-1";
			attachmentKey.RowIndex = 0;
			attachmentKey.AttachmentCountFieldKeys = new List<string>
			{
				"FAttachmentCount"
			};
			attView.OpenParameter.SetCustomParameter("AttachmentKey", AttachmentKey.ConvertToString(attachmentKey));
			string filterString = string.Format("FBILLTYPE='{0}' and FINTERID='{1}' and FENTRYKEY='{2}' and FENTRYINTERID='{3}'", new object[]
			{
				attachmentKey.BillType,
				attachmentKey.BillInterID,
				attachmentKey.EntryKey,
				attachmentKey.EntryInterID
			});
			(attView as IListView).OpenParameter.FilterParameter.FilterString = filterString;
			attView.RefreshByFilter();
			base.View.SendDynamicFormAction(attView);
		}

		// Token: 0x060002E3 RID: 739 RVA: 0x000226B0 File Offset: 0x000208B0
		private void SetAttBtnNoable(string status)
		{
			if (base.View.PageId != null)
			{
				string text = string.Format("{0}_{1}_F7", base.View.PageId, "ENG_Attachment");
				IDynamicFormView view = base.View.GetView(text);
				if (view != null)
				{
					ListView listView = (ListView)view;
					BarDataManager listMenu = listView.BillLayoutInfo.GetFormAppearance().ListMenu;
					if (status.Equals("A"))
					{
						listMenu.GetBarItem("tbNew").Enabled = 0;
						listMenu.GetBarItem("tbbtnEdit").Enabled = 0;
						listMenu.GetBarItem("tbbtnDel").Enabled = 0;
					}
					else
					{
						listMenu.GetBarItem("tbNew").Enabled = 1;
						listMenu.GetBarItem("tbbtnEdit").Enabled = 1;
						listMenu.GetBarItem("tbbtnDel").Enabled = 1;
					}
					listView.Refresh();
					base.View.SendDynamicFormAction(view);
				}
			}
		}

		// Token: 0x060002E4 RID: 740 RVA: 0x0002279C File Offset: 0x0002099C
		private void SetAttBtnInVisible()
		{
			string text = string.Format("{0}_{1}_F7", base.View.PageId, "ENG_Attachment");
			IDynamicFormView view = base.View.GetView(text);
			if (view != null)
			{
				ListView listView = (ListView)view;
				BarDataManager listMenu = listView.BillLayoutInfo.GetFormAppearance().ListMenu;
				listMenu.GetBarItem("tbBatchNew").Visible = 0;
				listMenu.GetBarItem("tbbtnClose").Visible = 0;
				listView.Refresh();
				base.View.SendDynamicFormAction(view);
			}
		}

		// Token: 0x060002E5 RID: 741 RVA: 0x000228BC File Offset: 0x00020ABC
		private void SelectFormField(string formId, string isFlexiblePL, string fieldKey, int iRow)
		{
			DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter
			{
				FormId = "ENG_BillFieldSelector",
				ParentPageId = base.View.PageId
			};
			dynamicFormShowParameter.OpenStyle.ShowType = 6;
			dynamicFormShowParameter.CustomParams["IsFlexiblePL"] = isFlexiblePL;
			dynamicFormShowParameter.CustomParams["FormId"] = formId;
			dynamicFormShowParameter.CustomParams["SelMode"] = "1";
			if (this.topNode != null && this.mapOrmName != null)
			{
				dynamicFormShowParameter.CustomComplexParams["TopNode"] = this.topNode;
				dynamicFormShowParameter.CustomComplexParams["MapOrmName"] = this.mapOrmName;
			}
			base.View.ShowForm(dynamicFormShowParameter, delegate(FormResult result)
			{
				if (result.ReturnData is object[])
				{
					object[] array = result.ReturnData as object[];
					this.Model.SetValue("FFieldOrm", array[0].ToString(), iRow);
					this.Model.SetValue(fieldKey, array[1].ToString(), iRow);
					this.topNode = (array[2] as TreeNode);
					this.mapOrmName = (array[3] as Dictionary<string, string[]>);
				}
			});
		}

		// Token: 0x060002E6 RID: 742 RVA: 0x000229A0 File Offset: 0x00020BA0
		private string GetSelectEntryKey(BusinessInfo BillBusinessInfo)
		{
			StringBuilder stringBuilder = new StringBuilder(",");
			foreach (Entity entity in BillBusinessInfo.Entrys)
			{
				stringBuilder.AppendFormat("{0},", entity.Key);
			}
			return stringBuilder.ToString();
		}

		// Token: 0x060002E7 RID: 743 RVA: 0x00022A10 File Offset: 0x00020C10
		public override void BeforeFilterGridF7Select(BeforeFilterGridF7SelectEventArgs e)
		{
			e.CommonFilterModel = this._listFilterModel;
		}

		// Token: 0x060002E8 RID: 744 RVA: 0x00022A20 File Offset: 0x00020C20
		private void AnalyzeFilterSetting(string strFilterSetting)
		{
			if (StringUtils.IsEmpty(strFilterSetting))
			{
				return;
			}
			if (this._listFilterModel == null)
			{
				return;
			}
			if (this._listFilterModel.FilterObject.AllFilterFieldList == null)
			{
				return;
			}
			this._listFilterModel.FilterObject.Setting = strFilterSetting;
			string filterSQLString = this._listFilterModel.FilterObject.GetFilterSQLString(base.Context, this.GetUserNow(this._listFilterModel.FilterObject));
			this.Model.SetValue("FFilterSetting", this._listFilterModel.FilterObject.Setting);
			this.Model.SetValue("FFilterString", filterSQLString);
		}

		// Token: 0x060002E9 RID: 745 RVA: 0x00022ABC File Offset: 0x00020CBC
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
				return new DateTime?(TimeServiceHelper.GetUserDateTime(base.Context));
			}
			return null;
		}

		// Token: 0x060002EA RID: 746 RVA: 0x00022B60 File Offset: 0x00020D60
		private void SetFiterItemsEnable()
		{
			string a = Convert.ToString(base.View.Model.DataObject["DocumentStatus"]);
			if (a == "B" || a == "C")
			{
				base.View.GetControl("FFilterGrid").Enabled = false;
				return;
			}
			base.View.GetControl("FFilterGrid").Enabled = true;
		}

		// Token: 0x060002EB RID: 747 RVA: 0x00022BE4 File Offset: 0x00020DE4
		private void BindNoteTemplates(string formID)
		{
			ComboFieldEditor comboFieldEditor = base.View.GetFieldEditor("FNoteTemplate", 0) as ComboFieldEditor;
			comboFieldEditor.SetComboItems((from x in this.GetValidNoteTemplates(base.Context, formID)
			where x != null && x.TemplateEnable
			select x).ToList<EnumItem>());
		}

		// Token: 0x060002EC RID: 748 RVA: 0x00022C44 File Offset: 0x00020E44
		private List<LabelTemplateEdit.TemplateEnumItem> GetValidNoteTemplates(Context ctx, string formID)
		{
			string text = string.Empty;
			if (base.View.ParentFormView is IListView)
			{
				text = base.View.ParentFormView.BillBusinessInfo.GetForm().Note;
			}
			else
			{
				text = base.View.ParentFormView.BusinessInfo.GetForm().Note;
			}
			string text2 = ObjectUtils.Object2String(base.View.Model.GetValue("FChkShowDefault"));
			List<LabelTemplateEdit.TemplateEnumItem> list = new List<LabelTemplateEdit.TemplateEnumItem>();
			LabelTemplateEdit.TemplateEnumItem item = new LabelTemplateEdit.TemplateEnumItem(new DynamicObject(EnumItem.EnumItemType))
			{
				EnumId = "empty",
				Value = "empty",
				Caption = new LocaleValue(string.Empty, ctx.UserLocale.LCID),
				TemplateEnable = true
			};
			list.Insert(0, item);
			PrintTemplateSetting printTemplateSetting = PrintServiceHelper.GetPrintTemplateSetting(ctx, formID, ctx.UserId);
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(" select");
			stringBuilder.Append(" a.fid, lang.fname, ext.flocaleid , ext.fstatus,ext.fdisabled, a.fpackageid from t_meta_objecttype a ");
			stringBuilder.Append(" left join t_meta_objecttype_l lang on a.fid = lang.fid");
			stringBuilder.Append(" left join T_META_NOTEPRINTEXTEND ext on a.fid = ext.fid");
			stringBuilder.Append(" left join t_bos_installedpackage pkg on a.fpackageid = pkg.fpkgid");
			stringBuilder.Append(" where a.fmodeltypeid = " + 600);
			stringBuilder.Append(" and a.FBaseObjectID = @FBaseObjectID");
			stringBuilder.Append(" and  (ext.flocaleid is null or ext.flocaleid = " + base.Context.UserLocale.LCID + ")");
			stringBuilder.Append(" and lang.flocaleid = " + base.Context.UserLocale.LCID);
			if (text2.Equals("true", StringComparison.OrdinalIgnoreCase))
			{
				stringBuilder.Append(" and (a.fpackageid <> 'K3Cloud_ERP' or a.fpackageid is NULL)");
				stringBuilder.Append(" and (pkg.fisvid <> 'Kingdee' or pkg.fisvid is NULL)");
			}
			stringBuilder.Append(" order by fname asc");
			List<SqlParam> list2 = new List<SqlParam>();
			list2.Add(new SqlParam("@FBaseObjectID", 16, formID));
			using (IDataReader dataReader = DBServiceHelper.ExecuteReader(base.View.Context, stringBuilder.ToString(), list2))
			{
				while (dataReader.Read())
				{
					string @string = DBReaderUtils.GetString(dataReader, "FId");
					bool flag = dataReader["fdisabled"] is DBNull || DBReaderUtils.GetInt(dataReader, "fdisabled") == 0;
					if (printTemplateSetting != null)
					{
						flag &= printTemplateSetting.NoteTemplateEnable(@string);
					}
					string string2 = DBReaderUtils.GetString(dataReader, "FName");
					LabelTemplateEdit.TemplateEnumItem templateEnumItem = new LabelTemplateEdit.TemplateEnumItem(new DynamicObject(EnumItem.EnumItemType))
					{
						EnumId = @string,
						Value = @string,
						TemplateEnable = flag
					};
					if (!string.IsNullOrWhiteSpace(text) && text.Equals(@string))
					{
						templateEnumItem.Caption = new LocaleValue(string2 + ResManager.LoadKDString("(默认)", "015072030041392", 7, new object[0]), ctx.UserLocale.LCID);
					}
					else
					{
						templateEnumItem.Caption = new LocaleValue(string2, ctx.UserLocale.LCID);
					}
					list.Add(templateEnumItem);
				}
			}
			return list;
		}

		// Token: 0x04000158 RID: 344
		private TreeNode topNode;

		// Token: 0x04000159 RID: 345
		private Dictionary<string, string[]> mapOrmName;

		// Token: 0x0400015A RID: 346
		private bool isDraft;

		// Token: 0x0400015B RID: 347
		private object _compareTypes;

		// Token: 0x0400015C RID: 348
		private object _logicData;

		// Token: 0x0400015D RID: 349
		private ListFilterModel _listFilterModel;

		// Token: 0x0400015E RID: 350
		private FilterMetaData _filterMetaData;

		// Token: 0x0400015F RID: 351
		private List<string> _rptFilterFields;

		// Token: 0x04000160 RID: 352
		private bool isClose;

		// Token: 0x02000026 RID: 38
		internal class TemplateEnumItem : EnumItem
		{
			// Token: 0x060002F6 RID: 758 RVA: 0x00022F8C File Offset: 0x0002118C
			public TemplateEnumItem()
			{
			}

			// Token: 0x060002F7 RID: 759 RVA: 0x00022F94 File Offset: 0x00021194
			public TemplateEnumItem(DynamicObject obj) : base(obj)
			{
			}

			// Token: 0x04000169 RID: 361
			public bool TemplateEnable;
		}
	}
}
