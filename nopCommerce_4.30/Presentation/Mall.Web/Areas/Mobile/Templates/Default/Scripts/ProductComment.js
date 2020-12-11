$(function() {
    loadProductComments(1);
})

var postOldCommentType;//post里面类型是否变了
var page = 1;
var isend = false;
var pid = QueryString("pId");
var commentType = QueryString("commentType");
$(".comment-tab a").eq(commentType).addClass("active");

function returnComment(type) {
    // location.href = '/' + areaName + '/Product/ProductComment?pId=' + pid + '&commentType=' + commentType;
    commentType = type;
    $(".comment-tab a").removeClass("active");
    $(".comment-tab a").eq(commentType).addClass("active");
    $('#productComment').html("");
    loadProductComments(1);
}

$(window).scroll(function() {
    var scrollTop = $(this).scrollTop();
    var scrollHeight = $(document).height();
    var windowHeight = $(this).height();
    if (scrollTop + windowHeight >= scrollHeight) {
        if (!isend) {
            $('#autoLoad').removeClass('hide');
            loadProductComments(++page);
        }
    }
});

function loadProductComments(page) {
    var areaname = '@ViewBag.AreaName';
    var url = '/' + areaName + '/Product/GetProductComment';
    //$.ajaxSettings.async = false;
    $.post(url, { pId: pid, pageNo: page, commentType: commentType, pageSize: 8, shopBranchId: $("#shopBranchId").val() }, function (result) {
        $('#autoLoad').addClass('hide');
        var html = '';
        var liW = $('#div1').width();
        if (postOldCommentType != commentType) {
            postOldCommentType = commentType;
            $('#productComment').html("");
        }
        if (result.data.length > 0) {
            $.each(result.data, function(i, items) {
                // console.log(items);
                html += '<li><div class="gray"><span class="name">' + items.UserName[0] + "***" + items.UserName[items.UserName.length-1] + '</span>';
                html += '<span>' + items.ReviewDate + '</span>';
                if (items.Sku != null && items.Sku.length > 1) {
                    html += '<p>购买了 ' + items.Sku + '</span>';
                }
                html += '</div>';
                var dataImg = showImg = '';
                for (var j = 0; j < items.Images.length; j++) {
                    dataImg += '<li style="width:' + liW + 'px"><img src="' + items.Images[j].CommentImage + '"/></li>';
                    showImg += '<dd><span><div class="son"><img src="' + items.Images[j].CommentImage + '"/></div></span></dd>'
                }
                html += '<p class="c-2a2a2a">' + items.ReviewContent + '</p><dl class="comment-img" data-img=\''+dataImg+'\'>'+showImg+'</dl>';

                if (items.ReplyContent != null && items.ReplyContent != '' && items.ReplyContent != "暂无回复") {
                    html += '<dl class="shop-reply">商家回复：' + items.ReplyContent + '</><div class="date-answer">' + items.ReplyDate + '</div></dl>'
                }
                if (items.AppendDate != null && items.AppendDate != "") {
                    dataImg = showImg = '';
                    for (var k = 0; k < items.AppendImages.length; k++) {
                        dataImg += '<li style="width:' + liW + 'px"><img src="' + items.AppendImages[k].CommentImage + '"/></li>';
                        showImg += '<dd><span><div class="son"><img src="' + items.AppendImages[k].CommentImage + '"/></div></span></dd>';
                    }
                    html += '<dl class="comment-ago"><dt>收货' + GetDateDiff(items.FinshDate, items.AppendDate) + '后追加</dt><dd>' + items.AppendContent + '</dd></dl><dl class="comment-img" data-img=\''+dataImg+'\'>'+showImg+'</dl>';
                }
                if (items.ReplyDate != null && items.ReplyAppendContent != null && items.ReplyAppendContent != "" && items.ReplyAppendContent != "暂无回复") {
                    html += '<dl class="shop-reply">商家回复：' + items.ReplyAppendContent + '</dl>';//e.ReplyDate
                }

                html += '</li>';

            });
            if (page == 1) {
                isSend = false; 
                $('#productComment').html("");
            }
            $('#productComment').append(html);
           
            // 放大评论图
            $('#productComment').on('click', '.comment-img dd', function() {
                var len = $(this).parent().find('dd').length;
                var index = $(this).index();
                iNow = num = index;
                $('#title').html( num + 1 + ' / ' + len );
                $('#ul1').css({'left':-index * liW,'width':len*liW}).html( $(this).parent().data('img') );
                $('.comment-popup').addClass('is-show');
            });
            $('.comment-popup').on('click', function(ev) {
                if ( $(ev.target).is('.comment-popup') || $(ev.target).is('.comment-popup-header') ) {
                    $(this).removeClass('is-show');
                }
            });  
        }
        else {
            isend = true;
            $('#autoLoad').html('没有更多评论了').removeClass('hide');
        }
    });
    //$.ajaxSettings.async = true; 
}

function GetDateDiff(startDate, endDate) {
    var daysRound = GetDateDiffResult(startDate, endDate, "day");
    var hoursRound = GetDateDiffResult(startDate, endDate, "hour");
    var minutesRound = GetDateDiffResult(startDate, endDate, "minute");

    if (hoursRound > 24) {
        return parseInt(daysRound) + "天";
    } else if (minutesRound > 60) {
        return parseInt(hoursRound) + "小时";
    } else {
        if (parseInt(minutesRound) <= 0) {
            return "1分钟";
        }
        return parseInt(minutesRound) + "分钟";
    }
}


function GetDateDiffResult(startTime, endTime, diffType) {
    startTime = startTime.replace(/\-/g, "/");
    endTime = endTime.replace(/\-/g, "/");
    diffType = diffType.toLowerCase();
    var sTime = new Date(startTime); //开始时间
    var eTime = new Date(endTime); //结束时间
    var timeType = 1;
    switch (diffType) {
        case "second":
            timeType = 1000;
            break;
        case "minute":
            timeType = 1000 * 60;
            break;
        case "hour":
            timeType = 1000 * 3600;
            break;
        case "day":
            timeType = 1000 * 3600 * 24;
            break;
        default:
            break;
    }
    return parseInt((eTime.getTime() - sTime.getTime()) / parseInt(timeType));
}

// 评论图滑动
var iNow = 0;
var num = 0;
window.onload = function() {
    var oTitle = document.getElementById('title');
    var oDiv = document.getElementById('div1');
    var oUl = document.getElementById('ul1');
    var aLi = oUl.getElementsByTagName('li');
    var w = oDiv.offsetWidth;
    for ( var i = 0; i < aLi.length; i++ ) {
        aLi[i].style.width = w + 'px';
    }
    document.querySelector('.comment-popup').ontouchmove = function(ev) {
        ev.preventDefault();
    };
    
    var downLeft = 0;
    var downX = 0;
    var downTime = 0;
    oUl.ontouchstart = function(ev) {
        var touchs = ev.changedTouches[0];
        var bBtn = true;
        downLeft = this.offsetLeft;
        downX = touchs.pageX;
        downTime = Date.now();
        oUl.ontouchmove = function(ev) {
            var touchs = ev.changedTouches[0];
            if( this.offsetLeft >= 0 ) {
                if(bBtn) {
                    bBtn = false;
                    downX = touchs.pageX;       
                }
                this.style.left = (touchs.pageX - downX)/3 + 'px';
            }
            else if(this.offsetLeft <= oDiv.offsetWidth - this.offsetWidth) {
                if(bBtn) {
                    bBtn = false;
                    downX = touchs.pageX;       
                }
                this.style.left = (touchs.pageX - downX)/3 + (oDiv.offsetWidth - this.offsetWidth) + 'px';
            }
            else{
                this.style.left = touchs.pageX - downX + downLeft + 'px';
            }
        };
        oUl.ontouchend = function(ev) {
            var touchs = ev.changedTouches[0];
            this.ontouchmove = null;
            this.ontouchend = null;
            if( downX < touchs.pageX ) {  //→
                if( iNow != 0) {
                    if(touchs.pageX - downX > oDiv.offsetWidth/2 || Date.now() - downTime < 300 &&  touchs.pageX - downX > 30) {
                        iNow--;
                        num--;
                        oTitle.innerHTML = num + 1 + ' / ' + aLi.length;
                    }
                }
                startMove(oUl,{left : -iNow*w},400,'easeOut');
            }
            else{  //←
                if( iNow != aLi.length-1) {
                    if(downX - touchs.pageX > oDiv.offsetWidth/2 || Date.now() - downTime < 300 && downX - touchs.pageX > 30) {
                        iNow++;
                        num++;
                        oTitle.innerHTML = num + 1 + ' / ' + aLi.length;
                    }
                }
                startMove(oUl,{left : -iNow*w},400,'easeOut');
            }
        };
    };
};
