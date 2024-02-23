using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.MFG.Utils;
using Kingdee.K3.MFG.ENG.Business.PlugIn.Base;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm
{
	// Token: 0x0200006D RID: 109
	[Description("BOM批量维护子项修改内容界面插件")]
	public class BomBatchEditChildEdit : BOMEdit
	{
		// Token: 0x17000046 RID: 70
		// (get) Token: 0x060007F9 RID: 2041 RVA: 0x0005E11C File Offset: 0x0005C31C
		// (set) Token: 0x060007FA RID: 2042 RVA: 0x0005E124 File Offset: 0x0005C324
		private IDynamicFormView childView { get; set; }

		// Token: 0x060007FB RID: 2043 RVA: 0x0005E130 File Offset: 0x0005C330
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
			this._EditType = Convert.ToString(e.Paramter.GetCustomParameter("EditType"));
			this._UseOrgId = Convert.ToInt64(e.Paramter.GetCustomParameter("UseOrgId"));
			this._PageId = Convert.ToString(e.Paramter.GetCustomParameter("EditPageId"));
			this.childView = base.View.GetView(this._PageId);
			object obj = null;
			object obj2 = null;
			if (base.View.ParentFormView != null)
			{
				Dictionary<string, object> session = base.View.ParentFormView.Session;
				session.TryGetValue("CanEditFieldKeys", out obj);
				session.TryGetValue("CanAppendFieldKeys", out obj2);
				if (obj != null)
				{
					this._CanEditFieldKeys = (List<string>)obj;
				}
				if (obj2 != null)
				{
					this._CanAppendFieldKeys = (List<string>)obj2;
				}
			}
		}

		// Token: 0x060007FC RID: 2044 RVA: 0x0005E208 File Offset: 0x0005C408
		public override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			base.View.Model.SetValue("FCreateOrgId", this._UseOrgId);
			base.View.Model.SetValue("FUseOrgId", this._UseOrgId);
		}

		// Token: 0x060007FD RID: 2045 RVA: 0x0005E25C File Offset: 0x0005C45C
		public override void AfterBindData(EventArgs e)
		{
			base.AfterBindData(e);
			this.HideBomChildTitle();
			this.LockBomChildEntry();
			this.LockEntryBarItems();
		}

		// Token: 0x060007FE RID: 2046 RVA: 0x0005E278 File Offset: 0x0005C478
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			string a;
			if ((a = e.FieldKey.ToUpper()) != null)
			{
				if (a == "FDEFAULTSTOCK")
				{
					base.SetDefaultStockFilter(ref e);
					return;
				}
				if (a == "FMATERIALIDCHILD")
				{
					e.ListFilterParameter.Filter = base.SqlAppendAnd(e.ListFilterParameter.Filter, this.SetChildMaterilIdFilterStringByEditType(e.Row));
					return;
				}
				if (a == "FMATERIALID")
				{
					string text = base.ParentMaterilIdFilterString();
					e.ListFilterParameter.Filter = base.SqlAppendAnd(e.ListFilterParameter.Filter, text);
					text = base.GetChildMaterialIdFilter();
					e.ListFilterParameter.Filter = base.SqlAppendAnd(e.ListFilterParameter.Filter, text);
					return;
				}
				if (a == "FBOMID")
				{
					e.DynamicFormShowParameter.MultiSelect = false;
					e.IsShowApproved = false;
					e.ListFilterParameter.Filter = base.GetBomId2Filter(e.ListFilterParameter.Filter, e.Row);
					return;
				}
				if (!(a == "FCHILDSUPPLYORGID"))
				{
					return;
				}
				e.ListFilterParameter.Filter = base.GetChildSupplyOrgFilter(e.ListFilterParameter.Filter, e.Row);
			}
		}

		// Token: 0x060007FF RID: 2047 RVA: 0x0005E3B8 File Offset: 0x0005C5B8
		public override void BeforeSetItemValueByNumber(BeforeSetItemValueByNumberArgs e)
		{
			string a;
			if ((a = e.BaseDataFieldKey.ToUpper()) != null)
			{
				if (a == "FDEFAULTSTOCK")
				{
					base.SetDefaultStockFilter(ref e);
					return;
				}
				if (a == "FMATERIALIDCHILD")
				{
					e.Filter = base.SqlAppendAnd(e.Filter, this.SetChildMaterilIdFilterStringByEditType(e.Row));
					return;
				}
				if (a == "FMATERIALID")
				{
					string text = base.ParentMaterilIdFilterString();
					e.Filter = base.SqlAppendAnd(e.Filter, text);
					text = base.GetChildMaterialIdFilter();
					e.Filter = base.SqlAppendAnd(e.Filter, text);
					return;
				}
				if (a == "FBOMID")
				{
					e.IsShowApproved = false;
					e.Filter = base.GetBomId2Filter(e.Filter, e.Row);
					return;
				}
				if (!(a == "FCHILDSUPPLYORGID"))
				{
					return;
				}
				e.Filter = base.GetChildSupplyOrgFilter(e.Filter, e.Row);
			}
		}

		// Token: 0x06000800 RID: 2048 RVA: 0x0005E4B4 File Offset: 0x0005C6B4
		public override void DataChanged(DataChangedEventArgs e)
		{
			base.DataChanged(e);
			string key;
			if ((key = e.Field.Key) != null)
			{
				if (key == "FEditMaterialId")
				{
					if (this._EditType == "d")
					{
						base.View.Model.SetValue("FMATERIALIDCHILD", e.NewValue, 0);
						base.View.InvokeFieldUpdateService("FMATERIALIDCHILD", 0);
					}
					if (this._EditType == "b")
					{
						base.View.Model.DeleteEntryData("FTreeEntity");
						base.View.UpdateView("FTreeEntity");
					}
					if (base.View.ParentFormView.Session.ContainsKey("EditMaterialId"))
					{
						this.childView.ParentFormView.Session.Remove("EditMaterialId");
					}
					if (ObjectUtils.IsNullOrEmpty(this.childView))
					{
						this.childView = base.View.GetView(this._PageId);
					}
					this.childView.ParentFormView.Session.Add("EditMaterialId", e.NewValue);
					this.childView.UpdateViewState();
					return;
				}
				if (!(key == "FMATERIALIDCHILD"))
				{
					return;
				}
				if (this._EditType == "a" || this._EditType == "b")
				{
					MFGBillUtil.SetEffectDate(base.View, "FEFFECTDATE", e.Row, this._UseOrgId);
				}
			}
		}

		// Token: 0x06000801 RID: 2049 RVA: 0x0005E638 File Offset: 0x0005C838
		private void HideBomChildTitle()
		{
			if (this._EditType == "a" || this._EditType == "c")
			{
				FieldEditor control = base.View.GetControl<FieldEditor>("FEditMaterialId");
				if (control != null)
				{
					control.Visible = false;
				}
				SplitContainer splitContainer = (SplitContainer)base.View.GetControl("FSpliteContainer");
				splitContainer.HideFirstPanel(true);
			}
		}

		// Token: 0x06000802 RID: 2050 RVA: 0x0005E7F4 File Offset: 0x0005C9F4
		private void LockBomChildEntry()
		{
			if (this._EditType != "d" && this._EditType != "c" && this._EditType != "b")
			{
				return;
			}
			object obj = null;
			object obj2 = null;
			if (base.View.ParentFormView != null)
			{
				Dictionary<string, object> session = base.View.ParentFormView.Session;
				session.TryGetValue("CanEditFieldKeys", out obj);
				session.TryGetValue("CanAppendFieldKeys", out obj2);
				if (obj != null)
				{
					this._CanEditFieldKeys = (List<string>)obj;
				}
				if (obj2 != null)
				{
					this._CanAppendFieldKeys = (List<string>)obj2;
				}
			}
			List<Field> fields = base.View.BillBusinessInfo.GetEntity("FTreeEntity").Fields;
			List<Field> fields2 = base.View.BillBusinessInfo.GetEntity("FBOMCHILDLOTBASEDQTY").Fields;
			List<string> list = new List<string>();
			List<string> list2 = new List<string>();
			List<string> list3 = new List<string>();
			List<string> list4 = new List<string>();
			if (this._EditType == "d" || this._EditType == "b")
			{
				list = (from c in this._CanEditFieldKeys
				where c.Split(new char[]
				{
					'*'
				}).First<string>() == "FTreeEntity"
				select c.Split(new char[]
				{
					'*'
				})[1]).ToList<string>();
				list3 = (from c in this._CanEditFieldKeys
				where c.Split(new char[]
				{
					'*'
				}).First<string>() == "FBOMCHILDLOTBASEDQTY"
				select c.Split(new char[]
				{
					'*'
				})[1]).ToList<string>();
				list2 = (from c in this._CanAppendFieldKeys
				where c.Split(new char[]
				{
					'*'
				}).First<string>() == "FTreeEntity"
				select c.Split(new char[]
				{
					'*'
				})[1]).ToList<string>();
				list4 = (from c in this._CanAppendFieldKeys
				where c.Split(new char[]
				{
					'*'
				}).First<string>() == "FBOMCHILDLOTBASEDQTY"
				select c.Split(new char[]
				{
					'*'
				})[1]).ToList<string>();
			}
			if (this._EditType != "d")
			{
				list.Add("FMATERIALIDCHILD");
			}
			foreach (Field field in fields)
			{
				if (!list.Contains(field.Key) && !list2.Contains(field.Key))
				{
					base.View.LockField(field.Key, false);
				}
			}
			if (this._EditType == "b")
			{
				foreach (Field field2 in fields2)
				{
					if (!list3.Contains(field2.Key) && !list4.Contains(field2.Key))
					{
						base.View.LockField(field2.Key, false);
					}
				}
			}
			if (this._EditType == "c")
			{
				base.View.LockField("FMEMO", true);
				base.View.LockField("FAuxPropId", true);
			}
		}

		// Token: 0x06000803 RID: 2051 RVA: 0x0005EB94 File Offset: 0x0005CD94
		private void LockEntryBarItems()
		{
			string editType;
			if ((editType = this._EditType) != null)
			{
				if (editType == "a")
				{
					base.View.LockField("FEditMaterialId", false);
					return;
				}
				if (!(editType == "b"))
				{
					if (editType == "c")
					{
						base.View.LockField("FEditMaterialId", false);
						return;
					}
					if (!(editType == "d"))
					{
						return;
					}
					base.View.GetBarItem<BarItemControl>("FTreeEntity", "tbNewEntry").Enabled = false;
					base.View.GetBarItem<BarItemControl>("FTreeEntity", "tbDeleteEntry").Enabled = false;
				}
			}
		}

		// Token: 0x06000804 RID: 2052 RVA: 0x0005EC40 File Offset: 0x0005CE40
		private string SetChildMaterilIdFilterStringByEditType(int iRow)
		{
			long value = MFGBillUtil.GetValue<long>(base.View.Model, "FEditMaterialId", -1, 0L, null);
			if (this._EditType != "b" || value <= 0L)
			{
				return string.Empty;
			}
			return string.Format(" FMATERIALID <> {0} ", value);
		}

		// Token: 0x040003A5 RID: 933
		private string _EditType = string.Empty;

		// Token: 0x040003A6 RID: 934
		private long _UseOrgId;

		// Token: 0x040003A7 RID: 935
		private List<string> _CanEditFieldKeys = new List<string>();

		// Token: 0x040003A8 RID: 936
		private List<string> _CanAppendFieldKeys = new List<string>();

		// Token: 0x040003A9 RID: 937
		private string _PageId = string.Empty;
	}
}
