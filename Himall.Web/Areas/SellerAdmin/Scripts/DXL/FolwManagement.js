/// <reference path="../../../Scripts/jqeury.himallLinkage.js" />


var AuditOnOff = 0;

var _saleStatus = '1';
var _auditStatus =1;
AuditOnOff = $("#VBAO").val();
//});

var categoryId;
var lastType;
var curType;
 
$(function () {

 
    bindTabSwich();
    if (val == '') {
        initGrid();
    }
    initDatePicker();
    bindSearchBtnClick();
    initBrandAutoComplete();

    $('#list').on('click', '.good-up', function () {
        var name = $(this).siblings('.thisName').val();
        var ids = $(this).siblings('.thisId').val();
        $.dialog.confirm('您要确认' + (name ? ' “' + name + '” ' : ('这' + ($.isArray(ids) ? ids.length : 1) + '次随访')) + '的随访吗？', function () {
            InitialDialog({ title: '确认随访-患者:' + name + '', name: name, id: ids })
        });


    });
    $('#list').on('click', '.good-cancel', function () {
        var name = $(this).siblings('.thisName').val();
        var ids = $(this).siblings('.thisId').val();
        $.dialog.confirm('您确定要取消' + (name ? ' “' + name + '” ' : ('这' + ($.isArray(ids) ? ids.length : 1) + '次随访')) + '的随访吗？', function () {
            var loading = showLoading();
            $.post('Cancel', { ids: ids.toString() }, function (result) {
                loading.close();
                if (result.success) {
                    $.dialog.tips('取消随访成功');
                    var pageNo = $("#list").hiMallDatagrid('options').pageNumber;
                    reload(pageNo);
                }
                else
                    $.dialog.alert('取消随访失败!' + result.msg);
            });
        });


    });
    

    $('#list').on('click', '.good-del', function () {
        var name = $(this).siblings('.thisName').val();
        var ids = $(this).siblings('.thisId').val();
        $.dialog.confirm('您确定要删除' + (name ? ' “' + name + '” ' : ('这' + ($.isArray(ids) ? ids.length : 1) + '次随访')) + '的随访吗？', function () {
            var loading = showLoading();
            $.post('Delete', { ids: ids.toString() }, function (result) {
                loading.close();
                if (result.success) {
                    $.dialog.tips('删除随访成功');
                    var pageNo = $("#list").hiMallDatagrid('options').pageNumber;
                    reload(pageNo);
                }
                else
                    $.dialog.alert('删除随访失败!' + result.msg);
            });
        });
    });

    $('#list').on('hover', '.good-share', function () {
        $(this).toggleClass('active');
    });

    $(document).on('click', '.recommend', function () {
        var $this = $(this);
        var relationProducts = $this.data('relation-products').toString();
        var exceptProducts = [];
        exceptProducts.push($this.data('product-id'));
        var productids = relationProducts.split(',');
        $.productSelector.show(productids, function (selectedProducts) {
            if (selectedProducts.length > 10) {
                $.dialog.errorTips('随访最大数不能超过10！');
                return;
            }
            relationProducts = selectedProducts.newitem(function (p) { return p.id; }).join(',');
            $this.data('relation-products', relationProducts);

            $.get('/selleradmin/product/Recommend?productId={0}&productIds={1}'.format($this.data('product-id'), relationProducts), function (data) {
                if (data && data.success == true)
                    $.dialog.succeedTips('操作成功！');
                else
                    $.dialog.errorTips('操作失败，请重试！');
            }).error(function () {
                $.dialog.errorTips('网络出错！');
            });
        }, 'selleradmin', true, exceptProducts, '#productSelectorBtns');
    }).on('click', '#selectAll', function () {
        //随访选择对话框批量选择按钮
        $.productSelector.selectAll();
    });
});


function InitialDialog(option) {
    $.dialog({
        title: option.title,
        lock: true,
        id: 'addAtrr',
        content: ['<div class="dialog-form">',
            '<div class="form-group">',
            '<label class="label-inline fl" for="">被随访人</label><input value="' + option.name + '" id="newCategoryName" class="form-control input-sm" type="text" >',

            '</div>',
            '<div class="form-group">',
            '<label class="label-inline fl" for="">随访人</label><input value="" id="newSFName" class="form-control input-sm" type="text" ><p id="nameErrorMsg" class="help-block">请填写真实姓名</p>',

            '</div>',
            '<div class="form-group">',
            '<label class="label-inline fl" for="">随访内容</label><textarea class="form-control" cols="38"   rows="3" id="Ncontent"></textarea><p id="txtErrorMsg" class="help-block">不能为空且不能多于200个字</p>',
            '</div>',
            
            '</div>'].join(''),
        
        padding: '0 40px',
        okVal: '保存',
        ok: function () {
            var len = $("#newSFName").val().length;
            if (len > 12 || len <= 0) {
                $("#nameErrorMsg").css('color', 'red');
                $("#newSFName").focus();
                return false;
            }
            var len = $("#Ncontent").val().length;
            if (len > 200 || len <= 0) {
                $("#txtErrorMsg").css('color', 'red');
                $("#Ncontent").focus();
                return false;
            }
            var name = $("#newSFName").val();
            var result = $("#Ncontent").val();
            var loading = showLoading();
            $.ajax({
                type: 'POST',
                url: 'Down',
                cache: false,
                data: { ids: option.id,lastuser: name, result: result },
                dataType: "json",
                success: function (data) {
                    loading.close();
                    if (data.success == true) {
                        $.dialog.tips('确认成功');
                        location.reload();
                    }
                    else {
                        $.dialog.errorTips(data.msg);
                    }
                }
            });

        }
    });
}
function deleteFollow(ids) {

    $.dialog.confirm('您确定要删除这些随访的随访吗？', function () {
        var loading = showLoading();
        $.post('Delete', { ids: ids.join(',').toString() }, function (result) {
            loading.close();
            if (result.success) {
                $.dialog.tips('删除随访成功');
                var pageNo = $("#list").hiMallDatagrid('options').pageNumber;
                reload(pageNo);
            }
            else
                $.dialog.alert('删除随访失败!' + result.msg);
        });
    });
}

function bindSearchBtnClick() {

    $('#searchButton').click(function (e) {
        searchClose(e);
        search();
    });
}

function bindTabSwich() {
    $('div[type="filter"]').hide();
    $('.nav-tabs-custom li').click(function (e) {
        var _t = $(this);
        searchClose(e);
        clearFilter();
        _t.addClass('active').siblings().removeClass('active');
        var params = {};
        var type = _t.attr('type');
        curType = type;
        //初始数据
        params.patientName = '';
        params.auditStatus =1;
        params.doctorName = '';
        params.typtState =2;
        params.startDate = '';
        params.endDate = '';

        //补充条次
        var girdType = 'normal';
        switch (type) {
            case 'onDown':
                params.auditStatus =1;
                normalFilter();
                break;
            case 'unCancel':
                girdType = 'cancel';
                params.auditStatus =2;
                normalFilter();
                break;
            case 'unAudit':
                params.auditStatus =0;
                girdType = 'audit';
                auditFilter();
                break;
        }
        _auditStatus = params.auditStatus;
        switch (girdType) {
            case 'normal':
                if (lastType == girdType)
                    $("#list").hiMallDatagrid('reload', params);
                else
                    initGrid(params);
                break;
            case 'audit':
                if (lastType == girdType) {
                    $("#list").hiMallDatagrid('reload', params);
                }
                else
                    initAuditGrid();
                break;
            case 'cancel':
                if (lastType == girdType) {
                    $("#list").hiMallDatagrid('reload', params);
                }
                else
                    initcancelGrid();
                break;
        }
    

    });

}

function normalFilter() {
    $('.search-box').removeClass('only-line');
    $('div[saleOff]').hide();
    $('div[audit]').hide();
    $('div[normal]').show();
    var submit = $('form.custom-inline #searchButton');
    submit.prependTo($('div.submit'));

}

function auditFilter() {
    $('.search-box').addClass('only-line');
    $('div[saleOff]').hide();
    $('div[normal]').hide();
    $('div[audit]').show();
    var submit = $('div.submit #searchButton');
    submit.appendTo(submit.parent().parent());
}

function saleOffFilter() {
    $('.search-box').addClass('only-line');
    $('div[normal]').hide();
    $('div[audit]').hide();
    $('div[saleOff]').show();
    var submit = $('div.submit #searchButton');
    submit.appendTo(submit.parent().parent());
}

function clearFilter() {
    //$('#brandBox').val('');
    //$('#searchBox').val('');
    //$('#productCode').val('');
    //$('.start_datetime').val('');
    //$('.end_datetime').val('');
    $(".search-box form")[0].reset();
    categoryId = '';
}


function initBrandAutoComplete() {
   
    
    //自动填充
    $('#brandBox').autocomplete({
        source: function (query, process) {
            var matchCount = this.options.items;//返回结果集最大数量
            $.post("../brand/getBrands", { "keyWords": $('#brandBox').val() }, function (respData) {
                return process(respData);
            });
        },
        formatItem: function (item) {
            if (item.envalue != null) {
                return item.value + "(" + item.envalue + ")";
            }
            return item.value;
        },
        setValue: function (item) {
            return { 'data-value': item.value, 'real-value': item.key };
        }
    });

}

function initcancelGrid() {

    lastType = 'cancel';
    $("#list").html('');

    //随访表格
    $("#list").hiMallDatagrid({
        url: 'list',
        nowrap: false,
        rownumbers: true,
        NoDataMsg: '没有找到符合条次的随访',
        border: false,
        fit: true,
        fitColumns: true,
        pagination: true,
        idField: "HFP_ID",
        pageSize: 9,
        pagePosition: 'bottom',
        pageNumber: 1,
        queryParams: { auditStatus: 2 },
        operationButtons: "#saleOff",
        columns:
            [[
                { checkbox: true, width: 40 },
                { field: "patientName", title: "患者", width: 90, align: "center" },
                { field: "doctorName", title: "医生", width: 90, align: "center" },
                { field: "HFP_Name", title: "随访名称", width: 90, align: "center" },
                { field: "RemindType", title: "提醒方式", width: 90, align: "center" },
                { field: "HFP_Date", title: "随访时间", width: 90, align: "center" },
                { field: "State", title: "随访状态", width: 90, align: "center" },
                {
                    field: "s", title: "操作", width: 120, align: "center",
                    formatter: function (value, row, index) {
                        html = '<span class="btn-a text-left inline-block"><input class="thisId" type="hidden" value="' + row.HFP_ID + '"/><input class="thisName" type="hidden" value="' + row.patientName + '"/>';
                        html += '<a class="good-del">删除</a>';
                        return html;
                    }
                }
            ]],
        onLoadSuccess: function () {
            initBatchBtnShow();

        }
    });
}

function initAuditGrid() {
    
    lastType = 'audit';
    $("#list").html('');

    //随访表格
    $("#list").hiMallDatagrid({
        url: 'list',
        nowrap: false,
        rownumbers: true,
        NoDataMsg: '没有找到符合条次的随访',
        border: false,
        fit: true,
        fitColumns: true,
        pagination: true,
        idField: "HFP_ID",
        pageSize: 9,
        pagePosition: 'bottom',
        pageNumber: 1,
        queryParams:{ auditStatus: 0 },
        operationButtons: "#saleOff",
        columns:
            [[
                { checkbox: true, width: 40 },
                { field: "patientName", title: "患者", width: 90, align: "center" },
                { field: "doctorName", title: "医生", width: 90, align: "center" },
                { field: "HFP_Name", title: "随访名称", width: 90, align: "center" },
                { field: "RemindType", title: "提醒方式", width: 90, align: "center" },
                { field: "HFP_Date", title: "随访时间", width: 90, align: "center" },
                { field: "State", title: "随访状态", width: 90, align: "center" },
                {
                    field: "s", title: "操作", width: 120, align: "center",
                    formatter: function (value, row, index) {
                        html = '<span class="btn-a text-left inline-block"><input class="thisId" type="hidden" value="' + row.HFP_ID + '"/><input class="thisName" type="hidden" value="' + row.patientName + '"/>';
                        //if (!row.IsLimitTimeBuy) {
                        //    html += '<a class="good-check" href="edit/' + row.Id + '">编辑</a>';
                        //}
                        if (row.HFP_State == 1) {
                            html += '<a class="good-down" href="FollowPlanDetail?id=' + row.HFP_ID+'">查看</a>';
                        } else if (row.HFP_State == 0) {
                            html += '<a class="good-cancel">取消随访</a><a class="good-up">确认随访</a><a class="good-del">删除</a>';
                        }
                        return html;
                    }
                }
            ]],
        onLoadSuccess: function () {
            initBatchBtnShow();
            
        }
    });
}

//初始化查询
function initGrid(params) {

    console.log(params);
    lastType = 'normal';
    $("#list").html('');
    normalFilter();
    $("#list").hiMallDatagrid({
        url: 'list',
        nowrap: false,
        rownumbers: true,
        NoDataMsg: '没有找到符合条次的随访',
        border: false,
        fit: true,
        fitColumns: true,
        pagination: true,
        idField: "HFP_ID",
        pageSize: 9,
        pagePosition: 'bottom',
        pageNumber: 1,
        queryParams: params ? params : {auditStatus:1},
        operationButtons: "#saleOff",
        columns:
            [[
                { checkbox: true, width: 40 },
                { field: "patientName", title: "患者", width: 90, align: "center" },
                { field: "doctorName", title: "医生", width: 90, align: "center" },
                { field: "HFP_Name", title: "随访名称", width: 90, align: "center" },
                { field: "RemindType", title: "提醒方式", width: 90, align: "center" },
                { field: "HFP_Date", title: "随访时间", width: 90, align: "center" },
                { field: "State", title: "随访状态", width: 90, align: "center" },
                {
                    field: "s", title: "操作", width: 120, align: "center",
                    formatter: function (value, row, index) {
                        html = '<span class="btn-a text-left inline-block"><input class="thisId" type="hidden" value="' + row.HFP_ID + '"/><input class="thisName" type="hidden" value="' + row.patientName + '"/>';
                        html += '<a   href="FollowPlanDetail?id=' + row.HFP_ID + '">查看</a>';
                        return html;
                    }
                }
            ]],
        onLoadSuccess: function () {
            initBatchBtnShow();
           
        }
    });
}

 
//搜索
function search() {
    var patientNames = $.trim($('#PatientName').val());//患者姓名
    var doctorNames = $.trim($('#DoctorName').val());//医生姓名
    var typtStates = $('select[name="typtState"]').val();//提醒方式
    var startDate = $('.start_datetime').val();
    var endDate = $('.end_datetime').val();
    var auditState = _auditStatus;//随访状态
    $("#list").hiMallDatagrid('clearReload', {
        patientName: patientNames, doctorName: doctorNames, typtState: typtStates,
        startDate: startDate, endDate: endDate, auditStatus: auditState,
    });
}

function reload(pageNo) {

    $("#list").hiMallDatagrid('reload', { pageNumber: pageNo });
}

function getSelectedIds() {
    var selecteds = $("#list").hiMallDatagrid('getSelections');
    var ids = [];
    $.each(selecteds, function () {
        ids.push(this.HFP_ID);
    });
    return ids;
}
//取消随访
function CancelOff(ids) {
    $.dialog.confirm('您确定要取消这些随访的随访吗？', function () {
        var loading = showLoading();
        $.post('Cancel', { ids: ids.join(',').toString() }, function (result) {
            loading.close();
            if (result.success) {
                $.dialog.tips('取消随访成功');
                var pageNo = $("#list").hiMallDatagrid('options').pageNumber;
                reload(pageNo);
            }
            else
                $.dialog.alert('取消随访失败!' + result.msg);
        });
    });
}


function initBatchBtnShow() {
    var type = $('.nav-tabs-custom li.active').attr('type');
    if (type == 'onDown') {//当前为使用中tab
        //$('#batchSaleOff')
        //    .show()
        //    .unbind('click')
        //    .click(function () {
        //        var ids = getSelectedIds();
        //        if (ids.length > 0)
        //            saleOff(ids);
        //        else
        //            $.dialog.tips('请至少选择一次随访');
        //    });
       // $('#batchDelete').hide();
    }
    //else if (type == 'unCancel') {
    //    $('#batchOnSale')
    //        .show()
    //        .unbind('click')
    //        .click(function () {
    //            var ids = getSelectedIds();
    //            if (ids.length > 0)
    //                onSale(ids);
    //            else
    //                $.dialog.tips('请至少选择一次随访');
    //        });
    //}

    if (type == 'unAudit') {

          $('#batchSaleOff')
            .show()
            .unbind('click')
            .click(function () {
                var ids = getSelectedIds();
                if (ids.length > 0)
                    CancelOff(ids);
                else
                    $.dialog.tips('请至少选择一次随访');
            });
        $('#associateTemplate').hide();
    }
    else
        $('#associateTemplate').show();
    $('#batchDelete')
        .unbind('click')
        .click(function () {
            var ids = getSelectedIds();
            if (ids.length > 0)
                deleteFollow(ids);
            else
                $.dialog.tips('请至少选择一项');
        });
}

function initDatePicker() {

    $(".start_datetime").datetimepicker({
        language: 'zh-CN',
        format: 'yyyy-mm-dd',
        autoclose: true,
        weekStart: 1,
        minView: 2
    });
    $(".end_datetime").datetimepicker({
        language: 'zh-CN',
        format: 'yyyy-mm-dd',
        autoclose: true,
        weekStart: 1,
        minView: 2
    });
    //$(".start_datetime").click(function () {
    //    $('.end_datetime').datetimepicker('show');
    //});
    //$(".end_datetime").click(function () {
    //    $('.start_datetime').datetimepicker('show');
    //});

    $('.start_datetime').on('changeDate', function () {
        if ($(".end_datetime").val()) {
            if ($(".start_datetime").val() > $(".end_datetime").val()) {
                $('.end_datetime').val($(".start_datetime").val());
            }
        }

        $('.end_datetime').datetimepicker('setStartDate', $(".start_datetime").val());
    });

}

 

function clearGridSelect() {
    $("#list").hiMallDatagrid('clearSelections');
}



