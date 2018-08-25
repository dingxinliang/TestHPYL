var count = 120;

$("#CashType1").on("click", function () {
    $(".dbank").show();
    $(".dWei").hide();
    $(".dAlipay").hide();
});

$("#CashType2").on("click", function () {
    $(".dbank").hide();
    $(".dWei").show();
    $(".dAlipay").hide();
});

$("#CashType3").on("click", function () {
    $(".dbank").hide();
    $(".dWei").hide();
    $(".dAlipay").show();
});

$("#btn_winxinfi").on("click", function () {
    window.location.reload();
})

//发送验证码
$("#sendCodesa").on("click", function () {
    $.post('/SellerAdmin/AccountSettings/SendCode', { pluginId: $("#pluginId").val(), destination: $("#destination").val() }, function (result) {
        if (result.success) {
            $(".sendSp").show();
            siaw = setInterval(function () { count--; countDown1tsiaw(count, "sendCodesa"); }, 1000);
        }
        else {
            $.dialog.errorTips('发送验证码失败：' + result.msg);
        }
    });
})


function countDown1tsiaw(ss, dv) {
    if (ss > 0) {
        $("#" + dv).val("重新获取（" + ss + "s）");
        $("#" + dv).attr("disabled", "disabled");
    } else {
        $("#" + dv).val("获取验证码");
        $("#" + dv).removeAttr("disabled");
        clearInterval(siaw);
    }
}

$("#btn_Apply").on("click", function () {

    var reg = new RegExp('^[0-9]+([.]{1}[0-9]{1,2})?$');
    if ($("#balance").val() == "") {
        $.dialog.errorTips("请输入要提现金额");
        return false;
    }
    if (!reg.test($("#balance").val())) {
        $.dialog.errorTips("金额格式不对");
        return false;
    }
    if (parseFloat($("#balance").val()) <= 0) {
        $.dialog.errorTips("提现金额必需大于零");
        return false;
    }
    if (parseFloat($("#balance").val()) > 5000) {
        $.dialog.errorTips("提现金额最大不能超过5000");
        return false;
    }
    var WithdrawType = 2;
    var WithdrawType = 3;
    var AlipayAccount = "";
    var RealName = "";
    if ($("#CashType2").attr("checked")) {
        WithdrawType = 1;
    }
    else if ($("#CashType3").attr("checked")) {
        if ($("#alipayAccount").val() == "") {
            $.dialog.errorTips("请输入支付宝账户");
            return false;
        }
        if ($("#realName").val() == "") {
            $.dialog.errorTips("请输入真实姓名");
            return false;
        }

        WithdrawType = 3;
        AlipayAccount = $("#alipayAccount").val();
        RealName = $("#realName").val();
    }

    var loading = showLoading();
    $.post("ApplyWithDrawSubmit", {
        pluginId: $("#pluginId").val(),
        destination: $("#destination").val(),
        code: $("#code").val(),
        amount: parseFloat($("#balance").val()),
        WithdrawType: WithdrawType,
        Account: AlipayAccount,
        AccountName, RealName
    }, function (result) {
        loading.close()
        if (result.success) {
            $.dialog.succeedTips("提现申请已提交！", function () {
                window.history.go(0);
            }, 3);
        } else {
            $.dialog.errorTips(result.msg);
        }
    })
})