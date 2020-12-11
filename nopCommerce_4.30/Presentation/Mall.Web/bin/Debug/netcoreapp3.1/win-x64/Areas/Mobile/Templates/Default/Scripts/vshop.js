var page = 1;

$(window).scroll(function () {
    $('#autoLoad').removeClass('hide');
    var scrollTop = $(this).scrollTop();
    var scrollHeight = $(document).height();
    var windowHeight = $(this).height();

    if (scrollTop + windowHeight >= scrollHeight) {
        //alert('执行加载');
        loadVShops(page++);
    }
});

$(function () {

    loadVShops(page++);
    initAddFavorite();

});




function loadVShops(page) {
    var areaname = areaName;

    var url = '/' + areaname + '/vshop/list';
    $.post(url, { page: page, pageSize: 8 }, function (result) {
        var html = '';
        if (result.data.length > 0) {
            $.each(result.data, function (i, vshop) {
                var tags = vshop.tags ? vshop.tags.split(';') : '';
                var tag1 = tags.length > 0 ? tags[0] : '';
                var tag2 = tags.length > 1 ? '<i></i>' + tags[1] : '';
                var url = '/' + areaname + '/vshop/detail/' + vshop.id;

                html += '<div class="item">\
		    		<div class="info">\
		    			<a href="'+url+'"><img src="' + vshop.image + '" /></a>\
		    			<h3><a href="'+url+'">' + vshop.name + '</a><i type="addFavorite" class="iconfonts '+(vshop.favorite?'icon-aixin':'icon-guanzhu')+'" shopId="'+vshop.shopId+'"></i></h3>\
		    			<p>宝贝数 <span>' + vshop.productCount + '</span><em></em>关注度 <span>' + vshop.FavoritesCount + '</span></p>\
		    			<h5><span>' + tag1 + '</span><span>' + tag2 + '</span></h5>\
		    		</div>\
		    	</div>';
            });
            $('#autoLoad').addClass('hide');
            $('#shopList').append(html);

        }
        else {
            $('#autoLoad').html('没有更多店铺了');
        }
    });
}


$('#shopList').on('click', 'i[type="addFavorite"]', function () {
    var shopId = $(this).attr('shopId');
    var isAdd = $(this).hasClass('icon-aixin'); 
    var returnUrl = '/' + areaName + '/vshop/list?addFavorite=' + shopId + '&isAdd=' + isAdd;
    checkLogin(returnUrl, function () {
        addFavorite(shopId, isAdd);
    });
});




function initAddFavorite() {
    var shopId = QueryString('addFavorite');
    var isAdd = QueryString('isAdd');
    if (shopId) {//带有addFavorite参数，说明为登录后回调此页面添加收藏
        addFavorite(shopId, isAdd);
    }

}

function addFavorite(shopId,isAdd) {
	console.log(isAdd)
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
            var cur=$('i[type="addFavorite"][shopId="' + shopId + '"]')[0];
            if(isAdd){
            	cur.className='iconfonts icon-guanzhu';
            }else{
            	cur.className='iconfonts icon-aixin';
            }
        }
        else
            $.dialog.errorTips(result.msg);
    });
}