using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.MFG.EntityHelper;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm
{
	// Token: 0x020000AA RID: 170
	[Description("模型字段显示")]
	public class ModelSysLableShow : AbstractDynamicFormPlugIn
	{
		// Token: 0x06000BEC RID: 3052 RVA: 0x00088C4C File Offset: 0x00086E4C
		public override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			this.ShowSysLable();
		}

		// Token: 0x06000BED RID: 3053 RVA: 0x00088C5C File Offset: 0x00086E5C
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

		// Token: 0x06000BEE RID: 3054 RVA: 0x00088CE0 File Offset: 0x00086EE0
		public override void EntityRowDoubleClick(EntityRowClickEventArgs e)
		{
			base.EntityRowDoubleClick(e);
			EntryEntity entryEntity = this.View.BusinessInfo.GetEntryEntity("FEntity");
			DynamicObject entityDataObject = this.View.Model.GetEntityDataObject(entryEntity, e.Row);
			this.View.ReturnToParentWindow(entityDataObject);
			this.View.Close();
		}

		// Token: 0x06000BEF RID: 3055 RVA: 0x00088E04 File Offset: 0x00087004
		private void ShowSysLable()
		{
			FormMetadata formMetadata = (FormMetadata)MetaDataServiceHelper.Load(this.View.Context, "ENG_BOM", true);
			List<Field> fields = formMetadata.BusinessInfo.GetEntryEntity("FTreeEntity").Fields;
			List<Field> list = (from w in fields
			where w.Key == "FMATERIALIDCHILD" || w.Key == "FAuxPropId" || w.Key == "FNUMERATOR" || w.Key == "FDENOMINATOR" || w.Key == "FSCRAPRATE"
			select w).ToList<Field>();
			Entity entity = this.View.BusinessInfo.GetEntity("FEntity");
			DynamicObjectCollection entityDataObject = this.View.Model.GetEntityDataObject(entity);
			List<DynamicObject> auxProDatas = this.LoadAuxProDatas();
			foreach (Field field in list)
			{
				if (field is RelatedFlexGroupField)
				{
					this.SetAuxProData(entityDataObject, field, auxProDatas, formMetadata);
				}
				else
				{
					DynamicObject dynamicObject = entityDataObject.DynamicCollectionItemPropertyType.CreateInstance() as DynamicObject;
					DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "FormID", formMetadata.BusinessInfo.GetForm().Id);
					DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "FORMNAME", formMetadata.BusinessInfo.GetForm().Name.ToString());
					DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "PropertyGroup", formMetadata.BusinessInfo.GetEntryEntity("FTreeEntity").Name.ToString());
					DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "SYSLBALENUMBER", field.Key);
					DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "SYSLBALENAME", field.Name.ToString());
					string text = (field is BaseDataField) ? "B" : "D";
					DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "SYSLABLETYPE", text);
					entityDataObject.Add(dynamicObject);
				}
			}
			string text2 = Convert.ToString(this.View.OpenParameter.GetCustomParameter("baseDataFormID"));
			if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(text2))
			{
				FormMetadata formMetadata2 = (FormMetadata)MetaDataServiceHelper.Load(this.View.Context, text2, true);
				EntryEntity entryEntity = formMetadata2.BusinessInfo.GetEntryEntity("FEntity");
				if (!ObjectUtils.IsNullOrEmpty(entryEntity))
				{
					List<Field> fields2 = entryEntity.Fields;
					List<Field> list2 = (from w in fields2
					where w is BaseDataField || w is TextField || w is CheckBoxField || w is RelatedFlexGroupField || w is AssistantField || w is MulAssistantField || w is DecimalField || w is QtyField || w is BaseQtyField || w is IntegerField
					select w).ToList<Field>();
					foreach (Field field2 in list2)
					{
						if (field2 is RelatedFlexGroupField)
						{
							this.SetAuxProData(entityDataObject, field2, auxProDatas, formMetadata2);
						}
						else
						{
							DynamicObject dynamicObject2 = entityDataObject.DynamicCollectionItemPropertyType.CreateInstance() as DynamicObject;
							DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "FormID", formMetadata2.BusinessInfo.GetForm().Id);
							DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "FORMNAME", formMetadata2.BusinessInfo.GetForm().Name.ToString());
							DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "PropertyGroup", formMetadata2.BusinessInfo.GetEntryEntity("FEntity").Name.ToString());
							DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "SYSLBALENUMBER", field2.Key);
							DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "SYSLBALENAME", field2.Name.ToString());
							string text3 = string.Empty;
							if (field2 is BaseDataField)
							{
								text3 = "B";
							}
							else if (field2 is AssistantField || field2 is MulAssistantField)
							{
								text3 = "A";
							}
							else if (field2 is TextField)
							{
								text3 = "C";
							}
							else if (field2 is CheckBoxField)
							{
								text3 = "E";
							}
							else
							{
								text3 = "D";
							}
							DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "SYSLABLETYPE", text3);
							entityDataObject.Add(dynamicObject2);
						}
					}
				}
			}
			this.View.UpdateView("FEntity");
		}

		// Token: 0x06000BF0 RID: 3056 RVA: 0x000891FC File Offset: 0x000873FC
		private List<DynamicObject> LoadAuxProDatas()
		{
			List<SelectorItemInfo> list = new List<SelectorItemInfo>();
			list.Add(new SelectorItemInfo("FId"));
			list.Add(new SelectorItemInfo("FNUMBER"));
			list.Add(new SelectorItemInfo("FNAME"));
			list.Add(new SelectorItemInfo("FVALUETYPE"));
			string text = string.Format("{0} = 'C' and {1} = 'A'", "FDocumentStatus", "FFORBIDSTATUS");
			OQLFilter oqlfilter = OQLFilter.CreateHeadEntityFilter(text);
			return BusinessDataServiceHelper.Load(base.Context, "BD_FLEXAUXPROPERTY", list, oqlfilter).ToList<DynamicObject>();
		}

		// Token: 0x06000BF1 RID: 3057 RVA: 0x00089284 File Offset: 0x00087484
		private void SetAuxProData(DynamicObjectCollection entrys, Field field, List<DynamicObject> auxProDatas, FormMetadata formMetaData)
		{
			foreach (DynamicObject dynamicObject in auxProDatas)
			{
				DynamicObject dynamicObject2 = entrys.DynamicCollectionItemPropertyType.CreateInstance() as DynamicObject;
				DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "FormID", formMetaData.BusinessInfo.GetForm().Id);
				DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "FORMNAME", formMetaData.BusinessInfo.GetForm().Name.ToString());
				DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "PropertyGroup", formMetaData.BusinessInfo.GetEntryEntity("FTreeEntity").Name.ToString());
				DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "SYSLBALENUMBER", DataEntityExtend.GetDynamicValue<string>(dynamicObject, "NUMBER", null));
				DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "SYSLBALENAME", DataEntityExtend.GetDynamicValue<object>(dynamicObject, "NAME", null).ToString());
				string dynamicValue = DataEntityExtend.GetDynamicValue<string>(dynamicObject, "VALUETYPE", null);
				string text = string.Empty;
				if (dynamicValue == "0")
				{
					text = "B";
				}
				else if (dynamicValue == "1")
				{
					text = "A";
				}
				else
				{
					text = "D";
				}
				DataEntityExtend.SetDynamicObjectItemValue(dynamicObject2, "SYSLABLETYPE", text);
				entrys.Add(dynamicObject2);
			}
		}
	}
}
