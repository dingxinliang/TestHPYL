// JavaScript Document

/*; (function () {
    
}());*/

$(function () {
	
	//图片延迟加载
	$(".lazyload").scrollLoading();

    //下拉菜单
    $('.dropdown').hover(function () {
        $(this).addClass('hover');
    },function(){
    	$(this).removeClass('hover');
    });

    //焦点图渐变切换
    var lenSlide = $("#slide ul li").length,
		indexSlide = 0,
		picTimer,
		btn = '<div class="slide-controls">';
    for (var i = 1; i <= lenSlide; i++) {
        btn += "<span "+(i==1?"class=cur":"")+">" + i + "</span>";
    }
    btn += "</div>";
    $('#slide').append(btn);
    $("#slide ul li").eq(0).addClass('active');
    $("#slide span").mouseenter(function () {
        indexSlide = $(this).index();
		clearInterval(picTimer);
        showPics(indexSlide);
    });
    
    startLoop();
	
    $("#slide").mouseenter(function () {
        clearInterval(picTimer);
    }).mouseleave(function () {
        startLoop();
    });

    function showPics(index) {
        $("#slide ul li").eq(index).addClass('active').siblings().removeClass();
        $("#slide span").removeClass("cur").eq(index).addClass("cur");

    }
    
    function startLoop(){
    	picTimer = setInterval(function () {
            showPics(indexSlide);
            indexSlide++;
            if (indexSlide == lenSlide) { indexSlide = 0; }
        }, 4000);
    }


    //楼层选项卡切换
    $('.floor-hd li,.floor5-hd li').mouseDelay(100).hover(function () {
        $(this).addClass('active').siblings().removeClass('active');
        $(this).parent().parent().siblings(".floor-bd").find('.content-right-box').children().eq($(this).index()).show().siblings().hide();
    });


    //图片滚动切换
	
	$('.floors .slide').each(function() {
	    var len=$(this).find('li').length,
	        liWidth = $(this).find('li').first().width();
		
		var btn = '<div class="slide-controls">';
        for (var i = 0; i < len; i++) {
            btn += "<span>" + i + "</span>";
        }
        btn += "</div>";
        $(this).append(btn).find('ul').width(len * liWidth);
		$(this).find('span').first().addClass("cur");
		$(this).find('span').mouseDelay().mouseenter(function(){
		    $(this).addClass("cur").siblings().removeClass().parent().siblings().stop(false, true).animate({ 'left': $(this).index() * (-liWidth) }, 200);
		});
		
	});


    //商品导航
    $('.categorys .item').hover(function () {
        $(this).addClass('hover');
    },function(){
    	$(this).removeClass('hover');
    });
    if ($('.category').css('display') == 'none') {
        $('.categorys').mouseDelay().hover(function () {
            $('.category').show();
        });
        $('.categorys').mouseleave(function () {
            $('.category').hide();
        });
    }



    clickChange('#tab-brand li', '.brandslist.tabcon');



    /*商品列表页*/
    $('#refilter .item').each(function () {
        var _this = $(this);
        $(this).find('b').click(function () {
            _this.toggleClass('hover');
        });
    });

    $('#refilter .extra').click(function () {
        if ($(this).siblings('.undis').css('display') == 'none') {
            $(this).siblings('.undis').show();
            $(this).find('.more').html('<span>收起</span><b class="close"></b>');
        } else {
            $(this).siblings('.undis').hide();
            $(this).find('.more').html('<span>显示全部分类</span><b class="open"></b>');
        }

    });


    //商铺排行切换
    hoverChange('.shop-ranking span', '.shop-ranking ul');
	
	
	//兼容IE8及以下浏览器
	if(!+[1,]){
		$('.content_recont ul').children().last().addClass('last-child');
		$('.content_mcontl').children().last().addClass('last-child');
		
	}

});

//点击切换
function clickChange(tabHd, tabBd) {

    $(tabHd).click(function () {
        $(this).addClass('curr').siblings().removeClass('curr');
        $(this).parent().siblings(tabBd).eq($(this).index()).show().siblings(tabBd).hide();
    });
}

//hover切换
function hoverChange(tabHd, tabBd) {

    $(tabHd).hover(function () {
        $(this).addClass('current').siblings().removeClass('current');
        $(this).parent().siblings(tabBd).eq($(this).index()).show().siblings(tabBd).hide();
    });
}

function isHaveLogin()
{
    var result = false;
    var memberId = $.cookie('Himall-User');
    if (memberId) {
        result = true;
    }
    return result;
}

function checkLogin(callBack) {
    if (isHaveLogin()) {
        callBack();
    }
    else {
        $.fn.login({}, function () {
            callBack(function () { location.reload(); });
        }, './', '', '/Login/Login');
    }
}