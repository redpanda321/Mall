$(function () {

    //左侧导航
    var floors = $(".floors .template-node .title");
    if (floors.length > 0) {
        $(".floor-nav li").click(function () {
            var currentE = floors.eq($(this).index());
            $("html,body").stop().animate({ scrollTop: currentE.offset().top - 1 }, 600);
        });

        $(window).scroll(function () {
        	var scrollTop=$(document).scrollTop(),
        		winH=$(window).height(),
        		docH=$(document).height(),
        		nav=$('.floor-nav');
            if (scrollTop > (floors.first().offset().top-100) && scrollTop +  winH< docH - 200) {
                nav.fadeIn();
            } else {
                nav.fadeOut();
            }

            floors.each(function(index) {
                if ($(this).offset().top <= scrollTop + winH / 2) {
                    $("li",nav).delay(300).eq(index).addClass("cur").siblings().removeClass();
                }
            })
        });
    }
    
    //楼层选项卡切换
    $('.floor-hd li,.floor5-hd li').mouseDelay(100).hover(function () {
        $(this).addClass('active').siblings().removeClass('active');
        $(this).parent().parent().siblings(".floor-bd").find('.content-right-box').children().eq($(this).index()).show().siblings().hide();
        $(".floor5-hd,.floor-hd").parent().parent().parent().find(".lazyload").scrollLoading();
    });


    //图片滚动切换
	
	$('.floor .slide').each(function() {
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
	
	$('.floor-scroll-btn span').click(function(){
		var _this=$(this),
			type=_this.data('type'),
			list=_this.parent().parent().siblings().find('ul'),
			num=_this.parent().data('num'),
			liWidth=list.find('li').width()+10,
			len=list.find('li').length,
			index=parseInt(_this.parent().data('index'));
		if(_this.hasClass('disabled')){
			return;
		}
		_this.siblings().removeClass('disabled');
		if(type=='prev'){
			--index;
			if(index==0){
				_this.addClass('disabled');
			}
		}else{
			++index;
			if(index==Math.ceil(len/num)-1){
				_this.addClass('disabled');
			}
		}
		_this.parent().data('index',index);
		list.css('left',-index*num*liWidth);
	});
    
    //tab选项卡切换
    var timeoutid;
    $(".floorA .floorA-hd ul li").each(function (index) {
        var current = $(this);
        $(this).mouseover(function () {
            var t = $(this);
            timeoutid = setTimeout(function () {

                t.addClass("active").siblings().removeClass("active");

                $(".floorA .floorA-right .tab-right").eq(index).addClass("current").siblings().removeClass("current");
                current.parent().parent().parent().find(".lazyload").scrollLoading();
            }, 300);
        }).mouseout(function () {
            clearTimeout(timeoutid);
        });
    });

    var timeout_id;
    $(".floor-six-hd ul li").each(function (index) {
        var current = $(this);
        $(this).mouseover(function () {
            var t = $(this);
            timeout_id = setTimeout(function () {

                t.addClass("active").siblings().removeClass("active");

                $(".floor-six .floor-six-right .tab-right").eq(index).addClass("current").siblings().removeClass("current");
                current.parent().parent().parent().find(".lazyload").scrollLoading();
            }, 300);
        }).mouseout(function () {
            clearTimeout(timeout_id);
        });
    });


    var timeout_id;
    $(".floor-seven-hd ul li").each(function (index) {
        var current = $(this);
        $(this).mouseover(function () {
            var t = $(this);
            timeout_id = setTimeout(function () {

                t.addClass("active").siblings().removeClass("active");

                $(".floor-seven .fst-mid .tab-right").eq(index).addClass("current").siblings().removeClass("current");
                current.parent().parent().parent().find(".lazyload").scrollLoading();
            }, 300);
        }).mouseout(function () {
            clearTimeout(timeout_id);
        });
    });

	$('.content-brand').each(function() {
		scrollFloorBrand($(this));
	});
	
	$('.BD-content').each(function() {
		scrollBdBrand( $(this) );
	});
	
	function scrollFloorBrand(_this) {
		var Bindex = 0,
			scrollA = $(_this).find('.scroll-A'),
			Blen = Math.ceil(scrollA.find('li').length/4),
			oPrev = $(_this).children('.prev'),
			oNext = $(_this).children('.next');
		function NextBrand() {
			Bindex++;
			if (Bindex > Blen-1) {
				Bindex=Blen-1;
			}
			scrollA.stop(true, false).animate({ top: -Bindex * 112 }, 600)
		}
		function PrevBrand() {
			Bindex--;
			if (Bindex == -1) {
				Bindex=0;
			}
			scrollA.stop(true, false).animate({ top: -Bindex * 112}, 600)
		}
		oPrev.click(function () {
			PrevBrand();
		});
		oNext.click(function () {
			NextBrand();
		});
	}
	
	//品牌楼层轮播 
	function scrollBdBrand(_this){
		var index = 0,
			Swidth = 530,
			brands=_this.find(".scroll-brand"),
			Slen = brands.length;

		$(".BD-mid b",_this).click(function () {
			if($(this).hasClass('bd-next')){
				index++;
				if (index > Slen - 1) {
					index = 0;
				}
			}else{
				index--;
				if (index < 0) {
					index=0;
					return;
				}
			}
			brands.stop(true, false).animate({ left: -index * Swidth + "px" }, 600)
		});
	}
})



