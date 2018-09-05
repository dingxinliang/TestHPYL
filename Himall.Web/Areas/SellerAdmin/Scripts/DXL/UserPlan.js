$(function () {
    if (!$.isNullOrEmpty(GetQueryString("id"))) {
         
        FlowPlanView(GetQueryString("id"));
    }

})


//查看
function FlowPlanView(ids) {
    $.post("UserFollowPlan", { "ids": ids }, function (respData) {
        var data = respData.FollowPlanList;
       
        var str = "";
        $("#list").empty();
        $.each(data, function (i, n) {
            str += "<form class=\"form-horizontal\" role=\"form\">";
            str += "<table class=\"table\">";
            str += "<thead>";
            str += "<tr>";
            str += "<th colspan=\"2\"";
            str += "<h3>" + (n.HFP_Date ? n.HFP_Date : n.HFP_CreateTime) + "随访[" + n.State + "]</h3>";
            str += "</th>";
            str += "</tr>";
            str += " </thead>";
            str += "<tbody>";
            str += "<tr>";
            str += " <td align=\"right\" width=\"15% \">患者姓名</td>";
            str += "<td>" + n.patientName + " </td>";
            str += "</tr>";
            str += "<tr>";
            str += "<td align=\"right\">医生姓名</td>";
            str += "<td>" + n.doctorName + " </td>";
            str += "</tr>";
            str += "<tr>";
            str += "<td align=\"right\">随访时间</td>";
            str += "<td>" + (n.HFP_Date ? n.HFP_Date:"/") + " </td>";
            str += "</tr>";
            str += "<tr>";
            str += "<td align=\"right\">随访名称</td>";
            str += " <td>" + n.HFP_Name + " </td>";
            str += "</tr>";
            str += "<tr>";
            str += "<td align=\"right\">随访内容</td>";
            str += " <td>" + n.HFP_Content + " </td>";
            str += "</tr>";
            str += "<tr>";
            str += "<td align=\"right\">随访人</td>";
            str += "<td>" + (n.HFP_LastUser ? n.HFP_LastUser:"/") + " </td>";
            str += "</tr>";
            str += " <tr>";
            str += "<td align=\"right\">随访结论</td>";
            str += "<td>" + (n.HFP_Result ? n.HFP_Result:"/") + " </td>";
            str += "</tr>";
            str += "</tbody>";
            str += "</table >";
            str += " </form >";
        });
        $("#list").append(str);
    });

}