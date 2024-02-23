using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Kingdee.BOS;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.Core.MFG.Utils;
using Kingdee.K3.MFG.Common.BusinessEntity.SFC.SFCEntity.SFC.Base;
using Kingdee.K3.MFG.ServiceHelper;
using Kingdee.K3.MFG.ServiceHelper.ENG;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.Base
{
	// Token: 0x0200004A RID: 74
	[Description("工作中心表单插件")]
	public class WorkCenterEdit : BaseControlEdit
	{
		// Token: 0x06000502 RID: 1282 RVA: 0x0003D009 File Offset: 0x0003B209
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
			this.InitCustomParameters();
			if (this.workCenterType.Equals("D"))
			{
				this.InitFormTitle(base.View.OpenParameter.Status);
			}
		}

		// Token: 0x06000503 RID: 1283 RVA: 0x0003D040 File Offset: 0x0003B240
		public override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			OperationStatus status = base.View.OpenParameter.Status;
			if (status == null)
			{
				this.SetDefalultSecduleFormula();
			}
			if (this.workCenterType.Equals("D") && status == null)
			{
				base.View.Model.SetValue("FWorkCenterType", "D");
				this.CreateActivityEntry();
			}
		}

		// Token: 0x06000504 RID: 1284 RVA: 0x0003D0A4 File Offset: 0x0003B2A4
		public override void DataChanged(DataChangedEventArgs e)
		{
			base.DataChanged(e);
			DynamicObject value = MFGBillUtil.GetValue<DynamicObject>(base.View.Model, "FUseOrgId", -1, null, null);
			long useOrgID = (value == null) ? 0L : OtherExtend.ConvertTo<long>(value["ID"], 0L);
			string key;
			if ((key = e.Field.Key) != null)
			{
				if (!(key == "FResourceID"))
				{
					if (!(key == "FJoinScheduling"))
					{
						if (!(key == "FBaseActivityID"))
						{
							return;
						}
						EntryEntity entryEntity = base.View.BusinessInfo.GetEntryEntity("FEntityBaseActivity");
						DynamicObjectCollection entityDataObject = this.Model.GetEntityDataObject(entryEntity);
						if (e.OldValue != null && e.NewValue == null)
						{
							MFGBillUtil.SetValue(base.View, "FDefaultValue", entityDataObject[e.Row], null);
							MFGBillUtil.SetValue(base.View, "FTimeUnit", entityDataObject[e.Row], null);
							MFGBillUtil.SetValue(base.View, "FActRepFormula", entityDataObject[e.Row], null);
							return;
						}
						if (e.NewValue != null && e.OldValue != e.NewValue)
						{
							string filter = string.Format("{0}={1}", "FID", OtherExtend.ConvertTo<long>(e.NewValue, 0L));
							this.SetActRelationDefalt(e, this.GetBaseActInfo(filter), useOrgID);
						}
					}
					else
					{
						DynamicObjectCollection dynamicObjectCollection = (DynamicObjectCollection)base.View.Model.DataObject["WorkCenterCapacity"];
						if (OtherExtend.ConvertTo<int>(e.NewValue, 0) == 1)
						{
							for (int i = 0; i < dynamicObjectCollection.Count; i++)
							{
								MFGBillUtil.SetValue(base.View, "FJoinScheduling", dynamicObjectCollection[i], (i != e.Row) ? 0 : 1);
							}
							return;
						}
					}
				}
				else if (e.NewValue != null && e.OldValue != e.NewValue)
				{
					string text = string.Format("{0}={1}", "FID", OtherExtend.ConvertTo<long>(e.NewValue, 0L));
					List<SelectorItemInfo> list = new List<SelectorItemInfo>
					{
						new SelectorItemInfo("FID"),
						new SelectorItemInfo("FNUMBER"),
						new SelectorItemInfo("FRSRCCATEGORY")
					};
					List<DynamicObject> baseBillInfo = MFGServiceHelper.GetBaseBillInfo(base.Context, "ENG_Resource", list, text, "");
					this.SetCapFormDefalt(e, baseBillInfo, useOrgID);
					string filter2 = string.Format(" FDOCUMENTSTATUS='C' AND FFORBIDSTATUS = 'A' AND FISDEFAULTACTIVITY='1' ", new object[0]);
					this.SetActDefalt(e, this.GetBaseActInfo(filter2));
					return;
				}
			}
		}

		// Token: 0x06000505 RID: 1285 RVA: 0x0003D340 File Offset: 0x0003B540
		private void SetCapFormDefalt(DataChangedEventArgs e, List<DynamicObject> lstResInfo, long useOrgID)
		{
			if (lstResInfo.Count == 0)
			{
				return;
			}
			DynamicObject dynamicObject = lstResInfo[0];
			if (((string)dynamicObject["RsrcCategory"]).Equals("10") || ((string)dynamicObject["RsrcCategory"]).Equals("20"))
			{
				string filter = string.Format(" FUSEORGID={0} AND FDOCUMENTSTATUS='C' AND fforbidstatus = 'A' AND ((FFORMULAUSE = '4' AND FISDEFAULT='1') OR (FFORMULAUSE = '5' AND FISDEFAULT='1') OR (FFORMULAUSE = '6' AND FISDEFAULT='1'))", useOrgID);
				List<DynamicObject> formulaInfo = this.GetFormulaInfo(filter);
				foreach (DynamicObject dynamicObject2 in formulaInfo)
				{
					string a;
					if ((a = (string)dynamicObject2["FormulaUse"]) != null)
					{
						if (!(a == "4"))
						{
							if (!(a == "5"))
							{
								if (a == "6")
								{
									base.View.Model.SetValue("FTearDownFormula", dynamicObject2["ID"], e.Row);
								}
							}
							else
							{
								base.View.Model.SetValue("FProcessFormula", dynamicObject2["ID"], e.Row);
							}
						}
						else
						{
							base.View.Model.SetValue("FSetFormula", dynamicObject2["ID"], e.Row);
						}
					}
				}
			}
		}

		// Token: 0x06000506 RID: 1286 RVA: 0x0003D4B0 File Offset: 0x0003B6B0
		private void SetActDefalt(DataChangedEventArgs e, List<DynamicObject> lstBaseActInfo)
		{
			foreach (DynamicObject dynamicObject in lstBaseActInfo)
			{
				string a;
				if ((a = (string)dynamicObject["Phase"]) != null)
				{
					if (!(a == "10"))
					{
						if (!(a == "20"))
						{
							if (a == "30")
							{
								base.View.Model.SetValue("FTearDownActivity", dynamicObject["ID"], e.Row);
							}
						}
						else
						{
							base.View.Model.SetValue("FProcessActivity", dynamicObject["ID"], e.Row);
						}
					}
					else
					{
						base.View.Model.SetValue("FSetActivity", dynamicObject["ID"], e.Row);
					}
				}
			}
		}

		// Token: 0x06000507 RID: 1287 RVA: 0x0003D5B4 File Offset: 0x0003B7B4
		private void SetActFormulaDefalt(DataChangedEventArgs e, long useOrgID, int formulaId)
		{
			string filter = string.Format(" FUSEORGID={0} AND FMASTERID={1} AND FDOCUMENTSTATUS='C' AND fforbidstatus = 'A' AND FFORMULAUSE = '7' ", useOrgID, formulaId);
			List<DynamicObject> formulaInfo = this.GetFormulaInfo(filter);
			if (formulaInfo.Count > 0)
			{
				base.View.Model.SetValue("FActFormula", formulaInfo[0]["ID"], e.Row);
				return;
			}
			base.View.Model.SetValue("FActFormula", null, e.Row);
		}

		// Token: 0x06000508 RID: 1288 RVA: 0x0003D634 File Offset: 0x0003B834
		private void SetActRepFormulaDefalt(DataChangedEventArgs e, long useOrgID, string formulaUse)
		{
			string filter = string.Format(" FUSEORGID={0} AND FDOCUMENTSTATUS='C' AND fforbidstatus = 'A' AND FFORMULAUSE = '{1}' AND FISDEFAULT='1' ", useOrgID, formulaUse);
			List<DynamicObject> formulaInfo = this.GetFormulaInfo(filter);
			if (formulaInfo.Count > 0)
			{
				base.View.Model.SetValue("FActRepFormula", formulaInfo[0]["ID"], e.Row);
				return;
			}
			base.View.Model.SetValue("FActRepFormula", null, e.Row);
		}

		// Token: 0x06000509 RID: 1289 RVA: 0x0003D6B0 File Offset: 0x0003B8B0
		private void SetActRelationDefalt(DataChangedEventArgs e, List<DynamicObject> lstBaseActInfo, long useOrgID)
		{
			if (lstBaseActInfo.Count == 0 || OtherExtend.StringNullTrim(OtherExtend.ConvertTo<string>(lstBaseActInfo[0]["Phase"], null)).Equals(""))
			{
				return;
			}
			string text = OtherExtend.ConvertTo<string>(lstBaseActInfo[0]["Phase"], null);
			string formulaUse = null;
			int formulaId = 0;
			string a;
			if ((a = text) != null)
			{
				if (!(a == "10"))
				{
					if (!(a == "20"))
					{
						if (a == "30")
						{
							formulaUse = "10";
							formulaId = 40388;
							base.View.Model.SetValue("FBaseActType", "B", e.Row);
						}
					}
					else
					{
						formulaUse = "9";
						formulaId = 40387;
						base.View.Model.SetValue("FBaseActType", "A", e.Row);
					}
				}
				else
				{
					formulaUse = "8";
					formulaId = 40386;
					base.View.Model.SetValue("FBaseActType", "B", e.Row);
				}
			}
			this.SetActFormulaDefalt(e, useOrgID, formulaId);
			this.SetActRepFormulaDefalt(e, useOrgID, formulaUse);
		}

		// Token: 0x0600050A RID: 1290 RVA: 0x0003D7D8 File Offset: 0x0003B9D8
		private List<DynamicObject> GetBaseActInfo(string filter)
		{
			List<SelectorItemInfo> list = new List<SelectorItemInfo>
			{
				new SelectorItemInfo("FID"),
				new SelectorItemInfo("FNUMBER"),
				new SelectorItemInfo("FISDEFAULTACTIVITY"),
				new SelectorItemInfo("FPHASE")
			};
			return MFGServiceHelper.GetBaseBillInfo(base.Context, "ENG_BaseActivity", list, filter, "");
		}

		// Token: 0x0600050B RID: 1291 RVA: 0x0003D844 File Offset: 0x0003BA44
		private List<DynamicObject> GetFormulaInfo(string filter)
		{
			List<SelectorItemInfo> list = new List<SelectorItemInfo>
			{
				new SelectorItemInfo("FID"),
				new SelectorItemInfo("FNUMBER"),
				new SelectorItemInfo("FISDEFAULT"),
				new SelectorItemInfo("FFORMULAUSE")
			};
			return MFGServiceHelper.GetBaseBillInfo(base.Context, "ENG_FORMULA", list, filter, "");
		}

		// Token: 0x0600050C RID: 1292 RVA: 0x0003D8B0 File Offset: 0x0003BAB0
		public override void EntryBarItemClick(BarItemClickEventArgs e)
		{
			base.EntryBarItemClick(e);
			string barItemKey;
			if ((barItemKey = e.BarItemKey) != null)
			{
				if (!(barItemKey == "tbAddNewLine") && !(barItemKey == "tbSplitButton_AddNew") && !(barItemKey == "tbInsertNewline"))
				{
					return;
				}
				int entryRowCount = base.View.Model.GetEntryRowCount("FEntityBaseActivity");
				if (entryRowCount > 5)
				{
					e.Cancel = true;
					string text = ResManager.LoadKDString("基本活动最多允许定义6行", "015072000012577", 7, new object[0]);
					base.View.ShowErrMessage(text, "", 0);
				}
			}
		}

		// Token: 0x0600050D RID: 1293 RVA: 0x0003D940 File Offset: 0x0003BB40
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			base.BeforeF7Select(e);
			this.f7Location = e.Row;
			if (e.FieldKey.Equals("FResourceID"))
			{
				e.ListFilterParameter.Filter = base.SqlAppendAnd(e.ListFilterParameter.Filter, " FISCALCCAPACITY = '1' ");
				long num = OtherExtend.ConvertTo<long>(base.View.Model.GetPKValue(), 0L);
				string text;
				if (num > 0L)
				{
					text = string.Format(" NOT EXISTS (SELECT TOP 1 1 FROM t_eng_workcenterdata  tewd LEFT OUTER JOIN  t_eng_workcenter tew ON  tewd.fid = tew.fid WHERE tewd.fresourceid=t0.fid  and tew.fforbidstatus = 'A' and t0.FISCALCCAPACITY = 1  and t0.FISSHARERSC = 0 and tew.fid <> {0} )", num);
				}
				else
				{
					text = " NOT EXISTS (SELECT TOP 1 1 FROM t_eng_workcenterdata  tewd LEFT OUTER JOIN  t_eng_workcenter tew ON  tewd.fid = tew.fid WHERE tewd.fresourceid=t0.fid  and tew.fforbidstatus = 'A' and t0.FISCALCCAPACITY = 1  and t0.FISSHARERSC = 0 ) ";
				}
				e.ListFilterParameter.Filter = base.SqlAppendAnd(e.ListFilterParameter.Filter, text);
			}
			string fieldKey;
			switch (fieldKey = e.FieldKey)
			{
			case "FSetTimeFormula":
			{
				string formulaUse = "1";
				this.SetDefaultformulaFilter(ref e, formulaUse);
				return;
			}
			case "FProcessTimeFormula":
			{
				string formulaUse = "2";
				this.SetDefaultformulaFilter(ref e, formulaUse);
				return;
			}
			case "FTearDownTimeFormula":
			{
				string formulaUse = "3";
				this.SetDefaultformulaFilter(ref e, formulaUse);
				return;
			}
			case "FSetFormula":
			{
				string formulaUse = "4";
				this.SetDefaultformulaFilter(ref e, formulaUse);
				return;
			}
			case "FProcessFormula":
			{
				string formulaUse = "5";
				this.SetDefaultformulaFilter(ref e, formulaUse);
				return;
			}
			case "FTearDownFormula":
			{
				string formulaUse = "6";
				this.SetDefaultformulaFilter(ref e, formulaUse);
				return;
			}
			case "FActFormula":
			{
				string formulaUse = "7";
				this.SetDefaultformulaFilter(ref e, formulaUse);
				return;
			}
			case "FSetActivity":
			{
				string activityPhase = "10";
				this.SetDefaultActivityFilter(ref e, activityPhase);
				return;
			}
			case "FProcessActivity":
			{
				string activityPhase = "20";
				this.SetDefaultActivityFilter(ref e, activityPhase);
				return;
			}
			case "FTearDownActivity":
			{
				string activityPhase = "30";
				this.SetDefaultActivityFilter(ref e, activityPhase);
				return;
			}
			case "FActRepFormula":
			{
				long value = MFGBillUtil.GetValue<long>(base.View.Model, "FBaseActivityID", e.Row, 0L, null);
				if (value <= 0L)
				{
					string text2 = ResManager.LoadKDString("请先选择基本活动！", "015072000012578", 7, new object[0]);
					base.View.ShowErrMessage(text2, "", 0);
					e.Cancel = true;
					return;
				}
				string filter = string.Format("{0}={1}", "FID", value);
				List<DynamicObject> baseActInfo = this.GetBaseActInfo(filter);
				string text3 = null;
				if (baseActInfo.Count > 0)
				{
					text3 = (string)baseActInfo[0]["Phase"];
				}
				string a;
				if ((a = text3) != null)
				{
					string formulaUse;
					if (a == "10")
					{
						formulaUse = "8";
						this.SetDefaultformulaFilter(ref e, formulaUse);
						return;
					}
					if (a == "20")
					{
						formulaUse = "9";
						this.SetDefaultformulaFilter(ref e, formulaUse);
						return;
					}
					if (!(a == "30"))
					{
						return;
					}
					formulaUse = "10";
					this.SetDefaultformulaFilter(ref e, formulaUse);
				}
				break;
			}

				return;
			}
		}

		// Token: 0x0600050E RID: 1294 RVA: 0x0003DC84 File Offset: 0x0003BE84
		public override void AfterF7Select(AfterF7SelectEventArgs e)
		{
			base.AfterF7Select(e);
			string format = ResManager.LoadKDString("当前操作试图从第{0}行活动开始增加{1}行新活动,已超过最大允许6行活动的限制,请重新选择", "015072000012579", 7, new object[0]);
			string fieldKey;
			if ((fieldKey = e.FieldKey) != null)
			{
				if (!(fieldKey == "FBASEACTIVITYID") && !(fieldKey == "FACTFORMULA") && !(fieldKey == "FACTREPFORMULA"))
				{
					return;
				}
				int count = e.SelectRows.Count;
				if (this.f7Location + count > 6)
				{
					base.View.ShowErrMessage(string.Format(format, this.f7Location + 1, count), "", 0);
					e.Cancel = true;
				}
			}
		}

		// Token: 0x0600050F RID: 1295 RVA: 0x0003DD2C File Offset: 0x0003BF2C
		public override void BeforeUpdateValue(BeforeUpdateValueEventArgs e)
		{
			base.BeforeUpdateValue(e);
			if (e.Key.Equals("FResourceID"))
			{
				List<long> list = new List<long>();
				DynamicObject dynamicObject = null;
				if (e.Value is DynamicObject)
				{
					list.Add(DataEntityExtend.GetDynamicObjectItemValue<long>((DynamicObject)e.Value, "Id", 0L));
					dynamicObject = (DynamicObject)e.Value;
				}
				else if (e.Value != null)
				{
					list.Add(Convert.ToInt64(e.Value));
					FormMetadata formMetadata = MetaDataServiceHelper.Load(base.View.Context, "ENG_Resource", true) as FormMetadata;
					dynamicObject = BusinessDataServiceHelper.LoadSingle(base.View.Context, e.Value, formMetadata.BusinessInfo.GetDynamicObjectType(), null);
				}
				DynamicObjectCollection usingWorkCentersUnshared = ENGServiceHelper.GetUsingWorkCentersUnshared(base.Context, list);
				if (usingWorkCentersUnshared != null && usingWorkCentersUnshared.Count<DynamicObject>() >= 1)
				{
					string value = MFGBillUtil.GetValue<string>(base.View.Model, "FNumber", -1, null, null);
					string dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<string>(usingWorkCentersUnshared.First<DynamicObject>(), "FNumber", null);
					if (string.IsNullOrEmpty(value) || !value.Equals(dynamicObjectItemValue))
					{
						string format = ResManager.LoadKDString("{0}资源已被其他工作中心{1}所引用", "015072000001933", 7, new object[0]);
						base.View.ShowNotificationMessage(string.Format(format, DataEntityExtend.GetDynamicObjectItemValue<string>(dynamicObject, "Number", null), dynamicObjectItemValue), "", 0);
						base.View.Model.SetValue(e.Key, null, e.Row);
						e.Cancel = true;
					}
				}
			}
		}

		// Token: 0x06000510 RID: 1296 RVA: 0x0003DEB0 File Offset: 0x0003C0B0
		public override void AfterBindData(EventArgs e)
		{
			base.AfterBindData(e);
			if (base.View.ParentFormView != null)
			{
				Dictionary<string, object> customParameters = base.View.ParentFormView.OpenParameter.GetCustomParameters();
				if (customParameters.Keys.Contains("IsChangeView"))
				{
					this.ChangeView();
				}
			}
		}

		// Token: 0x06000511 RID: 1297 RVA: 0x0003DEFF File Offset: 0x0003C0FF
		public override void AfterDoOperation(AfterDoOperationEventArgs e)
		{
			base.AfterDoOperation(e);
			this.InitFormTitle(base.View.OpenParameter.Status);
		}

		// Token: 0x06000512 RID: 1298 RVA: 0x0003DF20 File Offset: 0x0003C120
		private void ChangeView()
		{
			string layoutId = "";
			if (this.IsNeedChangeView(out layoutId) && !base.View.OpenParameter.IsOutOfTime)
			{
				BillShowParameter billShowParameter = new BillShowParameter();
				this.CopyCustomeParameter(billShowParameter);
				billShowParameter.OpenStyle.ShowType = 8;
				billShowParameter.FormId = base.View.OpenParameter.FormId;
				if (this.workCenterType.Equals("D"))
				{
					billShowParameter.LayoutId = layoutId;
				}
				billShowParameter.ParentPageId = base.View.OpenParameter.ParentPageId;
				billShowParameter.PageId = base.View.PageId;
				billShowParameter.PKey = ((DataEntityExtend.GetDynamicObjectItemValue<long>(this.Model.DataObject, "Id", 0L) == 0L) ? null : this.Model.DataObject["Id"].ToString());
				billShowParameter.Status = base.View.OpenParameter.Status;
				billShowParameter.CustomComplexParams.Add("DataCache", this.Model.DataObject);
				billShowParameter.CustomComplexParams.Add("NeedMapping", true);
				base.View.OpenParameter.IsOutOfTime = true;
				base.View.ShowForm(billShowParameter);
			}
		}

		// Token: 0x06000513 RID: 1299 RVA: 0x0003E068 File Offset: 0x0003C268
		private bool IsNeedChangeView(out string newLayoutId)
		{
			string text = base.View.OpenParameter.LayoutId;
			if (text == null)
			{
				FormMetadata formMetadata = MetaDataServiceHelper.Load(base.Context, "ENG_WorkCenter", true) as FormMetadata;
				text = this.GetLayoutIDByBillType(formMetadata);
			}
			newLayoutId = this.layOutId;
			return !text.Equals(newLayoutId);
		}

		// Token: 0x06000514 RID: 1300 RVA: 0x0003E0F4 File Offset: 0x0003C2F4
		private string GetLayoutIDByBillType(FormMetadata formMetadata)
		{
			string result = string.Empty;
			BillTypeField billTypeField = formMetadata.BusinessInfo.GetFieldList().FirstOrDefault((Field p) => p is BillTypeField) as BillTypeField;
			if (billTypeField != null && billTypeField.BillTypeInfo.Count > 0)
			{
				DynamicObject dynamicObject = billTypeField.GetDefaultBillTypeInfo();
				if (dynamicObject == null)
				{
					List<EnumItem> eItems = billTypeField.BuildEnumList(false);
					if (eItems != null && eItems.Count > 0)
					{
						dynamicObject = billTypeField.BillTypeInfo.Find((DynamicObject p) => p["Id"].Equals(eItems[0].EnumId));
					}
				}
				if (dynamicObject != null)
				{
					DynamicObject dynamicObject2 = dynamicObject["LayoutSolution"] as DynamicObject;
					if (dynamicObject2 != null)
					{
						result = dynamicObject2["Id"].ToString();
					}
				}
			}
			return result;
		}

		// Token: 0x06000515 RID: 1301 RVA: 0x0003E1F8 File Offset: 0x0003C3F8
		private void CopyCustomeParameter(BillShowParameter para)
		{
			Type type = base.View.OpenParameter.GetType();
			PropertyInfo[] properties = type.GetProperties();
			List<string> list = new List<string>
			{
				"pk",
				"billType"
			};
			Dictionary<string, object> customParameters = base.View.OpenParameter.GetCustomParameters();
			if (customParameters != null && customParameters.Count > 0)
			{
				using (Dictionary<string, object>.Enumerator enumerator = customParameters.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						KeyValuePair<string, object> item = enumerator.Current;
						List<string> list2 = list;
						KeyValuePair<string, object> item7 = item;
						if (!list2.Contains(item7.Key))
						{
							KeyValuePair<string, object> item2 = item;
							if (item2.Value != null)
							{
								KeyValuePair<string, object> item3 = item;
								if (item3.Value is string)
								{
									PropertyInfo left = properties.FirstOrDefault(delegate(PropertyInfo p)
									{
										string name = p.Name;
										KeyValuePair<string, object> item6 = item;
										return name.Equals(item6.Key);
									});
									if (!(left != null))
									{
										Dictionary<string, string> customParams = para.CustomParams;
										KeyValuePair<string, object> item4 = item;
										string key = item4.Key;
										KeyValuePair<string, object> item5 = item;
										customParams[key] = Convert.ToString(item5.Value);
									}
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x06000516 RID: 1302 RVA: 0x0003E350 File Offset: 0x0003C550
		public override void BeforeBindData(EventArgs e)
		{
			base.BeforeBindData(e);
			this.InitFormTitle(base.View.OpenParameter.Status);
		}

		// Token: 0x06000517 RID: 1303 RVA: 0x0003E370 File Offset: 0x0003C570
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			base.BarItemClick(e);
			string a;
			if ((a = e.BarItemKey.ToUpper()) != null)
			{
				if (!(a == "TBSHAREDPL"))
				{
					return;
				}
				this.ShowSharedPL(e);
			}
		}

		// Token: 0x06000518 RID: 1304 RVA: 0x0003E3A8 File Offset: 0x0003C5A8
		private void ShowSharedPL(BarItemClickEventArgs e)
		{
			long num = Convert.ToInt64(base.View.Model.GetPKValue());
			if (num <= 0L)
			{
				string text = ResManager.LoadKDString("请先保存当前数据", "015072000014886", 7, new object[0]);
				base.View.ShowErrMessage(text, "", 0);
				return;
			}
			long existSharedPL = SFCSharedPLEntity.Instance.GetExistSharedPL(base.Context, num);
			BillShowParameter billShowParameter = new BillShowParameter();
			billShowParameter.FormId = "SFC_SharedPLConfig";
			billShowParameter.ParentPageId = base.View.PageId;
			if (existSharedPL > 0L)
			{
				billShowParameter.Status = 1;
				billShowParameter.PKey = Convert.ToString(existSharedPL);
			}
			base.View.ShowForm(billShowParameter);
		}

		// Token: 0x06000519 RID: 1305 RVA: 0x0003E454 File Offset: 0x0003C654
		private void SetDefaultformulaFilter(ref BeforeF7SelectEventArgs e, string formulaUse)
		{
			string text = string.Format("{0}={1}", " FFORMULAUSE ", formulaUse);
			e.ListFilterParameter.Filter = base.SqlAppendAnd(e.ListFilterParameter.Filter, text);
		}

		// Token: 0x0600051A RID: 1306 RVA: 0x0003E494 File Offset: 0x0003C694
		private void SetDefaultActivityFilter(ref BeforeF7SelectEventArgs e, string activityPhase)
		{
			string text = string.Format("{0}={1}", " FPHASE ", activityPhase);
			e.ListFilterParameter.Filter = base.SqlAppendAnd(e.ListFilterParameter.Filter, text);
			DynamicObjectCollection dynamicObjectCollection = (DynamicObjectCollection)base.View.Model.DataObject["WorkCenterBaseActivity"];
			List<long> dynamicObjectColumnValues = DataEntityExtend.GetDynamicObjectColumnValues<long>(dynamicObjectCollection, "BaseActivityID_Id");
			string text2 = string.Join<long>(",", dynamicObjectColumnValues);
			if (text2.Length > 0)
			{
				e.ListFilterParameter.Filter = e.ListFilterParameter.Filter + " AND (FID IN (" + text2 + "))";
				return;
			}
			e.ListFilterParameter.Filter = e.ListFilterParameter.Filter + " AND 1=2";
		}

		// Token: 0x0600051B RID: 1307 RVA: 0x0003E560 File Offset: 0x0003C760
		private void SetDefalultSecduleFormula()
		{
			DynamicObject value = MFGBillUtil.GetValue<DynamicObject>(base.View.Model, "FUseOrgId", -1, null, null);
			long num = (value == null) ? 0L : OtherExtend.ConvertTo<long>(value["ID"], 0L);
			string filter = string.Format("FUSEORGID={0} AND FDOCUMENTSTATUS='C' AND fforbidstatus = 'A' AND ((FFORMULAUSE = '1' AND FISDEFAULT='1') OR (FFORMULAUSE = '2' AND FISDEFAULT='1') OR (FFORMULAUSE = '3' AND FISDEFAULT='1'))", num);
			List<DynamicObject> formulaInfo = this.GetFormulaInfo(filter);
			foreach (DynamicObject dynamicObject in formulaInfo)
			{
				string a;
				if ((a = (string)dynamicObject["FormulaUse"]) != null)
				{
					if (!(a == "1"))
					{
						if (!(a == "2"))
						{
							if (a == "3")
							{
								base.View.Model.SetValue("FTearDownTimeFormula", dynamicObject["ID"]);
							}
						}
						else
						{
							base.View.Model.SetValue("FProcessTimeFormula", dynamicObject["ID"]);
						}
					}
					else
					{
						base.View.Model.SetValue("FSetTimeFormula", dynamicObject["ID"]);
					}
				}
			}
			if (formulaInfo.Count == 0)
			{
				base.View.Model.SetValue("FSetTimeFormula", null);
				base.View.Model.SetValue("FProcessTimeFormula", null);
				base.View.Model.SetValue("FTearDownTimeFormula", null);
			}
		}

		// Token: 0x0600051C RID: 1308 RVA: 0x0003E6F4 File Offset: 0x0003C8F4
		private void CreateActivityEntry()
		{
			string filter = string.Format(" FISSYSTEMSET = 1", new object[0]);
			List<DynamicObject> baseActInfo = this.GetBaseActInfo(filter);
			int num = 0;
			this.Model.DeleteEntryData("FEntityBaseActivity");
			Entity entity = this.Model.BusinessInfo.GetEntity("FEntityBaseActivity");
			foreach (DynamicObject dynamicObject in baseActInfo)
			{
				this.Model.CreateNewEntryRow(entity, num);
				base.View.Model.SetValue("FBaseActivityID", dynamicObject["ID"], num);
				num++;
			}
			this.Model.DeleteEntryRow("FEntityBaseActivity", 3);
		}

		// Token: 0x0600051D RID: 1309 RVA: 0x0003E7C4 File Offset: 0x0003C9C4
		private void InitFormTitle(OperationStatus operStatus)
		{
			if (!this.workCenterType.Equals("D"))
			{
				return;
			}
			LocaleValue localeValue;
			if (operStatus == null)
			{
				localeValue = new LocaleValue(ResManager.LoadKDString("柔性产线 - 新增", "015072000011015", 7, new object[0]), base.Context.UserLocale.LCID);
			}
			else if (operStatus == 2)
			{
				localeValue = new LocaleValue(ResManager.LoadKDString("柔性产线 - 修改", "015072000011016", 7, new object[0]), base.Context.UserLocale.LCID);
			}
			else
			{
				localeValue = new LocaleValue(ResManager.LoadKDString("柔性产线 - 查看", "015072000011017", 7, new object[0]), base.Context.UserLocale.LCID);
			}
			base.View.SetFormTitle(localeValue);
			base.View.SetInnerTitle(localeValue);
		}

		// Token: 0x0600051E RID: 1310 RVA: 0x0003E890 File Offset: 0x0003CA90
		private void InitCustomParameters()
		{
			if (base.View.OpenParameter != null)
			{
				Dictionary<string, object> customParameters = base.View.OpenParameter.GetCustomParameters();
				if (customParameters.ContainsKey("WorkCenterType"))
				{
					this.workCenterType = customParameters["WorkCenterType"].ToString();
				}
				else if (base.View.ParentFormView != null)
				{
					Dictionary<string, object> customParameters2 = base.View.ParentFormView.OpenParameter.GetCustomParameters();
					if (customParameters2.ContainsKey("WorkCenterType"))
					{
						this.workCenterType = customParameters2["WorkCenterType"].ToString();
					}
					customParameters.Add("WorkCenterType", this.workCenterType);
				}
				if (this.workCenterType.Equals("D") && !customParameters.ContainsKey("LayoutId"))
				{
					customParameters.Add("LayoutId", this.layOutId);
				}
			}
		}

		// Token: 0x04000237 RID: 567
		private const int prepareFormulaId = 40386;

		// Token: 0x04000238 RID: 568
		private const int processFormulaId = 40387;

		// Token: 0x04000239 RID: 569
		private const int removeFormulaId = 40388;

		// Token: 0x0400023A RID: 570
		private int f7Location;

		// Token: 0x0400023B RID: 571
		private string workCenterType = "A";

		// Token: 0x0400023C RID: 572
		private string layOutId = "a40c32c9-c663-4389-aedd-99bbe73792a0";
	}
}
