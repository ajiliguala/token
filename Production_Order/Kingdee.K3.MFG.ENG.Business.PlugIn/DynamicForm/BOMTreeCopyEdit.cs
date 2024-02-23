using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Kingdee.BOS;
using Kingdee.BOS.Core.DependencyRules;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Log;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.FormElement;
using Kingdee.BOS.Core.Permission;
using Kingdee.BOS.Core.Validation;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.MFG.ENG.BomTree;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.MFG.ServiceHelper.ENG;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.DynamicForm
{
	// Token: 0x02000081 RID: 129
	[Description("BOM树形升版插件")]
	public class BOMTreeCopyEdit : AbstractDynamicFormPlugIn
	{
		// Token: 0x060009C4 RID: 2500 RVA: 0x00072C38 File Offset: 0x00070E38
		public override void OnInitialize(InitializeEventArgs e)
		{
			this.View.RuleContainer.AddPluginRule("FEntity", 1, delegate(DynamicObject row, dynamic dynObj)
			{
				bool isSelect = DataEntityExtend.GetDynamicValue<bool>(row, "IsSelect", false);
				this.View.RuleContainer.Suspend();
				string path = DataEntityExtend.GetDynamicValue<string>(row, "Path", null);
				DynamicObjectCollection allRows = this.Model.GetEntityDataObject(this.View.BusinessInfo.GetEntity("FEntity"));
				if (!isSelect)
				{
					DataEntityExtend.SetDynamicObjectItemValue(row, "AssignBomNum", "");
					this.View.StyleManager.SetEnabled("FAssignBomNum", row, "locked", false);
					this.View.UpdateView("FIsSelect", allRows.IndexOf(row));
					this.Model.BeginIniti();
					(from x in allRows
					where DataEntityExtend.GetDynamicValue<string>(x, "Path", null).StartsWith(path) && x != row
					select x).ToList<DynamicObject>().ForEach(delegate(DynamicObject subRow)
					{
						this.Model.SetValue("FIsSelect", isSelect, allRows.IndexOf(subRow));
						DataEntityExtend.SetDynamicObjectItemValue(subRow, "AssignBomNum", "");
						this.View.StyleManager.SetEnabled("FAssignBomNum", subRow, "locked", false);
						this.View.UpdateView("FIsSelect", allRows.IndexOf(subRow));
					});
					this.Model.EndIniti();
				}
				else
				{
					List<string> parents = path.Split(new string[]
					{
						"."
					}, StringSplitOptions.RemoveEmptyEntries).ToList<string>();
					this.View.StyleManager.SetEnabled("FAssignBomNum", row, "locked", true);
					this.View.UpdateView("FIsSelect", allRows.IndexOf(row));
					parents.Remove(DataEntityExtend.GetDynamicValue<string>(row, "RowId", null));
					if (!ListUtils.IsEmpty<string>(parents))
					{
						this.Model.BeginIniti();
						(from x in allRows
						where parents.Contains(DataEntityExtend.GetDynamicValue<string>(x, "RowId", null))
						select x).ToList<DynamicObject>().ForEach(delegate(DynamicObject parentRow)
						{
							this.Model.SetValue("FIsSelect", isSelect, allRows.IndexOf(parentRow));
							this.View.StyleManager.SetEnabled("FAssignBomNum", parentRow, "locked", true);
							this.View.UpdateView("FIsSelect", allRows.IndexOf(parentRow));
						});
						this.Model.EndIniti();
					}
				}
				this.View.RuleContainer.Resume(new BOSActionExecuteContext(this.View));
			}, new string[]
			{
				"FIsSelect"
			});
		}

		// Token: 0x060009C5 RID: 2501 RVA: 0x00072C78 File Offset: 0x00070E78
		public override void BeforeBindData(EventArgs e)
		{
			this.LoadBomTreeNode();
		}

		// Token: 0x060009C6 RID: 2502 RVA: 0x00072E94 File Offset: 0x00071094
		private void LoadBomTreeNode()
		{
			List<BomExpandNodeTreeMode> list = this.View.OpenParameter.GetCustomParameter("treeNodeDatas", false) as List<BomExpandNodeTreeMode>;
			if (ListUtils.IsEmpty<BomExpandNodeTreeMode>(list))
			{
				return;
			}
			int idx = 0;
			HashSet<string> hashSet = new HashSet<string>(from x in list
			select x.ParentEntryId);
			Dictionary<string, string> dictPath = new Dictionary<string, string>();
			List<BomExpandNodeTreeMode> list2 = new List<BomExpandNodeTreeMode>();
			foreach (BomExpandNodeTreeMode bomExpandNodeTreeMode in list)
			{
				if (hashSet.Contains(bomExpandNodeTreeMode.EntryId))
				{
					list2.Add(bomExpandNodeTreeMode);
				}
				else if (DataEntityExtend.GetDynamicValue<string>(bomExpandNodeTreeMode.DataEntity, "ErpClsID", null) == "1" && bomExpandNodeTreeMode.BomId_Id != 0L)
				{
					list2.Add(bomExpandNodeTreeMode);
				}
			}
			List<BomExpandNodeTreeMode> list3 = (from x in list2
			orderby x.BomLevel
			select x).ToList<BomExpandNodeTreeMode>();
			this.View.Session["bomTreeNode"] = list3;
			List<string> nonUpgradeRowIds = new List<string>();
			list3.ForEach(delegate(BomExpandNodeTreeMode tn)
			{
				DynamicObject dynamicObject = tn.DataEntity["BOMID"] as DynamicObject;
				bool flag = DataEntityExtend.GetDynamicValue<long>(dynamicObject, "UseOrgId_Id", 0L) == DataEntityExtend.GetDynamicValue<long>(dynamicObject, "CreateOrgId_Id", 0L);
				string text = (tn.BomLevel == 0L) ? "" : tn.ParentEntryId;
				if (!flag || nonUpgradeRowIds.Contains(text))
				{
					nonUpgradeRowIds.Add(tn.EntryId);
					return;
				}
				this.Model.CreateNewEntryRow("FEntity");
				this.Model.SetValue("FRowId", tn.EntryId, idx);
				this.Model.SetValue("FParentRowId", text, idx);
				this.Model.SetValue("FOrgId", DataEntityExtend.GetDynamicValue<long>(tn.BomId, "UseOrgId_Id", 0L), idx);
				this.Model.SetValue("FMaterialId", DataEntityExtend.GetDynamicValue<long>(tn.DataEntity, "MaterialId_Id", 0L), idx);
				this.Model.SetValue("FBOMID", DataEntityExtend.GetDynamicValue<long>(tn.DataEntity, "BOMID_Id", 0L), idx);
				this.Model.SetValue("FIsSelect", true, idx);
				string text2 = (dictPath.TryGetValue(text, out text2) && !ObjectUtils.IsNullOrEmptyOrWhiteSpace(text2)) ? string.Format("{0}.{1}", text2, tn.EntryId) : tn.EntryId;
				this.Model.SetValue("FPath", text2, idx);
				dictPath.Add(tn.EntryId, text2);
				idx++;
			});
		}

		// Token: 0x060009C7 RID: 2503 RVA: 0x00072FFC File Offset: 0x000711FC
		public override void BeforeF7Select(BeforeF7SelectEventArgs e)
		{
			string a;
			if ((a = e.FieldKey.ToUpperInvariant()) != null)
			{
				if (!(a == "FBOMID"))
				{
					return;
				}
				e.IsShowApproved = false;
			}
		}

		// Token: 0x060009C8 RID: 2504 RVA: 0x00073030 File Offset: 0x00071230
		public override void BeforeSetItemValueByNumber(BeforeSetItemValueByNumberArgs e)
		{
			string baseDataFieldKey;
			if ((baseDataFieldKey = e.BaseDataFieldKey) != null)
			{
				if (!(baseDataFieldKey == "FBOMID"))
				{
					return;
				}
				e.IsShowApproved = false;
			}
		}

		// Token: 0x060009C9 RID: 2505 RVA: 0x00073264 File Offset: 0x00071464
		public override void BarItemClick(BarItemClickEventArgs e)
		{
			Context context = base.Context;
			string a;
			if ((a = e.BarItemKey.ToUpperInvariant()) != null)
			{
				if (!(a == "TBCONFIRM"))
				{
					return;
				}
				DynamicObjectCollection entityDataObject = this.Model.GetEntityDataObject(this.View.BusinessInfo.GetEntity("FEntity"));
				if (!this.ValidateBomCreatePermission(entityDataObject))
				{
					e.Cancel = true;
					return;
				}
				List<BomExpandNodeTreeMode> allTreeNodes = this.View.Session["bomTreeNode"] as List<BomExpandNodeTreeMode>;
				FormMetadata formMetadata = MetaDataServiceHelper.Load(context, "ENG_BOM", true) as FormMetadata;
				List<IGrouping<long, DynamicObject>> list = (from w in entityDataObject
				where DataEntityExtend.GetDynamicValue<bool>(w, "IsSelect", false)
				select w into g
				group g by DataEntityExtend.GetDynamicValue<long>(g, "MaterialId_Id", 0L)).ToList<IGrouping<long, DynamicObject>>();
				StringBuilder stringBuilder = new StringBuilder();
				foreach (IGrouping<long, DynamicObject> grouping in list)
				{
					List<IGrouping<long, DynamicObject>> list2 = (from w in grouping
					group w by DataEntityExtend.GetDynamicValue<long>(w, "BomId_Id", 0L)).ToList<IGrouping<long, DynamicObject>>();
					List<Tuple<string, string>> source = (from w in grouping
					where !ObjectUtils.IsNullOrEmptyOrWhiteSpace(DataEntityExtend.GetDynamicValue<string>(w, "AssignBomNum", null))
					select w into s
					select new Tuple<string, string>(DataEntityExtend.GetDynamicValue<string>(s, "AssignBomNum", null), DataEntityExtend.GetDynamicValue<string>(DataEntityExtend.GetDynamicValue<DynamicObject>(s, "BOMID", null), "Number", null))).ToList<Tuple<string, string>>();
					List<IGrouping<string, Tuple<string, string>>> source2 = (from g in source.Distinct<Tuple<string, string>>()
					group g by g.Item1).ToList<IGrouping<string, Tuple<string, string>>>();
					List<IGrouping<string, Tuple<string, string>>> list3 = (from w in source2
					where w.Count<Tuple<string, string>>() >= 2
					select w).ToList<IGrouping<string, Tuple<string, string>>>();
					if (list2.Count > 1 && !ListUtils.IsEmpty<IGrouping<string, Tuple<string, string>>>(list3) && !ListUtils.IsEmpty<DynamicObject>(grouping))
					{
						List<Tuple<string, string>> source3 = list3.SelectMany((IGrouping<string, Tuple<string, string>> s) => from s1 in s
						select new Tuple<string, string>(s1.Item1, s1.Item2)).ToList<Tuple<string, string>>();
						string dynamicValue = DataEntityExtend.GetDynamicValue<string>(DataEntityExtend.GetDynamicValue<DynamicObject>(grouping.FirstOrDefault<DynamicObject>(), "MaterialID", null), "Number", null);
						string arg = string.Join(",", (from s in source3
						select s.Item2).ToList<string>());
						string arg2 = string.Join(",", (from s in source3
						select s.Item1).ToList<string>());
						stringBuilder.AppendFormat("物料编码【{0}】不允许不同BOM【{1}】指向同一特定BOM版本【{2}】", dynamicValue, arg, arg2).AppendLine();
					}
				}
				if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(stringBuilder.ToString()))
				{
					this.View.ShowErrMessage(stringBuilder.ToString(), "", 0);
					e.Cancel = true;
					return;
				}
				Dictionary<string, Tuple<DynamicObject, DynamicObject>> dictionary = this.BuildRelations(context, entityDataObject, allTreeNodes, formMetadata);
				DynamicObject[] source4 = (from x in dictionary.Values
				select x.Item2).ToArray<DynamicObject>();
				DynamicObject[] array = (from w in source4
				where ObjectUtils.IsNullOrEmptyOrWhiteSpace(DataEntityExtend.GetDynamicValue<string>(w, "Number", null))
				select w).ToArray<DynamicObject>();
				string empty = string.Empty;
				if (!ListUtils.IsEmpty<DynamicObject>(array))
				{
					for (int i = 0; i < array.Length; i++)
					{
						string text = this.ApplyNewNumber(context, formMetadata, array[i], ref empty);
						if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(empty))
						{
							this.View.ShowErrMessage(empty, "", 0);
							e.Cancel = true;
							return;
						}
						DataEntityExtend.SetDynamicObjectItemValue(array[i], "Number", text);
					}
				}
				stringBuilder = new StringBuilder();
				DynamicObject[] array2 = (from x in dictionary.Values
				select x.Item2).ToArray<DynamicObject>();
				IEnumerable<IGrouping<string, DynamicObject>> enumerable = from g in array2
				group g by string.Format("{0}|{1}", DataEntityExtend.GetDynamicValue<string>(g, "MaterialId_Id", null), DataEntityExtend.GetDynamicValue<string>(g, "UseOrgId_Id", null));
				foreach (IGrouping<string, DynamicObject> source5 in enumerable)
				{
					IEnumerable<string> source6 = from s in source5
					select DataEntityExtend.GetDynamicValue<string>(s, "Number", null);
					List<IGrouping<string, string>> source7 = (from g in source6
					group g by g).ToList<IGrouping<string, string>>();
					List<string> list4 = (from w in source7
					where w.Count<string>() >= 2
					select w into s
					select s.Key).ToList<string>();
					if (!ListUtils.IsEmpty<string>(list4))
					{
						string dynamicValue2 = DataEntityExtend.GetDynamicValue<string>(DataEntityExtend.GetDynamicValue<DynamicObject>(source5.FirstOrDefault<DynamicObject>(), "MaterialId", null), "Number", null);
						string arg3 = DataEntityExtend.GetDynamicValue<LocaleValue>(DataEntityExtend.GetDynamicValue<DynamicObject>(source5.FirstOrDefault<DynamicObject>(), "UseOrgId", null), "Name", null)[base.Context.UserLocale.LCID];
						stringBuilder.AppendFormat("使用组织【{2}】下物料编码【{0}】不能同时保存相同的BOM版本号【{1}】(其中的重复BOM版本号包括系统自动生成的)", dynamicValue2, string.Join(",", list4), arg3).AppendLine();
					}
				}
				if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(stringBuilder.ToString()))
				{
					this.View.ShowErrMessage(stringBuilder.ToString(), "", 0);
					e.Cancel = true;
					return;
				}
				OperateOption operateOption = OperateOption.Create();
				OperateOptionUtils.SetOnlyValidate(operateOption, true);
				operateOption.SetVariableValue("IsBomTreeUpdate", true);
				IOperationResult operationResult = BusinessDataServiceHelper.Save(context, formMetadata.BusinessInfo, array2, operateOption, "");
				if (!ListUtils.IsEmpty<ValidationErrorInfo>(operationResult.ValidationErrors))
				{
					StringBuilder msgSb = new StringBuilder();
					operationResult.ValidationErrors.ForEach(delegate(ValidationErrorInfo f)
					{
						msgSb.AppendLine(f.Message);
					});
					this.View.ShowErrMessage(msgSb.ToString(), "", 0);
					e.Cancel = true;
					return;
				}
				this.UpdateTreeNode(context, entityDataObject, allTreeNodes, dictionary);
				operateOption = OperateOption.Create();
				OperateOptionUtils.SetValidateFlag(operateOption, false);
				IOperationResult operationResult2 = BusinessDataServiceHelper.Save(context, formMetadata.BusinessInfo, (from x in dictionary.Values
				select x.Item2).ToArray<DynamicObject>(), operateOption, "");
				this.View.ShowOperateResult(operationResult2.OperateResult, delegate(FormResult cb)
				{
					BomExpandNodeTreeMode bomExpandNodeTreeMode = (from n in allTreeNodes
					where n.BomLevel == 0L
					select n).FirstOrDefault<BomExpandNodeTreeMode>();
					DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
					dynamicFormShowParameter.OpenStyle.ShowType = 7;
					dynamicFormShowParameter.FormId = "ENG_BOMTREE";
					dynamicFormShowParameter.CustomParams["BOMID"] = bomExpandNodeTreeMode.BomId_Id.ToString();
					this.View.ShowForm(dynamicFormShowParameter);
					this.View.Close();
				}, "BOS_BatchTips");
			}
		}

		// Token: 0x060009CA RID: 2506 RVA: 0x00073990 File Offset: 0x00071B90
		protected string ApplyNewNumber(Context ctx, FormMetadata meta, DynamicObject BomData, ref string errMsg)
		{
			string result = string.Empty;
			for (int i = 0; i < 5; i++)
			{
				try
				{
					List<BillNoInfo> billNo = BusinessDataServiceHelper.GetBillNo(ctx, meta.BusinessInfo, new DynamicObject[]
					{
						BomData
					}, true, "");
					result = billNo.FirstOrDefault<BillNoInfo>().BillNo;
					LogObject logObject = new LogObject
					{
						Description = string.Format("物料【{0}】BOM树形升版申请新版本号【{1}】.", DataEntityExtend.GetDynamicValue<string>(DataEntityExtend.GetDynamicValue<DynamicObject>(BomData, "MaterialId", null), "Number", null), billNo.FirstOrDefault<BillNoInfo>().BillNo),
						Environment = 3,
						OperateName = "BOM树形升版",
						ObjectTypeId = "ENG_BOMTREECOPY",
						SubSystemId = "25"
					};
					LogServiceHelper.WriteLog(ctx, logObject);
					break;
				}
				catch (KDBusinessException ex)
				{
					if (i == 4)
					{
						errMsg = ex.Message;
						LogObject logObject2 = new LogObject
						{
							Description = string.Format(ResManager.LoadKDString("物料【{0}】BOM树形升版申请新版本号重试5次失败，请尝试修复编码流水号.", "0151515153499000026366", 7, new object[0]), DataEntityExtend.GetDynamicValue<string>(DataEntityExtend.GetDynamicValue<DynamicObject>(BomData, "MaterialId", null), "Number", null)),
							Environment = 3,
							OperateName = "BOM树形升版",
							ObjectTypeId = "ENG_BOMTREECOPY",
							SubSystemId = "25"
						};
						LogServiceHelper.WriteLog(ctx, logObject2);
					}
				}
				catch (KDDuplicateNoException ex2)
				{
					if (i == 4)
					{
						errMsg = ex2.Message;
						LogObject logObject3 = new LogObject
						{
							Description = string.Format(ResManager.LoadKDString("物料【{0}】BOM树形升版申请新版本号重试5次失败，请尝试修复编码流水号.", "0151515153499000026366", 7, new object[0]), DataEntityExtend.GetDynamicValue<string>(DataEntityExtend.GetDynamicValue<DynamicObject>(BomData, "MaterialId", null), "Number", null)),
							Environment = 3,
							OperateName = "BOM树形升版",
							ObjectTypeId = "ENG_BOMTREECOPY",
							SubSystemId = "25"
						};
						LogServiceHelper.WriteLog(ctx, logObject3);
					}
				}
			}
			return result;
		}

		// Token: 0x060009CB RID: 2507 RVA: 0x00073BA4 File Offset: 0x00071DA4
		private bool ValidateBomCreatePermission(DynamicObjectCollection entityDatas)
		{
			if (ListUtils.IsEmpty<DynamicObject>(entityDatas))
			{
				return true;
			}
			IEnumerable<DynamicObject> enumerable = from o in entityDatas
			where DataEntityExtend.GetDynamicValue<bool>(o, "IsSelect", false)
			select o;
			if (ListUtils.IsEmpty<DynamicObject>(enumerable))
			{
				return true;
			}
			List<long> list = new List<long>();
			StringBuilder stringBuilder = new StringBuilder();
			StringBuilder stringBuilder2 = new StringBuilder();
			foreach (DynamicObject dynamicObject in enumerable)
			{
				long dynamicValue = DataEntityExtend.GetDynamicValue<long>(dynamicObject, "OrgId_Id", 0L);
				if (!list.Contains(dynamicValue))
				{
					if (!this.ValidatePermission("fce8b1aca2144beeb3c6655eaf78bc34", dynamicValue))
					{
						DynamicObject dynamicValue2 = DataEntityExtend.GetDynamicValue<DynamicObject>(dynamicObject, "OrgId", null);
						DynamicObject dynamicValue3 = DataEntityExtend.GetDynamicValue<DynamicObject>(dynamicObject, "BOMId", null);
						if (dynamicValue2 == null || dynamicValue3 == null)
						{
							continue;
						}
						string value = DataEntityExtend.GetDynamicValue<LocaleValue>(dynamicValue2, "Name", null)[base.Context.UserLocale.LCID];
						string dynamicValue4 = DataEntityExtend.GetDynamicValue<string>(dynamicValue3, "Number", null);
						stringBuilder.Append(",").Append(value);
						stringBuilder2.Append(",").Append(dynamicValue4);
					}
					list.Add(dynamicValue);
				}
			}
			if (stringBuilder.Length != 0)
			{
				stringBuilder = stringBuilder.Remove(0, 1);
				if (stringBuilder2.Length != 0)
				{
					stringBuilder2 = stringBuilder2.Remove(0, 1);
				}
				this.View.ShowMessage(string.Format("您没有【{0}】的【物料清单】新增 权限，不允许复制更新BOM版本{1}！", stringBuilder.ToString(), stringBuilder2.ToString()), 0);
				return false;
			}
			return true;
		}

		// Token: 0x060009CC RID: 2508 RVA: 0x00073D38 File Offset: 0x00071F38
		private bool ValidatePermission(string permission, long orgId)
		{
			PermissionAuthResult permissionAuthResult = PermissionServiceHelper.FuncPermissionAuth(base.Context, new BusinessObject(orgId)
			{
				Id = "ENG_BOM",
				SubSystemId = this.View.Model.SubSytemId
			}, permission);
			return permissionAuthResult.Passed;
		}

		// Token: 0x060009CD RID: 2509 RVA: 0x00073E3C File Offset: 0x0007203C
		private Dictionary<string, Tuple<DynamicObject, DynamicObject>> BuildRelations(Context ctx, DynamicObjectCollection entityDatas, List<BomExpandNodeTreeMode> allTreeNodes, FormMetadata meta)
		{
			DateTime systemDateTime = TimeServiceHelper.GetSystemDateTime(ctx);
			(from tn in allTreeNodes
			group tn by tn.ParentEntryId).ToDictionary((IGrouping<string, BomExpandNodeTreeMode> x) => x.Key);
			HashSet<string> selectedEntryIds = new HashSet<string>(from row in entityDatas
			where DataEntityExtend.GetDynamicValue<bool>(row, "IsSelect", false)
			select row into ele
			select DataEntityExtend.GetDynamicValue<string>(ele, "RowId", null));
			Dictionary<string, IGrouping<string, DynamicObject>> dictionary = (from g in entityDatas
			group g by DataEntityExtend.GetDynamicValue<string>(g, "RowId", null)).ToDictionary((IGrouping<string, DynamicObject> k) => k.Key);
			List<BomExpandNodeTreeMode> list = (from node in allTreeNodes
			where selectedEntryIds.Contains(node.EntryId)
			orderby node.BomLevel
			select node).ToList<BomExpandNodeTreeMode>();
			Dictionary<string, Tuple<DynamicObject, DynamicObject>> dictionary2 = new Dictionary<string, Tuple<DynamicObject, DynamicObject>>();
			Dictionary<string, string> dictionary3 = new Dictionary<string, string>();
			object[] array = (from w in list
			where DataEntityExtend.GetDynamicValue<long>(w.DataEntity, "BOMID_ID", 0L) != 0L
			select w into s
			select DataEntityExtend.GetDynamicValue<object>(s.DataEntity, "BOMID_ID", null)).Distinct<object>().ToArray<object>();
			DynamicObject[] source = BusinessDataServiceHelper.Load(ctx, array, meta.BusinessInfo.GetDynamicObjectType());
			Dictionary<long, IGrouping<long, DynamicObject>> dictionary4 = (from g in source
			group g by DataEntityExtend.GetDynamicValue<long>(g, "Id", 0L)).ToDictionary((IGrouping<long, DynamicObject> d) => d.Key);
			foreach (BomExpandNodeTreeMode bomExpandNodeTreeMode in list)
			{
				IGrouping<long, DynamicObject> grouping = null;
				long num = 0L;
				long dynamicValue;
				DynamicObject dynamicObject;
				if (dictionary4.TryGetValue(DataEntityExtend.GetDynamicValue<long>(bomExpandNodeTreeMode.DataEntity, "BOMID_ID", 0L), out grouping))
				{
					if (ListUtils.IsEmpty<DynamicObject>(grouping))
					{
						continue;
					}
					num = DataEntityExtend.GetDynamicValue<long>(grouping.FirstOrDefault<DynamicObject>(), "Id", 0L);
					dynamicValue = DataEntityExtend.GetDynamicValue<long>(bomExpandNodeTreeMode.DataEntity, "MaterialId_ID", 0L);
					dynamicObject = (OrmUtils.Clone(grouping.FirstOrDefault<DynamicObject>(), false, true) as DynamicObject);
				}
				else
				{
					if (DataEntityExtend.GetDynamicValue<long>(bomExpandNodeTreeMode.DataEntity, "BOMID_ID", 0L) != 0L || !(DataEntityExtend.GetDynamicValue<string>(bomExpandNodeTreeMode.DataEntity, "ErpClsId", null) == "1") || dictionary2.ContainsKey(bomExpandNodeTreeMode.EntryId))
					{
						continue;
					}
					dynamicValue = DataEntityExtend.GetDynamicValue<long>(bomExpandNodeTreeMode.DataEntity, "MaterialId_ID", 0L);
					dynamicObject = BOMServiceHelper.CreateBomViewByMaterialId(ctx, dynamicValue);
				}
				DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "msterID", 0);
				string text = string.Empty;
				IGrouping<string, DynamicObject> source2;
				if (dictionary.TryGetValue(bomExpandNodeTreeMode.EntryId, out source2))
				{
					DynamicObject dynamicObject2 = source2.FirstOrDefault<DynamicObject>();
					if (DataEntityExtend.GetDynamicValue<bool>(dynamicObject2, "IsSelect", false) && !ObjectUtils.IsNullOrEmptyOrWhiteSpace(DataEntityExtend.GetDynamicValue<string>(dynamicObject2, "AssignBomNum", null)))
					{
						text = DataEntityExtend.GetDynamicValue<string>(dynamicObject2, "AssignBomNum", null);
					}
				}
				DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "Number", text);
				DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "DocumentStatus", "Z");
				DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "ForbidStatus", "A");
				DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "CreateDate", systemDateTime);
				DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "CreatorId_Id", base.Context.UserId);
				DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "ModifyDate", null);
				DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "ModifierId_Id", 0);
				DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "ApproveDate", null);
				DataEntityExtend.SetDynamicObjectItemValue(dynamicObject, "ApproverId_Id", 0);
				Tuple<DynamicObject, DynamicObject> value;
				if (DataEntityExtend.GetDynamicValue<long>(bomExpandNodeTreeMode.DataEntity, "BOMID_ID", 0L) == 0L && DataEntityExtend.GetDynamicValue<string>(bomExpandNodeTreeMode.DataEntity, "ErpClsId", null) == "1")
				{
					value = new Tuple<DynamicObject, DynamicObject>(dynamicObject, dynamicObject);
				}
				else
				{
					value = new Tuple<DynamicObject, DynamicObject>(grouping.FirstOrDefault<DynamicObject>(), dynamicObject);
				}
				if (!dictionary2.ContainsKey(bomExpandNodeTreeMode.EntryId))
				{
					string empty = string.Empty;
					if (dictionary3.TryGetValue(string.Format("{0}|{1}", dynamicValue, num), out empty))
					{
						if (empty != text)
						{
							dictionary2.Add(bomExpandNodeTreeMode.EntryId, value);
						}
					}
					else
					{
						dictionary2.Add(bomExpandNodeTreeMode.EntryId, value);
					}
					if (!dictionary3.ContainsKey(string.Format("{0}|{1}", dynamicValue, num)))
					{
						dictionary3.Add(string.Format("{0}|{1}", dynamicValue, num), text);
					}
				}
			}
			DBServiceHelper.AutoSetPrimaryKey(ctx, (from x in dictionary2
			select x.Value.Item2).ToArray<DynamicObject>(), meta.BusinessInfo.GetDynamicObjectType());
			return dictionary2;
		}

		// Token: 0x060009CE RID: 2510 RVA: 0x00074400 File Offset: 0x00072600
		private void UpdateTreeNode(Context ctx, DynamicObjectCollection entityDatas, List<BomExpandNodeTreeMode> allTreeNodes, Dictionary<string, Tuple<DynamicObject, DynamicObject>> srcBomFullObjects)
		{
			Dictionary<string, IGrouping<string, BomExpandNodeTreeMode>> dictionary = (from tn in allTreeNodes
			group tn by tn.ParentEntryId).ToDictionary((IGrouping<string, BomExpandNodeTreeMode> x) => x.Key);
			HashSet<string> selectedEntryIds = new HashSet<string>(from row in entityDatas
			where DataEntityExtend.GetDynamicValue<bool>(row, "IsSelect", false)
			select row into ele
			select DataEntityExtend.GetDynamicValue<string>(ele, "RowId", null));
			List<BomExpandNodeTreeMode> list = (from node in allTreeNodes
			where selectedEntryIds.Contains(node.EntryId)
			orderby node.BomLevel
			select node).ToList<BomExpandNodeTreeMode>();
			Dictionary<long, Tuple<DynamicObject, DynamicObject>> dictionary2 = new Dictionary<long, Tuple<DynamicObject, DynamicObject>>();
			foreach (KeyValuePair<string, Tuple<DynamicObject, DynamicObject>> keyValuePair in srcBomFullObjects)
			{
				long dynamicValue = DataEntityExtend.GetDynamicValue<long>(keyValuePair.Value.Item1, "Id", 0L);
				if (!dictionary2.ContainsKey(dynamicValue))
				{
					dictionary2.Add(dynamicValue, keyValuePair.Value);
				}
			}
			foreach (BomExpandNodeTreeMode bomExpandNodeTreeMode in list)
			{
				Tuple<DynamicObject, DynamicObject> tuple;
				if (!srcBomFullObjects.TryGetValue(bomExpandNodeTreeMode.EntryId, out tuple))
				{
					dictionary2.TryGetValue(DataEntityExtend.GetDynamicValue<long>(bomExpandNodeTreeMode.DataEntity, "BomId_Id", 0L), out tuple);
				}
				if (!ObjectUtils.IsNullOrEmpty(tuple))
				{
					bomExpandNodeTreeMode.BomId = tuple.Item2;
					if (bomExpandNodeTreeMode.ParentBomId != null)
					{
						DataEntityExtend.SetDynamicObjectItemValue(bomExpandNodeTreeMode.ParentBomEntry, "BOMID_Id", DataEntityExtend.GetDynamicValue<long>(bomExpandNodeTreeMode.BomId, "Id", 0L));
						DataEntityExtend.SetDynamicObjectItemValue(bomExpandNodeTreeMode.ParentBomEntry, "ChildSupplyOrgId_Id", DataEntityExtend.GetDynamicValue<long>(bomExpandNodeTreeMode.BomId, "UseOrgId_Id", 0L));
					}
					IGrouping<string, BomExpandNodeTreeMode> grouping;
					if (dictionary.TryGetValue(bomExpandNodeTreeMode.EntryId, out grouping))
					{
						DynamicObjectCollection source = bomExpandNodeTreeMode.BomId["TreeEntity"] as DynamicObjectCollection;
						Dictionary<string, IGrouping<string, DynamicObject>> dictionary3 = (from x in source
						group x by DataEntityExtend.GetDynamicValue<string>(x, "RowId", null)).ToDictionary((IGrouping<string, DynamicObject> k) => k.Key);
						foreach (BomExpandNodeTreeMode bomExpandNodeTreeMode2 in grouping)
						{
							bomExpandNodeTreeMode2.ParentBomId = tuple.Item1;
							string dynamicValue2 = DataEntityExtend.GetDynamicValue<string>(bomExpandNodeTreeMode2.ParentBomEntry, "RowId", null);
							bomExpandNodeTreeMode2.ParentBomId = tuple.Item2;
							IGrouping<string, DynamicObject> source2;
							if (dictionary3.TryGetValue(dynamicValue2, out source2))
							{
								bomExpandNodeTreeMode2.ParentBomEntryId = DataEntityExtend.GetDynamicValue<long>(source2.FirstOrDefault<DynamicObject>(), "Id", 0L);
							}
						}
					}
				}
			}
		}
	}
}
