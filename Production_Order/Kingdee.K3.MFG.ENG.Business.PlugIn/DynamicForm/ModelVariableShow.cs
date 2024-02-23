using System;
using System.Collections.Generic;
using System.ComponentModel;
using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.MFG.ENG.ProductModel;
using Kingdee.K3.MFG.ServiceHelper.ENG;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm
{
	// Token: 0x020000AB RID: 171
	[Description("产品模型上变量和计算变量展示界面")]
	public class ModelVariableShow : AbstractDynamicFormPlugIn
	{
		// Token: 0x06000BF5 RID: 3061 RVA: 0x000893E4 File Offset: 0x000875E4
		public override void OnInitialize(InitializeEventArgs e)
		{
			base.OnInitialize(e);
			string text = string.Empty;
			text = Convert.ToString(this.View.OpenParameter.GetCustomParameter("CustomTitle"));
			if (!string.IsNullOrWhiteSpace(text))
			{
				this.View.SetFormTitle(new LocaleValue(text));
			}
		}

		// Token: 0x06000BF6 RID: 3062 RVA: 0x00089432 File Offset: 0x00087632
		public override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			this.ShowData();
		}

		// Token: 0x06000BF7 RID: 3063 RVA: 0x00089444 File Offset: 0x00087644
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			base.BarItemClick(e);
			string barItemKey;
			if ((barItemKey = e.BarItemKey) != null)
			{
				if (!(barItemKey == "tbReturnData"))
				{
					return;
				}
				int entryCurrentRowIndex = this.View.Model.GetEntryCurrentRowIndex("FEntity");
				EntryEntity entryEntity = this.View.BusinessInfo.GetEntryEntity("FEntity");
				DynamicObject entityDataObject = this.View.Model.GetEntityDataObject(entryEntity, entryCurrentRowIndex);
				this.View.ReturnToParentWindow(entityDataObject);
				this.View.Close();
			}
		}

		// Token: 0x06000BF8 RID: 3064 RVA: 0x000894C8 File Offset: 0x000876C8
		public override void EntityRowDoubleClick(EntityRowClickEventArgs e)
		{
			base.EntityRowDoubleClick(e);
			EntryEntity entryEntity = this.View.BusinessInfo.GetEntryEntity("FEntity");
			DynamicObject entityDataObject = this.View.Model.GetEntityDataObject(entryEntity, e.Row);
			this.View.ReturnToParentWindow(entityDataObject);
			this.View.Close();
		}

		// Token: 0x06000BF9 RID: 3065 RVA: 0x00089524 File Offset: 0x00087724
		protected void ShowData()
		{
			long num = Convert.ToInt64(this.View.OpenParameter.GetCustomParameter("ShowType"));
			if (num == 0L)
			{
				num = 1L;
			}
			this.View.Model.SetValue("FShowType", num);
			if (num == 1L)
			{
				this.ShowModelVariable();
				return;
			}
			List<string> list = new List<string>();
			object customParameter = this.View.OpenParameter.GetCustomParameter("ListShowValue");
			if (customParameter != null)
			{
				list = (List<string>)customParameter;
			}
			this.View.Model.DeleteEntryData("FEntity");
			DynamicObjectCollection dynamicObjectCollection = this.View.Model.DataObject["Entity"] as DynamicObjectCollection;
			foreach (string text in list)
			{
				this.View.Model.CreateNewEntryRow("FEntity");
				this.View.Model.SetValue("FValue", text, dynamicObjectCollection.Count - 1);
			}
			this.View.UpdateView("FEntity");
		}

		// Token: 0x06000BFA RID: 3066 RVA: 0x00089658 File Offset: 0x00087858
		private void ShowModelVariable()
		{
			ModelVariableOption modelVariableOption = new ModelVariableOption();
			List<DynamicObject> list = new List<DynamicObject>();
			object customParameter = this.View.OpenParameter.GetCustomParameter("ListShowModeVariable");
			if (customParameter == null)
			{
				long item = Convert.ToInt64(this.View.OpenParameter.GetCustomParameter("ProductModelId"));
				modelVariableOption.productModelIds = new List<long>
				{
					item
				};
				string varType = Convert.ToString(this.View.OpenParameter.GetCustomParameter("VarType"));
				modelVariableOption.varType = varType;
				IOperationResult modelVariableDatas = ProductModelRuleServiceHelper.GetModelVariableDatas(base.Context, modelVariableOption);
				List<DynamicObject> list2 = (List<DynamicObject>)modelVariableDatas.FuncResult;
				Entity entity = this.View.BusinessInfo.GetEntity("FEntity");
				DynamicObjectCollection entityDataObject = this.View.Model.GetEntityDataObject(entity);
				entityDataObject.Clear();
				if (!modelVariableDatas.IsSuccess || ListUtils.IsEmpty<DynamicObject>(list2))
				{
					this.View.UpdateView("FEntity");
					return;
				}
				this.Model.BeginIniti();
				foreach (DynamicObject item2 in list2)
				{
					entityDataObject.Add(item2);
				}
				this.Model.EndIniti();
			}
			else
			{
				list = (List<DynamicObject>)customParameter;
				DynamicObjectCollection dynamicObjectCollection = this.View.Model.DataObject["Entity"] as DynamicObjectCollection;
				dynamicObjectCollection.Clear();
				foreach (DynamicObject item3 in list)
				{
					dynamicObjectCollection.Add(item3);
				}
			}
			this.View.UpdateView("FEntity");
		}
	}
}
