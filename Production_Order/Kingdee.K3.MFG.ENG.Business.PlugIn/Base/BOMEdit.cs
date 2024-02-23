using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kingdee.BOS;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.Bill.PlugIn.Args;
using Kingdee.BOS.Core.DependencyRules;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.Enums;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.BarElement;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Core.Metadata.FormElement;
using Kingdee.BOS.Core.Permission;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Orm.Metadata.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.BOS.VerificationHelper;
using Kingdee.K3.BD.ServiceHelper;
using Kingdee.K3.Core.BD;
using Kingdee.K3.Core.BD.ServiceArgs;
using Kingdee.K3.Core.MFG.ENG.BomTree;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.Core.MFG.PLN.ParamOption;
using Kingdee.K3.Core.MFG.Utils;
using Kingdee.K3.MFG.BusinessCommon;
using Kingdee.K3.MFG.ENG.Business.PlugIn.Utils;
using Kingdee.K3.MFG.ServiceHelper;
using Kingdee.K3.MFG.ServiceHelper.ENG;
using Kingdee.K3.MFG.ServiceHelper.PRD;
using Kingdee.K3.PLM.Common.Core.ServiceHelper;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.Base
{
	// Token: 0x02000008 RID: 8
	public class BOMEdit : BaseControlEdit
	{
		// Token: 0x1700000D RID: 13
		// (get) Token: 0x060000AA RID: 170 RVA: 0x00008567 File Offset: 0x00006767
		// (set) Token: 0x060000AB RID: 171 RVA: 0x0000856F File Offset: 0x0000676F
		private PermissionAuthResult authSynsResult { get; set; }

		// Token: 0x1700000E RID: 14
		// (get) Token: 0x060000AC RID: 172 RVA: 0x00008578 File Offset: 0x00006778
		// (set) Token: 0x060000AD RID: 173 RVA: 0x00008580 File Offset: 0x00006780
		private object isImportObj { get; set; }

		// Token: 0x060000AE RID: 174 RVA: 0x0000858C File Offset: 0x0000678C
		public override void DataUpdateEnd()
		{
			base.DataUpdateEnd();
			Entity entryEntity = base.View.BillBusinessInfo.GetEntryEntity("FTreeEntity");
			DynamicObjectCollection entityDataObject = base.View.Model.GetEntityDataObject(entryEntity);
			for (int i = entityDataObject.Count - 1; i >= 0; i--)
			{
				base.View.UpdateView("FReplaceGroup", i);
			}
		}

		// Token: 0x060000AF RID: 175 RVA: 0x000085EC File Offset: 0x000067EC
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
			if (!base.View.Context.IsMultiOrg)
			{
				base.View.RuleContainer.AddPluginRule("FTreeEntity", 1, new Action<DynamicObject, object>(this.SetSupplyorg), new string[]
				{
					"FMATERIALIDCHILD"
				});
			}
			base.View.RuleContainer.AddPluginRule("FTreeEntity", 1, new Action<DynamicObject, object>(this.SetReplaceGroupByMaterial), new string[]
			{
				"FMATERIALIDCHILD"
			});
			if (base.View.ParentFormView != null && base.View.ParentFormView.OpenParameter.FormId.Equals("PLN_MAINTAINLLC"))
			{
				this.param = MFGBillUtil.GetParentFormSession<LowestBomCodeOption.T_NewBomFromIntegrityCheckParam>(base.View, "FormInputParam");
			}
			this.isImportObj = base.View.OpenParameter.GetCustomParameter("ImportView");
			if ((ObjectUtils.IsNullOrEmpty(this.isImportObj) || !StringUtils.EqualsIgnoreCase(this.isImportObj.ToString(), "TRUE")) && base.Context.ServiceType == null)
			{
				this.fieldNameList = this.GetRecordFields();
			}
		}

		// Token: 0x060000B0 RID: 176 RVA: 0x00008714 File Offset: 0x00006914
		public override void VerifyImportData(VerifyImportDataArgs e)
		{
			base.VerifyImportData(e);
			this.canUpdate = true;
		}

		// Token: 0x060000B1 RID: 177 RVA: 0x00008724 File Offset: 0x00006924
		public override void AfterBindData(EventArgs e)
		{
			base.AfterBindData(e);
			if (this.param.SourceCaller == 1 && MFGBillUtil.GetValue<long>(base.View.Model, "FMATERIALID", -1, 0L, null) == 0L)
			{
				base.View.Model.SetValue("FCreateOrgId", this.param.OrgId);
				base.View.Model.SetValue("FMATERIALID", this.param.MaterialId);
			}
			this.InitBomRowIdGroup(false);
			this.SetBomEntrySource();
			if (base.Context.IsStandardEdition())
			{
				FormUtils.StdLockField(base.View, "FSupplyMode");
			}
			this.SetBarItemHideWhenParentViewIsBomTree();
		}

		// Token: 0x060000B2 RID: 178 RVA: 0x000087E0 File Offset: 0x000069E0
		private void SetBarItemHideWhenParentViewIsBomTree()
		{
			BomExpandNodeTreeMode bomExpandNodeTreeMode;
			if (!this.GetBomExpandNodeTreeMode(out bomExpandNodeTreeMode))
			{
				return;
			}
			base.SetAllMainBarItemVisible(false);
		}

		// Token: 0x060000B3 RID: 179 RVA: 0x00008800 File Offset: 0x00006A00
		private void SetSupplyorg(DynamicObject dyObj, dynamic obj)
		{
			long num = Convert.ToInt64(MFGBillUtil.GetValue<int>(base.View.Model, "FUSEORGID", -1, 0, null));
			long num2 = Convert.ToInt64(dyObj["MATERIALIDCHILD_Id"]);
			int num3 = Convert.ToInt32(dyObj["Seq"]);
			if (num2 > 0L)
			{
				base.View.Model.SetValue("FSUPPLYORG", num, num3 - 1);
				return;
			}
			base.View.Model.SetValue("FSUPPLYORG", 0, num3 - 1);
		}

		// Token: 0x060000B4 RID: 180 RVA: 0x00008890 File Offset: 0x00006A90
		private void SetReplaceGroupByMaterial(DynamicObject dyObj, dynamic obj)
		{
			long num = (long)Convert.ToInt32(dyObj["MATERIALIDCHILD_Id"]);
			long num2 = (long)Convert.ToInt32(dyObj["ReplaceGroup"]);
			if (num > 0L && num2 <= 0L)
			{
				this.SetReplaceGroup();
			}
		}

		// Token: 0x060000B5 RID: 181 RVA: 0x00008958 File Offset: 0x00006B58
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			base.BarItemClick(e);
			string barItemKey;
			switch (barItemKey = e.BarItemKey)
			{
			case "tbBomBatchEdit":
				this.ShowBomTreeEdit("ENG_BOMTREE");
				return;
			case "tbBomSynUpdate":
				if (MFGCommonUtil.IsOnlyQueryUser(base.Context))
				{
					base.View.ShowErrMessage(string.Format(ResManager.LoadKDString("用户“{0}”为仅查询许可用户，不能执行 “同步更新” 操作，请联系系统管理员！", "015072030034925", 7, new object[0]), base.Context.UserName), "", 0);
					e.Cancel = true;
					return;
				}
				this.ShowListDatas("tbBomSynUpdate");
				return;
			case "tbBomModifyLog":
				this.ShowBomModifyLogEdit("ENG_BomModifyLog");
				return;
			case "tbQueryEcn":
				this.OpenEcnList(e);
				return;
			case "tbForbid":
			{
				FormMetadata formMetadata = (FormMetadata)MetaDataServiceHelper.Load(base.View.Context, "ENG_BOM_BILLPARAM", true);
				DynamicObject dynamicObject = UserParamterServiceHelper.Load(base.View.Context, formMetadata.BusinessInfo, base.View.Context.UserId, "ENG_BOM", "UserParameter");
				this.IsEnablelForbidreason = DataEntityExtend.GetDynamicValue<bool>(dynamicObject, "EnablelForbidreason", false);
				if (this.IsEnablelForbidreason)
				{
					e.Cancel = true;
					DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
					dynamicFormShowParameter.FormId = "ENG_FORBIDREASON";
					dynamicFormShowParameter.PageId = Guid.NewGuid().ToString();
					dynamicFormShowParameter.OpenStyle.ShowType = 6;
					base.View.ShowForm(dynamicFormShowParameter, delegate(FormResult x)
					{
						object returnData = x.ReturnData;
						if (StringUtils.EqualsIgnoreCase(returnData.ToString(), "FBTNCANCEL"))
						{
							e.Cancel = true;
							return;
						}
						if (ObjectUtils.IsNullOrEmpty(x.ReturnData))
						{
							this.forbidReason = null;
						}
						else
						{
							this.forbidReason = returnData.ToString();
						}
						this.View.InvokeFormOperation("Forbid");
					});
					return;
				}
				break;
			}
			case "tbSelectPLMMaterial":
			{
				if (!this.IsHasLicense("5795bb97b0a8c4"))
				{
					base.View.ShowErrMessage(ResManager.LoadKDString("没有购买PLM的研发物料模块", "015072030042066", 7, new object[0]), "", 0);
					e.Cancel = true;
					return;
				}
				DynamicObject dynamicValue = DataEntityExtend.GetDynamicValue<DynamicObject>(base.View.Model.DataObject, "MATERIALID", null);
				if (ObjectUtils.IsNullOrEmpty(dynamicValue))
				{
					base.View.ShowErrMessage(ResManager.LoadKDString("没有录入物料，无法查询对应的PLM信息", "015072030042128", 7, new object[0]), "", 0);
					e.Cancel = true;
					return;
				}
				DynamicObject plminfo = this.GetPLMInfo(dynamicValue, "tbSelectPLMMaterial");
				if (ObjectUtils.IsNullOrEmpty(plminfo))
				{
					base.View.ShowErrMessage(ResManager.LoadKDString("该物料没有对应的PLM信息", "015072000021981", 7, new object[0]), "", 0);
					e.Cancel = true;
					return;
				}
				BillShowParameter billShowParameter = new BillShowParameter
				{
					FormId = LicenseHelper.GetMetaDataID(Convert.ToInt64(plminfo["FCATEGORYID"].ToString())),
					Status = 1,
					PKey = plminfo["FID"].ToString()
				};
				base.View.ShowForm(billShowParameter, delegate(FormResult result)
				{
				});
				return;
			}
			case "tbSelectPLMBom":
			{
				if (!this.IsHasLicense("58f85c7cbff07f"))
				{
					base.View.ShowErrMessage(ResManager.LoadKDString("没有购买PLM的设计BOM模块", "015072030042069", 7, new object[0]), "", 0);
					e.Cancel = true;
					return;
				}
				DynamicObject plminfo2 = this.GetPLMInfo(this.Model.DataObject, "tbSelectPLMBom");
				if (ObjectUtils.IsNullOrEmpty(plminfo2))
				{
					base.View.ShowErrMessage(ResManager.LoadKDString("该BOM没有对应的PLM信息", "015072000021983", 7, new object[0]), "", 0);
					e.Cancel = true;
					return;
				}
				BillShowParameter billShowParameter2 = new BillShowParameter
				{
					FormId = LicenseHelper.GetMetaDataID(Convert.ToInt64(plminfo2["FCATEGORYID"].ToString())),
					Status = 1,
					PKey = plminfo2["FID"].ToString()
				};
				base.View.ShowForm(billShowParameter2, delegate(FormResult result)
				{
				});
				return;
			}
			case "tbPageBomIntCheck":
				this.BomIntegrityCheck();
				break;

				return;
			}
		}

		// Token: 0x060000B6 RID: 182 RVA: 0x00008E28 File Offset: 0x00007028
		public override void BeforeClosed(BeforeClosedEventArgs e)
		{
			base.BeforeClosed(e);
			if (base.View.ParentFormView != null)
			{
				string id = base.View.ParentFormView.BillBusinessInfo.GetForm().Id;
				if (!StringUtils.EqualsIgnoreCase(id, "WF_AssignmentApproval"))
				{
					(base.View.ParentFormView as IDynamicFormViewService).CustomEvents("BOMTREE", "BOMTREETOBOM", "VIEWCLOSE");
					base.View.SendDynamicFormAction(base.View.ParentFormView);
				}
			}
		}

		// Token: 0x060000B7 RID: 183 RVA: 0x00008F58 File Offset: 0x00007158
		public override void EntryBarItemClick(BarItemClickEventArgs e)
		{
			base.EntryBarItemClick(e);
			string barItemKey;
			if ((barItemKey = e.BarItemKey) != null)
			{
				if (<PrivateImplementationDetails>{68AB44FA-F3AC-4F72-AC7E-FDDDF03A1557}.$$method0x6000078-1 == null)
				{
					<PrivateImplementationDetails>{68AB44FA-F3AC-4F72-AC7E-FDDDF03A1557}.$$method0x6000078-1 = new Dictionary<string, int>(12)
					{
						{
							"tbSetupRep",
							0
						},
						{
							"tbRemoveRep",
							1
						},
						{
							"tbGetRep",
							2
						},
						{
							"tbReGetRep",
							3
						},
						{
							"tbDeleteEntry",
							4
						},
						{
							"tbCopyEntryRow",
							5
						},
						{
							"tbMoveUp",
							6
						},
						{
							"tbMoveDown",
							7
						},
						{
							"tbExpandNodes",
							8
						},
						{
							"tbCollapseNodes",
							9
						},
						{
							"tbGetLadderLoss",
							10
						},
						{
							"tbUpdateLadderLoss",
							11
						}
					};
				}
				int num;
				if (<PrivateImplementationDetails>{68AB44FA-F3AC-4F72-AC7E-FDDDF03A1557}.$$method0x6000078-1.TryGetValue(barItemKey, out num))
				{
					switch (num)
					{
					case 0:
						this.RepaceSetup();
						return;
					case 1:
						this.ReplaceDelte();
						return;
					case 2:
						this.GetSubstitute(false);
						return;
					case 3:
						this.ReGetSubstitute();
						return;
					case 4:
						e.Cancel = !this.ValidateRemoveRep();
						return;
					case 5:
						this.bIsReplaceDataChange = true;
						return;
					case 6:
						this.bMoveUpOrDown = true;
						this.ExchangeEntryRow(true);
						return;
					case 7:
						this.bMoveUpOrDown = true;
						this.ExchangeEntryRow(false);
						return;
					case 8:
					{
						TreeEntryGrid control = base.View.GetControl<TreeEntryGrid>("FTreeEntity");
						DynamicObjectCollection entryData = this.Model.GetEntityDataObject(base.View.BusinessInfo.GetEntryEntity("FTreeEntity"));
						if (ListUtils.IsEmpty<DynamicObject>(entryData))
						{
							return;
						}
						List<string> list = (from t in entryData
						where "3".Equals(OtherExtend.ConvertTo<string>(t["MaterialType"], null))
						select t into x
						select OtherExtend.ConvertTo<string>(x["ParentRowId"], null)).ToList<string>();
						Dictionary<string, int> dictionary = entryData.ToDictionary((DynamicObject x) => OtherExtend.ConvertTo<string>(x["RowId"], null), (DynamicObject y) => entryData.IndexOf(y));
						if (ListUtils.IsEmpty<string>(list))
						{
							return;
						}
						int num2 = 0;
						using (List<string>.Enumerator enumerator = list.GetEnumerator())
						{
							while (enumerator.MoveNext())
							{
								string key = enumerator.Current;
								if (dictionary.TryGetValue(key, out num2))
								{
									control.ExpandedRow(num2);
								}
							}
							return;
						}
						break;
					}
					case 9:
						break;
					case 10:
						goto IL_3C2;
					case 11:
						this.UpdateLadderLoss(false);
						return;
					default:
						return;
					}
					TreeEntryGrid control2 = base.View.GetControl<TreeEntryGrid>("FTreeEntity");
					DynamicObjectCollection entryData2 = this.Model.GetEntityDataObject(base.View.BusinessInfo.GetEntryEntity("FTreeEntity"));
					if (ListUtils.IsEmpty<DynamicObject>(entryData2))
					{
						return;
					}
					List<string> list2 = (from t in entryData2
					where "3".Equals(OtherExtend.ConvertTo<string>(t["MaterialType"], null))
					select t into x
					select OtherExtend.ConvertTo<string>(x["ParentRowId"], null)).ToList<string>();
					Dictionary<string, int> dictionary2 = entryData2.ToDictionary((DynamicObject x) => OtherExtend.ConvertTo<string>(x["RowId"], null), (DynamicObject y) => entryData2.IndexOf(y));
					if (ListUtils.IsEmpty<string>(list2))
					{
						return;
					}
					int num3 = 0;
					using (List<string>.Enumerator enumerator2 = list2.GetEnumerator())
					{
						while (enumerator2.MoveNext())
						{
							string key2 = enumerator2.Current;
							if (dictionary2.TryGetValue(key2, out num3))
							{
								control2.CollapsedRow(num3);
							}
						}
						return;
					}
					IL_3C2:
					this.GetLadderLoss(false);
					return;
				}
			}
		}

		// Token: 0x060000B8 RID: 184 RVA: 0x00009354 File Offset: 0x00007554
		public override void AfterEntryBarItemClick(AfterBarItemClickEventArgs e)
		{
			base.AfterEntryBarItemClick(e);
			string barItemKey;
			if ((barItemKey = e.BarItemKey) != null)
			{
				if (!(barItemKey == "tbMoveUp") && !(barItemKey == "tbMoveDown"))
				{
					return;
				}
				this.bMoveUpOrDown = false;
			}
		}

		// Token: 0x060000B9 RID: 185 RVA: 0x000093E0 File Offset: 0x000075E0
		private void ExchangeEntryRow(bool isMoveUp)
		{
			EntryEntity entryEntity = base.View.BusinessInfo.GetEntryEntity("FTreeEntity");
			DynamicObjectCollection entityDataObject = base.View.Model.GetEntityDataObject(entryEntity);
			Dictionary<int, IGrouping<int, DynamicObject>> dictionary = (from r in entityDataObject
			where DataEntityExtend.GetDynamicObjectItemValue<int>(r, "ReplaceGroup", 0) > 0
			select r into e
			group e by DataEntityExtend.GetDynamicObjectItemValue<int>(e, "ReplaceGroup", 0)).ToDictionary((IGrouping<int, DynamicObject> g) => g.Key);
			if (ListUtils.IsEmpty<KeyValuePair<int, IGrouping<int, DynamicObject>>>(dictionary))
			{
				return;
			}
			int dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<int>(SysBclExtend.GetSelectedRowDatas(base.View, entryEntity.Key).FirstOrDefault<DynamicObject>(), "ReplaceGroup", 0);
			int num = isMoveUp ? (dynamicObjectItemValue - 1) : (dynamicObjectItemValue + 1);
			if (num < 1 || num > dictionary.Count)
			{
				return;
			}
			IGrouping<int, DynamicObject> grouping;
			dictionary.TryGetValue(dynamicObjectItemValue, out grouping);
			IGrouping<int, DynamicObject> grouping2;
			dictionary.TryGetValue(num, out grouping2);
			if (ListUtils.IsEmpty<DynamicObject>(grouping) || ListUtils.IsEmpty<DynamicObject>(grouping2))
			{
				return;
			}
			foreach (DynamicObject item in grouping)
			{
				base.View.Model.DeleteEntryRow("FTreeEntity", entityDataObject.IndexOf(item));
			}
			int num2 = 0;
			int num3 = grouping2.Count<DynamicObject>();
			for (int i = 0; i < num3; i++)
			{
				DynamicObject dynamicObject = grouping2.ElementAt(i);
				DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "ReplaceGroup", dynamicObjectItemValue);
				int num4 = entityDataObject.IndexOf(dynamicObject);
				if (i == 0)
				{
					num2 = num4;
				}
				else if (isMoveUp)
				{
					if (num4 < num2)
					{
						num2 = num4;
					}
				}
				else if (num4 > num2)
				{
					num2 = num4;
				}
			}
			num2 = (isMoveUp ? num2 : (num2 + 1));
			DynamicObject dynamicObject2 = grouping.FirstOrDefault((DynamicObject w) => string.IsNullOrWhiteSpace(DataEntityExtend.GetDynamicObjectItemValue<string>(w, "ParentRowId", null)) && DataEntityExtend.GetDynamicObjectItemValue<bool>(w, "IskeyItem", false));
			foreach (DynamicObject dynamicObject3 in grouping)
			{
				DataEntityExtend.SetDynamicObjectItemValue(dynamicObject3, "ReplaceGroup", num);
				if (dynamicObject2 != dynamicObject3)
				{
					base.View.Model.CreateNewEntryRow(entryEntity, num2, dynamicObject3);
				}
			}
			if (dynamicObject2 != null)
			{
				base.View.Model.CreateNewEntryRow(entryEntity, num2, dynamicObject2);
			}
			base.View.UpdateView("FTreeEntity");
			base.View.SetEntityFocusRow("FTreeEntity", num2);
			TreeEntryGrid control = base.View.GetControl<TreeEntryGrid>("FTreeEntity");
			control.CollapsedRow(num2);
		}

		// Token: 0x060000BA RID: 186 RVA: 0x0000969C File Offset: 0x0000789C
		public override void AfterSave(AfterSaveEventArgs e)
		{
			base.AfterSave(e);
			this.InitBomRowIdGroup(true);
		}

		// Token: 0x060000BB RID: 187 RVA: 0x000096AC File Offset: 0x000078AC
		public override void BeforeDeleteRow(BeforeDeleteRowEventArgs e)
		{
			base.BeforeDeleteRow(e);
			string entityKey;
			if ((entityKey = e.EntityKey) != null)
			{
				if (!(entityKey == "FBOMCHILDLOTBASEDQTY"))
				{
					if (!(entityKey == "FTreeEntity"))
					{
						return;
					}
					this.ResetRepGroup(e.Row);
				}
				else if (base.View.Model.GetEntryRowCount("FBOMCHILDLOTBASEDQTY") == 1)
				{
					base.View.ShowMessage(ResManager.LoadKDString("阶梯用量表体至少保留一行！", "015072000002168", 7, new object[0]), 0);
					e.Cancel = true;
					return;
				}
			}
		}

		// Token: 0x060000BC RID: 188 RVA: 0x00009754 File Offset: 0x00007954
		private List<string> GetRecordFields()
		{
			List<string> list = new List<string>();
			FormMetadata formMetadata = (FormMetadata)MetaDataServiceHelper.Load(base.View.Context, base.View.BillBusinessInfo.GetForm().Id, true);
			List<Field> list2 = (from f in formMetadata.BusinessInfo.GetFieldList()
			where StringUtils.EqualsIgnoreCase(f.EntityKey, "FTreeEntity") || f is ProxyField
			select f).ToList<Field>();
			foreach (Field field in list2)
			{
				Appearance appearance = formMetadata.GetLayoutInfo().GetAppearance(field.Key);
				if (appearance != null && (appearance.Visible & this.vis) == this.vis)
				{
					if (field is ProxyField && !ObjectUtils.IsNullOrEmpty(field.ControlField) && StringUtils.EqualsIgnoreCase(field.ControlField.EntityKey, "FTreeEntity"))
					{
						list.Add(field.ControlField.Key);
					}
					else
					{
						list.Add(field.Key);
					}
				}
			}
			List<string> list3 = list.Distinct<string>().ToList<string>();
			if (list3.Contains("FEntrySource"))
			{
				list3.Remove("FEntrySource");
			}
			if (list3.Contains("FModifiedField"))
			{
				list3.Remove("FModifiedField");
			}
			return list3;
		}

		// Token: 0x060000BD RID: 189 RVA: 0x000098C4 File Offset: 0x00007AC4
		private void SetColorForChangedField(DataChangedEventArgs e)
		{
			if (ListUtils.IsEmpty<string>(this.fieldNameList))
			{
				return;
			}
			if (!this.fieldNameList.Contains(e.Field.Key))
			{
				return;
			}
			if ((!ObjectUtils.IsNullOrEmptyOrWhiteSpace(e.OldValue) && e.OldValue.Equals(e.NewValue)) || (ObjectUtils.IsNullOrEmptyOrWhiteSpace(e.OldValue) && ObjectUtils.IsNullOrEmptyOrWhiteSpace(e.NewValue)))
			{
				return;
			}
			EntryGrid control = base.View.GetControl<EntryGrid>("FTreeEntity");
			control.SetBackcolor("FReplaceGroup", "#FF0000", e.Row);
			control.SetBackcolor("FModifiedField", "#FF0000", e.Row);
			string empty = string.Empty;
			if (!e.Field.Name.TryGetValue(base.Context.UserLocale.LCID, ref empty))
			{
				return;
			}
			string value = MFGBillUtil.GetValue<string>(base.View.Model, "FModifiedField", e.Row, null, null);
			string text = string.Empty;
			if (ObjectUtils.IsNullOrEmptyOrWhiteSpace(value))
			{
				text = empty;
			}
			else
			{
				List<string> list = value.Split(new char[]
				{
					','
				}).ToList<string>();
				if (list.Contains(empty))
				{
					list.Remove(empty);
				}
				list.Add(empty);
				text = string.Join(",", list);
			}
			base.View.Model.SetValue("FModifiedField", text, e.Row);
		}

		// Token: 0x060000BE RID: 190 RVA: 0x00009B00 File Offset: 0x00007D00
		public override void DataChanged(DataChangedEventArgs e)
		{
			base.DataChanged(e);
			if (this.bCancelDataChangedEvent)
			{
				return;
			}
			if ((ObjectUtils.IsNullOrEmpty(this.isImportObj) || !StringUtils.EqualsIgnoreCase(this.isImportObj.ToString(), "TRUE")) && base.Context.ServiceType == null)
			{
				FormMetadata formMetadata = (FormMetadata)MetaDataServiceHelper.Load(base.Context, "ENG_BOM_BILLPARAM", true);
				DynamicObject dynamicObject = UserParamterServiceHelper.Load(base.Context, formMetadata.BusinessInfo, base.Context.UserId, base.View.BillBusinessInfo.GetForm().Id, "UserParameter");
				if (dynamicObject != null && DataEntityExtend.GetDynamicObjectItemValue<bool>(dynamicObject, "IsMarkContent", false))
				{
					this.SetColorForChangedField(e);
				}
			}
			string key;
			switch (key = e.Field.Key)
			{
			case "FMATERIALIDCHILD":
				this.MatefialChildIdDataChanged(e);
				this.SyncBopF8Data(e, "BopMaterialId", "BD_MATERIAL");
				MFGBillUtil.SetEffectDate(base.View, "FEffectDate", e.Row, 0L);
				if ((ObjectUtils.IsNullOrEmpty(this.isImportObj) || !StringUtils.EqualsIgnoreCase(this.isImportObj.ToString(), "TRUE")) && base.Context.ServiceType == null && !this.bIsReplaceDataChange && e.OldValue != e.NewValue)
				{
					Entity entryEntity = this.Model.BillBusinessInfo.GetEntryEntity("FTreeEntity");
					long dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<long>(base.View.Model.GetEntityDataObject(entryEntity, e.Row), "ReplaceGroup", 0L);
					List<DynamicObject> list = base.View.Model.GetEntityDataObject(entryEntity).ToList<DynamicObject>();
					if (ListUtils.IsEmpty<DynamicObject>(list))
					{
						return;
					}
					foreach (DynamicObject dynamicObject2 in list)
					{
						if (DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObject2, "ReplaceGroup", 0L) == dynamicObjectItemValue && DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObject2, "SubstitutionId_Id", 0L) > 0L)
						{
							int num2 = list.IndexOf(dynamicObject2);
							base.View.Model.SetValue("FSubstitutionId", 0, num2);
							base.View.Model.SetValue("FSTEntryId", 0, num2);
						}
					}
				}
				this.bIsReplaceDataChange = false;
				return;
			case "FDOSAGETYPE":
				this.BOMDosageTypeChanged(e);
				this.SyncBopData(e, "BopDosageType");
				return;
			case "FChildSupplyOrgId":
				this.SendSupplyOrgChangeEvents(MFGBillUtil.GetValue<long>(base.View.Model, "FMATERIALIDCHILD", e.Row, 0L, null));
				return;
			case "FReplaceGroup":
				this.SyncBopData(e, "ReplaceGroupBop");
				return;
			case "FCHILDUNITID":
				this.SyncBopF8Data(e, "BopUnitId", "BD_UNIT");
				return;
			case "FChildBaseUnitID":
				this.SyncBopF8Data(e, "BopBaseUnitID", "BD_UNIT");
				return;
			case "FDENOMINATOR":
				this.SyncBopData(e, "BopDenominator");
				return;
			case "FBaseDenominator":
				this.SyncBopData(e, "BaseBopDenominator");
				return;
			case "FNUMERATOR":
				this.SyncBopData(e, "BopNumerator");
				return;
			case "FBaseNumerator":
				this.SyncBopData(e, "BaseBopNumerator");
				return;
			case "FAuxPropId":
			{
				object customParameter = base.View.OpenParameter.GetCustomParameter("IsImport");
				if ((ObjectUtils.IsNullOrEmpty(customParameter) || !StringUtils.EqualsIgnoreCase(customParameter.ToString(), "TRUE")) && base.Context.ServiceType == null)
				{
					this.Model.SetValue("FBOMID", 0, e.Row);
				}
				if (e.NewValue != null)
				{
					this.AutoHighBomByAuxProp(e.Row);
					return;
				}
				break;
			}
			case "FPOSITIONNO":
				this.ChangePositonNo(e);
				return;
			case "FNETDEMANDRATE":
				if ((ObjectUtils.IsNullOrEmpty(this.isImportObj) || !StringUtils.EqualsIgnoreCase(this.isImportObj.ToString(), "TRUE")) && base.Context.ServiceType == null)
				{
					string value = MFGBillUtil.GetValue<string>(base.View.Model, "FReplaceType", e.Row, null, null);
					string value2 = MFGBillUtil.GetValue<string>(base.View.Model, "FMATERIALTYPE", e.Row, null, null);
					int replaceGroup = MFGBillUtil.GetValue<int>(base.View.Model, "FReplaceGroup", e.Row, 0, null);
					DynamicObjectCollection dynamicValue = DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(base.View.Model.DataObject, "TreeEntity", null);
					decimal d2 = Convert.ToDecimal(e.NewValue);
					if (value == "3" && value2 == "1")
					{
						IEnumerable<DynamicObject> enumerable = from w in dynamicValue
						where DataEntityExtend.GetDynamicValue<int>(w, "ReplaceGroup", 0) == replaceGroup && DataEntityExtend.GetDynamicValue<string>(w, "MATERIALTYPE", null) == "3"
						select w;
						if (!ListUtils.IsEmpty<DynamicObject>(enumerable))
						{
							Dictionary<int, IGrouping<int, DynamicObject>> dictionary = (from g in enumerable
							group g by DataEntityExtend.GetDynamicValue<int>(g, "ReplacePriority", 0)).ToDictionary((IGrouping<int, DynamicObject> d) => d.Key);
							if (dictionary.Keys.Count == 1)
							{
								foreach (KeyValuePair<int, IGrouping<int, DynamicObject>> keyValuePair in dictionary)
								{
									foreach (DynamicObject dynamicObject3 in keyValuePair.Value)
									{
										bool dynamicValue2 = DataEntityExtend.GetDynamicValue<bool>(dynamicObject3, "IskeyItem", false);
										if (dynamicValue2)
										{
											int num3 = dynamicValue.IndexOf(dynamicObject3);
											base.View.Model.BeginIniti();
											base.View.Model.SetValue("FNETDEMANDRATE", 100m - d2, num3);
											base.View.Model.EndIniti();
											base.View.UpdateView("FNETDEMANDRATE", num3);
										}
									}
								}
							}
						}
					}
					if (value == "3" && value2 == "3")
					{
						IEnumerable<DynamicObject> enumerable2 = from w in dynamicValue
						where DataEntityExtend.GetDynamicValue<int>(w, "ReplaceGroup", 0) == replaceGroup && DataEntityExtend.GetDynamicValue<string>(w, "MATERIALTYPE", null) == "1"
						select w;
						IEnumerable<DynamicObject> enumerable3 = from w in dynamicValue
						where DataEntityExtend.GetDynamicValue<int>(w, "ReplaceGroup", 0) == replaceGroup && DataEntityExtend.GetDynamicValue<string>(w, "MATERIALTYPE", null) == "3"
						select w;
						Dictionary<int, IGrouping<int, DynamicObject>> dictionary2 = new Dictionary<int, IGrouping<int, DynamicObject>>();
						Dictionary<int, IGrouping<int, DynamicObject>> dictionary3 = new Dictionary<int, IGrouping<int, DynamicObject>>();
						if (!ListUtils.IsEmpty<DynamicObject>(enumerable2))
						{
							dictionary2 = (from g in enumerable2
							group g by DataEntityExtend.GetDynamicValue<int>(g, "ReplacePriority", 0)).ToDictionary((IGrouping<int, DynamicObject> d) => d.Key);
						}
						if (!ListUtils.IsEmpty<DynamicObject>(enumerable3))
						{
							dictionary3 = (from g in enumerable3
							group g by DataEntityExtend.GetDynamicValue<int>(g, "ReplacePriority", 0)).ToDictionary((IGrouping<int, DynamicObject> d) => d.Key);
						}
						if (dictionary2.Keys.Count == 1 && dictionary3.Keys.Count == 1)
						{
							foreach (KeyValuePair<int, IGrouping<int, DynamicObject>> keyValuePair2 in dictionary2)
							{
								foreach (DynamicObject dynamicObject4 in keyValuePair2.Value)
								{
									bool dynamicValue3 = DataEntityExtend.GetDynamicValue<bool>(dynamicObject4, "IskeyItem", false);
									if (dynamicValue3)
									{
										int num4 = dynamicValue.IndexOf(dynamicObject4);
										base.View.Model.BeginIniti();
										base.View.Model.SetValue("FNETDEMANDRATE", 100m - d2, num4);
										base.View.Model.EndIniti();
										base.View.UpdateView("FNETDEMANDRATE", num4);
									}
								}
							}
						}
					}
				}
				break;

				return;
			}
		}

		// Token: 0x060000BF RID: 191 RVA: 0x0000A41C File Offset: 0x0000861C
		public void AutoHighBomByAuxProp(int row)
		{
			bool userParam = MFGBillUtil.GetUserParam<bool>(base.View, "ChildBomVerByAuxPropEn", false);
			long value = MFGBillUtil.GetValue<long>(base.View.Model, "FCHILDSUPPLYORGID", row, 0L, null);
			DynamicObject value2 = MFGBillUtil.GetValue<DynamicObject>(base.View.Model, "FMATERIALIDCHILD", row, null, null);
			if (!userParam || value == 0L || ObjectUtils.IsNullOrEmpty(value2))
			{
				return;
			}
			long dynamicValue = DataEntityExtend.GetDynamicValue<long>(value2, "msterId", 0L);
			DynamicObject value3 = MFGBillUtil.GetValue<DynamicObject>(base.View.Model, "FAuxPropId", row, null, null);
			if (ObjectUtils.IsNullOrEmpty(value3))
			{
				return;
			}
			long flexDataId = FlexServiceHelper.GetFlexDataId(base.Context, value3, "BD_FLEXSITEMDETAILV");
			long hightVersionBomKey = BOMServiceHelper.GetHightVersionBomKey(base.Context, dynamicValue, value, flexDataId);
			base.View.Model.SetValue("FBOMID", hightVersionBomKey, row);
		}

		// Token: 0x060000C0 RID: 192 RVA: 0x0000A4F4 File Offset: 0x000086F4
		public override void AfterShowFlexForm(AfterShowFlexFormEventArgs e)
		{
			base.AfterShowFlexForm(e);
			if (e.Result == 1 && StringUtils.EqualsIgnoreCase(e.FlexField.Key, "FAuxPropId"))
			{
				this.Model.SetValue("FBOMID", 0, e.Row);
				this.AutoHighBomByAuxProp(e.Row);
			}
		}

		// Token: 0x060000C1 RID: 193 RVA: 0x0000A550 File Offset: 0x00008750
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			base.BeforeF7Select(e);
			string key;
			switch (key = e.FieldKey.ToUpper())
			{
			case "FDEFAULTSTOCK":
				this.SetDefaultStockFilter(ref e);
				return;
			case "FMATERIALIDCHILD":
			{
				e.ListFilterParameter.Filter = base.SqlAppendAnd(e.ListFilterParameter.Filter, this.SetChildMaterilIdFilterString(e.Row));
				DynamicObject dynamicObject = base.View.Model.GetValue("FMATERIALID") as DynamicObject;
				if (ObjectUtils.IsNullOrEmpty(dynamicObject))
				{
					return;
				}
				string dynamicValue = DataEntityExtend.GetDynamicValue<string>((dynamicObject["MaterialBase"] as DynamicObjectCollection).FirstOrDefault<DynamicObject>(), "Suite", null);
				if (dynamicValue == "0")
				{
					e.ListFilterParameter.Filter = base.SqlAppendAnd(e.ListFilterParameter.Filter, "FSUITE = '0' ");
					return;
				}
				break;
			}
			case "FMATERIALID":
			{
				string text = this.ParentMaterilIdFilterString();
				e.ListFilterParameter.Filter = base.SqlAppendAnd(e.ListFilterParameter.Filter, text);
				text = this.GetChildMaterialIdFilter();
				e.ListFilterParameter.Filter = base.SqlAppendAnd(e.ListFilterParameter.Filter, text);
				return;
			}
			case "FBOMID":
				e.DynamicFormShowParameter.MultiSelect = false;
				e.IsShowApproved = false;
				e.ListFilterParameter.Filter = this.GetBomId2Filter(e.ListFilterParameter.Filter, e.Row);
				return;
			case "FCHILDSUPPLYORGID":
				e.ListFilterParameter.Filter = this.GetChildSupplyOrgFilter(e.ListFilterParameter.Filter, e.Row);
				return;
			case "FBOMIDCOBY":
				e.DynamicFormShowParameter.MultiSelect = false;
				e.IsShowApproved = false;
				e.ListFilterParameter.Filter = this.GetBomIdCopyFilter(e.ListFilterParameter.Filter, e.Row);
				break;

				return;
			}
		}

		// Token: 0x060000C2 RID: 194 RVA: 0x0000A790 File Offset: 0x00008990
		private string GetFilterBaseData(string key, string filter, int row)
		{
			string text = "";
			string a;
			if ((a = key.ToUpper()) != null && a == "FCHILDUNITID")
			{
				DynamicObject value = MFGBillUtil.GetValue<DynamicObject>(base.View.Model, "FChildBaseUnitID", row, null, "TreeEntity");
				if (value != null)
				{
					long num = Convert.ToInt64(value["Id"]);
					long num2 = Convert.ToInt64(value["UnitGroupId_Id"]);
					long num3 = Convert.ToInt64(MFGBillUtil.GetValue<DynamicObject>(base.View.Model, "FMATERIALIDCHILD", row, null, "TreeEntity")["Id"]);
					if (num > 0L)
					{
						text = string.Format("FUNITGROUPID = {1} OR FUNITGROUPID IN (SELECT distinct tu2.FUNITGROUPID FROM T_BD_UNITCONVERTRATE tuv2 INNER JOIN T_BD_UNIT tu2 ON tuv2.FCURRENTUNITID=tu2.FUNITID WHERE  FDESTUNITID={0} AND FMATERIALID = {2})", num, num2, num3);
					}
				}
			}
			filter = ((string.IsNullOrEmpty(filter) || string.IsNullOrEmpty(text)) ? "" : " AND ") + text;
			return filter;
		}

		// Token: 0x060000C3 RID: 195 RVA: 0x0000A87C File Offset: 0x00008A7C
		public override void BeforeSetItemValueByNumber(BeforeSetItemValueByNumberArgs e)
		{
			base.BeforeSetItemValueByNumber(e);
			string key;
			switch (key = e.BaseDataFieldKey.ToUpper())
			{
			case "FDEFAULTSTOCK":
				this.SetDefaultStockFilter(ref e);
				return;
			case "FMATERIALIDCHILD":
			{
				e.Filter = base.SqlAppendAnd(e.Filter, this.SetChildMaterilIdFilterString(e.Row));
				DynamicObject dynamicObject = base.View.Model.GetValue("FMATERIALID") as DynamicObject;
				if (ObjectUtils.IsNullOrEmpty(dynamicObject))
				{
					return;
				}
				string dynamicValue = DataEntityExtend.GetDynamicValue<string>((dynamicObject["MaterialBase"] as DynamicObjectCollection).FirstOrDefault<DynamicObject>(), "Suite", null);
				if (dynamicValue == "0")
				{
					e.Filter = base.SqlAppendAnd(e.Filter, "FSUITE = '0' ");
					return;
				}
				break;
			}
			case "FMATERIALID":
			{
				string text = this.ParentMaterilIdFilterString();
				e.Filter = base.SqlAppendAnd(e.Filter, text);
				text = this.GetChildMaterialIdFilter();
				e.Filter = base.SqlAppendAnd(e.Filter, text);
				return;
			}
			case "FBOMID":
				e.IsShowApproved = false;
				e.Filter = this.GetBomId2Filter(e.Filter, e.Row);
				return;
			case "FBOMIDCOPY":
				e.IsShowApproved = false;
				e.Filter = this.GetBomIdCopyFilter(e.Filter, e.Row);
				return;
			case "FCHILDSUPPLYORGID":
				e.Filter = this.GetChildSupplyOrgFilter(e.Filter, e.Row);
				break;

				return;
			}
		}

		// Token: 0x060000C4 RID: 196 RVA: 0x0000AA5C File Offset: 0x00008C5C
		public override void AfterF7Select(AfterF7SelectEventArgs e)
		{
			base.AfterF7Select(e);
			if (e.FieldKey.Equals("FMATERIALIDCHILD", StringComparison.OrdinalIgnoreCase) && e.SelectRows.Count > 1)
			{
				int entryCurrentRowIndex = base.View.Model.GetEntryCurrentRowIndex("FTreeEntity");
				int num = entryCurrentRowIndex;
				try
				{
					this.bCancelDataChangedEvent = true;
					this.DoMaterialLookUpSetValueBatch(e.FieldKey, ref num, e.SelectRows);
				}
				finally
				{
					e.Cancel = true;
					entryCurrentRowIndex = base.View.Model.GetEntryCurrentRowIndex("FTreeEntity");
					this.SetReplaceGroup();
					base.AutoCreateNewRow("FTreeEntity", "FMATERIALIDCHILD");
					this.bCancelDataChangedEvent = false;
					base.View.UpdateView("FTreeEntity");
					this.Model.SetEntryCurrentRowIndex("FTreeEntity", entryCurrentRowIndex);
					base.View.SetEntityFocusRow("FTreeEntity", entryCurrentRowIndex);
				}
			}
		}

		// Token: 0x060000C5 RID: 197 RVA: 0x0000AB4C File Offset: 0x00008D4C
		public override void AfterCreateNewEntryRow(CreateNewEntryEventArgs e)
		{
			base.AfterCreateNewEntryRow(e);
			if (StringUtils.EqualsIgnoreCase(e.Entity.Key, "FTreeEntity") && !this.bMoveUpOrDown)
			{
				this.CreateEntryRowIdFieldValue(e.Row);
			}
		}

		// Token: 0x060000C6 RID: 198 RVA: 0x0000AB80 File Offset: 0x00008D80
		public override void AfterDeleteRow(AfterDeleteRowEventArgs e)
		{
			base.AfterDeleteRow(e);
			string entityKey;
			if ((entityKey = e.EntityKey) != null)
			{
				if (!(entityKey == "FTreeEntity"))
				{
					return;
				}
				if (!this.bMoveUpOrDown)
				{
					this.SetReplaceGroup();
				}
				base.View.UpdateView("FBOMCHILDLOTBASEDQTY");
			}
		}

		// Token: 0x060000C7 RID: 199 RVA: 0x0000ABCC File Offset: 0x00008DCC
		public override void AfterCopyRow(AfterCopyRowEventArgs e)
		{
			if (e.EntityKey.ToString().ToUpper().Equals("FTreeEntity".ToUpper()))
			{
				this.RemoveRowRepInfo(e.NewRow);
				this.CreateEntryRowIdFieldValue(e.NewRow);
				this.ResetIssueType(e.Row, e.NewRow);
			}
			this.canUpdate = false;
		}

		// Token: 0x060000C8 RID: 200 RVA: 0x0000AC2C File Offset: 0x00008E2C
		private void ResetIssueType(int oldRowIndex, int newIndex)
		{
			ComboField comboField = base.View.BillBusinessInfo.GetField("FISSUETYPE") as ComboField;
			if ((comboField.FunControl & Field.FUNCONTROL_COPY) == Field.FUNCONTROL_COPY)
			{
				base.View.BillBusinessInfo.GetEntryEntity("FTreeEntity");
				string value = MFGBillUtil.GetValue<string>(base.View.Model, "FISSUETYPE", oldRowIndex, "", null);
				base.View.Model.SetValue("FISSUETYPE", value, newIndex);
				this.IsCopyDataIssueType = true;
			}
		}

		// Token: 0x060000C9 RID: 201 RVA: 0x0000ACB8 File Offset: 0x00008EB8
		public override void BeforeUpdateValue(BeforeUpdateValueEventArgs e)
		{
			base.BeforeUpdateValue(e);
			string a;
			if ((a = e.Key.ToUpperInvariant()) != null)
			{
				if (!(a == "FMATERIALIDCHILD"))
				{
					if (!(a == "FMATERIALTYPE"))
					{
						if (!(a == "FISSUETYPE"))
						{
							return;
						}
						if (this.IsCopyDataIssueType)
						{
							e.Cancel = true;
							this.IsCopyDataIssueType = false;
						}
					}
					else if (!this.IsCanBCTypeUpdate(e))
					{
						e.Cancel = true;
						return;
					}
				}
				else
				{
					if (!this.IsCanUpdateMaterial(e))
					{
						e.Cancel = true;
					}
					if (!this.CheckMaterialChildSuite(e))
					{
						e.Cancel = true;
						return;
					}
				}
			}
		}

		// Token: 0x060000CA RID: 202 RVA: 0x0000AD64 File Offset: 0x00008F64
		private void DoMaterialLookUpSetValueBatch(string fieldKey, ref int rowIndex, ListSelectedRowCollection selectRows)
		{
			IEnumerable<string> enumerable = from p in selectRows
			select p.PrimaryKeyValue;
			ILookUpField lookupField = this.GetLookupField(fieldKey);
			FieldAppearance fieldAppearance = base.View.LayoutInfo.GetFieldAppearance(fieldKey);
			DynamicObject[] array = BusinessDataServiceHelper.Load(base.Context, enumerable.ToArray<string>(), lookupField.RefFormDynamicObjectType);
			Dictionary<long, DynamicObject> dictionary = array.ToDictionary((DynamicObject d) => DataEntityExtend.GetDynamicValue<long>(d, "Id", 0L));
			List<DynamicObject> list = new List<DynamicObject>();
			foreach (string value in enumerable)
			{
				DynamicObject item;
				if (dictionary.TryGetValue(Convert.ToInt64(value), out item))
				{
					list.Add(item);
				}
			}
			array = list.ToArray();
			int entryRowCount = base.View.Model.GetEntryRowCount(fieldAppearance.EntityKey);
			base.View.RuleContainer.Suspend();
			for (int i = 0; i < array.Length; i++)
			{
				if (MFGBillUtil.GetValue<long>(base.View.Model, "FMATERIALIDCHILD", rowIndex, 0L, null) > 0L)
				{
					base.View.Model.InsertEntryRow(fieldAppearance.EntityKey, rowIndex);
				}
				else if (rowIndex + 1 > entryRowCount)
				{
					this.Model.CreateNewEntryRow(fieldAppearance.EntityKey);
				}
				this.Model.SetEntryCurrentRowIndex(fieldAppearance.EntityKey, rowIndex);
				base.View.SetEntityFocusRow(fieldAppearance.EntityKey, rowIndex);
				this.Model.SetValue(fieldAppearance.Key, array[i], rowIndex);
				base.View.InvokeFieldUpdateService(fieldAppearance.Key, rowIndex);
				MFGBillUtil.SetEffectDate(base.View, "FEffectDate", rowIndex, 0L);
				rowIndex++;
			}
			base.View.RuleContainer.Resume(new BOSActionExecuteContext(base.View));
		}

		// Token: 0x060000CB RID: 203 RVA: 0x0000AF90 File Offset: 0x00009190
		private void CreateEntryRowIdFieldValue(int iRow)
		{
			DynamicObjectCollection entityDataObject = this.Model.GetEntityDataObject(base.View.BillBusinessInfo.GetEntity("FTreeEntity"));
			if (entityDataObject == null || entityDataObject.Count == 0)
			{
				return;
			}
			string text = string.Format("{0:00000}", 1);
			if (entityDataObject != null)
			{
				text = (entityDataObject.Max(delegate(DynamicObject o)
				{
					int result = 0;
					int.TryParse(DataEntityExtend.GetDynamicObjectItemValue<string>(o, "EntryRowId", "0"), out result);
					return result;
				}) + 1).ToString("D5");
			}
			(this.Model.DataObject["TreeEntity"] as DynamicObjectCollection)[iRow]["EntryRowId"] = text;
			this.Model.SetValue("FRowId", SequentialGuid.NewGuid().ToString(), iRow);
			this.Model.SetValue("FRowExpandType", 0, iRow);
		}

		// Token: 0x060000CC RID: 204 RVA: 0x0000B077 File Offset: 0x00009277
		private void MatefialChildIdDataChanged(DataChangedEventArgs e)
		{
			this.Model.SetValue("FBOMID", 0, e.Row);
			this.Model.SetValue("FAuxPropId", 0, e.Row);
		}

		// Token: 0x060000CD RID: 205 RVA: 0x0000B0B4 File Offset: 0x000092B4
		private void BOMDosageTypeChanged(DataChangedEventArgs e)
		{
			long value = MFGBillUtil.GetValue<long>(base.View.Model, "FDOSAGETYPE", e.Row, -1L, "TreeEntity");
			if ((int)value != 3)
			{
				this.DeleteFlotBasedqty();
				return;
			}
			if (MFGBillUtil.GetUserParam<bool>(base.View, "AutoGetLadderLoss", false) && this.CreateFlotBasedqtyByLadderLoss(e.Row, true))
			{
				return;
			}
			DynamicObject value2 = MFGBillUtil.GetValue<DynamicObject>(base.View.Model, "FMATERIALIDCHILD", e.Row, null, "TreeEntity");
			this.CreateFlotBasedqty(value2);
		}

		// Token: 0x060000CE RID: 206 RVA: 0x0000B140 File Offset: 0x00009340
		private void ShowBomBatchEdit()
		{
			PermissionAuthResult permissionAuthResult = PermissionServiceHelper.FuncPermissionAuth(base.View.Context, new BusinessObject
			{
				Id = "ENG_BOMBATCHEDIT"
			}, "6e44119a58cb4a8e86f6c385e14a17ad");
			if (!permissionAuthResult.Passed)
			{
				base.View.ShowMessage(string.Format(ResManager.LoadKDString("您在【{0}】组织下没有【物料清单批量维护】的【查看】权限，请联系系统管理员", "015072000025054", 7, new object[0]), base.View.Context.CurrentOrganizationInfo.Name), 0);
				return;
			}
			DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
			dynamicFormShowParameter.OpenStyle.ShowType = 7;
			dynamicFormShowParameter.FormId = "ENG_BOMBATCHEDIT";
			base.View.ShowForm(dynamicFormShowParameter);
		}

		// Token: 0x060000CF RID: 207 RVA: 0x0000B1E4 File Offset: 0x000093E4
		private void ShowBomSearchRowView()
		{
			EntryEntity entryEntity = base.View.BusinessInfo.GetEntryEntity("FTreeEntity");
			DynamicObjectCollection entityDataObject = base.View.Model.GetEntityDataObject(entryEntity);
			if (entityDataObject.Count == 0)
			{
				base.View.ShowMessage(ResManager.LoadKDString("没有可进行查找的分录行，请先录入子项物料！", "015072000012051", 7, new object[0]), 0);
				return;
			}
			DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
			dynamicFormShowParameter.OpenStyle.ShowType = 4;
			dynamicFormShowParameter.FormId = "ENG_BOMSearchRow";
			base.View.ShowForm(dynamicFormShowParameter);
		}

		// Token: 0x060000D0 RID: 208 RVA: 0x0000B270 File Offset: 0x00009470
		private void BomTreeEntitySearchRow(string searchContent, bool isSearchNext)
		{
			EntryEntity entryEntity = base.View.BusinessInfo.GetEntryEntity("FTreeEntity");
			DynamicObjectCollection entityDataObject = base.View.Model.GetEntityDataObject(entryEntity);
			TreeEntryGrid control = base.View.GetControl<TreeEntryGrid>("FTreeEntity");
			int focusRowIndex = control.GetFocusRowIndex();
			int num;
			if (isSearchNext)
			{
				if (focusRowIndex == entityDataObject.Count - 1)
				{
					base.View.ShowMessage(ResManager.LoadKDString("当前行已是末行", "015072000012052", 7, new object[0]), 0);
					return;
				}
				num = this.FindNextRowIndex(entityDataObject, searchContent, focusRowIndex + 1);
			}
			else
			{
				if (focusRowIndex == 0)
				{
					base.View.ShowMessage(ResManager.LoadKDString("当前行已是首行", "015072000012053", 7, new object[0]), 0);
					return;
				}
				num = this.FindPrevRowIndex(entityDataObject, searchContent, focusRowIndex - 1);
			}
			if (num == -1)
			{
				base.View.ShowMessage(ResManager.LoadKDString("未找到匹配结果", "015072000012054", 7, new object[0]), 0);
				return;
			}
			control.SetFocusRowIndex(num);
		}

		// Token: 0x060000D1 RID: 209 RVA: 0x0000B364 File Offset: 0x00009564
		private int FindPrevRowIndex(DynamicObjectCollection entityDatas, string searchContent, int start)
		{
			int result = -1;
			for (int i = start; i >= 0; i--)
			{
				if (this.IsMatchedSearchContent(entityDatas[i], searchContent))
				{
					result = i;
					break;
				}
			}
			return result;
		}

		// Token: 0x060000D2 RID: 210 RVA: 0x0000B394 File Offset: 0x00009594
		private int FindNextRowIndex(DynamicObjectCollection entityDatas, string searchContent, int start)
		{
			int result = -1;
			for (int i = start; i < entityDatas.Count; i++)
			{
				if (this.IsMatchedSearchContent(entityDatas[i], searchContent))
				{
					result = i;
					break;
				}
			}
			return result;
		}

		// Token: 0x060000D3 RID: 211 RVA: 0x0000B3CC File Offset: 0x000095CC
		private bool IsMatchedSearchContent(DynamicObject row, string searchContent)
		{
			DynamicObject dynamicValue = DataEntityExtend.GetDynamicValue<DynamicObject>(row, "MATERIALIDCHILD", null);
			if (dynamicValue != null)
			{
				string dynamicValue2 = DataEntityExtend.GetDynamicValue<string>(dynamicValue, "Number", null);
				string text = DataEntityExtend.GetDynamicValue<LocaleValue>(dynamicValue, "Name", null).ToString();
				string text2 = DataEntityExtend.GetDynamicValue<LocaleValue>(dynamicValue, "Specification", null).ToString();
				return dynamicValue2.Contains(searchContent) || text.Contains(searchContent) || text2.Contains(searchContent);
			}
			return false;
		}

		// Token: 0x060000D4 RID: 212 RVA: 0x0000B438 File Offset: 0x00009638
		protected string GetChildSupplyOrgFilter(string filter, int row)
		{
			if (filter == null)
			{
				filter = string.Empty;
			}
			DynamicObject dynamicObject = this.Model.GetValue("FMATERIALIDCHILD", row) as DynamicObject;
			long num = (dynamicObject == null) ? 0L : OtherExtend.ConvertTo<long>(dynamicObject["msterID"], 0L);
			long value = MFGBillUtil.GetValue<long>(base.View.Model, "FUseOrgId", -1, 0L, null);
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
			List<long> orgByBizRelationOrgs = MFGServiceHelper.GetOrgByBizRelationOrgs(base.Context, value, list);
			if (orgByBizRelationOrgs == null || orgByBizRelationOrgs.Count < 1)
			{
				return filter;
			}
			list2.AddRange(orgByBizRelationOrgs);
			list2.Add(value);
			filter += ((filter.Length > 0) ? (" AND " + string.Format("FORGID in ({0})", string.Join<long>(",", list2))) : (string.Format("FORGID in ({0})", string.Join<long>(",", list2)) + string.Format(" AND EXISTS (SELECT 1 FROM T_BD_MATERIAL TM WHERE TM.FMASTERID = {0} AND T0.FORGID = TM.FUSEORGID)", num)));
			return filter;
		}

		// Token: 0x060000D5 RID: 213 RVA: 0x0000B578 File Offset: 0x00009778
		protected string GetBomId2Filter(string filter, int row)
		{
			DynamicObject value = MFGBillUtil.GetValue<DynamicObject>(base.View.Model, "FMATERIALIDCHILD", row, null, null);
			long value2 = MFGBillUtil.GetValue<long>(base.View.Model, "FChildSupplyOrgId", row, 0L, null);
			DynamicObjectCollection dynamicObjectCollection = base.View.Model.DataObject["TreeEntity"] as DynamicObjectCollection;
			long dynamicValue = DataEntityExtend.GetDynamicValue<long>(dynamicObjectCollection[row], "AuxPropId_Id", 0L);
			DynamicObject dynamicValue2 = DataEntityExtend.GetDynamicValue<DynamicObject>(dynamicObjectCollection[row], "AuxPropId", null);
			return this.GetBomIdFilter(filter, value, value2, dynamicValue, dynamicValue2);
		}

		// Token: 0x060000D6 RID: 214 RVA: 0x0000B60C File Offset: 0x0000980C
		protected string GetBomIdCopyFilter(string filter, int row)
		{
			DynamicObjectCollection dynamicObjectCollection = base.View.Model.DataObject["EntryBOMCOBY"] as DynamicObjectCollection;
			DynamicObject value = MFGBillUtil.GetValue<DynamicObject>(base.View.Model, "FMATERIALIDCOBY", row, null, null);
			long value2 = MFGBillUtil.GetValue<long>(base.View.Model, "FUseOrgId", -1, 0L, null);
			string value3 = MFGBillUtil.GetValue<string>(base.View.Model, "FBOMUSE", -1, null, null);
			long dynamicValue = DataEntityExtend.GetDynamicValue<long>(dynamicObjectCollection[row], "AuxPropId_Id", 0L);
			DynamicObject dynamicValue2 = DataEntityExtend.GetDynamicValue<DynamicObject>(dynamicObjectCollection[row], "AuxPropId", null);
			filter = this.GetBomIdFilter(filter, value, value2, dynamicValue, dynamicValue2);
			filter += string.Format(" AND FDOCUMENTSTATUS='C' AND FFORBIDSTATUS='A' AND FBOMUSE='{0}' ", value3);
			return filter;
		}

		// Token: 0x060000D7 RID: 215 RVA: 0x0000B6D4 File Offset: 0x000098D4
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
			filter += ((filter.Length > 0) ? (" AND " + text) : text);
			return filter;
		}

		// Token: 0x060000D8 RID: 216 RVA: 0x0000B776 File Offset: 0x00009976
		protected void SetDefaultStockFilter(ref BeforeF7SelectEventArgs e)
		{
			e.ListFilterParameter.Filter = this.SetDefaultStockFilter(e.ListFilterParameter.Filter, e.Row);
		}

		// Token: 0x060000D9 RID: 217 RVA: 0x0000B79D File Offset: 0x0000999D
		protected void SetDefaultStockFilter(ref BeforeSetItemValueByNumberArgs e)
		{
			e.Filter = this.SetDefaultStockFilter(e.Filter, e.Row);
		}

		// Token: 0x060000DA RID: 218 RVA: 0x0000B7BC File Offset: 0x000099BC
		protected string SetDefaultStockFilter(string filter, int row)
		{
			long value = MFGBillUtil.GetValue<long>(base.View.Model, "FSUPPLYORG", row, -1L, "TreeEntity");
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

		// Token: 0x060000DB RID: 219 RVA: 0x0000B824 File Offset: 0x00009A24
		protected string ParentMaterilIdFilterString()
		{
			string format = " (FErpClsID IN('{0}'))";
			long value = MFGBillUtil.GetValue<long>(base.View.Model, "FBOMCATEGORY", -1, 0L, null);
			if ((int)value == 1)
			{
				return string.Format(format, string.Join<int>("','", new List<int>
				{
					2,
					3,
					5,
					1,
					9,
					4,
					12,
					13
				}));
			}
			return string.Format(format, string.Join<int>("','", new List<int>
			{
				9
			}));
		}

		// Token: 0x060000DC RID: 220 RVA: 0x0000B8DC File Offset: 0x00009ADC
		protected string GetChildMaterialIdFilter()
		{
			DynamicObjectCollection source = this.Model.DataObject["TreeEntity"] as DynamicObjectCollection;
			List<string> list = (from x in source
			select OtherExtend.ConvertTo<string>(x["MATERIALIDCHILD_ID"], null)).ToList<string>();
			if (!list.Any<string>())
			{
				return string.Empty;
			}
			return string.Format(" FMaterialId NOT IN ({0})", string.Join(",", list));
		}

		// Token: 0x060000DD RID: 221 RVA: 0x0000B97C File Offset: 0x00009B7C
		protected string SetChildMaterilIdFilterString(int row)
		{
			DynamicObject dynamicObject = base.View.Model.GetValue("FMATERIALID") as DynamicObject;
			if (dynamicObject == null)
			{
				return "1=0";
			}
			string materialFilter = this.GetMaterialFilter();
			long parentMProperty = 0L;
			parentMProperty = Convert.ToInt64((dynamicObject["MaterialBase"] as DynamicObjectCollection).FirstOrDefault<DynamicObject>()["ErpClsID"]);
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
				return materialFilter;
			}
			return materialFilter + string.Format(" and (FErpClsID IN('{0}'))", string.Join<int>("','", list));
		}

		// Token: 0x060000DE RID: 222 RVA: 0x0000BC98 File Offset: 0x00009E98
		protected virtual string GetMaterialFilter()
		{
			DynamicObject dynamicObject = base.View.Model.GetValue("FMATERIALID") as DynamicObject;
			if (dynamicObject == null)
			{
				return "1=0";
			}
			return string.Format("(FMATERIALID <> {0})", DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObject, "Id", 0L));
		}

		// Token: 0x060000DF RID: 223 RVA: 0x0000BCE7 File Offset: 0x00009EE7
		private void DeleteFlotBasedqty()
		{
			if (base.View.Model.GetEntryRowCount("FBOMCHILDLOTBASEDQTY") > 0)
			{
				base.View.Model.DeleteEntryData("FBOMCHILDLOTBASEDQTY");
			}
		}

		// Token: 0x060000E0 RID: 224 RVA: 0x0000BD18 File Offset: 0x00009F18
		private void CreateFlotBasedqty(DynamicObject dyBOMChildMaterial)
		{
			if (base.View.Model.GetEntryRowCount("FBOMCHILDLOTBASEDQTY") <= 0)
			{
				base.View.Model.CreateNewEntryRow("FBOMCHILDLOTBASEDQTY");
			}
			long dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<long>(dyBOMChildMaterial, "Id", 0L);
			if (dynamicObjectItemValue > 0L)
			{
				long num = (long)base.View.Model.GetEntryRowCount("FBOMCHILDLOTBASEDQTY");
				Entity entryEntity = base.View.BillBusinessInfo.GetEntryEntity("FBOMCHILDLOTBASEDQTY");
				Field field = base.View.BillBusinessInfo.GetField("FBASENUMERATORLOT");
				Field field2 = base.View.BillBusinessInfo.GetField("FBASEDENOMINATORLOT");
				long value = MFGBillUtil.GetValue<long>(this.Model, "FUNITID", -1, 0L, null);
				long value2 = MFGBillUtil.GetValue<long>(this.Model, "FBaseUnitId", -1, 0L, null);
				long value3 = MFGBillUtil.GetValue<long>(this.Model, "FMATERIALID", -1, 0L, null);
				DynamicObject value4 = MFGBillUtil.GetValue<DynamicObject>(this.Model, "FMATERIALID", -1, null, null);
				UnitConvert unitConvertRate = UnitConvertServiceHelper.GetUnitConvertRate(base.View.Context, new GetUnitConvertRateArgs
				{
					MaterialId = value3,
					MasterId = DataEntityExtend.GetDynamicObjectItemValue<long>(value4, "msterID", 0L),
					SourceUnitId = value,
					DestUnitId = value2
				});
				Field field3 = base.View.BillBusinessInfo.GetField("FUNITIDLOT");
				Field field4 = base.View.BillBusinessInfo.GetField("FBASEUNITIDLOT");
				while (num > 0L)
				{
					if (MFGBillUtil.GetValue<long>(base.View.Model, "FMATERIALIDLOTBASED", (int)(num - 1L), -1L, "FBOMCHILDLOTBASEDQTY") <= 0L)
					{
						base.View.Model.SetValue("FMATERIALIDLOTBASED", dynamicObjectItemValue, (int)(num - 1L));
						DynamicObject entityDataObject = this.Model.GetEntityDataObject(entryEntity, (int)num - 1);
						if (entityDataObject != null && field != null)
						{
							long dynamicObjectItemValue2 = DataEntityExtend.GetDynamicObjectItemValue<long>(entityDataObject, field3, 0L);
							long dynamicObjectItemValue3 = DataEntityExtend.GetDynamicObjectItemValue<long>(entityDataObject, field4, 0L);
							UnitConvert unitConvertRate2 = UnitConvertServiceHelper.GetUnitConvertRate(base.View.Context, new GetUnitConvertRateArgs
							{
								MaterialId = dynamicObjectItemValue,
								MasterId = DataEntityExtend.GetDynamicObjectItemValue<long>(dyBOMChildMaterial, "msterID", 0L),
								SourceUnitId = dynamicObjectItemValue2,
								DestUnitId = dynamicObjectItemValue3
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
						IEnumerable<DynamicObject> selectedRowDatas = SysBclExtend.GetSelectedRowDatas(base.View, "FTreeEntity");
						if (!ListUtils.IsEmpty<DynamicObject>(selectedRowDatas))
						{
							int dynamicValue = DataEntityExtend.GetDynamicValue<int>(selectedRowDatas.FirstOrDefault<DynamicObject>(), "Seq", 0);
							if (dynamicValue >= 1)
							{
								long value5 = MFGBillUtil.GetValue<long>(base.View.Model, "FNUMERATOR", dynamicValue - 1, -1L, "FTreeEntity");
								long value6 = MFGBillUtil.GetValue<long>(base.View.Model, "FDENOMINATOR", dynamicValue - 1, -1L, "FTreeEntity");
								long value7 = MFGBillUtil.GetValue<long>(base.View.Model, "FFIXSCRAPQTY", dynamicValue - 1, -1L, "FTreeEntity");
								decimal value8 = MFGBillUtil.GetValue<decimal>(base.View.Model, "FSCRAPRATE", dynamicValue - 1, -1m, "FTreeEntity");
								this.Model.SetValue("FNUMERATORLOT", value5, (int)(num - 1L));
								this.Model.SetValue("FDENOMINATORLOT", value6, (int)(num - 1L));
								this.Model.SetValue("FFIXSCRAPQTYLOT", value7, (int)(num - 1L));
								this.Model.SetValue("FSCRAPRATELOT", value8, (int)(num - 1L));
							}
						}
						base.View.UpdateView("FMATERIALIDLOTBASED", (int)(num - 1L));
						base.View.UpdateView("FBOMCHILDLOTBASEDQTY");
					}
					num -= 1L;
				}
			}
		}

		// Token: 0x060000E1 RID: 225 RVA: 0x0000C120 File Offset: 0x0000A320
		private ILookUpField GetLookupField(string key)
		{
			FieldAppearance fieldAppearance = base.View.LayoutInfo.GetFieldAppearance(key);
			ILookUpField result;
			if (fieldAppearance is ProxyFieldAppearance)
			{
				result = (((ProxyFieldAppearance)fieldAppearance).ControlFieldAppearance.Field as ILookUpField);
			}
			else
			{
				result = (base.View.BusinessInfo.GetField(key) as ILookUpField);
			}
			return result;
		}

		// Token: 0x060000E2 RID: 226 RVA: 0x0000C1A0 File Offset: 0x0000A3A0
		private void ShowListDatas(string barItemName)
		{
			if (ObjectUtils.IsNullOrEmpty(this.authSynsResult))
			{
				this.authSynsResult = PermissionServiceHelper.FuncPermissionAuth(base.View.Context, new BusinessObject
				{
					Id = "ENG_BOM"
				}, "55488307023b99");
			}
			if (!this.authSynsResult.Passed)
			{
				base.View.ShowMessage(ResManager.LoadKDString("您没有【物料清单】的【同步更新】权限，请联系系统管理员！", "015072000022386", 7, new object[0]), 0);
				return;
			}
			DynamicObject dataObject = base.View.Model.DataObject;
			List<long> list = new List<long>();
			list.Add(DataEntityExtend.GetDynamicValue<long>(dataObject, "ID", 0L));
			List<long> list2 = new List<long>();
			long value = MFGBillUtil.GetValue<long>(base.View.Model, "FUseOrgId", -1, 0L, null);
			list2.Add(value);
			DynamicObject parameterData = this.Model.ParameterData;
			string dynamicValue = DataEntityExtend.GetDynamicValue<string>(parameterData, "FUpdateType", null);
			bool dynamicValue2 = DataEntityExtend.GetDynamicValue<bool>(parameterData, "FPrdPPBOM", false);
			bool dynamicValue3 = DataEntityExtend.GetDynamicValue<bool>(parameterData, "FSubPPBOM", false);
			bool dynamicValue4 = DataEntityExtend.GetDynamicValue<bool>(parameterData, "PLBOM", false);
			if (dynamicValue == "1" && barItemName == "tbBomSynUpdate")
			{
				base.View.ShowMessage(ResManager.LoadKDString("用户参数选择更新方式是审核时更新", "015072000018119", 7, new object[0]), 0);
				return;
			}
			if (!dynamicValue2 && !dynamicValue3 && !dynamicValue4)
			{
				base.View.ShowErrMessage(ResManager.LoadKDString("用户参数未选择任何需要同步的订单", "015072000018120", 7, new object[0]), "", 0);
				return;
			}
			List<DynamicObject> list3 = new List<DynamicObject>();
			list3.Add(dataObject);
			long value2 = MFGBillUtil.GetValue<long>(base.View.Model, "FCreateOrgId", -1, 0L, null);
			if (value2 == value)
			{
				string dynamicValue5 = DataEntityExtend.GetDynamicValue<string>(parameterData, "UpdateRange", null);
				List<long> list4 = new List<long>();
				list4.Add(DataEntityExtend.GetDynamicValue<long>(dataObject, "MsterId", 0L));
				if (dynamicValue5 == "2")
				{
					List<DynamicObject> allocatedBOM = this.GetAllocatedBOM(list4);
					if (!ListUtils.IsEmpty<DynamicObject>(allocatedBOM))
					{
						list2.AddRange((from s in allocatedBOM
						select DataEntityExtend.GetDynamicValue<long>(s, "UseOrgId_Id", 0L)).ToList<long>());
						list.AddRange((from s in allocatedBOM
						select DataEntityExtend.GetDynamicValue<long>(s, "Id", 0L)).ToList<long>());
						list3.AddRange(allocatedBOM);
					}
				}
			}
			List<long> first = new List<long>();
			List<long> bomBackwardByVirtualBom = BomSyncBackwardUtil.GetBomBackwardByVirtualBom(base.Context, list3, ref first);
			if (!ListUtils.IsEmpty<long>(bomBackwardByVirtualBom))
			{
				list.AddRange(bomBackwardByVirtualBom.Except(list));
				list2.AddRange(first.Except(list2));
				DynamicObject[] source = BusinessDataServiceHelper.Load(base.Context, (from i in list.Distinct<long>()
				select i).ToArray<object>(), base.View.BusinessInfo.GetDynamicObjectType());
				list3 = source.ToList<DynamicObject>();
			}
			string dynamicValue6 = DataEntityExtend.GetDynamicValue<string>(parameterData, "ConSultDate", null);
			bool dynamicValue7 = DataEntityExtend.GetDynamicValue<bool>(parameterData, "IsSkipExpand", false);
			DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
			dynamicFormShowParameter.OpenStyle.ShowType = 4;
			dynamicFormShowParameter.FormId = "ENG_SYNSUPDATEPPBOM";
			dynamicFormShowParameter.CustomComplexParams.Add("BomData", list3);
			dynamicFormShowParameter.CustomComplexParams.Add("BomId", list);
			dynamicFormShowParameter.CustomComplexParams.Add("UserOrgId", list2);
			dynamicFormShowParameter.CustomComplexParams.Add("IsShowPrdList", dynamicValue2);
			dynamicFormShowParameter.CustomComplexParams.Add("IsShowSubList", dynamicValue3);
			dynamicFormShowParameter.CustomComplexParams.Add("ConSultDate", dynamicValue6);
			dynamicFormShowParameter.CustomComplexParams.Add("IsSkipExpand", dynamicValue7);
			dynamicFormShowParameter.CustomComplexParams.Add("IsShowPlnList", dynamicValue4);
			base.View.ShowForm(dynamicFormShowParameter);
		}

		// Token: 0x060000E3 RID: 227 RVA: 0x0000C590 File Offset: 0x0000A790
		private List<DynamicObject> GetAllocatedBOM(List<long> bomMasterIds)
		{
			QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter();
			queryBuilderParemeter.BusinessInfo = base.View.Model.BusinessInfo;
			queryBuilderParemeter.ExtJoinTables.Add(new ExtJoinTableDescription
			{
				TableName = "table(fn_StrSplit(@msterId,',',1))",
				TableNameAs = "tms",
				FieldName = "FID",
				ScourceKey = "FMASTERID"
			});
			queryBuilderParemeter.FilterClauseWihtKey = "FCreateOrgId<>FUseOrgId";
			queryBuilderParemeter.SqlParams.Add(new SqlParam("@msterId", 161, bomMasterIds.Distinct<long>().ToArray<long>()));
			return BusinessDataServiceHelper.Load(base.Context, queryBuilderParemeter.BusinessInfo.GetDynamicObjectType(), queryBuilderParemeter).ToList<DynamicObject>();
		}

		// Token: 0x060000E4 RID: 228 RVA: 0x0000C65C File Offset: 0x0000A85C
		private void SetBomEntrySource()
		{
			DynamicObject dataObject = base.View.Model.DataObject;
			long dynamicValue = DataEntityExtend.GetDynamicValue<long>(base.View.Model.DataObject, "Id", 0L);
			if (dynamicValue == 0L)
			{
				DynamicObjectCollection dynamicValue2 = DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(dataObject, "TreeEntity", null);
				foreach (DynamicObject dynamicObject in dynamicValue2)
				{
					MFGBillUtil.SetValue(base.View, "FEntrySource", dynamicObject, "1");
				}
				return;
			}
			List<long> list = new List<long>();
			list.Add(dynamicValue);
			DynamicObjectCollection dynamicValue3 = DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(dataObject, "TreeEntity", null);
			Dictionary<long, IGrouping<long, DynamicObject>> dictionary = (from g in dynamicValue3
			group g by DataEntityExtend.GetDynamicValue<long>(g, "Id", 0L)).ToDictionary((IGrouping<long, DynamicObject> d) => d.Key);
			DynamicObjectCollection bomEntryDatasByIds = BOMServiceHelper.GetBomEntryDatasByIds(base.Context, list);
			foreach (DynamicObject dynamicObject2 in bomEntryDatasByIds)
			{
				long dynamicValue4 = DataEntityExtend.GetDynamicValue<long>(dynamicObject2, "FENTRYID", 0L);
				IGrouping<long, DynamicObject> source;
				if (dictionary.TryGetValue(dynamicValue4, out source))
				{
					long dynamicValue5 = DataEntityExtend.GetDynamicValue<long>(dynamicObject2, "FMASTERID", 0L);
					if (dynamicValue5 > 0L)
					{
						MFGBillUtil.SetValue(base.View, "FEntrySource", source.FirstOrDefault<DynamicObject>(), "2");
					}
					else
					{
						MFGBillUtil.SetValue(base.View, "FEntrySource", source.FirstOrDefault<DynamicObject>(), "1");
					}
				}
			}
		}

		// Token: 0x060000E5 RID: 229 RVA: 0x0000C814 File Offset: 0x0000AA14
		private void ShowBomTreeEdit(string formId)
		{
			if (!this.VaildateIsHavePermission("ENG_BOMTREE"))
			{
				base.View.ShowMessage(ResManager.LoadKDString("没有BOM“树形维护”的“查看”权限！", "015072000003453", 7, new object[0]), 0);
				return;
			}
			DynamicObject dataObject = base.View.Model.DataObject;
			long dynamicValue = DataEntityExtend.GetDynamicValue<long>(dataObject, "ID", 0L);
			DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
			dynamicFormShowParameter.OpenStyle.ShowType = 7;
			dynamicFormShowParameter.FormId = formId;
			dynamicFormShowParameter.CustomParams["BOMID"] = dynamicValue.ToString();
			base.View.ShowForm(dynamicFormShowParameter);
		}

		// Token: 0x060000E6 RID: 230 RVA: 0x0000C8AC File Offset: 0x0000AAAC
		private void ShowBomModifyLogEdit(string formId)
		{
			if (!this.VaildateIsHavePermission("ENG_BomModifyLog"))
			{
				base.View.ShowMessage(ResManager.LoadKDString("没有BOM修改日志查询的“查看”权限！", "015072000012829", 7, new object[0]), 0);
				return;
			}
			DynamicObject dataObject = base.View.Model.DataObject;
			long value = MFGBillUtil.GetValue<long>(base.View.Model, "FID", -1, 0L, null);
			long value2 = MFGBillUtil.GetValue<long>(base.View.Model, "FMATERIALID", -1, 0L, null);
			long value3 = MFGBillUtil.GetValue<long>(base.View.Model, "FUseOrgId", -1, 0L, null);
			DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
			dynamicFormShowParameter.OpenStyle.ShowType = 7;
			dynamicFormShowParameter.FormId = formId;
			dynamicFormShowParameter.CustomComplexParams.Add("HeadBomId", value);
			dynamicFormShowParameter.CustomComplexParams.Add("ParMaterialId", value2);
			dynamicFormShowParameter.CustomComplexParams.Add("HeadUseOrgId", value3);
			base.View.ShowForm(dynamicFormShowParameter);
		}

		// Token: 0x060000E7 RID: 231 RVA: 0x0000C9B0 File Offset: 0x0000ABB0
		private bool VaildateIsHavePermission(string perItemId)
		{
			PermissionAuthResult permissionAuthResult = PermissionServiceHelper.FuncPermissionAuth(base.Context, new BusinessObject
			{
				Id = perItemId,
				SubSystemId = base.View.Model.SubSytemId
			}, "6e44119a58cb4a8e86f6c385e14a17ad");
			return permissionAuthResult.Passed;
		}

		// Token: 0x060000E8 RID: 232 RVA: 0x0000C9F8 File Offset: 0x0000ABF8
		private bool CheckMaterialChildSuite(BeforeUpdateValueEventArgs e)
		{
			DynamicObject dynamicValue = DataEntityExtend.GetDynamicValue<DynamicObject>(this.Model.DataObject, "MATERIALID", null);
			if (dynamicValue == null)
			{
				return true;
			}
			long dynamicValue2 = DataEntityExtend.GetDynamicValue<long>(DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(dynamicValue, "MaterialBase", null).First<DynamicObject>(), "Suite", 0L);
			if (dynamicValue2 == 0L)
			{
				if (e.Value == null)
				{
					return true;
				}
				if (e.Value.GetType() == typeof(long))
				{
					return true;
				}
				if (e.Value.GetType() == typeof(DynamicObject))
				{
					long dynamicValue3 = DataEntityExtend.GetDynamicValue<long>(DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(e.Value as DynamicObject, "MaterialBase", null).First<DynamicObject>(), "Suite", 0L);
					if (dynamicValue3 == 1L)
					{
						base.View.ShowErrMessage(ResManager.LoadKDString("父项物料为非套件，子项物料不允许选择套件物料", "015072030033293", 7, new object[0]), "", 0);
						base.View.Model.DeleteEntryRow("FTreeEntity", e.Row);
						return false;
					}
				}
			}
			return true;
		}

		// Token: 0x060000E9 RID: 233 RVA: 0x0000CB34 File Offset: 0x0000AD34
		private void ChangePositonNo(DataChangedEventArgs e)
		{
			Entity entity = base.View.BusinessInfo.GetEntity("FTreeEntity");
			DynamicObject entityDataObject = this.Model.GetEntityDataObject(entity, e.Row);
			string dynamicValue = DataEntityExtend.GetDynamicValue<string>(entityDataObject, "MATERIALTYPE", null);
			bool dynamicValue2 = DataEntityExtend.GetDynamicValue<bool>(entityDataObject, "IskeyItem", false);
			if (ObjectUtils.IsNullOrEmptyOrWhiteSpace(dynamicValue) || !StringUtils.EqualsIgnoreCase(dynamicValue, "1") || !dynamicValue2)
			{
				return;
			}
			int replaceGroup = DataEntityExtend.GetDynamicValue<int>(entityDataObject, "ReplaceGroup", 0);
			DynamicObjectCollection entityDataObject2 = this.Model.GetEntityDataObject(entity);
			List<DynamicObject> list = (from i in entityDataObject2
			where DataEntityExtend.GetDynamicValue<int>(i, "ReplaceGroup", 0) == replaceGroup && StringUtils.EqualsIgnoreCase(DataEntityExtend.GetDynamicValue<string>(i, "MATERIALTYPE", null), "3")
			select i).ToList<DynamicObject>();
			foreach (DynamicObject dynamicObject in list)
			{
				DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "POSITIONNO", e.NewValue);
				base.View.UpdateView("FPOSITIONNO", entityDataObject2.IndexOf(dynamicObject));
			}
		}

		// Token: 0x060000EA RID: 234 RVA: 0x0000CC48 File Offset: 0x0000AE48
		private void OpenEcnList(BarItemClickEventArgs e)
		{
			ListShowParameter listShowParameter = new ListShowParameter
			{
				FormId = "ENG_ECNOrder",
				PermissionItemId = "6e44119a58cb4a8e86f6c385e14a17ad",
				IsLookUp = false,
				IsShowApproved = false,
				IsShowUsed = true,
				IsIsolationOrg = false
			};
			List<long> list = new List<long>
			{
				Convert.ToInt64(base.View.Model.GetPKValue())
			};
			List<long> ecnIdByBomId = BOMServiceHelper.GetEcnIdByBomId(base.Context, list);
			if (ListUtils.IsEmpty<long>(ecnIdByBomId))
			{
				e.Cancel = true;
				base.View.ShowMessage(ResManager.LoadKDString("当前物料清单没有关联的工程变更单!", "015072030038261", 7, new object[0]), 0);
				return;
			}
			string text = string.Format("fid in ({0})", string.Join<long>(",", ecnIdByBomId.Distinct<long>()));
			listShowParameter.ListFilterParameter.Filter = StringUtils.JoinFilterString(listShowParameter.ListFilterParameter.Filter, text, "AND");
			base.View.ShowForm(listShowParameter);
		}

		// Token: 0x060000EB RID: 235 RVA: 0x0000CD74 File Offset: 0x0000AF74
		private void GetLadderLoss(bool isSaveOperation = false)
		{
			if (!LadderLossUtils.IsUpdatePermission(base.Context))
			{
				base.View.ShowMessage(ResManager.LoadKDString("当前用户没有【更新物料清单】权限，请联系管理员授权！", "0151515153499030038837", 7, new object[0]), 4);
				return;
			}
			if (!LadderLossUtils.IsModifyPermisson(base.Context, "ENG_BOM"))
			{
				base.View.ShowErrMessage(ResManager.LoadKDString("当前用户没有【物料清单修改】权限，请联系管理员授权！", "0151515153499000018754", 7, new object[0]), "", 0);
				return;
			}
			int[] selectedRows = base.View.GetControl<TreeEntryGrid>("FTreeEntity").GetSelectedRows();
			if (selectedRows.Length > 1)
			{
				base.View.ShowMessage(ResManager.LoadKDString("请选择一行子项明细！", "015072030039143", 7, new object[0]), 0);
				return;
			}
			if (MFGBillUtil.GetValue<long>(this.Model, "FDOSAGETYPE", selectedRows[0], 0L, null) != 3L)
			{
				base.View.ShowMessage(ResManager.LoadKDString("当前选择的子项明细不是“阶梯”用量类型！", "015072030039144", 7, new object[0]), 0);
				return;
			}
			base.View.ShowMessage(ResManager.LoadKDString("是否要获取并覆盖阶梯用量？", "015072030039145", 7, new object[0]), 4, delegate(MessageBoxResult result)
			{
				if (result == 6 && selectedRows.Length == 1)
				{
					this.CreateFlotBasedqtyByLadderLoss(selectedRows[0], false);
				}
			}, "", 0);
		}

		// Token: 0x060000EC RID: 236 RVA: 0x0000CEB8 File Offset: 0x0000B0B8
		private bool CreateFlotBasedqtyByLadderLoss(int rowIndex, bool blnIfAuto)
		{
			DynamicObject value = MFGBillUtil.GetValue<DynamicObject>(this.Model, "FMATERIALIDCHILD", rowIndex, null, null);
			if (value == null)
			{
				return false;
			}
			long value2 = MFGBillUtil.GetValue<long>(base.View.Model, "FUseOrgId", -1, 0L, null);
			FormMetadata formMetadata = MetaDataServiceHelper.Load(base.Context, "ENG_LadderLoss", true) as FormMetadata;
			QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter();
			queryBuilderParemeter.BusinessInfo = formMetadata.BusinessInfo;
			queryBuilderParemeter.FormId = "ENG_LadderLoss";
			queryBuilderParemeter.FilterClauseWihtKey = string.Format("FDocumentStatus='C' and FFORBIDSTATUS= 'A' and FMATERIALID='{0}' and FUseOrgId='{1}'", value["Id"], value2);
			DynamicObject[] array = BusinessDataServiceHelper.Load(base.Context, formMetadata.BusinessInfo.GetDynamicObjectType(), queryBuilderParemeter);
			if (array.Length == 0)
			{
				if (!blnIfAuto)
				{
					base.View.ShowMessage(ResManager.LoadKDString("该物料没有对应的阶梯损耗数据！", "015072030039146", 7, new object[0]), 0);
				}
				return false;
			}
			if (array.Length == 1)
			{
				this.DeleteFlotBasedqty();
				decimal num = 1m;
				decimal num2 = 1m;
				DynamicObjectCollection entityDataObject = this.Model.GetEntityDataObject(base.View.BillBusinessInfo.GetEntity("FTreeEntity"));
				if (entityDataObject.Count > rowIndex)
				{
					DynamicObject dynamicObject = entityDataObject[rowIndex];
					num = DataEntityExtend.GetDynamicValue<decimal>(dynamicObject, "NUMERATOR", 1m);
					num2 = DataEntityExtend.GetDynamicValue<decimal>(dynamicObject, "DENOMINATOR", 1m);
				}
				DynamicObject dynamicObject2 = array[0];
				DynamicObjectCollection dynamicObjectCollection = dynamicObject2["LADDERLOSSENTRY"] as DynamicObjectCollection;
				DynamicObject value3 = MFGBillUtil.GetValue<DynamicObject>(this.Model, "FMATERIALID", -1, null, null);
				UnitConvert unitConvertRate = UnitConvertServiceHelper.GetUnitConvertRate(base.View.Context, new GetUnitConvertRateArgs
				{
					MaterialId = DataEntityExtend.GetDynamicValue<long>(value3, "Id", 0L),
					MasterId = DataEntityExtend.GetDynamicValue<long>(value3, "msterID", 0L),
					SourceUnitId = MFGBillUtil.GetValue<long>(this.Model, "FUNITID", -1, 0L, null),
					DestUnitId = MFGBillUtil.GetValue<long>(this.Model, "FBaseUnitId", -1, 0L, null)
				});
				DynamicObject entityDataObject2 = this.Model.GetEntityDataObject(base.View.BillBusinessInfo.GetEntity("FTreeEntity"), rowIndex);
				UnitConvert unitConvertRate2 = UnitConvertServiceHelper.GetUnitConvertRate(base.View.Context, new GetUnitConvertRateArgs
				{
					MaterialId = DataEntityExtend.GetDynamicValue<long>(value, "Id", 0L),
					MasterId = DataEntityExtend.GetDynamicValue<long>(value, "msterID", 0L),
					SourceUnitId = Convert.ToInt64(dynamicObject2["UnitID_Id"]),
					DestUnitId = Convert.ToInt64(entityDataObject2["CHILDUNITID_Id"])
				});
				foreach (DynamicObject dynamicObject3 in dynamicObjectCollection)
				{
					base.View.Model.CreateNewEntryRow("FBOMCHILDLOTBASEDQTY");
					int entryRowCount = base.View.Model.GetEntryRowCount("FBOMCHILDLOTBASEDQTY");
					this.Model.SetValue("FMATERIALIDLOTBASED", value["Id"], entryRowCount - 1);
					this.Model.SetValue("FSTARTQTY", dynamicObject3["STARTQTY"], entryRowCount - 1);
					this.Model.SetValue("FENDQTY", dynamicObject3["ENDQTY"], entryRowCount - 1);
					this.Model.SetValue("FFIXSCRAPQTYLOT", unitConvertRate2.ConvertQty(Convert.ToInt64(dynamicObject3["FIXSCRAPQTY"]), ""), entryRowCount - 1);
					this.Model.SetValue("FSCRAPRATELOT", dynamicObject3["SCRAPRATE"], entryRowCount - 1);
					this.Model.SetValue("FNOTELOT", dynamicObject3["NOTE"], entryRowCount - 1);
					this.Model.SetValue("FNUMERATORLOT", num, entryRowCount - 1);
					this.Model.SetValue("FDENOMINATORLOT", num2, entryRowCount - 1);
					UnitConvert unitConvertRate3 = UnitConvertServiceHelper.GetUnitConvertRate(base.View.Context, new GetUnitConvertRateArgs
					{
						MaterialId = DataEntityExtend.GetDynamicObjectItemValue<long>(value, "Id", 0L),
						MasterId = DataEntityExtend.GetDynamicObjectItemValue<long>(value, "msterID", 0L),
						SourceUnitId = MFGBillUtil.GetValue<long>(this.Model, "FUNITIDLOT", entryRowCount - 1, 0L, null),
						DestUnitId = MFGBillUtil.GetValue<long>(this.Model, "FBASEUNITIDLOT", entryRowCount - 1, 0L, null)
					});
					this.Model.SetValue("FBASENUMERATORLOT", unitConvertRate3.ConvertQty(num, ""), entryRowCount - 1);
					this.Model.SetValue("FBASEDENOMINATORLOT", unitConvertRate.ConvertQty(num2, ""), entryRowCount - 1);
				}
				base.View.UpdateView("FBOMCHILDLOTBASEDQTY");
				return true;
			}
			return false;
		}

		// Token: 0x060000ED RID: 237 RVA: 0x0000D70C File Offset: 0x0000B90C
		private void UpdateLadderLoss(bool isSaveOperation = false)
		{
			if (!LadderLossUtils.IsGetDataPermission(base.Context))
			{
				base.View.ShowMessage(ResManager.LoadKDString("当前用户没有【获取物料清单阶梯用量】权限，请联系管理员授权！", "0151515153499030038838", 7, new object[0]), 4);
				return;
			}
			if (!LadderLossUtils.IsModifyPermisson(base.Context, "ENG_LadderLoss"))
			{
				base.View.ShowErrMessage(ResManager.LoadKDString("当前用户没有【物料阶梯损耗修改】权限，请联系管理员授权！", "0151515153499000018753", 7, new object[0]), "", 0);
				return;
			}
			Entity entity = base.View.BusinessInfo.GetEntity("FBOMCHILDLOTBASEDQTY");
			DynamicObjectCollection dataObjectLot = this.Model.GetEntityDataObject(entity);
			if (dataObjectLot.Count == 0)
			{
				base.View.ShowMessage(ResManager.LoadKDString("当前列表没有数据！", "015072030039139", 7, new object[0]), 0);
				return;
			}
			base.View.ShowMessage(ResManager.LoadKDString("是否要将当前数据更新到阶梯损耗表？", "015072030039140", 7, new object[0]), 4, delegate(MessageBoxResult result)
			{
				if (result != 6)
				{
					return;
				}
				long value = MFGBillUtil.GetValue<long>(this.View.Model, "FUseOrgId", -1, 0L, null);
				FormMetadata formMetadata = MetaDataServiceHelper.Load(this.Context, "ENG_LadderLoss", true) as FormMetadata;
				QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter();
				queryBuilderParemeter.BusinessInfo = formMetadata.BusinessInfo;
				queryBuilderParemeter.FormId = "ENG_LadderLoss";
				queryBuilderParemeter.FilterClauseWihtKey = string.Format("FMATERIALID='{0}' and FUseOrgId='{1}'", dataObjectLot[0]["MATERIALIDLOTBASED_Id"], value);
				DynamicObject[] array = BusinessDataServiceHelper.Load(this.Context, formMetadata.BusinessInfo.GetDynamicObjectType(), queryBuilderParemeter);
				DynamicObject dyn;
				if (array.Length != 1)
				{
					FormMetadata formMetadata2 = (FormMetadata)MetaDataServiceHelper.Load(this.Context, "ENG_LadderLoss", true);
					DynamicObjectType dynamicObjectType = formMetadata2.BusinessInfo.GetDynamicObjectType();
					dyn = new DynamicObject(dynamicObjectType);
					dyn["MATERIALID_Id"] = dataObjectLot[0]["MATERIALIDLOTBASED_Id"];
					dyn["FORBIDSTATUS"] = "A";
					dyn["CreateOrgId_Id"] = value;
					dyn["UseOrgId_Id"] = value;
					dyn["CreatorId_Id"] = Convert.ToInt32(this.Context.UserId);
					dyn["CreateDate"] = TimeServiceHelper.GetSystemDateTime(this.Context);
					dyn["MODIFIERID_Id"] = Convert.ToInt32(this.Context.UserId);
					dyn["MODIFYDate"] = TimeServiceHelper.GetSystemDateTime(this.Context);
					DynamicObject dynamicObject = dataObjectLot[0]["MATERIALIDLOTBASED"] as DynamicObject;
					DynamicObjectCollection dynamicObjectCollection = dynamicObject["MaterialBase"] as DynamicObjectCollection;
					if (dynamicObjectCollection.Count > 0)
					{
						dyn["UnitID_Id"] = dynamicObjectCollection[0]["BaseUnitId_Id"];
					}
					this.UpdateLadderLossSave(dyn, dataObjectLot, true);
					return;
				}
				dyn = array[0];
				if (dyn["DocumentStatus"].ToString() == "C")
				{
					this.View.ShowMessage(ResManager.LoadKDString("该物料的阶梯损耗是已审核状态，确定要覆盖吗？", "015072030039141", 7, new object[0]), 4, delegate(MessageBoxResult result1)
					{
						if (result1 != 6)
						{
							return;
						}
						this.UpdateLadderLossSave(dyn, dataObjectLot, false);
					}, "", 0);
					return;
				}
				this.UpdateLadderLossSave(dyn, dataObjectLot, false);
			}, "", 0);
		}

		// Token: 0x060000EE RID: 238 RVA: 0x0000D83C File Offset: 0x0000BA3C
		private void UpdateLadderLossSave(DynamicObject dyn, DynamicObjectCollection dataObjectLot, bool blnIfNew = false)
		{
			DynamicObjectCollection dynamicObjectCollection = dyn["LADDERLOSSENTRY"] as DynamicObjectCollection;
			dynamicObjectCollection.Clear();
			int num = 1;
			foreach (DynamicObject dynamicObject in dataObjectLot)
			{
				DynamicObject dynamicObject2 = new DynamicObject(dynamicObjectCollection.DynamicCollectionItemPropertyType);
				dynamicObject2["Seq"] = num++;
				dynamicObject2["STARTQTY"] = dynamicObject["STARTQTY"];
				dynamicObject2["ENDQTY"] = dynamicObject["ENDQTY"];
				dynamicObject2["FIXSCRAPQTY"] = dynamicObject["FIXSCRAPQTYLOT"];
				dynamicObject2["SCRAPRATE"] = dynamicObject["SCRAPRATELOT"];
				dynamicObject2["NOTE"] = dynamicObject["NOTELOT"];
				dyn["UnitID_Id"] = dynamicObject["UNITIDLOT_Id"];
				dynamicObjectCollection.Add(dynamicObject2);
				if (Convert.ToDecimal(dynamicObject2["FIXSCRAPQTY"]) == 0m && Convert.ToDecimal(dynamicObject2["SCRAPRATE"]) == 0m)
				{
					base.View.ShowMessage(string.Format(ResManager.LoadKDString("阶梯用量第{0}分录中存在固定损耗和变动损耗率同时为0，请检查！", "0151515153498030039116", 7, new object[0]), dynamicObject2["seq"]), 4);
					return;
				}
			}
			FormMetadata formMetadata = MetaDataServiceHelper.Load(base.Context, "ENG_LadderLoss", true) as FormMetadata;
			IOperationResult operationResult = BusinessDataServiceHelper.Save(base.Context, formMetadata.BusinessInfo, dyn, null, "");
			if (blnIfNew)
			{
				FormMetadata formMetadata2 = MetaDataServiceHelper.Load(base.Context, "BOS_BillUserParameter", true) as FormMetadata;
				DynamicObject dynamicObject3 = UserParamterServiceHelper.Load(base.Context, formMetadata2.BusinessInfo, base.Context.UserId, "ENG_LadderLoss", "UserParameter");
				bool dynamicValue = DataEntityExtend.GetDynamicValue<bool>(dynamicObject3, "FSaveAndSubmit", false);
				bool dynamicValue2 = DataEntityExtend.GetDynamicValue<bool>(dynamicObject3, "FSubmitAndAudit", false);
				OperateOption operateOption = OperateOption.Create();
				if (dynamicValue)
				{
					IEnumerable<object> source = from e in operationResult.OperateResult
					where e.SuccessStatus
					select e into x
					select x.PKValue;
					if (source.Count<object>() > 0)
					{
						IOperationResult operationResult2 = BusinessDataServiceHelper.Submit(base.Context, formMetadata.BusinessInfo, source.ToArray<object>(), 6.ToString(), operateOption.Copy());
						if (dynamicValue2)
						{
							IEnumerable<object> source2 = from e in operationResult2.OperateResult
							where e.SuccessStatus
							select e into x
							select x.PKValue;
							if (source2.Count<object>() > 0)
							{
								BusinessDataServiceHelper.Audit(base.Context, formMetadata.BusinessInfo, source2.ToArray<object>(), operateOption.Copy());
							}
						}
					}
				}
			}
			base.View.ShowMessage(ResManager.LoadKDString("更新成功！", "015072030039142", 7, new object[0]), 0);
		}

		// Token: 0x060000EF RID: 239 RVA: 0x0000DBE0 File Offset: 0x0000BDE0
		private void BomIntegrityCheck()
		{
			object pkvalue = base.View.Model.GetPKValue();
			long item = 0L;
			List<long> list = new List<long>();
			if (!ObjectUtils.IsNullOrEmpty(pkvalue) && long.TryParse(pkvalue.ToString(), out item))
			{
				list.Add(item);
			}
			EntryEntity entryEntity = base.View.BusinessInfo.GetEntryEntity("FTreeEntity");
			DynamicObjectCollection entityDataObject = base.View.Model.GetEntityDataObject(entryEntity);
			List<long> list2 = new List<long>();
			IEnumerable<long> enumerable = (from x in entityDataObject
			select DataEntityExtend.GetDynamicValue<long>(x, "Id", 0L)).Distinct<long>();
			DynamicObjectCollection mtrlMster = BOMServiceHelper.GetMtrlMster(base.Context, enumerable);
			Dictionary<string, IGrouping<string, DynamicObject>> dictionary = (from g in mtrlMster
			group g by DataEntityExtend.GetDynamicValue<long>(g, "FMASTERID", 0L) + "_" + DataEntityExtend.GetDynamicValue<long>(g, "FUSEORGID", 0L)).ToDictionary((IGrouping<string, DynamicObject> x) => x.Key);
			foreach (DynamicObject dynamicObject in entityDataObject)
			{
				if (DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObject, "ChildSupplyOrgId_Id", 0L) > 0L)
				{
					DynamicObject dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<DynamicObject>(dynamicObject, "MATERIALIDCHILD", null);
					string key = DataEntityExtend.GetDynamicValue<long>(dynamicObjectItemValue, "msterID", 0L) + "_" + DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObject, "ChildSupplyOrgId_Id", 0L);
					IGrouping<string, DynamicObject> grouping = null;
					if (dictionary.TryGetValue(key, out grouping) && !ListUtils.IsEmpty<DynamicObject>(grouping))
					{
						list2.Add(DataEntityExtend.GetDynamicValue<long>(grouping.FirstOrDefault<DynamicObject>(), "FMATERIALID", 0L));
					}
				}
				else
				{
					list2.Add(DataEntityExtend.GetDynamicObjectItemValue<long>(dynamicObject, "MATERIALIDCHILD_Id", 0L));
				}
			}
			list2 = list2.Distinct<long>().ToList<long>();
			base.View.ShowBomIntegrity(list, list2);
		}

		// Token: 0x060000F0 RID: 240 RVA: 0x0000DDD4 File Offset: 0x0000BFD4
		private DynamicObject GetPLMInfo(DynamicObject dynamicObject, string barItem)
		{
			long dynamicValue = DataEntityExtend.GetDynamicValue<long>(dynamicObject, "msterId", 0L);
			if (dynamicValue == 0L)
			{
				return null;
			}
			DynamicObjectCollection dynamicObjectCollection;
			if (StringUtils.EqualsIgnoreCase(barItem, "tbSelectPLMMaterial"))
			{
				dynamicObjectCollection = BOMServiceHelper.GetPlmMaterial(base.Context, dynamicValue);
			}
			else
			{
				dynamicObjectCollection = BOMServiceHelper.GetPlmBom(base.Context, dynamicValue);
			}
			if (!ListUtils.IsEmpty<DynamicObject>(dynamicObjectCollection))
			{
				return dynamicObjectCollection[0];
			}
			return null;
		}

		// Token: 0x060000F1 RID: 241 RVA: 0x0000DE34 File Offset: 0x0000C034
		private bool IsHasLicense(string id)
		{
			bool result = true;
			try
			{
				LicenseVerifier.CheckUserAppGroup(base.Context, base.Context.UserName, "PLM");
				LicenseVerifier.CheckSystemLicense(base.Context, id);
			}
			catch
			{
				result = false;
			}
			return result;
		}

		// Token: 0x1700000F RID: 15
		// (get) Token: 0x060000F2 RID: 242 RVA: 0x0000DE84 File Offset: 0x0000C084
		// (set) Token: 0x060000F3 RID: 243 RVA: 0x0000DE8C File Offset: 0x0000C08C
		private List<DynamicObject> selBomRows { get; set; }

		// Token: 0x060000F4 RID: 244 RVA: 0x0000DED0 File Offset: 0x0000C0D0
		private void RepaceSetup()
		{
			if (!this.ValidatePermission(false))
			{
				return;
			}
			EntryEntity entryEntity = base.View.BusinessInfo.GetEntryEntity("FTreeEntity");
			List<int> list = (from i in base.View.GetControl<EntryGrid>(entryEntity.Key).GetSelectedRows()
			orderby i
			select i).ToList<int>();
			if (list.Count <= 0 || list.FirstOrDefault<int>() < 0)
			{
				return;
			}
			DynamicObjectCollection entityDataObject = base.View.Model.GetEntityDataObject(entryEntity);
			bool flag = true;
			List<DynamicObject> list2 = this.ValidateSelRep(list, entityDataObject, ref flag);
			if (list2.Any((DynamicObject p) => DataEntityExtend.GetDynamicValue<string>(p, "EntrySource", null) == "2") && !this.ValidateIsInCreateOrgId())
			{
				return;
			}
			if (!flag)
			{
				return;
			}
			if (list2.Count == 0)
			{
				base.View.ShowErrMessage(ResManager.LoadKDString("未选中有效子项物料，不能进行替代设置！", "015072000002169", 7, new object[0]), "", 0);
				return;
			}
			this.selBomRows = new List<DynamicObject>();
			this.selBomRows.AddRange(list2);
			PlugIn plugIn = new PlugIn("SubStituteView");
			plugIn.Id = SequentialGuid.NewGuid().ToString();
			plugIn.OrderId = 0;
			plugIn.IsEnabled = true;
			plugIn.ClassName = "Kingdee.K3.MFG.ENG.Business.PlugIn.Base.SubStituteView, Kingdee.K3.MFG.ENG.Business.PlugIn";
			BillShowParameter billShowParameter = new BillShowParameter();
			billShowParameter.OpenStyle.ShowType = 6;
			billShowParameter.FormId = "ENG_Substitution";
			billShowParameter.LayoutId = "c00e6a5e-931f-497b-9908-8b9b3035b797";
			billShowParameter.ParentPageId = base.View.PageId;
			billShowParameter.DynamicPlugins.Add(plugIn);
			billShowParameter.CustomParams.Add("showbeforesave", "1");
			base.View.Session["SelBomChItems"] = list2;
			base.View.ShowForm(billShowParameter, delegate(FormResult result)
			{
				if (result.ReturnData is DynamicObject)
				{
					this.BindRepMainData((DynamicObject)result.ReturnData);
				}
			});
		}

		// Token: 0x060000F5 RID: 245 RVA: 0x0000E0C0 File Offset: 0x0000C2C0
		private bool ValidateIsInCreateOrgId()
		{
			long value = MFGBillUtil.GetValue<long>(this.Model, "FCreateOrgId", -1, 0L, null);
			long value2 = MFGBillUtil.GetValue<long>(this.Model, "FUseOrgId", -1, 0L, null);
			if (value != value2)
			{
				base.View.ShowMessage(ResManager.LoadKDString("暂不支持在使用组织下进行替代设置，请在创建组织进行替代设置!", "015072000018121", 7, new object[0]), 0);
				return false;
			}
			return true;
		}

		// Token: 0x060000F6 RID: 246 RVA: 0x0000E14C File Offset: 0x0000C34C
		private List<DynamicObject> ValidateSelRep(List<int> selRowsSeq, DynamicObjectCollection allRows, ref bool chkResult)
		{
			StringBuilder stringBuilder = new StringBuilder();
			List<int> list = new List<int>();
			List<int> list2 = new List<int>();
			List<int> list3 = new List<int>();
			List<int> list4 = new List<int>();
			List<DynamicObject> list5 = new List<DynamicObject>();
			int count = allRows.Count;
			foreach (int num in selRowsSeq)
			{
				if (num >= count)
				{
					break;
				}
				if (DataEntityExtend.GetDynamicObjectItemValue<long>(allRows[num], "MATERIALIDCHILD_Id", 0L) > 0L)
				{
					if (DataEntityExtend.GetDynamicObjectItemValue<int>(allRows[num], "MATERIALTYPE", 0) == 2)
					{
						list.Add(DataEntityExtend.GetDynamicObjectItemValue<int>(allRows[num], "Seq", 0));
					}
					else if (DataEntityExtend.GetDynamicObjectItemValue<int>(DataEntityExtend.GetDynamicObjectItemValue<DynamicObjectCollection>(DataEntityExtend.GetDynamicObjectItemValue<DynamicObject>(allRows[num], "MATERIALIDCHILD", null), "MaterialBase", null).First<DynamicObject>(), "ErpClsID", 0) == 4 || DataEntityExtend.GetDynamicObjectItemValue<int>(DataEntityExtend.GetDynamicObjectItemValue<DynamicObjectCollection>(DataEntityExtend.GetDynamicObjectItemValue<DynamicObject>(allRows[num], "MATERIALIDCHILD", null), "MaterialBase", null).First<DynamicObject>(), "ErpClsID", 0) == 5)
					{
						list2.Add(DataEntityExtend.GetDynamicObjectItemValue<int>(allRows[num], "Seq", 0));
					}
					else
					{
						string dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<string>(base.View.Model.DataObject, "BOMCATEGORY", null);
						string dynamicObjectItemValue2 = DataEntityExtend.GetDynamicObjectItemValue<string>(DataEntityExtend.GetDynamicObjectItemValue<DynamicObjectCollection>(DataEntityExtend.GetDynamicObjectItemValue<DynamicObject>(base.View.Model.DataObject, "MATERIALID", null), "MaterialBase", null).First<DynamicObject>(), "ErpClsID", null);
						if (DataEntityExtend.GetDynamicObjectItemValue<int>(DataEntityExtend.GetDynamicObjectItemValue<DynamicObjectCollection>(DataEntityExtend.GetDynamicObjectItemValue<DynamicObject>(allRows[num], "MATERIALIDCHILD", null), "MaterialBase", null).First<DynamicObject>(), "ErpClsID", 0) == 9 && (!StringUtils.EqualsIgnoreCase(dynamicObjectItemValue, "1") || StringUtils.EqualsIgnoreCase(dynamicObjectItemValue2, "4")))
						{
							list3.Add(DataEntityExtend.GetDynamicObjectItemValue<int>(allRows[num], "Seq", 0));
						}
						else if (DataEntityExtend.GetDynamicObjectItemValue<bool>(allRows[num], "IsSkip", false))
						{
							list4.Add(DataEntityExtend.GetDynamicObjectItemValue<int>(allRows[num], "Seq", 0));
						}
						else
						{
							list5.Add(allRows[num]);
						}
					}
				}
			}
			if (list.Count > 0)
			{
				stringBuilder.AppendLine(string.Format(ResManager.LoadKDString("第{0}行子项物料为返还件，不能进行替代设置！", "015072000002170", 7, new object[0]), string.Join<int>(",", list)));
			}
			if (list2.Count > 0)
			{
				stringBuilder.AppendLine(string.Format(ResManager.LoadKDString("第{0}行子项物料为特征件/虚拟件，不能进行替代设置！", "015072000016542", 7, new object[0]), string.Join<int>(",", list2)));
			}
			if (list3.Count > 0)
			{
				stringBuilder.AppendLine(string.Format(ResManager.LoadKDString("第{0}行子项物料为配制件但BOM分录不为标准BOM或父项物料不为非特征件，不能进行替代设置！", "015072030036805", 7, new object[0]), string.Join<int>(",", list3)));
			}
			if (list4.Count > 0)
			{
				stringBuilder.AppendLine(string.Format(ResManager.LoadKDString("第{0}行子项物料勾选了跳层，不能进行替代设置！", "015072000017268", 7, new object[0]), string.Join<int>(",", list4)));
			}
			if (stringBuilder.Length > 0)
			{
				base.View.ShowErrMessage(stringBuilder.ToString(), "", 0);
				chkResult = false;
			}
			IEnumerable<IGrouping<int, DynamicObject>> source = from g in list5
			group g by DataEntityExtend.GetDynamicObjectItemValue<int>(g, "ReplaceGroup", 0);
			if (source.Count<IGrouping<int, DynamicObject>>() == 1)
			{
				list5 = new List<DynamicObject>();
				using (IEnumerator<DynamicObject> enumerator2 = allRows.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						DynamicObject dynamicObject = enumerator2.Current;
						if (DataEntityExtend.GetDynamicObjectItemValue<int>(dynamicObject, "ReplaceGroup", 0) == source.First<IGrouping<int, DynamicObject>>().Key)
						{
							list5.Add(dynamicObject);
						}
					}
					return list5;
				}
			}
			IEnumerable<DynamicObject> source2 = from w in list5
			where !string.IsNullOrWhiteSpace(DataEntityExtend.GetDynamicObjectItemValue<string>(w, "ReplacePolicy", null))
			select w;
			if (source2.Count<DynamicObject>() > 0)
			{
				base.View.ShowErrMessage(string.Format(ResManager.LoadKDString("BOM子项第{0}行不属于同一个替代维护，请重新选择！", "015072000002172", 7, new object[0]), string.Join<int>(",", (from s in selRowsSeq
				select s + 1).ToList<int>())), "", 0);
				chkResult = false;
			}
			return list5;
		}

		// Token: 0x060000F7 RID: 247 RVA: 0x0000E64C File Offset: 0x0000C84C
		private void ReplaceDelte()
		{
			if (!this.ValidatePermission(false))
			{
				return;
			}
			EntryEntity entryEntity = base.View.BusinessInfo.GetEntryEntity("FTreeEntity");
			DynamicObjectCollection allRows = base.View.Model.GetEntityDataObject(entryEntity);
			List<int> list = (from o in base.View.GetControl<EntryGrid>(entryEntity.Key).GetSelectedRows()
			orderby o
			select o).ToList<int>();
			if (list.Count <= 0 || list.FirstOrDefault<int>() < 0)
			{
				return;
			}
			List<int> list2 = new List<int>();
			List<string> list3 = new List<string>();
			int count = allRows.Count;
			using (List<int>.Enumerator enumerator = list.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					int i = enumerator.Current;
					if (i >= count)
					{
						break;
					}
					if (!string.IsNullOrWhiteSpace(DataEntityExtend.GetDynamicObjectItemValue<string>(allRows[i], "ReplacePolicy", null)))
					{
						List<string> collection = (from w in allRows
						where DataEntityExtend.GetDynamicObjectItemValue<int>(w, "ReplaceGroup", 0) == DataEntityExtend.GetDynamicObjectItemValue<int>(allRows[i], "ReplaceGroup", 0) && DataEntityExtend.GetDynamicObjectItemValue<int>(w, "MATERIALTYPE", 0) == 1
						select w into s
						select DataEntityExtend.GetDynamicObjectItemValue<string>(s, "RowId", null)).ToList<string>();
						list3.AddRange(collection);
					}
					else
					{
						list2.Add(DataEntityExtend.GetDynamicObjectItemValue<int>(allRows[i], "Seq", 0));
					}
				}
			}
			if (list3.Count > 0)
			{
				this.DelAllSubsItems(list3);
				this.RemoveMainRepInfo(list3);
				this.SetReplaceGroup();
			}
			if (list2.Count > 0)
			{
				base.View.ShowErrMessage(string.Format(ResManager.LoadKDString("第{0}行不存在替代关系，替代删除不成功！", "015072000002173", 7, new object[0]), string.Join<int>(",", list2)), "", 0);
			}
		}

		// Token: 0x060000F8 RID: 248 RVA: 0x0000E868 File Offset: 0x0000CA68
		private bool ValidatePermission(bool isSaveOperation = false)
		{
			bool flag = MFGCommonUtil.AuthPermissionBeforeShowF7Form(base.View, "ENG_BOM", "ad440ef0395e453891b47f9f6d41c3de");
			if (!flag)
			{
				if (!isSaveOperation)
				{
					base.View.ShowMessage(ResManager.LoadKDString("没有物料清单的“替代设置”权限！", "015072000002174", 7, new object[0]), 0);
				}
				flag = false;
			}
			return flag;
		}

		// Token: 0x060000F9 RID: 249 RVA: 0x0000EA00 File Offset: 0x0000CC00
		private void BindRepMainData(DynamicObject repData)
		{
			int entryCurrentRowIndex = base.View.Model.GetEntryCurrentRowIndex("FTreeEntity");
			string dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<string>(repData, "ReplacePolicy", null);
			string dynamicObjectItemValue2 = DataEntityExtend.GetDynamicObjectItemValue<string>(repData, "ReplaceType", null);
			long dynamicObjectItemValue3 = DataEntityExtend.GetDynamicObjectItemValue<long>(repData, "ReplaceNo_Id", 0L);
			DynamicObjectCollection dynamicObjectItemValue4 = DataEntityExtend.GetDynamicObjectItemValue<DynamicObjectCollection>(repData, "EntityMainItems", null);
			DynamicObjectCollection dynamicObjectItemValue5 = DataEntityExtend.GetDynamicObjectItemValue<DynamicObjectCollection>(repData, "Entity", null);
			DynamicObjectCollection dynamicObjectCollection = (DynamicObjectCollection)base.View.Model.DataObject["TreeEntity"];
			List<string> mainItemRowIds = (from s in dynamicObjectItemValue4
			select DataEntityExtend.GetDynamicObjectItemValue<string>(s, "BOMRowId", null)).ToList<string>();
			List<string> subsItemRowIds = (from s in dynamicObjectItemValue5
			select DataEntityExtend.GetDynamicObjectItemValue<string>(s, "SubBOMRowId", null)).ToList<string>();
			List<string> mainItemRowIds2 = (from w in this.selBomRows
			where !mainItemRowIds.Contains(DataEntityExtend.GetDynamicObjectItemValue<string>(w, "RowId", null)) && ObjectUtils.IsNullOrEmptyOrWhiteSpace(DataEntityExtend.GetDynamicObjectItemValue<string>(w, "ParentRowId", null))
			select w into s
			select DataEntityExtend.GetDynamicObjectItemValue<string>(s, "RowId", null)).ToList<string>();
			this.RemoveMainRepInfo(mainItemRowIds2);
			List<string> subsItemRowIds2 = (from w in this.selBomRows
			where !subsItemRowIds.Contains(DataEntityExtend.GetDynamicObjectItemValue<string>(w, "RowId", null)) && !ObjectUtils.IsNullOrEmptyOrWhiteSpace(DataEntityExtend.GetDynamicObjectItemValue<string>(w, "ParentRowId", null))
			select w into s
			select DataEntityExtend.GetDynamicObjectItemValue<string>(s, "RowId", null)).ToList<string>();
			this.DelOldSubsItems(subsItemRowIds2);
			this.selBomRows = (from w in dynamicObjectCollection
			where mainItemRowIds.Contains(DataEntityExtend.GetDynamicObjectItemValue<string>(w, "RowId", null)) || subsItemRowIds.Contains(DataEntityExtend.GetDynamicObjectItemValue<string>(w, "RowId", null))
			select w).ToList<DynamicObject>();
			List<string> backSubsRowIds = (from w in this.selBomRows
			where subsItemRowIds.Contains(DataEntityExtend.GetDynamicObjectItemValue<string>(w, "RowId", null)) && !ObjectUtils.IsNullOrEmptyOrWhiteSpace(DataEntityExtend.GetDynamicObjectItemValue<string>(w, "ParentRowId", null))
			select w into s
			select DataEntityExtend.GetDynamicObjectItemValue<string>(s, "RowId", null)).ToList<string>();
			List<DynamicObject> backSubsItems = (from w in dynamicObjectItemValue5
			where backSubsRowIds.Contains(DataEntityExtend.GetDynamicObjectItemValue<string>(w, "SubBOMRowId", null))
			select w).ToList<DynamicObject>();
			foreach (DynamicObject dynamicObject in dynamicObjectItemValue4)
			{
				string dynamicObjectItemValue6 = DataEntityExtend.GetDynamicObjectItemValue<string>(dynamicObject, "BOMRowId", null);
				int i = 0;
				while (i < dynamicObjectCollection.Count)
				{
					DynamicObject dynamicObject2 = dynamicObjectCollection[i];
					if (!(DataEntityExtend.GetDynamicObjectItemValue<string>(dynamicObject2, "RowId", null) != dynamicObjectItemValue6))
					{
						bool dynamicObjectItemValue7 = DataEntityExtend.GetDynamicObjectItemValue<bool>(dynamicObject, "IsKeyItem", false);
						base.View.Model.SetValue("FReplacePolicy", dynamicObjectItemValue, i);
						base.View.Model.SetValue("FReplaceType", dynamicObjectItemValue2, i);
						base.View.Model.SetValue("FIskeyItem", dynamicObjectItemValue7, i);
						base.View.Model.SetValue("FSubstitutionId", dynamicObjectItemValue3, i);
						this.SetMainInfo(i, dynamicObject, false);
						if (dynamicObjectItemValue7)
						{
							this.SetBomRowIdGroup(dynamicObjectItemValue4, this.selBomRows);
							List<DynamicObject> addSubsItems = (from w in dynamicObjectItemValue5
							where !backSubsRowIds.Contains(DataEntityExtend.GetDynamicObjectItemValue<string>(w, "SubBOMRowId", null))
							select w).ToList<DynamicObject>();
							this.AddSubsItems(i, dynamicObject2, addSubsItems, null);
							break;
						}
						break;
					}
					else
					{
						i++;
					}
				}
			}
			this.EditSubsItems(backSubsItems, dynamicObjectItemValue4, dynamicObjectCollection.ToList<DynamicObject>());
			this.SetReplaceGroup();
			this.BomChItemsSort();
			base.View.SetEntityFocusRow("FTreeEntity", entryCurrentRowIndex);
			if (ObjectUtils.IsNullOrEmpty(this.isImportObj) || !StringUtils.EqualsIgnoreCase(this.isImportObj.ToString(), "TRUE") || base.Context.ServiceType == null)
			{
				Entity entryEntity = base.View.BillBusinessInfo.GetEntryEntity("FTreeEntity");
				DynamicObjectCollection entityDataObject = base.View.Model.GetEntityDataObject(entryEntity);
				EntryGrid control = base.View.GetControl<EntryGrid>("FTreeEntity");
				foreach (DynamicObject item in entityDataObject)
				{
					int num = entityDataObject.IndexOf(item);
					string value = MFGBillUtil.GetValue<string>(base.View.Model, "FModifiedField", num, null, null);
					if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(value))
					{
						control.SetBackcolor("FReplaceGroup", "#FF0000", num);
						control.SetBackcolor("FModifiedField", "#FF0000", num);
					}
				}
			}
		}

		// Token: 0x060000FA RID: 250 RVA: 0x0000EEE0 File Offset: 0x0000D0E0
		private void EditSubsItems(List<DynamicObject> backSubsItems, IEnumerable<DynamicObject> mainItems, List<DynamicObject> bomChItems)
		{
			if (backSubsItems.Count <= 0)
			{
				return;
			}
			string mainKeyRowId = (from w in mainItems
			where DataEntityExtend.GetDynamicObjectItemValue<bool>(w, "IsKeyItem", false)
			select w into s
			select DataEntityExtend.GetDynamicObjectItemValue<string>(s, "BOMRowId", null)).FirstOrDefault<string>();
			DynamicObject mainKeyItem = (from w in bomChItems
			where DataEntityExtend.GetDynamicObjectItemValue<string>(w, "RowId", null) == mainKeyRowId
			select w).FirstOrDefault<DynamicObject>();
			foreach (DynamicObject dynamicObject in backSubsItems)
			{
				for (int i = 0; i < bomChItems.Count<DynamicObject>(); i++)
				{
					if (!(DataEntityExtend.GetDynamicObjectItemValue<string>(dynamicObject, "SubBOMRowId", null) != DataEntityExtend.GetDynamicObjectItemValue<string>(bomChItems[i], "RowId", null)))
					{
						this.SetSubsInfo(i, mainKeyItem, dynamicObject, null);
						break;
					}
				}
			}
		}

		// Token: 0x060000FB RID: 251 RVA: 0x0000EFE4 File Offset: 0x0000D1E4
		private void DelOldSubsItems(List<string> subsItemRowIds)
		{
			if (subsItemRowIds.Count <= 0)
			{
				return;
			}
			DynamicObjectCollection dynamicObjectCollection = (DynamicObjectCollection)base.View.Model.DataObject["TreeEntity"];
			for (int i = dynamicObjectCollection.Count - 1; i >= 0; i--)
			{
				if (subsItemRowIds.Contains(DataEntityExtend.GetDynamicObjectItemValue<string>(dynamicObjectCollection[i], "RowId", null)))
				{
					base.View.Model.DeleteEntryRow("FTreeEntity", i);
				}
			}
		}

		// Token: 0x060000FC RID: 252 RVA: 0x0000F060 File Offset: 0x0000D260
		private void DelAllSubsItems(List<string> mainItemRowIds)
		{
			DynamicObjectCollection dynamicObjectCollection = (DynamicObjectCollection)base.View.Model.DataObject["TreeEntity"];
			for (int i = dynamicObjectCollection.Count - 1; i >= 0; i--)
			{
				if (mainItemRowIds.Contains(DataEntityExtend.GetDynamicObjectItemValue<string>(dynamicObjectCollection[i], "ParentRowId", null)))
				{
					base.View.Model.DeleteEntryRow("FTreeEntity", i);
				}
			}
		}

		// Token: 0x060000FD RID: 253 RVA: 0x0000F0D0 File Offset: 0x0000D2D0
		private void RemoveMainRepInfo(List<string> mainItemRowIds)
		{
			if (mainItemRowIds.Count <= 0)
			{
				return;
			}
			DynamicObjectCollection dynamicObjectCollection = (DynamicObjectCollection)base.View.Model.DataObject["TreeEntity"];
			for (int i = 0; i < dynamicObjectCollection.Count; i++)
			{
				string dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<string>(dynamicObjectCollection[i], "RowId", null);
				if (mainItemRowIds.Contains(dynamicObjectItemValue))
				{
					this.RemoveRowRep(i);
					if (this.bomRowIdGroup.Keys.Contains(dynamicObjectItemValue))
					{
						this.bomRowIdGroup.Remove(dynamicObjectItemValue);
					}
				}
			}
		}

		// Token: 0x060000FE RID: 254 RVA: 0x0000F16C File Offset: 0x0000D36C
		private void RemoveRowRepInfo(int rowIndex)
		{
			Entity entryEntity = base.View.BillBusinessInfo.GetEntryEntity("FTreeEntity");
			int num = (from s in base.View.Model.GetEntityDataObject(entryEntity)
			select DataEntityExtend.GetDynamicObjectItemValue<int>(s, "ReplaceGroup", 0)).Max();
			base.View.Model.SetValue("FReplaceGroup", num + 1, rowIndex);
			base.View.Model.SetValue("FMATERIALTYPE", 1, rowIndex);
			base.View.Model.SetValue("FParentRowId", string.Empty, rowIndex);
			this.RemoveRowRep(rowIndex);
		}

		// Token: 0x060000FF RID: 255 RVA: 0x0000F224 File Offset: 0x0000D424
		private void RemoveRowRep(int rowIndex)
		{
			base.View.Model.SetValue("FReplacePolicy", string.Empty, rowIndex);
			base.View.Model.SetValue("FReplaceType", string.Empty, rowIndex);
			base.View.Model.SetValue("FReplacePriority", 0, rowIndex);
			base.View.Model.SetValue("FMRPPriority", 0, rowIndex);
			base.View.Model.SetValue("FIskeyItem", false, rowIndex);
			base.View.Model.SetValue("FSubstitutionId", 0, rowIndex);
			base.View.Model.SetValue("FSTEntryId", 0, rowIndex);
			base.View.Model.BeginIniti();
			base.View.Model.SetValue("FNETDEMANDRATE", 0, rowIndex);
			base.View.Model.EndIniti();
			base.View.UpdateView("FNETDEMANDRATE", rowIndex);
			base.View.Model.SetValue("FIsMulCsd", false, rowIndex);
		}

		// Token: 0x06000100 RID: 256 RVA: 0x0000F35C File Offset: 0x0000D55C
		private void AddSubsItems(int mainKeyItemIndex, DynamicObject bomChItem, List<DynamicObject> addSubsItems, DynamicObject mainItem = null)
		{
			foreach (DynamicObject subsItem in addSubsItems)
			{
				int entryRowCount = base.View.Model.GetEntryRowCount("FTreeEntity");
				mainKeyItemIndex++;
				if (mainKeyItemIndex == entryRowCount)
				{
					base.View.Model.CreateNewEntryRow("FTreeEntity");
				}
				else
				{
					base.View.Model.InsertEntryRow("FTreeEntity", mainKeyItemIndex);
				}
				this.SetSubsInfo(mainKeyItemIndex, bomChItem, subsItem, mainItem);
			}
		}

		// Token: 0x06000101 RID: 257 RVA: 0x0000F3FC File Offset: 0x0000D5FC
		private void SetMainInfo(int i, DynamicObject mainItem, bool isGetRep = false)
		{
			base.View.Model.SetValue("FReplacePriority", DataEntityExtend.GetDynamicObjectItemValue<int>(mainItem, "MainPriority", 0), i);
			base.View.Model.SetValue("FMRPPriority", DataEntityExtend.GetDynamicObjectItemValue<int>(mainItem, "MainPriority", 0), i);
			base.View.Model.SetValue("FSTEntryId", DataEntityExtend.GetDynamicObjectItemValue<int>(mainItem, "Id", 0), i);
			base.View.Model.BeginIniti();
			base.View.Model.SetValue("FNETDEMANDRATE", DataEntityExtend.GetDynamicValue<decimal>(mainItem, "NETDEMANDRATE", 0m), i);
			base.View.Model.EndIniti();
			base.View.UpdateView("FNETDEMANDRATE", i);
			if (!isGetRep)
			{
				base.View.Model.SetValue("FAuxPropId", DataEntityExtend.GetDynamicObjectItemValue<long>(mainItem, "AuxPropID_Id", 0L), i);
				base.View.Model.SetValue("FAuxPropId", DataEntityExtend.GetDynamicObjectItemValue<DynamicObject>(mainItem, "AuxPropID", null), i);
				base.View.UpdateView("FAuxPropId", i);
			}
			base.View.Model.SetValue("FChildSupplyOrgId", DataEntityExtend.GetDynamicObjectItemValue<long>(mainItem, "MainSupplyOrgId_Id", 0L), i);
			base.View.Model.SetValue("FBOMID", DataEntityExtend.GetDynamicObjectItemValue<long>(mainItem, "BomId_Id", 0L), i);
			if (!isGetRep)
			{
				base.View.Model.SetValue("FNUMERATOR", DataEntityExtend.GetDynamicObjectItemValue<decimal>(mainItem, "Numerator", 0m), i);
				base.View.Model.SetValue("FDENOMINATOR", DataEntityExtend.GetDynamicObjectItemValue<decimal>(mainItem, "Denominator", 0m), i);
			}
		}

		// Token: 0x06000102 RID: 258 RVA: 0x0000F5E4 File Offset: 0x0000D7E4
		private void SetSubsInfo(int mainKeyItemIndex, DynamicObject mainKeyItem, DynamicObject subsItem, DynamicObject mainItem = null)
		{
			base.View.Model.SetValue("FParentRowId", DataEntityExtend.GetDynamicObjectItemValue<string>(mainKeyItem, "RowId", null), mainKeyItemIndex);
			base.View.Model.SetValue("FReplacePolicy", DataEntityExtend.GetDynamicObjectItemValue<string>(mainKeyItem, "ReplacePolicy", null), mainKeyItemIndex);
			base.View.Model.SetValue("FReplaceType", DataEntityExtend.GetDynamicObjectItemValue<string>(mainKeyItem, "ReplaceType", null), mainKeyItemIndex);
			base.View.Model.SetValue("FSubstitutionId", DataEntityExtend.GetDynamicObjectItemValue<string>(mainKeyItem, "SubstitutionId_Id", null), mainKeyItemIndex);
			base.View.Model.SetValue("FOptQueue", DataEntityExtend.GetDynamicObjectItemValue<string>(mainKeyItem, "OptQueue", null), mainKeyItemIndex);
			base.View.Model.SetValue("FOPERID", DataEntityExtend.GetDynamicObjectItemValue<long>(mainKeyItem, "OPERID", 0L), mainKeyItemIndex);
			base.View.Model.SetValue("FPROCESSID", DataEntityExtend.GetDynamicObjectItemValue<long>(mainKeyItem, "PROCESSID_Id", 0L), mainKeyItemIndex);
			base.View.Model.SetValue("FPOSITIONNO", DataEntityExtend.GetDynamicObjectItemValue<string>(mainKeyItem, "POSITIONNO", null), mainKeyItemIndex);
			base.View.Model.SetValue("FDOSAGETYPE", "2", mainKeyItemIndex);
			base.View.Model.SetValue("FSupplyMode", DataEntityExtend.GetDynamicValue<string>(mainKeyItem, "SupplyMode", null), mainKeyItemIndex);
			base.View.Model.SetValue("FMATERIALTYPE", 3, mainKeyItemIndex);
			this.bIsReplaceDataChange = true;
			base.View.Model.SetValue("FMATERIALIDCHILD", DataEntityExtend.GetDynamicObjectItemValue<long>(subsItem, "SubMaterialID_Id", 0L), mainKeyItemIndex);
			base.View.Model.SetValue("FIskeyItem", DataEntityExtend.GetDynamicObjectItemValue<bool>(subsItem, "SubIsKeyItem", false), mainKeyItemIndex);
			base.View.Model.SetValue("FReplacePriority", DataEntityExtend.GetDynamicObjectItemValue<int>(subsItem, "Priority", 0), mainKeyItemIndex);
			base.View.Model.SetValue("FMRPPriority", DataEntityExtend.GetDynamicObjectItemValue<int>(subsItem, "Priority", 0), mainKeyItemIndex);
			base.View.Model.SetValue("FEFFECTDATE", DataEntityExtend.GetDynamicObjectItemValue<DateTime>(subsItem, "EffectDate", default(DateTime)), mainKeyItemIndex);
			base.View.Model.SetValue("FEXPIREDATE", DataEntityExtend.GetDynamicObjectItemValue<DateTime>(subsItem, "ExpireDate", default(DateTime)), mainKeyItemIndex);
			base.View.Model.SetValue("FMEMO", DataEntityExtend.GetDynamicObjectItemValue<LocaleValue>(subsItem, "MEMO", null), mainKeyItemIndex);
			base.View.Model.SetValue("FSTEntryId", DataEntityExtend.GetDynamicObjectItemValue<int>(subsItem, "Id", 0), mainKeyItemIndex);
			base.View.Model.BeginIniti();
			base.View.Model.SetValue("FNETDEMANDRATE", DataEntityExtend.GetDynamicValue<decimal>(subsItem, "SUBNETDEMANDRATE", 0m), mainKeyItemIndex);
			base.View.Model.EndIniti();
			base.View.UpdateView("FNETDEMANDRATE", mainKeyItemIndex);
			base.View.Model.SetValue("FAuxPropId", DataEntityExtend.GetDynamicObjectItemValue<long>(subsItem, "SubAuxPropID_Id", 0L), mainKeyItemIndex);
			base.View.Model.SetValue("FAuxPropId", DataEntityExtend.GetDynamicObjectItemValue<DynamicObject>(subsItem, "SubAuxPropID", null), mainKeyItemIndex);
			base.View.UpdateView("FAuxPropId", mainKeyItemIndex);
			base.View.Model.SetValue("FChildSupplyOrgId", DataEntityExtend.GetDynamicObjectItemValue<long>(subsItem, "SubSupplyOrgId_Id", 0L), mainKeyItemIndex);
			base.View.Model.SetValue("FBOMID", DataEntityExtend.GetDynamicObjectItemValue<long>(subsItem, "SubBomId_Id", 0L), mainKeyItemIndex);
			base.View.Model.SetValue("FChildBaseUnitID", DataEntityExtend.GetDynamicObjectItemValue<long>(subsItem, "SubBaseUnitID_Id", 0L), mainKeyItemIndex);
			base.View.Model.SetValue("FCHILDUNITID", DataEntityExtend.GetDynamicObjectItemValue<long>(subsItem, "SubUnitID_Id", 0L), mainKeyItemIndex);
			base.View.Model.SetValue("FNUMERATOR", DataEntityExtend.GetDynamicObjectItemValue<decimal>(subsItem, "SubNumerator", 0m), mainKeyItemIndex);
			base.View.Model.SetValue("FDENOMINATOR", DataEntityExtend.GetDynamicObjectItemValue<decimal>(subsItem, "SubDenominator", 0m), mainKeyItemIndex);
			base.View.Model.SetValue("FOWNERTYPEID", DataEntityExtend.GetDynamicObjectItemValue<string>(mainKeyItem, "OWNERTYPEID", null), mainKeyItemIndex);
			base.View.Model.SetValue("FOWNERID", DataEntityExtend.GetDynamicObjectItemValue<long>(mainKeyItem, "OWNERID_Id", 0L), mainKeyItemIndex);
			long dynamicValue = DataEntityExtend.GetDynamicValue<long>(mainKeyItem, "SUPPLYORG_Id", 0L);
			if (dynamicValue != 0L)
			{
				base.View.Model.SetValue("FSUPPLYORG", dynamicValue, mainKeyItemIndex);
				base.View.Model.SetValue("FSTOCKID", DataEntityExtend.GetDynamicValue<long>(mainKeyItem, "STOCKID_Id", 0L), mainKeyItemIndex);
				base.View.Model.SetValue("FSTOCKLOCID", DataEntityExtend.GetDynamicValue<long>(mainKeyItem, "STOCKLOCID_Id", 0L), mainKeyItemIndex);
			}
			long value = MFGBillUtil.GetValue<long>(this.Model, "FUseOrgId", -1, 0L, null);
			int systemProfile = MFGServiceHelper.GetSystemProfile<int>(base.Context, value, "MFG_EngParameter", "SubItemScrapDependOn", 2);
			if (systemProfile == 1)
			{
				base.View.Model.SetValue("FSCRAPRATE", DataEntityExtend.GetDynamicObjectItemValue<decimal>(mainKeyItem, "FSCRAPRATE", 0m), mainKeyItemIndex);
				base.View.Model.SetValue("FFIXSCRAPQTY", DataEntityExtend.GetDynamicObjectItemValue<decimal>(mainKeyItem, "FIXSCRAPQTY", 0m), mainKeyItemIndex);
				base.View.Model.SetValue("FISGETSCRAP", DataEntityExtend.GetDynamicObjectItemValue<bool>(mainKeyItem, "FISGETSCRAP", false), mainKeyItemIndex);
			}
			if (!ObjectUtils.IsNullOrEmpty(mainItem))
			{
				decimal dynamicValue2 = DataEntityExtend.GetDynamicValue<decimal>(subsItem, "SubBaseNumerator", 0m);
				decimal dynamicValue3 = DataEntityExtend.GetDynamicValue<decimal>(mainKeyItem, "BaseNumerator", 0m);
				decimal dynamicValue4 = DataEntityExtend.GetDynamicValue<decimal>(mainItem, "BaseNumerator", 0m);
				decimal dynamicValue5 = DataEntityExtend.GetDynamicValue<decimal>(subsItem, "SubNumerator", 0m);
				decimal dynamicValue6 = DataEntityExtend.GetDynamicValue<decimal>(mainItem, "NUMERATOR", 0m);
				decimal dynamicValue7 = DataEntityExtend.GetDynamicValue<decimal>(subsItem, "SubDenominator", 0m);
				decimal dynamicValue8 = DataEntityExtend.GetDynamicValue<decimal>(mainKeyItem, "BaseDenominator", 0m);
				decimal dynamicValue9 = DataEntityExtend.GetDynamicValue<decimal>(mainItem, "Denominator", 0m);
				decimal num = dynamicValue2 * dynamicValue3 / dynamicValue4;
				decimal num2 = dynamicValue7 * dynamicValue8 / dynamicValue9;
				DynamicObject dynamicValue10 = DataEntityExtend.GetDynamicValue<DynamicObject>(subsItem, "SubMaterialID", null);
				DynamicObject dynamicValue11 = DataEntityExtend.GetDynamicValue<DynamicObject>((DynamicObject)mainKeyItem.Parent, "MATERIALID", null);
				long value2 = Convert.ToInt64(num);
				long value3 = Convert.ToInt64(num2);
				bool flag = value2 == num && value3 == num2;
				dynamicValue5 * dynamicValue9 != dynamicValue6 * dynamicValue7;
				bool systemProfile2 = MFGServiceHelper.GetSystemProfile<bool>(base.View.Context, value, "MFG_EngParameter", "SubMtrlAutoNotReFraction", false);
				if (flag && !systemProfile2)
				{
					PRDCommonServiceHelper.GetPureDecimalGcb(base.Context, dynamicValue2 * dynamicValue3 / dynamicValue4, dynamicValue7 * dynamicValue8 / dynamicValue9, ref num, ref num2);
				}
				GetUnitConvertRateArgs getUnitConvertRateArgs = new GetUnitConvertRateArgs
				{
					PrimaryKey = DataEntityExtend.GetDynamicValue<long>(subsItem, "Id", 0L),
					MasterId = DataEntityExtend.GetDynamicValue<long>(dynamicValue10, "MsterId", 0L),
					SourceUnitId = DataEntityExtend.GetDynamicValue<long>(subsItem, "SubBaseUnitID_Id", 0L),
					DestUnitId = DataEntityExtend.GetDynamicValue<long>(subsItem, "SubUnitID_Id", 0L)
				};
				GetUnitConvertRateArgs getUnitConvertRateArgs2 = new GetUnitConvertRateArgs
				{
					PrimaryKey = DataEntityExtend.GetDynamicValue<long>(mainKeyItem, "Id", 0L),
					MasterId = DataEntityExtend.GetDynamicValue<long>(dynamicValue11, "MsterId", 0L),
					SourceUnitId = DataEntityExtend.GetDynamicValue<long>((DynamicObject)mainKeyItem.Parent, "BaseUnitId_Id", 0L),
					DestUnitId = DataEntityExtend.GetDynamicValue<long>((DynamicObject)mainKeyItem.Parent, "FUNITID_Id", 0L)
				};
				UnitConvert unitConvertRate = UnitConvertServiceHelper.GetUnitConvertRate(base.Context, getUnitConvertRateArgs);
				UnitConvert unitConvertRate2 = UnitConvertServiceHelper.GetUnitConvertRate(base.Context, getUnitConvertRateArgs2);
				base.View.Model.SetValue("FNUMERATOR", unitConvertRate.ConvertQty(num, ""), mainKeyItemIndex);
				base.View.Model.SetValue("FDENOMINATOR", unitConvertRate2.ConvertQty(num2, ""), mainKeyItemIndex);
			}
		}

		// Token: 0x06000103 RID: 259 RVA: 0x0000FED8 File Offset: 0x0000E0D8
		private void InitBomRowIdGroup(bool isForce = false)
		{
			if (!isForce && this.bomRowIdGroup.Count > 0)
			{
				return;
			}
			Entity entryEntity = base.View.BillBusinessInfo.GetEntryEntity("FTreeEntity");
			IEnumerable<DynamicObject> enumerable = (from w in base.View.Model.GetEntityDataObject(entryEntity)
			where ObjectUtils.IsNullOrEmptyOrWhiteSpace(DataEntityExtend.GetDynamicObjectItemValue<string>(w, "ParentRowId", null)) && !string.IsNullOrWhiteSpace(DataEntityExtend.GetDynamicObjectItemValue<string>(w, "ReplacePolicy", null))
			select w).ToList<DynamicObject>();
			foreach (DynamicObject dynamicObject in enumerable)
			{
				string dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<string>(dynamicObject, "RowId", null);
				if (this.bomRowIdGroup.Keys.Contains(dynamicObjectItemValue))
				{
					this.bomRowIdGroup.Remove(dynamicObjectItemValue);
				}
				this.bomRowIdGroup.Add(dynamicObjectItemValue, DataEntityExtend.GetDynamicObjectItemValue<int>(dynamicObject, "ReplaceGroup", 0));
			}
		}

		// Token: 0x06000104 RID: 260 RVA: 0x00010064 File Offset: 0x0000E264
		private void SetBomRowIdGroup(IEnumerable<DynamicObject> mainItems, IEnumerable<DynamicObject> selBomRows)
		{
			BOMEdit.<>c__DisplayClassb9 CS$<>8__locals1 = new BOMEdit.<>c__DisplayClassb9();
			CS$<>8__locals1.<>4__this = this;
			Entity entryEntity = base.View.BillBusinessInfo.GetEntryEntity("FTreeEntity");
			CS$<>8__locals1.bomChItems = base.View.Model.GetEntityDataObject(entryEntity).ToList<DynamicObject>();
			int i;
			for (i = 0; i < CS$<>8__locals1.bomChItems.Count; i++)
			{
				if (selBomRows.Any((DynamicObject w) => DataEntityExtend.GetDynamicObjectItemValue<string>(w, "RowId", null) == DataEntityExtend.GetDynamicObjectItemValue<string>(CS$<>8__locals1.bomChItems[i], "RowId", null)))
				{
					this.ResetRepGroup(i);
				}
			}
			CS$<>8__locals1.replaceGroup = 1;
			if (this.bomRowIdGroup.Count > 0)
			{
				CS$<>8__locals1.replaceGroup = this.bomRowIdGroup.Values.Max() + 1;
			}
			mainItems.ToList<DynamicObject>().ForEach(delegate(DynamicObject e)
			{
				string dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<string>(e, "BOMRowId", null);
				if (CS$<>8__locals1.<>4__this.bomRowIdGroup.Keys.Contains(dynamicObjectItemValue))
				{
					CS$<>8__locals1.<>4__this.bomRowIdGroup.Remove(dynamicObjectItemValue);
				}
				CS$<>8__locals1.<>4__this.bomRowIdGroup.Add(dynamicObjectItemValue, CS$<>8__locals1.replaceGroup);
			});
		}

		// Token: 0x06000105 RID: 261 RVA: 0x00010154 File Offset: 0x0000E354
		private void ResetRepGroup(int rowIndex)
		{
			string value = MFGBillUtil.GetValue<string>(base.View.Model, "FRowId", rowIndex, null, null);
			if (this.bomRowIdGroup.Keys.Contains(value))
			{
				this.bomRowIdGroup.Remove(value);
			}
		}

		// Token: 0x06000106 RID: 262 RVA: 0x00010210 File Offset: 0x0000E410
		private void SetReplaceGroup()
		{
			if (base.Context.ServiceType == 1)
			{
				return;
			}
			this.InitBomRowIdGroup(false);
			Entity entryEntity = base.View.BillBusinessInfo.GetEntryEntity("FTreeEntity");
			DynamicObjectCollection entityDataObject = base.View.Model.GetEntityDataObject(entryEntity);
			IEnumerable<DynamicObject> enumerable = (from w in entityDataObject
			where DataEntityExtend.GetDynamicObjectItemValue<long>(w, "MATERIALIDCHILD_Id", 0L) <= 0L
			select w).ToList<DynamicObject>();
			foreach (DynamicObject dynamicObject in enumerable)
			{
				DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "ReplaceGroup", 0);
			}
			int num = 1;
			int num2 = 0;
			Dictionary<int, int> dictionary = new Dictionary<int, int>();
			IEnumerable<DynamicObject> enumerable2 = (from w in entityDataObject
			where DataEntityExtend.GetDynamicObjectItemValue<long>(w, "MATERIALIDCHILD_Id", 0L) > 0L && ObjectUtils.IsNullOrEmptyOrWhiteSpace(DataEntityExtend.GetDynamicObjectItemValue<string>(w, "ParentRowId", null))
			select w).ToList<DynamicObject>();
			base.View.Model.BeginIniti();
			foreach (DynamicObject dynamicObject2 in enumerable2)
			{
				string dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<string>(dynamicObject2, "RowId", null);
				if (!this.bomRowIdGroup.Keys.Contains(dynamicObjectItemValue))
				{
					DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "ReplaceGroup", num++);
				}
				else
				{
					if (num2 != this.bomRowIdGroup[dynamicObjectItemValue] && !dictionary.Keys.Contains(this.bomRowIdGroup[dynamicObjectItemValue]))
					{
						int value = num++;
						num2 = this.bomRowIdGroup[dynamicObjectItemValue];
						dictionary.Add(this.bomRowIdGroup[dynamicObjectItemValue], value);
					}
					DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "ReplaceGroup", dictionary[this.bomRowIdGroup[dynamicObjectItemValue]]);
				}
			}
			IEnumerable<DynamicObject> enumerable3 = (from w in entityDataObject
			where !ObjectUtils.IsNullOrEmptyOrWhiteSpace(DataEntityExtend.GetDynamicObjectItemValue<string>(w, "ParentRowId", null))
			select w).ToList<DynamicObject>();
			Dictionary<string, IGrouping<string, DynamicObject>> dictionary2 = (from g in enumerable2
			group g by DataEntityExtend.GetDynamicObjectItemValue<string>(g, "RowId", null)).ToDictionary((IGrouping<string, DynamicObject> d) => d.Key);
			foreach (DynamicObject dynamicObject3 in enumerable3)
			{
				IGrouping<string, DynamicObject> source;
				if (dictionary2.TryGetValue(DataEntityExtend.GetDynamicObjectItemValue<string>(dynamicObject3, "ParentRowId", null), out source))
				{
					int num3 = (from s in source
					select DataEntityExtend.GetDynamicObjectItemValue<int>(s, "ReplaceGroup", 0)).FirstOrDefault<int>();
					DataEntityExtend.SetDynamicObjectItemValue(dynamicObject3, "ReplaceGroup", num3);
				}
			}
			base.View.Model.EndIniti();
			WebType serviceType = base.Context.ServiceType;
		}

		// Token: 0x06000107 RID: 263 RVA: 0x0001057C File Offset: 0x0000E77C
		private void BomChItemsSort()
		{
			base.View.Model.BeginIniti();
			EntryEntity entryEntity = this.Model.BillBusinessInfo.GetEntryEntity("FTreeEntity");
			if (entryEntity == null)
			{
				return;
			}
			List<DynamicObject> list = (from w in base.View.Model.GetEntityDataObject(entryEntity)
			where DataEntityExtend.GetDynamicObjectItemValue<long>(w, "MATERIALIDCHILD_Id", 0L) > 0L
			select w).ToList<DynamicObject>();
			List<DynamicObject> list2 = (from q in list
			orderby DataEntityExtend.GetDynamicObjectItemValue<int>(q, "ReplaceGroup", 0), DataEntityExtend.GetDynamicObjectItemValue<int>(q, "MATERIALTYPE", 0), DataEntityExtend.GetDynamicObjectItemValue<int>(q, "ReplacePriority", 0), DataEntityExtend.GetDynamicObjectItemValue<int>(q, "IskeyItem", 0) descending
			select q).ToList<DynamicObject>();
			int num = 1;
			foreach (DynamicObject dynamicObject in list2)
			{
				string dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<string>(dynamicObject, "RowId", null);
				foreach (DynamicObject dynamicObject2 in list)
				{
					if (DataEntityExtend.GetDynamicObjectItemValue<string>(dynamicObject2, "RowId", null) == dynamicObjectItemValue)
					{
						DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "Seq", num);
						break;
					}
				}
				num++;
			}
			base.View.Model.EndIniti();
			base.View.UpdateView("FTreeEntity");
		}

		// Token: 0x06000108 RID: 264 RVA: 0x00010754 File Offset: 0x0000E954
		private bool ValidateRemoveRep()
		{
			EntryEntity entryEntity = base.View.BusinessInfo.GetEntryEntity("FTreeEntity");
			DynamicObjectCollection entityDataObject = base.View.Model.GetEntityDataObject(entryEntity);
			List<int> list = (from o in base.View.GetControl<EntryGrid>(entryEntity.Key).GetSelectedRows()
			orderby o
			select o).ToList<int>();
			if (list.Count <= 0 || list.FirstOrDefault<int>() < 0)
			{
				return false;
			}
			List<int> list2 = new List<int>();
			int count = entityDataObject.Count;
			foreach (int num in list)
			{
				if (num >= count)
				{
					break;
				}
				if (!string.IsNullOrWhiteSpace(DataEntityExtend.GetDynamicObjectItemValue<string>(entityDataObject[num], "ReplacePolicy", null)))
				{
					list2.Add(DataEntityExtend.GetDynamicObjectItemValue<int>(entityDataObject[num], "Seq", 0));
				}
			}
			if (list2.Count > 0)
			{
				base.View.ShowErrMessage(string.Format(ResManager.LoadKDString("第{0}行分录存在替代关系，不能直接删除，请先删除替代关系。", "015072000002175", 7, new object[0]), string.Join<int>(",", list2)), "", 0);
				return false;
			}
			return true;
		}

		// Token: 0x06000109 RID: 265 RVA: 0x000108A4 File Offset: 0x0000EAA4
		private bool IsCanBCTypeUpdate(BeforeUpdateValueEventArgs e)
		{
			bool result = true;
			if (this.canUpdate)
			{
				return result;
			}
			if (base.Context.ServiceType == 1)
			{
				return result;
			}
			if (!string.IsNullOrWhiteSpace(MFGBillUtil.GetValue<string>(base.View.Model, "FReplacePolicy", e.Row, null, null)))
			{
				return result;
			}
			string value = MFGBillUtil.GetValue<string>(base.View.Model, "FMaterialType", e.Row, string.Empty, "FEntity");
			if (ObjectUtils.IsNullOrEmptyOrWhiteSpace(value))
			{
				return result;
			}
			int num = int.Parse(value);
			int num2 = 0;
			int.TryParse(Convert.ToString(e.Value), out num2);
			if ((num == 1 || num == 2) && num2 == 3)
			{
				base.View.ShowErrMessage(ResManager.LoadKDString("标准件或返还件不允许修改为替代件！", "015072000002176", 7, new object[0]), "", 0);
				base.View.Model.SetValue("FMATERIALTYPE", num, e.Row);
				result = false;
			}
			return result;
		}

		// Token: 0x0600010A RID: 266 RVA: 0x00010998 File Offset: 0x0000EB98
		private bool IsCanUpdateMaterial(BeforeUpdateValueEventArgs e)
		{
			bool result = true;
			if (this.canUpdate)
			{
				return result;
			}
			if (base.Context.ServiceType == 1)
			{
				return result;
			}
			string value = MFGBillUtil.GetValue<string>(base.View.Model, "FReplacePolicy", e.Row, null, null);
			if (string.IsNullOrWhiteSpace(value) || ObjectUtils.IsNullOrEmpty(e.Value))
			{
				return result;
			}
			long value2 = MFGBillUtil.GetValue<long>(base.View.Model, "FMATERIALIDCHILD", e.Row, 0L, null);
			DynamicObject dynamicObject = e.Value as DynamicObject;
			if (ObjectUtils.IsNullOrEmpty(dynamicObject))
			{
				return result;
			}
			int dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<int>(DataEntityExtend.GetDynamicObjectItemValue<DynamicObjectCollection>(dynamicObject, "MaterialBase", null).FirstOrDefault<DynamicObject>(), "ErpClsID", 0);
			if (dynamicObjectItemValue == 5)
			{
				result = false;
				base.View.ShowErrMessage(ResManager.LoadKDString("存在替代设置不允许修改为虚拟件！", "015072000017269", 7, new object[0]), "", 0);
				base.View.Model.SetValue("FMATERIALIDCHILD", value2, e.Row);
			}
			return result;
		}

		// Token: 0x0600010B RID: 267 RVA: 0x00010B20 File Offset: 0x0000ED20
		private void GetSubstitute(bool isSaveOperation = false)
		{
			if (!this.ValidatePermission(isSaveOperation))
			{
				return;
			}
			EntryEntity entryEntity = base.View.BusinessInfo.GetEntryEntity("FTreeEntity");
			List<DynamicObject> source = new List<DynamicObject>();
			source = (isSaveOperation ? base.View.Model.GetEntityDataObject(entryEntity).ToList<DynamicObject>() : SysBclExtend.GetSelectedRowDatas(base.View, entryEntity.Key).ToList<DynamicObject>());
			List<DynamicObject> list = (from w in source
			where DataEntityExtend.GetDynamicValue<long>(w, "MATERIALIDCHILD_Id", 0L) > 0L && DataEntityExtend.GetDynamicValue<int>(w, "MATERIALTYPE", 0) == 1 && ObjectUtils.IsNullOrEmptyOrWhiteSpace(DataEntityExtend.GetDynamicValue<string>(w, "ReplacePolicy", null)) && !DataEntityExtend.GetDynamicValue<bool>(w, "IsSkip", false)
			select w).ToList<DynamicObject>();
			if (ListUtils.IsEmpty<DynamicObject>(list))
			{
				return;
			}
			if (!isSaveOperation)
			{
				if (!list.Any((DynamicObject p) => DataEntityExtend.GetDynamicValue<string>(p, "EntrySource", null) == "1") && !this.ValidateIsInCreateOrgId())
				{
					return;
				}
			}
			List<DynamicObject> list2 = (from w in list
			where DataEntityExtend.GetDynamicValue<string>(w, "EntrySource", null) == "1"
			select w).ToList<DynamicObject>();
			if (ListUtils.IsEmpty<DynamicObject>(list2))
			{
				return;
			}
			this.BindRepMainDataForGetSubs(list2, false);
		}

		// Token: 0x0600010C RID: 268 RVA: 0x00010CC0 File Offset: 0x0000EEC0
		private void ReGetSubstitute()
		{
			if (!this.ValidatePermission(false))
			{
				return;
			}
			EntryEntity entryEntity = base.View.BusinessInfo.GetEntryEntity("FTreeEntity");
			DynamicObjectCollection entityDataObject = base.View.Model.GetEntityDataObject(entryEntity);
			if (ListUtils.IsEmpty<DynamicObject>(entityDataObject))
			{
				return;
			}
			List<DynamicObject> list = (from w in entityDataObject
			where DataEntityExtend.GetDynamicValue<long>(w, "MATERIALIDCHILD_Id", 0L) > 0L && DataEntityExtend.GetDynamicValue<int>(w, "MATERIALTYPE", 0) == 1 && !DataEntityExtend.GetDynamicValue<bool>(w, "IsSkip", false)
			select w).ToList<DynamicObject>();
			if (ListUtils.IsEmpty<DynamicObject>(list))
			{
				return;
			}
			if (!list.Any((DynamicObject p) => DataEntityExtend.GetDynamicValue<string>(p, "EntrySource", null) == "1") && !this.ValidateIsInCreateOrgId())
			{
				return;
			}
			List<DynamicObject> list2 = (from w in list
			where DataEntityExtend.GetDynamicValue<string>(w, "EntrySource", null) == "1"
			select w).ToList<DynamicObject>();
			if (ListUtils.IsEmpty<DynamicObject>(list2))
			{
				return;
			}
			list2 = (from i in (from i in list2
			group i by DataEntityExtend.GetDynamicValue<long>(i, "ReplaceGroup", 0L)).ToDictionary((IGrouping<long, DynamicObject> i) => i.Key, (IGrouping<long, DynamicObject> v) => v.ToList<DynamicObject>())
			where i.Value.Count<DynamicObject>() == 1
			select i).SelectMany((KeyValuePair<long, List<DynamicObject>> i) => i.Value).ToList<DynamicObject>();
			if (ListUtils.IsEmpty<DynamicObject>(list2))
			{
				return;
			}
			this.BindRepMainDataForGetSubs(list2, true);
		}

		// Token: 0x0600010D RID: 269 RVA: 0x00010E58 File Offset: 0x0000F058
		private void GetSubstitueWhenSaving()
		{
			string value = MFGBillUtil.GetValue<string>(base.View.Model, "FDocumentStatus", -1, null, null);
			if (value == "Z")
			{
				FormMetadata formMetadata = (FormMetadata)MetaDataServiceHelper.Load(base.View.Context, "ENG_BOM_BILLPARAM", true);
				DynamicObject dynamicObject = UserParamterServiceHelper.Load(base.View.Context, formMetadata.BusinessInfo, base.View.Context.UserId, "ENG_BOM", "UserParameter");
				if (!DataEntityExtend.GetDynamicValue<bool>(dynamicObject, "AutoGetRepWhenSaving", false))
				{
					return;
				}
				if (this.IsHasSubstitue())
				{
					return;
				}
				if (this.IsFeatureMaterial())
				{
					return;
				}
				this.GetSubstitute(true);
			}
		}

		// Token: 0x0600010E RID: 270 RVA: 0x00010F18 File Offset: 0x0000F118
		private bool IsHasSubstitue()
		{
			EntryEntity entryEntity = base.View.BusinessInfo.GetEntryEntity("FTreeEntity");
			DynamicObjectCollection entityDataObject = base.View.Model.GetEntityDataObject(entryEntity);
			return entityDataObject.Any((DynamicObject p) => !ObjectUtils.IsNullOrEmptyOrWhiteSpace(DataEntityExtend.GetDynamicValue<string>(p, "ReplacePolicy", null)));
		}

		// Token: 0x0600010F RID: 271 RVA: 0x00010F70 File Offset: 0x0000F170
		private IEnumerable<DynamicObject> GetSubstitueScheme(IEnumerable<DynamicObject> bomEntryDataLst)
		{
			return SubStituteViewServiceHelper.GetSubstitueSchemeData(base.Context, bomEntryDataLst);
		}

		// Token: 0x06000110 RID: 272 RVA: 0x00010FB4 File Offset: 0x0000F1B4
		private void DeleteAllRep(IEnumerable<DynamicObject> treeEntityDataObject)
		{
			List<string> list = new List<string>();
			List<IGrouping<int, DynamicObject>> list2 = (from w in treeEntityDataObject
			where DataEntityExtend.GetDynamicValue<int>(w, "MATERIALTYPE", 0) == 1 && !ObjectUtils.IsNullOrEmptyOrWhiteSpace(DataEntityExtend.GetDynamicValue<string>(w, "ReplacePolicy", null))
			select w into p
			group p by DataEntityExtend.GetDynamicValue<int>(p, "ReplaceGroup", 0)).ToList<IGrouping<int, DynamicObject>>();
			foreach (IGrouping<int, DynamicObject> source in list2)
			{
				if (source.Count<DynamicObject>() == 1)
				{
					list.Add(DataEntityExtend.GetDynamicValue<string>(source.First<DynamicObject>(), "RowId", null));
				}
			}
			if (!ListUtils.IsEmpty<string>(list))
			{
				this.DelAllSubsItems(list);
				this.RemoveMainRepInfo(list);
				this.SetReplaceGroup();
			}
		}

		// Token: 0x06000111 RID: 273 RVA: 0x00011124 File Offset: 0x0000F324
		private void BindRepMainDataForGetSubs(IEnumerable<DynamicObject> bomEntryDataLst, bool IsReGetSubstitute)
		{
			string currentRowGuid = string.Empty;
			if (IsReGetSubstitute)
			{
				IEnumerable<DynamicObject> selectedRowDatas = SysBclExtend.GetSelectedRowDatas(base.View, "FTreeEntity");
				if (!ListUtils.IsEmpty<DynamicObject>(selectedRowDatas))
				{
					currentRowGuid = DataEntityExtend.GetDynamicValue<string>(selectedRowDatas.FirstOrDefault<DynamicObject>(), "RowId", null);
				}
				else
				{
					currentRowGuid = DataEntityExtend.GetDynamicValue<string>(bomEntryDataLst.FirstOrDefault<DynamicObject>(), "RowId", null);
				}
			}
			else
			{
				currentRowGuid = DataEntityExtend.GetDynamicValue<string>(bomEntryDataLst.FirstOrDefault<DynamicObject>(), "RowId", null);
			}
			IEnumerable<DynamicObject> substitueScheme = this.GetSubstitueScheme(bomEntryDataLst);
			if (ListUtils.IsEmpty<DynamicObject>(substitueScheme))
			{
				return;
			}
			List<DynamicObject> substitueSchemeDatas = SubStituteViewServiceHelper.GetSubstitueSchemeDatas(base.Context, bomEntryDataLst);
			foreach (DynamicObject dynamicObject in substitueScheme)
			{
				string dynamicValue = DataEntityExtend.GetDynamicValue<string>(dynamicObject, "ReplacePolicy", null);
				string dynamicValue2 = DataEntityExtend.GetDynamicValue<string>(dynamicObject, "ReplaceType", null);
				long num = (DataEntityExtend.GetDynamicValue<long>(dynamicObject, "ReplaceNo_Id", 0L) == 0L) ? DataEntityExtend.GetDynamicValue<long>(dynamicObject, "Id", 0L) : DataEntityExtend.GetDynamicValue<long>(dynamicObject, "ReplaceNo_Id", 0L);
				DynamicObjectCollection dynamicValue3 = DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(dynamicObject, "EntityMainItems", null);
				DynamicObject dynamicObject2 = dynamicValue3.FirstOrDefault<DynamicObject>();
				DynamicObjectCollection dynamicValue4 = DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(dynamicObject, "Entity", null);
				DynamicObjectCollection dynamicObjectCollection = (DynamicObjectCollection)base.View.Model.DataObject["TreeEntity"];
				long dynamicValue5 = DataEntityExtend.GetDynamicValue<long>(dynamicObject2, "MaterialID_Id", 0L);
				long dynamicValue6 = DataEntityExtend.GetDynamicValue<long>(dynamicObject2, "AuxPropID_Id", 0L);
				long dynamicValue7 = DataEntityExtend.GetDynamicValue<long>(dynamicObject2, "BomId_Id", 0L);
				bool dynamicValue8 = DataEntityExtend.GetDynamicValue<bool>(dynamicObject2, "IsKeyItem", false);
				using (IEnumerator<DynamicObject> enumerator2 = bomEntryDataLst.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						DynamicObject bomEntry = enumerator2.Current;
						int num2 = dynamicObjectCollection.IndexOf(bomEntry);
						DataEntityExtend.GetDynamicValue<long>(bomEntry, "MATERIALIDCHILD_Id", 0L);
						if (DataEntityExtend.GetDynamicValue<long>(bomEntry, "MATERIALIDCHILD_Id", 0L) == dynamicValue5 && DataEntityExtend.GetDynamicValue<long>(bomEntry, "AuxPropId_Id", 0L) == dynamicValue6 && DataEntityExtend.GetDynamicValue<long>(bomEntry, "BOMID_Id", 0L) == dynamicValue7)
						{
							if (IsReGetSubstitute)
							{
								List<string> mainItemRowIds = new List<string>
								{
									DataEntityExtend.GetDynamicValue<string>(bomEntry, "RowId", null)
								};
								this.DelAllSubsItems(mainItemRowIds);
								this.RemoveMainRepInfo(mainItemRowIds);
							}
							base.View.Model.SetValue("FReplacePolicy", dynamicValue, num2);
							base.View.Model.SetValue("FReplaceType", dynamicValue2, num2);
							base.View.Model.SetValue("FIskeyItem", dynamicValue8, num2);
							base.View.Model.SetValue("FSubstitutionId", num, num2);
							int num3 = (from w in substitueSchemeDatas
							where DataEntityExtend.GetDynamicValue<long>(w, "MaterialID_Id", 0L) == DataEntityExtend.GetDynamicValue<long>(bomEntry, "MATERIALIDCHILD_Id", 0L) && DataEntityExtend.GetDynamicValue<long>(w, "AuxPropID_Id", 0L) == DataEntityExtend.GetDynamicValue<long>(bomEntry, "AuxPropId_Id", 0L) && DataEntityExtend.GetDynamicValue<long>(w, "BomId_Id", 0L) == DataEntityExtend.GetDynamicValue<long>(bomEntry, "BOMID_Id", 0L)
							select w).Count<DynamicObject>();
							if (num3 > 1)
							{
								base.View.Model.SetValue("FIsMulCsd", true, num2);
							}
							else
							{
								base.View.Model.SetValue("FIsMulCsd", false, num2);
							}
							dynamicObject2["BOMRowId"] = bomEntry["RowId"];
							this.SetMainInfo(num2, dynamicObject2, true);
							if (dynamicValue8)
							{
								this.SetBomRowIdGroup(dynamicValue3, bomEntryDataLst);
								this.AddSubsItems(num2, bomEntry, dynamicValue4.ToList<DynamicObject>(), dynamicObject2);
							}
						}
					}
				}
			}
			this.SetReplaceGroup();
			this.BomChItemsSort();
			DynamicObjectCollection dynamicValue9 = DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(base.View.Model.DataObject, "TreeEntity", null);
			if (!ListUtils.IsEmpty<DynamicObject>(dynamicValue9))
			{
				DynamicObject dynamicObject3 = (from i in dynamicValue9
				where DataEntityExtend.GetDynamicValue<string>(i, "RowId", null).Equals(currentRowGuid)
				select i).FirstOrDefault<DynamicObject>();
				if (dynamicObject3 != null)
				{
					int num4 = dynamicValue9.IndexOf(dynamicObject3);
					base.View.SetEntityFocusRow("FTreeEntity", num4);
				}
			}
		}

		// Token: 0x06000112 RID: 274 RVA: 0x00011588 File Offset: 0x0000F788
		private bool IsFeatureMaterial()
		{
			DynamicObject dynamicValue = DataEntityExtend.GetDynamicValue<DynamicObject>(base.View.Model.DataObject, "MaterialId", null);
			if (ObjectUtils.IsNullOrEmpty(dynamicValue))
			{
				return false;
			}
			DynamicObjectCollection dynamicValue2 = DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(dynamicValue, "MaterialBase", null);
			if (ListUtils.IsEmpty<DynamicObject>(dynamicValue2))
			{
				return false;
			}
			string dynamicValue3 = DataEntityExtend.GetDynamicValue<string>(dynamicValue2.First<DynamicObject>(), "ErpClsId", null);
			return dynamicValue3 == "4";
		}

		// Token: 0x06000113 RID: 275 RVA: 0x000115F4 File Offset: 0x0000F7F4
		private void SyncBopData(DataChangedEventArgs e, string bopField)
		{
			long num = Convert.ToInt64(base.View.Model.GetEntryPKValue("FTreeEntity", e.Row));
			object newValue = e.NewValue;
			Entity entity = base.View.BusinessInfo.GetEntity("FBopEntity");
			DynamicObjectCollection entityDataObject = base.View.Model.GetEntityDataObject(entity);
			foreach (DynamicObject dynamicObject in entityDataObject)
			{
				if (Convert.ToInt64(dynamicObject["TreeEntryId"]) == num)
				{
					dynamicObject[bopField] = newValue;
				}
			}
			base.View.UpdateView("FBopEntity");
		}

		// Token: 0x06000114 RID: 276 RVA: 0x000116B8 File Offset: 0x0000F8B8
		private void SyncBopF8Data(DataChangedEventArgs e, string bopField, string f8FormId)
		{
			long num = Convert.ToInt64(base.View.Model.GetEntryPKValue("FTreeEntity", e.Row));
			object newValue = e.NewValue;
			FormMetadata formMetadata = MetaDataServiceHelper.Load(base.Context, f8FormId, true) as FormMetadata;
			Entity entity = base.View.BusinessInfo.GetEntity("FBopEntity");
			DynamicObjectCollection entityDataObject = base.View.Model.GetEntityDataObject(entity);
			foreach (DynamicObject dynamicObject in entityDataObject)
			{
				if (Convert.ToInt64(dynamicObject["TreeEntryId"]) == num)
				{
					dynamicObject[bopField + "_Id"] = newValue;
					DynamicObject dynamicObject2 = null;
					if (newValue != null)
					{
						dynamicObject2 = BusinessDataServiceHelper.LoadSingle(base.Context, newValue, formMetadata.BusinessInfo.GetDynamicObjectType(), null);
					}
					dynamicObject[bopField] = dynamicObject2;
				}
			}
			base.View.UpdateView("FBopEntity");
		}

		// Token: 0x06000115 RID: 277 RVA: 0x000117C8 File Offset: 0x0000F9C8
		public override void BeforeBindData(EventArgs e)
		{
			base.BeforeBindData(e);
			this.SetBarItemVisibleLocal();
		}

		// Token: 0x06000116 RID: 278 RVA: 0x000117D7 File Offset: 0x0000F9D7
		public override void AfterUpdateViewState(EventArgs e)
		{
			base.AfterUpdateViewState(e);
			this.SetBarItemVisibleLocal();
		}

		// Token: 0x06000117 RID: 279 RVA: 0x000117E8 File Offset: 0x0000F9E8
		private void SetBarItemVisibleLocal()
		{
			BomExpandNodeTreeMode bomExpandNodeTreeMode;
			if (!this.GetBomExpandNodeTreeMode(out bomExpandNodeTreeMode))
			{
				return;
			}
			base.SetAllMainBarItemVisible(false);
			if (!MFGBillUtil.GetParam<bool>(base.View, "IsFromSelectBill", false))
			{
				this.SetParentViewBarItemLocal();
			}
			if (bomExpandNodeTreeMode.ParentBomEntryId > 0L)
			{
				base.View.LockField("FMATERIALID", false);
				base.View.LockField("FCreateOrgId", false);
			}
			if (bomExpandNodeTreeMode.MaterId_Id > 0L && bomExpandNodeTreeMode.BomId_Id <= 0L)
			{
				base.View.Model.SetValue("FCreateOrgId", bomExpandNodeTreeMode.SupplyerOrg_Id);
				MFGBillUtil.GetValue<long>(base.View.Model, "FCreateOrgId", -1, 0L, null);
				if (MFGBillUtil.GetValue<long>(base.View.Model, "FCreateOrgId", -1, 0L, null) <= 0L)
				{
					base.View.Model.SetValue("FUseOrgId", 0);
				}
				else if (MFGBillUtil.GetValue<long>(base.View.Model, "FUseOrgId", -1, 0L, null) > 0L)
				{
					base.View.Model.SetItemValueByNumber("FMATERIALID", bomExpandNodeTreeMode.MaterNumber, -1);
					long auxPropId_Id = bomExpandNodeTreeMode.AuxPropId_Id;
					if (auxPropId_Id > 0L)
					{
						base.View.Model.SetItemValueByID("FParentAuxPropId", bomExpandNodeTreeMode.AuxPropId_Id, -1);
					}
				}
				if (bomExpandNodeTreeMode.MaterErpClsID == 9.ToString() && (bomExpandNodeTreeMode.ParentBomId == null || (bomExpandNodeTreeMode.ParentBomId != null && (2 == DataEntityExtend.GetDynamicObjectItemValue<int>(bomExpandNodeTreeMode.ParentBomId, "BOMCATEGORY", 0) || this.GetMaterErpClsID(DataEntityExtend.GetDynamicObjectItemValue<long>(bomExpandNodeTreeMode.ParentBomId, "MATERIALID_Id", 0L)) == 4))))
				{
					base.View.Model.SetValue("FBOMCATEGORY", 2);
				}
			}
		}

		// Token: 0x06000118 RID: 280 RVA: 0x000119B8 File Offset: 0x0000FBB8
		private BOSEnums.Enu_MaterialType GetMaterErpClsID(long materialId)
		{
			string text = string.Format("{0}={1}", "FMATERIALID", materialId);
			List<SelectorItemInfo> list = new List<SelectorItemInfo>
			{
				new SelectorItemInfo("FErpClsID")
			};
			DynamicObject dynamicObject = MFGServiceHelper.GetBaseBillInfo(base.Context, "BD_MATERIAL", list, text, "").FirstOrDefault<DynamicObject>();
			DynamicObject dynamicObject2 = DataEntityExtend.GetDynamicObjectItemValue<DynamicObjectCollection>(dynamicObject, "MaterialBase", null).First<DynamicObject>();
			return DataEntityExtend.GetDynamicObjectItemValue<int>(dynamicObject2, "ErpClsID", 0);
		}

		// Token: 0x06000119 RID: 281 RVA: 0x00011A34 File Offset: 0x0000FC34
		public override void BeforeDoOperation(BeforeDoOperationEventArgs e)
		{
			base.BeforeDoOperation(e);
			if (e.Operation.FormOperation.Operation == "Save")
			{
				this.GetSubstitueWhenSaving();
			}
			BomExpandNodeTreeMode bomExpandNodeTreeMode;
			if (!this.GetBomExpandNodeTreeMode(out bomExpandNodeTreeMode))
			{
				return;
			}
			string operation;
			if ((operation = e.Operation.FormOperation.Operation) != null)
			{
				if (operation == "Save" || operation == "Draft")
				{
					e.Option.SetVariableValue("parentBomEntityId", bomExpandNodeTreeMode.ParentBomEntryId);
					return;
				}
				if (operation == "Audit")
				{
					e.Option.SetVariableValue("lstParentBomEntryId", new List<long>
					{
						bomExpandNodeTreeMode.ParentBomEntryId
					});
					return;
				}
				if (!(operation == "BatchEdit"))
				{
					return;
				}
				this.ShowBomBatchEdit();
			}
		}

		// Token: 0x0600011A RID: 282 RVA: 0x00011B24 File Offset: 0x0000FD24
		public override void AfterDoOperation(AfterDoOperationEventArgs e)
		{
			base.AfterDoOperation(e);
			BomExpandNodeTreeMode bomExpandNodeTreeMode;
			if (base.View.ParentFormView != null && !this.GetBomExpandNodeTreeMode(out bomExpandNodeTreeMode) && base.View.ParentFormView.BusinessInfo.GetForm().Id == "ENG_BOMTREE")
			{
				return;
			}
			string operation;
			switch (operation = e.Operation.Operation)
			{
			case "Save":
			{
				if (base.View.ParentFormView != null)
				{
					(base.View.ParentFormView as IDynamicFormViewService).CustomEvents(base.View.PageId, "ReflashBomData", e.OperationResult.IsSuccess ? "1" : "0");
					base.View.SendDynamicFormAction(base.View.ParentFormView);
				}
				FormMetadata formMetadata = (FormMetadata)MetaDataServiceHelper.Load(base.View.Context, "ENG_BOM_BILLPARAM", true);
				DynamicObject dynamicObject = UserParamterServiceHelper.Load(base.View.Context, formMetadata.BusinessInfo, base.View.Context.UserId, "ENG_BOM", "UserParameter");
				if (DataEntityExtend.GetDynamicValue<bool>(dynamicObject, "AutoGetRepWhenSaving", false))
				{
					base.View.UpdateView("FTreeEntity");
					return;
				}
				break;
			}
			case "Draft":
			case "Submit":
			case "CancelAssign":
			case "UnAudit":
				if (base.View.ParentFormView != null)
				{
					(base.View.ParentFormView as IDynamicFormViewService).CustomEvents(base.View.PageId, "ReflashBomData", e.OperationResult.IsSuccess ? "1" : "0");
					base.View.SendDynamicFormAction(base.View.ParentFormView);
					return;
				}
				break;
			case "Audit":
			{
				if (base.View.ParentFormView != null)
				{
					(base.View.ParentFormView as IDynamicFormViewService).CustomEvents(base.View.PageId, "ReflashBomData", e.OperationResult.IsSuccess ? "1" : "0");
					base.View.SendDynamicFormAction(base.View.ParentFormView);
				}
				DynamicObject parameterData = this.Model.ParameterData;
				string dynamicValue = DataEntityExtend.GetDynamicValue<string>(parameterData, "FUpdateType", null);
				if (dynamicValue == "1" && e.OperationResult.IsSuccess && base.View.ParentFormView.BusinessInfo.GetForm().Id != "ENG_BOMTREE")
				{
					this.ShowListDatas("tbApprove");
					return;
				}
				break;
			}
			case "Forbid":
				if (this.IsEnablelForbidreason)
				{
					List<object> list = (from p in e.OperationResult.OperateResult
					select p.PKValue).ToList<object>();
					if (ListUtils.IsEmpty<object>(list))
					{
						return;
					}
					string[] array = (from s in list
					select Convert.ToString(s)).ToArray<string>();
					if (!ListUtils.IsEmpty<string>(array))
					{
						base.View.Model.SetValue("FForbidReson", this.forbidReason);
						base.View.Model.Save(null);
						return;
					}
				}
				break;
			case "Enable":
			{
				List<object> list2 = (from p in e.OperationResult.OperateResult
				select p.PKValue).ToList<object>();
				if (ListUtils.IsEmpty<object>(list2))
				{
					return;
				}
				string[] array2 = (from s in list2
				select Convert.ToString(s)).ToArray<string>();
				if (!ListUtils.IsEmpty<string>(array2))
				{
					base.View.Model.SetValue("FForbidReson", null);
					base.View.Model.Save(null);
				}
				break;
			}

				return;
			}
		}

		// Token: 0x0600011B RID: 283 RVA: 0x00011F8A File Offset: 0x0001018A
		private bool GetBomExpandNodeTreeMode(out BomExpandNodeTreeMode bomFormParam)
		{
			bomFormParam = MFGBillUtil.GetParentFormSession<BomExpandNodeTreeMode>(base.View, "FormInputParam");
			return bomFormParam != null;
		}

		// Token: 0x0600011C RID: 284 RVA: 0x00011FA8 File Offset: 0x000101A8
		private void SetParentViewBarItemLocal()
		{
			if (base.View.ParentFormView != null && base.View.ParentFormView.BusinessInfo.GetForm().Id == "ENG_BOMTREE")
			{
				Dictionary<string, BarItem> allBarItems = base.View.ParentFormView.LayoutInfo.GetFormAppearance().Menu.GetAllBarItems();
				Dictionary<string, BarItem> allBarItems2 = base.View.LayoutInfo.GetFormAppearance().Menu.GetAllBarItems();
				new List<string>();
				foreach (KeyValuePair<string, BarItem> keyValuePair in allBarItems)
				{
					BarItem barItem;
					base.View.ParentFormView.GetMainBarItem(keyValuePair.Key).Enabled = (!allBarItems2.TryGetValue(keyValuePair.Key, out barItem) || base.View.GetMainBarItem(keyValuePair.Key).Enabled);
				}
				base.View.SendDynamicFormAction(base.View.ParentFormView);
			}
		}

		// Token: 0x0600011D RID: 285 RVA: 0x000120C8 File Offset: 0x000102C8
		private void SendSupplyOrgChangeEvents(long materialId)
		{
			if (materialId <= 0L || base.View.ParentFormView == null)
			{
				return;
			}
			(base.View.ParentFormView as IDynamicFormViewService).CustomEvents(base.View.PageId, "ChangeTreeBySupplyOrg", Convert.ToString(materialId));
			base.View.SendDynamicFormAction(base.View.ParentFormView);
		}

		// Token: 0x04000049 RID: 73
		private bool bCancelDataChangedEvent;

		// Token: 0x0400004A RID: 74
		private bool bMoveUpOrDown;

		// Token: 0x0400004B RID: 75
		private bool bIsReplaceDataChange;

		// Token: 0x0400004C RID: 76
		private string forbidReason;

		// Token: 0x0400004D RID: 77
		private bool IsEnablelForbidreason;

		// Token: 0x0400004E RID: 78
		private LowestBomCodeOption.T_NewBomFromIntegrityCheckParam param;

		// Token: 0x0400004F RID: 79
		private bool canUpdate;

		// Token: 0x04000050 RID: 80
		private bool IsCopyDataIssueType;

		// Token: 0x04000051 RID: 81
		private List<string> fieldNameList = new List<string>();

		// Token: 0x04000052 RID: 82
		private int vis = 224;

		// Token: 0x04000053 RID: 83
		private Dictionary<string, int> bomRowIdGroup = new Dictionary<string, int>();
	}
}
