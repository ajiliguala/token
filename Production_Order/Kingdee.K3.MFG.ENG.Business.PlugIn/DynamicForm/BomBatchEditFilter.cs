using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using Kingdee.BOS;
using Kingdee.BOS.Core.CommonFilter.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.Core.MFG.Utils;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm
{
	// Token: 0x0200006E RID: 110
	[Description("BOM批量维护过滤界面插件")]
	public class BomBatchEditFilter : AbstractCommonFilterPlugIn
	{
		// Token: 0x17000047 RID: 71
		// (get) Token: 0x0600080E RID: 2062 RVA: 0x0005ECC8 File Offset: 0x0005CEC8
		// (set) Token: 0x0600080F RID: 2063 RVA: 0x0005ECD0 File Offset: 0x0005CED0
		private DynamicObjectCollection ChangeEntrys { get; set; }

		// Token: 0x06000810 RID: 2064 RVA: 0x0005ECDC File Offset: 0x0005CEDC
		public override void TreeNodeClick(TreeNodeArgs e)
		{
			base.TreeNodeClick(e);
			DynamicObjectCollection dynamicValue = DataEntityExtend.GetDynamicValue<DynamicObjectCollection>(this.View.Model.DataObject, "ChangeEntity", null);
			if (!ListUtils.IsEmpty<DynamicObject>(dynamicValue))
			{
				this.View.UpdateView("FChangeEntity");
				foreach (DynamicObject dynamicObject in dynamicValue)
				{
					string a = DataEntityExtend.GetDynamicValue<string>(dynamicObject, "ChangeFieldKey", null).Split(new char[]
					{
						'*'
					})[1];
					if (a == "FCHILDUNITID" || a == "FChildSupplyOrgId" || a == "FBOMID" || a == "FDOSAGETYPE")
					{
						MFGBillUtil.SetEnabled(this.View, "FIsCanChange", dynamicObject, false, "");
					}
				}
			}
		}

		// Token: 0x06000811 RID: 2065 RVA: 0x0005EDCC File Offset: 0x0005CFCC
		public override void DataChanged(DataChangedEventArgs e)
		{
			base.DataChanged(e);
			if (e.Field.Key == "FEditType")
			{
				List<Field> list = new List<Field>();
				string a;
				if ((a = e.NewValue.ToString()) != null)
				{
					if (!(a == "d"))
					{
						if (!(a == "m"))
						{
							if (a == "b")
							{
								list.AddRange(this.ReturnFieldListFTreeEntity(e.NewValue.ToString()));
							}
						}
						else
						{
							list.AddRange(this.ReturnFieldListFBillHead());
						}
					}
					else
					{
						list.AddRange(this.ReturnFieldListFTreeEntity(e.NewValue.ToString()));
					}
				}
				this.SetShow(list, (e.NewValue.ToString() == "b") ? "FChangeEntity" : "FEntity");
			}
			if (e.Field.Key == "FIsCanChange" && !ObjectUtils.IsNullOrEmpty(this.ChangeEntrys))
			{
				if (this.ChangeEntrys.Count == 42)
				{
					this.SetChecked(e.Row);
				}
				if (!MFGBillUtil.GetValue<bool>(this.View.Model, e.Field.Key, e.Row, false, null))
				{
					this.View.Model.SetValue("FIsCanAppendC", false, e.Row);
					this.View.UpdateView("FIsCanAppendC", e.Row);
				}
			}
			if (e.Field.Key == "FIsCanEdit" && !MFGBillUtil.GetValue<bool>(this.View.Model, e.Field.Key, e.Row, false, null))
			{
				this.View.Model.SetValue("FIsCanAppend", false, e.Row);
				this.View.UpdateView("FIsCanAppend", e.Row);
			}
			if (e.Field.Key == "FIsCanAppendC" && MFGBillUtil.GetValue<bool>(this.View.Model, e.Field.Key, e.Row, false, null))
			{
				this.View.Model.SetValue("FIsCanChange", true, e.Row);
				this.View.UpdateView("FIsCanChange", e.Row);
			}
			if (e.Field.Key == "FIsCanAppend" && MFGBillUtil.GetValue<bool>(this.View.Model, e.Field.Key, e.Row, false, null))
			{
				this.View.Model.SetValue("FIsCanEdit", true, e.Row);
				this.View.UpdateView("FIsCanEdit", e.Row);
			}
		}

		// Token: 0x06000812 RID: 2066 RVA: 0x0005F098 File Offset: 0x0005D298
		private void SetShow(List<Field> list, string entityKey)
		{
			int num = 0;
			this.View.Model.DeleteEntryData("FEntity");
			this.View.Model.DeleteEntryData("FChangeEntity");
			foreach (Field field in list)
			{
				this.View.Model.CreateNewEntryRow(entityKey);
				this.View.Model.SetValue((entityKey == "FChangeEntity") ? "FChangeFieldName" : "FFieldName", field.Name.ToString(), num);
				this.View.Model.SetValue((entityKey == "FChangeEntity") ? "FChangeFieldKey" : "FFieldKey", field.EntityKey + "*" + field.Key, num);
				this.View.Model.SetValue("FIsCanChange", true, num);
				this.View.Model.SetValue((entityKey == "FChangeEntity") ? "FChangeFieldProp" : "FFieldProp", field.PropertyName, num);
				num++;
			}
			this.View.UpdateView(entityKey);
			if (entityKey == "FChangeEntity")
			{
				num = 0;
				this.ChangeEntrys = DataEntityExtend.GetDynamicObjectItemValue<DynamicObjectCollection>(this.View.Model.DataObject, "ChangeEntity", null);
				using (List<Field>.Enumerator enumerator2 = list.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						Field field2 = enumerator2.Current;
						if (field2.Key == "FCHILDUNITID" || field2.Key == "FChildSupplyOrgId" || field2.Key == "FBOMID" || field2.Key == "FDOSAGETYPE")
						{
							MFGBillUtil.SetEnabled(this.View, "FIsCanChange", this.ChangeEntrys[num], false, "");
						}
						if (field2.ElementType != 1 && field2.ElementType != 36 && field2.ElementType != 45 && field2.ElementType != 85)
						{
							this.View.Model.SetValue("FIsCanAppendC", false, num);
							MFGBillUtil.SetEnabled(this.View, "FIsCanAppendC", this.ChangeEntrys[num], false, "");
						}
						num++;
					}
					return;
				}
			}
			num = 0;
			DynamicObjectCollection dynamicObjectItemValue = DataEntityExtend.GetDynamicObjectItemValue<DynamicObjectCollection>(this.View.Model.DataObject, "Entity", null);
			foreach (Field field3 in list)
			{
				if (field3.ElementType != 1 && field3.ElementType != 36 && field3.ElementType != 45 && field3.ElementType != 85)
				{
					this.View.Model.SetValue("FIsCanAppend", false, num);
					MFGBillUtil.SetEnabled(this.View, "FIsCanAppend", dynamicObjectItemValue[num], false, "");
				}
				num++;
			}
		}

		// Token: 0x06000813 RID: 2067 RVA: 0x0005F3F8 File Offset: 0x0005D5F8
		private List<Field> ReturnFieldListFTreeEntity(string fieldKey)
		{
			Dictionary<string, List<Field>> dictionary = new Dictionary<string, List<Field>>();
			dictionary = this.GetCustomField();
			List<Field> list = new List<Field>();
			list.Add(new Field
			{
				Name = new LocaleValue(ResManager.LoadKDString("供应类型", "015072000019292", 7, new object[0])),
				Key = "FSupplyType",
				PropertyName = "SupplyType",
				EntityKey = "FTreeEntity",
				ElementType = 9
			});
			list.Add(new Field
			{
				Name = new LocaleValue(ResManager.LoadKDString("子项类型", "015072000014369", 7, new object[0])),
				Key = "FMATERIALTYPE",
				PropertyName = "MATERIALTYPE",
				EntityKey = "FTreeEntity",
				ElementType = 9
			});
			list.Add(new Field
			{
				Name = new LocaleValue(ResManager.LoadKDString("子项单位", "015072000014370", 7, new object[0])),
				Key = "FCHILDUNITID",
				PropertyName = "CHILDUNITID_Id",
				EntityKey = "FTreeEntity",
				ElementType = 46
			});
			list.Add(new Field
			{
				Name = new LocaleValue(ResManager.LoadKDString("用量类型", "015072000014371", 7, new object[0])),
				Key = "FDOSAGETYPE",
				PropertyName = "DOSAGETYPE",
				EntityKey = "FTreeEntity",
				ElementType = 9
			});
			list.Add(new Field
			{
				Name = new LocaleValue(ResManager.LoadKDString("用量：分子", "015072000014372", 7, new object[0])),
				Key = "FNUMERATOR",
				PropertyName = "NUMERATOR*BaseNumerator",
				EntityKey = "FTreeEntity",
				ElementType = 22
			});
			list.Add(new Field
			{
				Name = new LocaleValue(ResManager.LoadKDString("用量：分母", "015072000014373", 7, new object[0])),
				Key = "FDENOMINATOR",
				PropertyName = "DENOMINATOR*BaseDenominator",
				EntityKey = "FTreeEntity",
				ElementType = 22
			});
			list.Add(new Field
			{
				Name = new LocaleValue(ResManager.LoadKDString("固定损耗", "015072000014374", 7, new object[0])),
				Key = "FFIXSCRAPQTY",
				PropertyName = "FIXSCRAPQTY*BaseFixscrapQty",
				EntityKey = "FTreeEntity",
				ElementType = 22
			});
			list.Add(new Field
			{
				Name = new LocaleValue(ResManager.LoadKDString("变动损耗率%", "015072000014375", 7, new object[0])),
				Key = "FSCRAPRATE",
				PropertyName = "FSCRAPRATE",
				EntityKey = "FTreeEntity",
				ElementType = 2
			});
			if (!base.Context.IsStandardEdition())
			{
				list.Add(new Field
				{
					Name = new LocaleValue(ResManager.LoadKDString("供应组织", "015072000014376", 7, new object[0])),
					Key = "FChildSupplyOrgId",
					PropertyName = "ChildSupplyOrgId_Id",
					EntityKey = "FTreeEntity",
					ElementType = 7
				});
			}
			list.Add(new Field
			{
				Name = new LocaleValue(ResManager.LoadKDString("子项BOM版本", "015072000014377", 7, new object[0])),
				Key = "FBOMID",
				PropertyName = "BOMID_Id",
				EntityKey = "FTreeEntity",
				ElementType = 13
			});
			list.Add(new Field
			{
				Name = new LocaleValue(ResManager.LoadKDString("发料方式", "015072000014378", 7, new object[0])),
				Key = "FISSUETYPE",
				PropertyName = "ISSUETYPE",
				EntityKey = "FTreeEntity",
				ElementType = 9
			});
			list.Add(new Field
			{
				Name = new LocaleValue(ResManager.LoadKDString("倒冲时机", "015072000014379", 7, new object[0])),
				Key = "FBACKFLUSHTYPE",
				PropertyName = "BACKFLUSHTYPE",
				EntityKey = "FTreeEntity",
				ElementType = 9
			});
			if (!base.Context.IsStandardEdition())
			{
				list.Add(new Field
				{
					Name = new LocaleValue(ResManager.LoadKDString("发料组织", "015072000014380", 7, new object[0])),
					Key = "FSUPPLYORG",
					PropertyName = "SUPPLYORG_Id",
					EntityKey = "FTreeEntity",
					ElementType = 7
				});
			}
			list.Add(new Field
			{
				Name = new LocaleValue(ResManager.LoadKDString("默认发料仓库", "015072000014381", 7, new object[0])),
				Key = "FSTOCKID",
				PropertyName = "STOCKID_Id",
				EntityKey = "FTreeEntity",
				ElementType = 13
			});
			list.Add(new Field
			{
				Name = new LocaleValue(ResManager.LoadKDString("默认发料仓位", "015072000014382", 7, new object[0])),
				Key = "FSTOCKLOCID",
				PropertyName = "STOCKLOCID_Id",
				EntityKey = "FTreeEntity",
				ElementType = 13
			});
			list.Add(new Field
			{
				Name = new LocaleValue(ResManager.LoadKDString("超发控制方式", "015072000014383", 7, new object[0])),
				Key = "FOverControlMode",
				PropertyName = "OverControlMode",
				EntityKey = "FTreeEntity",
				ElementType = 9
			});
			if (fieldKey == "d")
			{
				list.Add(new Field
				{
					Name = new LocaleValue(ResManager.LoadKDString("领料考虑最小发料批量", "015072000025077", 7, new object[0])),
					Key = "FISMinIssueQty",
					PropertyName = "ISMinIssueQty",
					EntityKey = "FTreeEntity",
					ElementType = 8
				});
			}
			list.Add(new Field
			{
				Name = new LocaleValue(ResManager.LoadKDString("是否发损耗", "015072000014384", 7, new object[0])),
				Key = "FISGETSCRAP",
				PropertyName = "FISGETSCRAP",
				EntityKey = "FTreeEntity",
				ElementType = 8
			});
			list.Add(new Field
			{
				Name = new LocaleValue(ResManager.LoadKDString("偏置时间", "015072000014385", 7, new object[0])),
				Key = "FOFFSETTIME",
				PropertyName = "OFFSETTIME",
				EntityKey = "FTreeEntity",
				ElementType = 3
			});
			list.Add(new Field
			{
				Name = new LocaleValue(ResManager.LoadKDString("时间单位", "015072000014386", 7, new object[0])),
				Key = "FTIMEUNIT",
				PropertyName = "FTIMEUNIT",
				EntityKey = "FTreeEntity",
				ElementType = 9
			});
			list.Add(new Field
			{
				Name = new LocaleValue(ResManager.LoadKDString("是否关键件", "015072000014387", 7, new object[0])),
				Key = "FISKEYCOMPONENT",
				PropertyName = "ISKEYCOMPONENT",
				EntityKey = "FTreeEntity",
				ElementType = 8
			});
			list.Add(new Field
			{
				Name = new LocaleValue(ResManager.LoadKDString("货主类型", "015072000014388", 7, new object[0])),
				Key = "FOWNERTYPEID",
				PropertyName = "OWNERTYPEID",
				EntityKey = "FTreeEntity",
				ElementType = 15
			});
			list.Add(new Field
			{
				Name = new LocaleValue(ResManager.LoadKDString("货主", "015072000014389", 7, new object[0])),
				Key = "FOWNERID",
				PropertyName = "OWNERID_Id",
				EntityKey = "FTreeEntity",
				ElementType = 16
			});
			list.Add(new Field
			{
				Name = new LocaleValue(ResManager.LoadKDString("工序序列", "015072000014390", 7, new object[0])),
				Key = "FOptQueue",
				PropertyName = "OptQueue",
				EntityKey = "FTreeEntity",
				ElementType = 1
			});
			list.Add(new Field
			{
				Name = new LocaleValue(ResManager.LoadKDString("工序", "015072000014391", 7, new object[0])),
				Key = "FOPERID",
				PropertyName = "OPERID",
				EntityKey = "FTreeEntity",
				ElementType = 13
			});
			list.Add(new Field
			{
				Name = new LocaleValue(ResManager.LoadKDString("位置号", "015072000014392", 7, new object[0])),
				Key = "FPOSITIONNO",
				PropertyName = "POSITIONNO",
				EntityKey = "FTreeEntity",
				ElementType = 1
			});
			list.Add(new Field
			{
				Name = new LocaleValue(ResManager.LoadKDString("作业", "015072000037256", 7, new object[0])),
				Key = "FPROCESSID",
				PropertyName = "PROCESSID_Id",
				EntityKey = "FTreeEntity",
				ElementType = 13
			});
			list.Add(new Field
			{
				Name = new LocaleValue(ResManager.LoadKDString("生效日期", "015072000014394", 7, new object[0])),
				Key = "FEFFECTDATE",
				PropertyName = "EFFECTDATE",
				EntityKey = "FTreeEntity",
				ElementType = 4
			});
			list.Add(new Field
			{
				Name = new LocaleValue(ResManager.LoadKDString("失效日期", "015072000014395", 7, new object[0])),
				Key = "FEXPIREDATE",
				PropertyName = "EXPIREDATE",
				EntityKey = "FTreeEntity",
				ElementType = 4
			});
			list.Add(new Field
			{
				Name = new LocaleValue(ResManager.LoadKDString("辅助属性", "015072000014396", 7, new object[0])),
				Key = "FAuxPropId",
				PropertyName = "AuxPropId_Id",
				EntityKey = "FTreeEntity",
				ElementType = 13
			});
			list.Add(new Field
			{
				Name = new LocaleValue(ResManager.LoadKDString("拆卸成本比例", "015072000014397", 7, new object[0])),
				Key = "FDISASSMBLERATE",
				PropertyName = "DISASSMBLERATE",
				EntityKey = "FTreeEntity",
				ElementType = 2
			});
			list.Add(new Field
			{
				Name = new LocaleValue(ResManager.LoadKDString("备注", "015072000014398", 7, new object[0])),
				Key = "FMEMO",
				PropertyName = "MEMO",
				EntityKey = "FTreeEntity",
				ElementType = 1
			});
			list.Add(new Field
			{
				Name = new LocaleValue(ResManager.LoadKDString("可选择", "015072000014399", 7, new object[0])),
				Key = "FIsCanChoose",
				PropertyName = "IsCanChoose",
				EntityKey = "FTreeEntity",
				ElementType = 8
			});
			list.Add(new Field
			{
				Name = new LocaleValue(ResManager.LoadKDString("可修改", "015072000014400", 7, new object[0])),
				Key = "FIsCanEdit",
				PropertyName = "IsCanEdit",
				EntityKey = "FTreeEntity",
				ElementType = 8
			});
			list.Add(new Field
			{
				Name = new LocaleValue(ResManager.LoadKDString("可替换", "015072000014401", 7, new object[0])),
				Key = "FIsCanReplace",
				PropertyName = "IsCanReplace",
				EntityKey = "FTreeEntity",
				ElementType = 8
			});
			list.Add(new Field
			{
				Name = new LocaleValue(ResManager.LoadKDString("跳层", "015072000018137", 7, new object[0])),
				Key = "FISSkip",
				PropertyName = "ISSkip",
				EntityKey = "FTreeEntity",
				ElementType = 8
			});
			if (!base.Context.IsStandardEdition())
			{
				list.Add(new Field
				{
					Name = new LocaleValue(ResManager.LoadKDString("供料方式", "015072000012078", 7, new object[0])),
					Key = "FSupplyMode",
					PropertyName = "SupplyMode",
					EntityKey = "FTreeEntity",
					ElementType = 9
				});
			}
			list.Add(new Field
			{
				Name = new LocaleValue(ResManager.LoadKDString("MRP运算", "015072000037257", 7, new object[0])),
				Key = "FIsMrpRun",
				PropertyName = "IsMrpRun",
				EntityKey = "FTreeEntity",
				ElementType = 8
			});
			foreach (KeyValuePair<string, List<Field>> keyValuePair in dictionary)
			{
				List<Field> value = keyValuePair.Value;
				foreach (Field field in value)
				{
					list.Add(new Field
					{
						Name = new LocaleValue(field.Name),
						Key = field.Key,
						PropertyName = ((field is BaseDataField) ? (field.PropertyName + "_Id") : field.PropertyName),
						EntityKey = "FTreeEntity",
						ElementType = field.ElementType
					});
				}
			}
			if (fieldKey == "b")
			{
				list.Add(new Field
				{
					Name = new LocaleValue(ResManager.LoadKDString("阶梯-起始数量（含）", "015072000018138", 7, new object[0])),
					Key = "FSTARTQTY",
					PropertyName = "STARTQTY",
					EntityKey = "FBOMCHILDLOTBASEDQTY",
					ElementType = 22
				});
				list.Add(new Field
				{
					Name = new LocaleValue(ResManager.LoadKDString("阶梯-截止数量", "015072000018139", 7, new object[0])),
					Key = "FENDQTY",
					PropertyName = "ENDQTY",
					EntityKey = "FBOMCHILDLOTBASEDQTY",
					ElementType = 22
				});
				list.Add(new Field
				{
					Name = new LocaleValue(ResManager.LoadKDString("阶梯-子项单位", "015072000018140", 7, new object[0])),
					Key = "FUNITIDLOT",
					PropertyName = "UNITIDLOT",
					EntityKey = "FBOMCHILDLOTBASEDQTY",
					ElementType = 46
				});
				list.Add(new Field
				{
					Name = new LocaleValue(ResManager.LoadKDString("阶梯-固定损耗", "015072000018141", 7, new object[0])),
					Key = "FFIXSCRAPQTYLOT",
					PropertyName = "FIXSCRAPQTYLOT",
					EntityKey = "FBOMCHILDLOTBASEDQTY",
					ElementType = 22
				});
				list.Add(new Field
				{
					Name = new LocaleValue(ResManager.LoadKDString("阶梯-变动损耗率", "015072000018142", 7, new object[0])),
					Key = "FSCRAPRATELOT",
					PropertyName = "SCRAPRATELOT",
					EntityKey = "FBOMCHILDLOTBASEDQTY",
					ElementType = 2
				});
				list.Add(new Field
				{
					Name = new LocaleValue(ResManager.LoadKDString("阶梯-用量：分子", "015072000018143", 7, new object[0])),
					Key = "FNUMERATORLOT",
					PropertyName = "NUMERATORLOT",
					EntityKey = "FBOMCHILDLOTBASEDQTY",
					ElementType = 22
				});
				list.Add(new Field
				{
					Name = new LocaleValue(ResManager.LoadKDString("阶梯-用量：分母", "015072000018144", 7, new object[0])),
					Key = "FDENOMINATORLOT",
					PropertyName = "DENOMINATORLOT",
					EntityKey = "FBOMCHILDLOTBASEDQTY",
					ElementType = 22
				});
				list.Add(new Field
				{
					Name = new LocaleValue(ResManager.LoadKDString("阶梯-备注", "015072000018145", 7, new object[0])),
					Key = "FNOTELOT",
					PropertyName = "NOTELOT",
					EntityKey = "FBOMCHILDLOTBASEDQTY",
					ElementType = 1
				});
			}
			return list;
		}

		// Token: 0x06000814 RID: 2068 RVA: 0x00060624 File Offset: 0x0005E824
		private List<Field> ReturnFieldListFBillHead()
		{
			return new List<Field>
			{
				new Field
				{
					Name = new LocaleValue(ResManager.LoadKDString("BOM用途", "015072000014402", 7, new object[0])),
					Key = "FBOMUSE",
					PropertyName = "BOMUSE",
					EntityKey = "FBillHead"
				},
				new Field
				{
					Name = new LocaleValue(ResManager.LoadKDString("BOM分组", "015072000014403", 7, new object[0])),
					Key = "FGroup",
					PropertyName = "GROUP_Id",
					EntityKey = "FBillHead"
				}
			};
		}

		// Token: 0x06000815 RID: 2069 RVA: 0x000606D8 File Offset: 0x0005E8D8
		private void SetChecked(int row)
		{
			if (ObjectUtils.IsNullOrEmpty(this.ChangeEntrys))
			{
				this.ChangeEntrys = DataEntityExtend.GetDynamicObjectItemValue<DynamicObjectCollection>(this.View.Model.DataObject, "ChangeEntity", null);
			}
			string text = this.ChangeEntrys[row][2].ToString();
			bool flag = Convert.ToBoolean(this.ChangeEntrys[row][3]);
			string key;
			switch (key = text)
			{
			case "FTreeEntity*FSUPPLYORG":
				this.View.Model.SetValue("FIsCanChange", flag, row + 1);
				this.View.Model.SetValue("FIsCanChange", flag, row + 2);
				return;
			case "FTreeEntity*FSTOCKID":
				this.View.Model.SetValue("FIsCanChange", flag, row - 1);
				this.View.Model.SetValue("FIsCanChange", flag, row + 1);
				return;
			case "FTreeEntity*FSTOCKLOCID":
				this.View.Model.SetValue("FIsCanChange", flag, row - 1);
				this.View.Model.SetValue("FIsCanChange", flag, row - 2);
				return;
			case "FTreeEntity*FISSUETYPE":
				this.View.Model.SetValue("FIsCanChange", flag, row + 1);
				return;
			case "FTreeEntity*FBACKFLUSHTYPE":
				this.View.Model.SetValue("FIsCanChange", flag, row - 1);
				return;
			case "FTreeEntity*FOWNERTYPEID":
				this.View.Model.SetValue("FIsCanChange", flag, row + 1);
				return;
			case "FTreeEntity*FOWNERID":
				this.View.Model.SetValue("FIsCanChange", flag, row - 1);
				break;

				return;
			}
		}

		// Token: 0x06000816 RID: 2070 RVA: 0x00060954 File Offset: 0x0005EB54
		private Dictionary<string, List<Field>> GetCustomField()
		{
			Dictionary<string, List<Field>> dictionary = new Dictionary<string, List<Field>>();
			string text = "SELECT T.FID,T.FBASEOBJECTID FROM T_META_OBJECTTYPE T WHERE T.FINHERITPATH LIKE '%ENG_BOM%' AND T.FDEVTYPE=2 ";
			DynamicObjectCollection dynamicObjectCollection = DBServiceHelper.ExecuteDynamicObject(base.Context, text, null, null, CommandType.Text, new SqlParam[0]);
			if (ListUtils.IsEmpty<DynamicObject>(dynamicObjectCollection))
			{
				return dictionary;
			}
			string baseObjectId = "ENG_BOM";
			Dictionary<string, IGrouping<string, DynamicObject>> dyExpandGroups = (from g in dynamicObjectCollection
			group g by DataEntityExtend.GetDynamicValue<string>(g, "FBASEOBJECTID", null)).ToDictionary((IGrouping<string, DynamicObject> d) => d.Key);
			string text2 = this.ExpandSelectFid(baseObjectId, dyExpandGroups);
			if (ObjectUtils.IsNullOrEmptyOrWhiteSpace(text2))
			{
				return dictionary;
			}
			FormMetadata formMetadata = (FormMetadata)MetaDataServiceHelper.Load(base.Context, "ENG_BOM", false);
			EntryEntity entryEntity = formMetadata.BusinessInfo.GetEntryEntity("FTreeEntity");
			List<Field> fields = entryEntity.Fields;
			FormMetadata formMetadata2 = (FormMetadata)MetaDataServiceHelper.Load(base.Context, text2, false);
			List<Field> fields2 = formMetadata2.BusinessInfo.GetEntryEntity("FTreeEntity").Fields;
			List<Field> list = new List<Field>();
			using (List<Field>.Enumerator enumerator = fields2.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Field field = enumerator.Current;
					if ((from w in fields
					where w.Key == field.Key
					select w).ToList<Field>().Count<Field>() <= 0 && !(field is BasePropertyField))
					{
						list.Add(field);
					}
				}
			}
			dictionary.Add(text2, list);
			return dictionary;
		}

		// Token: 0x06000817 RID: 2071 RVA: 0x00060AF4 File Offset: 0x0005ECF4
		private string ExpandSelectFid(string baseObjectId, Dictionary<string, IGrouping<string, DynamicObject>> dyExpandGroups)
		{
			IGrouping<string, DynamicObject> source;
			if (!dyExpandGroups.TryGetValue(baseObjectId, out source))
			{
				return baseObjectId;
			}
			baseObjectId = DataEntityExtend.GetDynamicValue<string>(source.First<DynamicObject>(), "FID", null);
			return this.ExpandSelectFid(baseObjectId, dyExpandGroups);
		}
	}
}
