using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;

namespace Kingdee.Wychl.Token.Business
{
	// Token: 0x02000015 RID: 21
	public class planSubmit : AbstractOperationServicePlugIn
	{
		// Token: 0x06000041 RID: 65 RVA: 0x000056AC File Offset: 0x000038AC
		public override void OnPreparePropertys(PreparePropertysEventArgs e)
		{
			base.OnPreparePropertys(e);
			e.FieldKeys.Add("F_DATE");
			e.FieldKeys.Add("F_QTY1");
			e.FieldKeys.Add("F_QTY2");
			e.FieldKeys.Add("F_QTY3");
			e.FieldKeys.Add("F_QTY4");
			e.FieldKeys.Add("F_QTY5");
			e.FieldKeys.Add("FEntity");
			e.FieldKeys.Add("FALLEntity");
			e.FieldKeys.Add("F_MONTH");
			e.FieldKeys.Add("F_ALLQTY1");
			e.FieldKeys.Add("F_ALLQTY2");
			e.FieldKeys.Add("F_ALLQTY3");
			e.FieldKeys.Add("F_ALLQTY4");
			e.FieldKeys.Add("F_ALLQTY5");
		}

		// Token: 0x06000042 RID: 66 RVA: 0x000057B0 File Offset: 0x000039B0
		public override void BeginOperationTransaction(BeginOperationTransactionArgs e)
		{
			base.BeginOperationTransaction(e);
			foreach (DynamicObject dynamicObject in e.DataEntitys)
			{
				DynamicObjectCollection dynamicObjectCollection = dynamicObject["FALLEntity"] as DynamicObjectCollection;
				dynamicObjectCollection.Clear();
				string text = string.Format("/*dialect*/select concat(DATEPART(YYYY,F_DATE),'-',DATEPART(MM,F_DATE)) as FMONTH,sum(F_QTY1) F_QTY1,\r\n                         sum(F_QTY2) F_QTY2, sum(F_QTY3) as F_QTY3, sum(F_QTY4) F_QTY4, sum(F_QTY5) F_QTY5\r\n                         from PCQE_PCJHENTRY WHERE FID ={0}\r\n                         group by DATEPART(YYYY, F_DATE), DATEPART(MM, F_DATE)", dynamicObject["id"].ToString());
				DynamicObjectCollection dynamicObjectCollection2 = DBServiceHelper.ExecuteDynamicObject(base.Context, text, null, null, CommandType.Text, Array.Empty<SqlParam>());
				bool flag = dynamicObjectCollection2.Count > 0;
				if (flag)
				{
					IEnumerable<long> sequenceInt = DBServiceHelper.GetSequenceInt64(base.Context, "PCQE_PCJHALLENTRY", dynamicObjectCollection2.Count);
					for (int j = 0; j < dynamicObjectCollection2.Count; j++)
					{
						DynamicObject dynamicObject2 = new DynamicObject(dynamicObjectCollection.DynamicCollectionItemPropertyType);
						dynamicObject2["id"] = sequenceInt.ElementAt(j);
						dynamicObject2["F_MONTH"] = dynamicObjectCollection2[j]["FMONTH"];
						dynamicObject2["F_ALLQTY1"] = Convert.ToDecimal(dynamicObjectCollection2[j]["F_QTY1"]);
						dynamicObject2["F_ALLQTY2"] = Convert.ToDecimal(dynamicObjectCollection2[j]["F_QTY2"]);
						dynamicObject2["F_ALLQTY3"] = Convert.ToDecimal(dynamicObjectCollection2[j]["F_QTY3"]);
						dynamicObject2["F_ALLQTY4"] = Convert.ToDecimal(dynamicObjectCollection2[j]["F_QTY4"]);
						dynamicObject2["F_ALLQTY5"] = Convert.ToDecimal(dynamicObjectCollection2[j]["F_QTY5"]);
						dynamicObjectCollection.Add(dynamicObject2);
					}
				}
			}
		}
	}
}
