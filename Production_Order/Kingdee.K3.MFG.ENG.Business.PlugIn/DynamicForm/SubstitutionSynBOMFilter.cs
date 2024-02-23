using System;
using System.Collections.Generic;
using System.Text;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.CommonFilter.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.MFG.Utils;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm
{
	// Token: 0x020000B5 RID: 181
	public class SubstitutionSynBOMFilter : AbstractCommonFilterPlugIn
	{
		// Token: 0x06000D2C RID: 3372 RVA: 0x0009B312 File Offset: 0x00099512
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
			(this.Model as ICommonFilterModelService).FormId = "ENG_SynBOMFilter";
		}

		// Token: 0x06000D2D RID: 3373 RVA: 0x0009B330 File Offset: 0x00099530
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			base.BeforeF7Select(e);
			string format = string.Empty;
			Tuple<long, List<long>, List<long>, List<long>, List<long>> tuple = MFGBillUtil.GetParentFormSession<object>(this.View, "FormInputParam") as Tuple<long, List<long>, List<long>, List<long>, List<long>>;
			if (ObjectUtils.IsNullOrEmptyOrWhiteSpace(tuple.Item1))
			{
				this.View.ShowMessage(ResManager.LoadKDString("请先设置替代方案其他页签的使用组织！", "015072000037254", 7, new object[0]), 0);
				e.Cancel = true;
				return;
			}
			string fieldKey;
			switch (fieldKey = e.FieldKey)
			{
			case "FBomId":
			case "FMtrlFrom":
			case "FMtrlTo":
			case "FParentMtrl":
			{
				string str = StringUtils.EqualsIgnoreCase(e.FieldKey, "FBomId") ? string.Format("FID IN({0})", string.Join<long>(",", tuple.Item2)) : string.Format("FMATERIALID IN({0})", string.Join<long>(",", tuple.Item3));
				if (e.FieldKey == "FMtrlFrom" || e.FieldKey == "FMtrlTo" || e.FieldKey == "FParentMtrl")
				{
					format = str + "AND FUSEORGID ={0} AND FFORBIDSTATUS='A' {1}";
				}
				else
				{
					format = str + "AND FUSEORGID ={0} AND FCREATEORGID=FUSEORGID AND FFORBIDSTATUS='A' {1}";
				}
				if (ObjectUtils.IsNullOrEmptyOrWhiteSpace(e.ListFilterParameter.Filter))
				{
					e.ListFilterParameter.Filter = string.Format(format, tuple.Item1, "");
				}
				else
				{
					e.ListFilterParameter.Filter = string.Format(format, tuple.Item1, " AND " + e.ListFilterParameter.Filter);
				}
				e.IsShowApproved = !StringUtils.EqualsIgnoreCase(e.FieldKey, "FBomId");
				return;
			}
			case "FCreatorId":
			case "FApproverId":
			{
				string format2 = " FUSERID IN({0}) {1}";
				if (StringUtils.EqualsIgnoreCase(e.FieldKey, "FCreatorId") && ListUtils.IsEmpty<long>(tuple.Item4))
				{
					return;
				}
				if (StringUtils.EqualsIgnoreCase(e.FieldKey, "FApproverId") && ListUtils.IsEmpty<long>(tuple.Item5))
				{
					return;
				}
				if (ObjectUtils.IsNullOrEmptyOrWhiteSpace(e.ListFilterParameter.Filter))
				{
					e.ListFilterParameter.Filter = string.Format(format2, StringUtils.EqualsIgnoreCase(e.FieldKey, "FCreatorId") ? string.Join<long>(",", tuple.Item4) : string.Join<long>(",", tuple.Item5), "");
					return;
				}
				e.ListFilterParameter.Filter = string.Format(format2, StringUtils.EqualsIgnoreCase(e.FieldKey, "FCreatorId") ? string.Join<long>(",", tuple.Item4) : string.Join<long>(",", tuple.Item5), " AND " + e.ListFilterParameter.Filter);
				break;
			}

				return;
			}
		}

		// Token: 0x06000D2E RID: 3374 RVA: 0x0009B658 File Offset: 0x00099858
		public override void AuthPermissionBeforeF7Select(AuthPermissionBeforeF7SelectEventArgs e)
		{
			base.AuthPermissionBeforeF7Select(e);
			string fieldKey;
			if ((fieldKey = e.FieldKey) != null)
			{
				if (!(fieldKey == "FMtrlFrom") && !(fieldKey == "FMtrlTo") && !(fieldKey == "FParentMtrl"))
				{
					return;
				}
				e.IsIsolationOrg = false;
			}
		}

		// Token: 0x06000D2F RID: 3375 RVA: 0x0009B6A8 File Offset: 0x000998A8
		public override void BeforeUpdateValue(BeforeUpdateValueEventArgs e)
		{
			base.BeforeUpdateValue(e);
			StringBuilder stringBuilder = new StringBuilder();
			string key;
			if ((key = e.Key) != null)
			{
				if (!(key == "FMtrlFrom") && !(key == "FMtrlTo"))
				{
					return;
				}
				if (!this.Validator("FMtrlFrom", "FMtrlTo", e, true))
				{
					e.Cancel = true;
					stringBuilder.AppendLine(ResManager.LoadKDString("起始父项物料编码必须小于或等于结束父项物料编码！", "015072000037255", 7, new object[0]));
					this.View.ShowErrMessage(stringBuilder.ToString(), "", 0);
				}
			}
		}

		// Token: 0x06000D30 RID: 3376 RVA: 0x0009B738 File Offset: 0x00099938
		public override void DataChanged(DataChangedEventArgs e)
		{
			base.DataChanged(e);
			string key;
			if ((key = e.Field.Key) != null)
			{
				if (!(key == "FMtrlFrom") && !(key == "FMtrlTo"))
				{
					return;
				}
				this.CarryValue("FMtrlFrom", "FMtrlTo");
			}
		}

		// Token: 0x06000D31 RID: 3377 RVA: 0x0009B788 File Offset: 0x00099988
		public override void ButtonClick(ButtonClickEventArgs e)
		{
			base.ButtonClick(e);
			string a;
			if ((a = e.Key.ToUpperInvariant()) != null)
			{
				if (!(a == "FBTNOK"))
				{
					return;
				}
				string empty = string.Empty;
				if (!this.DateValidate("FBeginCreateDate", "FEndCreateDate", out empty))
				{
					this.View.ShowErrMessage(empty, "", 0);
					e.Cancel = true;
				}
			}
		}

		// Token: 0x06000D32 RID: 3378 RVA: 0x0009B7EC File Offset: 0x000999EC
		private void CarryValue(string SFiledKey, string EFiledKey)
		{
			string value = MFGBillUtil.GetValue<string>(this.View.Model, SFiledKey, -1, null, null);
			string value2 = MFGBillUtil.GetValue<string>(this.View.Model, EFiledKey, -1, null, null);
			if (ObjectUtils.IsNullOrEmptyOrWhiteSpace(value) && !ObjectUtils.IsNullOrEmptyOrWhiteSpace(value2))
			{
				this.View.Model.SetValue(SFiledKey, value2);
			}
			if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(value) && ObjectUtils.IsNullOrEmptyOrWhiteSpace(value2))
			{
				this.View.Model.SetValue(EFiledKey, value);
			}
		}

		// Token: 0x06000D33 RID: 3379 RVA: 0x0009B868 File Offset: 0x00099A68
		private bool Validator(string SFiledKey, string EFiledKey, BeforeUpdateValueEventArgs e, bool valNumber = false)
		{
			bool result = true;
			string text = string.Empty;
			string text2 = string.Empty;
			if (ObjectUtils.IsNullOrEmpty(e.Value))
			{
				return true;
			}
			Type type = e.Value.GetType();
			if (SFiledKey == e.Key)
			{
				if (type.Name == "DynamicObject")
				{
					if (valNumber)
					{
						text = Convert.ToString(((DynamicObject)e.Value)["Number"]);
					}
					else
					{
						text = Convert.ToString(((DynamicObject)e.Value)["Id"]);
					}
				}
				else
				{
					text = Convert.ToString(e.Value);
				}
				if (type.Name == "DynamicObject" && valNumber)
				{
					DynamicObject value = MFGBillUtil.GetValue<DynamicObject>(this.View.Model, EFiledKey, -1, null, null);
					if (ObjectUtils.IsNullOrEmpty(value))
					{
						text2 = string.Empty;
					}
					else
					{
						text2 = Convert.ToString(value["Number"]);
					}
				}
				else
				{
					text2 = MFGBillUtil.GetValue<string>(this.View.Model, EFiledKey, -1, null, null);
				}
			}
			else
			{
				if (!(EFiledKey == e.Key))
				{
					return result;
				}
				if (type.Name == "DynamicObject")
				{
					if (valNumber)
					{
						text2 = Convert.ToString(((DynamicObject)e.Value)["Number"]);
					}
					else
					{
						text2 = Convert.ToString(((DynamicObject)e.Value)["Id"]);
					}
				}
				else
				{
					text2 = Convert.ToString(e.Value);
				}
				if (type.Name == "DynamicObject" && valNumber)
				{
					DynamicObject value2 = MFGBillUtil.GetValue<DynamicObject>(this.View.Model, EFiledKey, -1, null, null);
					if (ObjectUtils.IsNullOrEmpty(value2))
					{
						text = string.Empty;
					}
					else
					{
						text = Convert.ToString(value2["Number"]);
					}
				}
				else
				{
					text = MFGBillUtil.GetValue<string>(this.View.Model, SFiledKey, -1, null, null);
				}
			}
			if (ObjectUtils.IsNullOrEmptyOrWhiteSpace(text) || ObjectUtils.IsNullOrEmptyOrWhiteSpace(text2))
			{
				return result;
			}
			if (string.Compare(text, text2) > 0)
			{
				result = false;
			}
			return result;
		}

		// Token: 0x06000D34 RID: 3380 RVA: 0x0009BA74 File Offset: 0x00099C74
		protected bool DateValidate(string sField, string eField, out string errorMsg)
		{
			bool result = true;
			StringBuilder stringBuilder = new StringBuilder();
			DateTime value = MFGBillUtil.GetValue<DateTime>(this.View.Model, sField, -1, default(DateTime), null);
			DateTime value2 = MFGBillUtil.GetValue<DateTime>(this.View.Model, eField, -1, default(DateTime), null);
			if (value2 != DateTime.MinValue && DateTime.Compare(value, value2) > 0)
			{
				stringBuilder.AppendLine(ResManager.LoadKDString("创建开始日期必须小于或等于结束日期。", "0151515153499000014057", 7, new object[0]));
				result = false;
			}
			errorMsg = stringBuilder.ToString();
			return result;
		}
	}
}
