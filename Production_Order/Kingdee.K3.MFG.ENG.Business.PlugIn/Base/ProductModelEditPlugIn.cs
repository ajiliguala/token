using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Kingdee.BOS;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.Bill.PlugIn.Args;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.ControlElement;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Core.Metadata.FormElement;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.JSON;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Orm.Metadata.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.BOS.VerificationHelper.Verifiers;
using Kingdee.K3.Core.MFG;
using Kingdee.K3.Core.MFG.ENG.ProductModel;
using Kingdee.K3.Core.MFG.ENG.ProductModel.Expression;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.Core.MFG.Utils;
using Kingdee.K3.MFG.ServiceHelper.ENG;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.Base
{
	// Token: 0x02000030 RID: 48
	[Description("产品模型插件")]
	public class ProductModelEditPlugIn : BaseControlEdit
	{
		// Token: 0x17000017 RID: 23
		// (get) Token: 0x06000334 RID: 820 RVA: 0x0002589B File Offset: 0x00023A9B
		public string BomConfigView_pageId
		{
			get
			{
				return string.Format("{0}_BomConfigView", base.View.PageId);
			}
		}

		// Token: 0x17000018 RID: 24
		// (get) Token: 0x06000335 RID: 821 RVA: 0x000258B4 File Offset: 0x00023AB4
		public Entity VariableShowEntity
		{
			get
			{
				if (this.variableShowEntity == null)
				{
					FormMetadata formMetadata = (FormMetadata)MetaDataServiceHelper.Load(base.View.Context, "ENG_MODELVARIABLESHOW", true);
					this.variableShowEntity = formMetadata.BusinessInfo.GetEntity("FEntity");
				}
				return this.variableShowEntity;
			}
		}

		// Token: 0x06000336 RID: 822 RVA: 0x00025904 File Offset: 0x00023B04
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
			this.dicNoteTypeMap = ProductModelConst.GetNoteTypeMap();
			base.View.RuleContainer.AddPluginRule("FBillHead", 3, new Action<DynamicObject, object>(this.SynMaterialId), new string[]
			{
				"FMaterialId"
			});
		}

		// Token: 0x06000337 RID: 823 RVA: 0x00025956 File Offset: 0x00023B56
		public override void PreOpenForm(PreOpenFormEventArgs e)
		{
			base.PreOpenForm(e);
			FeatureVerifier.CheckFeature(e.Context, "PDB");
		}

		// Token: 0x06000338 RID: 824 RVA: 0x0002596F File Offset: 0x00023B6F
		public override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			this.LoadConfigView();
			this.SetLeftTree();
		}

		// Token: 0x06000339 RID: 825 RVA: 0x00025984 File Offset: 0x00023B84
		public override void BeforeSetItemValueByNumber(BeforeSetItemValueByNumberArgs e)
		{
			string baseDataFieldKey;
			if ((baseDataFieldKey = e.BaseDataFieldKey) != null)
			{
				if (baseDataFieldKey == "FBillCodeRule")
				{
					e.IsShowApproved = false;
					return;
				}
				if (!(baseDataFieldKey == "FAssistantDataValueId"))
				{
					return;
				}
				DynamicObject entityDataObject = this.Model.GetEntityDataObject(base.View.BusinessInfo.GetEntity("FVarValueSet"), e.Row);
				DynamicObject dynamicObject = entityDataObject.Parent as DynamicObject;
				DynamicObject dynamicValue = DataEntityExtend.GetDynamicValue<DynamicObject>(dynamicObject, "ModelVarId", null);
				string dynamicValue2 = DataEntityExtend.GetDynamicValue<string>(dynamicValue, "VarSource_Id", null);
				string text = string.Format(" FId ='{0}' ", dynamicValue2);
				e.Filter += (ObjectUtils.IsNullOrEmptyOrWhiteSpace(e.Filter) ? text : (" And" + text));
			}
		}

		// Token: 0x0600033A RID: 826 RVA: 0x00025D44 File Offset: 0x00023F44
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			base.BeforeF7Select(e);
			string fieldKey;
			switch (fieldKey = e.FieldKey)
			{
			case "FModelVarId":
			{
				string variableFiltering = this.GetVariableFiltering();
				if (string.IsNullOrWhiteSpace(e.ListFilterParameter.Filter))
				{
					e.ListFilterParameter.Filter = variableFiltering;
					return;
				}
				if (!string.IsNullOrWhiteSpace(variableFiltering))
				{
					e.ListFilterParameter.Filter = e.ListFilterParameter.Filter + " AND " + variableFiltering;
					return;
				}
				break;
			}
			case "FDefaultValue":
			case "FVarDefaultValue":
			{
				string varType = this.GetVarType(e.FieldKey, e.Row);
				string valueSource = string.Empty;
				if (StringUtils.EqualsIgnoreCase(varType, "A") || StringUtils.EqualsIgnoreCase(varType, "B"))
				{
					valueSource = this.GetValueSource(e.Row);
				}
				this.SetDefaultValue(varType, valueSource, e.FieldKey, e.Row);
				return;
			}
			case "FShowText":
				e.Cancel = true;
				this.SelModelRuleData(e.Row);
				return;
			case "FMtrlFormat":
			{
				if (!MFGBillUtil.GetValue<bool>(base.View.Model, "FIsChooseMtrl", -1, false, null))
				{
					base.View.ShowMessage(ResManager.LoadKDString("需勾选物料编码才能编绎格式！", "015072000025055", 7, new object[0]), 0);
					return;
				}
				long value = MFGBillUtil.GetValue<long>(base.View.Model, "FMtrExpsId", -1, 0L, null);
				this.ShowExpsForm(value, string.Empty, delegate(FormResult result)
				{
					if (result != null && result.ReturnData != null)
					{
						DynamicObject dynamicObject3 = result.ReturnData as DynamicObject;
						long dynamicValue4 = DataEntityExtend.GetDynamicValue<long>(dynamicObject3, "Id", 0L);
						string text2 = this.ConvertFomatDataToString(dynamicObject3);
						this.View.Model.SetValue(e.FieldKey, text2);
						this.View.Model.SetValue("FMtrExpsId", dynamicValue4);
					}
				});
				return;
			}
			case "FSpcFormat":
			{
				if (!MFGBillUtil.GetValue<bool>(base.View.Model, "FIsChooseSpc", -1, false, null))
				{
					base.View.ShowMessage(ResManager.LoadKDString("需勾选规格型号才能编绎格式！", "015072000014122", 7, new object[0]), 0);
					return;
				}
				long value2 = MFGBillUtil.GetValue<long>(base.View.Model, "FSpcExpsId", -1, 0L, null);
				this.ShowExpsForm(value2, string.Empty, delegate(FormResult result)
				{
					if (result != null && result.ReturnData != null)
					{
						DynamicObject dynamicObject3 = result.ReturnData as DynamicObject;
						long dynamicValue4 = DataEntityExtend.GetDynamicValue<long>(dynamicObject3, "Id", 0L);
						string text2 = this.ConvertFomatDataToString(dynamicObject3);
						this.View.Model.SetValue(e.FieldKey, text2);
						this.View.Model.SetValue("FSpcExpsId", dynamicValue4);
					}
				});
				return;
			}
			case "FBOMFormat":
			{
				if (!MFGBillUtil.GetValue<bool>(base.View.Model, "FIsChooseBOM", -1, false, null))
				{
					base.View.ShowMessage(ResManager.LoadKDString("需勾选BOM版本才能编绎格式！", "015072000014123", 7, new object[0]), 0);
					return;
				}
				long value3 = MFGBillUtil.GetValue<long>(base.View.Model, "FBOMExpsId", -1, 0L, null);
				this.ShowExpsForm(value3, string.Empty, delegate(FormResult result)
				{
					if (result != null && result.ReturnData != null)
					{
						DynamicObject dynamicObject3 = result.ReturnData as DynamicObject;
						long dynamicValue4 = DataEntityExtend.GetDynamicValue<long>(dynamicObject3, "Id", 0L);
						string text2 = this.ConvertFomatDataToString(dynamicObject3);
						this.View.Model.SetValue(e.FieldKey, text2);
						this.View.Model.SetValue("FBOMExpsId", dynamicValue4);
					}
				});
				return;
			}
			case "FNameFormat":
			{
				if (!MFGBillUtil.GetValue<bool>(base.View.Model, "FIsChooseName", -1, false, null))
				{
					base.View.ShowMessage(ResManager.LoadKDString("需勾选BOM简称才能编绎格式！", "015072000014124", 7, new object[0]), 0);
					return;
				}
				long value4 = MFGBillUtil.GetValue<long>(base.View.Model, "FNameExpsId", -1, 0L, null);
				this.ShowExpsForm(value4, string.Empty, delegate(FormResult result)
				{
					if (result != null && result.ReturnData != null)
					{
						DynamicObject dynamicObject3 = result.ReturnData as DynamicObject;
						long dynamicValue4 = DataEntityExtend.GetDynamicValue<long>(dynamicObject3, "Id", 0L);
						string text2 = this.ConvertFomatDataToString(dynamicObject3);
						this.View.Model.SetValue(e.FieldKey, text2);
						this.View.Model.SetValue("FNameExpsId", dynamicValue4);
					}
				});
				return;
			}
			case "FPlanAuxFormat":
			{
				if (!MFGBillUtil.GetValue<bool>(base.View.Model, "FIsEnable", e.Row, false, null))
				{
					DynamicObjectCollection dynamicObjectCollection = base.View.Model.DataObject["PlanEntry"] as DynamicObjectCollection;
					DynamicObject dynamicObject = dynamicObjectCollection[e.Row];
					int dynamicValue = DataEntityExtend.GetDynamicValue<int>(dynamicObject, "Seq", 0);
					base.View.ShowMessage(string.Format(ResManager.LoadKDString("第{0}分录，需勾选启用才能编辑格式！", "015072000025056", 7, new object[0]), dynamicValue), 0);
					return;
				}
				long value5 = MFGBillUtil.GetValue<long>(base.View.Model, "FAuxExpsId", e.Row, 0L, null);
				this.ShowExpsForm(value5, string.Empty, delegate(FormResult result)
				{
					if (result != null && result.ReturnData != null)
					{
						DynamicObject dynamicObject3 = result.ReturnData as DynamicObject;
						long dynamicValue4 = DataEntityExtend.GetDynamicValue<long>(dynamicObject3, "Id", 0L);
						string text2 = this.ConvertFomatDataToString(dynamicObject3);
						this.View.Model.SetValue(e.FieldKey, text2, e.Row);
						this.View.Model.SetValue("FAuxExpsId", dynamicValue4, e.Row);
					}
				});
				return;
			}
			case "FAuxPropTypeId":
			{
				if (!(this.Model.GetValue("FModelVarId", e.Row) is DynamicObject))
				{
					return;
				}
				string text = this.GetAuxPropTypeFilter();
				if (ObjectUtils.IsNullOrEmptyOrWhiteSpace(text))
				{
					return;
				}
				IRegularFilterParameter listFilterParameter = e.ListFilterParameter;
				listFilterParameter.Filter += (ObjectUtils.IsNullOrEmptyOrWhiteSpace(e.ListFilterParameter.Filter) ? text : (" And " + text));
				return;
			}
			case "FOutPut":
				this.ShowVariableForm(delegate(FormResult result)
				{
					if (result != null && result.ReturnData != null)
					{
						DynamicObject dynamicObject3 = result.ReturnData as DynamicObject;
						string dynamicValue4 = DataEntityExtend.GetDynamicValue<string>(dynamicObject3, "VarName", null);
						this.Model.SetValue("FOutPut", dynamicValue4, e.Row);
					}
				});
				return;
			case "FBillCodeRule":
				e.IsShowApproved = false;
				return;
			case "FAssistantDataValueId":
			{
				DynamicObject entityDataObject = this.Model.GetEntityDataObject(base.View.BusinessInfo.GetEntity("FVarValueSet"), e.Row);
				DynamicObject dynamicObject2 = entityDataObject.Parent as DynamicObject;
				DynamicObject dynamicValue2 = DataEntityExtend.GetDynamicValue<DynamicObject>(dynamicObject2, "ModelVarId", null);
				string dynamicValue3 = DataEntityExtend.GetDynamicValue<string>(dynamicValue2, "VarSource_Id", null);
				string text = string.Format(" FId ='{0}' ", dynamicValue3);
				IRegularFilterParameter listFilterParameter2 = e.ListFilterParameter;
				listFilterParameter2.Filter += (ObjectUtils.IsNullOrEmptyOrWhiteSpace(e.ListFilterParameter.Filter) ? text : (" And" + text));
				break;
			}

				return;
			}
		}

		// Token: 0x0600033B RID: 827 RVA: 0x000263FC File Offset: 0x000245FC
		public override void BeforeUpdateValue(BeforeUpdateValueEventArgs e)
		{
			base.BeforeUpdateValue(e);
			string key;
			if ((key = e.Key) != null)
			{
				if (!(key == "FDefaultValue") && !(key == "FVarDefaultValue"))
				{
					if (key == "FCondition")
					{
						string value = MFGBillUtil.GetValue<string>(base.View.Model, "FNodeType", e.Row, null, null);
						if (StringUtils.EqualsIgnoreCase(value, "G"))
						{
							DynamicObject switchVarialRowData = this.GetSwitchVarialRowData(e.Row);
							if (switchVarialRowData == null)
							{
								return;
							}
							string text = DataEntityExtend.GetDynamicValue<string>(switchVarialRowData, "VarType", null);
							if (StringUtils.EqualsIgnoreCase(text, "D") && !this.IsNumber(e.Value.ToString()))
							{
								base.View.ShowErrMessage(ResManager.LoadKDString("Switch节点的变量类型为数值，Case节点只能输入数值!", "015072000025058", 7, new object[0]), "", 0);
								e.Cancel = true;
							}
						}
					}
				}
				else
				{
					string text = this.GetVarType(e.Key, e.Row);
					if (StringUtils.EqualsIgnoreCase(text, "D"))
					{
						bool flag = this.IsNumber(e.Value.ToString());
						if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(e.Value) && !flag)
						{
							base.View.ShowErrMessage(ResManager.LoadKDString("类型为数值的变量，默认值只能输入数值!", "015072000025057", 7, new object[0]), "", 0);
							e.Cancel = true;
						}
					}
				}
			}
			Field field = base.View.BusinessInfo.GetField(e.Key);
			if (!e.Cancel && StringUtils.EqualsIgnoreCase(field.EntityKey, "FPlanEntry"))
			{
				string fieldKey = e.Key;
				if (StringUtils.EqualsIgnoreCase(e.Key, "FAuxPropId"))
				{
					fieldKey = base.View.Model.GetEntryCurrentFieldKey("FPlanEntry");
				}
				DynamicObjectCollection dynamicObjectCollection = base.View.Model.DataObject["PlanEntry"] as DynamicObjectCollection;
				DynamicObject dynamicObject = dynamicObjectCollection[e.Row];
				DynamicObjectCollection dynamicObjectCollection2 = dynamicObject["AuxVarEntity"] as DynamicObjectCollection;
				IEnumerable<DynamicObject> enumerable = from p in dynamicObjectCollection2
				where StringUtils.EqualsIgnoreCase(DataEntityExtend.GetDynamicValue<string>(p, "FieldKey", null), fieldKey)
				select p;
				if (!ListUtils.IsEmpty<DynamicObject>(enumerable))
				{
					dynamicObjectCollection2.Remove(enumerable.FirstOrDefault<DynamicObject>());
				}
				base.View.UpdateView("FAuxVarEntity");
			}
		}

		// Token: 0x0600033C RID: 828 RVA: 0x000266C0 File Offset: 0x000248C0
		public override void AfterBarItemClick(AfterBarItemClickEventArgs e)
		{
			string barItemKey;
			if ((barItemKey = e.BarItemKey) != null)
			{
				if (!(barItemKey == "tbCreateForm"))
				{
					return;
				}
				FormMetadata formMetadata = MetaDataServiceHelper.Load(base.Context, "ENG_MDLCFGPANEL", true) as FormMetadata;
				JSONObject jsonobject = MdlCfgServiceHelper.BuildDynFieldMdlFromPrdModeling(base.Context, DataEntityExtend.GetDynamicValue<long>(this.Model.DataObject, "Id", 0L));
				Tuple<BusinessInfo, LayoutInfo, LayoutInfo> tuple = MdlCfgServiceHelper.BuildDynPanelBusiness(base.Context, formMetadata, jsonobject);
				tuple.Item3.Appearances.ForEach(delegate(Appearance x)
				{
					((ControlAppearance)x).Container = "FPanel";
				});
				tuple.Item1.GetForm().FormPlugins.ForEach(delegate(PlugIn plugin)
				{
					plugin.IsEnabled = false;
				});
				tuple.Item1.GetForm().FormPlugins.Add(new PlugIn
				{
					IsEnabled = true,
					ClassName = "Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm.MdlCfgFixPanelEdit, Kingdee.K3.MFG.ENG.Business.PlugIn",
					OrderId = 1,
					PlugInType = 0,
					ElementType = 0
				});
				LocaleValue name = tuple.Item1.GetForm().Name;
				LocaleValue dynamicValue = DataEntityExtend.GetDynamicValue<LocaleValue>(this.Model.DataObject, "Name", null);
				string text = string.Format("{0}({1}|{2})", name[base.Context.UserLocale.LCID], dynamicValue[base.Context.UserLocale.LCID], DataEntityExtend.GetDynamicValue<string>(this.Model.DataObject, "Number", null));
				name[base.Context.UserLocale.LCID] = text;
				string dynamicValue2 = DataEntityExtend.GetDynamicValue<string>(this.Model.DataObject, "FixFormId_Id", null);
				if (ObjectUtils.IsNullOrEmptyOrWhiteSpace(dynamicValue2))
				{
					FormMetadata formMetadata2 = new FormMetadata(tuple.Item1, new List<LayoutInfo>
					{
						tuple.Item2
					});
					formMetadata2.DevType = 1;
					formMetadata2.BaseObjectId = formMetadata.Id;
					formMetadata2.Id = string.Format("MFG_MDLCFG_{0}", DataEntityExtend.GetDynamicValue<long>(this.Model.DataObject, "Id", 0L));
					formMetadata2.SubSystemId = formMetadata.SubSystemId;
					formMetadata2.ISV = formMetadata.ISV;
					formMetadata2.ModelTypeId = formMetadata.ModelTypeId;
					formMetadata2.ModelTypeSubId = formMetadata.ModelTypeSubId;
					MetaDataServiceHelper.Save(base.Context, formMetadata2);
					this.Model.SetValue("FFixFormId", formMetadata2.Id);
					return;
				}
				FormMetadata formMetadata3 = MetaDataServiceHelper.Load(base.Context, dynamicValue2, true) as FormMetadata;
				if (formMetadata3 == null)
				{
					throw new KDException("ENG_MDLCFGPANEL", "Old FixForm has been removed!");
				}
				List<Field> fieldList = tuple.Item1.GetFieldList();
				List<FieldAppearance> fieldAppearances = tuple.Item2.GetFieldAppearances();
				List<Field> list = formMetadata3.BusinessInfo.GetFieldList().ToList<Field>();
				List<FieldAppearance> list2 = formMetadata3.GetLayoutInfo().GetFieldAppearances().ToList<FieldAppearance>();
				using (List<Field>.Enumerator enumerator = list.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Field oldfield = enumerator.Current;
						Field field = (from x in fieldList
						where x.Key == oldfield.Key
						select x).FirstOrDefault<Field>();
						if (field != null)
						{
							field.Id = oldfield.Id;
							formMetadata3.BusinessInfo.Remove(oldfield);
							formMetadata3.BusinessInfo.Add(field);
							fieldList.Remove(field);
						}
						else
						{
							formMetadata3.BusinessInfo.Remove(oldfield);
						}
					}
				}
				foreach (Field field2 in fieldList)
				{
					formMetadata3.BusinessInfo.Add(field2);
				}
				using (List<FieldAppearance>.Enumerator enumerator3 = list2.GetEnumerator())
				{
					while (enumerator3.MoveNext())
					{
						FieldAppearance oldAppearance = enumerator3.Current;
						FieldAppearance fieldAppearance = (from x in fieldAppearances
						where x.Key == oldAppearance.Key
						select x).FirstOrDefault<FieldAppearance>();
						if (fieldAppearance != null)
						{
							fieldAppearance.Id = oldAppearance.Id;
							formMetadata3.GetLayoutInfo().Remove(oldAppearance);
							formMetadata3.GetLayoutInfo().Add(fieldAppearance);
							fieldAppearances.Remove(fieldAppearance);
						}
						else
						{
							formMetadata3.GetLayoutInfo().Remove(oldAppearance);
						}
					}
				}
				foreach (FieldAppearance fieldAppearance2 in fieldAppearances)
				{
					formMetadata3.GetLayoutInfo().Add(fieldAppearance2);
				}
				MetaDataServiceHelper.Save(base.Context, formMetadata3);
			}
		}

		// Token: 0x0600033D RID: 829 RVA: 0x00026BE8 File Offset: 0x00024DE8
		public override void BeforeDoOperation(BeforeDoOperationEventArgs e)
		{
			string a;
			if ((a = e.Operation.FormOperation.Operation.ToUpperInvariant()) != null)
			{
				if (!(a == "SAVE"))
				{
					return;
				}
				IDynamicFormView view = base.View.GetView(this.BomConfigView_pageId);
				if (view != null)
				{
					Dictionary<DynamicObject, DynamicObject> dictionary = new Dictionary<DynamicObject, DynamicObject>();
					dictionary.Add(this.Model.DataObject, view.Model.DataObject);
					e.Option.SetVariableValue("ProductModelBom", dictionary);
				}
			}
		}

		// Token: 0x0600033E RID: 830 RVA: 0x00026C64 File Offset: 0x00024E64
		public override void BeforeSave(BeforeSaveEventArgs e)
		{
			long num = Convert.ToInt64(base.View.Model.DataObject["Id"]);
			if (num > 0L)
			{
				IDynamicFormView view = base.View.GetView(this.BomConfigView_pageId);
				if (view != null)
				{
					(view as IDynamicFormViewService).CustomEvents("Save", "Save", num.ToString());
					base.View.SendDynamicFormAction(view);
				}
			}
		}

		// Token: 0x0600033F RID: 831 RVA: 0x00026CD4 File Offset: 0x00024ED4
		public override void AfterSave(AfterSaveEventArgs e)
		{
			long num = Convert.ToInt64(base.View.Model.DataObject["Id"]);
			if (num > 0L)
			{
				IDynamicFormView view = base.View.GetView(this.BomConfigView_pageId);
				if (view != null)
				{
					(view as IDynamicFormViewService).CustomEvents("Save", "Save", num.ToString());
					base.View.SendDynamicFormAction(view);
				}
			}
		}

		// Token: 0x06000340 RID: 832 RVA: 0x00026D44 File Offset: 0x00024F44
		public override void AfterDoOperation(AfterDoOperationEventArgs e)
		{
			base.AfterDoOperation(e);
			if (this.lstSendEvenOp.Contains(e.Operation.Operation) && e.OperationResult.IsSuccess)
			{
				IDynamicFormView view = base.View.GetView(this.BomConfigView_pageId);
				if (view != null)
				{
					string text = Convert.ToString(base.View.Model.DataObject["Id"]);
					(view as IDynamicFormViewService).CustomEvents(e.Operation.Operation, e.Operation.Operation, text);
					base.View.SendDynamicFormAction(view);
				}
			}
		}

		// Token: 0x06000341 RID: 833 RVA: 0x00026E1C File Offset: 0x0002501C
		public override void AfterCopyData(CopyDataEventArgs e)
		{
			base.AfterCopyData(e);
			long dynamicValue = DataEntityExtend.GetDynamicValue<long>(e.DataObject, "MtrExpsId", 0L);
			long dynamicValue2 = DataEntityExtend.GetDynamicValue<long>(e.DataObject, "SpcExpsId", 0L);
			long dynamicValue3 = DataEntityExtend.GetDynamicValue<long>(e.DataObject, "BOMExpsId", 0L);
			long dynamicValue4 = DataEntityExtend.GetDynamicValue<long>(e.DataObject, "NameExpsId", 0L);
			DynamicObjectCollection dynamicObjectCollection = e.DataObject["TreeEntryConfig"] as DynamicObjectCollection;
			if (ListUtils.IsEmpty<DynamicObject>(dynamicObjectCollection) && dynamicValue == 0L && dynamicValue2 == 0L && dynamicValue3 == 0L && dynamicValue4 == 0L)
			{
				return;
			}
			List<long> list = new List<long>();
			if (dynamicValue > 0L)
			{
				list.Add(dynamicValue);
			}
			if (dynamicValue2 > 0L)
			{
				list.Add(dynamicValue2);
			}
			if (dynamicValue3 > 0L)
			{
				list.Add(dynamicValue3);
			}
			if (dynamicValue4 > 0L)
			{
				list.Add(dynamicValue4);
			}
			List<long> list2 = (from p in dynamicObjectCollection
			where DataEntityExtend.GetDynamicValue<long>(p, "FNodeExpsId", 0L) > 0L
			select p into d
			select DataEntityExtend.GetDynamicValue<long>(d, "FNodeExpsId", 0L)).Distinct<long>().ToList<long>();
			if (!ListUtils.IsEmpty<long>(list2))
			{
				list.AddRange(list2);
			}
			if (ListUtils.IsEmpty<long>(list))
			{
				return;
			}
			FormMetadata formMetadata = MetaDataServiceHelper.Load(base.View.Context, "ENG_PRDMODELEXPS", true) as FormMetadata;
			DynamicObjectType dynamicObjectType = formMetadata.BusinessInfo.GetDynamicObjectType();
			DynamicObject[] source = BusinessDataServiceHelper.Load(base.View.Context, (from p in list
			select p).ToArray<object>(), dynamicObjectType);
			Dictionary<long, DynamicObject> dictionary = source.ToDictionary((DynamicObject p) => DataEntityExtend.GetDynamicValue<long>(p, "Id", 0L));
			List<DynamicObject> list3 = new List<DynamicObject>();
			foreach (DynamicObject dynamicObject in dynamicObjectCollection)
			{
				long dynamicValue5 = DataEntityExtend.GetDynamicValue<long>(dynamicObject, "FNodeExpsId", 0L);
				if (dynamicValue5 > 0L)
				{
					DynamicObject dynamicObject2 = OrmUtils.Clone(dictionary[dynamicValue5], false, true) as DynamicObject;
					DBServiceHelper.AutoSetPrimaryKey(base.View.Context, new DynamicObject[]
					{
						dynamicObject2
					}, dynamicObjectType);
					list3.Add(dynamicObject2);
					dynamicObject["FNodeExpsId"] = dynamicObject2["Id"];
				}
			}
			if (dynamicValue > 0L)
			{
				DynamicObject dynamicObject3 = OrmUtils.Clone(dictionary[dynamicValue], false, true) as DynamicObject;
				DBServiceHelper.AutoSetPrimaryKey(base.View.Context, new DynamicObject[]
				{
					dynamicObject3
				}, dynamicObjectType);
				list3.Add(dynamicObject3);
				e.DataObject["MtrExpsId"] = dynamicObject3["Id"];
			}
			if (dynamicValue2 > 0L)
			{
				DynamicObject dynamicObject4 = OrmUtils.Clone(dictionary[dynamicValue2], false, true) as DynamicObject;
				DBServiceHelper.AutoSetPrimaryKey(base.View.Context, new DynamicObject[]
				{
					dynamicObject4
				}, dynamicObjectType);
				list3.Add(dynamicObject4);
				e.DataObject["SpcExpsId"] = dynamicObject4["Id"];
			}
			if (dynamicValue3 > 0L)
			{
				DynamicObject dynamicObject5 = OrmUtils.Clone(dictionary[dynamicValue3], false, true) as DynamicObject;
				DBServiceHelper.AutoSetPrimaryKey(base.View.Context, new DynamicObject[]
				{
					dynamicObject5
				}, dynamicObjectType);
				list3.Add(dynamicObject5);
				e.DataObject["BOMExpsId"] = dynamicObject5["Id"];
			}
			if (dynamicValue4 > 0L)
			{
				DynamicObject dynamicObject6 = OrmUtils.Clone(dictionary[dynamicValue4], false, true) as DynamicObject;
				DBServiceHelper.AutoSetPrimaryKey(base.View.Context, new DynamicObject[]
				{
					dynamicObject6
				}, dynamicObjectType);
				list3.Add(dynamicObject6);
				e.DataObject["NameExpsId"] = dynamicObject6["Id"];
			}
			if (!ListUtils.IsEmpty<DynamicObject>(list3))
			{
				BusinessDataServiceHelper.Save(base.View.Context, list3.ToArray());
			}
		}

		// Token: 0x06000342 RID: 834 RVA: 0x00027290 File Offset: 0x00025490
		public override void DataChanged(DataChangedEventArgs e)
		{
			base.DataChanged(e);
			string text = string.Empty;
			string key;
			if ((key = e.Field.Key) != null)
			{
				if (!(key == "FModelVarId"))
				{
					if (!(key == "FVarType"))
					{
						if (!(key == "FMaterialId"))
						{
							if (!(key == "FOutPut"))
							{
								return;
							}
						}
						else
						{
							IDynamicFormView view = base.View.GetView(this.BomConfigView_pageId);
							if (view != null)
							{
								Convert.ToString(base.View.Model.DataObject["Id"]);
								(view as IDynamicFormViewService).CustomEvents("MaterialChange", "MaterialChange", Convert.ToString(e.NewValue));
								base.View.SendDynamicFormAction(view);
							}
							DynamicObject dynamicObject = this.Model.GetValue("FMaterialId") as DynamicObject;
							List<long> list = new List<long>();
							if (dynamicObject != null)
							{
								DynamicObjectCollection dynamicObjectCollection = dynamicObject["MaterialAuxPty"] as DynamicObjectCollection;
								if (!ListUtils.IsEmpty<DynamicObject>(dynamicObjectCollection))
								{
									list = (from x in dynamicObjectCollection
									where DataEntityExtend.GetDynamicValue<bool>(x, "IsEnable1", false)
									select DataEntityExtend.GetDynamicValue<long>(x, "AuxPropertyId_Id", 0L)).ToList<long>();
								}
							}
							DynamicObjectCollection dynamicObjectCollection2 = this.Model.DataObject["Entity"] as DynamicObjectCollection;
							using (IEnumerator<DynamicObject> enumerator = dynamicObjectCollection2.GetEnumerator())
							{
								while (enumerator.MoveNext())
								{
									DynamicObject dynamicObject2 = enumerator.Current;
									if (!list.Contains(DataEntityExtend.GetDynamicValue<long>(dynamicObject2, "AuxPropTypeId_Id", 0L)))
									{
										this.Model.SetValue("FAuxPropTypeId", 0, dynamicObjectCollection2.IndexOf(dynamicObject2));
									}
								}
								return;
							}
						}
						if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(e.NewValue))
						{
							List<DynamicObject> variableList = this.GetVariableList();
							DynamicObject dynamicObject3 = (from x in variableList
							where DataEntityExtend.GetDynamicValue<string>(x, "VarName", null) == e.NewValue.ToString()
							select x).FirstOrDefault<DynamicObject>();
							if (dynamicObject3 != null)
							{
								this.Model.SetValue("FOutPutType", DataEntityExtend.GetDynamicValue<string>(dynamicObject3, "VarType", null), e.Row);
							}
						}
					}
					else
					{
						text = this.GetVarType("FVarDefaultValue", e.Row);
						FieldEditor fieldEditor = base.View.GetFieldEditor("FVarDefaultValue", e.Row);
						if (StringUtils.EqualsIgnoreCase(text, "E"))
						{
							fieldEditor.SetCustomPropertyValue("editable", false);
							fieldEditor.SetCustomPropertyValue("showEditButton", true);
							return;
						}
						fieldEditor.SetCustomPropertyValue("editable", true);
						fieldEditor.SetCustomPropertyValue("showEditButton", false);
						return;
					}
				}
				else
				{
					text = this.GetVarType("FDefaultValue", e.Row);
					FieldEditor fieldEditor = base.View.GetFieldEditor("FDefaultValue", e.Row);
					if (StringUtils.EqualsIgnoreCase(text, "A") || StringUtils.EqualsIgnoreCase(text, "B") || StringUtils.EqualsIgnoreCase(text, "E"))
					{
						fieldEditor.SetCustomPropertyValue("editable", false);
						fieldEditor.SetCustomPropertyValue("showEditButton", true);
						return;
					}
					fieldEditor.SetCustomPropertyValue("editable", true);
					fieldEditor.SetCustomPropertyValue("showEditButton", false);
					return;
				}
			}
		}

		// Token: 0x06000343 RID: 835 RVA: 0x00027630 File Offset: 0x00025830
		public override void EntityRowClick(EntityRowClickEventArgs e)
		{
			base.EntityRowClick(e);
			string text = string.Empty;
			if (StringUtils.EqualsIgnoreCase(e.Key, "FEntity"))
			{
				text = this.GetVarType("FDefaultValue", e.Row);
				FieldEditor fieldEditor = base.View.GetFieldEditor("FDefaultValue", e.Row);
				if (StringUtils.EqualsIgnoreCase(text, "A") || StringUtils.EqualsIgnoreCase(text, "B") || StringUtils.EqualsIgnoreCase(text, "E"))
				{
					fieldEditor.SetCustomPropertyValue("editable", false);
					fieldEditor.SetCustomPropertyValue("showEditButton", true);
					return;
				}
				fieldEditor.SetCustomPropertyValue("editable", true);
				fieldEditor.SetCustomPropertyValue("showEditButton", false);
				return;
			}
			else
			{
				if (!StringUtils.EqualsIgnoreCase(e.Key, "FCalEntity"))
				{
					if (StringUtils.EqualsIgnoreCase(e.Key, "FTreeEntryConfig"))
					{
						string value = MFGBillUtil.GetValue<string>(base.View.Model, "FNodeType", e.Row, null, null);
						if (StringUtils.EqualsIgnoreCase(value, "G"))
						{
							DynamicObject switchVarialRowData = this.GetSwitchVarialRowData(e.Row);
							if (switchVarialRowData == null)
							{
								return;
							}
							text = DataEntityExtend.GetDynamicValue<string>(switchVarialRowData, "VarType", null);
							if (text.Equals("A") || text.Equals("B") || text.Equals("E"))
							{
								base.View.StyleManager.SetEnabled("FCondition", "CaseCondition", false);
								return;
							}
							base.View.StyleManager.SetEnabled("FCondition", "CaseCondition", true);
							return;
						}
						else
						{
							base.View.StyleManager.SetEnabled("FCondition", "CaseCondition", false);
						}
					}
					return;
				}
				text = this.GetVarType("FVarDefaultValue", e.Row);
				FieldEditor fieldEditor = base.View.GetFieldEditor("FVarDefaultValue", e.Row);
				if (StringUtils.EqualsIgnoreCase(text, "E"))
				{
					fieldEditor.SetCustomPropertyValue("editable", false);
					fieldEditor.SetCustomPropertyValue("showEditButton", true);
					return;
				}
				fieldEditor.SetCustomPropertyValue("editable", true);
				fieldEditor.SetCustomPropertyValue("showEditButton", false);
				return;
			}
		}

		// Token: 0x06000344 RID: 836 RVA: 0x000279B8 File Offset: 0x00025BB8
		public override void EntityRowDoubleClick(EntityRowClickEventArgs e)
		{
			base.EntityRowDoubleClick(e);
			if (StringUtils.EqualsIgnoreCase(e.Key, "FTreeEntryConfig"))
			{
				if (base.BillStatus.Equals('C'))
				{
					return;
				}
				string value = MFGBillUtil.GetValue<string>(base.View.Model, "FNodeType", e.Row, null, null);
				if (StringUtils.EqualsIgnoreCase(value, "F"))
				{
					this.ShowVariableForm(delegate(FormResult result)
					{
						if (result != null && result.ReturnData != null)
						{
							DynamicObject dynamicObject2 = result.ReturnData as DynamicObject;
							string dynamicValue3 = DataEntityExtend.GetDynamicValue<string>(dynamicObject2, "VarName", null);
							this.View.Model.SetValue("FCondition", dynamicValue3, e.Row);
						}
					});
					return;
				}
				if (StringUtils.EqualsIgnoreCase(value, "G"))
				{
					DynamicObject switchVariable = this.GetSwitchVarialRowData(e.Row);
					if (switchVariable == null)
					{
						base.View.ShowErrMessage(ResManager.LoadKDString("Swich节点的条件变量没有定义或己被变量表体移除，请检查!", "015072000025059", 7, new object[0]), "", 0);
						return;
					}
					string dynamicValue = DataEntityExtend.GetDynamicValue<string>(switchVariable, "VarType", null);
					if (!dynamicValue.Equals("A") && !dynamicValue.Equals("B") && !dynamicValue.Equals("E"))
					{
						base.View.ShowErrMessage(ResManager.LoadKDString("Switch变量为数值或文本，请填接在条件中输入对应的Case值即可!", "015072000025060", 7, new object[0]), "", 0);
						return;
					}
					DynamicObjectCollection source = base.View.Model.DataObject["Entity"] as DynamicObjectCollection;
					DynamicObject dynamicObject = source.FirstOrDefault((DynamicObject p) => DataEntityExtend.GetDynamicValue<string>(p, "ModelVarId_Id", null).Equals(DataEntityExtend.GetDynamicValue<string>(switchVariable, "VariableId", null)));
					if (dynamicObject != null)
					{
						string dynamicValue2 = DataEntityExtend.GetDynamicValue<string>(DataEntityExtend.GetDynamicValue<DynamicObject>(dynamicObject, "ModelVarId", null), "VarSource_Id", null);
						this.SetDefaultValue(dynamicValue, dynamicValue2, "FCondition", e.Row);
						return;
					}
				}
				else if (StringUtils.EqualsIgnoreCase(value, "C"))
				{
					long value2 = MFGBillUtil.GetValue<long>(base.View.Model, "FNodeExpsId", e.Row, 0L, null);
					this.ShowExpsForm(value2, "373fe6d7-a63c-4d51-9eb9-d7be603ef0e8", delegate(FormResult result)
					{
						if (result != null && result.ReturnData != null)
						{
							DynamicObject dynamicObject2 = result.ReturnData as DynamicObject;
							long dynamicValue3 = DataEntityExtend.GetDynamicValue<long>(dynamicObject2, "Id", 0L);
							DynamicObjectCollection dynamicObjectCollection = dynamicObject2["OperatioEntity"] as DynamicObjectCollection;
							List<AbstractModelExpression> operationData = ProductModelRuleServiceHelper.GetOperationData(this.View.Context, dynamicObjectCollection, false);
							if (!ListUtils.IsEmpty<AbstractModelExpression>(operationData))
							{
								string expression = operationData.FirstOrDefault<AbstractModelExpression>().Expression;
								this.View.Model.SetValue("FCondition", expression, e.Row);
							}
							this.View.Model.SetValue("FNodeExpsId", dynamicValue3, e.Row);
						}
					});
					return;
				}
			}
			else if (StringUtils.EqualsIgnoreCase(e.Key, "FEntity") && StringUtils.EqualsIgnoreCase(e.ColKey, "FDefaultValue"))
			{
				if (!(this.Model.GetValue("FModelVarId") is DynamicObject))
				{
					return;
				}
				string varType = this.GetVarType("FDefaultValue", e.Row);
				if (StringUtils.EqualsIgnoreCase(varType, "A") || StringUtils.EqualsIgnoreCase(varType, "B") || StringUtils.EqualsIgnoreCase(varType, "E"))
				{
					this.Model.SetValue("FDefaultValue", "", e.Row);
				}
			}
		}

		// Token: 0x06000345 RID: 837 RVA: 0x00027E7C File Offset: 0x0002607C
		public override void EntryBarItemClick(BarItemClickEventArgs e)
		{
			base.EntryBarItemClick(e);
			string barItemKey;
			switch (barItemKey = e.BarItemKey)
			{
			case "tbAddNewEntryRow":
				this.AddEntryTreeRow(this.currentSelectNoteId);
				return;
			case "tbIF":
				this.AddEntryTreeRow("tdIf");
				return;
			case "tbSWITCH":
				this.AddEntryTreeRow("tdSwitch");
				return;
			case "tbCASE":
				this.AddEntryTreeRow("tdCase");
				return;
			case "tbBOMItem":
				this.AddEntryTreeRow("tdBom");
				return;
			case "tbRuleMoveUp":
				this.EntryRowMoveUpOrDown("FRuleEntry", true);
				return;
			case "tbRuleMoveDown":
				this.EntryRowMoveUpOrDown("FRuleEntry", false);
				return;
			case "tbVariableSet":
			{
				int rowIndex = base.View.Model.GetEntryCurrentRowIndex("FPlanEntry");
				if (rowIndex < 0)
				{
					return;
				}
				DynamicObjectCollection dynamicObjectCollection = base.View.Model.DataObject["PlanEntry"] as DynamicObjectCollection;
				string fieldKey = base.View.Model.GetEntryCurrentFieldKey("FPlanEntry");
				Field objField = MFGFormBusinessUtil.GetFlexField(base.View.BillBusinessInfo, fieldKey);
				if (!base.View.StyleManager.GetFlexEnabled(fieldKey, dynamicObjectCollection[rowIndex]))
				{
					base.View.ShowErrMessage(ResManager.LoadKDString("该辅助属性字段没有启用!", "015072000037252", 7, new object[0]), "", 0);
					return;
				}
				this.ShowVariableForm(objField, delegate(FormResult result)
				{
					if (result != null && result.ReturnData != null)
					{
						DynamicObject dynamicObject = result.ReturnData as DynamicObject;
						string dynamicValue = DataEntityExtend.GetDynamicValue<string>(dynamicObject, "VarNumber", null);
						if (string.IsNullOrWhiteSpace(dynamicValue))
						{
							return;
						}
						DynamicObjectCollection dynamicObjectCollection2 = this.View.Model.DataObject["PlanEntry"] as DynamicObjectCollection;
						DynamicObject dynamicObject2 = dynamicObjectCollection2[rowIndex];
						object varDefaultValue = this.GetVarDefaultValue(objField);
						if (!StringUtils.EqualsIgnoreCase(objField.Key, fieldKey))
						{
							this.SetFlexFieldValue(objField, rowIndex, varDefaultValue);
						}
						else
						{
							this.View.Model.SetValue(objField.Key, varDefaultValue, rowIndex);
						}
						DynamicObjectCollection dynamicObjectCollection3 = dynamicObject2["AuxVarEntity"] as DynamicObjectCollection;
						IEnumerable<DynamicObject> enumerable = from p in dynamicObjectCollection3
						where StringUtils.EqualsIgnoreCase(DataEntityExtend.GetDynamicValue<string>(p, "FieldKey", null), fieldKey)
						select p;
						if (!ListUtils.IsEmpty<DynamicObject>(enumerable))
						{
							DynamicObject dynamicObject3 = enumerable.FirstOrDefault<DynamicObject>();
							dynamicObject3["variableKey"] = dynamicValue;
						}
						else
						{
							DynamicObject dynamicObject4 = new DynamicObject(dynamicObjectCollection3.DynamicCollectionItemPropertyType);
							dynamicObject4["FieldKey"] = fieldKey;
							dynamicObject4["FieldName"] = objField.Name.ToString();
							dynamicObject4["variableKey"] = dynamicValue;
							dynamicObjectCollection3.Add(dynamicObject4);
						}
						this.View.UpdateView("FAuxVarEntity");
					}
				});
				return;
			}
			case "tbFunctionLab":
			{
				DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter
				{
					FormId = "MFG_FUNCTIONLAB",
					ParentPageId = base.View.PageId
				};
				dynamicFormShowParameter.OpenStyle.ShowType = 0;
				base.View.ShowForm(dynamicFormShowParameter);
				break;
			}

				return;
			}
		}

		// Token: 0x06000346 RID: 838 RVA: 0x000280F0 File Offset: 0x000262F0
		public override void TreeNodeClick(TreeNodeArgs e)
		{
			base.TreeNodeClick(e);
			this.currentSelectNoteId = e.NodeId;
		}

		// Token: 0x06000347 RID: 839 RVA: 0x00028105 File Offset: 0x00026305
		public override void TreeNodeDoubleClick(TreeNodeArgs e)
		{
			base.TreeNodeDoubleClick(e);
			this.AddEntryTreeRow(e.NodeId);
			this.currentSelectNoteId = e.NodeId;
		}

		// Token: 0x06000348 RID: 840 RVA: 0x00028128 File Offset: 0x00026328
		public override void BeforeDeleteRow(BeforeDeleteRowEventArgs e)
		{
			base.BeforeDeleteRow(e);
			if (StringUtils.EqualsIgnoreCase(e.EntityKey, "FTreeEntryConfig"))
			{
				string value = MFGBillUtil.GetValue<string>(base.View.Model, "FNodeType", e.Row, null, null);
				if (!this.deleteTreeEntryRow && (value.Equals("H") || value.Equals("D") || value.Equals("E")))
				{
					base.View.ShowMessage(ResManager.LoadKDString("该节点类型不能删除!", "015072000025061", 7, new object[0]), 0);
					e.Cancel = true;
					return;
				}
				string value2 = MFGBillUtil.GetValue<string>(base.View.Model, "FRowId", e.Row, null, null);
				int entryRowCount = base.View.Model.GetEntryRowCount("FTreeEntryConfig");
				if (value.Equals("A") || value.Equals("B"))
				{
					DynamicObjectCollection dynamicObjectCollection = base.View.Model.DataObject["TreeEntryConfig"] as DynamicObjectCollection;
					DynamicObject bomEntryRow = dynamicObjectCollection[e.Row];
					if (value.Equals("A"))
					{
						this.DeleteBOMChild(bomEntryRow);
					}
					value.Equals("B");
				}
				for (int i = entryRowCount - 1; i >= 0; i--)
				{
					string value3 = MFGBillUtil.GetValue<string>(base.View.Model, "FParentRowId", i, null, null);
					if (value2.Equals(value3))
					{
						this.deleteTreeEntryRow = true;
						base.View.Model.DeleteEntryRow("FTreeEntryConfig", i);
						this.deleteTreeEntryRow = false;
					}
				}
			}
		}

		// Token: 0x06000349 RID: 841 RVA: 0x000282C1 File Offset: 0x000264C1
		public override void BeforeClosed(BeforeClosedEventArgs e)
		{
			base.BeforeClosed(e);
			this.ClearExpsData();
		}

		// Token: 0x0600034A RID: 842 RVA: 0x000282F0 File Offset: 0x000264F0
		private string GetAuxPropTypeFilter()
		{
			DynamicObject dynamicObject = this.Model.GetValue("FMaterialId") as DynamicObject;
			if (dynamicObject == null)
			{
				return " 1= 0 ";
			}
			DynamicObjectCollection source = dynamicObject["MaterialAuxPty"] as DynamicObjectCollection;
			List<long> list = (from x in source
			where DataEntityExtend.GetDynamicValue<bool>(x, "IsEnable1", false)
			select DataEntityExtend.GetDynamicValue<long>(x, "AuxPropertyId_Id", 0L)).ToList<long>();
			if (ListUtils.IsEmpty<long>(list))
			{
				return " 1=0 ";
			}
			return string.Format(" FID IN ({0}) ", string.Join<long>(",", list));
		}

		// Token: 0x0600034B RID: 843 RVA: 0x000283EC File Offset: 0x000265EC
		private void EntryRowMoveUpOrDown(string entryKey, bool isMoveUp = true)
		{
			EntryEntity entryEntity = base.View.BusinessInfo.GetEntryEntity(entryKey);
			DynamicObjectCollection entityDataObject = base.View.Model.GetEntityDataObject(entryEntity);
			int[] selectedRows = base.View.GetControl<EntryGrid>(entryKey).GetSelectedRows();
			if (selectedRows.Length > 1 || selectedRows[0] < 0)
			{
				base.View.ShowErrMessage(ResManager.LoadKDString("请选择一行分录进行上移或下移操作操作", "015072000025062", 7, new object[0]), "", 0);
				return;
			}
			int rowIndex = base.View.Model.GetEntryCurrentRowIndex(entryKey) + 1;
			if (rowIndex == 1 && isMoveUp)
			{
				base.View.ShowErrMessage(ResManager.LoadKDString("已是首行，不允许进行上移操作", "015072000025063", 7, new object[0]), "", 0);
				return;
			}
			if (rowIndex == entityDataObject.Count && !isMoveUp)
			{
				base.View.ShowErrMessage(ResManager.LoadKDString("已是末行，不允许进行下移操作", "015072000025064", 7, new object[0]), "", 0);
				return;
			}
			DynamicObject dynamicObject = (from w in entityDataObject
			where DataEntityExtend.GetDynamicValue<int>(w, "Seq", 0) == rowIndex
			select w).FirstOrDefault<DynamicObject>();
			if (isMoveUp)
			{
				DynamicObject dynamicObject2 = (from w in entityDataObject
				where DataEntityExtend.GetDynamicValue<int>(w, "Seq", 0) == rowIndex - 1
				select w).FirstOrDefault<DynamicObject>();
				int dynamicValue = DataEntityExtend.GetDynamicValue<int>(dynamicObject2, "Seq", 0);
				DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "Seq", dynamicValue);
				DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "Seq", rowIndex);
			}
			if (!isMoveUp)
			{
				DynamicObject dynamicObject3 = (from w in entityDataObject
				where DataEntityExtend.GetDynamicValue<int>(w, "Seq", 0) == rowIndex + 1
				select w).FirstOrDefault<DynamicObject>();
				int dynamicValue2 = DataEntityExtend.GetDynamicValue<int>(dynamicObject3, "Seq", 0);
				DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "Seq", dynamicValue2);
				DataEntityExtend.SetDynamicObjectItemValue(dynamicObject3, "Seq", rowIndex);
			}
			base.View.UpdateView(entryKey);
			int num = isMoveUp ? (base.View.Model.GetEntryCurrentRowIndex(entryKey) - 1) : (base.View.Model.GetEntryCurrentRowIndex(entryKey) + 1);
			base.View.SetEntityFocusRow(entryKey, num);
		}

		// Token: 0x0600034C RID: 844 RVA: 0x00028638 File Offset: 0x00026838
		protected string GetVariableFiltering()
		{
			string empty = string.Empty;
			DynamicObjectCollection source = base.View.Model.DataObject["Entity"] as DynamicObjectCollection;
			List<long> list = (from p in source
			where DataEntityExtend.GetDynamicValue<long>(p, "ModelVarId_Id", 0L) > 0L
			select p into d
			select DataEntityExtend.GetDynamicValue<long>(d, "ModelVarId_Id", 0L)).ToList<long>();
			if (ListUtils.IsEmpty<long>(list))
			{
				return empty;
			}
			return string.Format(" FID NOT IN ({0})", string.Join<long>(",", list));
		}

		// Token: 0x0600034D RID: 845 RVA: 0x000287C0 File Offset: 0x000269C0
		protected void SetDefaultValue(string varType, string valueSource, string fieldKey, int row)
		{
			string formId = string.Empty;
			string filterString = string.Empty;
			if (StringUtils.EqualsIgnoreCase(varType, "A") || StringUtils.EqualsIgnoreCase(varType, "B"))
			{
				if (StringUtils.EqualsIgnoreCase(varType, "A"))
				{
					formId = "BOS_ASSISTANTDATA_DETAIL";
					filterString = string.Format(" FID = '{0}' ", valueSource);
				}
				else
				{
					formId = valueSource;
				}
				this.ShowBaseDataFrom(formId, filterString, delegate(FormResult result)
				{
					if (result.ReturnData != null && result.ReturnData is ListSelectedRowCollection)
					{
						ListSelectedRowCollection listSelectedRowCollection = result.ReturnData as ListSelectedRowCollection;
						using (IEnumerator<ListSelectedRow> enumerator = listSelectedRowCollection.GetEnumerator())
						{
							if (enumerator.MoveNext())
							{
								ListSelectedRow listSelectedRow = enumerator.Current;
								this.View.Model.SetValue(fieldKey, listSelectedRow.Number, row);
							}
						}
					}
				});
				return;
			}
			if (StringUtils.EqualsIgnoreCase(varType, "E"))
			{
				this.ShowBooleanValueForm(delegate(FormResult result)
				{
					if (result != null && result.ReturnData != null)
					{
						DynamicObject dynamicObject = result.ReturnData as DynamicObject;
						string dynamicValue = DataEntityExtend.GetDynamicValue<string>(dynamicObject, "Value", null);
						this.View.Model.SetValue(fieldKey, dynamicValue, row);
					}
				});
			}
		}

		// Token: 0x0600034E RID: 846 RVA: 0x0002887C File Offset: 0x00026A7C
		protected void ShowBaseDataFrom(string formId, string filterString, Action<FormResult> action = null)
		{
			ListSelBillShowParameter listSelBillShowParameter = new ListSelBillShowParameter();
			listSelBillShowParameter.FormId = formId;
			listSelBillShowParameter.PermissionItemId = "6e44119a58cb4a8e86f6c385e14a17ad";
			listSelBillShowParameter.ListFilterParameter.Filter = filterString;
			listSelBillShowParameter.IsShowApproved = true;
			listSelBillShowParameter.IsShowUsed = true;
			listSelBillShowParameter.MultiSelect = false;
			listSelBillShowParameter.IsLookUp = true;
			base.View.ShowForm(listSelBillShowParameter, action);
		}

		// Token: 0x0600034F RID: 847 RVA: 0x000288D8 File Offset: 0x00026AD8
		protected void ShowBooleanValueForm(Action<FormResult> action = null)
		{
			bool flag = true;
			bool flag2 = false;
			DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
			dynamicFormShowParameter.OpenStyle.ShowType = 4;
			dynamicFormShowParameter.MultiSelect = false;
			dynamicFormShowParameter.FormId = "ENG_MODELVARIABLESHOW";
			dynamicFormShowParameter.CustomComplexParams.Add("CustomTitle", ResManager.LoadKDString("值选择", "015072000025065", 7, new object[0]));
			dynamicFormShowParameter.CustomComplexParams.Add("ShowType", 2);
			dynamicFormShowParameter.CustomComplexParams.Add("ListShowValue", new List<string>
			{
				flag.ToString(),
				flag2.ToString()
			});
			base.View.ShowForm(dynamicFormShowParameter, action);
		}

		// Token: 0x06000350 RID: 848 RVA: 0x00028988 File Offset: 0x00026B88
		protected void ShowVariableForm(Action<FormResult> action = null)
		{
			List<DynamicObject> value = new List<DynamicObject>();
			value = this.GetVariableList();
			DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
			dynamicFormShowParameter.OpenStyle.ShowType = 4;
			dynamicFormShowParameter.MultiSelect = false;
			dynamicFormShowParameter.FormId = "ENG_MODELVARIABLESHOW";
			dynamicFormShowParameter.CustomComplexParams.Add("ListShowModeVariable", value);
			base.View.ShowForm(dynamicFormShowParameter, action);
		}

		// Token: 0x06000351 RID: 849 RVA: 0x000289E4 File Offset: 0x00026BE4
		protected void ShowVariableForm(Field objField, Action<FormResult> action = null)
		{
			List<DynamicObject> value = new List<DynamicObject>();
			value = this.GetVariableList(objField);
			DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
			dynamicFormShowParameter.OpenStyle.ShowType = 4;
			dynamicFormShowParameter.MultiSelect = false;
			dynamicFormShowParameter.FormId = "ENG_MODELVARIABLESHOW";
			dynamicFormShowParameter.CustomComplexParams.Add("ListShowModeVariable", value);
			base.View.ShowForm(dynamicFormShowParameter, action);
		}

		// Token: 0x06000352 RID: 850 RVA: 0x00028A44 File Offset: 0x00026C44
		protected void ShowExpsForm(long expsId, string layoutId, Action<FormResult> action = null)
		{
			BillShowParameter billShowParameter = new BillShowParameter();
			billShowParameter.OpenStyle.ShowType = 4;
			billShowParameter.MultiSelect = false;
			billShowParameter.FormId = "ENG_PRDMODELEXPS";
			if (!string.IsNullOrWhiteSpace(layoutId))
			{
				billShowParameter.LayoutId = layoutId;
			}
			if (expsId > 0L)
			{
				billShowParameter.PKey = Convert.ToString(expsId);
				billShowParameter.Status = 2;
			}
			else
			{
				billShowParameter.Status = 0;
			}
			base.View.ShowForm(billShowParameter, action);
		}

		// Token: 0x06000353 RID: 851 RVA: 0x00028AB4 File Offset: 0x00026CB4
		protected List<DynamicObject> GetVariableList()
		{
			List<DynamicObject> list = new List<DynamicObject>();
			DynamicObject dynamicObject = new DynamicObject(this.VariableShowEntity.DynamicObjectType);
			DynamicObjectCollection dynamicObjectCollection = base.View.Model.DataObject["Entity"] as DynamicObjectCollection;
			DynamicObjectCollection dynamicObjectCollection2 = base.View.Model.DataObject["CalEntity"] as DynamicObjectCollection;
			foreach (DynamicObject dynamicObject2 in dynamicObjectCollection)
			{
				DynamicObject dynamicObject3 = dynamicObject2["ModelVarId"] as DynamicObject;
				if (dynamicObject3 != null)
				{
					DynamicObject dynamicObject4 = OrmUtils.Clone(dynamicObject, false, true) as DynamicObject;
					dynamicObject4["VarName"] = dynamicObject3["Number"];
					dynamicObject4["VarType"] = dynamicObject3["VarType"];
					dynamicObject4["VarControl"] = dynamicObject3["ControlType"];
					dynamicObject4["VariableId"] = dynamicObject3["Id"];
					dynamicObject4["IsCalVariable"] = 0;
					list.Add(dynamicObject4);
				}
			}
			foreach (DynamicObject dynamicObject5 in dynamicObjectCollection2)
			{
				DynamicObject dynamicObject6 = OrmUtils.Clone(dynamicObject, false, true) as DynamicObject;
				dynamicObject6["VarName"] = dynamicObject5["VarNumber"];
				dynamicObject6["VarType"] = dynamicObject5["VarType"];
				dynamicObject6["VarControl"] = 'B';
				dynamicObject6["IsCalVariable"] = 1;
				list.Add(dynamicObject6);
			}
			return list;
		}

		// Token: 0x06000354 RID: 852 RVA: 0x00028CA0 File Offset: 0x00026EA0
		private List<DynamicObject> GetVariableList(Field objField)
		{
			List<DynamicObject> list = new List<DynamicObject>();
			if (objField == null)
			{
				return list;
			}
			string text = string.Empty;
			text = ProductModelConst.GetVarTypeByField(objField);
			DynamicObject dynamicObject = new DynamicObject(this.VariableShowEntity.DynamicObjectType);
			DynamicObjectCollection dynamicObjectCollection = base.View.Model.DataObject["Entity"] as DynamicObjectCollection;
			DynamicObjectCollection dynamicObjectCollection2 = base.View.Model.DataObject["CalEntity"] as DynamicObjectCollection;
			foreach (DynamicObject dynamicObject2 in dynamicObjectCollection)
			{
				DynamicObject dynamicObject3 = dynamicObject2["ModelVarId"] as DynamicObject;
				if (dynamicObject3 != null)
				{
					string text2 = Convert.ToString(dynamicObject3["VarType"]);
					if (StringUtils.EqualsIgnoreCase(text, text2) || (StringUtils.EqualsIgnoreCase(text, "C") && StringUtils.EqualsIgnoreCase(text2, "D")))
					{
						if (StringUtils.EqualsIgnoreCase(text, "A") || StringUtils.EqualsIgnoreCase(text, "B"))
						{
							string text3 = string.Empty;
							if (StringUtils.EqualsIgnoreCase(text, "A"))
							{
								text3 = (objField as BaseDataField).LookUpObjectID;
							}
							else
							{
								text3 = (objField as BaseDataField).LookUpObject.FormId;
							}
							string dynamicValue = DataEntityExtend.GetDynamicValue<string>(dynamicObject3, "VarSource_Id", null);
							if (!StringUtils.EqualsIgnoreCase(text3, dynamicValue))
							{
								continue;
							}
						}
						DynamicObject dynamicObject4 = OrmUtils.Clone(dynamicObject, false, true) as DynamicObject;
						dynamicObject4["VarNumber"] = dynamicObject3["Number"];
						dynamicObject4["VarName"] = Convert.ToString(dynamicObject3["Name"]);
						dynamicObject4["VarType"] = dynamicObject3["VarType"];
						dynamicObject4["VarControl"] = dynamicObject3["ControlType"];
						dynamicObject4["IsCalVariable"] = 0;
						list.Add(dynamicObject4);
					}
				}
			}
			foreach (DynamicObject dynamicObject5 in dynamicObjectCollection2)
			{
				string text4 = Convert.ToString(dynamicObject5["VarType"]);
				if (StringUtils.EqualsIgnoreCase(text, text4) || (StringUtils.EqualsIgnoreCase(text, "C") && StringUtils.EqualsIgnoreCase(text4, "D")))
				{
					DynamicObject dynamicObject6 = OrmUtils.Clone(dynamicObject, false, true) as DynamicObject;
					dynamicObject6["VarNumber"] = dynamicObject5["VarNumber"];
					dynamicObject6["VarName"] = dynamicObject5["VarDescript"];
					dynamicObject6["VarType"] = dynamicObject5["VarType"];
					dynamicObject6["VarControl"] = 'B';
					dynamicObject6["IsCalVariable"] = 1;
					list.Add(dynamicObject6);
				}
			}
			return list;
		}

		// Token: 0x06000355 RID: 853 RVA: 0x00028FC4 File Offset: 0x000271C4
		protected string GetVarType(string fieldKey, int row)
		{
			string result = string.Empty;
			if (StringUtils.EqualsIgnoreCase(fieldKey, "FDefaultValue"))
			{
				DynamicObject value = MFGBillUtil.GetValue<DynamicObject>(base.View.Model, "FModelVarId", row, null, null);
				result = DataEntityExtend.GetDynamicValue<string>(value, "VarType", null);
			}
			else
			{
				result = MFGBillUtil.GetValue<string>(base.View.Model, "FVarType", row, null, null);
			}
			return result;
		}

		// Token: 0x06000356 RID: 854 RVA: 0x00029028 File Offset: 0x00027228
		protected string GetValueSource(int row)
		{
			DynamicObject value = MFGBillUtil.GetValue<DynamicObject>(base.View.Model, "FModelVarId", row, null, null);
			return DataEntityExtend.GetDynamicValue<string>(value, "VarSource_Id", null);
		}

		// Token: 0x06000357 RID: 855 RVA: 0x0002905C File Offset: 0x0002725C
		protected bool IsNumber(string value)
		{
			double num;
			return double.TryParse(value, out num);
		}

		// Token: 0x06000358 RID: 856 RVA: 0x0002935C File Offset: 0x0002755C
		protected void SelModelRuleData(int row)
		{
			List<string> lstKey = new List<string>();
			string entryIdKey = string.Format("{0}_{1}", "FEntity", "FEntryID");
			string entrySeqKey = string.Format("{0}_{1}", "FEntity", "FSeq");
			lstKey.Add("FID");
			lstKey.Add("FName");
			lstKey.Add("FRuleDescription");
			lstKey.Add("FRuleType");
			lstKey.Add("FExeCute");
			ListSelBillShowParameter listSelBillShowParameter = new ListSelBillShowParameter();
			listSelBillShowParameter.FormId = "ENG_MODELRULE";
			listSelBillShowParameter.PermissionItemId = "6e44119a58cb4a8e86f6c385e14a17ad";
			listSelBillShowParameter.IsShowApproved = true;
			listSelBillShowParameter.IsShowUsed = true;
			listSelBillShowParameter.MultiSelect = true;
			listSelBillShowParameter.IsLookUp = true;
			listSelBillShowParameter.IsIsolationOrg = false;
			listSelBillShowParameter.ListFilterParameter.SelectFields = lstKey;
			listSelBillShowParameter.ListFilterParameter.SelectEntitys = new List<string>
			{
				"FBillHead",
				"FEntity"
			};
			DynamicObjectCollection source = base.View.Model.DataObject["RuleEntry"] as DynamicObjectCollection;
			List<int> list = (from p in source
			select DataEntityExtend.GetDynamicValue<int>(p, "ModelRuleEntryId", 0)).ToList<int>();
			long dynamicValue = DataEntityExtend.GetDynamicValue<long>(base.View.Model.DataObject, "Id", 0L);
			if (dynamicValue > 0L)
			{
				listSelBillShowParameter.ListFilterParameter.Filter = string.Format(" ( FMODELCODE = {0} OR FMODELCODE = 0 )", dynamicValue);
			}
			else
			{
				listSelBillShowParameter.ListFilterParameter.Filter = " FMODELCODE = 0  ";
			}
			list = (from p in list
			where p > 0
			select p).ToList<int>();
			if (!ListUtils.IsEmpty<int>(list))
			{
				IRegularFilterParameter listFilterParameter = listSelBillShowParameter.ListFilterParameter;
				listFilterParameter.Filter += string.Format(" AND {0} NOT IN ({1})", entryIdKey, string.Join<int>(",", list));
			}
			base.View.ShowForm(listSelBillShowParameter, delegate(FormResult result)
			{
				if (result.ReturnData != null && result.ReturnData is ListSelectedRowCollection)
				{
					ListSelectedRowCollection source2 = result.ReturnData as ListSelectedRowCollection;
					List<string> list2 = (from p in source2
					where !string.IsNullOrWhiteSpace(p.EntryPrimaryKeyValue)
					select p into d
					select d.EntryPrimaryKeyValue).ToList<string>();
					if (!ListUtils.IsEmpty<string>(list2))
					{
						lstKey.Add("FNumber");
						lstKey.Add(entryIdKey);
						lstKey.Add(entrySeqKey);
						QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter();
						queryBuilderParemeter.FormId = "ENG_MODELRULE";
						queryBuilderParemeter.SelectItems = SelectorItemInfo.CreateItems(lstKey.ToArray());
						queryBuilderParemeter.FilterClauseWihtKey = string.Format("   {0} in ({1}) ", entryIdKey, string.Join(",", list2));
						DynamicObjectCollection dynamicObjectCollection = QueryServiceHelper.GetDynamicObjectCollection(this.View.Context, queryBuilderParemeter, null);
						int num = 1;
						int num2 = 0;
						foreach (DynamicObject dynamicObject in dynamicObjectCollection)
						{
							if (num == 1)
							{
								num2 = row;
							}
							else
							{
								num2++;
								this.View.Model.InsertEntryRow("FRuleEntry", num2);
							}
							this.View.Model.SetItemValueByID("FModelRuleId", dynamicObject["FID"], num2);
							this.View.Model.SetValue("FRuleDescription", Convert.ToString(dynamicObject["FRuleDescription"]), num2);
							this.View.Model.SetValue("FModelRuleType", dynamicObject["FRuleType"], num2);
							this.View.Model.SetValue("FRuleExecute", dynamicObject["FExeCute"], num2);
							this.View.Model.SetValue("FModelRuleEntryId", dynamicObject[entryIdKey], num2);
							this.View.Model.SetValue("FShowText", string.Format("{0}-{1}", dynamicObject["FNumber"], dynamicObject[entrySeqKey]), num2);
							num++;
						}
					}
				}
			});
		}

		// Token: 0x06000359 RID: 857 RVA: 0x000295A8 File Offset: 0x000277A8
		protected void SynMaterialId(DynamicObject dyObj, dynamic objData)
		{
			object value = base.View.Model.GetValue("FCreateOrgId");
			DynamicObject dynamicValue = DataEntityExtend.GetDynamicValue<DynamicObject>(dyObj, "FMaterialId", null);
			if (value == null || dynamicValue == null)
			{
				base.View.Model.SetItemValueByID("FPlanMaterialId", 0, 0);
			}
			else
			{
				base.View.Model.SetItemValueByID("FPlanMaterialId", DataEntityExtend.GetDynamicValue<long>(dyObj, "FMaterialId_Id", 0L), 0);
			}
			DynamicObjectCollection source = this.Model.DataObject["PlanEntry"] as DynamicObjectCollection;
			this.SetFlexFieldEnableByMaterial(source.FirstOrDefault<DynamicObject>());
			base.View.UpdateView("FPlanEntry");
		}

		// Token: 0x0600035A RID: 858 RVA: 0x000296A8 File Offset: 0x000278A8
		protected void LoaduxProperty(DynamicObject dyObj, dynamic objData)
		{
			object value = base.View.Model.GetValue("FCreateOrgId");
			DynamicObject dynamicValue = DataEntityExtend.GetDynamicValue<DynamicObject>(dyObj, "FMaterialId", null);
			base.View.Model.DeleteEntryData("FPlanEntry");
			if (value == null || dynamicValue == null)
			{
				return;
			}
			long num = Convert.ToInt64(((DynamicObject)value)["Id"]);
			Dictionary<long, string> auxPropIdByOrgId = FlexServiceHelper.GetAuxPropIdByOrgId(base.Context, num);
			if (ListUtils.IsEmpty<KeyValuePair<long, string>>(auxPropIdByOrgId))
			{
				return;
			}
			FormMetadata formMetaData = MetaDataServiceHelper.GetFormMetaData(base.Context, "BD_FLEXAUXPROPERTY");
			DynamicObjectType dynamicObjectType = formMetaData.BusinessInfo.GetSubBusinessInfo(new List<string>
			{
				"FID",
				"FNumber",
				"FName",
				"FValueType"
			}).GetDynamicObjectType();
			DynamicObject[] array = BusinessDataServiceHelper.LoadFromCache(base.Context, (from w in auxPropIdByOrgId.Keys
			select w).ToArray<object>(), dynamicObjectType);
			if (ListUtils.IsEmpty<DynamicObject>(array))
			{
				return;
			}
			Dictionary<long, DynamicObject> dictionary = array.ToDictionary((DynamicObject p) => Convert.ToInt64(p["Id"]));
			DynamicObjectCollection dynamicObjectCollection = (DynamicObjectCollection)base.View.Model.DataObject["PlanEntry"];
			DynamicObjectCollection source = (DynamicObjectCollection)dynamicValue["MaterialAuxPty"];
			List<long> list = (from p in source
			where DataEntityExtend.GetDynamicValue<long>(p, "Id", 0L) > 0L && DataEntityExtend.GetDynamicValue<bool>(p, "IsEnable1", false)
			select p into d
			select DataEntityExtend.GetDynamicValue<long>(d, "AuxPropertyId_Id", 0L)).ToList<long>();
			foreach (long key in list)
			{
				DynamicObject dynamicObject = dictionary[key];
				base.View.Model.CreateNewEntryRow("FPlanEntry");
				int index = dynamicObjectCollection.Count - 1;
				DynamicObject dynamicObject2 = dynamicObjectCollection[index];
				dynamicObject2["PlanAuxpropId_Id"] = Convert.ToInt32(dynamicObject["Id"]);
				dynamicObject2["PlanAuxpropId"] = dynamicObject;
				dynamicObject2["IsEnable"] = false;
			}
			base.View.UpdateView("FPlanEntry");
		}

		// Token: 0x0600035B RID: 859 RVA: 0x00029938 File Offset: 0x00027B38
		protected void LoadConfigView()
		{
			this.ShowBomConfigView();
		}

		// Token: 0x0600035C RID: 860 RVA: 0x00029940 File Offset: 0x00027B40
		protected void ShowBomConfigView()
		{
			IDynamicFormView view = base.View.GetView(this.BomConfigView_pageId);
			if (view == null)
			{
				long num = 0L;
				long num2 = Convert.ToInt64(base.View.Model.DataObject["Id"]);
				if (num2 <= 0L && base.View.OpenParameter.CreateFrom == 3)
				{
					num2 = Convert.ToInt64(base.View.OpenParameter.PkValue);
				}
				if (num2 > 0L)
				{
					num = ProductModelServiceHelper.GetPrdModelBomId(base.View.Context, num2);
				}
				BillShowParameter billShowParameter = new BillShowParameter();
				billShowParameter.OpenStyle.ShowType = 3;
				billShowParameter.OpenStyle.TagetKey = "FBomShowPanel";
				billShowParameter.FormId = "ENG_PRDMODELBOMCONFIG";
				billShowParameter.PageId = this.BomConfigView_pageId;
				billShowParameter.CreateFrom = base.View.OpenParameter.CreateFrom;
				if (num > 0L)
				{
					billShowParameter.PKey = num.ToString();
					billShowParameter.Status = base.View.OpenParameter.Status;
				}
				else
				{
					billShowParameter.Status = 0;
				}
				base.View.ShowForm(billShowParameter);
				return;
			}
			if (base.View.OpenParameter.Status == null)
			{
				if (base.View.OpenParameter.CreateFrom == 3)
				{
					string text = Convert.ToString(base.View.OpenParameter.PkValue.ToString());
					if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(text) && !(text == "0"))
					{
						(view as IDynamicFormViewService).CustomEvents("Copy", "Copy", text);
					}
				}
				else
				{
					(view as IDynamicFormViewService).CustomEvents("New", "New", "");
				}
				base.View.SendDynamicFormAction(view);
				return;
			}
			if (base.View.OpenParameter.PkValue != null)
			{
				string text2 = Convert.ToString(base.View.OpenParameter.PkValue.ToString());
				(view as IDynamicFormViewService).CustomEvents("BillReloadData", "BillReloadData", text2);
				base.View.SendDynamicFormAction(view);
			}
		}

		// Token: 0x0600035D RID: 861 RVA: 0x00029B46 File Offset: 0x00027D46
		protected void ShowRouteConfigView()
		{
		}

		// Token: 0x0600035E RID: 862 RVA: 0x00029B48 File Offset: 0x00027D48
		protected void SetLeftTree()
		{
			TreeView treeView = (TreeView)base.View.GetControl("FTreeView");
			treeView.SetAutoScroll(true);
			TreeNode treeNode = this.CreateTreeNote("tdRootNode", ResManager.LoadKDString("节点列表", "015072000025066", 7, new object[0]));
			TreeNode item = this.CreateTreeNote("tdIf", ResManager.LoadKDString("If节点", "015072000025067", 7, new object[0]));
			treeNode.children.Add(item);
			TreeNode treeNode2 = this.CreateTreeNote("tdSwitch", ResManager.LoadKDString("Switch节点", "015072000025068", 7, new object[0]));
			treeNode.children.Add(treeNode2);
			treeNode.children.Add(this.CreateTreeNote("tdBom", ResManager.LoadKDString("BOM节点", "015072000025069", 7, new object[0])));
			treeNode.children.Add(this.CreateTreeNote("rdRoute", ResManager.LoadKDString("工序节点", "015072000025070", 7, new object[0])));
			treeNode2.children.Add(this.CreateTreeNote("tdCase", ResManager.LoadKDString("Case节点", "015072000025071", 7, new object[0])));
			treeNode2.children.Add(this.CreateTreeNote("tdDefault", ResManager.LoadKDString("Default节点", "015072000025072", 7, new object[0])));
			treeView.SetRootNode(treeNode);
			treeView.ExpandNode("tdSwitch");
			treeView.SetNodeVisible("rdRoute", false);
			this.currentSelectNoteId = treeNode2.id;
		}

		// Token: 0x0600035F RID: 863 RVA: 0x00029CCC File Offset: 0x00027ECC
		protected TreeNode CreateTreeNote(string id, string nodeText)
		{
			return new TreeNode
			{
				text = nodeText,
				id = id
			};
		}

		// Token: 0x06000360 RID: 864 RVA: 0x00029D14 File Offset: 0x00027F14
		protected void AddEntryTreeRow(string TreeNoteType)
		{
			if (base.BillStatus.Equals('C'))
			{
				return;
			}
			string text = TreeNoteType;
			Entity entity = base.View.BusinessInfo.GetEntity("FTreeEntryConfig");
			DynamicObject dynamicObject = null;
			if (string.IsNullOrWhiteSpace(text))
			{
				TreeView treeView = (TreeView)base.View.GetControl("FTreeView");
				text = treeView.SelectedNodeId;
			}
			DynamicObjectCollection dynamicObjectCollection = base.View.Model.DataObject["TreeEntryConfig"] as DynamicObjectCollection;
			int num = base.View.Model.GetEntryCurrentRowIndex("FTreeEntryConfig");
			if (!ListUtils.IsEmpty<DynamicObject>(dynamicObjectCollection) && num != -1)
			{
				DynamicObject dynamicObject2 = dynamicObjectCollection[num];
				string parentRowId = DataEntityExtend.GetDynamicValue<string>(dynamicObject2, "ParentRowId", null);
				string dynamicValue = DataEntityExtend.GetDynamicValue<string>(dynamicObject2, "NodeType", null);
				string text2 = string.Empty;
				int num2 = -1;
				if (!string.IsNullOrWhiteSpace(parentRowId))
				{
					DynamicObject dynamicObject3 = dynamicObjectCollection.FirstOrDefault((DynamicObject p) => DataEntityExtend.GetDynamicValue<string>(p, "RowId", null).Equals(parentRowId));
					text2 = DataEntityExtend.GetDynamicValue<string>(dynamicObject3, "NodeType", null);
					num2 = this.GetNoteLeval(text, text2);
				}
				int noteLeval = this.GetNoteLeval(text, dynamicValue);
				if (noteLeval == 0 || (!string.IsNullOrWhiteSpace(text2) && num2 == 0))
				{
					base.View.ShowMessage(ResManager.LoadKDString("无法添加在当前节点！", "015072000025073", 7, new object[0]), 0);
					return;
				}
				List<DynamicObject> list = this.CreateTreeEntryRowByNoteType(text);
				if (!ListUtils.IsEmpty<DynamicObject>(list))
				{
					if (text.Equals("tdBom"))
					{
						dynamicObject = list.FirstOrDefault<DynamicObject>();
					}
					if (text.Equals("rdRoute"))
					{
						DynamicObject dynamicObject4 = list.FirstOrDefault<DynamicObject>();
					}
					DynamicObject dynamicObject5 = list.FirstOrDefault<DynamicObject>();
					int num3 = dynamicObjectCollection.IndexOf(dynamicObject2);
					if (noteLeval == 2)
					{
						dynamicObject5["ParentRowId"] = dynamicObject2["RowId"];
					}
					else
					{
						dynamicObject5["ParentRowId"] = dynamicObject2["ParentRowId"];
					}
					int num4 = this.GetTreeEntryMaxGroupNo();
					num4++;
					foreach (DynamicObject dynamicObject6 in list)
					{
						dynamicObject6["ReplaceGroup"] = num4;
						if (text.Equals("tdCase"))
						{
							base.View.Model.CreateNewEntryRow(entity, num3 + 1, dynamicObject6);
						}
						else
						{
							base.View.Model.CreateNewEntryRow(entity, num3 + 1, dynamicObject6);
						}
						num4++;
						num3++;
					}
					num = dynamicObjectCollection.IndexOf(dynamicObject2);
				}
			}
			else
			{
				List<DynamicObject> list2 = new List<DynamicObject>();
				if (text.Equals("tdCase") || text.Equals("tdDefault"))
				{
					text = "tdSwitch";
				}
				list2 = this.CreateTreeEntryRowByNoteType(text);
				if (text.Equals("tdBom"))
				{
					dynamicObject = list2.FirstOrDefault<DynamicObject>();
				}
				if (text.Equals("rdRoute"))
				{
					DynamicObject dynamicObject4 = list2.FirstOrDefault<DynamicObject>();
				}
				int num5 = 1;
				foreach (DynamicObject dynamicObject7 in list2)
				{
					dynamicObject7["ReplaceGroup"] = num5;
					base.View.Model.CreateNewEntryRow(entity, dynamicObjectCollection.Count, dynamicObject7);
					num5++;
				}
				num = dynamicObjectCollection.Count - 1;
			}
			base.View.Model.SetEntryCurrentRowIndex("FTreeEntryConfig", num);
			if (dynamicObject != null)
			{
				this.AddBOMChild(dynamicObject);
			}
		}

		// Token: 0x06000361 RID: 865 RVA: 0x0002A0C4 File Offset: 0x000282C4
		protected void AddBOMChild(DynamicObject bomEntryRow)
		{
			string dynamicValue = DataEntityExtend.GetDynamicValue<string>(bomEntryRow, "RowId", null);
			long dynamicValue2 = DataEntityExtend.GetDynamicValue<long>(bomEntryRow, "ReplaceGroup", 0L);
			IDynamicFormView view = base.View.GetView(this.BomConfigView_pageId);
			if (view != null)
			{
				view.Model.CreateNewEntryRow("FTreeEntity");
				int entryRowCount = view.Model.GetEntryRowCount("FTreeEntity");
				view.Model.SetValue("FPMRowId", dynamicValue, entryRowCount - 1);
				view.Model.SetValue("FReplaceGroup", dynamicValue2, entryRowCount - 1);
				base.View.SendDynamicFormAction(view);
			}
		}

		// Token: 0x06000362 RID: 866 RVA: 0x0002A17C File Offset: 0x0002837C
		protected void DeleteBOMChild(DynamicObject bomEntryRow)
		{
			string rowId = DataEntityExtend.GetDynamicValue<string>(bomEntryRow, "RowId", null);
			IDynamicFormView view = base.View.GetView(this.BomConfigView_pageId);
			if (view != null)
			{
				DynamicObjectCollection dynamicObjectCollection = view.Model.DataObject["TreeEntity"] as DynamicObjectCollection;
				DynamicObject dynamicObject = (from p in dynamicObjectCollection
				where DataEntityExtend.GetDynamicValue<string>(p, "PMRowId", null).Equals(rowId)
				select p).FirstOrDefault<DynamicObject>();
				if (dynamicObject != null)
				{
					int num = dynamicObjectCollection.IndexOf(dynamicObject);
					(view as IDynamicFormViewService).CustomEvents("Delete", "Delete", num.ToString());
					base.View.SendDynamicFormAction(view);
				}
			}
		}

		// Token: 0x06000363 RID: 867 RVA: 0x0002A22C File Offset: 0x0002842C
		protected string ConvertFomatDataToString(DynamicObject txtData)
		{
			StringBuilder stringBuilder = new StringBuilder();
			DynamicObjectCollection dynamicObjectCollection = txtData["Entity"] as DynamicObjectCollection;
			foreach (DynamicObject dynamicObject in dynamicObjectCollection)
			{
				DynamicObject dynamicObject2 = dynamicObject["ModelVarId"] as DynamicObject;
				if (dynamicObject2 != null)
				{
					stringBuilder.Append(DataEntityExtend.GetDynamicValue<string>(dynamicObject2, "Number", null));
				}
				string dynamicValue = DataEntityExtend.GetDynamicValue<string>(dynamicObject, "SplitTxt", null);
				stringBuilder.AppendLine(dynamicValue);
			}
			return stringBuilder.ToString();
		}

		// Token: 0x06000364 RID: 868 RVA: 0x0002A2D0 File Offset: 0x000284D0
		protected List<DynamicObject> CreateTreeEntryRowByNoteType(string treeNoteType)
		{
			List<DynamicObject> list = new List<DynamicObject>();
			DynamicObjectType dynamicObjectType = base.View.BusinessInfo.GetEntity("FTreeEntryConfig").DynamicObjectType;
			if (StringUtils.EqualsIgnoreCase(treeNoteType, "tdIf"))
			{
				DynamicObject dynamicObject = new DynamicObject(dynamicObjectType);
				dynamicObject["RowId"] = Guid.NewGuid().ToString();
				dynamicObject["RowExpandType"] = 0;
				dynamicObject["NodeType"] = "C";
				list.Add(dynamicObject);
				DynamicObject dynamicObject2 = new DynamicObject(dynamicObjectType);
				dynamicObject2["RowId"] = Guid.NewGuid().ToString();
				dynamicObject2["ParentRowId"] = dynamicObject["RowId"];
				dynamicObject2["RowExpandType"] = 0;
				dynamicObject2["NodeType"] = "D";
				list.Add(dynamicObject2);
				DynamicObject dynamicObject3 = new DynamicObject(dynamicObjectType);
				dynamicObject3["RowId"] = Guid.NewGuid().ToString();
				dynamicObject3["ParentRowId"] = dynamicObject["RowId"];
				dynamicObject3["RowExpandType"] = 0;
				dynamicObject3["NodeType"] = "E";
				list.Add(dynamicObject3);
			}
			else if (StringUtils.EqualsIgnoreCase(treeNoteType, "tdSwitch"))
			{
				DynamicObject dynamicObject4 = new DynamicObject(dynamicObjectType);
				dynamicObject4["RowId"] = Guid.NewGuid().ToString();
				dynamicObject4["RowExpandType"] = 0;
				dynamicObject4["NodeType"] = "F";
				list.Add(dynamicObject4);
				DynamicObject dynamicObject5 = new DynamicObject(dynamicObjectType);
				dynamicObject5["RowId"] = Guid.NewGuid().ToString();
				dynamicObject5["ParentRowId"] = dynamicObject4["RowId"];
				dynamicObject5["RowExpandType"] = 0;
				dynamicObject5["NodeType"] = "G";
				list.Add(dynamicObject5);
				DynamicObject dynamicObject6 = new DynamicObject(dynamicObjectType);
				dynamicObject6["RowId"] = Guid.NewGuid().ToString();
				dynamicObject6["ParentRowId"] = dynamicObject4["RowId"];
				dynamicObject6["RowExpandType"] = 0;
				dynamicObject6["NodeType"] = "H";
				list.Add(dynamicObject6);
			}
			else
			{
				DynamicObject dynamicObject7 = new DynamicObject(dynamicObjectType);
				dynamicObject7["RowId"] = Guid.NewGuid().ToString();
				dynamicObject7["RowExpandType"] = 0;
				dynamicObject7["NodeType"] = this.dicNoteTypeMap[treeNoteType];
				list.Add(dynamicObject7);
			}
			return list;
		}

		// Token: 0x06000365 RID: 869 RVA: 0x0002A5C8 File Offset: 0x000287C8
		protected int GetNoteLeval(string leftNoteType, string rightNoteType)
		{
			if (string.IsNullOrWhiteSpace(rightNoteType))
			{
				return 1;
			}
			int result = 0;
			if (this.dicNodeCombin == null)
			{
				this.dicNodeCombin = ProductModelConst.GetNoteTypeCombinDic();
			}
			string key = leftNoteType + rightNoteType;
			this.dicNodeCombin.TryGetValue(key, out result);
			return result;
		}

		// Token: 0x06000366 RID: 870 RVA: 0x0002A60C File Offset: 0x0002880C
		private void ClearExpsData()
		{
			DynamicObject dataObject = base.View.Model.DataObject;
			long num = (long)Convert.ToInt32(dataObject["Id"]);
			if (num == 0L)
			{
				List<long> list = new List<long>();
				long dynamicValue = DataEntityExtend.GetDynamicValue<long>(dataObject, "MtrExpsId", 0L);
				if (dynamicValue > 0L)
				{
					list.Add(dynamicValue);
				}
				DynamicObjectCollection dynamicObjectCollection = dataObject["PlanEntry"] as DynamicObjectCollection;
				foreach (DynamicObject dynamicObject in dynamicObjectCollection)
				{
					dynamicValue = DataEntityExtend.GetDynamicValue<long>(dynamicObject, "AuxExpsId", 0L);
					if (dynamicValue > 0L)
					{
						list.Add(dynamicValue);
					}
				}
				DynamicObjectCollection dynamicObjectCollection2 = dataObject["TreeEntryConfig"] as DynamicObjectCollection;
				foreach (DynamicObject dynamicObject2 in dynamicObjectCollection2)
				{
					dynamicValue = DataEntityExtend.GetDynamicValue<long>(dynamicObject2, "FNodeExpsId", 0L);
					if (dynamicValue > 0L)
					{
						list.Add(dynamicValue);
					}
				}
				if (!ListUtils.IsEmpty<long>(list))
				{
					ProductModelServiceHelper.DeletePrdModelExps(base.Context, list);
				}
			}
		}

		// Token: 0x06000367 RID: 871 RVA: 0x0002A788 File Offset: 0x00028988
		private DynamicObject GetSwitchVarialRowData(int caseRowIndex)
		{
			DynamicObject result = null;
			string parentRowId = MFGBillUtil.GetValue<string>(base.View.Model, "FParentRowId", caseRowIndex, null, null);
			DynamicObjectCollection source = base.View.Model.DataObject["TreeEntryConfig"] as DynamicObjectCollection;
			DynamicObject dynamicObject = source.FirstOrDefault((DynamicObject p) => DataEntityExtend.GetDynamicValue<string>(p, "RowId", null).Equals(parentRowId));
			string VarNumber = DataEntityExtend.GetDynamicValue<string>(dynamicObject, "Condition", null);
			if (string.IsNullOrWhiteSpace(VarNumber))
			{
				return result;
			}
			List<DynamicObject> variableList = this.GetVariableList();
			return variableList.FirstOrDefault((DynamicObject p) => StringUtils.EqualsIgnoreCase(DataEntityExtend.GetDynamicValue<string>(p, "VarName", null), VarNumber));
		}

		// Token: 0x06000368 RID: 872 RVA: 0x0002A840 File Offset: 0x00028A40
		private int GetTreeEntryMaxGroupNo()
		{
			DynamicObjectCollection source = base.View.Model.DataObject["TreeEntryConfig"] as DynamicObjectCollection;
			return source.Max((DynamicObject p) => DataEntityExtend.GetDynamicValue<int>(p, "ReplaceGroup", 0));
		}

		// Token: 0x06000369 RID: 873 RVA: 0x0002A8D4 File Offset: 0x00028AD4
		private void SetFlexFieldEnableByMaterial(DynamicObject dyObj)
		{
			DynamicObject dynamicObject = dyObj["PlanMaterialId"] as DynamicObject;
			RelatedFlexGroupFieldAppearance relatedFlexGroupFieldAppearance = base.View.LayoutInfo.GetFieldAppearance("FAuxPropId") as RelatedFlexGroupFieldAppearance;
			if (dynamicObject == null)
			{
				this.LockPlanEntryAllFlexField(relatedFlexGroupFieldAppearance, dyObj);
				return;
			}
			DynamicObjectCollection source = (DynamicObjectCollection)dynamicObject["MaterialAuxPty"];
			List<DynamicObject> list = (from p in source
			where DataEntityExtend.GetDynamicValue<long>(p, "Id", 0L) > 0L && DataEntityExtend.GetDynamicValue<bool>(p, "IsEnable1", false)
			select p into d
			select DataEntityExtend.GetDynamicValue<DynamicObject>(d, "AuxPropertyId", null)).ToList<DynamicObject>();
			if (ListUtils.IsEmpty<DynamicObject>(list))
			{
				this.LockPlanEntryAllFlexField(relatedFlexGroupFieldAppearance, dyObj);
				return;
			}
			List<string> list2 = (from p in list
			select Convert.ToString(p["Name"])).ToList<string>();
			foreach (FieldAppearance fieldAppearance in relatedFlexGroupFieldAppearance.RelateFlexLayoutInfo.GetFieldAppearances())
			{
				if (fieldAppearance.Field.Key.StartsWith("FF"))
				{
					string text = string.Format("$${0}__{1}", relatedFlexGroupFieldAppearance.Key.ToUpperInvariant(), fieldAppearance.Key.ToUpperInvariant());
					if (list2.Contains(fieldAppearance.Field.Name.ToString()))
					{
						base.View.StyleManager.SetEnabled(text, dyObj, "LockAuxByPlanMaterial", true);
					}
					else
					{
						base.View.StyleManager.SetEnabled(text, dyObj, "LockAuxByPlanMaterial", false);
					}
				}
			}
		}

		// Token: 0x0600036A RID: 874 RVA: 0x0002AA88 File Offset: 0x00028C88
		private void LockPlanEntryAllFlexField(RelatedFlexGroupFieldAppearance relatedFlexAp, DynamicObject dyObj)
		{
			if (relatedFlexAp == null)
			{
				return;
			}
			foreach (FieldAppearance fieldAppearance in relatedFlexAp.RelateFlexLayoutInfo.GetFieldAppearances())
			{
				string text = string.Format("$${0}__{1}", relatedFlexAp.Key.ToUpperInvariant(), fieldAppearance.Key.ToUpperInvariant());
				base.View.StyleManager.SetEnabled(text, dyObj, "LockAuxByPlanMaterial", false);
			}
		}

		// Token: 0x0600036B RID: 875 RVA: 0x0002AB18 File Offset: 0x00028D18
		private object GetVarDefaultValue(Field objField)
		{
			string text = string.Empty;
			text = ProductModelConst.GetVarTypeByField(objField);
			string a;
			if ((a = text) != null)
			{
				if (a == "A" || a == "B" || a == "D" || a == "E")
				{
					return 0;
				}
				if (a == "C")
				{
					return string.Empty;
				}
			}
			return string.Empty;
		}

		// Token: 0x0600036C RID: 876 RVA: 0x0002AB94 File Offset: 0x00028D94
		private void SetFlexFieldValue(Field flexField, int rowIndex, object value)
		{
			DynamicObjectCollection dynamicObjectCollection = base.View.Model.DataObject["PlanEntry"] as DynamicObjectCollection;
			DynamicObject dynamicObject = dynamicObjectCollection[rowIndex]["AuxPropId"] as DynamicObject;
			if (dynamicObject != null)
			{
				if (flexField is BaseDataField)
				{
					((BaseDataField)flexField).RefIDDynamicProperty.SetValue(dynamicObject, value);
					flexField.DynamicProperty.SetValue(dynamicObject, null);
				}
				else
				{
					flexField.DynamicProperty.SetValue(dynamicObject, value);
				}
			}
			base.View.UpdateView("FAuxPropId", rowIndex);
		}

		// Token: 0x04000178 RID: 376
		protected Dictionary<string, string> dicNoteTypeMap = new Dictionary<string, string>();

		// Token: 0x04000179 RID: 377
		protected Dictionary<string, int> dicNodeCombin;

		// Token: 0x0400017A RID: 378
		private bool deleteTreeEntryRow;

		// Token: 0x0400017B RID: 379
		private string currentSelectNoteId = string.Empty;

		// Token: 0x0400017C RID: 380
		private Entity variableShowEntity;

		// Token: 0x0400017D RID: 381
		private List<string> lstSendEvenOp = new List<string>
		{
			6.ToString(),
			"Audit",
			"UnAudit"
		};
	}
}
