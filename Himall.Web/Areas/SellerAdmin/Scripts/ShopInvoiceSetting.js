$(function () {
    $('input[type="checkbox"]').onoff();

    $('input.pluginCheck').change(function () {
        var _this = $(this),
            state = _this[0].checked,
            loading = showLoading();
        $.post('./InvoiceSetting', { enable: state }, function (result) {
            loading.close();
            if (!result.success) {
                $.dialog.errorTips('操作失败!失败原因：' + result.msg);
            }
        }, "json");
    })

});
