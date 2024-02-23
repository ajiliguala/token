using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.JSON;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.Util;
using Kingdee.BOS.VerificationHelper.Verifiers;
using Kingdee.K3.Core.MFG.ENG.ProductModel;
using Kingdee.K3.Core.MFG.ENG.ProductModel.Expression;
using Kingdee.K3.Core.MFG.ENG.ProductModel.Rule;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.Core.MFG.Utils;
using Kingdee.K3.MFG.ServiceHelper.ENG;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.Base
{
	// Token: 0x02000031 RID: 49
	[Description("模型规则插件")]
	public class ProductModeRuleEdit : BaseControlEdit
	{
		// Token: 0x06000384 RID: 900 RVA: 0x0002AC84 File Offset: 0x00028E84
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			base.BarItemClick(e);
			string barItemKey;
			if ((barItemKey = e.BarItemKey) != null)
			{
				if (!(barItemKey == "tbTransRule"))
				{
					return;
				}
				long value = MFGBillUtil.GetValue<long>(base.View.Model, "FMODELCODE", -1, 0L, null);
				new List<AbstractRule>();
				ProductModelRuleServiceHelper.GetRuleByProductModelId(base.Context, value);
				DynamicObjectCollection dynamicValue = DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(base.View.Model.DataObject, "Entity", null);
				foreach (DynamicObject dynamicObject in dynamicValue)
				{
					int dynamicValue2 = DataEntityExtend.GetDynamicValue<int>(dynamicObject, "RuleType", 0);
					if (dynamicValue2 == 1)
					{
						DynamicObjectCollection dynamicValue3 = DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(dynamicObject, "ForMulaEntity", null);
						new List<AbstractModelExpression>();
						ProductModelRuleServiceHelper.GetForMulaRuleData(base.Context, dynamicValue3, false);
					}
					if (dynamicValue2 == 2)
					{
						DynamicObjectCollection dynamicValue4 = DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(dynamicObject, "OperatioEntity", null);
						new List<AbstractModelExpression>();
						ProductModelRuleServiceHelper.GetOperationData(base.Context, dynamicValue4, false);
					}
					if (dynamicValue2 == 3)
					{
						DynamicObjectCollection dynamicValue5 = DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(dynamicObject, "CONEntity", null);
						new List<AbstractModelExpression>();
						ProductModelRuleServiceHelper.GetConstraintData(base.Context, dynamicValue5, false);
					}
				}
			}
		}

		// Token: 0x06000385 RID: 901 RVA: 0x0002ADC0 File Offset: 0x00028FC0
		public override void PreOpenForm(PreOpenFormEventArgs e)
		{
			base.PreOpenForm(e);
			FeatureVerifier.CheckFeature(e.Context, "PDB");
		}

		// Token: 0x06000386 RID: 902 RVA: 0x0002ADD9 File Offset: 0x00028FD9
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
			this.SetSimpleCol("FForMulaEntity");
			this.SetSimpleCol("FOperatioEntity");
			this.SetSimpleCol("FOPForMulaEntity");
			this.SetSimpleCol("FCONEntity");
		}

		// Token: 0x06000387 RID: 903 RVA: 0x0002AE10 File Offset: 0x00029010
		private void SetSimpleCol(string entryKey)
		{
			JSONArray jsonarray = new JSONArray();
			Entity entryEntity = base.View.BillBusinessInfo.GetEntryEntity(entryKey);
			jsonarray.Add("FSeq");
			foreach (Field field in entryEntity.Fields)
			{
				jsonarray.Add(field.Key);
			}
			base.View.GetControl<EntryGrid>(entryKey).InvokeControlMethod("SetAllowMovingAndResizing", new object[]
			{
				jsonarray,
				false
			});
		}

		// Token: 0x06000388 RID: 904 RVA: 0x0002AEBC File Offset: 0x000290BC
		public override void EntityRowClick(EntityRowClickEventArgs e)
		{
			base.EntityRowClick(e);
			string a;
			if ((a = e.Key.ToUpperInvariant()) != null)
			{
				if (a == "FENTITY")
				{
					this.LockEntry(e.Row);
					return;
				}
				if (!(a == "FFORMULAENTITY") && !(a == "FOPERATIOENTITY") && !(a == "FOPFORMULAENTITY") && !(a == "FCONENTITY"))
				{
					return;
				}
				this.SetForMulaVarEditButton(e.Row, e.Key.ToUpper());
			}
		}

		// Token: 0x06000389 RID: 905 RVA: 0x0002AF48 File Offset: 0x00029148
		public override void DataChanged(DataChangedEventArgs e)
		{
			base.DataChanged(e);
			string key;
			switch (key = e.Field.Key)
			{
			case "FRuleType":
				this.LockEntry(e.Row);
				return;
			case "FModelVariable":
				break;
			case "FVariableType":
			case "FOpVBLeftType":
			case "FOPVBRightType":
			case "FOPForMulaType":
			case "FConVariableType":
				this.SetForMulaVarEditButton(e.Row, e.Field.Key);
				return;
			case "FForMulaOperator":
				this.ResetCurrentRow(e.NewValue, e.Row);
				return;
			case "FOpForMulaOperator":
				this.ResetOpCurrentRow(e.NewValue, e.Row);
				break;

				return;
			}
		}

		// Token: 0x0600038A RID: 906 RVA: 0x0002B098 File Offset: 0x00029298
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			base.BeforeF7Select(e);
			string fieldKey;
			switch (fieldKey = e.FieldKey)
			{
			case "FMODELCODE":
				e.IsShowApproved = false;
				e.IsShowUsed = true;
				return;
			case "FModelVariable":
			case "FOpVariable":
				this.GetVariableFilter(e.Row, string.Empty, e.FieldKey);
				return;
			case "FForMulaVBLeft":
			case "FOPForMulaVariable":
			{
				EntryEntity entryEntity = base.View.BusinessInfo.GetEntryEntity("FEntity");
				int entryCurrentRowIndex = base.View.Model.GetEntryCurrentRowIndex("FEntity");
				DynamicObject entityDataObject = base.View.Model.GetEntityDataObject(entryEntity, entryCurrentRowIndex);
				string dynamicValue = DataEntityExtend.GetDynamicValue<string>(entityDataObject, "ModelVariable", null);
				string dynamicValue2 = DataEntityExtend.GetDynamicValue<string>(entityDataObject, "OpVariable", null);
				if (e.FieldKey == "FForMulaVBLeft" && ObjectUtils.IsNullOrEmptyOrWhiteSpace(dynamicValue))
				{
					base.View.ShowMessage(ResManager.LoadKDString("请先选择公式页签下方的目标变量！", "015072000025074", 7, new object[0]), 0);
					e.Cancel = true;
					return;
				}
				if (e.FieldKey == "FOPForMulaVariable" && ObjectUtils.IsNullOrEmptyOrWhiteSpace(dynamicValue2))
				{
					base.View.ShowMessage(ResManager.LoadKDString("请先选择操作页签下方的目标变量！", "015072000025075", 7, new object[0]), 0);
					e.Cancel = true;
					return;
				}
				long value = MFGBillUtil.GetValue<long>(base.View.Model, "FMODELCODE", -1, 0L, null);
				ModelVariableOption modelVariableOption = new ModelVariableOption();
				List<long> list = new List<long>();
				new List<long>();
				if (value != 0L)
				{
					list.Add(value);
				}
				modelVariableOption.productModelIds = list;
				DynamicObjectCollection variableType = ProductModelRuleServiceHelper.GetVariableType(base.Context, modelVariableOption);
				long varId = DataEntityExtend.GetDynamicValue<long>(entityDataObject, "VariableID", 0L);
				DynamicObject dynamicObject = (from w in variableType
				where DataEntityExtend.GetDynamicValue<long>(w, "VariableId", 0L) == varId
				select w).FirstOrDefault<DynamicObject>();
				string dynamicValue3 = DataEntityExtend.GetDynamicValue<string>(dynamicObject, "VarType", null);
				if (e.FieldKey == "FForMulaVBLeft" || e.FieldKey == "FConLeftVar" || e.FieldKey == "FConRightVar")
				{
					this.GetVariableFilter(e.Row, dynamicValue3, e.FieldKey);
				}
				if (e.FieldKey == "FOPForMulaVariable")
				{
					int num2 = Convert.ToInt32(base.View.Model.GetValue("FOPForMulaType", e.Row));
					if (num2 == 3)
					{
						this.ShowModelSysLable(e.Row, e.FieldKey);
					}
					if (num2 == 1)
					{
						this.GetVariableFilter(e.Row, dynamicValue3, e.FieldKey);
						return;
					}
				}
				break;
			}
			case "FConLeftVar":
			case "FConRightVar":
				this.GetVariableFilter(e.Row, string.Empty, e.FieldKey);
				return;
			case "FOperationLeftVB":
			{
				int num3 = Convert.ToInt32(base.View.Model.GetValue("FOpVBLeftType", e.Row));
				if (num3 == 3)
				{
					this.ShowModelSysLable(e.Row, e.FieldKey);
				}
				if (num3 == 1)
				{
					this.GetVariableFilter(e.Row, string.Empty, e.FieldKey);
					return;
				}
				break;
			}
			case "FOperationRightVB":
			{
				int num4 = Convert.ToInt32(base.View.Model.GetValue("FOPVBRightType", e.Row));
				if (num4 == 3)
				{
					this.ShowModelSysLable(e.Row, e.FieldKey);
				}
				if (num4 == 1)
				{
					this.GetVariableFilter(e.Row, string.Empty, e.FieldKey);
				}
				break;
			}

				return;
			}
		}

		// Token: 0x0600038B RID: 907 RVA: 0x0002B49C File Offset: 0x0002969C
		public override void EntryBarItemClick(BarItemClickEventArgs e)
		{
			base.EntryBarItemClick(e);
			string barItemKey;
			switch (barItemKey = e.BarItemKey)
			{
			case "tbMoveUp":
				this.EntryRowMoveUpOrDown("FEntity", true);
				return;
			case "tbMoveDown":
				this.EntryRowMoveUpOrDown("FEntity", false);
				return;
			case "tbForMulaMoveUp":
				this.EntryRowMoveUpOrDown("FForMulaEntity", true);
				return;
			case "tbForMulaMoveDown":
				this.EntryRowMoveUpOrDown("FForMulaEntity", false);
				return;
			case "tbOPMoveUp":
				this.EntryRowMoveUpOrDown("FOperatioEntity", true);
				return;
			case "tbOPMoveDown":
				this.EntryRowMoveUpOrDown("FOperatioEntity", false);
				return;
			case "tbOpForMulaMoveUp":
				this.EntryRowMoveUpOrDown("FOPForMulaEntity", true);
				return;
			case "tbOpForMulaMoveDown":
				this.EntryRowMoveUpOrDown("FOPForMulaEntity", false);
				return;
			case "tbConMoveUp":
				this.EntryRowMoveUpOrDown("FCONEntity", true);
				return;
			case "tbConMoveDown":
				this.EntryRowMoveUpOrDown("FCONEntity", false);
				break;

				return;
			}
		}

		// Token: 0x0600038C RID: 908 RVA: 0x0002B614 File Offset: 0x00029814
		public override void BeforeDoOperation(BeforeDoOperationEventArgs e)
		{
			base.BeforeDoOperation(e);
			this.curCursor = base.View.Model.GetEntryCurrentRowIndex("FEntity");
			string a;
			if ((a = e.Operation.FormOperation.Operation.ToUpperInvariant()) != null)
			{
				if (!(a == "SAVE"))
				{
					return;
				}
				this.DelInvalidRow();
			}
		}

		// Token: 0x0600038D RID: 909 RVA: 0x0002B670 File Offset: 0x00029870
		public override void AfterDoOperation(AfterDoOperationEventArgs e)
		{
			base.AfterDoOperation(e);
			string a;
			if ((a = e.Operation.Operation.ToString().ToUpper()) != null)
			{
				if (!(a == "SAVE"))
				{
					return;
				}
				DynamicObjectCollection dynamicValue = DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(base.View.Model.DataObject, "Entity", null);
				foreach (DynamicObject dynamicObject in dynamicValue)
				{
					int rowIndex = DataEntityExtend.GetDynamicValue<int>(dynamicObject, "Seq", 0) - 1;
					this.LockEntry(rowIndex);
				}
				base.View.UpdateView("FEntity");
				base.View.UpdateView("FForMulaEntity");
				base.View.UpdateView("FOperatioEntity");
				base.View.UpdateView("FOPForMulaEntity");
				base.View.UpdateView("FCONEntity");
				base.View.SetEntityFocusRow("FEntity", this.curCursor);
			}
		}

		// Token: 0x0600038E RID: 910 RVA: 0x0002B780 File Offset: 0x00029980
		public override void BeforeSetItemValueByNumber(BeforeSetItemValueByNumberArgs e)
		{
			base.BeforeSetItemValueByNumber(e);
			if (e.BaseDataFieldKey == "FMODELCODE")
			{
				e.IsShowApproved = false;
				e.IsShowUsed = true;
			}
		}

		// Token: 0x0600038F RID: 911 RVA: 0x0002B7A9 File Offset: 0x000299A9
		public override void AfterBindData(EventArgs e)
		{
			base.AfterBindData(e);
			this.SetAllowSorting("FForMulaEntity");
			this.SetAllowSorting("FOperatioEntity");
			this.SetAllowSorting("FOPForMulaEntity");
			this.SetAllowSorting("FCONEntity");
		}

		// Token: 0x06000390 RID: 912 RVA: 0x0002B7E0 File Offset: 0x000299E0
		private void SetAllowSorting(string entryKey)
		{
			EntryGrid control = base.View.GetControl<EntryGrid>(entryKey);
			control.SetCustomPropertyValue("AllowSorting", false);
		}

		// Token: 0x06000391 RID: 913 RVA: 0x0002B80C File Offset: 0x00029A0C
		private void LockEntry(int rowIndex)
		{
			EntryEntity entryEntity = base.View.BusinessInfo.GetEntryEntity("FEntity");
			EntryEntity entryEntity2 = base.View.BusinessInfo.GetEntryEntity("FForMulaEntity");
			DynamicObjectCollection entityDataObject = base.View.Model.GetEntityDataObject(entryEntity2);
			EntryEntity entryEntity3 = base.View.BusinessInfo.GetEntryEntity("FOperatioEntity");
			EntryEntity entryEntity4 = base.View.BusinessInfo.GetEntryEntity("FOPForMulaEntity");
			DynamicObjectCollection entityDataObject2 = base.View.Model.GetEntityDataObject(entryEntity4);
			EntryEntity entryEntity5 = base.View.BusinessInfo.GetEntryEntity("FCONEntity");
			DynamicObject entityDataObject3 = base.View.Model.GetEntityDataObject(entryEntity, rowIndex);
			int dynamicValue = DataEntityExtend.GetDynamicValue<int>(entityDataObject3, "RuleType", 0);
			DataEntityExtend.GetDynamicValue<string>(entityDataObject3, "ModelVariable", null);
			switch (dynamicValue)
			{
			case 1:
				base.View.GetFieldEditor("FOpVariable", rowIndex).Enabled = false;
				base.View.GetFieldEditor("FOpeCode", rowIndex).Enabled = false;
				base.View.GetFieldEditor("FVariableControl", rowIndex).Enabled = false;
				base.View.GetFieldEditor("FBaseDataTypeId", rowIndex).Enabled = false;
				base.View.StyleManager.SetEnabled(entryEntity3, "", false);
				base.View.StyleManager.SetEnabled(entryEntity4, "", false);
				base.View.GetBarItem("FOperatioEntity", "tbOPButton").Enabled = false;
				base.View.GetBarItem("FOperatioEntity", "tbOPNewEntry").Enabled = false;
				base.View.GetBarItem("FOperatioEntity", "tbOPInsertEntry").Enabled = false;
				base.View.GetBarItem("FOperatioEntity", "tbOPDeleteEntry").Enabled = false;
				base.View.GetBarItem("FOperatioEntity", "tbOPMoveUp").Enabled = false;
				base.View.GetBarItem("FOperatioEntity", "tbOPMoveDown").Enabled = false;
				base.View.GetBarItem("FOPForMulaEntity", "tbOPForMula").Enabled = false;
				base.View.GetBarItem("FOPForMulaEntity", "tbOPForMulaNewEntry").Enabled = false;
				base.View.GetBarItem("FOPForMulaEntity", "tbOpForMulaInsertEntry").Enabled = false;
				base.View.GetBarItem("FOPForMulaEntity", "tbOpForMulaDeleteEntry").Enabled = false;
				base.View.GetBarItem("FOPForMulaEntity", "tbOpForMulaMoveUp").Enabled = false;
				base.View.GetBarItem("FOPForMulaEntity", "tbOpForMulaMoveDown").Enabled = false;
				base.View.GetFieldEditor("FRemark", rowIndex).Enabled = false;
				base.View.StyleManager.SetEnabled(entryEntity5, "", false);
				base.View.GetBarItem("FCONEntity", "tbConSplitButton").Enabled = false;
				base.View.GetBarItem("FCONEntity", "tbConInsertEntry").Enabled = false;
				base.View.GetBarItem("FCONEntity", "tbConNewEntry").Enabled = false;
				base.View.GetBarItem("FCONEntity", "tbConDeleteEntry").Enabled = false;
				base.View.GetBarItem("FCONEntity", "tbConMoveUp").Enabled = false;
				base.View.GetBarItem("FCONEntity", "tbConMoveDown").Enabled = false;
				base.View.GetFieldEditor("FModelVariable", rowIndex).Enabled = true;
				base.View.GetFieldEditor("FCode", rowIndex).Enabled = true;
				base.View.StyleManager.SetEnabled(entryEntity2, "", true);
				foreach (DynamicObject dynamicObject in entityDataObject)
				{
					if (DataEntityExtend.GetDynamicObjectItemValue<string>(dynamicObject, "ForMulaOperator", null) == "5" || DataEntityExtend.GetDynamicObjectItemValue<string>(dynamicObject, "ForMulaOperator", null) == "6" || DataEntityExtend.GetDynamicObjectItemValue<string>(dynamicObject, "ForMulaOperator", null) == "7")
					{
						this.DisableCurrentRow(dynamicObject);
					}
				}
				base.View.GetBarItem("FForMulaEntity", "tbForMula").Enabled = true;
				base.View.GetBarItem("FForMulaEntity", "tbForMulaInsertEntry").Enabled = true;
				base.View.GetBarItem("FForMulaEntity", "tbForMulaNewEntry").Enabled = true;
				base.View.GetBarItem("FForMulaEntity", "tbForMulaDelteEntry").Enabled = true;
				base.View.GetBarItem("FForMulaEntity", "tbForMulaMoveUp").Enabled = true;
				base.View.GetBarItem("FForMulaEntity", "tbForMulaMoveDown").Enabled = true;
				return;
			case 2:
				base.View.GetFieldEditor("FModelVariable", rowIndex).Enabled = false;
				base.View.GetFieldEditor("FCode", rowIndex).Enabled = false;
				base.View.StyleManager.SetEnabled(entryEntity2, "", false);
				base.View.GetBarItem("FForMulaEntity", "tbForMula").Enabled = false;
				base.View.GetBarItem("FForMulaEntity", "tbForMulaInsertEntry").Enabled = false;
				base.View.GetBarItem("FForMulaEntity", "tbForMulaNewEntry").Enabled = false;
				base.View.GetBarItem("FForMulaEntity", "tbForMulaDelteEntry").Enabled = false;
				base.View.GetBarItem("FForMulaEntity", "tbForMulaMoveUp").Enabled = false;
				base.View.GetBarItem("FForMulaEntity", "tbForMulaMoveDown").Enabled = false;
				base.View.GetFieldEditor("FRemark", rowIndex).Enabled = false;
				base.View.StyleManager.SetEnabled(entryEntity5, "", false);
				base.View.GetBarItem("FCONEntity", "tbConSplitButton").Enabled = false;
				base.View.GetBarItem("FCONEntity", "tbConInsertEntry").Enabled = false;
				base.View.GetBarItem("FCONEntity", "tbConNewEntry").Enabled = false;
				base.View.GetBarItem("FCONEntity", "tbConDeleteEntry").Enabled = false;
				base.View.GetBarItem("FCONEntity", "tbConMoveUp").Enabled = false;
				base.View.GetBarItem("FCONEntity", "tbConMoveDown").Enabled = false;
				base.View.GetFieldEditor("FOpVariable", rowIndex).Enabled = true;
				base.View.GetFieldEditor("FOpeCode", rowIndex).Enabled = true;
				base.View.GetFieldEditor("FVariableControl", rowIndex).Enabled = true;
				base.View.GetFieldEditor("FBaseDataTypeId", rowIndex).Enabled = true;
				base.View.StyleManager.SetEnabled(entryEntity3, "", true);
				base.View.StyleManager.SetEnabled(entryEntity4, "", true);
				foreach (DynamicObject dynamicObject2 in entityDataObject2)
				{
					if (DataEntityExtend.GetDynamicObjectItemValue<string>(dynamicObject2, "OpForMulaOperator", null) == "5" || DataEntityExtend.GetDynamicObjectItemValue<string>(dynamicObject2, "OpForMulaOperator", null) == "6" || DataEntityExtend.GetDynamicObjectItemValue<string>(dynamicObject2, "OpForMulaOperator", null) == "7")
					{
						this.DisableOpCurrentRow(dynamicObject2);
					}
				}
				base.View.GetBarItem("FOperatioEntity", "tbOPButton").Enabled = true;
				base.View.GetBarItem("FOperatioEntity", "tbOPNewEntry").Enabled = true;
				base.View.GetBarItem("FOperatioEntity", "tbOPInsertEntry").Enabled = true;
				base.View.GetBarItem("FOperatioEntity", "tbOPDeleteEntry").Enabled = true;
				base.View.GetBarItem("FOperatioEntity", "tbOPMoveUp").Enabled = true;
				base.View.GetBarItem("FOperatioEntity", "tbOPMoveDown").Enabled = true;
				base.View.GetBarItem("FOPForMulaEntity", "tbOPForMula").Enabled = true;
				base.View.GetBarItem("FOPForMulaEntity", "tbOPForMulaNewEntry").Enabled = true;
				base.View.GetBarItem("FOPForMulaEntity", "tbOpForMulaInsertEntry").Enabled = true;
				base.View.GetBarItem("FOPForMulaEntity", "tbOpForMulaDeleteEntry").Enabled = true;
				base.View.GetBarItem("FOPForMulaEntity", "tbOpForMulaMoveUp").Enabled = true;
				base.View.GetBarItem("FOPForMulaEntity", "tbOpForMulaMoveDown").Enabled = true;
				return;
			case 3:
				base.View.GetFieldEditor("FModelVariable", rowIndex).Enabled = false;
				base.View.GetFieldEditor("FCode", rowIndex).Enabled = false;
				base.View.StyleManager.SetEnabled(entryEntity2, "", false);
				base.View.GetBarItem("FForMulaEntity", "tbForMula").Enabled = false;
				base.View.GetBarItem("FForMulaEntity", "tbForMulaInsertEntry").Enabled = false;
				base.View.GetBarItem("FForMulaEntity", "tbForMulaNewEntry").Enabled = false;
				base.View.GetBarItem("FForMulaEntity", "tbForMulaDelteEntry").Enabled = false;
				base.View.GetBarItem("FForMulaEntity", "tbForMulaMoveUp").Enabled = false;
				base.View.GetBarItem("FForMulaEntity", "tbForMulaMoveDown").Enabled = false;
				base.View.GetFieldEditor("FOpVariable", rowIndex).Enabled = false;
				base.View.GetFieldEditor("FOpeCode", rowIndex).Enabled = false;
				base.View.GetFieldEditor("FVariableControl", rowIndex).Enabled = false;
				base.View.GetFieldEditor("FBaseDataTypeId", rowIndex).Enabled = false;
				base.View.StyleManager.SetEnabled(entryEntity3, "", false);
				base.View.StyleManager.SetEnabled(entryEntity4, "", false);
				base.View.GetBarItem("FOperatioEntity", "tbOPButton").Enabled = false;
				base.View.GetBarItem("FOperatioEntity", "tbOPNewEntry").Enabled = false;
				base.View.GetBarItem("FOperatioEntity", "tbOPInsertEntry").Enabled = false;
				base.View.GetBarItem("FOperatioEntity", "tbOPDeleteEntry").Enabled = false;
				base.View.GetBarItem("FOperatioEntity", "tbOPMoveUp").Enabled = false;
				base.View.GetBarItem("FOperatioEntity", "tbOPMoveDown").Enabled = false;
				base.View.GetBarItem("FOPForMulaEntity", "tbOPForMula").Enabled = false;
				base.View.GetBarItem("FOPForMulaEntity", "tbOPForMulaNewEntry").Enabled = false;
				base.View.GetBarItem("FOPForMulaEntity", "tbOpForMulaInsertEntry").Enabled = false;
				base.View.GetBarItem("FOPForMulaEntity", "tbOpForMulaDeleteEntry").Enabled = false;
				base.View.GetBarItem("FOPForMulaEntity", "tbOpForMulaMoveUp").Enabled = false;
				base.View.GetBarItem("FOPForMulaEntity", "tbOpForMulaMoveDown").Enabled = false;
				base.View.GetFieldEditor("FRemark", rowIndex).Enabled = true;
				base.View.StyleManager.SetEnabled(entryEntity5, "", true);
				base.View.GetBarItem("FCONEntity", "tbConSplitButton").Enabled = true;
				base.View.GetBarItem("FCONEntity", "tbConInsertEntry").Enabled = true;
				base.View.GetBarItem("FCONEntity", "tbConNewEntry").Enabled = true;
				base.View.GetBarItem("FCONEntity", "tbConDeleteEntry").Enabled = true;
				base.View.GetBarItem("FCONEntity", "tbConMoveUp").Enabled = true;
				base.View.GetBarItem("FCONEntity", "tbConMoveDown").Enabled = true;
				return;
			default:
				return;
			}
		}

		// Token: 0x06000392 RID: 914 RVA: 0x0002C4A8 File Offset: 0x0002A6A8
		private void LockForMulaVariableType(int rowIndex)
		{
			EntryEntity entryEntity = base.View.BusinessInfo.GetEntryEntity("FEntity");
			DynamicObject entityDataObject = base.View.Model.GetEntityDataObject(entryEntity, rowIndex);
			string dynamicValue = DataEntityExtend.GetDynamicValue<string>(entityDataObject, "VarType", null);
			string a;
			if ((a = dynamicValue) != null)
			{
				if (a == "A" || a == "B" || a == "E")
				{
					this.LockAndSetForMulaVariableType("1");
					return;
				}
				if (!(a == "C") && !(a == "D"))
				{
					return;
				}
				this.LockAndSetForMulaVariableType("2");
			}
		}

		// Token: 0x06000393 RID: 915 RVA: 0x0002C54C File Offset: 0x0002A74C
		private void LockAndSetForMulaVariableType(string varType)
		{
			base.View.GetControl("FVariableType").Enabled = false;
			int entryRowCount = base.View.Model.GetEntryRowCount("FForMulaEntity");
			for (int i = 0; i <= entryRowCount - 1; i++)
			{
				base.View.Model.SetValue("FVariableType", varType, i);
			}
		}

		// Token: 0x06000394 RID: 916 RVA: 0x0002C5AC File Offset: 0x0002A7AC
		private void SetForMulaVarEditButton(int rowIndex, string key)
		{
			string text = string.Empty;
			string text2 = string.Empty;
			string varTypeKey = string.Empty;
			string empty = string.Empty;
			if (key == "FFORMULAENTITY" || key == "FVariableType")
			{
				text = "FForMulaEntity";
				text2 = "FForMulaVBLeft";
				varTypeKey = "VariableType";
			}
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
			if (key == "FOPFORMULAENTITY" || key == "FOPForMulaType")
			{
				text = "FOPForMulaEntity";
				text2 = "FOPForMulaVariable";
				varTypeKey = "OPForMulaType";
			}
			if (key == "FCONENTITY" || key == "FConVariableType")
			{
				text = "FCONEntity";
				text2 = "FConRightVar";
				varTypeKey = "ConVariableType";
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

		// Token: 0x06000395 RID: 917 RVA: 0x0002C70C File Offset: 0x0002A90C
		private void ResetCurrentRow(object newValue, int rowIndex)
		{
			EntryEntity entryEntity = base.View.BusinessInfo.GetEntryEntity("FForMulaEntity");
			DynamicObject entityDataObject = base.View.Model.GetEntityDataObject(entryEntity, rowIndex);
			if (Convert.ToString(newValue) == "5" || Convert.ToString(newValue) == "6" || Convert.ToString(newValue) == "7")
			{
				this.ClearCurrentRow(rowIndex);
				this.DisableCurrentRow(entityDataObject);
				return;
			}
			this.EnableCurrentRow(entityDataObject);
		}

		// Token: 0x06000396 RID: 918 RVA: 0x0002C78E File Offset: 0x0002A98E
		private void ClearCurrentRow(int rowIndex)
		{
			base.View.Model.SetValue("FVariableType", "", rowIndex);
			base.View.Model.SetValue("FForMulaVBLeft", "", rowIndex);
		}

		// Token: 0x06000397 RID: 919 RVA: 0x0002C7C6 File Offset: 0x0002A9C6
		private void DisableCurrentRow(DynamicObject entryData)
		{
			MFGBillUtil.SetEnabled(base.View, "FVariableType", entryData, false, "");
			MFGBillUtil.SetEnabled(base.View, "FForMulaVBLeft", entryData, false, "");
		}

		// Token: 0x06000398 RID: 920 RVA: 0x0002C7F6 File Offset: 0x0002A9F6
		private void EnableCurrentRow(DynamicObject entryData)
		{
			MFGBillUtil.SetEnabled(base.View, "FVariableType", entryData, true, "");
			MFGBillUtil.SetEnabled(base.View, "FForMulaVBLeft", entryData, true, "");
		}

		// Token: 0x06000399 RID: 921 RVA: 0x0002C828 File Offset: 0x0002AA28
		private void ResetOpCurrentRow(object newValue, int rowIndex)
		{
			EntryEntity entryEntity = base.View.BusinessInfo.GetEntryEntity("FOPForMulaEntity");
			DynamicObject entityDataObject = base.View.Model.GetEntityDataObject(entryEntity, rowIndex);
			if (Convert.ToString(newValue) == "5" || Convert.ToString(newValue) == "6" || Convert.ToString(newValue) == "7")
			{
				this.ClearOpCurrentRow(rowIndex);
				this.DisableOpCurrentRow(entityDataObject);
				return;
			}
			this.EnableOpCurrentRow(entityDataObject);
		}

		// Token: 0x0600039A RID: 922 RVA: 0x0002C8AA File Offset: 0x0002AAAA
		private void ClearOpCurrentRow(int rowIndex)
		{
			base.View.Model.SetValue("FOPForMulaType", "", rowIndex);
			base.View.Model.SetValue("FOPForMulaVariable", "", rowIndex);
		}

		// Token: 0x0600039B RID: 923 RVA: 0x0002C8E2 File Offset: 0x0002AAE2
		private void DisableOpCurrentRow(DynamicObject entryData)
		{
			MFGBillUtil.SetEnabled(base.View, "FOPForMulaType", entryData, false, "");
			MFGBillUtil.SetEnabled(base.View, "FOPForMulaVariable", entryData, false, "");
		}

		// Token: 0x0600039C RID: 924 RVA: 0x0002C912 File Offset: 0x0002AB12
		private void EnableOpCurrentRow(DynamicObject entryData)
		{
			MFGBillUtil.SetEnabled(base.View, "FOPForMulaType", entryData, true, "");
			MFGBillUtil.SetEnabled(base.View, "FOPForMulaVariable", entryData, true, "");
		}

		// Token: 0x0600039D RID: 925 RVA: 0x0002C944 File Offset: 0x0002AB44
		private void SetTextFieldProperty(DynamicObject entryData, string varTypeKey, string fieldKey, int rowIndex)
		{
			if (ObjectUtils.IsNullOrEmpty(entryData))
			{
				return;
			}
			int num = OtherExtend.ConvertTo<int>(entryData[varTypeKey], -1);
			if (num == 2)
			{
				base.View.GetFieldEditor(fieldKey, rowIndex).SetCustomPropertyValue("editable", true);
				base.View.GetFieldEditor(fieldKey, rowIndex).SetCustomPropertyValue("showEditButton", false);
				return;
			}
			if (num == 1)
			{
				base.View.GetFieldEditor(fieldKey, rowIndex).SetCustomPropertyValue("editable", false);
				base.View.GetFieldEditor(fieldKey, rowIndex).SetCustomPropertyValue("showEditButton", true);
				return;
			}
			if (num == 3)
			{
				base.View.GetFieldEditor(fieldKey, rowIndex).SetCustomPropertyValue("editable", false);
				base.View.GetFieldEditor(fieldKey, rowIndex).SetCustomPropertyValue("showEditButton", true);
				return;
			}
			if (num == -1)
			{
				base.View.GetFieldEditor(fieldKey, rowIndex).SetCustomPropertyValue("editable", false);
				base.View.GetFieldEditor(fieldKey, rowIndex).SetCustomPropertyValue("showEditButton", false);
			}
		}

		// Token: 0x0600039E RID: 926 RVA: 0x0002CBFC File Offset: 0x0002ADFC
		private void GetVariableFilter(int rowIndex, string modelvarType, string key)
		{
			long value = MFGBillUtil.GetValue<long>(base.View.Model, "FMODELCODE", -1, 0L, null);
			if (value != 0L)
			{
				DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
				dynamicFormShowParameter.OpenStyle.ShowType = 4;
				dynamicFormShowParameter.MultiSelect = false;
				dynamicFormShowParameter.FormId = "ENG_MODELVARIABLESHOW";
				dynamicFormShowParameter.CustomComplexParams.Add("ProductModelId", value);
				if (key == "FModelVariable" || key == "FOpVariable")
				{
					dynamicFormShowParameter.CustomComplexParams.Add("VarType", "ALL");
				}
				else
				{
					dynamicFormShowParameter.CustomComplexParams.Add("VarType", modelvarType);
				}
				base.View.ShowForm(dynamicFormShowParameter, delegate(FormResult result)
				{
					if (result != null && result.ReturnData != null)
					{
						DynamicObject dynamicObject = result.ReturnData as DynamicObject;
						string dynamicValue = DataEntityExtend.GetDynamicValue<string>(dynamicObject, "VarName", null);
						string dynamicValue2 = DataEntityExtend.GetDynamicValue<string>(dynamicObject, "VarType", null);
						string dynamicValue3 = DataEntityExtend.GetDynamicValue<string>(dynamicObject, "VarControl", null);
						long dynamicValue4 = DataEntityExtend.GetDynamicValue<long>(dynamicObject, "VariableId", 0L);
						string dynamicValue5 = DataEntityExtend.GetDynamicValue<string>(dynamicObject, "IsCalVariable", null);
						this.SetValueByKey(dynamicValue, dynamicValue2, dynamicValue3, dynamicValue4, dynamicValue5, key, rowIndex);
					}
				});
				return;
			}
			long value2 = MFGBillUtil.GetValue<long>(base.View.Model, "FUseOrgId", -1, 0L, null);
			ListShowParameter listShowParameter = new ListShowParameter();
			listShowParameter.FormId = "ENG_MODEVARIABLE";
			listShowParameter.PermissionItemId = "6e44119a58cb4a8e86f6c385e14a17ad";
			listShowParameter.ParentPageId = base.View.PageId;
			listShowParameter.IsShowApproved = true;
			listShowParameter.IsLookUp = true;
			listShowParameter.MultiSelect = false;
			listShowParameter.UseOrgId = value2;
			if (key == "FModelVariable" || key == "FOpVariable")
			{
				listShowParameter.ListFilterParameter.Filter = string.Format("FVARTYPE NOT IN('A','B')", new object[0]);
			}
			else if (modelvarType == "E" || modelvarType == "D")
			{
				listShowParameter.ListFilterParameter.Filter = string.Format("FVARTYPE='{0}'", modelvarType);
			}
			else if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(modelvarType))
			{
				listShowParameter.ListFilterParameter.Filter = string.Format("FVARTYPE<>'{0}'", "E");
			}
			base.View.ShowForm(listShowParameter, delegate(FormResult result)
			{
				if (result != null && result.ReturnData != null && (result.ReturnData as ListSelectedRowCollection).Count > 0)
				{
					ListSelectedRowCollection listSelectedRowCollection = (ListSelectedRowCollection)result.ReturnData;
					foreach (ListSelectedRow listSelectedRow in listSelectedRowCollection)
					{
						string varNumber = Convert.ToString(listSelectedRow.DataRow["FNUMBER"]);
						string varType = Convert.ToString(listSelectedRow.DataRow["FVARTYPE"]);
						string varControl = Convert.ToString(listSelectedRow.DataRow["FCONTROLTYPE"]);
						long varId = Convert.ToInt64(listSelectedRow.DataRow["FID"]);
						string isCalVar = "0";
						this.SetValueByKey(varNumber, varType, varControl, varId, isCalVar, key, rowIndex);
					}
				}
			});
		}

		// Token: 0x0600039F RID: 927 RVA: 0x0002CE18 File Offset: 0x0002B018
		private void SetValueByKey(string varNumber, string varType, string varControl, long varId, string isCalVar, string key, int rowIndex)
		{
			if (key == "FModelVariable")
			{
				base.View.Model.SetValue("FModelVariable", varNumber, rowIndex);
				base.View.Model.SetValue("FVarType", varType, rowIndex);
				base.View.Model.SetValue("FVariableID", varId, rowIndex);
				base.View.Model.SetValue("FIsCalVariable", isCalVar, rowIndex);
			}
			if (key == "FForMulaVBLeft")
			{
				base.View.Model.SetValue("FForMulaVBLeft", varNumber, rowIndex);
				base.View.Model.SetValue("FForMulaVarType", varType, rowIndex);
				base.View.Model.SetValue("FForMulaVarId", varId, rowIndex);
				base.View.Model.SetValue("FForMulaIsCalVar", isCalVar, rowIndex);
			}
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
			if (key == "FOpVariable")
			{
				base.View.Model.SetValue("FOpVariable", varNumber, rowIndex);
				base.View.Model.SetValue("FVarType", varType, rowIndex);
				base.View.Model.SetValue("FVariableControl", varControl, rowIndex);
				base.View.Model.SetValue("FVariableID", varId, rowIndex);
				base.View.Model.SetValue("FIsCalVariable", isCalVar, rowIndex);
			}
			if (key == "FOPForMulaVariable")
			{
				base.View.Model.SetValue("FOPForMulaVariable", varNumber, rowIndex);
				base.View.Model.SetValue("FOPForMulaVarType", varType, rowIndex);
				base.View.Model.SetValue("FOPFORMULAVARID", varId, rowIndex);
				base.View.Model.SetValue("FOPForMulaIsCalVar", isCalVar, rowIndex);
			}
			if (key == "FConLeftVar")
			{
				base.View.Model.SetValue("FConLeftVar", varNumber, rowIndex);
				base.View.Model.SetValue("FConLeftVarType", varType, rowIndex);
				base.View.Model.SetValue("FConLeftVarId", varId, rowIndex);
				base.View.Model.SetValue("FConLeftIsCalVar", isCalVar, rowIndex);
			}
			if (key == "FConRightVar")
			{
				base.View.Model.SetValue("FConRightVar", varNumber, rowIndex);
				base.View.Model.SetValue("FConRightVarType", varType, rowIndex);
				base.View.Model.SetValue("FConRightVarId", varId, rowIndex);
				base.View.Model.SetValue("FConRightIsCalVar", isCalVar, rowIndex);
			}
		}

		// Token: 0x060003A0 RID: 928 RVA: 0x0002D344 File Offset: 0x0002B544
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

		// Token: 0x060003A1 RID: 929 RVA: 0x0002D460 File Offset: 0x0002B660
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

		// Token: 0x060003A2 RID: 930 RVA: 0x0002D6DC File Offset: 0x0002B8DC
		private void DelInvalidRow()
		{
			DynamicObjectCollection dynamicValue = DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(base.View.Model.DataObject, "Entity", null);
			foreach (DynamicObject dynamicObject in dynamicValue)
			{
				int dynamicValue2 = DataEntityExtend.GetDynamicValue<int>(dynamicObject, "RuleType", 0);
				if (dynamicValue2 == 2)
				{
					DynamicObjectCollection dynamicValue3 = DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(dynamicObject, "OperatioEntity", null);
					IEnumerable<DynamicObject> source = from w in dynamicValue3
					where string.IsNullOrWhiteSpace(DataEntityExtend.GetDynamicValue<string>(w, "OperationLeftVB", null)) && string.IsNullOrWhiteSpace(DataEntityExtend.GetDynamicValue<string>(w, "OperationRightVB", null))
					select w;
					for (int i = source.Count<DynamicObject>() - 1; i >= 0; i--)
					{
						int num = DataEntityExtend.GetDynamicObjectItemValue<int>(source.ElementAt(i), "Seq", 0) - 1;
						this.Model.DeleteEntryRow("FOperatioEntity", num);
					}
				}
				if (dynamicValue2 == 3)
				{
					DynamicObjectCollection dynamicValue4 = DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(dynamicObject, "CONEntity", null);
					IEnumerable<DynamicObject> source2 = from w in dynamicValue4
					where string.IsNullOrWhiteSpace(DataEntityExtend.GetDynamicValue<string>(w, "ConLeftVar", null)) && string.IsNullOrWhiteSpace(DataEntityExtend.GetDynamicValue<string>(w, "ConRightVar", null))
					select w;
					for (int j = source2.Count<DynamicObject>() - 1; j >= 0; j--)
					{
						int num2 = DataEntityExtend.GetDynamicObjectItemValue<int>(source2.ElementAt(j), "Seq", 0) - 1;
						this.Model.DeleteEntryRow("FCONEntity", num2);
					}
				}
			}
		}

		// Token: 0x04000194 RID: 404
		private int curCursor;
	}
}
