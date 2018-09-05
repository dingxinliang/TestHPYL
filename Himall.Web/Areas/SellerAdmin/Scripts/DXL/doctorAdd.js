$(function () {
    if (!$.isNullOrEmpty(GetQueryString("id"))) {
        GetInfo(GetQueryString("id"));
    }  
    InitBrandLetter();
    //初始化富文本框
    UE.getEditor('desContainer');
});


function InitBrandLetter(brandLetter) {
    $('#brandLetter').hishopUpload({
        title: '<b>*</b>文章封面：',
        imageDescript: '只能上传一张<label class="">图片大小不能超过1M</label>',
        displayImgSrc: brandLetter,
        imgFieldName: "doctorPhoto",
        dataWidth: 8,
        imagesCount: 1
    });
}

function GetInfo(id) {
    var loading = showLoading();
    $.ajax({
        type: "POST",
        url: "GetDoctor",
        data: {ids:id},
        dataType: "json",
        success: function (data) {
            loading.close();
            if (data.success == true) {
                $("#dName").val(data.data[0].HAA_Title);
                $("#GoodsCategory").val(data.data[0].HFT_ID);
               
                $("#hdpic").val(data.data[0].HAA_PicUrl);
                var ue = UE.getEditor('desContainer');
                ue.addListener('ready', function (editor) {
                    ue.setContent(data.data[0].HAA_Content);
                });
            } else {
                $.dialog.errorTips("无效的参数")
            }
        }
    });
}


function AddApply() {
    var ajaxGet = function (url, d, fn) {
        var loading = showLoading();
        $.ajax({
            type: "POST",
            url: url,
            data: d,
            dataType: "json",
            success: function (data) {
                loading.close();
                if (data.success == true) {
                    $.dialog.succeedTips('内容保存成功!', function () { window.location.reload(); });
                } else {
                    $.dialog.errorTips(data.msg)
                }
            }
        });
    };

    var titles = $("#dName").val();
    var cid = $("#GoodsCategory").val();
    var des = UE.getEditor('desContainer').getContent();
    var Pic = $('#brandLetter').hishopUpload('getImgSrc').toString();
    if ($.isNullOrEmpty(titles)) {
        $.dialog.errorTips("请填写文章标题！");
        return false;
    } else {
            if (titles.length > 50) {
                $.dialog.errorTips("标题长度不能大于50个字符！");
                $('#dName').focus();
                return false;
            }
    }
    if (cid == "0") {
        $.dialog.errorTips("请选择科目！");
        return false;
    }
    if ($.isNullOrEmpty(Pic) && $("#hdpic").val()=="") {
        $.dialog.errorTips("请上传文章封面！");
        return false;
    }
    if ($.isNullOrEmpty(des)) {
        $.dialog.errorTips("请填写内容,尽量以图片形式展现");
        return false;
    }
    if ($.isNullOrEmpty(Pic) && $("#hdpic").val() != "") {
        Pic = $("#hdpic").val();
    }
    var id ="";
    if (!$.isNullOrEmpty(GetQueryString("id"))) {
        id = GetQueryString("id");
    }  
    ajaxGet('Creatdoctor', { cid: cid, title: titles, pic: Pic, remark: des, ids: id });
   

}