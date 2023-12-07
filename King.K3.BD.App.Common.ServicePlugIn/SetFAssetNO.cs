using Kingdee.BOS.App.Core;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using System.ComponentModel;


namespace King.K3.BD.App.Common.ServicePlugIn.CardSplit
{
    [Description("设置编码")]
    [Kingdee.BOS.Util.HotUpdate]
    public class SetFAssetNO : AbstractBillPlugIn
    {
        public override void DataChanged(DataChangedEventArgs e)
        {
            base.DataChanged(e);

            DynamicObject org = (DynamicObject)this.View.Model.GetValue("FASSETORGID");

            if (org["Id"].ToString().Equals("1") || org["Id"].ToString().Equals("1431804"))
            {
                if (e.Field.Key == "FAssetTypeID")
                {
                    BusinessDataService dataService = new BusinessDataService();
                    var businInfo = View.BillBusinessInfo;
                    var dataObjs = new DynamicObject[] { this.Model.DataObject };
                    bool isUpdateMax = false;
                    const string specifiedRuleId = "0050568273ec904911e347b7d6dd9d1a";
                    var billNoList = dataService.GetBillNo(Context, businInfo, dataObjs, isUpdateMax, specifiedRuleId);
                    this.Model.SetValue("FNumber", billNoList[0].BillNo);
                    this.View.UpdateView("FNumber");
                    this.View.Model.SetValue("FASSETNO", this.View.Model.GetValue("FNumber"));
                    this.View.UpdateView("FASSETNO");
                    this.View.Model.SetValue("FAllocAssetNO", this.View.Model.GetValue("FNumber"));
                    this.View.UpdateView("FAllocAssetNO");
                }
                
            }



        }
    }
}
