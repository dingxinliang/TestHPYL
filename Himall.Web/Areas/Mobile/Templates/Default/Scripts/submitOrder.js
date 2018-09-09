﻿$(function () {
    InvoiceOperationInit();
});

var paymentShown = false;
var loading;
var orderIds = '';
function integralSubmit(ids, isgroup) {
    ids=ids.replace(/(.+?)\,+$/,"$1");
    ajaxRequest({
        type: 'POST',
        url: '/' + areaName + '/Order/PayOrderByIntegral',
        param: { orderIds: ids },
        dataType: 'json',
        success: function (data) {
            if (data.success == true) {
                $.dialog.succeedTips('支付成功！', function () {
                    //location.href = '/' + areaName + '/Member/Orders';
                    if (!isgroup || orderIds.indexOf(",")>0) {
                        location.href = '/' + areaName + '/Order/OrderShare?orderids=' + orderIds;
                    } else {
                        //拼团跳转
                        window.location.href = '/' + areaName + "/FightGroup/GroupOrderOk?orderid=" + orderIds;
                    }
                }, 0.5);

            }
        },
        error: function (data) { $.dialog.tips('支付失败,请稍候尝试.', null, 0.5); }
    });
}

$('#submit-order').click(function () {
    var collPIds = $("#collPIds").val();
    var cartItemIds = QueryString('cartItemIds');
    //发票相关
    var invoiceType = 0
    var invoiceTitle = "";
    var invoiceCode = "";
    var invoiceContext = "";

    if (!($(".bill a").text() == "不需要发票")) {
        invoiceType = 2;
        invoiceTitle = $(".bill a").text();
        invoiceCode = $(".bill #invoicecode").text();;
        invoiceContext = $(".bill-Cart .content-bill .active ").parent().text();
    }

    //是否货到付款
    var isCashOnDelivery = false;
    if ($("#icod").val() == "True") {
        isCashOnDelivery = $(".way-01 .offline").hasClass("active");
    }

    var integral = 0;
    if (isintegral) {
        integral = $("#userIntegrals").val();
        integral = isNaN(integral) ? 0 : integral;
    }
    var capital = 0;
    if (iscapital) {
        capital = $("#userCapitals").val();
        capital = isNaN(capital) ? 0 : capital;
    }
    var couponIds = "";
    $('input[name="couponIds"]').each(function (i, e) {
        var type = $(this).attr("data-type");
        couponIds = couponIds + $(e).val() + '_' + type + ',';
    })
    if (couponIds != '') {
        couponIds = couponIds.substring(0, couponIds.length - 1);
    }
    var latAndLng = $("#latAndLng").val();
    var recieveAddressId = $('#shippingAddressId').val();
    recieveAddressId = parseInt(recieveAddressId);
    recieveAddressId = isNaN(recieveAddressId) ? null : recieveAddressId;
    if (!recieveAddressId)
        $.dialog.alert('请选择或新建收货地址');
    else {
        var model = {};
        model.cartItemIds = cartItemIds;
        model.latAndLng = latAndLng;
        model.recieveAddressId = recieveAddressId;
        model.couponIds = couponIds;
        model.integral = integral;
        model.capital = capital;
        model.isCashOnDelivery = isCashOnDelivery;
        model.invoiceType = invoiceType;
        model.invoiceTitle = invoiceTitle;
        model.invoiceCode = invoiceCode;
        model.invoiceContext = invoiceContext;
        model.collPIds = collPIds;
        model.groupActionId = $('#groupActionId').val();
        model.groupId = $('#groupId').val();
        model.payPwd = $("#PayCapitalPwd").val();

        var isTrue = false;
        var orderShops = [];
        $('.goods-info[shopid]').each(function () {
            var shopId = $(this).attr('shopid');
            var orderShop = {};
            orderShop.shopId = shopId;
            orderShop.orderSkus = [];
            $('.item[skuid][count]', this).each(function () {
                orderShop.orderSkus.push({ skuId: $(this).attr('skuid'), count: $(this).attr('count') });
            });
            var deliveryType = $('input:radio[name="shop{0}.DeliveryType"]:checked'.format(shopId));
            orderShop.deliveryType = deliveryType.val();
            orderShop.shopBrandId = deliveryType.attr('sbid');

            if (orderShop.deliveryType == "1" && !orderShop.shopBrandId) {
                $.dialog.tips('到店自提必须选择门店！');
                isTrue = true;
                return false;
            }
            orderShop.remark = $('.orderRemarks#remark_' + shopId).val();
            if (orderShop.remark.length > 200) {
                $.dialog.tips('留言信息至多200个汉字！');
                isTrue = true;
                return false;
            }
            orderShops.push(orderShop);
        });
        if (isTrue) {
            return false;
        }
        model.orderShops = orderShops;

        loading = showLoading();
        var total = parseFloat($("#total").val());
        $.post('/' + areaName + '/Order/IsAllDeductible', { integral: model.integral, total: total }, function (result) {
            if (result) {
                loading.close();
                $.dialog.confirm("您确定用积分抵扣全部金额吗?", function () {
                    submit(model);
                });
            }
            else {
                submit(model, loading);
            }
        });
    }
});

function submit(model, loading) {
    if (loading == null)
        loading = showLoading();
    var url = '/' + areaName + '/Order/SubmitOrder';
    if (isLimitTimeBuy == "True") {
        url = '/' + areaName + '/Order/SubmitLimitOrder';
    }
    $.post(url, model, function (result) {
        if (result.success) {
            if (result.orderIds != null && result != undefined) {
                orderIds = result.orderIds;//当前预约单号
                //在货到付款，且只有一个诊所时
                if (model.isCashOnDelivery && model.orderShops.length == 1) {
                    loading.close();
                    if (result.realTotalIsZero) {
                        integralSubmit(result.orderIds.toString(), model.groupActionId>0);
                    }
                    else {
                        $.dialog.succeedTips('提交成功！', function () {
                            location.href = '/' + areaName + '/Member/Orders';
                        }, 0.5);
                    }
                }
                else if (result.realTotalIsZero) {
                    loading && loading.close();
                    integralSubmit(result.orderIds.toString(), model.groupActionId > 0);
                }
                else {
                    loading && loading.close();
                    GetPayment(result.orderIds.toString());
                }
            }
            else if (result.Id != null && result != undefined) {//限时购
                var requestcount = 0;
                ///检查预约单状态并做处理
                function checkOrderState() {
                    $.getJSON('/OrderState/Check', { Id: result.Id }, function (r) {
                        if (r.state == "Processed" && r.Total === 0) {
                            loading && loading.close();
                            integralSubmit(r.orderIds[0].toString(), model.groupActionId > 0);
                        }
                        else if (r.state == "Processed") {
                            loading && loading.close();
                            GetPayment(r.orderIds[0].toString());
                        }
                        else if (r.state == "Untreated") {
                            requestcount = requestcount + 1;
                            if (requestcount <= 10)
                                setTimeout(checkOrderState, 0);
                            else {
                                $.dialog.tips("服务器繁忙,请稍后去预约单中心查询预约单");
                                loading && loading.close();
                            }
                        }
                        else {
                            $.dialog.tips('预约单提交失败,错误原因:' + r.message);
                            loading && loading.close();
                        }
                    });
                }
                checkOrderState();
            }
            else {
                loading && loading.close();
                $.dialog.alert(result.msg);
            }
        } else {
            loading && loading.close();
            $.dialog.alert(result.msg);
        }
    });
}

$(document).on('click', '.cover, #paymentsChooser .close', function () {
    $('.cover,.custom-dialog').hide();
    $('#capitalstepone').remove();
    $('#payCapitalPwd').remove();
    if ($(".bootstrapSwitch1").bootstrapSwitch("state")) {
        $('.bootstrapSwitch1').click();
    }
    if (paymentShown) {//如果已经显示支付方式，则跳转到预约单列表页面
        //location.href = '/' + areaName + '/Member/Orders';
        location.href = '/common/site/pay?area=mobile&platform=' + areaName.replace('m-', '') + '&controller=member&action=orders&neworderids=' + orderIds;
    }
});

//计算总价格
function CalcPrice() {
    var sum = 0;
    $('.goods-info .item .price').each(function () {
        sum += parseFloat($(this).data('price'));
    });
    var enabledIntegral = $('.bootstrapSwitch2').bootstrapSwitch('state');
    if (enabledIntegral)
        sum -= parseFloat($("#integralPerMoney").val());
    var enabledCapital = $(".bootstrapSwitch1").bootstrapSwitch("state");
    if (enabledCapital) {
        var totalCapital = parseFloat($("#capitalAmount").val());
        var inputcapital = parseFloat($("#capital").val());
        var capital = totalCapital;
        if (sum <= 0) {
            capital = 0;
            if (inputcapital != capital) {
                $("#capital").val(capital.toFixed(2));
            }
            $("#capitalPrice").html("￥-" + capital.toFixed(2));
            $("#userCapitals").val(capital.toFixed(2));
        } else {
            if (!inputcapital || inputcapital < 0) {
                inputcapital = 0;
            }
            if (inputcapital > totalCapital) {
                inputcapital = totalCapital;
            }
            if (sum <= inputcapital) {
                capital = sum;
            } else {
                capital = inputcapital;
            }
            //重新计算余额
            if (isResetUseCapital && totalCapital > 0) {
                if (totalCapital < sum) {
                    capital = totalCapital;
                } else {
                    capital = sum;
                }
                isResetUseCapital = false;
            }
            sum -= capital;
            if (inputcapital != capital) {
                $("#capital").val(capital.toFixed(2));
            }
            $("#capitalPrice").html("￥-" + capital.toFixed(2));
            $("#userCapitals").val(capital.toFixed(2));
        }
    }
    if (sum <= 0) sum = 0;
    $('#allTotal').html('¥' + sum.toFixed(2)).attr('data-alltotal', sum.toFixed(2));
}

function getCount() {
    var result = [];
    $('.goods-info[shopid]').each(function () {
        var shopId = $(this).attr('shopid');
        $('.item[pid][count]', this).each(function () {
            var pid = $(this).attr('pid');
            var count = $(this).attr('count');
            result.push({ shopId: shopId, productId: pid, count: count });
        });
    });

    return result;
}

function freeFreight(shopId) {
    var goodsInfo = $('.goods-info#' + shopId);
    var priceElement = goodsInfo.find('.item .price');
    var oldPrice = parseFloat(priceElement.data('oldprice'));
    var price = oldPrice - parseFloat(priceElement.data('freight'));
    priceElement.html('￥' + price.toFixed(2)).data('price', price);
    goodsInfo.find(".showfreight").html("免运费");
    CalcPrice();
}

//刷新运费
function refreshFreight(regionId) {
    //获取运费
    var data = getCount();
    $.post('/{0}/order/CalcFreight'.format(areaName), { parameters: data, addressId: regionId }, function (result) {
        if (result.success == true) {
            for (var i = 0; i < result.freights.length; i++) {
                var item = result.freights[i];
                var shopId = item.shopId;
                var freight = item.freight;

                var priceDiv = $('.goods-info#{0} .price'.format(shopId));
                var amount = parseFloat(priceDiv.data('price')) - parseFloat(priceDiv.data('freight'));
                var freeFreightAmount = parseFloat(priceDiv.data('freefreight'));
                if (freeFreightAmount <= 0 || amount < freeFreightAmount) {
                    $('.goods-info#{0} .showfreight'.format(shopId)).html('￥' + freight.toFixed(2));
                    priceDiv.data('price', amount + freight).data('freight', freight).html('￥' + (amount + freight).toFixed(2));
                }
                if (priceDiv.is('[selftake]'))
                    freeFreight(shopId);
            }
            CalcPrice();
        } else
            $.dialog.errorTips(result.msg);
    });
}

$("#btnAddInvoice").click(function () {
    var _t = $(this);
    _t.hide();

    var html = '<div>';
    html += '<div class="top">';
    html += '<span class="bill-check" aria-hidden="true" onclick="AddSpanClick(this)"></span>';
    html += '公司';
    html += '</div>';
    html += '<div class="rights">';
    html += '<a href="javascript:void(0);" class="update-tit" style="color:#2894FF">保存</a>';
    html += '<a href="javascript:void(0);" class="ml10 del-tit hide" style="color:#2894FF">删除</a>';
    html += '</div>';
    html += '<div style="display:block;">';
    html += '<div><input type="text" placeholder="公司全称" id="invoicename"  style="opacity:1"/></div>';
    html += '<div><input type="text" placeholder="税号" id="invoicecode"  style="opacity:1"/></div>';
    html += '</div>';
    html += '</div>';

    $("#AllInvoice").prepend(html);
    InvoiceOperationInit();
})

function AddSpanClick(obj) {
    $("#AllInvoice .bill-check").removeClass("active");
    $(obj).addClass("active");
}


function InvoiceOperationInit() {
    $("#dvInvoice .del-tit").click(function () {
        var self = this;
        var id = $(self).attr("key");
        $.dialog.confirm("确定删除该发票抬头吗？", function () {
            var loading = showLoading();
            $.post("/" + areaName + "/BranchOrder/DeleteInvoiceTitle", { id: id }, function (result) {
                loading.close();
                if (result == true) {
                    $.dialog.tips('删除成功！');
                    var _p = $(self).parent().parent();
                    var needSelOne = false;
                    if ($(".bill-check", _p).hasClass("active")) {
                        needSelOne = true;
                    }
                    _p.remove();
                    if (needSelOne) {
                        AddSpanClick($("#AllInvoice>div>div>span").eq(0));
                    }
                }
                else {
                    $.dialog.tips('删除失败！');
                }
            });
        });
    });

    $("#dvInvoice .update-tit").click(function () {
        var self = this;
        var name = $(this).parents().parents().find("#invoicename").val();
        var code = $(this).parents().parents().find("#invoicecode").val();
        if ($.trim(name) == "") {
            $.dialog.tips('公司名称不能为空！');
            return;
        }
        if ($.trim(code) == "") {
            $.dialog.tips('税号不能为空！');
            return;
        }
        var loading = showLoading();
        $.post("/" + areaName + "/Order/SaveInvoiceTitle", { name: name, code: code }, function (result) {
            loading.close();
            if (result != undefined && result != null && result > 0) {
                $(self).parents().find(".del-tit").removeClass("hide").attr("key", result);
                $(self).addClass("hide");
                $(self).parents().parents().find("#invoicename").attr("disabled", "disabled");
                $(self).parents().parents().find("#invoicecode").attr("disabled", "disabled");

                InvoiceOperationInit();
                $.dialog.tips('保存成功！');
                $("#btnAddInvoice").show();
            }
            else {
                if (result == -1) {
                    $.dialog.tips('发票抬头不可为空！');
                } else {
                    $.dialog.tips('保存失败！');
                }
            }
        });
    });
}

