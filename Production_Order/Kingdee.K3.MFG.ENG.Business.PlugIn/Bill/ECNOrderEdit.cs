using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kingdee.BOS.BusinessEntity.Organizations;
using Kingdee.BOS.Core.DependencyRules;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.List.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.ConvertElement;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.JSON;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Orm.Metadata.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.BD.ServiceHelper;
using Kingdee.K3.Core.BD;
using Kingdee.K3.Core.BD.ServiceArgs;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.Core.MFG.Utils;
using Kingdee.K3.MFG.BusinessCommon;
using Kingdee.K3.MFG.ENG.Business.PlugIn.Base;
using Kingdee.K3.MFG.ENG.Business.PlugIn.ECNControler;
using Kingdee.K3.MFG.ServiceHelper.ENG;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.Bill
{
	// Token: 0x02000052 RID: 82
	public class ECNOrderEdit : BaseControlEdit
	{
		// Token: 0x17000031 RID: 49
		// (get) Token: 0x06000612 RID: 1554 RVA: 0x00047CF0 File Offset: 0x00045EF0
		// (set) Token: 0x06000613 RID: 1555 RVA: 0x00047CF8 File Offset: 0x00045EF8
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

		// Token: 0x06000614 RID: 1556 RVA: 0x00047DEC File Offset: 0x00045FEC
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.View.RuleContainer.AddPluginRule("FTreeEntity", 3, delegate(DynamicObject dataObj, dynamic dynObj)
			{
				Field field = base.View.BusinessInfo.GetField("FInsertRow");
				if (Convert.ToInt32(dataObj["RowType"]) == 1)
				{
					base.View.StyleManager.SetEnabled(field, dataObj, "locked", true);
				}
				else
				{
					base.View.StyleManager.SetEnabled(field, dataObj, "locked", false);
				}
				Field field2 = base.View.BusinessInfo.GetField("FMATERIALIDCHILD");
				if (Convert.ToInt32(dataObj["RowType"]) == 3)
				{
					base.View.StyleManager.SetEnabled(field2, dataObj, "locked", false);
				}
				RelatedFlexGroupField relatedFlexGroupField = base.View.BusinessInfo.GetField("FAuxPropId") as RelatedFlexGroupField;
				AuxPtyLocker.SetEnabled(base.View, relatedFlexGroupField, dataObj, "locked");
			}, new string[]
			{
				"FMATERIALIDCHILD"
			});
			base.View.RuleContainer.AddPluginRule("FBillHead", 3, new Action<DynamicObject, object>(this.SetMenuLocked), new string[]
			{
				"FChangeType"
			});
			base.View.RuleContainer.AddPluginRule("FTreeEntity", 2, delegate(BOSActionExecuteContext x)
			{
				this.SetEnbleHeadField();
			}, new string[0]);
			base.View.RuleContainer.AddPluginRule("FCobyEntity", 2, delegate(BOSActionExecuteContext x)
			{
				this.SetEnbleHeadField();
			}, new string[0]);
		}

		// Token: 0x06000615 RID: 1557 RVA: 0x00047EB4 File Offset: 0x000460B4
		public override void AfterDoOperation(AfterDoOperationEventArgs e)
		{
			base.AfterDoOperation(e);
			string operation;
			switch (operation = e.Operation.Operation)
			{
			case "DeleteItem":
				this.SetEnbleHeadField();
				return;
			case "DeleteCobyEntry":
				this.SetEnbleHeadField();
				return;
			case "Audit":
			{
				string value = MFGBillUtil.GetValue<string>(base.View.Model, "FBillTypeID", -1, null, null);
				DynamicObject dynamicObject = BusinessDataServiceHelper.LoadBillTypePara(base.Context, "ENG_ECNBillParameter", value, true);
				int dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<int>(dynamicObject, "TriggerEffect", 0);
				if (dynamicObjectItemValue == 1 && e.OperationResult.IsSuccess)
				{
					base.View.Refresh();
				}
				if (e.OperationResult.IsSuccess)
				{
					this.SetColorForChangedField();
					return;
				}
				break;
			}
			case "Submit":
				if (e.OperationResult.IsSuccess)
				{
					this.SetColorForChangedField();
					this.delEcnEntrys.Clear();
					return;
				}
				break;
			case "Save":
				if (e.OperationResult.IsSuccess)
				{
					this.delEcnEntrys.Clear();
					return;
				}
				break;
			case "CancelAssign":
				base.View.UpdateView("FTreeEntity");
				return;
			case "UnAudit":
				base.View.UpdateView("FTreeEntity");
				break;

				return;
			}
		}

		// Token: 0x06000616 RID: 1558 RVA: 0x0004807C File Offset: 0x0004627C
		public override void AfterBindData(EventArgs e)
		{
			this.ecrEntryBomIds = new List<long>();
			this.useECREntryBomId = false;
			this.controls.Clear();
			OrganizationServiceHelper.GetBaseDataControlPolicyDObj(base.Context, OtherExtend.ConvertTo<long>(this.Model.DataObject["ChangeOrgId_Id"], 0L), "ENG_BOM");
			this.bomEntryCtrlSettings = new Dictionary<long, BaseDataControlPolicyTargetOrgEntry>();
			this.controls.Add(1, new NewItemControler
			{
				View = base.View,
				Model = this.Model,
				RowControlBuffer = this.RowControlBuffer,
				BomEntryCtrlSettings = this.bomEntryCtrlSettings
			});
			this.controls.Add(2, new AlterItemControler
			{
				View = base.View,
				Model = this.Model,
				RowControlBuffer = this.RowControlBuffer,
				BomEntryCtrlSettings = this.bomEntryCtrlSettings
			});
			this.controls.Add(3, new RemoveItemControler
			{
				View = base.View,
				Model = this.Model,
				RowControlBuffer = this.RowControlBuffer,
				BomEntryCtrlSettings = this.bomEntryCtrlSettings
			});
			this.controls.Add(4, new ExpireItemControler
			{
				View = base.View,
				Model = this.Model,
				RowControlBuffer = this.RowControlBuffer,
				BomEntryCtrlSettings = this.bomEntryCtrlSettings
			});
			this.controls.Add(5, new DeleteItemControler
			{
				View = base.View,
				Model = this.Model,
				RowControlBuffer = this.RowControlBuffer,
				BomEntryCtrlSettings = this.bomEntryCtrlSettings
			});
			this.controls.Add(6, new EffectChangeControler
			{
				View = base.View,
				Model = this.Model,
				RowControlBuffer = this.RowControlBuffer,
				BomEntryCtrlSettings = this.bomEntryCtrlSettings
			});
			this.controls.Add(7, new SyncPPBomControler
			{
				View = base.View,
				Model = this.Model
			});
			this.controls.Add(8, new ECNQueryControler
			{
				View = base.View,
				Model = this.Model
			});
			this.controls.Add(9, new ChooseRepControler
			{
				View = base.View,
				Model = this.Model,
				RowControlBuffer = this.RowControlBuffer,
				BomEntryCtrlSettings = this.bomEntryCtrlSettings
			});
			this.controls.Add(10, new SetupRepControler
			{
				View = base.View,
				Model = this.Model,
				RowControlBuffer = this.RowControlBuffer,
				BomEntryCtrlSettings = this.bomEntryCtrlSettings
			});
			this.controls.Add(11, new SimEffectChangeControler
			{
				View = base.View,
				Model = this.Model
			});
			this.controls.Add(12, new NewCobyControler
			{
				View = base.View,
				Model = this.Model
			});
			this.controls.Add(13, new AlterCobyControler
			{
				View = base.View,
				Model = this.Model
			});
			this.controls.Add(14, new RemoveCobyControler
			{
				View = base.View,
				Model = this.Model
			});
			this.controls.Add(15, new ExpireCobyControler
			{
				View = base.View,
				Model = this.Model
			});
			this.controls.Add(16, new DeleteCobyControler
			{
				View = base.View,
				Model = this.Model
			});
			this.controls.Add(17, new NewNItemControler
			{
				View = base.View,
				Model = this.Model
			});
			this.controls.Add(18, new CustomBatchFillControler
			{
				View = base.View,
				Model = this.Model
			});
			Entity entity = base.View.BusinessInfo.GetEntity("FTreeEntity");
			this.Model.GetEntityDataObject(entity);
			((IDynamicFormViewService)base.View).CustomEvents("FTreeEntity", "RowEdiableEvent", "");
			if (base.Context.IsStandardEdition())
			{
				FormUtils.StdLockField(base.View, "FSUPPLYMODE");
			}
			string value = MFGBillUtil.GetValue<string>(this.Model, "FDocumentStatus", -1, null, null);
			if (StringUtils.EqualsIgnoreCase(value, "C") || StringUtils.EqualsIgnoreCase(value, "B"))
			{
				this.SetColorForChangedField();
			}
			ComboFieldEditor control = base.View.GetControl<ComboFieldEditor>("FMATERIALTYPE");
			ComboField comboField = base.View.BusinessInfo.GetField("FMATERIALTYPE") as ComboField;
			DynamicObjectCollection source = comboField.EnumObject["Items"] as DynamicObjectCollection;
			List<EnumItem> list = (from item in source
			select new EnumItem(OrmUtils.Clone(item, false, true) as DynamicObject) into x
			orderby x.Seq
			select x).ToList<EnumItem>();
			(from x in list
			where x.Value == "3"
			select x).FirstOrDefault<EnumItem>().Invalid = true;
			control.SetComboItems(list);
		}

		// Token: 0x06000617 RID: 1559 RVA: 0x00048670 File Offset: 0x00046870
		public override void BeforeDoOperation(BeforeDoOperationEventArgs e)
		{
			base.BeforeDoOperation(e);
			this.IsUseECREntryBomId();
			string operation;
			switch (operation = e.Operation.FormOperation.Operation)
			{
			case "AddItem":
				this.controls[1].DoOperation();
				return;
			case "AlterItem":
				this.controls[2].UseECREntryBomId = this.useECREntryBomId;
				this.controls[2].ECREntryBomId = this.ecrEntryBomIds;
				this.controls[2].DoOperation();
				return;
			case "RemoveItem":
				this.controls[3].UseECREntryBomId = this.useECREntryBomId;
				this.controls[3].ECREntryBomId = this.ecrEntryBomIds;
				this.controls[3].DoOperation();
				return;
			case "ExpireItem":
				this.controls[4].UseECREntryBomId = this.useECREntryBomId;
				this.controls[4].ECREntryBomId = this.ecrEntryBomIds;
				this.controls[4].DoOperation();
				return;
			case "DeleteItem":
				this.controls[5].DoOperation();
				return;
			case "Save":
			{
				List<DynamicObject> list = new List<DynamicObject>();
				list.AddRange(this.delEcnEntrys);
				e.Option.SetVariableValue("DelEcnEntryInfo", list);
				return;
			}
			case "Submit":
			{
				List<DynamicObject> list2 = new List<DynamicObject>();
				list2.AddRange(this.delEcnEntrys);
				e.Option.SetVariableValue("DelEcnEntryInfo", list2);
				return;
			}
			case "Effect":
				e.Cancel = true;
				this.controls[6].DoOperation();
				return;
			case "SyncPPBom":
				this.controls[7].DoOperation();
				return;
			case "ECNQuery":
				this.controls[8].DoOperation();
				return;
			case "ChooseRep":
				this.controls[9].UseECREntryBomId = this.useECREntryBomId;
				this.controls[9].ECREntryBomId = this.ecrEntryBomIds;
				this.controls[9].DoOperation();
				return;
			case "SetupRep":
				this.controls[10].SelectRows = (from f in SysBclExtend.GetSelectedRowDatas(base.View, "FTreeEntity")
				orderby DataEntityExtend.GetDynamicValue<int>(f, "Seq", 0)
				select f).ToList<DynamicObject>();
				this.controls[10].DoOperation();
				return;
			case "SimEffect":
				this.controls[11].DoOperation();
				return;
			case "NewCobyEntry":
				this.controls[12].UseECREntryBomId = this.useECREntryBomId;
				this.controls[12].ECREntryBomId = this.ecrEntryBomIds;
				this.controls[12].DoOperation();
				return;
			case "ModifyCobyEntry":
				this.controls[13].UseECREntryBomId = this.useECREntryBomId;
				this.controls[13].ECREntryBomId = this.ecrEntryBomIds;
				this.controls[13].DoOperation();
				return;
			case "RemoveCobyEntry":
				this.controls[14].UseECREntryBomId = this.useECREntryBomId;
				this.controls[14].ECREntryBomId = this.ecrEntryBomIds;
				this.controls[14].DoOperation();
				return;
			case "ExpireCobyEntey":
				this.controls[15].UseECREntryBomId = this.useECREntryBomId;
				this.controls[15].ECREntryBomId = this.ecrEntryBomIds;
				this.controls[15].DoOperation();
				return;
			case "DeleteCobyEntry":
				this.controls[16].DoOperation();
				return;
			case "AddNItem":
				this.controls[17].DoOperation();
				return;
			case "CustomBatchFill":
				this.controls[18].DoOperation();
				break;

				return;
			}
		}

		// Token: 0x06000618 RID: 1560 RVA: 0x00048B80 File Offset: 0x00046D80
		public override void EntryBarItemClick(BarItemClickEventArgs e)
		{
			base.EntryBarItemClick(e);
			string barItemKey;
			if ((barItemKey = e.BarItemKey) != null)
			{
				if (!(barItemKey == "tbSetupRep"))
				{
					return;
				}
				e.Cancel = this.SetupRepValidator();
			}
		}

		// Token: 0x06000619 RID: 1561 RVA: 0x00048BB8 File Offset: 0x00046DB8
		public override void CustomEvents(CustomEventsArgs e)
		{
			e.EventName == "AfterLoadTreeEntityData";
			if (e.EventName == "RowEdiableEvent")
			{
				if (base.Context.ServiceType != null)
				{
					return;
				}
				int num = OtherExtend.ConvertTo<int>(e.EventArgs, 1);
				Entity entity = base.View.BusinessInfo.GetEntity("FTreeEntity");
				if (!ListUtils.IsEmpty<DynamicObject>(this.RowControlBuffer))
				{
					goto IL_D7;
				}
				using (IEnumerator<DynamicObject> enumerator = this.Model.GetEntityDataObject(entity).GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						DynamicObject item = enumerator.Current;
						this.RowControlBuffer.Enqueue(item);
					}
					goto IL_D7;
				}
				IL_A4:
				DynamicObject dynamicObject = this.RowControlBuffer.Dequeue();
				int dynamicValue = DataEntityExtend.GetDynamicValue<int>(dynamicObject, "RowType", 0);
				AbstractItemControler abstractItemControler;
				if (this.controls.TryGetValue(dynamicValue, out abstractItemControler))
				{
					abstractItemControler.SetRowFieldControl(dynamicObject);
				}
				IL_D7:
				if (ListUtils.IsEmpty<DynamicObject>(this.RowControlBuffer))
				{
					this.SetMenuLocked(null, null);
					((IDynamicFormViewService)base.View).EntityRowClick("FTreeEntity".ToUpperInvariant(), num);
					return;
				}
				goto IL_A4;
			}
			else
			{
				if (e.EventName == "SetEntityRowFocus")
				{
					int num2 = OtherExtend.ConvertTo<int>(e.EventArgs, 1);
					base.View.SetEntityFocusRow("FTreeEntity", num2);
					base.View.SendDynamicFormAction(base.View);
					return;
				}
				if (e.EventName == "AddEntryRow")
				{
					JSONObject jsonobject = KDObjectConverter.DeserializeObject<JSONObject>(e.EventArgs);
					int startIndex = OtherExtend.ConvertTo<int>(jsonobject.Get("RowIndex"), 0);
					object obj = jsonobject.Get("SelectedRows");
					ListSelectedRowCollection collection = ObjectUtils.IsNullOrEmptyOrWhiteSpace(obj) ? null : KDObjectConverter.DeserializeObject<ListSelectedRowCollection>(obj.ToString());
					int key = OtherExtend.ConvertTo<int>(jsonobject.Get("ControlType"), 0);
					AbstractItemControler abstractItemControler2 = null;
					if (this.controls.TryGetValue(key, out abstractItemControler2))
					{
						abstractItemControler2.AddEntryRow(collection, startIndex);
					}
				}
				return;
			}
		}

		// Token: 0x0600061A RID: 1562 RVA: 0x00048DB0 File Offset: 0x00046FB0
		public override void AfterUpdateViewState(EventArgs e)
		{
			this.SetMenuLocked(null, null);
		}

		// Token: 0x0600061B RID: 1563 RVA: 0x00048DBC File Offset: 0x00046FBC
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(e.BaseDataField) && e.BaseDataField.Entity.Key == "FTreeEntity")
			{
				int key = OtherExtend.ConvertTo<int>(this.Model.GetValue("FRowType", e.Row), 0);
				AbstractItemControler abstractItemControler;
				if (this.controls.TryGetValue(key, out abstractItemControler))
				{
					abstractItemControler.BeforeF7Select(e);
				}
				string fieldKey;
				if ((fieldKey = e.FieldKey) != null)
				{
					if (!(fieldKey == "FMATERIALIDCHILD"))
					{
						if (fieldKey == "FBOMVERSION")
						{
							this.IsUseECREntryBomId();
							if (this.useECREntryBomId && !ListUtils.IsEmpty<long>(this.ecrEntryBomIds))
							{
								e.ListFilterParameter.Filter = base.SqlAppendAnd(e.ListFilterParameter.Filter, string.Format("FId in ({0})", string.Join<long>(",", this.ecrEntryBomIds)));
							}
						}
					}
					else
					{
						DynamicObject dynamicObject = base.View.Model.GetValue("FParentMaterialId", e.Row) as DynamicObject;
						if (ObjectUtils.IsNullOrEmpty(dynamicObject))
						{
							return;
						}
						string dynamicValue = DataEntityExtend.GetDynamicValue<string>((dynamicObject["MaterialBase"] as DynamicObjectCollection).FirstOrDefault<DynamicObject>(), "Suite", null);
						if (dynamicValue == "0")
						{
							e.ListFilterParameter.Filter = base.SqlAppendAnd(e.ListFilterParameter.Filter, "FSUITE = '0' ");
						}
					}
				}
			}
			string a;
			if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(e.BaseDataField) && e.BaseDataField.Entity.Key == "FUpdateVersionEntity" && (a = e.FieldKey.ToUpper()) != null && a == "FMATERIALIDU")
			{
				e.IsShowUsed = false;
				e.IsShowApproved = false;
			}
			if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(e.BaseDataField) && e.BaseDataField.Entity.Key == "FCobyEntity")
			{
				int key2 = OtherExtend.ConvertTo<int>(this.Model.GetValue("FCobyECNRowType", e.Row), 0);
				AbstractItemControler abstractItemControler2;
				if (this.controls.TryGetValue(key2, out abstractItemControler2))
				{
					abstractItemControler2.BeforeF7Select(e);
				}
				string a2;
				if ((a2 = e.FieldKey.ToUpperInvariant()) != null)
				{
					if (!(a2 == "FBOMIDCOBY"))
					{
						return;
					}
					e.DynamicFormShowParameter.MultiSelect = false;
					e.IsShowApproved = false;
					bool flag = false;
					e.ListFilterParameter.Filter = this.GetBomIdCopyFilter(e.ListFilterParameter.Filter, e.Row, out flag);
					if (!flag)
					{
						e.Cancel = true;
					}
				}
			}
		}

		// Token: 0x0600061C RID: 1564 RVA: 0x0004904C File Offset: 0x0004724C
		public override void BeforeSetItemValueByNumber(BeforeSetItemValueByNumberArgs e)
		{
			base.BeforeSetItemValueByNumber(e);
			string a;
			if ((a = e.BaseDataFieldKey.ToUpperInvariant()) != null)
			{
				if (a == "FMATERIALIDU")
				{
					e.IsShowUsed = false;
					e.IsShowApproved = false;
					return;
				}
				if (!(a == "FBOMVERSION"))
				{
					return;
				}
				int key = OtherExtend.ConvertTo<int>(this.Model.GetValue("FRowType", e.Row), 0);
				AbstractItemControler abstractItemControler;
				if (this.controls.TryGetValue(key, out abstractItemControler))
				{
					abstractItemControler.BeforeSetItemValueByNumber(e);
				}
			}
		}

		// Token: 0x0600061D RID: 1565 RVA: 0x000490D0 File Offset: 0x000472D0
		public override void DataChanged(DataChangedEventArgs e)
		{
			if (e.Field.Entity.Key == "FTreeEntity")
			{
				int key = OtherExtend.ConvertTo<int>(this.Model.GetValue("FRowType", e.Row), 0);
				AbstractItemControler abstractItemControler;
				if (this.controls.TryGetValue(key, out abstractItemControler))
				{
					abstractItemControler.DataChanged(e);
				}
				string key2;
				if ((key2 = e.Field.Key) != null)
				{
					if (key2 == "FDOSAGETYPE")
					{
						this.ECNEntityDosageTypeChange(e);
						return;
					}
					if (!(key2 == "FPOSITIONNO"))
					{
						return;
					}
					this.ChangePositonNo(e);
					return;
				}
			}
			else if (e.Field.Entity.Key == "FBillHead")
			{
				string key3;
				if ((key3 = e.Field.Key) != null)
				{
					if (!(key3 == "FIsUpdateVersion"))
					{
						return;
					}
					if (!OtherExtend.ConvertTo<bool>(e.NewValue, false))
					{
						this.Model.DeleteEntryData("FUpdateVersionEntity");
						base.View.UpdateView("FUpdateVersionEntity");
						return;
					}
					this.controls.FirstOrDefault<KeyValuePair<int, AbstractItemControler>>().Value.SummaryUpdtBOMVers();
					return;
				}
			}
			else if (e.Field.Entity.Key == "FCobyEntity")
			{
				int key4 = OtherExtend.ConvertTo<int>(this.Model.GetValue("FCobyECNRowType", e.Row), 0);
				AbstractItemControler abstractItemControler2;
				if (this.controls.TryGetValue(key4, out abstractItemControler2))
				{
					abstractItemControler2.DataChanged(e);
				}
			}
		}

		// Token: 0x0600061E RID: 1566 RVA: 0x00049288 File Offset: 0x00047488
		public override void BeforeUpdateValue(BeforeUpdateValueEventArgs e)
		{
			Field field = base.View.BusinessInfo.GetField(e.Key);
			if (field.Entity.Key == "FTreeEntity")
			{
				int key = OtherExtend.ConvertTo<int>(this.Model.GetValue("FRowType", e.Row), 0);
				AbstractItemControler abstractItemControler;
				if (this.controls.TryGetValue(key, out abstractItemControler))
				{
					abstractItemControler.BeforeUpdateValue(e);
				}
			}
			string key2;
			if ((key2 = e.Key) != null)
			{
				if (!(key2 == "FChangeType"))
				{
					return;
				}
				Entity entity = base.View.BusinessInfo.GetEntity("FTreeEntity");
				DynamicObjectCollection entityDataObject = base.View.Model.GetEntityDataObject(entity);
				if (entityDataObject.Count <= 0)
				{
					return;
				}
				switch (Convert.ToInt32(e.Value))
				{
				case 0:
				{
					List<DynamicObject> list = (from o in entityDataObject
					where DataEntityExtend.GetDynamicObjectItemValue<int>(o, "RowType", 0) == 1 || DataEntityExtend.GetDynamicObjectItemValue<int>(o, "RowType", 0) == 3 || DataEntityExtend.GetDynamicObjectItemValue<int>(o, "RowType", 0) == 4
					select o).ToList<DynamicObject>();
					if (list.Count > 0)
					{
						e.Cancel = true;
						base.View.ShowErrMessage(ResManager.LoadKDString("分录明细中无“新增”、“删除”、“失效”类的分录，可更改变更类型为“用完旧料”", "015072000014916", 7, new object[0]), "", 0);
						return;
					}
					break;
				}
				case 1:
					break;
				case 2:
				{
					List<DynamicObject> list2 = (from o in entityDataObject
					where DataEntityExtend.GetDynamicObjectItemValue<int>(o, "RowType", 0) == 3
					select o).ToList<DynamicObject>();
					if (list2.Count > 0)
					{
						e.Cancel = true;
						base.View.ShowErrMessage(ResManager.LoadKDString("分录明细中无“删除”类的分录，可更改变更类型为“按日期变更”", "015072000014917", 7, new object[0]), "", 0);
					}
					break;
				}
				default:
					return;
				}
			}
		}

		// Token: 0x0600061F RID: 1567 RVA: 0x00049434 File Offset: 0x00047634
		public override void AfterCreateNewEntryRow(CreateNewEntryEventArgs e)
		{
			this.Model.SetValue("FECNRowId", SequentialGuid.NewGuid().ToString(), e.Row);
		}

		// Token: 0x06000620 RID: 1568 RVA: 0x0004946C File Offset: 0x0004766C
		public override void AfterF7Select(AfterF7SelectEventArgs e)
		{
			Field field = base.View.BillBusinessInfo.GetField(e.FieldKey);
			if (field != null && StringUtils.EqualsIgnoreCase(field.EntityKey, "FTreeEntity"))
			{
				int entryCurrentRowIndex = base.View.Model.GetEntryCurrentRowIndex("FTreeEntity");
				int num = entryCurrentRowIndex;
				int key = OtherExtend.ConvertTo<int>(this.Model.GetValue("FRowType", num), 0);
				AbstractItemControler abstractItemControler;
				if (this.controls.TryGetValue(key, out abstractItemControler))
				{
					abstractItemControler.AfterF7Select(e, num);
				}
			}
		}

		// Token: 0x06000621 RID: 1569 RVA: 0x000494F0 File Offset: 0x000476F0
		public override void BeforeDeleteRow(BeforeDeleteRowEventArgs e)
		{
			base.BeforeDeleteRow(e);
			DynamicObject item = null;
			int num = 0;
			base.View.Model.TryGetEntryCurrentRow("FTreeEntity", ref item, ref num);
			this.delEcnEntrys.Add(item);
			string entityKey;
			if ((entityKey = e.EntityKey) != null)
			{
				if (!(entityKey == "FStairDosage"))
				{
					return;
				}
				if (base.View.Model.GetEntryRowCount("FStairDosage") == 1)
				{
					base.View.ShowMessage(ResManager.LoadKDString("阶梯用量表体至少保留一行！", "015072000002168", 7, new object[0]), 0);
					e.Cancel = true;
				}
			}
		}

		// Token: 0x06000622 RID: 1570 RVA: 0x00049588 File Offset: 0x00047788
		private void SetEnbleHeadField()
		{
			int entryRowCount = this.Model.GetEntryRowCount("FTreeEntity");
			int entryRowCount2 = this.Model.GetEntryRowCount("FCobyEntity");
			base.View.StyleManager.SetEnabled("FChangeType", "locked", entryRowCount + entryRowCount2 == 0);
		}

		// Token: 0x06000623 RID: 1571 RVA: 0x000495DC File Offset: 0x000477DC
		private void SetMenuLocked(DynamicObject dataObject, dynamic dynObj)
		{
			if (DataEntityExtend.GetDynamicValue<char>(this.Model.DataObject, "DocumentStatus", '\0') == 'C' || DataEntityExtend.GetDynamicValue<char>(this.Model.DataObject, "DocumentStatus", '\0') == 'B')
			{
				return;
			}
			switch (OtherExtend.ConvertTo<int>(this.Model.GetValue("FChangeType"), 0))
			{
			case 0:
				base.View.GetBarItem("FTreeEntity", "tbAddItem").Enabled = false;
				base.View.GetBarItem("FTreeEntity", "tbAlterItem").Enabled = true;
				base.View.GetBarItem("FTreeEntity", "tbRemoveItem").Enabled = false;
				base.View.GetBarItem("FTreeEntity", "tbExpireItem").Enabled = false;
				base.View.GetBarItem("FTreeEntity", "tbSetupRep").Enabled = false;
				base.View.GetBarItem("FTreeEntity", "tbChooseRep").Enabled = false;
				base.View.GetBarItem("FCobyEntity", "tbDeleteCobyEntry").Enabled = false;
				base.View.GetBarItem("FCobyEntity", "tbExpireCobyEntey").Enabled = false;
				base.View.GetBarItem("FCobyEntity", "tbModifyCobyEntry").Enabled = false;
				base.View.GetBarItem("FCobyEntity", "tbNewCobyEntry").Enabled = false;
				base.View.GetBarItem("FCobyEntity", "tbRemoveCobyEntry").Enabled = false;
				base.View.GetBarItem("FTreeEntity", "tbAddNItem").Enabled = false;
				return;
			case 1:
				base.View.GetBarItem("FTreeEntity", "tbAddItem").Enabled = true;
				base.View.GetBarItem("FTreeEntity", "tbAlterItem").Enabled = true;
				base.View.GetBarItem("FTreeEntity", "tbRemoveItem").Enabled = true;
				base.View.GetBarItem("FTreeEntity", "tbExpireItem").Enabled = true;
				base.View.GetBarItem("FTreeEntity", "tbSetupRep").Enabled = true;
				base.View.GetBarItem("FTreeEntity", "tbChooseRep").Enabled = true;
				base.View.GetBarItem("FCobyEntity", "tbDeleteCobyEntry").Enabled = true;
				base.View.GetBarItem("FCobyEntity", "tbExpireCobyEntey").Enabled = true;
				base.View.GetBarItem("FCobyEntity", "tbModifyCobyEntry").Enabled = true;
				base.View.GetBarItem("FCobyEntity", "tbNewCobyEntry").Enabled = true;
				base.View.GetBarItem("FCobyEntity", "tbRemoveCobyEntry").Enabled = true;
				base.View.GetBarItem("FTreeEntity", "tbAddNItem").Enabled = true;
				return;
			case 2:
				base.View.GetBarItem("FTreeEntity", "tbAddItem").Enabled = true;
				base.View.GetBarItem("FTreeEntity", "tbAlterItem").Enabled = true;
				base.View.GetBarItem("FTreeEntity", "tbRemoveItem").Enabled = false;
				base.View.GetBarItem("FTreeEntity", "tbExpireItem").Enabled = true;
				base.View.GetBarItem("FTreeEntity", "tbSetupRep").Enabled = true;
				base.View.GetBarItem("FTreeEntity", "tbChooseRep").Enabled = true;
				base.View.GetBarItem("FCobyEntity", "tbDeleteCobyEntry").Enabled = false;
				base.View.GetBarItem("FCobyEntity", "tbExpireCobyEntey").Enabled = false;
				base.View.GetBarItem("FCobyEntity", "tbModifyCobyEntry").Enabled = false;
				base.View.GetBarItem("FCobyEntity", "tbNewCobyEntry").Enabled = false;
				base.View.GetBarItem("FCobyEntity", "tbRemoveCobyEntry").Enabled = false;
				base.View.GetBarItem("FTreeEntity", "tbAddNItem").Enabled = true;
				return;
			default:
				return;
			}
		}

		// Token: 0x06000624 RID: 1572 RVA: 0x00049A44 File Offset: 0x00047C44
		public override void OnShowConvertOpForm(ShowConvertOpFormEventArgs e)
		{
			base.OnShowConvertOpForm(e);
			FormOperationEnum convertOperation = e.ConvertOperation;
			if (convertOperation != 13)
			{
				return;
			}
			List<ConvertBillElement> list = e.Bills as List<ConvertBillElement>;
			if (ListUtils.IsEmpty<ConvertBillElement>(list))
			{
				return;
			}
			list = (from w in list
			where !StringUtils.EqualsIgnoreCase(w.FormID, "ENG_BOM") && !StringUtils.EqualsIgnoreCase(w.FormID, "ENG_BBEBOM")
			select w).ToList<ConvertBillElement>();
			e.Bills = list;
		}

		// Token: 0x06000625 RID: 1573 RVA: 0x00049B00 File Offset: 0x00047D00
		public bool SetupRepValidator()
		{
			List<DynamicObject> list = (from f in SysBclExtend.GetSelectedRowDatas(base.View, "FTreeEntity")
			orderby DataEntityExtend.GetDynamicValue<int>(f, "Seq", 0)
			select f).ToList<DynamicObject>();
			if (list.Count<DynamicObject>() <= 0 || list.Count<DynamicObject>() > 1)
			{
				base.View.ShowErrMessage(ResManager.LoadKDString("必须选择一行分录，才能进行替代设置！", "015072000013578", 7, new object[0]), "", 0);
				return true;
			}
			Entity entity = base.View.BusinessInfo.GetEntity("FTreeEntity");
			DynamicObjectCollection entityDataObject = this.Model.GetEntityDataObject(entity);
			StringBuilder stringBuilder = new StringBuilder();
			DynamicObject row = list[0];
			IEnumerable<DynamicObject> source = from f in entityDataObject
			where DataEntityExtend.GetDynamicValue<string>(f, "ECNGroup", null) == DataEntityExtend.GetDynamicValue<string>(row, "ECNGroup", null) && DataEntityExtend.GetDynamicValue<string>(f, "MATERIALTYPE", null) == "1"
			select f;
			bool dynamicValue = DataEntityExtend.GetDynamicValue<bool>(row, "IskeyItem", false);
			DynamicObject dynamicValue2 = DataEntityExtend.GetDynamicValue<DynamicObject>(row, "MATERIALIDCHILD", null);
			if (ObjectUtils.IsNullOrEmpty(dynamicValue2))
			{
				base.View.ShowErrMessage("当前分录必须设置子项物料编码，才能进行替代设置", "", 0);
				return true;
			}
			string dynamicValue3 = DataEntityExtend.GetDynamicValue<string>(row, "MATERIALTYPE", null);
			DynamicObject dynamicValue4 = DataEntityExtend.GetDynamicValue<DynamicObject>(row, "BomVersion", null);
			long dynamicValue5 = DataEntityExtend.GetDynamicValue<long>(dynamicValue4, "UseOrgId_Id", 0L);
			long dynamicValue6 = DataEntityExtend.GetDynamicValue<long>(dynamicValue4, "CreateOrgId_Id", 0L);
			int dynamicValue7 = DataEntityExtend.GetDynamicValue<int>(row, "ChangeLabel", 0);
			DataEntityExtend.GetDynamicValue<int>(row, "Seq", 0);
			string dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<string>(row, "BOMCATEGORY", null);
			DynamicObjectCollection dynamicObjectItemValue2 = DataEntityExtend.GetDynamicObjectItemValue<DynamicObjectCollection>(DataEntityExtend.GetDynamicObjectItemValue<DynamicObject>(row, "ParentMaterialId", null), "MaterialBase", null);
			if (ListUtils.IsEmpty<DynamicObject>(dynamicObjectItemValue2))
			{
				string text = "父项物料未审核或已禁用，不能进行替代设置";
				base.View.ShowErrMessage(text, "", 0);
				return true;
			}
			string dynamicObjectItemValue3 = DataEntityExtend.GetDynamicObjectItemValue<string>(dynamicObjectItemValue2.FirstOrDefault<DynamicObject>(), "ErpClsID", null);
			DynamicObjectCollection dynamicObjectItemValue4 = DataEntityExtend.GetDynamicObjectItemValue<DynamicObjectCollection>(DataEntityExtend.GetDynamicObjectItemValue<DynamicObject>(row, "MATERIALIDCHILD", null), "MaterialBase", null);
			if (ListUtils.IsEmpty<DynamicObject>(dynamicObjectItemValue4))
			{
				string text2 = "子项物料未审核或已禁用，不能进行替代设置";
				base.View.ShowErrMessage(text2, "", 0);
				return true;
			}
			string dynamicObjectItemValue5 = DataEntityExtend.GetDynamicObjectItemValue<string>(dynamicObjectItemValue4.First<DynamicObject>(), "ErpClsID", null);
			if (dynamicValue7 != 2 && dynamicValue7 != 5)
			{
				stringBuilder.Append(ResManager.LoadKDString("行标识为新增或主物料,", "015072000013579", 7, new object[0]));
			}
			if (ObjectUtils.IsNullOrEmpty(dynamicValue2))
			{
				stringBuilder.Append(ResManager.LoadKDString("子项物料不为空,", "015072000013580", 7, new object[0]));
			}
			if (dynamicValue3 != "1")
			{
				stringBuilder.Append(ResManager.LoadKDString("子项类型为标准件,", "015072000013581", 7, new object[0]));
			}
			if (dynamicValue5 != dynamicValue6)
			{
				stringBuilder.Append(ResManager.LoadKDString("BOM版本的创建组织和使用组织相同,", "015072000013582", 7, new object[0]));
			}
			if (source.Count<DynamicObject>() > 1 && !dynamicValue)
			{
				stringBuilder.Append(ResManager.LoadKDString("组合替代下替代主料,", "015072000013583", 7, new object[0]));
			}
			if ((!StringUtils.EqualsIgnoreCase(dynamicObjectItemValue, "1") || StringUtils.EqualsIgnoreCase(dynamicObjectItemValue3, "4")) && StringUtils.EqualsIgnoreCase(dynamicObjectItemValue5, "9"))
			{
				stringBuilder.Append(ResManager.LoadKDString("标准BOM且父项物料属性为非特征件的配制件,", "015072030036808", 7, new object[0]));
			}
			if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(stringBuilder))
			{
				string text3 = string.Format(ResManager.LoadKDString("无法进行替代设置，原因：{0}才能进行替代设置！", "015072000013584", 7, new object[0]), stringBuilder);
				base.View.ShowErrMessage(text3, "", 0);
				return true;
			}
			return false;
		}

		// Token: 0x06000626 RID: 1574 RVA: 0x00049E98 File Offset: 0x00048098
		private void ECNEntityDosageTypeChange(DataChangedEventArgs e)
		{
			string text = string.Empty;
			text = MFGBillUtil.GetValue<string>(base.View.Model, "FDOSAGETYPE", e.Row, "", "TreeEntity");
			if (!StringUtils.EqualsIgnoreCase(text, "3"))
			{
				this.DeleteFlotBasedqty();
				return;
			}
			DynamicObject value = MFGBillUtil.GetValue<DynamicObject>(base.View.Model, "FMATERIALIDCHILD", e.Row, null, "TreeEntity");
			this.CreateFlotBasedqty(value, e.Row);
		}

		// Token: 0x06000627 RID: 1575 RVA: 0x00049F14 File Offset: 0x00048114
		private void DeleteFlotBasedqty()
		{
			if (base.View.Model.GetEntryRowCount("FStairDosage") > 0)
			{
				base.View.Model.DeleteEntryData("FStairDosage");
			}
		}

		// Token: 0x06000628 RID: 1576 RVA: 0x00049F44 File Offset: 0x00048144
		private void CreateFlotBasedqty(DynamicObject dyChildMaterial, int row)
		{
			if (base.View.Model.GetEntryRowCount("FStairDosage") <= 0)
			{
				base.View.Model.CreateNewEntryRow("FStairDosage");
			}
			long dynamicValue = DataEntityExtend.GetDynamicValue<long>(dyChildMaterial, "Id", 0L);
			if (dynamicValue > 0L)
			{
				int entryRowCount = base.View.Model.GetEntryRowCount("FStairDosage");
				Entity entryEntity = base.View.BillBusinessInfo.GetEntryEntity("FStairDosage");
				Field field = base.View.BillBusinessInfo.GetField("FBaseNumeratorLot");
				Field field2 = base.View.BillBusinessInfo.GetField("FBaseDenominatorLot");
				long value = MFGBillUtil.GetValue<long>(base.View.Model, "FParentUnitID", row, 0L, "FTreeEntity");
				long value2 = MFGBillUtil.GetValue<long>(base.View.Model, "FParentBaseUnitId", row, 0L, "FTreeEntity");
				MFGBillUtil.GetValue<long>(base.View.Model, "FParentMaterialId", row, 0L, "FTreeEntity");
				DynamicObject dynamicValue2 = DataEntityExtend.GetDynamicValue<DynamicObject>(DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(this.Model.DataObject, "TreeEntity", null)[row], "ParentMaterialId", null);
				UnitConvert unitConvertRate = UnitConvertServiceHelper.GetUnitConvertRate(base.View.Context, new GetUnitConvertRateArgs
				{
					MasterId = DataEntityExtend.GetDynamicValue<long>(dynamicValue2, "msterID", 0L),
					SourceUnitId = value,
					DestUnitId = value2
				});
				base.View.BillBusinessInfo.GetField("FUnitIDLot");
				base.View.BillBusinessInfo.GetField("FBaseUnitIDLot");
				int num = entryRowCount - 1;
				base.View.Model.SetValue("FMaterialIdLotBased", dynamicValue, num);
				DynamicObject entityDataObject = this.Model.GetEntityDataObject(entryEntity, num);
				if (entityDataObject != null && field != null)
				{
					long dynamicValue3 = DataEntityExtend.GetDynamicValue<long>(entityDataObject, "UnitIDLot_Id", 0L);
					long dynamicValue4 = DataEntityExtend.GetDynamicValue<long>(entityDataObject, "BaseUnitIDLot_Id", 0L);
					UnitConvert unitConvertRate2 = UnitConvertServiceHelper.GetUnitConvertRate(base.View.Context, new GetUnitConvertRateArgs
					{
						MasterId = DataEntityExtend.GetDynamicValue<long>(dyChildMaterial, "msterID", 0L),
						SourceUnitId = dynamicValue3,
						DestUnitId = dynamicValue4
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
				base.View.UpdateView("FMaterialIdLotBased", num);
				base.View.UpdateView("FStairDosage");
			}
		}

		// Token: 0x06000629 RID: 1577 RVA: 0x0004A1F7 File Offset: 0x000483F7
		private void SetColorForChangedField()
		{
			this.SetColorForEntry("FTreeEntity", "ChangeLabel", "ECNGroup");
			this.SetColorForEntry("FCobyEntity", "CobyChangeLabel", "EcnCobyGroup");
		}

		// Token: 0x0600062A RID: 1578 RVA: 0x0004A304 File Offset: 0x00048504
		private void SetColorForEntry(string entryKey, string changeLabel, string ecnGroup)
		{
			int entryCurrentRowIndex = base.View.Model.GetEntryCurrentRowIndex(entryKey);
			base.View.UpdateView(entryKey);
			base.View.SetEntityFocusRow(entryKey, entryCurrentRowIndex);
			Func<DynamicProperty, bool> func = (DynamicProperty prop) => StringUtils.EqualsIgnoreCase(prop.Name, "Id") || StringUtils.EqualsIgnoreCase(prop.Name, "Seq") || StringUtils.EqualsIgnoreCase(prop.Name, "ChangeLabel") || StringUtils.EqualsIgnoreCase(prop.Name, "AuxPropId") || StringUtils.EqualsIgnoreCase(prop.Name, "CobyChangeLabel") || StringUtils.EqualsIgnoreCase(prop.Name, "RowId") || StringUtils.EqualsIgnoreCase(prop.Name, "ECNParentRowId");
			Entity entity = base.View.BusinessInfo.GetEntity(entryKey);
			DynamicObjectCollection entityDataObject = this.Model.GetEntityDataObject(entity);
			if (ListUtils.IsEmpty<DynamicObject>(entityDataObject))
			{
				return;
			}
			List<DynamicObject> source = (from i in entityDataObject
			where DataEntityExtend.GetDynamicObjectItemValue<int>(i, changeLabel, 0) == 0 || DataEntityExtend.GetDynamicObjectItemValue<int>(i, changeLabel, 0) == 1
			select i).ToList<DynamicObject>();
			Dictionary<string, List<DynamicObject>> dictionary = (from k in source
			group k by DataEntityExtend.GetDynamicObjectItemValue<string>(k, ecnGroup, null)).ToDictionary((IGrouping<string, DynamicObject> i) => i.Key, (IGrouping<string, DynamicObject> v) => v.ToList<DynamicObject>());
			EntryGrid control = base.View.GetControl<EntryGrid>(entryKey);
			Dictionary<string, Field> dictionary2 = entity.Fields.ToDictionary((Field x) => x.PropertyName);
			foreach (KeyValuePair<string, List<DynamicObject>> keyValuePair in dictionary)
			{
				if (!ListUtils.IsEmpty<DynamicObject>(keyValuePair.Value) && keyValuePair.Value.Count<DynamicObject>() == 2)
				{
					int num = entityDataObject.IndexOf(keyValuePair.Value[1]);
					List<string> diffValuePropName = MFGBillUtil.GetDiffValuePropName(keyValuePair.Value[0], keyValuePair.Value[1], entity.DynamicObjectType, func);
					foreach (string key in diffValuePropName)
					{
						Field field;
						if (dictionary2.TryGetValue(key, out field))
						{
							control.SetBackcolor(field.Key, "#FF0000", num);
						}
					}
				}
			}
		}

		// Token: 0x0600062B RID: 1579 RVA: 0x0004A540 File Offset: 0x00048740
		public List<string> GetDiffValuePropName(DynamicObject one, DynamicObject two, DynamicObjectType type)
		{
			List<string> list = new List<string>();
			foreach (DynamicProperty dynamicProperty in type.Properties)
			{
				if (!StringUtils.EqualsIgnoreCase(dynamicProperty.Name, "Id") && !StringUtils.EqualsIgnoreCase(dynamicProperty.Name, "Seq") && !StringUtils.EqualsIgnoreCase(dynamicProperty.Name, "ChangeLabel") && !StringUtils.EqualsIgnoreCase(dynamicProperty.Name, "AuxPropId"))
				{
					object obj = one[dynamicProperty.Name];
					object obj2 = two[dynamicProperty.Name];
					if (obj != null || obj2 != null)
					{
						Type left = (obj != null) ? obj.GetType() : obj2.GetType();
						if (left == typeof(DynamicObject))
						{
							string dynamicValue = DataEntityExtend.GetDynamicValue<string>(obj as DynamicObject, "Id", null);
							string dynamicValue2 = DataEntityExtend.GetDynamicValue<string>(obj2 as DynamicObject, "Id", null);
							if ((ObjectUtils.IsNullOrEmptyOrWhiteSpace(dynamicValue) && !ObjectUtils.IsNullOrEmptyOrWhiteSpace(dynamicValue2)) || (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(dynamicValue) && ObjectUtils.IsNullOrEmptyOrWhiteSpace(dynamicValue2)) || !StringUtils.EqualsIgnoreCase(dynamicValue, dynamicValue2))
							{
								list.Add(dynamicProperty.Name);
							}
							else if (obj == null || obj2 == null)
							{
								list.Add(dynamicProperty.Name);
							}
						}
						else if (left == typeof(int))
						{
							if (OtherExtend.ConvertTo<int>(obj, 0) != OtherExtend.ConvertTo<int>(obj2, 0))
							{
								list.Add(dynamicProperty.Name);
							}
							else if ((OtherExtend.ConvertTo<int>(obj, 0) == 0 && OtherExtend.ConvertTo<int>(obj2, 0) != 0) || (OtherExtend.ConvertTo<int>(obj, 0) != 0 && OtherExtend.ConvertTo<int>(obj2, 0) == 0))
							{
								list.Add(dynamicProperty.Name);
							}
						}
						else if (left == typeof(long))
						{
							if (OtherExtend.ConvertTo<long>(obj, 0L) != OtherExtend.ConvertTo<long>(obj2, 0L))
							{
								list.Add(dynamicProperty.Name);
							}
							else if ((OtherExtend.ConvertTo<long>(obj, 0L) == 0L && OtherExtend.ConvertTo<long>(obj2, 0L) != 0L) || (OtherExtend.ConvertTo<long>(obj, 0L) != 0L && OtherExtend.ConvertTo<long>(obj2, 0L) == 0L))
							{
								list.Add(dynamicProperty.Name);
							}
						}
						else if (left == typeof(decimal))
						{
							if (OtherExtend.ConvertTo<decimal>(obj, 0m) != OtherExtend.ConvertTo<decimal>(obj2, 0m))
							{
								list.Add(dynamicProperty.Name);
							}
							else if ((OtherExtend.ConvertTo<decimal>(obj, 0m) == 0m && OtherExtend.ConvertTo<decimal>(obj2, 0m) != 0m) || (OtherExtend.ConvertTo<decimal>(obj, 0m) != 0m && OtherExtend.ConvertTo<decimal>(obj2, 0m) == 0m))
							{
								list.Add(dynamicProperty.Name);
							}
						}
						else if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(obj) || !ObjectUtils.IsNullOrEmptyOrWhiteSpace(obj2))
						{
							if (ObjectUtils.IsNullOrEmptyOrWhiteSpace(obj) || ObjectUtils.IsNullOrEmptyOrWhiteSpace(obj2))
							{
								list.Add(dynamicProperty.Name);
							}
							else if (!StringUtils.EqualsIgnoreCase(obj.ToString(), obj2.ToString()))
							{
								list.Add(dynamicProperty.Name);
							}
						}
					}
				}
			}
			return list;
		}

		// Token: 0x0600062C RID: 1580 RVA: 0x0004A8C0 File Offset: 0x00048AC0
		private void IsUseECREntryBomId()
		{
			string value = MFGBillUtil.GetValue<string>(base.View.Model, "FApplyBillNo", -1, null, null);
			List<long> list = new List<long>();
			string value2 = MFGBillUtil.GetValue<string>(base.View.Model, "FECREnTryId", -1, null, null);
			if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(value2))
			{
				list = value2.Split(new char[]
				{
					','
				}).Distinct<string>().ToList<string>().ConvertAll<long>((string i) => Convert.ToInt64(i));
			}
			if (!ListUtils.IsEmpty<long>(list) && !ObjectUtils.IsNullOrEmptyOrWhiteSpace(value))
			{
				this.ecrEntryBomIds = ECRApplyServiceHelper.GetECREntryBomId(base.Context, value, list);
				if (this.ecrEntryBomIds.Contains(0L))
				{
					this.ecrEntryBomIds.Clear();
					return;
				}
				string value3 = MFGBillUtil.GetValue<string>(base.View.Model, "FBillTypeID", -1, null, null);
				DynamicObject dynamicObject = BusinessDataServiceHelper.LoadBillTypePara(base.Context, "ENG_ECNBillParameter", value3, true);
				this.useECREntryBomId = DataEntityExtend.GetDynamicValue<bool>(dynamicObject, "OnlyECRBomId", false);
			}
		}

		// Token: 0x0600062D RID: 1581 RVA: 0x0004AA0C File Offset: 0x00048C0C
		private void ChangePositonNo(DataChangedEventArgs e)
		{
			Entity entity = base.View.BusinessInfo.GetEntity("FTreeEntity");
			DynamicObject entityDataObject = this.Model.GetEntityDataObject(entity, e.Row);
			int num = 2;
			string dynamicValue = DataEntityExtend.GetDynamicValue<string>(entityDataObject, "ChangeLabel", null);
			string dynamicValue2 = DataEntityExtend.GetDynamicValue<string>(entityDataObject, "MATERIALTYPE", null);
			bool dynamicValue3 = DataEntityExtend.GetDynamicValue<bool>(entityDataObject, "IskeyItem", false);
			if (ObjectUtils.IsNullOrEmptyOrWhiteSpace(dynamicValue) || !StringUtils.EqualsIgnoreCase(dynamicValue, num.ToString()) || !StringUtils.EqualsIgnoreCase(dynamicValue2, "1") || !dynamicValue3)
			{
				return;
			}
			string encGroup = DataEntityExtend.GetDynamicValue<string>(entityDataObject, "ECNGroup", null);
			DynamicObjectCollection entityDataObject2 = this.Model.GetEntityDataObject(entity);
			List<DynamicObject> list = (from i in entityDataObject2
			where StringUtils.EqualsIgnoreCase(DataEntityExtend.GetDynamicValue<string>(i, "ECNGroup", null), encGroup) && StringUtils.EqualsIgnoreCase(DataEntityExtend.GetDynamicValue<string>(i, "MATERIALTYPE", null), "3")
			select i).ToList<DynamicObject>();
			foreach (DynamicObject dynamicObject in list)
			{
				DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "POSITIONNO", e.NewValue);
				base.View.UpdateView("FPOSITIONNO", entityDataObject2.IndexOf(dynamicObject));
			}
		}

		// Token: 0x0600062E RID: 1582 RVA: 0x0004AB44 File Offset: 0x00048D44
		private string GetBomIdCopyFilter(string filter, int row, out bool result)
		{
			Entity entity = base.View.BusinessInfo.GetEntity("FCobyEntity");
			DynamicObjectCollection entityDataObject = base.View.Model.GetEntityDataObject(entity);
			DynamicObject value = MFGBillUtil.GetValue<DynamicObject>(base.View.Model, "FMATERIALIDCOBY", row, null, null);
			DynamicObject value2 = MFGBillUtil.GetValue<DynamicObject>(base.View.Model, "FCobyBomVersion", row, null, null);
			if (value2 == null)
			{
				base.View.ShowErrMessage(ResManager.LoadKDString("当前行BOM版本不可为空", "015072030041398", 7, new object[0]), "", 0);
				result = false;
				return "1=0";
			}
			long dynamicValue = DataEntityExtend.GetDynamicValue<long>(value2, "UseOrgId_Id", 0L);
			string dynamicValue2 = DataEntityExtend.GetDynamicValue<string>(value2, "BOMUSE", null);
			long dynamicValue3 = DataEntityExtend.GetDynamicValue<long>(entityDataObject[row], "AuxPropId_Id", 0L);
			DynamicObject dynamicValue4 = DataEntityExtend.GetDynamicValue<DynamicObject>(entityDataObject[row], "AuxPropId", null);
			filter = this.GetBomIdFilter(filter, value, dynamicValue, dynamicValue3, dynamicValue4);
			filter = StringUtils.JoinFilterString(filter, string.Format(" FDOCUMENTSTATUS='C' AND FFORBIDSTATUS='A' AND FBOMUSE='{0}' ", dynamicValue2), "AND");
			result = true;
			return filter;
		}

		// Token: 0x0600062F RID: 1583 RVA: 0x0004AC54 File Offset: 0x00048E54
		private string GetBomIdFilter(string filter, DynamicObject mtrl, long useOrgId, long auxpropId, DynamicObject auxpropData)
		{
			if (filter == null)
			{
				filter = string.Empty;
			}
			string text = string.Empty;
			text = " FMATERIALID=0 ";
			if (mtrl != null)
			{
				long dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<long>(mtrl, "MsterId", 0L);
				List<long> list = new List<long>();
				list = BOMServiceHelper.GetAllBomIdByAuxProp(base.View.Context, dynamicObjectItemValue, useOrgId, auxpropData, false, true);
				if (!ListUtils.IsEmpty<long>(list))
				{
					text = string.Format(" FID IN ({0}) ", string.Join<long>(",", list));
				}
				else
				{
					text = string.Format(" FID={0}", 0);
				}
			}
			filter = StringUtils.JoinFilterString(filter, text, "AND");
			return filter;
		}

		// Token: 0x040002B3 RID: 691
		public Dictionary<int, AbstractItemControler> controls = new Dictionary<int, AbstractItemControler>();

		// Token: 0x040002B4 RID: 692
		public Dictionary<long, BaseDataControlPolicyTargetOrgEntry> bomEntryCtrlSettings;

		// Token: 0x040002B5 RID: 693
		public Queue<DynamicObject> RowControlBuffer = new Queue<DynamicObject>();

		// Token: 0x040002B6 RID: 694
		private List<long> ecrEntryBomIds = new List<long>();

		// Token: 0x040002B7 RID: 695
		private bool useECREntryBomId;

		// Token: 0x040002B8 RID: 696
		private List<DynamicObject> delEcnEntrys = new List<DynamicObject>();
	}
}
