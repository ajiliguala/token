using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Orm.Metadata.DataEntity;
using Kingdee.BOS.Util;
using Kingdee.K3.MFG.Common.BusinessEntity.SFC.SFCDymObjManager;
using Kingdee.K3.MFG.Common.BusinessEntity.SFC.SFCEntity;
using Kingdee.K3.MFG.ServiceHelper.SFS;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm
{
	// Token: 0x020000AC RID: 172
	[HotUpdate]
	[Description("PLM文件选择列表（动态表单）")]
	public class PLMDocumentSelectDF : AbstractDynamicFormPlugIn
	{
		// Token: 0x06000BFC RID: 3068 RVA: 0x0008983C File Offset: 0x00087A3C
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
			this.dicMaterialId = (this.View.OpenParameter.GetCustomParameter("dicMaterialId") as Dictionary<long, string>);
		}

		// Token: 0x06000BFD RID: 3069 RVA: 0x00089865 File Offset: 0x00087A65
		public override void AfterBindData(EventArgs e)
		{
			base.AfterBindData(e);
			this.LoadFiles();
		}

		// Token: 0x06000BFE RID: 3070 RVA: 0x00089874 File Offset: 0x00087A74
		public override void ButtonClick(ButtonClickEventArgs e)
		{
			base.ButtonClick(e);
			string a;
			if ((a = e.Key.ToUpper()) != null)
			{
				if (!(a == "FBUTTON_FILTER"))
				{
					return;
				}
				this.LoadFiles();
			}
		}

		// Token: 0x06000BFF RID: 3071 RVA: 0x000898AC File Offset: 0x00087AAC
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			base.BarItemClick(e);
			string a;
			if ((a = e.BarItemKey.ToUpper()) != null)
			{
				if (!(a == "TBRETURN"))
				{
					return;
				}
				this.GetSelectedLogList();
				this.View.ReturnToParentWindow(this.lstSelectDatas);
				this.View.Close();
			}
		}

		// Token: 0x06000C00 RID: 3072 RVA: 0x00089900 File Offset: 0x00087B00
		public override void EntryBarItemClick(BarItemClickEventArgs e)
		{
			base.EntryBarItemClick(e);
			string a;
			if ((a = e.BarItemKey.ToUpper()) != null)
			{
				if (!(a == "TBRETURN"))
				{
					return;
				}
				this.GetSelectedLogList();
				this.View.ReturnToParentWindow(this.lstSelectDatas);
				this.View.Close();
			}
		}

		// Token: 0x06000C01 RID: 3073 RVA: 0x00089954 File Offset: 0x00087B54
		private void LoadFiles()
		{
			Dictionary<DynamicObject, List<DynamicObject>> plmfileList = this.GetPLMFileList1();
			this.BindDataToDevEntry(plmfileList);
		}

		// Token: 0x06000C02 RID: 3074 RVA: 0x00089970 File Offset: 0x00087B70
		private Dictionary<DynamicObject, List<DynamicObject>> GetPLMFileList1()
		{
			List<long> list = null;
			string text = this.View.Model.GetValue("FMaterialScope").ToString();
			string a;
			if ((a = text) != null)
			{
				if (!(a == "A"))
				{
					if (a == "B")
					{
						list = SFCTechDocEntity.Instance.GetPLMMaterialIds(base.Context);
					}
				}
				else
				{
					list = this.dicMaterialId.Keys.ToList<long>();
				}
			}
			string text2 = Convert.ToString(this.View.Model.GetValue("FMaterialNumber"));
			if (!string.IsNullOrWhiteSpace(text2))
			{
				list = SFCMaterialEntity.Instance.GetMaterialIdsFilterByMaterialNumber(base.Context, list, text2);
			}
			return SFSPLMPreviewServiceHelper.DoSFSAllMatDocsAction(base.Context, list);
		}

		// Token: 0x06000C03 RID: 3075 RVA: 0x00089A28 File Offset: 0x00087C28
		private void BindDataToDevEntry(Dictionary<DynamicObject, List<DynamicObject>> dicFile)
		{
			string value = Convert.ToString(this.View.Model.GetValue("FFileName"));
			DynamicObject dynamicObject;
			int num;
			this.View.Model.TryGetEntryCurrentRow("FEntity", ref dynamicObject, ref num);
			DynamicObjectCollection dynamicObjectCollection = this.View.Model.DataObject["Entity"] as DynamicObjectCollection;
			dynamicObjectCollection.Clear();
			DynamicObjectType dynamicCollectionItemPropertyType = dynamicObjectCollection.DynamicCollectionItemPropertyType;
			foreach (KeyValuePair<DynamicObject, List<DynamicObject>> keyValuePair in dicFile)
			{
				List<DynamicObject> value2 = keyValuePair.Value;
				DynamicObject key = keyValuePair.Key;
				foreach (DynamicObject dynamicObject2 in value2)
				{
					if (!string.IsNullOrWhiteSpace(value))
					{
						string text = Convert.ToString(dynamicObject2["Name"]);
						if (!text.Contains(value))
						{
							continue;
						}
					}
					DynamicObject dynamicObject3 = new DynamicObject(dynamicCollectionItemPropertyType);
					dynamicObject3["PLMFID"] = dynamicObject2["Id"];
					dynamicObject3["PLMFileId"] = dynamicObject2["FileId_Id"];
					dynamicObject3["PLMFileName"] = Convert.ToString(dynamicObject2["Name"]);
					dynamicObject3["PLMVerNO"] = Convert.ToString(dynamicObject2["VerNO"]);
					dynamicObject3["PLMBuildVer"] = Convert.ToString(dynamicObject2["BuildVer"]);
					dynamicObject3["PLMMaxVer"] = Convert.ToString(dynamicObject2["MaxVer"]);
					dynamicObject3["PLMMinVer"] = Convert.ToString(dynamicObject2["MinVer"]);
					DynamicObject dynamicObject4 = dynamicObject2["CreatorId"] as DynamicObject;
					dynamicObject3["PLMCreatorName"] = Convert.ToString(dynamicObject4["Name"]);
					dynamicObject3["PLMModifyTime"] = dynamicObject2["ModifyDate"];
					long num2 = Convert.ToInt64(key["ErpMaterialID_Id"]);
					dynamicObject3["MaterialId_Id"] = num2;
					dynamicObject3["MaterialId"] = SFCMaterialManager.Instance.LoadSingle(base.Context, num2);
					dynamicObjectCollection.Add(dynamicObject3);
				}
			}
			if (num == -1)
			{
				num = 0;
			}
			this.View.UpdateView("FEntity");
			this.Model.SetEntryCurrentRowIndex("FEntity", num);
			this.View.SetEntityFocusRow("FEntity", num);
		}

		// Token: 0x06000C04 RID: 3076 RVA: 0x00089D1C File Offset: 0x00087F1C
		private List<DynamicObject> GetSelectedLogList()
		{
			this.lstSelectDatas.Clear();
			DynamicObjectCollection source = this.View.Model.DataObject["Entity"] as DynamicObjectCollection;
			List<DynamicObject> result = (from o in source
			where Convert.ToBoolean(o["CheckBox"])
			select o).ToList<DynamicObject>();
			this.lstSelectDatas = result;
			return result;
		}

		// Token: 0x0400059A RID: 1434
		private Dictionary<long, string> dicMaterialId;

		// Token: 0x0400059B RID: 1435
		private List<DynamicObject> lstSelectDatas = new List<DynamicObject>();
	}
}
