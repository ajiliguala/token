using System;
using System.Collections.Generic;
using System.Linq;
using Kingdee.BOS;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Log;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.MFG.EntityHelper;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.ECNControler
{
	// Token: 0x020000C1 RID: 193
	public class CustomBatchFillControler : AbstractItemControler
	{
		// Token: 0x06000E39 RID: 3641 RVA: 0x000A4B18 File Offset: 0x000A2D18
		public override void DoOperation()
		{
			base.DoOperation();
			EntryEntity entryEntity = (EntryEntity)base.View.BusinessInfo.GetEntity("FTreeEntity");
			DynamicObjectCollection entityDataObject = base.View.Model.GetEntityDataObject(entryEntity);
			if (ListUtils.IsEmpty<DynamicObject>(entityDataObject))
			{
				return;
			}
			int focusRowIndex = base.View.GetControl<EntryGrid>("FTreeEntity").GetFocusRowIndex();
			if (focusRowIndex < 0)
			{
				return;
			}
			DynamicObject entityDataObject2 = base.View.Model.GetEntityDataObject(entryEntity, focusRowIndex);
			string currentRowChangeLabel = DataEntityExtend.GetDynamicValue<string>(entityDataObject2, "ChangeLabel", null);
			if (currentRowChangeLabel != "1" && currentRowChangeLabel != "2")
			{
				base.View.ShowErrMessage(ResManager.LoadKDString("仅行标识为“新增”或“变更后”的分录支持批量填充！", "0151515153499000024825", 7, new object[0]), "", 0);
				return;
			}
			if (focusRowIndex + 1 >= entityDataObject.Count<DynamicObject>())
			{
				return;
			}
			IEnumerable<DynamicObject> enumerable = entityDataObject.Skip(focusRowIndex + 1);
			if (ListUtils.IsEmpty<DynamicObject>(enumerable))
			{
				return;
			}
			List<DynamicObject> list = (from i in enumerable
			where StringUtils.EqualsIgnoreCase(DataEntityExtend.GetDynamicValue<string>(i, "ChangeLabel", null), currentRowChangeLabel)
			select i).ToList<DynamicObject>();
			if (ListUtils.IsEmpty<DynamicObject>(list))
			{
				return;
			}
			string columnFKey = "";
			string text = "";
			bool bBatchControlFlex = false;
			columnFKey = base.View.GetControl<EntryGrid>("FTreeEntity").GetFocusField();
			CustomBatchFillControler.RelatedFlexGroupArgs relatedFlexGroupArgs = null;
			if (base.View.Model.IsFlexField(columnFKey))
			{
				bBatchControlFlex = KDConfigurationServiceHelper.GetIsEnableBatchControlFlex(base.View.Context);
				relatedFlexGroupArgs = new CustomBatchFillControler.RelatedFlexGroupArgs(columnFKey, base.View.BillBusinessInfo);
			}
			if (base.View.BusinessInfo.GetField(columnFKey) != null)
			{
				text = base.View.BusinessInfo.GetField(columnFKey).PropertyName;
			}
			if (relatedFlexGroupArgs != null && relatedFlexGroupArgs.FlexField != null)
			{
				columnFKey = relatedFlexGroupArgs.FlexFieldKey;
				text = relatedFlexGroupArgs.FlexField.PropertyName;
			}
			if (this.notBatchFillField.Contains(columnFKey))
			{
				base.View.ShowErrMessage(ResManager.LoadKDString("当前字段不支持批量填充！", "0151515153499000024826", 7, new object[0]), "", 0);
				return;
			}
			if (text != "" && base.View.GetControl(columnFKey).Enabled)
			{
				bool flag = false;
				if (base.View.Model.ParameterData != null && DynamicObjectUtils.Contains(base.View.Model.ParameterData, "FBatchFillIsNull"))
				{
					flag = Convert.ToBoolean(base.View.Model.ParameterData["FBatchFillIsNull"]);
				}
				object obj = null;
				Field field = base.View.BusinessInfo.GetField(columnFKey);
				if ((field.FunControl & Field.FUNCONTROL_BATCHFILL) != Field.FUNCONTROL_BATCHFILL)
				{
					return;
				}
				if (!(field is BasePropertyField))
				{
					if (field is MulBaseDataField || field is MulAssistantField)
					{
						DynamicObjectCollection source = entityDataObject[focusRowIndex][text] as DynamicObjectCollection;
						obj = (from i in source
						select (i[columnFKey] as DynamicObject)["Id"]).ToArray<object>();
					}
					else if (field is BaseDataField)
					{
						if (field is LotField || field is BaseDataTextField)
						{
							obj = base.View.Model.GetValue(columnFKey, focusRowIndex);
						}
						else if (entityDataObject[focusRowIndex][text] != null)
						{
							obj = (entityDataObject[focusRowIndex][text] as DynamicObject)["Id"];
						}
					}
					else
					{
						obj = entityDataObject[focusRowIndex][text];
					}
				}
				Field keyField = entryEntity.GetKeyField();
				object obj2 = entityDataObject2[text];
				int j = 0;
				while (j < list.Count<DynamicObject>())
				{
					int num = entityDataObject.IndexOf(list[j]);
					if (keyField == null || StringUtils.EqualsIgnoreCase(columnFKey, keyField.Key) || !ObjectUtils.IsNullOrEmptyOrWhiteSpace(base.View.Model.GetValue(keyField, num)))
					{
						goto IL_45F;
					}
					bool flag2 = false;
					if (base.View is IBillView && (base.View as IBillView).Model.GlobalParameter != null && (base.View as IBillView).Model.GlobalParameter.BatchFillAllRows)
					{
						flag2 = true;
					}
					if (flag2)
					{
						goto IL_45F;
					}
					IL_5DF:
					j++;
					continue;
					IL_45F:
					if (!base.View.GetFieldEditor(columnFKey, num).Enabled || !this.CurRowIsFlexEnabled(relatedFlexGroupArgs, list[j]))
					{
						goto IL_5DF;
					}
					if (relatedFlexGroupArgs != null && relatedFlexGroupArgs.FlexField != null)
					{
						if (!flag || this.IsNullFlexValue(relatedFlexGroupArgs, obj2))
						{
							DynamicObject flexObj = (DynamicObject)obj;
							base.View.Model.SetEntryCurrentRowIndex(entryEntity.Key, num);
							this.FlexGroupFieldBatchFill(focusRowIndex, columnFKey, relatedFlexGroupArgs, num, flexObj, bBatchControlFlex);
							goto IL_5DF;
						}
						goto IL_5DF;
					}
					else
					{
						if (!(field is RelatedFlexGroupField))
						{
							if (flag)
							{
								if (field is LotField || field is BaseDataTextField)
								{
									obj2 = field.GetFieldValue(base.View.Model.GetEntityDataObject(entryEntity)[num]);
								}
								if (!this.IsNullValue(field, obj2))
								{
									goto IL_5DF;
								}
							}
							base.View.Model.SetEntryCurrentRowIndex(entryEntity.Key, num);
							base.View.Model.SetValue(columnFKey, obj, num);
							base.View.InvokeFieldUpdateService(columnFKey, num);
							goto IL_5DF;
						}
						if (!flag || this.IsNullValue(field, obj2))
						{
							CustomBatchFillControler.RelatedFlexGroupBase flexArgs = new CustomBatchFillControler.RelatedFlexGroupBase((RelatedFlexGroupField)field);
							DynamicObject flexObj2 = (DynamicObject)obj;
							base.View.Model.SetEntryCurrentRowIndex(entryEntity.Key, num);
							this.FlexGroupFieldBatchFill(focusRowIndex, columnFKey, flexArgs, num, flexObj2, bBatchControlFlex);
							goto IL_5DF;
						}
						goto IL_5DF;
					}
				}
				base.View.Model.SetEntryCurrentRowIndex(entryEntity.Key, focusRowIndex);
			}
		}

		// Token: 0x06000E3A RID: 3642 RVA: 0x000A5130 File Offset: 0x000A3330
		private bool IsNullValue(Field field, object currentValue)
		{
			return ObjectUtils.IsNullOrEmptyOrWhiteSpace(currentValue) || ((field is DecimalField || field is IntegerField) && currentValue.ToString() == "0") || (currentValue is DynamicObjectCollection && (currentValue as DynamicObjectCollection).Count == 0);
		}

		// Token: 0x06000E3B RID: 3643 RVA: 0x000A5184 File Offset: 0x000A3384
		private bool IsNullFlexValue(CustomBatchFillControler.RelatedFlexGroupArgs flexArgs, object value)
		{
			DynamicObject dynamicObject = value as DynamicObject;
			if (dynamicObject == null)
			{
				return true;
			}
			object obj = dynamicObject[flexArgs.FlexSubField.PropertyName];
			return ObjectUtils.IsNullOrEmptyOrWhiteSpace(obj);
		}

		// Token: 0x06000E3C RID: 3644 RVA: 0x000A51B8 File Offset: 0x000A33B8
		private void FlexGroupFieldBatchFill(int BaseRowIndex, string columnFKey, CustomBatchFillControler.RelatedFlexGroupBase flexArgs, int i, DynamicObject flexObj, bool bBatchControlFlex)
		{
			string flexBaseDataValue = this.GetFlexBaseDataValue(flexArgs.FlexField, BaseRowIndex);
			string flexBaseDataValue2 = this.GetFlexBaseDataValue(flexArgs.FlexField, i);
			if (flexBaseDataValue == flexBaseDataValue2)
			{
				this.FlexGroupFieldBatchFillExt(columnFKey, flexArgs, i, flexObj, false);
				return;
			}
			if (bBatchControlFlex)
			{
				this.FlexGroupFieldBatchFillExt(columnFKey, flexArgs, i, flexObj, true);
			}
		}

		// Token: 0x06000E3D RID: 3645 RVA: 0x000A520C File Offset: 0x000A340C
		private string GetFlexBaseDataValue(RelatedFlexGroupField field, int row)
		{
			DynamicObject dynamicObject = base.View.Model.GetValue(field.RelatedBaseDataFlexGroupField, row) as DynamicObject;
			string result = "-1";
			if (dynamicObject != null)
			{
				result = dynamicObject["Id"].ToString();
			}
			return result;
		}

		// Token: 0x06000E3E RID: 3646 RVA: 0x000A5254 File Offset: 0x000A3454
		private void FlexGroupFieldBatchFillExt(string columnFKey, CustomBatchFillControler.RelatedFlexGroupBase flexBase, int i, DynamicObject flexObj, bool checkValComControl)
		{
			DynamicObject dynamicObject = (DynamicObject)ObjectUtils.CreateCopy(flexObj);
			bool flag = dynamicObject == null;
			CustomBatchFillControler.RelatedFlexGroupArgs relatedFlexGroupArgs = flexBase as CustomBatchFillControler.RelatedFlexGroupArgs;
			if (flexBase.FlexField.FlexDisplayFormat == 2 && relatedFlexGroupArgs != null)
			{
				RelatedFlexGroupFieldAppearance relatedFlexGroupFieldAppearance = (RelatedFlexGroupFieldAppearance)base.View.LayoutInfo.GetFieldAppearance(relatedFlexGroupArgs.FlexFieldKey);
				using (List<FieldAppearance>.Enumerator enumerator = relatedFlexGroupFieldAppearance.RelateFlexLayoutInfo.GetFieldAppearances().GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						FieldAppearance fieldAppearance = enumerator.Current;
						if (fieldAppearance.Key.Equals(relatedFlexGroupArgs.FlexSubFieldKey, StringComparison.OrdinalIgnoreCase))
						{
							object obj = flag ? null : dynamicObject[fieldAppearance.Field.PropertyName];
							ILookUpField lookUpField = relatedFlexGroupArgs.FlexSubField as ILookUpField;
							if (lookUpField != null)
							{
								if (checkValComControl)
								{
									BaseDataField baseDataField = base.View.BillBusinessInfo.GetField(flexBase.FlexField.RelatedBaseDataFlexGroupField) as BaseDataField;
									if (baseDataField == null)
									{
										break;
									}
									DynamicObject dynamicObject2 = base.View.Model.GetValue(baseDataField, i) as DynamicObject;
									if (dynamicObject2 == null)
									{
										break;
									}
									bool flag2 = flexBase.FlexField.IsValComControl(relatedFlexGroupArgs.ColumnKey, baseDataField, dynamicObject2, base.View.Context);
									if (flag2)
									{
										break;
									}
								}
								DynamicObject dynamicObject3 = obj as DynamicObject;
								if (lookUpField.NumberProperty == null || dynamicObject3 == null || !dynamicObject3.DynamicObjectType.Properties.Contains(lookUpField.NumberProperty.PropertyName))
								{
									base.View.Model.SetValue(relatedFlexGroupArgs.ColumnKey, obj, i);
									base.View.InvokeFieldUpdateService(flexBase.FlexField.Key, i);
								}
								else
								{
									base.View.Model.SetItemValueByNumber(relatedFlexGroupArgs.ColumnKey, ObjectUtils.Object2String(dynamicObject3[lookUpField.NumberProperty.PropertyName]), i);
									base.View.InvokeFieldUpdateService(flexBase.FlexField.Key, i);
								}
							}
							else
							{
								base.View.Model.SetValue(relatedFlexGroupArgs.ColumnKey, obj, i);
								base.View.InvokeFieldUpdateService(flexBase.FlexField.Key, i);
							}
						}
					}
					return;
				}
			}
			if (flexBase.FlexField.FlexDisplayFormat == 1)
			{
				long num = flag ? 0L : Convert.ToInt64(dynamicObject["Id"]);
				base.View.Model.SetValue(columnFKey, num, i);
				base.View.InvokeFieldUpdateService(flexBase.FlexField.Key, i);
			}
		}

		// Token: 0x06000E3F RID: 3647 RVA: 0x000A5500 File Offset: 0x000A3700
		private bool CurRowIsFlexEnabled(CustomBatchFillControler.RelatedFlexGroupArgs flexArgs, DynamicObject dy)
		{
			bool result = true;
			if (flexArgs != null && !ObjectUtils.IsNullOrEmptyOrWhiteSpace(flexArgs.ColumnKey))
			{
				result = base.View.StyleManager.GetFlexEnabled(flexArgs.ColumnKey, dy);
			}
			return result;
		}

		// Token: 0x04000687 RID: 1671
		private List<string> notBatchFillField = new List<string>
		{
			"FECNRowId",
			"FECNGroup",
			"FBomEntryId",
			"FRowId",
			"FParentRowId"
		};

		// Token: 0x020000C2 RID: 194
		private class RelatedFlexGroupBase
		{
			// Token: 0x170000AB RID: 171
			// (get) Token: 0x06000E41 RID: 3649 RVA: 0x000A558F File Offset: 0x000A378F
			// (set) Token: 0x06000E42 RID: 3650 RVA: 0x000A5597 File Offset: 0x000A3797
			public RelatedFlexGroupField FlexField { get; protected set; }

			// Token: 0x06000E43 RID: 3651 RVA: 0x000A55A0 File Offset: 0x000A37A0
			internal RelatedFlexGroupBase(RelatedFlexGroupField flexField)
			{
				this.FlexField = flexField;
			}
		}

		// Token: 0x020000C3 RID: 195
		private class RelatedFlexGroupArgs : CustomBatchFillControler.RelatedFlexGroupBase
		{
			// Token: 0x06000E44 RID: 3652 RVA: 0x000A55B0 File Offset: 0x000A37B0
			internal RelatedFlexGroupArgs(string columnKey, BusinessInfo billBusinessInfo) : base(null)
			{
				this.ColumnKey = columnKey;
				int num = this.ColumnKey.IndexOf("__");
				if (num == -1)
				{
					KDBusinessException ex = new KDBusinessException("EntryOperation.BatchFill.RelatedFlexGroupFillArgs", string.Format("维度关联字段字段标识异常，字段标识：{0}，Error:Index", columnKey));
					Logger.Error(ex.Code, ex.Message, ex);
					throw ex;
				}
				this.FlexFieldKey = this.ColumnKey.Substring(2, num - 2);
				this.FlexSubFieldKey = this.ColumnKey.Substring(num + 2);
				base.FlexField = (billBusinessInfo.GetField(this.FlexFieldKey) as RelatedFlexGroupField);
				if (base.FlexField == null)
				{
					KDBusinessException ex2 = new KDBusinessException("EntryOperation.BatchFill.RelatedFlexGroupFillArgs", string.Format("维度关联字段字段标识异常，字段标识：{0}，Error:FlexField", columnKey));
					Logger.Error(ex2.Code, ex2.Message, ex2);
					throw ex2;
				}
				this.FlexSubField = base.FlexField.RelateFlexBusinessInfo.GetField(this.FlexSubFieldKey);
				if (this.FlexSubField == null)
				{
					KDBusinessException ex3 = new KDBusinessException("EntryOperation.BatchFill.RelatedFlexGroupFillArgs", string.Format("维度关联字段字段标识异常，字段标识：{0}，Error:FlexSubField", columnKey));
					Logger.Error(ex3.Code, ex3.Message, ex3);
					throw ex3;
				}
			}

			// Token: 0x04000689 RID: 1673
			public readonly string ColumnKey;

			// Token: 0x0400068A RID: 1674
			public readonly string FlexFieldKey;

			// Token: 0x0400068B RID: 1675
			public readonly string FlexSubFieldKey;

			// Token: 0x0400068C RID: 1676
			public readonly Field FlexSubField;
		}
	}
}
