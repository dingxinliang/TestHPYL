/// <reference path="../../../Scripts/jqeury.himallLinkage.js" />


var AuditOnOff = 0;
var _saleStatus = '1';
var _auditStatus =1;
var val = "";
AuditOnOff = $("#VBAO").val();
var categoryId;
var lastType;
var curType;
var fgstock;
$(function () {
    fgstock = $('.fg-stock input:checkbox').get(0);
    bindTabSwich();
    if (val == '') {
        initGrid();
    }
    bindSearchBtnClick();
    initCategoryLinkage();
 


    $('#list').on('click', '.good-down', function () {
        var name = $(this).siblings('.thisName').val();
        var ids = $(this).siblings('.thisId').val();
        $.dialog.confirm('您确定要停用' + (name ? ' “' + name + '” ' : ('这' + ($.isArray(ids) ? ids.length : 1) + '件模板')) + '吗？', function () {
            var loading = showLoading();
            $.post('batchStop', { ids: ids.toString() }, function (result) {
                loading.close();
                if (result.success) {
                    $.dialog.tips('停用模板成功');
                    var pageNo = $("#list").hiMallDatagrid('options').pageNumber;
                    reload(pageNo);
                }
                else
                    $.dialog.alert('停用模板失败!' + result.msg);
            });
        });


    });

    $('#list').on('click', '.good-up', function () {
        var name = $(this).siblings('.thisName').val();
        var ids = $(this).siblings('.thisId').val();
        if (curType == 'saleOff' && AuditOnOff == 0) {
            $.dialog.alert('违规停用的模板，不能再使用!');
            return;
        }
        $.dialog.confirm('您确定要使用' + (name ? ' “' + name + '” ' : ('这' + ($.isArray(ids) ? ids.length : 1) + '件模板')) + '吗？', function () {
            var loading = showLoading();
            $.post('batchOn', { ids: ids.toString() }, function (result) {
                loading.close();
                if (result.success) {
                    $.dialog.tips('申请模板使用成功');
                    var pageNo = $("#list").hiMallDatagrid('options').pageNumber;
                    reload(pageNo);
                }
                else
                    $.dialog.alert('申请模板使用失败!' + result.msg);
            });
        });
    });

    $('#list').on('click', '.good-del', function () {
        var name = $(this).siblings('.thisName').val();
        var ids = $(this).siblings('.thisId').val();
       
        $.dialog.confirm('您确定要删除' + (name ? ' “' + name + '” ' : ('这' + ($.isArray(ids) ? ids.length : 1) + '件模板')) + '吗？', function () {
            var loading = showLoading();
            $.post('Delete', { ids: ids.toString() }, function (result) {
                loading.close();
                if (result.success) {
                    $.dialog.tips('删除模板成功');
                    var pageNo = $("#list").hiMallDatagrid('options').pageNumber;
                    reload(pageNo);
                }
                else
                    $.dialog.alert('删除模板失败!' + result.msg);
            });
        });
    });

    $('#list').on('click', '.good-edit', function () {
        var name = $(this).siblings('.thisName').val();
        var ids = $(this).siblings('.thisId').val();
        var cId = $(this).siblings('.thiscId').val();
        InitialDialog({ title: '新增/编辑模板', name: name, id: ids, pId: cId })
    });
    $('.nav-tabs-custom li').on('click', '.addFollow', function () {
        InitialDialog({ title: '新增/编辑模板', name: '', id: '', pId: '' })
    });
});


function InitialDialog(option) {
    $.dialog({
        title: option.title,
        lock: true,
        id: 'addAtrr',
        content: ['<div class="dialog-form">',
            '<div class="form-group">',
            '<label class="label-inline fl" for="">模板名称</label><input value="' + option.name + '" id="newCategoryName" class="form-control input-sm" type="text" ><p id="nameErrorMsg" class="help-block">不能为空且不能多于12个字</p>',

            '</div>',

            '<div class="form-group"  style="display:">',
            '<label class="label-inline fl" for="">上级分类</label>',
            '<select class="form-control input-sm" id="categoryDrop"></select>',
            '</div>',
            '</div>'].join(''),
        init: function () {
            $("#newCategoryName").focus();
            $.ajax({
                type: 'GET',
                url: '../Category/GetCategoryDrop',
                cache: false,
                data: { id: option.id },
                dataType: "json",
                success: function (data) {
                    if (data.success == true) {
                        var drop = $("#categoryDrop");
                        var html = [], cate = data.category;
                        for (var i = 0; i < cate.length; i++) {
                            var selected = cate[i].Selected == true ? "selected" : "";
                            html.push('<option ' + selected + ' value="' + cate[i].Value + '">' + cate[i].Text + '</option>');
                        }
                        $(drop).append($(html.join('')));
                        $(drop).val(option.pId);
                    }

                }
            });
        },
        padding: '0 40px',
        okVal: '保存',
        ok: function () {
            var len = $("#newCategoryName").val().length;
            if (len > 12 || len <= 0) {
                $("#nameErrorMsg").css('color', 'red');
                $("#newCategoryName").focus();
                return false;
            }
            var name = $("#newCategoryName").val();
            var cId = parseInt($("#categoryDrop option:selected").val());
            if (cId==0) {
                $("#categoryDrop").css('border-color', 'red');
                return false;
            }
            var loading = showLoading();
            $.ajax({
                type: 'POST',
                url: 'CreateFollow',
                cache: false,
                data: { name: name, cId: cId, ids: option.id },
                dataType: "json",
                success: function (data) {
                    loading.close();
                    if (data.success == true) {
                        $.dialog.tips('操作模板成功');
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
function deleteProduct(ids) {
    $.dialog.confirm('您确定要删除这些模板吗？', function () {
        var loading = showLoading();
     
        $.post('Delete', { ids: ids.join(',').toString() }, function (result) {
            loading.close();
            if (result.success) {
                $.dialog.tips('删除模板成功');
                var pageNo = $("#list").hiMallDatagrid('options').pageNumber;
                reload(pageNo);
            }
            else
                $.dialog.alert('删除模板失败!' + result.msg);
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
        params.auditStatus = [1];
        params.keyWords = '';
        params.categoryId = '';
        
        
        //补充条件
        var girdType = 'normal';
        switch (type) {
            case 'onSale':
                params.auditStatus =1;
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
    $("#batchOnSale").show();
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
    $('.fg-stock input:checkbox').get(0).checked = false;
    categoryId = '';
    $('#category1,#category2,#category3').himallLinkage('reset');
}

 

 

//状态查询
function initAuditGrid() {

  
    lastType = 'audit';
    $("#list").html('');

    //模板表格
    $("#list").hiMallDatagrid({
        url: 'list',
        nowrap: false,
        rownumbers: true,
        NoDataMsg: '没有找到符合条件的模板',
        border: false,
        fit: true,
        fitColumns: true,
        pagination: true,
        idField: "HTP_ID",
        pageSize: 9,
        pagePosition: 'bottom',
        pageNumber: 1,
        queryParams: { auditStatus:0},
        operationButtons: "#saleOff",
        columns:
            [[
                { checkbox: true, width: 40 },
                { field: "IsLimitTimeBuy", hidden: true, width: 40 },
                { field: "HTP_Name", title: "模板名称", width: 90, align: "center" },
                { field: "Name", title: "科目分类", width: 90, align: "center" },
                { field: "State", title: "启用状态", width: 90, align: "center" },
                { field: "Time", title: "创建时间", width: 90, align: "center" },
                {
                    field: "s", title: "操作", width: 120, align: "center",
                    formatter: function (value, row, index) {
                        html = '<span class="btn-a text-left inline-block"><input class="thisId" type="hidden" value="' + row.HTP_ID + '"/><input class="thisName" type="hidden" value="' + row.HTP_Name + '"/><input class="thiscId" type="hidden" value="' + row.HFT_ID + '"/>';
                        if (!row.IsLimitTimeBuy) {
                            html += '<a class="good-edit">编辑</a>';
                        }

                        
                        if (row.HTP_State == 1) {
                            html += '<a class="good-down">停用</a><a class="good-del">删除</a>';
                        }
                        else { html += '<a class="good-up">使用</a><a class="good-del">删除</a>'; }
                        return html;
                    }
                }
            ]],
        onLoadSuccess: function () {
            initBatchBtnShow();
           
        }
    });
}

//初始查询
function initGrid(params) {
    lastType = 'normal';
    $("#list").html('');
    normalFilter();
    console.log(params);
    //模板表格
    $("#list").hiMallDatagrid({
        url: 'list',
        nowrap: false,
        rownumbers: true,
        NoDataMsg: '没有找到符合条件的模板',
        border: false,
        fit: true,
        fitColumns: true,
        pagination: true,
        idField: "HTP_ID",
        pageSize: 9,
        pagePosition: 'bottom',
        pageNumber: 1,
        queryParams: params ? params : { auditStatuses:[1] },
        operationButtons: "#saleOff",
        columns:
            [[
                { checkbox: true, width: 40 },
                { field: "IsLimitTimeBuy", hidden: true, width: 40 },
                { field: "HTP_Name", title: "模板名称", width: 90, align: "center" },
                { field: "Name", title: "科目分类", width: 90, align: "center" },
                { field: "State", title: "启用状态", width: 90, align: "center" },
                { field: "Time", title: "创建时间", width: 90, align: "center" },
                {
                    field: "s", title: "操作", width: 120, align: "center",
                    formatter: function (value, row, index) {
                        html = '<span class="btn-a text-left inline-block"><input class="thiscId" type="hidden" value="' + row.HFT_ID + '"/><input class="thisId" type="hidden" value="' + row.HTP_ID + '"/><input class="thisName" type="hidden" value="' + row.HTP_Name + '"/>';
                        if (!row.IsLimitTimeBuy) {
                            html += '<a class="good-edit" >编辑</a>';
                        }
                       
                       
                        if (row.HTP_State == 1) {
                            html += '<a class="good-down">停用</a><a class="good-del">删除</a><a class="follow-txt" href="followcontent?id=' + row.HTP_ID+'">模板内容</a>'; 
                        }
                        else { html += '<a class="good-up">使用</a><a class="good-del">删除</a>'; }
                       
                        return html;
                    }
                }
            ]],
        onLoadSuccess: function () {
            initBatchBtnShow();
            
        }
    });
}

function initCategoryLinkage() {

    $('#category1,#category2').himallLinkage({
        url: '../Category/getCategory',
        enableDefaultItem: true,
        defaultItemsText: '全部',
        onChange: function (level, value, text) {
            categoryId = value;
        }
    });

}

function search() {
    
    var keyWords = $.trim($('#searchBox').val());
    var auditStatus = _auditStatus;
    if ($('.nav-tabs-custom li[type=unAudit]').hasClass('active')) {
        var auditState = $('select[name="auditState"]').val();
      
        if (!auditState) {
            auditStatus = [0];
            
        } else
            auditStatus = [auditState];
    }

    
    $("#list").hiMallDatagrid('clearReload', {
        keyWords: keyWords, 
        ShopCategoryId: categoryId, auditStatus: auditStatus,
    });
}

function reload(pageNo) {

    $("#list").hiMallDatagrid('reload', { pageNumber: pageNo });
}

function getSelectedIds() {
    var selecteds = $("#list").hiMallDatagrid('getSelections');
    console.log(selecteds);
    var ids = [];
    $.each(selecteds, function () {
        ids.push(this.HTP_ID);
    });
    return ids;
}
 
function saleOff(ids) {
    $.dialog.confirm('您确定要停用这些模板吗？', function () {
        var loading = showLoading();
        $.post('batchStop', { ids: ids.join(',').toString() }, function (result) {
            loading.close();
            if (result.success) {
                $.dialog.tips('停用模板成功');
                var pageNo = $("#list").hiMallDatagrid('options').pageNumber;
                reload(pageNo);
            }
            else
                $.dialog.alert('停用模板失败!' + result.msg);
        });
    });
}

function onSale(ids) {
    $.dialog.confirm('您确定要使用这些模板吗？', function () {
        var loading = showLoading();
        $.post('batchOn', { ids: ids.join(',').toString() }, function (result) {
            loading.close();
            if (result.success) {
                $.dialog.tips('使用模板成功');
                var pageNo = $("#list").hiMallDatagrid('options').pageNumber;
                reload(pageNo);
            }
            else
                $.dialog.alert('使用模板失败!' + result.msg);
        });
    });
}

function initBatchBtnShow() {

    var type = $('.nav-tabs-custom li.active').attr('type');
    if (type == 'onSale') {//当前为销售中tab
        $('#batchSaleOff')
            .show()
            .unbind('click')
            .click(function () {
                var ids = getSelectedIds();
                if (ids.length > 0)
                    saleOff(ids);
                else
                    $.dialog.tips('请至少选择一件模板');
            });

    }
    else if (type == 'unAudit' ) {
        $('#batchOnSale')
            .show()
            .unbind('click')
            .click(function () {
                var ids = getSelectedIds();
                if (ids.length > 0)
                    onSale(ids);
                else
                    $.dialog.tips('请至少选择一件模板');
            });
    } 
    else
        $('#associateTemplate').show();
    $('#batchDelete')
        .unbind('click')
        .click(function () {
            var ids = getSelectedIds();
            if (ids.length > 0)
                deleteProduct(ids);
            else
                $.dialog.tips('请至少选择一件模板');
        });
 

}

 

 

function clearGridSelect() {
    $("#list").hiMallDatagrid('clearSelections');
}

 
 