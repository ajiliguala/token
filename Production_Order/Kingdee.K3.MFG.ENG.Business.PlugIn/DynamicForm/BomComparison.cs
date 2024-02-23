using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Orm.Metadata.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.BD.ServiceHelper;
using Kingdee.K3.Core.BD;
using Kingdee.K3.Core.BD.ServiceArgs;
using Kingdee.K3.Core.MFG.ENG.BomExpand;
using Kingdee.K3.Core.MFG.ENG.ParamOption;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.Core.MFG.Utils;
using Kingdee.K3.MFG.Common.BusinessEntity.ENG.BomExpand;
using Kingdee.K3.MFG.ServiceHelper;
using Kingdee.K3.MFG.ServiceHelper.ENG;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm
{
	// Token: 0x02000068 RID: 104
	[Description("BOM 对比")]
	public class BomComparison : AbstractDynamicFormPlugIn
	{
		// Token: 0x0600079B RID: 1947 RVA: 0x0005AA38 File Offset: 0x00058C38
		protected void OpetationForIntegrationQuery()
		{
			this.InitializeBomQueryOption();
			this.memBomExpandOption.IsExpandForbidden = true;
			this.memBomExpandOption.ExpandSkipRow = true;
			this.memBomExpandOption.CsdSubstitution = false;
			this.memBomExpandOption.ExpandLevelTo = this.memBomExpandOption.BomMaxLevel;
			this.memBomExpandOption.MaterialNum = 1;
			this.memBomExpandOption.BomExpandCalType = 0;
			this.memBomExpandOption.Mode = 0;
			if (OtherExtend.ConvertTo<bool>(this.View.Model.DataObject["IsOutSourceOlny"], false))
			{
				this.memBomExpandOption.IsShowOutSource = true;
			}
			else
			{
				this.memBomExpandOption.IsShowOutSource = false;
			}
			if (OtherExtend.ConvertTo<bool>(this.View.Model.DataObject["IsDisplayRep"], false))
			{
				this.memBomExpandOption.CsdSubstitution = true;
				this.memBomExpandOption.IsExpandSubMtrl = true;
				return;
			}
			this.memBomExpandOption.CsdSubstitution = false;
			this.memBomExpandOption.IsExpandSubMtrl = false;
		}

		// Token: 0x0600079C RID: 1948 RVA: 0x0005AB38 File Offset: 0x00058D38
		protected void BindIntegrateTempEntities()
		{
			this.firstTempEntities = this.GetIntegrateBomExpandResult(1);
			this.secondTempEntities = this.GetIntegrateBomExpandResult(2);
		}

		// Token: 0x0600079D RID: 1949 RVA: 0x0005AB54 File Offset: 0x00058D54
		protected List<DynamicObject> GetIntegrateBomExpandResult(int bomItem)
		{
			return this.GetBomChildDataForIntegrate(new List<DynamicObject>
			{
				this.BuildBomExpandSourceData(this.View.Model.DataObject, bomItem)
			}, this.memBomExpandOption, bomItem);
		}

		// Token: 0x0600079E RID: 1950 RVA: 0x0005AB94 File Offset: 0x00058D94
		protected List<DynamicObject> GetBomChildDataForIntegrate(List<DynamicObject> lstExpandSource, MemBomExpandOption_ForPSV memBomExpandOption, int bomItem)
		{
			if (OtherExtend.ConvertTo<int>(this.View.Model.DataObject["CompareType"], 0) == 1)
			{
				List<DynamicObject> bomQueryIntegrationResult = BomQueryServiceHelper.GetBomQueryIntegrationResult(base.Context, lstExpandSource, memBomExpandOption);
				return this.BindTempEntityDataForIntegrate(bomQueryIntegrationResult, bomItem);
			}
			return BomQueryServiceHelper.GetBomQuerySingleAndSkipResult(base.Context, lstExpandSource, memBomExpandOption);
		}

		// Token: 0x0600079F RID: 1951 RVA: 0x0005ABEC File Offset: 0x00058DEC
		private List<DynamicObject> BindTempEntityDataForIntegrate(List<DynamicObject> bomExpandResults, int bomItem)
		{
			List<DynamicObject> list = new List<DynamicObject>();
			foreach (DynamicObject dynamicObject in bomExpandResults)
			{
				DynamicObject dynamicObject2 = new DynamicObject(this.View.Model.BusinessInfo.GetEntryEntity("FEntity").DynamicObjectType);
				dynamicObject2["MaterialId_Id"] = OtherExtend.ConvertTo<long>(dynamicObject["MaterialId_Id"], 0L);
				dynamicObject2["MaterialType"] = OtherExtend.ConvertTo<string>(dynamicObject["MaterialType"], null);
				DynamicObject dynamicObject3 = dynamicObject["MaterialId"] as DynamicObject;
				DynamicObject dynamicObject4 = (dynamicObject3["MaterialBase"] as DynamicObjectCollection).FirstOrDefault<DynamicObject>();
				dynamicObject2["BaseUnitId_Id"] = dynamicObject4["BaseUnitId_Id"];
				dynamicObject2[string.Format("BOMID{0}_Id", bomItem)] = OtherExtend.ConvertTo<long>(dynamicObject["BomId_Id"], 0L);
				dynamicObject2["BomEntryId"] = OtherExtend.ConvertTo<long>(dynamicObject["Seq"], 0L);
				dynamicObject2[string.Format("CHILDUNITID{0}_Id", bomItem)] = OtherExtend.ConvertTo<long>(dynamicObject["UnitId_Id"], 0L);
				dynamicObject2[string.Format("NUMERATOR{0}", bomItem)] = OtherExtend.ConvertTo<decimal>(dynamicObject["Qty"], 0m);
				dynamicObject2[string.Format("DENOMINATOR{0}", bomItem)] = 1m;
				dynamicObject2[string.Format("AuxPropId{0}_Id", bomItem)] = OtherExtend.ConvertTo<long>(dynamicObject["AuxPropId_Id"], 0L);
				dynamicObject2[string.Format("Bom{0}Mtrl{1}_Id", bomItem, bomItem)] = OtherExtend.ConvertTo<long>(dynamicObject["MaterialId_Id"], 0L);
				dynamicObject2[string.Format("Bom{0}MtrlType{1}", bomItem, bomItem)] = OtherExtend.ConvertTo<string>(dynamicObject["MaterialType"], null);
				list.Add(dynamicObject2);
			}
			return list;
		}

		// Token: 0x060007A0 RID: 1952 RVA: 0x0005AE90 File Offset: 0x00059090
		protected virtual void ICombineAndBindToEntity(List<DynamicObject> mainObj, List<DynamicObject> subObj)
		{
			HashSet<long> hashSet = new HashSet<long>();
			IGrouping<string, DynamicObject> grouping = null;
			int num = 0;
			Dictionary<string, IGrouping<string, DynamicObject>> dictionary = (from p in subObj
			group p by string.Format("{0}_{1}", DataEntityExtend.GetDynamicValue<string>(DataEntityExtend.GetDynamicValue<DynamicObject>(p, "MaterialId", null), "MsterId", null), p["MaterialType"].ToString())).ToDictionary((IGrouping<string, DynamicObject> x) => x.Key);
			int num2 = mainObj.Count<DynamicObject>();
			for (int i = 0; i < num2; i++)
			{
				if (!dictionary.TryGetValue(string.Format("{0}_{1}", DataEntityExtend.GetDynamicValue<string>(DataEntityExtend.GetDynamicValue<DynamicObject>(mainObj[i], "MaterialId", null), "MsterId", null), mainObj[i]["MaterialType"].ToString()), out grouping))
				{
					num = this.InsertNewEntryRow(num, mainObj[i]);
				}
				else
				{
					foreach (DynamicObject dynamicObject in grouping)
					{
						if (!hashSet.Contains(OtherExtend.ConvertTo<long>(dynamicObject["BomEntryId"], 0L)))
						{
							num = this.InsertNewEntryRow(num, this.DoCombineFields(mainObj[i], dynamicObject, this.entityFields));
							hashSet.Add(OtherExtend.ConvertTo<long>(dynamicObject["BomEntryId"], 0L));
							break;
						}
					}
				}
			}
			this.BatchAddRows(num, subObj, hashSet, false);
		}

		// Token: 0x17000045 RID: 69
		// (get) Token: 0x060007A1 RID: 1953 RVA: 0x0005B004 File Offset: 0x00059204
		// (set) Token: 0x060007A2 RID: 1954 RVA: 0x0005B00C File Offset: 0x0005920C
		protected bool GroupChange
		{
			get
			{
				return this._GroupChange;
			}
			set
			{
				this._GroupChange = value;
			}
		}

		// Token: 0x060007A3 RID: 1955 RVA: 0x0005B015 File Offset: 0x00059215
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
			this.entity = this.View.Model.BillBusinessInfo.GetEntity("FEntity");
		}

		// Token: 0x060007A4 RID: 1956 RVA: 0x0005B03E File Offset: 0x0005923E
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			base.BeforeF7Select(e);
			e.IsShowApproved = false;
			e.IsShowUsed = false;
		}

		// Token: 0x060007A5 RID: 1957 RVA: 0x0005B058 File Offset: 0x00059258
		protected void InitializeBomQueryOption()
		{
			this.memBomExpandOption = new MemBomExpandOption_ForPSV();
			DateTime sysDate = MFGServiceHelper.GetSysDate(base.Context);
			this.memBomExpandOption.ValidDate = new DateTime?(sysDate);
			this.memBomExpandOption.BomExpandId = this.bomExpandGuid;
			this.memBomExpandOption.ParentCsdYieldRate = true;
			this.memBomExpandOption.ChildCsdYieldRate = true;
			this.memBomExpandOption.DeleteSkipRow = false;
			this.memBomExpandOption.IsConvertUnitQty = true;
			this.memBomExpandOption.IsKeepNumeratorUnchange = true;
			this.memBomExpandOption.IsExpandForbidden = true;
			bool value = MFGBillUtil.GetValue<bool>(this.View.Model, "FIsHideOutSourceBOM", -1, false, null);
			if (value)
			{
				this.memBomExpandOption.IsHideOutSourceBOM = true;
			}
		}

		// Token: 0x060007A6 RID: 1958 RVA: 0x0005B110 File Offset: 0x00059310
		public override void ButtonClick(ButtonClickEventArgs e)
		{
			base.ButtonClick(e);
			string a;
			if ((a = e.Key.ToUpper()) != null)
			{
				if (!(a == "FREFRESH"))
				{
					return;
				}
				this.View.Model.DeleteEntryData("FEntity");
				this.lstNeedComparFieldKey = this.GetComparFieldKey();
				this.BeginComparison();
			}
		}

		// Token: 0x060007A7 RID: 1959 RVA: 0x0005B168 File Offset: 0x00059368
		protected void BeginComparison()
		{
			if (OtherExtend.ConvertTo<long>(this.View.Model.DataObject["BillBomId1_Id"], 0L) == 0L || OtherExtend.ConvertTo<long>(this.View.Model.DataObject["BillBomId2_Id"], 0L) == 0L || OtherExtend.ConvertTo<long>(this.View.Model.DataObject["BillBomId1_Id"], 0L) == OtherExtend.ConvertTo<long>(this.View.Model.DataObject["BillBomId2_Id"], 0L))
			{
				this.View.ShowErrMessage(ResManager.LoadKDString("请正确选择物料清单版本！", "015072000018146", 7, new object[0]), "", 0);
				return;
			}
			this.SetExpandOptions();
			this.firstTempEntities = new List<DynamicObject>();
			this.secondTempEntities = new List<DynamicObject>();
			this.colors = new List<KeyValuePair<int, string>>();
			this.GetBomExpandData();
			this.Comparison();
			DynamicObjectCollection entityDataObject = this.Model.GetEntityDataObject(this.Model.BusinessInfo.GetEntity("FEntity"));
			EntryEntity entryEntity = this.Model.BusinessInfo.GetEntryEntity("FEntity");
			DBServiceHelper.LoadReferenceObject(base.Context, entityDataObject.ToArray<DynamicObject>(), entryEntity.DynamicObjectType, false);
			this.RemoveDiffRow();
			this.View.UpdateView("FEntity");
			this.SetEntityColor("FEntity");
		}

		// Token: 0x060007A8 RID: 1960 RVA: 0x0005B2CE File Offset: 0x000594CE
		public override void AfterBindData(EventArgs e)
		{
			base.AfterBindData(e);
			this.SetEntityColor();
		}

		// Token: 0x060007A9 RID: 1961 RVA: 0x0005B2DD File Offset: 0x000594DD
		public override void EntryBarItemClick(BarItemClickEventArgs e)
		{
			base.EntryBarItemClick(e);
			this.SetEntityColor();
		}

		// Token: 0x060007AA RID: 1962 RVA: 0x0005B2EC File Offset: 0x000594EC
		public override void EntityRowClick(EntityRowClickEventArgs e)
		{
			base.EntityRowClick(e);
			this.SetEntityColor();
		}

		// Token: 0x060007AB RID: 1963 RVA: 0x0005B2FB File Offset: 0x000594FB
		public override void BeforeClosed(BeforeClosedEventArgs e)
		{
			base.BeforeClosed(e);
			BomExpandServiceHelper.ClearBomExpandResult(base.Context, this.memBomExpandOption);
		}

		// Token: 0x060007AC RID: 1964 RVA: 0x0005B318 File Offset: 0x00059518
		public override void BeforeDoOperation(BeforeDoOperationEventArgs e)
		{
			base.BeforeDoOperation(e);
			if ((e.Operation.FormOperation.IsEqualOperation("Print") || e.Operation.FormOperation.IsEqualOperation("EntityExport")) && !MFGCommonUtil.AuthPermissionBeforeShowF7Form(this.View, this.View.BillBusinessInfo.GetForm().Id, e.Operation.FormOperation.PermissionItemId))
			{
				this.View.ShowErrMessage(string.Format(ResManager.LoadKDString("没有物料清单对比的{0}权限", "015072000019374", 7, new object[0]), e.Operation.FormOperation.OperationName[base.Context.UserLocale.LCID]), "", 0);
				e.Cancel = true;
			}
		}

		// Token: 0x060007AD RID: 1965 RVA: 0x0005B3EC File Offset: 0x000595EC
		protected void SetExpandOptions()
		{
			switch (OtherExtend.ConvertTo<int>(this.View.Model.DataObject["CompareType"], 0))
			{
			case 0:
				this.OpetationForSingleQuery();
				return;
			case 1:
				BomExpandServiceHelper.ClearBomExpandResult(base.Context, this.memBomExpandOption);
				this.OpetationForIntegrationQuery();
				return;
			case 2:
				BomExpandServiceHelper.ClearBomExpandResult(base.Context, this.memBomExpandOption);
				this.OpetationForSingleAndShipQuery();
				return;
			default:
				return;
			}
		}

		// Token: 0x060007AE RID: 1966 RVA: 0x0005B464 File Offset: 0x00059664
		protected void RemoveDiffRow()
		{
			bool dynamicValue = DataEntityExtend.GetDynamicValue<bool>(this.View.Model.DataObject, "IsDifferences", false);
			if (dynamicValue)
			{
				DynamicObjectCollection entityDataObject = this.Model.GetEntityDataObject(this.Model.BusinessInfo.GetEntity("FEntity"));
				this.Model.BusinessInfo.GetEntryEntity("FEntity");
				List<DynamicObject> list = new List<DynamicObject>();
				for (int i = 0; i < entityDataObject.Count; i++)
				{
					List<KeyValuePair<string, string>> list2 = new List<KeyValuePair<string, string>>();
					foreach (KeyValuePair<string, string> keyValuePair in this.lstNeedComparFieldKey)
					{
						if (!this.CompareFieldVale(i, keyValuePair))
						{
							list2.Add(keyValuePair);
						}
					}
					if (ListUtils.IsEmpty<KeyValuePair<string, string>>(list2))
					{
						list.Add(entityDataObject[i]);
					}
				}
				if (!ListUtils.IsEmpty<DynamicObject>(list))
				{
					foreach (DynamicObject item in list)
					{
						this.View.Model.DeleteEntryRow("FEntity", entityDataObject.IndexOf(item));
					}
				}
			}
		}

		// Token: 0x060007AF RID: 1967 RVA: 0x0005B5B0 File Offset: 0x000597B0
		protected void OpetationForSingleAndShipQuery()
		{
			this.InitializeBomQueryOption();
			this.memBomExpandOption.IsExpandForbidden = true;
			this.memBomExpandOption.ExpandSkipRow = true;
			this.memBomExpandOption.ExpandLevelTo = 1;
			this.memBomExpandOption.MaterialNum = 1;
			this.memBomExpandOption.DeleteSkipRow = true;
			this.memBomExpandOption.BomExpandCalType = 0;
			this.memBomExpandOption.Mode = 0;
			this.memBomExpandOption.IsShowOutSource = false;
			if (OtherExtend.ConvertTo<bool>(this.View.Model.DataObject["IsDisplayRep"], false))
			{
				this.memBomExpandOption.CsdSubstitution = true;
				this.memBomExpandOption.IsExpandSubMtrl = true;
				return;
			}
			this.memBomExpandOption.CsdSubstitution = false;
			this.memBomExpandOption.IsExpandSubMtrl = false;
		}

		// Token: 0x060007B0 RID: 1968 RVA: 0x0005B678 File Offset: 0x00059878
		protected void OpetationForSingleQuery()
		{
			this.InitializeBomQueryOption();
			this.memBomExpandOption.Mode = 0;
			this.memBomExpandOption.ExpandLevelTo = 1;
			this.memBomExpandOption.BomMaxLevel = 1;
			this.memBomExpandOption.ExpandVirtualMaterial = false;
			this.memBomExpandOption.DeleteVirtualMaterial = false;
			this.memBomExpandOption.ExpandSkipRow = false;
			this.memBomExpandOption.IsShowOutSource = false;
			this.memBomExpandOption.BomExpandCalType = 0;
			if (OtherExtend.ConvertTo<bool>(this.View.Model.DataObject["IsDisplayRep"], false))
			{
				this.memBomExpandOption.CsdSubstitution = true;
				return;
			}
			this.memBomExpandOption.CsdSubstitution = false;
		}

		// Token: 0x060007B1 RID: 1969 RVA: 0x0005B726 File Offset: 0x00059926
		private int InsertNewEntryRow(int rowNum, DynamicObject rowData)
		{
			this.View.Model.CreateNewEntryRow(this.entity, rowNum, rowData);
			if (this.GroupChange)
			{
				this.colors.Add(new KeyValuePair<int, string>(rowNum, "#99FFFF"));
			}
			return rowNum + 1;
		}

		// Token: 0x060007B2 RID: 1970 RVA: 0x0005B761 File Offset: 0x00059961
		private void SetEntityColor()
		{
		}

		// Token: 0x060007B3 RID: 1971 RVA: 0x0005B764 File Offset: 0x00059964
		private void SetEntityColor(string entityKey)
		{
			Dictionary<KeyValuePair<string, string>, List<KeyValuePair<int, string>>> dictionary = new Dictionary<KeyValuePair<string, string>, List<KeyValuePair<int, string>>>();
			List<KeyValuePair<int, string>> list = new List<KeyValuePair<int, string>>();
			List<KeyValuePair<int, string>> list2 = new List<KeyValuePair<int, string>>();
			bool flag = false;
			int entryRowCount = this.View.Model.GetEntryRowCount(entityKey);
			for (int i = 0; i < entryRowCount; i++)
			{
				foreach (KeyValuePair<string, string> keyValuePair in this.lstNeedComparFieldKey)
				{
					if (!this.CompareFieldVale(i, keyValuePair))
					{
						if (!dictionary.TryGetValue(keyValuePair, out list))
						{
							list = new List<KeyValuePair<int, string>>();
							dictionary[keyValuePair] = list;
						}
						list.Add(this.CreateRowColor(i, "#E6B8B7"));
						flag = true;
					}
				}
				if (flag)
				{
					list2.Add(this.CreateRowColor(i, "#E6B8B7"));
				}
				flag = false;
			}
			if (!ListUtils.IsEmpty<KeyValuePair<KeyValuePair<string, string>, List<KeyValuePair<int, string>>>>(dictionary))
			{
				foreach (KeyValuePair<KeyValuePair<string, string>, List<KeyValuePair<int, string>>> keyValuePair2 in dictionary)
				{
					this.View.GetControl<EntryGrid>("FEntity").SetCellsBackcolor(keyValuePair2.Key.Key, keyValuePair2.Value);
					this.View.GetControl<EntryGrid>("FEntity").SetCellsBackcolor(keyValuePair2.Key.Value, keyValuePair2.Value);
				}
				this.View.GetControl<EntryGrid>("FEntity").SetCellsBackcolor("FMaterialId", list2);
				this.View.GetControl<EntryGrid>("FEntity").SetCellsBackcolor("FMaterialName", list2);
				this.View.GetControl<EntryGrid>("FEntity").SetCellsBackcolor("FMaterialMode", list2);
			}
		}

		// Token: 0x060007B4 RID: 1972 RVA: 0x0005B944 File Offset: 0x00059B44
		private bool CompareFieldVale(int rowIndex, KeyValuePair<string, string> compareField)
		{
			object value = this.Model.GetValue(compareField.Key, rowIndex);
			object value2 = this.Model.GetValue(compareField.Value, rowIndex);
			bool result;
			if (value is DynamicObject && value2 is DynamicObject)
			{
				string text = "Id";
				if ((value as DynamicObject).DynamicObjectType.Properties.ToArray().Any((DynamicProperty f) => StringUtils.EqualsIgnoreCase(f.Name, "MsterId")))
				{
					text = "MsterId";
				}
				result = (DataEntityExtend.GetDynamicValue<string>(value as DynamicObject, text, null) == DataEntityExtend.GetDynamicValue<string>(value2 as DynamicObject, text, null));
			}
			else
			{
				result = object.Equals(value, value2);
			}
			return result;
		}

		// Token: 0x060007B5 RID: 1973 RVA: 0x0005B9FC File Offset: 0x00059BFC
		protected virtual List<KeyValuePair<string, string>> GetComparFieldKey()
		{
			List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
			return this.GetUserParamSettingField();
		}

		// Token: 0x060007B6 RID: 1974 RVA: 0x0005BA18 File Offset: 0x00059C18
		private List<KeyValuePair<string, string>> GetUserParamSettingField()
		{
			List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
			FormMetadata formMetadata = (FormMetadata)MetaDataServiceHelper.Load(this.View.Context, "ENG_BomComparisonParam", true);
			DynamicObject dynamicObject = UserParamterServiceHelper.Load(this.View.Context, formMetadata.BusinessInfo, this.View.Context.UserId, "ENG_BomComparison", "UserParameter");
			if (!ObjectUtils.IsNullOrEmpty(dynamicObject))
			{
				bool dynamicValue = DataEntityExtend.GetDynamicValue<bool>(dynamicObject, "MaterialType", false);
				bool dynamicValue2 = DataEntityExtend.GetDynamicValue<bool>(dynamicObject, "AuxPropId", false);
				bool dynamicValue3 = DataEntityExtend.GetDynamicValue<bool>(dynamicObject, "BomId", false);
				bool dynamicValue4 = DataEntityExtend.GetDynamicValue<bool>(dynamicObject, "OwnerId", false);
				bool dynamicValue5 = DataEntityExtend.GetDynamicValue<bool>(dynamicObject, "ChildSupplyOrgId", false);
				bool dynamicValue6 = DataEntityExtend.GetDynamicValue<bool>(dynamicObject, "IssueType", false);
				bool dynamicValue7 = DataEntityExtend.GetDynamicValue<bool>(dynamicObject, "SupplyOrg", false);
				bool dynamicValue8 = DataEntityExtend.GetDynamicValue<bool>(dynamicObject, "StockId", false);
				bool dynamicValue9 = DataEntityExtend.GetDynamicValue<bool>(dynamicObject, "OverControlMode", false);
				bool dynamicValue10 = DataEntityExtend.GetDynamicValue<bool>(dynamicObject, "IsGetScrap", false);
				bool dynamicValue11 = DataEntityExtend.GetDynamicValue<bool>(dynamicObject, "IsKeyComponent", false);
				bool dynamicValue12 = DataEntityExtend.GetDynamicValue<bool>(dynamicObject, "ProcessId", false);
				bool dynamicValue13 = DataEntityExtend.GetDynamicValue<bool>(dynamicObject, "PositionNo", false);
				bool dynamicValue14 = DataEntityExtend.GetDynamicValue<bool>(dynamicObject, "EffectDate", false);
				bool dynamicValue15 = DataEntityExtend.GetDynamicValue<bool>(dynamicObject, "ExpireDate", false);
				bool dynamicValue16 = DataEntityExtend.GetDynamicValue<bool>(dynamicObject, "CurrencyId", false);
				bool dynamicValue17 = DataEntityExtend.GetDynamicValue<bool>(dynamicObject, "RefCost", false);
				bool dynamicValue18 = DataEntityExtend.GetDynamicValue<bool>(dynamicObject, "ChildUnitId", false);
				bool dynamicValue19 = DataEntityExtend.GetDynamicValue<bool>(dynamicObject, "Numerator", false);
				bool dynamicValue20 = DataEntityExtend.GetDynamicValue<bool>(dynamicObject, "Denominator", false);
				bool dynamicValue21 = DataEntityExtend.GetDynamicValue<bool>(dynamicObject, "Scraprate", false);
				if (!dynamicValue)
				{
					list.Add(new KeyValuePair<string, string>("FBom1MtrlType1", "FBom2MtrlType2"));
				}
				if (!dynamicValue2)
				{
					list.Add(new KeyValuePair<string, string>("FAuxPropId1", "FAuxPropId2"));
				}
				if (!dynamicValue3)
				{
					list.Add(new KeyValuePair<string, string>("FBOMID1", "FBOMID2"));
				}
				if (!dynamicValue4)
				{
					list.Add(new KeyValuePair<string, string>("FOWNERID1", "FOWNERID2"));
				}
				if (!dynamicValue5)
				{
					list.Add(new KeyValuePair<string, string>("FChildSupplyOrgId1", "FChildSupplyOrgId2"));
				}
				if (!dynamicValue6)
				{
					list.Add(new KeyValuePair<string, string>("FISSUETYPE1", "FISSUETYPE2"));
				}
				if (!dynamicValue7)
				{
					list.Add(new KeyValuePair<string, string>("FSUPPLYORG1", "FSUPPLYORG2"));
				}
				if (!dynamicValue8)
				{
					list.Add(new KeyValuePair<string, string>("FSTOCKID1", "FSTOCKID2"));
				}
				if (!dynamicValue9)
				{
					list.Add(new KeyValuePair<string, string>("FOverControlMode1", "FOverControlMode2"));
				}
				if (!dynamicValue10)
				{
					list.Add(new KeyValuePair<string, string>("FISGETSCRAP1", "FISGETSCRAP2"));
				}
				if (!dynamicValue11)
				{
					list.Add(new KeyValuePair<string, string>("FISKEYCOMPONENT1", "FISKEYCOMPONENT2"));
				}
				if (!dynamicValue12)
				{
					list.Add(new KeyValuePair<string, string>("FPROCESSID1", "FPROCESSID2"));
				}
				if (!dynamicValue13)
				{
					list.Add(new KeyValuePair<string, string>("FPOSITIONNO1", "FPOSITIONNO2"));
				}
				if (!dynamicValue14)
				{
					list.Add(new KeyValuePair<string, string>("FEFFECTDATE1", "FEFFECTDATE2"));
				}
				if (!dynamicValue15)
				{
					list.Add(new KeyValuePair<string, string>("FEXPIREDATE1", "FEXPIREDATE2"));
				}
				if (!dynamicValue16)
				{
					list.Add(new KeyValuePair<string, string>("FB1CurrencyId", "FB2CurrencyId"));
				}
				if (!dynamicValue17)
				{
					list.Add(new KeyValuePair<string, string>("FB1RefCost", "FB2RefCost"));
				}
				if (!dynamicValue18)
				{
					list.Add(new KeyValuePair<string, string>("FCHILDUNITID1", "FCHILDUNITID2"));
				}
				if (!dynamicValue19)
				{
					list.Add(new KeyValuePair<string, string>("FNUMERATOR1", "FNUMERATOR2"));
				}
				if (!dynamicValue20)
				{
					list.Add(new KeyValuePair<string, string>("FDENOMINATOR1", "FDENOMINATOR2"));
				}
				if (!dynamicValue21)
				{
					list.Add(new KeyValuePair<string, string>("FSCRAPRATE1", "FSCRAPRATE2"));
				}
			}
			return list;
		}

		// Token: 0x060007B7 RID: 1975 RVA: 0x0005BDB8 File Offset: 0x00059FB8
		private KeyValuePair<int, string> CreateRowColor(int rowIndex, string rowColor)
		{
			KeyValuePair<int, string> result = new KeyValuePair<int, string>(rowIndex, rowColor);
			return result;
		}

		// Token: 0x060007B8 RID: 1976 RVA: 0x0005BDD0 File Offset: 0x00059FD0
		protected void GetBomExpandData()
		{
			if (OtherExtend.ConvertTo<int>(this.View.Model.DataObject["CompareType"], 0) == 0)
			{
				List<BomExpandView.BomExpandResult> bomChildDataForSingle = this.GetBomChildDataForSingle(new List<DynamicObject>
				{
					this.BuildBomExpandSourceData(this.View.Model.DataObject, 1),
					this.BuildBomExpandSourceData(this.View.Model.DataObject, 2)
				}, this.memBomExpandOption);
				this.BindBomExpandResult(bomChildDataForSingle, 1);
				this.BindBomExpandResult(bomChildDataForSingle, 2);
				this.BindSingleTempEntityData(this.firstBomItem, 1);
				this.BindSingleTempEntityData(this.secondBomItem, 2);
			}
			else if (OtherExtend.ConvertTo<int>(this.View.Model.DataObject["CompareType"], 0) == 1)
			{
				this.BindIntegrateTempEntities();
			}
			else if (OtherExtend.ConvertTo<int>(this.View.Model.DataObject["CompareType"], 0) == 2)
			{
				List<DynamicObject> integrateBomExpandResult = this.GetIntegrateBomExpandResult(1);
				List<DynamicObject> integrateBomExpandResult2 = this.GetIntegrateBomExpandResult(2);
				this.BindSingleAndShipTempEntityData(integrateBomExpandResult, 1);
				this.BindSingleAndShipTempEntityData(integrateBomExpandResult2, 2);
			}
			if (!ListUtils.IsEmpty<DynamicObject>(this.firstTempEntities))
			{
				DBServiceHelper.LoadReferenceObject(this.View.Context, this.firstTempEntities.ToArray(), this.firstTempEntities.First<DynamicObject>().DynamicObjectType, false);
			}
			if (!ListUtils.IsEmpty<DynamicObject>(this.secondTempEntities))
			{
				DBServiceHelper.LoadReferenceObject(this.View.Context, this.secondTempEntities.ToArray(), this.secondTempEntities.First<DynamicObject>().DynamicObjectType, false);
			}
		}

		// Token: 0x060007B9 RID: 1977 RVA: 0x0005BFB8 File Offset: 0x0005A1B8
		protected void BindBomExpandResult(List<BomExpandView.BomExpandResult> expandResults, int bomItem)
		{
			DynamicObject dynamicObject = this.View.Model.DataObject["BillBomId" + bomItem] as DynamicObject;
			long bomId = Convert.ToInt64(dynamicObject["Id"]);
			string entryId = (from m in expandResults
			where m.BomId_Id == bomId
			select m into n
			select n.TopEntryId).FirstOrDefault<string>().ToString();
			if (bomItem == 1)
			{
				this.firstBomItem = (from m in expandResults
				where m.BomLevel == 1L && m.TopEntryId == entryId
				select m).ToList<BomExpandView.BomExpandResult>();
				return;
			}
			this.secondBomItem = (from m in expandResults
			where m.BomLevel == 1L && m.TopEntryId == entryId
			select m).ToList<BomExpandView.BomExpandResult>();
		}

		// Token: 0x060007BA RID: 1978 RVA: 0x0005C0A0 File Offset: 0x0005A2A0
		protected DynamicObject BuildBomExpandSourceData(DynamicObject BillDataObject, int bomItem)
		{
			DynamicObject dynamicObject = BillDataObject["BillBomId" + bomItem] as DynamicObject;
			if (dynamicObject == null)
			{
				return null;
			}
			DynamicObject dynamicObject2 = dynamicObject["MaterialID"] as DynamicObject;
			long materialId = Convert.ToInt64(dynamicObject2["Id"]);
			DateTime sysDate = MFGServiceHelper.GetSysDate(base.Context);
			long bomId_Id = Convert.ToInt64(dynamicObject["Id"]);
			long num = Convert.ToInt64(dynamicObject["FUNITID_Id"]);
			long num2 = Convert.ToInt64(dynamicObject["BaseUnitId_Id"]);
			long supplyOrgId_Id = Convert.ToInt64(dynamicObject["UseOrgId_Id"]);
			BomForwardSourceDynamicRow bomForwardSourceDynamicRow = BomForwardSourceDynamicRow.CreateInstance();
			decimal num3 = 1m;
			UnitConvert unitConvertRate = UnitConvertServiceHelper.GetUnitConvertRate(this.View.Context, new GetUnitConvertRateArgs
			{
				MaterialId = materialId,
				MasterId = DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObject2, "msterID", 0L),
				SourceUnitId = num,
				DestUnitId = num2
			});
			if (unitConvertRate != null)
			{
				num3 = unitConvertRate.ConvertQty(num3, "");
			}
			bomForwardSourceDynamicRow.MaterialId_Id = OtherExtend.ConvertTo<long>(dynamicObject["MaterialID_Id"], 0L);
			bomForwardSourceDynamicRow.BomId_Id = bomId_Id;
			bomForwardSourceDynamicRow.NeedQty = num3;
			bomForwardSourceDynamicRow.NeedDate = new DateTime?(sysDate);
			bomForwardSourceDynamicRow.UnitId_Id = num;
			bomForwardSourceDynamicRow.BaseUnitId_Id = num2;
			bomForwardSourceDynamicRow.SupplyOrgId_Id = supplyOrgId_Id;
			bomForwardSourceDynamicRow.TimeUnit = 1.ToString();
			return bomForwardSourceDynamicRow.DataEntity;
		}

		// Token: 0x060007BB RID: 1979 RVA: 0x0005C220 File Offset: 0x0005A420
		protected List<BomExpandView.BomExpandResult> GetBomChildDataForSingle(List<DynamicObject> lstExpandSource, MemBomExpandOption_ForPSV memBomExpandOption)
		{
			memBomExpandOption.IsConvertUnitQty = true;
			BomExpandView bomExpandView = BomExpandServiceHelper.ExpandBomForwardMen(base.Context, lstExpandSource, memBomExpandOption);
			return bomExpandView.BomExpandResultList.ToList<BomExpandView.BomExpandResult>();
		}

		// Token: 0x060007BC RID: 1980 RVA: 0x0005C2B8 File Offset: 0x0005A4B8
		private void BindSingleTempEntityData(List<BomExpandView.BomExpandResult> bomExpandResults, int bomItem)
		{
			DynamicObject tempEntityRow = null;
			if (ListUtils.IsEmpty<BomExpandView.BomExpandResult>(bomExpandResults))
			{
				return;
			}
			DynamicObject bomCompaFieldMapping = MFGServiceHelper.GetFieldMapping(base.Context, (bomItem == 1) ? "ENG_BOMCOMPA_FIRST" : "ENG_BOMCOMPA_SECOND");
			bomExpandResults.ForEach(delegate(BomExpandView.BomExpandResult bomExpandResult)
			{
				tempEntityRow = new DynamicObject(this.View.Model.BusinessInfo.GetEntryEntity("FEntity").DynamicObjectType);
				this.AddExpandResToEntrys(tempEntityRow, bomExpandResult, bomCompaFieldMapping, bomItem);
			});
		}

		// Token: 0x060007BD RID: 1981 RVA: 0x0005C38C File Offset: 0x0005A58C
		private void BindSingleAndShipTempEntityData(List<DynamicObject> bomExpandResults, int bomItem)
		{
			DynamicObject tempEntityRow = null;
			if (ListUtils.IsEmpty<DynamicObject>(bomExpandResults))
			{
				return;
			}
			DynamicObject bomCompaFieldMapping = MFGServiceHelper.GetFieldMapping(base.Context, (bomItem == 1) ? "ENG_BOMCOMPA_FIRST" : "ENG_BOMCOMPA_SECOND");
			bomExpandResults.ForEach(delegate(DynamicObject bomExpandResult)
			{
				tempEntityRow = new DynamicObject(this.View.Model.BusinessInfo.GetEntryEntity("FEntity").DynamicObjectType);
				this.AddExpandResToEntrys(tempEntityRow, bomExpandResult, bomCompaFieldMapping, bomItem);
			});
		}

		// Token: 0x060007BE RID: 1982 RVA: 0x0005C3F5 File Offset: 0x0005A5F5
		private void AddExpandResToEntrys(DynamicObject entityRow, BomExpandView.BomExpandResult bomExpandResult, DynamicObject billLink, int bomItem)
		{
			MFGServiceHelper.DoFieldMapping(base.Context, billLink, entityRow, bomExpandResult.DataEntity);
			if (bomItem == 1)
			{
				this.firstTempEntities.Add(entityRow);
				return;
			}
			this.secondTempEntities.Add(entityRow);
		}

		// Token: 0x060007BF RID: 1983 RVA: 0x0005C42C File Offset: 0x0005A62C
		protected void Comparison()
		{
			this.GetEntityFields(this.firstTempEntities, this.secondTempEntities);
			if (OtherExtend.ConvertTo<int>(this.View.Model.DataObject["CompareType"], 0) == 0)
			{
				if (this.firstTempEntities.Count >= this.secondTempEntities.Count)
				{
					this.SCombineAndBindToEntity(this.secondTempEntities, this.firstTempEntities);
					return;
				}
				this.SCombineAndBindToEntity(this.firstTempEntities, this.secondTempEntities);
				return;
			}
			else
			{
				if (this.firstTempEntities.Count >= this.secondTempEntities.Count)
				{
					this.ICombineAndBindToEntity(this.secondTempEntities, this.firstTempEntities);
					return;
				}
				this.ICombineAndBindToEntity(this.firstTempEntities, this.secondTempEntities);
				return;
			}
		}

		// Token: 0x060007C0 RID: 1984 RVA: 0x0005C5E8 File Offset: 0x0005A7E8
		protected void GetEntityFields(List<DynamicObject> firstTempEntities, List<DynamicObject> secondTempEntities)
		{
			if (firstTempEntities.Count >= secondTempEntities.Count)
			{
				this.entityFields.Clear();
				this.entityFields = (from m in this.View.Model.BusinessInfo.GetEntryEntity("FEntity").Fields
				where m.PropertyName.EndsWith("1") && !(m is BasePropertyField)
				select m).Select(delegate(Field field)
				{
					if (field is BaseDataField || field is OrgField || field is UnitField || field is BaseUnitField || field is ItemClassField || field is AssistantField || field is RelatedFlexGroupField)
					{
						return field.PropertyName + "_Id";
					}
					return field.PropertyName;
				}).ToList<string>();
				return;
			}
			this.entityFields.Clear();
			this.entityFields = (from m in this.View.Model.BusinessInfo.GetEntryEntity("FEntity").Fields
			where m.PropertyName.EndsWith("2") && !(m is BasePropertyField)
			select m).Select(delegate(Field field)
			{
				if (field is BaseDataField || field is OrgField || field is UnitField || field is BaseUnitField || field is ItemClassField || field is AssistantField || field is RelatedFlexGroupField)
				{
					return field.PropertyName + "_Id";
				}
				return field.PropertyName;
			}).ToList<string>();
		}

		// Token: 0x060007C1 RID: 1985 RVA: 0x0005C834 File Offset: 0x0005AA34
		protected void SCombineAndBindToEntity(List<DynamicObject> mainObj, List<DynamicObject> subObj)
		{
			Dictionary<long, long> dictionary = new Dictionary<long, long>();
			int num = mainObj.Count<DynamicObject>();
			int num2 = 0;
			long num3 = -1L;
			HashSet<long> hashSet = new HashSet<long>();
			bool flag = false;
			bool flag2 = false;
			Dictionary<long, IGrouping<long, DynamicObject>> dictionary2 = (from m in subObj
			where m["MaterialType"].ToString() == "3"
			select m into p
			group p by OtherExtend.ConvertTo<long>(p["ReplaceGroup"], 0L)).ToDictionary((IGrouping<long, DynamicObject> x) => x.Key);
			Dictionary<long, IGrouping<long, DynamicObject>> dictionary3 = (from m in subObj
			where !string.IsNullOrWhiteSpace(m["ReplacePolicy"].ToString()) && m["MaterialType"].ToString() == "1" && !OtherExtend.ConvertTo<bool>(m["IsSubsKeyItem"], false)
			select m into p
			group p by OtherExtend.ConvertTo<long>(p["ReplaceGroup"], 0L)).ToDictionary((IGrouping<long, DynamicObject> x) => x.Key);
			Dictionary<string, IGrouping<string, DynamicObject>> dictSubObj = (from m in subObj
			where string.IsNullOrWhiteSpace(m["ReplacePolicy"].ToString()) || (m["MaterialType"].ToString() == "1" && OtherExtend.ConvertTo<bool>(m["IsSubsKeyItem"], false))
			select m into p
			group p by string.Format("{0}_{1}", DataEntityExtend.GetDynamicValue<string>(DataEntityExtend.GetDynamicValue<DynamicObject>(p, "MaterialId", null), "MsterId", null), p["MaterialType"].ToString())).ToDictionary((IGrouping<string, DynamicObject> x) => x.Key);
			IGrouping<long, DynamicObject> source = null;
			for (int i = 0; i < num; i++)
			{
				bool flag3 = i == num - 1 || OtherExtend.ConvertTo<long>(mainObj[i]["ReplaceGroup"], 0L) != OtherExtend.ConvertTo<long>(mainObj[i + 1]["ReplaceGroup"], 0L);
				if (!dictionary.TryGetValue(OtherExtend.ConvertTo<long>(mainObj[i]["ReplaceGroup"], 0L), out num3))
				{
					if (this.GroupChange)
					{
						this.GroupChange = false;
					}
					else
					{
						this.GroupChange = true;
					}
					flag2 = false;
					flag = this.CombineStandardMain(mainObj[i], dictSubObj, hashSet, ref num2, out num3);
					dictionary.Add(OtherExtend.ConvertTo<long>(mainObj[i]["ReplaceGroup"], 0L), num3);
					if (flag3 && flag && dictionary3.TryGetValue(num3, out source))
					{
						num2 = this.BatchAddRows(num2, source.ToList<DynamicObject>(), hashSet, true);
					}
					if (flag3 && flag && dictionary2.TryGetValue(num3, out source))
					{
						num2 = this.BatchAddRows(num2, source.ToList<DynamicObject>(), hashSet, true);
					}
					if (flag3)
					{
						flag = false;
					}
				}
				else if (flag && mainObj[i]["MaterialType"].ToString() == "3")
				{
					if (!flag2 && dictionary3.TryGetValue(num3, out source))
					{
						num2 = this.BatchAddRows(num2, source.ToList<DynamicObject>(), hashSet, true);
					}
					this.CombineSubMatchObj(mainObj[i], dictionary2, hashSet, ref num2, num3, flag3);
				}
				else if (flag)
				{
					bool tailMark = flag3 || mainObj[i]["MaterialType"].ToString() != mainObj[i + 1]["MaterialType"].ToString();
					flag2 = this.CombineSubMatchObj(mainObj[i], dictionary3, hashSet, ref num2, num3, tailMark);
				}
				else
				{
					num2 = this.InsertNewEntryRow(num2, mainObj[i]);
					hashSet.Add(OtherExtend.ConvertTo<long>(mainObj[i]["BomEntryId"], 0L));
				}
			}
			if (this.GroupChange)
			{
				this.GroupChange = false;
			}
			else
			{
				this.GroupChange = true;
			}
			this.BatchAddRows(num2, subObj, hashSet, false);
			this.DealMissRows(num2, mainObj, hashSet);
		}

		// Token: 0x060007C2 RID: 1986 RVA: 0x0005CBFC File Offset: 0x0005ADFC
		protected bool CombineSubMatchObj(DynamicObject curMainObj, Dictionary<long, IGrouping<long, DynamicObject>> dictRepGroupObj, HashSet<long> addRecord, ref int rowCount, long replaceGroup, bool tailMark)
		{
			bool flag = false;
			IGrouping<long, DynamicObject> grouping = null;
			if (!dictRepGroupObj.TryGetValue(replaceGroup, out grouping))
			{
				rowCount = this.InsertNewEntryRow(rowCount, curMainObj);
				addRecord.Add(OtherExtend.ConvertTo<long>(curMainObj["BomEntryId"], 0L));
				return true;
			}
			foreach (DynamicObject dynamicObject in grouping)
			{
				long dynamicValue = DataEntityExtend.GetDynamicValue<long>(DataEntityExtend.GetDynamicValue<DynamicObject>(curMainObj, "MaterialId", null), "MsterId", 0L);
				long dynamicValue2 = DataEntityExtend.GetDynamicValue<long>(DataEntityExtend.GetDynamicValue<DynamicObject>(dynamicObject, "MaterialId", null), "MsterId", 0L);
				if (!addRecord.Contains(OtherExtend.ConvertTo<long>(dynamicObject["BomEntryId"], 0L)) && dynamicValue == dynamicValue2)
				{
					flag = true;
					rowCount = this.InsertNewEntryRow(rowCount, this.DoCombineFields(curMainObj, dynamicObject, this.entityFields));
					addRecord.Add(OtherExtend.ConvertTo<long>(dynamicObject["BomEntryId"], 0L));
					addRecord.Add(OtherExtend.ConvertTo<long>(curMainObj["BomEntryId"], 0L));
					break;
				}
			}
			if (!flag)
			{
				rowCount = this.InsertNewEntryRow(rowCount, curMainObj);
				addRecord.Add(OtherExtend.ConvertTo<long>(curMainObj["BomEntryId"], 0L));
			}
			if (tailMark)
			{
				rowCount = this.BatchAddRows(rowCount, grouping.ToList<DynamicObject>(), addRecord, true);
			}
			return true;
		}

		// Token: 0x060007C3 RID: 1987 RVA: 0x0005CD64 File Offset: 0x0005AF64
		protected bool CombineStandardMain(DynamicObject curMainObj, Dictionary<string, IGrouping<string, DynamicObject>> dictSubObj, HashSet<long> addRecord, ref int rowCount, out long replaceGroup)
		{
			replaceGroup = -1L;
			bool flag = false;
			IGrouping<string, DynamicObject> grouping;
			if (!dictSubObj.TryGetValue(string.Format("{0}_{1}", DataEntityExtend.GetDynamicValue<string>(DataEntityExtend.GetDynamicValue<DynamicObject>(curMainObj, "MaterialId", null), "MsterId", null), curMainObj["MaterialType"].ToString()), out grouping))
			{
				rowCount = this.InsertNewEntryRow(rowCount, curMainObj);
				addRecord.Add(OtherExtend.ConvertTo<long>(curMainObj["BomEntryId"], 0L));
				return false;
			}
			foreach (DynamicObject dynamicObject in grouping)
			{
				if (!addRecord.Contains(OtherExtend.ConvertTo<long>(dynamicObject["BomEntryId"], 0L)))
				{
					rowCount = this.InsertNewEntryRow(rowCount, this.DoCombineFields(curMainObj, dynamicObject, this.entityFields));
					addRecord.Add(OtherExtend.ConvertTo<long>(dynamicObject["BomEntryId"], 0L));
					addRecord.Add(OtherExtend.ConvertTo<long>(curMainObj["BomEntryId"], 0L));
					replaceGroup = OtherExtend.ConvertTo<long>(dynamicObject["ReplaceGroup"], 0L);
					flag = true;
					break;
				}
			}
			return flag && !string.IsNullOrWhiteSpace(curMainObj["ReplacePolicy"].ToString());
		}

		// Token: 0x060007C4 RID: 1988 RVA: 0x0005CF20 File Offset: 0x0005B120
		protected int DealMissRows(int rowCount, List<DynamicObject> mainObj, HashSet<long> addRecord)
		{
			mainObj.ForEach(delegate(DynamicObject item)
			{
				if (!addRecord.Contains(OtherExtend.ConvertTo<long>(item["BomEntryId"], 0L)))
				{
					rowCount = this.InsertNewEntryRow(rowCount, item);
					addRecord.Add(OtherExtend.ConvertTo<long>(item["BomEntryId"], 0L));
				}
			});
			return rowCount;
		}

		// Token: 0x060007C5 RID: 1989 RVA: 0x0005CFD4 File Offset: 0x0005B1D4
		protected int BatchAddRows(int rowCount, List<DynamicObject> loopObj, HashSet<long> addRecord, bool isRecord = true)
		{
			loopObj.ForEach(delegate(DynamicObject item)
			{
				if (!addRecord.Contains(OtherExtend.ConvertTo<long>(item["BomEntryId"], 0L)))
				{
					rowCount = this.InsertNewEntryRow(rowCount, item);
					if (isRecord)
					{
						addRecord.Add(OtherExtend.ConvertTo<long>(item["BomEntryId"], 0L));
					}
				}
			});
			return rowCount;
		}

		// Token: 0x060007C6 RID: 1990 RVA: 0x0005D040 File Offset: 0x0005B240
		protected DynamicObject DoCombineFields(DynamicObject mainObj, DynamicObject subObj, List<string> fields)
		{
			fields.ForEach(delegate(string m)
			{
				mainObj[m] = subObj[m];
			});
			return mainObj;
		}

		// Token: 0x060007C7 RID: 1991 RVA: 0x0005D07C File Offset: 0x0005B27C
		private void ResetNeedCompareFieldKey()
		{
			bool dynamicValue = DataEntityExtend.GetDynamicValue<bool>(this.View.Model.DataObject, "IsE2DateNoCompare", false);
			if (dynamicValue)
			{
				this.lstNeedComparFieldKey.Remove(new KeyValuePair<string, string>("FEFFECTDATE1", "FEFFECTDATE2"));
				this.lstNeedComparFieldKey.Remove(new KeyValuePair<string, string>("FEXPIREDATE1", "FEXPIREDATE2"));
				return;
			}
			this.lstNeedComparFieldKey.Add(new KeyValuePair<string, string>("FEFFECTDATE1", "FEFFECTDATE2"));
			this.lstNeedComparFieldKey.Add(new KeyValuePair<string, string>("FEXPIREDATE1", "FEXPIREDATE2"));
		}

		// Token: 0x060007C8 RID: 1992 RVA: 0x0005D114 File Offset: 0x0005B314
		public override void OnPrepareNotePrintData(PreparePrintDataEventArgs e)
		{
			base.OnPrepareNotePrintData(e);
			BusinessInfo businessInfo = this.View.BusinessInfo;
			List<DynamicObject> list = null;
			if (e.DataSourceId.Equals("FBillHead", StringComparison.OrdinalIgnoreCase))
			{
				list = new DynamicObject[]
				{
					this.View.Model.DataObject
				}.ToList<DynamicObject>();
			}
			if (e.DataSourceId.Equals("FEntity", StringComparison.OrdinalIgnoreCase))
			{
				list = this.View.Model.GetEntityDataObject(this.View.Model.BusinessInfo.GetEntity("FEntity")).ToList<DynamicObject>();
			}
			e.DataObjects = MFGCommonUtil.ReflushDynamicObjectTypeSource(this.View, businessInfo, e.DataSourceId, e.DynamicObjectType, list);
		}

		// Token: 0x04000371 RID: 881
		protected const string EntityKey = "FEntity";

		// Token: 0x04000372 RID: 882
		protected const string HeadKey = "FBillHead";

		// Token: 0x04000373 RID: 883
		private const string CONST_DIFCOLOR = "#E6B8B7";

		// Token: 0x04000374 RID: 884
		protected MemBomExpandOption_ForPSV memBomExpandOption = new MemBomExpandOption_ForPSV();

		// Token: 0x04000375 RID: 885
		protected List<BomExpandView.BomExpandResult> firstBomItem = new List<BomExpandView.BomExpandResult>();

		// Token: 0x04000376 RID: 886
		protected List<BomExpandView.BomExpandResult> secondBomItem = new List<BomExpandView.BomExpandResult>();

		// Token: 0x04000377 RID: 887
		private List<KeyValuePair<string, string>> lstNeedComparFieldKey = new List<KeyValuePair<string, string>>();

		// Token: 0x04000378 RID: 888
		private string bomExpandGuid = SequentialGuid.NewGuid().ToString();

		// Token: 0x04000379 RID: 889
		protected List<DynamicObject> firstTempEntities;

		// Token: 0x0400037A RID: 890
		protected List<DynamicObject> secondTempEntities;

		// Token: 0x0400037B RID: 891
		protected List<string> entityFields = new List<string>();

		// Token: 0x0400037C RID: 892
		private Entity entity;

		// Token: 0x0400037D RID: 893
		private bool _GroupChange = true;

		// Token: 0x0400037E RID: 894
		private List<KeyValuePair<int, string>> colors;

		// Token: 0x02000069 RID: 105
		protected enum BomItems
		{
			// Token: 0x04000391 RID: 913
			FirstBom = 1,
			// Token: 0x04000392 RID: 914
			SecondBom
		}

		// Token: 0x0200006A RID: 106
		protected enum CompareType
		{
			// Token: 0x04000394 RID: 916
			Single,
			// Token: 0x04000395 RID: 917
			Integration,
			// Token: 0x04000396 RID: 918
			SingleAndShip
		}
	}
}
