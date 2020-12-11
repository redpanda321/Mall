$(function () {
    loadProducts(1);
    
    var list=$('.search-list');
    list.on('click','.del',function(e){
    	var _this=$(this);
    	$.dialog.confirm('确认取消收藏？',function(){
    		var loading = showLoading();
    		$.post('/' + areaName + '/Product/DeleteFavoriteProduct', { pid: _this.parent().data('id') }, function (data) {
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

$(window).scroll(function () {
    var scrollTop = $(this).scrollTop();
    var scrollHeight = $(document).height();
    var windowHeight = $(this).height();

    if (scrollTop + windowHeight >= scrollHeight) {
        $('#autoLoad').removeClass('hide');
        loadProducts(++page);
    }
});


function loadProducts(page) { //后台图片已处理
    var url = '/' + areaName + '/Member/GetUserCollectionProduct';
    $.post(url, { pageNo: page, pageSize: 8 }, function (result) {
        $('#autoLoad').addClass('hide');
        var html = '';
        if (result.data.length > 0) {
            $.each(result.data, function (i, item) {
                html += '<li data-id="' + item.Id + '"><a class="p-img" href="/' + areaName + '/product/detail/' + item.Id + '"><img class="lazyload" src="' + item.Image + '" width="100%" alt="">';
                if (item.Status > 0) {
                    switch (item.Status) {
                        case 1:
                            html += '<i class="sale-out">已失效</i>';
                            break;
                        case 2:
                            html += '<i class="sale-out">已售罄</i>';
                            break;
                        case 3:
                            html += '<i class="sale-out">已下架</i>';
                            break;
                    }
                }
                html += '</a>';
                html += ' <h3><a href="/' + areaName + '/product/detail/' + item.Id + '">' + item.ProductName + '</a></h3>';
                html += ' <p class="red">￥' + item.SalePrice + '</p><p>' + item.Evaluation + '人评价</p><i class="glyphicon glyphicon-trash del del-bottom"></i></li>';
            })
            $('.search-list').append(html);
        }
        else {
            $('#autoLoad').html('没有更多商品了').removeClass('hide');
        }
    });
}