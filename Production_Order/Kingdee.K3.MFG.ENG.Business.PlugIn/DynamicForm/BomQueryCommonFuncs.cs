using System;
using System.Collections.Generic;
using System.Linq;
using Kingdee.BOS;
using Kingdee.BOS.Cache;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.K3.Core.MFG.Utils;
using Kingdee.K3.MFG.ServiceHelper.ENG;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm
{
	// Token: 0x02000077 RID: 119
	public static class BomQueryCommonFuncs
	{
		// Token: 0x060008B5 RID: 2229 RVA: 0x00066676 File Offset: 0x00064876
		public static bool IsEmptyValue(object value)
		{
			return value == null || string.IsNullOrWhiteSpace(value.ToString()) || value.ToString() == "0";
		}

		// Token: 0x060008B6 RID: 2230 RVA: 0x0006669C File Offset: 0x0006489C
		public static void SetEntityFieldValue(IDynamicFormView View, Field field, int row, DynamicObject dataEntity, Func<Field, DynamicObject, object> GetEntityFieldValue)
		{
			if (!BomQueryCommonFuncs.fieldIgnore(field))
			{
				object obj = GetEntityFieldValue(field, dataEntity);
				if (field != null)
				{
					if (field is BaseDataField && BomQueryCommonFuncs.IsEmptyValue(obj))
					{
						return;
					}
					if (field is RelatedFlexGroupField)
					{
						if (!BomQueryCommonFuncs.IsEmptyValue(obj))
						{
							MFGBillUtil.SetAuxPropValue(View, field.Key, obj, row, true);
							return;
						}
					}
					else
					{
						View.Model.SetValue(field.Key, obj, row);
					}
				}
			}
		}

		// Token: 0x060008B7 RID: 2231 RVA: 0x00066702 File Offset: 0x00064902
		public static bool fieldIgnore(Field fieldItem)
		{
			return fieldItem is BasePropertyField;
		}

		// Token: 0x060008B8 RID: 2232 RVA: 0x00066730 File Offset: 0x00064930
		public static void ShowOrHideField(IDynamicFormView view, FilterParameter filterParam)
		{
			if (filterParam == null)
			{
				return;
			}
			using (List<Field>.Enumerator enumerator = view.BillBusinessInfo.GetFieldList().GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Field field = enumerator.Current;
					if (!(field.Entity is HeadEntity))
					{
						try
						{
							bool visible = false;
							ColumnField columnField = filterParam.ColumnInfo.FirstOrDefault((ColumnField item) => item.Key.Equals(field.Key, StringComparison.InvariantCultureIgnoreCase));
							if (columnField != null)
							{
								visible = columnField.Visible;
							}
							Control control = view.GetControl(field.Key);
							if (control != null)
							{
								control.Visible = visible;
							}
						}
						catch
						{
						}
					}
				}
			}
		}

		// Token: 0x060008B9 RID: 2233 RVA: 0x00066800 File Offset: 0x00064A00
		public static void PutSubStituteDataToCache(Context ctx, long ReplaceID, List<DynamicObject> SubsData)
		{
			IKCacheManager cacheManager = RCacheManagerFactory.Instance.GetCacheManager("Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm.BomQueryCommonFuncs.SubStituteData", ctx.GetAreaCacheKey());
			if (cacheManager != null)
			{
				cacheManager.Put(ReplaceID.ToString(), SubsData, null);
			}
		}

		// Token: 0x060008BA RID: 2234 RVA: 0x00066838 File Offset: 0x00064A38
		public static List<DynamicObject> GetSubStituteDataFromCache(Context ctx, long ReplaceID)
		{
			List<DynamicObject> list = null;
			IKCacheManager cacheManager = RCacheManagerFactory.Instance.GetCacheManager("Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm.BomQueryCommonFuncs.SubStituteData", ctx.GetAreaCacheKey());
			string text = ReplaceID.ToString();
			if (cacheManager != null)
			{
				list = (cacheManager.Get(text) as List<DynamicObject>);
			}
			if (list == null)
			{
				list = BomQueryServiceHelper.GetBomSubStituteData(ctx, new List<long>
				{
					ReplaceID
				});
				if (list != null)
				{
					BomQueryCommonFuncs.PutSubStituteDataToCache(ctx, ReplaceID, list);
				}
			}
			return list;
		}

		// Token: 0x0400040B RID: 1035
		private const string SubstituteCacheKey = "Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm.BomQueryCommonFuncs.SubStituteData";

		// Token: 0x0400040C RID: 1036
		private const string FieldKey_FREPLACEID = "FREPLACEID";
	}
}
