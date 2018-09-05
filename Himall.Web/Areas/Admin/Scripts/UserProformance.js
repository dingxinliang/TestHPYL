﻿$(function () {
    query();
    $("#searchBtn").click(function () { query(); });
    $(".daySpan").bind("click", function () {
        $("#list").hiMallDatagrid('reload', { userId: $("#userId").val(), orderId: $("#orderId").val(), days: $(this).data("day") });
    });
})

function query() {
    $("#list").hiMallDatagrid({
        url: '../UserPerformanceList',
        nowrap: false,
        rownumbers: true,
        NoDataMsg: '没有找到符合条件的数据',
        border: false,
        fit: true,
        fitColumns: true,
        pagination: true,
        idField: "Id",
        pageSize: 10,
        pageNumber: 1,
        queryParams: { userId: $("#userId").val(), orderId: $("#orderId").val(), days: 0 },
        toolbar: /*"#goods-datagrid-toolbar",*/'',
        columns:
        [[
            { field: "OrderIdString", title: '预约单编号' },
            { field: "ProductName", title: '诊疗项目名称' },
            { field: "RealTotalPrice", title: '结算金额',
                formatter: function (value, row, index) {
                    var html = row.RealTotalPrice - row.RealTotalPriceRefund;
                    return html;
                } },
            { field: "OrderStatusDesc", title: '预约单状态' },
            { field: "OrderTimeString", title: '下单时间' },
            { field: "ExpriedStatus", title: '是否已过维权期' },
            { field: "Brokerage", title: '结算佣金',
                formatter: function (value, row, index) {
                    var html = row.Brokerage - row.BrokerageRefund;
                    return html;
                }
            },
        ]]
    });
}


