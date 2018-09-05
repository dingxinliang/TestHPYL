﻿//咨询
$(function () {
    
    setTimeout(function () { getProductConsultation(1); }, 100);
});

function getProductConsultation(page) {
    var pid = $('#gid').val();// 诊疗项目id
    $.ajax({
        type: 'get',
        url: '/Product/GetConsultationByProduct?pId=' + pid + '&pageNo=' + page + '&pageSize=' + 10,
        dataType: 'html',
        cache: true,// 开启ajax缓存
        success: function (data) {
            if (data) {
                data = JSON.parse(data);
                var str = '', i, e;
                for (i = 0; e = data.consults[i++];) {
                    str += '<div class="item"><div class="user"><span class="u-name">网　　友：' + e.UserName + '</span>'
                       + '<span class="date-ask">' + e.ConsultationDate + '</span>'
                       + '<dl class="ask"><dt>咨询内容：</dt><dd>' + html_decode(e.ConsultationContent) + '</dd></dl>';

                    if (e.ReplyContent != "暂无回复") {
                        str += '<dl class="answer"><dt>诊所回复：</dt><dd><div class="content">' + html_decode(e.ReplyContent) + '</></div><div class="date-answer">' + e.ReplyDate + '</div></dd></dl>';
                    }
                    str += '</div></div>';
                }
                $('#consult-0').html(str);


                var paging = $('#consult-0').next('.pagin');
                if (paging.length == 0) {
                    paging = commonJS.generatePaging(page, data.totalPage, function (pi) {
                        getProductConsultation(pi);
                    });
                    //$('#consult-0').after(paging.div);
                    $('#consult-0').append(paging.div);
                }
            } else {
                $('#consult-0').hide();
            }
        },
        error: function (e) {
            $('#consult-0').html('').hide();
        }
    });
}

function html_decode(str) {
    var s = "";
    if (str.length == 0) return "";
    s = str.replace(/<[^>]+>/g, "");
    /*s = str.replace(/&amp;/g, "&");
    s = s.replace(/&lt;/g, "<");
    s = s.replace(/&gt;/g, ">");
    s = s.replace(/&nbsp;/g, " ");
    s = s.replace(/&#39;/g, "\'");
    s = s.replace(/&quot;/g, "\"");
    s = s.replace(/<br\/>/g, "\n");*/
    return s;
}