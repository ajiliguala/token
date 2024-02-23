using System;
using System.Collections.Generic;
using System.Linq;
using Kingdee.BOS;
using Kingdee.BOS.BusinessEntity.Organizations;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args.WizardForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.WizardForm;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.MFG.ENG.BomExpand;
using Kingdee.K3.Core.MFG.ENG.ParamOption;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.Core.MFG.EnumConst;
using Kingdee.K3.Core.MFG.Utils;
using Kingdee.K3.MFG.Common.BusinessEntity.ENG.BomExpand;
using Kingdee.K3.MFG.ServiceHelper.ENG;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm.BOMBatchAllocate
{
	// Token: 0x0200005C RID: 92
	public class BomBatchAllocateWizardEdit : AbstractWizardFormPlugIn
	{
		// Token: 0x060006B3 RID: 1715 RVA: 0x0004F0C3 File Offset: 0x0004D2C3
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
			this.AddChangingEventHandlers();
			this.AddChangedEventHandlers();
		}

		// Token: 0x060006B4 RID: 1716 RVA: 0x0004F0D8 File Offset: 0x0004D2D8
		public override void BeforeBindData(EventArgs e)
		{
			this.CalledFromOther();
		}

		// Token: 0x060006B5 RID: 1717 RVA: 0x0004F0E0 File Offset: 0x0004D2E0
		public override void AfterBindData(EventArgs e)
		{
			if (!this.ValidatePermission())
			{
				base.View.ShowErrMessage(ResManager.LoadKDString("当前用户没有当前组织BOM的查看权限！", "015072000012062", 7, new object[0]), "", 0);
				base.View.GetControl("FNext").Enabled = false;
			}
		}

		// Token: 0x060006B6 RID: 1718 RVA: 0x0004F132 File Offset: 0x0004D332
		public override void ButtonClick(ButtonClickEventArgs e)
		{
			base.ButtonClick(e);
		}

		// Token: 0x060006B7 RID: 1719 RVA: 0x0004F13C File Offset: 0x0004D33C
		public override void WizardStepChanged(WizardStepChangedEventArgs e)
		{
			base.WizardStepChanged(e);
			Action<WizardStepChangedEventArgs> action = null;
			if (this._stepChangedHandles.TryGetValue(string.Format("{0}_{1}", e.UpDownEnum, e.WizardStep.ContainerKey), out action))
			{
				action(e);
			}
		}

		// Token: 0x060006B8 RID: 1720 RVA: 0x0004F188 File Offset: 0x0004D388
		public override void WizardStepChanging(WizardStepChangingEventArgs e)
		{
			base.WizardStepChanging(e);
			Action<WizardStepChangingEventArgs> action = null;
			string arg = (e.OldWizardStep == null) ? "" : e.OldWizardStep.ContainerKey;
			if (this._stepChangingHandles.TryGetValue(string.Format("{0}_{1}", arg, e.NewWizardStep.ContainerKey), out action))
			{
				action(e);
			}
		}

		// Token: 0x060006B9 RID: 1721 RVA: 0x0004F1E8 File Offset: 0x0004D3E8
		public override void AfterCreateNewEntryRow(CreateNewEntryEventArgs e)
		{
			if (e.Entity.Key == "FCalResult")
			{
				base.View.Model.SetValue("FROWID0", SequentialGuid.NewGuid().ToString(), e.Row);
			}
		}

		// Token: 0x060006BA RID: 1722 RVA: 0x0004F23A File Offset: 0x0004D43A
		public override void BeforeClosed(BeforeClosedEventArgs e)
		{
			base.BeforeClosed(e);
		}

		// Token: 0x060006BB RID: 1723 RVA: 0x0004F244 File Offset: 0x0004D444
		private void AddChangingEventHandlers()
		{
			this._stepChangingHandles.Add("FAllocateScope_FCalculateResult", new Action<WizardStepChangingEventArgs>(this.step0ChangingStep1));
			this._stepChangingHandles.Add("FCalculateResult_FAllocateScope", new Action<WizardStepChangingEventArgs>(this.step1ChangingStep0));
			this._stepChangingHandles.Add("FCalculateResult_FAllocateResult", new Action<WizardStepChangingEventArgs>(this.step1ChangingStep2));
			this._stepChangingHandles.Add("FAllocateResult_FCalculateResult", new Action<WizardStepChangingEventArgs>(this.step2ChangingStep1));
		}

		// Token: 0x060006BC RID: 1724 RVA: 0x0004F2C4 File Offset: 0x0004D4C4
		private void AddChangedEventHandlers()
		{
			this._stepChangedHandles.Add("1_FCalculateResult", new Action<WizardStepChangedEventArgs>(this.step0Changed2Step1));
			this._stepChangedHandles.Add("2_FAllocateScope", new Action<WizardStepChangedEventArgs>(this.step1Changed2Step0));
			this._stepChangedHandles.Add("1_FAllocateResult", new Action<WizardStepChangedEventArgs>(this.step1Changed2Step2));
			this._stepChangedHandles.Add("2_FCalculateResult", new Action<WizardStepChangedEventArgs>(this.step2Changed2Step1));
		}

		// Token: 0x060006BD RID: 1725 RVA: 0x0004F344 File Offset: 0x0004D544
		private bool ValidatePermission()
		{
			bool flag = MFGCommonUtil.AuthPermissionBeforeShowF7Form(base.View, "ENG_BOM", "6e44119a58cb4a8e86f6c385e14a17ad");
			if (!flag)
			{
				flag = false;
			}
			return flag;
		}

		// Token: 0x060006BE RID: 1726 RVA: 0x0004F3A0 File Offset: 0x0004D5A0
		private void step0ChangingStep1(WizardStepChangingEventArgs args)
		{
			DynamicObjectCollection source = this.Model.DataObject["BOMScope"] as DynamicObjectCollection;
			DynamicObjectCollection source2 = this.Model.DataObject["OrgScope"] as DynamicObjectCollection;
			if (!source.Any((DynamicObject x) => OtherExtend.ConvertTo<long>(x["BOMID_Id"], 0L) > 0L))
			{
				base.View.ShowErrMessage(ResManager.LoadKDString("请选择一个或多个BOM版本", "015072000012063", 7, new object[0]), "", 0);
				args.Cancel = true;
			}
			if (!source2.Any((DynamicObject x) => OtherExtend.ConvertTo<long>(x["OrgId_Id"], 0L) > 0L))
			{
				base.View.ShowErrMessage(ResManager.LoadKDString("请选择分配的组织范围", "015072000012064", 7, new object[0]), "", 0);
				args.Cancel = true;
			}
		}

		// Token: 0x060006BF RID: 1727 RVA: 0x0004F489 File Offset: 0x0004D689
		private void step1ChangingStep0(WizardStepChangingEventArgs args)
		{
		}

		// Token: 0x060006C0 RID: 1728 RVA: 0x0004F4B4 File Offset: 0x0004D6B4
		private void step1ChangingStep2(WizardStepChangingEventArgs args)
		{
			DynamicObjectCollection source = this.Model.DataObject["CalResult"] as DynamicObjectCollection;
			List<DynamicObject> list = (from x in source
			where OtherExtend.ConvertTo<bool>(x["IsFatalErr"], false)
			select x).ToList<DynamicObject>();
			if (ListUtils.IsEmpty<DynamicObject>(list))
			{
				return;
			}
			List<IGrouping<string, DynamicObject>> list2 = (from x in list
			group x by OtherExtend.ConvertTo<string>(x["AllocStatus"], null)).ToList<IGrouping<string, DynamicObject>>();
			OperateResultCollection operateResultCollection = new OperateResultCollection();
			foreach (IGrouping<string, DynamicObject> grouping in list2)
			{
				if (grouping.Key == CONST_ENG_BOMBatchAllocate.Enu_AllocStatus.NoCtrlPolicy)
				{
					using (IEnumerator<DynamicObject> enumerator2 = grouping.GetEnumerator())
					{
						while (enumerator2.MoveNext())
						{
							DynamicObject dynamicObject = enumerator2.Current;
							DynamicObject dynamicObject2 = dynamicObject["MaterialId1"] as DynamicObject;
							DynamicObject dynamicObject3 = dynamicObject["BOMID1"] as DynamicObject;
							DynamicObject dynamicObject4 = dynamicObject["CreateOrgId"] as DynamicObject;
							LocaleValue localeValue = dynamicObject4["Name"] as LocaleValue;
							DynamicObject dynamicObject5 = dynamicObject["tgtOrgId"] as DynamicObject;
							if (dynamicObject5 != null)
							{
								LocaleValue localeValue2 = dynamicObject5["Name"] as LocaleValue;
								operateResultCollection.Add(new OperateResult
								{
									Message = string.Format(ResManager.LoadKDString("父项编码{0}，BOM版本{1}，源组织{2}，目标组织{3}不存在分配关系，请维护.", "015072000012065", 7, new object[0]), new object[]
									{
										dynamicObject2["Number"],
										dynamicObject3["Number"],
										localeValue[base.Context.LogLocale.LCID],
										localeValue2[base.Context.LogLocale.LCID]
									}),
									MessageType = 0,
									SuccessStatus = false,
									Name = dynamicObject3["Number"].ToString()
								});
							}
							else
							{
								operateResultCollection.Add(new OperateResult
								{
									Message = string.Format(ResManager.LoadKDString("父项编码{0}，BOM版本{1}，源组织{2}的父级BOM未设置分配策略，请维护.", "015072000012066", 7, new object[0]), dynamicObject2["Number"], dynamicObject3["Number"], localeValue[base.Context.LogLocale.LCID]),
									MessageType = 0,
									SuccessStatus = false,
									Name = dynamicObject3["Number"].ToString()
								});
							}
						}
						continue;
					}
				}
				if (grouping.Key == CONST_ENG_BOMBatchAllocate.Enu_AllocStatus.MtrlYetToAlloc)
				{
					using (IEnumerator<DynamicObject> enumerator3 = grouping.GetEnumerator())
					{
						while (enumerator3.MoveNext())
						{
							DynamicObject dynamicObject6 = enumerator3.Current;
							DynamicObject dynamicObject7 = dynamicObject6["MaterialId1"] as DynamicObject;
							DynamicObject dynamicObject8 = dynamicObject6["BOMID1"] as DynamicObject;
							DynamicObject dynamicObject9 = dynamicObject6["CreateOrgId"] as DynamicObject;
							LocaleValue localeValue3 = dynamicObject9["Name"] as LocaleValue;
							DynamicObject dynamicObject10 = dynamicObject6["tgtOrgId"] as DynamicObject;
							LocaleValue localeValue4 = dynamicObject10["Name"] as LocaleValue;
							operateResultCollection.Add(new OperateResult
							{
								Message = string.Format(ResManager.LoadKDString("父项编码{0}，BOM版本{1}，源组织{2}，目标组织{3}父项物料未分配，请先分配。", "015072000012067", 7, new object[0]), new object[]
								{
									dynamicObject7["Number"],
									dynamicObject8["Number"],
									localeValue3[base.Context.LogLocale.LCID],
									localeValue4[base.Context.LogLocale.LCID]
								}),
								MessageType = 0,
								SuccessStatus = false,
								Name = dynamicObject8["Number"].ToString()
							});
						}
						continue;
					}
				}
				if (grouping.Key == CONST_ENG_BOMBatchAllocate.Enu_AllocStatus.YetToAudit)
				{
					foreach (DynamicObject dynamicObject11 in grouping)
					{
						DynamicObject dynamicObject12 = dynamicObject11["MaterialId1"] as DynamicObject;
						DynamicObject dynamicObject13 = dynamicObject11["BOMID1"] as DynamicObject;
						DynamicObject dynamicObject14 = dynamicObject11["CreateOrgId"] as DynamicObject;
						object obj = dynamicObject14["Name"];
						DynamicObject dynamicObject15 = dynamicObject11["tgtOrgId"] as DynamicObject;
						object obj2 = dynamicObject15["Name"];
						operateResultCollection.Add(new OperateResult
						{
							Message = string.Format(ResManager.LoadKDString("父项编码{0}，BOM版本{1}，未处于已审核状态，请先审核。", "015072000012068", 7, new object[0]), dynamicObject12["Number"], dynamicObject13["Number"]),
							MessageType = 0,
							SuccessStatus = false,
							Name = dynamicObject13["Number"].ToString()
						});
					}
				}
			}
			args.Cancel = true;
			base.View.ShowOperateResult(operateResultCollection, "BOS_BatchTips");
		}

		// Token: 0x060006C1 RID: 1729 RVA: 0x0004FA88 File Offset: 0x0004DC88
		private void step2ChangingStep1(WizardStepChangingEventArgs args)
		{
			IDynamicFormView view = base.View.GetView(string.Format("{0}_{1}", base.View.PageId, "AE"));
			if (view != null)
			{
				view.Model.DataChanged = false;
				view.Close();
				base.View.SendDynamicFormAction(view);
			}
		}

		// Token: 0x060006C2 RID: 1730 RVA: 0x0004FADC File Offset: 0x0004DCDC
		private void step0Changed2Step1(WizardStepChangedEventArgs args)
		{
			this.DoBomExpand();
		}

		// Token: 0x060006C3 RID: 1731 RVA: 0x0004FAE4 File Offset: 0x0004DCE4
		private void step1Changed2Step0(WizardStepChangedEventArgs args)
		{
		}

		// Token: 0x060006C4 RID: 1732 RVA: 0x0004FAE6 File Offset: 0x0004DCE6
		private void step1Changed2Step2(WizardStepChangedEventArgs args)
		{
			base.View.StyleManager.SetEnabled("FIsAutoAudit", "focus", false);
			this.BuildAllocateList();
		}

		// Token: 0x060006C5 RID: 1733 RVA: 0x0004FB09 File Offset: 0x0004DD09
		private void step2Changed2Step1(WizardStepChangedEventArgs args)
		{
			base.View.StyleManager.SetEnabled("FIsAutoAudit", "focus", true);
			this.DoBomExpand();
		}

		// Token: 0x060006C6 RID: 1734 RVA: 0x0004FB3C File Offset: 0x0004DD3C
		public override void EntryBarItemClick(BarItemClickEventArgs e)
		{
			string barItemKey;
			if ((barItemKey = e.BarItemKey) != null)
			{
				if (!(barItemKey == "tbAllocRefMtrl"))
				{
					if (!(barItemKey == "tbReload"))
					{
						return;
					}
					this.DoBomExpand();
				}
				else
				{
					if (ListUtils.IsEmpty<string>(this._needAllocMtrls))
					{
						base.View.ShowMessage(ResManager.LoadKDString("没有需要分配的物料", "015072000012289", 7, new object[0]), 0);
						return;
					}
					DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter
					{
						FormId = "ENG_MtrlBatchAlloc",
						PageId = SequentialGuid.NewGuid().ToString(),
						ParentPageId = base.View.PageId
					};
					dynamicFormShowParameter.OpenStyle.ShowType = 7;
					dynamicFormShowParameter.CustomComplexParams.Add("IsAutoAudit", OtherExtend.ConvertTo<bool>(this.Model.GetValue("FIsAutoAudit"), false));
					dynamicFormShowParameter.CustomComplexParams.Add("NeedAllocMtrls", this._needAllocMtrls);
					base.View.ShowForm(dynamicFormShowParameter, delegate(FormResult x)
					{
						if (this.Model != null)
						{
							this.DoBomExpand();
						}
					});
					return;
				}
			}
		}

		// Token: 0x060006C7 RID: 1735 RVA: 0x0004FC58 File Offset: 0x0004DE58
		public override void AuthPermissionBeforeF7Select(AuthPermissionBeforeF7SelectEventArgs e)
		{
			string fieldKey;
			if ((fieldKey = e.FieldKey) != null)
			{
				if (!(fieldKey == "FBOMID1") && !(fieldKey == "FMaterialId1"))
				{
					return;
				}
				e.IsIsolationOrg = false;
			}
		}

		// Token: 0x060006C8 RID: 1736 RVA: 0x0004FCC8 File Offset: 0x0004DEC8
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			string fieldKey;
			if ((fieldKey = e.FieldKey) != null)
			{
				if (!(fieldKey == "FBOMID"))
				{
					if (!(fieldKey == "FOrgId"))
					{
						if (!(fieldKey == "FBOMID1") && !(fieldKey == "FMaterialId1"))
						{
							return;
						}
						e.IsShowApproved = false;
					}
					else
					{
						DynamicObjectCollection dynamicObjectCollection = this.Model.DataObject["OrgScope"] as DynamicObjectCollection;
						if (ListUtils.IsEmpty<DynamicObject>(dynamicObjectCollection))
						{
							return;
						}
						List<long> list = (from os in dynamicObjectCollection
						select OtherExtend.ConvertTo<long>(os["OrgId_Id"], 0L) into orgId
						where orgId > 0L
						select orgId).Distinct<long>().ToList<long>();
						list.Add(base.Context.CurrentOrganizationInfo.ID);
						list.Distinct<long>();
						if (ListUtils.IsEmpty<long>(list))
						{
							return;
						}
						IRegularFilterParameter listFilterParameter = e.ListFilterParameter;
						listFilterParameter.Filter += string.Format(" {0} FORGID NOT IN ({1}) ", (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(e.ListFilterParameter.Filter)) ? "AND" : "", string.Join<long>(",", list));
						return;
					}
				}
				else
				{
					IRegularFilterParameter listFilterParameter2 = e.ListFilterParameter;
					listFilterParameter2.Filter += string.Format(" {0} FCREATEORGID = FUSEORGID ", ObjectUtils.IsNullOrEmptyOrWhiteSpace(e.ListFilterParameter.Filter) ? "" : "AND");
					DynamicObjectCollection dynamicObjectCollection2 = this.Model.DataObject["BOMScope"] as DynamicObjectCollection;
					if (ListUtils.IsEmpty<DynamicObject>(dynamicObjectCollection2))
					{
						return;
					}
					List<long> list2 = (from bs in dynamicObjectCollection2
					select OtherExtend.ConvertTo<long>(bs["BOMID_Id"], 0L) into bomId
					where bomId > 0L
					select bomId).Distinct<long>().ToList<long>();
					if (ListUtils.IsEmpty<long>(list2))
					{
						return;
					}
					string str = string.Format(" {0} FID NOT IN ({1}) ", (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(e.ListFilterParameter.Filter)) ? "AND" : "", string.Join<long>(",", list2));
					IRegularFilterParameter listFilterParameter3 = e.ListFilterParameter;
					listFilterParameter3.Filter += str;
					return;
				}
			}
		}

		// Token: 0x060006C9 RID: 1737 RVA: 0x0004FF54 File Offset: 0x0004E154
		public override void BeforeSetItemValueByNumber(BeforeSetItemValueByNumberArgs e)
		{
			string baseDataFieldKey;
			if ((baseDataFieldKey = e.BaseDataFieldKey) != null)
			{
				if (!(baseDataFieldKey == "FBOMID"))
				{
					if (!(baseDataFieldKey == "FOrgId"))
					{
						if (!(baseDataFieldKey == "FBOMID1") && !(baseDataFieldKey == "FMaterialId1"))
						{
							return;
						}
						e.IsShowApproved = false;
					}
					else
					{
						DynamicObjectCollection dynamicObjectCollection = this.Model.DataObject["OrgScope"] as DynamicObjectCollection;
						if (ListUtils.IsEmpty<DynamicObject>(dynamicObjectCollection))
						{
							return;
						}
						List<long> list = (from os in dynamicObjectCollection
						select OtherExtend.ConvertTo<long>(os["OrgId_Id"], 0L) into orgId
						where orgId > 0L
						select orgId).Distinct<long>().ToList<long>();
						list.Add(base.Context.CurrentOrganizationInfo.ID);
						list.Distinct<long>();
						if (ListUtils.IsEmpty<long>(list))
						{
							return;
						}
						e.Filter += string.Format(" {0} FORGID NOT IN ({1}) ", (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(e.Filter)) ? "AND" : "", string.Join<long>(",", list));
						return;
					}
				}
				else
				{
					e.Filter += string.Format(" {0} FCREATEORGID = FUSEORGID ", ObjectUtils.IsNullOrEmptyOrWhiteSpace(e.Filter) ? "" : "AND");
					DynamicObjectCollection dynamicObjectCollection2 = this.Model.DataObject["BOMScope"] as DynamicObjectCollection;
					if (ListUtils.IsEmpty<DynamicObject>(dynamicObjectCollection2))
					{
						return;
					}
					List<long> list2 = (from bs in dynamicObjectCollection2
					select OtherExtend.ConvertTo<long>(bs["BOMID_Id"], 0L) into bomId
					where bomId > 0L
					select bomId).Distinct<long>().ToList<long>();
					if (ListUtils.IsEmpty<long>(list2))
					{
						return;
					}
					string str = string.Format(" {0} FID NOT IN ({1}) ", (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(e.Filter)) ? "AND" : "", string.Join<long>(",", list2));
					e.Filter += str;
					return;
				}
			}
		}

		// Token: 0x060006CA RID: 1738 RVA: 0x00050224 File Offset: 0x0004E424
		private Dictionary<long, long> GetMasterBOMIds(List<List<DynamicObject>> bomTrees)
		{
			List<KeyValuePair<long, long>> source = bomTrees.SelectMany((List<DynamicObject> x) => x.Select(delegate(DynamicObject bomRow)
			{
				long num = OtherExtend.ConvertTo<long>(bomRow["BOMID_ID"], 0L);
				DynamicObject dynamicObject = bomRow["BOMID"] as DynamicObject;
				long key = 0L;
				long value = 0L;
				if (num > 0L)
				{
					key = OtherExtend.ConvertTo<long>(dynamicObject["CreateOrgId_Id"], 0L);
					value = OtherExtend.ConvertTo<long>(dynamicObject["msterID"], 0L);
				}
				return new KeyValuePair<long, long>(key, value);
			})).ToList<KeyValuePair<long, long>>();
			IEnumerable<IGrouping<long, KeyValuePair<long, long>>> enumerable = from x in source
			group x by x.Key;
			Dictionary<long, long> dictionary = new Dictionary<long, long>();
			foreach (IGrouping<long, KeyValuePair<long, long>> grouping in enumerable)
			{
				foreach (KeyValuePair<long, long> keyValuePair in grouping)
				{
					dictionary[keyValuePair.Value] = keyValuePair.Value;
				}
			}
			return dictionary;
		}

		// Token: 0x060006CB RID: 1739 RVA: 0x00050308 File Offset: 0x0004E508
		private DynamicObjectCollection GetMasterBomIds(long createOrgId, List<long> msterID)
		{
			QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter
			{
				FormId = "ENG_BOM",
				SelectItems = SelectorItemInfo.CreateItems(new string[]
				{
					"FMASTERID",
					"FID"
				}),
				FilterClauseWihtKey = string.Format("FUseOrgId=@useOrgId", new object[0])
			};
			queryBuilderParemeter.ExtJoinTables.Add(new ExtJoinTableDescription
			{
				TableName = "table(fn_StrSplit(@msterId,',',1))",
				TableNameAs = "tms",
				FieldName = "FID",
				ScourceKey = "FMASTERID"
			});
			List<SqlParam> list = new List<SqlParam>
			{
				new SqlParam("@msterId", 161, msterID.Distinct<long>().ToArray<long>()),
				new SqlParam("@useOrgId", 12, createOrgId)
			};
			return QueryServiceHelper.GetDynamicObjectCollection(base.Context, queryBuilderParemeter, list);
		}

		// Token: 0x060006CC RID: 1740 RVA: 0x00050444 File Offset: 0x0004E644
		private void DoBomExpand()
		{
			this._needAllocMtrls.Clear();
			DynamicObjectCollection dynamicObjectCollection = this.Model.DataObject["BOMScope"] as DynamicObjectCollection;
			DynamicObjectCollection dynamicObjectCollection2 = this.Model.DataObject["OrgScope"] as DynamicObjectCollection;
			List<DynamicObject> list = new List<DynamicObject>();
			foreach (DynamicObject dynamicObject in dynamicObjectCollection)
			{
				BomForwardSourceDynamicRow bomForwardSourceDynamicRow = BomForwardSourceDynamicRow.CreateInstance();
				bomForwardSourceDynamicRow.MaterialId_Id = OtherExtend.ConvertTo<long>(dynamicObject["MaterialId_Id"], 0L);
				bomForwardSourceDynamicRow.BomId_Id = OtherExtend.ConvertTo<long>(dynamicObject["BOMID_Id"], 0L);
				bomForwardSourceDynamicRow.NeedQty = 100m;
				bomForwardSourceDynamicRow.NeedDate = new DateTime?(DateTime.Today);
				bomForwardSourceDynamicRow.WorkCalId_Id = 0L;
				bomForwardSourceDynamicRow.TimeUnit = 1.ToString();
				list.Add(bomForwardSourceDynamicRow.DataEntity);
			}
			MemBomExpandOption_ForPSV bomQueryOption = this.GetBomQueryOption();
			DynamicObject dynamicObject2 = BomExpandServiceHelper.ExpandBomForwardMen(base.View.Context, list, bomQueryOption);
			List<DynamicObject> expandResult = (dynamicObject2["BomExpandResult"] as DynamicObjectCollection).ToList<DynamicObject>();
			List<List<DynamicObject>> bomTrees = this.GetBomTrees(expandResult);
			List<long> allOrg = this.GetAllOrg();
			Dictionary<long, BaseDataControlPolicy> dictionary = new Dictionary<long, BaseDataControlPolicy>();
			foreach (long num in allOrg)
			{
				BaseDataControlPolicy baseDataControlPolicyDObj = OrganizationServiceHelper.GetBaseDataControlPolicyDObj(base.Context, num, "ENG_BOM");
				if (baseDataControlPolicyDObj != null)
				{
					dictionary.Add(num, baseDataControlPolicyDObj);
				}
			}
			Entity entity = base.View.BusinessInfo.GetEntity("FCalResult");
			this.Model.DeleteEntryData("FCalResult");
			this.Model.ClearNoDataRow();
			Dictionary<long, long> masterBOMIds = this.GetMasterBOMIds(bomTrees);
			foreach (DynamicObject dynamicObject3 in dynamicObjectCollection2)
			{
				foreach (List<DynamicObject> source in bomTrees)
				{
					List<IGrouping<int, DynamicObject>> list2 = (from x in source
					group x by OtherExtend.ConvertTo<int>(x["BOMLevel"], 0) into x
					orderby x.Key
					select x).ToList<IGrouping<int, DynamicObject>>();
					Dictionary<string, DynamicObject> dictionary2 = new Dictionary<string, DynamicObject>();
					foreach (IGrouping<int, DynamicObject> grouping in list2)
					{
						foreach (DynamicObject dynamicObject4 in grouping)
						{
							int num2 = 0;
							DynamicObject dynamicObject5 = DataEntityExtend.CreateNewEntryRow(this.Model, entity, -1, ref num2);
							OtherExtend.ConvertTo<string>(dynamicObject5["ROWID"], null);
							string text = dynamicObject4["RowId"].ToString();
							string text2 = dynamicObject4["ParentEntryId"].ToString();
							this.Model.SetValue("FEntryId", text, num2);
							this.Model.SetValue("FParentEntryId", text2, num2);
							long num3 = OtherExtend.ConvertTo<long>(dynamicObject4["BOMID_Id"], 0L);
							this.Model.SetValue("FAllocSeq", dynamicObject4["BomLevel"], num2);
							if (num3 > 0L)
							{
								DynamicObject dynamicObject6 = dynamicObject4["BOMID"] as DynamicObject;
								long num4 = OtherExtend.ConvertTo<long>(dynamicObject6["UseOrgId_Id"], 0L);
								long num5 = OtherExtend.ConvertTo<long>(dynamicObject6["CreateOrgId_Id"], 0L);
								this.Model.SetValue("FCreateOrgId", num5, num2);
								if (num4 != num5)
								{
									long num6 = 0L;
									num3 = (masterBOMIds.TryGetValue(OtherExtend.ConvertTo<long>(dynamicObject6["msterID"], 0L), out num6) ? num6 : 0L);
								}
							}
							this.Model.SetValue("FBOMID1", num3, num2);
							this.Model.SetValue("FMaterialId1", dynamicObject4["MaterialId_Id"], num2);
							DynamicObject dynamicObject7 = null;
							if (dictionary2.TryGetValue(text2, out dynamicObject7))
							{
								this.Model.SetValue("FParentRowId", dynamicObject7["ROWID"], num2);
								long num7 = OtherExtend.ConvertTo<long>(dynamicObject7["CreateOrgId_Id"], 0L);
								long parentTgtOrgId = OtherExtend.ConvertTo<long>(dynamicObject7["tgtOrgId_Id"], 0L);
								BaseDataControlPolicy baseDataControlPolicy = null;
								if (dictionary.TryGetValue(num7, out baseDataControlPolicy))
								{
									BaseDataControlPolicyTargetOrgEntry baseDataControlPolicyTargetOrgEntry = (from x in baseDataControlPolicy.TargetOrgEntrys
									where x.TargetOrgId == parentTgtOrgId
									select x).FirstOrDefault<BaseDataControlPolicyTargetOrgEntry>();
									if (baseDataControlPolicyTargetOrgEntry != null)
									{
										BaseDataControlPolicyPropertyEntry baseDataControlPolicyPropertyEntry = (from x in baseDataControlPolicyTargetOrgEntry.PropertyEntrys
										where StringUtils.EqualsIgnoreCase(x.Key, "FCHILDSUPPLYORGID")
										select x).FirstOrDefault<BaseDataControlPolicyPropertyEntry>();
										switch ((baseDataControlPolicyPropertyEntry == null) ? short.Parse("1") : baseDataControlPolicyPropertyEntry.ControlTypeId)
										{
										case 1:
										{
											BomExpandView.BomExpandResult bomExpandResult = dynamicObject4;
											if (bomExpandResult.HasControlAttribute(8))
											{
												this.Model.SetValue("FTgtOrgId", parentTgtOrgId, num2);
											}
											else
											{
												this.Model.SetValue("FTgtOrgId", dynamicObject4["SupplyOrgId_Id"], num2);
											}
											break;
										}
										case 2:
											this.Model.SetValue("FTgtOrgId", ObjectUtils.IsNullOrEmptyOrWhiteSpace(baseDataControlPolicyPropertyEntry.DefaultValue) ? parentTgtOrgId : OtherExtend.ConvertTo<long>(baseDataControlPolicyPropertyEntry.DefaultValue, 0L), num2);
											break;
										case 3:
											this.Model.SetValue("FTgtOrgId", parentTgtOrgId, num2);
											break;
										}
									}
									else if (num7 == parentTgtOrgId)
									{
										BomExpandView.BomExpandResult bomExpandResult2 = dynamicObject4;
										long num8 = bomExpandResult2.HasControlAttribute(8) ? parentTgtOrgId : OtherExtend.ConvertTo<long>(dynamicObject4["SupplyOrgId_Id"], 0L);
										this.Model.SetValue("FTgtOrgId", num8, num2);
									}
								}
							}
							else
							{
								this.Model.SetValue("FTgtOrgId", dynamicObject3["OrgId_Id"], num2);
							}
							this.Model.SetValue("FKey", string.Format("{0}_{1}", num3, dynamicObject5["tgtOrgId_Id"]), num2);
							dictionary2.Add(text, dynamicObject5);
						}
					}
				}
			}
			DynamicObjectCollection entityDataObject = this.Model.GetEntityDataObject(entity);
			this.ValidateCalResult(entityDataObject, dictionary);
			base.View.UpdateView("FCalResult");
		}

		// Token: 0x060006CD RID: 1741 RVA: 0x00050BE8 File Offset: 0x0004EDE8
		private MemBomExpandOption_ForPSV GetBomQueryOption()
		{
			MetaDataServiceHelper.GetFormMetaData(base.Context, "ENG_BOM");
			MemBomExpandOption_ForPSV memBomExpandOption_ForPSV = new MemBomExpandOption_ForPSV();
			memBomExpandOption_ForPSV.ExpandLevelTo = 0;
			memBomExpandOption_ForPSV.ExpandVirtualMaterial = false;
			memBomExpandOption_ForPSV.DeleteVirtualMaterial = false;
			memBomExpandOption_ForPSV.DeleteSkipRow = false;
			memBomExpandOption_ForPSV.ExpandSkipRow = false;
			memBomExpandOption_ForPSV.IsShowOutSource = false;
			memBomExpandOption_ForPSV.BomExpandId = SequentialGuid.NewGuid().ToString();
			memBomExpandOption_ForPSV.ParentCsdYieldRate = false;
			memBomExpandOption_ForPSV.ChildCsdYieldRate = false;
			memBomExpandOption_ForPSV.CsdSubstitution = true;
			memBomExpandOption_ForPSV.Option.SetVariableValue("IsSimpleExpand", true);
			return memBomExpandOption_ForPSV;
		}

		// Token: 0x060006CE RID: 1742 RVA: 0x00050CE8 File Offset: 0x0004EEE8
		private List<List<DynamicObject>> GetBomTrees(List<DynamicObject> expandResult)
		{
			List<List<DynamicObject>> list = new List<List<DynamicObject>>();
			List<DynamicObject> list2 = (from x in expandResult
			where OtherExtend.ConvertTo<int>(x["BOMLevel"], 0) == 0
			select x).ToList<DynamicObject>();
			Dictionary<int, IGrouping<int, DynamicObject>> dictionary = (from x in expandResult
			group x by OtherExtend.ConvertTo<int>(x["BOMLevel"], 0)).ToDictionary((IGrouping<int, DynamicObject> x) => x.Key);
			int num = expandResult.Max((DynamicObject x) => OtherExtend.ConvertTo<int>(x["BOMLevel"], 0));
			foreach (DynamicObject dynamicObject in list2)
			{
				List<DynamicObject> list3 = new List<DynamicObject>();
				HashSet<string> parentRowIds = new HashSet<string>();
				parentRowIds.Add(OtherExtend.ConvertTo<string>(dynamicObject["RowId"], null));
				list3.Add(dynamicObject);
				for (int i = 1; i <= num; i++)
				{
					IGrouping<int, DynamicObject> source = null;
					if (dictionary.TryGetValue(i, out source))
					{
						List<DynamicObject> list4 = (from x in source
						where parentRowIds.Contains(OtherExtend.ConvertTo<string>(x["ParentEntryId"], null))
						select x).ToList<DynamicObject>();
						if (ListUtils.IsEmpty<DynamicObject>(list4))
						{
							i = num + 1;
							break;
						}
						parentRowIds.Clear();
						foreach (DynamicObject dynamicObject2 in list4)
						{
							if (OtherExtend.ConvertTo<long>(dynamicObject2["BOMID_ID"], 0L) != 0L)
							{
								list3.Add(dynamicObject2);
								parentRowIds.Add(OtherExtend.ConvertTo<string>(dynamicObject2["RowId"], null));
							}
						}
					}
				}
				list.Add(list3);
			}
			return list;
		}

		// Token: 0x060006CF RID: 1743 RVA: 0x00050F3C File Offset: 0x0004F13C
		private List<string> GetAllocatedBOM(List<long> bomMasterIds, long tgtOrgId)
		{
			QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter
			{
				FormId = "ENG_BOM",
				SelectItems = SelectorItemInfo.CreateItems(new string[]
				{
					"FMASTERID",
					"FUseOrgId"
				}),
				FilterClauseWihtKey = string.Format("FUseOrgId=@useOrgId", new object[0])
			};
			queryBuilderParemeter.ExtJoinTables.Add(new ExtJoinTableDescription
			{
				TableName = "table(fn_StrSplit(@msterId,',',1))",
				TableNameAs = "tms",
				FieldName = "FID",
				ScourceKey = "FMASTERID"
			});
			List<SqlParam> list = new List<SqlParam>
			{
				new SqlParam("@msterId", 161, bomMasterIds.Distinct<long>().ToArray<long>()),
				new SqlParam("@useOrgId", 12, tgtOrgId)
			};
			DynamicObjectCollection dynamicObjectCollection = QueryServiceHelper.GetDynamicObjectCollection(base.Context, queryBuilderParemeter, list);
			if (dynamicObjectCollection == null)
			{
				return new List<string>();
			}
			return (from x in dynamicObjectCollection
			select string.Format("{0}_{1}", x["FMASTERID"], x["FUseOrgId"])).ToList<string>();
		}

		// Token: 0x060006D0 RID: 1744 RVA: 0x00051084 File Offset: 0x0004F284
		private List<string> GetAllocatedMtrl(List<long> mtrlMasterIds, long tgtOrgId)
		{
			QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter
			{
				FormId = "BD_MATERIAL",
				SelectItems = SelectorItemInfo.CreateItems(new string[]
				{
					"FMASTERID",
					"FUseOrgId"
				}),
				FilterClauseWihtKey = string.Format("FUseOrgId=@useOrgId", new object[0])
			};
			queryBuilderParemeter.ExtJoinTables.Add(new ExtJoinTableDescription
			{
				TableName = "table(fn_StrSplit(@msterId,',',1))",
				TableNameAs = "tms",
				FieldName = "FID",
				ScourceKey = "FMASTERID"
			});
			List<SqlParam> list = new List<SqlParam>
			{
				new SqlParam("@msterId", 161, mtrlMasterIds.Distinct<long>().ToArray<long>()),
				new SqlParam("@useOrgId", 12, tgtOrgId)
			};
			DynamicObjectCollection dynamicObjectCollection = QueryServiceHelper.GetDynamicObjectCollection(base.Context, queryBuilderParemeter, list);
			if (dynamicObjectCollection == null)
			{
				return new List<string>();
			}
			return (from x in dynamicObjectCollection
			select string.Format("{0}_{1}", x["FMASTERID"], x["FUseOrgId"])).ToList<string>();
		}

		// Token: 0x060006D1 RID: 1745 RVA: 0x000511BC File Offset: 0x0004F3BC
		private List<long> GetAllOrg()
		{
			List<long> result = new List<long>();
			QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter
			{
				FormId = "ORG_Organizations",
				SelectItems = SelectorItemInfo.CreateItems("FOrgId")
			};
			DynamicObjectCollection dynamicObjectCollection = QueryServiceHelper.GetDynamicObjectCollection(base.Context, queryBuilderParemeter, null);
			if (!ListUtils.IsEmpty<DynamicObject>(dynamicObjectCollection))
			{
				result = (from x in dynamicObjectCollection
				select OtherExtend.ConvertTo<long>(x["FOrgId"], 0L)).ToList<long>();
			}
			return result;
		}

		// Token: 0x060006D2 RID: 1746 RVA: 0x00051234 File Offset: 0x0004F434
		private bool ValidateIsAllocatedMtrl(HashSet<string> mtrlKeys, DynamicObject row, long tgtOrgId)
		{
			new List<long>();
			DynamicObject dynamicObject = row["BOMID1"] as DynamicObject;
			List<long> list = new List<long>();
			DynamicObject dynamicObject2 = dynamicObject["MATERIALID"] as DynamicObject;
			list.Add(OtherExtend.ConvertTo<long>(dynamicObject2["msterID"], 0L));
			DynamicObjectCollection dynamicObjectCollection = dynamicObject["TreeEntity"] as DynamicObjectCollection;
			foreach (DynamicObject dynamicObject3 in dynamicObjectCollection)
			{
				DynamicObject dynamicObject4 = dynamicObject3["MATERIALIDCHILD"] as DynamicObject;
				list.Add(OtherExtend.ConvertTo<long>(dynamicObject4["msterID"], 0L));
			}
			DynamicObjectCollection dynamicObjectCollection2 = dynamicObject["EntryBOMCOBY"] as DynamicObjectCollection;
			foreach (DynamicObject dynamicObject5 in dynamicObjectCollection2)
			{
				DynamicObject dynamicObject6 = dynamicObject5["MATERIALIDCOBY"] as DynamicObject;
				list.Add(OtherExtend.ConvertTo<long>(dynamicObject6["msterID"], 0L));
			}
			bool result = true;
			foreach (long num in list)
			{
				string item = string.Format("{0}_{1}", num, tgtOrgId);
				if (!mtrlKeys.Contains(item))
				{
					this._needAllocMtrls.Add(item);
					result = false;
				}
			}
			return result;
		}

		// Token: 0x060006D3 RID: 1747 RVA: 0x000515A0 File Offset: 0x0004F7A0
		private void ValidateCalResult(DynamicObjectCollection entryDatas, Dictionary<long, BaseDataControlPolicy> bomCtrlPolicys)
		{
			HashSet<string> allocatedKeys = new HashSet<string>();
			HashSet<string> allocatedMtrlKeys = new HashSet<string>();
			IEnumerable<IGrouping<long, DynamicObject>> enumerable = from x in entryDatas
			group x by OtherExtend.ConvertTo<long>(x["tgtOrgId_Id"], 0L);
			foreach (IGrouping<long, DynamicObject> grouping in enumerable)
			{
				List<long> bomMasterIds = (from x in grouping
				select OtherExtend.ConvertTo<long>((x["BOMID1"] as DynamicObject)["msterID"], 0L)).ToList<long>();
				this.GetAllocatedBOM(bomMasterIds, grouping.Key).ForEach(delegate(string x)
				{
					allocatedKeys.Add(x);
				});
				List<long> list = new List<long>();
				list.AddRange(grouping.SelectMany(delegate(DynamicObject x)
				{
					DynamicObject dynamicObject2 = x["BOMID1"] as DynamicObject;
					List<long> list2 = new List<long>();
					DynamicObject dynamicObject3 = dynamicObject2["MATERIALID"] as DynamicObject;
					list2.Add(OtherExtend.ConvertTo<long>(dynamicObject3["msterID"], 0L));
					DynamicObjectCollection dynamicObjectCollection = dynamicObject2["TreeEntity"] as DynamicObjectCollection;
					foreach (DynamicObject dynamicObject4 in dynamicObjectCollection)
					{
						DynamicObject dynamicObject5 = dynamicObject4["MATERIALIDCHILD"] as DynamicObject;
						list2.Add(OtherExtend.ConvertTo<long>(dynamicObject5["msterID"], 0L));
					}
					DynamicObjectCollection dynamicObjectCollection2 = dynamicObject2["EntryBOMCOBY"] as DynamicObjectCollection;
					if (!ListUtils.IsEmpty<DynamicObject>(dynamicObjectCollection2))
					{
						foreach (DynamicObject dynamicObject6 in dynamicObjectCollection2)
						{
							DynamicObject dynamicObject7 = dynamicObject6["MATERIALIDCOBY"] as DynamicObject;
							list2.Add(OtherExtend.ConvertTo<long>(dynamicObject7["msterID"], 0L));
						}
					}
					return list2;
				}));
				this.GetAllocatedMtrl(list, grouping.Key).ForEach(delegate(string x)
				{
					allocatedMtrlKeys.Add(x);
				});
			}
			foreach (DynamicObject dynamicObject in entryDatas)
			{
				long num = OtherExtend.ConvertTo<long>(dynamicObject["CreateOrgId_Id"], 0L);
				long tgtOrgId = OtherExtend.ConvertTo<long>(dynamicObject["tgtOrgId_Id"], 0L);
				long num2 = OtherExtend.ConvertTo<long>((dynamicObject["BOMID1"] as DynamicObject)["msterID"], 0L);
				string a = (dynamicObject["BOMID1"] as DynamicObject)["DocumentStatus"].ToString();
				int num3 = entryDatas.IndexOf(dynamicObject);
				BaseDataControlPolicy baseDataControlPolicy = null;
				if (num == tgtOrgId)
				{
					this.Model.SetValue("FAllocStatus", CONST_ENG_BOMBatchAllocate.Enu_AllocStatus.Allocated, num3);
					this.Model.SetValue("FIsFatalErr", false, num3);
				}
				else if (a != "C")
				{
					this.Model.SetValue("FAllocStatus", CONST_ENG_BOMBatchAllocate.Enu_AllocStatus.YetToAudit, num3);
					this.Model.SetValue("FIsFatalErr", true, num3);
				}
				else if (tgtOrgId == 0L || !bomCtrlPolicys.TryGetValue(num, out baseDataControlPolicy) || !baseDataControlPolicy.TargetOrgEntrys.Any((BaseDataControlPolicyTargetOrgEntry x) => x.TargetOrgId == tgtOrgId))
				{
					this.Model.SetValue("FAllocStatus", CONST_ENG_BOMBatchAllocate.Enu_AllocStatus.NoCtrlPolicy, num3);
					this.Model.SetValue("FIsFatalErr", true, num3);
				}
				else if (!this.ValidateIsAllocatedMtrl(allocatedMtrlKeys, dynamicObject, tgtOrgId))
				{
					this.Model.SetValue("FAllocStatus", CONST_ENG_BOMBatchAllocate.Enu_AllocStatus.MtrlYetToAlloc, num3);
					this.Model.SetValue("FIsFatalErr", true, num3);
				}
				else if (allocatedKeys.Contains(string.Format("{0}_{1}", num2, tgtOrgId)))
				{
					this.Model.SetValue("FAllocStatus", CONST_ENG_BOMBatchAllocate.Enu_AllocStatus.Allocated, num3);
					this.Model.SetValue("FIsFatalErr", false, num3);
				}
				else
				{
					this.Model.SetValue("FAllocStatus", CONST_ENG_BOMBatchAllocate.Enu_AllocStatus.CanAlloc, num3);
					this.Model.SetValue("FIsFatalErr", false, num3);
				}
			}
		}

		// Token: 0x060006D4 RID: 1748 RVA: 0x0005199C File Offset: 0x0004FB9C
		private void BuildAllocateList()
		{
			DynamicObjectCollection dynamicObjectCollection = this.Model.DataObject["CalResult"] as DynamicObjectCollection;
			IOrderedEnumerable<IGrouping<string, DynamicObject>> orderedEnumerable = from x in dynamicObjectCollection
			group x by x["AllocSeq"].ToString() into x
			orderby x.Key descending
			select x;
			List<DynamicObject> list = new List<DynamicObject>();
			FormMetadata formMetadata = MetaDataServiceHelper.Load(base.Context, "ENG_BOMAllocateExe", true) as FormMetadata;
			Entity entity = formMetadata.BusinessInfo.GetEntity("FAllocateQueue");
			HashSet<string> hashSet = new HashSet<string>();
			foreach (IGrouping<string, DynamicObject> grouping in orderedEnumerable)
			{
				foreach (DynamicObject dynamicObject in grouping)
				{
					string item = dynamicObject["Key"].ToString();
					if (!hashSet.Contains(item))
					{
						DynamicObject dynamicObject2 = new DynamicObject(entity.DynamicObjectType);
						dynamicObject2["BOMID1"] = dynamicObject["BOMID1"];
						dynamicObject2["BOMID1_Id"] = dynamicObject["BOMID1_Id"];
						dynamicObject2["CreateOrgId1_Id"] = dynamicObject["CreateOrgId_Id"];
						dynamicObject2["CreateOrgId1"] = dynamicObject["CreateOrgId"];
						dynamicObject2["tgtOrgId1"] = dynamicObject["tgtOrgId"];
						dynamicObject2["tgtOrgId1_Id"] = dynamicObject["tgtOrgId_Id"];
						dynamicObject2["Key"] = dynamicObject["Key"];
						dynamicObject2["AllocStatus"] = dynamicObject["AllocStatus"];
						hashSet.Add(item);
						list.Add(dynamicObject2);
					}
				}
			}
			Entity entity2 = formMetadata.BusinessInfo.GetEntity("FResult");
			DynamicObjectCollection dynamicObjectCollection2 = new DynamicObjectCollection(entity2.DynamicObjectType, null);
			foreach (DynamicObject dynamicObject3 in dynamicObjectCollection)
			{
				DynamicObject dynamicObject4 = new DynamicObject(entity2.DynamicObjectType);
				dynamicObject4["ROWID"] = dynamicObject3["RowId"];
				dynamicObject4["PARENTROWID"] = dynamicObject3["ParentRowId"];
				dynamicObject4["AllocSeq"] = dynamicObject3["AllocSeq"];
				dynamicObject4["MaterialId"] = dynamicObject3["MaterialId1"];
				dynamicObject4["MaterialId_Id"] = dynamicObject3["MaterialId1_Id"];
				dynamicObject4["BOMID_Id"] = dynamicObject3["BOMID1_Id"];
				dynamicObject4["BOMID"] = dynamicObject3["BOMID1"];
				dynamicObject4["CreateOrgId"] = dynamicObject3["CreateOrgId"];
				dynamicObject4["CreateOrgId_Id"] = dynamicObject3["CreateOrgId_Id"];
				dynamicObject4["TgtOrgId"] = dynamicObject3["TgtOrgId"];
				dynamicObject4["TgtOrgId_Id"] = dynamicObject3["TgtOrgId_Id"];
				dynamicObject4["Key1"] = dynamicObject3["Key"];
				dynamicObjectCollection2.Add(dynamicObject4);
			}
			DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter
			{
				FormId = "ENG_BOMAllocateExe",
				PageId = string.Format("{0}_{1}", base.View.PageId, "AE"),
				ParentPageId = base.View.PageId
			};
			dynamicFormShowParameter.OpenStyle.ShowType = 3;
			dynamicFormShowParameter.OpenStyle.TagetKey = "FAllocateResult";
			dynamicFormShowParameter.CustomComplexParams.Add("allocQueue", list);
			dynamicFormShowParameter.CustomComplexParams.Add("calResult", dynamicObjectCollection2);
			dynamicFormShowParameter.CustomComplexParams.Add("IsAutoAudit", OtherExtend.ConvertTo<bool>(this.Model.GetValue("FIsAutoAudit"), false));
			base.View.ShowForm(dynamicFormShowParameter);
		}

		// Token: 0x060006D5 RID: 1749 RVA: 0x00051E3C File Offset: 0x0005003C
		private void CalledFromOther()
		{
			object customParameter = base.View.OpenParameter.GetCustomParameter("CurrentBomId");
			if (customParameter == null)
			{
				return;
			}
			this.Model.ClearNoDataRow();
			int num = 0;
			DataEntityExtend.CreateNewEntryRow(this.Model, base.View.BusinessInfo.GetEntity("FBOMScope"), -1, ref num);
			this.Model.SetValue("FBOMID", OtherExtend.ConvertTo<long>(customParameter, 0L), num);
			DynamicObject dynamicObject = this.Model.GetValue("FBOMID", num) as DynamicObject;
			if (dynamicObject == null)
			{
				return;
			}
			long num2 = OtherExtend.ConvertTo<long>(dynamicObject["MATERIALID_Id"], 0L);
			this.Model.SetValue("FMaterialId", num2, num);
		}

		// Token: 0x040002F8 RID: 760
		private Dictionary<string, Action<ButtonClickEventArgs>> _buttonClickedHandles = new Dictionary<string, Action<ButtonClickEventArgs>>();

		// Token: 0x040002F9 RID: 761
		private Dictionary<string, Action<WizardStepChangingEventArgs>> _stepChangingHandles = new Dictionary<string, Action<WizardStepChangingEventArgs>>();

		// Token: 0x040002FA RID: 762
		private Dictionary<string, Action<WizardStepChangedEventArgs>> _stepChangedHandles = new Dictionary<string, Action<WizardStepChangedEventArgs>>();

		// Token: 0x040002FB RID: 763
		protected List<string> _needAllocMtrls = new List<string>();
	}
}
