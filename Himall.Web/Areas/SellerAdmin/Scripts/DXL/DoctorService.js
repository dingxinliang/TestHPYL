/// <reference path="../../../Scripts/jqeury.himallLinkage.js" />


var AuditOnOff = 0;
var val = "";
var _saleStatus = '1';
var _auditStatus = 1;
AuditOnOff = 1;
//});

var categoryId;
var lastType;
var curType;

$(function () {


    bindTabSwich();
    if (val == '') {
        initGrid();
    }
    bindSearchBtnClick();
    initBrandAutoComplete();

    $('#list').on('click', '.good-up', function () {
        var name = $(this).siblings('.thisName').val();
        var ids = $(this).siblings('.thisId').val();
        $.dialog.confirm('您要确认' + (name ? ' “' + name + '” ' : ('这' + ($.isArray(ids) ? ids.length : 1) + '次数据')) + '的数据吗？', function () {
            InitialDialog({ title: '确认数据-患者:' + name + '', name: name, id: ids })
        });


    });
    $('#list').on('click', '.good-cancel', function () {
        var name = $(this).siblings('.thisName').val();
        var ids = $(this).siblings('.thisId').val();
        $.dialog.confirm('您确定要取消' + (name ? ' “' + name + '” ' : ('这' + ($.isArray(ids) ? ids.length : 1) + '次数据')) + '的数据吗？', function () {
            var loading = showLoading();
            $.post('Cancel', { ids: ids.toString() }, function (result) {
                loading.close();
                if (result.success) {
                    $.dialog.tips('取消数据成功');
                    var pageNo = $("#list").hiMallDatagrid('options').pageNumber;
                    reload(pageNo);
                }
                else
                    $.dialog.alert('取消数据失败!' + result.msg);
            });
        });


    });


    $('#list').on('click', '.good-del', function () {
        var name = $(this).siblings('.thisName').val();
        var ids = $(this).siblings('.thisId').val();
        $.dialog.confirm('您确定要删除' + (name ? ' “' + name + '” ' : ('这' + ($.isArray(ids) ? ids.length : 1) + '次数据')) + '的数据吗？', function () {
            var loading = showLoading();
            $.post('Delete', { ids: ids.toString() }, function (result) {
                loading.close();
                if (result.success) {
                    $.dialog.tips('删除数据成功');
                    var pageNo = $("#list").hiMallDatagrid('options').pageNumber;
                    reload(pageNo);
                }
                else
                    $.dialog.alert('删除数据失败!' + result.msg);
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
                $.dialog.errorTips('数据最大数不能超过10！');
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
        //数据选择对话框批量选择按钮
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
            '<label class="label-inline fl" for="">被数据人</label><input value="' + option.name + '" id="newCategoryName" class="form-control input-sm" type="text" >',

            '</div>',
            '<div class="form-group">',
            '<label class="label-inline fl" for="">数据人</label><input value="" id="newSFName" class="form-control input-sm" type="text" ><p id="nameErrorMsg" class="help-block">请填写真实姓名</p>',

            '</div>',
            '<div class="form-group">',
            '<label class="label-inline fl" for="">数据内容</label><textarea class="form-control" cols="38"   rows="3" id="Ncontent"></textarea><p id="txtErrorMsg" class="help-block">不能为空且不能多于200个字</p>',
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
                data: { ids: option.id, lastuser: name, result: result },
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

    $.dialog.confirm('您确定要删除这些数据的数据吗？', function () {
        var loading = showLoading();
        $.post('Delete', { ids: ids.join(',').toString() }, function (result) {
            loading.close();
            if (result.success) {
                $.dialog.tips('删除数据成功');
                var pageNo = $("#list").hiMallDatagrid('options').pageNumber;
                reload(pageNo);
            }
            else
                $.dialog.alert('删除数据失败!' + result.msg);
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
        params.UserName = '';
        params.auditStatus =0;
        params.UserRealName = '';
        params.UserCell = '';

        //补充条件
        var girdType = 'normal';
        switch (type) {
            case 'onDown':
                params.auditStatus =0;
                normalFilter();
                break;
            case 'unAudit':
                params.auditStatus =1;
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
        }


    });

}

function Lock(id) {
    $.dialog.confirm('冻结之后，医生将不能登录，您确定冻结？', function () {
        var loading = showLoading();
        $.post("./Lock", { id: id }, function (data) {
            $.dialog.tips(data.msg); var pageNo = $("#list").hiMallDatagrid('options').pageNumber;
            reload(pageNo);  loading.close(); });
    });
}
function UnLock(id) {
    $.dialog.confirm('确定重新激活该用户吗？', function () {
        var loading = showLoading();
        $.post("./UnLock", { id: id }, function (data) {
            $.dialog.tips(data.msg); var pageNo = $("#list").hiMallDatagrid('options').pageNumber;
            reload(pageNo);  loading.close(); });
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
function initAuditGrid() {

    lastType = 'audit';
    $("#list").html('');

    //数据表格
    $("#list").hiMallDatagrid({
        url: 'list',
        nowrap: false,
        rownumbers: true,
        NoDataMsg: '没有找到符合条件的数据',
        border: false,
        fit: true,
        fitColumns: true,
        pagination: true,
        idField: "Id",
        pageSize: 9,
        pagePosition: 'bottom',
        pageNumber: 1,
        queryParams: { auditStatus: 1 },
        operationButtons: "#saleOff",
        columns:
            [[
                { checkbox: false, width: 40 },
                { field: "UserName", title: "医生账号", width: 90, align: "center" },
                { field: "RealName", title: "真实姓名", width: 90, align: "center" },
                { field: "Nick", title: '微信昵称' },
                { field: "CellPhone", title: "电话", width: 90, align: "center" },
                {
                    field: "Disabled", title: '状态',
                    formatter: function (value, row, index) {
                        var html = "";
                        if (row.Disabled)
                            html += '冻结';
                        else
                            html += '正常';
                        return html;
                    }
                },
                { field: "CreateDateStr", title: "注册时间", width: 90, align: "center" },
                {
                    field: "s", title: "操作", width: 120, align: "center",
                    formatter: function (value, row, index) {
                        html = '<span class="btn-a text-left inline-block"><input class="thisId" type="hidden" value="' + row.Id + '"/><input class="thisName" type="hidden" value="' + row.UserName + '"/>';
                        if (row.Disabled == 1)
                            html += "<a onclick=\"UnLock('" + row.Id + "');\">解冻</a>";
                        else
                            html += "<a onclick=\"Lock('" + row.Id + "');\">冻结</a>";
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

   
    lastType = 'normal';
    $("#list").html('');
    normalFilter();
    $("#list").hiMallDatagrid({
        url: 'list',
        nowrap: false,
        rownumbers: true,
        NoDataMsg: '没有找到符合条件的数据',
        border: false,
        fit: true,
        fitColumns: true,
        pagination: true,
        idField: "Id",
        pageSize: 9,
        pagePosition: 'bottom',
        pageNumber: 1,
        queryParams: params ? params : { auditStatus:0 },
        operationButtons: "#saleOff",
        columns:
            [[
                { checkbox: false, width: 40 },
                { field: "UserName", title: "医生账号", width: 90, align: "center" },
                { field: "RealName", title: "真实姓名", width: 90, align: "center" },
                { field: "Nick", title: '微信昵称' },
                { field: "CellPhone", title: "电话", width: 90, align: "center" },
                {
                    field: "Disabled", title: '状态',
                    formatter: function (value, row, index) {
                        var html = "";
                        if (row.Disabled)
                            html += '冻结';
                        else
                            html += '正常';
                        return html;
                    }
                },
                { field: "CreateDateStr", title: "注册时间", width: 90, align: "center" },
                {
                    field: "s", title: "操作", width: 120, align: "center",
                    formatter: function (value, row, index) {
                        html = '<span class="btn-a text-left inline-block"><input class="thisId" type="hidden" value="' + row.Id + '"/><input class="thisName" type="hidden" value="' + row.UserName + '"/>';
                        if (row.Disabled==1)
                            html+="<a onclick=\"UnLock('" + row.Id + "');\">解冻</a>";
                        else
                            html +="<a onclick=\"Lock('" + row.Id + "');\">冻结</a>";
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
    var UserName = $.trim($('#UserName').val());//账号
    var UserRealName = $.trim($('#UserRealName').val());//医生姓名
    var UserCell = $.trim($('#UserCell').val());//注册手机号
    var auditState = _auditStatus;//禁用状态
    $("#list").hiMallDatagrid('clearReload', {
        UserName: UserName, UserRealName: UserRealName,
        UserCell: UserCell, auditStatus: auditState,
    });
}

function reload(pageNo) {

    $("#list").hiMallDatagrid('reload', { pageNumber: pageNo });
}

function getSelectedIds() {
    var selecteds = $("#list").hiMallDatagrid('getSelections');
    var ids = [];
    $.each(selecteds, function () {
        ids.push(this.Id);
    });
    return ids;
}
//取消数据
function CancelOff(ids) {
    $.dialog.confirm('您确定要取消这些数据的数据吗？', function () {
        var loading = showLoading();
        $.post('Cancel', { ids: ids.join(',').toString() }, function (result) {
            loading.close();
            if (result.success) {
                $.dialog.tips('取消数据成功');
                var pageNo = $("#list").hiMallDatagrid('options').pageNumber;
                reload(pageNo);
            }
            else
                $.dialog.alert('取消数据失败!' + result.msg);
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
        //            $.dialog.tips('请至少选择一次数据');
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
    //                $.dialog.tips('请至少选择一次数据');
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
                    $.dialog.tips('请至少选择一次数据');
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



