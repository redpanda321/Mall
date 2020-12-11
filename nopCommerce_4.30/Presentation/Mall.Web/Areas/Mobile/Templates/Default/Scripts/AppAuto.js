document.addEventListener('plusready', function() {
	$(".members_con .add-buy-btn").hide();//app中没有在列表页加入购物车的功能，所以隐藏
	$.cookie('Mall-User', plus.storage.getItem('appuserkey'));
	$("footer").hide();
	//$(".container").css("padding-bottom", "0");
	var ms = plus.webview.currentWebview();
	var login = plus.storage.getItem('appuserkey');
	var url = ms.url;
	$(".members_search").on('click', 'input', function(e) {
		mui.openWindow({
			id: 'search.html',
			url: url + 'search.html'
		});
	});
	$(".members_con").on("click", "a", function(e) {
		login = plus.storage.getItem('appuserkey');
		e.preventDefault();
		var href = $(this).attr("href").toLowerCase();
		if(href.indexOf('product/detail') != -1) { //商品详情
			var id = href.split("/");
			id = id[id.length - 1];
			showProduct(id);
		} else if(href.indexOf('topic/detail') != -1) { //专题页面
			var id = href.split("/");
			id = id[id.length - 1];
			mui.fire(plus.webview.getWebviewById('topic-detail.html'), 'updateData', {
				topicId: id
			});
		} else if(href.indexOf('/fightgroup') != -1) { //拼团列表
			mui.openWindow({
				id: 'merge-list.html',
				url: url + 'merge-list.html',
			});
		}  else if(href.indexOf('/gifts') != -1) { //积分商城
			mui.openWindow({
				id: 'integral-home.html',
				url: url + 'integral-home.html',
			});
		} else if(href.indexOf('/limittimebuy/detail') != -1) { //限时购详情   
            var id = href.split("productid=");
			showProduct(id[1]);
		} else if(href.indexOf('/limittimebuy/home') != -1) { //限时购列表   
			mui.openWindow({
				id: 'limitbuy-list.html',
				url: url + 'limitbuy-list.html',
			});
		} else if(href.indexOf('/m-wap/signin') != -1) { //签到 
			if(isLogin(login, "1")) {
				mui.openWindow({
					id: 'wx-page.html',
					url: url + 'wx-page.html',
					extras: {
						title: "签到",
						url: "/m-Wap/SignIn",
						fullPath: url
					}
				});
			}
		} else if(href.indexOf('/m-wap/shopregister/step1') != -1) { //商家入驻 
			if(isLogin(login, "1")) {
				mui.openWindow({
					id: 'wx-page.html',
					url: url + 'wx-page.html',
					extras: {
						title: "商家入驻",
						url: "/m-wap/ShopRegisterJump/AppJump?tourl=1&userkey=" + plus.storage.getItem('appuserkey'),
						fullPath: url
					}
				});
			}
		} else if(href.indexOf('/m-wap/shopbranch/storelist') != -1) { //周边门店 
			mui.fire(plus.webview.getLaunchWebview(),'gostore');
			plus.webview.currentWebview().opener().close();
		} else if(href.indexOf('/m-wap/vshop/couponinfo') != -1) { //优惠券详情页 
			var id = href.split("/");
			id = id[id.length - 1];
			if(isLogin(login, "1")) {
				mui.openWindow({
					id: 'wx-page.html',
					url: url + 'wx-page.html',
					extras: {
						title: "优惠券详情页",
						url: "/m-wap/vshop/CouponInfo/" + id,
						fullPath: url
					}
				});
			}
		} else if(href.indexOf('/m-weixin/bonus/index') != -1) { //现金红包 
			var id = href.split("/");
			id = id[id.length - 1];
			mui.openWindow({
				id: 'wx-page.html',
				url: url + 'wx-page.html',
				extras: {
					title: "现金红包",
					url: "/m-weixin/bonus/index/" + id,
					fullPath: url
				}
			});
		}else if(href.indexOf('/m-mobile/scratchcard/index') != -1) { //刮刮卡
			var id = href.split("/");
			id = id[id.length - 1];
			mui.openWindow({
				id: 'wx-page.html',
				url: url + 'wx-page.html',
				extras: {
					title: "开心刮刮乐",
					url: "/m-Mobile/ScratchCard/index/" + id,
					fullPath: url
				}
			});
		}else{
			plus.runtime.openURL(href);
		}
	});

	function isLogin(login, type) {
		if(login == "" || login == null) {
			mui.openWindow({
				id: 'login.html',
				url: url + 'login.html'
			});
			return;
		} else {
			return true;
		}
	}

	function showProduct(id) {
		mui.fire(plus.webview.getWebviewById('product-detail.html'), 'updateData', {
			productId: id
		});
		mui.openWindow({
			id: 'product-detail.html',
			url: url + 'product-detail.html',
			show: {
				autoShow: true,
				aniShow: 'pop-in',
				duration: 300
			},
			waiting: {
				autoShow: false
			}
		});
	}
});