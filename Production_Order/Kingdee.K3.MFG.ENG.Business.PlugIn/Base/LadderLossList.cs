using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Core.Validation;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.MFG.ENG.Business.PlugIn.Utils;
using Kingdee.K3.MFG.ServiceHelper.ENG;

namespace Kingdee.K3.MFG.ENG.Business.PlugIn.Base
{
	// Token: 0x02000013 RID: 19
	[Description("物料阶梯损耗列表插件")]
	public class LadderLossList : BaseControlList
	{
		// Token: 0x0600021A RID: 538 RVA: 0x00019AE8 File Offset: 0x00017CE8
		public override void AfterBarItemClick(AfterBarItemClickEventArgs e)
		{
			base.AfterBarItemClick(e);
			string[] primaryKeyValues = this.ListView.SelectedRowsInfo.GetPrimaryKeyValues();
			string a;
			if ((a = e.BarItemKey.ToUpperInvariant()) != null)
			{
				if (!(a == "TBUPDATE"))
				{
					if (!(a == "TBGETDATA"))
					{
						return;
					}
					if (LadderLossUtils.IsGetDataPermission(base.Context))
					{
						if (LadderLossUtils.IsModifyPermisson(base.Context, "ENG_LadderLoss"))
						{
							DynamicObjectCollection bomData = LadderLossServiceHelper.GetBomData(base.Context, base.Context.CurrentOrganizationInfo.ID);
							DynamicFormShowParameter dynamicFormShowParameter = new DynamicFormShowParameter();
							dynamicFormShowParameter.OpenStyle.ShowType = 0;
							dynamicFormShowParameter.FormId = "ENG_BOMLIST";
							dynamicFormShowParameter.CustomComplexParams.Add("Enable", true);
							dynamicFormShowParameter.CustomComplexParams.Add("title", ResManager.LoadKDString("物料清单阶梯用量更新至物料阶梯损耗", "0151515153499000018595", 7, new object[0]));
							dynamicFormShowParameter.CustomComplexParams.Add("data", bomData);
							this.View.ShowForm(dynamicFormShowParameter, delegate(FormResult FormResult)
							{
								Dictionary<string, object> dictionary = FormResult.ReturnData as Dictionary<string, object>;
								bool flag = Convert.ToBoolean(dictionary["isUpdate"]);
								if (flag)
								{
									List<IOperationResult> list = LadderLossUtils.UpdateLadderLoss(base.Context, this.View, dictionary);
									bool flag2 = true;
									StringBuilder stringBuilder = new StringBuilder();
									foreach (IOperationResult operationResult in list)
									{
										if (!operationResult.IsSuccess)
										{
											flag2 = false;
											List<ValidationErrorInfo> fatalErrorResults = operationResult.GetFatalErrorResults();
											foreach (ValidationErrorInfo validationErrorInfo in fatalErrorResults)
											{
												stringBuilder.AppendLine(validationErrorInfo.Message);
											}
										}
									}
									if (flag2)
									{
										FormMetadata formMetadata = MetaDataServiceHelper.Load(base.Context, "ENG_LadderLoss", true) as FormMetadata;
										LadderLossUtils.WriteOperateLog(base.Context, ResManager.LoadKDString("获取物料清单阶梯用量", "0151515153499030038886", 7, new object[0]), ResManager.LoadKDString("获取物料清单的阶梯用量信息", "0151515153499030038887", 7, new object[0]), formMetadata.BusinessInfo, "ENG_LadderLoss");
										this.View.ShowMessage(ResManager.LoadKDString("更新成功", "0151515153499030038769", 7, new object[0]), 0);
										this.View.Refresh();
										return;
									}
									this.View.ShowMessage(stringBuilder.ToString(), 4);
									stringBuilder.Clear();
								}
							});
							return;
						}
						this.View.ShowErrMessage(ResManager.LoadKDString("当前用户没有【物料阶梯损耗修改】权限，请联系管理员授权！", "0151515153499000018753", 7, new object[0]), "", 0);
						return;
					}
					else
					{
						this.View.ShowMessage(ResManager.LoadKDString("当前用户没有【获取物料清单阶梯用量】权限，请联系管理员授权！", "0151515153499030038838", 7, new object[0]), 4);
					}
				}
				else
				{
					Dictionary<long, DynamicObject> LadderInfoDictionary = new Dictionary<long, DynamicObject>();
					if (!LadderLossUtils.IsUpdatePermission(base.Context))
					{
						this.View.ShowMessage(ResManager.LoadKDString("当前用户没有【更新物料清单】权限，请联系管理员授权！", "0151515153499030038837", 7, new object[0]), 4);
						return;
					}
					if (!LadderLossUtils.IsModifyPermisson(base.Context, "ENG_BOM"))
					{
						this.View.ShowErrMessage(ResManager.LoadKDString("当前用户没有【物料清单修改】权限，请联系管理员授权！", "0151515153499000018754", 7, new object[0]), "", 0);
						return;
					}
					if (primaryKeyValues.Length < 1)
					{
						this.View.ShowMessage(ResManager.LoadKDString("没有选择任何数据，请选择数据！", "0151515153499000018573", 7, new object[0]), 4);
						return;
					}
					object[] fids = Array.ConvertAll<string, object>(primaryKeyValues, (string s) => s);
					DynamicObject[] dataObject = LadderLossUtils.GetDataObject(base.Context, fids, "ENG_LadderLoss");
					if (!ObjectUtils.IsNullOrEmpty((from x in dataObject
					where !x["DocumentStatus"].Equals("C")
					select x).FirstOrDefault<DynamicObject>()))
					{
						this.View.ShowMessage(ResManager.LoadKDString("勾选了未审核的单据，请重新选择", "0151515153499030046227", 7, new object[0]), 0);
						return;
					}
					if (!ObjectUtils.IsNullOrEmpty((from x in dataObject
					where !x["ForbidStatus"].Equals("A")
					select x).FirstOrDefault<DynamicObject>()))
					{
						this.View.ShowMessage(ResManager.LoadKDString("勾选了已禁用的单据，请重新选择", "0151515153499000026360", 7, new object[0]), 0);
						return;
					}
					List<long> source = (from t in dataObject
					select DataEntityExtend.GetDynamicValue<long>(t, "MATERIALID_ID", 0L)).ToList<long>();
					if (source.Distinct<long>().Count<long>() != source.Count<long>())
					{
						this.View.ShowMessage(ResManager.LoadKDString("被勾选的阶梯损耗中，存在同组织下同编码的物料，无法判断根据哪个阶梯损耗做更新，请重新选择", "0151515153499030046229", 7, new object[0]), 0);
						return;
					}
					LadderInfoDictionary = dataObject.ToDictionary((DynamicObject x) => DataEntityExtend.GetDynamicValue<long>(x, "MATERIALID_ID", 0L));
					DynamicObjectCollection value = LadderLossServiceHelper.BatchGetBomData(base.Context, source.Distinct<long>().ToList<long>());
					DynamicFormShowParameter dynamicFormShowParameter2 = new DynamicFormShowParameter();
					dynamicFormShowParameter2.OpenStyle.ShowType = 0;
					dynamicFormShowParameter2.FormId = "ENG_BOMLIST";
					dynamicFormShowParameter2.CustomComplexParams.Add("Enable", false);
					dynamicFormShowParameter2.CustomComplexParams.Add("title", ResManager.LoadKDString("物料阶梯损耗更新物料清单阶梯用量", "0151515153499030038686", 7, new object[0]));
					dynamicFormShowParameter2.CustomComplexParams.Add("data", value);
					this.View.ShowForm(dynamicFormShowParameter2, delegate(FormResult FormResult)
					{
						StringBuilder stringBuilder = new StringBuilder();
						Dictionary<string, object> dictionary = FormResult.ReturnData as Dictionary<string, object>;
						bool flag = Convert.ToBoolean(dictionary["isUpdate"]);
						if (flag)
						{
							List<List<LadderLossUtils.DataList>> list = (from u in DictionaryUtils.GetValue<List<LadderLossUtils.DataList>>(dictionary, "datalist")
							group u by u.materialidList into grp
							select grp.ToList<LadderLossUtils.DataList>()).ToList<List<LadderLossUtils.DataList>>();
							foreach (List<LadderLossUtils.DataList> list2 in list)
							{
								dictionary["datalist"] = list2;
								long materialidList = list2.FirstOrDefault<LadderLossUtils.DataList>().materialidList;
								IOperationResult operationResult = LadderLossUtils.UpdateBom(this.Context, dictionary, LadderInfoDictionary[materialidList], LadderInfoDictionary[materialidList]["LADDERLOSSENTRY"] as DynamicObjectCollection);
								bool flag2 = true;
								StringBuilder stringBuilder2 = new StringBuilder();
								if (!operationResult.IsSuccess)
								{
									List<ValidationErrorInfo> fatalErrorResults = operationResult.GetFatalErrorResults();
									flag2 = false;
									foreach (ValidationErrorInfo validationErrorInfo in fatalErrorResults)
									{
										stringBuilder2.AppendLine(validationErrorInfo.Message);
									}
								}
								if (flag2)
								{
									FormMetadata formMetadata = MetaDataServiceHelper.Load(this.Context, "ENG_LadderLoss", true) as FormMetadata;
									LadderLossUtils.WriteOperateLog(this.Context, ResManager.LoadKDString("更新物料清单", "0151515153499030038885", 7, new object[0]), ResManager.LoadKDString("更新物料清单的梯用量信息", "0151515153499030038888", 7, new object[0]), formMetadata.BusinessInfo, "ENG_LadderLoss");
								}
								else
								{
									stringBuilder.Append(stringBuilder2.ToString());
									stringBuilder2.Clear();
								}
							}
							if (ObjectUtils.IsNullOrEmptyOrWhiteSpace(stringBuilder) && stringBuilder.Length > 0)
							{
								this.View.ShowMessage(stringBuilder.ToString(), 4);
								return;
							}
							this.View.ShowMessage(ResManager.LoadKDString("更新成功", "0151515153499030038769", 7, new object[0]), 0);
						}
					});
					return;
				}
			}
		}

		// Token: 0x0600021B RID: 539 RVA: 0x00019F6C File Offset: 0x0001816C
		public override void BeforeDoOperation(BeforeDoOperationEventArgs e)
		{
			base.BeforeDoOperation(e);
			string text = e.Operation.FormOperation.Operation.ToUpperInvariant();
			string a;
			if ((a = text) != null)
			{
				if (!(a == "COPY"))
				{
					return;
				}
				bool flag = this.IsCurrentOrgDisable();
				if (flag)
				{
					this.View.ShowErrMessage(ResManager.LoadKDString("当前组织未审核或已禁用，不允许新增。", "0151515153499030042470", 7, new object[0]), "", 0);
					e.Cancel = true;
				}
			}
		}

		// Token: 0x0600021C RID: 540 RVA: 0x00019FF0 File Offset: 0x000181F0
		private bool IsCurrentOrgDisable()
		{
			QueryBuilderParemeter queryBuilderParemeter = new QueryBuilderParemeter();
			queryBuilderParemeter.FormId = "ORG_Organizations";
			queryBuilderParemeter.SelectItems = SelectorItemInfo.CreateItems("FFORBIDSTATUS");
			queryBuilderParemeter.FilterClauseWihtKey = "FORGID=" + base.Context.CurrentOrganizationInfo.ID;
			DynamicObjectCollection dynamicObjectCollection = QueryServiceHelper.GetDynamicObjectCollection(base.Context, queryBuilderParemeter, null);
			if (!ListUtils.IsEmpty<DynamicObject>(dynamicObjectCollection))
			{
				string a = (from o in dynamicObjectCollection
				select DataEntityExtend.GetDynamicValue<string>(o, "FFORBIDSTATUS", null)).FirstOrDefault<string>();
				if (a == "B")
				{
					return true;
				}
			}
			return false;
		}
	}
}
