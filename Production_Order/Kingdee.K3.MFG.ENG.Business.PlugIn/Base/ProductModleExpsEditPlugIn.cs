using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Kingdee.BOS;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Validation;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.BOS.VerificationHelper.Verifiers;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.Core.MFG.Utils;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.Base
{
	// Token: 0x02000032 RID: 50
	[Description("产品模型表达式设置插件")]
	public class ProductModleExpsEditPlugIn : AbstractBillPlugIn
	{
		// Token: 0x17000019 RID: 25
		// (get) Token: 0x060003A6 RID: 934 RVA: 0x0002D858 File Offset: 0x0002BA58
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

		// Token: 0x060003A7 RID: 935 RVA: 0x0002D8A5 File Offset: 0x0002BAA5
		public override void PreOpenForm(PreOpenFormEventArgs e)
		{
			base.PreOpenForm(e);
			FeatureVerifier.CheckFeature(e.Context, "PDB");
		}

		// Token: 0x060003A8 RID: 936 RVA: 0x0002D8C0 File Offset: 0x0002BAC0
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			base.BarItemClick(e);
			string barItemKey;
			if ((barItemKey = e.BarItemKey) != null)
			{
				if (!(barItemKey == "tbReturnData"))
				{
					return;
				}
				DynamicObject dataObject = base.View.Model.DataObject;
				this.DelInvalidRow();
				IOperationResult operationResult = BusinessDataServiceHelper.Save(base.View.Context, base.View.BusinessInfo, dataObject, null, "Save");
				if (!ListUtils.IsEmpty<ValidationErrorInfo>(operationResult.ValidationErrors))
				{
					MFGBillUtil.ShowOperateResult(base.View, operationResult, base.View.BusinessInfo.GetForm().GetOperation("Save"));
					e.Cancel = true;
					return;
				}
				base.View.ReturnToParentWindow(dataObject);
				base.View.Model.DataChanged = false;
				base.View.Close();
			}
		}

		// Token: 0x060003A9 RID: 937 RVA: 0x0002D98C File Offset: 0x0002BB8C
		public override void EntityRowClick(EntityRowClickEventArgs e)
		{
			base.EntityRowClick(e);
			string a;
			if ((a = e.Key.ToUpper()) != null)
			{
				if (!(a == "FFORMULAENTITY") && !(a == "FOPERATIOENTITY") && !(a == "FOPFORMULAENTITY") && !(a == "FCONENTITY"))
				{
					return;
				}
				this.SetForMulaVarEditButton(e.Row, e.Key.ToUpper());
			}
		}

		// Token: 0x060003AA RID: 938 RVA: 0x0002D9FC File Offset: 0x0002BBFC
		public override void DataChanged(DataChangedEventArgs e)
		{
			base.DataChanged(e);
			string key;
			if ((key = e.Field.Key) != null)
			{
				if (!(key == "FVariableType") && !(key == "FOpVBLeftType") && !(key == "FOPVBRightType") && !(key == "FOPForMulaType") && !(key == "FConVariableType"))
				{
					return;
				}
				this.SetForMulaVarEditButton(e.Row, e.Field.Key);
			}
		}

		// Token: 0x060003AB RID: 939 RVA: 0x0002DBA8 File Offset: 0x0002BDA8
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			base.BeforeF7Select(e);
			string fieldKey;
			if ((fieldKey = e.FieldKey) != null)
			{
				if (!(fieldKey == "FModelVarId"))
				{
					if (!(fieldKey == "FOperationLeftVB"))
					{
						if (!(fieldKey == "FOperationRightVB"))
						{
							return;
						}
						int num = Convert.ToInt32(base.View.Model.GetValue("FOPVBRightType", e.Row));
						if (num == 3)
						{
							this.ShowModelSysLable(e.Row, e.FieldKey);
						}
						if (num == 1)
						{
							this.ShowVariableForm(delegate(FormResult result)
							{
								if (result != null && result.ReturnData != null)
								{
									DynamicObject dynamicObject2 = result.ReturnData as DynamicObject;
									string dynamicValue2 = DataEntityExtend.GetDynamicValue<string>(dynamicObject2, "VarNumber", null);
									string dynamicValue3 = DataEntityExtend.GetDynamicValue<string>(dynamicObject2, "VarType", null);
									string dynamicValue4 = DataEntityExtend.GetDynamicValue<string>(dynamicObject2, "VarControl", null);
									long dynamicValue5 = DataEntityExtend.GetDynamicValue<long>(dynamicObject2, "VariableId", 0L);
									string dynamicValue6 = DataEntityExtend.GetDynamicValue<string>(dynamicObject2, "IsCalVariable", null);
									this.SetValueByKey(dynamicValue2, dynamicValue3, dynamicValue4, dynamicValue5, dynamicValue6, e.FieldKey, e.Row);
								}
							});
						}
					}
					else
					{
						int num2 = Convert.ToInt32(base.View.Model.GetValue("FOpVBLeftType", e.Row));
						if (num2 == 3)
						{
							this.ShowModelSysLable(e.Row, e.FieldKey);
						}
						if (num2 == 1)
						{
							this.ShowVariableForm(delegate(FormResult result)
							{
								if (result != null && result.ReturnData != null)
								{
									DynamicObject dynamicObject2 = result.ReturnData as DynamicObject;
									string dynamicValue2 = DataEntityExtend.GetDynamicValue<string>(dynamicObject2, "VarNumber", null);
									string dynamicValue3 = DataEntityExtend.GetDynamicValue<string>(dynamicObject2, "VarType", null);
									string dynamicValue4 = DataEntityExtend.GetDynamicValue<string>(dynamicObject2, "VarControl", null);
									long dynamicValue5 = DataEntityExtend.GetDynamicValue<long>(dynamicObject2, "VariableId", 0L);
									string dynamicValue6 = DataEntityExtend.GetDynamicValue<string>(dynamicObject2, "IsCalVariable", null);
									this.SetValueByKey(dynamicValue2, dynamicValue3, dynamicValue4, dynamicValue5, dynamicValue6, e.FieldKey, e.Row);
								}
							});
							return;
						}
					}
				}
				else if (base.View.ParentFormView != null)
				{
					List<long> list = new List<long>();
					DynamicObjectCollection dynamicObjectCollection = base.View.ParentFormView.Model.DataObject["Entity"] as DynamicObjectCollection;
					foreach (DynamicObject dynamicObject in dynamicObjectCollection)
					{
						long dynamicValue = DataEntityExtend.GetDynamicValue<long>(dynamicObject, "ModelVarId_Id", 0L);
						if (dynamicValue > 0L)
						{
							list.Add(dynamicValue);
						}
					}
					string text = string.Empty;
					if (!ListUtils.IsEmpty<long>(list))
					{
						text = string.Format(" FID IN ({0}) ", string.Join<long>(",", list));
					}
					else
					{
						text = " FID = 0 ";
					}
					if (!string.IsNullOrWhiteSpace(e.ListFilterParameter.Filter))
					{
						e.ListFilterParameter.Filter = e.ListFilterParameter.Filter + " AND " + text;
						return;
					}
					e.ListFilterParameter.Filter = text;
					return;
				}
			}
		}

		// Token: 0x060003AC RID: 940 RVA: 0x0002DF74 File Offset: 0x0002C174
		private void ShowModelSysLable(int rowIndex, string key)
		{
			EntryEntity entryEntity = base.View.BusinessInfo.GetEntryEntity("FEntity");
			int entryCurrentRowIndex = base.View.Model.GetEntryCurrentRowIndex("FEntity");
			DynamicObject entityDataObject = base.View.Model.GetEntityDataObject(entryEntity, entryCurrentRowIndex);
			string dynamicValue = DataEntityExtend.GetDynamicValue<string>(entityDataObject, "BaseDataTypeId_Id", null);
			DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
			dynamicFormShowParameter.OpenStyle.ShowType = 4;
			dynamicFormShowParameter.MultiSelect = false;
			dynamicFormShowParameter.FormId = "ENG_MODELSYSLABLESHOW";
			dynamicFormShowParameter.CustomComplexParams.Add("baseDataFormID", dynamicValue);
			base.View.ShowForm(dynamicFormShowParameter, delegate(FormResult result)
			{
				if (result != null && result.ReturnData != null && result != null && result.ReturnData != null)
				{
					DynamicObject dynamicObject = result.ReturnData as DynamicObject;
					string dynamicValue2 = DataEntityExtend.GetDynamicValue<string>(dynamicObject, "SYSLBALENUMBER", null);
					string dynamicValue3 = DataEntityExtend.GetDynamicValue<string>(dynamicObject, "SYSLABLETYPE", null);
					if (key == "FOperationLeftVB")
					{
						this.View.Model.SetValue("FOperationLeftVB", dynamicValue2, rowIndex);
						this.View.Model.SetValue("FOPLeftVARTYPE", dynamicValue3, rowIndex);
					}
					if (key == "FOperationRightVB")
					{
						this.View.Model.SetValue("FOperationRightVB", dynamicValue2, rowIndex);
						this.View.Model.SetValue("FOPRightVARTYPE", dynamicValue3, rowIndex);
					}
					if (key == "FOPForMulaVariable")
					{
						this.View.Model.SetValue("FOPForMulaVariable", dynamicValue2, rowIndex);
						this.View.Model.SetValue("FOPForMulaVarType", dynamicValue3, rowIndex);
					}
				}
			});
		}

		// Token: 0x060003AD RID: 941 RVA: 0x0002E040 File Offset: 0x0002C240
		private List<DynamicObject> GetVariableList()
		{
			List<DynamicObject> list = new List<DynamicObject>();
			DynamicObject dynamicObject = new DynamicObject(this.VariableShowEntity.DynamicObjectType);
			DynamicObjectCollection dynamicObjectCollection = base.View.ParentFormView.Model.DataObject["Entity"] as DynamicObjectCollection;
			DynamicObjectCollection dynamicObjectCollection2 = base.View.ParentFormView.Model.DataObject["CalEntity"] as DynamicObjectCollection;
			foreach (DynamicObject dynamicObject2 in dynamicObjectCollection)
			{
				DynamicObject dynamicObject3 = dynamicObject2["ModelVarId"] as DynamicObject;
				if (dynamicObject3 != null)
				{
					Convert.ToString(dynamicObject3["VarType"]);
					DynamicObject dynamicObject4 = OrmUtils.Clone(dynamicObject, false, true) as DynamicObject;
					dynamicObject4["VarNumber"] = dynamicObject3["Number"];
					dynamicObject4["VarName"] = DataEntityExtend.GetDynamicValue<LocaleValue>(dynamicObject3, "Name", null).ToString();
					dynamicObject4["VarType"] = dynamicObject3["VarType"].ToString();
					dynamicObject4["VarControl"] = dynamicObject3["ControlType"];
					dynamicObject4["IsCalVariable"] = 0;
					list.Add(dynamicObject4);
				}
			}
			foreach (DynamicObject dynamicObject5 in dynamicObjectCollection2)
			{
				Convert.ToString(dynamicObject5["VarType"]);
				DynamicObject dynamicObject6 = OrmUtils.Clone(dynamicObject, false, true) as DynamicObject;
				dynamicObject6["VarName"] = dynamicObject5["VarNumber"];
				dynamicObject6["VarType"] = dynamicObject5["VarType"];
				dynamicObject6["VarControl"] = 'B';
				dynamicObject6["IsCalVariable"] = 1;
				list.Add(dynamicObject6);
			}
			return list;
		}

		// Token: 0x060003AE RID: 942 RVA: 0x0002E26C File Offset: 0x0002C46C
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

		// Token: 0x060003AF RID: 943 RVA: 0x0002E2C8 File Offset: 0x0002C4C8
		private void SetForMulaVarEditButton(int rowIndex, string key)
		{
			string text = string.Empty;
			string text2 = string.Empty;
			string varTypeKey = string.Empty;
			if (key == "FOPERATIOENTITY" || key == "FOpVBLeftType")
			{
				text = "FOperatioEntity";
				text2 = "FOperationLeftVB";
				varTypeKey = "OpVBLeftType";
			}
			if (key == "FOPERATIOENTITY" || key == "FOPVBRightType")
			{
				text = "FOperatioEntity";
				text2 = "FOperationRightVB";
				varTypeKey = "OPVBRightType";
			}
			EntryEntity entryEntity = base.View.BusinessInfo.GetEntryEntity(text);
			DynamicObject entityDataObject = base.View.Model.GetEntityDataObject(entryEntity, rowIndex);
			if (key == "FOPERATIOENTITY")
			{
				this.SetTextFieldProperty(entityDataObject, "OpVBLeftType", "FOperationLeftVB", rowIndex);
			}
			this.SetTextFieldProperty(entityDataObject, varTypeKey, text2, rowIndex);
			base.View.UpdateView(text2);
		}

		// Token: 0x060003B0 RID: 944 RVA: 0x0002E39C File Offset: 0x0002C59C
		private void SetTextFieldProperty(DynamicObject entryData, string varTypeKey, string fieldKey, int rowIndex)
		{
			int dynamicValue = DataEntityExtend.GetDynamicValue<int>(entryData, varTypeKey, 0);
			if (dynamicValue == 2)
			{
				base.View.GetFieldEditor(fieldKey, rowIndex).SetCustomPropertyValue("editable", true);
				base.View.GetFieldEditor(fieldKey, rowIndex).SetCustomPropertyValue("showEditButton", false);
				return;
			}
			base.View.GetFieldEditor(fieldKey, rowIndex).SetCustomPropertyValue("editable", false);
			base.View.GetFieldEditor(fieldKey, rowIndex).SetCustomPropertyValue("showEditButton", true);
		}

		// Token: 0x060003B1 RID: 945 RVA: 0x0002E430 File Offset: 0x0002C630
		private void SetValueByKey(string varNumber, string varType, string varControl, long varId, string isCalVar, string key, int rowIndex)
		{
			if (key == "FOperationLeftVB")
			{
				base.View.Model.SetValue("FOPLeftVARID", varId, rowIndex);
				base.View.Model.SetValue("FOPLeftVARTYPE", varType, rowIndex);
				base.View.Model.SetValue("FOperationLeftVB", varNumber, rowIndex);
				base.View.Model.SetValue("FOPLeftIsCalVar", isCalVar, rowIndex);
			}
			if (key == "FOperationRightVB")
			{
				base.View.Model.SetValue("FOPRightVARID", varId, rowIndex);
				base.View.Model.SetValue("FOPRightVARTYPE", varType, rowIndex);
				base.View.Model.SetValue("FOperationRightVB", varNumber, rowIndex);
				base.View.Model.SetValue("FOPRightIsCalVar", isCalVar, rowIndex);
			}
		}

		// Token: 0x060003B2 RID: 946 RVA: 0x0002E550 File Offset: 0x0002C750
		private void DelInvalidRow()
		{
			DynamicObjectCollection dynamicValue = DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(base.View.Model.DataObject, "OperatioEntity", null);
			IEnumerable<DynamicObject> source = from w in dynamicValue
			where string.IsNullOrWhiteSpace(DataEntityExtend.GetDynamicValue<string>(w, "OperationLeftVB", null)) && string.IsNullOrWhiteSpace(DataEntityExtend.GetDynamicValue<string>(w, "OperationRightVB", null))
			select w;
			for (int i = source.Count<DynamicObject>() - 1; i >= 0; i--)
			{
				int num = DataEntityExtend.GetDynamicObjectItemValue<int>(source.ElementAt(i), "Seq", 0) - 1;
				this.Model.DeleteEntryRow("FOperatioEntity", num);
			}
		}

		// Token: 0x04000197 RID: 407
		private Entity variableShowEntity;
	}
}
