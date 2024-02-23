using System;
using System.Collections.Generic;
using System.Linq;
using Kingdee.BOS;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.DependencyRules;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata.FormElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.Core.MFG.Utils;
using Kingdee.K3.MFG.ServiceHelper;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.ECNControler
{
	// Token: 0x020000CF RID: 207
	public class SetupRepControler : AbstractItemControler
	{
		// Token: 0x06000E84 RID: 3716 RVA: 0x000A7B10 File Offset: 0x000A5D10
		public override void DoOperation()
		{
			this.BeforeF7Select(new BeforeF7SelectEventArgs
			{
				FieldKey = "FMATERIALIDCHILD",
				Row = -1
			});
		}

		// Token: 0x06000E85 RID: 3717 RVA: 0x000A7B80 File Offset: 0x000A5D80
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			string fieldKey;
			if ((fieldKey = e.FieldKey) != null)
			{
				if (!(fieldKey == "FMATERIALIDCHILD"))
				{
					return;
				}
				e.Cancel = true;
				this.selBomRows = this.GetBomChItems(base.SelectRows);
				PlugIn plugIn = new PlugIn("SubStituteViewForECN");
				plugIn.Id = SequentialGuid.NewGuid().ToString();
				plugIn.OrderId = 0;
				plugIn.IsEnabled = true;
				plugIn.ClassName = "Kingdee.K3.MFG.ENG.Business.PlugIn.Base.SubStituteViewForECN, Kingdee.K3.MFG.ENG.Business.PlugIn";
				BillShowParameter billShowParameter = new BillShowParameter();
				billShowParameter.OpenStyle.ShowType = 6;
				billShowParameter.FormId = "ENG_Substitution";
				billShowParameter.LayoutId = "2df30538-ce50-445e-9ce3-31100a38f100";
				billShowParameter.ParentPageId = base.View.PageId;
				billShowParameter.DynamicPlugins.Add(plugIn);
				billShowParameter.CustomParams.Add("showbeforesave", "1");
				base.View.Session["SelBomChItems"] = this.selBomRows;
				base.View.ShowForm(billShowParameter, delegate(FormResult x)
				{
					if (x.ReturnData == null)
					{
						return;
					}
					if (x.ReturnData is DynamicObject)
					{
						this.BindRepMainData((DynamicObject)x.ReturnData, e.Row);
					}
				});
			}
		}

		// Token: 0x06000E86 RID: 3718 RVA: 0x000A7CB3 File Offset: 0x000A5EB3
		public override void SetRowFieldControl(DynamicObject dynObj)
		{
		}

		// Token: 0x06000E87 RID: 3719 RVA: 0x000A7CE4 File Offset: 0x000A5EE4
		private List<DynamicObject> GetBomChItems(List<DynamicObject> eCNItems)
		{
			List<DynamicObject> list = new List<DynamicObject>();
			FormMetadata formMetadata = MetaDataServiceHelper.Load(base.View.Context, "ENG_BOM", true) as FormMetadata;
			Entity entity = formMetadata.BusinessInfo.GetEntity("FTreeEntity");
			foreach (DynamicObject dynamicObject in eCNItems)
			{
				int dynamicValue = DataEntityExtend.GetDynamicValue<int>(dynamicObject, "RowType", 0);
				long dynamicValue2 = DataEntityExtend.GetDynamicValue<long>(dynamicObject, "BomVersion_Id", 0L);
				DynamicObject[] array = BusinessDataServiceHelper.Load(base.View.Context, new string[]
				{
					dynamicValue2.ToString()
				}, formMetadata.BusinessInfo.GetDynamicObjectType());
				if (ObjectUtils.IsNullOrEmpty(array))
				{
					return list;
				}
				DynamicObjectCollection dynamicValue3 = DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(array.FirstOrDefault<DynamicObject>(), "TreeEntity", null);
				int num = dynamicValue;
				if (num != 1)
				{
					if (num == 5)
					{
						long dynamicValue4 = DataEntityExtend.GetDynamicValue<long>(dynamicObject, "BomEntryId", 0L);
						DynamicObject[] bomEntry = BusinessDataServiceHelper.Load(base.View.Context, new string[]
						{
							dynamicValue4.ToString()
						}, entity.DynamicObjectType);
						List<DynamicObject> collection = (from f in dynamicValue3
						where DataEntityExtend.GetDynamicValue<int>(f, "ReplaceGroup", 0) == DataEntityExtend.GetDynamicValue<int>(bomEntry.FirstOrDefault<DynamicObject>(), "ReplaceGroup", 0)
						select f).ToList<DynamicObject>();
						list.AddRange(collection);
					}
				}
				else
				{
					DynamicObject dynamicObject2 = new DynamicObject(entity.DynamicObjectType);
					DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "MATERIALIDCHILD_Id", DataEntityExtend.GetDynamicValue<long>(dynamicObject, "MATERIALIDCHILD_Id", 0L));
					DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "CHILDSUPPLYORGID_Id", DataEntityExtend.GetDynamicValue<long>(dynamicObject, "ChildSupplyOrgId_Id", 0L));
					DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "ParentRowId", "");
					DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "ReplacePolicy", DataEntityExtend.GetDynamicValue<string>(dynamicObject, "ReplacePolicy", null));
					DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "ReplaceType", DataEntityExtend.GetDynamicValue<string>(dynamicObject, "ReplaceType", null));
					DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "ReplacePriority", DataEntityExtend.GetDynamicValue<int>(dynamicObject, "ReplacePriority", 0));
					DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "IskeyItem", DataEntityExtend.GetDynamicValue<bool>(dynamicObject, "IskeyItem", false));
					DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "RowId", DataEntityExtend.GetDynamicValue<string>(dynamicObject, "RowId", null));
					DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "AuxPropId_Id", DataEntityExtend.GetDynamicValue<long>(dynamicObject, "AuxPropId_Id", 0L));
					DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "AuxPropId", DataEntityExtend.GetDynamicValue<DynamicObject>(dynamicObject, "AuxPropId", null));
					DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "BOMID_Id", DataEntityExtend.GetDynamicValue<long>(dynamicObject, "BOMID_Id", 0L));
					DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "ChildBaseUnitID_Id", DataEntityExtend.GetDynamicValue<long>(dynamicObject, "ChildBaseUnitID_Id", 0L));
					DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "CHILDUNITID_Id", DataEntityExtend.GetDynamicValue<long>(dynamicObject, "CHILDUNITID_Id", 0L));
					DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "NUMERATOR", DataEntityExtend.GetDynamicValue<decimal>(dynamicObject, "NUMERATOR", 0m));
					DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "DENOMINATOR", DataEntityExtend.GetDynamicValue<decimal>(dynamicObject, "DENOMINATOR", 0m));
					DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "BaseNumerator", DataEntityExtend.GetDynamicValue<decimal>(dynamicObject, "BaseNumerator", 0m));
					DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "BaseDenominator", DataEntityExtend.GetDynamicValue<decimal>(dynamicObject, "BaseDenominator", 0m));
					dynamicValue3.Add(dynamicObject2);
					list.Add(dynamicObject2);
				}
			}
			return list;
		}

		// Token: 0x06000E88 RID: 3720 RVA: 0x000A81BC File Offset: 0x000A63BC
		private void BindRepMainData(DynamicObject repData, int startIndex)
		{
			int entryCurrentRowIndex = base.View.Model.GetEntryCurrentRowIndex("FTreeEntity");
			Entity treeEntity = base.View.BusinessInfo.GetEntity("FTreeEntity");
			DynamicObjectCollection entityDataObject = base.View.Model.GetEntityDataObject(treeEntity);
			this.DeleteItems(entityDataObject, entryCurrentRowIndex);
			DynamicObject dynamicObject = entityDataObject[entryCurrentRowIndex];
			DynamicObjectCollection dynamicValue = DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(repData, "EntityMainItems", null);
			DynamicObjectCollection dynamicValue2 = DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(repData, "Entity", null);
			string dynamicValue3 = DataEntityExtend.GetDynamicValue<string>(repData, "ReplaceType", null);
			string dynamicValue4 = DataEntityExtend.GetDynamicValue<string>(repData, "ReplacePolicy", null);
			long dynamicValue5 = DataEntityExtend.GetDynamicValue<long>(repData, "ReplaceNo_Id", 0L);
			DynamicObject dynamicValue6 = DataEntityExtend.GetDynamicValue<DynamicObject>(repData, "ReplaceNo", null);
			if (dynamicValue5 <= 0L)
			{
				dynamicValue5 = DataEntityExtend.GetDynamicValue<long>(dynamicObject, "SubstitutionId_Id", 0L);
				dynamicValue6 = DataEntityExtend.GetDynamicValue<DynamicObject>(dynamicObject, "SubstitutionId", null);
			}
			DynamicObject dynamicObject2 = dynamicValue.FirstOrDefault((DynamicObject f) => DataEntityExtend.GetDynamicValue<bool>(f, "IsKeyItem", false));
			IEnumerable<DynamicObject> enumerable = from f in dynamicValue
			where !DataEntityExtend.GetDynamicValue<bool>(f, "IsKeyItem", false)
			select f;
			DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "IskeyItem", DataEntityExtend.GetDynamicValue<long>(dynamicObject2, "IsKeyItem", 0L));
			DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "ReplacePriority", DataEntityExtend.GetDynamicValue<long>(dynamicObject2, "MainPriority", 0L));
			DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "ReplacePolicy", dynamicValue4);
			DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "ReplaceType", dynamicValue3);
			DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "SubstitutionId_Id", dynamicValue5);
			DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "SubstitutionId", dynamicValue6);
			DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "STEntryId", DataEntityExtend.GetDynamicValue<long>(dynamicObject2, "Id", 0L));
			DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "NETDEMANDRATE", DataEntityExtend.GetDynamicValue<decimal>(dynamicObject2, "NETDEMANDRATE", 0m));
			IEnumerable<DynamicObject> enumerable2 = from f in dynamicValue2
			where DataEntityExtend.GetDynamicValue<long>(f, "SubBomEntryId", 0L) <= 0L
			select f;
			IEnumerable<DynamicObject> source = from f in dynamicValue2
			where DataEntityExtend.GetDynamicValue<long>(f, "SubBomEntryId", 0L) > 0L
			select f;
			IEnumerable<long> subBomEntryIds = from f in source
			select DataEntityExtend.GetDynamicValue<long>(f, "SubBomEntryId", 0L);
			from f in this.selBomRows
			where subBomEntryIds.Contains(DataEntityExtend.GetDynamicValue<long>(f, "Id", 0L))
			select f;
			IEnumerable<DynamicObject> enumerable3 = from f in this.selBomRows
			where !ObjectUtils.IsNullOrEmptyOrWhiteSpace(DataEntityExtend.GetDynamicValue<string>(f, "ParentRowId", null)) && !subBomEntryIds.Contains(DataEntityExtend.GetDynamicValue<long>(f, "Id", 0L))
			select f;
			int num = entryCurrentRowIndex + 1;
			List<DynamicObject> list = new List<DynamicObject>();
			list.Add(dynamicObject);
			if (DataEntityExtend.GetDynamicValue<int>(dynamicObject, "RowType", 0) == 1)
			{
				foreach (DynamicObject subsItem in enumerable2)
				{
					((AbstractDynamicFormModel)base.Model).InsertEntryRow("FTreeEntity", num);
					DynamicObject entityDataObject2 = base.Model.GetEntityDataObject(treeEntity, num);
					this.SetSubsInfo(entityDataObject2, dynamicObject2, subsItem, dynamicObject, 1, 2);
					DataEntityExtend.SetDynamicObjectItemValue(entityDataObject2, "ReplaceType", dynamicValue3);
					DataEntityExtend.SetDynamicObjectItemValue(entityDataObject2, "ReplacePolicy", dynamicValue4);
					DataEntityExtend.SetDynamicObjectItemValue(entityDataObject2, "SubstitutionId_Id", dynamicValue5);
					DataEntityExtend.SetDynamicObjectItemValue(entityDataObject2, "InsertRow", DataEntityExtend.GetDynamicValue<int>(dynamicObject, "InsertRow", 0));
					DynamicObject dynamicValue7 = DataEntityExtend.GetDynamicValue<DynamicObject>(entityDataObject2, "MATERIALIDCHILD", null);
					DynamicObject dynamicObject3 = DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(dynamicValue7, "MaterialProduce", null).FirstOrDefault<DynamicObject>();
					if (dynamicObject3 != null)
					{
						DataEntityExtend.SetDynamicObjectItemValue(entityDataObject2, "OverControlMode", DataEntityExtend.GetDynamicValue<string>(dynamicObject3, "OverControlMode", null));
					}
					list.Add(entityDataObject2);
					num++;
				}
			}
			if (DataEntityExtend.GetDynamicValue<int>(dynamicObject, "RowType", 0) == 5)
			{
				foreach (DynamicObject dynamicObject4 in dynamicValue2)
				{
					((AbstractDynamicFormModel)base.Model).InsertEntryRow("FTreeEntity", num);
					DynamicObject entityDataObject3 = base.Model.GetEntityDataObject(treeEntity, num);
					long subBomEntryId = DataEntityExtend.GetDynamicValue<long>(dynamicObject4, "SubBomEntryId", 0L);
					if (subBomEntryId <= 0L)
					{
						this.SetSubsInfo(entityDataObject3, dynamicObject4, dynamicObject4, dynamicObject, 5, 2);
						DataEntityExtend.SetDynamicObjectItemValue(entityDataObject3, "ReplaceGroup", DataEntityExtend.GetDynamicValue<int>(dynamicObject, "ReplaceGroup", 0));
						DataEntityExtend.SetDynamicObjectItemValue(entityDataObject3, "ReplaceType", dynamicValue3);
						DataEntityExtend.SetDynamicObjectItemValue(entityDataObject3, "ReplacePolicy", dynamicValue4);
						DataEntityExtend.SetDynamicObjectItemValue(entityDataObject3, "SubstitutionId_Id", dynamicValue5);
						DynamicObject dynamicValue8 = DataEntityExtend.GetDynamicValue<DynamicObject>(entityDataObject3, "MATERIALIDCHILD", null);
						DynamicObject dynamicObject5 = DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(dynamicValue8, "MaterialProduce", null).FirstOrDefault<DynamicObject>();
						if (dynamicObject5 != null)
						{
							DataEntityExtend.SetDynamicObjectItemValue(entityDataObject3, "OverControlMode", DataEntityExtend.GetDynamicValue<string>(dynamicObject5, "OverControlMode", null));
						}
						list.Add(entityDataObject3);
						num++;
					}
					else
					{
						DynamicObject dynamicObject6 = (from f in this.selBomRows
						where DataEntityExtend.GetDynamicValue<long>(f, "Id", 0L) == subBomEntryId
						select f).FirstOrDefault<DynamicObject>();
						if (!ObjectUtils.IsNullOrEmpty(dynamicObject6))
						{
							this.FillEntry(entityDataObject3, dynamicObject6, 0);
							if (!ListUtils.IsEmpty<DynamicObject>(DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(dynamicObject6, "BOMCHILDLOTBASEDQTY", null)))
							{
								Entity entity = base.View.BusinessInfo.GetEntity("FStairDosage");
								DynamicObjectCollection entityDataObject4 = base.Model.GetEntityDataObject(entity);
								int num2 = 1;
								foreach (DynamicObject sourceBomChildEntry in DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(dynamicObject6, "BOMCHILDLOTBASEDQTY", null))
								{
									base.FillStairDosageEntry(entityDataObject4, sourceBomChildEntry, num2);
									num2++;
								}
							}
							list.Add(entityDataObject3);
							num++;
							((AbstractDynamicFormModel)base.Model).InsertEntryRow("FTreeEntity", num);
							DynamicObject entityDataObject5 = base.Model.GetEntityDataObject(treeEntity, num);
							base.FillEntryValue(entityDataObject5, dynamicObject6);
							this.SetSubsInfo(entityDataObject5, dynamicObject4, dynamicObject4, dynamicObject, 5, 1);
							DataEntityExtend.SetDynamicObjectItemValue(entityDataObject5, "ReplaceGroup", DataEntityExtend.GetDynamicValue<int>(dynamicObject, "ReplaceGroup", 0));
							DataEntityExtend.SetDynamicObjectItemValue(entityDataObject5, "ReplaceType", dynamicValue3);
							DataEntityExtend.SetDynamicObjectItemValue(entityDataObject5, "ReplacePolicy", dynamicValue4);
							DataEntityExtend.SetDynamicObjectItemValue(entityDataObject5, "SubstitutionId_Id", dynamicValue5);
							list.Add(entityDataObject5);
							num++;
						}
					}
				}
				foreach (DynamicObject dynamicObject7 in enumerable3)
				{
					((AbstractDynamicFormModel)base.Model).InsertEntryRow("FTreeEntity", num);
					DynamicObject entityDataObject6 = base.Model.GetEntityDataObject(treeEntity, num);
					this.FillEntry(entityDataObject6, dynamicObject7, 3);
					if (!ListUtils.IsEmpty<DynamicObject>(DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(dynamicObject7, "BOMCHILDLOTBASEDQTY", null)))
					{
						Entity entity2 = base.View.BusinessInfo.GetEntity("FStairDosage");
						DynamicObjectCollection entityDataObject7 = base.Model.GetEntityDataObject(entity2);
						int num3 = 1;
						foreach (DynamicObject sourceBomChildEntry2 in DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(dynamicObject7, "BOMCHILDLOTBASEDQTY", null))
						{
							base.FillStairDosageEntry(entityDataObject7, sourceBomChildEntry2, num3);
							num3++;
						}
					}
					DataEntityExtend.GetDynamicValue<long>(dynamicObject7, "DOSAGETYPE", 0L);
					list.Add(entityDataObject6);
					num++;
				}
				using (IEnumerator<DynamicObject> enumerator6 = enumerable.GetEnumerator())
				{
					while (enumerator6.MoveNext())
					{
						DynamicObject mainNotKeyItem = enumerator6.Current;
						((AbstractDynamicFormModel)base.Model).InsertEntryRow("FTreeEntity", num);
						DynamicObject entityDataObject8 = base.Model.GetEntityDataObject(treeEntity, num);
						DynamicObject dynamicObject8 = (from f in this.selBomRows
						where DataEntityExtend.GetDynamicValue<long>(f, "Id", 0L) == DataEntityExtend.GetDynamicValue<long>(mainNotKeyItem, "BomEntryId", 0L)
						select f).FirstOrDefault<DynamicObject>();
						this.FillEntry(entityDataObject8, dynamicObject8, 5);
						if (!ListUtils.IsEmpty<DynamicObject>(DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(dynamicObject8, "BOMCHILDLOTBASEDQTY", null)))
						{
							Entity entity3 = base.View.BusinessInfo.GetEntity("FStairDosage");
							DynamicObjectCollection entityDataObject9 = base.Model.GetEntityDataObject(entity3);
							int num4 = 1;
							foreach (DynamicObject sourceBomChildEntry3 in DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(dynamicObject8, "BOMCHILDLOTBASEDQTY", null))
							{
								base.FillStairDosageEntry(entityDataObject9, sourceBomChildEntry3, num4);
								num4++;
							}
						}
						DataEntityExtend.SetDynamicObjectItemValue(entityDataObject8, "SubstitutionId_Id", dynamicValue5);
						DataEntityExtend.SetDynamicObjectItemValue(entityDataObject8, "SubstitutionId", dynamicValue6);
						DataEntityExtend.SetDynamicObjectItemValue(entityDataObject8, "STEntryId", DataEntityExtend.GetDynamicValue<long>(mainNotKeyItem, "Id", 0L));
						DataEntityExtend.SetDynamicObjectItemValue(entityDataObject8, "NETDEMANDRATE", DataEntityExtend.GetDynamicValue<decimal>(mainNotKeyItem, "NETDEMANDRATE", 0m));
						list.Add(entityDataObject8);
						num++;
					}
				}
			}
			this.SetECNGoup(list.ToArray());
			DynamicObject[] array = (from f in list
			where DataEntityExtend.GetDynamicValue<int>(f, "ChangeLabel", 0) == 2 || DataEntityExtend.GetDynamicValue<int>(f, "ChangeLabel", 0) == 1
			select f).ToArray<DynamicObject>();
			if (!ListUtils.IsEmpty<DynamicObject>(array))
			{
				DBServiceHelper.LoadReferenceObject(base.View.Context, array, array.FirstOrDefault<DynamicObject>().DynamicObjectType, false);
			}
			num--;
			base.SortItem(entityDataObject);
			base.SummaryUpdtBOMVers();
			list.ForEach(delegate(DynamicObject x)
			{
				this.View.UpdateView("FTreeEntity", this.Model.GetRowIndex(treeEntity, x));
			});
			base.View.RuleContainer.RaiseInitialized("FTreeEntity", entityDataObject.Parent, new BOSActionExecuteContext(base.View));
			base.View.SetEntityFocusRow("FTreeEntity", entryCurrentRowIndex);
		}

		// Token: 0x06000E89 RID: 3721 RVA: 0x000A8C68 File Offset: 0x000A6E68
		private void SetSubsInfo(DynamicObject targetRow, DynamicObject mainKeyItem, DynamicObject subsItem, DynamicObject ecnChItem, int rowType, int changeLabel)
		{
			DataEntityExtend.SetDynamicObjectItemValue(targetRow, "FISGETSCRAP", true);
			DataEntityExtend.SetDynamicObjectItemValue(targetRow, "ChildSupplyOrgId", null);
			DataEntityExtend.SetDynamicObjectItemValue(targetRow, "ChildSupplyOrgId_Id", 0);
			DataEntityExtend.SetDynamicObjectItemValue(targetRow, "SUPPLYORG_Id", DataEntityExtend.GetDynamicValue<long>(ecnChItem, "SUPPLYORG_Id", 0L));
			DataEntityExtend.SetDynamicObjectItemValue(targetRow, "SUPPLYORG", DataEntityExtend.GetDynamicValue<DynamicObject>(ecnChItem, "SUPPLYORG", null));
			DataEntityExtend.SetDynamicObjectItemValue(targetRow, "RowType", rowType);
			DataEntityExtend.SetDynamicObjectItemValue(targetRow, "ChangeLabel", changeLabel);
			DataEntityExtend.SetDynamicObjectItemValue(targetRow, "BOMCATEGORY", DataEntityExtend.GetDynamicValue<string>(ecnChItem, "BOMCATEGORY", null));
			DataEntityExtend.SetDynamicObjectItemValue(targetRow, "ParentMaterialId_Id", DataEntityExtend.GetDynamicValue<long>(ecnChItem, "ParentMaterialId_Id", 0L));
			DataEntityExtend.SetDynamicObjectItemValue(targetRow, "BomVersion_Id", DataEntityExtend.GetDynamicValue<long>(ecnChItem, "BomVersion_Id", 0L));
			DataEntityExtend.SetDynamicObjectItemValue(targetRow, "ParentRowId", DataEntityExtend.GetDynamicValue<string>(ecnChItem, "RowId", null));
			DataEntityExtend.SetDynamicObjectItemValue(targetRow, "ParentUnitID_Id", DataEntityExtend.GetDynamicValue<long>(ecnChItem, "ParentUnitID_Id", 0L));
			DataEntityExtend.SetDynamicObjectItemValue(targetRow, "ParentBaseUnitId_Id", DataEntityExtend.GetDynamicValue<long>(ecnChItem, "ParentBaseUnitId_Id", 0L));
			DataEntityExtend.SetDynamicObjectItemValue(targetRow, "POSITIONNO", DataEntityExtend.GetDynamicValue<string>(ecnChItem, "POSITIONNO", null));
			DataEntityExtend.SetDynamicObjectItemValue(targetRow, "MATERIALIDCHILD_Id", DataEntityExtend.GetDynamicValue<long>(subsItem, "SubMaterialID_Id", 0L));
			DataEntityExtend.SetDynamicObjectItemValue(targetRow, "ReplacePriority", DataEntityExtend.GetDynamicValue<int>(subsItem, "Priority", 0));
			DataEntityExtend.SetDynamicObjectItemValue(targetRow, "MRPPriority", DataEntityExtend.GetDynamicValue<int>(subsItem, "Priority", 0));
			DataEntityExtend.SetDynamicObjectItemValue(targetRow, "MRPPriority", DataEntityExtend.GetDynamicValue<int>(subsItem, "Priority", 0));
			DataEntityExtend.SetDynamicObjectItemValue(targetRow, "AuxPropId_Id", DataEntityExtend.GetDynamicValue<long>(subsItem, "SubAuxPropID_Id", 0L));
			DataEntityExtend.SetDynamicObjectItemValue(targetRow, "ChildSupplyOrgId_Id", DataEntityExtend.GetDynamicValue<long>(subsItem, "SubSupplyOrgId_Id", 0L));
			DataEntityExtend.SetDynamicObjectItemValue(targetRow, "BOMID_Id", DataEntityExtend.GetDynamicValue<long>(subsItem, "SubBomId_Id", 0L));
			DataEntityExtend.SetDynamicObjectItemValue(targetRow, "NUMERATOR", DataEntityExtend.GetDynamicValue<decimal>(subsItem, "SubNumerator", 0m));
			DataEntityExtend.SetDynamicObjectItemValue(targetRow, "DENOMINATOR", DataEntityExtend.GetDynamicValue<decimal>(subsItem, "SubDenominator", 0m));
			DataEntityExtend.SetDynamicObjectItemValue(targetRow, "EFFECTDATE", DataEntityExtend.GetDynamicValue<DateTime>(subsItem, "EffectDate", default(DateTime)));
			DataEntityExtend.SetDynamicObjectItemValue(targetRow, "EXPIREDATE", DataEntityExtend.GetDynamicValue<DateTime>(subsItem, "ExpireDate", default(DateTime)));
			DataEntityExtend.SetDynamicObjectItemValue(targetRow, "MATERIALTYPE", 3);
			DataEntityExtend.SetDynamicObjectItemValue(targetRow, "IskeyItem", DataEntityExtend.GetDynamicValue<bool>(subsItem, "SubIsKeyItem", false));
			DataEntityExtend.SetDynamicObjectItemValue(targetRow, "MEMO", DataEntityExtend.GetDynamicValue<LocaleValue>(subsItem, "MEMO", null));
			DataEntityExtend.SetDynamicObjectItemValue(targetRow, "CHILDUNITID_Id", DataEntityExtend.GetDynamicValue<long>(subsItem, "SubUnitID_Id", 0L));
			DataEntityExtend.SetDynamicObjectItemValue(targetRow, "ChildBaseUnitID_Id", DataEntityExtend.GetDynamicValue<long>(subsItem, "SubBaseUnitID_Id", 0L));
			DataEntityExtend.SetDynamicObjectItemValue(targetRow, "BaseNumerator", DataEntityExtend.GetDynamicValue<decimal>(subsItem, "SubBaseNumerator", 0m));
			DataEntityExtend.SetDynamicObjectItemValue(targetRow, "BaseDenominator", DataEntityExtend.GetDynamicValue<decimal>(subsItem, "SubBaseDenominator", 0m));
			DataEntityExtend.SetDynamicObjectItemValue(targetRow, "BomEntryId", DataEntityExtend.GetDynamicValue<long>(subsItem, "SubBomEntryId", 0L));
			DataEntityExtend.SetDynamicObjectItemValue(targetRow, "STEntryId", DataEntityExtend.GetDynamicValue<long>(subsItem, "Id", 0L));
			DataEntityExtend.SetDynamicObjectItemValue(targetRow, "NETDEMANDRATE", DataEntityExtend.GetDynamicValue<decimal>(subsItem, "SUBNETDEMANDRATE", 0m));
			DynamicObject dynamicObject = DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(DataEntityExtend.GetDynamicValue<DynamicObject>(subsItem, "SubMaterialID", null), "MaterialProduce", null).FirstOrDefault<DynamicObject>();
			if (dynamicObject != null)
			{
				DataEntityExtend.SetDynamicObjectItemValue(targetRow, "FIXSCRAPQTY", DataEntityExtend.GetDynamicValue<decimal>(dynamicObject, "FIXLOSS", 0m));
				DataEntityExtend.SetDynamicObjectItemValue(targetRow, "FSCRAPRATE", DataEntityExtend.GetDynamicValue<decimal>(dynamicObject, "LossPercent", 0m));
			}
			long value = MFGBillUtil.GetValue<long>(base.Model, "FChangeOrgId", -1, 0L, null);
			int systemProfile = MFGServiceHelper.GetSystemProfile<int>(base.View.Context, value, "MFG_EngParameter", "SubItemScrapDependOn", 2);
			if (systemProfile == 1)
			{
				DataEntityExtend.SetDynamicObjectItemValue(targetRow, "FIXSCRAPQTY", DataEntityExtend.GetDynamicValue<decimal>(ecnChItem, "FIXSCRAPQTY", 0m));
				DataEntityExtend.SetDynamicObjectItemValue(targetRow, "FSCRAPRATE", DataEntityExtend.GetDynamicValue<decimal>(ecnChItem, "FSCRAPRATE", 0m));
				DataEntityExtend.SetDynamicObjectItemValue(targetRow, "FISGETSCRAP", DataEntityExtend.GetDynamicValue<bool>(ecnChItem, "FISGETSCRAP", false));
			}
			DynamicObject dynamicValue = DataEntityExtend.GetDynamicValue<DynamicObject>(subsItem, "SubMaterialID", null);
			if (!ObjectUtils.IsNullOrEmpty(dynamicValue))
			{
				DynamicObjectCollection dynamicValue2 = DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(dynamicValue, "MaterialProduce", null);
				DynamicObjectCollection dynamicValue3 = DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(dynamicValue, "MaterialPlan", null);
				if (!ListUtils.IsEmpty<DynamicObject>(dynamicValue2))
				{
					string dynamicValue4 = DataEntityExtend.GetDynamicValue<string>(dynamicValue2.FirstOrDefault<DynamicObject>(), "IssueType", null);
					string dynamicValue5 = DataEntityExtend.GetDynamicValue<string>(dynamicValue2.FirstOrDefault<DynamicObject>(), "BKFLTime", null);
					DataEntityExtend.SetDynamicObjectItemValue(targetRow, "ISSUETYPE", dynamicValue4);
					DataEntityExtend.SetDynamicObjectItemValue(targetRow, "BACKFLUSHTYPE", dynamicValue5);
				}
				if (!ListUtils.IsEmpty<DynamicObject>(dynamicValue3))
				{
					string dynamicValue6 = DataEntityExtend.GetDynamicValue<string>(dynamicValue3.FirstOrDefault<DynamicObject>(), "PlanningStrategy", null);
					if (dynamicValue6.Equals("1") || dynamicValue6.Equals("0"))
					{
						DataEntityExtend.SetDynamicObjectItemValue(targetRow, "IsMrpRun", true);
					}
					else
					{
						DataEntityExtend.SetDynamicObjectItemValue(targetRow, "IsMrpRun", false);
					}
				}
			}
			DynamicObject[] array = new DynamicObject[]
			{
				targetRow
			};
			DBServiceHelper.LoadReferenceObject(base.View.Context, array, array.FirstOrDefault<DynamicObject>().DynamicObjectType, false);
			DataEntityExtend.SetDynamicObjectItemValue(targetRow, "DOSAGETYPE", "2");
		}

		// Token: 0x06000E8A RID: 3722 RVA: 0x000A9244 File Offset: 0x000A7444
		private void FillEntry(DynamicObject targetRow, DynamicObject sourceBomEntry, int changeLabel)
		{
			base.FillEntryValue(targetRow, sourceBomEntry);
			DataEntityExtend.SetDynamicObjectItemValue(targetRow, "RowType", 5);
			DataEntityExtend.SetDynamicObjectItemValue(targetRow, "ChangeLabel", changeLabel);
			DataEntityExtend.SetDynamicObjectItemValue(targetRow, "BomEntryId", DataEntityExtend.GetDynamicValue<long>(sourceBomEntry, "Id", 0L));
			DataEntityExtend.SetDynamicObjectItemValue(targetRow, "ParentMaterialId_Id", DataEntityExtend.GetDynamicValue<long>(sourceBomEntry.Parent as DynamicObject, "MATERIALID_Id", 0L));
			DataEntityExtend.SetDynamicObjectItemValue(targetRow, "ParentMaterialId", DataEntityExtend.GetDynamicValue<DynamicObject>(sourceBomEntry.Parent as DynamicObject, "MATERIALID", null));
			DataEntityExtend.SetDynamicObjectItemValue(targetRow, "ParentUnitID_Id", DataEntityExtend.GetDynamicValue<long>(sourceBomEntry.Parent as DynamicObject, "FUNITID_Id", 0L));
			DataEntityExtend.SetDynamicObjectItemValue(targetRow, "ParentUnitID", DataEntityExtend.GetDynamicValue<DynamicObject>(sourceBomEntry.Parent as DynamicObject, "FUNITID", null));
			DataEntityExtend.SetDynamicObjectItemValue(targetRow, "ParentBaseUnitId_Id", DataEntityExtend.GetDynamicValue<long>(sourceBomEntry.Parent as DynamicObject, "BaseUnitId_Id", 0L));
			DataEntityExtend.SetDynamicObjectItemValue(targetRow, "ParentBaseUnitId", DataEntityExtend.GetDynamicValue<DynamicObject>(sourceBomEntry.Parent as DynamicObject, "BaseUnitId", null));
			DataEntityExtend.SetDynamicObjectItemValue(targetRow, "BomVersion_Id", DataEntityExtend.GetDynamicValue<long>(sourceBomEntry.Parent as DynamicObject, "Id", 0L));
			DataEntityExtend.SetDynamicObjectItemValue(targetRow, "BomVersion", sourceBomEntry.Parent as DynamicObject);
			DataEntityExtend.SetDynamicObjectItemValue(targetRow, "BOMCATEGORY", DataEntityExtend.GetDynamicValue<string>(sourceBomEntry.Parent as DynamicObject, "BOMCATEGORY", null));
			DataEntityExtend.SetDynamicObjectItemValue(targetRow, "SUPPLYORG_Id", DataEntityExtend.GetDynamicValue<long>(sourceBomEntry, "SUPPLYORG_Id", 0L));
			DataEntityExtend.SetDynamicObjectItemValue(targetRow, "SUPPLYORG", DataEntityExtend.GetDynamicValue<DynamicObject>(sourceBomEntry, "SUPPLYORG", null));
		}

		// Token: 0x06000E8B RID: 3723 RVA: 0x000A9498 File Offset: 0x000A7698
		private void DeleteItems(DynamicObjectCollection dataCollection, int curCursor)
		{
			HashSet<string> selectGroup = new HashSet<string>(from x in dataCollection
			where dataCollection.IndexOf(x) == curCursor
			select x into r
			select DataEntityExtend.GetDynamicValue<string>(r, "ECNGroup", null));
			List<DynamicObject> list = (from x in dataCollection
			where selectGroup.Contains(DataEntityExtend.GetDynamicValue<string>(x, "ECNGroup", null)) && dataCollection.IndexOf(x) != curCursor
			select x).ToList<DynamicObject>();
			list.ForEach(delegate(DynamicObject r)
			{
				base.Model.DeleteEntryRow("FTreeEntity", base.Model.GetRowIndex(base.View.BusinessInfo.GetEntity("FTreeEntity"), r));
			});
		}

		// Token: 0x04000697 RID: 1687
		private List<DynamicObject> selBomRows;
	}
}
