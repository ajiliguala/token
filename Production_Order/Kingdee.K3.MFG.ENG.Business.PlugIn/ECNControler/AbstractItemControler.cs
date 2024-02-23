using System;
using System.Collections.Generic;
using System.Linq;
using Kingdee.BOS.BusinessEntity.Organizations;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.BD.ServiceHelper;
using Kingdee.K3.Core.BD;
using Kingdee.K3.Core.BD.ServiceArgs;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.Core.MFG.Utils;
using Kingdee.K3.MFG.ServiceHelper;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.ECNControler
{
	// Token: 0x020000BC RID: 188
	public abstract class AbstractItemControler
	{
		// Token: 0x170000A1 RID: 161
		// (get) Token: 0x06000DDF RID: 3551 RVA: 0x000A1BF6 File Offset: 0x0009FDF6
		// (set) Token: 0x06000DE0 RID: 3552 RVA: 0x000A1BFE File Offset: 0x0009FDFE
		public IDynamicFormView View { get; set; }

		// Token: 0x170000A2 RID: 162
		// (get) Token: 0x06000DE1 RID: 3553 RVA: 0x000A1C07 File Offset: 0x0009FE07
		// (set) Token: 0x06000DE2 RID: 3554 RVA: 0x000A1C0F File Offset: 0x0009FE0F
		public IDynamicFormModel Model { get; set; }

		// Token: 0x170000A3 RID: 163
		// (get) Token: 0x06000DE3 RID: 3555 RVA: 0x000A1C18 File Offset: 0x0009FE18
		// (set) Token: 0x06000DE4 RID: 3556 RVA: 0x000A1C20 File Offset: 0x0009FE20
		public Queue<DynamicObject> RowControlBuffer { get; set; }

		// Token: 0x170000A4 RID: 164
		// (get) Token: 0x06000DE5 RID: 3557 RVA: 0x000A1C29 File Offset: 0x0009FE29
		// (set) Token: 0x06000DE6 RID: 3558 RVA: 0x000A1C31 File Offset: 0x0009FE31
		public List<DynamicObject> SelectRows { get; set; }

		// Token: 0x170000A5 RID: 165
		// (get) Token: 0x06000DE7 RID: 3559 RVA: 0x000A1C3A File Offset: 0x0009FE3A
		// (set) Token: 0x06000DE8 RID: 3560 RVA: 0x000A1C42 File Offset: 0x0009FE42
		public List<long> ECREntryBomId { get; set; }

		// Token: 0x170000A6 RID: 166
		// (get) Token: 0x06000DE9 RID: 3561 RVA: 0x000A1C4B File Offset: 0x0009FE4B
		// (set) Token: 0x06000DEA RID: 3562 RVA: 0x000A1C53 File Offset: 0x0009FE53
		public bool UseECREntryBomId { get; set; }

		// Token: 0x170000A7 RID: 167
		// (get) Token: 0x06000DEB RID: 3563 RVA: 0x000A1C5C File Offset: 0x0009FE5C
		// (set) Token: 0x06000DEC RID: 3564 RVA: 0x000A1C64 File Offset: 0x0009FE64
		public Dictionary<long, BaseDataControlPolicyTargetOrgEntry> BomEntryCtrlSettings
		{
			get
			{
				return this.bomEntryCtrlSettings;
			}
			set
			{
				this.bomEntryCtrlSettings = value;
			}
		}

		// Token: 0x170000A8 RID: 168
		// (get) Token: 0x06000DED RID: 3565 RVA: 0x000A1C6D File Offset: 0x0009FE6D
		protected FormMetadata BomMeta
		{
			get
			{
				if (this.bomMeta == null)
				{
					this.bomMeta = (MetaDataServiceHelper.Load(this.View.Context, "ENG_BOM", true) as FormMetadata);
				}
				return this.bomMeta;
			}
		}

		// Token: 0x170000A9 RID: 169
		// (get) Token: 0x06000DEE RID: 3566 RVA: 0x000A1C9E File Offset: 0x0009FE9E
		// (set) Token: 0x06000DEF RID: 3567 RVA: 0x000A1CB5 File Offset: 0x0009FEB5
		protected Entity BomTreeEntity
		{
			get
			{
				return this.BomMeta.BusinessInfo.GetEntity("FTreeEntity");
			}
			set
			{
				this.bomTreeEntity = value;
			}
		}

		// Token: 0x06000DF0 RID: 3568 RVA: 0x000A1CBE File Offset: 0x0009FEBE
		public virtual void AddEntryRow(ListSelectedRowCollection collection, int startIndex)
		{
		}

		// Token: 0x06000DF1 RID: 3569 RVA: 0x000A1CC0 File Offset: 0x0009FEC0
		public virtual void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			string fieldKey;
			if ((fieldKey = e.FieldKey) != null)
			{
				if (fieldKey == "FBOMID")
				{
					e.IsShowApproved = false;
					e.ListFilterParameter.Filter = this.GetBomId2Filter(e.ListFilterParameter.Filter, e.Row);
					return;
				}
				if (!(fieldKey == "FSTOCKID"))
				{
					return;
				}
				e.ListFilterParameter.Filter = this.SetDefaultStockFilter(e.ListFilterParameter.Filter, e.Row);
			}
		}

		// Token: 0x06000DF2 RID: 3570 RVA: 0x000A1D3E File Offset: 0x0009FF3E
		public virtual void BeforeSetItemValueByNumber(BeforeSetItemValueByNumberArgs e)
		{
		}

		// Token: 0x06000DF3 RID: 3571 RVA: 0x000A1D40 File Offset: 0x0009FF40
		public virtual void DoOperation()
		{
		}

		// Token: 0x06000DF4 RID: 3572 RVA: 0x000A1D42 File Offset: 0x0009FF42
		public virtual void BeforeUpdateValue(BeforeUpdateValueEventArgs e)
		{
		}

		// Token: 0x06000DF5 RID: 3573 RVA: 0x000A1D44 File Offset: 0x0009FF44
		public virtual void DataChanged(DataChangedEventArgs e)
		{
		}

		// Token: 0x06000DF6 RID: 3574 RVA: 0x000A1D46 File Offset: 0x0009FF46
		public virtual void AfterF7Select(AfterF7SelectEventArgs e, int rowIndex)
		{
		}

		// Token: 0x06000DF7 RID: 3575 RVA: 0x000A1D74 File Offset: 0x0009FF74
		public virtual void SetRowFieldControl(DynamicObject dynObj)
		{
			Entity entity = this.View.BusinessInfo.GetEntity("FTreeEntity");
			DynamicObject dynamicObject = dynObj["BomVersion"] as DynamicObject;
			if (dynamicObject == null)
			{
				return;
			}
			long dynamicValue = DataEntityExtend.GetDynamicValue<long>(dynamicObject, "CreateOrgId_Id", 0L);
			if (dynamicValue == DataEntityExtend.GetDynamicValue<long>(this.Model.DataObject, "ChangeOrgId_Id", 0L))
			{
				return;
			}
			BaseDataControlPolicyTargetOrgEntry baseDataControlPolicyTargetOrgEntry;
			if (ListUtils.IsEmpty<KeyValuePair<long, BaseDataControlPolicyTargetOrgEntry>>(this.BomEntryCtrlSettings) || !this.BomEntryCtrlSettings.TryGetValue(dynamicValue, out baseDataControlPolicyTargetOrgEntry))
			{
				BaseDataControlPolicy baseDataControlPolicyDObj = OrganizationServiceHelper.GetBaseDataControlPolicyDObj(this.View.Context, dynamicValue, "ENG_BOM");
				baseDataControlPolicyTargetOrgEntry = (from x in baseDataControlPolicyDObj.TargetOrgEntrys
				where x.TargetOrgId == DataEntityExtend.GetDynamicValue<long>(this.Model.DataObject, "ChangeOrgId_Id", 0L)
				select x).FirstOrDefault<BaseDataControlPolicyTargetOrgEntry>();
				if (baseDataControlPolicyTargetOrgEntry == null)
				{
					return;
				}
				this.BomEntryCtrlSettings.Add(dynamicValue, baseDataControlPolicyTargetOrgEntry);
			}
			Dictionary<string, BaseDataControlPolicyPropertyEntry> dictionary = baseDataControlPolicyTargetOrgEntry.PropertyEntrys.ToDictionary((BaseDataControlPolicyPropertyEntry x) => x.Key);
			foreach (Field field in entity.Fields)
			{
				BaseDataControlPolicyPropertyEntry baseDataControlPolicyPropertyEntry;
				if (!(field is BasePropertyField) && dictionary.TryGetValue(field.Key, out baseDataControlPolicyPropertyEntry) && baseDataControlPolicyPropertyEntry.Locked == 1)
				{
					this.View.StyleManager.SetEnabled(field, dynObj, "bdCtrl", false);
				}
			}
		}

		// Token: 0x06000DF8 RID: 3576 RVA: 0x000A1F1C File Offset: 0x000A011C
		protected Dictionary<string, DynamicObject> GetBomHeadObjectBySelectedRows(ListSelectedRowCollection lsr)
		{
			object[] array = (from x in lsr
			select x.PrimaryKeyValue).ToArray<object>();
			List<string> list = (from x in this.BomMeta.BusinessInfo.GetEntity("FBillHead").Fields
			select x.Key).ToList<string>();
			DynamicObject[] source = BusinessDataServiceHelper.Load(this.View.Context, array, this.BomMeta.BusinessInfo.GetSubBusinessInfo(list).GetDynamicObjectType());
			Dictionary<string, DynamicObject> dictionary = new Dictionary<string, DynamicObject>();
			Dictionary<string, IGrouping<string, DynamicObject>> dictionary2 = (from x in source
			group x by x["Id"].ToString()).ToDictionary((IGrouping<string, DynamicObject> x) => x.Key);
			foreach (ListSelectedRow listSelectedRow in lsr)
			{
				IGrouping<string, DynamicObject> source2;
				if (dictionary2.TryGetValue(listSelectedRow.PrimaryKeyValue, out source2) && !ObjectUtils.IsNullOrEmptyOrWhiteSpace(listSelectedRow.EntryPrimaryKeyValue))
				{
					dictionary.Add(listSelectedRow.EntryPrimaryKeyValue, source2.First<DynamicObject>());
				}
			}
			return dictionary;
		}

		// Token: 0x06000DF9 RID: 3577 RVA: 0x000A2078 File Offset: 0x000A0278
		protected void RemoveEntryRows(List<DynamicObject> delCollection)
		{
			DynamicObjectCollection dynamicObjectCollection = this.Model.DataObject["TreeEntity"] as DynamicObjectCollection;
			foreach (DynamicObject item in delCollection)
			{
				this.Model.DeleteEntryRow("FTreeEntity", dynamicObjectCollection.IndexOf(item));
			}
		}

		// Token: 0x06000DFA RID: 3578 RVA: 0x000A20F4 File Offset: 0x000A02F4
		protected virtual void FillBomHeadObjectValue(DynamicObject targetRow, Dictionary<string, DynamicObject> dictBomHeadsGroupByEntryId, long parentRowId, int index)
		{
			DynamicObject dynamicObject;
			if (dictBomHeadsGroupByEntryId.TryGetValue(targetRow["BomEntryId"].ToString(), out dynamicObject) || dictBomHeadsGroupByEntryId.TryGetValue(parentRowId.ToString(), out dynamicObject))
			{
				this.Model.SetValue("FBOMVERSION", dynamicObject, index);
			}
		}

		// Token: 0x06000DFB RID: 3579 RVA: 0x000A219C File Offset: 0x000A039C
		public void SummaryUpdtBOMVers()
		{
			if (!OtherExtend.ConvertTo<bool>(this.Model.GetValue("FIsUpdateVersion"), false))
			{
				return;
			}
			DynamicObjectCollection entityDataObject = this.Model.GetEntityDataObject(this.View.BusinessInfo.GetEntity("FUpdateVersionEntity"));
			HashSet<string> hashSet = new HashSet<string>();
			DynamicObjectCollection entityDataObject2 = this.Model.GetEntityDataObject(this.View.BusinessInfo.GetEntity("FTreeEntity"));
			foreach (DynamicObject dynamicObject in entityDataObject2)
			{
				DynamicObject dynamicObject2 = dynamicObject["BomVersion"] as DynamicObject;
				if (dynamicObject2 != null)
				{
					hashSet.Add(DataEntityExtend.GetDynamicValue<string>(dynamicObject2, "MATERIALID_Id", null) + "^*^" + DataEntityExtend.GetDynamicValue<string>(dynamicObject2, "Number", null));
				}
			}
			DynamicObjectCollection entityDataObject3 = this.Model.GetEntityDataObject(this.View.BusinessInfo.GetEntity("FCobyEntity"));
			foreach (DynamicObject dynamicObject3 in entityDataObject3)
			{
				DynamicObject dynamicObject4 = dynamicObject3["CobyBomVersion"] as DynamicObject;
				if (dynamicObject4 != null)
				{
					hashSet.Add(DataEntityExtend.GetDynamicValue<string>(dynamicObject4, "MATERIALID_Id", null) + "^*^" + DataEntityExtend.GetDynamicValue<string>(dynamicObject4, "Number", null));
				}
			}
			HashSet<string> hashSet2 = new HashSet<string>(from x in entityDataObject
			select DataEntityExtend.GetDynamicValue<string>(x, "MaterialId_Id", null) + "^*^" + DataEntityExtend.GetDynamicValue<string>(x, "SRCBOM", null));
			List<DynamicObject> list = new List<DynamicObject>();
			using (IEnumerator<string> enumerator3 = hashSet2.Except(hashSet).GetEnumerator())
			{
				while (enumerator3.MoveNext())
				{
					string rmVer = enumerator3.Current;
					list.AddRange(from x in entityDataObject
					where DataEntityExtend.GetDynamicValue<string>(x, "MaterialId_Id", null) + "^*^" + DataEntityExtend.GetDynamicValue<string>(x, "SRCBOM", null) == rmVer
					select x);
				}
			}
			foreach (DynamicObject item in list)
			{
				this.Model.DeleteEntryRow("FUpdateVersionEntity", entityDataObject.IndexOf(item));
			}
			foreach (string text in hashSet.Except(hashSet2))
			{
				int num;
				DataEntityExtend.CreateNewEntryRow(this.Model, this.View.BusinessInfo.GetEntity("FUpdateVersionEntity"), -1, ref num);
				string[] array = text.Split(new string[]
				{
					"^*^"
				}, StringSplitOptions.RemoveEmptyEntries);
				this.Model.SetValue("FMaterialIdU", array[0], num);
				this.Model.SetValue("FSRCBOM", array[1], num);
			}
		}

		// Token: 0x06000DFC RID: 3580 RVA: 0x000A24BC File Offset: 0x000A06BC
		protected virtual void FillEntryValue(DynamicObject targetRow, DynamicObject sourceBomEntry)
		{
			DataEntityExtend.CopyPropertyValues(targetRow, sourceBomEntry, new Func<GetFieldValueCallbackParam, object>(this.GetCHFieldValueCallback), null);
			DataEntityExtend.SetDynamicObjectItemValue(targetRow, "BomEntryId", sourceBomEntry["Id"]);
			DBServiceHelper.LoadReferenceObject(this.View.Context, new DynamicObject[]
			{
				targetRow
			}, targetRow.DynamicObjectType, true);
		}

		// Token: 0x06000DFD RID: 3581 RVA: 0x000A2524 File Offset: 0x000A0724
		protected void SortItem(DynamicObjectCollection dataCollection)
		{
			IEnumerable<IGrouping<string, DynamicObject>> enumerable = from x in dataCollection
			group x by DataEntityExtend.GetDynamicValue<string>(x, "ECNGroup", null);
			int num = 1;
			foreach (IGrouping<string, DynamicObject> grouping in enumerable)
			{
				foreach (DynamicObject dynamicObject in grouping)
				{
					dynamicObject["Seq"] = num++;
				}
			}
		}

		// Token: 0x06000DFE RID: 3582 RVA: 0x000A25DC File Offset: 0x000A07DC
		protected virtual void SetECNGoup(params DynamicObject[] entryGroup)
		{
			string text = SequentialGuid.NewGuid().ToString();
			foreach (DynamicObject dynamicObject in entryGroup)
			{
				DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "ECNGroup", text);
			}
		}

		// Token: 0x06000DFF RID: 3583 RVA: 0x000A2624 File Offset: 0x000A0824
		protected object GetCHFieldValueCallback(GetFieldValueCallbackParam param)
		{
			FormMetadata formMetadata = null;
			param.Options.TryGetVariableValue<FormMetadata>("FormMetadata", ref formMetadata);
			string name;
			if ((name = param.TargetProperty.Name) != null)
			{
				if (!(name == "OWNERTYPEID") && !(name == "ISSUETYPE") && !(name == "BACKFLUSHTYPE"))
				{
					if (!(name == "ECNRowId"))
					{
						if (!(name == "AuxPropId"))
						{
							if (name == "ChildSupplyOrgId")
							{
								return param.SourceProperty.GetValue(param.SourceDataEntity);
							}
						}
						else
						{
							object value = param.SourceProperty.GetValue(param.SourceDataEntity);
							if (value != null)
							{
								return OrmUtils.Clone((DynamicObject)value, false, false);
							}
							param.IsCancel = true;
						}
					}
					else
					{
						param.IsCancel = true;
					}
				}
				else if (param.SourceProperty != null && param.SourceDataEntity != null)
				{
					object value2 = param.SourceProperty.GetValue(param.SourceDataEntity);
					if (value2 == null)
					{
						param.IsCancel = true;
					}
					else
					{
						if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(value2))
						{
							return value2;
						}
						param.IsCancel = true;
					}
				}
			}
			if (param.SourceProperty != null && param.SourceDataEntity != null)
			{
				object value3 = param.SourceProperty.GetValue(param.SourceDataEntity);
				if (value3 != null)
				{
					return value3;
				}
				param.IsCancel = true;
			}
			return null;
		}

		// Token: 0x06000E00 RID: 3584 RVA: 0x000A2770 File Offset: 0x000A0970
		protected string GetChildSupplyOrgFilter(string filter, int row)
		{
			if (filter == null)
			{
				filter = string.Empty;
			}
			DynamicObject dynamicObject = this.Model.GetValue("FMATERIALIDCHILD", row) as DynamicObject;
			long num = (dynamicObject == null) ? 0L : OtherExtend.ConvertTo<long>(dynamicObject["msterID"], 0L);
			long num2 = OtherExtend.ConvertTo<long>(this.View.Model.DataObject["ChangeOrgId_Id"], 0L);
			List<long> list = new List<long>
			{
				102L,
				112L,
				101L,
				104L,
				103L,
				109L
			};
			List<long> list2 = new List<long>();
			List<long> orgByBizRelationOrgs = MFGServiceHelper.GetOrgByBizRelationOrgs(this.View.Context, num2, list);
			if (orgByBizRelationOrgs == null || orgByBizRelationOrgs.Count < 1)
			{
				return filter;
			}
			list2.AddRange(orgByBizRelationOrgs);
			list2.Add(num2);
			filter += ((filter.Length > 0) ? (" AND " + string.Format("FORGID in ({0})", string.Join<long>(",", list2))) : (string.Format("FORGID in ({0})", string.Join<long>(",", list2)) + string.Format(" AND EXISTS (SELECT 1 FROM T_BD_MATERIAL TM WHERE TM.FMASTERID = {0} AND T0.FORGID = TM.FUSEORGID)", num)));
			return filter;
		}

		// Token: 0x06000E01 RID: 3585 RVA: 0x000A28E8 File Offset: 0x000A0AE8
		protected string SetChildMaterilIdFilterString(int row)
		{
			DynamicObject dynamicObject = this.View.Model.GetValue("FParentMaterialId", row) as DynamicObject;
			if (dynamicObject == null)
			{
				return "1=0";
			}
			string text = string.Format("(FMATERIALID <> {0})", DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObject, "Id", 0L));
			long parentMProperty = 0L;
			parentMProperty = Convert.ToInt64((dynamicObject["MaterialBase"] as DynamicObjectCollection).FirstOrDefault<DynamicObject>()["ErpClsID"]);
			long num = OtherExtend.ConvertTo<long>(this.View.Model.GetValue("FBOMCATEGORY", row), 0L);
			List<int> list = new List<int>();
			if ((int)num == 1)
			{
				list = new List<KeyValuePair<int, List<int>>>
				{
					new KeyValuePair<int, List<int>>(2, new List<int>
					{
						2,
						3,
						5,
						1,
						10,
						6
					}),
					new KeyValuePair<int, List<int>>(3, new List<int>
					{
						2,
						3,
						5,
						1,
						10,
						6
					}),
					new KeyValuePair<int, List<int>>(5, new List<int>
					{
						2,
						3,
						5,
						1,
						10,
						6
					}),
					new KeyValuePair<int, List<int>>(1, new List<int>
					{
						1,
						6
					}),
					new KeyValuePair<int, List<int>>(9, new List<int>
					{
						2,
						3,
						5,
						1,
						9,
						10,
						6
					}),
					new KeyValuePair<int, List<int>>(4, new List<int>
					{
						2,
						3,
						5,
						1,
						9,
						10,
						6
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
						10,
						6
					})
				}.FindLast((KeyValuePair<int, List<int>> w) => (long)w.Key == parentMProperty).Value;
			}
			if (ListUtils.IsEmpty<int>(list))
			{
				return text;
			}
			return text + string.Format(" and (FErpClsID IN('{0}'))", string.Join<int>("','", list));
		}

		// Token: 0x06000E02 RID: 3586 RVA: 0x000A2C20 File Offset: 0x000A0E20
		protected string GetBomId2Filter(string filter, int row)
		{
			if (filter == null)
			{
				filter = string.Empty;
			}
			string text = string.Empty;
			text = " FMATERIALID=0 ";
			DynamicObject dynamicObject = this.View.Model.GetValue("FMATERIALIDCHILD", row) as DynamicObject;
			if (dynamicObject != null)
			{
				long dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObject, "MsterId", 0L);
				long value = MFGBillUtil.GetValue<long>(this.View.Model, "FChildSupplyOrgId", row, 0L, null);
				FormMetadata formMetadata = MetaDataServiceHelper.Load(this.View.Context, "BD_MATERIAL", true) as FormMetadata;
				object baseDataPkId = BaseDataServiceHelper.GetBaseDataPkId(this.View.Context, formMetadata.BusinessInfo, "BD_MATERIAL", dynamicObjectItemValue, value);
				if (baseDataPkId != null && baseDataPkId is DynamicObject)
				{
					text = string.Format(" FMATERIALID={0} ", Convert.ToInt64(((DynamicObject)baseDataPkId)[0]));
				}
				else if (baseDataPkId != null && baseDataPkId is long)
				{
					text = string.Format(" FMATERIALID={0} ", Convert.ToInt64(baseDataPkId));
				}
			}
			filter += ((filter.Length > 0) ? (" AND " + text) : text);
			return filter;
		}

		// Token: 0x06000E03 RID: 3587 RVA: 0x000A2D44 File Offset: 0x000A0F44
		protected string SetDefaultStockFilter(string filter, int row)
		{
			long value = MFGBillUtil.GetValue<long>(this.View.Model, "FSUPPLYORG", row, -1L, "TreeEntity");
			if (value > 0L)
			{
				if (filter.Length > 0)
				{
					filter = filter + " AND FUSEORGID = " + value.ToString();
				}
				else
				{
					filter = filter + " FUSEORGID = " + value.ToString();
				}
			}
			return filter;
		}

		// Token: 0x06000E04 RID: 3588 RVA: 0x000A2DC4 File Offset: 0x000A0FC4
		protected string GetSelectedBomEntryFilterString()
		{
			Entity entity = this.View.BusinessInfo.GetEntity("FTreeEntity");
			DynamicObjectCollection entityDataObject = this.Model.GetEntityDataObject(entity);
			if (ListUtils.IsEmpty<DynamicObject>(entityDataObject))
			{
				return string.Empty;
			}
			List<long> list = (from x in entityDataObject
			select DataEntityExtend.GetDynamicValue<long>(x, "BomEntryId", 0L) into x
			where x > 0L
			select x).Distinct<long>().ToList<long>();
			if (ListUtils.IsEmpty<long>(list))
			{
				return string.Empty;
			}
			return string.Format(" t1.FEntryId NOT IN({0}) ", string.Join<long>(",", list));
		}

		// Token: 0x06000E05 RID: 3589 RVA: 0x000A2E78 File Offset: 0x000A1078
		protected void CreateStairDosageEntity(DynamicObject dyChildMaterial, DynamicObject dyParentMaterial, long lParentUnitId, long lParentBaseUnitId, long lParentMaterialId)
		{
			if (this.View.Model.GetEntryRowCount("FStairDosage") <= 0)
			{
				this.View.Model.CreateNewEntryRow("FStairDosage");
			}
			long dynamicValue = DataEntityExtend.GetDynamicValue<long>(dyChildMaterial, "Id", 0L);
			if (dynamicValue > 0L)
			{
				int entryRowCount = this.View.Model.GetEntryRowCount("FStairDosage");
				Entity entryEntity = this.View.BillBusinessInfo.GetEntryEntity("FStairDosage");
				Field field = this.View.BillBusinessInfo.GetField("FBaseNumeratorLot");
				Field field2 = this.View.BillBusinessInfo.GetField("FBaseDenominatorLot");
				UnitConvert unitConvertRate = UnitConvertServiceHelper.GetUnitConvertRate(this.View.Context, new GetUnitConvertRateArgs
				{
					MasterId = DataEntityExtend.GetDynamicObjectItemValue<long>(dyParentMaterial, "msterID", 0L),
					SourceUnitId = lParentUnitId,
					DestUnitId = lParentBaseUnitId
				});
				this.View.BillBusinessInfo.GetField("FUnitIDLot");
				this.View.BillBusinessInfo.GetField("FBaseUnitIDLot");
				this.View.Model.SetValue("FMaterialIdLotBased", dynamicValue, entryRowCount - 1);
				DynamicObject entityDataObject = this.Model.GetEntityDataObject(entryEntity, entryRowCount - 1);
				if (entityDataObject != null && field != null)
				{
					long dynamicValue2 = DataEntityExtend.GetDynamicValue<long>(entityDataObject, "UnitIDLot_Id", 0L);
					long dynamicValue3 = DataEntityExtend.GetDynamicValue<long>(entityDataObject, "BaseUnitIDLot_Id", 0L);
					UnitConvert unitConvertRate2 = UnitConvertServiceHelper.GetUnitConvertRate(this.View.Context, new GetUnitConvertRateArgs
					{
						MasterId = DataEntityExtend.GetDynamicObjectItemValue<long>(dyChildMaterial, "msterID", 0L),
						SourceUnitId = dynamicValue2,
						DestUnitId = dynamicValue3
					});
					if (unitConvertRate2 != null)
					{
						this.Model.SetValue(field, entityDataObject, unitConvertRate2.ConvertQty(1m, ""));
					}
					if (unitConvertRate != null)
					{
						this.Model.SetValue(field2, entityDataObject, unitConvertRate.ConvertQty(1m, ""));
					}
				}
				this.View.UpdateView("FMatreialIdLotBased", entryRowCount - 1);
				this.View.UpdateView("FStairDosage");
			}
		}

		// Token: 0x06000E06 RID: 3590 RVA: 0x000A30A4 File Offset: 0x000A12A4
		protected void FillStairDosageEntry(DynamicObjectCollection targetCollection, DynamicObject sourceBomChildEntry, int index)
		{
			DynamicObject dynamicObject = new DynamicObject(targetCollection.DynamicCollectionItemPropertyType);
			DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "Seq", index);
			DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "MaterialIdLotBased_Id", DataEntityExtend.GetDynamicValue<long>(sourceBomChildEntry, "MATERIALIDLOTBASED_Id", 0L));
			DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "MaterialIdLotBased", DataEntityExtend.GetDynamicValue<DynamicObject>(sourceBomChildEntry, "MATERIALIDLOTBASED", null));
			DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "StartQty", DataEntityExtend.GetDynamicValue<decimal>(sourceBomChildEntry, "STARTQTY", 0m));
			DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "EndQty", DataEntityExtend.GetDynamicValue<decimal>(sourceBomChildEntry, "ENDQTY", 0m));
			DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "UnitIDLot_Id", DataEntityExtend.GetDynamicValue<long>(sourceBomChildEntry, "UNITIDLOT_Id", 0L));
			DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "UnitIDLot", DataEntityExtend.GetDynamicValue<DynamicObject>(sourceBomChildEntry, "UNITIDLOT", null));
			DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "NumeratorLot", DataEntityExtend.GetDynamicValue<decimal>(sourceBomChildEntry, "NUMERATORLOT", 0m));
			DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "DenominatorLot", DataEntityExtend.GetDynamicValue<decimal>(sourceBomChildEntry, "DENOMINATORLOT", 0m));
			DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "FixScrapQtyLot", DataEntityExtend.GetDynamicValue<decimal>(sourceBomChildEntry, "FIXSCRAPQTYLOT", 0m));
			DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "ScrapraQtyLot", DataEntityExtend.GetDynamicValue<decimal>(sourceBomChildEntry, "SCRAPRATELOT", 0m));
			DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "NoteLot", DataEntityExtend.GetDynamicValue<string>(sourceBomChildEntry, "NOTELOT", null));
			DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "BaseNumeratorLot", DataEntityExtend.GetDynamicValue<decimal>(sourceBomChildEntry, "BASENUMERATORLOT", 0m));
			DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "BaseStartQty", DataEntityExtend.GetDynamicValue<decimal>(sourceBomChildEntry, "BASESTARTQTY", 0m));
			DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "BaseEndQty", DataEntityExtend.GetDynamicValue<decimal>(sourceBomChildEntry, "BASEENDQTY", 0m));
			DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "BaseFixScrapQty", DataEntityExtend.GetDynamicValue<decimal>(sourceBomChildEntry, "BASEFIXSCRAPQTYLOT", 0m));
			DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "BaseUnitIdLot_Id", DataEntityExtend.GetDynamicValue<long>(sourceBomChildEntry, "BASEUNITIDLOT_Id", 0L));
			DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "BaseUnitIdLot", DataEntityExtend.GetDynamicValue<DynamicObject>(sourceBomChildEntry, "BASEUNITIDLOT", null));
			DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "BaseDenominatorLot", DataEntityExtend.GetDynamicValue<decimal>(sourceBomChildEntry, "BASEDENOMINATORLOT", 0m));
			targetCollection.Add(dynamicObject);
		}

		// Token: 0x0400066B RID: 1643
		private Dictionary<long, BaseDataControlPolicyTargetOrgEntry> bomEntryCtrlSettings;

		// Token: 0x0400066C RID: 1644
		private FormMetadata bomMeta;

		// Token: 0x0400066D RID: 1645
		private Entity bomTreeEntity;
	}
}
