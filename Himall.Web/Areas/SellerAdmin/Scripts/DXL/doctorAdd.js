$(function () {
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




function AddApply() {
    var ajaxGet = function (url, d, fn) {
        console.log(url);
        console.log(d);
        var loading = showLoading();
        $.ajax({
            type: "POST",
            url: url,
            data: d,
            dataType: "json",
            success: function (data) {
                loading.close();
                if (data.success == true) {
                    $.dialog.succeedTips('内容添加成功!', function () { window.location.reload(); });
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

    if ($.isNullOrEmpty(brandLetter)) {
        $.dialog.errorTips("请上传文章封面！");
        return false;
    }
    if ($.isNullOrEmpty(des)) {
        $.dialog.errorTips("请填写内容,尽量以图片形式展现");
        return false;
    }
   
    ajaxGet('Creatdoctor', { cid: cid, title: titles, pic: Pic, remark: des, ids:'' });
   

}