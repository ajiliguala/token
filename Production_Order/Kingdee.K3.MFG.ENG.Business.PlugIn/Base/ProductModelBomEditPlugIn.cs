using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Log;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.MFG;
using Kingdee.K3.Core.MFG.ENG.ProductModel;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.Core.MFG.Utils;
using Kingdee.K3.MFG.ServiceHelper.ENG;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.Base
{
	// Token: 0x0200002F RID: 47
	[Description("产品模型BOM配置插件")]
	public class ProductModelBomEditPlugIn : BaseControlEdit
	{
		// Token: 0x17000016 RID: 22
		// (get) Token: 0x0600030E RID: 782 RVA: 0x000237BC File Offset: 0x000219BC
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

		// Token: 0x0600030F RID: 783 RVA: 0x0002380C File Offset: 0x00021A0C
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
			base.View.RuleContainer.AddPluginRule("FTreeEntity", 3, new Action<DynamicObject, object>(this.SetFlexFieldEnableByMaterial), new string[]
			{
				"FMATERIALIDCHILD"
			});
		}

		// Token: 0x06000310 RID: 784 RVA: 0x00023853 File Offset: 0x00021A53
		public override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
		}

		// Token: 0x06000311 RID: 785 RVA: 0x0002385C File Offset: 0x00021A5C
		public override void AfterBindData(EventArgs e)
		{
			base.AfterBindData(e);
			this.SyncBomConfigToParentWindow();
			if (base.View.ParentFormView != null && DataEntityExtend.GetDynamicValue<string>(base.View.ParentFormView.Model.DataObject, "DocumentStatus", null).Equals("C"))
			{
				this.SetTreeEntryEnable(false);
			}
		}

		// Token: 0x06000312 RID: 786 RVA: 0x000238B6 File Offset: 0x00021AB6
		public override void DataChanged(DataChangedEventArgs e)
		{
			base.DataChanged(e);
		}

		// Token: 0x06000313 RID: 787 RVA: 0x000238C0 File Offset: 0x00021AC0
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			base.BeforeF7Select(e);
			string fieldKey;
			if ((fieldKey = e.FieldKey) != null)
			{
				if (!(fieldKey == "FMATERIALIDCHILD"))
				{
					return;
				}
				string childMaterialFilterString = this.GetChildMaterialFilterString();
				if (!string.IsNullOrWhiteSpace(e.ListFilterParameter.Filter))
				{
					e.ListFilterParameter.Filter = e.ListFilterParameter.Filter + " AND " + childMaterialFilterString;
					return;
				}
				e.ListFilterParameter.Filter = childMaterialFilterString;
			}
		}

		// Token: 0x06000314 RID: 788 RVA: 0x00023934 File Offset: 0x00021B34
		public override void BeforeSetItemValueByNumber(BeforeSetItemValueByNumberArgs e)
		{
			base.BeforeSetItemValueByNumber(e);
			string baseDataFieldKey;
			if ((baseDataFieldKey = e.BaseDataFieldKey) != null)
			{
				if (!(baseDataFieldKey == "FMATERIALIDCHILD"))
				{
					return;
				}
				string childMaterialFilterString = this.GetChildMaterialFilterString();
				if (!string.IsNullOrWhiteSpace(e.Filter))
				{
					e.Filter = e.Filter + " AND " + childMaterialFilterString;
					return;
				}
				e.Filter = childMaterialFilterString;
			}
		}

		// Token: 0x06000315 RID: 789 RVA: 0x00023994 File Offset: 0x00021B94
		public override void AfterF7Select(AfterF7SelectEventArgs e)
		{
			base.AfterF7Select(e);
			int entryCurrentRowIndex = base.View.Model.GetEntryCurrentRowIndex("FTreeEntity");
			int entryRowCount = base.View.Model.GetEntryRowCount("FTreeEntity");
			int num = entryRowCount - entryCurrentRowIndex;
			if (num < e.SelectRows.Count)
			{
				int num2 = e.SelectRows.Count - num;
				for (int i = 1; i <= num2; i++)
				{
					e.SelectRows.RemoveAt(e.SelectRows.Count - 1);
				}
			}
		}

		// Token: 0x06000316 RID: 790 RVA: 0x00023A1C File Offset: 0x00021C1C
		public override void AfterCreateNewEntryRow(CreateNewEntryEventArgs e)
		{
			base.AfterCreateNewEntryRow(e);
			base.View.Model.SetValue("FRowId", Guid.NewGuid().ToString(), e.Row);
			base.View.Model.SetValue("FParentRowId", " ", e.Row);
			this.Model.SetValue("FRowExpandType", 0, e.Row);
		}

		// Token: 0x06000317 RID: 791 RVA: 0x00023A9C File Offset: 0x00021C9C
		public override void CustomEvents(CustomEventsArgs e)
		{
			base.CustomEvents(e);
			if (e.EventName.Equals("Save"))
			{
				if (!string.IsNullOrWhiteSpace(e.EventArgs))
				{
					base.View.Model.DataObject["MDLID_Id"] = e.EventArgs;
				}
				this.RemoveNullRow();
				IOperationResult operationResult = this.SavePrdModelBomData(base.View.Model.DataObject);
				if (!operationResult.IsSuccess)
				{
					FormUtils.ShowOperationResult(base.View, operationResult, null);
				}
				this.SyncBomConfigToParentWindow();
				return;
			}
			if (e.EventName.Equals(6.ToString()) || e.EventName.Equals("Audit"))
			{
				this.SetTreeEntryEnable(false);
				return;
			}
			if (e.EventName.Equals("UnAudit"))
			{
				this.SetTreeEntryEnable(true);
				return;
			}
			if (e.EventName.Equals("Copy"))
			{
				DynamicObject dynamicObject = OrmUtils.Clone(base.View.Model.DataObject, false, true) as DynamicObject;
				dynamicObject["msterID"] = 0;
				base.View.Model.CreateNewData();
				base.View.Model.DataObject = dynamicObject;
				this.SetTreeEntryEnable(true);
				base.View.UpdateView("FTreeEntity");
				return;
			}
			if (e.EventName.Equals("New"))
			{
				base.View.Model.CreateNewData();
				this.SetTreeEntryEnable(true);
				base.View.UpdateView("FTreeEntity");
				return;
			}
			if (StringUtils.EqualsIgnoreCase(e.EventName, "MaterialChange"))
			{
				base.View.Model.SetValue("FMATERIALID", e.EventArgs);
				return;
			}
			if (StringUtils.EqualsIgnoreCase(e.EventName, "BillReloadData"))
			{
				bool treeEntryEnable = true;
				long prdModelBomId = ProductModelServiceHelper.GetPrdModelBomId(base.View.Context, Convert.ToInt64(e.EventArgs));
				base.View.Model.CreateNewData();
				if (prdModelBomId > 0L)
				{
					DynamicObject dataObject = BusinessDataServiceHelper.LoadSingle(base.View.Context, prdModelBomId, base.View.BusinessInfo.GetDynamicObjectType(), null);
					base.View.Model.DataObject = dataObject;
				}
				if (base.View.ParentFormView != null)
				{
					string dynamicValue = DataEntityExtend.GetDynamicValue<string>(base.View.ParentFormView.Model.DataObject, "DocumentStatus", null);
					if (StringUtils.EqualsIgnoreCase("C", dynamicValue) || StringUtils.EqualsIgnoreCase("B", dynamicValue))
					{
						treeEntryEnable = false;
					}
				}
				this.SetTreeEntryEnable(treeEntryEnable);
				this.SyncBomConfigToParentWindow();
				base.View.UpdateView("FTreeEntity");
				return;
			}
			if (StringUtils.EqualsIgnoreCase(e.EventName, "Delete"))
			{
				this.Model.DeleteEntryRow("FTreeEntity", OtherExtend.ConvertTo<int>(e.EventArgs, 0));
			}
		}

		// Token: 0x06000318 RID: 792 RVA: 0x00023DB0 File Offset: 0x00021FB0
		private void RemoveNullRow()
		{
			DynamicObjectCollection treeEntityDatas = this.Model.GetEntityDataObject(base.View.BusinessInfo.GetEntity("FTreeEntity"));
			List<DynamicObject> list = (from x in treeEntityDatas
			where ObjectUtils.IsNullOrEmptyOrWhiteSpace(DataEntityExtend.GetDynamicValue<string>(x, "PMRowId", null))
			select x).ToList<DynamicObject>();
			if (!ListUtils.IsEmpty<DynamicObject>(list))
			{
				list.ForEach(delegate(DynamicObject x)
				{
					this.Model.DeleteEntryRow("FTreeEntity", treeEntityDatas.IndexOf(x));
				});
				base.View.UpdateView("FTreeEntity");
				base.View.UpdateView("FBomVarEntity");
			}
		}

		// Token: 0x06000319 RID: 793 RVA: 0x00023E80 File Offset: 0x00022080
		public override void BeforeUpdateValue(BeforeUpdateValueEventArgs e)
		{
			base.BeforeUpdateValue(e);
			string a;
			if ((a = e.Key.ToUpperInvariant()) != null)
			{
				if (!(a == "FMATERIALTYPE"))
				{
					if (a == "FSCRAPRATE")
					{
						if (OtherExtend.ConvertTo<decimal>(e.Value, 0m) == 100m)
						{
							e.Cancel = true;
						}
					}
				}
				else if (!this.IsCanBCTypeUpdate(e))
				{
					e.Cancel = true;
				}
			}
			Field field = base.View.BusinessInfo.GetField(e.Key);
			if (!e.Cancel && StringUtils.EqualsIgnoreCase(field.EntityKey, "FTreeEntity"))
			{
				if (e.Key.Equals("FMATERIALIDCHILD"))
				{
					DynamicObjectCollection dynamicObjectCollection = base.View.Model.DataObject["TreeEntity"] as DynamicObjectCollection;
					DynamicObject dynamicObject = dynamicObjectCollection[e.Row];
					DynamicObjectCollection dynamicObjectCollection2 = dynamicObject["BomVarEntity"] as DynamicObjectCollection;
					if (!ListUtils.IsEmpty<DynamicObject>(dynamicObjectCollection2))
					{
						dynamicObjectCollection2.Clear();
						base.View.UpdateView("FBomVarEntity");
						return;
					}
				}
				else
				{
					string fieldKey = e.Key;
					if (StringUtils.EqualsIgnoreCase(e.Key, "FAuxPropId"))
					{
						fieldKey = base.View.Model.GetEntryCurrentFieldKey("FTreeEntity");
					}
					DynamicObjectCollection dynamicObjectCollection3 = base.View.Model.DataObject["TreeEntity"] as DynamicObjectCollection;
					DynamicObject dynamicObject2 = dynamicObjectCollection3[e.Row];
					DynamicObjectCollection dynamicObjectCollection4 = dynamicObject2["BomVarEntity"] as DynamicObjectCollection;
					IEnumerable<DynamicObject> enumerable = from p in dynamicObjectCollection4
					where StringUtils.EqualsIgnoreCase(DataEntityExtend.GetDynamicValue<string>(p, "FieldKey", null), fieldKey)
					select p;
					if (!ListUtils.IsEmpty<DynamicObject>(enumerable))
					{
						dynamicObjectCollection4.Remove(enumerable.FirstOrDefault<DynamicObject>());
						base.View.UpdateView("FBomVarEntity");
					}
				}
			}
		}

		// Token: 0x0600031A RID: 794 RVA: 0x00024280 File Offset: 0x00022480
		public override void EntryBarItemClick(BarItemClickEventArgs e)
		{
			ProductModelBomEditPlugIn.<>c__DisplayClassf CS$<>8__locals1 = new ProductModelBomEditPlugIn.<>c__DisplayClassf();
			CS$<>8__locals1.<>4__this = this;
			base.EntryBarItemClick(e);
			CS$<>8__locals1.rowIndex = base.View.Model.GetEntryCurrentRowIndex("FTreeEntity");
			string barItemKey;
			if ((barItemKey = e.BarItemKey) != null)
			{
				if (!(barItemKey == "tbVariableSet"))
				{
					if (barItemKey == "tbMoveUp")
					{
						this.DoUp(e.ParentKey, CS$<>8__locals1.rowIndex);
						return;
					}
					if (barItemKey == "tbMoveDown")
					{
						this.DoDown(e.ParentKey, CS$<>8__locals1.rowIndex);
						return;
					}
					if (!(barItemKey == "tbMoveTo"))
					{
						return;
					}
					this.MoveTo(e, CS$<>8__locals1.rowIndex);
				}
				else
				{
					if (CS$<>8__locals1.rowIndex < 0)
					{
						return;
					}
					string fieldKey = base.View.Model.GetEntryCurrentFieldKey("FTreeEntity");
					Entity entity = base.View.BusinessInfo.GetEntity("FTreeEntity");
					Field objField = MFGFormBusinessUtil.GetFlexField(base.View.BillBusinessInfo, fieldKey);
					if (objField == null)
					{
						return;
					}
					DynamicObject entityDataObject = this.Model.GetEntityDataObject(entity, CS$<>8__locals1.rowIndex);
					if (fieldKey.StartsWith("$$"))
					{
						DynamicObject dynamicValue = DataEntityExtend.GetDynamicValue<DynamicObject>(entityDataObject, "MATERIALIDCHILD", null);
						DynamicObjectCollection dynamicValue2 = DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(dynamicValue, "MaterialAuxPty", null);
						if (!ListUtils.IsEmpty<DynamicObject>(dynamicValue2))
						{
							List<string> list = (from x in dynamicValue2
							where DataEntityExtend.GetDynamicValue<bool>(x, "IsEnable1", false)
							select string.Format("FF{0}", DataEntityExtend.GetDynamicValue<long>(x, "AuxPropertyId_Id", 0L))).ToList<string>();
							if (!list.Contains(objField.Key))
							{
								base.View.ShowErrMessage("", string.Format(ResManager.LoadKDString("当前的子项物料未启用选中字段对应的辅助属性维度，不能设置变量", "015072000024823", 7, new object[0]), new object[0]), 0);
								return;
							}
						}
					}
					else if (objField is BasePropertyField || !base.View.GetControl(fieldKey).Enabled || !base.View.StyleManager.GetEnabled(objField, entityDataObject))
					{
						base.View.ShowErrMessage("", string.Format(ResManager.LoadKDString("当前选中的字段为锁定状态，不能设置变量.", "015072000024824", 7, new object[0]), new object[0]), 0);
						return;
					}
					this.ShowVariableForm(objField, delegate(FormResult result)
					{
						if (result != null && result.ReturnData != null)
						{
							DynamicObject dynamicObject = result.ReturnData as DynamicObject;
							string dynamicValue3 = DataEntityExtend.GetDynamicValue<string>(dynamicObject, "VarNumber", null);
							if (string.IsNullOrWhiteSpace(dynamicValue3))
							{
								return;
							}
							DynamicObjectCollection dynamicObjectCollection = CS$<>8__locals1.<>4__this.View.Model.DataObject["TreeEntity"] as DynamicObjectCollection;
							DynamicObject dynamicObject2 = dynamicObjectCollection[CS$<>8__locals1.rowIndex];
							object varDefaultValue = CS$<>8__locals1.<>4__this.GetVarDefaultValue(objField);
							if (!StringUtils.EqualsIgnoreCase(objField.Key, fieldKey))
							{
								CS$<>8__locals1.<>4__this.SetFlexFieldValue(objField, CS$<>8__locals1.rowIndex, varDefaultValue);
							}
							else
							{
								CS$<>8__locals1.<>4__this.View.Model.SetValue(objField.Key, varDefaultValue, CS$<>8__locals1.rowIndex);
							}
							DynamicObjectCollection dynamicObjectCollection2 = dynamicObject2["BomVarEntity"] as DynamicObjectCollection;
							IEnumerable<DynamicObject> enumerable = from p in dynamicObjectCollection2
							where StringUtils.EqualsIgnoreCase(DataEntityExtend.GetDynamicValue<string>(p, "FieldKey", null), fieldKey)
							select p;
							if (!ListUtils.IsEmpty<DynamicObject>(enumerable))
							{
								DynamicObject dynamicObject3 = enumerable.FirstOrDefault<DynamicObject>();
								dynamicObject3["variableKey"] = dynamicValue3;
							}
							else
							{
								DynamicObject dynamicObject4 = new DynamicObject(dynamicObjectCollection2.DynamicCollectionItemPropertyType);
								dynamicObject4["FieldKey"] = fieldKey;
								dynamicObject4["FieldName"] = objField.Name.ToString();
								dynamicObject4["variableKey"] = dynamicValue3;
								dynamicObjectCollection2.Add(dynamicObject4);
							}
							CS$<>8__locals1.<>4__this.View.UpdateView("FBomVarEntity");
						}
					});
					return;
				}
			}
		}

		// Token: 0x0600031B RID: 795 RVA: 0x0002452F File Offset: 0x0002272F
		public override void AfterDoOperation(AfterDoOperationEventArgs e)
		{
			base.AfterDoOperation(e);
		}

		// Token: 0x0600031C RID: 796 RVA: 0x00024538 File Offset: 0x00022738
		private void DoUp(string entityKey, int rowIndex)
		{
			EntryEntity entryEntity = base.View.BusinessInfo.GetEntryEntity(entityKey);
			DynamicObjectCollection entityDataObject = base.View.Model.GetEntityDataObject(entryEntity);
			if (rowIndex < 0 || rowIndex >= entityDataObject.Count)
			{
				return;
			}
			if (rowIndex == 0)
			{
				return;
			}
			DynamicObject dynamicObject = entityDataObject[rowIndex];
			base.View.Model.DeleteEntryRow(entityKey, rowIndex);
			base.View.Model.CreateNewEntryRow(entryEntity, rowIndex - 1, dynamicObject);
			if (entryEntity.SeqDynamicProperty != null)
			{
				entryEntity.SeqDynamicProperty.SetValue(entityDataObject[rowIndex - 1], rowIndex);
				entryEntity.SeqDynamicProperty.SetValue(entityDataObject[rowIndex], rowIndex + 1);
			}
			base.View.UpdateView("FSEQ", rowIndex);
			base.View.SetEntityFocusRow(entityKey, rowIndex - 1);
		}

		// Token: 0x0600031D RID: 797 RVA: 0x00024608 File Offset: 0x00022808
		private void DoDown(string entityKey, int rowIndex)
		{
			EntryEntity entryEntity = base.View.BusinessInfo.GetEntryEntity(entityKey);
			DynamicObjectCollection entityDataObject = base.View.Model.GetEntityDataObject(entryEntity);
			if (rowIndex < 0 || rowIndex >= entityDataObject.Count)
			{
				return;
			}
			if (rowIndex == entityDataObject.Count - 1)
			{
				return;
			}
			DynamicObject dynamicObject = entityDataObject[rowIndex];
			base.View.Model.DeleteEntryRow(entityKey, rowIndex);
			base.View.Model.CreateNewEntryRow(entryEntity, rowIndex + 1, dynamicObject);
			if (entryEntity.SeqDynamicProperty != null)
			{
				entryEntity.SeqDynamicProperty.SetValue(entityDataObject[rowIndex], rowIndex + 1);
				entryEntity.SeqDynamicProperty.SetValue(entityDataObject[rowIndex + 1], rowIndex + 2);
			}
			base.View.UpdateView("FSEQ", rowIndex);
			base.View.SetEntityFocusRow(entityKey, rowIndex + 1);
		}

		// Token: 0x0600031E RID: 798 RVA: 0x0002470C File Offset: 0x0002290C
		private string GetChildMaterialFilterString()
		{
			string text = "  1=1 ";
			if (base.View.ParentFormView != null)
			{
				long dynamicValue = DataEntityExtend.GetDynamicValue<long>(base.View.ParentFormView.Model.DataObject, "FMaterialId_Id", 0L);
				if (dynamicValue > 0L)
				{
					text += string.Format(" AND FMaterialId <> {0} ", dynamicValue);
					DynamicObject dynamicValue2 = DataEntityExtend.GetDynamicValue<DynamicObject>(base.View.ParentFormView.Model.DataObject, "FMaterialId", null);
					long parentMProperty = 0L;
					parentMProperty = Convert.ToInt64((dynamicValue2["MaterialBase"] as DynamicObjectCollection).FirstOrDefault<DynamicObject>()["ErpClsID"]);
					long value = MFGBillUtil.GetValue<long>(base.View.Model, "FBOMCATEGORY", -1, 0L, null);
					List<int> list = new List<int>();
					if ((int)value == 1)
					{
						list = new List<KeyValuePair<int, List<int>>>
						{
							new KeyValuePair<int, List<int>>(2, new List<int>
							{
								2,
								3,
								5,
								1,
								10
							}),
							new KeyValuePair<int, List<int>>(3, new List<int>
							{
								2,
								3,
								5,
								1,
								10
							}),
							new KeyValuePair<int, List<int>>(5, new List<int>
							{
								2,
								3,
								5,
								1,
								10
							}),
							new KeyValuePair<int, List<int>>(1, new List<int>
							{
								1
							}),
							new KeyValuePair<int, List<int>>(9, new List<int>
							{
								2,
								3,
								5,
								1,
								9,
								10
							}),
							new KeyValuePair<int, List<int>>(4, new List<int>
							{
								2,
								3,
								5,
								1,
								9,
								10
							})
						}.FindLast((KeyValuePair<int, List<int>> w) => (long)w.Key == parentMProperty).Value;
					}
					else
					{
						list = new List<KeyValuePair<int, List<int>>>
						{
							new KeyValuePair<int, List<int>>(9, new List<int>
							{
								2,
								3,
								5,
								1,
								9,
								4,
								10
							})
						}.FindLast((KeyValuePair<int, List<int>> w) => (long)w.Key == parentMProperty).Value;
					}
					if (ListUtils.IsEmpty<int>(list))
					{
						return text;
					}
					text += string.Format(" and (FErpClsID IN('{0}'))", string.Join<int>("','", list));
				}
			}
			return text;
		}

		// Token: 0x0600031F RID: 799 RVA: 0x00024A40 File Offset: 0x00022C40
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

		// Token: 0x06000320 RID: 800 RVA: 0x00024ABC File Offset: 0x00022CBC
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
			DynamicObjectCollection dynamicObjectCollection = base.View.ParentFormView.Model.DataObject["Entity"] as DynamicObjectCollection;
			DynamicObjectCollection dynamicObjectCollection2 = base.View.ParentFormView.Model.DataObject["CalEntity"] as DynamicObjectCollection;
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

		// Token: 0x06000321 RID: 801 RVA: 0x00024DE8 File Offset: 0x00022FE8
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

		// Token: 0x06000322 RID: 802 RVA: 0x00024E45 File Offset: 0x00023045
		protected void SetTreeEntryEnable(bool isEnable)
		{
			this.Model.SetValue("FLocked", !isEnable);
		}

		// Token: 0x06000323 RID: 803 RVA: 0x00024E60 File Offset: 0x00023060
		private bool IsCanBCTypeUpdate(BeforeUpdateValueEventArgs e)
		{
			bool result = true;
			if (base.Context.ServiceType == 1)
			{
				return result;
			}
			if (!string.IsNullOrWhiteSpace(MFGBillUtil.GetValue<string>(base.View.Model, "FReplacePolicy", e.Row, null, null)))
			{
				return result;
			}
			int value = MFGBillUtil.GetValue<int>(base.View.Model, "FMaterialType", e.Row, 0, "FEntity");
			int num = 0;
			int.TryParse(Convert.ToString(e.Value), out num);
			if ((value == 1 || value == 2) && num == 3)
			{
				base.View.ShowErrMessage(ResManager.LoadKDString("标准件或返还件不允许修改为替代件！", "015072000002176", 7, new object[0]), "", 0);
				base.View.Model.SetValue("FMATERIALTYPE", value, e.Row);
				result = false;
			}
			return result;
		}

		// Token: 0x06000324 RID: 804 RVA: 0x00024F34 File Offset: 0x00023134
		private void SetFlexFieldValue(Field flexField, int rowIndex, object value)
		{
			DynamicObjectCollection dynamicObjectCollection = base.View.Model.DataObject["TreeEntity"] as DynamicObjectCollection;
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

		// Token: 0x06000325 RID: 805 RVA: 0x00025004 File Offset: 0x00023204
		private void SetFlexFieldEnableByMaterial(DynamicObject dyObj, dynamic obj)
		{
			DynamicObject dynamicObject = dyObj["MATERIALIDCHILD"] as DynamicObject;
			RelatedFlexGroupFieldAppearance relatedFlexGroupFieldAppearance = base.View.LayoutInfo.GetFieldAppearance("FAuxPropId") as RelatedFlexGroupFieldAppearance;
			if (dynamicObject == null)
			{
				base.View.StyleManager.SetEnabled(relatedFlexGroupFieldAppearance, dyObj, "LockAuxByMaterial", true);
				return;
			}
			DynamicObjectCollection source = (DynamicObjectCollection)dynamicObject["MaterialAuxPty"];
			List<DynamicObject> list = (from p in source
			where DataEntityExtend.GetDynamicValue<long>(p, "Id", 0L) > 0L && DataEntityExtend.GetDynamicValue<bool>(p, "IsEnable1", false)
			select p into d
			select DataEntityExtend.GetDynamicValue<DynamicObject>(d, "AuxPropertyId", null)).ToList<DynamicObject>();
			if (ListUtils.IsEmpty<DynamicObject>(list))
			{
				base.View.StyleManager.SetEnabled(relatedFlexGroupFieldAppearance, dyObj, "LockAuxByMaterial", false);
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
						base.View.StyleManager.SetEnabled(text, dyObj, "LockAuxByMaterial", true);
					}
					else
					{
						base.View.StyleManager.SetEnabled(text, dyObj, "LockAuxByMaterial", false);
					}
				}
			}
		}

		// Token: 0x06000326 RID: 806 RVA: 0x000251D8 File Offset: 0x000233D8
		private IOperationResult SavePrdModelBomData(DynamicObject bomData)
		{
			OperateOption operateOption = OperateOption.Create();
			OperateOptionUtils.SetValidateFlag(operateOption, false);
			return BusinessDataServiceHelper.Save(base.View.Context, base.View.BusinessInfo, bomData, operateOption, "");
		}

		// Token: 0x06000327 RID: 807 RVA: 0x0002524C File Offset: 0x0002344C
		private void SyncBomConfigToParentWindow()
		{
			IDynamicFormView parentFormView = base.View.ParentFormView;
			if (parentFormView == null)
			{
				return;
			}
			DynamicObjectCollection entityDataObject = parentFormView.Model.GetEntityDataObject(parentFormView.BusinessInfo.GetEntity("FTreeEntryConfig"));
			if (ListUtils.IsEmpty<DynamicObject>(entityDataObject))
			{
				return;
			}
			DynamicObjectCollection entityDataObject2 = this.Model.GetEntityDataObject(base.View.BusinessInfo.GetEntity("FTreeEntity"));
			Dictionary<string, DynamicObject> dictionary = (from x in entityDataObject
			where DataEntityExtend.GetDynamicValue<string>(x, "NodeType", null) == "A"
			group x by DataEntityExtend.GetDynamicValue<string>(x, "RowId", null)).ToDictionary((IGrouping<string, DynamicObject> x) => x.Key, (IGrouping<string, DynamicObject> v) => v.FirstOrDefault<DynamicObject>());
			if (ListUtils.IsEmpty<KeyValuePair<string, DynamicObject>>(dictionary))
			{
				return;
			}
			base.View.BusinessInfo.GetEntity("FBomVarEntity");
			List<int> list = new List<int>();
			foreach (DynamicObject dynamicObject in entityDataObject2)
			{
				string dynamicValue = DataEntityExtend.GetDynamicValue<string>(dynamicObject, "PMRowId", null);
				if (ObjectUtils.IsNullOrEmptyOrWhiteSpace(dynamicValue))
				{
					list.Add(DataEntityExtend.GetDynamicValue<int>(dynamicObject, "Seq", 0));
				}
				else
				{
					DynamicObject item = null;
					if (dictionary.TryGetValue(dynamicValue, out item))
					{
						int num = entityDataObject.IndexOf(item);
						DynamicObject dynamicObject2 = dynamicObject["MaterialIdChild"] as DynamicObject;
						if (dynamicObject2 != null)
						{
							parentFormView.Model.SetValue("FMtrlNumber", DataEntityExtend.GetDynamicValue<string>(dynamicObject2, "Number", null), num);
							parentFormView.Model.SetValue("FMtrlName", dynamicObject2["Name"], num);
						}
						parentFormView.Model.SetValue("FMtrlNumerator", DataEntityExtend.GetDynamicValue<decimal>(dynamicObject, "Numerator", 0m).ToString().TrimEnd(new char[]
						{
							'0'
						}).TrimEnd(new char[]
						{
							'.'
						}), num);
						parentFormView.Model.SetValue("FMtrlDenominator", DataEntityExtend.GetDynamicValue<decimal>(dynamicObject, "Denominator", 0m).ToString().TrimEnd(new char[]
						{
							'0'
						}).TrimEnd(new char[]
						{
							'.'
						}), num);
						DynamicObjectCollection dynamicObjectCollection = dynamicObject["BomVarEntity"] as DynamicObjectCollection;
						if (!ListUtils.IsEmpty<DynamicObject>(dynamicObjectCollection))
						{
							foreach (DynamicObject dynamicObject3 in dynamicObjectCollection)
							{
								string dynamicValue2 = DataEntityExtend.GetDynamicValue<string>(dynamicObject3, "FieldKey", null);
								string dynamicValue3 = DataEntityExtend.GetDynamicValue<string>(dynamicObject3, "variableKey", null);
								string a;
								if ((a = dynamicValue2) != null)
								{
									if (!(a == "FMATERIALIDCHILD"))
									{
										if (!(a == "FNUMERATOR"))
										{
											if (a == "FDENOMINATOR")
											{
												parentFormView.Model.SetValue("FMtrlDenominator", dynamicValue3, num);
											}
										}
										else
										{
											parentFormView.Model.SetValue("FMtrlNumerator", dynamicValue3, num);
										}
									}
									else
									{
										parentFormView.Model.SetValue("FMtrlNumber", dynamicValue3, num);
									}
								}
							}
						}
					}
				}
			}
			base.View.SendDynamicFormAction(parentFormView);
		}

		// Token: 0x06000328 RID: 808 RVA: 0x000257C8 File Offset: 0x000239C8
		private void MoveTo(BarItemClickEventArgs e, int rowIndex)
		{
			DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
			dynamicFormShowParameter.FormId = "ENG_MOVETO";
			dynamicFormShowParameter.ParentPageId = base.View.PageId;
			dynamicFormShowParameter.PageId = Guid.NewGuid().ToString();
			dynamicFormShowParameter.CustomParams.Add("currentIndex", rowIndex.ToString());
			int num = base.View.Model.GetEntryRowCount(e.ParentKey) - 1;
			dynamicFormShowParameter.CustomParams.Add("maxIndex", num.ToString());
			base.View.ShowForm(dynamicFormShowParameter, delegate(FormResult result)
			{
				if (this.View == null)
				{
					Logger.Error("MFG_ProductModelBom", ResManager.LoadKDString("产品模型移动至操作回调后View为null", "015072000016545", 7, new object[0]), null);
					return;
				}
				object returnData = result.ReturnData;
				if (StringUtils.EqualsIgnoreCase(returnData.ToString(), "FCANCEL"))
				{
					e.Cancel = true;
					return;
				}
				int num2 = -1;
				if (int.TryParse(returnData.ToString(), out num2))
				{
					Entity entryEntity = this.View.BusinessInfo.GetEntryEntity(e.ParentKey);
					DynamicObjectCollection entityDataObject = this.View.Model.GetEntityDataObject(entryEntity);
					DynamicObject item = entityDataObject[rowIndex];
					entityDataObject.RemoveAt(rowIndex);
					entityDataObject.Insert(num2, item);
					if (rowIndex < num2)
					{
						for (int i = rowIndex; i <= num2; i++)
						{
							DataEntityExtend.SetDynamicObjectItemValue(entityDataObject[i], "seq", i + 1);
							this.View.UpdateView(e.ParentKey, i);
						}
					}
					else
					{
						for (int j = num2; j <= rowIndex; j++)
						{
							DataEntityExtend.SetDynamicObjectItemValue(entityDataObject[j], "seq", j + 1);
							this.View.UpdateView(e.ParentKey, j);
						}
					}
					this.View.UpdateView("FBomVarEntity");
					return;
				}
				this.View.ShowErrMessage(string.Format(ResManager.LoadKDString("输入的值：{0}格式不正确，请输入整数类型的值！", "015072000016544", 7, new object[0]), returnData), "", 0);
			});
		}

		// Token: 0x0400016D RID: 365
		private Entity variableShowEntity;
	}
}
