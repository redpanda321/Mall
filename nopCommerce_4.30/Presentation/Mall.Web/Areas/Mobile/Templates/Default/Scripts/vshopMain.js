$(function () {
    loadVshops(page++);
    initAddFavorite();
});


var page = 1;

$(window).scroll(function () {
    $('#autoLoad').removeClass('hide');
    var scrollTop = $(this).scrollTop();
    var scrollHeight = $(document).height();
    var windowHeight = $(this).height();

    if (scrollTop + windowHeight >= scrollHeight) {
        //alert('执行加载');
        loadVshops(page++);
    }
});


function loadVshops(page) {
    var areaname = areaName,
    	lastFn=function(){
    		$('#autoLoad').html('');
        	$('#more').html('<a class="btn btn-primary btn-sm" href="/' + areaname + '/VShop/list"> 查看更多微店 </a>');
    	},
		url = '/' + areaname + '/vshop/GetHotShops';
    $.post(url, { page: page, pageSize: 8 }, function (data) {
        var result = data.data;
        var html = '';
        if(page==1 && result.length<8){
        	lastFn();
        }
        if (result.length > 0) {
            $.each(result, function (i, shop) {
                var tags = shop.sourcetags ? shop.sourcetags.split(';') : '';
                var tag1 = tags.length > 0 ? tags[0] : '';
                var tag2 = tags.length > 1 ? '<i></i>' + tags[1] : '';
                html += ' <div class="item">\
                <div class="info">\
                    <a href="/' + areaname + '/vshop/detail/' + shop.id + '"><img src="' + shop.logo + '" /></a>\
                    <h3><a href="/' + areaname + '/vshop/detail/' + shop.id + '">' + shop.name + '</a><i onclick="goAddFav(this)" type="addFavorite" class="iconfonts ' + (shop.favorite ? 'icon-aixin' : 'icon-guanzhu') + '" shopId="' + shop.shopId + '"></i></h3>\
                    <p>宝贝数 <span>' + shop.productCount + '</span><em></em>关注度 <span>' + shop.FavoritesCount + '</span></p>\
                    <h5><span>' + tag1 + '</span><span>' + tag2 + '</span></h5>\
                 </div>\
                <ul class="product clearfix">';
                $.each(shop.products, function (j, product) {
                    html += '<li>\
                        <a class="p-img" href="/' + areaname + '/product/detail/' + product.id + '"><img src="' + product.image + '" alt=""></a>\
                    </li>';
                });
                html += '</ul></div>';
            });
            $('#autoLoad').addClass('hide');
            $('#shopList').append(html);
        }
        else {
            lastFn();
        }
        
    });
}

function initAddFavorite() {
    var shopId = QueryString('addFavorite');
    var isAdd = QueryString('isAdd');
    if (shopId) {//带有addFavorite参数，说明为登录后回调此页面添加收藏
        addFavorite(shopId, isAdd);
    }

}

function goAddFav(obj){
	var shopId = $(obj).attr('shopId');
    var isAdd = $(obj).hasClass('icon-aixin');
    var returnUrl = '/' + areaName + '/vshop?addFavorite=' + shopId + '&isAdd=' + isAdd;
    checkLogin(returnUrl, function () {
        addFavorite(shopId, isAdd);
    });
}

function addFavorite(shopId, isAdd) {
    var loading = showLoading();
    var method = '';
    var title;
    if (!isAdd) {
        method = 'AddFavorite';
        title = '';
    }
    else {
        method = 'DeleteFavorite';
        title = '取消';
    }
    $.post('/' + areaName + '/vshop/' + method, { shopId: shopId }, function (result) {
        loading.close();
        if (result.success) {
            $.dialog.succeedTips(title + '收藏成功!');
            var cur = $('i[type="addFavorite"][shopId="' + shopId + '"]')[0];
            if (isAdd) {
                cur.className = 'iconfonts icon-guanzhu';
            } else {
                cur.className = 'iconfonts icon-aixin';
            }
        }
        else
            $.dialog.errorTips(result.msg);
    });
}