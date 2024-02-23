using System;
using System.Collections.Generic;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.K3.Core.MFG.EntityHelper;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm
{
	// Token: 0x0200009B RID: 155
	internal class MyComparer : IEqualityComparer<DynamicObject>
	{
		// Token: 0x06000B18 RID: 2840 RVA: 0x0007EF8C File Offset: 0x0007D18C
		public bool Equals(DynamicObject x, DynamicObject y)
		{
			return (x != null || y != null) && (DataEntityExtend.GetDynamicObjectItemValue<long>(x, "ProductLineId_Id", 0L) == DataEntityExtend.GetDynamicObjectItemValue<long>(y, "ProductLineId_Id", 0L) && DataEntityExtend.GetDynamicObjectItemValue<long>(x, "PrdLineLocId_Id", 0L) == DataEntityExtend.GetDynamicObjectItemValue<long>(y, "PrdLineLocId_Id", 0L)) && DataEntityExtend.GetDynamicObjectItemValue<long>(x, "BopMaterialId_Id", 0L) == DataEntityExtend.GetDynamicObjectItemValue<long>(y, "BopMaterialId_Id", 0L);
		}

		// Token: 0x06000B19 RID: 2841 RVA: 0x0007EFF7 File Offset: 0x0007D1F7
		public int GetHashCode(DynamicObject obj)
		{
			return 0;
		}
	}
}
