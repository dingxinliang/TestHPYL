/// <reference path="../../../Scripts/jqeury.himallLinkage.js" />

var val = "";
$(function () {
    initGrid({ id: GetQueryString("id") });
    $('#list').on('click', '.good-del', function () {
        var name = $(this).siblings('.thisName').val();
        var ids = $(this).siblings('.thisId').val();

        $.dialog.confirm('您确定要删除' + (name ? ' “' + name + '” ' : ('这' + ($.isArray(ids) ? ids.length : 1) + '件随访')) + '吗？', function () {
            var loading = showLoading();
            $.post('DeletedoctorContent', { ids: ids.toString() }, function (result) {
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

    $('#list').on('click', '.good-edit', function () {
        var name = $(this).siblings('.thisName').val();
        var ids = $(this).siblings('.thisId').val();
        var cId = $(this).siblings('.thiscId').val();
        var txtcon = $(this).siblings('.thiscon').val();
        var days = $(this).siblings('.thisday').val();
        InitialDialog({ title: '新增/编辑随访', name: name, id: ids, pId: cId, day: days, content: txtcon })
    });
    $('.nav-tabs-custom li').on('click', '.addFollowContent', function () {
        InitialDialog({ title: '新增/编辑随访', name: "", id: '', pId: GetQueryString("id"), day: 7, content: '' })
    });
});


function InitialDialog(option) {

    $.dialog({
        title: option.title,
        lock: true,
        id: 'addAtrr',
        content: ['<div class="dialog-form">',
            '<div class="form-group" style=display:none>',
            '<label class="label-inline fl" for="">所属模板</label><input value="' + option.name + '" id="FollowName" class="form-control input-sm" type="text" >',

            '</div>',
            '<div class="form-group">',
            '<label class="label-inline fl" for="">随访时间</label><input value="' + option.day + '" id="Nday" class="form-control input-sm" type="text" ><p id="dayErrorMsg" class="help-block">请输入纯数字</p>',

            '</div>',

            '<div class="form-group">',
            '<label class="label-inline fl" for="">随访内容</label><textarea class="form-control" cols="38"   rows="3" id="Ncontent">' + option.content + '</textarea><p id="txtErrorMsg" class="help-block">不能为空且不能多于200个字</p>',
            '</div>',

            '</div>'].join(''),
        padding: '0 40px',
        okVal: '保存',
        ok: function () {

            var len = $("#Ncontent").val().length;
            if (len > 200 || len <= 0) {
                $("#txtErrorMsg").css('color', 'red');
                $("#Ncontent").focus();
                return false;
            }
            var day = $("#Nday").val();
            if (!Isnum(day)) {
                $("#dayErrorMsg").css('color', 'red');
                $("#Nday").focus();
                return false;
            }
            if (!Isnum(option.pId)) {
                $.dialog.tips('参数错误');
                return false;
            }
            var loading = showLoading();
            $.ajax({
                type: 'POST',
                url: 'CreatedoctorContent',
                cache: false,
                data: { cId: option.pId, ids: option.id, day: $("#Nday").val(), cont: $("#Ncontent").val() },
                dataType: "json",
                success: function (data) {
                    loading.close();
                    if (data.success == true) {
                        $.dialog.tips('操作随访成功');
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

function Isnum(str) {
    var r = /^[0-9]*[1-9][0-9]*$/　　//正整数 
    return r.test(str); //str为你要判断的字符 执行返回结果 true 或 false
}
function deleteFollowContent(ids) {
    $.dialog.confirm('您确定要删除这些随访吗？', function () {
        var loading = showLoading();
        $.post('DeletedoctorContent', { ids: ids.join(',').toString() }, function (result) {
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



//初始查询
function initGrid(params) {

    lastType = 'normal';
    $("#list").html('');
    //随访表格
    $("#list").hiMallDatagrid({
        url: 'ListDoctorContent',
        nowrap: false,
        rownumbers: true,
        NoDataMsg: '没有找到符合条件的随访内容',
        border: false,
        fit: true,
        fitColumns: true,
        pagination: true,
        idField: "HDC_ID",
        pageSize: 9,
        pagePosition: 'bottom',
        pageNumber: 1,
        queryParams: { id: params.id },
        operationButtons: "#saleOff",
        columns:
            [[
                { checkbox: true, width: 40 },
                { field: "IsLimitTimeBuy", hidden: true, width: 40 },
                { field: "Days", title: "随访时间", width: 90, align: "center" },
                { field: "HDC_Content", title: "随访内容", width: 90, align: "center" },
                { field: "FollowName", title: "所属随访", width: 90, align: "center" },
                { field: "CategoryName", title: "随访分类", width: 90, align: "center" },
                {
                    field: "s", title: "操作", width: 120, align: "center",
                    formatter: function (value, row, index) {
                        html = '<span class="btn-a text-left inline-block"><input class="thisday" type="hidden" value="' + row.HDC_Days + '"/><input class="thiscon" type="hidden" value="' + row.HDC_Content + '"/><input class="thiscId" type="hidden" value="' + row.HAA_ID + '"/><input class="thisId" type="hidden" value="' + row.HDC_ID + '"/><input class="thisName" type="hidden" value="' + row.FollowName + '"/>';
                        if (!row.IsLimitTimeBuy) {
                            html += '<a class="good-edit" >编辑</a> <a class="good-del">删除</a>';
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





function reload(pageNo) {

    $("#list").hiMallDatagrid('reload', { pageNumber: pageNo });
}

function getSelectedIds() {
    var selecteds = $("#list").hiMallDatagrid('getSelections');
    console.log(selecteds);
    var ids = [];
    $.each(selecteds, function () {
        ids.push(this.HDC_ID);
    });
    return ids;
}

function initBatchBtnShow() {
    $('#batchDelete')
        .unbind('click')
        .click(function () {
            var ids = getSelectedIds();
            if (ids.length > 0)
                deleteFollowContent(ids);
            else
                $.dialog.tips('请至少选择一件随访');
        });
}

function clearGridSelect() {
    $("#list").hiMallDatagrid('clearSelections');
}


