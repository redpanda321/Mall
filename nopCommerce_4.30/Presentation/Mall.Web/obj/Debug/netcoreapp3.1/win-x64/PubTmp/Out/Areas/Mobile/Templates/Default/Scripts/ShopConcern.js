$(function () {
    loadProducts(1);
    
    var list=$('#List');
    list.on('click','.del',function(e){
    	var _this=$(this);
    	$.dialog.confirm('确认取消收藏？',function(){
    		var loading = showLoading();
    		$.post('/' + areaName + '/VShop/DeleteFavorite', { shopId: _this.parent().data('shopid') }, function (data) {
				$.dialog.succeedTips(data.msg);
				_this.parent().remove();
				loading.close();
			});
    	});
    });
    
    document.getElementById('Edit').addEventListener('click',function(){
		if(list.hasClass('active')){
			list.removeClass('active');
			this.innerText='编辑';
		}else{
			list.addClass('active');
			this.innerText='保存';
		}
	});
});

var page = 1;
var siteName = $("#siteName").val();

$(window).scroll(function () {
    var scrollTop = $(this).scrollTop();
    var scrollHeight = $(document).height();
    var windowHeight = $(this).height();

    if (scrollTop + windowHeight >= scrollHeight) {
        $('#autoLoad').removeClass('hide');
        loadProducts(++page);
    }
});


function loadProducts(page) {
    var url = '/' + areaName + '/Member/GetUserCollectionShop';
    $.post(url, { pageNo: page, pageSize: 8 }, function (result) {
        $('#autoLoad').addClass('hide');
        var html = '';
        if (result.data.length > 0) {
            $.each(result.data, function (i, item) {
                var shopinfo = item.FavoriteShopInfo;
                html += '<li class="border-bot" data-shopid="' + shopinfo.ShopId + '"><a class="p-img" onclick="checkVshop(' + shopinfo.ShopId + ')" ><img class="lazyload" src="' + shopinfo.Logo + '" width="100%" alt=""></a>';
                if (shopinfo.ShopStatus != 6) {
                    html += ' <h3><a onclick="checkVshop(' + shopinfo.ShopId + ')">' + shopinfo.ShopName + '</a></h3>';
                    html += '<p>关注人数：'+shopinfo.ConcernCount+'</p><p>关注时间：'+shopinfo.ConcernTimeStr+'</p>';
                } else {
                    html += ' <h3><a>' + shopinfo.ShopName + '</a></h3>';
                    html += ' <p><span class="red">此店铺已被冻结</span></p>';
                }
                html += ' <i class="glyphicon glyphicon-trash del"></i></li>';
            })
            $('#List').append(html);
        }
        else {
            $('#autoLoad').html('没有更多店铺了').removeClass('hide');
        }
    });
}
function checkVshop(shopid)
{
    var url = '/' + areaName + '/Member/CheckVshopIfExist';
    $.post(url, { shopid: shopid }, function (result) {
        if (result.success)
        {
            window.location.href = '/' + areaName + '/vshop/detail/' + result.data.vshopid;
        }
        else {
            $.dialog.errorTips("商家暂未开通微店！");
        }
    });
}