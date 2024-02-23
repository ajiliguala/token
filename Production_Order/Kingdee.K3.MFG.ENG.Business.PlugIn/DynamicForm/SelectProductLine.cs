using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.MFG.ServiceHelper;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm
{
	// Token: 0x020000B2 RID: 178
	[Description("选择生产线_表单插件")]
	public class SelectProductLine : AbstractDynamicFormPlugIn
	{
		// Token: 0x06000CEF RID: 3311 RVA: 0x00099651 File Offset: 0x00097851
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
		}

		// Token: 0x06000CF0 RID: 3312 RVA: 0x0009965A File Offset: 0x0009785A
		public override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			this.InitData();
		}

		// Token: 0x06000CF1 RID: 3313 RVA: 0x000997D8 File Offset: 0x000979D8
		public override void ButtonClick(ButtonClickEventArgs e)
		{
			base.ButtonClick(e);
			string a;
			if ((a = e.Key.ToUpper()) != null)
			{
				if (!(a == "FCONFIRM"))
				{
					if (!(a == "FCANCEL"))
					{
						return;
					}
					this.View.Close();
				}
				else
				{
					if (this.ValidataData())
					{
						DynamicObjectCollection dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<DynamicObjectCollection>(this.View.Model.DataObject, "Entity", null);
						List<DynamicObject> source = (from o in dynamicObjectItemValue
						where DataEntityExtend.GetDynamicObjectItemValue<bool>(o, "Selected", false)
						select o).ToList<DynamicObject>();
						List<DynamicObject> list = (from c in source
						from o in this.lstProductLine
						where Convert.ToInt64(c["ProductLineId"]) == Convert.ToInt64(o["Id"])
						select o).ToList<DynamicObject>();
						this.View.ReturnToParentWindow(list);
						this.View.Close();
						return;
					}
					e.Cancel = true;
					return;
				}
			}
		}

		// Token: 0x06000CF2 RID: 3314 RVA: 0x00099940 File Offset: 0x00097B40
		private void InitData()
		{
			if (this.View.ParentFormView == null)
			{
				return;
			}
			long useOrgId = OtherExtend.ConvertTo<long>(this.View.ParentFormView.Session["UseOrgId"], 0L);
			List<long> lstPrdLineIds = OtherExtend.ConvertTo<List<long>>(this.View.ParentFormView.Session["ProductLineIds"], null);
			this.lstProductLine = this.GetProductLineCollection(useOrgId, lstPrdLineIds);
			if (this.lstProductLine == null || this.lstProductLine.Count == 0)
			{
				return;
			}
			Entity entity = this.View.BusinessInfo.GetEntity("FEntity");
			this.Model.CreateNewData();
			int num = 0;
			foreach (DynamicObject dynamicObject in from o in this.lstProductLine
			orderby Convert.ToInt64(o["DeptID_Id"]), o["Number"].ToString()
			select o)
			{
				DynamicObject dynamicObject2 = new DynamicObject(entity.DynamicObjectType);
				DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "Seq", num + 1);
				DynamicObject dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<DynamicObject>(dynamicObject, "DeptID", null);
				DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "DepartId", Convert.ToInt64(dynamicObjectItemValue["Id"]));
				DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "DepartNumber", dynamicObjectItemValue["Number"]);
				LocaleValue localeValue = dynamicObjectItemValue["Name"] as LocaleValue;
				DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "DepartName", localeValue[base.Context.UserLocale.LCID]);
				DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "ProductLineId", Convert.ToInt64(dynamicObject["Id"]));
				DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "ProductLineNumber", dynamicObject["Number"].ToString());
				localeValue = (dynamicObject["Name"] as LocaleValue);
				DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "ProductLineName", localeValue[base.Context.UserLocale.LCID]);
				this.Model.CreateNewEntryRow(entity, num, dynamicObject2);
				num++;
			}
		}

		// Token: 0x06000CF3 RID: 3315 RVA: 0x00099BA4 File Offset: 0x00097DA4
		private List<DynamicObject> GetProductLineCollection(long useOrgId, List<long> lstPrdLineIds)
		{
			if (useOrgId == 0L)
			{
				return null;
			}
			string text = string.Format(" FUseOrgId = {0} and FDOCUMENTSTATUS = 'C' and FFORBIDSTATUS = 'A' and FWorkCenterType in ('B','C')", useOrgId);
			if (lstPrdLineIds != null && lstPrdLineIds.Count > 0)
			{
				text += string.Format(" and FId not in ({0})", string.Join<long>(",", lstPrdLineIds));
			}
			List<SelectorItemInfo> list = new List<SelectorItemInfo>
			{
				new SelectorItemInfo("FDeptID"),
				new SelectorItemInfo("FId"),
				new SelectorItemInfo("Fnumber"),
				new SelectorItemInfo("FName")
			};
			return MFGServiceHelper.GetBaseBillInfo(base.Context, "ENG_ProductLineF8", list, text, "");
		}

		// Token: 0x06000CF4 RID: 3316 RVA: 0x00099C60 File Offset: 0x00097E60
		private bool ValidataData()
		{
			DynamicObject dataObject = this.View.Model.DataObject;
			DynamicObjectCollection dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<DynamicObjectCollection>(dataObject, "Entity", null);
			if (dynamicObjectItemValue.FirstOrDefault((DynamicObject o) => DataEntityExtend.GetDynamicObjectItemValue<bool>(o, "Selected", false)) == null)
			{
				this.View.ShowErrMessage("", this.CONSTMSG, 0);
				return false;
			}
			return true;
		}

		// Token: 0x06000CF5 RID: 3317 RVA: 0x00099CCC File Offset: 0x00097ECC
		public override void EntryCellFocued(EntryCellFocuedEventArgs e)
		{
			base.EntryCellFocued(e);
			if (e.NewRow < 0)
			{
				return;
			}
			if (!StringUtils.EqualsIgnoreCase(e.NewFieldKey.ToString(), "FSelected"))
			{
				for (int i = 0; i < this.lstProductLine.Count; i++)
				{
					this.Model.SetValue("FSelected", false, i);
				}
				this.Model.SetValue("FSelected", true, e.NewRow);
			}
		}

		// Token: 0x040005E3 RID: 1507
		private string CONSTMSG = ResManager.LoadKDString("必须至少选择一条生产线!", "015072000017275", 7, new object[0]);

		// Token: 0x040005E4 RID: 1508
		private List<DynamicObject> lstProductLine = new List<DynamicObject>();
	}
}
